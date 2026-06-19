using Godot;

namespace Embervale.Items;

/// <summary>
/// Resource-driven definition of an item *template*. Designers author one
/// <c>.tres</c> per item under <c>data/items/</c>; the <see cref="ItemDatabase"/>
/// indexes them by <see cref="Id"/>. Runtime quantities live in an
/// <see cref="ItemStack"/>; per-instance rolled affixes arrive with loot
/// generation (Phase 7) as a separate item-instance layer over this template.
/// </summary>
[GlobalClass]
public partial class ItemResource : Resource
{
    /// <summary>Stable unique id, e.g. "item.potion.health". The save/database key.</summary>
    [Export] public string Id { get; set; } = "item.unknown";

    [Export] public string DisplayName { get; set; } = "Unknown Item";

    [Export(PropertyHint.MultilineText)]
    public string Description { get; set; } = string.Empty;

    [Export] public ItemType Type { get; set; } = ItemType.Misc;
    [Export] public ItemRarity Rarity { get; set; } = ItemRarity.Common;

    /// <summary>Max units in one stack. 1 = non-stackable (weapons, armor).</summary>
    [Export] public int MaxStack { get; set; } = 99;

    [Export] public float Weight { get; set; } = 0.1f;

    /// <summary>Base merchant value in gold.</summary>
    [Export] public int Value { get; set; } = 1;

    /// <summary>Optional inventory icon; UI falls back to text/rarity colour when null.</summary>
    [Export] public Texture2D? Icon { get; set; }

    public bool IsStackable => MaxStack > 1;
}
