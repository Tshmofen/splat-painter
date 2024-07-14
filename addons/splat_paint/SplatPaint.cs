using Godot;

namespace GroundPainter;

public partial class SplatPaint : MeshInstance3D
{
    public const string PluginNodeAlias = nameof(SplatPaint);
    public const string PluginBaseAlias = nameof(MeshInstance3D);
    public const string ScriptPath = $"{nameof(SplatPaint)}.cs";
}