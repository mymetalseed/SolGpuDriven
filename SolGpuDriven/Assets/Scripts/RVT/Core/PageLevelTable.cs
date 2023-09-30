using UnityEngine;

public class PageData
{
    private static readonly Vector2Int SInvalidTileIndex = new(-1, -1);

    public int ActiveFrame;

    public RenderRequest LoadRequest;

    /// <summary>
    /// 对应TiledTexture中的id
    /// </summary>
    public Vector2Int TileIndex = SInvalidTileIndex;

    public bool IsReady => TileIndex != SInvalidTileIndex;

    public void ResetTileIndex()
    {
        TileIndex = SInvalidTileIndex;
    }
}

public class PageLevelTableNode
{
    /// <summary>
    /// 占据的Rect区域
    /// </summary>
    public RectInt Rect { get; }
    /// <summary>
    /// 页数据
    /// </summary>
    public PageData Data { get; }
    /// <summary>
    /// MipMap等级
    /// </summary>
    public int MipLevel { get; }

    public PageLevelTableNode(int x, int y, int width, int height, int mip)
    {
        Rect = new RectInt(x, y, width, height);
        MipLevel = mip;
        Data = new PageData();
    }
}

/// <summary>
/// 页表
/// </summary>
public class PageLevelTable
{
    /// <summary>
    /// 当前层级的Cell总数量
    /// </summary>
    public readonly int CellCount;
    /// <summary>
    /// 每个Cell占数据的尺寸
    /// </summary>
    public readonly int PerCellSize;
    
    public PageLevelTableNode[,] Cell { get; }
    /// <summary>
    /// Mip层级
    /// </summary>
    public int MipLevel { get; }

    public PageLevelTableNode Get(int x, int y)
    {
        return Cell[x / PerCellSize % CellCount, y / PerCellSize % CellCount];
    }

    public PageLevelTable(int mipLevel, int tableSize)
    {
        MipLevel = mipLevel;
        PerCellSize = (int)Mathf.Pow(2, mipLevel);
        CellCount = tableSize / PerCellSize;
        Cell = new PageLevelTableNode[CellCount, CellCount];
        for (var i = 0; i < CellCount; ++i)
        {
            for (var j = 0; j < CellCount; ++j)
            {
                Cell[i, j] = new PageLevelTableNode(
                    i*PerCellSize,
                    j*PerCellSize,
                    PerCellSize,
                    PerCellSize,
                    MipLevel
                );
            }
        }
    }
}