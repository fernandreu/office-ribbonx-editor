// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMessageBoxService.cs" company="FA">
//   Fernando Andreu
// </copyright>
// <summary>
//   Defines the IMessageBoxService type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CustomUIEditor.Services
{
    using System.Windows;

    public interface IMessageBoxService
    {
        MessageBoxResult Show(string text, string caption, MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None);
    }
}
