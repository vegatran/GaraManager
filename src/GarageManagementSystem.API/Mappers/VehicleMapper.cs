using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Shared.DTOs;

namespace GarageManagementSystem.Api.Mappers
{
    public static class VehicleMapper
    {
        public static VehicleDto ToDto(Vehicle vehicle)
        {
            if (vehicle == null) return null;

            return new VehicleDto
            {
                Id = vehicle.Id,
                LicensePlate = vehicle.LicensePlate,
                Brand = vehicle.Brand,
                Model = vehicle.Model,
                Year = vehicle.Year,
                Color = vehicle.Color,
                VIN = vehicle.VIN,
                EngineNumber = vehicle.EngineNumber,
                CustomerId = vehicle.CustomerId,
                CreatedAt = vehicle.CreatedAt,
                UpdatedAt = vehicle.UpdatedAt
            };
        }

        public static Vehicle ToEntity(CreateVehicleDto dto)
        {
            if (dto == null) return null;

            return new Vehicle
            {
                LicensePlate = dto.LicensePlate,
                Brand = dto.Brand,
                Model = dto.Model,
                Year = dto.Year,
                Color = dto.Color,
                VIN = dto.VIN,
                EngineNumber = dto.EngineNumber,
                CustomerId = dto.CustomerId
            };
        }

        public static void UpdateEntity(Vehicle entity, UpdateVehicleDto dto)
        {
            if (entity == null || dto == null) return;

            entity.LicensePlate = dto.LicensePlate;
            entity.Brand = dto.Brand;
            entity.Model = dto.Model;
            entity.Year = dto.Year;
            entity.Color = dto.Color;
            entity.VIN = dto.VIN;
            entity.EngineNumber = dto.EngineNumber;
            entity.CustomerId = dto.CustomerId;
        }

        public static List<VehicleDto> ToDtoList(IEnumerable<Vehicle> vehicles)
        {
            if (vehicles == null) return new List<VehicleDto>();

            return vehicles.Select(ToDto).ToList();
        }
    }
}
