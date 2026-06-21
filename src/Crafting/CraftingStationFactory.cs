using Embervale.Entities;
using Godot;

namespace Embervale.Crafting;

/// <summary>
/// Builds a world crafting station: a coloured block with a collider (so the player's
/// interaction raycast can hit it) and a <see cref="CraftingStationComponent"/>. Mirrors
/// the other code-built actors (e.g. <see cref="Items.ItemPickupFactory"/>); promote to a
/// packed scene later without touching callers.
/// </summary>
public static class CraftingStationFactory
{
    public static Entity Create(CraftingStationType station, string name, Vector3 position, Color color)
    {
        var entity = new Entity
        {
            Name = $"Station_{station}",
            DisplayName = name,
            TemplateId = $"station.{station.ToString().ToLowerInvariant()}",
            Position = position,
        };

        entity.AddChild(new MeshInstance3D
        {
            Name = "Mesh",
            Mesh = new BoxMesh { Size = new Vector3(0.9f, 1.0f, 0.9f) },
            Position = new Vector3(0f, 0.5f, 0f),
            MaterialOverride = new StandardMaterial3D
            {
                AlbedoColor = color,
                EmissionEnabled = true,
                Emission = color,
                EmissionEnergyMultiplier = 0.25f,
            },
        });

        var collider = new StaticBody3D { Name = "Collider" };
        collider.AddChild(new CollisionShape3D
        {
            Shape = new BoxShape3D { Size = new Vector3(0.9f, 1.0f, 0.9f) },
            Position = new Vector3(0f, 0.5f, 0f),
        });
        entity.AddChild(collider);

        entity.AddChild(new CraftingStationComponent
        {
            Name = "Station",
            Station = station,
            StationName = name,
        });

        return entity;
    }
}
