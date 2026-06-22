namespace Embervale.Corruption;

/// <summary>
/// Named bands of the player's corruption, derived from a numeric corruption value in the
/// range [0, 100]. Tiers drive consequences (appearance, dialogue gates, NPC dread, corrupted
/// abilities) and feed the Dawnfire / Lord of Embers ending dial. Ordered low→high so tier
/// comparisons (e.g. "at or above a threshold") work directly.
/// </summary>
// APPEND ONLY: ordinals persist in .tres/saves and ride CorruptionTier* events — never
// reorder/insert/remove (EnumStabilityTests).
public enum CorruptionTier
{
    Untainted,
    Touched,
    Marked,
    Ashbound,
    Embers,
}

/// <summary>Maps corruption values to <see cref="CorruptionTier"/>s and provides labels.</summary>
public static class CorruptionTiers
{
    public const int Min = 0;
    public const int Max = 100;

    /// <summary>The tier a numeric corruption value falls into.</summary>
    public static CorruptionTier Of(int value)
    {
        return value switch
        {
            < 20 => CorruptionTier.Untainted,
            < 40 => CorruptionTier.Touched,
            < 60 => CorruptionTier.Marked,
            < 80 => CorruptionTier.Ashbound,
            _ => CorruptionTier.Embers,
        };
    }

    public static string Label(CorruptionTier tier) => tier switch
    {
        CorruptionTier.Untainted => "Untainted",
        CorruptionTier.Touched => "Touched",
        CorruptionTier.Marked => "Marked",
        CorruptionTier.Ashbound => "Ashbound",
        _ => "Embers",
    };

    /// <summary>Corruption at or above this commits the player to the Lord of Embers path.</summary>
    public const int LordOfEmbersThreshold = 60;

    /// <summary>Corruption below this keeps the player eligible for the Dawnfire path.</summary>
    public const int DawnfireThreshold = 40;

    /// <summary>The ending the player's corruption currently makes them eligible for (Phase 23H).
    /// Low corruption leans <see cref="EndingPath.Dawnfire"/>, high leans
    /// <see cref="EndingPath.LordOfEmbers"/>, and the band between is
    /// <see cref="EndingPath.Undecided"/>. Phase 49 consumes this for the final choice.</summary>
    public static EndingPath EligibilityOf(int value)
    {
        if (value >= LordOfEmbersThreshold)
        {
            return EndingPath.LordOfEmbers;
        }

        if (value < DawnfireThreshold)
        {
            return EndingPath.Dawnfire;
        }

        return EndingPath.Undecided;
    }
}
