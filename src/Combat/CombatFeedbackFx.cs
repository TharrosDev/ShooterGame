namespace Embervale.Combat;

/// <summary>The player combat states that get distinct screen feedback (Phase 29D).</summary>
public enum CombatFeedback
{
    Crit,
    Block,
    Stagger,
    Parry,
}

/// <summary>
/// Pure mapping from a player combat state to its screen-flash tint + intensity (Phase 29D). Godot-free
/// so it's unit-testable; <see cref="Embervale.UI.CombatFeedbackOverlay"/> turns it into a fading
/// full-screen flash. Tints follow the dying-world UI identity (30.5E, docs/UI_STYLE.md §1): crit is
/// THE ember-orange accent, block is cold steel, stagger ashen red, parry a bright ember-gold pop.
/// Knobs live here.
/// </summary>
public static class CombatFeedbackFx
{
    public const float HoldSeconds = 0.28f;

    /// <summary>Flash tint (r,g,b 0..1) for a state.</summary>
    public static (float R, float G, float B) Tint(CombatFeedback state) => state switch
    {
        CombatFeedback.Crit => (0.91f, 0.45f, 0.17f),    // ember orange (UiTheme.AccentHot)
        CombatFeedback.Block => (0.49f, 0.53f, 0.57f),   // cold steel (ART_STYLE palette)
        CombatFeedback.Stagger => (0.82f, 0.42f, 0.36f), // ashen red (UiTheme.Bad)
        CombatFeedback.Parry => (1.0f, 0.88f, 0.60f),    // bright ember-gold pop
        _ => (1f, 1f, 1f),
    };

    /// <summary>Peak flash alpha for a state — a parry/crit pops harder than a routine block.</summary>
    public static float PeakAlpha(CombatFeedback state) => state switch
    {
        CombatFeedback.Parry => 0.45f,
        CombatFeedback.Crit => 0.32f,
        CombatFeedback.Stagger => 0.40f,
        CombatFeedback.Block => 0.22f,
        _ => 0.25f,
    };
}
