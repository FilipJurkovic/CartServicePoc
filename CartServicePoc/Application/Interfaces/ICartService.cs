using CartServicePoc.Domain;

namespace CartServicePoc.Application.Interfaces;

public interface ICartService
{
    Task<Cart?> GetCartAsync(string userId, CancellationToken ct = default);
    Task<Cart> AddOrUpdateItemAsync(string userId, CartItem item, CancellationToken ct = default);
    Task<Cart?> UpdateItemQuantityAsync(string userId, string productId, int quantity, CancellationToken ct = default);
    Task DeleteItemAsync(string userId, string productId, CancellationToken ct = default);
    Task DeleteCartAsync(string userId, CancellationToken ct = default);

}