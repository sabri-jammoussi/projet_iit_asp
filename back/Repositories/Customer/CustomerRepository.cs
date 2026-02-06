using Back.Data.Infrastructure.EF.Models;
using Back.Data.Infrastructure.EF;
using Microsoft.EntityFrameworkCore;

namespace back.Repositories.Customer;

public class CustomerRepository : ICustomerRepository
{
    private readonly OltpDbContext _dbContext;

    public CustomerRepository(OltpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IList<CustomerDao>> GetAllAsync()
    {
        return await _dbContext.Customers
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<CustomerDao?> GetByIdAsync(int id)
    {
        var customerById = await _dbContext.Customers
            .Include(c => c.Orders)
            .FirstOrDefaultAsync(c => c.Id == id);
        return customerById;
    }

    public async Task<CustomerDao?> GetByEmailAsync(string email)
    {
        var existingEmail = await _dbContext.Customers
            .FirstOrDefaultAsync(c => c.Email == email);
        return existingEmail;
    }

    public async Task<CustomerDao> AddAsync(CustomerDao customer)
    {
        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync();
        return customer;
    }

    public async Task<CustomerDao> UpdateAsync(CustomerDao customer)
    {
        _dbContext.Customers.Update(customer);
        await _dbContext.SaveChangesAsync();
        return customer;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var customer = await _dbContext.Customers.FindAsync(id);
        if (customer == null)
            return false;

        _dbContext.Customers.Remove(customer);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _dbContext.Customers.AnyAsync(c => c.Id == id);
    }
}
