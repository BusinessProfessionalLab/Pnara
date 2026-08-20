using Domain.Exceptions;

namespace Domain.Entities;

public class MenuAddonRecipeComponent
{
    public Guid Id { get; private set; }
    public Guid RecipeId { get; private set; }
    public Guid IngredientId { get; private set; }
    public decimal Quantity { get; private set; }

    private MenuAddonRecipeComponent()
    {
    }

    private MenuAddonRecipeComponent(Guid ingredientId, decimal quantity)
    {
        Id = Guid.NewGuid();
        IngredientId = ingredientId;
        Quantity = quantity;
    }

    internal static MenuAddonRecipeComponent Create(Guid ingredientId, decimal quantity)
    {
        if (ingredientId == Guid.Empty)
            throw new DomainException("Add-on recipe component must reference a valid ingredient.");

        if (quantity <= 0)
            throw new DomainException("Add-on recipe component quantity must be greater than zero.");

        return new MenuAddonRecipeComponent(ingredientId, quantity);
    }

    internal void AssignToRecipe(Guid recipeId)
    {
        if (recipeId == Guid.Empty)
            throw new DomainException("Add-on recipe ID cannot be empty.");

        if (RecipeId != Guid.Empty && RecipeId != recipeId)
            throw new DomainException("Add-on recipe component already belongs to another recipe.");

        RecipeId = recipeId;
    }
}
