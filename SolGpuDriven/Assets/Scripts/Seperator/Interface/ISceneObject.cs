using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 场景物体接口
/// </summary>
public interface ISceneObject
{
    /// <summary>
    /// 该物体的包围盒
    /// </summary>
    Bounds Bounds { get; }
    /// <summary>
    /// 该物体进入显示区域时调用
    /// </summary>
    /// <param name="parent"></param>
    /// <returns></returns>
    bool OnShow(Transform parent);

    /// <summary>
    /// 该物体离开显示区域时调用
    /// </summary>
    void OnHide();

}

public interface ISOLinkedListNode
{
    Dictionary<uint, System.Object> getNodes();

    LinkedListNode<T> GetLinkedListNode<T>(uint morton) where T : ISceneObject;

    void SetLinkedListNode<T>(uint morton, LinkedListNode<T> node);
}