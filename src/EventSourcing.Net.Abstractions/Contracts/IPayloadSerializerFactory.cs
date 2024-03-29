﻿namespace EventSourcing.Net.Abstractions.Contracts;

/// <summary>
/// Factory to create payload serializer.
/// </summary>
public interface IPayloadSerializerFactory
{
    /// <summary>
    /// Return payload serializer.
    /// </summary>
    /// <returns>Payload serializer.</returns>
    IPayloadSerializer GetSerializer();
}