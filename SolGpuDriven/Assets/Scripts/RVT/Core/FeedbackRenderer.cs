using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class FeedbackRenderer : MonoBehaviour
{
    private static readonly int VTFeedbackParam = Shader.PropertyToID("_VTFeedbackParam");
    public Camera FeedbackCamera { get; private set; }
    // Feedback RT 缩放
    private readonly float _scaleFactor = 1.0f / 32.0f;

    private int mipmapBias = 0;
    
    // Feedback RT
    public RenderTexture TargetTexture { get; private set; }

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        FollowMainCamera();
    }
    
    private void Init()
    {
        var mainCamera = Camera.main;
        if (mainCamera == null) return;

        FeedbackCamera = GetComponent<Camera>();
        if (FeedbackCamera == null) FeedbackCamera = gameObject.AddComponent<Camera>();
        FeedbackCamera.enabled = false;
        
        var width = (int)(mainCamera.pixelWidth * _scaleFactor);
        var height = (int)(mainCamera.pixelHeight * _scaleFactor);
        if (TargetTexture == null || TargetTexture.width != width || TargetTexture.height != height)
        {
            TargetTexture = new RenderTexture(width, height, 0, GraphicsFormat.R8G8B8A8_UNorm)
            {
                useMipMap = false,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point
            };
            FeedbackCamera.targetTexture = TargetTexture;
        }
        
        //设置全局变量
        // x:pageTable size
        // y : Virtual Texture Size
        // z : Max MipMap Level
        var tileTexture = GetComponent<TiledTexture>();
        var virtualTable = GetComponent<PageTable>();
        Shader.SetGlobalVector(
            VTFeedbackParam,
            new Vector4(
                virtualTable.TableSize, //页表大小
                virtualTable.TableSize * tileTexture.TileSize * _scaleFactor,   //virtual Texture Size (实际的分块是和页表数量一致的)
                virtualTable.MaxMipLevel - 1,//最大的MipmapLevel
                mipmapBias));//mipmap偏移(固定)
    }
    
    private void FollowMainCamera()
    {
        var mainCamera = Camera.main;
        var fbTransform = FeedbackCamera.transform;
        var mcTransform = mainCamera.transform;
        fbTransform.position = mcTransform.position;
        fbTransform.rotation = mcTransform.rotation;
        FeedbackCamera.projectionMatrix = mainCamera.projectionMatrix;
    }
}