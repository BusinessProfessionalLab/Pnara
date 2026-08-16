using Application.DTOs;
using Application.Interfaces;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/modifiers")]
    public class ModifiersController : ControllerBase
    {
        private readonly IModifiersService _service;

        public ModifiersController(IModifiersService service)
        {
            _service = service;
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ModifierDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken)
        {
            var modifier = await _service.GetByIdAsync(id, cancellationToken);

            return modifier is null ? NotFound() : Ok(modifier);
        }

        [HttpGet("menu-item/{menuItemId:guid}")]
        [ProducesResponseType(typeof(IReadOnlyList<ModifierDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByMenuItemId(
            Guid menuItemId,
            CancellationToken cancellationToken)
        {
            var modifiers = await _service.GetByMenuItemIdAsync(menuItemId, cancellationToken);

            return Ok(modifiers);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ModifierDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(
            [FromBody] CreateModifierRequest request,
            CancellationToken cancellationToken)
        {
            var created = await _service.CreateAsync(request, cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Id },
                created);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ModifierDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateModifierRequest request,
            CancellationToken cancellationToken)
        {
            var updated = await _service.UpdateAsync(id, request, cancellationToken);

            return updated is null ? NotFound() : Ok(updated);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(
            Guid id,
            CancellationToken cancellationToken)
        {
            var deleted = await _service.DeleteAsync(id, cancellationToken);

            return deleted ? NoContent() : NotFound();
        }
    }
}
