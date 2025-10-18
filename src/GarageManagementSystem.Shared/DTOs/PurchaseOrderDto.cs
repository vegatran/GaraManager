using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Shared.DTOs
{
    /// <summary>
    /// DTO cho Purchase Order
    /// </summary>
    public class PurchaseOrderDto
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string OrderNumber { get; set; } = string.Empty;
        
        public int SupplierId { get; set; }
        
        [Required]
        public DateTime OrderDate { get; set; }
        
        public DateTime? ExpectedDeliveryDate { get; set; }
        
        public DateTime? ActualDeliveryDate { get; set; }
        
        [StringLength(20)]
        public string Status { get; set; } = "Pending";
        
        [StringLength(50)]
        public string? SupplierOrderNumber { get; set; }
        
        [StringLength(100)]
        public string? ContactPerson { get; set; }
        
        [StringLength(20)]
        public string? ContactPhone { get; set; }
        
        [StringLength(100)]
        public string? ContactEmail { get; set; }
        
        [StringLength(500)]
        public string? DeliveryAddress { get; set; }
        
        [StringLength(50)]
        public string? PaymentTerms { get; set; }
        
        [StringLength(100)]
        public string? DeliveryTerms { get; set; }
        
        [StringLength(50)]
        public string? Currency { get; set; } = "VND";
        
        public decimal SubTotal { get; set; } = 0;
        
        public decimal TaxAmount { get; set; } = 0;
        
        public decimal ShippingCost { get; set; } = 0;
        
        public decimal TotalAmount { get; set; } = 0;
        
        public int? EmployeeId { get; set; }
        
        [StringLength(100)]
        public string? ApprovedBy { get; set; }
        
        public DateTime? ApprovedDate { get; set; }
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        public bool IsApproved { get; set; } = false;
        
        // Additional properties for display
        public string? SupplierName { get; set; }
        public string? EmployeeName { get; set; }
        public int ItemCount { get; set; }
        public List<PurchaseOrderItemDto> Items { get; set; } = new List<PurchaseOrderItemDto>();
        
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }

    /// <summary>
    /// DTO cho Purchase Order Item
    /// </summary>
    public class PurchaseOrderItemDto
    {
        public int Id { get; set; }
        
        public int PurchaseOrderId { get; set; }
        
        public int PartId { get; set; }
        
        [StringLength(200)]
        public string? PartName { get; set; }
        
        [Required]
        public int QuantityOrdered { get; set; }
        
        public int QuantityReceived { get; set; } = 0;
        
        [Required]
        public decimal UnitPrice { get; set; }
        
        [Required]
        public decimal TotalPrice { get; set; }
        
        [StringLength(50)]
        public string? SupplierPartNumber { get; set; }
        
        [StringLength(100)]
        public string? PartDescription { get; set; }
        
        [StringLength(50)]
        public string? Unit { get; set; }
        
        public DateTime? ExpectedDeliveryDate { get; set; }
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        public bool IsReceived { get; set; } = false;
        
        public DateTime? ReceivedDate { get; set; }
        
        // Additional properties for display
        public string? PartNumber { get; set; }
        public string? PartCategory { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }

    /// <summary>
    /// DTO để tạo Purchase Order mới
    /// </summary>
    public class CreatePurchaseOrderDto
    {
        [Required(ErrorMessage = "Nhà cung cấp là bắt buộc")]
        public int SupplierId { get; set; }
        
        [Required(ErrorMessage = "Ngày đặt hàng là bắt buộc")]
        public DateTime OrderDate { get; set; } = DateTime.Now;
        
        public DateTime? ExpectedDeliveryDate { get; set; }
        
        [StringLength(50)]
        public string? SupplierOrderNumber { get; set; }
        
        [StringLength(100)]
        public string? ContactPerson { get; set; }
        
        [StringLength(20)]
        public string? ContactPhone { get; set; }
        
        [StringLength(100)]
        public string? ContactEmail { get; set; }
        
        [StringLength(500)]
        public string? DeliveryAddress { get; set; }
        
        [StringLength(50)]
        public string? PaymentTerms { get; set; }
        
        [StringLength(100)]
        public string? DeliveryTerms { get; set; }
        
        [StringLength(50)]
        public string? Currency { get; set; } = "VND";
        
        public decimal SubTotal { get; set; } = 0;
        
        public decimal TaxAmount { get; set; } = 0;
        
        public decimal ShippingCost { get; set; } = 0;
        
        public decimal TotalAmount { get; set; } = 0;
        
        public int? EmployeeId { get; set; }
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        public List<CreatePurchaseOrderItemDto> Items { get; set; } = new List<CreatePurchaseOrderItemDto>();
    }

    /// <summary>
    /// DTO để tạo Purchase Order Item mới
    /// </summary>
    public class CreatePurchaseOrderItemDto
    {
        [Required(ErrorMessage = "Phụ tùng là bắt buộc")]
        public int PartId { get; set; }
        
        [Required(ErrorMessage = "Số lượng đặt là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int QuantityOrdered { get; set; }
        
        [Required(ErrorMessage = "Giá đơn vị là bắt buộc")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        public decimal UnitPrice { get; set; }
        
        [StringLength(50)]
        public string? SupplierPartNumber { get; set; }
        
        [StringLength(100)]
        public string? PartDescription { get; set; }
        
        [StringLength(50)]
        public string? Unit { get; set; }
        
        public DateTime? ExpectedDeliveryDate { get; set; }
        
        [StringLength(500)]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO để cập nhật Purchase Order
    /// </summary>
    public class UpdatePurchaseOrderDto
    {
        [Required]
        public int Id { get; set; }
        
        public DateTime? ExpectedDeliveryDate { get; set; }
        
        public DateTime? ActualDeliveryDate { get; set; }
        
        [StringLength(20)]
        public string? Status { get; set; }
        
        [StringLength(50)]
        public string? SupplierOrderNumber { get; set; }
        
        [StringLength(100)]
        public string? ContactPerson { get; set; }
        
        [StringLength(20)]
        public string? ContactPhone { get; set; }
        
        [StringLength(100)]
        public string? ContactEmail { get; set; }
        
        [StringLength(500)]
        public string? DeliveryAddress { get; set; }
        
        [StringLength(50)]
        public string? PaymentTerms { get; set; }
        
        [StringLength(100)]
        public string? DeliveryTerms { get; set; }
        
        [StringLength(50)]
        public string? Currency { get; set; }
        
        public decimal? SubTotal { get; set; }
        
        public decimal? TaxAmount { get; set; }
        
        public decimal? ShippingCost { get; set; }
        
        public decimal? TotalAmount { get; set; }
        
        [StringLength(100)]
        public string? ApprovedBy { get; set; }
        
        public DateTime? ApprovedDate { get; set; }
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        public bool? IsApproved { get; set; }
    }
}
