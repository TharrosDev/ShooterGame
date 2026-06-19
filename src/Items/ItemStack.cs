using Godot;

namespace Embervale.Items;

/// <summary>
/// A runtime quantity of one <see cref="ItemResource"/> occupying a single
/// inventory slot. Mutable quantity; the owning <see cref="InventoryComponent"/>
/// enforces stacking rules against <see cref="ItemResource.MaxStack"/>.
/// </summary>
public sealed class ItemStack
{
    public ItemStack(ItemResource item, int quantity)
    {
        Item = item;
        Quantity = quantity;
    }

    public ItemResource Item { get; }

    public int Quantity { get; set; }

    public bool IsFull => Quantity >= Item.MaxStack;

    public int SpaceLeft => Mathf.Max(0, Item.MaxStack - Quantity);

    public float Weight => Quantity * Item.Weight;
}
