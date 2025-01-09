using System;
using System.Collections.Generic;
using UnityEngine;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static UnityMainThreadDispatcher instance;
    private readonly Queue<Action> executionQueue = new Queue<Action>();

    public static UnityMainThreadDispatcher Instance()
    {
        if (instance == null)
        {
            Debug.LogError("UnityMainThreadDispatcher has not been initialized. Ensure it exists in the scene.");
        }
        return instance;
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject); // Prevent duplicate instances
        }
    }

    void Update()
    {
        lock (executionQueue)
        {
            while (executionQueue.Count > 0)
            {
                var action = executionQueue.Dequeue();
                action.Invoke();
            }
        }
    }

    public void Enqueue(Action action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        lock (executionQueue)
        {
            executionQueue.Enqueue(action);
        }
    }
}
