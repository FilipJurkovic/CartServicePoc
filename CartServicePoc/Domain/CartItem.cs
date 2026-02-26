namespace CartServicePoc.Domain;

public class CartItem
{
    public Guid Id { get; set; }
    public Guid CartId { get; set; }
    public string ProductId { get; set; } = default!;
    public string ProductName { get; set; } = default!;
    public string? ImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal => UnitPrice * Quantity;
}