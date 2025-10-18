using GarageManagementSystem.Shared.DTOs;

namespace GarageManagementSystem.Web.Models
{
    /// <summary>
    /// View Model cho trang in báo giá
    /// </summary>
    public class PrintQuotationViewModel
    {
        /// <summary>
        /// Dữ liệu báo giá
        /// </summary>
        public ServiceQuotationDto Quotation { get; set; } = null!;

        /// <summary>
        /// Template in (có thể null nếu chưa có template)
        /// </summary>
        public PrintTemplateDto? Template { get; set; }

        /// <summary>
        /// Thông tin công ty từ template (parsed từ JSON)
        /// </summary>
        public CompanyInfo? CompanyInfo
        {
            get
            {
                if (Template?.CompanyInfo == null) 
                {
                    // Fallback to default company info
                    return new CompanyInfo
                    {
                        CompanyName = "GARAGE Ô TÔ THUẬN PHÁT",
                        CompanyAddress = "313 Võ Văn Vân, Vĩnh Lộc B, H. Bình Chánh, TP.HCM",
                        CompanyPhone = "032.7007.985 (Mr. Bằng)",
                        CompanyEmail = "garage@thuanphat.com",
                        CompanyWebsite = "www.thuanphat.com",
                        TaxCode = ""
                    };
                }
                
                try
                {
                    return System.Text.Json.JsonSerializer.Deserialize<CompanyInfo>(Template.CompanyInfo);
                }
                catch
                {
                    // Fallback to default company info on parse error
                    return new CompanyInfo
                    {
                        CompanyName = "GARAGE Ô TÔ THUẬN PHÁT",
                        CompanyAddress = "313 Võ Văn Vân, Vĩnh Lộc B, H. Bình Chánh, TP.HCM",
                        CompanyPhone = "032.7007.985 (Mr. Bằng)",
                        CompanyEmail = "garage@thuanphat.com",
                        CompanyWebsite = "www.thuanphat.com",
                        TaxCode = ""
                    };
                }
            }
        }
    }

    /// <summary>
    /// Thông tin công ty
    /// </summary>
    public class CompanyInfo
    {
        [System.Text.Json.Serialization.JsonPropertyName("companyName")]
        public string CompanyName { get; set; } = string.Empty;
        
        [System.Text.Json.Serialization.JsonPropertyName("companyAddress")]
        public string CompanyAddress { get; set; } = string.Empty;
        
        [System.Text.Json.Serialization.JsonPropertyName("companyPhone")]
        public string CompanyPhone { get; set; } = string.Empty;
        
        [System.Text.Json.Serialization.JsonPropertyName("companyEmail")]
        public string CompanyEmail { get; set; } = string.Empty;
        
        [System.Text.Json.Serialization.JsonPropertyName("companyWebsite")]
        public string CompanyWebsite { get; set; } = string.Empty;
        
        [System.Text.Json.Serialization.JsonPropertyName("taxCode")]
        public string TaxCode { get; set; } = string.Empty;
    }
}
