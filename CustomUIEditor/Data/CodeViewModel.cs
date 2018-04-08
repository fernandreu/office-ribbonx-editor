using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomUIEditor.Data
{
    public class CodeViewModel : INotifyPropertyChanged
    {
        string _rawText;
        public string RawText
        {
            get => _rawText;
            set
            {
                if (value == _rawText) return;
                _rawText = value;
                OnPropertyChanged(nameof(RawText));
            }
        }
        
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion // INotifyPropertyChanged Members
    }
}
