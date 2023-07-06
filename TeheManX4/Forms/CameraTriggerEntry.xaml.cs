using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TeheManX4.Forms
{
    /// <summary>
    /// Interaction logic for CameraTriggerEntry.xaml
    /// </summary>
    public partial class CameraTriggerEntry : UserControl
    {
        #region Properties
        internal ushort rightSide;
        internal ushort leftSide;
        internal ushort bottomSide;
        internal ushort topSide;
        internal List<ushort> settings = new List<ushort>();
        #endregion Properties

        #region Constructors
        public CameraTriggerEntry()
        {
            InitializeComponent();
            settings.Add(1);
            UpdateCountLabel();
        }
        public CameraTriggerEntry(int offset)
        {
            InitializeComponent();

            rightSide = BitConverter.ToUInt16(PSX.exe, offset);
            leftSide = BitConverter.ToUInt16(PSX.exe, offset + 2);
            bottomSide = BitConverter.ToUInt16(PSX.exe, offset + 4);
            topSide = BitConverter.ToUInt16(PSX.exe, offset + 6);
            while (true)
            {
                ushort setting = BitConverter.ToUInt16(PSX.exe, offset + 8);
                if (setting == 0) break;

                settings.Add(setting);
                offset += 2;
            }
            UpdateCountLabel();
        }
        #endregion Constructors

        #region Methoids
        public byte[] GetEntryData()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            bw.Write(rightSide);
            bw.Write(leftSide);
            bw.Write(bottomSide);
            bw.Write(topSide);

            foreach (var s in this.settings)
            {
                bw.Write(s);
            }

            bw.Write((ushort)0);
            return ms.ToArray();
        }
        private void UpdateCountLabel()
        {
            countLbl.Content = settings.Count.ToString();
        }
        #endregion Methoids

        #region Events
        private void AddSettingButton_Click(object sender, RoutedEventArgs e)
        {
            if (settings.Count == 4)
            {
                MessageBox.Show("Theres no need to add more than 4 Border Settings");
                return;
            }
            settings.Add(1);
            UpdateCountLabel();
        }
        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            var parent = VisualTreeHelper.GetParent(this);
            var items = parent as Panel;
            items.Children.Remove(this);
        }
        #endregion Events
    }
}
