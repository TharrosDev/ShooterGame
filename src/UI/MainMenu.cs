using Embervale.Core;
using Embervale.Save;
using Godot;

namespace Embervale.UI;

/// <summary>
/// The title / main menu (Phase 24A): the first thing shown on launch, before any world is
/// built. <see cref="GameBootstrap"/> boots into <see cref="GameState.MainMenu"/> and shows this
/// instead of constructing the sandbox. <b>New Game</b> and <b>Load Game</b> open the
/// <see cref="SaveSlotPanel"/> to pick a slot (24C), <b>Continue</b> resumes the most-recent save,
/// and <b>Quit</b> exits. Settings remains a disabled stub until 24E–24F. Built in code through
/// <see cref="UiTheme"/>, mirroring <see cref="PauseMenu"/>.
/// </summary>
public partial class MainMenu : CanvasLayer
{
    /// <summary>Invoked with the chosen slot when the player starts a new game.</summary>
    public System.Action<string>? NewGameRequested { get; set; }

    /// <summary>Invoked with the chosen slot when the player loads/continues a save.</summary>
    public System.Action<string>? LoadGameRequested { get; set; }

    public override void _Ready()
    {
        Layer = 11; // above the (not-yet-built) HUD and the pause menu
        // No world/player yet on the title screen, so make sure the cursor is free.
        Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Visible;
        Build();
    }

    private void Build()
    {
        var backdrop = new ColorRect { Color = new Color(0.03f, 0.03f, 0.05f, 1f) };
        backdrop.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        backdrop.MouseFilter = Control.MouseFilterEnum.Stop;
        AddChild(backdrop);

        PanelContainer panel = UiTheme.Panel();
        panel.SetAnchorsPreset(Control.LayoutPreset.Center);
        panel.GrowHorizontal = Control.GrowDirection.Both;
        panel.GrowVertical = Control.GrowDirection.Both;
        panel.CustomMinimumSize = new Vector2(320, 0);
        AddChild(panel);

        MarginContainer pad = UiTheme.Padding(20);
        panel.AddChild(pad);

        var col = new VBoxContainer();
        col.AddThemeConstantOverride("separation", 8);
        pad.AddChild(col);

        Label title = UiTheme.Header("EMBERVALE");
        title.HorizontalAlignment = HorizontalAlignment.Center;
        title.AddThemeFontSizeOverride("font_size", 28);
        col.AddChild(title);

        Label subtitle = UiTheme.Body("Choose whether to save creation — or become its next Ash King.", UiTheme.Dim);
        subtitle.HorizontalAlignment = HorizontalAlignment.Center;
        subtitle.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        col.AddChild(subtitle);

        col.AddChild(new HSeparator());

        bool hasSaves = (SaveManager.Instance?.ListSlots().Count ?? 0) > 0;

        col.AddChild(MenuButton("New Game", () => OpenSlotPanel(SaveSlotPanel.Intent.New)));
        col.AddChild(MenuButton("Continue", hasSaves ? ContinueMostRecent : null));
        col.AddChild(MenuButton("Load Game", hasSaves ? () => OpenSlotPanel(SaveSlotPanel.Intent.Load) : null));
        // Settings is still a stub until 24E–24F.
        col.AddChild(MenuButton("Settings", null));
        col.AddChild(MenuButton("Quit", () => GetTree().Quit()));
    }

    private void OpenSlotPanel(SaveSlotPanel.Intent mode)
    {
        var panel = new SaveSlotPanel();
        System.Action<string> chosen = mode == SaveSlotPanel.Intent.New
            ? slot => NewGameRequested?.Invoke(slot)
            : slot => LoadGameRequested?.Invoke(slot);

        // Hide the menu behind the panel; restore it if the player backs out.
        Visible = false;
        panel.Configure(mode, chosen, () => Visible = true);
        AddChild(panel);
    }

    private void ContinueMostRecent()
    {
        if (SaveManager.Instance is not { } manager)
        {
            return;
        }

        SaveSlotInfo? latest = null;
        foreach (SaveSlotInfo info in manager.ListSlots())
        {
            if (latest == null || info.TimestampUnix > latest.TimestampUnix)
            {
                latest = info;
            }
        }

        if (latest != null)
        {
            LoadGameRequested?.Invoke(latest.Slot);
        }
    }

    private static Button MenuButton(string text, System.Action? onPressed)
    {
        Button button = UiTheme.Action(text);
        button.CustomMinimumSize = new Vector2(0, 36);
        button.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        if (onPressed == null)
        {
            button.Disabled = true;
            button.TooltipText = "Coming soon";
        }
        else
        {
            button.Pressed += () => onPressed();
        }

        return button;
    }
}
