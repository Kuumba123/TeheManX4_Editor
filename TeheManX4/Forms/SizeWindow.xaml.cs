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

            int player = -1;
            if (!Level.zeroFlag && Level.playerArcs[0] != null && !MainWindow.settings.ultimate)
                player = 0;
            else if (!Level.zeroFlag && Level.playerArcs[2] != null)
                player = 2;
            else if (Level.zeroFlag && Level.playerArcs[1] != null)
                player = 1;

            //Setup Labels
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

                if (e.type == 0)
                {
                    size += PSX.levels[Level.Id].screenData.Length * 2;
                }
                else if (e.type == 1)
                {
                    size += PSX.levels[Level.Id].tileInfo.Length;
                }
                else if (e.type == 13)
                    size += PSX.levels[Level.Id].clutAnime.Length;
                else
                    size += e.data.Length;
            }
            cpuSizeLbl.Content = "Size in CPU RAM: " + size.ToString("X");

            int player = -1;
            //Use Player File to Get Start Address
            if (!Level.zeroFlag && Level.playerArcs[0] != null && !MainWindow.settings.ultimate)
                player = 0;
            else if (!Level.zeroFlag && Level.playerArcs[2] != null)
                player = 2;
            else if (Level.zeroFlag && Level.playerArcs[1] != null)
                player = 1;

            size += 0x1000;
            if (player != -1)
            {
                foreach (var e in Level.playerArcs[player].entries)
                {
                    if ((e.type >> 16) != 0)
                        continue;
                    size += e.data.Length;
                }
            }
            if(player == -1)
            {
                if (arcSizeLbl.Foreground != System.Windows.Media.Brushes.Red)
                    arcSizeLbl.Foreground = System.Windows.Media.Brushes.Red;
            }
            arcSizeLbl.Content = "Size in Arc Buffer: " + size.ToString("X");
        }
        private void UpdateSize2()
        {
            if (screenInt.Value == null || tileInt.Value == null) return;

            int size = 0;
            foreach (var e in PSX.levels[Level.Id].arc.entries)
            {
                if ((e.type >> 0x10) != 0)
                    continue;

                if (e.type == 0)
                    size += (int)screenInt.Value * 0x200 * 2;
                else if (e.type == 1)
                {
                    size += (int)tileInt.Value * 4;
                }
                else if (e.type == 13)
                    size += PSX.levels[Level.Id].clutAnime.Length;
                else
                    size += e.data.Length;
            }
            cpuSizeLbl.Content = "Size in CPU RAM: " + size.ToString("X");

            int player = -1;
            //Use Player File to Get Start Address
            if (!Level.zeroFlag && Level.playerArcs[0] != null && !MainWindow.settings.ultimate)
                player = 0;
            else if (!Level.zeroFlag && Level.playerArcs[2] != null)
                player = 2;
            else if (Level.zeroFlag && Level.playerArcs[1] != null)
                player = 1;

            size += 0x1000;
            if (player != -1)
            {
                foreach (var e in Level.playerArcs[player].entries)
                {
                    if ((e.type >> 16) != 0)
                        continue;
                    size += e.data.Length;
                }
            }
            if (player == -1)
            {
                if (arcSizeLbl.Foreground != System.Windows.Media.Brushes.Red)
                    arcSizeLbl.Foreground = System.Windows.Media.Brushes.Red;
            }
            arcSizeLbl.Content = "Size in Arc Buffer: " + size.ToString("X");
        }
        #endregion Methoids

        #region Events
        private void Int_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!enable) return;
            change = true;
            UpdateSize2();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateSize();
        }
        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (change)
            {
                if (screenInt.Value == null || tileInt.Value == null || widthInt.Value == null || heightInt.Value == null) return;

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

                //Clear Non Existing Tiles
                int maxTileId = (int)tileInt.Value - 1;
                for (int s = 0; s < screenInt.Value; s++)
                {
                    for (int t = 0; t < 0x100; t++)
                    {
                        int index = s * 0x200 + t * 2;
                        ushort id = (ushort)(BitConverter.ToUInt16(PSX.levels[Level.Id].screenData, index) & 0x3FFF);

                        if(id > maxTileId)
                        {
                            PSX.levels[Level.Id].screenData[index] = 0;
                            PSX.levels[Level.Id].screenData[index + 1] = 0;
                        }
                    }
                }

                PSX.levels[Level.Id].edit = true;
                PSX.edit = true;
                Undo.ClearLevelUndos();
                MainWindow.window.Update();
            }
            Close();
        }
        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            HelpWindow h = new HelpWindow(5);
            h.ShowDialog();
        }
        #endregion Events
    }
}
