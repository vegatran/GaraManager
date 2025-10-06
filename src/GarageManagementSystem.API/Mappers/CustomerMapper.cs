using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Shared.DTOs;

namespace GarageManagementSystem.Api.Mappers
{
    public static class CustomerMapper
    {
        public static CustomerDto ToDto(Customer customer)
        {
            if (customer == null) return null;

            return new CustomerDto
            {
                Id = customer.Id,
                Name = customer.Name,
                Phone = customer.Phone,
                Email = customer.Email,
                Address = customer.Address,
                DateOfBirth = customer.DateOfBirth,
                Gender = customer.Gender,
                CreatedAt = customer.CreatedAt,
                UpdatedAt = customer.UpdatedAt
            };
        }

        public static Customer ToEntity(CreateCustomerDto dto)
        {
            if (dto == null) return null;

            return new Customer
            {
                Name = dto.Name,
                Phone = dto.Phone,
                Email = dto.Email,
                Address = dto.Address,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender
            };
        }

        public static void UpdateEntity(Customer entity, UpdateCustomerDto dto)
        {
            if (entity == null || dto == null) return;

            entity.Name = dto.Name;
            entity.Phone = dto.Phone;
            entity.Email = dto.Email;
            entity.Address = dto.Address;
            entity.DateOfBirth = dto.DateOfBirth;
            entity.Gender = dto.Gender;
        }

        public static List<CustomerDto> ToDtoList(IEnumerable<Customer> customers)
        {
            if (customers == null) return new List<CustomerDto>();

            return customers.Select(ToDto).ToList();
        }
    }
}
