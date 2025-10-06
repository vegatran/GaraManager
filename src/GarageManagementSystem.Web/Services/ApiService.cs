using System.Security.Claims;
using System.Text;
using System.Text.Json;
using GarageManagementSystem.Web.Configuration;

namespace GarageManagementSystem.Web.Services
{
    /// <summary>
    /// Service for making API calls with automatic token management
    /// </summary>
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ClaimsPrincipal _user;

        public ApiService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient(ApiConfiguration.HttpClientName);
            _user = httpContextAccessor.HttpContext?.User ?? throw new ArgumentNullException("User not found");
        }

        /// <summary>
        /// Set authorization header with access token
        /// </summary>
        private void SetAuthorizationHeader()
        {
            var accessToken = _user.FindFirst("access_token")?.Value;
            if (!string.IsNullOrEmpty(accessToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            }
        }

        /// <summary>
        /// Generic GET request
        /// </summary>
        public async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
        {
            try
            {
                SetAuthorizationHeader();
                var response = await _httpClient.GetAsync(endpoint);
                
                var content = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    var data = JsonSerializer.Deserialize<T>(content);
                    return new ApiResponse<T>
                    {
                        Success = true,
                        Data = data,
                        StatusCode = response.StatusCode
                    };
                }
                else
                {
                    return new ApiResponse<T>
                    {
                        Success = false,
                        ErrorMessage = $"API Error: {response.StatusCode} - {content}",
                        StatusCode = response.StatusCode
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<T>
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    StatusCode = System.Net.HttpStatusCode.InternalServerError
                };
            }
        }

        /// <summary>
        /// Generic POST request
        /// </summary>
        public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data)
        {
            try
            {
                SetAuthorizationHeader();
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(endpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    var responseData = JsonSerializer.Deserialize<T>(responseContent);
                    return new ApiResponse<T>
                    {
                        Success = true,
                        Data = responseData,
                        StatusCode = response.StatusCode
                    };
                }
                else
                {
                    return new ApiResponse<T>
                    {
                        Success = false,
                        ErrorMessage = $"API Error: {response.StatusCode} - {responseContent}",
                        StatusCode = response.StatusCode
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<T>
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    StatusCode = System.Net.HttpStatusCode.InternalServerError
                };
            }
        }

        /// <summary>
        /// Generic PUT request
        /// </summary>
        public async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object data)
        {
            try
            {
                SetAuthorizationHeader();
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PutAsync(endpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    var responseData = JsonSerializer.Deserialize<T>(responseContent);
                    return new ApiResponse<T>
                    {
                        Success = true,
                        Data = responseData,
                        StatusCode = response.StatusCode
                    };
                }
                else
                {
                    return new ApiResponse<T>
                    {
                        Success = false,
                        ErrorMessage = $"API Error: {response.StatusCode} - {responseContent}",
                        StatusCode = response.StatusCode
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<T>
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    StatusCode = System.Net.HttpStatusCode.InternalServerError
                };
            }
        }

        /// <summary>
        /// Generic DELETE request
        /// </summary>
        public async Task<ApiResponse<T>> DeleteAsync<T>(string endpoint)
        {
            try
            {
                SetAuthorizationHeader();
                var response = await _httpClient.DeleteAsync(endpoint);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    var responseData = JsonSerializer.Deserialize<T>(responseContent);
                    return new ApiResponse<T>
                    {
                        Success = true,
                        Data = responseData,
                        StatusCode = response.StatusCode
                    };
                }
                else
                {
                    return new ApiResponse<T>
                    {
                        Success = false,
                        ErrorMessage = $"API Error: {response.StatusCode} - {responseContent}",
                        StatusCode = response.StatusCode
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<T>
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    StatusCode = System.Net.HttpStatusCode.InternalServerError
                };
            }
        }

        /// <summary>
        /// Simple GET request that returns string content (for non-JSON responses)
        /// </summary>
        public async Task<ApiResponse<string>> GetStringAsync(string endpoint)
        {
            try
            {
                SetAuthorizationHeader();
                var response = await _httpClient.GetAsync(endpoint);
                var content = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse<string>
                    {
                        Success = true,
                        Data = content,
                        StatusCode = response.StatusCode
                    };
                }
                else
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        ErrorMessage = $"API Error: {response.StatusCode} - {content}",
                        StatusCode = response.StatusCode
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    StatusCode = System.Net.HttpStatusCode.InternalServerError
                };
            }
        }
    }

    /// <summary>
    /// Generic API response wrapper
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public System.Net.HttpStatusCode StatusCode { get; set; }
    }
}
