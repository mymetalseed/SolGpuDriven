
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;

public static class CoroutineManager
{
    /// <summary>
    /// 协程任务,管理方式为限制每帧执行的时间
    /// </summary>
    public class Task
    {
        public IEnumerator routine;
        public string name;
        public int priority;

        public void Start()
        {
            CoroutineManager.Enqueue(this);
        }
        public void Stop()
        {
            CoroutineManager.Stop(this);
        }
    }

    private static List<Task> queue = new List<Task>();
    private static Task current = null;
    public static float timePerFrame = 1;
    private static System.Diagnostics.Stopwatch timer = new Stopwatch();
    
    static CoroutineManager()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.playModeStateChanged -= AbortOnPlaymodeChange;
        Application.wantsToQuit -= QuitOnExit;
        UnityEditor.EditorApplication.playModeStateChanged += AbortOnPlaymodeChange;
        Application.wantsToQuit += QuitOnExit;
#endif
    }

    public static Task Enqueue(IEnumerator routine, int priority = 0, string name = null)
    {
        Task task = new Task()
        {
            routine = routine,
            name = name,
            priority = priority
        };
        Enqueue(task);
        return task;
    }

    public static void Enqueue(Task task)
    {
        queue.Add(task);
    }

    public static void Dequeue(Task task)
    {
        if (queue.Contains(task))
        {
            queue.Remove(task);
        }
    }

    public static void Stop(Task task)
    {
        if (queue.Contains(task))
        {
            queue.Remove(task);
        }

        if (current == task)
        {
            current = null;
        }
    }

    public static void Update()
    {
        timer.Reset();

        while (timer.ElapsedMilliseconds < timePerFrame || !timer.IsRunning)
        {
            if(!timer.IsRunning) timer.Start();
            if (current == null)
            {
                if (queue.Count == 0) break;
                int taskIndex = GetMaxPriorityNum(queue);
                if (taskIndex < queue.Count && taskIndex >= 0)
                {
                    current = queue[taskIndex];
                    queue.RemoveAt(taskIndex);
                }
            }

            if (current!=null)
            {
                bool move = false;
                //有需要执行的协程,执行一次,耗费的时间都在这个MoveNext里
                //如果协程还有下一步,则move不为true
                if (current.routine != null)
                {
                    move = current.routine.MoveNext();
                }
                //没有需要执行的协程,current直接为null
                if (!move)
                {
                    current = null;
                }
            }
        }
        timer.Stop();
    }

    public static int GetMaxPriorityNum(List<Task> list)
    {
        int maxPriority = int.MinValue;
        int maxPriorityNum = -1;
        for (int i = 0; i < list.Count; ++i)
        {
            int priority = list[i].priority;
            if (priority > maxPriority)
            {
                maxPriority = priority;
                maxPriorityNum = i;
            }
        }

        return maxPriorityNum;
    }

    public static void Quit()
    {
        queue.Clear();
        if (current != null)
        {
            try
            {
                if (current.routine != null)
                {
                    current.routine.Reset();
                }
            }catch(Exception){}

            current.routine = null;
        }
    }
    
#if UNITY_EDITOR
    static void AbortOnPlaymodeChange(UnityEditor.PlayModeStateChange state)
    {
        if(state == UnityEditor.PlayModeStateChange.ExitingEditMode || state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
        {
            Quit();
        }
    }
#endif
    
    static bool QuitOnExit()
    {
        Quit();
        return true;
    }
}
