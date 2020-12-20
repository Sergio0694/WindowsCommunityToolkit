﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Windows.UI.Xaml.Media.Animation;
using static Microsoft.Toolkit.Uwp.UI.Animations.Extensions.AnimationExtensions;

namespace Microsoft.Toolkit.Uwp.UI.Animations.Xaml
{
    /// <summary>
    /// An opacity animation working on the composition or XAML layer.
    /// This animation maps to <see cref="AnimationBuilder.Opacity(double?, double, TimeSpan?, TimeSpan, EasingType, EasingMode, FrameworkLayer)"/>.
    /// </summary>
    public class OpacityAnimation : TypedAnimation<double>, ITimeline
    {
        /// <summary>
        /// Gets or sets the target framework layer to animate.
        /// </summary>
        public FrameworkLayer Layer { get; set; }

        /// <inheritdoc/>
        AnimationBuilder ITimeline.AppendToBuilder(AnimationBuilder builder, TimeSpan? delayHint, TimeSpan? durationHint, EasingType? easingTypeHint, EasingMode? easingModeHint)
        {
            return builder.Opacity(
                From,
                To,
                Delay ?? delayHint,
                Duration ?? durationHint.GetValueOrDefault(),
                EasingType ?? easingTypeHint ?? DefaultEasingType,
                EasingMode ?? easingModeHint ?? DefaultEasingMode,
                Layer);
        }
    }
}