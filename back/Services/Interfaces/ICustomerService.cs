using back.DTOs;

namespace back.Services.Interfaces;

public interface ICustomerService
{
    Task<IList<CustomerDto>> GetAllAsync();
    Task<CustomerDto?> GetByIdAsync(int id);
    Task<CustomerDto> CreateAsync(CreateCustomerDto dto);
    Task<CustomerDto?> UpdateAsync(int id, UpdateCustomerDto dto);
    Task<bool> DeleteAsync(int id);
}
