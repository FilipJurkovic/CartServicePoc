namespace CartServicePoc.Domain;

public class Cart
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = default!;
    public List<CartItem> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public decimal Total => Items.Sum(i => i.LineTotal);

}