#if TOOLS

using Godot;
using System.Linq;
using GroundPainter.Data;
using GroundPainter.UI;

namespace GroundPainter;

public partial class WaterwaysPlugin : EditorPlugin
{
    public const string PluginPath = "res://addons/ground_painter";
    public const string PaintControlNodePath = "/UI/paint_control.tscn";

    public EditorSelection Selection { get; private set; }
    public PaintControl PaintControl { get; private set; }
    public MeshPaint MeshPaint { get; private set; }

    #region Util

    private void OnSelectionChange()
    {
        var selectedNode = Selection.GetSelectedNodes().FirstOrDefault();

        if (selectedNode is not MeshPaint meshPainter)
        {
            MeshPaint = null;
            SwitchPaintControl(false);
            return;
        }

        MeshPaint = meshPainter;
        SwitchPaintControl(true);
    }

    private void OnMenuActionPressed(PaintMenuActionType action)
    {
        if (MeshPaint == null)
        {
            return;
        }

        //switch (action)
        //{
        //    case RiverMenuActionType.GenerateMeshSibling:
        //        if (RiverManager.Owner != null)
        //        {
        //            var meshCopy = RiverManager.GetMeshCopy();
        //            RiverManager.GetParent().AddChild(meshCopy);
        //            meshCopy.Owner = RiverManager.GetTree().EditedSceneRoot;
        //        }
        //        else
        //        {
        //            GD.PushWarning("Cannot create MeshInstance3D sibling when River is root.");
        //        }

        //        break;

        //    case RiverMenuActionType.RecenterRiver:
        //        RiverManager.RecenterCurve();
        //        var undoRedo = GetUndoRedo();
        //        var riverId = undoRedo.GetObjectHistoryId(RiverManager);
        //        undoRedo.GetHistoryUndoRedo(riverId).ClearHistory();
        //        GD.PushWarning("RiverManager UndoRedo history was cleared.");
        //        break;
        //}
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
        PaintControl.MenuAction += OnMenuActionPressed;

        Selection = EditorInterface.Singleton.GetSelection();
        Selection.SelectionChanged += OnSelectionChange;
        OnSelectionChange();

        //AddNode3DGizmoPlugin(RiverGizmo);
        AddCustomType(MeshPaint.PluginNodeAlias, MeshPaint.PluginBaseAlias, MeshPaint.ScriptPath);
    }

    public override void _ExitTree()
    {
        RemoveCustomType(MeshPaint.PluginNodeAlias);
        //RemoveNode3DGizmoPlugin(RiverGizmo);
        SwitchPaintControl(false);
        Selection.SelectionChanged -= OnSelectionChange;
    }

    public override bool _Handles(GodotObject @object)
    {
        return @object is MeshPaint;
    }

    public override int _Forward3DGuiInput(Camera3D camera, InputEvent @event)
    {
        // TODO: handle painting
        return 0;
    }
}

#endif