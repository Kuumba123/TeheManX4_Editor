using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TeheManX4.Forms
{
    /// <summary>
    /// Interaction logic for SpriteEditor.xaml
    /// </summary>
    public partial class SpriteEditor : Window
    {
        #region Fields
        bool enable = false;
        byte[] buffer = new byte[1760 * 256 / 2];
        bool added = false;
        int mode = -1;
        int entrySize = 0;
        List<Sprite> spriteSlots = new List<Sprite>();
        List<WriteableBitmap> textures = new List<WriteableBitmap>();
        BitmapPalette[] palette = new BitmapPalette[0x80];
        RenderTargetBitmap target = new RenderTargetBitmap(256, 256, 96, 96, PixelFormats.Default);
        DrawingVisual drawingVisual = new DrawingVisual();
        ARC arc;
        ARC clutArc;
        int slotId = 0;
        int clutId = 0;
        int frameId = 0;
        byte[] sprtData;
        byte[] texCordData;
        #endregion Fields

        #region Constructors
        public SpriteEditor()
        {
            InitializeComponent();
        }
        #endregion Constructors

        #region Methods
        private void DrawSprite()
        {
            if (spriteSlots[slotId].frames.Count == 0)
            {
                ClearCanvas();
                return;
            }
            ImageBrush brush;
            using (var dc = drawingVisual.RenderOpen())
            {
                dc.DrawRectangle(Brushes.Gray, null, new Rect(0, 0, 256, 256));
                foreach (var q in spriteSlots[slotId].frames[frameId].quads)
                {
                    int srcY = spriteSlots[slotId].cord + (q.tex & 0xF0) + q.tpage * 256;
                    int srcX = (q.tex & 0xF) * 16;
                    int destX = q.x + 128;
                    int destY = q.y + 128;

                    int scalX;
                    int scalY;
                    if (q.flipH)
                    {
                        scalX = -1;
                    }
                    else
                    {
                        scalX = 1;
                    }
                    if (q.flipV)
                    {
                        scalY = -1;
                    }
                    else
                    {
                        scalY = 1;
                    }
                    brush = new ImageBrush(textures[q.clut]);
                    brush.RelativeTransform = new ScaleTransform(scalX, scalY, 0.5, 0.5);
                    brush.Viewbox = new Rect(srcX, srcY, 16, 16);
                    brush.ViewboxUnits = BrushMappingMode.Absolute;
                    brush.Viewport = new Rect(0, 0, 1, 1);
                    brush.ViewportUnits = BrushMappingMode.RelativeToBoundingBox;
                    dc.DrawRectangle(brush, null, new Rect(destX, destY, 16, 16));
                }

                if (spriteSlots[slotId].frames[frameId].quads.Count == 0)
                    goto Done;

                if ((bool)outCheck.IsChecked)
                {
                    sbyte x = spriteSlots[slotId].frames[frameId].quads[(int)quadInt.Value].x;
                    sbyte y = spriteSlots[slotId].frames[frameId].quads[(int)quadInt.Value].y;
                    // Create a red pen for the outline
                    Pen pen = new Pen(Brushes.Red, 1);
                    dc.DrawRectangle(null, pen, new Rect(128 + x, 128 + y, 16, 16));
                }
            Done:;
            }
            target.Render(drawingVisual);
            renderImg.Source = target;
        }
        private void UpdateSpriteInfo()
        {
            if(mode == 2)
            {
                if (!quadInt.IsEnabled)
                    quadInt.IsEnabled = true;
                sbyte x = spriteSlots[slotId].frames[frameId].quads[(int)quadInt.Value].x;
                sbyte y = spriteSlots[slotId].frames[frameId].quads[(int)quadInt.Value].y;
                xInt.Value = x;
                yInt.Value = y;
                horCheck.IsChecked = spriteSlots[slotId].frames[frameId].quads[(int)quadInt.Value].flipH;
                verCheck.IsChecked = spriteSlots[slotId].frames[frameId].quads[(int)quadInt.Value].flipV;
                quadInt.Maximum = spriteSlots[slotId].frames[frameId].quads.Count - 1;

                horCheck.IsChecked = spriteSlots[slotId].frames[frameId].quads[(int)quadInt.Value].flipH;
                verCheck.IsChecked = spriteSlots[slotId].frames[frameId].quads[(int)quadInt.Value].flipV;
                quadCordInt.Value = spriteSlots[slotId].frames[frameId].quads[(int)quadInt.Value].tex + (spriteSlots[slotId].frames[frameId].quads[(int)quadInt.Value].tpage << 8);

            }
        }
        private void UpdateTexCursor()
        {
            int tex = spriteSlots[slotId].frames[frameId].quads[(int)quadInt.Value].tex;

            int y = (tex & 0xF0) << 1;
            y += spriteSlots[slotId].frames[frameId].quads[(int)quadInt.Value].tpage * 256 * 2;
            int x = (tex & 0xF) << 5;

            Canvas.SetLeft(tileCursor, x);
            Canvas.SetTop(tileCursor, y);
        }
        private void DrawSpriteFromArc()
        {
            //ViewBox == Tex location
            Frame frame = Sprite.GetFrame(sprtData, BitConverter.ToInt32(sprtData, slotId * 4), frameId);
            int texCord;
            if (slotId * 4 < texCordData.Length)
                texCord = BitConverter.ToInt32(texCordData, slotId * 4);
            else
                texCord = BitConverter.ToInt32(texCordData, texCordData.Length - 4);

            ImageBrush brush;
            using (var dc = drawingVisual.RenderOpen())
            {
                dc.DrawRectangle(Brushes.Gray, null, new Rect(0, 0, 256, 256));
                foreach (var q in frame.quads)
                {
                    int srcY = (texCord >> 7) + (q.tex & 0xF0) + q.tpage * 256;
                    int srcX = (q.tex & 0xF) * 16;
                    int destX = q.x + 128;
                    int destY = q.y + 128;

                    int scalX;
                    int scalY;
                    if (q.flipH)
                    {
                        scalX = -1;
                    }
                    else
                    {
                        scalX = 1;
                    }
                    if (q.flipV)
                    {
                        scalY = -1;
                    }
                    else
                    {
                        scalY = 1;
                    }
                    brush = new ImageBrush(textures[q.clut]);
                    brush.RelativeTransform = new ScaleTransform(scalX, scalY,0.5,0.5);
                    brush.Viewbox = new Rect(srcX, srcY, 16, 16);
                    brush.ViewboxUnits = BrushMappingMode.Absolute;
                    brush.Viewport = new Rect(0, 0, 1, 1);
                    brush.ViewportUnits = BrushMappingMode.RelativeToBoundingBox;
                    dc.DrawRectangle(brush, null, new Rect(destX, destY, 16, 16));
                }
            }
            target.Render(drawingVisual);
            renderImg.Source = target;
        }
        private void ClearCanvas()
        {
            using(var dc = drawingVisual.RenderOpen())
            {
                dc.DrawRectangle(Brushes.Gray, null, new Rect(0, 0, 256, 256));
            }
            target.Render(drawingVisual);
            renderImg.Source = target;
        }
        public void UpdateClutTxt() //also update Cursor
        {
            Grid.SetRow(cursor, clutId);
            cursor.Fill = Brushes.Transparent;
            palBtn.Content = "CLUT: " + Convert.ToString(clutId, 16).ToUpper().PadLeft(2, '0');
        }
        private void AddClut()
        {
            added = true;
            for (int y = 0; y < 0x40; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    //Create Color
                    Rectangle r = new Rectangle();
                    r.Fill = Brushes.Blue;
                    r.Focusable = false;
                    r.Width = 16;
                    r.Height = 16;
                    r.Fill = new SolidColorBrush(Color.FromRgb(palette[y].Colors[x].R, palette[y].Colors[x].G, palette[y].Colors[x].B));
                    r.MouseDown += Color_Down;
                    r.HorizontalAlignment = HorizontalAlignment.Stretch;
                    r.VerticalAlignment = VerticalAlignment.Stretch;
                    Grid.SetRow(r, y);
                    Grid.SetColumn(r, x);
                    Panel.SetZIndex(r, 0);
                    clutGrid.Children.Add(r);
                }
            }
        }
        private void DrawClut()
        {
            if (!added)
            {
                AddClut();
                return;
            }
            foreach (var p in clutGrid.Children)
            {
                var c = Grid.GetColumn(p as UIElement);
                var r = Grid.GetRow(p as UIElement);

                var rect = (Rectangle)p;
                rect.Fill = new SolidColorBrush(Color.FromRgb(palette[r].Colors[c].R, palette[r].Colors[c].G, palette[r].Colors[c].B));
            }
        }
        private void EnableQuadControls()
        {
            quadInt.IsEnabled = true;
            xInt.IsEnabled = true;
            yInt.IsEnabled = true;
            clutInt.IsEnabled = true;
            quadCordInt.IsEnabled = true;
            addQuadBtn.IsEnabled = true;
            rmvQuadBtn.IsEnabled = true;
            horCheck.Visibility = Visibility.Visible;
            verCheck.Visibility = Visibility.Visible;
            allCheck.Visibility = Visibility.Visible;
            outCheck.Visibility = Visibility.Visible;
            toolsBtn.IsEnabled = true;
            tileCursor.Visibility = Visibility.Visible;
        }
        private void DisableQuadControls()
        {
            quadInt.IsEnabled = false;
            xInt.IsEnabled = false;
            yInt.IsEnabled = false;
            clutInt.IsEnabled = false;
            quadCordInt.IsEnabled = false;
            addQuadBtn.IsEnabled = false;
            rmvQuadBtn.IsEnabled = false;
            horCheck.Visibility = Visibility.Hidden;
            verCheck.Visibility = Visibility.Hidden;
            allCheck.Visibility = Visibility.Hidden;
            outCheck.Visibility = Visibility.Hidden;
            toolsBtn.IsEnabled = false;
            tileCursor.Visibility = Visibility.Hidden;
        }
        private void EnableSlotButtons()
        {
            addSlotBtn.IsEnabled = true;
            rmvSlotBtn.IsEnabled = true;
            addTexturesBtn.IsEnabled = true;
            addFramesBtn.IsEnabled = true;
        }
        private void DisableSlotButtons()
        {
            addSlotBtn.IsEnabled = false;
            rmvSlotBtn.IsEnabled = false;
            addTexturesBtn.IsEnabled = false;
            addFramesBtn.IsEnabled = false;
        }
        private void UpdateTitle()
        {
            this.Title = "TeheMan X4 Sprite Editor - " + arc.filename + " w/ " + clutArc.filename;
        }
        #endregion Methods

        #region Events
        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            using(var fd = new System.Windows.Forms.OpenFileDialog())
            {
                fd.Filter = "ARC |*.ARC";
                fd.Title = "Open an MegaMan X4 ARC File that contains Sprite Quad Data";
                if(fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        ARC arcT = new ARC(File.ReadAllBytes(fd.FileName));
                        if (!arcT.ContainsEntry(0xA) && !arcT.ContainsEntry(18))
                        {
                            MessageBox.Show("Could not find any SPRT Quad Data in this file");
                            return;
                        }
                        

                        arcT.filename = fd.SafeFileName;
                        byte[] data;
                        if (arcT.ContainsEntry(0xA))    //Normal Objects
                        {
                            data = arcT.LoadEntry(0x010102);
                            entrySize = arcT.GetEntrySize(0x010102);
                            sprtData = arcT.LoadEntry(0xA);
                            texCordData = arcT.LoadEntry(12);
                        }
                        else //Title Objects
                        {
                            data = arcT.LoadEntry(0x10002);
                            entrySize = arcT.GetEntrySize(0x10002);
                            byte[] tmp = new byte[] { 4, 0, 0, 0 };
                            sprtData = tmp.Concat(arcT.LoadEntry(18)).ToArray();
                            texCordData = new byte[4];
                        }

                        byte[] clut = null;
                        Level.ConvertBmp(data);

                        fd.Title = "Select ARC File containing the CLUT";
                        if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            clutArc = new ARC(File.ReadAllBytes(fd.FileName));
                            clutArc.filename = fd.SafeFileName;
                            if (!clutArc.ContainsEntry(5))
                            {
                                MessageBox.Show("There is no CLUT in this ARC file.");
                                return;
                            }
                            clut = clutArc.LoadEntry(5);
                        }
                        else
                            return;

                        for (int b = 0; b < 0x40; b++)
                        {
                            List<Color> l = new List<Color>();
                            for (int i = 0; i < 16; i++)
                            {
                                ushort color = BitConverter.ToUInt16(clut, (i + b * 16) * 2);

                                byte R = (byte)(color % 32 * 8);
                                byte G = (byte)(color / 32 % 32 * 8);
                                byte B = (byte)(color / 1024 % 32 * 8);
                                if (R == 0 && G == 0 && B == 0)
                                    l.Add(Color.FromArgb(0, 0, 0, 0));
                                else
                                    l.Add(Color.FromRgb(R, G, B));
                            }
                            palette[b] = new BitmapPalette(l);
                        }
                        Array.Clear(buffer, 0, 1760 * 256 / 2);
                        textures.Clear();
                        for (int i = 0; i < 4; i++)
                        {
                            textures.Add(new WriteableBitmap(256, Const.MaxHeight, 96, 96, PixelFormats.Indexed4, palette[i + 0x18]));
                            textures[i].WritePixels(new Int32Rect(0, 0, 256, data.Length / 128), data, 128, 0);
                        }
                        data.CopyTo(buffer, 0);

                        mode = 1;
                        slotId = 0;
                        frameId = 0;
                        clutId = 0x18;
                        frameInt.Value = 0;
                        frameInt.Maximum = 0xFF;
                        frameInt.IsEnabled = true;
                        slotInt.Value = 0;


                        int start = BitConverter.ToInt32(sprtData, 0);
                        slotInt.Maximum = (start / 4) - 1;
                        slotInt.IsEnabled = true;
                        arc = arcT;
                        palBtn.Visibility = Visibility.Visible;
                        palBtn.Width = 76;
                        UpdateTitle();
                        UpdateClutTxt();
                        DrawSpriteFromArc();
                        textureImg.Source = textures[0];
                        DrawClut();

                        //Disable Create Mode buttons
                        DisableQuadControls();
                        DisableSlotButtons();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "ERROR");
                        Application.Current.Shutdown();
                    }
                }
            }
        }
        private void ExportFramesButton_Click(object sender, RoutedEventArgs e) //Save Quad Frame Info
        {
            if (mode == 0)
                return;
            else if(mode == 2)
            {
                if (spriteSlots[slotId].frames.Count == 0) return;
            }
            ListWindow l = new ListWindow();
            l.Width = 300;
            l.Height = 150;
            l.ResizeMode = ResizeMode.NoResize;
            l.Title = "Enter Amount of Frames";

            //Make Frame Int for Input
            NumInt frameInt = new NumInt();
            frameInt.FontSize = 30;
            frameInt.Minimum = 1;
            frameInt.Value = frameId + 1;
            frameInt.Maximum = 0xFF;

            if(mode == 2)
            {
                frameInt.Maximum = this.frameInt.Maximum;
            }

            frameInt.Width = 70;
            frameInt.Height = 45;
            frameInt.VerticalAlignment = VerticalAlignment.Top;
            l.outGrid.Children.Add(frameInt);

            //Just Single Frame Check
            CheckBox check = new CheckBox();
            check.Content = "Single Frame";
            check.Foreground = Brushes.White;
            check.Checked += (s, ev) =>
            {
                l.Title = "Select Frame Id to Export";
                frameInt.Minimum = 0;
                frameInt.Value--;
            };
            check.Unchecked += (s, ev) =>
            {
                l.Title = "Enter Amount of Frames";
                frameInt.Minimum = 1;
                frameInt.Value++;
            };
            check.VerticalAlignment = VerticalAlignment.Bottom;
            check.HorizontalAlignment = HorizontalAlignment.Left;
            l.outGrid.Children.Add(check);

            //Confirm Button
            Button confirmBtn = new Button();
            confirmBtn.Content = "Confirm";
            confirmBtn.Width = 90;
            confirmBtn.Height = 30;
            confirmBtn.VerticalAlignment = VerticalAlignment.Bottom;

            confirmBtn.Click += (s, ev) =>
            {
                try
                {
                    string json = null;
                    if ((bool)check.IsChecked)
                    {
                        if (mode == 1)
                        {
                            Frame frame = Sprite.GetFrame(sprtData, BitConverter.ToInt32(sprtData, slotId * 4), (int)frameInt.Value);
                            json = JsonConvert.SerializeObject(frame, Formatting.Indented);
                        }
                        else
                        {
                            Frame frame = spriteSlots[slotId].frames[(int)frameInt.Value];
                            json = JsonConvert.SerializeObject(frame, Formatting.Indented);
                        }
                    }
                    else
                    {
                        if (mode == 1)
                        {
                            List<Frame> frames = Sprite.GetFrames((int)frameInt.Value, sprtData, BitConverter.ToInt32(sprtData, slotId * 4));
                            json = JsonConvert.SerializeObject(frames, Formatting.Indented);
                        }
                        else
                        {
                            List<Frame> frames = spriteSlots[slotId].frames;
                            json = JsonConvert.SerializeObject(frames, Formatting.Indented);
                        }
                    }

                    using (var fd = new System.Windows.Forms.SaveFileDialog())
                    {
                        fd.Filter = "JSON |*json";
                        fd.Title = "Select Frame Data save Location";

                        if ((bool)check.IsChecked)
                            fd.FileName = "frame.json";
                        else
                            fd.FileName = "frames.json";
                        if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            File.WriteAllText(fd.FileName, json);
                            MessageBox.Show("Frame Data Saved!");
                            //Close Window
                            Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive).Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "ERROR");
                }
            };
            l.outGrid.Children.Add(confirmBtn);

            l.ShowDialog();
        }
        private void ExportArcButton_Click(object sender, RoutedEventArgs e)
        {
            if(mode != 2)
            {
                MessageBox.Show("You need to be in Create mode before you can export the data to an existing ARC file.");
                return;
            }
            using(var fd =  new System.Windows.Forms.OpenFileDialog())
            {
                fd.Filter = "ARC |*.ARC";
                fd.Title = "Select the Level ARC file you would like to export to.";
                if(fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    ARC fileARC = new ARC(File.ReadAllBytes(fd.FileName));

                    if (fileARC.ContainsEntry(18))
                    {
                        fileARC.SaveEntry(18, spriteSlots[0].CreateSprtData());
                        //Replace Textures
                        byte[] tmp = new byte[(int)freeInt.Value * 128];
                        textures[0].CopyPixels(buffer, 128, 0);
                        Level.ConvertBmp(buffer);
                        Array.Copy(buffer, tmp, tmp.Length);
                        fileARC.SaveEntry(0x10002, tmp);
                        File.WriteAllBytes(fd.FileName, fileARC.GetEntriesData());

                        //Done
                        MessageBox.Show("Title Screen Object SPRT Data Saved!");
                        return;
                    }

                    if (!fileARC.ContainsEntry(0xA))
                    {
                        MessageBox.Show("There is no SPRT data in this ARC file to overwrite.");
                        return;
                    }
                    if (!fileARC.ContainsEntry(12))
                    {
                        MessageBox.Show("There is no TexCord data in this ARC file to overwrite.");
                        return;
                    }
                    if (!fileARC.ContainsEntry(0x010102))
                    {
                        MessageBox.Show("There is no Object Texture data in this ARC file to overwrite");
                        return;
                    }

                    //Prep Export
                    List<byte[]> dataSlots = new List<byte[]>();
                    int freeOffset = spriteSlots.Count * 4;
                    MemoryStream ms = new MemoryStream();
                    BinaryWriter bw = new BinaryWriter(ms);

                    foreach (var s in spriteSlots)
                    {
                        byte[] data = s.CreateSprtData();
                        dataSlots.Add(data);
                        bw.Write(freeOffset);
                        freeOffset += data.Length;
                    }
                    foreach (var array in dataSlots)
                        bw.Write(array);

                    fileARC.SaveEntry(0xA, ms.ToArray()); //Save SPRT Data
                    MemoryStream ms2 = new MemoryStream();
                    bw = new BinaryWriter(ms2);

                    foreach (var s in spriteSlots)
                        bw.Write(s.cord << 7);

                    fileARC.SaveEntry(12, ms2.ToArray()); //Save Texcord Data


                    //Replace Textures
                    byte[] tmp2 = new byte[(int)freeInt.Value * 128];
                    textures[0].CopyPixels(buffer, 128, 0);
                    Level.ConvertBmp(buffer);
                    Array.Copy(buffer, tmp2, tmp2.Length);
                    fileARC.SaveEntry(0x010102, tmp2);
                    File.WriteAllBytes(fd.FileName, fileARC.GetEntriesData());


                    //Optional CLUT export
                    fd.Title = "(Optional) Select the CLUT ARC file to export to.";
                    if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        clutArc = new ARC(File.ReadAllBytes(fd.FileName));
                        if (!clutArc.ContainsEntry(5))
                        {
                            MessageBox.Show("There is no CLUT in this file , however the SPRT data was still exported!");
                            return;
                        }
                        byte[] clut = clutArc.LoadEntry(5);
                        for (int i = 0; i < spriteSlots.Count; i++)
                        {
                            if (i > 9)
                                break;
                            if (spriteSlots[i].colors.Count == 0)
                                continue;
                            for (int c = 0; c < 64; c++)
                            {
                                int color = Level.To15Rgb(spriteSlots[i].colors[c].B, spriteSlots[i].colors[c].G, spriteSlots[i].colors[c].R);
                                BitConverter.GetBytes((ushort)color).CopyTo(clut, (c + 384) * 2 + (i * 128));
                            }
                        }
                        clutArc.SaveEntry(5, clut);
                        File.WriteAllBytes(fd.FileName, clutArc.GetEntriesData());

                        MessageBox.Show("SPRT & CLUT DATA Exported !");
                    }
                    else
                        MessageBox.Show("SPRT Data was exported!");
                }
            }

        }
        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            HelpWindow h = new HelpWindow(4);
            h.ShowDialog();
        }
        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            mode = 2;
            var l = Const.GreyScale;
            l[0] = Color.FromArgb(0, 0, 0, 0);
            palette[0x40] = new BitmapPalette(l);
            ClearCanvas();
            textures.Clear();
            quadInt.Value = 0;
            for (int i = 0; i < 4; i++)
                textures.Add(new WriteableBitmap(256,  1760, 96, 96, PixelFormats.Indexed4, palette[0x40]));
            textureImg.Source = textures[0];
            //Reset SPRT Slots
            spriteSlots.Clear();
            spriteSlots.Add(new Sprite() { cord = 0 });
            /*Reset Ints*/
            slotInt.Value = 0;
            slotInt.Maximum = 0;
            slotInt.IsEnabled = true;
            frameInt.Value = 0;
            frameInt.IsEnabled = false;

            freeInt.Value = 0;
            //Enable Buttons
            EnableSlotButtons();
            EnableQuadControls();
            palBtn.Width = 100;
            palBtn.Content = "ADD CLUT";
            palBtn.Visibility = Visibility.Visible;
        }

        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
        }
        private void Color_Down(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (mode != 1)
                return;
            if (e.ChangedButton == System.Windows.Input.MouseButton.Right)
            {
                using(var fd = new System.Windows.Forms.SaveFileDialog())
                {
                    fd.Filter = "Text File (*.txt)|*txt";
                    fd.Title = "Select CLUT: " + Convert.ToString(clutId, 16).ToUpper() + " save Location";
                    fd.FileName = "palette.txt";
                    if(fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        string txt = null;
                        for (int i = 0; i < 64; i++)
                        {
                            string r = Convert.ToString(palette[clutId + (i >> 4)].Colors[i & 0xF].R, 16).ToUpper().PadLeft(2, '0');
                            string g = Convert.ToString(palette[clutId + (i >> 4)].Colors[i & 0xF].G, 16).ToUpper().PadLeft(2, '0');
                            string b = Convert.ToString(palette[clutId + (i >> 4)].Colors[i & 0xF].B, 16).ToUpper().PadLeft(2, '0');

                            if (txt == null)
                                txt = "#" + r + g + b;
                            else
                                txt += "\n#" + r + g + b;
                        }
                        File.WriteAllText(fd.FileName, txt);
                        MessageBox.Show("Clut Saved!");
                    }
                }
            }
            else
            {
                clutId = Grid.GetRow(sender as UIElement);
                if (clutId > 60)
                    clutId = 60;
                UpdateClutTxt();
                for (int i = 0; i < 4; i++)
                {
                    int size = entrySize / 128;
                    var b = new WriteableBitmap(256, size, 96, 96, PixelFormats.Indexed4, palette[clutId + i]);
                    b.WritePixels(new Int32Rect(0, 0, 256, size), buffer, 128, 0);
                    textures[i] = b;
                }
                DrawSpriteFromArc();
                textureImg.Source = textures[0];
            }
        }

        private void frameInt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue == null || e.OldValue == null)
                return;
            if (frameId == (int)e.NewValue) return;

            frameId = (int)e.NewValue;
            try
            {
                if (mode == 1)
                {
                    DrawSpriteFromArc();
                }else if(mode == 2)
                {
                    enable = false;
                    quadInt.Value = 0;
                    UpdateSpriteInfo();
                    UpdateTexCursor();
                    DrawSprite();
                    enable = true;
                }
            }
            catch (IndexOutOfRangeException)
            {
                MessageBox.Show("Index Out of Range.\nPlease go back to the previous frame to avoid editor glitches");
            }catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR");
            }
        }

        private void slotInt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue == null || e.OldValue == null)
                return;
            if (slotId == (int)e.NewValue) return;
            slotId = (int)e.NewValue;
            frameId = 0;
            frameInt.Value = 0;

            if (mode == 1)
            {
                if ((bool)autoCheck.IsChecked)
                {
                    clutId = slotId * 4 + 0x18;
                    if (clutId > 0x3C)
                        clutId = 0x3C;
                    for (int i = 0; i < 4; i++)
                    {
                        int size = entrySize / 128;
                        var b = new WriteableBitmap(256, (int)textures[0].Height, 96, 96, PixelFormats.Indexed4, palette[clutId + i]);
                        b.WritePixels(new Int32Rect(0, 0, 256, size), buffer, 128, 0);
                        textures[i] = b;
                    }
                    if (slotId * 4 < texCordData.Length)
                        cordInt.Value = BitConverter.ToInt32(texCordData, slotId * 4) >> 7;
                    else
                        cordInt.Value = BitConverter.ToInt32(texCordData, texCordData.Length - 4) >> 7;
                    textureImg.Source = textures[0];
                    UpdateClutTxt();
                }
                try
                {
                    DrawSpriteFromArc();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "ERROR");
                }
            }
            else if(mode == 2) //Create Mode
            {
                cordInt.Value = spriteSlots[slotId].cord;
                if (spriteSlots[slotId].frames.Count == 0)
                {
                    frameInt.IsEnabled = false;
                }
                else
                {
                    if (!frameInt.IsEnabled)
                        frameInt.IsEnabled = true;
                    frameInt.Maximum = spriteSlots[slotId].frames.Count - 1;
                    if(spriteSlots[slotId].colors.Count != 0)
                    {
                        //Set CLUT to texture
                        textures[0].CopyPixels(buffer, 128, 0);
                        for (int i = 0; i < 4; i++)
                        {
                            var b = new WriteableBitmap(256, (int)textures[0].Height, 96, 96, PixelFormats.Indexed4, palette[slotId * 4 + i]);
                            b.WritePixels(new Int32Rect(0, 0, 256, (int)textures[0].Height), buffer, 128, 0);
                            textures[i] = b;
                        }
                    }
                    else
                    {
                        //Set CLUT to texture
                        textures[0].CopyPixels(buffer, 128, 0);
                        for (int i = 0; i < 4; i++)
                        {
                            var b = new WriteableBitmap(256, (int)textures[0].Height, 96, 96, PixelFormats.Indexed4, palette[0x40]);
                            b.WritePixels(new Int32Rect(0, 0, 256, (int)textures[0].Height), buffer, 128, 0);
                            textures[i] = b;
                        }
                    }
                    textureImg.Source = textures[0];
                    cordInt.Value = spriteSlots[slotId].cord;

                    enable = false;
                    quadInt.Value = 0;
                    DrawSprite();
                    UpdateSpriteInfo();
                    UpdateTexCursor();
                    enable = true;
                }
            }
        }
        private void addSlotBtn_Click(object sender, RoutedEventArgs e)
        {
            spriteSlots.Add(new Sprite() { cord = (int)freeInt.Value });
            slotInt.Maximum = spriteSlots.Count - 1;
        }

        private void rmvSlotBtn_Click(object sender, RoutedEventArgs e)
        {
            if(slotInt.Maximum == 0)
            {
                MessageBox.Show("You need atleast single slot for editing purposes.");
                return;
            }
            spriteSlots.RemoveAt((int)slotInt.Maximum);
            slotInt.Maximum -= 1;
            if (slotInt.Value > slotInt.Maximum)
                slotInt.Value -= 1;
        }
        private void addTexturesBtn_Click(object sender, RoutedEventArgs e)
        {
            using(var fd = new System.Windows.Forms.OpenFileDialog())
            {
                fd.Filter = "4bpp BMP |*.BMP";
                fd.Title = "Select the BMP files";
                if(fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Uri uri = new Uri(fd.FileName);
                    WriteableBitmap tex = new WriteableBitmap(new BitmapImage(uri));
                    Uri.EscapeUriString(fd.FileName);
                    if (tex.PixelWidth != 256)
                    {
                        MessageBox.Show("Texture must\nHave a width of 256 pixels.", "ERROR");
                        return;
                    }
                    if (tex.Format != PixelFormats.Indexed4)
                    {
                        MessageBox.Show("Texture must be in 4bpp.", "ERROR");
                        return;
                    }
                    if ((tex.PixelHeight & 0xF) != 0)
                    {
                        MessageBox.Show("Texture height must be a multiple of 16", "ERROR");
                        return;
                    }
                    if((freeInt.Value + tex.PixelHeight) > Const.MaxHeight)
                    {
                        MessageBox.Show("Max combined texture height is " + Const.MaxHeight);
                        return;
                    }

                    /*
                     * DUMP TEXTURES & UPDATE FREE CORD
                     */

                    byte[] data = new byte[256 * tex.PixelHeight / 2];
                    tex.CopyPixels(data, 128, 0);

                    textures[0].WritePixels(new Int32Rect(0, (int)freeInt.Value, 256, tex.PixelHeight), data, 128, 0);
                    textures[1].WritePixels(new Int32Rect(0, (int)freeInt.Value, 256, tex.PixelHeight), data, 128, 0);
                    textures[2].WritePixels(new Int32Rect(0, (int)freeInt.Value, 256, tex.PixelHeight), data, 128, 0);
                    textures[3].WritePixels(new Int32Rect(0, (int)freeInt.Value, 256, tex.PixelHeight), data, 128, 0);
                    freeInt.Value += tex.PixelHeight;

                    textureImg.Source = textures[0];
                    DrawSprite();
                }
            }
        }
        private void addFramesBtn_Click(object sender, RoutedEventArgs e)
        {
            using(var fd = new System.Windows.Forms.OpenFileDialog())
            {
                fd.Filter = "JSON |*json";
                fd.Title = "Select the JSON file containing the frame data";
                if(fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    List<Frame> frames = JsonConvert.DeserializeObject<List<Frame>>(File.ReadAllText(fd.FileName));
                    spriteSlots[slotId].frames = frames;

                    //Enalbe Viewing Frames
                    frameInt.Maximum = frames.Count - 1;
                    frameInt.IsEnabled = true;

                    enable = false;
                    UpdateSpriteInfo();
                    UpdateTexCursor();
                    DrawSprite();
                    enable = true;
                }
            }
        }
        private void importFrameBtn_Click(object sender, RoutedEventArgs e)
        {
            if (spriteSlots[slotId].frames.Count == 0)
                return;
            using(var fd = new System.Windows.Forms.OpenFileDialog())
            {
                fd.Filter = "JSON |*json";
                fd.Title = "Select the JSON file containing the frame data";
                if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Frame frame = JsonConvert.DeserializeObject<Frame>(File.ReadAllText(fd.FileName));
                    spriteSlots[slotId].frames[frameId] = frame;

                    enable = false;
                    UpdateSpriteInfo();
                    UpdateTexCursor();
                    DrawSprite();
                    enable = true;
                }
            }
        }
        private void palBtn_Click(object sender, RoutedEventArgs e)
        {
            if(mode == 2) //Get CLUT from txt File
            {
                using(var fd = new System.Windows.Forms.OpenFileDialog())
                {
                    fd.Filter = "Text File |*.TXT";
                    fd.Title = "Select the Text File containning your CLUT";
                    if(fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        string[] lines = File.ReadAllLines(fd.FileName);
                        spriteSlots[slotId].colors.Clear();
                        for (int s = 0; s < 4; s++)
                        {
                            List<Color> colors = new List<Color>();
                            for (int c = 0; c < 16; c++)
                            {
                                if (lines[c + s * 16].Trim() == "") continue;

                                uint val = Convert.ToUInt32(lines[c + s * 16].Replace("#", "").Trim(), 16);
                                Color color;
                                if (val == 0)
                                    color = Color.FromArgb(0, 0, 0, 0);
                                else
                                    color = Color.FromRgb((byte)(val >> 16), (byte)((val >> 8) & 0xFF), (byte)(val & 0xFF));
                                colors.Add(color);
                                spriteSlots[slotId].colors.Add(color);
                            }
                            palette[slotId * 4 + s] = new BitmapPalette(colors);
                        }

                        //Set CLUT to texture
                        textures[0].CopyPixels(buffer, 128, 0);
                        for (int i = 0; i < 4; i++)
                        {
                            var b = new WriteableBitmap(256, (int)textures[0].Height, 96, 96, PixelFormats.Indexed4, palette[slotId * 4 + i]);
                            b.WritePixels(new Int32Rect(0, 0, 256, (int)textures[0].Height), buffer, 128, 0);
                            textures[i] = b;
                        }
                        enable = false;
                        DrawSprite();
                        enable = true;
                    }
                }
            }
        }
        private void textureImg_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) //Get Boss Textures
        {
            if(mode == 1)
            {
                using(var fd = new System.Windows.Forms.OpenFileDialog())
                {
                    fd.Filter = "Boss ARC |*.ARC";
                    fd.Title = "Select the Maverick Boss ARC File";
                    if(fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        ARC bossArc = new ARC(File.ReadAllBytes(fd.FileName));
                        byte[] data;

                        foreach (var en in bossArc.entries)
                        {
                            if((en.type >> 16) == 1) //Textures
                            {
                                data = bossArc.LoadEntry(en.type);
                                Level.ConvertBmp(data);
                                for (int i = 0; i < 4; i++)
                                {
                                    textures[i].WritePixels(new Int32Rect(0, Const.CordTabe[en.type & 0xFF] - 384, 256, data.Length / 128), data, 128, 0);
                                }
                                textureImg.Source = textures[0];
                                if (slotId * 4 < texCordData.Length)
                                    BitConverter.GetBytes((Const.CordTabe[en.type & 0xFF] - 384) << 7).CopyTo(texCordData, slotId * 4);
                                else
                                    BitConverter.GetBytes((Const.CordTabe[en.type & 0xFF] - 384) << 7).CopyTo(texCordData, texCordData.Length - 4);
                                DrawSpriteFromArc();
                                cordInt.Value = Const.CordTabe[en.type & 0xFF] - 384;
                                break;
                            }
                        }
                    }
                }
            }
        }
        private void textureImg_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if(mode == 2)
            {
                if (spriteSlots[slotId].frames.Count == 0) return;

                var p = e.GetPosition(textureImg);
                int x = (int)p.X;
                int y = (int)p.Y;

                int cX = Level.GetSelectedTile(x, textureImg.ActualWidth, 16);
                int cY = Level.GetSelectedTile(y, textureImg.ActualHeight, 110);
                int page = cY / 16;

                quadCordInt.Value = (page << 8) + cX + ((cY & 0xF) << 4);
            }
        }

        private void cordInt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue == null || e.OldValue == null)
                return;
            if (mode == 1)
            {
            }else if(mode == 2)
            {
                spriteSlots[slotId].cord = (int)e.NewValue;
                DrawSprite();
            }
        }
        private void xInt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue == null || e.OldValue == null || !enable) return;

            if(mode == 2)
            {
                if ((bool)allCheck.IsChecked)
                {
                    int val = (int)e.NewValue - (int)e.OldValue;
                    foreach (var q in spriteSlots[slotId].frames[frameId].quads)
                        q.x = (sbyte)(q.x + val);
                }
                else
                {
                    spriteSlots[slotId].frames[frameId].quads[(int)quadInt.Value].x = (sbyte)(int)e.NewValue;
                }
                DrawSprite();
            }
        }

        private void yInt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue == null || e.OldValue == null || !enable) return;

            if (mode == 2)
            {
                if ((bool)allCheck.IsChecked)
                {
                    int val = (int)e.NewValue - (int)e.OldValue;
                    foreach (var q in spriteSlots[slotId].frames[frameId].quads)
                        q.y = (sbyte)(q.y + val);
                }
                else
                {
                    spriteSlots[slotId].frames[frameId].quads[(int)quadInt.Value].y = (sbyte)(int)e.NewValue;
                }
                DrawSprite();
            }
        }

        private void clutInt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue == null || e.OldValue == null || !enable) return;

            if (mode == 2)
            {
                spriteSlots[slotId].frames[frameId].quads[(int)quadInt.Value].clut = (byte)(int)e.NewValue;
                DrawSprite();
            }
        }
        private void quadCordInt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue == null || e.OldValue == null || !enable) return;

            if(mode == 2)
            {
                spriteSlots[slotId].frames[frameId].quads[(int)quadInt.Value].tpage = (byte)((int)e.NewValue >> 8);
                spriteSlots[slotId].frames[frameId].quads[(int)quadInt.Value].tex = (byte)((int)e.NewValue & 0xFF);
                UpdateTexCursor();
                DrawSprite();
            }
        }

        private void quadInt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue == null || e.OldValue == null || !enable) return;

            if (mode == 2)
            {
                UpdateSpriteInfo();
                UpdateTexCursor();
                DrawSprite();
            }
        }

        private void horCheckChange_Checked(object sender, RoutedEventArgs e)
        {
            if (!enable) return;
            if (mode == 2)
            {
                spriteSlots[slotId].frames[frameId].quads[(int)quadInt.Value].flipH = (bool)horCheck.IsChecked;
                DrawSprite();
            }
        }

        private void verCheckChange_Checked(object sender, RoutedEventArgs e)
        {
            if (!enable) return;
            if (mode == 2)
            {
                spriteSlots[slotId].frames[frameId].quads[(int)quadInt.Value].flipV = (bool)verCheck.IsChecked;
                DrawSprite();
            }
        }
        private void outCheckChange_Checked(object sender, RoutedEventArgs e)
        {
            if (mode == 2)
                DrawSprite();
        }
        private void addQuadBtn_Click(object sender, RoutedEventArgs e)
        {
            if (spriteSlots[slotId].frames.Count == 0)
                return;
            if (spriteSlots[slotId].frames[frameId].quads.Count > 999)
                return;
            spriteSlots[slotId].frames[frameId].quads.Add(new Quad());
            DrawSprite();
        }

        private void rmvQuadBtn_Click(object sender, RoutedEventArgs e)
        {
            if (spriteSlots[slotId].frames.Count == 0)
                return;
            if (spriteSlots[slotId].frames[frameId].quads.Count < 2)
                return;
            enable = false;
            spriteSlots[slotId].frames[frameId].quads.RemoveAt((int)quadInt.Value);

            if ((int)quadInt.Value > spriteSlots[slotId].frames[frameId].quads.Count - 1)
            {
                quadInt.Value = spriteSlots[slotId].frames[frameId].quads.Count - 1;
                UpdateSpriteInfo();
            }

            DrawSprite();
            enable = true;

        }
        private void toolsBtn_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion Events
    }
}
