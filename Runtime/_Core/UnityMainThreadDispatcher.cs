using System;
using System.Collections.Generic;
using UnityEngine;
// First, add this dispatcher class to your project
public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> _executionQueue = new Queue<Action>();
    private static UnityMainThreadDispatcher _instance = null;
    private static bool _initialized = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (_instance == null)
        {
            _instance = new GameObject("MainThreadDispatcher").AddComponent<UnityMainThreadDispatcher>();
            DontDestroyOnLoad(_instance.gameObject);
            _initialized = true;
        }
    }

    public void Update()
    {
        lock (_executionQueue)
        {
            while (_executionQueue.Count > 0)
            {
                _executionQueue.Dequeue().Invoke();
            }
        }
    }

    public static void Enqueue(Action action)
    {
        if (!_initialized)
        {
            Debug.LogError("MainThreadDispatcher not initialized!");
            return;
        }

        lock (_executionQueue)
        {
            _executionQueue.Enqueue(action);
        }
    }
}