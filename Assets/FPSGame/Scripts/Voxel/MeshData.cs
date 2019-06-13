using UnityEngine;
using System.Collections.Generic;

public class MeshData {

    public List<Vector3> vertexList = new List<Vector3>();
    public List<int> triangleList = new List<int>();
    public List<Vector2> UVList = new List<Vector2>();

    public void AddMeshVertex(Vector3 vertex)
    {
        vertexList.Add(vertex);
    }

    public void AddMeshTriangles()
    {
        triangleList.Add(vertexList.Count - 4);
        triangleList.Add(vertexList.Count - 3);
        triangleList.Add(vertexList.Count - 2);

        triangleList.Add(vertexList.Count - 4);
        triangleList.Add(vertexList.Count - 2);
        triangleList.Add(vertexList.Count - 1);
    }
}
