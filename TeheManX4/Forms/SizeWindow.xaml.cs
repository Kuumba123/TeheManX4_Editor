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
        bool clutChange;
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
            if (PSX.levels[Level.Id].GetIndex() < 26)
                enemyCountLbl.Content = "Total Enemies: " + PSX.levels[Level.Id].enemies.Count.ToString();

            //Setup other Ints
            screenInt.Value = PSX.levels[Level.Id].screenData.Length / 0x200;
            tileInt.Value = PSX.levels[Level.Id].tileInfo.Length / 4;
            widthInt.Value = PSX.levels[Level.Id].width;
            heightInt.Value = PSX.levels[Level.Id].height;

            if (!Level.zeroFlag && PSX.levels[Level.Id].clut_X != null)
                clutInt.Value = PSX.levels[Level.Id].clut_X.entries[0].data.Length / 32;
            else if (Level.zeroFlag && PSX.levels[Level.Id].clut_Z != null)
                clutInt.Value = PSX.levels[Level.Id].clut_Z.entries[0].data.Length / 32;

            if (!Level.textureSupport)
            {
                clutInt.Visibility = Visibility.Collapsed;
                setLbl.Visibility = Visibility.Collapsed;
            }
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

            if (!Level.zeroFlag && PSX.levels[Level.Id].clut_X != null)
                size += PSX.levels[Level.Id].clut_X.entries[0].data.Length;
            else if (Level.zeroFlag && PSX.levels[Level.Id].clut_Z != null)
                size += PSX.levels[Level.Id].clut_Z.entries[0].data.Length;

            cpuSizeLbl.Content = "Size in CPU RAM: " + size.ToString("X");

            int player = -1;
            //Use Player File to Get Start Address
            if (!Level.zeroFlag && Level.playerArcs[0] != null && !MainWindow.settings.ultimate)
                player = 0;
            else if (!Level.zeroFlag && Level.playerArcs[2] != null)
                player = 2;
            else if (Level.zeroFlag && Level.playerArcs[1] != null)
                player = 1;

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
            if (clutInt.Value == null && clutInt.Visibility != Visibility.Visible) return;

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

            if(clutInt.Value != null)
                size += (int)clutInt.Value * 32;

            cpuSizeLbl.Content = "Size in CPU RAM: " + size.ToString("X");

            int player = -1;
            //Use Player File to Get Start Address
            if (!Level.zeroFlag && Level.playerArcs[0] != null && !MainWindow.settings.ultimate)
                player = 0;
            else if (!Level.zeroFlag && Level.playerArcs[2] != null)
                player = 2;
            else if (Level.zeroFlag && Level.playerArcs[1] != null)
                player = 1;


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
        private void ClutInt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!enable) return;
            clutChange = true;
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

                Level.ClearInvalidTiles();

                PSX.levels[Level.Id].edit = true;
                PSX.edit = true;
                Undo.ClearLevelUndos();
            }
            if (clutChange && clutInt.Value != null)
            {
                ClutEditor.clut = 0;
                if (PSX.levels[Level.Id].clut_X != null)
                    Array.Resize(ref PSX.levels[Level.Id].clut_X.entries[0].data, (int)clutInt.Value * 32);
                if (PSX.levels[Level.Id].clut_Z != null)
                    Array.Resize(ref PSX.levels[Level.Id].clut_Z.entries[0].data, (int)clutInt.Value * 32);
                PSX.levels[Level.Id].megaEdit = true;
                PSX.levels[Level.Id].zeroEdit = true;
            }

            if(clutChange || change)
                MainWindow.window.Update();

            //Completly Done
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
