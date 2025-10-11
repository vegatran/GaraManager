using System.Security.Claims;
using System.Text;
using System.Text.Json;
using GarageManagementSystem.Web.Configuration;
using GarageManagementSystem.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using System.Text.Json.Serialization;

namespace GarageManagementSystem.Web.Services
{
    /// <summary>
    /// Service for making API calls with automatic token management
    /// </summary>
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ClaimsPrincipal _user;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient(ApiConfiguration.HttpClientName);
            _user = httpContextAccessor.HttpContext?.User ?? throw new ArgumentNullException("User not found");
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Set authorization header with access token
        /// </summary>
        private async Task SetAuthorizationHeaderAsync()
        {
            // Clear existing authorization header
            _httpClient.DefaultRequestHeaders.Authorization = null;
            
            // Thử nhiều cách lấy token - Ưu tiên token store trước
            var accessToken = await _httpContextAccessor.HttpContext?.GetTokenAsync("access_token") // From token store - ƯU TIÊN
                           ?? _user.FindFirst("access_token")?.Value 
                           ?? _user.FindFirst("at")?.Value;  // Alternative claim name

            if (!string.IsNullOrEmpty(accessToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                Console.WriteLine($"✅ Token da duoc thiet lap cho cuoc goi API: {accessToken.Substring(0, Math.Min(20, accessToken.Length))}...");
            }
            else
            {
                Console.WriteLine("❌ Khong tim thay access token! Cac claims co san:");
                foreach (var claim in _user.Claims)
                {
                    Console.WriteLine($"  {claim.Type}: {claim.Value}");
                }
                
                // Log token store info
                var idToken = await _httpContextAccessor.HttpContext?.GetTokenAsync("id_token");
                var refreshToken = await _httpContextAccessor.HttpContext?.GetTokenAsync("refresh_token");
                
                Console.WriteLine($"🔍 Thong tin Token Store:");
                Console.WriteLine($"  ID Token: {(string.IsNullOrEmpty(idToken) ? "null" : "EXISTS")}");
                Console.WriteLine($"  Refresh Token: {(string.IsNullOrEmpty(refreshToken) ? "null" : "EXISTS")}");
            }
        }

        /// <summary>
        /// Generic GET request
        /// </summary>
        public async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
        {
            try
            {
                await SetAuthorizationHeaderAsync();
                var response = await _httpClient.GetAsync(endpoint);
                
                var content = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    // API trả về ApiResponse<T>, cần deserialize đúng cách với camelCase support
                    var options = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        PropertyNameCaseInsensitive = true
                    };
                    
                    var apiResponse = JsonSerializer.Deserialize<Shared.Models.ApiResponse<T>>(content, options);
                    if (apiResponse != null)
                    {
                        return new ApiResponse<T>
                        {
                            Success = apiResponse.Success,
                            Data = apiResponse.Data,  // apiResponse.Data chính là T (ví dụ: List<CustomerDto>)
                            Message = apiResponse.Message,
                            StatusCode = response.StatusCode
                        };
                    }
                    
                    // Fallback nếu deserialize thất bại
                    return new ApiResponse<T>
                    {
                        Success = false,
                        ErrorMessage = "Failed to deserialize API response",
                        StatusCode = response.StatusCode
                    };
                }
                else
                {
                    // Xử lý 401 Unauthorized - Token timeout
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        // Set Ma trang thai HTTP để middleware có thể catch
                        _httpContextAccessor.HttpContext.Response.StatusCode = 401;
                        
                        return new ApiResponse<T>
                        {
                            Success = false,
                            ErrorMessage = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.",
                            StatusCode = response.StatusCode,
                            RequiresLogin = true
                        };
                    }
                    
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
                await SetAuthorizationHeaderAsync();
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(endpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    // API trả về ApiResponse<T>, cần deserialize đúng cách với camelCase support
                    var options = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        PropertyNameCaseInsensitive = true
                    };
                    
                    var apiResponse = JsonSerializer.Deserialize<Shared.Models.ApiResponse<T>>(responseContent, options);
                    if (apiResponse != null)
                    {
                        return new ApiResponse<T>
                        {
                            Success = apiResponse.Success,
                            Data = apiResponse.Data,
                            Message = apiResponse.Message,
                            StatusCode = response.StatusCode
                        };
                    }
                    
                    // Fallback nếu deserialize thất bại
                    return new ApiResponse<T>
                    {
                        Success = false,
                        ErrorMessage = "Failed to deserialize API response",
                        StatusCode = response.StatusCode
                    };
                }
                else
                {
                    // Xử lý 401 Unauthorized - Token timeout
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        // Set Ma trang thai HTTP để middleware có thể catch
                        _httpContextAccessor.HttpContext.Response.StatusCode = 401;
                        
                        return new ApiResponse<T>
                        {
                            Success = false,
                            ErrorMessage = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.",
                            StatusCode = response.StatusCode,
                            RequiresLogin = true
                        };
                    }
                    
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
                await SetAuthorizationHeaderAsync();
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
                    // Xử lý 401 Unauthorized - Token timeout
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        // Set Ma trang thai HTTP để middleware có thể catch
                        _httpContextAccessor.HttpContext.Response.StatusCode = 401;
                        
                        return new ApiResponse<T>
                        {
                            Success = false,
                            ErrorMessage = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.",
                            StatusCode = response.StatusCode,
                            RequiresLogin = true
                        };
                    }
                    
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
                await SetAuthorizationHeaderAsync();
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
                    // Xử lý 401 Unauthorized - Token timeout
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        // Set Ma trang thai HTTP để middleware có thể catch
                        _httpContextAccessor.HttpContext.Response.StatusCode = 401;
                        
                        return new ApiResponse<T>
                        {
                            Success = false,
                            ErrorMessage = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.",
                            StatusCode = response.StatusCode,
                            RequiresLogin = true
                        };
                    }
                    
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
                await SetAuthorizationHeaderAsync();
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

        /// <summary>
        /// POST file to API endpoint
        /// </summary>
        public async Task<ApiResponse<T>> PostFileAsync<T>(string endpoint, IFormFile file)
        {
            try
            {
                await SetAuthorizationHeaderAsync();

                using var content = new MultipartFormDataContent();
                using var fileStream = file.OpenReadStream();
                content.Add(new StreamContent(fileStream), "file", file.FileName);

                var response = await _httpClient.PostAsync(endpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var jsonOptions = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                    };
                    
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(responseContent, jsonOptions);
                    return apiResponse ?? new ApiResponse<T>
                    {
                        Success = false,
                        ErrorMessage = "Không thể parse response từ API",
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
    }
}
