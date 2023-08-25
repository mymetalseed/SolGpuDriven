using UnityEngine;

/// <summary>
/// 检查器接口,用于检测和场景物件的触发
/// </summary>
public interface IDetector
{
    /// <summary>
    /// 是否使用相机裁剪检测
    /// </summary>
    bool UseCameraCulling { get; }

    /// <summary>
    /// 包围盒
    /// </summary>
    /// <param name="bounds"></param>
    /// <returns></returns>
    bool IsDetected(Bounds bounds);

    /// <summary>
    /// 计算坐标的裁剪掩码
    /// 如果ignoreY为true,则对应四个象限
    /// 为false 对应八个象限
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="ignoreY"></param>
    /// <returns></returns>
    int GetDetectedCode(float x, float y, float z, bool ignoreY);
    
    /// <summary>
    /// 触发器位置
    /// </summary>
    Vector3 Position { get; }
}