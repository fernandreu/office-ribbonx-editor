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
        private IconTabViewModel? viewModel;

        private Point scrollMousePoint;

        private double hOffset = 1;
        private double vOffset = 1;

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
                this.viewModel = null;
                return;
            }

            this.viewModel = model;

            // TODO: Add listeners
        }

        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.scrollMousePoint = e.GetPosition(this.ScrollViewer);
            this.hOffset = this.ScrollViewer.HorizontalOffset;
            this.vOffset = this.ScrollViewer.VerticalOffset;
            this.ScrollViewer.CaptureMouse();
        }

        private void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (this.ScrollViewer.IsMouseCaptured)
            {
                this.ScrollViewer.ScrollToHorizontalOffset(this.hOffset + (this.scrollMousePoint.X - e.GetPosition(this.ScrollViewer).X));
                this.ScrollViewer.ScrollToVerticalOffset(this.vOffset + (this.scrollMousePoint.Y - e.GetPosition(this.ScrollViewer).Y));
            }
        }

        private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.ScrollViewer.ReleaseMouseCapture();
        }

        private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            if (this.viewModel != null)
            {
                this.viewModel.Zoom += Math.Sign(e.Delta);
            }
        }
    }
}
