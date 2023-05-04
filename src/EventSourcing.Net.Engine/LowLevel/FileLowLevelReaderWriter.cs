using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using EventSourcing.Net.Engine.Extensions;

namespace EventSourcing.Net.Engine.LowLevel;

/// <summary>
/// File reader/writer operation mode.
/// </summary>
public enum FileLowLevelReaderWriterMode
{
    Read,
    Write
}

/// <summary>
/// Provides functionality to read/write raw data from file.
/// </summary>
public sealed class FileLowLevelReaderWriter : IDisposable
{
    private readonly ZipArchive _archive;

    private readonly LowLevelEntry _events;
    private readonly LowLevelEntry _commands;
    private readonly LowLevelEntry _metadata;

    private long _commandsCount = 0;
    private long _eventsCount = 0;
    private readonly FileLowLevelReaderWriterMode _mode;

    public FileLowLevelReaderWriter(Stream stream, FileLowLevelReaderWriterMode mode)
    {
        _mode = mode;

        if (mode == FileLowLevelReaderWriterMode.Write && !stream.CanWrite)
        {
            throw new InvalidOperationException("Stream doesn't allow write to it");
        }

        if (mode == FileLowLevelReaderWriterMode.Read && !stream.CanRead)
        {
            throw new InvalidOperationException("Stream doesn't allow read from it");
        }

        ZipArchiveMode zipArchiveMode =
            mode == FileLowLevelReaderWriterMode.Read ? ZipArchiveMode.Read : ZipArchiveMode.Create;
        _archive = new ZipArchive(stream, zipArchiveMode, true, Encoding.UTF8);

        _commands = new LowLevelEntry(_archive, "commands");
        _events = new LowLevelEntry(_archive, "events");
        _metadata = new LowLevelEntry(_archive, "metadata");
    }

    /// <summary>
    /// Add command data.
    /// </summary>
    /// <param name="entry">Command data.</param>
    /// <exception cref="InvalidOperationException">Class created for reading.</exception>
    public void Add(CommandEntry entry)
    {
        if (_mode != FileLowLevelReaderWriterMode.Write)
        {
            throw new InvalidOperationException("ReaderWriter isn't created for writing");
        }

        _commands.Writer.Write(entry);
        _commandsCount++;
    }

    /// <summary>
    /// Add event data.
    /// </summary>
    /// <param name="entry">Event data.</param>
    /// <exception cref="InvalidOperationException">Class created for reading.</exception>
    public void Add(EventEntry entry)
    {
        if (_mode != FileLowLevelReaderWriterMode.Write)
        {
            throw new InvalidOperationException("ReaderWriter isn't created for writing");
        }

        _events.Writer.Write(entry);
        _eventsCount++;
    }

    /// <summary>
    /// Add metadata.
    /// </summary>
    /// <returns>Metadata.</returns>
    /// <exception cref="InvalidOperationException">Class created for reading.</exception>
    public FileLowLevelReaderWriterMetadata StoreAndReturnMetadata()
    {
        if (_mode != FileLowLevelReaderWriterMode.Write)
        {
            throw new InvalidOperationException("ReaderWriter isn't created for writing");
        }

        _metadata.Writer.Write(_commandsCount);
        _metadata.Writer.Write(_eventsCount);

        _metadata.Dispose();

        return new FileLowLevelReaderWriterMetadata(_commandsCount, _eventsCount);
    }

    /// <summary>
    /// Read and return next command.
    /// </summary>
    /// <returns>Command data.</returns>
    /// <exception cref="InvalidOperationException">Class created for writing.</exception>
    public CommandEntry GetNextCommand()
    {
        if (_mode != FileLowLevelReaderWriterMode.Read)
        {
            throw new InvalidOperationException("ReaderWriter isn't created for reading");
        }

        if (_commands.Reader == null)
        {
            return null;
        }

        return _commands.Reader.ReadCommandEntry();
    }

    /// <summary>
    /// Read and return next event.
    /// </summary>
    /// <returns>Event data.</returns>
    /// <exception cref="InvalidOperationException">Class created for writing.</exception>
    public EventEntry GetNextEvent()
    {
        if (_mode != FileLowLevelReaderWriterMode.Read)
        {
            throw new InvalidOperationException("ReaderWriter isn't created for reading");
        }

        if (_events.Reader == null)
        {
            return null;
        }

        return _events.Reader.ReadEventEntry();
    }

    /// <summary>
    /// Return metadata.
    /// </summary>
    /// <returns>Metadata.</returns>
    /// <exception cref="InvalidOperationException">Class created for writing.</exception>
    public FileLowLevelReaderWriterMetadata GetMetadata()
    {
        if (_mode != FileLowLevelReaderWriterMode.Read)
        {
            throw new InvalidOperationException("ReaderWriter isn't created for reading");
        }

        if (_metadata.Reader == null)
        {
            return new FileLowLevelReaderWriterMetadata(0, 0);
        }

        long commandsCount = _metadata.Reader.ReadInt64();
        long eventsCount = _metadata.Reader.ReadInt64();

        return new FileLowLevelReaderWriterMetadata(commandsCount, eventsCount);
    }

    /// <summary>
    /// Mark commands writing done.
    /// </summary>
    public void MarkCommandsWritingDone()
    {
        _commands.Dispose();
    }

    /// <summary>
    /// Mark events writing done.
    /// </summary>
    public void MarkEventsWritingDone()
    {
        _events.Dispose();
    }

    public void Dispose()
    {
        _commands.Dispose();
        _events.Dispose();
        _metadata.Dispose();
        _archive.Dispose();
    }

    private sealed class LowLevelEntry : IDisposable
    {
        private readonly Lazy<BinaryWriter> _writer;
        private readonly Lazy<BinaryReader> _reader;
        private Stream _stream;
        private bool _isDisposed = false;

        internal LowLevelEntry(ZipArchive archive, string entryName)
        {
            if (archive == null)
            {
                throw new ArgumentNullException(nameof(archive));
            }

            Lazy<ZipArchiveEntry?> entry = new(() =>
            {
                if (archive.Mode == ZipArchiveMode.Read)
                {
                    return archive.GetEntry(entryName);
                }

                return archive.CreateEntry(entryName);
            }, false);
            _writer = new Lazy<BinaryWriter>(() =>
            {
                IsOpen = true;
                _stream = entry.Value.Open();
                return new BinaryWriter(_stream, Encoding.UTF8);
            }, false);
            _reader = new Lazy<BinaryReader>(() =>
            {
                IsOpen = true;
                _stream = entry.Value.Open();
                return new BinaryReader(_stream, Encoding.UTF8);
            }, false);
        }


        internal BinaryWriter Writer
        {
            get
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(LowLevelEntry));
                }

                return _writer.Value;
            }
        }

        internal BinaryReader Reader
        {
            get
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(LowLevelEntry));
                }

                return _reader.Value;
            }
        }

        internal bool IsOpen { get; private set; }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            if (_writer.IsValueCreated)
            {
                _writer.Value?.Dispose();
            }

            if (_reader.IsValueCreated)
            {
                _reader.Value?.Dispose();
            }

            _stream?.Close();
            IsOpen = false;
            _isDisposed = true;
        }
    }
}

public record FileLowLevelReaderWriterMetadata(long CommandsCount, long EventsCount);