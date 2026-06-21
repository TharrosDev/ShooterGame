using Godot;

namespace Embervale.Crafting;

/// <summary>
/// One required input of a <see cref="CraftingRecipeResource"/>: a quantity of an item
/// resolved by <see cref="Embervale.Items.ItemDatabase"/> id. Authored as a sub-resource
/// inside a recipe <c>.tres</c> (the same pattern as <c>LootEntry</c> in a loot table).
/// </summary>
[GlobalClass]
public partial class RecipeIngredient : Resource
{
    /// <summary>Item id consumed, e.g. "item.material.iron_ore".</summary>
    [Export] public string ItemId { get; set; } = string.Empty;

    [Export] public int Quantity { get; set; } = 1;
}
