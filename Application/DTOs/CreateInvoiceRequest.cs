using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.DTOs;

public record CreateInvoiceRequest(
    SalesChannel Channel,
    [Required, MinLength(1)] IReadOnlyList<CreateInvoiceItemRequest> Items,
    [Range(0, double.MaxValue)] decimal DiscountAmount = 0,
    [Range(0, double.MaxValue)] decimal TaxAmount = 0);

public record CreateInvoiceItemRequest(
    Guid MenuItemId,
    [Range(typeof(decimal), "0.001", "1000000")] decimal Quantity,
    IReadOnlyList<CreateInvoiceItemAddonRequest>? Addons = null);

public record CreateInvoiceItemAddonRequest(
    Guid MenuAddonId,
    [Range(typeof(decimal), "0.001", "1000000")] decimal Quantity = 1);
