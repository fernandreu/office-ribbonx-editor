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
            this.InitializeComponent();
        }
        
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            base.OnPropertyChanged(args);

            if (args.Property != DataContextProperty)
            {
                return;
            }

            if (args.OldValue is IconTabViewModel previousModel)
            {
                // TODO: Remove listeners
            }

            if (!(args.NewValue is IconTabViewModel model))
            {
                this._viewModel = null;
                return;
            }

            this._viewModel = model;

            // TODO: Add listeners
        }

        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this._scrollMousePoint = e.GetPosition(this.ScrollViewer);
            this._hOffset = this.ScrollViewer.HorizontalOffset;
            this._vOffset = this.ScrollViewer.VerticalOffset;
            this.ScrollViewer.CaptureMouse();
        }

        private void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (this.ScrollViewer.IsMouseCaptured)
            {
                this.ScrollViewer.ScrollToHorizontalOffset(this._hOffset + (this._scrollMousePoint.X - e.GetPosition(this.ScrollViewer).X));
                this.ScrollViewer.ScrollToVerticalOffset(this._vOffset + (this._scrollMousePoint.Y - e.GetPosition(this.ScrollViewer).Y));
            }
        }

        private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.ScrollViewer.ReleaseMouseCapture();
        }

        private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            if (this._viewModel != null)
            {
                this._viewModel.Zoom += Math.Sign(e.Delta);
            }
        }
    }
}
