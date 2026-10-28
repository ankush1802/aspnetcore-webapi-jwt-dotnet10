namespace ProductsApi.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public int Stock { get; set; }

    // Internal fields: these never reach the AI
    public decimal CostPrice { get; set; }
    public string Supplier { get; set; } = "";
}
