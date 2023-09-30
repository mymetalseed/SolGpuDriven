using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Profiling;

public class PageTable : MonoBehaviour
{
    private static readonly int VTLookupTex = Shader.PropertyToID("_VTLookupTex");
    private static readonly int VTPageParam = Shader.PropertyToID("_VTPageParam");

    // 页表尺寸
    [SerializeField] private int tableSize = 128;

    [SerializeField] private Material debugMaterial;

    private readonly Dictionary<Vector2Int, PageLevelTableNode> _activePages = new();

    [HideInInspector]
    public Texture2D _lookupTexture;
    
    private PageLevelTable[] _pageTable;

    //RT
    private RenderTask _renderTask;

    private TiledTexture _tileTexture;

    //调试贴图
    private RenderTexture DebugTexture { get; set; }

    // 页表尺寸.
    public int TableSize => tableSize;
    // 最大mipmap等级
    public int MaxMipLevel => (int)Mathf.Log(TableSize, 2);

    public void Init(RenderTask task)
    {
        _renderTask = task;
        _renderTask.StartRenderTask += OnRenderTask;

        _lookupTexture = new Texture2D(TableSize, TableSize, TextureFormat.RGBA32, false, true)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        
        //有多少mipmap,有几个pageTable
        _pageTable = new PageLevelTable[MaxMipLevel + 1];
        for (var i = 0; i <= MaxMipLevel; ++i)
        {
            _pageTable[i] = new PageLevelTable(i, TableSize);
        }
        
        Shader.SetGlobalTexture(
                VTLookupTex,
                _lookupTexture
            );
        Shader.SetGlobalVector(
            VTPageParam,
            new Vector4(
                    TableSize,
                    1.0f/TableSize,
                    MaxMipLevel,
                    0
                ));
        
        // 创建 DebugTexture
#if UNITY_EDITOR
        DebugTexture = new RenderTexture(TableSize, TableSize, 0, GraphicsFormat.R8G8B8A8_UNorm)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Point
        };
#endif

        _tileTexture = GetComponent<TiledTexture>();
        _tileTexture.OnTileUpdateComplete += InvalidatePage;
        GetComponent<FeedbackReader>().OnFeedbackReadComplete += ProcessFeedback;
    }

    private void ProcessFeedback(Texture2D texture)
    {
        //遍历回读的Feedback Texture,设置Page数据
        foreach (var color in texture.GetRawTextureData<Color32>())
        {
            ActivatePage(color.r, color.g, color.b);
        }
        //更新查找表
        UpdateLookup();
    }

    private void ActivatePage(int x, int y, int mip)
    {
        if (mip > MaxMipLevel || mip < 0 || x < 0 || y < 0 || x >= TableSize || y >= TableSize)
            return;
        
        //找到当前页表
        var page = _pageTable[mip].Get(x, y);
        if (page == null) return;
        //如果页数据还没有加载完成,加载页
        if (!page.Data.IsReady)
        {
            LoadPage(x, y, page);
            
            //向上找到最近的父节点
            while (mip < MaxMipLevel && !page.Data.IsReady)
            {
                mip++;
                page = _pageTable[mip].Get(x, y);
            }
        }
        //如果发现当前已经有对应的page了(任意mip),则继续往下
        if (!page.Data.IsReady) return;
        //激活对应的tileTexture
        _tileTexture.SetActive(page.Data.TileIndex);
        page.Data.ActiveFrame = Time.frameCount;
    }

    /// <summary>
    /// 加载页表
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="node"></param>
    private void LoadPage(int x, int y, PageLevelTableNode node)
    {
        if (node == null) return;
        if (node.Data.LoadRequest != null) return;
        
        //加载请求
        node.Data.LoadRequest = _renderTask.Request(x, y, node.MipLevel);
    }

    private void UpdateLookup()
    {
        Profiler.BeginSample("Write LUT");

        var pixels = _lookupTexture.GetRawTextureData<Color32>();
        //将页表数据写入页表中
        var curFrame = (byte)Time.frameCount;
        foreach (var kv in _activePages)
        {
            var page = kv.Value;
            
            //只写入当前帧活跃的页表
            if(page.Data.ActiveFrame != Time.frameCount)
                continue;

            var color = new Color32(
                    (byte)page.Data.TileIndex.x,
                    (byte)page.Data.TileIndex.y,
                    (byte)page.MipLevel,
                    curFrame
                );
            
            for (var x = page.Rect.x; x < page.Rect.xMax; ++x)
            {
                for (var y = page.Rect.y; y < page.Rect.yMax; ++y)
                {
                    var id = y * TableSize + x;
                    if (pixels[id].b > color.b || pixels[id].a != curFrame)
                        pixels[id] = color;
                }
            }
        }

        _lookupTexture.Apply(false);
        
        UpdateDebugTexture();
        
        Profiler.EndSample();
    }
    private void UpdateDebugTexture()
    {
#if (UNITY_EDITOR && RVT_DEBUG)
        if (debugMaterial == null)
            return;

        DebugTexture.DiscardContents();
        Graphics.Blit(_lookupTexture, DebugTexture, debugMaterial);
#endif
    }
    
    private void OnRenderTask(RenderRequest request)
    {
        var node = _pageTable[request.MipLevel].Get(request.PageX, request.PageY);
        if (node == null || node.Data.LoadRequest != request)
            return;

        node.Data.LoadRequest = null;

        var id = _tileTexture.RequestTile();
        _tileTexture.UpdateTile(id,request);

        node.Data.TileIndex = id;
        _activePages[id] = node;
    }

    private void InvalidatePage(Vector2Int id)
    {
        if (!_activePages.TryGetValue(id, out var node))
        {
            return;
        }
        
        node.Data.ResetTileIndex();
        _activePages.Remove(id);
    }

    public void Reset()
    {
        for (var i = 0; i <= MaxMipLevel; i++)
        for (var j = 0; j < _pageTable[i].CellCount; j++)
        for (var k = 0; k < _pageTable[i].CellCount; k++)
            InvalidatePage(_pageTable[i].Cell[j, k].Data.TileIndex);
        _activePages.Clear();
    }
    
}