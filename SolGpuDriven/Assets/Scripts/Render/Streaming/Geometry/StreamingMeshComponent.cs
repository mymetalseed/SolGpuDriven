using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class StreamingMeshComponent : MonoBehaviour
{
    [SerializeField]
    private StreamingMeshResource meshResource;
    
    /// <summary>
    /// Debug用的MeshFilter
    /// </summary>
    [SerializeField]
    private Material debugMaterial;
    
    public StreamingMeshResource MeshResource
    {
        get => meshResource;
        set => meshResource = value;
    }
    
    [ContextMenu("test_LoadMesh")]
    void Test()
    {
        StreamingMeshResource mesh = ScriptableObject.CreateInstance<StreamingMeshResource>();
        string path = "Assets/Asset/Mesh/TimeCenter/StreamingModel/U_Char_1_3.mesh";
        mesh.path = path;
        mesh.modelName = "齿轮开始拟合";
        mesh.Load();
        meshResource = mesh;
    }

    private void Update()
    {
        if (meshResource && meshResource.streamingMesh)
        {
            Matrix4x4 matrix = new Matrix4x4();
            matrix.SetTRS(transform.position,quaternion.identity, Vector3.one);
            Graphics.DrawMesh(meshResource.streamingMesh,transform.position,Quaternion.identity,debugMaterial,1);
        }
    }
    
    [ContextMenu("test_ReleaseMesh")]
    private void Release()
    {
        meshResource?.Release();
    }
}
