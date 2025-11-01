using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class ServiceLocator : MonoBehaviour
{
    // GameStateManager (Escape Level, Reset Scene), InputHandler, AudioHandler, SaveManager, UIManager, CardManager (unless we want to give enemies a card)
    private static ServiceLocator _instance;
    public static ServiceLocator Instance => _instance ??= new ServiceLocator();
    private Dictionary<Type, object> _services = new Dictionary<Type, object>();

    public void Register<T>(T service) where T : class
    {
        _services[typeof(T)] = service;
    }

    public T Get<T>() where T : class
    {
        if (_services.TryGetValue(typeof(T), out var service))
        {
            return service as T;
        }

        throw new Exception($"Service {typeof(T).Name} not registered");
    }

    public void ClearServices() => _services.Clear();
}
