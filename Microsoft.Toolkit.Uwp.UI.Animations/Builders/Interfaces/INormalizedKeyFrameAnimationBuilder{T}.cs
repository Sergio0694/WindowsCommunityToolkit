// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Windows.UI.Xaml.Media.Animation;
using static Microsoft.Toolkit.Uwp.UI.Animations.Extensions.AnimationExtensions;

namespace Microsoft.Toolkit.Uwp.UI.Animations
{
    /// <summary>
    /// An interface for an animation builder using normalized keyframes.
    /// </summary>
    /// <typeparam name="T">The type of values being set by the animation being constructed.</typeparam>
    public interface INormalizedKeyFrameAnimationBuilder<T>
        where T : unmanaged
    {
        /// <summary>
        /// Adds a new normalized keyframe to the builder in use.
        /// </summary>
        /// <param name="progress">The normalized progress for the keyframe (must be in the [0, 1] range).</param>
        /// <param name="value">The value for the new keyframe to add.</param>
        /// <param name="easingType">The easing type to use to reach the new keyframe.</param>
        /// <param name="easingMode">The easing mode to use to reach the new keyframe.</param>
        /// <returns>The same <see cref="INormalizedKeyFrameAnimationBuilder{T}"/> instance that the method was invoked upon.</returns>
        public INormalizedKeyFrameAnimationBuilder<T> KeyFrame(
            double progress,
            T value,
            EasingType easingType = DefaultEasingType,
            EasingMode easingMode = DefaultEasingMode);
    }
}