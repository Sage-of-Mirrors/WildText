using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WildText.src
{
    class Message : INotifyPropertyChanged
    {
        #region Label
        private string m_Label;

        public string Label
        {
            get { return m_Label; }
            set
            {
                if (value != m_Label)
                {
                    m_Label = value;
                    NotifyPropertyChanged();
                }
            }
        }
        #endregion

        #region Attribute
        private string m_Attribute;

        public string Attribute
        {
            get { return m_Attribute; }
            set
            {
                if (value != m_Attribute)
                {
                    m_Attribute = value;
                    NotifyPropertyChanged();
                }
            }
        }
        #endregion

        #region Text
        private string m_Text;

        public string Text
        {
            get { return m_Text; }
            set
            {
                if (value != m_Text)
                {
                    m_Text = value;
                    NotifyPropertyChanged();
                }
            }
        }
        #endregion

        #region NotifyPropertyChanged overhead
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public Message(string label, string attribute, string text)
        {
            Label = label;
            Attribute = attribute;
            Text = text;
        }

        public override string ToString()
        {
            return string.Format("[{0}]: {1}", Label, Text);
        }
    }
}
