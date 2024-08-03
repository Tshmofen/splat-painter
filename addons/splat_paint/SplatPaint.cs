using System.Linq;
using Godot;
using SplatPainter.UI;
using SplatPainter.Util;
using Image = Godot.Image;
using Vector4 = Godot.Vector4;

namespace SplatPainter;

[Tool]
public partial class SplatPaint : MeshInstance3D
{
    public const string PluginBaseAlias = nameof(MeshInstance3D);
    public const string PluginNodeAlias = nameof(SplatPaint);
    public const string ScriptPath = $"/{nameof(SplatPaint)}.cs";

    public const string SplatPaintStamp = "SplatPaint";
    public const string DefaultShaderPath = "/Shaders/ground_paint_shader.gdshader";
    public const string SplatMapParameter = "texture_splatmap";
    public const int SplatSize = 512;

    private MeshUvTool _meshUvTool;
    private PaintSelector _paintSelector;
    private StaticBody3D _selectorCollision;

    private int _splatResolution = 512;

    [Export] public int SplatResolution
    {
        get => _splatResolution;
        set
        {
            _splatResolution = value;

            if (_splatResolution < 32)
            {
                _splatResolution = 32;
            }

            if (Mesh == null || Mesh.GetSurfaceCount() == 0)
            {
                return;
            }

            var texture = GetSplatTexture();
            var imageSize = texture.GetSize();

            if ((int)imageSize.X == SplatResolution && (int)imageSize.Y == SplatResolution)
            {
                return;
            }

            var image = texture.GetImage();
            image.Resize(SplatResolution, SplatResolution);
            texture.SetImage(image);
        }
    }

    private uint _editorCollisionLayer = 1 << 15;
    [Export(PropertyHint.Layers3DPhysics)] public uint EditorCollisionLayer
    {
        get => _editorCollisionLayer;
        set
        {
            _editorCollisionLayer = value;
            if (_selectorCollision != null)
            {
                _selectorCollision.CollisionLayer = _editorCollisionLayer;
            }
        }
    }

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

    private static Image CreateSplatImage()
    {
        var image = Image.Create(SplatSize, SplatSize, false, Image.Format.Rgba8);
        image.Fill(new Color(1, 0, 0, 0));
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

    private static Color VectorToColor(Vector4 vector)
    {
        return new Color(vector.X, vector.Y, vector.Z, vector.W);
    }

    private static Vector4 ColorToVector(Color color)
    {
        return new Vector4(color.R, color.G, color.B, color.A);
    }

    private static Color BlendSplat(Color color, Vector4 paintMask, float factor)
    {
        var vectorColor = ColorToVector(color);
        return VectorToColor(paintMask * factor + vectorColor * (1 - factor));
    }

    private static float GetBrushFactor(float brushCenter, int x, int y, float maxFactor)
    {
        // Vector distance formula + hardcoded adjustments
        var adjustedFactor = 1 - ((x - brushCenter) * (x - brushCenter) + (y - brushCenter) * (y - brushCenter)) / (brushCenter * brushCenter / 2);
        return Mathf.Clamp(adjustedFactor, 0, maxFactor);
    }

    private ImageTexture GetSplatTexture()
    {
        if (!IsCurrentShaderSupportsSplatMap())
        {
            Mesh.SurfaceSetMaterial(0, new ShaderMaterial
            {
                Shader = SplatPaintPlugin.LoadPluginResource<Shader>(DefaultShaderPath)
            });
        }

        var shaderMaterial = (ShaderMaterial)Mesh.SurfaceGetMaterial(0);
        var splatTexture = shaderMaterial.GetShaderParameter(SplatMapParameter).As<ImageTexture>();

        if (splatTexture == null)
        {
            splatTexture = ImageTexture.CreateFromImage(CreateSplatImage());
            shaderMaterial.SetShaderParameter(SplatMapParameter, splatTexture);
        }

        return splatTexture;
    }

    private static void BlendMaskToImage(Image image, Vector4 paintMask, float paintSize, float paintForce, int destinationX, int destinationY)
    {
        var maxFactor = Mathf.Ease(paintForce / 100, 2.5f); // 2.5f Is best for gentle low values
        var splatSize = image.GetSize();

        // TODO: Supper ineffective, should be a viewport with shader, though is fast enough for me at the moment
        for (var x = 0; x < paintSize; x++)
        {
            for (var y = 0; y < paintSize; y++)
            {
                var finalX = destinationX + x;
                var finalY = destinationY + y;

                if ((finalY < 0 || finalY >= splatSize.Y) || (finalX < 0 || finalX >= splatSize.X))
                {
                    continue;
                }

                var factor = GetBrushFactor(paintSize / 2f, x, y, maxFactor);
                var currentColor = image.GetPixel(finalX, finalY);
                var targetColor = BlendSplat(currentColor, paintMask, factor);
                image.SetPixel(finalX, finalY, targetColor);
            }
        }
    }

    #endregion

    public override void _EnterTree()
    {
        _paintSelector = (PaintSelector) GetChildren().FirstOrDefault(c => c is PaintSelector);

        if (_paintSelector != null)
        {
            return;
        }

        _paintSelector = SplatPaintPlugin.LoadPluginResource<PackedScene>(PaintSelector.PluginNodePath).Instantiate<PaintSelector>();
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

        _selectorCollision = new StaticBody3D { CollisionLayer = EditorCollisionLayer };
        _selectorCollision.AddChild(new CollisionShape3D { Shape = Mesh.CreateTrimeshShape() });
        _selectorCollision.SetMeta(SplatPaintStamp, true);
        AddChild(_selectorCollision);
    }

    public void SwitchSelectorVisibility(bool show)
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

    public void SelectorPaint(Vector3 globalPoint, Vector3 normal, Vector4 paintMask, float paintForce, float paintSize)
    {
        _meshUvTool ??= new MeshUvTool(this);
        if (Mesh == null || Mesh.GetSurfaceCount() == 0)
        {
            return;
        }

        var uvPosition = _meshUvTool.GetUvCoordinates(globalPoint, normal);
        if (uvPosition == null)
        {
            return;
        }

        var splatTexture = GetSplatTexture();
        var splatImage = splatTexture.GetImage();
        var splatSize = splatImage.GetSize();

        var brushSize = paintSize * SplatResolution / Mesh.GetAabb().GetLongestAxisSize() / 1.8f; // Const to adjust for visible brush
        var destinationX = (int)(uvPosition.Value.X * splatSize.X - brushSize / 2f);
        var destinationY = (int)(uvPosition.Value.Y * splatSize.Y - brushSize / 2f);
        BlendMaskToImage(splatImage, paintMask, brushSize, paintForce, destinationX, destinationY);

        splatTexture.SetImage(splatImage);
    }

    public void SetSplatImage(Image image)
    {
        if (Mesh == null || Mesh.GetSurfaceCount() == 0)
        {
            return;
        }

        var splatTexture = GetSplatTexture();
        splatTexture.SetImage(image);
    }

    public Image GetSplatImage()
    {
        if (Mesh == null || Mesh.GetSurfaceCount() == 0)
        {
            return null;
        }

        var splatTexture = GetSplatTexture();
        return splatTexture.GetImage();
    }

    public void ResetEditor()
    {
        _meshUvTool = null;
        _selectorCollision?.QueueFree();
        _selectorCollision = null;
        InitSelectorCollision();
    }

    public Node GetCollider()
    {
        foreach (var node in GetChildren())
        {
            if (node is StaticBody3D collider && collider != _selectorCollision && collider.HasMeta("SplatPaint"))
            {
                collider.QueueFree();
            }
        }

        return _selectorCollision;
    }

    #endregion
}