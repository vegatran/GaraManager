using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Enums;

namespace GarageManagementSystem.Core.Interfaces
{
    public interface ICustomerReceptionRepository : IGenericRepository<CustomerReception>
    {
        /// <summary>
        /// Lấy danh sách tiếp đón với thông tin khách hàng và xe
        /// </summary>
        Task<IEnumerable<CustomerReception>> GetAllWithDetailsAsync();

        /// <summary>
        /// Lấy tiếp đón theo ID với thông tin chi tiết
        /// </summary>
        Task<CustomerReception?> GetByIdWithDetailsAsync(int id);

        /// <summary>
        /// Lấy danh sách tiếp đón theo trạng thái
        /// </summary>
        Task<IEnumerable<CustomerReception>> GetByStatusAsync(ReceptionStatus status);

        /// <summary>
        /// Lấy danh sách tiếp đón theo kỹ thuật viên được phân công
        /// </summary>
        Task<IEnumerable<CustomerReception>> GetByAssignedTechnicianAsync(int technicianId);

        /// <summary>
        /// Lấy danh sách tiếp đón chờ kiểm tra (Pending)
        /// </summary>
        Task<IEnumerable<CustomerReception>> GetPendingReceptionsAsync();

        /// <summary>
        /// Kiểm tra xe có đang trong quy trình xử lý không
        /// </summary>
        Task<bool> IsVehicleInProcessAsync(int vehicleId);

        /// <summary>
        /// Lấy tiếp đón theo số phiếu
        /// </summary>
        Task<CustomerReception?> GetByReceptionNumberAsync(string receptionNumber);

        /// <summary>
        /// Cập nhật trạng thái tiếp đón
        /// </summary>
        Task<bool> UpdateStatusAsync(int id, ReceptionStatus status);

        /// <summary>
        /// Phân công kỹ thuật viên
        /// </summary>
        Task<bool> AssignTechnicianAsync(int id, int technicianId);

        /// <summary>
        /// Lấy thống kê theo trạng thái
        /// </summary>
        Task<Dictionary<ReceptionStatus, int>> GetStatusStatisticsAsync();

        /// <summary>
        /// Lấy danh sách tiếp đón theo mức độ ưu tiên
        /// </summary>
        Task<IEnumerable<CustomerReception>> GetByPriorityAsync(string priority);
    }
}
