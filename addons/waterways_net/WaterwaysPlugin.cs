#if TOOLS

using Godot;
using System.Linq;
using GroundPainter.Data.UI;
using GroundPainter.UI;

namespace GroundPainter;

[Tool]
public partial class WaterwaysPlugin : EditorPlugin
{
    public const string PluginPath = "res://addons/waterways_net";
    public const string PaintControlNodePath = "/UI/paint_control.tscn";

    public EditorSelection Selection { get; private set; }
    public PaintControl PaintControl { get; private set; }
    public MeshPaint MeshPaint { get; private set; }

    #region Util

    private void OnSelectionChange()
    {
        var selectedNode = Selection.GetSelectedNodes().FirstOrDefault();

        if (selectedNode is not MeshPaint meshPaint)
        {
            MeshPaint = null;
            SwitchRiverControl(false);
            return;
        }

        MeshPaint = meshPaint;
        SwitchRiverControl(true);
    }

    private void OnMenuActionPressed(PaintMenuActionType action)
    {
        if (MeshPaint == null)
        {
            return;
        }
    }

    private void AddCustomType(string type, string @base, string scriptPath, string iconPath)
    {
        var script = ResourceLoader.Load<Script>($"{PluginPath}/{scriptPath}");
        var icon = ResourceLoader.Load<Texture2D>($"{PluginPath}/Icons/{iconPath}");
        AddCustomType(type, @base, script, icon);
    }

    private void SwitchRiverControl(bool show)
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
        //RiverControl = ResourceLoader.Load<PackedScene>(PluginPath + RiverControlNodePath).Instantiate<RiverControl>();
        //RiverControl.MenuAction += OnMenuActionPressed;
        //RiverGizmo = new RiverGizmo { EditorPlugin = this };

        //Selection = EditorInterface.Singleton.GetSelection();
        //Selection.SelectionChanged += OnSelectionChange;
        //OnSelectionChange();

        //AddNode3DGizmoPlugin(RiverGizmo);
        //AddCustomType(RiverManager.PluginNodeAlias, RiverManager.PluginBaseAlias, RiverManager.ScriptPath, RiverManager.IconPath);
        //AddCustomType(RiverFloatSystem.PluginNodeAlias, RiverFloatSystem.PluginBaseAlias, RiverFloatSystem.ScriptPath, RiverFloatSystem.IconPath);
    }

    public override void _ExitTree()
    {
        //RemoveCustomType(RiverFloatSystem.PluginNodeAlias);
        //RemoveCustomType(RiverManager.PluginNodeAlias);
        //RemoveNode3DGizmoPlugin(RiverGizmo);
        //SwitchRiverControl(false);
        //Selection.SelectionChanged -= OnSelectionChange;
    }

    public override bool _Handles(GodotObject @object)
    {
        return @object is MeshPaint;
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

        return 1;
    }
}

#endif