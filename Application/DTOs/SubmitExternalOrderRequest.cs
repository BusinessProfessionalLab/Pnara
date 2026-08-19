using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public record ExternalOrderItemRequest(
    [Required] Guid MenuItemId,
    [Range(1, 1000)] int Quantity);

public record SubmitExternalOrderRequest(
    [Required] IReadOnlyList<ExternalOrderItemRequest> Items,
    [Required] Guid AddressId);
