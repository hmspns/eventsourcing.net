using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.Net;

/// <summary>
/// Provide methods for service registration.
/// </summary>
public abstract class EventSourcingConfigurationOptions
{
    private readonly IServiceCollection _services;

    /// <summary>
    /// Initialize new object.
    /// </summary>
    /// <param name="services">Service collection.</param>
    private protected EventSourcingConfigurationOptions(IServiceCollection services)
    {
        _services = services;
    }

    /// <summary>
    /// Replace transient service.
    /// </summary>
    /// <typeparam name="TInterface">Interface type.</typeparam>
    /// <typeparam name="TService">Service type.</typeparam>
    /// <returns>Options for the fluent flow.</returns>
    protected internal EventSourcingConfigurationOptions ReplaceTransient<TInterface, TService>()
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
    protected internal  EventSourcingConfigurationOptions ReplaceTransient<TInterface>(Func<IServiceProvider, TInterface> handler)
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
    /// <returns>Options for the fluent flow.</returns>
    protected internal  EventSourcingConfigurationOptions ReplaceTransient(Type type)
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
    protected internal  EventSourcingConfigurationOptions ReplaceScoped<TInterface, TService>()
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
    protected internal  EventSourcingConfigurationOptions ReplaceScoped<TInterface>(Func<IServiceProvider, TInterface> handler)
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
    /// <returns>Options for the fluent flow.</returns>
    protected internal  EventSourcingConfigurationOptions ReplaceScoped(Type type)
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
    protected internal  EventSourcingConfigurationOptions ReplaceSingleton<TInterface, TService>()
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
    protected internal  EventSourcingConfigurationOptions ReplaceSingleton<TInterface>(Func<IServiceProvider, TInterface> handler)
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
    /// <returns>Options for the fluent flow.</returns>
    protected internal  EventSourcingConfigurationOptions ReplaceSingleton(Type type)
    {
        _services.Remove(type);
        _services.AddSingleton(type);
        return this;
    }

    /// <summary>
    /// Replace singleton service.
    /// </summary>
    /// <param name="serviceInstance">Service type.</param>
    /// <returns>Options for the fluent flow.</returns>
    protected internal  EventSourcingConfigurationOptions ReplaceSingleton<TService>(TService serviceInstance)
        where TService : class
    {
        _services.Replace<TService>(services => services.AddSingleton<TService>(serviceInstance));
        return this;
    }

    /// <summary>
    /// Remove specific service.
    /// </summary>
    /// <typeparam name="TService">Service type.</typeparam>
    /// <returns>Options for the fluent flow.</returns>
    protected internal  EventSourcingConfigurationOptions Remove<TService>() where TService : class
    {
        _services.Remove<TService>();
        return this;
    }

    internal EventSourcingConfigurationOptions IfNotRegistered<TInterface>(Action<IServiceCollection> handler)
    {
        _services.IfNotRegistered<TInterface>(handler);
        return this;
    }
}