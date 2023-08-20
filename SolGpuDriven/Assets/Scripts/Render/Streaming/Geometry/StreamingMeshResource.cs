using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class StreamingMeshResource : ScriptableObject
{
    public string path;
    public string modelName;
    public Mesh streamingMesh;
    public StreamingMeshResource()
    {
        Debug.Log("create meshResource");
    }

    public void Load()
    {
        if (streamingMesh)
        {
            return;
        }
        StreamingMeshLoader.SendRequest(new StreamingMeshLoader.LoadRequest()
        {
            path = path,
            action = (mesh) =>
            {
                streamingMesh = mesh as Mesh;
                Debug.Log("Load Finish.");
            },
            modelName = modelName
        });
    }

    public void Release()
    {
        if (streamingMesh)
        {
            Addressables.Release(streamingMesh);
        }
    }
}
