using System.Collections.Generic;
using Embervale.Items;
using Godot;

namespace Embervale.Crafting;

/// <summary>
/// A designer-authored recipe: a set of ingredient items consumed at a particular
/// <see cref="CraftingStationType"/> to produce an output item. Authored as a
/// <c>.tres</c> under <c>data/recipes/</c> and indexed by <see cref="RecipeDatabase"/> —
/// a new recipe is a new resource, no code change.
///
/// Ingredients are an untyped array of <see cref="RecipeIngredient"/> sub-resources,
/// authored and read back by element cast (the established pattern shared with
/// <c>LootTable.Entries</c> / <c>QuestResource.Objectives</c>). When the output is an
/// <see cref="EquippableItemResource"/> and <see cref="OutputRarity"/> is above Common,
/// the crafted item rolls affixes through the loot pipeline, so smithing feeds the same
/// gear system as drops.
/// </summary>
[GlobalClass]
public partial class CraftingRecipeResource : Resource
{
    /// <summary>Stable id, e.g. "recipe.iron_ingot". The save/database key.</summary>
    [Export] public string Id { get; set; } = "recipe.unknown";

    [Export] public string DisplayName { get; set; } = "Unknown Recipe";

    [Export(PropertyHint.MultilineText)]
    public string Description { get; set; } = string.Empty;

    /// <summary>Station required to craft this. <see cref="CraftingStationType.Hand"/>
    /// recipes can be made at any station.</summary>
    [Export] public CraftingStationType Station { get; set; } = CraftingStationType.Hand;

    /// <summary>Inputs (untyped so authored sub-resource arrays bind cleanly); elements
    /// are read back as <see cref="RecipeIngredient"/> via <see cref="IngredientList"/>.</summary>
    [Export] public Godot.Collections.Array Ingredients { get; set; } = new();

    [Export] public string OutputItemId { get; set; } = string.Empty;

    [Export] public int OutputQuantity { get; set; } = 1;

    /// <summary>For an equippable output, the rarity to roll it at (Common = a plain,
    /// affix-less item; higher rolls affixes through the loot generator).</summary>
    [Export] public ItemRarity OutputRarity { get; set; } = ItemRarity.Common;

    /// <summary>The ingredients as a typed list (skipping malformed/empty rows).</summary>
    public List<RecipeIngredient> IngredientList()
    {
        var list = new List<RecipeIngredient>();
        foreach (Variant element in Ingredients)
        {
            if (element.As<RecipeIngredient>() is { } ingredient &&
                !string.IsNullOrEmpty(ingredient.ItemId) && ingredient.Quantity > 0)
            {
                list.Add(ingredient);
            }
        }

        return list;
    }
}
