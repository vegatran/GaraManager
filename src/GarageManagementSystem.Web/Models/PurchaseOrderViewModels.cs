using GarageManagementSystem.Shared.DTOs;

namespace GarageManagementSystem.Web.Models
{
    /// <summary>
    /// View model cho Purchase Order details
    /// </summary>
    public class PurchaseOrderDetailsViewModel
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int ItemCount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public string? PaymentTerms { get; set; }
        public string? DeliveryAddress { get; set; }
        public decimal VATRate { get; set; }
        public List<PurchaseOrderItemViewModel> Items { get; set; } = new List<PurchaseOrderItemViewModel>();
    }

    /// <summary>
    /// View model cho Purchase Order Item
    /// </summary>
    public class PurchaseOrderItemViewModel
    {
        public int Id { get; set; }
        public int PartId { get; set; }
        public string PartName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal VATRate { get; set; }
        public decimal VATAmount { get; set; }
        public string? Notes { get; set; }
    }
}
