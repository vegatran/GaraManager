using GarageManagementSystem.Shared.DTOs;

namespace GarageManagementSystem.Web.Models
{
    /// <summary>
    /// View model cho Part details
    /// </summary>
    public class PartDetailsViewModel
    {
        public int Id { get; set; }
        public string PartNumber { get; set; } = string.Empty;
        public string PartName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public decimal CostPrice { get; set; }
        public decimal SellPrice { get; set; }
        public int QuantityInStock { get; set; }
        public int MinimumStock { get; set; }
        public int ReorderLevel { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string SourceType { get; set; } = string.Empty;
        public string InvoiceType { get; set; } = string.Empty;
        public bool HasInvoice { get; set; }
        public string Condition { get; set; } = string.Empty;
        public string SourceReference { get; set; } = string.Empty;
        public bool CanUseForCompany { get; set; }
        public bool CanUseForInsurance { get; set; }
        public bool CanUseForIndividual { get; set; }
        public int WarrantyMonths { get; set; }
        public bool IsOEM { get; set; }
        public string OEMNumber { get; set; } = string.Empty;
        public string AftermarketNumber { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string Dimensions { get; set; } = string.Empty;
        public string Weight { get; set; } = string.Empty;
        public string Material { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }

    /// <summary>
    /// View model cho Supplier details
    /// </summary>
    public class SupplierDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string TaxCode { get; set; } = string.Empty;
        public string PaymentTerms { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// View model cho Stock Transaction details
    /// </summary>
    public class StockTransactionDetailsViewModel
    {
        public int Id { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public int PartId { get; set; }
        public string PartName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    /// <summary>
    /// View model cho Customer details
    /// </summary>
    public class CustomerDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string TaxCode { get; set; } = string.Empty;
        public string CustomerType { get; set; } = string.Empty;
        public string ContactPersonName { get; set; } = string.Empty;
    }

    /// <summary>
    /// View model cho Vehicle details
    /// </summary>
    public class VehicleDetailsViewModel
    {
        public int Id { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public string Color { get; set; } = string.Empty;
        public string VIN { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
    }

    /// <summary>
    /// View model cho Employee details
    /// </summary>
    public class EmployeeDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public DateTime HireDate { get; set; }
        public decimal Salary { get; set; }
        public string Skills { get; set; } = string.Empty;
    }
}
