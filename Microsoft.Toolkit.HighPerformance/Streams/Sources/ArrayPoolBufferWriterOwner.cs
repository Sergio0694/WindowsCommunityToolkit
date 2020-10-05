// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.CompilerServices;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace Microsoft.Toolkit.HighPerformance.Streams
{
    /// <summary>
    /// An <see cref="IBufferOwner"/> implementation wrapping an <see cref="ArrayPoolBufferWriter{T}"/> instance.
    /// </summary>
    internal readonly struct ArrayPoolBufferWriterOwner : IBufferOwner
    {
        /// <summary>
        /// The wrapped <see cref="ArrayPoolBufferWriter{T}"/> instance.
        /// </summary>
        private readonly ArrayPoolBufferWriter<byte> bufferWriter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayPoolBufferWriterOwner"/> struct.
        /// </summary>
        /// <param name="bufferWriter">The wrapped <see cref="ArrayPoolBufferWriter{T}"/> instance.</param>
        public ArrayPoolBufferWriterOwner(ArrayPoolBufferWriter<byte> bufferWriter)
        {
            this.bufferWriter = bufferWriter;
        }

        /// <inheritdoc/>
        public int CurrentLength
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.bufferWriter.Capacity;
        }

        /// <inheritdoc/>
        public int ReadableLength
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.bufferWriter.Capacity - this.bufferWriter.WrittenCount;
        }

        /// <inheritdoc/>
        public int Position
        {
            get => this.bufferWriter.WrittenCount;
            set => this.bufferWriter.AdvanceTo(value);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance(int count)
        {
            this.bufferWriter.Advance(count);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> GetSpan(int sizeHint = 0)
        {
            return this.bufferWriter.GetSpan(sizeHint);
        }
    }
}
