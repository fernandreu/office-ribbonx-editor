using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using OfficeRibbonXEditor.Views.Dialogs;

namespace OfficeRibbonXEditor.Views.Controls
{
    public class DialogControl : UserControl
    {
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(DialogControl));

        public string Title
        {
            get => (string) this.GetValue(TitleProperty);
            set => this.SetValue(TitleProperty, value);
        }
        
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            nameof(Icon),
            typeof(ImageSource),
            typeof(DialogControl));

        public ImageSource Icon
        {
            get => (ImageSource) this.GetValue(IconProperty);
            set => this.SetValue(IconProperty, value);
        }

        public static readonly DependencyProperty PreferredWidthProperty = DependencyProperty.Register(
            nameof(PreferredWidth),
            typeof(double),
            typeof(DialogControl));

        public double PreferredWidth
        {
            get => (double) this.GetValue(PreferredWidthProperty);
            set => this.SetValue(PreferredWidthProperty, value);
        }

        public static readonly DependencyProperty PreferredHeightProperty = DependencyProperty.Register(
            nameof(PreferredHeight),
            typeof(double),
            typeof(DialogControl));

        public double PreferredHeight
        {
            get => (double) this.GetValue(PreferredHeightProperty);
            set => this.SetValue(PreferredHeightProperty, value);
        }

        public static readonly DependencyProperty SizeToContentProperty = DependencyProperty.Register(
            nameof(SizeToContent),
            typeof(SizeToContent),
            typeof(DialogControl),
            new FrameworkPropertyMetadata(SizeToContent.WidthAndHeight));

        public SizeToContent SizeToContent
        {
            get => (SizeToContent) this.GetValue(SizeToContentProperty);
            set => this.SetValue(SizeToContentProperty, value);
        }

        public static readonly DependencyProperty ResizeModeProperty = DependencyProperty.Register(
            nameof(ResizeMode),
            typeof(ResizeMode),
            typeof(DialogControl),
            new FrameworkPropertyMetadata(ResizeMode.NoResize));

        public ResizeMode ResizeMode
        {
            get => (ResizeMode) this.GetValue(ResizeModeProperty);
            set => this.SetValue(ResizeModeProperty, value);
        }

        public static readonly DependencyProperty InactiveOpacityProperty = DependencyProperty.Register(
            nameof(InactiveOpacity),
            typeof(double),
            typeof(DialogControl),
            new FrameworkPropertyMetadata(1.0));

        public double InactiveOpacity
        {
            get => (double) this.GetValue(InactiveOpacityProperty);
            set => this.SetValue(InactiveOpacityProperty, value);
        }

        public DialogHostBase Host { get; set; }
    }
}
