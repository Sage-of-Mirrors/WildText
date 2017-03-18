using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFormatReader.Common;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace WildText.src
{
    class MessageManager : INotifyPropertyChanged
    {
        struct Label
        {
            public int MessageIndex;
            public string LabelData;

            public override string ToString()
            {
                return string.Format("[{0}] {1}", MessageIndex, LabelData);
            }
        }

        List<Label> LabelData; // This holds all of the label data
        List<string> AttributeData; // This holds all of the attribute data
        List<string> TextData; // This holds all of the text data

        private int m_FileSize;
        private int m_ChunkCount;

        private ObservableCollection<Message> m_Messages;

        public ObservableCollection<Message> Messages
        {
            get { return m_Messages; }
            set
            {
                if (value != m_Messages)
                {
                    m_Messages = value;
                    NotifyPropertyChanged();
                }
            }
        } // This holds the actual messages. A message is a combo of label/attribute/text data.

        #region NotifyPropertyChanged overhead
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public MessageManager(EndianBinaryReader reader)
        {
            LabelData = new List<Label>();
            AttributeData = new List<string>();
            TextData = new List<string>();
            Messages = new ObservableCollection<Message>();

            ReadHeader(reader);

            for (int i = 0; i < m_ChunkCount; i++)
            {
                string fourCC = new string(reader.ReadChars(4));

                // This is *supposed* to be the offset of the next chunk.
                // However, the size variable within each chunk doesn't include the padding.
                // So we'll get the "calculated" size here, and later on we'll pad out that offset
                // so we get the *correct* offset for the next chunk.
                //
                // To do the calculation, we read the next chunk's size (reader.PeekReadInt32()),
                // Subtract 4 since we already advanced the stream by 4 bytes to read the chunk's fourCC,
                // Add 16/0x10 because the chunk size doesn't include the chunk's header,
                // and add the stream's current position to make it an actual offset.
                long nextChunkOffset = (reader.PeekReadInt32() - 4 + 0x10) + reader.BaseStream.Position;

                switch (fourCC)
                {
                    case "LBL1":
                        ReadLBL1(reader);
                        break;
                    case "ATR1":
                        ReadATR1(reader);
                        break;
                    case "TXT2":
                        ReadTXT2(reader);
                        break;
                }

                // As mentioned above, we need to fix the offset so it includes the padding that each chunk has at the end.
                // Each chunk is padded to the nearest 16 bytes, so that's what we pass to the padding function.
                reader.BaseStream.Position = Util.PadOffset(nextChunkOffset, 16);
            }

            for (int i = 0; i < TextData.Count; i++)
            {
                Message mes = new Message(LabelData.Find(x => x.MessageIndex == i).LabelData, AttributeData.Count != 0 ? AttributeData[i] : "", TextData[i]);
                Messages.Add(mes);
            }
        }

        #region Header
        /// <summary>
        /// Reads the header of the MSBT, which consists of the first 32/0x20 bytes of the file.
        /// </summary>
        /// <param name="reader">Stream to read from</param>
        private void ReadHeader(EndianBinaryReader reader)
        {
            bool isMstbFile = CheckFileID(reader);

            if (!isMstbFile)
                throw new FormatException("Input file was not a valid MSTB!");

            // The file contains a ushort that's used to check the endian of the file stream.
            // If the ushort is 0xFEFF, the file is big endian.
            // If it's 0xFFFE, it's little endian.
            // For compatibility, we'll swap the endian of the reader to match what we get from the ushort.
            ushort endianCheck = reader.ReadUInt16();
            if (endianCheck == 0xFFFE)
                reader.CurrentEndian = Endian.Little;

            Trace.Assert(reader.ReadUInt16() == 0x0000); // Always 0?
            Trace.Assert(reader.ReadUInt16() == 0x0103); // Always 0x0103?

            Trace.Assert(reader.PeekReadUInt16() == 0x0003); // Breath of the Wild's MSBT files should only have 3 sections - LBL1, ATR1, TXT2
            m_ChunkCount = reader.ReadUInt16();

            Trace.Assert(reader.ReadUInt16() == 0x0000); // Always 0?

            m_FileSize = reader.ReadInt32();

            // The remaining 10 bytes of the header appear to be padding (all 0), but we'll check just to make sure.
            for (int i = 0; i < 10; i++)
                Trace.Assert(reader.ReadByte() == 0);
        }

        /// <summary>
        /// Determines whether the first 8 bytes match those of a proper MSTB file ("MsgStdBn").
        /// </summary>
        /// <param name="reader">File to check</param>
        /// <returns></returns>
        private bool CheckFileID(EndianBinaryReader reader)
        {
            byte[] fileIdentifierChars = new byte[8];

            for (int i = 0; i < 8; i++)
            {
                fileIdentifierChars[i] = reader.ReadByte();
            }

            string fileID = new string(Encoding.ASCII.GetChars(fileIdentifierChars));

            if (fileID == "MsgStdBn")
                return true;
            else
                return false;
        }
        #endregion

        #region Labels
        /// <summary>
        /// Reads the label data from the MSBT file.
        /// </summary>
        /// <param name="reader"></param>
        private void ReadLBL1(EndianBinaryReader reader)
        {
            // For some reason, you need to add 0x10 to this for it to reflect the actual size of the chunk.
            int lbl1Size = reader.ReadInt32() + 0x10;

            // The last 8 bytes of this 16-byte header seems to be padding (all 0), but we'll check them to be sure.
            for (int i = 0; i < 8; i++)
                Trace.Assert(reader.ReadByte() == 0);

            long baseOffset = reader.BaseStream.Position;
            int numGroups = reader.ReadInt32();

            for (int i = 0; i < numGroups; i++)
            {
                ReadLabelGroup(reader, baseOffset);
            }
        }

        /// <summary>
        /// Reads a label group from the LBL1 chunk.
        /// </summary>
        /// <param name="reader">Stream to read label group from</param>
        /// <param name="baseOffset">Base offset of the label data</param>
        private void ReadLabelGroup(EndianBinaryReader reader, long baseOffset)
        {
            int numLabels = reader.ReadInt32();
            int startOffset = reader.ReadInt32();

            long returnOffset = reader.BaseStream.Position;
            reader.BaseStream.Position = baseOffset + startOffset;

            for (int i = 0; i < numLabels; i++)
            {
                Label lab = new Label();

                byte numChars = reader.ReadByte();
                lab.LabelData = new string(reader.ReadChars(numChars));
                lab.MessageIndex = reader.ReadInt32();

                LabelData.Add(lab);
            }

            reader.BaseStream.Position = returnOffset;
        }
        #endregion

        #region Attributes
        /// <summary>
        /// Reads the attribute data from the MSBT file.
        /// </summary>
        /// <param name="reader"></param>
        private void ReadATR1(EndianBinaryReader reader)
        {
            // For some reason, you need to add 0x10 to this for it to reflect the actual size of the chunk.
            int atr1Size = reader.ReadInt32() + 0x10;

            // The last 8 bytes of this 16-byte header seems to be padding (all 0), but we'll check them to be sure.
            for (int i = 0; i < 8; i++)
                Trace.Assert(reader.ReadByte() == 0);

            long baseOffset = reader.BaseStream.Position;
            int numAttributes = reader.ReadInt32();

            if (reader.ReadInt32() == 0)
                numAttributes = 0;

            for (int i = 0; i < numAttributes; i++)
            {
                int atrOffset = reader.ReadInt32();

                long returnOffset = reader.BaseStream.Position;

                reader.BaseStream.Position = baseOffset + atrOffset;
                GetAttribute(reader);
                reader.BaseStream.Position = returnOffset;
            }
        }

        /// <summary>
        /// Reads an attribute from the ATR1 chunk.
        /// </summary>
        /// <param name="reader">Stream to read attribute from</param>
        private void GetAttribute(EndianBinaryReader reader)
        {
            // Attributes are stored as UTF-16 strings, where each char is stored in a short (two bytes).
            // We're going to read in shorts until we get a short that's just 0, which represents \n.
            // Then, we'll convert the list of shorts to a list of bytes.
            // From there we'll use the Unicode encoding to get the actual string.

            List<short> attributeChars = new List<short>();

            short testChar = reader.ReadInt16();
            while (testChar != 0)
            {
                attributeChars.Add(testChar);
                testChar = reader.ReadInt16();
            }

            byte[] byteChars = new byte[attributeChars.Count * sizeof(short)];
            Buffer.BlockCopy(attributeChars.ToArray(), 0, byteChars, 0, byteChars.Length);

            string attributeString = new string(Encoding.Unicode.GetChars(byteChars));
            AttributeData.Add(attributeString);
        }
        #endregion

        #region Text
        /// <summary>
        /// Reads the actual text data from the MSBT file.
        /// </summary>
        /// <param name="reader"></param>
        private void ReadTXT2(EndianBinaryReader reader)
        {
            // For some reason, you need to add 0x10 to this for it to reflect the actual size of the chunk.
            int txt2Size = reader.ReadInt32() + 0x10;

            // The last 8 bytes of this 16-byte header seems to be padding (all 0), but we'll check them to be sure.
            for (int i = 0; i < 8; i++)
                Trace.Assert(reader.ReadByte() == 0);

            long baseOffset = reader.BaseStream.Position;
            int numMessages = reader.ReadInt32();

            for (int i = 0; i < numMessages; i++)
            {
                int messageOffset = reader.ReadInt32();
                long returnOffset = reader.BaseStream.Position;

                reader.BaseStream.Position = baseOffset + messageOffset;
                ReadMessageText(reader);
                reader.BaseStream.Position = returnOffset;
            }
        }

        private void ReadMessageText(EndianBinaryReader reader)
        {
            List<short> messageCharsAsShorts = new List<short>();

            short testShort = reader.ReadInt16();

            while (testShort != 0)
            {
                // This is a control code!
                if (testShort == 0x000E)
                {
                    messageCharsAsShorts.AddRange(ProcControlCode(reader));
                }
                else
                    messageCharsAsShorts.Add(testShort);

                testShort = reader.ReadInt16();
            }

            byte[] messageByteArray = new byte[messageCharsAsShorts.Count * sizeof(short)];
            Buffer.BlockCopy(messageCharsAsShorts.ToArray(), 0, messageByteArray, 0, messageByteArray.Length);

            TextData.Add(Encoding.Unicode.GetString(messageByteArray));
        }

        private short[] ProcControlCode(EndianBinaryReader reader)
        {
            List<short> controlCode = new List<short>();
            controlCode.Add((short)'<');

            short primaryType = reader.ReadInt16();
            short secondaryType = reader.ReadInt16();
            short dataSize = reader.ReadInt16();

            reader.BaseStream.Position += (dataSize);
            controlCode.Add((short)'>');
            return controlCode.ToArray();
        }
        #endregion
    }
}
