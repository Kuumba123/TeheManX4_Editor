using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TeheManX4.Forms
{
    /// <summary>
    /// Interaction logic for LayoutEditor.xaml
    /// </summary>
    public partial class LayoutEditor : UserControl
    {
        #region Properties
        WriteableBitmap layoutBMP = new WriteableBitmap(768, 768, 96, 96, PixelFormats.Rgb24, null);
        WriteableBitmap selectBMP = new WriteableBitmap(256, 256, 96, 96, PixelFormats.Rgb24, null);
        public int viewerX = 0;
        public int viewerY = 0;
        public int selectedScreen = 2;
        public List<Label> screenLabels = new List<Label>();
        public Button pastLayer;
        #endregion Properties

        #region Fields
        internal static List<List<Undo>> undos = new List<List<Undo>>();
        private static bool addedLabels = false;
        #endregion Fields

        #region Constructors
        public LayoutEditor()
        {
            InitializeComponent();
            if(!addedLabels)
            {
                addedLabels = true;
                for (int y = 0; y < 3; y++)
                {
                    for (int x = 0; x < 3; x++)
                    {
                        Label l = new Label();
                        l.Foreground = Brushes.White;
                        l.FontFamily = new FontFamily("Consolas");
                        l.FontSize = 28;
                        l.Visibility = Visibility.Hidden;
                        l.VerticalAlignment = VerticalAlignment.Center;
                        l.HorizontalAlignment = HorizontalAlignment.Center;
                        Grid.SetColumn(l, x);
                        Grid.SetRow(l, y);
                        screenLabels.Add(l);
                        layoutGrid.Children.Add(l);
                    }
                }
            }
        }
        #endregion Constructors

        #region Methods
        public void DrawLayout(bool updateLbl = false)
        {
            int total = PSX.levels[Level.Id].screenData.Length / 0x200;
            IntPtr ptr = layoutBMP.BackBuffer;
            layoutBMP.Lock();
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    int offsetX = (MainWindow.window.layoutE.viewerX >> 8) + x;
                    int offsetY = (MainWindow.window.layoutE.viewerY >> 8) + y;
                    if (offsetX > PSX.levels[Level.Id].width - 1 || offsetY > PSX.levels[Level.Id].height - 1)
                        Level.DrawScreen(0, x * 256, y * 256, 2304, ptr);
                    else
                    {
                        byte screen = PSX.levels[Level.Id].layout[offsetX + offsetY * PSX.levels[Level.Id].width + PSX.levels[Level.Id].size * Level.BG];
                        Level.DrawScreen(screen, x * 256, y * 256, 2304, ptr);

                        if (updateLbl)
                        {
                            if (screen > total - 1)
                            {
                                foreach (var l in screenLabels)
                                {
                                    if (Grid.GetColumn(l) == x && Grid.GetRow(l) == y)
                                    {
                                        if (l.Visibility != Visibility.Visible)
                                            l.Visibility = Visibility.Visible;
                                        l.Content = Convert.ToString(screen, 16).ToUpper();
                                    }
                                }
                            }
                            else
                            {
                                foreach (var l in screenLabels)
                                {
                                    if (Grid.GetColumn(l) == x && Grid.GetRow(l) == y)
                                    {
                                        if (l.Visibility != Visibility.Hidden)
                                            l.Visibility = Visibility.Hidden;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            layoutBMP.AddDirtyRect(new Int32Rect(0, 0, 768, 768));
            layoutBMP.Unlock();
            MainWindow.window.layoutE.layoutImage.Source = layoutBMP;
        }
        public void DrawScreen()
        {
            selectBMP.Lock();
            Level.DrawScreen(selectedScreen, 768, selectBMP.BackBuffer);
            selectBMP.AddDirtyRect(new Int32Rect(0, 0, 256, 256));
            selectBMP.Unlock();
            MainWindow.window.layoutE.selectImage.Source = selectBMP;
        }
        public void UpdateBtn()
        {
            if (pastLayer != null)
            {
                pastLayer.Background = Brushes.Black;
                pastLayer.Foreground = Brushes.White;
            }
            if (Level.BG == 0)
            {
                btn1.Background = Brushes.LightBlue;
                btn1.Foreground = Brushes.Black;
                pastLayer = btn1;
            }
            else if (Level.BG == 1)
            {
                btn2.Background = Brushes.LightBlue;
                btn2.Foreground = Brushes.Black;
                pastLayer = btn2;
            }
            else
            {
                btn3.Background = Brushes.LightBlue;
                btn3.Foreground = Brushes.Black;
                pastLayer = btn3;
            }
        }
        public void AssignLimits()
        {
            int screenAmount = PSX.levels[Level.Id].screenData.Length / 0x200;
            screenAmount--;

            if (MainWindow.window.layoutE.viewerX >> 8 > PSX.levels[Level.Id].width - 1)
                MainWindow.window.layoutE.viewerX = (PSX.levels[Level.Id].width - 1) << 8;
            if (MainWindow.window.layoutE.viewerY >> 8 > PSX.levels[Level.Id].height - 1)
                MainWindow.window.layoutE.viewerY = (PSX.levels[Level.Id].height - 1) << 8;

            MainWindow.window.layoutE.screenInt.Maximum = screenAmount;
            if (MainWindow.window.layoutE.screenInt.Value > screenAmount)
                MainWindow.window.layoutE.screenInt.Value = screenAmount;


            DrawLayout(true);
            DrawScreen();
        }
        #endregion Methods

        #region Events
        private void layoutImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var p = e.GetPosition(layoutImage);
            int x = (int)p.X;
            int y = (int)p.Y;
            int size = PSX.levels[Level.Id].width * PSX.levels[Level.Id].height;
            int cX = Level.GetSelectedTile(x, layoutImage.ActualWidth, 3);
            int cY = Level.GetSelectedTile(y, layoutImage.ActualHeight, 3);
            int offsetX = (MainWindow.window.layoutE.viewerX >> 8) + cX;
            int offsetY = (MainWindow.window.layoutE.viewerY >> 8) + cY;
            if (offsetX > PSX.levels[Level.Id].width - 1 || offsetY > PSX.levels[Level.Id].height - 1)
            {
                MessageBox.Show("You clicked outside of the bounds of the level!");
                return;
            }
            int i = cX + (MainWindow.window.layoutE.viewerX >> 8) + ((cY + (MainWindow.window.layoutE.viewerY >> 8)) * PSX.levels[Level.Id].width) + Level.BG * size;
            if (e.ChangedButton == MouseButton.Right)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift))  //For Selecting in the Screen Tab
                {
                    MainWindow.window.screenE.screenInt.Value = PSX.levels[Level.Id].layout[i];
                    return;
                }
                selectedScreen = PSX.levels[Level.Id].layout[i];
                screenInt.Value = selectedScreen;
                DrawScreen();
            }
            else
            {
                if (Keyboard.IsKeyDown(Key.LeftShift)) //For Modifying the Clicked Screen
                {
                    ListWindow.layoutOffset = i;
                    ListWindow l = new ListWindow(1);
                    l.ShowDialog();
                    if (ListWindow.screenViewOpen)
                        MainWindow.layoutWindow.EditScreen(cX + (MainWindow.window.layoutE.viewerX >> 8), cY + (MainWindow.window.layoutE.viewerY >> 8));
                    return;
                }

                //Save Undo & Edit
                if (undos[Level.Id].Count == Const.MaxUndo)
                    undos[Level.Id].RemoveAt(0);
                undos[Level.Id].Add(Undo.CreateLayoutUndo(i));

                PSX.levels[Level.Id].layout[i] = (byte)selectedScreen;
                PSX.edit = true;
                DrawLayout(true);
                MainWindow.window.enemyE.Draw();
                if (ListWindow.screenViewOpen)
                    MainWindow.layoutWindow.EditScreen(cX + (MainWindow.window.layoutE.viewerX >> 8), cY + (MainWindow.window.layoutE.viewerY >> 8));
            }
        }
        private void IntegerUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue == null || PSX.levels.Count != Const.FilesCount)
                return;
            if (selectedScreen == (int)e.NewValue)
                return;
            selectedScreen = (int)e.NewValue;
            if ((uint)selectedScreen >= 0xEF)
                selectedScreen = 0xEF;
            DrawScreen();
        }
        private void gridBtn_Click(object sender, RoutedEventArgs e)
        {
            if (layoutGrid.ShowGridLines)
                layoutGrid.ShowGridLines = false;
            else
                layoutGrid.ShowGridLines = true;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var b = (Button)sender;
            int i = Convert.ToInt32(b.Content.ToString(), 16) - 1;
            if (Level.BG == i)
                return;
            Level.BG = i;
            if (pastLayer != null)
            {
                pastLayer.Background = Brushes.Black;
                pastLayer.Foreground = Brushes.White;
            }
            b.Background = Brushes.LightBlue;
            b.Foreground = Brushes.Black;
            pastLayer = b;
            DrawLayout();
            MainWindow.window.enemyE.Draw();
            if (MainWindow.layoutWindow != null)
                MainWindow.layoutWindow.DrawScreens();
        }
        private void ViewScreens_Click(object sender, RoutedEventArgs e)
        {
            if (ListWindow.screenViewOpen)
                return;
            MainWindow.layoutWindow = new ListWindow(0);
            MainWindow.layoutWindow.Show();
        }
        private void Help_Click(object sender, RoutedEventArgs e)
        {
            HelpWindow h = new HelpWindow(1);
            h.ShowDialog();
        }
        private void SnapButton_Click(object sender, RoutedEventArgs e)
        {
            using (var sfd = new System.Windows.Forms.SaveFileDialog())
            {
                sfd.Filter = "PNG |*.png";
                sfd.Title = "Select Level Layout Save Location";
                try
                {
                    if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        WriteableBitmap fileBmp = new WriteableBitmap(256 * PSX.levels[Level.Id].width, PSX.levels[Level.Id].height * 256, 96, 96, PixelFormats.Rgb24, null);
                        int stride = fileBmp.BackBufferStride;
                        fileBmp.Lock();
                        IntPtr ptr = fileBmp.BackBuffer;
                        for (int y = 0; y < PSX.levels[Level.Id].height; y++) //32 Screens  Tall
                        {
                            for (int x = 0; x < PSX.levels[Level.Id].width; x++) //32 Screens Wide
                            {
                                int size = PSX.levels[Level.Id].size;
                                Level.DrawScreen(PSX.levels[Level.Id].layout[(y * PSX.levels[Level.Id].width) + x + Level.BG * size], x * 256 , y * 256, stride, ptr);
                            }
                        }
                        fileBmp.AddDirtyRect(new Int32Rect(0, 0, 256 * PSX.levels[Level.Id].width, PSX.levels[Level.Id].height * 256));
                        fileBmp.Unlock();
                        PngBitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(fileBmp));
                        System.IO.FileStream fs = System.IO.File.Create(sfd.FileName);
                        encoder.Save(fs);
                        fs.Close();
                        MessageBox.Show("Layout Exported");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.window.layoutE.viewerY != 0)
            {
                MainWindow.window.layoutE.viewerY -= 0x100;
                MainWindow.window.layoutE.DrawLayout(true);
                MainWindow.window.UpdateViewrCam();
            }
        }
        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            if ((MainWindow.window.layoutE.viewerY >> 8) < (PSX.levels[Level.Id].height - 3))
            {
                MainWindow.window.layoutE.viewerY += 0x100;
                MainWindow.window.layoutE.DrawLayout(true);
                MainWindow.window.UpdateViewrCam();
            }
        }
        private void LeftButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.window.layoutE.viewerX != 0)
            {
                MainWindow.window.layoutE.viewerX -= 0x100;
                MainWindow.window.layoutE.DrawLayout(true);
                MainWindow.window.UpdateViewrCam();
            }
        }
        private void RightButton_Click(object sender, RoutedEventArgs e)
        {
            if ((MainWindow.window.layoutE.viewerX >> 8) < (PSX.levels[Level.Id].width - 3))
            {
                MainWindow.window.layoutE.viewerX += 0x100;
                MainWindow.window.layoutE.DrawLayout(true);
                MainWindow.window.Update();
            }
        }
        #endregion Events
    }
}
