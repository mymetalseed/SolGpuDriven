using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

public static class StreamingMeshLoader
{
    public class LoadRequest
    {
        public string path;
        public string modelName;
        public Action<object> action;
    }
    public static CoroutineManager.Task SendRequest(LoadRequest request)
    {
        Debug.Log($"StreamingMeshLoader.Load {Path.GetFileName(request.path)}");
        //加载完了以后
        CoroutineManager.Task task = new CoroutineManager.Task()
        {
            routine = Load(request),
            name = "streamingMeshLoader",
            priority = 1
        };
        task.Start();
        return task;
    }

    private static IEnumerator Load(LoadRequest request)
    {
        AsyncOperationHandle handle = Addressables.LoadAssetAsync<Mesh>(request.path);

        while (!handle.IsDone)
        {
            yield return null;
        }

        if (handle.Result == null)
        {
            yield break;
        }
        
        /*AssetBundleCreateRequest bundleLoadRequest = AssetBundle.LoadFromFileAsync(request.path);
        yield return bundleLoadRequest;
        var myLoadedAssetBundle = bundleLoadRequest.assetBundle;
        if (myLoadedAssetBundle == null)
        {
            Debug.LogError($"Failed to load AssetBundle.");
            yield break;
        }

        AssetBundleRequest assetLoadRequest = myLoadedAssetBundle.LoadAssetAsync<GameObject>(request.modelName);
        yield return assetLoadRequest;
        
        //GameObject prefab = assetLoadRequest.asset as GameObject;*/
        request.action?.Invoke(handle.Result);
    }
}
