using System.Collections.Generic;
using Embervale.Core.Diagnostics;
using Embervale.Entities;
using Embervale.Interaction;
using Embervale.Localization;
using Embervale.Loot;
using Embervale.Save;
using Godot;

namespace Embervale.Items;

/// <summary>
/// Makes a container entity lootable (30L): interacting transfers everything in the container's
/// <see cref="InventoryComponent"/> into the instigator's, stack by stack, leaving behind whatever
/// doesn't fit. No container UI — one press empties the chest, matching the hold-E loot-vacuum
/// feel.
///
/// The first-ever open also rolls **guaranteed legendary gear** (maintainer direction, 2026-07-02):
/// <see cref="MinLegendaryRolls"/>–<see cref="MaxLegendaryRolls"/> legendary-rarity equippables
/// generated at open time rather than seeded into the inventory, so chests reward properly even on
/// saves created before the container had contents. The looted flag persists via
/// <see cref="ISaveable"/>, so a plundered chest stays plundered across save/load.
/// </summary>
[GlobalClass]
public partial class ContainerLootComponent : InteractableComponent, ISaveable
{
    /// <summary>Guaranteed legendary equippables rolled on the first open (inclusive range).</summary>
    [Export] public int MinLegendaryRolls { get; set; } = 2;
    [Export] public int MaxLegendaryRolls { get; set; } = 3;

    private bool _looted;

    public string SaveId => SaveKey("container_loot");

    public override string Prompt =>
        Loc.TF("interact.loot", Entity?.DisplayName ?? Loc.T("interact.container"));

    protected override void OnInitialize()
    {
        RegisterSaveable();
    }

    protected override void OnTeardown()
    {
        SaveManager.Instance?.Unregister(this);
    }

    public override void Interact(IEntity instigator)
    {
        InventoryComponent? source = Entity?.GetComponent<InventoryComponent>();
        InventoryComponent? target = instigator.GetComponent<InventoryComponent>();
        if (target == null)
        {
            return;
        }

        // Transfer whatever the container holds. Snapshot the stacks — removing while
        // iterating would invalidate the list.
        if (source != null)
        {
            foreach (ItemStack stack in new List<ItemStack>(source.Stacks))
            {
                int moved = target.AddInstance(stack.Instance, stack.Quantity);
                if (moved > 0 && source.RemoveItem(stack.Instance.TemplateId, moved))
                {
                    Log.Info($"Looted {stack.Instance.DisplayName} x{moved} from {Entity!.DisplayName}.");
                }
            }
        }

        // First-ever open: the guaranteed legendary haul.
        if (!_looted)
        {
            _looted = true;
            RollLegendaries(target);
        }
    }

    private void RollLegendaries(InventoryComponent target)
    {
        var candidates = new List<EquippableItemResource>();
        foreach (ItemResource item in ItemDatabase.All.Values)
        {
            if (item is EquippableItemResource equippable)
            {
                candidates.Add(equippable);
            }
        }

        if (candidates.Count == 0)
        {
            return;
        }

        var rng = new RandomNumberGenerator();
        int rolls = rng.RandiRange(MinLegendaryRolls, MaxLegendaryRolls);
        for (int i = 0; i < rolls; i++)
        {
            EquippableItemResource template = candidates[rng.RandiRange(0, candidates.Count - 1)];
            ItemInstance legendary = LootGenerator.RollAffixed(template, ItemRarity.Legendary, 0.9f);
            target.AddInstance(legendary, 1);
            Log.Info($"The {Entity?.DisplayName ?? "container"} yields a legendary: {legendary.DisplayName}.");
        }
    }

    // --- ISaveable ----------------------------------------------------------

    public Godot.Collections.Dictionary Save() => new() { ["looted"] = _looted };

    public void Load(Godot.Collections.Dictionary data) =>
        _looted = data.TryGetValue("looted", out Variant looted) && looted.AsBool();
}
