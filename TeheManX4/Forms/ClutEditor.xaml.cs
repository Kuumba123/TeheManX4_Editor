using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TeheManX4.Forms
{
    /// <summary>
    /// Interaction logic for ClutWindow.xaml
    /// </summary>
    public partial class ClutEditor : UserControl
    {
        #region Fields
        private static bool added = false;
        public static int maxPage;
        public static int maxClut;
        public static int page = 0;
        public static int clut = 0;
        internal static int bgF = 1;
        private static Button pastPage;
        #endregion Fields

        #region Constructors
        public ClutEditor()
        {
            InitializeComponent();
            if (PSX.levels.Count != Const.FilesCount)
                return;
            AddClut();
        }
        #endregion Constructors

        #region Methods
        public void DrawTextures()
        {
            if(page > 7 && bgF == 0)
                page = 7;

            if (bgF == 1)
                clut %= maxClut;
            else
            {
                if (clut > 0x3F)
                    clut = 0x3F;
            }
            IntPtr pixelDataPtr = Level.bmp[page + bgF * 8].BackBuffer;
            BitmapPalette pal;
            int stride = 128;
            PixelFormat format = PixelFormats.Indexed4;
            if (page < 8)
                pal = Level.palette[clut + bgF * 64];
            else
            {
                List<Color> colors = new List<Color>();

                for (int i = 0; i < 256; i++)
                {
                    if (((((i >> 4) + clut) * 16) + (i & 0xF)) > 8191) break;
                    colors.Add(Level.palette[clut + 64 + (i >> 4)].Colors[i & 0xF]);
                }
                pal = new BitmapPalette(colors);
                format = PixelFormats.Indexed8;
                stride = 256;
            }

            MainWindow.window.clutE.textureImage.Source = BitmapSource.Create(256,
            256,
            96,
            96,
            format,
            pal,
            pixelDataPtr,
            256 * stride,
            stride);
        }
        public void DrawClut()
        {
            if (!added)
            {
                AddClut();
                return;
            }
            int length = AddGrid();

            while (MainWindow.window.clutE.clutGrid.Children.Count != (length * 16))
            {
                if(MainWindow.window.clutE.clutGrid.Children.Count > (length * 16))
                    MainWindow.window.clutE.clutGrid.Children.RemoveAt(clutGrid.Children.Count - 1);
                else
                {
                    int row = MainWindow.window.clutE.clutGrid.Children.Count / 16;
                    int col = MainWindow.window.clutE.clutGrid.Children.Count % 16;

                    Rectangle r = new Rectangle();
                    r.Focusable = false;
                    r.Width = 16;
                    r.Height = 16;
                    r.MouseDown += Color_Down;
                    r.HorizontalAlignment = HorizontalAlignment.Stretch;
                    r.VerticalAlignment = VerticalAlignment.Stretch;
                    Grid.SetRow(r, row);
                    Grid.SetColumn(r, col);
                    MainWindow.window.clutE.clutGrid.Children.Add(r);
                }
            }

            foreach (var p in MainWindow.window.clutE.clutGrid.Children)
            {
                var c = Grid.GetColumn(p as UIElement);
                var r = Grid.GetRow(p as UIElement);
                
                if ((r + 1) > length) break;

                var rect = (Rectangle)p;
                rect.Fill = new SolidColorBrush(Color.FromRgb(Level.palette[r + (bgF * 0x40)].Colors[c].R, Level.palette[r + (bgF * 0x40)].Colors[c].G, Level.palette[r + (bgF * 0x40)].Colors[c].B));
            }
        }
        private void AddClut()
        {
            if (added) return;

            added = true;
            AddGrid();
            for (int y = 0; y < clutGrid.RowDefinitions.Count; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    //Create Color
                    Rectangle r = new Rectangle();
                    r.Focusable = false;
                    r.Width = 16;
                    r.Height = 16;
                    r.Fill = new SolidColorBrush(Color.FromRgb(Level.palette[y + (bgF * 0x40)].Colors[x].R, Level.palette[y + (bgF * 0x40)].Colors[x].G, Level.palette[y + (bgF * 0x40)].Colors[x].B));
                    r.MouseDown += Color_Down;
                    r.HorizontalAlignment = HorizontalAlignment.Stretch;
                    r.VerticalAlignment = VerticalAlignment.Stretch;
                    Grid.SetRow(r, y);
                    Grid.SetColumn(r, x);
                    MainWindow.window.clutE.clutGrid.Children.Add(r);
                }
            }
        }
        private int AddGrid()
        {
            int length;

            if (bgF == 1)
            {
                if (!Level.zeroFlag && PSX.levels[Level.Id].clut_X != null)
                    length = PSX.levels[Level.Id].clut_X.entries[0].data.Length / 32;
                else if (Level.zeroFlag && PSX.levels[Level.Id].clut_Z != null)
                    length = PSX.levels[Level.Id].clut_Z.entries[0].data.Length / 32;
                else
                    length = 0x80;
                if (length > Level.palette.Length)
                    length = 64;
                else
                    length -= 64;
            }
            else
                length = 64;

            if(clutGrid.ColumnDefinitions.Count != 16)
            {
                for (int i = 0; i < 16; i++)
                    clutGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(16) });
            }

            while (true)
            {
                if (clutGrid.RowDefinitions.Count == length)
                    break;
                else if (clutGrid.RowDefinitions.Count > length)
                    clutGrid.RowDefinitions.RemoveAt(0);
                else
                {
                    clutGrid.RowDefinitions.Add(new RowDefinition());
                    clutGrid.RowDefinitions[clutGrid.RowDefinitions.Count - 1].Height = new GridLength(16);
                }
            }
            int newHeight = clutGrid.RowDefinitions.Count * 16;
            clutGrid.Height = newHeight;
            clutCanvas.Height = newHeight;
            if (bgF == 1)
                maxClut = length;
            return length;
        }
        public void UpdateClutTxt() //also update Cursor
        {
            MainWindow.window.clutE.palBtn.Content = "CLUT: " + Convert.ToString(clut, 16).ToUpper().PadLeft(2, '0'); //Update Txt
            Canvas.SetTop(MainWindow.window.clutE.cursor, clut * 16);
        }
        public void UpdateSelectedTexture(int type)
        {
            if (type == 0)
            {
                if (bgF == 0)
                    return;
                bgF = 0;
                if(page > 7)
                {
                    Button button = pagePannel.Children[8] as Button;
                    button.Foreground = Brushes.Black;
                    button.Background = Brushes.LightBlue;
                    pastPage.Background = Brushes.Black;
                    pastPage.Foreground = Brushes.White;
                    pastPage = button;
                }
                DrawClut();
                DrawTextures();
                UpdateClutTxt();
                textureGrid.ShowGridLines = true;
            }
            else
            {
                if (bgF == 1)
                    return;
                bgF = 1;
                DrawClut();
                DrawTextures();
                textureGrid.ShowGridLines = false;
            }
        }
        public void UpdateTpageButton(int t)
        {
            if(bgF == 1)
            {
                if (t < 0)
                    t = maxPage;
                else if (t > maxPage)
                    t = 0;
            }
            else
            {
                if (t < 0)
                    t = 7;
                else if (t > 7)
                    t = 0;
            }
            page = t;
            if (pastPage != null)
            {
                pastPage.Background = Brushes.Black;
                pastPage.Foreground = Brushes.White;
            }
            pastPage = (Button)pagePannel.Children[1 + t];
            ((Button)pagePannel.Children[1 + t]).Foreground = Brushes.Black;
            ((Button)pagePannel.Children[1 + t]).Background = Brushes.LightBlue;
            DrawTextures();
        }
        public static int GetMaxClutId() //For Num Int's in Other Tabs
        {
            int length;
            if (!Level.zeroFlag && PSX.levels[Level.Id].clut_X != null)
                length = PSX.levels[Level.Id].clut_X.entries[0].data.Length / 32;
            else if (Level.zeroFlag && PSX.levels[Level.Id].clut_Z != null)
                length = PSX.levels[Level.Id].clut_Z.entries[0].data.Length / 32;
            else
                length = 128;
            if (length > Level.palette.Length)
                length = 64;
            else
                length -= 64;
            if (length > 0xFF)
                return 0xFF;

            return length - 1;
        }
        #endregion Methods

        #region Events
        private void Color_Down(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Right) //Change Color
            {
                if (!Level.zeroFlag)
                {
                    if (PSX.levels[Level.Id].clut_X == null) return;
                }
                else
                {
                    if (PSX.levels[Level.Id].clut_Z == null) return;
                }
                //Get Current Color
                var c = Grid.GetColumn(sender as UIElement);
                var r = Grid.GetRow(sender as UIElement);
                ushort oldC;
                if (!Level.zeroFlag)
                    oldC = BitConverter.ToUInt16(PSX.levels[Level.Id].clut_X.entries[0].data, (c + (r + (bgF * 0x40)) * 16) * 2);
                else
                    oldC = BitConverter.ToUInt16(PSX.levels[Level.Id].clut_Z.entries[0].data, (c + (r + (bgF * 0x40)) * 16) * 2);

                var d = new ColorDialog(oldC, c, r);
                d.ShowDialog();
                if (d.confirm)
                {
                    ushort newC = (ushort)Level.To15Rgb(d.canvas.SelectedColor.Value.B, d.canvas.SelectedColor.Value.G, d.canvas.SelectedColor.Value.R);

                    if (newC != oldC)
                    {
                        //Edit Clut in PAC
                        if (!Level.zeroFlag)
                        {
                            BitConverter.GetBytes(newC).CopyTo(PSX.levels[Level.Id].clut_X.entries[0].data, (c + (r + (bgF * 0x40)) * 16) * 2);
                            PSX.levels[Level.Id].megaEdit = true;
                        }
                        else
                        {
                            BitConverter.GetBytes(newC).CopyTo(PSX.levels[Level.Id].clut_Z.entries[0].data, (c + (r + (bgF * 0x40)) * 16) * 2);
                            PSX.levels[Level.Id].zeroEdit = true;
                        }

                        if (MainWindow.window.x16E.page < 8 && page < 8)
                            Level.AssignPallete(r + (bgF * 0x40));
                        else
                            Level.AssignPallete();

                        //Convert & Change Clut in GUI
                        int rgb32 = Level.To32Rgb(newC);
                        ((Rectangle)sender).Fill = new SolidColorBrush(Color.FromRgb((byte)(rgb32 >> 16), (byte)((rgb32 >> 8) & 0xFF), (byte)(rgb32 & 0xFF)));

                        //Updating the rest of GUI
                        if (clut == r || page > 7)
                            DrawTextures();
                        if (bgF == 0)
                            return;
                        //Enemy Tab
                        MainWindow.window.enemyE.Draw();
                        //16x16 Tab
                        MainWindow.window.x16E.DrawTextures();
                        MainWindow.window.x16E.DrawTile();
                        MainWindow.window.x16E.DrawTiles();
                        //Screen Tab
                        MainWindow.window.screenE.DrawTile();
                        MainWindow.window.screenE.DrawScreen();
                        MainWindow.window.screenE.DrawTiles();
                        //Layout Tab
                        MainWindow.window.layoutE.DrawLayout();
                        MainWindow.window.layoutE.DrawScreen();
                    }
                }
            }
            else //Change selected Clut
            {
                clut = Grid.GetRow(sender as UIElement);
                if(clut > 0x1AF)
                    clut = 0x1AF;
                UpdateClutTxt();
                DrawTextures();
            }
        }
        private void Tpage_Click(object sender, RoutedEventArgs e)
        {
            int newP = Convert.ToInt32(((Button)sender).Content.ToString(), 16);
            if (newP == page)
                return;
            if (newP > 7 && bgF == 0)
                return;
            if (pastPage != null)
            {
                pastPage.Background = Brushes.Black;
                pastPage.Foreground = Brushes.White;
            }
            pastPage = (Button)sender;
            ((Button)sender).Foreground = Brushes.Black;
            ((Button)sender).Background = Brushes.LightBlue;
            page = newP;
            DrawTextures();
        }
        private void palBtn_Click(object sender, RoutedEventArgs e)
        {
            //...
        }
        private void GearBtn_Click(object sender, RoutedEventArgs e)
        {
            //...
            bool valid = true;
            if (!Level.zeroFlag)
            {
                if (PSX.levels[Level.Id].clut_X == null)
                    valid = false;
            }
            else
            {
                if (PSX.levels[Level.Id].clut_Z == null)
                    valid = false;
            }
            if (!valid) return;
            ListWindow.isAnime = false;
            ListWindow l = new ListWindow(5);
            l.ShowDialog();
        }
        private void BackgroudTex_Click(object sender, RoutedEventArgs e) //For Toggle Between Obj & BG
        {
            if (bgF == 1)
                return;
            bgF = 1;
            DrawClut();
            DrawTextures();
            textureGrid.ShowGridLines = false;
        }

        private void ObjectTex_Click(object sender, RoutedEventArgs e)
        {
            if (bgF == 0)
                return;
            bgF = 0;
            if (page > 7)
            {
                Button button = pagePannel.Children[8] as Button;
                button.Foreground = Brushes.Black;
                button.Background = Brushes.LightBlue;
                pastPage.Background = Brushes.Black;
                pastPage.Foreground = Brushes.White;
                pastPage = button;
            }
            DrawClut();
            DrawTextures();
            UpdateClutTxt();
            textureGrid.ShowGridLines = true;
        }
        private void CharButtonClick(object sender, RoutedEventArgs e) //X & Zero Toggle
        {
            Button b = sender as Button;

            if (!Level.zeroFlag)
            {
                b.Content = "Zero";
                Level.zeroFlag = true;
                Level.AssignPallete();
                if (bgF == 0)
                    PSX.levels[Level.Id].LoadTextures(false);
                MainWindow.window.Update();
            }
            else
            {
                b.Content = "X";
                Level.zeroFlag = false;
                Level.AssignPallete();
                if (bgF == 0)
                    PSX.levels[Level.Id].LoadTextures(false);
                MainWindow.window.Update();
            }
        }
        #endregion Events
    }
}
