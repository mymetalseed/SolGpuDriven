using UnityEngine;

public class TiledTexture : MonoBehaviour
{
    // 单个Tile的尺寸.
    [SerializeField] private int tileSize = 256;

    // 填充
    [SerializeField] private int boundSize = 4;
    // 单个Tile的尺寸
    public int TileSize => tileSize;
}