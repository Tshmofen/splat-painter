using System.Linq;
using Godot;
using GroundPainter.addons.splat_paint.UI;
using GroundPainter.addons.splat_paint.Util;
using Image = Godot.Image;

namespace GroundPainter;

[Tool]
public partial class SplatPaint : MeshInstance3D
{
    public const string DefaultShaderPath = "/Shaders/ground_paint_shader.gdshader";
    public const string SplatBaseDirectory = "res://Textures/Splats";
    public const string PluginNodeAlias = nameof(SplatPaint);
    public const string PluginBaseAlias = nameof(MeshInstance3D);
    public const string ScriptPath = $"{nameof(SplatPaint)}.cs";
    public const string SplatMapParameter = "texture_splatmap";
    public const int SplatSize = 1024;

    private MeshUvTool _meshUvTool;
    private PaintSelector _paintSelector;
    private StaticBody3D _selectorCollision;

    [Export(PropertyHint.SaveFile, "*.png")] public string SplatFileName { get; set; }

    #region Util

    private static string GetUniqueName(string name, string[] existingNames)
    {
        var searchName = name;
        var order = 1;

        while (existingNames.Contains(searchName))
        {
            searchName = $"{name}_{order++}";
        }

        return searchName;
    }

    private static Image LoadOrCreateImage(string imagePath)
    {
        Image image;

        if (!FileAccess.FileExists(imagePath))
        {
            image = Image.Create(SplatSize, SplatSize, false, Image.Format.Rgba8);
            image.SavePng(imagePath);
        }
        else
        {
            image = Image.LoadFromFile(imagePath);
        }

        return image;
    }

    private bool IsCurrentShaderSupportsSplatMap()
    {
        if (Mesh.GetSurfaceCount() == 0)
        {
            return false;
        }

        if (Mesh.SurfaceGetMaterial(0) is not ShaderMaterial { Shader: not null } material)
        {
            return false;
        }

        var shaderProperties = material.Shader.GetShaderUniformList().Select(a => a.AsGodotDictionary());
        return shaderProperties.Any(a => a["name"].AsString() == SplatMapParameter);
    }

    #endregion

    public override void _Ready()
    {
        if (!Engine.IsEditorHint())
        {
            return;
        }

        if (string.IsNullOrEmpty(SplatFileName))
        {
            if (!DirAccess.DirExistsAbsolute(SplatBaseDirectory))
            {
                DirAccess.MakeDirRecursiveAbsolute(SplatBaseDirectory);
            }

            var existingFiles = DirAccess.GetFilesAt(SplatBaseDirectory);
            SplatFileName = $"{SplatBaseDirectory}/{GetUniqueName("splat", existingFiles)}.png";
        }
    }

    public override void _EnterTree()
    {
        if (_paintSelector != null)
        {
            return;
        }

        _paintSelector = ResourceLoader.Load<PackedScene>(SplatPaintPlugin.PluginPath + PaintSelector.PluginNodePath).Instantiate<PaintSelector>();
        AddChild(_paintSelector);
        _paintSelector.Visible = false;
    }

    public override void _ExitTree()
    {
        _meshUvTool = null;
        _paintSelector = null;
        _selectorCollision = null;
    }

    public override string[] _GetConfigurationWarnings()
    {
        if (Mesh == null)
        {
            return [ "Please provide a mesh for this instance to access splat painting." ];
        }

        if (Mesh.GetSurfaceCount() == 0)
        {
            return [ "Please provide a mesh with at least one material surface. Splat will be used on the first one." ];
        }

        if (!IsCurrentShaderSupportsSplatMap())
        {
            return [ $"Currently used material doesn't support splat texture ('{SplatMapParameter}'), it will be replaced with default shader on first paint request." ];
        }

        return [];
    }

    #region Actions

    public void InitSelectorCollision()
    {
        if (Mesh == null || _selectorCollision != null)
        {
            return;
        }

        _selectorCollision = new StaticBody3D();
        _selectorCollision.AddChild(new CollisionShape3D { Shape = Mesh.CreateTrimeshShape() });
        AddChild(_selectorCollision);
    }

    public void SwitchSelector(bool show)
    {
        if (_paintSelector == null)
        {
            return;
        }

        _paintSelector.Visible = show;
    }

    public void UpdateSelector(Vector3 globalPosition, float radius)
    {
        if (_paintSelector == null)
        {
            return;
        }

        _paintSelector.GlobalPosition = globalPosition;
        _paintSelector.SetRadiusPower(radius);
    }

    public void SelectorPaint(Vector3 globalPoint, Vector3 normal, Vector4 paintMask, float force)
    {
        if (Mesh == null || Mesh.GetSurfaceCount() == 0)
        {
            return;
        }

        if (!IsCurrentShaderSupportsSplatMap())
        {
            Mesh.SurfaceSetMaterial(0, new ShaderMaterial
            {
                Shader = ResourceLoader.Load<Shader>(SplatPaintPlugin.PluginPath + DefaultShaderPath)
            });
        }

        _meshUvTool ??= new MeshUvTool(this);
        GD.Print(_meshUvTool.GetUvCoordinates(globalPoint, normal));
    }

    #endregion
}