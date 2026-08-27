using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public record CreateOrderItemAddonRequest(
    [Required] Guid ModifierId,
    [Range(typeof(decimal), "0.001", "1000000")] decimal Quantity = 1);

public record CreateOrderItemRequest(
    [Required] Guid MenuItemId,
    [Range(1, 1000)] int Quantity,
    IReadOnlyList<CreateOrderItemAddonRequest>? Addons = null);

public record RegisterOrderRequest(
    [Required, MinLength(1)] IReadOnlyList<CreateOrderItemRequest> Items,
    string? TableNumber = null);
