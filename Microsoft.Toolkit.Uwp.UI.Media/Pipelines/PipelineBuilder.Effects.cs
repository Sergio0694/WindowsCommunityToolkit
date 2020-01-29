﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas.Effects;
using Windows.Graphics.Effects;
using Windows.UI;
using Windows.UI.Composition;
using Microsoft.Toolkit.Uwp.UI.Media.Extensions;

namespace Microsoft.Toolkit.Uwp.UI.Media.Pipelines
{
    /// <summary>
    /// A <see langword="class"/> that allows to build custom effects pipelines and create <see cref="CompositionBrush"/> instances from them
    /// </summary>
    public sealed partial class PipelineBuilder
    {
        /// <summary>
        /// Blends two pipelines using a <see cref="BlendEffect"/> instance with the specified mode
        /// </summary>
        /// <param name="pipeline">The second <see cref="PipelineBuilder"/> instance to blend</param>
        /// <param name="mode">The desired <see cref="BlendEffectMode"/> to use to blend the input pipelines</param>
        /// <param name="placement">The placemeht to use with the two input pipelines</param>
        /// <returns>A new <see cref="PipelineBuilder"/> instance to use to keep adding new effects</returns>
        [Pure]
        public PipelineBuilder Blend(PipelineBuilder pipeline, BlendEffectMode mode, Placement placement = Placement.Foreground)
        {
            PipelineBuilder foreground, background;
            if (placement == Placement.Foreground)
            {
                foreground = this;
                background = pipeline;
            }
            else
            {
                foreground = pipeline;
                background = this;
            }

            async Task<IGraphicsEffectSource> Factory() => new BlendEffect
            {
                Foreground = await foreground.sourceProducer(),
                Background = await background.sourceProducer(),
                Mode = mode
            };

            return new PipelineBuilder(Factory, foreground, background);
        }

        /// <summary>
        /// Blends two pipelines using an <see cref="CrossFadeEffect"/> instance
        /// </summary>
        /// <param name="pipeline">The second <see cref="PipelineBuilder"/> instance to blend</param>
        /// <param name="factor">The cross fade factor to blend the input effects</param>
        /// <param name="placement">The placement to use with the two input pipelines</param>
        /// <returns>A new <see cref="PipelineBuilder"/> instance to use to keep adding new effects</returns>
        [Pure]
        public PipelineBuilder Mix(PipelineBuilder pipeline, float factor = 0.5f, Placement placement = Placement.Foreground)
        {
            if (factor < 0 || factor > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(factor), "The factor must be in the [0,1] range");
            }

            PipelineBuilder foreground, background;
            if (placement == Placement.Foreground)
            {
                foreground = this;
                background = pipeline;
            }
            else
            {
                foreground = pipeline;
                background = this;
            }

            async Task<IGraphicsEffectSource> Factory() => new CrossFadeEffect
            {
                CrossFade = factor,
                Source1 = await foreground.sourceProducer(),
                Source2 = await background.sourceProducer()
            };

            return new PipelineBuilder(Factory, foreground, background);
        }

        /// <summary>
        /// Blends two pipelines using an <see cref="CrossFadeEffect"/> instance
        /// </summary>
        /// <param name="pipeline">The second <see cref="PipelineBuilder"/> instance to blend</param>
        /// <param name="factor">The cross fade factor to blend the input effects</param>
        /// <param name="animation">The optional blur animation for the effect</param>
        /// <param name="placement">The placement to use with the two input pipelines</param>
        /// <remarks>Note that each pipeline can only contain a single instance of any of the built-in effects with animation support</remarks>
        /// <returns>A new <see cref="PipelineBuilder"/> instance to use to keep adding new effects</returns>
        [Pure]
        public PipelineBuilder Mix(PipelineBuilder pipeline, float factor, out EffectAnimation animation, Placement placement = Placement.Foreground)
        {
            if (factor < 0 || factor > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(factor), "The factor must be in the [0,1] range");
            }

            PipelineBuilder foreground, background;
            if (placement == Placement.Foreground)
            {
                foreground = this;
                background = pipeline;
            }
            else
            {
                foreground = pipeline;
                background = this;
            }

            async Task<IGraphicsEffectSource> Factory() => new CrossFadeEffect
            {
                CrossFade = factor,
                Source1 = await foreground.sourceProducer(),
                Source2 = await background.sourceProducer(),
                Name = "Fade"
            };

            animation = (brush, value, ms) =>
            {
                if (value < 0 || value > 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "The factor must be in the [0,1] range");
                }

                return brush.StartAnimationAsync("Fade.CrossFade", value, TimeSpan.FromMilliseconds(ms));
            };

            return new PipelineBuilder(Factory, foreground, background, new[] { "Fade.CrossFade" });
        }

        /// <summary>
        /// Blends two pipelines using the provided <see cref="Func{T1, T2, TResult}"/> to do so
        /// </summary>
        /// <param name="factory">The blend function to use</param>
        /// <param name="background">The background pipeline to blend with the current instance</param>
        /// <param name="animations">The list of optional animatable properties in the returned effect</param>
        /// <param name="initializers">The list of source parameters that require deferred initialization (see <see cref="CompositionEffectSourceParameter"/> for more info)</param>
        /// <returns>A new <see cref="PipelineBuilder"/> instance to use to keep adding new effects</returns>
        [Pure]
        public PipelineBuilder Merge(
            Func<IGraphicsEffectSource, IGraphicsEffectSource, IGraphicsEffectSource> factory,
            PipelineBuilder background,
            IEnumerable<string> animations = null,
            IEnumerable<BrushProvider> initializers = null)
        {
            async Task<IGraphicsEffectSource> Factory() => factory(await this.sourceProducer(), await background.sourceProducer());

            return new PipelineBuilder(Factory, this, background, animations?.ToArray(), initializers?.ToDictionary(item => item.Name, item => item.Initializer));
        }

        /// <summary>
        /// Blends two pipelines using the provided asynchronous <see cref="Func{T1, T2, TResult}"/> to do so
        /// </summary>
        /// <param name="factory">The asynchronous blend function to use</param>
        /// <param name="background">The background pipeline to blend with the current instance</param>
        /// <param name="animations">The list of optional animatable properties in the returned effect</param>
        /// <param name="initializers">The list of source parameters that require deferred initialization (see <see cref="CompositionEffectSourceParameter"/> for more info)</param>
        /// <returns>A new <see cref="PipelineBuilder"/> instance to use to keep adding new effects</returns>
        [Pure]
        public PipelineBuilder Merge(
            Func<IGraphicsEffectSource, IGraphicsEffectSource, Task<IGraphicsEffectSource>> factory,
            PipelineBuilder background,
            IEnumerable<string> animations = null,
            IEnumerable<BrushProvider> initializers = null)
        {
            async Task<IGraphicsEffectSource> Factory() => await factory(await this.sourceProducer(), await background.sourceProducer());

            return new PipelineBuilder(Factory, this, background, animations?.ToArray(), initializers?.ToDictionary(item => item.Name, item => item.Initializer));
        }

        /// <summary>
        /// Adds a new <see cref="GaussianBlurEffect"/> to the current pipeline
        /// </summary>
        /// <param name="blur">The blur amount to apply</param>
        /// <param name="mode">The <see cref="EffectBorderMode"/> parameter for the effect, defaults to <see cref="EffectBorderMode.Hard"/></param>
        /// <param name="optimization">The <see cref="EffectOptimization"/> parameter to use, defaults to <see cref="EffectOptimization.Balanced"/></param>
        /// <returns>A new <see cref="PipelineBuilder"/> instance to use to keep adding new effects</returns>
        [Pure]
        public PipelineBuilder Blur(float blur, EffectBorderMode mode = EffectBorderMode.Hard, EffectOptimization optimization = EffectOptimization.Balanced)
        {
            async Task<IGraphicsEffectSource> Factory() => new GaussianBlurEffect
            {
                BlurAmount = blur,
                BorderMode = mode,
                Optimization = optimization,
                Source = await this.sourceProducer()
            };

            return new PipelineBuilder(this, Factory);
        }

        /// <summary>
        /// Adds a new <see cref="GaussianBlurEffect"/> to the current pipeline
        /// </summary>
        /// <param name="blur">The initial blur amount</param>
        /// <param name="animation">The optional blur animation for the effect</param>
        /// <param name="mode">The <see cref="EffectBorderMode"/> parameter for the effect, defaults to <see cref="EffectBorderMode.Hard"/></param>
        /// <param name="optimization">The <see cref="EffectOptimization"/> parameter to use, defaults to <see cref="EffectOptimization.Balanced"/></param>
        /// <remarks>Note that each pipeline can only contain a single instance of any of the built-in effects with animation support</remarks>
        /// <returns>A new <see cref="PipelineBuilder"/> instance to use to keep adding new effects</returns>
        [Pure]
        public PipelineBuilder Blur(float blur, out EffectAnimation animation, EffectBorderMode mode = EffectBorderMode.Hard, EffectOptimization optimization = EffectOptimization.Balanced)
        {
            async Task<IGraphicsEffectSource> Factory() => new GaussianBlurEffect
            {
                BlurAmount = blur,
                BorderMode = mode,
                Optimization = optimization,
                Source = await this.sourceProducer(),
                Name = "Blur"
            };

            animation = (brush, value, ms) => brush.StartAnimationAsync("Blur.BlurAmount", value, TimeSpan.FromMilliseconds(ms));

            return new PipelineBuilder(this, Factory, new[] { "Blur.BlurAmount" });
        }

        /// <summary>
        /// Adds a new <see cref="SaturationEffect"/> to the current pipeline
        /// </summary>
        /// <param name="saturation">The saturation amount for the new effect</param>
        /// <returns>A new <see cref="PipelineBuilder"/> instance to use to keep adding new effects</returns>
        [Pure]
        public PipelineBuilder Saturation(float saturation)
        {
            if (saturation < 0 || saturation > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(saturation), "The saturation must be in the [0,1] range");
            }

            async Task<IGraphicsEffectSource> Factory() => new SaturationEffect
            {
                Saturation = saturation,
                Source = await this.sourceProducer()
            };

            return new PipelineBuilder(this, Factory);
        }

        /// <summary>
        /// Adds a new <see cref="SaturationEffect"/> to the current pipeline
        /// </summary>
        /// <param name="saturation">The initial saturation amount for the new effect</param>
        /// <param name="animation">The optional saturation animation for the effect</param>
        /// <remarks>Note that each pipeline can only contain a single instance of any of the built-in effects with animation support</remarks>
        /// <returns>A new <see cref="PipelineBuilder"/> instance to use to keep adding new effects</returns>
        [Pure]
        public PipelineBuilder Saturation(float saturation, out EffectAnimation animation)
        {
            if (saturation < 0 || saturation > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(saturation), "The saturation must be in the [0,1] range");
            }

            async Task<IGraphicsEffectSource> Factory() => new SaturationEffect
            {
                Saturation = saturation,
                Source = await this.sourceProducer(),
                Name = "Saturation"
            };

            animation = (brush, value, ms) =>
            {
                if (value < 0 || value > 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "The saturation must be in the [0,1] range");
                }

                return brush.StartAnimationAsync("Saturation.Saturation", value, TimeSpan.FromMilliseconds(ms));
            };

            return new PipelineBuilder(this, Factory, new[] { "Saturation.Saturation" });
        }

        /// <summary>
        /// Adds a new <see cref="OpacityEffect"/> to the current pipeline
        /// </summary>
        /// <param name="opacity">The opacity value to apply to the pipeline</param>
        /// <returns>A new <see cref="PipelineBuilder"/> instance to use to keep adding new effects</returns>
        [Pure]
        public PipelineBuilder Opacity(float opacity)
        {
            if (opacity < 0 || opacity > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(opacity), "The opacity must be in the [0,1] range");
            }

            async Task<IGraphicsEffectSource> Factory() => new OpacityEffect
            {
                Opacity = opacity,
                Source = await this.sourceProducer()
            };

            return new PipelineBuilder(this, Factory);
        }

        /// <summary>
        /// Adds a new <see cref="OpacityEffect"/> to the current pipeline
        /// </summary>
        /// <param name="opacity">The opacity value to apply to the pipeline</param>
        /// <param name="animation">The optional opacity animation for the effect</param>
        /// <remarks>Note that each pipeline can only contain a single instance of any of the built-in effects with animation support</remarks>
        /// <returns>A new <see cref="PipelineBuilder"/> instance to use to keep adding new effects</returns>
        [Pure]
        public PipelineBuilder Opacity(float opacity, out EffectAnimation animation)
        {
            if (opacity < 0 || opacity > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(opacity), "The opacity must be in the [0,1] range");
            }

            async Task<IGraphicsEffectSource> Factory() => new OpacityEffect
            {
                Opacity = opacity,
                Source = await this.sourceProducer(),
                Name = "Opacity"
            };

            animation = (brush, value, ms) =>
            {
                if (value < 0 || value > 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "The opacity must be in the [0,1] range");
                }

                return brush.StartAnimationAsync("Opacity.Opacity", value, TimeSpan.FromMilliseconds(ms));
            };

            return new PipelineBuilder(this, Factory, new[] { "Opacity.Opacity" });
        }

        /// <summary>
        /// Applies a tint color on the current pipeline
        /// </summary>
        /// <param name="color">The tint color to use</param>
        /// <param name="mix">The amount of tint to apply over the current effect</param>
        /// <returns>A new <see cref="PipelineBuilder"/> instance to use to keep adding new effects</returns>
        [Pure]
        public PipelineBuilder Tint(Color color, float mix)
        {
            return FromColor(color).Mix(this, mix);
        }

        /// <summary>
        /// Applies a tint color on the current pipeline
        /// </summary>
        /// <param name="color">The tint color to use</param>
        /// <param name="mix">The initial amount of tint to apply over the current effect</param>
        /// <param name="animation">The optional tint animation for the effect</param>
        /// <remarks>Note that each pipeline can only contain a single instance of any of the built-in effects with animation support</remarks>
        /// <returns>A new <see cref="PipelineBuilder"/> instance to use to keep adding new effects</returns>
        [Pure]
        public PipelineBuilder Tint(Color color, float mix, out EffectAnimation animation)
        {
            return FromColor(color).Mix(this, mix, out animation);
        }

        /// <summary>
        /// Applies a custom effect to the current pipeline
        /// </summary>
        /// <param name="factory">A <see cref="Func{T, TResult}"/> that takes the current <see cref="IGraphicsEffectSource"/> instance and produces a new effect to display</param>
        /// <param name="animations">The list of optional animatable properties in the returned effect</param>
        /// <param name="initializers">The list of source parameters that require deferred initialization (see <see cref="CompositionEffectSourceParameter"/> for more info)</param>
        /// <returns>A new <see cref="PipelineBuilder"/> instance to use to keep adding new effects</returns>
        [Pure]
        public PipelineBuilder Effect(
            Func<IGraphicsEffectSource, IGraphicsEffectSource> factory,
            IEnumerable<string> animations = null,
            IEnumerable<BrushProvider> initializers = null)
        {
            async Task<IGraphicsEffectSource> Factory() => factory(await this.sourceProducer());

            return new PipelineBuilder(this, Factory, animations?.ToArray(), initializers?.ToDictionary(item => item.Name, item => item.Initializer));
        }

        /// <summary>
        /// Applies a custom effect to the current pipeline
        /// </summary>
        /// <param name="factory">An asynchronous <see cref="Func{T, TResult}"/> that takes the current <see cref="IGraphicsEffectSource"/> instance and produces a new effect to display</param>
        /// <param name="animations">The list of optional animatable properties in the returned effect</param>
        /// <param name="initializers">The list of source parameters that require deferred initialization (see <see cref="CompositionEffectSourceParameter"/> for more info)</param>
        /// <returns>A new <see cref="PipelineBuilder"/> instance to use to keep adding new effects</returns>
        [Pure]
        public PipelineBuilder Effect(
            Func<IGraphicsEffectSource, Task<IGraphicsEffectSource>> factory,
            IEnumerable<string> animations = null,
            IEnumerable<BrushProvider> initializers = null)
        {
            async Task<IGraphicsEffectSource> Factory() => await factory(await this.sourceProducer());

            return new PipelineBuilder(this, Factory, animations?.ToArray(), initializers?.ToDictionary(item => item.Name, item => item.Initializer));
        }
    }
}
