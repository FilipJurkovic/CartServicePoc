using CartServicePoc.Domain;

namespace CartServicePoc.Application.Interfaces;

public interface ICartRepository
{
    Task<Cart?> GetCartAsync(string userId, CancellationToken ct = default);
    Task<Cart> CreateCartAsync(Cart cart, CancellationToken ct = default);
    Task<Cart> UpdateCartAsync(Cart cart, CancellationToken ct = default);
    Task DeleteCartAsync(string userId, CancellationToken ct = default);
    Task UpsertItemAsync(string userId, CartItem item, CancellationToken ct = default);
    Task DeleteItemAsync(string userId, string productId, CancellationToken ct = default);
}