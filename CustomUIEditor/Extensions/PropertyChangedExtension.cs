// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PropertyChangedExtension.cs" company="FA">
//   Fernando Andreu
// </copyright>
// <summary>
//   Defines several extension methods that simplify the application of INotifyPropertyChanged interface
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CustomUIEditor.Extensions
{
    using System.Collections.Generic;
    using System.ComponentModel;

    public static class PropertyChangedExtension
    {
        public static void OnPropertyChanged(this INotifyPropertyChanged entity, PropertyChangedEventHandler handler, string propertyName)
        {
            handler?.Invoke(entity, new PropertyChangedEventArgs(propertyName));
        }
        
        public static bool SetField<T>(this INotifyPropertyChanged entity, ref T field, T value, PropertyChangedEventHandler handler, params string[] propertyNames)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            foreach (var name in propertyNames)
            {
                entity.OnPropertyChanged(handler, name);
            }
            return true;
        }
    }
}
