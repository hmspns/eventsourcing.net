using System;
using EventSourcing.Abstractions.Contracts;
using EventSourcing.Core.Exceptions;

namespace EventSourcing.Core.Implementations;

internal static class EventSourcingEngineFactory
{
    private static Lazy<IEventSourcingEngine>? _lazy;
    private static IEventSourcingEngine? _engine;

    internal static void Initialize(Lazy<IEventSourcingEngine> lazy)
    {
        if (lazy == null)
        {
            Thrown.ArgumentNullException(nameof(lazy));
        }
        _lazy = lazy;
    }

    internal static IEventSourcingEngine Get()
    {
        if (_engine != null)
        {
            return _engine;
        }

        if (_lazy == null)
        {
            Thrown.InvalidOperationException($"Method {nameof(EventSourcingEngineFactory)}.{nameof(Initialize)} should be called");
        }
        _engine = _lazy.Value;
        _lazy = null;
        return _engine;
    }
}