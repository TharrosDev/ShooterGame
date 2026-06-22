using Embervale.Corruption;
using Embervale.Stats;
using Godot;

namespace Embervale.Progression;

/// <summary>
/// A designer-authored perk: a rankable passive that boosts one stat. Each rank
/// costs <see cref="Cost"/> skill points and adds <see cref="ValuePerRank"/> of the
/// target <see cref="Stat"/> (so rank R grants <c>ValuePerRank × R</c>). Authored as
/// a <c>.tres</c> under <c>data/perks/</c> and indexed by <see cref="PerkDatabase"/>;
/// a <see cref="PerksComponent"/> applies the bonus as a <see cref="StatModifier"/>.
///
/// One stat per perk keeps the <c>.tres</c> simple and authorable; richer perks
/// (multiple stats, unlocks, active abilities) can layer on later without changing
/// the spend/save flow.
/// </summary>
[GlobalClass]
public partial class PerkResource : Resource
{
    /// <summary>Stable id, e.g. "perk.toughness". The save/database key.</summary>
    [Export] public string Id { get; set; } = "perk.unknown";

    [Export] public string DisplayName { get; set; } = "Unknown Perk";

    [Export(PropertyHint.MultilineText)]
    public string Description { get; set; } = string.Empty;

    /// <summary>Maximum number of ranks that can be purchased.</summary>
    [Export] public int MaxRank { get; set; } = 1;

    /// <summary>Skill-point cost per rank.</summary>
    [Export] public int Cost { get; set; } = 1;

    [Export] public StatType Stat { get; set; } = StatType.Health;
    [Export] public ModifierType ModifierType { get; set; } = ModifierType.Flat;
    [Export] public float ValuePerRank { get; set; } = 1f;

    /// <summary>Minimum corruption tier required to learn this perk (Phase 23H).
    /// <see cref="CorruptionTier.Untainted"/> (the default) leaves a perk ungated; a higher
    /// value marks it a corrupted passive unlocked only by corruption.</summary>
    [Export] public CorruptionTier MinCorruptionTier { get; set; } = CorruptionTier.Untainted;

    /// <summary>The cumulative bonus value granted at the given rank.</summary>
    public float ValueAtRank(int rank) => ValuePerRank * rank;
}
