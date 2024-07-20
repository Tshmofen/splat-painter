using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace SplatPainter.Util;

public class MeshUvTool
{
    private readonly MeshDataTool _meshTool;
    private readonly int _faceCount;

    private readonly List<int[]> _localFaceIndexList = [];
    private readonly List<Vector3[]> _worldFaceList = [];
    private readonly List<Vector3> _worldNormalList = [];

    #region Util

    private void LoadMeshData(MeshInstance3D meshInstance)
    {
        for (var index = 0; index < _faceCount; index++)
        {
            _worldNormalList[index] = meshInstance.GlobalTransform.Basis * _meshTool.GetFaceNormal(index);

            var faceVertex1 = _meshTool.GetFaceVertex(index, 0);
            var faceVertex2 = _meshTool.GetFaceVertex(index, 1);
            var faceVertex3 = _meshTool.GetFaceVertex(index, 2);

            _localFaceIndexList.Add([
                faceVertex1,
                faceVertex2,
                faceVertex3
            ]);

            _worldFaceList.Add([
                meshInstance.GlobalTransform * _meshTool.GetVertex(faceVertex1),
                meshInstance.GlobalTransform * _meshTool.GetVertex(faceVertex2),
                meshInstance.GlobalTransform * _meshTool.GetVertex(faceVertex3)
            ]);
        }
    }

    private (int index, Vector3 barycentricPoint)? GetFace(Vector3 point, Vector3 normal, float epsilon = 0.2f)
    {
        for (var index = 0; index < _faceCount; index++)
        {
            var worldNormal = _worldNormalList[index];
            if (!IsEqualsEpsilon(worldNormal, normal, epsilon))
            {
                continue;
            }

            var vertices = _worldFaceList[index];
            if (IsPointInTriangle(point, vertices[0], vertices[1], vertices[2], out var barycentricPoint))
            {
                return (index, barycentricPoint);
            }
        }

        return null;
    }

    private static Vector3 ConvertCartesianToBarycentric(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
    {
        var v0 = b - a;
        var v1 = c - a;
        var v2 = p - a;

        var d00 = v0.Dot(v0);
        var d01 = v0.Dot(v1);
        var d11 = v1.Dot(v1);
        var d20 = v2.Dot(v0);
        var d21 = v2.Dot(v1);

        var denominator = d00 * d11 - d01 * d01;
        var v = (d11 * d20 - d01 * d21) / denominator;
        var w = (d00 * d21 - d01 * d20) / denominator;
        var u = 1f - v - w;

        return new Vector3(u, v, w);
    }

    private static bool IsPointInTriangle(Vector3 point, Vector3 v1, Vector3 v2, Vector3 v3, out Vector3 pointBarycentric)
    {
        pointBarycentric = ConvertCartesianToBarycentric(point, v1, v2, v3);
        return IsInRange([pointBarycentric.X, pointBarycentric.Y, pointBarycentric.Z], 0, 1);
    }

    private static bool IsInRange(float[] values, float min, float max)
    {
        return values.All(v => v >= min && v <= max);
    }

    private static bool IsEqualsEpsilon(Vector3 v1, Vector3 v2, float epsilon)
    {
        return v1.DistanceTo(v2) < epsilon;
    }

    #endregion

    public MeshUvTool(MeshInstance3D meshInstance)
    {
        var mesh = meshInstance.Mesh as ArrayMesh;

        if (mesh == null && meshInstance.Mesh is PrimitiveMesh primitive)
        {
            mesh = new ArrayMesh();
            mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, primitive.GetMeshArrays());
        }

        if (mesh == null)
        {
            throw new ArgumentOutOfRangeException(nameof(meshInstance.Mesh), "Provided mesh doesn't support array access or is not a primitive.");
        }

        _meshTool = new MeshDataTool();
        _meshTool.CreateFromSurface(mesh, 0);
        _faceCount = _meshTool.GetFaceCount();

        for (var i = 0; i <_faceCount; i++)
        {
            _worldNormalList.Add(Vector3.Zero);
        }

        LoadMeshData(meshInstance);
    }

    public Vector2? GetUvCoordinates(Vector3 point, Vector3 normal)
    {
        var face = GetFace(point, normal);
        if (face == null)
        {
          return null;
        }

        var (index, barycentricPoint) = face.Value;
        var uv1 = _meshTool.GetVertexUV(_localFaceIndexList[index][0]);
        var uv2 = _meshTool.GetVertexUV(_localFaceIndexList[index][1]);
        var uv3 = _meshTool.GetVertexUV(_localFaceIndexList[index][2]);

        return (uv1 * barycentricPoint.X) + (uv2 * barycentricPoint.Y) + (uv3 * barycentricPoint.Z);
    }
}