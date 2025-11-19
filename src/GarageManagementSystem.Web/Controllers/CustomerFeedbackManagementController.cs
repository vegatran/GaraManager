using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Web.Services;
using GarageManagementSystem.Web.Configuration;

namespace GarageManagementSystem.Web.Controllers
{
    /// <summary>
    /// Controller quản lý phản hồi khách hàng
    /// </summary>
    [Authorize]
    [Route("CustomerFeedbackManagement")]
    public class CustomerFeedbackManagementController : Controller
    {
        private readonly ApiService _apiService;

        public CustomerFeedbackManagementController(ApiService apiService)
        {
            _apiService = apiService;
        }

        /// <summary>
        /// Hiển thị trang quản lý phản hồi khách hàng
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Lấy danh sách phản hồi với pagination (hỗ trợ DataTable server-side)
        /// </summary>
        [HttpGet("GetFeedbacks")]
        public async Task<IActionResult> GetFeedbacks(
            int? draw = null,  // DataTable draw parameter
            int? start = null,  // DataTable start (offset)
            int? length = null,  // DataTable length (page size)
            int pageNumber = 1,  // Standard pagination
            int pageSize = 10,  // Standard pagination
            int? customerId = null,
            int? serviceOrderId = null,
            string? status = null,
            string? source = null,
            string? rating = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? keyword = null)
        {
            try
            {
                // ✅ SỬA: Hỗ trợ cả DataTable format và standard pagination
                // ✅ SỬA: Validate length > 0 để tránh division by zero
                bool isDataTableRequest = draw.HasValue;
                int actualPageNumber = isDataTableRequest && start.HasValue && length.HasValue && length.Value > 0
                    ? (start.Value / length.Value) + 1 
                    : pageNumber;
                int actualPageSize = isDataTableRequest && length.HasValue && length.Value > 0
                    ? length.Value 
                    : pageSize;

                var queryParams = new List<string>
                {
                    $"pageNumber={actualPageNumber}",
                    $"pageSize={actualPageSize}"
                };

                if (customerId.HasValue)
                    queryParams.Add($"customerId={customerId.Value}");
                if (serviceOrderId.HasValue)
                    queryParams.Add($"serviceOrderId={serviceOrderId.Value}");
                if (!string.IsNullOrEmpty(status))
                    queryParams.Add($"status={Uri.EscapeDataString(status)}");
                if (!string.IsNullOrEmpty(source))
                    queryParams.Add($"source={Uri.EscapeDataString(source)}");
                if (!string.IsNullOrEmpty(rating))
                    queryParams.Add($"rating={Uri.EscapeDataString(rating)}");
                if (startDate.HasValue)
                    queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");
                if (endDate.HasValue)
                    queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");
                if (!string.IsNullOrEmpty(keyword))
                    queryParams.Add($"keyword={Uri.EscapeDataString(keyword)}");

                var endpoint = ApiEndpoints.CustomerFeedbacks.GetAll + "?" + string.Join("&", queryParams);
                var response = await _apiService.GetAsync<PagedResponse<CustomerFeedbackDto>>(endpoint);

                if (response.Success && response.Data != null)
                {
                    // ✅ SỬA: Return DataTable format nếu là DataTable request
                    if (isDataTableRequest)
                    {
                        return Json(new
                        {
                            draw = draw.Value,
                            recordsTotal = response.Data.TotalCount,
                            recordsFiltered = response.Data.TotalCount,
                            data = response.Data.Data.Select(f => new
                            {
                                id = f.Id,
                                customerName = f.CustomerName ?? "-",
                                orderNumber = f.OrderNumber ?? "-",
                                feedbackChannelName = f.FeedbackChannelName ?? "-",
                                rating = f.Rating,
                                content = f.Content,
                                status = f.Status,
                                createdAt = f.CreatedAt
                            })
                        });
                    }
                    else
                    {
                        // Standard pagination format
                        return Json(response.Data);
                    }
                }
                else
                {
                    if (isDataTableRequest)
                    {
                        return Json(new
                        {
                            draw = draw ?? 1,
                            recordsTotal = 0,
                            recordsFiltered = 0,
                            data = new List<object>()
                        });
                    }
                    else
                    {
                        return Json(new PagedResponse<CustomerFeedbackDto>
                        {
                            Data = new List<CustomerFeedbackDto>(),
                            TotalCount = 0,
                            PageNumber = actualPageNumber,
                            PageSize = actualPageSize,
                            Success = false,
                            Message = response.ErrorMessage ?? "Lỗi khi lấy danh sách phản hồi"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                if (draw.HasValue)
                {
                    return Json(new
                    {
                        draw = draw.Value,
                        recordsTotal = 0,
                        recordsFiltered = 0,
                        data = new List<object>(),
                        error = ex.Message
                    });
                }
                else
                {
                    return Json(new PagedResponse<CustomerFeedbackDto>
                    {
                        Data = new List<CustomerFeedbackDto>(),
                        TotalCount = 0,
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        Success = false,
                        Message = $"Lỗi: {ex.Message}"
                    });
                }
            }
        }

        /// <summary>
        /// Lấy chi tiết phản hồi
        /// </summary>
        [HttpGet("GetFeedback/{id}")]
        public async Task<IActionResult> GetFeedback(int id)
        {
            try
            {
                var endpoint = string.Format(ApiEndpoints.CustomerFeedbacks.GetById, id);
                var response = await _apiService.GetAsync<ApiResponse<CustomerFeedbackDto>>(endpoint);

                if (response.Success && response.Data != null)
                {
                    return Json(response.Data);
                }
                else
                {
                    return Json(new ApiResponse<CustomerFeedbackDto>
                    {
                        Success = false,
                        ErrorMessage = response.ErrorMessage ?? "Không tìm thấy phản hồi"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse<CustomerFeedbackDto>
                {
                    Success = false,
                    ErrorMessage = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Tạo phản hồi mới
        /// </summary>
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CreateCustomerFeedbackDto request)
        {
            try
            {
                var response = await _apiService.PostAsync<ApiResponse<CustomerFeedbackDto>>(
                    ApiEndpoints.CustomerFeedbacks.Create, request);

                return Json(response);
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse<CustomerFeedbackDto>
                {
                    Success = false,
                    ErrorMessage = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Cập nhật phản hồi
        /// </summary>
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerFeedbackDto request)
        {
            try
            {
                var endpoint = string.Format(ApiEndpoints.CustomerFeedbacks.Update, id);
                var response = await _apiService.PutAsync<ApiResponse<CustomerFeedbackDto>>(endpoint, request);

                return Json(response);
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse<CustomerFeedbackDto>
                {
                    Success = false,
                    ErrorMessage = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Xóa phản hồi
        /// </summary>
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var endpoint = string.Format(ApiEndpoints.CustomerFeedbacks.Delete, id);
                var response = await _apiService.DeleteAsync<ApiResponse<bool>>(endpoint);

                return Json(response);
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse<bool>
                {
                    Success = false,
                    ErrorMessage = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Lấy danh sách khách hàng cho dropdown
        /// </summary>
        [HttpGet("GetAvailableCustomers")]
        public async Task<IActionResult> GetAvailableCustomers()
        {
            try
            {
                var response = await _apiService.GetAsync<List<CustomerDto>>(ApiEndpoints.Customers.GetAllForDropdown);
                
                if (response.Success && response.Data != null)
                {
                    var customers = response.Data.Select(c => new
                    {
                        value = c.Id.ToString(),
                        text = c.Name + (c.Phone != null ? " - " + c.Phone : "")
                    }).Cast<object>().ToList();
                    
                    return Json(customers);
                }

                return Json(new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new List<object>());
            }
        }

        /// <summary>
        /// Lấy danh sách service orders cho dropdown
        /// </summary>
        [HttpGet("GetAvailableServiceOrders")]
        public async Task<IActionResult> GetAvailableServiceOrders()
        {
            try
            {
                var response = await _apiService.GetAsync<PagedResponse<ServiceOrderDto>>(ApiEndpoints.ServiceOrders.GetAll + "?pageNumber=1&pageSize=1000");
                
                if (response != null && response.Success && response.Data != null && response.Data.Data != null && response.Data.Data.Any())
                {
                    var orders = response.Data.Data.Select(o => new
                    {
                        value = o.Id.ToString(),
                        text = o.OrderNumber + (o.Customer != null && o.Customer.Name != null ? " - " + o.Customer.Name : "")
                    }).Cast<object>().ToList();
                    
                    return Json(orders);
                }

                return Json(new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new List<object>());
            }
        }

        /// <summary>
        /// Lấy danh sách nhân viên cho dropdown
        /// </summary>
        [HttpGet("GetAvailableEmployees")]
        public async Task<IActionResult> GetAvailableEmployees()
        {
            try
            {
                var response = await _apiService.GetAsync<List<EmployeeDto>>(ApiEndpoints.Employees.GetActive);
                
                if (response != null && response.Success && response.Data != null && response.Data.Count > 0)
                {
                    var employees = response.Data.Select(e => new
                    {
                        value = e.Id.ToString(),
                        text = e.Name + (e.Position != null ? " - " + e.Position : "")
                    }).Cast<object>().ToList();
                    
                    return Json(employees);
                }

                return Json(new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new List<object>());
            }
        }

        /// <summary>
        /// Lấy danh sách kênh phản hồi cho dropdown
        /// </summary>
        [HttpGet("GetAvailableFeedbackChannels")]
        public async Task<IActionResult> GetAvailableFeedbackChannels()
        {
            try
            {
                // Load từ API - cần tạo endpoint hoặc load trực tiếp
                var response = await _apiService.GetAsync<List<FeedbackChannelDto>>("/api/feedbackchannels/active");
                
                if (response != null && response.Success && response.Data != null && response.Data.Count > 0)
                {
                    var channels = response.Data.Select(c => new
                    {
                        value = c.Id.ToString(),
                        text = c.Name
                    }).Cast<object>().ToList();
                    
                    return Json(channels);
                }

                // Fallback: Return empty list nếu không có API endpoint
                return Json(new List<object>());
            }
            catch (Exception ex)
            {
                // Fallback: Return default channels
                return Json(new[]
                {
                    new { value = "1", text = "Hotline" },
                    new { value = "2", text = "Tại gara" },
                    new { value = "3", text = "Email" },
                    new { value = "4", text = "Ứng dụng di động" },
                    new { value = "5", text = "Mạng xã hội" },
                    new { value = "6", text = "Survey hậu mãi" }
                });
            }
        }
    }
}

