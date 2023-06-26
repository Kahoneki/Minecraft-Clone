using System.Collections.Generic;
using UnityEngine;

public class MainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<System.Action> MainThreadActions = new Queue<System.Action>();

    private static MainThreadDispatcher _instance;

    private void Awake() {
        _instance = this;
    }

    private void Update() {
        lock (MainThreadActions)
            while (MainThreadActions.Count > 0)
                MainThreadActions.Dequeue()?.Invoke();
    }

    public static void RunOnMainThread(System.Action action) {
        lock (MainThreadActions)
            MainThreadActions.Enqueue(action);
    }
}