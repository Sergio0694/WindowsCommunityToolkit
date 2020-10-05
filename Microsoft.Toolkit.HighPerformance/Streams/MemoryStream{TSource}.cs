// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Toolkit.HighPerformance.Streams
{
    /// <summary>
    /// A <see cref="Stream"/> implementation wrapping a <see cref="Memory{T}"/> or <see cref="ReadOnlyMemory{T}"/> instance.
    /// </summary>
    /// <typeparam name="TSource">The type of source to use for the underlying data.</typeparam>
    /// <remarks>
    /// This type is not marked as <see langword="sealed"/> so that it can be inherited by
    /// <see cref="IMemoryOwnerStream{TSource}"/>, which adds the <see cref="IDisposable"/> support for
    /// the wrapped buffer. We're not worried about the performance penalty here caused by the JIT
    /// not being able to resolve the <see langword="callvirt"/> instruction, as this type is
    /// only exposed as a <see cref="Stream"/> anyway, so the generated code would be the same.
    /// </remarks>
    internal partial class MemoryStream<TSource> : Stream
        where TSource : struct, IBufferOwner
    {
        /// <summary>
        /// Indicates whether <see cref="source"/> can be written to.
        /// </summary>
        private readonly bool isReadOnly;

        /// <summary>
        /// The <typeparamref name="TSource"/> instance currently in use.
        /// </summary>
        private TSource source;

        /// <summary>
        /// Indicates whether or not the current instance has been disposed
        /// </summary>
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryStream{TSource}"/> class.
        /// </summary>
        /// <param name="source">The input <typeparamref name="TSource"/> instance to use.</param>
        /// <param name="isReadOnly">Indicates whether <paramref name="source"/> can be written to.</param>
        public MemoryStream(TSource source, bool isReadOnly)
        {
            this.source = source;
            this.isReadOnly = isReadOnly;
        }

        /// <inheritdoc/>
        public sealed override bool CanRead
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => !this.disposed;
        }

        /// <inheritdoc/>
        public sealed override bool CanSeek
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => !this.disposed;
        }

        /// <inheritdoc/>
        public sealed override bool CanWrite
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => !this.isReadOnly && !this.disposed;
        }

        /// <inheritdoc/>
        public sealed override long Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                MemoryStream.ValidateDisposed(this.disposed);

                return this.source.CurrentLength;
            }
        }

        /// <inheritdoc/>
        public sealed override long Position
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                MemoryStream.ValidateDisposed(this.disposed);

                return this.Position;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                MemoryStream.ValidateDisposed(this.disposed);
                MemoryStream.ValidatePosition(value, this.source.CurrentLength);

                this.Position = unchecked((int)value);
            }
        }

        /// <inheritdoc/>
        public sealed override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            try
            {
                CopyTo(destination, bufferSize);

                return Task.CompletedTask;
            }
            catch (OperationCanceledException e)
            {
                return Task.FromCanceled(e.CancellationToken);
            }
            catch (Exception e)
            {
                return Task.FromException(e);
            }
        }

        /// <inheritdoc/>
        public sealed override void Flush()
        {
        }

        /// <inheritdoc/>
        public sealed override Task FlushAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public sealed override Task<int> ReadAsync(byte[]? buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<int>(cancellationToken);
            }

            try
            {
                int result = Read(buffer, offset, count);

                return Task.FromResult(result);
            }
            catch (OperationCanceledException e)
            {
                return Task.FromCanceled<int>(e.CancellationToken);
            }
            catch (Exception e)
            {
                return Task.FromException<int>(e);
            }
        }

        /// <inheritdoc/>
        public sealed override Task WriteAsync(byte[]? buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            try
            {
                Write(buffer, offset, count);

                return Task.CompletedTask;
            }
            catch (OperationCanceledException e)
            {
                return Task.FromCanceled(e.CancellationToken);
            }
            catch (Exception e)
            {
                return Task.FromException(e);
            }
        }

        /// <inheritdoc/>
        public sealed override long Seek(long offset, SeekOrigin origin)
        {
            MemoryStream.ValidateDisposed(this.disposed);

            long index = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => this.source.Position + offset,
                SeekOrigin.End => this.source.CurrentLength + offset,
                _ => MemoryStream.ThrowArgumentExceptionForSeekOrigin()
            };

            MemoryStream.ValidatePosition(index, this.source.CurrentLength);

            this.source.Position = unchecked((int)index);

            return index;
        }

        /// <inheritdoc/>
        public sealed override void SetLength(long value)
        {
            MemoryStream.ThrowNotSupportedExceptionForSetLength();
        }

        /// <inheritdoc/>
        public sealed override int Read(byte[]? buffer, int offset, int count)
        {
            MemoryStream.ValidateDisposed(this.disposed);
            MemoryStream.ValidateBuffer(buffer, offset, count);

            int bytesCopied = Math.Min(this.source.ReadableLength, count);

            Span<byte>
                source = this.source.GetSpan(),
                destination = buffer.AsSpan(offset, bytesCopied);

            source.CopyTo(destination);

            this.source.Advance(bytesCopied);

            return bytesCopied;
        }

        /// <inheritdoc/>
        public sealed override int ReadByte()
        {
            MemoryStream.ValidateDisposed(this.disposed);

            Span<byte> source = this.source.GetSpan();

            if ((uint)source.Length > 0u)
            {
                int result = source[0];

                this.source.Advance(1);

                return result;
            }

            return -1;
        }

        /// <inheritdoc/>
        public sealed override void Write(byte[]? buffer, int offset, int count)
        {
            MemoryStream.ValidateDisposed(this.disposed);
            MemoryStream.ValidateCanWrite(CanWrite);
            MemoryStream.ValidateBuffer(buffer, offset, count);

            Span<byte>
                source = buffer.AsSpan(offset, count),
                destination = this.source.GetSpan(count);

            if (!source.TryCopyTo(destination))
            {
                MemoryStream.ThrowArgumentExceptionForEndOfStreamOnWrite();
            }

            this.source.Advance(count);
        }

        /// <inheritdoc/>
        public sealed override void WriteByte(byte value)
        {
            MemoryStream.ValidateDisposed(this.disposed);
            MemoryStream.ValidateCanWrite(CanWrite);

            Span<byte> destination = this.source.GetSpan(1);

            if ((uint)destination.Length >= 1)
            {
                MemoryStream.ThrowArgumentExceptionForEndOfStreamOnWrite();
            }

            destination[0] = value;

            this.source.Advance(1);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            this.source = default;
        }
    }
}
