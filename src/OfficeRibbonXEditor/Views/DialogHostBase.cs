using System;
using System.Windows;
using Autofac;
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
            if (!(sender is DialogHost host))
            {
                return;
            }

            if (e.NewValue == null)
            {
                host.View = null;
                return;
            }

            var view = GenerateControl(e.NewValue.GetType());
            view.DataContext = e.NewValue;
            host.View = view;
            view.Host = host;
            host.Content = view;
            host.SizeToContent = SizeToContent.Manual;
            // TODO: This does not work as expected in WPF (as opposed to Windows Forms), so disabled for now
            // The main reason is that the window can only be made transparent if it has WindowStyle set to None,
            // which means the dialog will need its own border, close controls, etc, and would lose the look and
            // feel of the main window.
            ////host.Deactivated += (o, args) => host.Opacity = view.InactiveOpacity;
            ////host.Activated += (o, args) => host.Opacity = 1.0;

            // What follows is done here and not in XAML bindings because there can be sizing issues otherwise

            if (view.PreferredWidth > 0)
            {
                host.Width = view.PreferredWidth;
            }

            if (view.PreferredHeight > 0)
            {
                host.Height = view.PreferredHeight;
            }

            host.SizeToContent = view.SizeToContent;
            host.CenterInOwner();
        }

        public static void RegisterDialogViewModels(ContainerBuilder builder)
        {
            builder.RegisterType<DialogHostViewModel>();
            builder.RegisterType<SettingsDialogViewModel>();
            builder.RegisterType<AboutDialogViewModel>();
            builder.RegisterType<CallbackDialogViewModel>();
            builder.RegisterType<GoToDialogViewModel>();

            // Using a singleton for this one ensures that the search criteria is preserved, which is especially
            // important for find next / previous commands
            builder.RegisterType<FindReplaceDialogViewModel>().SingleInstance();
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

            if (contentDialogType == typeof(GoToDialogViewModel))
            {
                return new GoToDialog();
            }

            if (contentDialogType == typeof(FindReplaceDialogViewModel))
            {
                return new FindReplaceDialog();
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
