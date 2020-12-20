// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Media.Animation;
using static Microsoft.Toolkit.Uwp.UI.Animations.Extensions.AnimationExtensions;

namespace Microsoft.Toolkit.Uwp.UI.Animations
{
    /// <summary>
    /// A generic keyframe animation builder.
    /// </summary>
    /// <typeparam name="T">The type of values being set by the animation being constructed.</typeparam>
    internal abstract partial class TimedKeyFrameAnimationBuilder<T> : ITimedKeyFrameAnimationBuilder<T>
        where T : unmanaged
    {
        /// <summary>
        /// The target property to animate.
        /// </summary>
        private readonly string property;

        /// <summary>
        /// The target delay for the animation, if any.
        /// </summary>
        private readonly TimeSpan? delay;

        /// <summary>
        /// The list of keyframes to use.
        /// </summary>
        private readonly List<KeyFrameInfo> keyFrames = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="TimedKeyFrameAnimationBuilder{T}"/> class.
        /// </summary>
        /// <param name="property">The target property to animate.</param>
        /// <param name="delay">The target delay for the animation.</param>
        protected TimedKeyFrameAnimationBuilder(string property, TimeSpan? delay)
        {
            this.property = property;
            this.delay = delay;
        }

        /// <inheritdoc/>
        public ITimedKeyFrameAnimationBuilder<T> KeyFrame(
            TimeSpan progress,
            T value,
            EasingType easingType = DefaultEasingType,
            EasingMode easingMode = DefaultEasingMode)
        {
            this.keyFrames.Add(new(progress, value, easingType, easingMode));

            return this;
        }

        /// <summary>
        /// The abstracted info for a timed animation keyframe.
        /// </summary>
        protected readonly struct KeyFrameInfo : IKeyFrameInfo
        {
            /// <summary>
            /// The progress for the keyframe.
            /// </summary>
            private readonly TimeSpan progress;

            /// <summary>
            /// The value for the current keyframe.
            /// </summary>
            private readonly T value;

            /// <summary>
            /// Initializes a new instance of the <see cref="KeyFrameInfo"/> struct.
            /// </summary>
            /// <param name="progress">The progress for the keyframe.</param>
            /// <param name="value">The value for the new keyframe to add.</param>
            /// <param name="easingType">The easing type to use to reach the new keyframe.</param>
            /// <param name="easingMode">The easing mode to use to reach the new keyframe.</param>
            public KeyFrameInfo(
                TimeSpan progress,
                T value,
                EasingType easingType,
                EasingMode easingMode)
            {
                this.progress = progress;
                this.value = value;

                EasingType = easingType;
                EasingMode = easingMode;
            }

            /// <inheritdoc/>
            public EasingType EasingType { get; }

            /// <inheritdoc/>
            public EasingMode EasingMode { get; }

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TValue GetValueAs<TValue>()
                where TValue : unmanaged
            {
                return Unsafe.As<T, TValue>(ref Unsafe.AsRef(in this.value));
            }

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public double GetNormalizedProgress(TimeSpan duration)
            {
                return Math.Clamp(this.progress.TotalMilliseconds * 100 / duration.TotalMilliseconds, 0, 1);
            }

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TimeSpan GetTimedProgress(TimeSpan duration)
            {
                return this.progress;
            }
        }
    }
}