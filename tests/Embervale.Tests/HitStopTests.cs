using Embervale.Combat;
using Xunit;

namespace Embervale.Tests;

/// <summary>
/// Covers the pure hit-stop feel curve (Phase 29A): heavier hits freeze longer, crits add weight, a
/// poise-break is the longest stop, a blocked hit only ticks, and trivial chip doesn't freeze at all.
/// </summary>
public class HitStopTests
{
    [Fact]
    public void HeavierHits_FreezeLonger()
    {
        int light = HitStop.DurationMs(8f, isCrit: false, isBlocked: false, staggered: false);
        int heavy = HitStop.DurationMs(HitStop.HeavyDamageRef, isCrit: false, isBlocked: false, staggered: false);
        Assert.True(heavy > light);
    }

    [Fact]
    public void Crit_AddsWeight()
    {
        int normal = HitStop.DurationMs(20f, isCrit: false, isBlocked: false, staggered: false);
        int crit = HitStop.DurationMs(20f, isCrit: true, isBlocked: false, staggered: false);
        Assert.True(crit > normal);
    }

    [Fact]
    public void Stagger_IsTheLongest()
    {
        int stagger = HitStop.DurationMs(0f, isCrit: false, isBlocked: false, staggered: true);
        int heavyCrit = HitStop.DurationMs(HitStop.HeavyDamageRef, isCrit: true, isBlocked: false, staggered: false);
        Assert.True(stagger > heavyCrit);
    }

    [Fact]
    public void Blocked_IsASmallFixedTick()
    {
        Assert.Equal(HitStop.BlockMs, HitStop.DurationMs(50f, isCrit: false, isBlocked: true, staggered: false));
    }

    [Fact]
    public void TrivialChip_DoesNotFreeze()
    {
        Assert.Equal(0, HitStop.DurationMs(HitStop.MinDamage - 1f, isCrit: false, isBlocked: false, staggered: false));
    }
}
