using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Entities;

public class ModifierGroup
{
    private readonly List<Modifier> _modifiers = [];
    private readonly List<ModifierGroupMenuItem> _menuItems = [];

    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public SelectionType SelectionType { get; private set; }
    public int MinSelection { get; private set; }
    public int MaxSelection { get; private set; }
    public bool IsRequired { get; private set; }

    public IReadOnlyList<Modifier> Modifiers => _modifiers.AsReadOnly();
    public IReadOnlyList<ModifierGroupMenuItem> MenuItems => _menuItems.AsReadOnly();

    private ModifierGroup()
    {
    }

    private ModifierGroup(string name, SelectionType selectionType, int minSelection, int maxSelection, bool isRequired)
    {
        Id = Guid.NewGuid();
        Name = name;
        SelectionType = selectionType;
        MinSelection = minSelection;
        MaxSelection = maxSelection;
        IsRequired = isRequired;
    }

    public static ModifierGroup Create(string name, SelectionType selectionType, int minSelection, int maxSelection, bool isRequired)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Modifier group name cannot be empty.");

        var group = new ModifierGroup(name.Trim(), selectionType, minSelection, maxSelection, isRequired);
        group.EnforceSelectionConstraints();
        return group;
    }

    public void Update(string name, SelectionType selectionType, int minSelection, int maxSelection, bool isRequired)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Modifier group name cannot be empty.");

        Name = name.Trim();
        SelectionType = selectionType;
        MinSelection = minSelection;
        MaxSelection = maxSelection;
        IsRequired = isRequired;
        EnforceSelectionConstraints();
    }

    public Modifier AddModifier(string name, decimal price, int displayOrder)
    {
        var modifier = Modifier.Create(Id, name, price, displayOrder);
        _modifiers.Add(modifier);
        return modifier;
    }

    public void RemoveModifier(Modifier modifier)
    {
        _modifiers.Remove(modifier);
    }

    public Modifier? GetModifierById(Guid modifierId)
    {
        return _modifiers.FirstOrDefault(m => m.Id == modifierId);
    }

    public void AttachMenuItem(Guid menuItemId)
    {
        if (_menuItems.Any(m => m.MenuItemId == menuItemId))
            throw new DomainException("This modifier group is already attached to this menu item.");

        _menuItems.Add(new ModifierGroupMenuItem { ModifierGroupId = Id, MenuItemId = menuItemId });
    }

    public void DetachMenuItem(Guid menuItemId)
    {
        var link = _menuItems.FirstOrDefault(m => m.MenuItemId == menuItemId);
        if (link is null)
            throw new DomainException("This modifier group is not attached to this menu item.");

        _menuItems.Remove(link);
    }

    public IReadOnlyList<Modifier> GetAvailableModifiers()
    {
        return _modifiers.Where(m => m.IsAvailable).ToList().AsReadOnly();
    }

    public void ValidateSelection(IList<Guid> selectedModifierIds)
    {
        var availableModifierIds = _modifiers
            .Where(m => m.IsAvailable)
            .Select(m => m.Id)
            .ToHashSet();

        foreach (var id in selectedModifierIds)
        {
            if (!availableModifierIds.Contains(id))
                throw new DomainException($"Modifier with id '{id}' is not available in group '{Name}'.");
        }

        if (SelectionType == SelectionType.Single && selectedModifierIds.Count > 1)
            throw new DomainException($"Modifier group '{Name}' allows only a single selection.");

        if (selectedModifierIds.Count < MinSelection)
            throw new DomainException($"Modifier group '{Name}' requires at least {MinSelection} selection(s).");

        if (selectedModifierIds.Count > MaxSelection)
            throw new DomainException($"Modifier group '{Name}' allows at most {MaxSelection} selection(s).");
    }

    private void EnforceSelectionConstraints()
    {
        if (MinSelection < 0)
            throw new DomainException("MinSelection cannot be negative.");

        if (MaxSelection < MinSelection)
            throw new DomainException("MaxSelection cannot be less than MinSelection.");

        if (SelectionType == SelectionType.Single)
        {
            MinSelection = Math.Min(MinSelection, 1);
            MaxSelection = Math.Min(MaxSelection, 1);
        }
    }
}
