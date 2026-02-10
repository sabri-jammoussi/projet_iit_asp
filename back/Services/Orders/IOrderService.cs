using back.DTOs;

namespace back.Services.Orders;

public interface IOrderService
{
    Task<IList<OrderDto>> GetAllAsync();
    Task<OrderDto?> GetByIdAsync(int id);
    Task<IList<OrderDto>> GetByCustomerIdAsync(int customerId);
    Task<IList<OrderDto>> GetByAccountIdAsync();
    Task<OrderDto> CreateByAccountIdAsync(CreateOrderDto dto);
    Task<OrderDto?> UpdateStatusAsync(int id, string status);
    Task<bool> DeleteAsync(int id);
}
