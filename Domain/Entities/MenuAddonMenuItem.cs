using Domain.Exceptions;

namespace Domain.Entities;

public class MenuAddonMenuItem
{
    public Guid MenuAddonId { get; private set; }
    public Guid MenuItemId { get; private set; }

    private MenuAddonMenuItem()
    {
    }

    private MenuAddonMenuItem(Guid menuAddonId, Guid menuItemId)
    {
        MenuAddonId = menuAddonId;
        MenuItemId = menuItemId;
    }

    public static MenuAddonMenuItem Create(Guid menuAddonId, Guid menuItemId)
    {
        if (menuAddonId == Guid.Empty)
            throw new DomainException("Menu add-on applicability must reference a valid add-on.");

        if (menuItemId == Guid.Empty)
            throw new DomainException("Menu add-on applicability must reference a valid menu item.");

        return new MenuAddonMenuItem(menuAddonId, menuItemId);
    }
}
