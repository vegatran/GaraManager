namespace GarageManagementSystem.Core.Enums
{
    /// <summary>
    /// Trạng thái tiếp đón khách hàng
    /// </summary>
    public enum ReceptionStatus
    {
        /// <summary>
        /// Chờ xử lý
        /// </summary>
        Pending = 0,
        
        /// <summary>
        /// Đã phân công kỹ thuật
        /// </summary>
        Assigned = 1,
        
        /// <summary>
        /// Đang kiểm tra
        /// </summary>
        InProgress = 2,
        
        /// <summary>
        /// Đã hoàn thành kiểm tra
        /// </summary>
        Completed = 3,
        
        /// <summary>
        /// Đã hủy
        /// </summary>
        Cancelled = 4
    }

    /// <summary>
    /// Trạng thái kiểm tra xe
    /// </summary>
    public enum InspectionStatus
    {
        /// <summary>
        /// Chờ kiểm tra
        /// </summary>
        Pending = 0,
        
        /// <summary>
        /// Đang kiểm tra
        /// </summary>
        InProgress = 1,
        
        /// <summary>
        /// Đã hoàn thành kiểm tra
        /// </summary>
        Completed = 2,
        
        /// <summary>
        /// Đã hủy
        /// </summary>
        Cancelled = 3
    }

    /// <summary>
    /// Trạng thái báo giá
    /// </summary>
    public enum QuotationStatus
    {
        /// <summary>
        /// Nháp
        /// </summary>
        Draft = 0,
        
        /// <summary>
        /// Đã gửi
        /// </summary>
        Sent = 1,
        
        /// <summary>
        /// Đã duyệt
        /// </summary>
        Approved = 2,
        
        /// <summary>
        /// Đã từ chối
        /// </summary>
        Rejected = 3,
        
        /// <summary>
        /// Đã hết hạn
        /// </summary>
        Expired = 4,
        
        /// <summary>
        /// Đã hủy
        /// </summary>
        Cancelled = 5
    }

    /// <summary>
    /// Trạng thái phiếu sửa chữa
    /// </summary>
    public enum ServiceOrderStatus
    {
        /// <summary>
        /// Chờ xử lý
        /// </summary>
        Pending = 0,
        
        /// <summary>
        /// Đang sửa chữa
        /// </summary>
        InProgress = 1,
        
        /// <summary>
        /// Đã hoàn thành
        /// </summary>
        Completed = 2,
        
        /// <summary>
        /// Đã hủy
        /// </summary>
        Cancelled = 3
    }

    /// <summary>
    /// Mức độ ưu tiên
    /// </summary>
    public enum PriorityLevel
    {
        /// <summary>
        /// Thấp
        /// </summary>
        Low = 0,
        
        /// <summary>
        /// Bình thường
        /// </summary>
        Normal = 1,
        
        /// <summary>
        /// Cao
        /// </summary>
        High = 2,
        
        /// <summary>
        /// Khẩn cấp
        /// </summary>
        Urgent = 3
    }

    /// <summary>
    /// Loại dịch vụ
    /// </summary>
    public enum ServiceType
    {
        /// <summary>
        /// Tổng quát
        /// </summary>
        General = 0,
        
        /// <summary>
        /// Khẩn cấp
        /// </summary>
        Emergency = 1,
        
        /// <summary>
        /// Bảo dưỡng
        /// </summary>
        Maintenance = 2,
        
        /// <summary>
        /// Sửa chữa
        /// </summary>
        Repair = 3,
        
        /// <summary>
        /// Kiểm tra
        /// </summary>
        Inspection = 4
    }

    /// <summary>
    /// Loại báo giá
    /// </summary>
    public enum QuotationType
    {
        /// <summary>
        /// Cá nhân - Không cần gửi bảo hiểm duyệt
        /// </summary>
        Personal = 0,
        
        /// <summary>
        /// Bảo hiểm - Cần gửi bảo hiểm duyệt
        /// </summary>
        Insurance = 1,
        
        /// <summary>
        /// Công ty - Cần gửi công ty duyệt
        /// </summary>
        Company = 2
    }
}
