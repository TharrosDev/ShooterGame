using Embervale.Entities;

namespace Embervale.Interaction;

/// <summary>
/// Base for anything the player can interact with via the <c>interact</c> action:
/// item pickups now, and later doors, levers, containers and NPC dialogue. The
/// player raycasts from the camera and calls <see cref="Interact"/> on the first
/// interactable it hits. Subclasses provide a <see cref="Prompt"/> for UI.
/// </summary>
public abstract partial class InteractableComponent : EntityComponent
{
    /// <summary>Short verb shown in the interaction prompt, e.g. "Pick up Health Potion".</summary>
    public abstract string Prompt { get; }

    /// <summary>Performs the interaction on behalf of <paramref name="instigator"/>.</summary>
    public abstract void Interact(IEntity instigator);
}
