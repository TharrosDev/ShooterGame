using System;
using Godot;

namespace Embervale.Enemies;

/// <summary>
/// Pure perception geometry behind <see cref="EnemyAIComponent"/>'s sight check. Kept Godot-free
/// (managed <see cref="Vector3"/> math only — no engine calls) so the load-bearing view-cone test,
/// run for every enemy on every perception tick, is unit-testable. The component still owns the
/// range check, the proximity bubble, and the line-of-sight raycast.
/// </summary>
public static class EnemyPerception
{
    /// <summary>Whether a target lies within a <paramref name="fovDegrees"/>-wide view cone centred on
    /// <paramref name="forwardFlat"/>. Both vectors are horizontal (Y stripped). A degenerate forward
    /// (zero-length) returns true — facing is unknown, so the cone can't reject. Matches the
    /// component's prior inline check exactly.</summary>
    public static bool InViewCone(Vector3 forwardFlat, Vector3 toTargetFlat, float fovDegrees)
    {
        if (forwardFlat.LengthSquared() < 1e-4f || toTargetFlat.LengthSquared() < 1e-4f)
        {
            return true;
        }

        float dot = forwardFlat.Normalized().Dot(toTargetFlat.Normalized());
        float cosHalfFov = MathF.Cos(fovDegrees * 0.5f * (MathF.PI / 180f));
        return dot >= cosHalfFov;
    }
}
