using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
public class MeshDeformer2 : MonoBehaviour
{
    public bool recalculateNormals;

    public bool collisionDetection;

    Mesh mesh;
    MeshCollider meshCollider;
    List<Vector3> vertices;

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        meshCollider = GetComponent<MeshCollider>();
        vertices = mesh.vertices.ToList();

    }

    Vector3 vi; Vector3 deformation;
    public void Deform(Vector3 point, float radius, float stepRadius, float strength, float stepStrength, Vector3 direction)
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            vi = transform.TransformPoint(vertices[i]);
            float distance = Vector3.Distance(point, vi);
            float s = strength;
            for (float r = 0.0f; r < radius; r += stepRadius)
            {
                if (distance < r)
                {
                    deformation = direction * s;
                    vertices[i] = transform.InverseTransformPoint(vi + deformation);
                    break;
                }
                s -= stepStrength;
            }
        }
        if (recalculateNormals)
            mesh.RecalculateNormals();

        if (collisionDetection)
        {
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;
        }
        mesh.SetVertices(vertices);
    }
}