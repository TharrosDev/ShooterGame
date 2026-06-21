using System;
using System.Collections.Generic;
using Embervale.Core.Diagnostics;
using Godot;

namespace Embervale.Save;

/// <summary>
/// Maps a stable actor template id (e.g. <c>"prop.cache"</c>) to the factory that builds that
/// archetype, for actors whose <em>existence</em> must survive save/load. The
/// <see cref="PersistentSpawnDirector"/> records live persistent actors in its save manifest and,
/// on load, recreates the missing ones by resolving their template id through here — so a saved
/// world rebuilds its named NPCs / containers / props, not just the components of actors that
/// happen to still be in the scene.
///
/// Mirrors <see cref="Embervale.Enemies.EnemyTemplateRegistry"/>: a new persistent archetype is a
/// new builder plus one <see cref="Register"/> line in the bootstrap. Builders return the actor's
/// host node (an <see cref="Embervale.Entities.IEntity"/>); the director assigns its identity and
/// transform after construction.
/// </summary>
public static class PersistentActorRegistry
{
    private static readonly Dictionary<string, Func<Vector3, Node3D>> Builders = new();

    /// <summary>All registered template ids (the validator/director source of truth).</summary>
    public static IReadOnlyCollection<string> TemplateIds => Builders.Keys;

    /// <summary>Registers (or replaces) the builder for a template id.</summary>
    public static void Register(string templateId, Func<Vector3, Node3D> builder)
    {
        if (string.IsNullOrEmpty(templateId) || builder == null)
        {
            Log.Warn("PersistentActorRegistry.Register ignored a null/empty template id or builder.");
            return;
        }

        if (Builders.ContainsKey(templateId))
        {
            Log.Warn($"Persistent actor template '{templateId}' is being replaced in the registry.");
        }

        Builders[templateId] = builder;
    }

    public static bool IsRegistered(string templateId)
    {
        return !string.IsNullOrEmpty(templateId) && Builders.ContainsKey(templateId);
    }

    /// <summary>Clears the registry (called before re-seeding from the bootstrap).</summary>
    public static void Clear() => Builders.Clear();

    /// <summary>Builds an actor of the given template at a position, or null (logged) if unknown.</summary>
    public static Node3D? Create(string templateId, Vector3 position)
    {
        if (Builders.TryGetValue(templateId, out Func<Vector3, Node3D>? builder))
        {
            return builder(position);
        }

        Log.Warn($"Persistent actor template '{templateId}' is not registered; cannot recreate it.");
        return null;
    }
}
