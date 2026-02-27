using System.Data;
using CartServicePoc.Application.Interfaces;
using CartServicePoc.Domain;
using Dapper;

namespace CartServicePoc.Infrastructure.Repositories;

public class CartRepository : ICartRepository
{
    private readonly IDbConnection _connection;

    public CartRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<Cart?> GetCartAsync(string userId, CancellationToken ct = default)
    {
        var sql = """
            SELECT c.* FROM Carts c WHERE c.UserId = @UserId;
            SELECT ci.* FROM CartItems ci
            INNER JOIN Carts c ON ci.CartId = c.Id
            WHERE c.UserId = @UserId;
            """;

        using var multi = await _connection.QueryMultipleAsync(sql, new { UserId = userId });

        var cart = await multi.ReadFirstOrDefaultAsync<Cart>();
        if (cart is null) return null;

        cart.Items = (await multi.ReadAsync<CartItem>()).ToList();
        return cart;
    }

    public async Task<Cart> CreateCartAsync(Cart cart, CancellationToken ct = default)
    {
        var sql = """
            INSERT INTO Carts (Id, UserId, CreatedAt, UpdatedAt)
            VALUES (@Id, @UserId, @CreatedAt, @UpdatedAt);
            """;

        await _connection.ExecuteAsync(sql, cart);
        return cart;
    }

    public async Task<Cart> UpdateCartAsync(Cart cart, CancellationToken ct = default)
    {
        var sql = """
            UPDATE Carts 
            SET UpdatedAt = @UpdatedAt
            WHERE UserId = @UserId;
            """;

        await _connection.ExecuteAsync(sql, cart);
        return cart;
    }

    public async Task DeleteCartAsync(string userId, CancellationToken ct = default)
    {
        var sql = "DELETE FROM Carts WHERE UserId = @UserId;";
        await _connection.ExecuteAsync(sql, new { UserId = userId });
    }

    public async Task UpsertItemAsync(string userId, CartItem item, CancellationToken ct = default)
    {
        var sql = """
            MERGE CartItems AS target
            USING (
                SELECT c.Id AS CartId 
                FROM Carts c 
                WHERE c.UserId = @UserId
            ) AS source ON target.CartId = source.CartId 
                        AND target.ProductId = @ProductId
            WHEN MATCHED THEN
                UPDATE SET 
                    Quantity = target.Quantity + @Quantity,
                    UnitPrice = @UnitPrice,
                    ProductName = @ProductName,
                    ImageUrl = @ImageUrl
            WHEN NOT MATCHED THEN
                INSERT (Id, CartId, ProductId, ProductName, UnitPrice, Quantity, ImageUrl, AddedAt)
                VALUES (
                    NEWID(),
                    source.CartId,
                    @ProductId,
                    @ProductName,
                    @UnitPrice,
                    @Quantity,
                    @ImageUrl,
                    GETUTCDATE()
                );

            UPDATE Carts SET UpdatedAt = GETUTCDATE() WHERE UserId = @UserId;
            """;

        await _connection.ExecuteAsync(sql, new
        {
            UserId = userId,
            item.ProductId,
            item.ProductName,
            item.UnitPrice,
            item.Quantity,
            item.ImageUrl
        });
    }

    public async Task DeleteItemAsync(string userId, string productId, CancellationToken ct = default)
    {
        var sql = """
            DELETE ci FROM CartItems ci
            INNER JOIN Carts c ON ci.CartId = c.Id
            WHERE c.UserId = @UserId AND ci.ProductId = @ProductId;
            
            UPDATE Carts SET UpdatedAt = GETUTCDATE() WHERE UserId = @UserId;
            """;

        await _connection.ExecuteAsync(sql, new { UserId = userId, ProductId = productId });
    }
}
