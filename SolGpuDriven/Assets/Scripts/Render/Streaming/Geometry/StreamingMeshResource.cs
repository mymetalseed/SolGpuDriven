using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StreamingMeshResource : ScriptableObject
{
    public string path;
    public string modelName;
    public StreamingMeshResource()
    {
        Debug.Log("create meshResource");
    }

    public void Load()
    {
        StreamingMeshLoader.SendRequest(new StreamingMeshLoader.LoadRequest()
        {
            path = path,
            action = () =>
            {
                Debug.Log("Load Finish.");
            },
            modelName = modelName
        });
    }
}
