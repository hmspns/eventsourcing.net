using System;
using System.Diagnostics;

namespace EventSourcing.Net.Engine;

public readonly struct TraceHelper : IDisposable
{
    private readonly Stopwatch _stopwatch;
    private readonly string _message;
    
    /// <summary>
    /// Write message to trace with measurement of time.
    /// </summary>
    /// <param name="message">Message to trace.</param>
    public TraceHelper(string message)
    {
        _message = message;
        _stopwatch = Stopwatch.StartNew();
    }

    /// <summary>
    /// Write message to trace with measurement of time.
    /// </summary>
    /// <param name="preMessage">Message to trace before operation.</param>
    /// <param name="postMessage">Message to trace after operation.</param>
    public TraceHelper(string preMessage, string postMessage)
    {
        Trace.WriteLine(preMessage);
        _message = postMessage;
        _stopwatch = Stopwatch.StartNew();
    }
    
    public void Dispose()
    {
        if (_message != null)
        {
            Trace.WriteLine(_message + ". Elapsed: " + _stopwatch.Elapsed.ToString());
        }
    }
}