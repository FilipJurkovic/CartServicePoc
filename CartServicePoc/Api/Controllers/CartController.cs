using CartServicePoc.Api.DTOs;
using CartServicePoc.Application.Interfaces;
using CartServicePoc.Domain;
using Microsoft.AspNetCore.Mvc;

namespace CartService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;
    private readonly ILogger<CartController> _logger;

    public CartController(ICartService cartService, ILogger<CartController> logger)
    {
        _cartService = cartService;
        _logger = logger;
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetCart(string userId, CancellationToken ct)
    {
        var cart = await _cartService.GetCartAsync(userId, ct);
        if (cart is null)
            return NotFound(new { message = $"Cart for user {userId} not found" });

        return Ok(MapToResponse(cart));
    }

    [HttpPost("{userId}/items")]
    public async Task<IActionResult> AddItem(
        string userId,
        [FromBody] AddCartItemRequest request,
        CancellationToken ct)
    {
        var item = new CartItem
        {
            Id = Guid.NewGuid(),
            ProductId = request.ProductId,
            ProductName = request.ProductName,
            UnitPrice = request.UnitPrice,
            Quantity = request.Quantity,
            ImageUrl = request.ImageUrl
        };

        var cart = await _cartService.AddOrUpdateItemAsync(userId, item, ct);
        return Ok(MapToResponse(cart));
    }

    [HttpPut("{userId}/items/{productId}")]
    public async Task<IActionResult> UpdateItem(
        string userId,
        string productId,
        [FromBody] UpdateCartItemRequest request,
        CancellationToken ct)
    {
        var cart = await _cartService.UpdateItemQuantityAsync(userId, productId, request.Quantity, ct);
        if (cart is null)
            return NotFound(new { message = "Cart or item not found" });

        return Ok(MapToResponse(cart));
    }

    [HttpDelete("{userId}/items/{productId}")]
    public async Task<IActionResult> DeleteItem(
        string userId,
        string productId,
        CancellationToken ct)
    {
        await _cartService.DeleteItemAsync(userId, productId, ct);
        return NoContent();
    }

    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteCart(string userId, CancellationToken ct)
    {
        await _cartService.DeleteCartAsync(userId, ct);
        return NoContent();
    }

    private static CartResponse MapToResponse(Cart cart) => new(
        cart.Id,
        cart.UserId,
        cart.Items.Select(i => new CartItemResponse(
            i.Id,
            i.ProductId,
            i.ProductName,
            i.ImageUrl,
            i.UnitPrice,
            i.Quantity,
            i.LineTotal
        )).ToList(),
        cart.Total,
        cart.UpdatedAt
    );
}