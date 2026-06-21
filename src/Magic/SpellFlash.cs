using Godot;

namespace Embervale.Magic;

/// <summary>
/// A short-lived expanding, fading sphere that visualises an area-of-effect burst.
/// Purely cosmetic and self-freeing — it has no gameplay role (the damage is already
/// resolved by <see cref="SpellResolver"/>), it just makes a burst legible at a glance.
/// </summary>
public partial class SpellFlash : Node3D
{
    /// <summary>The radius the flash grows to (matched to the spell's burst radius).</summary>
    public float Radius { get; set; } = 2f;

    public Color FlashColor { get; set; } = Colors.White;

    private const float SeedRadius = 0.1f;
    private const double LifeSeconds = 0.3d;

    private MeshInstance3D _mesh = null!;
    private StandardMaterial3D _material = null!;
    private double _age;

    public override void _Ready()
    {
        _material = new StandardMaterial3D
        {
            AlbedoColor = new Color(FlashColor.R, FlashColor.G, FlashColor.B, 0.5f),
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            EmissionEnabled = true,
            Emission = FlashColor,
        };

        _mesh = new MeshInstance3D
        {
            Mesh = new SphereMesh { Radius = SeedRadius, Height = SeedRadius * 2f },
            MaterialOverride = _material,
        };
        AddChild(_mesh);
    }

    public override void _Process(double delta)
    {
        _age += delta;
        float t = (float)(_age / LifeSeconds);
        if (t >= 1f)
        {
            QueueFree();
            return;
        }

        float scale = Mathf.Lerp(1f, Radius / SeedRadius, t);
        _mesh.Scale = Vector3.One * scale;
        _material.AlbedoColor = new Color(FlashColor.R, FlashColor.G, FlashColor.B, 0.5f * (1f - t));
    }
}
