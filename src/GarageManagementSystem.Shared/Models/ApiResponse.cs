namespace GarageManagementSystem.Shared.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
        public string? StackTrace { get; set; } // For debugging
        public System.Net.HttpStatusCode StatusCode { get; set; } = System.Net.HttpStatusCode.OK;
        public bool RequiresLogin { get; set; } = false;

        public static ApiResponse<T> SuccessResult(T data, string message = "Thành công")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> ErrorResult(string message, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }

        public static ApiResponse<T> ErrorResult(string message, string error)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = new List<string> { error }
            };
        }

        /// <summary>
        /// Tạo error response từ Exception với đầy đủ thông tin để debug
        /// </summary>
        public static ApiResponse<T> ErrorResult(string message, Exception ex, bool includeStackTrace = true)
        {
            var errors = new List<string> { ex.Message };
            
            // Thêm inner exceptions
            var innerEx = ex.InnerException;
            while (innerEx != null)
            {
                errors.Add($"Inner: {innerEx.Message}");
                innerEx = innerEx.InnerException;
            }
            
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors,
                StackTrace = includeStackTrace ? ex.StackTrace : null
            };
        }
    }

    public class ApiResponse : ApiResponse<object>
    {
        public static ApiResponse SuccessResult(string message = "Thành công")
        {
            return new ApiResponse
            {
                Success = true,
                Message = message
            };
        }

        public static new ApiResponse ErrorResult(string message, List<string>? errors = null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }

        public static new ApiResponse ErrorResult(string message, string error)
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                Errors = new List<string> { error }
            };
        }

        /// <summary>
        /// Tạo error response từ Exception với đầy đủ thông tin để debug
        /// </summary>
        public static ApiResponse ErrorResult(string message, Exception ex, bool includeStackTrace = true)
        {
            var errors = new List<string> { ex.Message };
            
            // Thêm inner exceptions
            var innerEx = ex.InnerException;
            while (innerEx != null)
            {
                errors.Add($"Inner: {innerEx.Message}");
                innerEx = innerEx.InnerException;
            }
            
            return new ApiResponse
            {
                Success = false,
                Message = message,
                Errors = errors,
                StackTrace = includeStackTrace ? ex.StackTrace : null
            };
        }
    }
}
