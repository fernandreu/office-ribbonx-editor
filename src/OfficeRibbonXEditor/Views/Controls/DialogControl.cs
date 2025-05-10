using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using OfficeRibbonXEditor.Views.Dialogs;

namespace OfficeRibbonXEditor.Views.Controls;

public class DialogControl : UserControl
{
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title),
        typeof(string),
        typeof(DialogControl));

    public string Title
    {
        get => (string) GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
        
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon),
        typeof(ImageSource),
        typeof(DialogControl));

    public ImageSource Icon
    {
        get => (ImageSource) GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly DependencyProperty PreferredWidthProperty = DependencyProperty.Register(
        nameof(PreferredWidth),
        typeof(double),
        typeof(DialogControl));

    public double PreferredWidth
    {
        get => (double) GetValue(PreferredWidthProperty);
        set => SetValue(PreferredWidthProperty, value);
    }

    public static readonly DependencyProperty PreferredHeightProperty = DependencyProperty.Register(
        nameof(PreferredHeight),
        typeof(double),
        typeof(DialogControl));

    public double PreferredHeight
    {
        get => (double) GetValue(PreferredHeightProperty);
        set => SetValue(PreferredHeightProperty, value);
    }

    public static readonly DependencyProperty SizeToContentProperty = DependencyProperty.Register(
        nameof(SizeToContent),
        typeof(SizeToContent),
        typeof(DialogControl),
        new FrameworkPropertyMetadata(SizeToContent.WidthAndHeight));

    public SizeToContent SizeToContent
    {
        get => (SizeToContent) GetValue(SizeToContentProperty);
        set => SetValue(SizeToContentProperty, value);
    }

    public static readonly DependencyProperty ResizeModeProperty = DependencyProperty.Register(
        nameof(ResizeMode),
        typeof(ResizeMode),
        typeof(DialogControl),
        new FrameworkPropertyMetadata(ResizeMode.NoResize));

    public ResizeMode ResizeMode
    {
        get => (ResizeMode) GetValue(ResizeModeProperty);
        set => SetValue(ResizeModeProperty, value);
    }

    public static readonly DependencyProperty InactiveOpacityProperty = DependencyProperty.Register(
        nameof(InactiveOpacity),
        typeof(double),
        typeof(DialogControl),
        new FrameworkPropertyMetadata(1.0));

    public double InactiveOpacity
    {
        get => (double) GetValue(InactiveOpacityProperty);
        set => SetValue(InactiveOpacityProperty, value);
    }

    public DialogHostBase? Host { get; set; }
}