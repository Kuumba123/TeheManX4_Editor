using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace TeheManX4.Forms
{
    /// <summary>
    /// Interaction logic for GameSettingsWindow.xaml
    /// </summary>
    public partial class GameSettingsWindow : Window
    {
        #region CONSTANTS
        readonly uint[] SongLengthsAddresses = new uint[]
        {
            0x800f188c,0x800f18ac,0x800f18cc,0x800f18ec,0x800f1908,0x800f1920,0x800f1940,0x800f1960,0x800f1980,0x800f19a0,0x800f19c0
        };
        #endregion CONSTANTS

        #region Properties
        private bool enable = false;
        private BinaryWriter bw;
        private BinaryReader br;
        private MemoryStream ms;
        private string path;
        #endregion Properties

        #region Constructors
        public GameSettingsWindow(string path)
        {
            InitializeComponent();
            ms = new MemoryStream(File.ReadAllBytes(path));
            bw = new BinaryWriter(ms);
            br = new BinaryReader(ms);
            this.path = path;

            //Setup General UP/DOWN
            foreach (var c in mainPannel.Children)
            {
                if (c.GetType() != typeof(Expander))
                    continue;
                if (((Expander)c).Name == "songExpand" || ((Expander)c).Name == "songLengthExpand")
                    continue;
                foreach (var c2 in ((StackPanel)((Expander)c).Content).Children)
                {
                    foreach (var c3 in ((StackPanel)c2).Children)
                    {

                        if (c3.GetType() != typeof(NumInt))
                            continue;

                        NumInt t = (NumInt)c3;
                        var words = t.Uid.Split();
                        uint addr = Convert.ToUInt32(words[0], 16);
                        int type = 0;
                        if (words.Length != 1)
                            type = Convert.ToInt32(words[1]);
                        switch (type)
                        {
                            case 1:
                                br.BaseStream.Position = PSX.CpuToOffset(addr, 0x80010000);
                                t.Value = br.ReadByte();
                                break;
                            case 2:
                                br.BaseStream.Position = PSX.CpuToOffset(addr, 0x80010000);
                                t.Value = br.ReadUInt16();
                                break;
                            default:
                                bw.BaseStream.Position = PSX.CpuToOffset(addr, 0x80010000);
                                t.Value = br.Read();
                                break;
                        }
                    }
                }
            }
            //Setup Song Lengths
            br.BaseStream.Position = PSX.CpuToOffset(SongLengthsAddresses[0], 0x80010000);
            songFileInt.Value = 0;
            songChannelInt.Value = 0;
            songStartInt.Value = br.ReadUInt16();
            songLengthInt.Value = br.ReadUInt16();

            //Setup Stage Song Settings
            uint address = 0x800f1a0c;

            if ((bool)midCheck.IsChecked)
                address += 4;
            if ((bool)zeroCheck.IsChecked)
                address += 2;
            stageSongIdInt.Value = 0;
            br.BaseStream.Position = PSX.CpuToOffset(address, 0x80010000);
            songInt.Value = br.ReadByte();
            volInt.Value = br.ReadByte();
            enable = true;
        }
        #endregion Constructors

        #region Events
        private void IntegerUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!enable || e.NewValue == null || e.OldValue == null) return;
            int type = 0;
            var words = ((Control)sender).Uid.Split();
            uint addr = Convert.ToUInt32(words[0], 16);
            if (words.Length != 1)
                type = Convert.ToInt32(words[1]);
            switch (type)
            {
                case 1:
                    bw.BaseStream.Position = PSX.CpuToOffset(addr, 0x80010000);
                    bw.Write((byte)(int)e.NewValue);
                    break;
                case 2:
                    bw.BaseStream.Position = PSX.CpuToOffset(addr, 0x80010000);
                    bw.Write((ushort)(int)e.NewValue);
                    break;
                default:
                    bw.BaseStream.Position = PSX.CpuToOffset(addr, 0x80010000);
                    bw.Write((int)e.NewValue);
                    break;
            }
        }
        private void stageSongIdInt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!enable || e.NewValue == null || e.OldValue == null) return;
            uint address = 0x800f1a0c;
            address += (uint)((int)e.NewValue * 8);

            if ((bool)midCheck.IsChecked)
                address += 4;
            if ((bool)zeroCheck.IsChecked)
                address += 2;

            br.BaseStream.Position = PSX.CpuToOffset(address, 0x80010000);
            songInt.Value = br.ReadByte();
            volInt.Value = br.ReadByte();
        }
        private void songInt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!enable || e.NewValue == null || e.OldValue == null) return;
            uint address = 0x800f1a0c;
            address += (uint)((int)stageSongIdInt.Value * 8);

            if ((bool)midCheck.IsChecked)
                address += 4;
            if ((bool)zeroCheck.IsChecked)
                address += 2;

            bw.BaseStream.Position = PSX.CpuToOffset(address, 0x80010000);
            bw.Write((byte)(int)e.NewValue);
        }
        private void volInt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!enable || e.NewValue == null || e.OldValue == null) return;
            uint address = 0x800f1a0c;
            address += (uint)((int)stageSongIdInt.Value * 8);

            if ((bool)midCheck.IsChecked)
                address += 4;
            if ((bool)zeroCheck.IsChecked)
                address += 2;

            bw.BaseStream.Position = PSX.CpuToOffset(address + 1, 0x80010000);
            bw.Write((byte)(int)e.NewValue);
        }
        private void midCheck_CheckChange(object sender, RoutedEventArgs e)
        {
            if (!enable) return;

            uint address = 0x800f1a0c;
            address += (uint)((int)stageSongIdInt.Value * 8);

            if ((bool)midCheck.IsChecked)
                address += 4;
            if ((bool)zeroCheck.IsChecked)
                address += 2;

            br.BaseStream.Position = PSX.CpuToOffset(address, 0x80010000);
            songInt.Value = br.ReadByte();
            volInt.Value = br.ReadByte();
        }

        private void zeroCheckChange(object sender, RoutedEventArgs e)
        {
            uint address = 0x800f1a0c;
            address += (uint)((int)stageSongIdInt.Value * 8);

            if ((bool)midCheck.IsChecked)
                address += 4;
            if ((bool)zeroCheck.IsChecked)
                address += 2;

            br.BaseStream.Position = PSX.CpuToOffset(address, 0x80010000);
            songInt.Value = br.ReadByte();
            volInt.Value = br.ReadByte();
        }
        private void SaveAsBtn_Click(object sender, RoutedEventArgs e)
        {
            using (var fd = new System.Windows.Forms.SaveFileDialog())
            {
                fd.Filter = "PSX-EXE |*61";
                fd.Title = "Select PSX-EXE Save Location";
                if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    path = fd.FileName;
                    try
                    {
                        var fs = File.Create(path);
                        ms.WriteTo(fs);
                        fs.Close();
                        MessageBox.Show("Parameters Saved!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "ERROR");
                    }
                }
            }
        }
        private void songFileInt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!enable || e.NewValue == null || e.OldValue == null) return;

            if ((int)e.NewValue == 3)
                songChannelInt.Maximum = 6;
            else if ((int)e.NewValue == 4)
                songChannelInt.Maximum = 5;
            else
                songChannelInt.Maximum = 7;

            if (songChannelInt.Value > songChannelInt.Maximum)
                songChannelInt.Value = songChannelInt.Maximum;

            br.BaseStream.Position = PSX.CpuToOffset((uint)(SongLengthsAddresses[(int)e.NewValue] + (int)songChannelInt.Value * 4), 0x80010000);

            songStartInt.Value = br.ReadUInt16();
            songLengthInt.Value = br.ReadUInt16();
        }

        private void songChannelInt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!enable || e.NewValue == null || e.OldValue == null) return;

            br.BaseStream.Position = PSX.CpuToOffset((uint)(SongLengthsAddresses[(int)songFileInt.Value] + (int)songChannelInt.Value * 4), 0x80010000);

            songStartInt.Value = br.ReadUInt16();
            songLengthInt.Value = br.ReadUInt16();
        }

        private void songLengthInt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!enable || e.NewValue == null || e.OldValue == null) return;

            bw.BaseStream.Position = PSX.CpuToOffset((uint)(SongLengthsAddresses[(int)songFileInt.Value] + (int)songChannelInt.Value * 4), 0x80010000) + 2;
            bw.Write((ushort)(int)e.NewValue);
        }

        private void songStartInt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!enable || e.NewValue == null || e.OldValue == null) return;

            bw.BaseStream.Position = PSX.CpuToOffset((uint)(SongLengthsAddresses[(int)songFileInt.Value] + (int)songChannelInt.Value * 4), 0x80010000);
            bw.Write((ushort)(int)e.NewValue);
        }
        #endregion Events
    }
}
