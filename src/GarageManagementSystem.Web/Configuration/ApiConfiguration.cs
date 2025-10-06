namespace GarageManagementSystem.Web.Configuration
{
    /// <summary>
    /// API configuration settings
    /// </summary>
    public class ApiConfiguration
    {
        public static string BaseUrl { get; set; } = "https://localhost:44303/api/";
        public const string HttpClientName = "GarageAPI";
        
        /// <summary>
        /// API timeout in seconds
        /// </summary>
        public static int TimeoutSeconds { get; set; } = 30;
        
        /// <summary>
        /// Retry policy configuration
        /// </summary>
        public const int MaxRetryAttempts = 3;
        public const int RetryDelaySeconds = 2;
    }
}
