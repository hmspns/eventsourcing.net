using StackExchange.Redis;

namespace EventSourcing.Storage.Redis;

public sealed class RedisConnection : IRedisConnection, IDisposable
{
    private long _lastReconnectTicks = DateTimeOffset.MinValue.UtcTicks;
    private DateTimeOffset _firstError = DateTimeOffset.MinValue;
    private DateTimeOffset _previousError = DateTimeOffset.MinValue;

    private readonly object _reconnectLock = new object();


    private string _connectionString = "TODO: CALL InitializeConnectionString() method with connection string";
    private Lazy<ConnectionMultiplexer> _multiplexer;

    private bool _isDisposed = false;

    // In general, let StackExchange.Redis handle most reconnects,
    // so limit the frequency of how often this will actually reconnect.
    public TimeSpan ReconnectMinFrequency { get; } = TimeSpan.FromSeconds(60);

    // if errors continue for longer than the below threshold, then the
    // multiplexer seems to not be reconnecting, so re-create the multiplexer
    public TimeSpan ReconnectErrorThreshold { get; } = TimeSpan.FromSeconds(30);

    public ConnectionMultiplexer Connection
    {
        get
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(RedisConnection));
            }
            return _multiplexer.Value;
        }
    }

    public RedisConnection(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString));
        }

        InitializeConnectionString(connectionString);
        _multiplexer = CreateMultiplexer();
    }

    private void InitializeConnectionString(string cnxString)
    {
        if (string.IsNullOrWhiteSpace(cnxString))
        {
            throw new ArgumentNullException(nameof(cnxString));
        }

        _connectionString = cnxString;
    }

    /// <summary>
    /// Force a new ConnectionMultiplexer to be created.
    /// NOTES:
    ///     1. Users of the ConnectionMultiplexer MUST handle ObjectDisposedExceptions, which can now happen as a result of calling ForceReconnect()
    ///     2. Don't call ForceReconnect for Timeouts, just for RedisConnectionExceptions or SocketExceptions
    ///     3. Call this method every time you see a connection exception, the code will wait to reconnect:
    ///         a. for at least the "ReconnectErrorThreshold" time of repeated errors before actually reconnecting
    ///         b. not reconnect more frequently than configured in "ReconnectMinFrequency"
    /// </summary>
    public void ForceReconnect()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(RedisConnection));
        }
        
        DateTimeOffset utcNow = DateTimeOffset.UtcNow;
        long previousTicks = Interlocked.Read(ref _lastReconnectTicks);
        DateTimeOffset previousReconnect = new DateTimeOffset(previousTicks, TimeSpan.Zero);
        TimeSpan elapsedSinceLastReconnect = utcNow - previousReconnect;

        // If mulitple threads call ForceReconnect at the same time, we only want to honor one of them.
        if (elapsedSinceLastReconnect > ReconnectMinFrequency)
        {
            lock (_reconnectLock)
            {
                utcNow = DateTimeOffset.UtcNow;
                elapsedSinceLastReconnect = utcNow - previousReconnect;

                if (_firstError == DateTimeOffset.MinValue)
                {
                    // We haven't seen an error since last reconnect, so set initial values.
                    _firstError = utcNow;
                    _previousError = utcNow;
                    return;
                }

                if (elapsedSinceLastReconnect < ReconnectMinFrequency)
                {
                    return; // Some other thread made it through the check and the lock, so nothing to do.
                }

                TimeSpan elapsedSinceFirstError = utcNow - _firstError;
                TimeSpan elapsedSinceMostRecentError = utcNow - _previousError;

                bool shouldReconnect =
                    elapsedSinceFirstError >=
                    ReconnectErrorThreshold // make sure we gave the multiplexer enough time to reconnect on its own if it can
                    && elapsedSinceMostRecentError <=
                    ReconnectErrorThreshold; //make sure we aren't working on stale data (e.g. if there was a gap in errors, don't reconnect yet).

                // Update the previousError timestamp to be now (e.g. this reconnect request)
                _previousError = utcNow;

                if (shouldReconnect)
                {
                    _firstError = DateTimeOffset.MinValue;
                    _previousError = DateTimeOffset.MinValue;

                    Lazy<ConnectionMultiplexer> oldMultiplexer = _multiplexer;
                    CloseMultiplexer(oldMultiplexer);
                    _multiplexer = CreateMultiplexer();
                    Interlocked.Exchange(ref _lastReconnectTicks, utcNow.UtcTicks);
                }
            }
        }
    }

    private Lazy<ConnectionMultiplexer> CreateMultiplexer()
    {
        return new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(_connectionString));
    }

    private void CloseMultiplexer(Lazy<ConnectionMultiplexer> oldMultiplexer)
    {
        if (oldMultiplexer != null)
        {
            try
            {
                oldMultiplexer.Value.Close();
            }
            catch (Exception)
            {
                // Example error condition: if accessing old.Value causes a connection attempt and that fails.
            }
        }
    }

    public void Dispose()
    {
        if (!_isDisposed)
        {
            CloseMultiplexer(_multiplexer);
            _isDisposed = true;
        }
    }
}