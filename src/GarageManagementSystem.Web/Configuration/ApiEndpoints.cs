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
            public const string Statistics = "Analytics/dashboard";
        }

        /// <summary>
        /// Customer management endpoints
        /// </summary>
        public static class Customers
        {
            public const string GetAll = "customers";
            public const string GetAllForDropdown = "customers/dropdown";
            public const string GetById = "customers/{0}"; // customers/{id}
            public const string Create = "customers";
            public const string Update = "customers/{0}"; // customers/{id}
            public const string Delete = "customers/{0}"; // customers/{id}
            public const string Search = "customers/search?term={0}"; // customers/search?term={searchTerm}
        }

        /// <summary>
        /// Customer Reception management endpoints
        /// </summary>
        public static class CustomerReceptions
        {
            public const string GetAll = "CustomerReceptions";
            public const string GetAllForDropdown = "CustomerReceptions/dropdown";
            public const string GetInspectionEligibleForDropdown = "CustomerReceptions/dropdown/inspection-eligible";
            public const string GetById = "CustomerReceptions/{0}"; // CustomerReceptions/{id}
            public const string Create = "CustomerReceptions";
            public const string Update = "CustomerReceptions/{0}"; // CustomerReceptions/{id}
            public const string Delete = "CustomerReceptions/{0}"; // CustomerReceptions/{id}
        }

        /// <summary>
        /// Vehicle management endpoints
        /// </summary>
        public static class Vehicles
        {
            public const string GetAll = "vehicles";
            public const string GetAllForDropdown = "vehicles/dropdown";
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
            public const string GetActive = "employees/active";
            public const string GetById = "employees/{0}"; // employees/{id}
            public const string GetByDepartment = "employees/department/{0}"; // employees/department/{dept}
            public const string Create = "employees";
            public const string Update = "employees/{0}"; // employees/{id}
            public const string Delete = "employees/{0}"; // employees/{id}
        }

        /// <summary>
        /// Department management endpoints
        /// </summary>
        public static class Departments
        {
            public const string GetAll = "departments";
            public const string GetById = "departments/{0}"; // departments/{id}
        }

        /// <summary>
        /// Position management endpoints
        /// </summary>
        public static class Positions
        {
            public const string GetAll = "positions";
            public const string GetById = "positions/{0}"; // positions/{id}
        }

        /// <summary>
        /// Vehicle Inspection endpoints
        /// </summary>
        public static class VehicleInspections
        {
            public const string GetAll = "vehicleinspections";
            public const string GetById = "vehicleinspections/{0}";
            public const string GetByVehicle = "vehicleinspections/vehicle/{0}";
            public const string GetByCustomer = "vehicleinspections/customer/{0}";
            public const string GetByStatus = "vehicleinspections/status/{0}";
            public const string GetLatestByVehicle = "vehicleinspections/latest/vehicle/{0}";
            public const string Create = "vehicleinspections";
            public const string Update = "vehicleinspections/{0}";
            public const string Delete = "vehicleinspections/{0}";
            public const string Complete = "vehicleinspections/{0}/complete";
        }

        /// <summary>
        /// Service Quotation endpoints
        /// </summary>
        public static class ServiceQuotations
        {
            public const string GetAll = "servicequotations";
            public const string GetAllForDropdown = "servicequotations/dropdown";
            public const string GetById = "servicequotations/{0}";
            public const string GetByVehicle = "servicequotations/vehicle/{0}";
            public const string GetByCustomer = "servicequotations/customer/{0}";
            public const string GetByInspection = "servicequotations/inspection/{0}";
            public const string GetByStatus = "servicequotations/status/{0}";
            public const string Create = "servicequotations";
            public const string Update = "servicequotations/{0}";
            public const string Delete = "servicequotations/{0}";
            public const string Approve = "servicequotations/{0}/approve";
            public const string Reject = "servicequotations/{0}/reject";
            public const string Send = "servicequotations/{0}/send";
            public const string UpdateInsuranceApprovedPricing = "servicequotations/{0}/insurance-approved-pricing";
            public const string GetInsuranceApprovedPricing = "servicequotations/{0}/insurance-approved-pricing";
            public const string UpdateCorporateApprovedPricing = "servicequotations/{0}/corporate-approved-pricing";
            public const string GetCorporateApprovedPricing = "servicequotations/{0}/corporate-approved-pricing";
            public const string UpdateStatus = "servicequotations/{0}/status";
        }

        /// <summary>
        /// Quotation Attachments endpoints
        /// </summary>
        public static class QuotationAttachments
        {
            public const string GetByQuotationId = "quotationattachments/quotation/{0}";
            public const string GetInsuranceDocumentsByQuotationId = "quotationattachments/quotation/{0}/insurance";
            public const string Upload = "quotationattachments/upload";
            public const string Download = "quotationattachments/{0}/download";
            public const string Delete = "quotationattachments/{0}";
        }

        /// <summary>
        /// Parts Management endpoints
        /// </summary>
        public static class Parts
        {
            public const string GetAll = "parts";
            public const string GetById = "parts/{0}";
            public const string GetLowStock = "parts/low-stock";
            public const string GetByCategory = "parts/category/{0}";
            public const string Search = "parts/search";
            public const string Create = "parts";
            public const string Update = "parts/{0}";
            public const string Delete = "parts/{0}";
        }

        /// <summary>
        /// Suppliers endpoints
        /// </summary>
        public static class Suppliers
        {
            public const string GetAll = "suppliers";
            public const string GetActive = "suppliers/active";
            public const string GetById = "suppliers/{0}";
            public const string Create = "suppliers";
            public const string Update = "suppliers/{0}";
            public const string Delete = "suppliers/{0}";
        }

        /// <summary>
        /// Payment Transactions endpoints
        /// </summary>
        public static class PaymentTransactions
        {
            public const string GetAll = "paymenttransactions";
            public const string GetById = "paymenttransactions/{0}";
            public const string GetByServiceOrder = "paymenttransactions/serviceorder/{0}";
            public const string Create = "paymenttransactions";
            public const string Update = "paymenttransactions/{0}";
            public const string Delete = "paymenttransactions/{0}";
        }

        /// <summary>
        /// Stock Transactions endpoints
        /// </summary>
        public static class StockTransactions
        {
            public const string GetAll = "stocktransactions";
            public const string GetByPart = "stocktransactions/part/{0}";
            public const string GetById = "stocktransactions/{0}";
            public const string Create = "stocktransactions";
            public const string Update = "stocktransactions/{0}";
            public const string Delete = "stocktransactions/{0}";
            public const string ImportOpeningBalance = "stocktransactions/opening-balance";
            public const string ValidateOpeningBalance = "stocktransactions/opening-balance/validate";
            public const string ImportExcel = "stocktransactions/import-excel";
            public const string ValidateExcel = "stocktransactions/validate-excel";
            public const string DownloadTemplate = "stocktransactions/download-template";
            public const string CreatePurchaseOrder = "stocktransactions/purchase-order";
        }

        /// <summary>
        /// Purchase Orders endpoints
        /// </summary>
        public static class PurchaseOrders
        {
            public const string GetAll = "purchaseorders";
            public const string GetById = "purchaseorders/{0}";
            public const string Create = "purchaseorders";
            public const string Update = "purchaseorders/{0}";
            public const string Delete = "purchaseorders/{0}";
            public const string ReceiveOrder = "purchaseorders/{0}/receive";
            public const string GetBySupplier = "purchaseorders?supplierId={0}";
            public const string GetByStatus = "purchaseorders?status={0}";
        }

        /// <summary>
        /// Insurance Invoices endpoints
        /// </summary>
        public static class InsuranceInvoices
        {
            public const string GetAll = "insuranceinvoices";
            public const string GetById = "insuranceinvoices/{0}";
            public const string Create = "insuranceinvoices";
            public const string Update = "insuranceinvoices/{0}";
            public const string Delete = "insuranceinvoices/{0}";
            public const string ExportPdf = "insuranceinvoices/{0}/export/pdf";
            public const string ExportExcel = "insuranceinvoices/{0}/export/excel";
        }

        /// <summary>
        /// Appointments endpoints
        /// </summary>
        public static class Appointments
        {
            public const string GetAll = "appointments";
            public const string GetToday = "appointments/today";
            public const string GetUpcoming = "appointments/upcoming";
            public const string GetById = "appointments/{0}";
            public const string GetTypes = "appointments/types";
            public const string Create = "appointments";
            public const string Update = "appointments/{0}";
            public const string Delete = "appointments/{0}";
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
        /// Setup endpoints
        /// </summary>
        public static class Setup
        {
            public const string GetCounts = "setup/counts";
            public const string CreateDemoData = "setup/create/{0}"; // setup/create/{module}
            public const string ClearAllData = "setup/clear-all";
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

        /// <summary>
        /// Print Template management endpoints
        /// </summary>
        public static class PrintTemplates
        {
            public const string GetAll = "printtemplates";
            public const string GetById = "printtemplates/{0}"; // printtemplates/{id}
            public const string GetDefault = "printtemplates/default/{0}"; // printtemplates/default/{templateType}
            public const string GetByType = "printtemplates/type/{0}"; // printtemplates/type/{templateType}
            public const string Create = "printtemplates";
            public const string Update = "printtemplates/{0}"; // printtemplates/{id}
            public const string Delete = "printtemplates/{0}"; // printtemplates/{id}
            public const string SetDefault = "printtemplates/{0}/set-default"; // printtemplates/{id}/set-default
            public const string CreateDefaultQuotation = "printtemplates/create-default-quotation";
        }
    }
}
