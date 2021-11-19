using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using OfficeRibbonXEditor.Helpers;
using OfficeRibbonXEditor.Interfaces;
using OfficeRibbonXEditor.Views.Controls;

namespace OfficeRibbonXEditor.Views.Dialogs
{
    public class DialogHostBase : Window
    {
        private static readonly IDictionary<Type, Type> RegisteredViews = new Dictionary<Type, Type>();

        static DialogHostBase()
        {
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                var attribute = type.GetCustomAttribute<ExportViewAttribute>();
                if (attribute == null)
                {
                    continue;
                }

                RegisteredViews[attribute.ViewModelType] = type;
            }
        }

        public static readonly DependencyProperty ViewProperty = DependencyProperty.Register(
            nameof(View),
            typeof(DialogControl),
            typeof(DialogHostBase));

        public DialogControl? View
        {
            get => (DialogControl) GetValue(ViewProperty);
            set => SetValue(ViewProperty, value);
        }

        public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(
            nameof(Model),
            typeof(IContentDialogBase),
            typeof(DialogHostBase),
            new FrameworkPropertyMetadata(OnModelChanged));

        public IContentDialogBase Model
        {
            get => (IContentDialogBase) GetValue(ModelProperty);
            set => SetValue(ModelProperty, value);
        }

        private static void OnModelChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not DialogHost host)
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

        private static DialogControl GenerateControl(Type contentDialogType)
        {
            if (!RegisteredViews.TryGetValue(contentDialogType, out var viewType))
            {
                throw new ArgumentException($"Type {contentDialogType.Name} does not have a registered control");
            }

            var instance = (DialogControl?)Activator.CreateInstance(viewType);
            if (instance == null)
            {
                throw new ArgumentException($"Cannot generate view of type {viewType?.Name} for dialog view model of type {contentDialogType.Name}");
            }

            return instance;
        }

        private void CenterInOwner()
        {
            if (Owner == null)
            {
                return;
            }

            Left = Owner.Left - (Width - Owner.Width) / 2;
            Top = Owner.Top - (Height - Owner.Height) / 2;
        }
    }
}
