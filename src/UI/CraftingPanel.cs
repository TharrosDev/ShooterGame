using Embervale.Core;
using Embervale.Core.Events;
using Embervale.Crafting;
using Embervale.Entities;
using Embervale.Items;
using Embervale.Localization;
using Godot;

namespace Embervale.UI;

/// <summary>
/// The crafting window (on the 30.5F <see cref="UiPanel"/> framework). It is event-driven: a
/// <see cref="CraftingStationComponent"/> publishes a <see cref="CraftingStationOpenedEvent"/>
/// on interact, this panel resolves the player's <see cref="CraftingComponent"/> +
/// <see cref="InventoryComponent"/> and lists the recipes that match the station (plus
/// <c>Hand</c> recipes the player knows), each with a have/need ingredient breakdown and a
/// Craft button enabled only when it can be made. The craft/salvage switch rides the shared
/// <see cref="UiTabs"/> strip; the base owns the modal contract and the dirty-flag rebuild.
/// </summary>
public partial class CraftingPanel : UiPanel
{
    private Label _title = null!;
    private UiTabs _modeTabs = null!;
    private VBoxContainer _list = null!;

    private IEntity? _player;
    private CraftingComponent? _crafting;
    private InventoryComponent? _inventory;
    private CraftingStationType _station;
    private string _stationName = "Crafting";
    private bool _justOpened;
    private bool _salvageMode;

    protected override void BuildShell(PanelContainer shell)
    {
        shell.AnchorLeft = 0.5f;
        shell.AnchorRight = 0.5f;
        shell.OffsetLeft = -230;
        shell.OffsetRight = 230;
        shell.OffsetTop = 60;
        shell.GrowHorizontal = Control.GrowDirection.Both;

        MarginContainer margin = UiTheme.Padding(12);
        shell.AddChild(margin);

        var column = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        column.AddThemeConstantOverride("separation", UiTheme.SpaceXs);
        margin.AddChild(column);

        _title = UiTheme.Header(string.Empty);
        column.AddChild(_title);

        // Craft / Salvage switch — static layout; the pages rebuild inside the list below.
        _modeTabs = new UiTabs();
        _modeTabs.Add(Loc.T("craft.mode_craft"));
        _modeTabs.Add(Loc.T("craft.mode_salvage"));
        _modeTabs.TabChanged += index =>
        {
            _salvageMode = index == 1;
            MarkDirty();
        };
        column.AddChild(_modeTabs);
        column.AddChild(new HSeparator());

        var scroll = new ScrollContainer
        {
            CustomMinimumSize = new Vector2(436, 500),
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
        };
        column.AddChild(scroll);

        _list = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        _list.AddThemeConstantOverride("separation", UiTheme.SpaceXs);
        scroll.AddChild(_list);
    }

    protected override void OnReady()
    {
        EventBus.Instance?.Subscribe<CraftingStationOpenedEvent>(OnStationOpened);
        EventBus.Instance?.Subscribe<InventoryChangedEvent>(OnInventoryChanged);
        EventBus.Instance?.Subscribe<ItemCraftedEvent>(OnItemCrafted);
        EventBus.Instance?.Subscribe<ItemDeconstructedEvent>(OnItemDeconstructed);
    }

    public override void _ExitTree()
    {
        EventBus.Instance?.Unsubscribe<CraftingStationOpenedEvent>(OnStationOpened);
        EventBus.Instance?.Unsubscribe<InventoryChangedEvent>(OnInventoryChanged);
        EventBus.Instance?.Unsubscribe<ItemCraftedEvent>(OnItemCrafted);
        EventBus.Instance?.Unsubscribe<ItemDeconstructedEvent>(OnItemDeconstructed);
    }

    private void OnStationOpened(CraftingStationOpenedEvent e)
    {
        // Ignore a second station while one is open.
        if (IsOpen)
        {
            return;
        }

        _player = e.Player;
        _crafting = e.Player.GetComponent<CraftingComponent>();
        _inventory = e.Player.GetComponent<InventoryComponent>();
        _station = e.Station;
        _stationName = e.StationName;

        if (_crafting == null)
        {
            return;
        }

        _salvageMode = false;
        _modeTabs.Select(0);
        SetOpen(true);

        // The same interact press that opened the station is still "just pressed" this
        // frame; swallow it so the close-on-interact below doesn't fire immediately.
        _justOpened = true;
    }

    private void OnInventoryChanged(InventoryChangedEvent e) => MarkDirty();

    private void OnItemCrafted(ItemCraftedEvent e) => MarkDirty();

    private void OnItemDeconstructed(ItemDeconstructedEvent e) => MarkDirty();

    public override void _Process(double delta)
    {
        if (IsOpen)
        {
            // Swallow the interact press that opened the station this frame.
            if (_justOpened)
            {
                _justOpened = false;
            }
            else if (Godot.Input.IsActionJustPressed(GameInput.Interact))
            {
                // A modal needs an easy out; the interact key both opens and closes it.
                Close();
                return;
            }
        }

        base._Process(delta);
    }

    private void Craft(CraftingRecipeResource recipe)
    {
        _crafting?.Craft(recipe, _station);
        MarkDirty(); // rebuild next frame (events also flag it)
    }

    private void Deconstruct(ItemInstance instance)
    {
        _crafting?.Deconstruct(instance, _station);
        MarkDirty(); // rebuild next frame (events also flag it)
    }

    private void Close()
    {
        IEntity? player = _player;
        SetOpen(false);
        _player = null;
        _crafting = null;
        _inventory = null;

        if (player != null)
        {
            EventBus.Instance?.Publish(new CraftingStationClosedEvent(player));
        }
    }

    protected override void Rebuild()
    {
        UiTheme.ClearChildren(_list);

        _title.Text = $"{_stationName}   (E to close)";

        if (_crafting == null)
        {
            return;
        }

        if (_salvageMode)
        {
            RebuildSalvage();
        }
        else
        {
            RebuildCraft();
        }
    }

    private void RebuildCraft()
    {
        bool any = false;
        foreach (CraftingRecipeResource recipe in RecipeDatabase.All)
        {
            if (!_crafting!.Knows(recipe.Id) || !StationShows(recipe.Station))
            {
                continue;
            }

            any = true;
            AddRecipe(recipe);
        }

        if (!any)
        {
            _list.AddChild(UiTheme.Body(Loc.T("craft.recipes_none"), UiTheme.Dim));
        }
    }

    /// <summary>Lists every salvageable inventory stack and worn item — recipe-backed (returns
    /// materials) or not (returns generic scrap) — each with its yield preview and a Deconstruct button.</summary>
    private void RebuildSalvage()
    {
        bool any = false;

        // Loose inventory items first, then the gear the player is wearing (badged) — both salvageable.
        if (_inventory != null)
        {
            foreach (ItemStack stack in _inventory.Stacks)
            {
                if (_crafting!.CanDeconstruct(stack.Instance, _station))
                {
                    any = true;
                    AddSalvage(stack.Instance, stack.Quantity, equipped: false,
                        _crafting.DeconstructionRecipe(stack.Instance.TemplateId, _station));
                }
            }
        }

        EquipmentComponent? equipment = _player?.GetComponent<EquipmentComponent>();
        if (equipment != null)
        {
            foreach (ItemInstance instance in equipment.EquippedInstances)
            {
                if (_crafting!.CanDeconstruct(instance, _station))
                {
                    any = true;
                    AddSalvage(instance, 1, equipped: true,
                        _crafting.DeconstructionRecipe(instance.TemplateId, _station));
                }
            }
        }

        if (!any)
        {
            _list.AddChild(UiTheme.Body(Loc.T("craft.salvage_none"), UiTheme.Dim));
        }
    }

    private void AddSalvage(ItemInstance instance, int quantity, bool equipped, CraftingRecipeResource? recipe)
    {
        var titleRow = new HBoxContainer();
        titleRow.AddThemeConstantOverride("separation", 8);

        Label title = UiTheme.Body($"{instance.DisplayName} x{quantity}", UiTheme.Text);
        title.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        title.SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;
        titleRow.AddChild(title);

        // The "small visual": worn gear gets an accent [Equipped] badge so it reads apart from loose
        // copies before the player salvages it (salvaging an equipped item takes it off first).
        if (equipped)
        {
            Label badge = UiTheme.Body($"[{Loc.T("craft.equipped")}]", UiTheme.Accent);
            badge.SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;
            titleRow.AddChild(badge);
        }

        Button button = UiTheme.Action(Loc.T("craft.deconstruct"));
        ItemInstance captured = instance;
        button.Pressed += () => Deconstruct(captured);
        titleRow.AddChild(button);

        _list.AddChild(titleRow);

        if (recipe != null)
        {
            foreach (RecipeIngredient ingredient in recipe.IngredientList())
            {
                int recovered = Deconstruction.RecoveredQuantity(ingredient.Quantity);
                if (recovered <= 0)
                {
                    continue;
                }

                ItemResource? material = ItemDatabase.Get(ingredient.ItemId);
                string name = material?.DisplayName ?? ingredient.ItemId;
                _list.AddChild(UiTheme.Body($"   → {recovered}x {name}", UiTheme.Accent));
            }
        }
        else
        {
            // Recipe-less: generic scrap, scaled by rarity.
            int scrap = Deconstruction.ScrapYield(instance.Rarity);
            string scrapName = ItemDatabase.Get(GameIds.Items.Scrap)?.DisplayName ?? "Scrap";
            _list.AddChild(UiTheme.Body($"   → {scrap}x {scrapName}", UiTheme.Accent));
        }

        int xp = Deconstruction.Xp(instance.Template.Value, instance.Rarity);
        _list.AddChild(UiTheme.Body($"   {Loc.TF("craft.yield_xp", xp)}", UiTheme.Good));
        _list.AddChild(new HSeparator());
    }

    private bool StationShows(CraftingStationType required)
    {
        return required == CraftingStationType.Hand || required == _station;
    }

    private void AddRecipe(CraftingRecipeResource recipe)
    {
        bool canCraft = _crafting != null && _crafting.CanCraft(recipe, _station);

        var titleRow = new HBoxContainer();
        titleRow.AddThemeConstantOverride("separation", 8);

        Label title = UiTheme.Body(recipe.DisplayName, canCraft ? UiTheme.Text : UiTheme.Dim);
        title.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        title.SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;
        titleRow.AddChild(title);

        Button craft = UiTheme.Action(Loc.T("craft.craft"));
        craft.Disabled = !canCraft;
        CraftingRecipeResource captured = recipe;
        craft.Pressed += () => Craft(captured);
        titleRow.AddChild(craft);

        _list.AddChild(titleRow);

        ItemResource? output = ItemDatabase.Get(recipe.OutputItemId);
        string outName = output?.DisplayName ?? recipe.OutputItemId;
        string rarity = recipe.OutputRarity != ItemRarity.Common ? $" [{recipe.OutputRarity}]" : string.Empty;
        _list.AddChild(UiTheme.Body($"   → {recipe.OutputQuantity}x {outName}{rarity}", UiTheme.Accent));

        foreach (RecipeIngredient ingredient in recipe.IngredientList())
        {
            int have = _inventory?.CountOf(ingredient.ItemId) ?? 0;
            ItemResource? item = ItemDatabase.Get(ingredient.ItemId);
            string itemName = item?.DisplayName ?? ingredient.ItemId;
            Color color = have >= ingredient.Quantity ? UiTheme.Good : UiTheme.Bad;
            _list.AddChild(UiTheme.Body($"      {ingredient.Quantity}x {itemName}  (have {have})", color));
        }

        _list.AddChild(new HSeparator());
    }
}
