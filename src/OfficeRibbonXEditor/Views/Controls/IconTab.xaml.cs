using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using OfficeRibbonXEditor.ViewModels.Tabs;

namespace OfficeRibbonXEditor.Views.Controls
{
    /// <summary>
    /// Interaction logic for EditorTab.xaml
    /// </summary>
    public partial class IconTab : UserControl
    {
        private IconTabViewModel? _viewModel;

        private Point _scrollMousePoint;

        private double _hOffset = 1;
        private double _vOffset = 1;

        public IconTab()
        {
            InitializeComponent();
        }
        
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property != DataContextProperty)
            {
                return;
            }

            if (e.NewValue is not IconTabViewModel model)
            {
                _viewModel = null;
                return;
            }

            _viewModel = model;
        }

        private void OnPreviewMouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
        {
            _scrollMousePoint = e.GetPosition(ScrollViewer);
            _hOffset = ScrollViewer.HorizontalOffset;
            _vOffset = ScrollViewer.VerticalOffset;
            ScrollViewer.CaptureMouse();
        }

        private void OnPreviewMouseMove(object? sender, MouseEventArgs e)
        {
            if (ScrollViewer.IsMouseCaptured)
            {
                ScrollViewer.ScrollToHorizontalOffset(_hOffset + (_scrollMousePoint.X - e.GetPosition(ScrollViewer).X));
                ScrollViewer.ScrollToVerticalOffset(_vOffset + (_scrollMousePoint.Y - e.GetPosition(ScrollViewer).Y));
            }
        }

        private void OnPreviewMouseLeftButtonUp(object? sender, MouseButtonEventArgs e)
        {
            ScrollViewer.ReleaseMouseCapture();
        }

        private void OnPreviewMouseWheel(object? sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            if (_viewModel != null)
            {
                _viewModel.Zoom += Math.Sign(e.Delta);
            }
        }
    }
}
