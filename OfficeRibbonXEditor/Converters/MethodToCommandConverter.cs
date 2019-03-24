// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MethodToCommandConverter.cs" company="FA">
//   Fernando Andreu
// </copyright>
// <summary>
//   Defines the MethodToCommandConverter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace OfficeRibbonXEditor.Converters
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Windows.Data;
    using System.Windows.Input;

    /// <summary>
    /// This converter takes a method name as an argument and returns an ICommand with a call to it. This a bit hacky,
    /// but useful to bind commands to methods.
    /// As to why you would want to do that: look at the cut, copy etc. commands from the Scintilla editor. The ViewModel
    /// hardly knows about the editor; only the view does. Hence, having to route these commands through the ViewModel
    /// seems like an overkill. Besides, those editor methods are unlikely to change their names anytime soon, so
    /// there should not be issues due to refactoring not renaming the ConverterParameter as well.
    /// </summary>
    [ValueConversion(typeof(object), typeof(ICommand))]
    public class MethodToCommandConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var methodName = parameter as string;
            if (value == null || methodName == null)
            {
                Debug.WriteLine("Either the path or the ConverterParameter of MethodToCommandConverter are null");
                return value;
            }

            var methodInfo = value.GetType().GetMethod(methodName, new Type[0]);
            if (methodInfo == null)
            {
                Debug.WriteLine($"Method '{methodName}' not found for object of type {value.GetType()}");
                return value;
            }

            return new MethodProxy(methodInfo, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException($"{nameof(MethodToCommandConverter)} can only be used OneWay.");
        }

        /// <summary>
        /// Tailored implementation of ICommand so that it holds the reference to the method and the sender. This
        /// avoids possible capture scope issues in lambdas
        /// </summary>
        private class MethodProxy : ICommand
        {
            private readonly MethodInfo method;

            private readonly object sender;

            public MethodProxy(MethodInfo method, object sender)
            {
                this.method = method;
                this.sender = sender;
            }

            /// <summary>
            /// The CanExecute property of this command will never change, but we still need
            /// to define the corresponding event. The explicit add / remove declaration
            /// avoids wasting the space of the backing field while suppressing warning C0067.
            /// Do not throw an exception in the add block, because WPF will automatically
            /// subscribe to it anyway.
            /// </summary>
            public event EventHandler CanExecuteChanged
            {
                add { }
                remove { }
            }

            public bool CanExecute(object parameter)
            {
                return this.sender != null;
            }

            public void Execute(object parameter)
            {
                this.method.Invoke(this.sender, new object[0]);
            }
        }
    }
}
