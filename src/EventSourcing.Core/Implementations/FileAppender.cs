using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.Identities;
using EventSourcing.Abstractions.Types;

namespace EventSourcing.Core.Implementations;

/// <inheritdoc />
public sealed class FileAppender : IAppendOnly
{
    private readonly Stream? _stream;

    public FileAppender(Stream? stream)
    {
        _stream = stream;
    }
    
    public Task<IAppendEventsResult> Append<TId>(StreamId streamName, IAppendDataPackage<TId> data, AggregateVersion expectedStreamVersion, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<IEventsData> ReadSpecificStream(StreamId streamName, StreamPosition from, StreamPosition to)
    {
        throw new NotSupportedException();
    }

    public Task<IEventsData> ReadAllStreams(StreamReadOptions readOptions)
    {
        throw new NotSupportedException();
    }

    public Task<StreamId[]> FindStreamIds(string startsWithPrefix)
    {
        throw new NotSupportedException();
    }

    public Task<bool> IsExist()
    {
        return Task.FromResult(_stream != null);
    }

    public Task Initialize()
    {
        return Task.CompletedTask;
    }
    
    public void Dispose()
    {
        _stream?.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_stream != null)
        {
            await _stream.DisposeAsync();
        }
    }
}