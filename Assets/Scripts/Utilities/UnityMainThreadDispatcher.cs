using System;
using System.Collections.Generic;
using UnityEngine;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> _executionQueue = new Queue<Action>();
    private static UnityMainThreadDispatcher _instance;

    public static UnityMainThreadDispatcher Instance()
    {
        if (_instance == null)
        {
            _instance = FindFirstObjectByType<UnityMainThreadDispatcher>();
            if (_instance == null)
            {
                var obj = new GameObject("UnityMainThreadDispatcher");
                _instance = obj.AddComponent<UnityMainThreadDispatcher>();
                DontDestroyOnLoad(obj);
            }
        }
        return _instance;
    }

    // void Awake()
    // {
    //     Debug.Log("UnityMainThreadDispatcher Awake");
    // }

    // void Start()
    // {
    //     Debug.Log("UnityMainThreadDispatcher Start");
    // }

    public void Enqueue(Action action)
    {
        // Debug.Log("UnityMainThreadDispatcher.Enqueue called");
        lock (_executionQueue)
        {
            _executionQueue.Enqueue(action);
            // Debug.Log($"Queue size after enqueue: {_executionQueue.Count}");
        }
    }

    void Update()
    {
        lock (_executionQueue)
        {
            while (_executionQueue.Count > 0)
            {
                // Debug.Log("UnityMainThreadDispatcher executing action from queue");
                _executionQueue.Dequeue().Invoke();
            }
        }
    }
}
