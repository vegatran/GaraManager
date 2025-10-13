using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GarageManagementSystem.Infrastructure.Repositories
{
    public class AppointmentRepository : GenericRepository<Appointment>, IAppointmentRepository
    {
        public AppointmentRepository(GarageDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Appointment>> GetByCustomerIdAsync(int customerId)
        {
            return await _dbSet
                .Where(a => !a.IsDeleted && a.CustomerId == customerId)
                .Include(a => a.Vehicle)
                .Include(a => a.AssignedTo)
                .OrderByDescending(a => a.ScheduledDateTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetByVehicleIdAsync(int vehicleId)
        {
            return await _dbSet
                .Where(a => !a.IsDeleted && a.VehicleId == vehicleId)
                .Include(a => a.Customer)
                .OrderByDescending(a => a.ScheduledDateTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetByDateAsync(DateTime date)
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1);
            
            return await _dbSet
                .Where(a => !a.IsDeleted && a.ScheduledDateTime >= startDate && a.ScheduledDateTime < endDate)
                .Include(a => a.Customer)
                .Include(a => a.Vehicle)
                .Include(a => a.AssignedTo)
                .OrderBy(a => a.ScheduledDateTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(a => !a.IsDeleted && a.ScheduledDateTime >= startDate && a.ScheduledDateTime <= endDate)
                .Include(a => a.Customer)
                .Include(a => a.Vehicle)
                .OrderBy(a => a.ScheduledDateTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetByStatusAsync(string status)
        {
            return await _dbSet
                .Where(a => !a.IsDeleted && a.Status == status)
                .Include(a => a.Customer)
                .Include(a => a.Vehicle)
                .OrderBy(a => a.ScheduledDateTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync()
        {
            var now = DateTime.Now;
            return await _dbSet
                .Where(a => !a.IsDeleted && a.ScheduledDateTime > now && 
                       (a.Status == "Scheduled" || a.Status == "Confirmed"))
                .Include(a => a.Customer)
                .Include(a => a.Vehicle)
                .OrderBy(a => a.ScheduledDateTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetTodayAppointmentsAsync()
        {
            return await GetByDateAsync(DateTime.Today);
        }

        public async Task<bool> IsTimeSlotAvailableAsync(DateTime scheduledDateTime, int duration, int? excludeId = null)
        {
            var endTime = scheduledDateTime.AddMinutes(duration);
            
            var query = _dbSet.Where(a => !a.IsDeleted && 
                a.Status != "Cancelled" && a.Status != "Completed" &&
                ((a.ScheduledDateTime >= scheduledDateTime && a.ScheduledDateTime < endTime) ||
                 (a.ScheduledDateTime.AddMinutes(a.EstimatedDuration) > scheduledDateTime && 
                  a.ScheduledDateTime < scheduledDateTime)));
            
            if (excludeId.HasValue)
            {
                query = query.Where(a => a.Id != excludeId.Value);
            }
            
            return !await query.AnyAsync();
        }

        public async Task<string> GenerateAppointmentNumberAsync()
        {
            var today = DateTime.Now;
            var prefix = $"APT{today:yyyyMMdd}";
            
            var lastAppointment = await _dbSet
                .Where(a => a.AppointmentNumber.StartsWith(prefix))
                .OrderByDescending(a => a.AppointmentNumber)
                .FirstOrDefaultAsync();

            int sequence = 1;
            if (lastAppointment != null)
            {
                var lastSequence = lastAppointment.AppointmentNumber.Substring(prefix.Length);
                if (int.TryParse(lastSequence, out int lastSeq))
                {
                    sequence = lastSeq + 1;
                }
            }

            return $"{prefix}{sequence:D4}";
        }

        public async Task<IEnumerable<Appointment>> GetAllWithDetailsAsync()
        {
            return await _dbSet
                .Where(a => !a.IsDeleted)
                .Include(a => a.Customer)
                .Include(a => a.Vehicle)
                .Include(a => a.AssignedTo)
                .OrderByDescending(a => a.ScheduledDateTime)
                .ToListAsync();
        }

        public async Task<Appointment?> GetByIdWithDetailsAsync(int id)
        {
            return await _dbSet
                .Where(a => !a.IsDeleted && a.Id == id)
                .Include(a => a.Customer)
                .Include(a => a.Vehicle)
                .Include(a => a.AssignedTo)
                .FirstOrDefaultAsync();
        }
    }
}

