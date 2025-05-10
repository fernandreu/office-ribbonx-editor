using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Input;

namespace OfficeRibbonXEditor.Converters;

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
    public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter is not string methodName)
        {
            Debug.WriteLine("Either the path or the ConverterParameter of MethodToCommandConverter are null");
            return value;
        }

        var methodInfo = value.GetType().GetMethod(methodName, Array.Empty<Type>());
        if (methodInfo == null)
        {
            Debug.WriteLine($"Method '{methodName}' not found for object of type {value.GetType()}");
            return value;
        }

        return new MethodProxy(methodInfo, value);
    }

    public object? ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new InvalidOperationException($"{nameof(MethodToCommandConverter)} can only be used OneWay.");
    }

    /// <summary>
    /// Tailored implementation of ICommand so that it holds the reference to the method and the sender. This
    /// avoids possible capture scope issues in lambdas
    /// </summary>
    private sealed class MethodProxy : ICommand
    {
        private readonly MethodInfo _method;

        private readonly object _sender;

        public MethodProxy(MethodInfo method, object sender)
        {
            _method = method;
            _sender = sender;
        }

        /// <summary>
        /// The CanExecute property of this command will never change, but we still need to define the
        /// the corresponding event. The empty add / remove is a way to tell the compiler (or the code
        /// readers) that this event is only added for the sake of the ICommand interface
        /// </summary>
        [SuppressMessage("SonarLint", "S108", Justification = "Warning is due to empty blocks. As explained above, we are doing this on purpose")]
        public event EventHandler? CanExecuteChanged
        {
            add {}
            remove {}
        }

        public bool CanExecute(object? parameter)
        {
            return _sender != null;
        }

        public void Execute(object? parameter)
        {
            _method.Invoke(_sender, Array.Empty<object>());
        }
    }
}