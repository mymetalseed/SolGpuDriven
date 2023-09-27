using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

public class FeedbackReader :MonoBehaviour
{
    /// <summary>
    /// 调试材质,用于编辑器显示贴图mipmap等级
    /// </summary>
    [SerializeField]
    private Material debugMaterial;

    /// <summary>
    /// 缩放材质
    /// </summary>
    [SerializeField]
    private Material downScaleMaterial;

    /// <summary>
    /// 回读目标缩放比例
    /// </summary>
    private readonly ScaleFactor readbackScale = ScaleFactor.Half;

    /// <summary>
    /// 缩小后的RT
    /// </summary>
    private RenderTexture _downScaleTexture;

    /// <summary>
    /// 处理中的回读请求
    /// </summary>
    private AsyncGPUReadbackRequest _readbackRequest;

    /// <summary>
    /// 回读到CPU的Texture
    /// </summary>
    private Texture2D _readbackTexture;

    /// <summary>
    /// 缩放材质使用的Pass
    /// </summary>
    private int downScaleMaterialPass;
    
    /// <summary>
    /// 调试用的RenderTexture(用于显示mipmap等级)
    /// </summary>
    public RenderTexture DebugTexture { get; private set; }

    public bool CanRead => _readbackRequest.done || _readbackRequest.hasError;

    private void Start()
    {
        if (readbackScale != ScaleFactor.One)
        {
            switch (readbackScale)
            {
                case ScaleFactor.Half:
                    downScaleMaterialPass = 0;
                    break;
                case ScaleFactor.Quarter:
                    downScaleMaterialPass = 1;
                    break;
                case ScaleFactor.Eighth:
                    downScaleMaterialPass = 2;
                    break;
            }
        }
    }

    /// <summary>
    /// 回读完成事件
    /// </summary>
    public event Action<Texture2D> OnFeedbackReadComplete;
    
    /// <summary>
    /// 发起回读请求
    /// </summary>
    /// <param name="texture"></param>
    public void ReadbackRequest(RenderTexture texture)
    {
        if (_readbackRequest is { done: false, hasError: false })
            return;
        
        //缩放后的尺寸
        var width = (int)(texture.width * readbackScale.ToFloat());
        var height = (int)(texture.height * readbackScale.ToFloat());
        
        //缩放
        if (_downScaleTexture == null || _downScaleTexture.width != width || _downScaleTexture.height != height)
        {
            _downScaleTexture = new RenderTexture(width, height, 0, GraphicsFormat.R8G8B8A8_UNorm);
        }
        
        Graphics.Blit(texture,_downScaleTexture,downScaleMaterial,downScaleMaterialPass);
        texture = _downScaleTexture;

        if (_readbackTexture == null || _readbackTexture.width != width || _readbackTexture.height != height)
        {
            _readbackTexture = new Texture2D(width, height, TextureFormat.RGBA32, false, true)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            
            #if UNITY_EDITOR
            DebugTexture = new RenderTexture(width, height,0, GraphicsFormat.R8G8B8A8_UNorm)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            #endif
        }
        
        //异步回读请求
        _readbackRequest = AsyncGPUReadback.Request(texture);
    }

    public void UpdateRequest()
    {
        if (_readbackRequest is not { done: true, hasError: false }) return;
        
        Profiler.BeginSample("ReadbackAndProcess");

        var colors = _readbackRequest.GetData<Color32>();
        _readbackTexture.GetRawTextureData<Color32>().CopyFrom(colors);
        
        //把在CPU端的更改同步到GPU端
        _readbackTexture.Apply(false);

        OnFeedbackReadComplete?.Invoke(_readbackTexture);
        UpdateDebugTexture();
        
        Profiler.EndSample();
    }
    
    private void UpdateDebugTexture()
    {
#if (UNITY_EDITOR && RVT_DEBUG)
        if (_readbackTexture == null || debugMaterial == null)
            return;

        Graphics.Blit(_readbackTexture, DebugTexture, debugMaterial);
#endif
    }
}