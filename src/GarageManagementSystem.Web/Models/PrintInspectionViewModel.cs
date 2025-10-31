using GarageManagementSystem.Shared.DTOs;

namespace GarageManagementSystem.Web.Models
{
    /// <summary>
    /// View Model cho trang in phiếu chẩn đoán kỹ thuật
    /// </summary>
    public class PrintInspectionViewModel
    {
        /// <summary>
        /// Dữ liệu phiếu kiểm tra xe
        /// </summary>
        public VehicleInspectionDto Inspection { get; set; } = null!;

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
}

