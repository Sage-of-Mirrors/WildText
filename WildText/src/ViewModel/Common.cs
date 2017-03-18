using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WildText.src.ViewModel
{
    partial class ViewModel : INotifyPropertyChanged
    {
        #region NotifyPropertyChanged overhead
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private MessageManager m_MessageManager;

        public MessageManager MessageManager
        {
            get { return m_MessageManager; }
            set
            {
                if (value != m_MessageManager)
                {
                    m_MessageManager = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private Message m_SelectedMessage;

        public Message SelectedMessage
        {
            get { return m_SelectedMessage; }
            set
            {
                if (value != m_SelectedMessage)
                {
                    m_SelectedMessage = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private string m_WindowTitle;

        public string WindowTitle
        {
            get { return m_WindowTitle + " - WildText"; }
            set
            {
                if (value != m_WindowTitle)
                {
                    m_WindowTitle = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public void Close()
        {

        }
    }
}
