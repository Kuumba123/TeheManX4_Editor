﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TeheManX4.Forms
{
    /// <summary>
    /// Interaction logic for Tile16Editor.xaml
    /// </summary>
    public partial class Tile16Editor : UserControl
    {
        #region Fields
        internal static List<List<Undo>> undos = new List<List<Undo>>();
        public static double manualClutLeft = double.NaN;
        public static double manualClutTop = double.NaN;
        #endregion Fields

        #region Properties
        WriteableBitmap tileBMP = new WriteableBitmap(256, 256, 96, 96, PixelFormats.Rgb24, null);
        WriteableBitmap tileBMP_S = new WriteableBitmap(16, 16, 96, 96, PixelFormats.Rgb24, null);
        Button past;
        Button past2;
        bool enable = true;
        bool tilesDown = false;
        public int tileCol = 0;
        public int selectedTile = 0;
        public int selectTex = 0;
        public int clut = 0;
        public int tileX = 0;
        public int tileY = 0;
        public int page = 0;
        #endregion Properties

        #region Constructors
        public Tile16Editor()
        {
            InitializeComponent();
        }
        #endregion Constructors

        #region Methods
        public void UpdateCursor()
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
            MainWindow.window.x16E.tileImage.Source = tileBMP;
        }
        public void DrawTextures()
        {
            palBtn.Content = "CLUT: " + Convert.ToString(clut, 16).ToUpper().PadLeft(2, '0'); //Update Txt
            IntPtr pixelDataPtr = Level.bmp[page + 8].BackBuffer;
            BitmapPalette pal;
            int stride = 128;
            PixelFormat format = PixelFormats.Indexed4;
            if (page < 8)
                pal = Level.palette[clut + 64];
            else
            {
                List<Color> colors = new List<Color>();

                for (int i = 0; i < 256; i++)
                {
                    if ( ((((i >> 4) + clut) * 16) + (i & 0xF)) > 8191) break;
                    colors.Add(Level.palette[clut + 64 + (i >> 4)].Colors[i & 0xF]);
                }
                pal = new BitmapPalette(colors);
                format = PixelFormats.Indexed8;
                stride = 256;
            }

            MainWindow.window.x16E.textureImage.Source = BitmapSource.Create(256,
            256,
            96,
            96,
            format,
            pal,
            pixelDataPtr,
            256 * stride,
            stride);
        }
        public void DrawTile()
        {
            tileBMP_S.Lock();
            Level.Draw16xTile(selectedTile, 0, 0, 0x30, tileBMP_S.BackBuffer);
            tileBMP_S.AddDirtyRect(new Int32Rect(0, 0, 16, 16));
            tileBMP_S.Unlock();
            MainWindow.window.x16E.tileImageS.Source = tileBMP_S;

            UpdateTileText();
        }
        private void UpdateTileText()
        {
            //Various Tile Info
            enable = false;
            MainWindow.window.x16E.tileInt.Value = selectedTile;
            MainWindow.window.x16E.cordInt.Value = PSX.levels[Level.Id].tileInfo[(selectedTile * 4) + 2];
            MainWindow.window.x16E.pageInt.Value = PSX.levels[Level.Id].tileInfo[(selectedTile * 4) + 3];
            MainWindow.window.x16E.clutInt.Value = PSX.levels[Level.Id].tileInfo[(selectedTile * 4) + 1];
            MainWindow.window.x16E.colInt.Value = PSX.levels[Level.Id].tileInfo[(selectedTile * 4) + 0];
            enable = true;
        }
        public void AssignLimits()
        {
            int tileAmount = PSX.levels[Level.Id].tileInfo.Length / 4;
            tileAmount--;
            //Max Tile Settings
            MainWindow.window.x16E.tileInt.Maximum = tileAmount;
            if (MainWindow.window.x16E.tileInt.Value > tileAmount)
                MainWindow.window.x16E.tileInt.Value = tileAmount;

            int maxClut = ClutEditor.GetMaxClutId();
            
            MainWindow.window.x16E.clutInt.Maximum = maxClut;
            if (MainWindow.window.x16E.clutInt.Value > maxClut)
                MainWindow.window.x16E.clutInt.Value = maxClut;

            if (clut > maxClut)
                clut = maxClut;
            if (page > pageInt.Maximum)
                page = (int)pageInt.Maximum;

            DrawTiles();
            DrawTile();
            UpdateTileText();
            DrawTextures();
        }
        #endregion Methods

        #region Events
        private void TpageButton_Click(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            int i = Convert.ToInt32(b.Content.ToString().Trim(), 16);
            if (page == i)
                return;


            page = i;
            if (past2 != null)
            {
                //Old Color
                past2.Background = Brushes.Black;
                past2.Foreground = Brushes.White;
            }
            //New Color
            b.Background = Brushes.LightBlue;
            b.Foreground = Brushes.Black;
            past2 = b;
            DrawTextures();
        }
        private void TileButton_Click(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
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
        private void tileImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (!tilesDown)
                return;
            int tileAmount = PSX.levels[Level.Id].tileInfo.Length / 4;
            tileAmount--;
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
                    Grid.SetColumnSpan(cursor, tX - cX + 1);
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
        private void tileImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            tilesDown = false;
        }
        private void tileImage_MouseLeave(object sender, MouseEventArgs e)
        {
            tilesDown = false;
        }
        private void textureImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var p = e.GetPosition(textureImage);
            int x = (int)p.X;
            int y = (int)p.Y;
            int cX = Level.GetSelectedTile(x, textureImage.ActualWidth, 16);
            int cY = Level.GetSelectedTile(y, textureImage.ActualHeight, 16);

            if (e.ChangedButton == MouseButton.Right) //Cycle forward through CLUT
            {
                if (clut != 0x3F)
                    clut++;
                else
                    clut = 0;
                DrawTextures();
            }
            else
            {
                int colLocation = Grid.GetColumn(cursor);
                int rowLocation = Grid.GetRow(cursor);

                if (Grid.GetColumnSpan(cursor) > 1 || Grid.GetRowSpan(cursor) > 1) //Mutli Cord Edit
                {
                    int tileAmount = PSX.levels[Level.Id].tileInfo.Length / 4;
                    tileAmount--;

                    byte val = (byte)(cX + cY * 0x10);


                    if (undos[Level.Id].Count == Const.MaxUndo)
                        undos[Level.Id].RemoveAt(0);
                    undos[Level.Id].Add(Undo.CreateTileTextureGroupEditUndo((byte)tileCol, (byte)colLocation, (byte)rowLocation, (byte)Grid.GetColumnSpan(cursor), (byte)Grid.GetRowSpan(cursor)));

                    for (int r = 0; r < Grid.GetRowSpan(cursor); r++)
                    {
                        for (int c = 0; c < Grid.GetColumnSpan(cursor); c++)
                        {
                            int id = colLocation + c + (rowLocation + r) * 0x10 + (tileCol << 8);
                            if (id > tileAmount)
                                continue;

                            PSX.levels[Level.Id].tileInfo[(id * 4) + 2] = (byte)(val + c + r * 0x10);
                            PSX.levels[Level.Id].tileInfo[(id * 4) + 3] = (byte)page;
                        }
                    }
                    //Update
                    MainWindow.window.layoutE.DrawLayout();
                    MainWindow.window.layoutE.DrawScreen();
                    MainWindow.window.enemyE.Draw();
                    DrawTile();
                    MainWindow.window.screenE.DrawScreen();
                    DrawTiles();
                    MainWindow.window.screenE.DrawTile();
                    MainWindow.window.screenE.DrawTiles();
                    PSX.levels[Level.Id].edit = true;
                    return;
                }

                if (undos[Level.Id].Count == Const.MaxUndo)
                    undos[Level.Id].RemoveAt(0);
                undos[Level.Id].Add(Undo.CreateTileTextureGroupEditUndo((byte)tileCol, (byte)colLocation, (byte)rowLocation, 1, 1));

                PSX.levels[Level.Id].tileInfo[(selectedTile * 4) + 2] = (byte)(cX + (cY * 16));
                PSX.levels[Level.Id].tileInfo[(selectedTile * 4) + 3] = (byte)page;
                PSX.levels[Level.Id].edit = true;

                //Update
                MainWindow.window.x16E.cordInt.Value = PSX.levels[Level.Id].tileInfo[(selectedTile * 4) + 2];
                MainWindow.window.x16E.pageInt.Value = PSX.levels[Level.Id].tileInfo[(selectedTile * 4) + 3];
                MainWindow.window.layoutE.DrawLayout();
                MainWindow.window.layoutE.DrawScreen();
                MainWindow.window.screenE.DrawScreen();
                MainWindow.window.enemyE.Draw();
                DrawTile();

                if (MainWindow.window.screenE.selectedTile == selectedTile)
                {
                    MainWindow.window.screenE.cordInt.Value = PSX.levels[Level.Id].tileInfo[(selectedTile * 4) + 2];
                    MainWindow.window.screenE.pageInt.Value = PSX.levels[Level.Id].tileInfo[(selectedTile * 4) + 3];
                    MainWindow.window.screenE.DrawTile();
                }
                if (MainWindow.window.screenE.tileCol == (selectedTile >> 8))
                    MainWindow.window.screenE.DrawTiles();
                if (MainWindow.window.x16E.tileCol == (selectedTile >> 8))
                    MainWindow.window.x16E.DrawTiles();
            }
        }
        private void textureImage_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            int max = ClutEditor.GetMaxClutId();
            if (e.Delta > 0)
            {
                if (clut != max)
                    clut++;
                else
                    clut = 0;
            }
            else
            {
                if (clut != 0)
                    clut--;
                else
                    clut = max;
            }
            DrawTextures();
        }
        private void tileGridBtn_Click(object sender, RoutedEventArgs e)
        {
            if (tileGrid.ShowGridLines)
                tileGrid.ShowGridLines = false;
            else
                tileGrid.ShowGridLines = true;
        }
        private void textureGridBtn_Click(object sender, RoutedEventArgs e)
        {
            if (textureGrid.ShowGridLines)
                textureGrid.ShowGridLines = false;
            else
                textureGrid.ShowGridLines = true;
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

                if (undos[Level.Id].Count == Const.MaxUndo)
                    undos[Level.Id].RemoveAt(0);
                undos[Level.Id].Add(Undo.CreateScreenTileGroupEditUndo((byte)tileCol, 2, (byte)colLocation, (byte)rowLocation, (byte)Grid.GetColumnSpan(cursor), (byte)Grid.GetRowSpan(cursor)));

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
                MainWindow.window.screenE.DrawScreen();
                DrawTiles();
                MainWindow.window.screenE.DrawTile();
                MainWindow.window.screenE.DrawTiles();
                PSX.levels[Level.Id].edit = true;
                return;
            }


            if (PSX.levels[Level.Id].tileInfo[(selectedTile * 4) + 2] == val)
                return;

            if (undos[Level.Id].Count == Const.MaxUndo)
                undos[Level.Id].RemoveAt(0);
            undos[Level.Id].Add(Undo.CreateScreenTileEditUndo((ushort)selectedTile, 2, PSX.levels[Level.Id].tileInfo[(selectedTile * 4) + 2]));

            PSX.levels[Level.Id].tileInfo[(selectedTile * 4) + 2] = val;
            PSX.levels[Level.Id].edit = true;

            //Update
            MainWindow.window.layoutE.DrawLayout();
            MainWindow.window.layoutE.DrawScreen();
            MainWindow.window.enemyE.Draw();
            DrawTile();
            MainWindow.window.screenE.DrawScreen();
            if (tileCol == (selectedTile >> 8))
                DrawTiles();

            if (MainWindow.window.screenE.selectedTile == selectedTile)
                MainWindow.window.screenE.DrawTile();

            if (MainWindow.window.screenE.tileCol == (selectedTile >> 8))
                MainWindow.window.screenE.DrawTiles();
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

                if (undos[Level.Id].Count == Const.MaxUndo)
                    undos[Level.Id].RemoveAt(0);
                undos[Level.Id].Add(Undo.CreateScreenTileGroupEditUndo((byte)tileCol, 3, (byte)colLocation, (byte)rowLocation, (byte)Grid.GetColumnSpan(cursor), (byte)Grid.GetRowSpan(cursor)));

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
                MainWindow.window.screenE.DrawScreen();
                DrawTiles();
                MainWindow.window.screenE.DrawTile();
                MainWindow.window.screenE.DrawTiles();
                PSX.levels[Level.Id].edit = true;
                return;
            }

            if (PSX.levels[Level.Id].tileInfo[(selectedTile * 4) + 3] == val)
                return;

            if (undos[Level.Id].Count == Const.MaxUndo)
                undos[Level.Id].RemoveAt(0);
            undos[Level.Id].Add(Undo.CreateScreenTileEditUndo((ushort)selectedTile, 3, PSX.levels[Level.Id].tileInfo[(selectedTile * 4) + 3]));

            PSX.levels[Level.Id].tileInfo[(selectedTile * 4) + 3] = val;
            PSX.levels[Level.Id].edit = true;

            //Update
            MainWindow.window.layoutE.DrawLayout();
            MainWindow.window.layoutE.DrawScreen();
            MainWindow.window.enemyE.Draw();
            DrawTile();
            MainWindow.window.screenE.DrawScreen();
            if (tileCol == (selectedTile >> 8))
                DrawTiles();

            if (MainWindow.window.screenE.selectedTile == selectedTile)
                MainWindow.window.screenE.DrawTile();
            if (MainWindow.window.screenE.tileCol == (selectedTile >> 8))
                MainWindow.window.screenE.DrawTiles();
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

                if (undos[Level.Id].Count == Const.MaxUndo)
                    undos[Level.Id].RemoveAt(0);
                undos[Level.Id].Add(Undo.CreateScreenTileGroupEditUndo((byte)tileCol, 1, (byte)colLocation, (byte)rowLocation, (byte)Grid.GetColumnSpan(cursor), (byte)Grid.GetRowSpan(cursor)));

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
                MainWindow.window.screenE.DrawScreen();
                MainWindow.window.enemyE.Draw();
                DrawTile();
                DrawTiles();
                MainWindow.window.screenE.DrawTile();
                MainWindow.window.screenE.DrawTiles();
                PSX.levels[Level.Id].edit = true;
                return;
            }
            if (PSX.levels[Level.Id].tileInfo[(selectedTile * 4) + 1] == val)
                return;

            if (undos[Level.Id].Count == Const.MaxUndo)
                undos[Level.Id].RemoveAt(0);
            undos[Level.Id].Add(Undo.CreateScreenTileEditUndo((ushort)selectedTile, 1, PSX.levels[Level.Id].tileInfo[(selectedTile * 4) + 1]));

            PSX.levels[Level.Id].tileInfo[(selectedTile * 4) + 1] = val;
            PSX.levels[Level.Id].edit = true;

            //Update
            MainWindow.window.layoutE.DrawLayout();
            MainWindow.window.layoutE.DrawScreen();
            MainWindow.window.screenE.DrawScreen();
            MainWindow.window.enemyE.Draw();
            DrawTile();

            if (tileCol == (selectedTile >> 8))
                DrawTiles();

            if (MainWindow.window.screenE.selectedTile == selectedTile)
                MainWindow.window.screenE.DrawTile();
            if (MainWindow.window.screenE.tileCol == (selectedTile >> 8))
                MainWindow.window.screenE.DrawTiles();
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

                if (undos[Level.Id].Count == Const.MaxUndo)
                    undos[Level.Id].RemoveAt(0);
                undos[Level.Id].Add(Undo.CreateScreenTileGroupEditUndo((byte)tileCol, 0, (byte)colLocation, (byte)rowLocation, (byte)Grid.GetColumnSpan(cursor), (byte)Grid.GetRowSpan(cursor)));

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
                if (Level.showCollision)
                {
                    MainWindow.window.layoutE.DrawLayout();
                    MainWindow.window.layoutE.DrawScreen();

                    MainWindow.window.screenE.DrawScreen();
                    MainWindow.window.screenE.DrawTiles();
                    MainWindow.window.screenE.DrawTile();

                    MainWindow.window.x16E.DrawTiles();
                    MainWindow.window.x16E.DrawTile();

                    MainWindow.window.enemyE.Draw();
                }
                return;
            }

            if (PSX.levels[Level.Id].tileInfo[(selectedTile * 4) + 0] == (byte)(int)e.NewValue)
                return;

            if (undos[Level.Id].Count == Const.MaxUndo)
                undos[Level.Id].RemoveAt(0);
            undos[Level.Id].Add(Undo.CreateScreenTileEditUndo((ushort)selectedTile, 0, PSX.levels[Level.Id].tileInfo[(selectedTile * 4) + 0]));

            PSX.levels[Level.Id].tileInfo[(selectedTile * 4) + 0] = (byte)(int)e.NewValue;
            PSX.levels[Level.Id].edit = true;

            if (Level.showCollision)
            {
                MainWindow.window.layoutE.DrawLayout();
                MainWindow.window.layoutE.DrawScreen();

                MainWindow.window.screenE.DrawScreen();
                MainWindow.window.screenE.DrawTiles();
                MainWindow.window.screenE.DrawTile();

                MainWindow.window.x16E.DrawTiles();
                MainWindow.window.x16E.DrawTile();

                MainWindow.window.enemyE.Draw();
            }
        }
        private void palBtn_Click(object sender, RoutedEventArgs e)
        {
            ListWindow clutWin = new ListWindow();
            clutWin.Title = "Set Clut";
            clutWin.pannel.Orientation = Orientation.Horizontal;
            if (!double.IsNaN(manualClutLeft))
            {
                clutWin.Left = manualClutLeft;
                clutWin.Top = manualClutTop;
            }
            clutWin.ResizeMode = ResizeMode.NoResize;
            clutWin.Width = 140;
            clutWin.Height = 85;
            clutWin.scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            NumInt clutInt = new NumInt()
            {
                FontSize = 28,
                Width = 70,
                ButtonSpinnerWidth = 25,
                Minimum = 0,
                Maximum = ClutEditor.GetMaxClutId(),
                Value = clut
            };
            Label clutLabel = new Label()
            {
                FontSize = 26,
                Content = "ID"
            };
            clutWin.Closing += (s, arg) =>
            {
                manualClutLeft = clutWin.Left;
                manualClutTop = clutWin.Top;
                if(clutInt.Value != null)
                {
                    if (clutInt.Value != clut)
                    {
                        clut = (int)clutInt.Value;
                        DrawTextures();
                    }
                }
            };

            clutWin.pannel.Children.Add(clutInt);
            clutWin.pannel.Children.Add(clutLabel);
            clutWin.ShowDialog();
        }
        private void GearBtn_Click(object sender, RoutedEventArgs e)
        {
            ListWindow win = new ListWindow();
            win.Title = "Level Tools";
            win.Height = 230;
            win.Width = 350;
            win.ResizeMode = ResizeMode.NoResize;
            win.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            win.scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;

            Button layoutExport = new Button()
            {
                Content = "Export Layout Data as BIN",
                Uid = "l"
            };
            layoutExport.Click += GearOptionExport_Click;
            Button layoutImport = new Button()
            {
                Content = "Import Layout Data as BIN",
                Uid = "l"
            };
            layoutImport.Click += GearOptionImport_Click;

            Button screenExport = new Button()
            {
                Content = "Export Screen Data as BIN",
                Uid = "s"
            };
            screenExport.Click += GearOptionExport_Click;
            Button screenImport = new Button()
            {
                Content = "Import Screen Data as BIN",
                Uid = "s"
            };
            screenImport.Click += GearOptionImport_Click;

            Button tileExport = new Button()
            {
                Content = "Export Tile Data as BIN",
                Uid = "t"
            };
            tileExport .Click += GearOptionExport_Click;
            Button tileImport = new Button()
            {
                Content = "Import Tile Data as Bin",
                Uid = "t"
            };
            tileImport .Click += GearOptionImport_Click;

            win.pannel.Children.Add(layoutExport);
            win.pannel.Children.Add(layoutImport);

            win.pannel.Children.Add(screenExport);
            win.pannel.Children.Add(screenImport);

            win.pannel.Children.Add(tileExport);
            win.pannel.Children.Add(tileImport);

            win.ShowDialog();
        }
        private void GearOptionImport_Click(object sender, RoutedEventArgs e)
        {
            using(var fd = new System.Windows.Forms.OpenFileDialog())
            {
                fd.Filter = "BIN File|*.bin";
                fd.Title = "Select Save File";
                if(fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    FileInfo fileInfo = new FileInfo(fd.FileName);
                    switch (((UIElement)sender).Uid)
                    {
                        case "l":   //Layout
                            if(fileInfo.Length != PSX.levels[Level.Id].size * 3)
                            {
                                MessageBox.Show("Provided BIN does not match the layout size. " +
                                    "Set the layout size in the Size Window.");
                                return;
                            }
                            PSX.levels[Level.Id].layout = File.ReadAllBytes(fd.FileName);
                            LayoutEditor.undos.Clear();
                            PSX.edit = true;
                            break;

                        case "s": //Screen
                            if(fileInfo.Length < 0x200)
                            {
                                MessageBox.Show("Provided BIN is too small to contain valid Screen Data.");
                                return;
                            }
                            else if ((fileInfo.Length % 0x200) != 0)
                            {
                                MessageBox.Show("Provided BIN is not a multiple of 200hex bytes");
                                return;
                            }
                            else if(fileInfo.Length > (0x100 * 0x200))
                            {
                                MessageBox.Show("Provided BIN is too big to contain valid Screen Data.");
                                return;
                            }
                            PSX.levels[Level.Id].screenData = File.ReadAllBytes(fd.FileName);
                            Level.ClearInvalidTiles();
                            undos.Clear();
                            ScreenEditor.undos.Clear();
                            PSX.levels[Level.Id].edit = true;
                            break;
                        default:
                            if (fileInfo.Length < 4)
                            {
                                MessageBox.Show("Provided BIN is too small to contain valid Tile Data.");
                                return;
                            }else if((fileInfo.Length & 3) != 0)
                            {
                                MessageBox.Show("Provided BIN is not a multiple of 4 bytes");
                                return;
                            }
                            else if (fileInfo.Length > (4 * 0x4000))
                            {
                                MessageBox.Show("Provided BIN is too big to contain valid Tile Data.");
                                return;
                            }
                            PSX.levels[Level.Id].tileInfo = File.ReadAllBytes(fd.FileName);
                            Level.ClearInvalidTiles();
                            undos.Clear();
                            ScreenEditor.undos.Clear();
                            PSX.levels[Level.Id].edit = true;
                            break;
                    }
                    //Done
                    MainWindow.window.layoutE.AssignLimits();
                    MainWindow.window.screenE.AssignLimits();
                    MainWindow.window.x16E.AssignLimits();
                    MessageBox.Show("Data Imported!");
                }
            }
        }
        private void GearOptionExport_Click(object sender, RoutedEventArgs e)
        {
            using(var sfd = new System.Windows.Forms.SaveFileDialog())
            {
                sfd.Filter = "BIN File|*.bin";

                if(sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string id = (((UIElement)sender).Uid);
                    byte[] data;
                    if (id == "l")
                        data = PSX.levels[Level.Id].layout;
                    else if (id == "s")
                        data = PSX.levels[Level.Id].screenData;
                    else
                        data = PSX.levels[Level.Id].tileInfo;
                    File.WriteAllBytes(sfd.FileName, data);
                    MessageBox.Show("Data Exported!");
                }
            }
        }
        #endregion Events
    }
}
