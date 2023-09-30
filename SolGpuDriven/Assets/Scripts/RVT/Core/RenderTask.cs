using System;
using System.Collections.Generic;

public class RenderRequest
{
    /// <summary>
    /// 页表X坐标
    /// </summary>
    public int PageX { get; }

    /// <summary>
    /// 页表Y 坐标
    /// </summary>
    public int PageY { get; }

    /// <summary>
    /// mipmap等级
    /// </summary>
    public int MipLevel { get; }
    
    public RenderRequest(int x, int y, int mip)
    {
        PageX = x;
        PageY = y;
        MipLevel = mip;
    }
}

public class RenderTask
{
    /// <summary>
    /// 每帧处理数量限制
    /// </summary>
    private readonly int _limit = 10;

    /// <summary>
    /// 等待处理的请求
    /// </summary>
    private readonly List<RenderRequest> _pendingRequests = new();

    /// <summary>
    /// 开始渲染的事件
    /// </summary>
    public event Action<RenderRequest> StartRenderTask;

    public void Update()
    {
        if (_pendingRequests.Count <= 0)
            return;
        
        _pendingRequests.Sort((lhs, rhs) =>
            -lhs.MipLevel.CompareTo(rhs.MipLevel)
        );

        var count = _limit;
        while (count > 0 && _pendingRequests.Count > 0)
        {
            count--;
            var request = _pendingRequests[0];
            _pendingRequests.RemoveAt(0);
            
            StartRenderTask?.Invoke(request);
        }
    }

    /// <summary>
    /// 渲染请求,新建一个家在请求放到请求队列中
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="mip"></param>
    /// <returns></returns>
    public RenderRequest Request(int x, int y, int mip)
    {
        foreach (var r in _pendingRequests)
        {
            if (r.PageX == x && r.PageY == y && r.MipLevel == mip)
                return null;
        }

        var request = new RenderRequest(x, y, mip);
        _pendingRequests.Add(request);

        return request;
    }
}