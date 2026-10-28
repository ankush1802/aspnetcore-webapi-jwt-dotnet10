using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;
using ProductsApi.Data;

namespace ProductsApi.Tools;

public record ProductDto(int Id, string Name, decimal Price, int Stock);

[McpServerToolType]
public class ProductTools(AppDbContext db)
{
    [McpServerTool, Description("Search products by name. Returns at most 20 results.")]
    public async Task<List<ProductDto>> SearchProducts(
        [Description("Part of the product name, e.g. 'mouse'")] string term)
        => await db.Products.AsNoTracking()
            .Where(p => EF.Functions.ILike(p.Name, $"%{term}%"))
            .Take(20)
            .Select(p => new ProductDto(p.Id, p.Name, p.Price, p.Stock))
            .ToListAsync();

    [McpServerTool, Description("Get one product by its id.")]
    public async Task<ProductDto?> GetProduct(int id)
        => await db.Products.AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new ProductDto(p.Id, p.Name, p.Price, p.Stock))
            .FirstOrDefaultAsync();

    [McpServerTool, Description("List products whose stock is below the given number.")]
    public async Task<List<ProductDto>> LowStock(
        [Description("Stock threshold, e.g. 10")] int below)
        => await db.Products.AsNoTracking()
            .Where(p => p.Stock < below)
            .OrderBy(p => p.Stock)
            .Take(20)
            .Select(p => new ProductDto(p.Id, p.Name, p.Price, p.Stock))
            .ToListAsync();
}
