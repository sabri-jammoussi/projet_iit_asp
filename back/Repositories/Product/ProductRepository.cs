using Back.Data.Infrastructure.EF.Models;
using Back.Data.Infrastructure.EF;
using Microsoft.EntityFrameworkCore;

namespace back.Repositories.Product;

public class ProductRepository : IProductRepository
{
    private readonly OltpDbContext _dbContext;

    public ProductRepository(OltpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IList<ProductDao>> GetAllAsync()
    {
        return await _dbContext.Products
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<ProductDao?> GetByIdAsync(int id)
    {
        return await _dbContext.Products
            .Include(p => p.OrderDetails)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<ProductDao> AddAsync(ProductDao product)
    {
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();
        return product;
    }

    public async Task<ProductDao> UpdateAsync(ProductDao product)
    {
        _dbContext.Products.Update(product);
        await _dbContext.SaveChangesAsync();
        return product;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _dbContext.Products.FindAsync(id);
        if (product == null)
            return false;

        _dbContext.Products.Remove(product);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _dbContext.Products.AnyAsync(p => p.Id == id);
    }

    public async Task<IList<ProductDao>> GetByCategoryAsync(string category)
    {
        return await _dbContext.Products
            .Where(p => p.Category == category)
            .AsNoTracking()
            .ToListAsync();
    }
}
