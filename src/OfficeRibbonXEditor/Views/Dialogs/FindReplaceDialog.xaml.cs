using System.Windows;
using OfficeRibbonXEditor.Events;
using OfficeRibbonXEditor.Helpers;
using OfficeRibbonXEditor.ViewModels.Dialogs;
using OfficeRibbonXEditor.Views.Controls;

namespace OfficeRibbonXEditor.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for FindReplaceDialog.xaml
    /// </summary>
    [ExportView(typeof(FindReplaceDialogViewModel))]
    public partial class FindReplaceDialog : DialogControl
    {
        public FindReplaceDialog()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property != DataContextProperty || e.NewValue is not FindReplaceDialogViewModel vm)
            {
                return;
            }

            vm.MoveDialogAway += OnMoveDialogAway;

            EditableComboBox target;
            if (vm.IsFindTabSelected)
            {
                target = FindBox;
            }
            else
            {
                target = ReplaceBox;
            }

            target.TextBox?.Focus();
            target.TextBox?.SelectAll();
        }

        private void OnMoveDialogAway(object? sender, PointEventArgs e)
        {
            if (Host == null)
            {
                return;
            }

            const int triggerMargin = 64;

            // First, we check if the current position is hiding the selection or not
            var current = new System.Drawing.Rectangle(
                (int) Host.Left - triggerMargin, 
                (int) Host.Top - triggerMargin,
                (int) Host.Width + 2 * triggerMargin, 
                (int) Host.Height + 2 * triggerMargin);

            if (!current.Contains(e.Data))
            {
                return;
            }

            // We simply try to put the dialog in one of the four corners of the main window, until we find one
            // which does not hide the selection
            
            const int margin = 32;
            var owner = Host.Owner;

            for (var i = 0; i < 2; ++i)
            {
                for (var j = 0; j < 2; ++j)
                {
                    var left = j == 0 ? owner.Left + margin : owner.Left + owner.Width - Host.Width - margin;
                    var top = i == 0 ? owner.Top + margin : owner.Top + owner.Height - Host.Height - margin;

                    var r = new System.Drawing.Rectangle(
                        (int) left, 
                        (int) top,
                        (int) Host.Width, 
                        (int) Host.Height);

                    if (r.Contains(e.Data))
                    {
                        continue;
                    }

                    Host.Left = left;
                    Host.Top = top;

                    return;
                }
            }
        }
    }
}
