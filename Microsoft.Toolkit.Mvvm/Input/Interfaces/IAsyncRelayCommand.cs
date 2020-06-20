// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.ComponentModel;
using System.Threading.Tasks;

namespace Microsoft.Toolkit.Mvvm.Input
{
    /// <summary>
    /// An interface expanding <see cref="IRelayCommand"/> to support asynchronous operations.
    /// </summary>
    public interface IAsyncRelayCommand : IRelayCommand, INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the last scheduled <see cref="Task"/>, if available.
        /// This property notifies a change when the <see cref="Task"/> completes.
        /// </summary>
        Task? ExecutionTask { get; }

        /// <summary>
        /// Provides a more specific version of <see cref="System.Windows.Input.ICommand.Execute"/>,
        /// also returning the <see cref="Task"/> representing the async operation being executed.
        /// </summary>
        /// <param name="parameter">The input parameter.</param>
        /// <returns>The <see cref="Task"/> representing the async operation being executed.</returns>
        Task ExecuteAsync(object? parameter);

        /// <summary>
        /// Cancels the current <see cref="ExecutionTask"/>, if one if running.
        /// </summary>
        public void Cancel();
    }
}
