﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class LinearSceneTree<T> : ISeperateTree<T> where T : ISceneObject,ISOLinkedListNode
{
    public Bounds bounds
    {
        get
        {
            return m_Bounds;
        }
    }

    public int MaxDepth
    {
        get
        {
            return m_MaxDepth;
        }
    }

    protected Bounds m_Bounds;
    protected int m_MaxDepth;
    //Morton码索引节点(为啥是字典?这不是为线性而生的么)
    protected Dictionary<uint, LinearSceneTreeLeaf<T>> m_Nodes;

    protected int m_Cols;

    public LinearSceneTree(Vector3 center, Vector3 size, int maxDepth)
    {
        this.m_MaxDepth = maxDepth;
        m_Bounds = new Bounds(center, size);

        m_Cols = (int)Mathf.Pow(2, maxDepth);
        m_Nodes = new Dictionary<uint, LinearSceneTreeLeaf<T>>();
    }

    public abstract void Add(T item);

    public void Clear()
    {
        m_Nodes.Clear();
    }

    public bool Contains(T item)
    {
        if (m_Nodes == null)
        {
            return false;
        }

        foreach (var node in m_Nodes)
        {
            if (node.Value != null && node.Value.Contains(item))
            {
                return true;
            }
        }

        return false;
    }

    public void Remove(T item)
    {
        if (item == null)
            return;
        if (m_Nodes == null)
            return;
        var nodes = item.getNodes();
        if (nodes == null)
            return;
        foreach (var node in nodes)
        {
            if (m_Nodes.ContainsKey(node.Key))
            {
                var n = m_Nodes[node.Key];
                if (n != null && n.Datas != null)
                {
                    var value = (LinkedListNode<T>)node.Value;
                    if (value.List == n.Datas)
                        n.Datas.Remove(value);
                }
            }
        }
        nodes.Clear();
    }

    public abstract void Trigger(IDetector detector, TriggerHandle<T> handle);
#if UNITY_EDITOR
    public abstract void DrawTree(Color treeMinDepthColor, Color treeMaxDepthColor, Color objColor, Color hitObjColor,
        int drawMinDepth,
        int drawMaxDepth, bool drawObj);
#endif
}


public class LinearSceneTreeLeaf<T> where T : ISceneObject, ISOLinkedListNode
{
    private LinkedList<T> m_DataList;
    public LinkedList<T> Datas
    {
        get
        {
            return m_DataList;
        }
    }

    public LinearSceneTreeLeaf()
    {
        m_DataList = new LinkedList<T>();
    }

    public LinkedListNode<T> Insert(T obj)
    {
        return m_DataList.AddFirst(obj);
    }

    public void Trigger(IDetector detector, TriggerHandle<T> handle)
    {
        if (handle != null)
        {
            LinkedListNode<T> node = m_DataList.First;

            while (node != null)
            {
                if (detector.IsDetected(node.Value.Bounds))
                    handle(node.Value);

                node = node.Next;
            }
        }
    }

    public bool Contains(T item)
    {
        if (m_DataList != null && m_DataList.Contains(item))
            return true;
        return false;
    }
    
    #if UNITY_EDITOR

    public bool DrawNode(Color objColor, Color hitObjColor, bool drawObj)
    {
        if (drawObj && m_DataList.Count > 0)
        {
            LinkedListNode<T> node = m_DataList.First;

            while (node != null)
            {
                var sceneobj = node.Value as SceneObject;
                if(sceneobj != null)
                    sceneobj.DrawArea(objColor,hitObjColor);

                node = node.Next;
            }

            return true;
        }

        return false;
    }
    #endif
    
}
