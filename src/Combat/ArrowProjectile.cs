using Embervale.Entities;
using Godot;

namespace Embervale.Combat;

/// <summary>
/// An arrow fired by a ranged <see cref="WeaponResource"/> (the bow). The weapon analogue of
/// <see cref="Magic.SpellProjectile"/>: an <see cref="Area3D"/> on the Hitbox layer that flies forward
/// each physics frame and delivers its <see cref="DamagePacket"/> to the first hostile
/// <see cref="Hurtbox"/> it overlaps (same friendly-fire rules as <see cref="Hitbox"/>), then expires
/// on a hit, on world geometry, or when its range runs out.
///
/// Not pooled — a bow fires one shot at a time, far below the churn that justified pooling spell bolts
/// (ponytail: add a NodePool if rapid fire ever shows up).
/// </summary>
public partial class ArrowProjectile : Area3D
{
    private DamagePacket _packet;
    private IEntity? _shooter;
    private int _team;
    private Vector3 _direction;
    private float _speed;
    private double _life;
    private bool _resolved = true; // inert until Launch arms it

    public override void _Ready()
    {
        CollisionLayer = CombatLayers.Hitbox;
        CollisionMask = CombatLayers.Hurtbox | CombatLayers.World;
        Monitorable = false;
        Monitoring = false;

        var material = new StandardMaterial3D { AlbedoColor = new Color(0.55f, 0.4f, 0.2f) };
        AddChild(new MeshInstance3D
        {
            Mesh = new BoxMesh { Size = new Vector3(0.05f, 0.05f, 0.7f) },
            MaterialOverride = material,
        });
        AddChild(new CollisionShape3D { Shape = new BoxShape3D { Size = new Vector3(0.1f, 0.1f, 0.7f) } });
    }

    /// <summary>Arms the arrow for flight. Add it to the tree and set GlobalPosition first.</summary>
    public void Launch(DamagePacket packet, IEntity? shooter, int team, Vector3 direction, float speed, float range)
    {
        _packet = packet;
        _shooter = shooter;
        _team = team;
        _direction = direction.Normalized();
        _speed = speed;
        _life = speed > 0f ? range / speed : 1d;

        // Point the shaft along its travel (skip near-vertical shots where LookAt is undefined).
        if (Mathf.Abs(_direction.Dot(Vector3.Up)) < 0.99f)
        {
            LookAt(GlobalPosition + _direction, Vector3.Up);
        }

        _resolved = false;
        Monitoring = true;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_resolved)
        {
            return;
        }

        GlobalPosition += _direction * _speed * (float)delta;
        _life -= delta;

        foreach (Area3D area in GetOverlappingAreas())
        {
            if (area is not Hurtbox hurtbox)
            {
                continue;
            }

            // Never the shooter, never an ally on the shooter's team (same rule as Hitbox).
            if (hurtbox.OwnerEntity != null && ReferenceEquals(hurtbox.OwnerEntity, _shooter))
            {
                continue;
            }

            if (hurtbox.Combat != null && hurtbox.Combat.Team == _team)
            {
                continue;
            }

            hurtbox.Receive(_packet);
            Resolve();
            return;
        }

        // Stick into world geometry; ignore the shooter's own body.
        foreach (Node3D body in GetOverlappingBodies())
        {
            if (_shooter?.Body != null && ReferenceEquals(body, _shooter.Body))
            {
                continue;
            }

            Resolve();
            return;
        }

        if (_life <= 0d)
        {
            Resolve();
        }
    }

    private void Resolve()
    {
        _resolved = true;
        Monitoring = false;
        QueueFree();
    }
}
