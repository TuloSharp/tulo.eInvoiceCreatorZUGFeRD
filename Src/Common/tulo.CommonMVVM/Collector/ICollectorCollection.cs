namespace tulo.CommonMVVM.Collector;

/// <summary>
/// A simple service container used to register and resolve application services (a lightweight DI registry).
/// Implementations typically store instances keyed by their service type and return them on request.
/// </summary>
public interface ICollectorCollection
{
    /// <summary>
    /// Registers a service instance for the given type <typeparamref name="T"/>.
    /// If a service of the same type is already registered, the implementation may replace it
    /// or throw an exception (implementation-specific behavior).
    /// </summary>
    /// <typeparam name="T">The service type (usually an interface or base class).</typeparam>
    /// <param name="service">The concrete service instance to register.</param>
    void AddService<T>(T service);

    /// <summary>
    /// Resolves (retrieves) the registered service instance for the given type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The service type to resolve.</typeparam>
    /// <returns>The registered instance of type <typeparamref name="T"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if no service of type <typeparamref name="T"/> is registered (recommended behavior).
    /// </exception>
    T GetService<T>();
}