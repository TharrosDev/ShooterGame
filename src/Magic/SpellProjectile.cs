using Embervale.Combat;
using Embervale.Entities;
using Godot;

namespace Embervale.Magic;

/// <summary>
/// A travelling spell bolt. It is the magic analogue of a melee <see cref="Hitbox"/>:
/// an <see cref="Area3D"/> on the Hitbox layer that flies forward each physics frame
/// and resolves the moment it overlaps an enemy hurtbox, hits world geometry, or runs
/// out of range. Resolution delegates to <see cref="SpellResolver"/> — a single-target
/// strike, or an area burst when the spell carries an <see cref="SpellResource.ImpactRadius"/>.
/// </summary>
public partial class SpellProjectile : Area3D
{
    private SpellResource _spell = null!;
    private DamagePacket _packet;
    private IEntity? _caster;
    private int _casterTeam;
    private Node? _casterBody;
    private Vector3 _direction;
    private double _life;
    private bool _resolved;

    /// <summary>Builds a detached projectile; the caller adds it to the scene and sets its position.</summary>
    public static SpellProjectile Create(
        SpellResource spell, DamagePacket packet, IEntity? caster, int casterTeam, Vector3 direction)
    {
        var projectile = new SpellProjectile
        {
            _spell = spell,
            _packet = packet,
            _caster = caster,
            _casterTeam = casterTeam,
            _casterBody = caster?.Body,
            _direction = direction.Normalized(),
        };

        projectile._life = spell.ProjectileSpeed > 0f ? spell.Range / spell.ProjectileSpeed : 2d;
        return projectile;
    }

    public override void _Ready()
    {
        CollisionLayer = CombatLayers.Hitbox;
        CollisionMask = CombatLayers.Hurtbox | CombatLayers.World;
        Monitorable = false;
        Monitoring = true;

        Color color = SpellSchools.Color(_spell.School);
        AddChild(new MeshInstance3D
        {
            Mesh = new SphereMesh { Radius = 0.18f, Height = 0.36f },
            MaterialOverride = new StandardMaterial3D
            {
                AlbedoColor = color,
                ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
                EmissionEnabled = true,
                Emission = color,
            },
        });

        AddChild(new CollisionShape3D { Shape = new SphereShape3D { Radius = 0.25f } });
        AddChild(new OmniLight3D { LightColor = color, OmniRange = 4f, LightEnergy = 1.2f });
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_resolved)
        {
            return;
        }

        GlobalPosition += _direction * _spell.ProjectileSpeed * (float)delta;
        _life -= delta;

        // Prefer a hurtbox we can actually damage.
        foreach (Area3D area in GetOverlappingAreas())
        {
            if (area is Hurtbox hurtbox && SpellResolver.IsHostileTarget(hurtbox, _caster, _casterTeam))
            {
                Resolve(hurtbox);
                return;
            }
        }

        // Otherwise detonate against world geometry / blocking bodies (but never the caster).
        foreach (Node3D body in GetOverlappingBodies())
        {
            if (_casterBody != null && ReferenceEquals(body, _casterBody))
            {
                continue;
            }

            Resolve(null);
            return;
        }

        if (_life <= 0d)
        {
            Resolve(null);
        }
    }

    private void Resolve(Hurtbox? primary)
    {
        _resolved = true;

        if (_spell.ImpactRadius > 0f)
        {
            SpellResolver.Detonate(this, _spell, _packet, _caster, _casterTeam, GlobalPosition, _spell.ImpactRadius);
        }
        else if (primary != null)
        {
            SpellResolver.HitOne(primary, _packet, _spell, _caster);
        }

        QueueFree();
    }
}
