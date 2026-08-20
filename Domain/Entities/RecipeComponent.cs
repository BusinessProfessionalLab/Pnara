using Domain.Exceptions;

namespace Domain.Entities;

public class RecipeComponent
{
    public Guid Id { get; private set; }
    public Guid RecipeId { get; private set; }
    public Guid IngredientId { get; private set; }
    public decimal Quantity { get; private set; }

    private RecipeComponent()
    {
    }

    private RecipeComponent(Guid ingredientId, decimal quantity)
    {
        Id = Guid.NewGuid();
        IngredientId = ingredientId;
        Quantity = quantity;
    }

    internal static RecipeComponent Create(Guid ingredientId, decimal quantity)
    {
        if (ingredientId == Guid.Empty)
            throw new DomainException("Recipe component must reference a valid ingredient.");

        if (quantity <= 0)
            throw new DomainException("Recipe component quantity must be greater than zero.");

        return new RecipeComponent(ingredientId, quantity);
    }

    internal void AssignToRecipe(Guid recipeId)
    {
        if (recipeId == Guid.Empty)
            throw new DomainException("Recipe ID cannot be empty.");

        if (RecipeId != Guid.Empty && RecipeId != recipeId)
            throw new DomainException("Recipe component already belongs to another recipe.");

        RecipeId = recipeId;
    }
}
