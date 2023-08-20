using System;
using System.Collections.Generic;
using System.Threading;
public static class ThreadManager
{
    public class Task
    {
        public Thread thread;
        public Action action;
        public string name = null;
        public int priority;
        
        public bool Enqueued { get { return waitingQueue.Contains(this); } }
        public bool Working { get { return workingQueue.Contains(this); } }
        public bool IsActive { get { return workingQueue.Contains(this) || waitingQueue.Contains(this); } }
    }

    public static bool useMultithreading = true;
    public static int maxThreads = 3;
    private static List<Task> workingQueue = new List<Task>();
    private static List<Task> waitingQueue = new List<Task>();

    public static Task Enqueue(Action action, int priority = 0, string name = null)
    {
        Task task = new Task() { action = action, priority = priority, name = name };
        Enqueue(task);
        return task;
    }

    public static void Enqueue(Task task)
    {
        lock (workingQueue)
        {
            if (workingQueue.Contains(task)) return;
        }

        lock (waitingQueue)
        {
            if (waitingQueue.Contains(task)) return;
            else
            {
                waitingQueue.Add(task);
            }
        }

        LaunchThreads();
    }

    public static void Dequeue(Task task)
    {
        lock (waitingQueue)
        {
            if (waitingQueue.Contains(task))
            {
                waitingQueue.Remove(task);
                return;
            }
        }
    }

    public static void LaunchThreads()
    {
        lock (waitingQueue)
        {
            while (true)
            {
                if (workingQueue.Count >= maxThreads)
                {
                    break;
                }

                Task task;
                lock (waitingQueue)
                {
                    if (workingQueue.Count == 0) break;

                    int jobNum = GetMaxPriorityNum(waitingQueue);
                    task = workingQueue[jobNum];
                    waitingQueue.RemoveAt(jobNum);
                }
                workingQueue.Add(task);

                Thread thread = new Thread(task.TaskThreadAction);
                lock (task)
                    task.thread = thread;
                thread.Start();
            }
        }        
    }

    public static void TaskThreadAction(this Task task)
    {
        try
        {
            task.action();
        }
        catch (ThreadAbortException)
        {
        }
        finally
        {
            lock (workingQueue)
            {
                if (workingQueue.Contains(task))
                    workingQueue.Remove(task);
            }
            LaunchThreads();
        }
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
}