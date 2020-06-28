// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
#if NETCORE_RUNTIME
#elif SPAN_RUNTIME_SUPPORT
using System.Runtime.InteropServices;
#else
using Microsoft.Toolkit.HighPerformance.Extensions;
#endif

namespace Microsoft.Toolkit.HighPerformance
{
    /// <summary>
    /// A <see langword="struct"/> that can store a reference to a value of a specified type.
    /// </summary>
    /// <typeparam name="T">The type of value to reference.</typeparam>
    public readonly ref struct Ref<T>
    {
#if NETCORE_RUNTIME
        /// <summary>
        /// The <see cref="ByReference{T}"/> instance holding the current reference.
        /// </summary>
        internal readonly ByReference<T> ByReference;

        /// <summary>
        /// Initializes a new instance of the <see cref="Ref{T}"/> struct.
        /// </summary>
        /// <param name="value">The reference to the target <typeparamref name="T"/> value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Ref(ref T value)
        {
            ByReference = new ByReference<T>(ref value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Ref{T}"/> struct.
        /// </summary>
        /// <param name="byReference">The input <see cref="ByReference{T}"/> to the target <typeparamref name="T"/> value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Ref(ByReference<T> byReference)
        {
            ByReference = byReference;
        }
#elif SPAN_RUNTIME_SUPPORT
        /// <summary>
        /// The 1-length <see cref="Span{T}"/> instance used to track the target <typeparamref name="T"/> value.
        /// </summary>
        internal readonly Span<T> Span;

        /// <summary>
        /// Initializes a new instance of the <see cref="Ref{T}"/> struct.
        /// </summary>
        /// <param name="value">The reference to the target <typeparamref name="T"/> value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Ref(ref T value)
        {
            Span = MemoryMarshal.CreateSpan(ref value, 1);
        }
#else
        /// <summary>
        /// The owner <see cref="object"/> the current instance belongs to
        /// </summary>
        internal readonly object Owner;

        /// <summary>
        /// The target offset within <see cref="Owner"/> the current instance is pointing to
        /// </summary>
        internal readonly IntPtr Offset;

        /// <summary>
        /// Initializes a new instance of the <see cref="Ref{T}"/> struct.
        /// </summary>
        /// <param name="owner">The owner <see cref="object"/> to create a portable reference for.</param>
        /// <param name="value">The target reference to point to (it must be within <paramref name="owner"/>).</param>
        /// <remarks>The <paramref name="value"/> parameter is not validated, and it's responsability of the caller to ensure it's valid.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Ref(object owner, ref T value)
        {
            Owner = owner;
            Offset = owner.DangerousGetObjectDataByteOffset(ref Unsafe.AsRef(value));
        }
#endif

        /// <summary>
        /// Gets the <typeparamref name="T"/> reference represented by the current <see cref="Ref{T}"/> instance.
        /// </summary>
        public ref T Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if NETCORE_RUNTIME
                return ref ByReference.Value;
#elif SPAN_RUNTIME_SUPPORT
                return ref MemoryMarshal.GetReference(Span);
#else
                return ref Owner.DangerousGetObjectDataReferenceAt<T>(Offset);
#endif
            }
        }

        /// <summary>
        /// Returns a reference to an element at a specified offset with respect to <see cref="Value"/>.
        /// </summary>
        /// <param name="offset">The offset of the element to retrieve, starting from the reference provided by <see cref="Value"/>.</param>
        /// <returns>A reference to the element at the specified offset from <see cref="Value"/>.</returns>
        /// <remarks>
        /// This method offers a layer of abstraction over <see cref="Unsafe.Add{T}(ref T,int)"/>, and similarly it does not does not do
        /// any kind of input validation. It is responsability of the caller to ensure the supplied offset is valid.
        /// </remarks>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T DangerousGetReferenceAt(int offset)
        {
#if NETCORE_RUNTIME
            return ref Unsafe.Add(ref ByReference.Value, offset);
#elif SPAN_RUNTIME_SUPPORT
            return ref Unsafe.Add(ref MemoryMarshal.GetReference(Span), offset);
#else
            return ref Owner.DangerousGetObjectDataReferenceAt<T>(Offset + offset);
#endif
        }

        /// <summary>
        /// Returns a reference to an element at a specified offset with respect to <see cref="Value"/>.
        /// </summary>
        /// <param name="offset">The offset of the element to retrieve, starting from the reference provided by <see cref="Value"/>.</param>
        /// <returns>A reference to the element at the specified offset from <see cref="Value"/>.</returns>
        /// <remarks>
        /// This method offers a layer of abstraction over <see cref="Unsafe.Add{T}(ref T,IntPtr)"/>, and similarly it does not does not do
        /// any kind of input validation. It is responsability of the caller to ensure the supplied offset is valid.
        /// </remarks>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T DangerousGetReferenceAt(IntPtr offset)
        {
#if NETCORE_RUNTIME
            return ref Unsafe.Add(ref ByReference.Value, offset);
#elif SPAN_RUNTIME_SUPPORT
            return ref Unsafe.Add(ref MemoryMarshal.GetReference(Span), offset);
#else
            unsafe
            {
                if (sizeof(IntPtr) == sizeof(long))
                {
                    return ref Owner.DangerousGetObjectDataReferenceAt<T>((IntPtr)((long)(byte*)Offset + (long)(byte*)offset));
                }

                return ref Owner.DangerousGetObjectDataReferenceAt<T>((IntPtr)((int)(byte*)Offset + (int)(byte*)offset));
            }
#endif
        }

        /// <summary>
        /// Implicitly gets the <typeparamref name="T"/> value from a given <see cref="Ref{T}"/> instance.
        /// </summary>
        /// <param name="reference">The input <see cref="Ref{T}"/> instance.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator T(Ref<T> reference)
        {
            return reference.Value;
        }
    }
}
