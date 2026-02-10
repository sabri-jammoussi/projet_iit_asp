using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Back.Data.Infrastructure.EF.Enums;
using Back.Data.Infrastructure.EF.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Notification.Models;

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

        // Account -> Customer (One-to-One, optional)
        modelBuilder.Entity<AccountDao>()
            .HasOne(a => a.Customer)
            .WithOne(c => c.Account)
            .HasForeignKey<CustomerDao>(c => c.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique index on Customer.AccountId (one account = one customer)
        modelBuilder.Entity<CustomerDao>()
            .HasIndex(c => c.AccountId)
            .IsUnique()
            .HasDatabaseName("IX_CUSTOMER_ACCOUNT_ID");

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
		#region seeds
		var hasher = new PasswordHasherX();

		var salt = Guid.Parse("bbba39f9-3e98-45ea-8d1a-b42731a63ceb").ToByteArray();
		var hash = hasher.Hash("123456", salt);
		modelBuilder.Entity<AccountDao>().HasData(new AccountDao
		{
			Id = 1,
			FirstName = "Admin",
			LastName = "Admin",
			Email = "Admin@Admin.tn",
			// PasswordHash and PasswordSalt can be null for seed if you don't need to login as this user immediately.
			PasswordHash = hash,
			PasswordSalt = salt,
			Role = UserRole.Admin
		});

		#endregion
	}

	// Authentication
	public DbSet<AccountDao> Accounts { get; set; }

    // OLTP Entities
    public DbSet<CustomerDao> Customers { get; set; }
    public DbSet<ProductDao> Products { get; set; }
    public DbSet<OrderDao> Orders { get; set; }
    public DbSet<OrderDetailDao> OrderDetails { get; set; }
	public DbSet<NotificationDao> Notifications { get; set; }


	
}
public class PasswordHasherX
{
	private readonly HMACSHA512 x = new HMACSHA512(Encoding.UTF8.GetBytes("realworld"));

	public byte[] Hash(string password, byte[] salt)
	{
		var bytes = Encoding.UTF8.GetBytes(password);

		var allBytes = new byte[bytes.Length + salt.Length];
		Buffer.BlockCopy(bytes, 0, allBytes, 0, bytes.Length);
		Buffer.BlockCopy(salt, 0, allBytes, bytes.Length, salt.Length);

		return x.ComputeHash(allBytes);
	}
}