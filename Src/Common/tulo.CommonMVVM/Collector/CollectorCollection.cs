namespace tulo.CommonMVVM.Collector;

/// <summary>
/// Provides a collection for storing and retrieving service instances by their type.
/// </summary>
public class CollectorCollection : ICollectorCollection
{
    /// <summary>
    /// Stores the registered services, keyed by their type.
    /// </summary>
    private Dictionary<Type, object> _services = new();

    /// <summary>
    /// Adds or replaces a service instance of the specified type in the collection.
    /// </summary>
    /// <typeparam name="T">The type of the service to add.</typeparam>
    /// <param name="service">The service instance to add.</param>
    public void AddService<T>(T service)
    {
        _services[typeof(T)] = service!;
    }

    /// <summary>
    /// Retrieves a service instance of the specified type from the collection.
    /// </summary>
    /// <typeparam name="T">The type of the service to retrieve.</typeparam>
    /// <returns>
    /// The service instance if found; otherwise, the default value for the type.
    /// </returns>
    public T GetService<T>()
    {
        if (_services.TryGetValue(typeof(T), out var service))
        {
            return (T)service;
        }

        return default!;
    }
}
