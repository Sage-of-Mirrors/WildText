using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFormatReader.Common;
using System.ComponentModel;
using Microsoft.Win32;
using System.IO;

namespace WildText.src.ViewModel
{
    partial class ViewModel : INotifyPropertyChanged
    {
        public void OpenFromDialog()
        {
            OpenFileDialog dia = new OpenFileDialog();
            dia.Filter = "MSBT Files|*.msbt|All Files|*";
            dia.Title = "Open an MSBT File";

            if (dia.ShowDialog() != true)
                return;

            WindowTitle = dia.SafeFileName;

            using (FileStream stream = new FileStream(dia.FileName, FileMode.Open, FileAccess.Read))
            {
                EndianBinaryReader reader = new EndianBinaryReader(stream, Endian.Big);

                MessageManager = new MessageManager(reader);
            }

            SelectedMessage = MessageManager.Messages[0];
        }
    }
}
