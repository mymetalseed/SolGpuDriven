

using UnityEngine;

public delegate void TriggerHandle<T>(T trigger);

public interface ISeperateTree<T> where T : ISceneObject,ISOLinkedListNode
{
    /// <summary>
    /// 树的根节点包围盒
    /// </summary>
    Bounds bounds { get; }
    
    /// <summary>
    /// 树的最大深度
    /// </summary>
    int MaxDepth { get; }

    void Add(T item);

    void Clear();

    bool Contains(T item);

    void Remove(T item);

    void Trigger(IDetector detector, TriggerHandle<T> handle);
    
#if UNITY_EDITOR
    void DrawTree(Color treeMinDepthColor, Color treeMaxDepthColor, Color objColor, Color hitObjColor, int drawMinDepth,
        int drawMaxDepth, bool drawObj);
#endif
}

public struct TreeCullingCode
{
    public int leftbottomback;
    public int leftbottomforward;
    public int lefttopback;
    public int lefttopforward;
    public int rightbottomback;
    public int rightbottomforward;
    public int righttopback;
    public int righttopforward;

    public bool IsCulled()
    {
        return (leftbottomback & leftbottomforward & lefttopback & lefttopforward
                & rightbottomback & rightbottomforward & righttopback & righttopforward) != 0;
    }
}