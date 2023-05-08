using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.Net;

/// <summary>
/// Provide methods for service registration.
/// </summary>
public abstract class SpecificOptions
{
    private readonly IServiceCollection _services;

    /// <summary>
    /// Initialize new object.
    /// </summary>
    /// <param name="services">Service collection.</param>
    private protected SpecificOptions(IServiceCollection services)
    {
        _services = services;
    }

    /// <summary>
    /// Replace transient service.
    /// </summary>
    /// <typeparam name="TInterface">Interface type.</typeparam>
    /// <typeparam name="TService">Service type.</typeparam>
    /// <returns>Options for the fluent flow.</returns>
    public SpecificOptions ReplaceTransient<TInterface, TService>()
        where TInterface : class
        where TService : class, TInterface
    {
        _services.Replace<TInterface>(services => services.AddTransient<TInterface, TService>());
        return this;
    }

    /// <summary>
    /// Replace transient service.
    /// </summary>
    /// <param name="handler">Handler to register service.</param>
    /// <typeparam name="TInterface">Interface type.</typeparam>
    /// <returns>Options for the fluent flow.</returns>
    public SpecificOptions ReplaceTransient<TInterface>(Func<IServiceProvider, TInterface> handler)
        where TInterface : class
    {
        _services.Replace<TInterface>(services => services.AddTransient<TInterface>(x =>
        {
            TInterface result = handler(x);
            return result;
        }));
        return this;
    }

    /// <summary>
    /// Replace transient service.
    /// </summary>
    /// <param name="type">Service type.</param>
    /// <returns></returns>
    public SpecificOptions ReplaceTransient(Type type)
    {
        _services.Remove(type);
        _services.AddTransient(type);
        return this;
    }
    
    /// <summary>
    /// Replace scoped service.
    /// </summary>
    /// <typeparam name="TInterface">Interface type.</typeparam>
    /// <typeparam name="TService">Service type.</typeparam>
    /// <returns>Options for the fluent flow.</returns>
    public SpecificOptions ReplaceScoped<TInterface, TService>()
        where TInterface : class
        where TService : class, TInterface
    {
        _services.Replace<TInterface>(services => services.AddScoped<TInterface, TService>());
        return this;
    }

    /// <summary>
    /// Replace scoped service.
    /// </summary>
    /// <param name="handler">Handler to register service.</param>
    /// <typeparam name="TInterface">Interface type.</typeparam>
    /// <returns>Options for the fluent flow.</returns>
    public SpecificOptions ReplaceScoped<TInterface>(Func<IServiceProvider, TInterface> handler)
        where TInterface : class
    {
        _services.Replace<TInterface>(services => services.AddScoped<TInterface>(x =>
        {
            TInterface result = handler(x);
            return result;
        }));
        return this;
    }
    
    /// <summary>
    /// Replace scoped service.
    /// </summary>
    /// <param name="type">Service type.</param>
    /// <returns></returns>
    public SpecificOptions ReplaceScoped(Type type)
    {
        _services.Remove(type);
        _services.AddScoped(type);
        return this;
    }
    
    /// <summary>
    /// Replace singleton service.
    /// </summary>
    /// <typeparam name="TInterface">Interface type.</typeparam>
    /// <typeparam name="TService">Service type.</typeparam>
    /// <returns>Options for the fluent flow.</returns>
    public SpecificOptions ReplaceSingleton<TInterface, TService>()
        where TInterface : class
        where TService : class, TInterface
    {
        _services.Replace<TInterface>(services => services.AddSingleton<TInterface, TService>());
        return this;
    }

    /// <summary>
    /// Replace singleton service.
    /// </summary>
    /// <param name="handler">Handler to register service.</param>
    /// <typeparam name="TInterface">Interface type.</typeparam>
    /// <returns>Options for the fluent flow.</returns>
    public SpecificOptions ReplaceSingleton<TInterface>(Func<IServiceProvider, TInterface> handler)
        where TInterface : class
    {
        _services.Replace<TInterface>(services => services.AddSingleton<TInterface>(x =>
        {
            TInterface result = handler(x);
            return result;
        }));
        return this;
    }
    
    /// <summary>
    /// Replace singleton service.
    /// </summary>
    /// <param name="type">Service type.</param>
    /// <returns></returns>
    public SpecificOptions ReplaceSingleton(Type type)
    {
        _services.Remove(type);
        _services.AddSingleton(type);
        return this;
    }

    internal SpecificOptions IfNotRegistered<TInterface>(Action<IServiceCollection> handler)
    {
        _services.IfNotRegistered<TInterface>(handler);
        return this;
    }
}