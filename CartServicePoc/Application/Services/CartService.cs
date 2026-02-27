using CartServicePoc.Application.Interfaces;
using CartServicePoc.Domain;

namespace CartServicePoc.Application.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly ILogger<CartService> _logger;

    public CartService(ICartRepository cartRepository, ILogger<CartService> logger)
    {
        _cartRepository = cartRepository;
        _logger = logger;
    }

    public async Task<Cart?> GetCartAsync(string userId, CancellationToken ct = default)
    {
        return await _cartRepository.GetCartAsync(userId, ct);
    }

    public async Task<Cart> AddOrUpdateItemAsync(string userId, CartItem item, CancellationToken ct = default)
    {
        var cart = await _cartRepository.GetCartAsync(userId, ct);

        if (cart is null)
        {
            cart = new Cart
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _cartRepository.CreateCartAsync(cart, ct);
        }

        await _cartRepository.UpsertItemAsync(userId, item, ct);

        return await _cartRepository.GetCartAsync(userId, ct) ?? cart;
    }

    public async Task<Cart?> UpdateItemQuantityAsync(string userId, string productId, 
                                                      int quantity, CancellationToken ct = default)
    {
        var cart = await _cartRepository.GetCartAsync(userId, ct);
        if (cart is null) return null;

        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null) return null;

        item.Quantity = quantity;
        await _cartRepository.UpsertItemAsync(userId, item, ct);

        return await _cartRepository.GetCartAsync(userId, ct);
    }

    public async Task DeleteItemAsync(string userId, string productId, CancellationToken ct = default)
    {
        await _cartRepository.DeleteItemAsync(userId, productId, ct);
    }

    public async Task DeleteCartAsync(string userId, CancellationToken ct = default)
    {
        await _cartRepository.DeleteCartAsync(userId, ct);
    }
}