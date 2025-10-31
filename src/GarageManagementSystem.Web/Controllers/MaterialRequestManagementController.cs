using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using GarageManagementSystem.Web.Configuration;
using GarageManagementSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GarageManagementSystem.Web.Controllers
{
    [Authorize]
    [Route("MaterialRequestManagement")]
    public class MaterialRequestManagementController : Controller
    {
        private readonly ApiService _apiService;

        public MaterialRequestManagementController(ApiService apiService)
        {
            _apiService = apiService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("GetMRsPaged")]
        public async Task<IActionResult> GetMRsPaged(int pageNumber = 1, int pageSize = 10, int? serviceOrderId = null, string? status = null)
        {
            try
            {
                var query = new List<string>
                {
                    $"pageNumber={pageNumber}",
                    $"pageSize={pageSize}"
                };
                if (serviceOrderId.HasValue) query.Add($"serviceOrderId={serviceOrderId.Value}");
                if (!string.IsNullOrEmpty(status)) query.Add($"status={Uri.EscapeDataString(status)}");

                var endpoint = ApiEndpoints.MaterialRequests.GetAll + "?" + string.Join("&", query);
                var response = await _apiService.GetAsync<PagedResponse<MaterialRequestDto>>(endpoint);

                if (response.Success && response.Data != null)
                {
                    var list = response.Data.Data.Select(m => new
                    {
                        id = m.Id,
                        mrNumber = m.MRNumber,
                        serviceOrderId = m.ServiceOrderId,
                        status = m.Status.ToString(),
                        createdAt = m.Id > 0 ? "" : "" // placeholder
                    }).Cast<object>().ToList();

                    return Json(new
                    {
                        success = true,
                        data = list,
                        totalCount = response.Data.TotalCount,
                        pageNumber = response.Data.PageNumber,
                        pageSize = response.Data.PageSize
                    });
                }

                return Json(new { success = false, data = new List<object>(), totalCount = 0, pageNumber, pageSize, message = response.ErrorMessage });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, data = new List<object>(), totalCount = 0, pageNumber, pageSize, message = ex.Message });
            }
        }

        [HttpPost("CreateMR")]
        public async Task<IActionResult> CreateMR([FromBody] CreateMaterialRequestDto dto)
        {
            var response = await _apiService.PostAsync<MaterialRequestDto>(ApiEndpoints.MaterialRequests.Create, dto);
            return Json(response);
        }

        [HttpPut("Submit/{id}")]
        public async Task<IActionResult> Submit(int id)
        {
            var response = await _apiService.PutAsync<object>(ApiEndpoints.Builder.WithId(ApiEndpoints.MaterialRequests.Submit, id), new { });
            return Json(response);
        }

        [HttpPut("Approve/{id}")]
        public async Task<IActionResult> Approve(int id)
        {
            var response = await _apiService.PutAsync<object>(ApiEndpoints.Builder.WithId(ApiEndpoints.MaterialRequests.Approve, id), new { });
            return Json(response);
        }

        [HttpPut("Reject/{id}")]
        public async Task<IActionResult> Reject(int id, [FromBody] ChangeMaterialRequestStatusDto dto)
        {
            var response = await _apiService.PutAsync<object>(ApiEndpoints.Builder.WithId(ApiEndpoints.MaterialRequests.Reject, id), dto);
            return Json(response);
        }
    }
}


