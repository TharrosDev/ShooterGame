using Godot;

namespace Embervale.UI;

/// <summary>
/// The HUD layout system (Phase 30.5B): a full-screen, mouse-transparent root with anchored
/// **widget slots** every HUD element parents into, replacing per-widget anchor/offset math.
/// A single safe-area margin insets all slots from the screen edges (one knob instead of
/// scattered magic 16s), and stacked slots (e.g. <see cref="TopCenter"/>) space their widgets
/// automatically — hidden widgets collapse, so the boss bar, event banner and nameplate never
/// overlap the way hand-tuned y-offsets did. Global UI scale is handled upstream by the
/// window's content scale factor (see <c>SettingsService.ApplyGraphics</c>), so slots need no
/// per-widget scaling.
///
/// Slots are plain containers: widgets keep owning their look and update logic; the layout
/// only owns *where they live*. OS safe-area insets (TV overscan, notches) are zero on the
/// desktop/Steam Deck targets, so the margin is the token inset; raise <see cref="SafeMargin"/>
/// if a platform ever needs more.
/// </summary>
public partial class HudLayout : Control
{
    /// <summary>Inset between the screen edge and every slot.</summary>
    public int SafeMargin { get; set; } = UiTheme.SpaceLg;

    /// <summary>Top-left stack (context: time, weather).</summary>
    public VBoxContainer TopLeft { get; private set; } = null!;

    /// <summary>Top-centre stack (compass, boss bar, event banner, nameplate) — order = add order.</summary>
    public VBoxContainer TopCenter { get; private set; } = null!;

    /// <summary>Top-right stack (quest tracker).</summary>
    public VBoxContainer TopRight { get; private set; } = null!;

    /// <summary>Bottom-left stack (vitals).</summary>
    public VBoxContainer BottomLeft { get; private set; } = null!;

    /// <summary>Bottom-centre stack (interaction prompt), floated above the screen edge.</summary>
    public VBoxContainer BottomCenter { get; private set; } = null!;

    /// <summary>Free layer for screen-space widgets that position themselves (lock reticle) and
    /// full-screen overlays (vignette, fades). Not inset by the safe margin.</summary>
    public Control Overlay { get; private set; } = null!;

    public override void _Ready()
    {
        SetAnchorsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Ignore;

        Overlay = new Control { Name = "Overlay", MouseFilter = MouseFilterEnum.Ignore };
        Overlay.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(Overlay);

        var safe = new MarginContainer { Name = "SafeArea", MouseFilter = MouseFilterEnum.Ignore };
        safe.SetAnchorsPreset(LayoutPreset.FullRect);
        safe.AddThemeConstantOverride("margin_left", SafeMargin);
        safe.AddThemeConstantOverride("margin_right", SafeMargin);
        safe.AddThemeConstantOverride("margin_top", SafeMargin);
        safe.AddThemeConstantOverride("margin_bottom", SafeMargin);
        AddChild(safe);

        TopLeft = Slot("TopLeft", safe, horizontal: 0f, vertical: 0f);
        TopCenter = Slot("TopCenter", safe, horizontal: 0.5f, vertical: 0f);
        TopRight = Slot("TopRight", safe, horizontal: 1f, vertical: 0f);
        BottomLeft = Slot("BottomLeft", safe, horizontal: 0f, vertical: 1f);
        BottomCenter = Slot("BottomCenter", safe, horizontal: 0.5f, vertical: 1f);

        // The prompt slot floats above the very bottom edge, near the player's natural gaze.
        BottomCenter.OffsetBottom = -96;
    }

    /// <summary>A stacked slot pinned to a corner/edge of the safe area. <paramref name="horizontal"/>
    /// and <paramref name="vertical"/> are 0/0.5/1 anchor fractions (left/centre/right, top/bottom).</summary>
    private static VBoxContainer Slot(string name, MarginContainer safe, float horizontal, float vertical)
    {
        // MarginContainer children fill it; an inner wrapper keeps each slot shrink-wrapped to its
        // content and aligned to its corner instead of stretching across the screen.
        var align = new Control { Name = name + "Anchor", MouseFilter = MouseFilterEnum.Ignore };
        safe.AddChild(align);

        var slot = new VBoxContainer { Name = name, MouseFilter = MouseFilterEnum.Ignore };
        slot.AddThemeConstantOverride("separation", UiTheme.SpaceSm);
        slot.AnchorLeft = horizontal;
        slot.AnchorRight = horizontal;
        slot.AnchorTop = vertical;
        slot.AnchorBottom = vertical;
        slot.GrowHorizontal = horizontal switch
        {
            0f => GrowDirection.End,
            1f => GrowDirection.Begin,
            _ => GrowDirection.Both,
        };
        slot.GrowVertical = vertical == 1f ? GrowDirection.Begin : GrowDirection.End;
        align.AddChild(slot);
        return slot;
    }
}
