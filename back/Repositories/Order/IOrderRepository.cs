using Back.Data.Infrastructure.EF.Models;

namespace back.Repositories.Order;

public interface IOrderRepository
{
    Task<IList<OrderDao>> GetAllAsync();
    Task<OrderDao?> GetByIdAsync(int id);
    Task<IList<OrderDao>> GetByCustomerIdAsync(int customerId);
    Task<OrderDao> AddAsync(OrderDao order);
    Task<OrderDao> UpdateAsync(OrderDao order);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
