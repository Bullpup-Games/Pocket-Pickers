using System;
using System.Collections.Generic;
using UnityEngine;


/*
#### Usage (ServiceLocator):
// Option 1: Via ServiceLocator directly
ServiceLocator.Instance.Get<SinManager>().CollectSin(sinObject);
ServiceLocator.Instance.Get<SinManager>().RemainingSin;
ServiceLocator.Instance.Get<GameStateManager>().EscapeLevel();

// Option 2: Via Services helper (transitional)
Services.SinManager.CollectSin(sinObject);
Services.SinManager.RemainingSin;
Services.GameStateManager.EscapeLevel();

// Option 3: Via dependency injection (preferred)
public class MyClass : MonoBehaviour
{
    private SinManager _sinManager;

    private void Awake()
    {
        _sinManager = ServiceLocator.Instance.Get<SinManager>();
    }

    private void DoSomething()
    {
        _sinManager.CollectSin(sinObject);
    }
}
*/

public class ServiceLocator : MonoBehaviour
{
    private static ServiceLocator _instance;

    public static ServiceLocator Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ServiceLocator>();

                if (_instance == null)
                {
                    GameObject go = new GameObject("ServiceLocator");
                    _instance = go.AddComponent<ServiceLocator>();
                }
            }
            return _instance;
        }
    }

    private Dictionary<Type, object> _services = new Dictionary<Type, object>();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Instead of having most services defined as a singleton we define just the ServiceLocator
        // This allows us to manage instances, lifecycles, and load order of each individual service from a centralized instance
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Register a class and an instance as a locatable service. For ex: GameStateManager.cs & IGameStateManager
    public void Register<T>(T service) where T : class
    {
        var type = typeof(T);
        if (_services.ContainsKey(type))
        {
            Debug.LogWarning($"Service {type.Name} is already registered. Overwriting...");
        }

        _services[type] = service;
        Debug.Log($"Registered service: {type.Name}");
    }

    // Get the stored service instance via its interface type
    public T Get<T>() where T : class
    {
        var type = typeof(T);
        if (_services.TryGetValue(type, out var service))
        {
            return service as T;
        }

        Debug.LogError($"Service {type.Name} not registered!");
        return null;
    }

    public bool TryGet<T>(out T service) where T : class
    {
        var type = typeof(T);
        if (_services.TryGetValue(type, out var serviceObj))
        {
            service = serviceObj as T;
            return service != null;
        }

        service = null;
        return false;
    }

    public void Unregister<T>() where T : class
    {
        var type = typeof(T);
        if (_services.Remove(type))
        {
            Debug.Log($"Unregistered service: {type.Name}");
        }
    }

    public void ClearServices()
    {
        _services.Clear();
        Debug.Log("All services cleared");
    }
}
