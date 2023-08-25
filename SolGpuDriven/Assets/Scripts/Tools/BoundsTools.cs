using UnityEngine;

public static class BoundsTools
{
    /// <summary>
    /// 绘制包围盒
    /// </summary>
    /// <param name="bounds"></param>
    /// <param name="color"></param>
    public static void DrawBounds(this Bounds bounds, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawWireCube(bounds.center,bounds.size);
    }
}