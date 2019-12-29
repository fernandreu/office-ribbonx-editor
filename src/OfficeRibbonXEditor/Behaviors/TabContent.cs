// TabContent.cs, version 1.2
// The code in this file is Copyright (c) Ivan Krivyakov
// See http://www.ikriv.com/legal.php for more information
//

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace OfficeRibbonXEditor.Behaviors
{
    /// <summary>
    /// Attached properties for persistent tab control
    /// </summary>
    /// <remarks>By default WPF TabControl bound to an ItemsSource destroys visual state of invisible tabs. 
    /// Set ikriv:TabContent.IsCached="True" to preserve visual state of each tab.
    /// </remarks>
    public static class TabContent
    {
        public static bool GetIsCached(DependencyObject d)
        {
            return (bool?)d?.GetValue(IsCachedProperty) ?? false;
        }

        public static void SetIsCached(DependencyObject d, bool value)
        {
            d?.SetValue(IsCachedProperty, value);
        }

        /// <summary>
        /// Controls whether tab content is cached or not
        /// </summary>
        /// <remarks>When TabContent.IsCached is true, visual state of each tab is preserved (cached), even when the tab is hidden</remarks>
        public static readonly DependencyProperty IsCachedProperty =
            DependencyProperty.RegisterAttached("IsCached", typeof(bool), typeof(TabContent), new UIPropertyMetadata(false, OnIsCachedChanged));


        public static DataTemplate GetTemplate(DependencyObject d)
        {
            return (DataTemplate)d?.GetValue(TemplateProperty);
        }

        public static void SetTemplate(DependencyObject d, DataTemplate value)
        {
            d?.SetValue(TemplateProperty, value);
        }

        /// <summary>
        /// Used instead of TabControl.ContentTemplate for cached tabs
        /// </summary>
        public static readonly DependencyProperty TemplateProperty =
            DependencyProperty.RegisterAttached("Template", typeof(DataTemplate), typeof(TabContent), new UIPropertyMetadata(null));

        public static DataTemplateSelector GetTemplateSelector(DependencyObject d)
        {
            return (DataTemplateSelector)d?.GetValue(TemplateSelectorProperty);
        }

        public static void SetTemplateSelector(DependencyObject d, DataTemplateSelector value)
        {
            d?.SetValue(TemplateSelectorProperty, value);
        }

        /// <summary>
        /// Used instead of TabControl.ContentTemplateSelector for cached tabs
        /// </summary>
        public static readonly DependencyProperty TemplateSelectorProperty =
            DependencyProperty.RegisterAttached("TemplateSelector", typeof(DataTemplateSelector), typeof(TabContent), new UIPropertyMetadata(null));

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static TabControl GetInternalTabControl(DependencyObject d)
        {
            return (TabControl)d?.GetValue(InternalTabControlProperty);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SetInternalTabControl(DependencyObject d, TabControl value)
        {
            d?.SetValue(InternalTabControlProperty, value);
        }

        // Using a DependencyProperty as the backing store for InternalTabControl.  This enables animation, styling, binding, etc...
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static readonly DependencyProperty InternalTabControlProperty =
            DependencyProperty.RegisterAttached("InternalTabControl", typeof(TabControl), typeof(TabContent), new UIPropertyMetadata(null, OnInternalTabControlChanged));

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static ContentControl GetInternalCachedContent(DependencyObject d)
        {
            return (ContentControl)d?.GetValue(InternalCachedContentProperty);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SetInternalCachedContent(DependencyObject d, ContentControl value)
        {
            d?.SetValue(InternalCachedContentProperty, value);
        }

        // Using a DependencyProperty as the backing store for InternalCachedContent.  This enables animation, styling, binding, etc...
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static readonly DependencyProperty InternalCachedContentProperty =
            DependencyProperty.RegisterAttached("InternalCachedContent", typeof(ContentControl), typeof(TabContent), new UIPropertyMetadata(null));

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static object GetInternalContentManager(DependencyObject d)
        {
            return (object)d?.GetValue(InternalContentManagerProperty);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SetInternalContentManager(DependencyObject d, object value)
        {
            d?.SetValue(InternalContentManagerProperty, value);
        }

        // Using a DependencyProperty as the backing store for InternalContentManager.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InternalContentManagerProperty =
            DependencyProperty.RegisterAttached("InternalContentManager", typeof(object), typeof(TabContent), new UIPropertyMetadata(null));

        private static void OnIsCachedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj == null) return;

            if (!(obj is TabControl tabControl))
            {
                throw new InvalidOperationException("Cannot set TabContent.IsCached on object of type " + args.NewValue.GetType().Name +
                    ". Only objects of type TabControl can have TabContent.IsCached property.");
            }

            bool newValue = (bool)args.NewValue;

            if (!newValue)
            {
                if (args.OldValue != null && ((bool)args.OldValue))
                {
                    throw new NotImplementedException("Cannot change TabContent.IsCached from True to False. Turning tab caching off is not implemented");
                }

                return;
            }

            EnsureContentTemplateIsNull(tabControl);
            tabControl.ContentTemplate = CreateContentTemplate();
            EnsureContentTemplateIsNotModified(tabControl);
        }

        private static DataTemplate CreateContentTemplate()
        {
            const string xaml = 
                "<DataTemplate><Border b:TabContent.InternalTabControl=\"{Binding RelativeSource={RelativeSource AncestorType=TabControl}}\" /></DataTemplate>";

            var context = new ParserContext();

            context.XamlTypeMapper = new XamlTypeMapper(Array.Empty<string>());
            context.XamlTypeMapper.AddMappingProcessingInstruction("b", typeof(TabContent).Namespace, typeof(TabContent).Assembly.FullName);

            context.XmlnsDictionary.Add("", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
            context.XmlnsDictionary.Add("b", "b");

            var template = (DataTemplate)XamlReader.Parse(xaml, context);
            return template;
        }

        private static void OnInternalTabControlChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj == null) return;
            var container = obj as Decorator;

            if (container == null)
            {
                var message = "Cannot set TabContent.InternalTabControl on object of type " + obj.GetType().Name +
                    ". Only controls that derive from Decorator, such as Border can have a TabContent.InternalTabControl.";
                throw new InvalidOperationException(message);
            }

            if (args.NewValue == null) return;
            if (!(args.NewValue is TabControl))
            {
                throw new InvalidOperationException("Value of TabContent.InternalTabControl cannot be of type " + args.NewValue.GetType().Name +", it must be of type TabControl");
            }

            var tabControl = (TabControl)args.NewValue;
            var contentManager = GetContentManager(tabControl, container);
            contentManager.UpdateSelectedTab();
        }

        private static ContentManager GetContentManager(TabControl tabControl, Decorator container)
        {
            var contentManager = (ContentManager)GetInternalContentManager(tabControl);
            if (contentManager != null)
            {
                /*
                 * Content manager already exists for the tab control. This means that tab content template is applied 
                 * again, and new instance of the Border control (container) has been created. The old container 
                 * referenced by the content manager is no longer visible and needs to be replaced
                 */
                contentManager.ReplaceContainer(container);
            }
            else
            {
                // create content manager for the first time
                contentManager = new ContentManager(tabControl, container);
                SetInternalContentManager(tabControl, contentManager);
            }

            return contentManager;
        }

        private static void EnsureContentTemplateIsNull(TabControl tabControl)
        {
            if (tabControl.ContentTemplate != null)
            {
                throw new InvalidOperationException("TabControl.ContentTemplate value is not null. If TabContent.IsCached is True, use TabContent.Template instead of ContentTemplate");
            }
        }

        private static void EnsureContentTemplateIsNotModified(TabControl tabControl)
        {
            var descriptor = DependencyPropertyDescriptor.FromProperty(TabControl.ContentTemplateProperty, typeof(TabControl));
            descriptor.AddValueChanged(tabControl, (sender, args) => 
                throw new InvalidOperationException("Cannot assign to TabControl.ContentTemplate when TabContent.IsCached is True. Use TabContent.Template instead"));
        }

        private class ContentManager
        {
            private readonly TabControl tabControl;

            private Decorator border;

            public ContentManager(TabControl tabControl, Decorator border)
            {
                this.tabControl = tabControl;
                this.border = border;
                this.tabControl.SelectionChanged += (sender, args) => { this.UpdateSelectedTab(); };
            }

            public void ReplaceContainer(Decorator newBorder)
            {
                if (ReferenceEquals(this.border, newBorder)) return;

                this.border.Child = null; // detach any tab content that old border may hold
                this.border = newBorder;
            }

            public void UpdateSelectedTab()
            {
                this.border.Child = this.GetCurrentContent();
            }

            private ContentControl GetCurrentContent()
            {
                var item = this.tabControl.SelectedItem;
                if (item == null) return null;

                var tabItem = this.tabControl.ItemContainerGenerator.ContainerFromItem(item);
                if (tabItem == null) return null;

                var cachedContent = TabContent.GetInternalCachedContent(tabItem);
                if (cachedContent == null)
                {
                    cachedContent = new ContentControl 
                    { 
                        DataContext = item,
                        ContentTemplate = TabContent.GetTemplate(this.tabControl), 
                        ContentTemplateSelector = TabContent.GetTemplateSelector(this.tabControl)
                    };
                
                    cachedContent.SetBinding(ContentControl.ContentProperty, new Binding());
                    TabContent.SetInternalCachedContent(tabItem, cachedContent);
                }

                return cachedContent;
            }
        }
    }
}
