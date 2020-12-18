﻿using Microsoft.Toolkit.Uwp.UI.Animations.Xaml;
using Microsoft.Xaml.Interactivity;

namespace Microsoft.Toolkit.Uwp.UI.Behaviors.Animations
{
    /// <summary>
    /// A custom <see cref="Trigger"/> that fires whenever a linked <see cref="AnimationCollection2"/> starts.
    /// </summary>
    public sealed class AnimationStartBehavior : Trigger<AnimationCollection2>
    {
        /// <summary>
        /// The current <see cref="AnimationCollection2"/> instance in use.
        /// </summary>
        private AnimationCollection2? animationCollection;

        /// <inheritdoc/>
        protected override void OnAttached()
        {
            base.OnAttached();

            SetResolvedCollection(AssociatedObject);
        }

        /// <inheritdoc/>
        protected override void OnDetaching()
        {
            base.OnDetaching();

            SetResolvedCollection(null);
        }

        /// <summary>
        /// Sets the current <see cref="AnimationCollection2"/> instance in use.
        /// </summary>
        /// <param name="animationCollection">The <see cref="AnimationCollection2"/> instance in use.</param>
        private void SetResolvedCollection(AnimationCollection2? animationCollection)
        {
            if (this.animationCollection == animationCollection)
            {
                return;
            }

            if (this.animationCollection is not null)
            {
                this.animationCollection.Started -= AnimationCollection_Started;
            }

            this.animationCollection = animationCollection;

            if (animationCollection is not null)
            {
                animationCollection.Started += AnimationCollection_Started;
            }
        }

        /// <summary>
        /// Invokes the current actions when the linked animations starts.
        /// </summary>
        /// <param name="sender">The source <see cref="AnimationCollection2"/> instance.</param>
        /// <param name="e">The arguments for the event (unused).</param>
        private void AnimationCollection_Started(object sender, System.EventArgs e)
        {
            Interaction.ExecuteActions(sender, Actions, e);
        }
    }
}