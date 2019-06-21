using System;
using System.Diagnostics;
using System.Windows;
using OfficeRibbonXEditor.Controls;
using OfficeRibbonXEditor.Interfaces;
using OfficeRibbonXEditor.ViewModels;

namespace OfficeRibbonXEditor.Views
{
    public class DialogHostBase : Window
    {
        public static readonly DependencyProperty ViewProperty = DependencyProperty.Register(
            nameof(View),
            typeof(DialogControl),
            typeof(DialogHostBase));

        public DialogControl View
        {
            get => (DialogControl) this.GetValue(ViewProperty);
            set => this.SetValue(ViewProperty, value);
        }

        public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(
            nameof(Model),
            typeof(IContentDialogBase),
            typeof(DialogHostBase),
            new FrameworkPropertyMetadata(OnModelChanged));

        public IContentDialogBase Model
        {
            get => (IContentDialogBase) this.GetValue(ModelProperty);
            set => this.SetValue(ModelProperty, value);
        }

        private static void OnModelChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(sender is DialogHost control))
            {
                return;
            }

            if (e.NewValue == null)
            {
                control.View = null;
                return;
            }

            var view = GenerateControl(e.NewValue.GetType());
            view.DataContext = e.NewValue;
            control.View = view;
            control.Content = view;
            control.SizeToContent = SizeToContent.Manual;

            // What follows is done here and not in XAML bindings because there can be sizing issues otherwise

            if (view.PreferredWidth > 0)
            {
                control.Width = view.PreferredWidth;
            }

            if (view.PreferredHeight > 0)
            {
                control.Height = view.PreferredHeight;
            }

            control.SizeToContent = view.SizeToContent;
            control.CenterInOwner();
        }

        private static DialogControl GenerateControl(Type contentDialogType)
        {
            if (contentDialogType == typeof(AboutDialogViewModel))
            {
                return new AboutDialog();
            }

            if (contentDialogType == typeof(CallbackDialogViewModel))
            {
                return new CallbackDialog();
            }

            if (contentDialogType == typeof(SettingsDialogViewModel))
            {
                return new SettingsDialog();
            }

            throw new ArgumentException($"Type {contentDialogType.Name} does not have an registered control");
        }

        private void CenterInOwner()
        {
            if (this.Owner == null)
            {
                return;
            }

            this.Left = this.Owner.Left - (this.Width - this.Owner.Width) / 2;
            this.Top = this.Owner.Top - (this.Height - this.Owner.Height) / 2;
        }
    }
}
