using Back.Data.Infrastructure.EF.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Back.Data.Infrastructure.EF;

public class OltpDbContext : DbContext
{
    private readonly DbContextOptions<OltpDbContext> _options;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OltpDbContext> _logger;

    public OltpDbContext(DbContextOptions<OltpDbContext> options, IConfiguration configuration, ILogger<OltpDbContext> logger)
    {
        _options = options;
        _configuration = configuration;
        _logger = logger;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var conn = new SqlConnectionStringBuilder(_configuration.GetConnectionString("UsersConnections"));

        _ = optionsBuilder
            .UseLazyLoadingProxies()
            .UseSqlServer(conn.ToString(), o => o.UseCompatibilityLevel(120));

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Customer -> Orders (One-to-Many)
        modelBuilder.Entity<CustomerDao>()
            .HasMany(c => c.Orders)
            .WithOne(o => o.Customer)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Order -> OrderDetails (One-to-Many)
        modelBuilder.Entity<OrderDao>()
            .HasMany(o => o.OrderDetails)
            .WithOne(od => od.Order)
            .HasForeignKey(od => od.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Product -> OrderDetails (One-to-Many)
        modelBuilder.Entity<ProductDao>()
            .HasMany(p => p.OrderDetails)
            .WithOne(od => od.Product)
            .HasForeignKey(od => od.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    // Authentication
    public DbSet<AccountDao> Accounts { get; set; }

    // OLTP Entities
    public DbSet<CustomerDao> Customers { get; set; }
    public DbSet<ProductDao> Products { get; set; }
    public DbSet<OrderDao> Orders { get; set; }
    public DbSet<OrderDetailDao> OrderDetails { get; set; }
}
