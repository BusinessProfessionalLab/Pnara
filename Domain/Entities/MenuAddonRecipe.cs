using Domain.Exceptions;

namespace Domain.Entities;

public class MenuAddonRecipe
{
    private readonly List<MenuAddonRecipeComponent> _components = [];

    public Guid Id { get; private set; }
    public Guid MenuAddonId { get; private set; }
    public IReadOnlyCollection<MenuAddonRecipeComponent> Components => _components.AsReadOnly();

    private MenuAddonRecipe()
    {
    }

    private MenuAddonRecipe(Guid menuAddonId)
    {
        Id = Guid.NewGuid();
        MenuAddonId = menuAddonId;
    }

    public static MenuAddonRecipe Create(
        Guid menuAddonId,
        IEnumerable<(Guid IngredientId, decimal Quantity)> components)
    {
        if (menuAddonId == Guid.Empty)
            throw new DomainException("Add-on recipe must reference a valid menu add-on.");

        var recipe = new MenuAddonRecipe(menuAddonId);
        recipe.ReplaceComponents(components);
        return recipe;
    }

    public void ReplaceComponents(
        IEnumerable<(Guid IngredientId, decimal Quantity)> components)
    {
        ArgumentNullException.ThrowIfNull(components);

        var componentList = components.ToList();
        if (componentList.Count == 0)
            throw new DomainException("Add-on recipe must contain at least one ingredient.");

        if (componentList.Any(component => component.IngredientId == Guid.Empty))
            throw new DomainException("Add-on recipe components must reference valid ingredients.");

        if (componentList.Any(component => component.Quantity <= 0))
            throw new DomainException("Add-on recipe component quantity must be greater than zero.");

        if (componentList.GroupBy(component => component.IngredientId).Any(group => group.Count() > 1))
            throw new DomainException("An ingredient can only appear once in an add-on recipe.");

        _components.Clear();
        foreach (var component in componentList)
        {
            var recipeComponent = MenuAddonRecipeComponent.Create(
                component.IngredientId,
                component.Quantity);
            recipeComponent.AssignToRecipe(Id);
            _components.Add(recipeComponent);
        }
    }
}
