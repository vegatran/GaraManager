using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CreateDefaultTemplate
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://localhost:5001/");
            
            try
            {
                Console.WriteLine("Đang tạo mẫu báo giá mặc định...");
                
                // Call API endpoint để tạo mẫu mặc định
                var response = await httpClient.PostAsync("api/PrintTemplates/create-default-quotation", null);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("✅ Tạo mẫu báo giá mặc định thành công!");
                    Console.WriteLine(content);
                }
                else
                {
                    Console.WriteLine($"❌ Lỗi: {response.StatusCode}");
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(error);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi: {ex.Message}");
            }
        }
    }
}

