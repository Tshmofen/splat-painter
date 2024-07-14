using Godot;

namespace GroundPainter;

public partial class MeshPaint : MeshInstance3D
{
    public const string PluginNodeAlias = nameof(MeshPaint);
    public const string PluginBaseAlias = nameof(MeshInstance3D);
    public const string ScriptPath = $"{nameof(MeshPaint)}.cs";
}