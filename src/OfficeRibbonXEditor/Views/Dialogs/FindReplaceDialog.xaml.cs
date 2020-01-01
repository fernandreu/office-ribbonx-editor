using System.Windows;
using OfficeRibbonXEditor.Models.Events;
using OfficeRibbonXEditor.ViewModels.Dialogs;
using OfficeRibbonXEditor.Views.Controls;

namespace OfficeRibbonXEditor.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for FindReplaceDialog.xaml
    /// </summary>
    public partial class FindReplaceDialog : DialogControl
    {
        public FindReplaceDialog()
        {
            this.InitializeComponent();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property != DataContextProperty || !(e.NewValue is FindReplaceDialogViewModel vm))
            {
                return;
            }

            vm.MoveDialogAway += this.OnMoveDialogAway;

            EditableComboBox target;
            if (vm.IsFindTabSelected)
            {
                target = this.FindBox;
            }
            else
            {
                target = this.ReplaceBox;
            }

            target.TextBox?.Focus();
            target.TextBox?.SelectAll();
        }

        private void OnMoveDialogAway(object? sender, PointEventArgs e)
        {
            if (this.Host == null)
            {
                return;
            }

            const int triggerMargin = 64;

            // First, we check if the current position is hiding the selection or not
            var current = new System.Drawing.Rectangle(
                (int) this.Host.Left - triggerMargin, 
                (int) this.Host.Top - triggerMargin,
                (int) this.Host.Width + 2 * triggerMargin, 
                (int) this.Host.Height + 2 * triggerMargin);

            if (!current.Contains(e.Data))
            {
                return;
            }

            // We simply try to put the dialog in one of the four corners of the main window, until we find one
            // which does not hide the selection
            
            const int margin = 32;
            var owner = this.Host.Owner;

            for (var i = 0; i < 2; ++i)
            {
                for (var j = 0; j < 2; ++j)
                {
                    var left = j == 0 ? owner.Left + margin : owner.Left + owner.Width - this.Host.Width - margin;
                    var top = i == 0 ? owner.Top + margin : owner.Top + owner.Height - this.Host.Height - margin;

                    var r = new System.Drawing.Rectangle(
                        (int) left, 
                        (int) top,
                        (int) this.Host.Width, 
                        (int) this.Host.Height);

                    if (r.Contains(e.Data))
                    {
                        continue;
                    }

                    this.Host.Left = left;
                    this.Host.Top = top;

                    return;
                }
            }
        }
    }
}
