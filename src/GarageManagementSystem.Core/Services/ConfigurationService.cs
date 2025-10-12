using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;

namespace GarageManagementSystem.Core.Services
{
    public interface IConfigurationService
    {
        Task<string> GetConfigValueAsync(string key, string defaultValue = "");
        Task<decimal> GetDecimalConfigAsync(string key, decimal defaultValue = 0m);
        Task<int> GetIntConfigAsync(string key, int defaultValue = 0);
        Task<bool> GetBoolConfigAsync(string key, bool defaultValue = false);
        Task<bool> SetConfigValueAsync(string key, string value, string? updatedBy = null);
        Task<Dictionary<string, string>> GetConfigsByCategoryAsync(string category);
        Task<decimal> GetVATRateAsync(); // Shortcut cho VAT rate
    }

    public class ConfigurationService : IConfigurationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private Dictionary<string, string>? _configCache;
        private DateTime _cacheExpiry = DateTime.MinValue;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);

        public ConfigurationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Load hoặc refresh cache
        /// </summary>
        private async Task LoadCacheAsync()
        {
            if (_configCache == null || DateTime.Now > _cacheExpiry)
            {
                var configs = await _unitOfWork.Repository<SystemConfiguration>().GetAllAsync();
                _configCache = configs.ToDictionary(c => c.ConfigKey, c => c.ConfigValue);
                _cacheExpiry = DateTime.Now.Add(_cacheDuration);
            }
        }

        /// <summary>
        /// Lấy giá trị config dạng string
        /// </summary>
        public async Task<string> GetConfigValueAsync(string key, string defaultValue = "")
        {
            await LoadCacheAsync();
            return _configCache!.GetValueOrDefault(key, defaultValue);
        }

        /// <summary>
        /// Lấy giá trị config dạng decimal
        /// </summary>
        public async Task<decimal> GetDecimalConfigAsync(string key, decimal defaultValue = 0m)
        {
            var value = await GetConfigValueAsync(key);
            return decimal.TryParse(value, out var result) ? result : defaultValue;
        }

        /// <summary>
        /// Lấy giá trị config dạng int
        /// </summary>
        public async Task<int> GetIntConfigAsync(string key, int defaultValue = 0)
        {
            var value = await GetConfigValueAsync(key);
            return int.TryParse(value, out var result) ? result : defaultValue;
        }

        /// <summary>
        /// Lấy giá trị config dạng bool
        /// </summary>
        public async Task<bool> GetBoolConfigAsync(string key, bool defaultValue = false)
        {
            var value = await GetConfigValueAsync(key);
            return bool.TryParse(value, out var result) ? result : defaultValue;
        }

        /// <summary>
        /// Set giá trị config
        /// </summary>
        public async Task<bool> SetConfigValueAsync(string key, string value, string? updatedBy = null)
        {
            try
            {
                var configs = await _unitOfWork.Repository<SystemConfiguration>().GetAllAsync();
                var config = configs.FirstOrDefault(c => c.ConfigKey == key);

                if (config == null)
                {
                    // Create new config
                    config = new SystemConfiguration
                    {
                        ConfigKey = key,
                        ConfigValue = value,
                        CreatedAt = DateTime.Now,
                        CreatedBy = updatedBy
                    };
                    await _unitOfWork.Repository<SystemConfiguration>().AddAsync(config);
                }
                else
                {
                    // Update existing
                    if (!config.IsEditable)
                    {
                        throw new InvalidOperationException($"Configuration '{key}' is not editable");
                    }

                    config.ConfigValue = value;
                    config.UpdatedAt = DateTime.Now;
                    config.UpdatedBy = updatedBy;
                    await _unitOfWork.Repository<SystemConfiguration>().UpdateAsync(config);
                }

                await _unitOfWork.SaveChangesAsync();

                // Clear cache để load lại
                _configCache = null;

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Lấy tất cả configs theo category
        /// </summary>
        public async Task<Dictionary<string, string>> GetConfigsByCategoryAsync(string category)
        {
            var configs = await _unitOfWork.Repository<SystemConfiguration>().GetAllAsync();
            return configs
                .Where(c => c.Category == category && c.IsVisible)
                .OrderBy(c => c.DisplayOrder)
                .ToDictionary(c => c.ConfigKey, c => c.ConfigValue);
        }

        /// <summary>
        /// Shortcut: Lấy VAT rate hiện tại
        /// </summary>
        public async Task<decimal> GetVATRateAsync()
        {
            return await GetDecimalConfigAsync("VAT.DefaultRate", 0.10m);
        }
    }
}

