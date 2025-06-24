using System.Windows;

namespace OfficeRibbonXEditor.Interfaces;

public interface IMessageBoxService
{
    MessageBoxResult Show(string text, string caption, MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None);
}