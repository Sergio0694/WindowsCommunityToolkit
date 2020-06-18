﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Toolkit.HighPerformance.Extensions;

namespace Microsoft.Toolkit.HighPerformance.Memory
{
    /// <summary>
    /// <see cref="Memory2D{T}"/> represents a 2D region of arbitrary memory. It is to <see cref="Span2D{T}"/>
    /// what <see cref="Memory{T}"/> is to <see cref="Span{T}"/>. For further details on how the internal layout
    /// is structured, see the docs for <see cref="Span2D{T}"/>. The <see cref="Memory2D{T}"/> type can wrap arrays
    /// of any rank, provided that a valid series of parameters for the target memory area(s) are specified.
    /// </summary>
    /// <typeparam name="T">The type of items in the current <see cref="Memory2D{T}"/> instance.</typeparam>
    public readonly struct Memory2D<T> : IEquatable<Memory2D<T>>
    {
        /// <summary>
        /// The target <see cref="object"/> instance, if present.
        /// </summary>
        private readonly object? instance;

        /// <summary>
        /// The initial offset within <see cref="instance"/>.
        /// </summary>
        private readonly IntPtr offset;

        /// <summary>
        /// The height of the specified 2D region.
        /// </summary>
        private readonly int height;

        /// <summary>
        /// The width of the specified 2D region.
        /// </summary>
        private readonly int width;

        /// <summary>
        /// The pitch of the specified 2D region.
        /// </summary>
        private readonly int pitch;

        /// <summary>
        /// Initializes a new instance of the <see cref="Memory2D{T}"/> struct.
        /// </summary>
        /// <param name="array">The target array to wrap.</param>
        /// <param name="offset">The initial offset within <paramref name="array"/>.</param>
        /// <param name="width">The width of each row in the resulting 2D area.</param>
        /// <param name="height">The height of the resulting 2D area.</param>
        /// <exception cref="ArrayTypeMismatchException">
        /// Thrown when <paramref name="array"/> doesn't match <typeparamref name="T"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when either <paramref name="offset"/>, <paramref name="height"/> or <paramref name="width"/> are invalid.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory2D(T[] array, int offset, int width, int height)
            : this(array, offset, width, height, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Memory2D{T}"/> struct.
        /// </summary>
        /// <param name="array">The target array to wrap.</param>
        /// <param name="offset">The initial offset within <paramref name="array"/>.</param>
        /// <param name="width">The width of each row in the resulting 2D area.</param>
        /// <param name="height">The height of the resulting 2D area.</param>
        /// <param name="pitch">The pitch in the resulting 2D area.</param>
        /// <exception cref="ArrayTypeMismatchException">
        /// Thrown when <paramref name="array"/> doesn't match <typeparamref name="T"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when either <paramref name="offset"/>, <paramref name="height"/>,
        /// <paramref name="width"/> or <paramref name="pitch"/> are invalid.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory2D(T[] array, int offset, int width, int height, int pitch)
        {
            if (array.IsCovariant())
            {
                ThrowHelper.ThrowArrayTypeMismatchException();
            }

            if ((uint)offset >= (uint)array.Length)
            {
                ThrowHelper.ThrowArgumentOutOfRangeExceptionForOffset();
            }

            int remaining = array.Length - offset;

            if ((((uint)width + (uint)pitch) * (uint)height) > (uint)remaining)
            {
                ThrowHelper.ThrowArgumentException();
            }

            this.instance = array;
            this.offset = array.DangerousGetObjectDataByteOffset(ref array.DangerousGetReferenceAt(offset));
            this.height = height;
            this.width = width;
            this.pitch = pitch;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Memory2D{T}"/> struct.
        /// </summary>
        /// <param name="array">The target array to wrap.</param>
        /// <exception cref="ArrayTypeMismatchException">
        /// Thrown when <paramref name="array"/> doesn't match <typeparamref name="T"/>.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory2D(T[,]? array)
        {
            if (array is null)
            {
                this = default;

                return;
            }

            if (array.IsCovariant())
            {
                ThrowHelper.ThrowArrayTypeMismatchException();
            }

            this.instance = array;
            this.offset = array.DangerousGetObjectDataByteOffset(ref array.DangerousGetReference());
            this.height = array.GetLength(0);
            this.width = array.GetLength(1);
            this.pitch = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Memory2D{T}"/> struct wrapping a 2D array.
        /// </summary>
        /// <param name="array">The given 2D array to wrap.</param>
        /// <param name="row">The target row to map within <paramref name="array"/>.</param>
        /// <param name="column">The target column to map within <paramref name="array"/>.</param>
        /// <param name="width">The width to map within <paramref name="array"/>.</param>
        /// <param name="height">The height to map within <paramref name="array"/>.</param>
        /// <exception cref="ArrayTypeMismatchException">
        /// Thrown when <paramref name="array"/> doesn't match <typeparamref name="T"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when either <paramref name="height"/>, <paramref name="width"/> or <paramref name="height"/>
        /// are negative or not within the bounds that are valid for <paramref name="array"/>.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory2D(T[,]? array, int row, int column, int width, int height)
        {
            if (array is null)
            {
                if ((row | column | width | height) != 0)
                {
                    ThrowHelper.ThrowArgumentException();
                }

                this = default;

                return;
            }

            if (array.IsCovariant())
            {
                ThrowHelper.ThrowArrayTypeMismatchException();
            }

            int
                rows = array.GetLength(0),
                columns = array.GetLength(1);

            if ((uint)row >= (uint)rows)
            {
                ThrowHelper.ThrowArgumentOutOfRangeExceptionForRow();
            }

            if ((uint)column >= (uint)columns)
            {
                ThrowHelper.ThrowArgumentOutOfRangeExceptionForColumn();
            }

            if (width > (columns - column))
            {
                ThrowHelper.ThrowArgumentOutOfRangeExceptionForWidth();
            }

            if (height > (rows - row))
            {
                ThrowHelper.ThrowArgumentOutOfRangeExceptionForHeight();
            }

            this.instance = array;
            this.offset = array.DangerousGetObjectDataByteOffset(ref array.DangerousGetReferenceAt(row, column));
            this.height = height;
            this.width = width;
            this.pitch = row + (array.GetLength(1) - column);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Memory2D{T}"/> struct wrapping a layer in a 3D array.
        /// </summary>
        /// <param name="array">The given 3D array to wrap.</param>
        /// <param name="depth">The target layer to map within <paramref name="array"/>.</param>
        /// <exception cref="ArrayTypeMismatchException">
        /// Thrown when <paramref name="array"/> doesn't match <typeparamref name="T"/>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when either <paramref name="depth"/> is invalid.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory2D(T[,,] array, int depth)
        {
            if (array.IsCovariant())
            {
                ThrowHelper.ThrowArrayTypeMismatchException();
            }

            if ((uint)depth >= (uint)array.GetLength(0))
            {
                ThrowHelper.ThrowArgumentOutOfRangeExceptionForDepth();
            }

            this.instance = array;
            this.offset = array.DangerousGetObjectDataByteOffset(ref array.DangerousGetReferenceAt(depth, 0, 0));
            this.height = array.GetLength(1);
            this.width = array.GetLength(2);
            this.pitch = 0;
        }

#if SPAN_RUNTIME_SUPPORT
        /// <summary>
        /// Initializes a new instance of the <see cref="Memory2D{T}"/> struct.
        /// </summary>
        /// <param name="memory">The target <see cref="Memory{T}"/> to wrap.</param>
        /// <param name="offset">The initial offset within <paramref name="memory"/>.</param>
        /// <param name="width">The width of each row in the resulting 2D area.</param>
        /// <param name="height">The height of the resulting 2D area.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when either <paramref name="offset"/>, <paramref name="height"/> or <paramref name="width"/> are invalid.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory2D(Memory<T> memory, int offset, int width, int height)
            : this(memory, offset, width, height, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Memory2D{T}"/> struct.
        /// </summary>
        /// <param name="memory">The target <see cref="Memory{T}"/> to wrap.</param>
        /// <param name="offset">The initial offset within <paramref name="memory"/>.</param>
        /// <param name="width">The width of each row in the resulting 2D area.</param>
        /// <param name="height">The height of the resulting 2D area.</param>
        /// <param name="pitch">The pitch in the resulting 2D area.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when either <paramref name="offset"/>, <paramref name="height"/>,
        /// <paramref name="width"/> or <paramref name="pitch"/> are invalid.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory2D(Memory<T> memory, int offset, int width, int height, int pitch)
        {
            if ((uint)offset >= (uint)memory.Length)
            {
                ThrowHelper.ThrowArgumentOutOfRangeExceptionForOffset();
            }

            int remaining = memory.Length - offset;

            if ((((uint)width + (uint)pitch) * (uint)height) > (uint)remaining)
            {
                ThrowHelper.ThrowArgumentException();
            }

            this.instance = memory.Slice(offset);
            this.offset = default;
            this.height = height;
            this.width = width;
            this.pitch = pitch;
        }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="Memory2D{T}"/> struct with the specified parameters.
        /// </summary>
        /// <param name="instance">The target <see cref="object"/> instance.</param>
        /// <param name="offset">The initial offset within <see cref="instance"/>.</param>
        /// <param name="height">The height of the 2D memory area to map.</param>
        /// <param name="width">The width of the 2D memory area to map.</param>
        /// <param name="pitch">The pitch of the 2D memory area to map.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Memory2D(object instance, IntPtr offset, int height, int width, int pitch)
        {
            this.instance = instance;
            this.offset = offset;
            this.height = height;
            this.width = width;
            this.pitch = pitch;
        }

        /// <summary>
        /// Gets an empty <see cref="Memory2D{T}"/> instance.
        /// </summary>
        public static Memory2D<T> Empty => default;

        /// <summary>
        /// Gets a value indicating whether the current <see cref="Memory2D{T}"/> instance is empty.
        /// </summary>
        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (this.height | this.width) == 0;
        }

        /// <summary>
        /// Gets the length of the current <see cref="Memory2D{T}"/> instance.
        /// </summary>
        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.height * this.width;
        }

        /// <summary>
        /// Gets a <see cref="Span2D{T}"/> instance from the current memory.
        /// </summary>
        public Span2D<T> Span
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (!(this.instance is null))
                {
#if SPAN_RUNTIME_SUPPORT
                    if (this.instance.GetType() == typeof(Memory<T>))
                    {
                        Memory<T> memory = (Memory<T>)this.instance;

                        // If the wrapped object is a Memory<T>, it is always pre-offset
                        ref T r0 = ref memory.Span.DangerousGetReference();

                        return new Span2D<T>(ref r0, this.height, this.width, this.pitch);
                    }
                    else
                    {
                        // The only other possible cases is with the instance being an array
                        ref T r0 = ref this.instance.DangerousGetObjectDataReferenceAt<T>(this.offset);

                        return new Span2D<T>(ref r0, this.height, this.width, this.pitch);
                    }
#else
                    return new Span2D<T>(this.instance, this.offset, this.height, this.width, this.pitch);
#endif
                }

                return default;
            }
        }

        /// <summary>
        /// Slices the current instance with the specified parameters.
        /// </summary>
        /// <param name="row">The target row to map within the current instance.</param>
        /// <param name="column">The target column to map within the current instance.</param>
        /// <param name="width">The width to map within the current instance.</param>
        /// <param name="height">The height to map within the current instance.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when either <paramref name="height"/>, <paramref name="width"/> or <paramref name="height"/>
        /// are negative or not within the bounds that are valid for the current instance.
        /// </exception>
        /// <returns>A new <see cref="Memory2D{T}"/> instance representing a slice of the current one.</returns>
        [Pure]
        public unsafe Memory2D<T> Slice(int row, int column, int width, int height)
        {
            if ((uint)row >= this.height)
            {
                ThrowHelper.ThrowArgumentOutOfRangeExceptionForRow();
            }

            if ((uint)column >= this.width)
            {
                ThrowHelper.ThrowArgumentOutOfRangeExceptionForColumn();
            }

            if ((uint)width > (this.width - column))
            {
                ThrowHelper.ThrowArgumentOutOfRangeExceptionForWidth();
            }

            if ((uint)height > (this.height - row))
            {
                ThrowHelper.ThrowArgumentOutOfRangeExceptionForHeight();
            }

            int shift = ((this.width + this.pitch) * row) + column;
            IntPtr offset = (IntPtr)((byte*)this.offset + shift);

            return new Memory2D<T>(this.instance!, offset, height, width, this.pitch);
        }

        /// <summary>
        /// Copies the contents of this <see cref="Memory2D{T}"/> into a destination <see cref="Memory{T}"/> instance.
        /// </summary>
        /// <param name="destination">The destination <see cref="Memory{T}"/> instance.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="destination" /> is shorter than the source <see cref="Memory2D{T}"/> instance.
        /// </exception>
        public void CopyTo(Memory<T> destination) => Span.CopyTo(destination.Span);

        /// <summary>
        /// Attempts to copy the current <see cref="Memory2D{T}"/> instance to a destination <see cref="Memory{T}"/>.
        /// </summary>
        /// <param name="destination">The target <see cref="Memory{T}"/> of the copy operation.</param>
        /// <returns>Whether or not the operaation was successful.</returns>
        public bool TryCopyTo(Memory<T> destination) => Span.TryCopyTo(destination.Span);

        /// <summary>
        /// Copies the contents of this <see cref="Memory2D{T}"/> into a destination <see cref="Memory2D{T}"/> instance.
        /// </summary>
        /// <param name="destination">The destination <see cref="Memory2D{T}"/> instance.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="destination" /> is shorter than the source <see cref="Memory2D{T}"/> instance.
        /// </exception>
        public void CopyTo(Memory2D<T> destination) => Span.CopyTo(destination.Span);

        /// <summary>
        /// Attempts to copy the current <see cref="Memory2D{T}"/> instance to a destination <see cref="Memory2D{T}"/>.
        /// </summary>
        /// <param name="destination">The target <see cref="Memory2D{T}"/> of the copy operation.</param>
        /// <returns>Whether or not the operaation was successful.</returns>
        public bool TryCopyTo(Memory2D<T> destination) => Span.TryCopyTo(destination.Span);

        /// <summary>
        /// Creates a handle for the memory.
        /// The GC will not move the memory until the returned <see cref="MemoryHandle"/>
        /// is disposed, enabling taking and using the memory's address.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// An instance with nonprimitive (non-blittable) members cannot be pinned.
        /// </exception>
        /// <returns>A <see cref="MemoryHandle"/> instance wrapping the pinned handle.</returns>
        public unsafe MemoryHandle Pin()
        {
            if (!(this.instance is null))
            {
                GCHandle handle = GCHandle.Alloc(this.instance, GCHandleType.Pinned);

                void* pointer = Unsafe.AsPointer(ref this.instance.DangerousGetObjectDataReferenceAt<T>(this.offset));

                return new MemoryHandle(pointer, handle);
            }

            return default;
        }

        /// <summary>
        /// Tries to get a <see cref="Memory{T}"/> instance, if the underlying buffer is contiguous.
        /// </summary>
        /// <param name="memory">The resulting <see cref="Memory{T}"/>, in case of success.</param>
        /// <returns>Whether or not <paramref name="memory"/> was correctly assigned.</returns>
        public bool TryGetMemory(out Memory<T> memory)
        {
            if (this.pitch == 0)
            {
                // Empty Memory2D<T> instance
                if (this.instance is null)
                {
                    memory = default;
                }
                else if (this.instance.GetType() == typeof(Memory<T>))
                {
                    // If the object is a Memory<T>, just slice it as needed
                    memory = ((Memory<T>)this.instance).Slice(0, this.height * this.width);
                }
                else if (this.instance.GetType() == typeof(T[]))
                {
                    // If it's a T[] array, also handle the initial offset
                    memory = Unsafe.As<T[]>(this.instance).AsMemory((int)this.offset, this.height * this.width);
                }
                else
                {
                    // Reuse a single failure path to reduce
                    // the number of returns in the method
                    goto Failure;
                }

                return true;
            }

            Failure:

            memory = default;

            return false;
        }

        /// <summary>
        /// Copies the contents of the current <see cref="Memory2D{T}"/> instance into a new 2D array.
        /// </summary>
        /// <returns>A 2D array containing the data in the current <see cref="Memory2D{T}"/> instance.</returns>
        [Pure]
        public T[,] ToArray() => Span.ToArray();

        /// <inheritdoc/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object? obj)
        {
            return obj is Memory2D<T> memory && Equals(memory);
        }

        /// <inheritdoc/>
        public bool Equals(Memory2D<T> other)
        {
            return
                this.instance == other.instance &&
                this.offset == other.offset &&
                this.height == other.height &&
                this.width == other.width &&
                this.pitch == other.pitch;
        }

        /// <inheritdoc/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            if (!(this.instance is null))
            {
#if SPAN_RUNTIME_SUPPORT
                return HashCode.Combine(
                    RuntimeHelpers.GetHashCode(this.instance),
                    this.offset,
                    this.height,
                    this.width,
                    this.pitch);
#else
                Span<int> values = stackalloc int[]
                {
                    RuntimeHelpers.GetHashCode(this.instance),
                    this.offset.GetHashCode(),
                    this.height,
                    this.width,
                    this.pitch
                };

                return values.GetDjb2HashCode();
#endif
            }

            return 0;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Microsoft.Toolkit.HighPerformance.Memory.Memory2D<{typeof(T)}>[{this.height}, {this.width}]";
        }

        /// <summary>
        /// Defines an implicit conversion of an array to a <see cref="Memory2D{T}"/>
        /// </summary>
        public static implicit operator Memory2D<T>(T[,]? array) => new Memory2D<T>(array);
    }
}