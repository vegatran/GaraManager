namespace GarageManagementSystem.Web.Configuration
{
    /// <summary>
    /// Centralized API endpoints configuration
    /// </summary>
    public static class ApiEndpoints
    {
        /// <summary>
        /// Dashboard related endpoints
        /// </summary>
        public static class Dashboard
        {
            public const string Statistics = "dashboard/statistics";
        }

        /// <summary>
        /// Customer management endpoints
        /// </summary>
        public static class Customers
        {
            public const string GetAll = "customers";
            public const string GetById = "customers/{0}"; // customers/{id}
            public const string Create = "customers";
            public const string Update = "customers/{0}"; // customers/{id}
            public const string Delete = "customers/{0}"; // customers/{id}
            public const string Search = "customers/search?term={0}"; // customers/search?term={searchTerm}
        }

        /// <summary>
        /// Vehicle management endpoints
        /// </summary>
        public static class Vehicles
        {
            public const string GetAll = "vehicles";
            public const string GetById = "vehicles/{0}"; // vehicles/{id}
            public const string Create = "vehicles";
            public const string Update = "vehicles/{0}"; // vehicles/{id}
            public const string Delete = "vehicles/{0}"; // vehicles/{id}
            public const string GetByCustomerId = "vehicles/customer/{0}"; // vehicles/customer/{customerId}
        }

        /// <summary>
        /// Service management endpoints
        /// </summary>
        public static class Services
        {
            public const string GetAll = "services";
            public const string GetById = "services/{0}"; // services/{id}
            public const string Create = "services";
            public const string Update = "services/{0}"; // services/{id}
            public const string Delete = "services/{0}"; // services/{id}
        }

        /// <summary>
        /// Service Order management endpoints
        /// </summary>
        public static class ServiceOrders
        {
            public const string GetAll = "serviceorders";
            public const string GetById = "serviceorders/{0}"; // serviceorders/{id}
            public const string Create = "serviceorders";
            public const string Update = "serviceorders/{0}"; // serviceorders/{id}
            public const string Delete = "serviceorders/{0}"; // serviceorders/{id}
            public const string GetByCustomerId = "serviceorders/customer/{0}"; // serviceorders/customer/{customerId}
            public const string GetByVehicleId = "serviceorders/vehicle/{0}"; // serviceorders/vehicle/{vehicleId}
        }

        /// <summary>
        /// Employee management endpoints
        /// </summary>
        public static class Employees
        {
            public const string GetAll = "employees";
            public const string GetById = "employees/{0}"; // employees/{id}
            public const string Create = "employees";
            public const string Update = "employees/{0}"; // employees/{id}
            public const string Delete = "employees/{0}"; // employees/{id}
        }

        /// <summary>
        /// Reports endpoints
        /// </summary>
        public static class Reports
        {
            public const string SalesReport = "reports/sales";
            public const string ServiceReport = "reports/services";
            public const string CustomerReport = "reports/customers";
            public const string RevenueReport = "reports/revenue";
        }

        /// <summary>
        /// Authentication and authorization endpoints
        /// </summary>
        public static class Auth
        {
            public const string Login = "auth/login";
            public const string Logout = "auth/logout";
            public const string RefreshToken = "auth/refresh";
            public const string UserProfile = "auth/profile";
        }

        /// <summary>
        /// Utility methods for building endpoints with parameters
        /// </summary>
        public static class Builder
        {
            /// <summary>
            /// Build endpoint with single ID parameter
            /// </summary>
            public static string WithId(string template, int id)
            {
                return string.Format(template, id);
            }

            /// <summary>
            /// Build endpoint with single string parameter
            /// </summary>
            public static string WithParameter(string template, string parameter)
            {
                return string.Format(template, parameter);
            }

            /// <summary>
            /// Build endpoint with multiple parameters
            /// </summary>
            public static string WithParameters(string template, params object[] parameters)
            {
                return string.Format(template, parameters);
            }
        }
    }
}
