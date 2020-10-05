// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Toolkit.HighPerformance.Buffers;
using Microsoft.Toolkit.HighPerformance.Streams;

namespace Microsoft.Toolkit.HighPerformance.Extensions
{
    /// <summary>
    /// Helpers for working with the <see cref="ArrayPoolBufferWriter{T}"/> type.
    /// </summary>
    public static class ArrayPoolBufferWriterExtensions
    {
        /// <summary>
        /// Returns a <see cref="Stream"/> wrapping the input <see cref="ArrayPoolBufferWriter{T}"/> of <see cref="byte"/> instance.
        /// </summary>
        /// <param name="bufferWriter">The input <see cref="ArrayPoolBufferWriter{T}"/> of <see cref="byte"/> instance.</param>
        /// <returns>A <see cref="Stream"/> wrapping the <paramref name="bufferWriter"/>.</returns>
        /// <remarks>
        /// The returned <see cref="Stream"/> will be able to read and write at arbitrary locations within the internal
        /// buffer for the input <see cref="ArrayPoolBufferWriter{T}"/> instannce. 
        /// </remarks>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Stream AsStream(this ArrayPoolBufferWriter<byte> bufferWriter)
        {
            return new MemoryStream<ArrayPoolBufferWriterOwner>(new ArrayPoolBufferWriterOwner(bufferWriter), false);
        }
    }
}
