using System;
using Godot;

namespace SplatPainter.UI;

[Tool]
public partial class PaintControl : HBoxContainer
{
    public const string PluginNodePath = "/UI/paint_control.tscn";

    [Export] private BaseButton PaintR { get; set; }
    [Export] private BaseButton PaintG { get; set; }
    [Export] private BaseButton PaintB { get; set; }
    [Export] private BaseButton PaintA { get; set; }
    [Export] private BaseButton Reset { get; set; }
    [Export] private Slider PaintForceSlider { get; set; }
    [Export] private Slider PaintSizeSlider { get; set; }
    [Export] private Label PaintForceLabel { get; set; }
    [Export] private Label PaintSizeLabel { get; set; }

    public Vector4 PaintMask { get; private set; } = new(0, 0, 0, 0);
    public float PaintForce { get; private set; } = 30;
    public float PaintSize { get; private set; } = 10;

    public event Action ResetCalled;

    #region Util

    private void UpdatePaintForce(double value)
    {
        PaintForce = (float) value;
        PaintForceLabel.Text = $"Force ({value})";
    }

    private void UpdatePaintSize(double value)
    {
        PaintSize = (float) value;
        PaintSizeLabel.Text = $"Size ({value})";
    }

    #endregion

    public override void _Ready()
    {
        PaintR.Pressed += () => UpdatePaintMask(new Vector4(1, 0, 0, 0));
        PaintG.Pressed += () => UpdatePaintMask(new Vector4(0, 1, 0, 0));
        PaintB.Pressed += () => UpdatePaintMask(new Vector4(0, 0, 1, 0));
        PaintA.Pressed += () => UpdatePaintMask(new Vector4(0, 0, 0, 1));
        Reset.Pressed += () => ResetCalled?.Invoke();
        UpdatePaintMask(new Vector4(1, 0, 0, 0));

        PaintForceSlider.ValueChanged += UpdatePaintForce;
        PaintForceSlider.Value = PaintForce;

        PaintSizeSlider.ValueChanged += UpdatePaintSize;
        PaintSizeSlider.Value = PaintSize;
    }

    public void UpdatePaintMask(Vector4 paintMask)
    {
        if (PaintMask == paintMask)
        {
            paintMask = Vector4.Zero;
        }

        PaintR.ButtonPressed = paintMask.X != 0;
        PaintG.ButtonPressed = paintMask.Y != 0;
        PaintB.ButtonPressed = paintMask.Z != 0;
        PaintA.ButtonPressed = paintMask.W != 0;
        PaintMask = paintMask;
    }
}
