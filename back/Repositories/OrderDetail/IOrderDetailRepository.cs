using Back.Data.Infrastructure.EF.Models;

namespace back.Repositories.OrderDetail;

public interface IOrderDetailRepository
{
    Task<IList<OrderDetailDao>> GetAllAsync();
    Task<OrderDetailDao?> GetByIdAsync(int id);
    Task<IList<OrderDetailDao>> GetByOrderIdAsync(int orderId);
    Task<OrderDetailDao> AddAsync(OrderDetailDao orderDetail);
    Task<OrderDetailDao> UpdateAsync(OrderDetailDao orderDetail);
    Task<bool> DeleteAsync(int id);
}
