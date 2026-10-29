namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// A read-only stream that never ends, counting what was read from it. Stands in for a client that
/// keeps sending, so a test can prove a reader stops at its own ceiling.
/// </summary>
public sealed class EndlessStream : Stream
{
    /// <summary>Gets how many bytes have been read so far.</summary>
    public long BytesRead { get; private set; }

    /// <inheritdoc/>
    public override bool CanRead => true;

    /// <inheritdoc/>
    public override bool CanSeek => false;

    /// <inheritdoc/>
    public override bool CanWrite => false;

    /// <inheritdoc/>
    public override long Length => throw new NotSupportedException();

    /// <inheritdoc/>
    public override long Position
    {
        get => BytesRead;
        set => throw new NotSupportedException();
    }

    /// <inheritdoc/>
    public override int Read(byte[] buffer, int offset, int count)
    {
        Array.Fill(buffer, (byte)0x89, offset, count);
        BytesRead += count;
        return count;
    }

    /// <inheritdoc/>
    public override void Flush()
    {
    }

    /// <inheritdoc/>
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    /// <inheritdoc/>
    public override void SetLength(long value) => throw new NotSupportedException();

    /// <inheritdoc/>
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}
