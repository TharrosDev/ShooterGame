using Embervale.Core.Diagnostics;
using Embervale.Core.Events;
using Embervale.Entities;
using Embervale.Interaction;
using Godot;

namespace Embervale.Items;

/// <summary>
/// Makes a world entity a collectable item. Interacting transfers the held item
/// into the instigator's <see cref="InventoryComponent"/> and despawns the pickup
/// once empty (or leaves the remainder if the inventory filled up).
/// </summary>
[GlobalClass]
public partial class ItemPickupComponent : InteractableComponent
{
    [Export] public ItemResource? Item { get; set; }
    [Export] public int Quantity { get; set; } = 1;

    public override string Prompt =>
        Item == null
            ? "Pick up"
            : Quantity > 1 ? $"Pick up {Item.DisplayName} x{Quantity}" : $"Pick up {Item.DisplayName}";

    public override void Interact(IEntity instigator)
    {
        if (Item == null)
        {
            return;
        }

        InventoryComponent? inventory = instigator.GetComponent<InventoryComponent>();
        if (inventory == null)
        {
            return;
        }

        int added = inventory.AddItem(Item, Quantity);
        if (added <= 0)
        {
            Log.Info($"{instigator.DisplayName}'s inventory is full.");
            return;
        }

        EventBus.Instance?.Publish(new ItemPickedUpEvent(instigator, Item, added));
        Log.Info($"{instigator.DisplayName} picked up {Item.DisplayName} x{added}.");

        Quantity -= added;
        if (Quantity <= 0 && Entity != null)
        {
            ((Node)Entity.Body).QueueFree();
        }
    }
}
