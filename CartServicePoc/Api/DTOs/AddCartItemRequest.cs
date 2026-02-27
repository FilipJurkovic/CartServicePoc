namespace CartServicePoc.Api.DTOs;

public record AddCartItemRequest(
    string ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    string? ImageUrl = null
);
