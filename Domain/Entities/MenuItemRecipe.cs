using Domain.Exceptions;

namespace Domain.Entities;

public class MenuItemRecipe
{
    private readonly List<RecipeComponent> _components = [];

    public Guid Id { get; private set; }
    public Guid MenuItemId { get; private set; }
    public IReadOnlyCollection<RecipeComponent> Components => _components.AsReadOnly();

    private MenuItemRecipe()
    {
    }

    private MenuItemRecipe(Guid menuItemId)
    {
        Id = Guid.NewGuid();
        MenuItemId = menuItemId;
    }

    public static MenuItemRecipe Create(
        Guid menuItemId,
        IEnumerable<(Guid IngredientId, decimal Quantity)> components)
    {
        if (menuItemId == Guid.Empty)
            throw new DomainException("Recipe must reference a valid menu item.");

        var recipe = new MenuItemRecipe(menuItemId);
        recipe.ReplaceComponents(components);
        return recipe;
    }

    public void ReplaceComponents(
        IEnumerable<(Guid IngredientId, decimal Quantity)> components)
    {
        ArgumentNullException.ThrowIfNull(components);

        var componentList = components.ToList();
        if (componentList.Count == 0)
            throw new DomainException("Recipe must contain at least one ingredient.");

        if (componentList.Any(component => component.IngredientId == Guid.Empty))
            throw new DomainException("Recipe components must reference valid ingredients.");

        if (componentList.Any(component => component.Quantity <= 0))
            throw new DomainException("Recipe component quantity must be greater than zero.");

        if (componentList.GroupBy(component => component.IngredientId).Any(group => group.Count() > 1))
            throw new DomainException("An ingredient can only appear once in a recipe.");

        _components.Clear();
        foreach (var component in componentList)
        {
            var recipeComponent = RecipeComponent.Create(component.IngredientId, component.Quantity);
            recipeComponent.AssignToRecipe(Id);
            _components.Add(recipeComponent);
        }
    }
}
