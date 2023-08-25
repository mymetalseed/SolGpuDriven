using UnityEngine;

/// <summary>
/// 线性四叉树
/// 节点字典存放叶子节点Morton作为Key
/// </summary>
/// <typeparam name="T"></typeparam>
public class LinearSceneQuadTree<T> : LinearSceneTree<T> where T : ISceneObject,ISOLinkedListNode
{
    //每个格子有多宽
    private float m_DeltaWidth;
    //每个个子有多高
    private float m_DeltaHeight;

    public LinearSceneQuadTree(Vector3 center, Vector3 size, int maxDepth) : base(center,size,maxDepth)
    {
        //划分为m_Cols*m_Cols大小的格子,计算每个各自的长和宽
        m_DeltaHeight = m_Bounds.size.z / m_Cols;
        m_DeltaWidth = m_Bounds.size.x / m_Cols;
    }

    public override void Add(T item)
    {
        if (item == null)
            return;
        if (m_Bounds.Intersects(item.Bounds))
        {
            if (m_MaxDepth == 0)
            {
                if (m_Nodes.ContainsKey(0) == false)
                    m_Nodes[0] = new LinearSceneTreeLeaf<T>();
                var node = m_Nodes[0].Insert(item);
                item.SetLinkedListNode<T>(0,node);
            }
            else
            {
                InsertToNode(item, 0, m_Bounds.center.x, m_Bounds.center.z, m_Bounds.size.x, m_Bounds.size.z);
            }
        }
    }

    private bool InsertToNode(T obj, int depth, float centerx, float centerz, float sizex, float sizez)
    {
        if (depth == m_MaxDepth)
        {
            uint m = Morton2FromWorldPos(centerx, centerz);
            if (m_Nodes.ContainsKey(m) == false)
            {
                m_Nodes[m] = new LinearSceneTreeLeaf<T>();
            }

            var node = m_Nodes[m].Insert(obj);
            obj.SetLinkedListNode<T>(m,node);
            return true;
        }
        else
        {
            int collider = 0;
            float minx = obj.Bounds.min.x;
            float minz = obj.Bounds.min.z;
            float maxx = obj.Bounds.max.x;
            float maxz = obj.Bounds.max.z;

            if (minx <= centerx && minz <= centerz)
                collider |= 1;
            if (minx <= centerx && maxz >= centerz)
                collider |= 2;
            if (maxx >= centerx && minz <= centerz)
                collider |= 4;
            if (maxx >= centerx && maxz >= centerz)
                collider |= 8;

            float sx = sizex * 0.5f, sz = sizez * 0.5f;

            bool insertresult = false;
            if ((collider & 1) != 0)
                insertresult = insertresult |
                               InsertToNode(obj, depth + 1, centerx - sx * 0.5f, centerz - sz * 0.5f, sx, sz);
            if ((collider & 2) != 0)
                insertresult = insertresult | InsertToNode(obj, depth + 1, centerx - sx * 0.5f, centerz + sz * 0.5f, sx, sz);
            if ((collider & 4) != 0)
                insertresult = insertresult | InsertToNode(obj, depth + 1, centerx + sx * 0.5f, centerz - sz * 0.5f, sx, sz);
            if ((collider & 8) != 0)
                insertresult = insertresult | InsertToNode(obj, depth + 1, centerx + sx * 0.5f, centerz + sz * 0.5f, sx, sz);
            return insertresult;
        }
    }

    private void TriggerToNodeByCamera(IDetector detector, TriggerHandle<T> handle, int depth,
        TreeCullingCode cullingCode, float centerx, float centerz, float sizex, float sizez)
    {
        if (cullingCode.IsCulled())
            return;
        //todo: 待实现
    }

    public override void Trigger(IDetector detector, TriggerHandle<T> handle)
    {
    }

    private void TriggerToNode(IDetector detector, TriggerHandle<T> handle, int depth, float centerx, float centerz,
        float sizex,
        float sizez)
    {
    }

    private uint Morton2FromWorldPos(float x,float z)
    {
        uint px = (uint)Mathf.FloorToInt((x - m_Bounds.min.x) / m_DeltaWidth);
        uint pz = (uint)Mathf.FloorToInt((z - m_Bounds.min.z) / m_DeltaHeight);
        return Morton2(px, pz);
    }
    
    private uint Morton2(uint x, uint y)
    {
        return (Part1By1(y) << 1) + Part1By1(x);
    }
    
    /// <summary>
    /// 计算MortonCode
    /// </summary>
    /// <param name="n"></param>
    /// <returns></returns>
    private uint Part1By1(uint n)
    {
        n = (n ^ (n << 8)) & 0x00ff00ff;
        n = (n ^ (n << 4)) & 0x0f0f0f0f;
        n = (n ^ (n << 2)) & 0x33333333;
        n = (n ^ (n << 1)) & 0x55555555;
        return n;
    }

    public static implicit operator bool(LinearSceneQuadTree<T> tree)
    {
        return tree != null;
    }
    
#if UNITY_EDITOR

    public override void DrawTree(Color treeMinDepthColor, Color treeMaxDepthColor, Color objColor, Color hitObjColor, int drawMinDepth,
        int drawMaxDepth, bool drawObj)
    {
        
    }

    private bool DrawNodeGizmos(Color treeMinDepthColor, Color treeMaxDepthColor, Color objColor, Color hitObjColor, int drawMinDepth, int drawMaxDepth, bool drawObj, int depth, Vector2 center, Vector2 size)
    {
        if (depth < drawMinDepth || depth > drawMaxDepth)
        {
            return false;
        }

        float d = ((float)depth) / m_MaxDepth;
        Color color = Color.Lerp(treeMinDepthColor, treeMaxDepthColor, d);
        if (depth == m_MaxDepth)
        {
            uint m = Morton2FromWorldPos(center.x, center.y);
            if (m_Nodes.ContainsKey(m) && m_Nodes[m] != null)
            {
                if (m_Nodes[m].DrawNode(objColor, hitObjColor, drawObj))
                {
                    Bounds b = new Bounds(new Vector3(center.x, m_Bounds.center.y, center.y),
                        new Vector3(size.x, m_Bounds.size.y, size.y));
                    b.DrawBounds(color);
                    return true;
                }
            }
        }
        else
        {
            //有叶子节点的时候,绘制根节点Bounds
            bool draw = false;
            float sx = size.x * 0.5f, sz = size.y * 0.5f;
            draw = draw | DrawNodeGizmos(treeMinDepthColor, treeMaxDepthColor, objColor, hitObjColor, drawMinDepth, drawMaxDepth, drawObj, depth + 1, new Vector2(center.x - sx * 0.5f, center.y - sz * 0.5f), new Vector2(sx, sz));
            draw = draw | DrawNodeGizmos(treeMinDepthColor, treeMaxDepthColor, objColor, hitObjColor, drawMinDepth, drawMaxDepth, drawObj, depth + 1, new Vector2(center.x + sx * 0.5f, center.y - sz * 0.5f), new Vector2(sx, sz));
            draw = draw | DrawNodeGizmos(treeMinDepthColor, treeMaxDepthColor, objColor, hitObjColor, drawMinDepth, drawMaxDepth, drawObj, depth + 1, new Vector2(center.x - sx * 0.5f, center.y + sz * 0.5f), new Vector2(sx, sz));
            draw = draw | DrawNodeGizmos(treeMinDepthColor, treeMaxDepthColor, objColor, hitObjColor, drawMinDepth, drawMaxDepth, drawObj, depth + 1, new Vector2(center.x + sx * 0.5f, center.y + sz * 0.5f), new Vector2(sx, sz));

            if (draw)
            {
                Bounds b = new Bounds(new Vector3(center.x, m_Bounds.center.y, center.y),
                    new Vector3(size.x, m_Bounds.size.y, size.y));
                b.DrawBounds(color);
            }

            return draw;
        }
        return false;
    }

#endif
}