using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Input;
using System.Runtime.CompilerServices;

namespace WildText.src.ViewModel
{
    partial class ViewModel : INotifyPropertyChanged
    {
        public ICommand OnRequestOpenFile
        {
            get { return new RelayCommand(x => OpenFromDialog(), x => true); }
        }
        public ICommand OnRequestCloseFile
        {
            get { return new RelayCommand(x => Close(), x => MessageManager != null); }
        }
        /*
        public ICommand OnRequestSaveFile
        {
            get { return new RelayCommand(x => Save(), x => MessageManager != null); }
        }
        public ICommand OnRequestSaveAs
        {
            get { return new RelayCommand(x => SaveAs(), x => MessageManager != null); }
        }
        public ICommand OnRequestAddMessage
        {
            get { return new RelayCommand(x => AddMessage(), x => MessageManager != null); }
        }
        public ICommand OnRequestRemoveMessage
        {
            get { return new RelayCommand(x => RemoveMessage(), x => MessageManager != null); }
        }
        public ICommand OnRequestAddControl
        {
            get { return new RelayCommand(x => InsertControlCode((string)x), x => SelectedMessage != null); }
        }*/
    }
}
