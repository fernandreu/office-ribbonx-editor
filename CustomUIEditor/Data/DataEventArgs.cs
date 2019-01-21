// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataEventArgs.cs" company="FA">
//   Fernando Andreu
// </copyright>
// <summary>
// An EventArgs with a Data property of any type
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CustomUIEditor.Data
{
    using System;

    /// <inheritdoc />
    /// <summary>
    /// An EventArgs with a Data property of any type
    /// </summary>
    /// <typeparam name="T">The type of the Data property</typeparam>
    public class DataEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Gets or sets the data passed by the event
        /// </summary>
        public T Data { get; set; }
    }
}
