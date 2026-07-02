using Embervale.Core.Services;
using Embervale.Settings;
using Godot;

namespace Embervale.UI;

/// <summary>
/// The UI design tokens and widget builders (Phase 30.5A) — the single source of truth every
/// surface answers to. Tokens (palette, type scale, spacing, radius, motion) encode the
/// dying-world identity pinned in <c>docs/UI_STYLE.md</c> (ash neutrals, bone-pale text,
/// ember accents — matched to <c>docs/ART_STYLE.md</c>); the builders below compose them into
/// the controls every panel uses. Change a token here and the whole UI follows.
/// </summary>
public static class UiTheme
{
    // --- Palette tokens (see docs/UI_STYLE.md §2) -----------------------------
    // Surfaces: warm charcoal ash, never blue-black.
    public static readonly Color PanelBg = new(0.09f, 0.085f, 0.075f, 0.92f);
    public static readonly Color PanelBorder = new(0.42f, 0.40f, 0.35f, 0.80f);
    public static readonly Color Trough = new(0.13f, 0.125f, 0.115f, 0.95f);

    // Text: bone pale primary, ash-grey secondary.
    public static readonly Color Text = new(0.79f, 0.75f, 0.68f);
    public static readonly Color Dim = new(0.55f, 0.53f, 0.47f);

    // Accents: ember gold is THE accent (headers, highlights, focus); ember orange is
    // reserved for the hottest emphasis (crits, warnings, the Flamebearer thread).
    public static readonly Color Accent = new(0.85f, 0.64f, 0.25f);
    public static readonly Color AccentHot = new(0.91f, 0.45f, 0.17f);

    // Semantic feedback.
    public static readonly Color Good = new(0.55f, 0.68f, 0.44f);
    public static readonly Color Bad = new(0.82f, 0.42f, 0.36f);

    // Resource bar fills.
    public static readonly Color Health = new(0.78f, 0.30f, 0.26f);
    public static readonly Color Stamina = new(0.80f, 0.66f, 0.30f);
    public static readonly Color Mana = new(0.42f, 0.56f, 0.76f);

    // The corruption identity — the art bible's corruption violet (ART_STYLE §2), used by
    // the gauge fill and the HUD vignette.
    public static readonly Color Corruption = new(0.48f, 0.30f, 0.55f);

    // --- Type scale ----------------------------------------------------------
    public const int CaptionFontSize = 11;
    public const int BodyFontSize = 14;
    public const int HeaderFontSize = 16;
    public const int TitleFontSize = 20;
    public const int DisplayFontSize = 26;

    // --- Spacing scale (px at reference scale) ---------------------------------
    public const int SpaceXs = 4;
    public const int SpaceSm = 6;
    public const int SpaceMd = 10;
    public const int SpaceLg = 16;
    public const int SpaceXl = 24;

    // --- Radii -----------------------------------------------------------------
    public const int RadiusSm = 3;
    public const int RadiusMd = 4;
    public const int RadiusLg = 6;

    // --- Motion tokens -----------------------------------------------------------
    // Durations in seconds; always route through Duration() so the reduced-motion
    // accessibility setting (Settings.ReducedMotion) collapses animation to instant.
    public const float DurationFast = 0.12f;
    public const float DurationBase = 0.20f;
    public const float DurationSlow = 0.35f;

    /// <summary>False while the player has reduced motion enabled in settings.</summary>
    public static bool MotionEnabled =>
        ServiceLocator.Instance is not { } locator ||
        !locator.TryGet(out SettingsService settings) ||
        !settings.Current.ReducedMotion;

    /// <summary>A motion duration honouring the reduced-motion setting (0 = instant).</summary>
    public static float Duration(float seconds) => MotionEnabled ? seconds : 0f;

    // --- Builders -----------------------------------------------------------

    /// <summary>A framed, semi-transparent panel with rounded corners.</summary>
    public static PanelContainer Panel()
    {
        var panel = new PanelContainer();
        panel.AddThemeStyleboxOverride("panel", PanelStyle());
        return panel;
    }

    /// <summary>The standard inner padding container panels wrap their content in.</summary>
    public static MarginContainer Padding(int amount = SpaceMd)
    {
        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", amount + 2);
        margin.AddThemeConstantOverride("margin_right", amount + 2);
        margin.AddThemeConstantOverride("margin_top", amount);
        margin.AddThemeConstantOverride("margin_bottom", amount);
        return margin;
    }

    public static Label Header(string text)
    {
        var label = new Label { Text = text };
        label.AddThemeFontSizeOverride("font_size", HeaderFontSize);
        label.AddThemeColorOverride("font_color", Accent);
        return label;
    }

    public static Label Body(string text, Color? color = null)
    {
        var label = new Label { Text = text };
        label.AddThemeFontSizeOverride("font_size", BodyFontSize);
        label.AddThemeColorOverride("font_color", color ?? Text);
        return label;
    }

    /// <summary>A small secondary line (slot numbers, hints, metadata).</summary>
    public static Label Caption(string text, Color? color = null)
    {
        var label = new Label { Text = text };
        label.AddThemeFontSizeOverride("font_size", CaptionFontSize);
        label.AddThemeColorOverride("font_color", color ?? Dim);
        return label;
    }

    public static Button Action(string text)
    {
        var button = new Button { Text = text };
        ApplyInteractiveStyle(button);
        return button;
    }

    /// <summary>A thin coloured resource bar (0..1) with a dark trough.</summary>
    public static ProgressBar Bar(Color fill, float width = 168f)
    {
        var bar = new ProgressBar
        {
            MinValue = 0d,
            MaxValue = 1d,
            Value = 1d,
            ShowPercentage = false,
            CustomMinimumSize = new Vector2(width, 13f),
        };
        bar.AddThemeStyleboxOverride("background", BarStyle(Trough));
        bar.AddThemeStyleboxOverride("fill", BarStyle(fill));
        return bar;
    }

    /// <summary>A labelled on/off switch (settings rows). Caller wires <c>Toggled</c>.</summary>
    public static CheckButton Toggle(bool value)
    {
        var check = new CheckButton { ButtonPressed = value };
        check.AddThemeColorOverride("font_color", Text);
        check.AddThemeColorOverride("font_hover_color", Accent);
        return check;
    }

    /// <summary>A horizontal value slider (volumes, sensitivity, UI scale). Caller wires
    /// <c>ValueChanged</c>/<c>DragEnded</c>.</summary>
    public static HSlider Slider(double min, double max, double step, double value, float width = 200f)
    {
        var slider = new HSlider
        {
            MinValue = min,
            MaxValue = max,
            Step = step,
            Value = value,
            CustomMinimumSize = new Vector2(width, 18f),
            SizeFlagsVertical = Control.SizeFlags.ShrinkCenter,
        };
        return slider;
    }

    /// <summary>An enumerated chooser (window mode, FPS cap, difficulty). Caller wires
    /// <c>ItemSelected</c>.</summary>
    public static OptionButton Dropdown(string[] options, int selected)
    {
        var option = new OptionButton();
        ApplyInteractiveStyle(option);
        for (int i = 0; i < options.Length; i++)
        {
            option.AddItem(options[i], i);
        }

        if (selected >= 0 && selected < options.Length)
        {
            option.Selected = selected;
        }

        return option;
    }

    // --- Style boxes --------------------------------------------------------

    /// <summary>The framed-panel stylebox (also used by transient widgets like toasts).</summary>
    public static StyleBoxFlat PanelStyle()
    {
        var box = new StyleBoxFlat { BgColor = PanelBg, BorderColor = PanelBorder };
        box.SetBorderWidthAll(1);
        box.SetCornerRadiusAll(RadiusLg);
        return box;
    }

    /// <summary>The shared normal/hover/pressed/focus styling for clickable controls. Focus
    /// draws an ember border — the visibility seam the gamepad navigation pass (30.5J) rides.</summary>
    private static void ApplyInteractiveStyle(Button button)
    {
        button.AddThemeColorOverride("font_color", Text);
        button.AddThemeColorOverride("font_hover_color", Accent);
        button.AddThemeStyleboxOverride("normal", ButtonStyle(new Color(0.16f, 0.15f, 0.13f, 0.95f)));
        button.AddThemeStyleboxOverride("hover", ButtonStyle(new Color(0.23f, 0.21f, 0.18f, 0.98f)));
        button.AddThemeStyleboxOverride("pressed", ButtonStyle(new Color(0.11f, 0.10f, 0.09f, 0.98f)));

        StyleBoxFlat focus = ButtonStyle(new Color(0.16f, 0.15f, 0.13f, 0.95f));
        focus.BorderColor = Accent;
        focus.SetBorderWidthAll(1);
        button.AddThemeStyleboxOverride("focus", focus);
    }

    private static StyleBoxFlat ButtonStyle(Color color)
    {
        var box = new StyleBoxFlat { BgColor = color };
        box.SetCornerRadiusAll(RadiusMd);
        box.SetContentMarginAll(SpaceXs);
        box.ContentMarginLeft = 9;
        box.ContentMarginRight = 9;
        return box;
    }

    /// <summary>The rounded bar stylebox (shared with <see cref="JuicedBar"/>).</summary>
    internal static StyleBoxFlat BarStyle(Color color)
    {
        var box = new StyleBoxFlat { BgColor = color };
        box.SetCornerRadiusAll(RadiusSm);
        return box;
    }
}
