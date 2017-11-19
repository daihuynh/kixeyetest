using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainThreadInvoker : MonoBehaviour
{
    public int maxProcessPerTick = 100;
    private List<Action> _actionQueue = new List<Action>();
    private object _lockObj = new object();

    public void Add(Action action)
    {
        lock (_lockObj)
        {
            _actionQueue.Add(action);
        }
    }

    void Update()
    {
        if (_actionQueue.Count == 0) return;

        List<Action> processActions = null;
        lock (_lockObj)
        {
            int maxGet = Math.Min(maxProcessPerTick, _actionQueue.Count);
            processActions = _actionQueue.GetRange(0, maxGet);
            _actionQueue.RemoveRange(0, maxGet);
        }

        if (processActions != null)
        {
            int count = processActions.Count;
            for (int counter = 0; counter < count; counter++)
            {
                processActions[counter]();
            }
        }
    }
}
