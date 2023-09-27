using UnityEngine;

public class PageTable : MonoBehaviour
{
    // 页表尺寸
    [SerializeField] private int tableSize = 128;
    // 页表尺寸.
    public int TableSize => tableSize;
    // 最大mipmap等级
    public int MaxMipLevel => (int)Mathf.Log(TableSize, 2);
}