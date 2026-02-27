using CartServicePoc.Application.Interfaces;
using CartServicePoc.Domain;
using CartServicePoc.Infrastructure.Cache;

namespace CartServicePoc.Application.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CartService> _logger;

    private static string CacheKey(string userId) => $"cart:{userId}";

    public CartService(
        ICartRepository cartRepository,
        ICacheService cacheService,
        ILogger<CartService> logger)
    {
        _cartRepository = cartRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Cart?> GetCartAsync(string userId, CancellationToken ct = default)
    {
        // 1. Provjeri cache
        var cached = await _cacheService.GetAsync<Cart>(CacheKey(userId), ct);
        if (cached is not null)
        {
            _logger.LogInformation("Cache HIT za user {UserId}", userId);
            return cached;
        }

        // 2. Cache miss - dohvati iz baze
        _logger.LogInformation("Cache MISS za user {UserId}", userId);
        var cart = await _cartRepository.GetCartAsync(userId, ct);
        if (cart is null) return null;

        // 3. Spremi u cache
        await _cacheService.SetAsync(CacheKey(userId), cart, ct);
        return cart;
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
        await _cacheService.RemoveAsync(CacheKey(userId), ct);

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
        await _cacheService.RemoveAsync(CacheKey(userId), ct);

        return await _cartRepository.GetCartAsync(userId, ct);
    }

    public async Task DeleteItemAsync(string userId, string productId, CancellationToken ct = default)
    {
        await _cartRepository.DeleteItemAsync(userId, productId, ct);
        await _cacheService.RemoveAsync(CacheKey(userId), ct);
    }

    public async Task DeleteCartAsync(string userId, CancellationToken ct = default)
    {
        await _cartRepository.DeleteCartAsync(userId, ct);
        await _cacheService.RemoveAsync(CacheKey(userId), ct);
    }
}