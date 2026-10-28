using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProductsApi.Models;

namespace ProductsApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<IdentityUser>(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Wireless Mouse",       Price = 799,  Stock = 42, CostPrice = 410,  Supplier = "Apex Traders" },
            new Product { Id = 2, Name = "Mechanical Keyboard",  Price = 3499, Stock = 6,  CostPrice = 2100, Supplier = "KeyWorks" },
            new Product { Id = 3, Name = "USB-C Hub",            Price = 1899, Stock = 3,  CostPrice = 950,  Supplier = "Apex Traders" },
            new Product { Id = 4, Name = "27-inch Monitor",      Price = 15999, Stock = 11, CostPrice = 11800, Supplier = "ViewLine" },
            new Product { Id = 5, Name = "Laptop Stand",         Price = 1299, Stock = 2,  CostPrice = 520,  Supplier = "DeskPro" },
            new Product { Id = 6, Name = "Noise-Cancelling Headphones", Price = 6999, Stock = 18, CostPrice = 4300, Supplier = "SoundHub" },
            new Product { Id = 7, Name = "Webcam 1080p",         Price = 2499, Stock = 9,  CostPrice = 1350, Supplier = "ViewLine" },
            new Product { Id = 8, Name = "Mouse Pad XL",         Price = 499,  Stock = 75, CostPrice = 160,  Supplier = "DeskPro" }
        );
    }
}
