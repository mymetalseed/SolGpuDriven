using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class StreamingMeshLoader
{
    public class LoadRequest
    {
        public string path;
        public string modelName;
        public Action action;
    }
    public static void SendRequest(LoadRequest request)
    {
        Debug.Log($"StreamingMeshLoader.Load {Path.GetFileName(request.path)}");
        //加载完了以后
        
    }

    private static IEnumerator Load(LoadRequest request)
    {
        AssetBundleCreateRequest bundleLoadRequest = AssetBundle.LoadFromFileAsync(request.path);
        yield return bundleLoadRequest;
        var myLoadedAssetBundle = bundleLoadRequest.assetBundle;
        if (myLoadedAssetBundle == null)
        {
            Debug.LogError($"Failed to load AssetBundle.");
            yield break;
        }

        AssetBundleRequest assetLoadRequest = myLoadedAssetBundle.LoadAssetAsync<GameObject>(request.modelName);
        yield return assetLoadRequest;
        
        //GameObject prefab = assetLoadRequest.asset as GameObject;
        request.action?.Invoke();
    }
}
