using Godot;

namespace GroundPainter.UI;

[Tool]
public partial class PaintControl : HBoxContainer
{
    [Export] private BaseButton PaintR { get; set; }
    [Export] private BaseButton PaintG { get; set; }
    [Export] private BaseButton PaintB { get; set; }
    [Export] private BaseButton PaintA { get; set; }
    [Export] private Slider PaintForceSlider { get; set; }
    [Export] private Slider PaintSizeSlider { get; set; }

    public Vector4 PaintMask { get; private set; } = new(0, 0, 0, 0);
    public float PaintForce { get; private set; } = 30;
    public float PaintSize { get; private set; } = 10;

    #region Util

    private void UpdatePaintMask(BaseButton button, Vector4 paintMask)
    {
        PaintR.ButtonPressed = PaintR == button;
        PaintG.ButtonPressed = PaintG == button;
        PaintB.ButtonPressed = PaintB == button;
        PaintA.ButtonPressed = PaintA == button;
        PaintMask = paintMask;
    }

    #endregion

    public override void _Ready()
    {
        PaintR.Pressed += () => UpdatePaintMask(PaintR, new Vector4(1, 0, 0, 0));
        PaintG.Pressed += () => UpdatePaintMask(PaintG, new Vector4(0, 1, 0, 0));
        PaintB.Pressed += () => UpdatePaintMask(PaintB, new Vector4(0, 0, 1, 0));
        PaintA.Pressed += () => UpdatePaintMask(PaintA, new Vector4(0, 0, 0, 1));
        UpdatePaintMask(PaintR, new Vector4(1, 0, 0, 0));

        PaintForceSlider.ValueChanged += value => PaintForce = (float) value;
        PaintSizeSlider.ValueChanged += value => PaintSize = (float) value;
    }
}
