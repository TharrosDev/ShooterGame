using Embervale.Core;
using Godot;

namespace Embervale.UI;

/// <summary>
/// The title / main menu (Phase 24A): the first thing shown on launch, before any world is
/// built. <see cref="GameBootstrap"/> boots into <see cref="GameState.MainMenu"/> and shows this
/// instead of constructing the sandbox; <b>New Game</b> invokes <see cref="NewGameRequested"/>
/// (the bootstrap's deferred build path) and <b>Quit</b> exits. Continue / Load / Settings are
/// present but disabled — they light up in 24B–24F as the save-slot and settings systems land.
/// Built in code through <see cref="UiTheme"/>, mirroring <see cref="PauseMenu"/>.
/// </summary>
public partial class MainMenu : CanvasLayer
{
    /// <summary>Invoked when the player chooses New Game; the bootstrap builds the world.</summary>
    public System.Action? NewGameRequested { get; set; }

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

        col.AddChild(MenuButton("New Game", () => NewGameRequested?.Invoke()));
        // Disabled stubs — the save-slot flow (24B–24C) and settings (24E–24F) light these up.
        col.AddChild(MenuButton("Continue", null));
        col.AddChild(MenuButton("Load Game", null));
        col.AddChild(MenuButton("Settings", null));
        col.AddChild(MenuButton("Quit", () => GetTree().Quit()));
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
