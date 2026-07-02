using System.Collections.Generic;
using Embervale.Core.Diagnostics;
using Embervale.Entities;
using Embervale.Interaction;
using Embervale.Localization;
using Godot;

namespace Embervale.Items;

/// <summary>
/// Makes a container entity lootable (30L): interacting transfers everything in the container's
/// <see cref="InventoryComponent"/> into the instigator's, stack by stack, leaving behind whatever
/// doesn't fit. No container UI — one press empties the chest, matching the hold-E loot-vacuum
/// feel. The container's inventory persists via its own <see cref="ISaveable"/> path, so a looted
/// chest stays looted across save/load.
/// </summary>
[GlobalClass]
public partial class ContainerLootComponent : InteractableComponent
{
    public override string Prompt =>
        Loc.TF("interact.loot", Entity?.DisplayName ?? Loc.T("interact.container"));

    public override void Interact(IEntity instigator)
    {
        InventoryComponent? source = Entity?.GetComponent<InventoryComponent>();
        InventoryComponent? target = instigator.GetComponent<InventoryComponent>();
        if (source == null || target == null || source.Stacks.Count == 0)
        {
            return;
        }

        // Snapshot the stacks — removing while iterating would invalidate the list.
        foreach (ItemStack stack in new List<ItemStack>(source.Stacks))
        {
            int moved = target.AddInstance(stack.Instance, stack.Quantity);
            if (moved > 0 && source.RemoveItem(stack.Instance.TemplateId, moved))
            {
                Log.Info($"Looted {stack.Instance.DisplayName} x{moved} from {Entity!.DisplayName}.");
            }
        }
    }
}
