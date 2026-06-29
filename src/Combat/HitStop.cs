namespace Embervale.Combat;

/// <summary>
/// Pure tuning maths for <b>hit-stop</b> (Phase 29A): how long the brief freeze-frame on a landed hit
/// lasts, in milliseconds, as a function of the hit's weight. Kept Godot-free so the feel curve is
/// unit-testable without an engine; <see cref="HitStopDirector"/> applies it against
/// <c>Engine.TimeScale</c>. All knobs live here — tweak and re-run to retune.
/// </summary>
public static class HitStop
{
    /// <summary>Damage at/above which a hit counts as fully "heavy" (the <see cref="HeavyMs"/> ceiling).</summary>
    public const float HeavyDamageRef = 30f;

    /// <summary>Hits dealing less than this never freeze — trivial chip stays fluid.</summary>
    public const float MinDamage = 4f;

    public const int LightMs = 45;   // a just-qualifying hit
    public const int HeavyMs = 110;  // a heavy (>= HeavyDamageRef) hit
    public const int CritBonusMs = 40;
    public const int StaggerMs = 160; // a poise-break — strictly the longest stop (> a heavy crit)
    public const int BlockMs = 30;    // a blocked hit: a light "tink", no real weight

    /// <summary>Freeze duration in ms for a hit of <paramref name="amount"/> damage. Blocked hits get a
    /// small fixed tick; otherwise the stop scales from <see cref="LightMs"/> to <see cref="HeavyMs"/>
    /// by damage, plus a crit bonus; a poise-break (<paramref name="staggered"/>) is the longest.
    /// Returns 0 below <see cref="MinDamage"/> (unless staggered) so trivial hits don't freeze.</summary>
    public static int DurationMs(float amount, bool isCrit, bool isBlocked, bool staggered)
    {
        if (staggered)
        {
            return StaggerMs;
        }

        if (isBlocked)
        {
            return BlockMs;
        }

        if (amount < MinDamage)
        {
            return 0;
        }

        float t = amount / HeavyDamageRef;
        t = t < 0f ? 0f : t > 1f ? 1f : t;
        int ms = LightMs + (int)((HeavyMs - LightMs) * t);
        return isCrit ? ms + CritBonusMs : ms;
    }
}
