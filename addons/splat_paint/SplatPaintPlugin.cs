#if TOOLS

using Godot;
using System.Linq;
using GroundPainter.UI;

namespace GroundPainter;

[Tool]
public partial class SplatPaintPlugin : EditorPlugin
{
    public const string PluginPath = "res://addons/splat_paint";
    public const string PaintControlNodePath = "/UI/paint_control.tscn";

    public EditorSelection Selection { get; private set; }
    public PaintControl PaintControl { get; private set; }
    public SplatPaint SplatPaint { get; private set; }

    #region Util

    private void OnSelectionChange()
    {
        var selectedNode = Selection.GetSelectedNodes().FirstOrDefault();

        if (selectedNode is not SplatPaint meshPaint)
        {
            SplatPaint = null;
            SwitchPaintControl(false);
            return;
        }

        SplatPaint = meshPaint;
        SwitchPaintControl(true);
    }

    private void AddCustomType(string type, string @base, string scriptPath, string iconPath = null)
    {
        var script = ResourceLoader.Load<Script>($"{PluginPath}/{scriptPath}");
        var icon = iconPath != null ? ResourceLoader.Load<Texture2D>($"{PluginPath}/Icons/{iconPath}") : null;
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

    #endregion

    public override void _EnterTree()
    {
        PaintControl = ResourceLoader.Load<PackedScene>(PluginPath + PaintControlNodePath).Instantiate<PaintControl>();

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
        //if (RiverManager is null)
        //{
        //    return 0;
        //}

        //if (@event is not InputEventMouseButton { ButtonIndex: MouseButton.Left } mouseEvent)
        //{
        //    return RiverControl.SpatialGuiInput(@event) ? 1 : 0;
        //}

        //var cameraPoint = mouseEvent.Position;
        //var (segment, point) = RiverCurveHelper.GetClosestPosition(RiverManager, camera, cameraPoint);

        //switch (RiverControl.CurrentEditMode)
        //{
        //    case RiverEditMode.Select:
        //    {
        //        return 0;
        //    }

        //    case RiverEditMode.Add when !mouseEvent.Pressed:
        //    {
        //        var newPoint = point;

        //        if (segment == -1)
        //        {
        //            newPoint = RiverCurveHelper.GetNewPoint(RiverManager, camera, cameraPoint, RiverControl.CurrentConstraint, RiverControl.IsLocalEditing);
        //        }

        //        if (newPoint == null)
        //        {
        //            return 0;
        //        }

        //        CommitPointAdd(newPoint.Value, segment, RiverCurveHelper.IsStartPointCloser(RiverManager, newPoint.Value));
        //        break;
        //    }

        //    case RiverEditMode.Remove when !mouseEvent.Pressed:
        //    {
        //        if (segment != -1 && point != null)
        //        {
        //            var closestIndex = RiverCurveHelper.GetClosestPointTo(RiverManager, point.Value);
        //            CommitPointRemove(closestIndex);
        //        }

        //        break;
        //    }
        //}

        return 0;
    }
}

#endif