using System;
using Embervale.Enemies;
using Godot;
using Xunit;

namespace Embervale.Tests;

/// <summary>
/// Covers the pure view-cone geometry behind enemy sight. The full perception path (range, proximity
/// bubble, line-of-sight raycast) runs in-engine, but the FOV cone test — run for every enemy on
/// every perception tick — is pure and pinned here. Forward is -Z (Godot convention).
/// </summary>
public class EnemyPerceptionTests
{
    private static readonly Vector3 Forward = new(0f, 0f, -1f);
    private const float Fov = 110f; // half-angle 55°

    /// <summary>A flat direction <paramref name="degreesOffForward"/> away from -Z, in the XZ plane.</summary>
    private static Vector3 OffForward(float degreesOffForward)
    {
        float r = degreesOffForward * (MathF.PI / 180f);
        return new Vector3(MathF.Sin(r), 0f, -MathF.Cos(r));
    }

    [Fact]
    public void DeadAhead_IsInCone()
    {
        Assert.True(EnemyPerception.InViewCone(Forward, Forward, Fov));
    }

    [Fact]
    public void DirectlyBehind_IsNotInCone()
    {
        Assert.False(EnemyPerception.InViewCone(Forward, new Vector3(0f, 0f, 1f), Fov));
    }

    [Fact]
    public void NinetyDegreesToTheSide_IsOutsideA110DegreeCone()
    {
        Assert.False(EnemyPerception.InViewCone(Forward, new Vector3(1f, 0f, 0f), Fov));
    }

    [Fact]
    public void JustInsideHalfFov_IsInCone()
    {
        Assert.True(EnemyPerception.InViewCone(Forward, OffForward(54f), Fov));
    }

    [Fact]
    public void JustOutsideHalfFov_IsNotInCone()
    {
        Assert.False(EnemyPerception.InViewCone(Forward, OffForward(56f), Fov));
    }

    [Fact]
    public void DegenerateVectors_DefaultToVisible()
    {
        Assert.True(EnemyPerception.InViewCone(Vector3.Zero, Forward, Fov));
        Assert.True(EnemyPerception.InViewCone(Forward, Vector3.Zero, Fov));
    }
}
