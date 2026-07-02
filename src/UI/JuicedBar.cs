using Godot;

namespace Embervale.UI;

/// <summary>
/// A resource bar with value-change juice (30.5C): rises instantly, but drains with a short
/// lag so hits read as a visible chunk sliding off, and pulses the fill white-hot for a beat
/// when the value drops. Honours reduced motion (snaps, no pulse) via <see cref="UiTheme"/>.
/// Drive it with <see cref="SetTarget"/> each frame; use <see cref="Snap"/> when the subject
/// changes (new nameplate target, new boss) so the lag never animates across subjects.
/// </summary>
public partial class JuicedBar : ProgressBar
{
    /// <summary>Normalized units drained per second while lagging down toward the target.</summary>
    private const float DrainPerSecond = 0.9f;
    private const float PulseSeconds = 0.25f;

    private StyleBoxFlat _fillBox = null!;
    private Color _fill;
    private double _target = 1d;
    private double _pulse;

    /// <summary>Builds a themed bar (same look as <see cref="UiTheme.Bar"/>) with juice.</summary>
    public static JuicedBar Create(Color fill, float width = 168f)
    {
        var bar = new JuicedBar
        {
            MinValue = 0d,
            MaxValue = 1d,
            Value = 1d,
            ShowPercentage = false,
            CustomMinimumSize = new Vector2(width, 13f),
            _fill = fill,
            _fillBox = UiTheme.BarStyle(fill),
        };
        bar.AddThemeStyleboxOverride("background", UiTheme.BarStyle(UiTheme.Trough));
        bar.AddThemeStyleboxOverride("fill", bar._fillBox);
        return bar;
    }

    /// <summary>Sets the value the bar settles toward; a drop triggers the drain lag + pulse.</summary>
    public void SetTarget(double value)
    {
        value = Mathf.Clamp(value, 0d, 1d);
        if (!UiTheme.MotionEnabled)
        {
            _target = value;
            Value = value;
            return;
        }

        if (value < _target - 0.001d)
        {
            _pulse = PulseSeconds;
        }

        _target = value;
    }

    /// <summary>Jumps straight to <paramref name="value"/> with no lag or pulse (subject changed).</summary>
    public void Snap(double value)
    {
        _target = Mathf.Clamp(value, 0d, 1d);
        Value = _target;
        _pulse = 0d;
        _fillBox.BgColor = _fill;
    }

    public override void _Process(double delta)
    {
        // Rise instantly (heals feel responsive); drain with a lag (hits read as a sliding chunk).
        Value = Value < _target ? _target : Mathf.MoveToward((float)Value, (float)_target, (float)delta * DrainPerSecond);

        if (_pulse > 0d)
        {
            _pulse = Mathf.Max(_pulse - delta, 0d);
            _fillBox.BgColor = _fill.Lerp(Colors.White, (float)(_pulse / PulseSeconds) * 0.75f);
        }
    }
}
