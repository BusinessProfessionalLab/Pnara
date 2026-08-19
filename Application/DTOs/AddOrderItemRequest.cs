using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public record AddOrderItemRequest(
    [Required] Guid MenuItemId,
    [Range(1, 1000)] int Quantity);
