using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Authorize(Policy = "AdminOnly")]
[Route("api/inventory")]
public class InventoryController(InventoryService inventoryService) : ControllerBase
{
    [HttpPost("units")]
    [ProducesResponseType(typeof(MeasurementUnitResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateUnit(
        CreateMeasurementUnitRequest request,
        CancellationToken cancellationToken = default) =>
        StatusCode(
            StatusCodes.Status201Created,
            await inventoryService.CreateUnitAsync(request, cancellationToken));

    [HttpPut("units/{id:guid}")]
    [ProducesResponseType(typeof(MeasurementUnitResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateUnit(
        Guid id,
        UpdateMeasurementUnitRequest request,
        CancellationToken cancellationToken = default) =>
        Ok(await inventoryService.UpdateUnitAsync(id, request, cancellationToken));

    [HttpGet("units")]
    [ProducesResponseType(typeof(IReadOnlyList<MeasurementUnitResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnits(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default) =>
        Ok(await inventoryService.GetUnitsAsync(includeInactive, cancellationToken));

    [HttpPost("ingredients")]
    [ProducesResponseType(typeof(IngredientResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateIngredient(
        CreateIngredientRequest request,
        CancellationToken cancellationToken = default) =>
        StatusCode(
            StatusCodes.Status201Created,
            await inventoryService.CreateIngredientAsync(request, cancellationToken));

    [HttpPut("ingredients/{id:guid}")]
    [ProducesResponseType(typeof(IngredientResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateIngredient(
        Guid id,
        UpdateIngredientRequest request,
        CancellationToken cancellationToken = default) =>
        Ok(await inventoryService.UpdateIngredientAsync(id, request, cancellationToken));

    [HttpGet("ingredients")]
    [ProducesResponseType(typeof(IReadOnlyList<IngredientResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetIngredients(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default) =>
        Ok(await inventoryService.GetIngredientsAsync(
            includeInactive,
            lowStockOnly: false,
            cancellationToken));

    [HttpGet("low-stock")]
    [ProducesResponseType(typeof(IReadOnlyList<IngredientResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLowStock(
        CancellationToken cancellationToken = default) =>
        Ok(await inventoryService.GetIngredientsAsync(
            includeInactive: false,
            lowStockOnly: true,
            cancellationToken));

    [HttpPost("ingredients/{id:guid}/adjustments")]
    [ProducesResponseType(typeof(IngredientResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> AdjustStock(
        Guid id,
        AdjustStockRequest request,
        CancellationToken cancellationToken = default) =>
        Ok(await inventoryService.AdjustStockAsync(id, request, cancellationToken));

    [HttpGet("ingredients/{id:guid}/ledger")]
    [ProducesResponseType(typeof(IReadOnlyList<StockLedgerEntryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLedger(
        Guid id,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        CancellationToken cancellationToken = default) =>
        Ok(await inventoryService.GetLedgerAsync(
            id,
            fromUtc,
            toUtc,
            cancellationToken));

    [HttpPut("recipes/menu-items/{menuItemId:guid}")]
    [ProducesResponseType(typeof(MenuItemRecipeResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ReplaceMenuItemRecipe(
        Guid menuItemId,
        ReplaceMenuItemRecipeRequest request,
        CancellationToken cancellationToken = default) =>
        Ok(await inventoryService.ReplaceMenuItemRecipeAsync(
            menuItemId,
            request,
            cancellationToken));

    [HttpGet("recipes/menu-items/{menuItemId:guid}")]
    [ProducesResponseType(typeof(MenuItemRecipeResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMenuItemRecipe(
        Guid menuItemId,
        CancellationToken cancellationToken = default) =>
        Ok(await inventoryService.GetMenuItemRecipeAsync(menuItemId, cancellationToken));

    [HttpPut("recipes/menu-addons/{menuAddonId:guid}")]
    [ProducesResponseType(typeof(MenuAddonRecipeResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ReplaceMenuAddonRecipe(
        Guid menuAddonId,
        ReplaceMenuItemRecipeRequest request,
        CancellationToken cancellationToken = default) =>
        Ok(await inventoryService.ReplaceMenuAddonRecipeAsync(
            menuAddonId,
            request,
            cancellationToken));

    [HttpGet("recipes/menu-addons/{menuAddonId:guid}")]
    [ProducesResponseType(typeof(MenuAddonRecipeResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMenuAddonRecipe(
        Guid menuAddonId,
        CancellationToken cancellationToken = default) =>
        Ok(await inventoryService.GetMenuAddonRecipeAsync(menuAddonId, cancellationToken));
}
