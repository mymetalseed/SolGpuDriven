using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum ScaleFactor
{
    One,
    Half,
    Quarter,
    Eighth,
}

public static class ScaleModeExtensions
{
    public static float ToFloat(this ScaleFactor mode)
    {
        switch (mode)
        {
            case ScaleFactor.Eighth:
                return 0.125f;
            case ScaleFactor.Quarter:
                return 0.25f;
            case ScaleFactor.Half:
                return 0.5f;
        }

        return 1.0f;
    }
}


public static class RVTUtil
{
    /// <summary>
    /// 在Editor面板绘制Texture
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="label"></param>
    public static void DrawTexture(Texture texture, string label = null)
    {
#if UNITY_EDITOR
        if(texture == null)
        {
            return;
        }
        EditorGUILayout.Space();
        if(!string.IsNullOrEmpty(label)) EditorGUILayout.LabelField(label);
        EditorGUILayout.LabelField($"Size: {texture.width} X {texture.height}");
        EditorGUI.DrawPreviewTexture(GUILayoutUtility.GetAspectRect(texture.width / (float)texture.height),texture);
#endif
    }
    
    public static Mesh BuildFullScreenQuadMesh()
    {
        var vertexes = new List<Vector3>();
        var triangles = new List<int>();
        var uvs = new List<Vector2>();

        vertexes.Add(new Vector3(-1, 1, 0.1f));
        uvs.Add(new Vector2(0, 1));
        vertexes.Add(new Vector3(-1, -1, 0.1f));
        uvs.Add(new Vector2(0, 0));
        vertexes.Add(new Vector3(1, -1, 0.1f));
        uvs.Add(new Vector2(1, 0));
        vertexes.Add(new Vector3(1, 1, 0.1f));
        uvs.Add(new Vector2(1, 1));

        triangles.Add(0);
        triangles.Add(1);
        triangles.Add(2);

        triangles.Add(2);
        triangles.Add(3);
        triangles.Add(0);

        var quadMesh = new Mesh();
        quadMesh.SetVertices(vertexes);
        quadMesh.SetUVs(0, uvs);
        quadMesh.SetTriangles(triangles, 0);

        return quadMesh;
    }

    public static Mesh BuildQuadMesh()
    {
        var vertexes = new List<Vector3>();
        var triangles = new List<int>();
        var uvs = new List<Vector2>();

        vertexes.Add(new Vector3(0, 1, 0.1f));
        uvs.Add(new Vector2(0, 1));
        vertexes.Add(new Vector3(0, 0, 0.1f));
        uvs.Add(new Vector2(0, 0));
        vertexes.Add(new Vector3(1, 0, 0.1f));
        uvs.Add(new Vector2(1, 0));
        vertexes.Add(new Vector3(1, 1, 0.1f));
        uvs.Add(new Vector2(1, 1));

        triangles.Add(0);
        triangles.Add(1);
        triangles.Add(2);

        triangles.Add(2);
        triangles.Add(3);
        triangles.Add(0);

        var quadMesh = new Mesh();
        quadMesh.SetVertices(vertexes);
        quadMesh.SetUVs(0, uvs);
        quadMesh.SetTriangles(triangles, 0);

        return quadMesh;
    }
}


public class LruCache
{
    private NodeInfo[] allNodes;
    private NodeInfo head;
    private NodeInfo tail;

    public int First => head.id;

    public void Init(int count)
    {
        allNodes = new NodeInfo[count];
        for (var i = 0; i < count; ++i)
        {
            allNodes[i] = new NodeInfo
            {
                id = i
            };
        }

        for (int i = 0; i < count; ++i)
        {
            allNodes[i].Next = i + 1 < count ? allNodes[i + 1] : null;
            allNodes[i].Prev = i != 0 ? allNodes[i - 1] : null;
        }

        head = allNodes[0];
        tail = allNodes[count - 1];
    }

    public bool SetActive(int id)
    {
        if (id < 0 || id >= allNodes.Length)
            return false;

        var node = allNodes[id];
        if (node == tail) return true;
        //把node放到最后
        Remove(node);
        AddLast(node);
        return true;
    }

    private void AddLast(NodeInfo node)
    {
        var lastTail = tail;
        lastTail.Next = node;
        tail = node;
        node.Prev = lastTail;
    }

    private void Remove(NodeInfo node)
    {
        if (head == node)
        {
            head = node.Next;
        }
        else
        {
            node.Prev.Next = node.Next;
            node.Next.Prev = node.Prev;
        }
    }    
    public class NodeInfo
    {
        public int id;
        public NodeInfo Next { get; set; }
        public NodeInfo Prev { get; set; }
    }
}
