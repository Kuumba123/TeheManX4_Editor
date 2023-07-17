using System;
using System.Windows;

namespace TeheManX4.Forms
{
    /// <summary>
    /// Interaction logic for SizeWindow.xaml
    /// </summary>
    public partial class SizeWindow : Window
    {

        #region Properties
        bool enable;
        bool change;
        #endregion Properties

        #region Constructors
        public SizeWindow()
        {
            InitializeComponent();

            if (ListWindow.screenViewOpen)
                MainWindow.layoutWindow.Close();
            if (ListWindow.extraOpen)
                MainWindow.extraWindow.Close();
            Title = PSX.levels[Level.Id].arc.filename + " Size Info";

            //Setup Labels
            UpdateSize();
            enemyCountLbl.Content = "Total Enemies: " + PSX.levels[Level.Id].enemies.Count.ToString();

            //Setup other Ints
            screenInt.Value = PSX.levels[Level.Id].screenData.Length / 0x200;
            tileInt.Value = PSX.levels[Level.Id].tileInfo.Length / 4;
            widthInt.Value = PSX.levels[Level.Id].width;
            heightInt.Value = PSX.levels[Level.Id].height;

            enable = true;
        }
        #endregion Constructors

        #region Methoids
        private void UpdateSize()
        {
            int size = 0;
            foreach (var e in PSX.levels[Level.Id].arc.entries)
            {
                if ((e.type >> 0x10) != 0)
                    continue;
                size += e.data.Length;
                if (e.type == 0)
                    size += e.data.Length;
            }
            cpuSizeLbl.Content = "Size in CPU RAM: " + Convert.ToString(size, 16).ToUpper();
        }
        #endregion Methoids

        #region Events
        private void Int_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!enable) return;
            change = true;
            UpdateSize();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (change)
            {
                Array.Resize(ref PSX.levels[Level.Id].screenData, (int)(screenInt.Value * 0x200));
                Array.Resize(ref PSX.levels[Level.Id].tileInfo, (int)(tileInt.Value * 4));

                int sizeNew = (int)(widthInt.Value * heightInt.Value);

                byte[] layoutNew = new byte[sizeNew * 3];
                int dumpWidth = (int)widthInt.Value;

                if (dumpWidth > PSX.levels[Level.Id].width)
                    dumpWidth = PSX.levels[Level.Id].width;

                for (int i = 0; i < 3; i++)
                {
                    for (int y = 0; y < (int)heightInt.Value; y++)
                    {
                        if (y > PSX.levels[Level.Id].height - 1) break;
                        Array.Copy(PSX.levels[Level.Id].layout, y * PSX.levels[Level.Id].width + i * PSX.levels[Level.Id].size, layoutNew, y * (int)widthInt.Value + i * sizeNew, dumpWidth);
                    }
                }


                PSX.levels[Level.Id].layout = layoutNew;
                PSX.levels[Level.Id].width = (int)widthInt.Value;
                PSX.levels[Level.Id].height = (int)heightInt.Value;
                PSX.levels[Level.Id].size = sizeNew;

                PSX.levels[Level.Id].edit = true;
                PSX.edit = true;
                Undo.ClearLevelUndos();
                MainWindow.window.Update();
            }
        }
        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            HelpWindow h = new HelpWindow(5);
            h.ShowDialog();
        }
        #endregion Events
    }
}
