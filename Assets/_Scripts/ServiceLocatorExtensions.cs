using UnityEngine;

/// <summary>
/// Extension methods and helper properties to make transitioning from Singletons easier.
/// These provide backward compatibility while migrating to the ServiceLocator pattern.
/// </summary>
public static class ServiceLocatorExtensions
{
    /// <summary>
    /// Gets a service from the ServiceLocator. Returns null if not found.
    /// </summary>
    public static T GetService<T>(this MonoBehaviour component) where T : class
    {
        return ServiceLocator.Instance.Get<T>();
    }

    /// <summary>
    /// Tries to get a service from the ServiceLocator.
    /// </summary>
    public static bool TryGetService<T>(this MonoBehaviour component, out T service) where T : class
    {
        return ServiceLocator.Instance.TryGet<T>(out service);
    }
}

/// <summary>
/// Static helper class to provide transitional access to services.
/// Use this to gradually migrate from Singleton pattern to ServiceLocator.
/// Eventually, this should be removed in favor of dependency injection.
/// </summary>
public static class Services
{
    public static SinManager SinManager => ServiceLocator.Instance.Get<SinManager>();
    public static GameStateManager GameStateManager => ServiceLocator.Instance.Get<GameStateManager>();
    public static UIManager UIManager => ServiceLocator.Instance.Get<UIManager>();
    public static SaveManager SaveManager => ServiceLocator.Instance.Get<SaveManager>();
    public static LevelLoader LevelLoader => ServiceLocator.Instance.Get<LevelLoader>();
}
