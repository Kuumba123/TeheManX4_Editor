using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TeheManX4.Forms
{
    /// <summary>
    /// Interaction logic for ScreenEditor.xaml
    /// </summary>
    public partial class ScreenEditor : UserControl
    {
        #region Properties
        WriteableBitmap screenBMP = new WriteableBitmap(256, 256, 96, 96, PixelFormats.Rgb24, null);
        WriteableBitmap tileBMP = new WriteableBitmap(256, 256, 96, 96, PixelFormats.Rgb24, null);
        WriteableBitmap tileBMP_S = new WriteableBitmap(16, 16, 96, 96, PixelFormats.Rgb24, null);
        Button past;
        bool enable = true;
        bool tilesDown = false;
        bool screenDown = false;
        public int tileCol = 0;
        public int tileX = 0;
        public int tileY = 0;
        public int screenSelect = -1;
        public int startCol = 0;
        public int startRow = 0;
        public int selectedTile = 0;
        public int screenId = 2;
        #endregion Properteis

        #region Constructors
        public ScreenEditor()
        {
            InitializeComponent();
        }
        #endregion Constructors

        #region Methods
        public void ResetScreenCursor()
        {
            Grid.SetColumnSpan(screenCursor, 1);
            Grid.SetRowSpan(screenCursor, 1);
            screenCursor.Visibility = Visibility.Hidden;
        }
        public void UpdateCursor() //Tile Cursor
        {
            Grid.SetColumnSpan(cursor, 1);
            Grid.SetRowSpan(cursor, 1);
            if (tileCol == selectedTile >> 8)
            {
                cursor.Visibility = Visibility.Visible;
                Grid.SetColumn(cursor, selectedTile & 0xF);
                Grid.SetRow(cursor, (selectedTile & 0xF0) >> 4);
            }
            else
                cursor.Visibility = Visibility.Hidden;
        }
        public void UpdateScreenCursor()
        {
            if (screenSelect == -1) return;

            if (screenSelect == screenId)
                screenCursor.Visibility = Visibility.Visible;
            else
                screenCursor.Visibility = Visibility.Hidden;
        }
        public void DrawScreen()
        {
            screenBMP.Lock();
            Level.DrawScreen(screenId, 768, screenBMP.BackBuffer);
            screenBMP.AddDirtyRect(new Int32Rect(0, 0, 256, 256));
            screenBMP.Unlock();
            MainWindow.window.screenE.screenImage.Source = screenBMP;
        }
        public void DrawTiles()
        {
            int tileAmount = PSX.levels[Level.Id].tileInfo.Length / 4;
            tileAmount--;
            tileBMP.Lock();
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    int id = (tileCol * 0x100) + x + (y * 16);
                    if (id > tileAmount)
                        id = 0;
                    Level.Draw16xTile(id, x * 16, y * 16, 768, tileBMP.BackBuffer);
                }
            }
            tileBMP.AddDirtyRect(new Int32Rect(0, 0, 256, 256));
            tileBMP.Unlock();
            MainWindow.window.screenE.tileImage.Source = tileBMP;
        }
        public void DrawTile()
        {
            tileBMP_S.Lock();
            Level.Draw16xTile(selectedTile, 0, 0, 0x30, tileBMP_S.BackBuffer);
            tileBMP_S.AddDirtyRect(new Int32Rect(0, 0, 16, 16));
            tileBMP_S.Unlock();
            MainWindow.window.screenE.tileImageS.Source = tileBMP_S;
            UpdateTileText();
        }
        private void UpdateTileText()
        {
            //Various Tile Info
            enable = false;
            MainWindow.window.screenE.tileInt.Value = selectedTile;
            MainWindow.window.screenE.cordInt.Value = PSX.levels[Level.Id].tileInfo[(selectedTile * 4) + 2];
            MainWindow.window.screenE.pageInt.Value = (PSX.levels[Level.Id].tileInfo[(selectedTile * 4) + 3]) & 7;
            MainWindow.window.screenE.clutInt.Value = PSX.levels[Level.Id].tileInfo[(selectedTile * 4) + 1];
            MainWindow.window.screenE.colInt.Value = PSX.levels[Level.Id].tileInfo[(selectedTile * 4) + 0];
            enable = true;
        }
        public void AssignLimits()
        {
            int screenAmount = PSX.levels[Level.Id].screenData.Length / 0x200;
            int tileAmount = PSX.levels[Level.Id].tileInfo.Length / 4;
            screenAmount--;
            tileAmount--;
            //Max Screen Settings
            MainWindow.window.screenE.screenInt.Maximum = screenAmount;
            if(MainWindow.window.screenE.screenInt.Value > screenAmount)
                MainWindow.window.screenE.screenInt.Value = screenAmount;

            //Max Tile Settings
            MainWindow.window.screenE.tileInt.Maximum = tileAmount;
            if(MainWindow.window.screenE.tileInt.Value > tileAmount)
                MainWindow.window.screenE.tileInt.Value = tileAmount;

            DrawScreen();
            DrawTiles();
            DrawTile();
            UpdateTileText();
            ResetScreenCursor();
        }
        #endregion Methods

        #region Events
        private void TilesButton_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            int i = Convert.ToInt32(b.Content.ToString().Trim(), 16);
            if (tileCol == i)
                return;
            tileCol = i;
            if (past != null)
            {
                past.Background = Brushes.Black;
                past.Foreground = Brushes.White;
            }
            b.Background = Brushes.LightBlue;
            b.Foreground = Brushes.Black;
            past = b;
            UpdateCursor();
            DrawTiles();
        }
        private void ScreenGridBtn_Click(object sender, RoutedEventArgs e)
        {
            if (screenGrid.ShowGridLines)
                screenGrid.ShowGridLines = false;
            else
                screenGrid.ShowGridLines = true;
        }
        private void TileGridBtn_Click(object sender, RoutedEventArgs e)
        {
            if (tileGrid.ShowGridLines)
                tileGrid.ShowGridLines = false;
            else
                tileGrid.ShowGridLines = true;
        }

        private void Extra_Click(object sender, RoutedEventArgs e)
        {
            if (ListWindow.extraOpen)
                return;
            MainWindow.extraWindow = new ListWindow(2);
            MainWindow.extraWindow.Show();
        }
        private void screenImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(screenImage);
            if (p.X > MainWindow.window.screenE.screenImage.Width || p.X < 0) return;
            if (p.Y > MainWindow.window.screenE.screenImage.Height || p.Y < 0) return;

            int x = (int)p.X;
            int y = (int)p.Y;
            int cX = Level.GetSelectedTile(x, screenImage.ActualWidth, 16);
            int cY = Level.GetSelectedTile(y, screenImage.ActualHeight, 16);
            int cord = (cX * 2) + (cY * 16 * 2);
            if (e.LeftButton == MouseButtonState.Pressed) //Paste
            {

                if (Keyboard.IsKeyDown(Key.LeftShift))
                {
                    if (Grid.GetColumnSpan(screenCursor) != 1 || Grid.GetRowSpan(screenCursor) != 1) //Paste From other Screen
                    {

                        for (int r = 0; r < Grid.GetRowSpan(screenCursor); r++)
                        {
                            for (int c = 0; c < Grid.GetColumnSpan(screenCursor); c++)
                            {
                                if (cX + c > 15)
                                    continue;
                                if (cY + r > 15)
                                    continue;
                                int dest = cord + c * 2 + r * 32 + (screenId * 0x200);

                                int srcCol = Grid.GetColumn(screenCursor) + c;
                                int srcRow = Grid.GetRow(screenCursor) + r;

                                ushort val = BitConverter.ToUInt16(PSX.levels[Level.Id].screenData, screenSelect * 0x200 + srcCol * 2 + srcRow * 32);
                                val &= 0x3FFF;
                                BitConverter.GetBytes(val).CopyTo(PSX.levels[Level.Id].screenData, dest);
                            }
                        }
                        PSX.levels[Level.Id].edit = true;
                        DrawScreen();
                        MainWindow.window.layoutE.DrawLayout();
                        if (MainWindow.window.layoutE.selectedScreen == screenId)
                            MainWindow.window.layoutE.DrawScreen();
                        MainWindow.window.enemyE.Draw();
                    }
                    return;
                }

                //Normal Paste
                screenDown = true;
                if (Grid.GetColumnSpan(cursor) > 1 || Grid.GetRowSpan(cursor) > 1) //Multi-Select
                {
                    int tileAmount = PSX.levels[Level.Id].tileInfo.Length / 4;
                    tileAmount--;
                    int rowSrc = Grid.GetRow(cursor);
                    int colSrc = Grid.GetColumn(cursor);
                    for (int r = 0; r < Grid.GetRowSpan(cursor); r++)
                    {
                        for (int c = 0; c < Grid.GetColumnSpan(cursor); c++)
                        {
                            if (cX + c > 15)
                                continue;
                            if (cY + r > 15)
                                continue;
                            int id = c + colSrc + (tileCol << 8) + (r + rowSrc) * 16;

                            if (id > tileAmount)
                                id = 0;
                            int offset = cord + c * 2 + r * 32 + (screenId * 0x200);
                            ushort val = BitConverter.ToUInt16(PSX.levels[Level.Id].screenData, offset);
                            PSX.levels[Level.Id].screenData[offset] = (byte)(id & 0xFF);
                            PSX.levels[Level.Id].screenData[offset + 1] = (byte)((id >> 8) + ((val & 0xC000) >> 8));
                        }
                    }
                    PSX.levels[Level.Id].edit = true;

                    //Update everything
                    DrawScreen();
                    MainWindow.window.layoutE.DrawLayout();
                    if (screenId == MainWindow.window.layoutE.selectedScreen)
                        MainWindow.window.layoutE.DrawScreen();
                    MainWindow.window.enemyE.Draw();
                    return;
                }
                ushort t = BitConverter.ToUInt16(PSX.levels[Level.Id].screenData, cord + (screenId * 0x200));
                if ((t & 0x3FFF) == selectedTile)
                    return;
                PSX.levels[Level.Id].screenData[cord + (screenId * 0x200)] = (byte)(selectedTile & 0xFF);
                PSX.levels[Level.Id].screenData[cord + 1 + (screenId * 0x200)] = (byte)((selectedTile >> 8) + ((t & 0xC000) >> 8));
                PSX.levels[Level.Id].edit = true;
                DrawScreen();
                MainWindow.window.layoutE.DrawLayout();
                if (screenId == MainWindow.window.layoutE.selectedScreen)
                    MainWindow.window.layoutE.DrawScreen();
            }
            else //Copy
            {
                if (Keyboard.IsKeyDown(Key.LeftShift)) //Multi-Select
                {
                    if(screenCursor.Visibility != Visibility.Visible)
                    {
                        Grid.SetColumnSpan(screenCursor, 1);
                        Grid.SetRowSpan(screenCursor, 1);
                        Grid.SetColumn(screenCursor, cX);
                        Grid.SetRow(screenCursor, cY);
                        startCol = cX;
                        startRow = cY;
                        screenCursor.Visibility = Visibility.Visible;
                        screenSelect = screenId;
                    }
                    else
                    {
                        if (cX > startCol)
                        {
                            Grid.SetColumn(screenCursor, startCol);
                            Grid.SetColumnSpan(screenCursor, cX - startCol + 1);
                        }
                        else if (cX < startCol)
                        {
                            Grid.SetColumn(screenCursor, cX);
                            Grid.SetColumnSpan(screenCursor, startCol - cX + 1);
                        }
                        else
                        {
                            Grid.SetColumn(screenCursor, startCol);
                            Grid.SetColumnSpan(screenCursor, 1);
                        }

                        if (cY > startRow)
                        {
                            Grid.SetRow(screenCursor, startRow);
                            Grid.SetRowSpan(screenCursor, cY - startRow + 1);
                        }
                        else if (cY < startRow)
                        {
                            Grid.SetRow(screenCursor, cY);
                            Grid.SetRowSpan(screenCursor, startRow - cY + 1);
                        }
                        else
                        {
                            Grid.SetRow(screenCursor, startRow);
                            Grid.SetRowSpan(screenCursor, 1);
                        }
                    }
                }
                else
                {
                    int b = PSX.levels[Level.Id].screenData[cord + (screenId * 0x200)];
                    b += PSX.levels[Level.Id].screenData[1 + cord + (screenId * 0x200)] << 8;
                    selectedTile = b & 0xFFF;
                    ResetScreenCursor();
                    UpdateCursor();
                    DrawTile();
                    UpdateTileText();
                }
            }
        }

        private void screenImage_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(screenImage);
            if (p.X > MainWindow.window.screenE.screenImage.ActualWidth || p.X < 0) return;
            if (p.Y > MainWindow.window.screenE.screenImage.ActualHeight || p.Y < 0) return;
            if (e.LeftButton == MouseButtonState.Pressed && screenDown)
            {

                HitTestResult result = VisualTreeHelper.HitTest(MainWindow.window.screenE.screenImage, p);

                if (result != null)
                {
                    //Get Cords
                    int x = (int)p.X;
                    int y = (int)p.Y;
                    int cX = Level.GetSelectedTile(x, screenImage.ActualWidth, 16);
                    int cY = Level.GetSelectedTile(y, screenImage.ActualHeight, 16);
                    int cord = (cX * 2) + (cY * 16 * 2);

                    ushort t = BitConverter.ToUInt16(PSX.levels[Level.Id].screenData, cord + (screenId * 0x200));
                    if ((t & 0x3FFF) == selectedTile)
                        return;
                    //New Tile
                    PSX.levels[Level.Id].screenData[cord + (screenId * 0x200)] = (byte)(selectedTile & 0xFF);
                    PSX.levels[Level.Id].screenData[cord + 1 + (screenId * 0x200)] = (byte)((selectedTile >> 8) + ((t & 0xC000) >> 8));
                    PSX.levels[Level.Id].edit = true;
                    DrawScreen();
                    MainWindow.window.layoutE.DrawLayout();
                }
            }
        }
        private void screenImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            screenDown = false;
            tilesDown = false;
        }
        private void tileImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            tilesDown = false;
            screenDown = false;
        }
        private void tileImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            int tileAmount = PSX.levels[Level.Id].tileInfo.Length / 4;
            tileAmount--;
            Point p = e.GetPosition(tileImage);
            int x = (int)p.X;
            int y = (int)p.Y;
            int cX = Level.GetSelectedTile(x, tileImage.ActualWidth, 16);
            int cY = Level.GetSelectedTile(y, tileImage.ActualHeight, 16);
            int id = cX + (cY * 16);
            ResetScreenCursor();
            if (!tilesDown)
            {
                if ((uint)id > 0xFF)
                    id = 0xFF;
                id += tileCol * 0x100;

                if (id > tileAmount)
                {
                    id = tileAmount;
                }
                selectedTile = id;
                UpdateCursor();
                DrawTile();
                UpdateTileText();
                tilesDown = true;
            }
        }
        private void tileImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (!tilesDown)
                return;

            Point p = e.GetPosition(tileImage);
            int x = (int)p.X;
            int y = (int)p.Y;
            int cX = Level.GetSelectedTile(x, tileImage.ActualWidth, 16);
            int cY = Level.GetSelectedTile(y, tileImage.ActualHeight, 16);


            int id = selectedTile & 0xFF;
            int id2 = cX + (cY * 16);
            if (id == id2)
                return;

            int tX = selectedTile & 0xF;
            int tY = (selectedTile >> 4) & 0xF;

            if (tX < cX) //Width Selection
                Grid.SetColumnSpan(cursor, 1 + cX - tX);
            else
            {
                if (tX == cX)
                    Grid.SetColumnSpan(cursor, 1);
                else
                {
                    Grid.SetColumnSpan(cursor,tX - cX + 1);
                    Grid.SetColumn(cursor, cX);
                }
            }
            if (tY < cY) //Height Selection
                Grid.SetRowSpan(cursor, 1 + cY - tY);
            else
            {
                if (tY == cY)
                    Grid.SetRowSpan(cursor, 1);
                else
                {
                    Grid.SetRowSpan(cursor, tY - cY + 1);
                    Grid.SetRow(cursor, cY);
                }
            }
        }
        private void tileImage_MouseLeave(object sender, MouseEventArgs e)
        {
            tilesDown = false;
        }
        private void screenInt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            //For current Screen
            if (PSX.levels.Count != Const.FilesCount || e.NewValue == null || e.OldValue == null || !enable)
                return;
            if (screenId == (int)e.NewValue)
                return;
            screenId = (int)e.NewValue;
            if ((uint)screenId >= 0xEF)
                screenId = 0xEF;

            UpdateScreenCursor();
            DrawScreen();
            if (ListWindow.extraOpen)
                MainWindow.extraWindow.DrawExtra();
        }
        private void tileInt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (PSX.levels.Count != Const.FilesCount || e.NewValue == null || e.OldValue == null || !enable)
                return;
            if (selectedTile == (int)e.NewValue)
                return;
            selectedTile = (int)e.NewValue;
            UpdateCursor();
            DrawTile();
        }

        private void cordInt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (PSX.levels.Count != Const.FilesCount || e.NewValue == null || e.OldValue == null || !enable)
                return;
            byte val = (byte)(int)e.NewValue;

            if (Grid.GetColumnSpan(cursor) > 1 || Grid.GetRowSpan(cursor) > 1) //Mutli Cord Edit
            {
                int tileAmount = PSX.levels[Level.Id].tileInfo.Length / 4;
                tileAmount--;

                int colLocation = Grid.GetColumn(cursor);
                int rowLocation = Grid.GetRow(cursor);
                for (int r = 0; r < Grid.GetRowSpan(cursor); r++)
                {
                    for (int c = 0; c < Grid.GetColumnSpan(cursor); c++)
                    {
                        int id = colLocation + c + (rowLocation + r) * 0x10 + (tileCol << 8);
                        if (id > tileAmount)
                            continue;

                        PSX.levels[Level.Id].tileInfo[(id * 4) + 2] = (byte)(val + c + r * 0x10);
                    }
                }
                //Update
                MainWindow.window.layoutE.DrawLayout();
                MainWindow.window.layoutE.DrawScreen();
                MainWindow.window.enemyE.Draw();
                DrawTile();
                DrawScreen();
                DrawTiles();
                MainWindow.window.x16E.DrawTile();
                MainWindow.window.x16E.DrawTiles();
                PSX.levels[Level.Id].edit = true;
                return;
            }


            if (PSX.levels[Level.Id].tileInfo[(selectedTile * 4) + 2] == val)
                return;
            PSX.levels[Level.Id].tileInfo[(selectedTile * 4) + 2] = val;
            PSX.levels[Level.Id].edit = true;

            //Update
            MainWindow.window.layoutE.DrawLayout();
            MainWindow.window.layoutE.DrawScreen();
            MainWindow.window.enemyE.Draw();
            DrawTile();
            DrawScreen();
            if (tileCol == (selectedTile >> 8))
                DrawTiles();

            if (MainWindow.window.x16E.selectedTile == selectedTile)
                MainWindow.window.x16E.DrawTile();
            if (MainWindow.window.x16E.tileCol == (selectedTile >> 8))
                MainWindow.window.x16E.DrawTiles();
        }

        private void pageInt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (PSX.levels.Count != Const.FilesCount || e.NewValue == null || e.OldValue == null || !enable)
                return;
            byte val = (byte)(int)e.NewValue;

            if (Grid.GetColumnSpan(cursor) > 1 || Grid.GetRowSpan(cursor) > 1) //Mutli Tpage Edit
            {
                int tileAmount = PSX.levels[Level.Id].tileInfo.Length / 4;
                tileAmount--;

                int colLocation = Grid.GetColumn(cursor);
                int rowLocation = Grid.GetRow(cursor);
                for (int r = 0; r < Grid.GetRowSpan(cursor); r++)
                {
                    for (int c = 0; c < Grid.GetColumnSpan(cursor); c++)
                    {
                        int id = colLocation + c + (rowLocation + r) * 0x10 + (tileCol << 8);
                        if (id > tileAmount)
                            continue;

                        PSX.levels[Level.Id].tileInfo[(id * 4) + 3] = val;
                    }
                }
                //Update
                MainWindow.window.layoutE.DrawLayout();
                MainWindow.window.layoutE.DrawScreen();
                MainWindow.window.enemyE.Draw();
                DrawTile();
                DrawScreen();
                DrawTiles();
                MainWindow.window.x16E.DrawTile();
                MainWindow.window.x16E.DrawTiles();
                PSX.levels[Level.Id].edit = true;
                return;
            }

            if (PSX.levels[Level.Id].tileInfo[(selectedTile * 4) + 3] == val)
                return;
            PSX.levels[Level.Id].tileInfo[(selectedTile * 4) + 3] = val;
            PSX.levels[Level.Id].edit = true;

            //Update
            MainWindow.window.layoutE.DrawLayout();
            MainWindow.window.layoutE.DrawScreen();
            MainWindow.window.enemyE.Draw();
            DrawTile();
            DrawScreen();
            if (tileCol == (selectedTile >> 8))
                DrawTiles();

            if (MainWindow.window.x16E.selectedTile == selectedTile)
                MainWindow.window.x16E.DrawTile();
            if (MainWindow.window.x16E.tileCol == (selectedTile >> 8))
                MainWindow.window.x16E.DrawTiles();
        }

        private void clutInt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (PSX.levels.Count != Const.FilesCount || e.NewValue == null || e.OldValue == null || !enable)
                return;
            byte val = (byte)(int)e.NewValue;

            if (Grid.GetColumnSpan(cursor) > 1 || Grid.GetRowSpan(cursor) > 1) //Mutli Clut Edit
            {
                int tileAmount = PSX.levels[Level.Id].tileInfo.Length / 4;
                tileAmount--;

                int colLocation = Grid.GetColumn(cursor);
                int rowLocation = Grid.GetRow(cursor);
                for (int r = 0; r < Grid.GetRowSpan(cursor); r++)
                {
                    for (int c = 0; c < Grid.GetColumnSpan(cursor); c++)
                    {
                        int id = colLocation + c + (rowLocation + r) * 0x10 + (tileCol << 8);
                        if (id > tileAmount)
                            continue;

                        PSX.levels[Level.Id].tileInfo[(id * 4) + 1] = val;
                    }
                }
                //Update
                MainWindow.window.layoutE.DrawLayout();
                MainWindow.window.layoutE.DrawScreen();
                MainWindow.window.enemyE.Draw();
                DrawTile();
                DrawScreen();
                DrawTiles();
                MainWindow.window.x16E.DrawTile();
                MainWindow.window.x16E.DrawTiles();
                PSX.levels[Level.Id].edit = true;
                return;
            }
            if (PSX.levels[Level.Id].tileInfo[(selectedTile * 4) + 1] == val)
                return;

            PSX.levels[Level.Id].tileInfo[(selectedTile * 4) + 1] = val;
            PSX.levels[Level.Id].edit = true;

            //Update
            MainWindow.window.layoutE.DrawLayout();
            MainWindow.window.layoutE.DrawScreen();
            MainWindow.window.enemyE.Draw();
            DrawTile();
            DrawScreen();
            if (tileCol == (selectedTile >> 8))
                DrawTiles();

            if (MainWindow.window.x16E.selectedTile == selectedTile)
                MainWindow.window.x16E.DrawTile();
            if (MainWindow.window.x16E.tileCol == (selectedTile >> 8))
                MainWindow.window.x16E.DrawTiles();
        }

        private void colInt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (PSX.levels.Count != Const.FilesCount || e.NewValue == null || e.OldValue == null || !enable)
                return;

            if (Grid.GetColumnSpan(cursor) > 1 || Grid.GetRowSpan(cursor) > 1) //Mutli Collision Edit
            {
                int tileAmount = PSX.levels[Level.Id].tileInfo.Length / 4;
                tileAmount--;

                int colLocation = Grid.GetColumn(cursor);
                int rowLocation = Grid.GetRow(cursor);
                for (int r = 0; r < Grid.GetRowSpan(cursor); r++)
                {
                    for (int c = 0; c < Grid.GetColumnSpan(cursor); c++)
                    {
                        int id = colLocation + c + (rowLocation + r) * 0x10 + (tileCol << 8);
                        if (id > tileAmount)
                            continue;

                        PSX.levels[Level.Id].tileInfo[(id * 4) + 0] = (byte)(int)e.NewValue;
                    }
                }
                PSX.levels[Level.Id].edit = true;
                return;
            }

            if (PSX.levels[Level.Id].tileInfo[(selectedTile * 4) + 0] == (byte)(int)e.NewValue)
                return;
            PSX.levels[Level.Id].tileInfo[(selectedTile * 4) + 0] = (byte)(int)e.NewValue;
            PSX.levels[Level.Id].edit = true;
        }
        private void Help_Click(object sender, RoutedEventArgs e)
        {
            HelpWindow h = new HelpWindow(2);
            h.ShowDialog();
        }
        private void SnapButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Do you want to save all screens?", "?", MessageBoxButton.YesNoCancel);

            if (result == MessageBoxResult.Cancel)
                return;
            if (result == MessageBoxResult.Yes) //Save All Screens
            {
                var sfd = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
                sfd.Description = "Select Screens Save Location";
                sfd.UseDescriptionForTitle = true;

                if ((bool)sfd.ShowDialog())
                {
                    for (int i = 0; i < PSX.levels[Level.Id].screenData.Length / 0x200; i++)
                    {
                        screenBMP.Lock();
                        Level.DrawScreen(i, 0, 0, 768, screenBMP.BackBuffer);
                        screenBMP.AddDirtyRect(new Int32Rect(0, 0, 256, 256));
                        screenBMP.Unlock();

                        PngBitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(screenBMP));
                        System.IO.FileStream fs = System.IO.File.Create(sfd.SelectedPath + "\\" + PSX.levels[Level.Id].arc.filename + "_SCREEN_" + Convert.ToString(i, 16) + ".PNG");
                        encoder.Save(fs);
                        fs.Close();
                    }
                    DrawScreen();
                    MessageBox.Show("All Screens have been exported !!!");
                }
            }
            else //Save the Specfic Screen
            {
                using (var sfd = new System.Windows.Forms.SaveFileDialog())
                {
                    sfd.Filter = "PNG |*.png";
                    sfd.Title = "Select the Screen Save Location";
                    if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        PngBitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(screenBMP));
                        System.IO.FileStream fs = System.IO.File.Create(sfd.FileName);
                        encoder.Save(fs);
                        fs.Close();
                        MessageBox.Show("Screen Exported");
                    }
                }
            }
        }
        #endregion Events
    }
}
