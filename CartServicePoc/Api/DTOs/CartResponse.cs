namespace CartServicePoc.Api.DTOs;

public record CartResponse(
    Guid Id,
    string UserId,
    List<CartItemResponse> Items,
    decimal Total,
    DateTime UpdatedAt
);

public record CartItemResponse(
    Guid Id,
    string ProductId,
    string ProductName,
    string? ImageUrl,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal
);