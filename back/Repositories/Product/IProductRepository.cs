using Back.Data.Infrastructure.EF.Models;

namespace back.Repositories.Product;

public interface IProductRepository
{
    Task<IList<ProductDao>> GetAllAsync();
    Task<ProductDao?> GetByIdAsync(int id);
    Task<ProductDao> AddAsync(ProductDao product);
    Task<ProductDao> UpdateAsync(ProductDao product);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<IList<ProductDao>> GetByCategoryAsync(string category);
}
