using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TeheManX4.Forms
{
    /// <summary>
    /// Interaction logic for AnimeEditor.xaml
    /// </summary>
    public partial class AnimeEditor : UserControl
    {
        #region Fields
        public static int clut;
        public static int copyId = -1;
        #endregion Fields

        #region Constructors
        public AnimeEditor()
        {
            InitializeComponent();
        }
        #endregion Constructors

        #region Methoids
        public void AssignLimits()
        {
            int index = PSX.levels[Level.Id].GetIndex();
            if (PSX.levels[Level.Id].clutAnime == null || index > 26) //Disable
            {
                MainWindow.window.animeE.animeInt.IsEnabled = false;
                MainWindow.window.animeE.setInt.IsEnabled = false;
                MainWindow.window.animeE.timerint.IsEnabled = false;
                MainWindow.window.animeE.destInt.IsEnabled = false;
                MainWindow.window.animeE.lengthInt.IsEnabled = false;
                MainWindow.window.animeE.frameInt.IsEnabled = false;
            }
            else
            {
                if (!MainWindow.window.animeE.animeInt.IsEnabled)
                {
                    MainWindow.window.animeE.animeInt.IsEnabled = true;
                    MainWindow.window.animeE.setInt.IsEnabled = true;
                    MainWindow.window.animeE.timerint.IsEnabled = true;
                    MainWindow.window.animeE.destInt.IsEnabled = true;
                    MainWindow.window.animeE.lengthInt.IsEnabled = true;
                    MainWindow.window.animeE.frameInt.IsEnabled = true;

                }
                MainWindow.window.animeE.animeInt.Maximum = Const.MaxClutAnimes[index];
                copyId = -1;
                if (MainWindow.window.animeE.animeInt.Value > Const.MaxClutAnimes[index])
                    MainWindow.window.animeE.animeInt.Value = Const.MaxClutAnimes[index];
                MainWindow.window.animeE.setInt.Maximum = PSX.levels[Level.Id].clutAnime.Length / 32;
                AssignClutFrameSettings();
                AssignClutDumpSettings();
            }
            DrawClut();
        }
        private void AssignClutFrameSettings()
        {
            int index = PSX.levels[Level.Id].GetIndex();

            uint address = BitConverter.ToUInt32(PSX.exe, PSX.CpuToOffset((uint)(Const.ClutInfoPointersAddress + (index * 4))));
            uint pointersAddress = BitConverter.ToUInt32(PSX.exe, PSX.CpuToOffset((uint)(address + (int)animeInt.Value * 4)));

            //Determine Max Anime Frame
            int max;
            if (MainWindow.window.animeE.animeInt.Value != Const.MaxClutAnimes[index])
                max = (int)((BitConverter.ToUInt32(PSX.exe, PSX.CpuToOffset((uint)(address + 4 + (int)animeInt.Value * 4))) - pointersAddress) / 2);
            else
                max = (int)((address - pointersAddress) / 2);
            max--;

            if (MainWindow.window.animeE.frameInt.Maximum != max)
                MainWindow.window.animeE.frameInt.Maximum = max;

            if (MainWindow.window.animeE.frameInt.Value > max)
                MainWindow.window.animeE.frameInt.Value = max;

            int offset = PSX.CpuToOffset((uint)(pointersAddress + frameInt.Value * 2));

            MainWindow.window.animeE.setInt.Value = PSX.exe[offset];
            MainWindow.window.animeE.timerint.Value = PSX.exe[offset + 1];


        }
        private void AssignClutDumpSettings()
        {
            int index = PSX.levels[Level.Id].GetIndex();

            uint address = BitConverter.ToUInt32(PSX.exe, PSX.CpuToOffset((uint)(Const.ClutDestPointersAddress + (index * 4))));
            int offset = PSX.CpuToOffset(address);
            offset += (int)animeInt.Value * 2;
            MainWindow.window.animeE.destInt.Value = PSX.exe[offset];
            MainWindow.window.animeE.lengthInt.Value = PSX.exe[offset + 1];
        }
        public void DrawClut()
        {
            if(PSX.levels[Level.Id].clutAnime != null)
            {
                int setCount = PSX.levels[Level.Id].clutAnime.Length / 32;


                while (MainWindow.window.animeE.clutGrid.RowDefinitions.Count != setCount)
                {
                    if(MainWindow.window.animeE.clutGrid.RowDefinitions.Count > setCount)
                    {
                        MainWindow.window.animeE.clutGrid.RowDefinitions.RemoveAt(0);
                        for (int i = 0; i < 16; i++)
                            MainWindow.window.animeE.clutGrid.Children.RemoveAt(0);
                    }
                    else
                    {
                        MainWindow.window.animeE.clutGrid.RowDefinitions.Add(new RowDefinition());
                        for (int i = 0; i < 16; i++)
                        {
                            Rectangle r = new Rectangle();
                            r.Focusable = false;
                            r.Width = 16;
                            r.Height = 16;
                            r.MouseDown += Color_Down;
                            r.HorizontalAlignment = HorizontalAlignment.Stretch;
                            r.VerticalAlignment = VerticalAlignment.Stretch;
                            MainWindow.window.animeE.clutGrid.Children.Add(r);
                        }
                    }
                }
                if (MainWindow.window.animeE.clutGrid.Visibility != Visibility.Visible)
                    MainWindow.window.animeE.clutGrid.Visibility = Visibility.Visible;

                for (int y = 0; y < setCount; y++)
                {
                    for (int x = 0; x < 16; x++)
                    {
                        //Create Color
                        Rectangle r = MainWindow.window.animeE.clutGrid.Children[x + y * 16] as Rectangle;

                        ushort val = BitConverter.ToUInt16(PSX.levels[Level.Id].clutAnime, (x + y * 16) * 2);
                        int rgb32 = Level.To32Rgb(val);

                        r.Fill = new SolidColorBrush(Color.FromRgb((byte)(rgb32 >> 16), (byte)((rgb32 >> 8) & 0xFF), (byte)(rgb32 & 0xFF)));

                        Grid.SetRow(r, y);
                        Grid.SetColumn(r, x);
                    }
                }
                MainWindow.window.animeE.canvas.Height = setCount * 16;
                MainWindow.window.animeE.clutGrid.Height = setCount * 16;
                int cord = (int)(Canvas.GetTop(MainWindow.window.animeE.cursor) / 16);
                if (cord > setCount)
                {
                    clut = setCount - 1;
                    UpdateClutTxt();
                }
            }
            else //Disable
            {
                MainWindow.window.animeE.clutGrid.Visibility = Visibility.Collapsed;
            }
        }
        public void UpdateClutTxt() //also update Cursor
        {
            MainWindow.window.animeE.clutTxt.Text = "CLUT: " + Convert.ToString(clut, 16).ToUpper().PadLeft(2, '0'); //Update Txt
            Canvas.SetTop(MainWindow.window.animeE.cursor, clut * 16);
        }
        private bool ContainsAnime()
        {
            if (PSX.levels[Level.Id].clutAnime == null)
            {
                MessageBox.Show("There is on Clut Anime in this Level");
                return false;
            }
            return true;
        }
        #endregion Methoids

        #region Events
        private void Color_Down(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Right) //Change Color
            {
                //Get Current Color
                var c = Grid.GetColumn(sender as UIElement);
                var r = Grid.GetRow(sender as UIElement);
                ushort oldC = BitConverter.ToUInt16(PSX.levels[Level.Id].clutAnime, (c + r * 16) * 2);

                var d = new ColorDialog(oldC, c, r);
                d.ShowDialog();
                if (d.confirm)
                {
                    ushort newC = (ushort)Level.To15Rgb(d.canvas.SelectedColor.Value.B, d.canvas.SelectedColor.Value.G, d.canvas.SelectedColor.Value.R);

                    if(newC != oldC)
                    {
                        BitConverter.GetBytes(newC).CopyTo(PSX.levels[Level.Id].clutAnime, (c + r * 16) * 2);
                        PSX.levels[Level.Id].edit = true;

                        //Convert & Change Clut in GUI
                        int rgb32 = Level.To32Rgb(newC);
                        ((Rectangle)sender).Fill = new SolidColorBrush(Color.FromRgb((byte)(rgb32 >> 16), (byte)((rgb32 >> 8) & 0xFF), (byte)(rgb32 & 0xFF)));
                    }
                }
            }
            else //Change selected Clut
            {
                clut = Grid.GetRow(sender as UIElement);
                UpdateClutTxt();
            }
        }
        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            if (ContainsAnime())
                copyId = clut;
        }

        private void PasteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ContainsAnime() && copyId != -1)
            {
                Array.Copy(PSX.levels[Level.Id].clutAnime, copyId * 32, PSX.levels[Level.Id].clutAnime, clut * 32, 32);
                PSX.levels[Level.Id].edit = true;
                DrawClut();
            }
        }

        private void GearBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ContainsAnime())
            {
                ListWindow.isAnime = true;
                ListWindow l = new ListWindow(5);
                l.ShowDialog();
            }
        }
        private void setInt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (PSX.levels.Count != Const.FilesCount || e.NewValue == null || e.OldValue == null) return;

            int index = PSX.levels[Level.Id].GetIndex();

            uint address = BitConverter.ToUInt32(PSX.exe, PSX.CpuToOffset((uint)(Const.ClutInfoPointersAddress + (index * 4))));
            uint pointersAddress = BitConverter.ToUInt32(PSX.exe, PSX.CpuToOffset((uint)(address + animeInt.Value * 4)));

            int offset = PSX.CpuToOffset((uint)(pointersAddress + frameInt.Value * 2));

            if ((int)e.NewValue != PSX.exe[offset])
            {
                PSX.exe[offset] = (byte)(int)e.NewValue;
                PSX.edit = true;
            }
        }

        private void timerint_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (PSX.levels.Count != Const.FilesCount || e.NewValue == null || e.OldValue == null) return;

            int index = PSX.levels[Level.Id].GetIndex();

            uint address = BitConverter.ToUInt32(PSX.exe, PSX.CpuToOffset((uint)(Const.ClutInfoPointersAddress + (index * 4))));
            uint pointersAddress = BitConverter.ToUInt32(PSX.exe, PSX.CpuToOffset((uint)(address + animeInt.Value * 4)));

            int offset = PSX.CpuToOffset((uint)(pointersAddress + frameInt.Value * 2));

            if ((int)e.NewValue != PSX.exe[offset + 1])
            {
                PSX.exe[offset + 1] = (byte)(int)e.NewValue;
                PSX.edit = true;
            }
        }

        private void destInt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (PSX.levels.Count != Const.FilesCount || e.NewValue == null || e.OldValue == null) return;

            int index = PSX.levels[Level.Id].GetIndex();

            uint address = BitConverter.ToUInt32(PSX.exe, PSX.CpuToOffset((uint)(Const.ClutDestPointersAddress + (index * 4))));
            int offset = PSX.CpuToOffset(address);
            offset += (int)animeInt.Value * 2;

            if ((int)e.NewValue != PSX.exe[offset])
            {
                PSX.exe[offset] = (byte)(int)e.NewValue;
                PSX.edit = true;
            }

        }

        private void lengthInt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (PSX.levels.Count != Const.FilesCount || e.NewValue == null || e.OldValue == null) return;

            int index = PSX.levels[Level.Id].GetIndex();

            uint address = BitConverter.ToUInt32(PSX.exe, PSX.CpuToOffset((uint)(Const.ClutDestPointersAddress + (index * 4))));
            int offset = PSX.CpuToOffset(address);
            offset += (int)animeInt.Value * 2;

            if ((int)e.NewValue != PSX.exe[offset + 1])
            {
                PSX.exe[offset + 1] = (byte)(int)e.NewValue;
                PSX.edit = true;
            }
        }

        private void animeInt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (PSX.levels.Count != Const.FilesCount || e.NewValue == null || e.OldValue == null) return;
            AssignClutFrameSettings();
            AssignClutDumpSettings();
        }
        private void frameInt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (PSX.levels.Count != Const.FilesCount || e.NewValue == null || e.OldValue == null) return;
            AssignClutFrameSettings();
        }
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ContainsAnime()) return;

            int setCount = PSX.levels[Level.Id].clutAnime.Length / 32;

            if (setCount == 256)
            {
                MessageBox.Show("You can't have more than 256 Clut Anime Sets");
                return;
            }
            Array.Resize(ref PSX.levels[Level.Id].clutAnime, (setCount + 1) * 32);
            setInt.Maximum++;
            PSX.levels[Level.Id].edit = true;
            DrawClut();
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ContainsAnime()) return;


            int setCount = PSX.levels[Level.Id].clutAnime.Length / 32;

            if(setCount != 1)
            {
                Array.Resize(ref PSX.levels[Level.Id].clutAnime, (setCount - 1) * 32);
                setInt.Maximum--;
                PSX.levels[Level.Id].edit = true;

                if(clut > setCount - 2)
                {
                    clut = setCount - 2;
                    UpdateClutTxt();
                }
                if (copyId > setCount - 2)
                    copyId = -1;
                DrawClut();
            }
        }
        private void Help_Click(object sender, RoutedEventArgs e)
        {
            HelpWindow h = new HelpWindow(6);
            h.ShowDialog();
        }
        #endregion Events
    }
}
