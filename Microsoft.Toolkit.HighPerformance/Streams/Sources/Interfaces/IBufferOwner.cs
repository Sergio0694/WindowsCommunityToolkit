// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers;

namespace Microsoft.Toolkit.HighPerformance.Streams
{
    /// <summary>
    /// An interface for types acting as sources for <see cref="Span{T}"/> instances.
    /// </summary>
    internal interface IBufferOwner
    {
        /// <summary>
        /// Gets the current length of the underlying memory area.
        /// </summary>
        int CurrentLength { get; }

        /// <summary>
        /// Gets the current readable length.
        /// </summary>
        int ReadableLength { get; }

        /// <summary>
        /// Gets or sets the current position within the underlying buffer.
        /// </summary>
        int Position { get; set; }

        /// <inheritdoc cref="IBufferWriter{T}.Advance(int)"/>
        void Advance(int count);

        /// <inheritdoc cref="IBufferWriter{T}.GetSpan(int)"/>
        Span<byte> GetSpan(int sizeHint = 0);
    }
}
