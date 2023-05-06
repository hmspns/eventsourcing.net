﻿using EventSourcing.Net.Abstractions.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.Net.Bus.Mediatr;

public static class EventSourcingBusExtensions
{
    /// <summary>
    /// Register Mediatr as service bus.
    /// </summary>
    /// <param name="bus">Options.</param>
    /// <remarks>
    /// MediatrEventPublisherResolver will be registered as transient.
    /// MediatrEventSourcingCommandBus will be registered as transient.
    /// </remarks>
    public static EventSourcingBusOptions UseMediatr(this EventSourcingBusOptions bus)
    {
        bus.Options.Services.AddTransient<IResolveEventPublisher, MediatrEventPublisherResolver>();
        bus.Options.Services.AddTransient<IEventSourcingCommandBus, MediatrEventSourcingCommandBus>();
        return bus;
    }
}