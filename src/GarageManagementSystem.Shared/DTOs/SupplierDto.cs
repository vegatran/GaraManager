using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Shared.DTOs
{
    public class SupplierDto : BaseDto
    {
        public string SupplierCode { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? ContactPerson { get; set; }
        public string? ContactPhone { get; set; }
        public string? TaxCode { get; set; }
        public string? BankAccount { get; set; }
        public string? BankName { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public int? Rating { get; set; }
    }

    public class CreateSupplierDto
    {
        [Required(ErrorMessage = "Mã nhà cung cấp là bắt buộc")]
        [StringLength(50, ErrorMessage = "Mã nhà cung cấp không được vượt quá 50 ký tự")]
        public string SupplierCode { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Tên nhà cung cấp là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tên nhà cung cấp không được vượt quá 200 ký tự")]
        public string SupplierName { get; set; } = string.Empty;
        
        [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
        public string? Phone { get; set; }
        
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(200, ErrorMessage = "Email không được vượt quá 200 ký tự")]
        public string? Email { get; set; }
        
        [StringLength(500, ErrorMessage = "Địa chỉ không được vượt quá 500 ký tự")]
        public string? Address { get; set; }
        
        [StringLength(100, ErrorMessage = "Tên người liên hệ không được vượt quá 100 ký tự")]
        public string? ContactPerson { get; set; }
        
        [StringLength(20, ErrorMessage = "Số điện thoại liên hệ không được vượt quá 20 ký tự")]
        public string? ContactPhone { get; set; }
        
        [StringLength(50, ErrorMessage = "Mã số thuế không được vượt quá 50 ký tự")]
        public string? TaxCode { get; set; }
        
        [StringLength(100, ErrorMessage = "Số tài khoản ngân hàng không được vượt quá 100 ký tự")]
        public string? BankAccount { get; set; }
        
        [StringLength(200, ErrorMessage = "Tên ngân hàng không được vượt quá 200 ký tự")]
        public string? BankName { get; set; }
        
        [StringLength(1000, ErrorMessage = "Ghi chú không được vượt quá 1000 ký tự")]
        public string? Notes { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        [Range(1, 5, ErrorMessage = "Đánh giá phải từ 1 đến 5 sao")]
        public int? Rating { get; set; }
    }

    public class UpdateSupplierDto : CreateSupplierDto
    {
        [Required(ErrorMessage = "ID là bắt buộc")]
        public int Id { get; set; }
    }
}

