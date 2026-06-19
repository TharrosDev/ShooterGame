using System.Text;
using Embervale.Core;
using Embervale.Core.Events;
using Embervale.Items;
using Godot;

namespace Embervale.UI;

/// <summary>
/// A toggleable (the <c>inventory</c> action) read-out of the player's inventory:
/// slot usage, total weight, and each stack with its rarity. Display-only for now
/// — drag/drop, use and equip come with the equipment UI in later phases. Built
/// in code; refreshes live while open via <see cref="InventoryChangedEvent"/>.
/// </summary>
public partial class InventoryPanel : CanvasLayer
{
    private InventoryComponent? _inventory;
    private PanelContainer _panel = null!;
    private Label _label = null!;

    public override void _Ready()
    {
        _panel = new PanelContainer
        {
            Visible = false,
            Position = new Vector2(940, 16),
            CustomMinimumSize = new Vector2(320, 0),
        };
        AddChild(_panel);

        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", 12);
        margin.AddThemeConstantOverride("margin_right", 12);
        margin.AddThemeConstantOverride("margin_top", 10);
        margin.AddThemeConstantOverride("margin_bottom", 10);
        _panel.AddChild(margin);

        _label = new Label();
        _label.AddThemeFontSizeOverride("font_size", 15);
        margin.AddChild(_label);

        EventBus.Instance?.Subscribe<InventoryChangedEvent>(OnInventoryChanged);
    }

    public override void _ExitTree()
    {
        EventBus.Instance?.Unsubscribe<InventoryChangedEvent>(OnInventoryChanged);
    }

    public void SetInventory(InventoryComponent? inventory)
    {
        _inventory = inventory;
        Refresh();
    }

    public override void _Process(double delta)
    {
        if (Godot.Input.IsActionJustPressed(GameInput.Inventory))
        {
            _panel.Visible = !_panel.Visible;
            if (_panel.Visible)
            {
                Refresh();
            }
        }
    }

    private void OnInventoryChanged(InventoryChangedEvent e)
    {
        if (_panel.Visible)
        {
            Refresh();
        }
    }

    private void Refresh()
    {
        var sb = new StringBuilder();
        sb.Append("INVENTORY   (I to close)\n");

        if (_inventory == null)
        {
            sb.Append("—");
            _label.Text = sb.ToString();
            return;
        }

        sb.Append($"Slots {_inventory.UsedSlots}/{_inventory.Capacity}   ");
        sb.Append($"Weight {_inventory.TotalWeight:0.0}/{_inventory.MaxWeight:0}\n\n");

        if (_inventory.Stacks.Count == 0)
        {
            sb.Append("(empty)");
        }
        else
        {
            foreach (ItemStack stack in _inventory.Stacks)
            {
                sb.Append($"• {stack.Item.DisplayName}  x{stack.Quantity}");
                if (stack.Item.Rarity != ItemRarity.Common)
                {
                    sb.Append($"  [{stack.Item.Rarity}]");
                }

                sb.Append('\n');
            }
        }

        _label.Text = sb.ToString();
    }
}
