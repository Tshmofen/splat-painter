using Godot;

namespace SplatPainter.UI;

[Tool]
public partial class PaintSelector : MeshInstance3D
{
    public const string PluginNodePath = "/UI/paint_selector.tscn";

    public const float MaxSize = 20;
    public const float MinSize = 0.5f;

    public void SetRadiusPower(float radiusPower)
    {
        var sphere = (SphereMesh)Mesh;
        var actualRadius = float.Lerp(MinSize, MaxSize, radiusPower / 100);
        sphere.Radius = actualRadius;
        sphere.Height = 2 * actualRadius;
    }
}
