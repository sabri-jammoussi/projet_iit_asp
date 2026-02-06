using Back.Data.Infrastructure.EF.Models;

namespace back.Repositories.Customer;
public interface ICustomerRepository
{
    Task<IList<CustomerDao>> GetAllAsync();
    Task<CustomerDao?> GetByIdAsync(int id);
    Task<CustomerDao?> GetByEmailAsync(string email);
    Task<CustomerDao> AddAsync(CustomerDao customer);
    Task<CustomerDao> UpdateAsync(CustomerDao customer);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
