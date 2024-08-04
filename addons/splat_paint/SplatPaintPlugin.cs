#if TOOLS

using Godot;
using System.Linq;
using SplatPainter.UI;

namespace SplatPainter;

[Tool]
public partial class SplatPaintPlugin : EditorPlugin
{
    public const string PluginPath = "res://addons/splat_paint";

    private Image _initialImage;
    private bool _wasPaintingStore;

    public EditorSelection Selection { get; private set; }
    public PaintControl PaintControl { get; private set; }
    public SplatPaint SplatPaint { get; private set; }

    #region Util

    private void OnSelectionChange()
    {
        var selectedNode = Selection.GetSelectedNodes().FirstOrDefault();

        if (selectedNode != SplatPaint)
        {
            SplatPaint?.SwitchSelectorVisibility(false);
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
            PaintControl.ResetCalled += ResetSplatPaint;
            AddControlToContainer(CustomControlContainer.SpatialEditorMenu, PaintControl);
        }
        else if (!show && PaintControl.IsInsideTree())
        {
            PaintControl.ResetCalled -= ResetSplatPaint;
            RemoveControlFromContainer(CustomControlContainer.SpatialEditorMenu, PaintControl);
        }
    }

    private static (Vector3 point, Vector3 normal)? GetCameraCollision(Camera3D camera, Vector2 cameraPoint, SplatPaint splatPaint)
    {
        var rayFrom = camera.ProjectRayOrigin(cameraPoint);
        var rayDir = camera.ProjectRayNormal(cameraPoint);
        var rayParams = new PhysicsRayQueryParameters3D
        {
            From = rayFrom,
            To = rayFrom + (rayDir * 4096),
            CollisionMask = splatPaint.EditorCollisionLayer
        };

        var result = splatPaint.GetWorld3D().DirectSpaceState.IntersectRay(rayParams);
        if (result == null || result.Count == 0)
        {
            return null;
        }

        var collider = result["collider"].As<Node>();
        if (collider != splatPaint.GetCollider())
        {
            return null;
        }

        var collisionPoint = result["position"].AsVector3();
        var collisionNormal = result["normal"].AsVector3();
        return (collisionPoint, collisionNormal);

    }

    private void CommitImageChange(SplatPaint paint, Image newImage, Image initialImage)
    {
        if (paint == null)
        {
            return;
        }

        var undoRedo = GetUndoRedo();
        undoRedo.CreateAction($"SplatPaint '{paint.Name}' image have been updated.");
        undoRedo.AddDoMethod(paint, SplatPaint.MethodName.SetSplatImage, newImage);
        undoRedo.AddUndoMethod(paint, SplatPaint.MethodName.SetSplatImage, initialImage);
        undoRedo.CommitAction(false); // No need to execute, image already set
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
        var toPaint = Input.IsMouseButtonPressed(MouseButton.Left);
        var wasPainting = _wasPaintingStore;
        _wasPaintingStore = false;

        if (!toPaint && wasPainting)
        {
            CommitImageChange(SplatPaint, SplatPaint.GetSplatImage(), _initialImage);
        }

        // Input to painting mask
        var isEditorAction = Input.IsKeyPressed(Key.Ctrl) || Input.IsMouseButtonPressed(MouseButton.Right);
        if (@event is InputEventKey { Pressed: true, Echo: false } keyEvent && !isEditorAction)
        {
            Vector4? paintMask = keyEvent.Keycode switch
            {
                Key.R => new Vector4(1, 0, 0, 0),
                Key.G => new Vector4(0, 1, 0, 0),
                Key.B => new Vector4(0, 0, 1, 0),
                Key.A => new Vector4(0, 0, 0, 1),
                Key.C => new Vector4(0, 0, 0, 0),
                _ => null
            };

            if (paintMask != null)
            {
                PaintControl.UpdatePaintMask(paintMask.Value);
            }

            return 1;
        }

        if (SplatPaint is null || PaintControl.PaintMask == Vector4.Zero || @event is not InputEventMouse mouseEvent)
        {
            return 0;
        }

        var collision = GetCameraCollision(camera, mouseEvent.Position, SplatPaint);
        if (collision == null)
        {
            SplatPaint.SwitchSelectorVisibility(false);
            return 0;
        }

        var (collisionPoint, collisionNormal) = collision.Value;
        SplatPaint.SwitchSelectorVisibility(true);
        SplatPaint.UpdateSelector(collisionPoint, PaintControl.PaintSize);

        if (!toPaint)
        {
            return 0;
        }

        if (!wasPainting) // Save pre-change image for redo
        {
            _initialImage = SplatPaint.GetSplatImage();
        }

        _wasPaintingStore = true;
        SplatPaint.SelectorPaint(collisionPoint, collisionNormal, PaintControl.PaintMask, PaintControl.PaintForce, PaintControl.PaintSize);
        return 1; // break other controls when painting
    }

    public void ResetSplatPaint()
    {
        SplatPaint?.ResetEditor();
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