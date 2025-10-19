using System.Text.Json.Serialization;

namespace GarageManagementSystem.Shared.Models
{
    /// <summary>
    /// Generic paged response for pagination
    /// </summary>
    public class PagedResponse<T>
    {
        [JsonPropertyName("data")]
        public IEnumerable<T> Data { get; set; } = new List<T>();

        [JsonPropertyName("pageNumber")]
        public int PageNumber { get; set; }

        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; }

        [JsonPropertyName("totalCount")]
        public int TotalCount { get; set; }

        [JsonPropertyName("totalPages")]
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        [JsonPropertyName("hasPrevious")]
        public bool HasPrevious => PageNumber > 1;

        [JsonPropertyName("hasNext")]
        public bool HasNext => PageNumber < TotalPages;

        [JsonPropertyName("success")]
        public bool Success { get; set; } = true;

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("errors")]
        public List<string>? Errors { get; set; }

        public static PagedResponse<T> CreateSuccessResult(IEnumerable<T> data, int pageNumber, int pageSize, int totalCount, string? message = null)
        {
            return new PagedResponse<T>
            {
                Data = data,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                Success = true,
                Message = message
            };
        }

        public static PagedResponse<T> CreateErrorResult(string errorMessage, List<string>? errors = null)
        {
            return new PagedResponse<T>
            {
                Data = new List<T>(),
                PageNumber = 1,
                PageSize = 10,
                TotalCount = 0,
                Success = false,
                Message = errorMessage,
                Errors = errors
            };
        }
    }
}
