// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PropertyChangedExtension.cs" company="FA">
//   Fernando Andreu
// </copyright>
// <summary>
//   Defines several extension methods that simplify the applicaiton of INotifyPropertyChanged interface
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CustomUIEditor
{
    using System.Collections.Generic;
    using System.ComponentModel;

    public static class PropertyChangedExtension
    {
        public static void OnPropertyChanged(this INotifyPropertyChanged entity, string propertyName, PropertyChangedEventHandler handler)
        {
            handler?.Invoke(entity, new PropertyChangedEventArgs(propertyName));
        }
        
        public static bool SetField<T>(this INotifyPropertyChanged entity, ref T field, T value, string propertyName, PropertyChangedEventHandler handler)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            entity.OnPropertyChanged(propertyName, handler);
            return true;
        }
    }
}
