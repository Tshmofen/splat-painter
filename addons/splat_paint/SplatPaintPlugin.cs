#if TOOLS

using Godot;
using System.Linq;
using GroundPainter.UI;

namespace GroundPainter;

[Tool]
public partial class SplatPaintPlugin : EditorPlugin
{
    public const string PluginPath = "res://addons/splat_paint";

    public EditorSelection Selection { get; private set; }
    public PaintControl PaintControl { get; private set; }
    public SplatPaint SplatPaint { get; private set; }

    #region Util

    private void OnSelectionChange()
    {
        var selectedNode = Selection.GetSelectedNodes().FirstOrDefault();

        if (selectedNode != SplatPaint)
        {
            SplatPaint?.SwitchSelector(false);
        }

        if (selectedNode is not SplatPaint newSplatPaint)
        {
            SplatPaint = null;
            SwitchPaintControl(false);
            return;
        }

        SplatPaint = newSplatPaint;
        SwitchPaintControl(true);
        SplatPaint.InitSelectorCollision();
    }

    private void AddCustomType(string type, string @base, string scriptPath, string iconPath = null)
    {
        var script = LoadPluginResource<Script>(scriptPath);
        var icon = iconPath != null ? LoadPluginResource<Texture2D>($"/Icons/{iconPath}") : null;
        AddCustomType(type, @base, script, icon);
    }

    private void SwitchPaintControl(bool show)
    {
        if (show && !PaintControl.IsInsideTree())
        {
            AddControlToContainer(CustomControlContainer.SpatialEditorMenu, PaintControl);
        }
        else if (!show && PaintControl.IsInsideTree())
        {
            RemoveControlFromContainer(CustomControlContainer.SpatialEditorMenu, PaintControl);
        }
    }

    private static (Vector3 point, Vector3 normal)? GetCameraCollision(Camera3D camera, Vector2 cameraPoint, World3D world)
    {
        var rayFrom = camera.ProjectRayOrigin(cameraPoint);
        var rayDir = camera.ProjectRayNormal(cameraPoint);
        var rayParams = new PhysicsRayQueryParameters3D
        {
            From = rayFrom,
            To = rayFrom + (rayDir * 4096)
        };

        var result = world.DirectSpaceState.IntersectRay(rayParams);
        if (result == null || result.Count == 0)
        {
            return null;
        }

        var collisionPoint = result["position"].AsVector3();
        var collisionNormal = result["normal"].AsVector3();
        return (collisionPoint, collisionNormal);

    }

    #endregion

    public override void _EnterTree()
    {
        PaintControl = LoadPluginResource<PackedScene>(PaintControl.PluginNodePath).Instantiate<PaintControl>();

        Selection = EditorInterface.Singleton.GetSelection();
        Selection.SelectionChanged += OnSelectionChange;
        OnSelectionChange();

        AddCustomType(SplatPaint.PluginNodeAlias, SplatPaint.PluginBaseAlias, SplatPaint.ScriptPath);
    }

    public override void _ExitTree()
    {
        RemoveCustomType(SplatPaint.PluginNodeAlias);
        SwitchPaintControl(false);
        Selection.SelectionChanged -= OnSelectionChange;
    }

    public override bool _Handles(GodotObject @object)
    {
        return @object is SplatPaint;
    }

    public override int _Forward3DGuiInput(Camera3D camera, InputEvent @event)
    {
        if (SplatPaint is null || PaintControl.PaintMask == Vector4.Zero)
        {
            return 0;
        }

        if (@event is not InputEventMouse mouseEvent)
        {
            return 0;
        }

        var cameraPoint = mouseEvent.Position;
        var toPaint = Input.IsMouseButtonPressed(MouseButton.Left);
        var collision = GetCameraCollision(camera, cameraPoint, SplatPaint.GetWorld3D());

        if (collision == null)
        {
            SplatPaint.SwitchSelector(false);
            return 0;
        }

        var (collisionPoint, collisionNormal) = collision.Value;
        SplatPaint.SwitchSelector(true);
        SplatPaint.UpdateSelector(collisionPoint, PaintControl.PaintSize);

        if (!toPaint)
        {
            return 0;
        }

        SplatPaint.SelectorPaint(collisionPoint, collisionNormal, PaintControl.PaintMask, PaintControl.PaintForce, PaintControl.PaintSize);
        return 1; // break other controls when painting
    }

    public static TResource LoadPluginResource<TResource>(string relativePath) where TResource : class
    {
        return ResourceLoader.Load<TResource>(PluginPath + relativePath);
    }

    public static Image LoadPluginImage(string relativePath)
    {
        return Image.LoadFromFile(PluginPath + relativePath);
    }
}

#endif