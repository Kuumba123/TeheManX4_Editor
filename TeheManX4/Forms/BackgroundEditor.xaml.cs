using System;
using System.Windows.Controls;

namespace TeheManX4.Forms
{
    /// <summary>
    /// Interaction logic for BackgroundEditor.xaml
    /// </summary>
    public partial class BackgroundEditor : UserControl
    {
        #region Constructors
        public BackgroundEditor()
        {
            InitializeComponent();
        }
        #endregion Constructors

        #region Methods
        public void SetLayerSettings()
        {
            int index = PSX.levels[Level.Id].GetIndex();
            MainWindow.window.bgE.transInt.Value = PSX.exe[PSX.CpuToOffset((uint)(Const.TransSettingsAddress + index))];

            int i = PSX.levels[Level.Id].GetIndex() * 10 + PSX.CpuToOffset(Const.BackgroundSettingsAddress);

            MainWindow.window.bgE.otInt.Value = PSX.exe[i + (int)objectInt.Value];
            MainWindow.window.bgE.priorityInt.Value = PSX.exe[i + 4 + (int)layerInt.Value * 2];
            MainWindow.window.bgE.normalInt.Value = PSX.exe[i + 5 + (int)layerInt.Value * 2];

            i = PSX.levels[Level.Id].GetIndex() * 2 + PSX.CpuToOffset(Const.BackgroundTypeTableAddress);
            MainWindow.window.bgE.scroll2Int.Value = PSX.exe[i];
            MainWindow.window.bgE.scroll3Int.Value = PSX.exe[i + 1];
        }
        public void SetSlotSettings()
        {
            int index = PSX.levels[Level.Id].GetIndex();

            if(index < 26)
            {
                MainWindow.window.bgE.slotInt.Value = 0;
                SetSlotMax();
                index = PSX.levels[Level.Id].GetIndex();
                int slotsOffset = PSX.CpuToOffset(BitConverter.ToUInt32(PSX.exe, Const.ObjectSlotPointersOffset + index * 4));

                MainWindow.window.bgE.idInt.Value = PSX.exe[slotsOffset];
                MainWindow.window.bgE.slotInt.IsEnabled = true;
                MainWindow.window.bgE.idInt.IsEnabled = true;
            }
            else
            {
                MainWindow.window.bgE.slotInt.IsEnabled = false;
                MainWindow.window.bgE.idInt.IsEnabled = false;
            }
        }
        private void SetSlotMax()
        {
            int index = PSX.levels[Level.Id].GetIndex();
            int slotsOffset = PSX.CpuToOffset(BitConverter.ToUInt32(PSX.exe, Const.ObjectSlotPointersOffset + index * 4));

            int max = 0;

            while (true)
            {
                if(max > 0xFF)
                {
                    System.Windows.MessageBox.Show("Invalid Slot List for " + PSX.levels[Level.Id].arc.filename);
                    System.Windows.Application.Current.Shutdown();
                }
                if (PSX.exe[slotsOffset] == 0xFF)
                    break;
                max++;
                slotsOffset++;
            }
            MainWindow.window.bgE.slotInt.Maximum = max - 1;
        }
        #endregion Methods

        #region Events
        private void scrollInt_ValueChange(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue == null || e.OldValue == null || PSX.levels.Count != Const.FilesCount) return;
            int i = PSX.levels[Level.Id].GetIndex() * 2 + PSX.CpuToOffset(Const.BackgroundTypeTableAddress) + Convert.ToInt32(((NumInt)sender).Uid);

            if (PSX.exe[i] == (byte)(int)e.NewValue) return;
            PSX.exe[i] = (byte)(int)e.NewValue;
            PSX.edit = true;
        }
        private void layerInt_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue == null || e.OldValue == null || PSX.levels.Count != Const.FilesCount) return;
            int index = PSX.levels[Level.Id].GetIndex() * 10 + PSX.CpuToOffset(Const.BackgroundSettingsAddress) + (int)e.NewValue * 2 + 4;

            MainWindow.window.bgE.priorityInt.Value = PSX.exe[index];
            MainWindow.window.bgE.normalInt.Value = PSX.exe[index + 1];
        }
        private void LayerInfo_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue == null || e.OldValue == null || PSX.levels.Count != Const.FilesCount) return;
            int index = PSX.levels[Level.Id].GetIndex() * 10 + PSX.CpuToOffset(Const.BackgroundSettingsAddress) + (int)MainWindow.window.bgE.layerInt.Value * 2 + 4 + Convert.ToInt32(((NumInt)sender).Uid);

            if (PSX.exe[index] == (byte)(int)e.NewValue) return;

            PSX.exe[index] = (byte)(int)e.NewValue;
            PSX.edit = true;
        }
        private void otInt_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue == null || e.OldValue == null || PSX.levels.Count != Const.FilesCount) return;
            int index = (int)(PSX.levels[Level.Id].GetIndex() * 10 + PSX.CpuToOffset(Const.BackgroundSettingsAddress) + objectInt.Value);
            if(PSX.exe[index] != (int)e.NewValue)
            {
                PSX.exe[index] = (byte)(int)e.NewValue;
                PSX.edit = true;
            }
        }
        private void objectInt_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue == null || e.OldValue == null || PSX.levels.Count != Const.FilesCount) return;
            int index = PSX.levels[Level.Id].GetIndex() * 10 + PSX.CpuToOffset(Const.BackgroundSettingsAddress) + (int)e.NewValue;
            MainWindow.window.bgE.otInt.Value = PSX.exe[index];
        }
        private void slotInt_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue == null || e.OldValue == null || PSX.levels.Count != Const.FilesCount) return;
            int index = PSX.levels[Level.Id].GetIndex();
            int slotsOffset = PSX.CpuToOffset(BitConverter.ToUInt32(PSX.exe, Const.ObjectSlotPointersOffset + index * 4)) + (int)e.NewValue;

            MainWindow.window.bgE.idInt.Value = PSX.exe[slotsOffset];
        }

        private void idInt_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue == null || e.OldValue == null || PSX.levels.Count != Const.FilesCount) return;
            int index = PSX.levels[Level.Id].GetIndex();
            int slotsOffset = PSX.CpuToOffset(BitConverter.ToUInt32(PSX.exe, Const.ObjectSlotPointersOffset + index * 4)) + (int)slotInt.Value;

            if (PSX.exe[slotsOffset] == (byte)(int)e.NewValue) return;

            PSX.exe[slotsOffset] = (byte)(int)e.NewValue;
            PSX.edit = true;
        }
        private void transInt_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue == null || e.OldValue == null || PSX.levels.Count != Const.FilesCount) return;
            int index = PSX.levels[Level.Id].GetIndex();

            if (PSX.exe[PSX.CpuToOffset(Const.TransSettingsAddress) + index] == (byte)(int)e.NewValue) return;

            PSX.exe[PSX.CpuToOffset(Const.TransSettingsAddress) + index] = (byte)(int)e.NewValue;
            PSX.edit = true;
        }
        #endregion Events
    }
}
