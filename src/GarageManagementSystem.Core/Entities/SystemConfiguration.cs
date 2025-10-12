namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// Cấu hình hệ thống - cho phép thay đổi các thông số quan trọng
    /// </summary>
    public class SystemConfiguration : BaseEntity
    {
        
        /// <summary>
        /// Khóa cấu hình (unique)
        /// </summary>
        public string ConfigKey { get; set; } = string.Empty;
        
        /// <summary>
        /// Giá trị cấu hình
        /// </summary>
        public string ConfigValue { get; set; } = string.Empty;
        
        /// <summary>
        /// Mô tả cấu hình
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// Loại dữ liệu: String, Number, Decimal, Boolean, JSON
        /// </summary>
        public string DataType { get; set; } = "String";
        
        /// <summary>
        /// Nhóm cấu hình: VAT, Invoice, Payment, System
        /// </summary>
        public string Category { get; set; } = "System";
        
        /// <summary>
        /// Có cho phép người dùng thay đổi không
        /// </summary>
        public bool IsEditable { get; set; } = true;
        
        /// <summary>
        /// Có hiển thị trong UI không
        /// </summary>
        public bool IsVisible { get; set; } = true;
        
        /// <summary>
        /// Thứ tự hiển thị
        /// </summary>
        public int DisplayOrder { get; set; }
    }
}

