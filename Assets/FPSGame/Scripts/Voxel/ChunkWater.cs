using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
public class ChunkWater : MonoBehaviour {

    public Chunk parentChunk;

    public Mesh mesh;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    private void Awake()
    {
        parentChunk = GetComponentInParent<Chunk>();

        mesh = GetComponent<MeshFilter>().mesh;
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        gameObject.layer = LayerMask.NameToLayer(parentChunk.blockLayerName);
    }

    private Vector3[] initialVertices;

    private void FixedUpdate()
    {
        if (QualitySettings.names[QualitySettings.GetQualityLevel()] == "Fantastic")
        {
            Vector3[] verts = mesh.vertices;
            for (int i = 0; i < verts.Length; i++)
            {
                Vector3 initialVert = initialVertices[i];
                float y = 
                    initialVertices[i].y
                    +
                    (   
                         Mathf.Sin(initialVert.x+transform.position.x+Time.time*2F)
                        -Mathf.Sin(initialVert.z+transform.position.z+Time.time*2F)
                    )
                    * 0.1F
                    - 0.15F;
                verts[i].y = y;
            }
            mesh.vertices = verts;
        }
        //mesh.RecalculateNormals();
    }

    public void RenderWaterMesh(MeshData meshData)
    {
        // Clear the mesh first
        mesh.Clear();

        // Mesh
        mesh.vertices = meshData.vertexList.ToArray();
        mesh.triangles = meshData.triangleList.ToArray();
        mesh.uv = meshData.UVList.ToArray();
        mesh.RecalculateNormals();

        initialVertices = mesh.vertices;
    }
}
