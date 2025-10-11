using GarageManagementSystem.Core.Entities;

namespace GarageManagementSystem.Core.Interfaces
{
    public interface IAppointmentRepository : IGenericRepository<Appointment>
    {
        Task<IEnumerable<Appointment>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<Appointment>> GetByVehicleIdAsync(int vehicleId);
        Task<IEnumerable<Appointment>> GetByDateAsync(DateTime date);
        Task<IEnumerable<Appointment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Appointment>> GetByStatusAsync(string status);
        Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync();
        Task<IEnumerable<Appointment>> GetTodayAppointmentsAsync();
        Task<bool> IsTimeSlotAvailableAsync(DateTime scheduledDateTime, int duration, int? excludeId = null);
        Task<string> GenerateAppointmentNumberAsync();
    }
}

