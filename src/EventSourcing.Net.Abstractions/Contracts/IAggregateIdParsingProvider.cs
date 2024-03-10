namespace EventSourcing.Net.Abstractions.Contracts;

using System;

public interface IAggregateIdParsingProvider
{
    public TId Parse<TId>(string value);

    public Func<string, object> GetParser(Type idType);
}