using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class StreamingMeshComponent : MonoBehaviour
{
    [SerializeField ]
    private StreamingMeshResource meshResource;

    public StreamingMeshResource MeshResource
    {
        get => meshResource;
        set => meshResource = value;
    }


    [ContextMenu("test")]
    void Test()
    {
        StreamingMeshResource mesh = ScriptableObject.CreateInstance<StreamingMeshResource>();
        string path = Path.Combine(Application.streamingAssetsPath, "model10");
        mesh.path = path;
        mesh.modelName = "Cube";
        mesh.Load();
        meshResource = mesh;
    }
}
