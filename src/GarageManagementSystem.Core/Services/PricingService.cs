using GarageManagementSystem.Core.Entities;
using System.Text.Json;

namespace GarageManagementSystem.Core.Services
{
    /// <summary>
    /// Service để tính toán giá theo các mô hình pricing khác nhau
    /// </summary>
    public class PricingService
    {
        /// <summary>
        /// Tính giá cho một dịch vụ theo pricing model
        /// </summary>
        public static PricingResult CalculateServicePrice(Service service, int quantity = 1)
        {
            var result = new PricingResult
            {
                PricingModel = service.PricingModel,
                Quantity = quantity,
                IsVATApplicable = service.IsVATApplicable,
                VATRate = service.IsVATApplicable ? service.VATRate : 0
            };

            // ✅ ĐƠN GIẢN: Tất cả dịch vụ đều dùng "Separated" model
            // Tách riêng vật liệu và công lao động cho rõ ràng
            result.MaterialCost = service.MaterialCost;
            result.LaborCost = service.LaborRate * service.LaborHours;
            result.UnitPrice = result.MaterialCost + result.LaborCost;
            // ✅ SỬA: Lấy IsVATApplicable từ service, không hardcode
            result.IsVATApplicable = service.IsVATApplicable;
            result.VATRate = service.VATRate;

            // Tính tổng
            result.SubTotal = result.UnitPrice * quantity;
            
            // ✅ VAT chỉ tính trên MaterialCost, KHÔNG tính trên LaborCost
            if (result.IsVATApplicable)
            {
                // VAT chỉ tính trên vật liệu, không tính trên công
                result.VATAmount = result.MaterialCost * quantity * (result.VATRate / 100m);
            }
            else
            {
                result.VATAmount = 0;
            }
            result.TotalAmount = result.SubTotal + result.VATAmount;

            // Tạo breakdown JSON
            result.PricingBreakdown = JsonSerializer.Serialize(new
            {
                Model = result.PricingModel,
                MaterialCost = result.MaterialCost,
                LaborCost = result.LaborCost,
                UnitPrice = result.UnitPrice,
                VATRate = result.VATRate,
                VATAmount = result.VATAmount,
                IsVATApplicable = result.IsVATApplicable,
                Description = GetPricingDescription(service)
            });

            return result;
        }

        /// <summary>
        /// Tính giá cho QuotationItem từ Service
        /// </summary>
        public static void ApplyPricingToQuotationItem(QuotationItem item, Service service)
        {
            var pricing = CalculateServicePrice(service, item.Quantity);

            item.PricingModel = pricing.PricingModel;
            item.MaterialCost = pricing.MaterialCost;
            item.LaborCost = pricing.LaborCost;
            item.UnitPrice = pricing.UnitPrice;
            item.VATRate = pricing.VATRate;
            item.IsVATApplicable = item.HasInvoice ? pricing.IsVATApplicable: item.HasInvoice;
            item.PricingBreakdown = pricing.PricingBreakdown;
            item.VATAmount = pricing.VATAmount;
            item.TotalPrice = pricing.UnitPrice * item.Quantity; // ✅ SỬA: TotalPrice = UnitPrice × Quantity (không bao gồm VAT)
            item.TotalAmount = pricing.TotalAmount;
            
            // ✅ THÊM: Set ItemCategory dựa trên service type
            item.ItemCategory = GetItemCategoryFromService(service);
        }

        /// <summary>
        /// Xác định ItemCategory dựa trên Service
        /// </summary>
        private static string GetItemCategoryFromService(Service service)
        {
            // Logic xác định ItemCategory dựa trên Service
            if (service.Name.Contains("Công") || service.Name.Contains("Lao động"))
            {
                return "Labor";
            }
            else if (service.Name.Contains("Phụ tùng") || service.Name.Contains("Vật liệu"))
            {
                return "Material";
            }
            else
            {
                return "Service";
            }
        }

        /// <summary>
        /// Lấy mô tả pricing model
        /// </summary>
        private static string GetPricingDescription(Service service)
        {
            return service.PricingModel switch
            {
                "Combined" => "Giá dịch vụ đã bao gồm vật liệu và công lao động",
                "Separated" => $"Vật liệu: {service.MaterialCost:N0} VNĐ + Công: {service.LaborRate * service.LaborHours:N0} VNĐ",
                "LaborOnly" => "Chỉ tính công lao động (không bán vật liệu)",
                _ => "Mô hình tính giá chuẩn"
            };
        }
    }

    /// <summary>
    /// Kết quả tính giá
    /// </summary>
    public class PricingResult
    {
        public string PricingModel { get; set; } = "Combined";
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public decimal MaterialCost { get; set; }
        public decimal LaborCost { get; set; }
        public decimal SubTotal { get; set; }
        public decimal VATAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsVATApplicable { get; set; } = true;
        public int VATRate { get; set; } = 10;
        public string? PricingBreakdown { get; set; }
    }
}
