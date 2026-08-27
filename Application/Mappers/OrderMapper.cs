using Application.Common;
using Application.DTOs;
using Domain.Entities;

namespace Application.Mappers;

public static class OrderMapper
{
    public static OrderItemAddonResponse ToResponse(this OrderItemAddon addon) =>
        new(addon.Id, addon.ModifierId, addon.AddonName, addon.Quantity, addon.UnitPrice, addon.LineTotal);

    public static OrderItemResponse ToResponse(this OrderItem item) =>
        new(
            item.Id,
            item.MenuItemId,
            item.ProductName,
            item.UnitPrice.Amount,
            item.UnitPrice.Currency,
            item.Quantity,
            item.LineTotal.Amount,
            item.Addons.Select(ToResponse).ToList());

    public static OrderResponse ToResponse(this Order order, Invoice? invoice = null) =>
        new(
            order.Id,
            order.OrderNumber,
            order.Channel.ToString(),
            order.Status.ToString(),
            order.CreatedAtUtc,
            PersianDateTime.ToJalaliString(order.CreatedAtUtc),
            order.RegisteredAtUtc,
            order.TableNumber,
            order.CustomerName,
            order.CustomerPhoneNumber,
            order.DeliveryAddressTitle,
            order.DeliveryAddressLine,
            order.DeliveryCity,
            order.DeliveryPostalCode,
            order.DeliveryPhoneNumber,
            order.RejectionReason,
            order.Items.Count > 0 ? order.CalculateSubTotal().Amount : null,
            order.Items.Select(ToResponse).ToList(),
            invoice?.ToResponse(order));
}
