using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TeheManX4.Forms
{
    /// <summary>
    /// Interaction logic for TextEditor.xaml
    /// </summary>
    public partial class TextEditor : Window
    {
        #region CONSTANTS
        const int MaxLineLength = 20;
        readonly Dictionary<char, byte> charTable = new Dictionary<char, byte>()
        {
            {'0',0x40 },
            {'1',0x41 },
            {'2',0x42 },
            {'3',0x43 },
            {'4',0x44 },
            {'5',0x45 },
            {'6',0x46 },
            {'7',0x47 },
            {'8',0x48 },
            {'9',0x49 },
            {'A',0x4A },
            {'B',0x4B },
            {'C',0x4C },
            {'D',0x4D },
            {'E',0x4E },
            {'F',0x4F },
            {'G',0x50 },
            {'H',0x51 },
            {'I',0x52 },
            {'J',0x53 },
            {'K',0x54 },
            {'L',0x55 },
            {'M',0x56 },
            {'N',0x57 },
            {'O',0x58 },
            {'P',0x59 },
            {'Q',0x5A },
            {'R',0x5B },
            {'S',0x5C },
            {'T',0x5D },
            {'U',0x5E },
            {'V',0x5F },
            {'W',0x60 },
            {'X',0x61 },
            {'Y',0x62 },
            {'Z',0x63 },
            {'a',0x64 },
            {'b',0x65 },
            {'c',0x66 },
            {'d',0x67 },
            {'e',0x68 },
            {'f',0x69 },
            {'g',0x6A },
            {'h',0x6B },
            {'i',0x6C },
            {'j',0x6D },
            {'k',0x6E },
            {'l',0x6F },
            {'m',0x70 },
            {'n',0x71 },
            {'o',0x72 },
            {'p',0x73 },
            {'q',0x74 },
            {'r',0x75 },
            {'s',0x76 },
            {'t',0x77 },
            {'u',0x78 },
            {'v',0x79 },
            {'w',0x7A },
            {'x',0x7B },
            {'y',0x7C },
            {'z',0x7D },
            {'/',0x7E },
            {'↓',0x7F },
            {'↑',0x80 },
            {'←',0x81 },
            {'→',0x82 },
            {'+',0x83 },
            {'-',0x84 },
            {',',0x85 },
            {'.',0x86 },
            {'!',0x87 },
            {'?',0x88 },
            {'"',0x89 },
            {'\'',0x8B },
            {' ',0xFF }
        };
        #endregion CONSTANTS

        #region Fields
        bool enable;
        Rectangle texRectangle = null;
        WriteableBitmap textTexture;
        BitmapImage boxTexture;
        RenderTargetBitmap target = new RenderTargetBitmap(246, 70, 96, 96, PixelFormats.Default);
        DrawingVisual drawingVisual = new DrawingVisual();
        List<TextEntry> textBoxes = new List<TextEntry>();
        #endregion Fields

        #region Constructors
        public TextEditor(ARC arc)
        {
            InitializeComponent();
            this.Title += arc.filename;

            // Get the assembly containing the embedded resource
            Assembly assembly = Assembly.GetExecutingAssembly();

            // Get the stream of the embedded resource
            Stream resourceStream = assembly.GetManifestResourceStream("TeheManX4.Resources.LOAD_U.ARC_TEX01.bmp");

            //Get Data For Text Textures
            BitmapImage bmpImage = new BitmapImage();
            bmpImage.BeginInit();
            bmpImage.StreamSource = resourceStream;
            bmpImage.EndInit();
            textTexture = new WriteableBitmap(bmpImage);
            textTextureImg.Source = textTexture;

            //Get Data For Box Texture
            resourceStream = assembly.GetManifestResourceStream("TeheManX4.Resources.Box.bmp");
            boxTexture = new BitmapImage();
            boxTexture.BeginInit();
            boxTexture.StreamSource = resourceStream;
            boxTexture.EndInit();


            byte[] textData = arc.LoadEntry(0x15);

            for (int i = 0; i < BitConverter.ToUInt16(textData,0) / 2; i++) //Get Text
            {
                TextEntry textBox = new TextEntry();
                textBox.boxes = TextEntry.GetBoxes(textData, BitConverter.ToUInt16(textData, i * 2));
                if ((textBox.boxes[0].data[0] & 0x800) == 0x800)
                    textBox.boxHigh = true;
                textBoxes.Add(textBox);
            }
            //Setup Texture Rect
            texRectangle = new Rectangle();
            canvas.Children.Add(texRectangle);

            //Setup Ints
            enable = false;
            textInt.Maximum = (BitConverter.ToUInt16(textData, 0) / 2) - 1;
            boxInt.Maximum = textBoxes[0].boxes.Count - 1;
            charInt.Value = 0;
            charInt.Maximum = (textBoxes[0].boxes[0].data.Length / 2) - 1;
            DrawText();
            UpdateCharInfo();
            enable = true;
        }
        #endregion Constructors

        #region Methods
        private void DrawText()
        {
            const int baseX = 4;
            const int baseY = 4;

            ImageBrush brush;
            using(var dc = drawingVisual.RenderOpen())
            {
                dc.DrawImage(boxTexture, new Rect(0, 0, 246, 70));

                int x = baseX;
                int y = baseY;
                for (int i = 0; i < textBoxes[(int)textInt.Value].boxes[(int)boxInt.Value].data.Length / 2; i++)
                {
                    byte cord = textBoxes[(int)textInt.Value].boxes[(int)boxInt.Value].data[i * 2];
                    byte type = textBoxes[(int)textInt.Value].boxes[(int)boxInt.Value].data[i * 2 + 1];

                    int srcX = (cord & 0xF) << 4;
                    int srcY = cord & 0xF0;


                    if (BitConverter.ToUInt16(textBoxes[(int)textInt.Value].boxes[(int)boxInt.Value].data, i * 2) != 0x1FF)
                    {
                        brush = new ImageBrush(textTexture);
                        brush.Viewbox = new Rect(srcX, srcY, 16, 16);
                        brush.ViewboxUnits = BrushMappingMode.Absolute;
                        brush.Viewport = new Rect(0, 0, 1, 1);
                        brush.ViewportUnits = BrushMappingMode.RelativeToBoundingBox;

                        dc.DrawRectangle(brush, null, new Rect(x, y, 16, 16));
                    }
                    if (i == charInt.Value)
                    {
                        // Create a red pen for the outline
                        Pen pen = new Pen(Brushes.Red, 1);
                        dc.DrawRectangle(null, pen, new Rect(x, y, 10, 15));
                    }
                    if ((type & 0x40) == 0x40) //Line Break
                    {
                        x = baseX;
                        y += 18;
                    }
                    else
                        x += 12;
                }
            }
            //Done
            target.Render(drawingVisual);
            textBoxImg.Source = target;
        }
        private void UpdateCharInfo()
        {
            ushort val = BitConverter.ToUInt16(textBoxes[(int)textInt.Value].boxes[(int)boxInt.Value].data, (int)(charInt.Value * 2));
            valInt.Value = val;

            if(val != 0x1FF)
            {
                texRectangle.Width = 32;
                texRectangle.Height = 32;
                texRectangle.Stroke = Brushes.Red;
                texRectangle.StrokeThickness = 1;
                Canvas.SetLeft(texRectangle, (val & 0xF) << 5);
                Canvas.SetTop(texRectangle, (val & 0xF0) << 1);
            }
            if ((val & 0x8000) == 0x8000)
                endCheck.IsChecked = true;
            else
                endCheck.IsChecked = false;

            if ((val & 0x4000) == 0x4000)
                lineCheck.IsChecked = true;
            else
                lineCheck.IsChecked = false;

            if ((val & 0x2000) == 0x2000)
                newCheck.IsChecked = true;
            else
                newCheck.IsChecked = false;

            if ((val & 0x1000) == 0x1000)
                scrollCheck.IsChecked = true;
            else
                scrollCheck.IsChecked = false;
        }
        #endregion Methods

        #region Events
        private void InsertButton_Click(object sender, RoutedEventArgs e)
        {
            Box box = new Box();
            byte[] data = new byte[] { 0xB9, 0x20 };
            box.data = data;
            textBoxes[(int)textInt.Value].boxes.Insert((int)boxInt.Value, box);
            boxInt.Maximum += 1;
            enable = false;
            charInt.Value = 0;
            charInt.Maximum = 0;
            DrawText();
            UpdateCharInfo();
            enable = true;
        }
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            textBoxes[(int)textInt.Value].boxes[textBoxes[(int)textInt.Value].boxes.Count - 1].data[textBoxes[(int)textInt.Value].boxes[textBoxes[(int)textInt.Value].boxes.Count - 1].data.Length - 1] &= 0x7F;
            Box box = new Box();
            byte[] data = new byte[] { 0xB9, 0x20 | 0x80 };
            box.data = data;
            textBoxes[(int)textInt.Value].boxes.Add(box);
            boxInt.Maximum += 1;
            enable = false;
            charInt.Value = 0;
            charInt.Maximum = 0;
            DrawText();
            UpdateCharInfo();
            enable = true;
        }

        private void ExportArcButton_Click(object sender, RoutedEventArgs e)
        {
            using(var fd = new System.Windows.Forms.OpenFileDialog())
            {
                fd.Filter = "ARC |*.ARC";
                fd.Title = "Select the Player ARC file you would like to export to.";
                if(fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    ARC fileARC = new ARC(File.ReadAllBytes(fd.FileName));
                    if (!fileARC.ContainsEntry(0x15))
                    {
                        MessageBox.Show("There is no Text data in this ARC file to overwrite.");
                        return;
                    }

                    //Prep Export
                    int freeOffset = textBoxes.Count * 2;
                    List<byte[]> dataSlots = new List<byte[]>();

                    MemoryStream ms = new MemoryStream();
                    BinaryWriter bw = new BinaryWriter(ms);

                    foreach (var text in textBoxes)
                    {
                        byte[] data = text.CreateTextData();
                        dataSlots.Add(data);
                        if(freeOffset > 0xFFFF)
                        {
                            MessageBox.Show("Error File Bigger than 64KB");
                            return;
                        }
                        bw.Write((ushort)freeOffset);
                        freeOffset += data.Length;
                    }
                    foreach (var array in dataSlots)
                        bw.Write(array);
                    byte[] entry = ms.ToArray();


                    //Keep everything Word Aligned
                    int remainder = entry.Length % 4;
                    if (remainder != 0)
                        Array.Resize(ref entry, entry.Length + (4 - remainder));

                    fileARC.SaveEntry(0x15, entry); //Save Text Data
                    File.WriteAllBytes(fd.FileName, fileARC.GetEntriesData());

                    //Done
                    MessageBox.Show($"Text Data Exported to {fd.SafeFileName}!");
                }
            }
        }

        private void ExportEntryButton_Click(object sender, RoutedEventArgs e)
        {
            using(var sfd = new System.Windows.Forms.SaveFileDialog())
            {
                sfd.Filter = "BIN |*.BIN";
                sfd.Title = "Select Save Location";
                if(sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    //Prep Export
                    int freeOffset = textBoxes.Count * 2;
                    List<byte[]> dataSlots = new List<byte[]>();

                    MemoryStream ms = new MemoryStream();
                    BinaryWriter bw = new BinaryWriter(ms);

                    foreach (var text in textBoxes)
                    {
                        if (freeOffset > 0xFFFF)
                        {
                            MessageBox.Show("Error File Bigger than 64KB");
                            return;
                        }
                        bw.Write((ushort)freeOffset);
                    }
                    foreach (var array in dataSlots)
                        bw.Write(array);

                    byte[] entry = ms.ToArray();

                    //Keep everything Word Aligned
                    int remainder = entry.Length % 4;
                    if (remainder != 0)
                        Array.Resize(ref entry, entry.Length + (4 - remainder));
                    File.WriteAllBytes(sfd.FileName, entry);

                    //Done
                    MessageBox.Show("Entry Exported!");
                }
            }
        }
        private void AddCharButton_Click(object sender, RoutedEventArgs e)
        {
            if (charInt.Maximum == 0xFFFF)
                return;
            int index = (int)charInt.Maximum * 2;

            //New Size
            int lengthNew = textBoxes[(int)textInt.Value].boxes[(int)boxInt.Value].data.Length + 2;
            Array.Resize(ref textBoxes[(int)textInt.Value].boxes[(int)boxInt.Value].data, lengthNew);
            charInt.Maximum = (lengthNew / 2) - 1;

            //End flags
            byte lastChar = textBoxes[(int)textInt.Value].boxes[(int)boxInt.Value].data[index + 1];
            lastChar &= 0x80 + 0x20;
            textBoxes[(int)textInt.Value].boxes[(int)boxInt.Value].data[index + 1] = lastChar;

            textBoxes[(int)textInt.Value].boxes[(int)boxInt.Value].data[index + 2] = 0xB9;
            
            if(boxInt.Maximum == boxInt.Value)
                textBoxes[(int)textInt.Value].boxes[(int)boxInt.Value].data[index + 3] = 0x80 + 0x20;
            else
                textBoxes[(int)textInt.Value].boxes[(int)boxInt.Value].data[index + 3] = 0x20;

            enable = false;
            DrawText();
            UpdateCharInfo();
            enable = true;

        }
        private void RemoveCharButton_Click(object sender, RoutedEventArgs e)
        {
            if (charInt.Maximum == 0)
                return;

            //New Size
            int lengthNew = textBoxes[(int)textInt.Value].boxes[(int)boxInt.Value].data.Length - 2;
            Array.Resize(ref textBoxes[(int)textInt.Value].boxes[(int)boxInt.Value].data, lengthNew);
            charInt.Maximum = (lengthNew / 2) - 1;

            int index = (int)charInt.Maximum * 2;

            //End flags
            byte lastChar = textBoxes[(int)textInt.Value].boxes[(int)boxInt.Value].data[index + 1];

            if (boxInt.Maximum == boxInt.Value)
                textBoxes[(int)textInt.Value].boxes[(int)boxInt.Value].data[index + 1] = (byte)(lastChar | (0x80 + 0x20));
            else
                textBoxes[(int)textInt.Value].boxes[(int)boxInt.Value].data[index + 1] = (byte)(lastChar | 0x20);

            enable = false;

            if (charInt.Value > charInt.Maximum)
                charInt.Value = charInt.Maximum;

            DrawText();
            UpdateCharInfo();
            enable = true;
        }
        private void AutoEditButton_Click(object sender, RoutedEventArgs e)
        {
            ListWindow textWindow = new ListWindow();
            textWindow.Title = "INPUT TEXT";
            textWindow.Width = 315;
            textWindow.Height = 160;
            textWindow.ResizeMode = ResizeMode.NoResize;
            textWindow.scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;

            List<TextBox> uiBoxes = new List<TextBox>();
            uiBoxes.Add(new TextBox() { MaxLength = MaxLineLength, MaxLines = 1, Foreground = Brushes.White , Background = Brushes.Black , FontSize = 18});
            uiBoxes.Add(new TextBox() { MaxLength = MaxLineLength, MaxLines = 1, Foreground = Brushes.White, Background = Brushes.Black, FontSize = 18 });
            uiBoxes.Add(new TextBox() { MaxLength = MaxLineLength, MaxLines = 1, Foreground = Brushes.White, Background = Brushes.Black, FontSize = 18 });

            List<CheckBox> scrollChecks = new List<CheckBox>();
            scrollChecks.Add(new CheckBox() { Content = "Scroll", FontSize = 18, Focusable = false});
            scrollChecks.Add(new CheckBox() { Content = "Scroll", FontSize = 18, Focusable = false, IsChecked = true });
            scrollChecks.Add(new CheckBox() { Content = "Scroll", FontSize = 18, Focusable = false, IsChecked = true });

            textWindow.outGrid.RowDefinitions.Add(new RowDefinition());
            textWindow.outGrid.RowDefinitions.Add(new RowDefinition());
            textWindow.outGrid.RowDefinitions[1].Height = new GridLength(30);

            textWindow.grid.ColumnDefinitions.Add(new ColumnDefinition());
            textWindow.grid.ColumnDefinitions.Add(new ColumnDefinition());
            textWindow.grid.ColumnDefinitions[0].Width = new GridLength(75);

            for (int i = 0; i < uiBoxes.Count; i++)
            {
                Grid.SetColumn(uiBoxes[i], 1);
                Grid.SetRow(uiBoxes[i], i);
                Grid.SetColumn(scrollChecks[i], 0);
                Grid.SetRow(scrollChecks[i], i);

                textWindow.grid.RowDefinitions.Add(new RowDefinition());
                textWindow.grid.Children.Add(uiBoxes[i]);
                textWindow.grid.Children.Add(scrollChecks[i]);
            }

            Button confirm = new Button() { Content  = "Confirm"};
            confirm.Click += (s, arg) => //Convert Char to VRAM XY
            {
                if (uiBoxes[0].Text == "" && uiBoxes[1].Text == "" && uiBoxes[2].Text == "")
                    return;

                MemoryStream ms = new MemoryStream();
                BinaryWriter bw = new BinaryWriter(ms);

                int finalLine = -1;
                for (int i = 0; i < 3; i++) //Check for valid Char
                {
                    if (uiBoxes[i].Text == "") continue;

                    finalLine = i;
                    foreach (var c in uiBoxes[i].Text)
                    {
                        if (!charTable.ContainsKey(c))
                        {
                            MessageBox.Show("The char '" + c + "' on line " + (i + 1) + " is not a valid character");
                            return;
                        }
                    }
                }

                for (int i = 0; i < 3; i++) //Create Data
                {
                    if (uiBoxes[i].Text == "") continue;

                    for (int c = 0; c < uiBoxes[i].Text.Length; c++)
                    {
                        bw.Write(charTable[uiBoxes[i].Text[c]]); //Write Char

                        byte flags = 0;

                        if(c + 1 == uiBoxes[i].Text.Length) //Last Char on Line
                        {

                            if (i == finalLine)
                            {
                                if (boxInt.Value == boxInt.Maximum) //End
                                    flags = 0x80 + 0x20;
                                else
                                    flags = 0x20;
                            }
                            else
                            {
                                if ((bool)scrollChecks[i].IsChecked)
                                    flags = 0x10;
                                if (i != finalLine) //New Line Flag
                                    flags |= 0x40;
                            }


                        }
                        else
                        {
                            if ((bool)scrollChecks[i].IsChecked)
                                flags = 0x10;
                        }
                        bw.Write(flags);
                    }
                }

                //Done
                textBoxes[(int)textInt.Value].boxes[(int)boxInt.Value].data = ms.ToArray();
                enable = false;
                charInt.Value = 0;
                charInt.Maximum = (textBoxes[(int)textInt.Value].boxes[(int)boxInt.Value].data.Length / 2) - 1;
                DrawText();
                UpdateCharInfo();
                enable = true;
                textWindow.Close();
            };
            Grid.SetRow(confirm, 1);
            textWindow.outGrid.Children.Add(confirm);

            textWindow.ShowDialog();
        }
        private void textTextureImg_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var p = e.GetPosition(textTextureImg);
            int x = (int)p.X;
            int y = (int)p.Y;
            int cX = Level.GetSelectedTile(x, textTextureImg.ActualWidth, 16);
            int cY = Level.GetSelectedTile(y, textTextureImg.ActualHeight, 16);
            ushort val = BitConverter.ToUInt16(textBoxes[(int)textInt.Value].boxes[(int)boxInt.Value].data, (int)(charInt.Value * 2));

            val = (ushort)((val & 0xFF00) + cX + (cY << 4));
            BitConverter.GetBytes(val).CopyTo(textBoxes[(int)textInt.Value].boxes[(int)boxInt.Value].data, (int)(charInt.Value * 2));

            enable = false;
            UpdateCharInfo();
            DrawText();
            enable = true;
        }
        private void textInt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue == null || e.OldValue == null || !enable) return;
            enable = false;
            boxInt.Value = 0;
            boxInt.Maximum = textBoxes[(int)e.NewValue].boxes.Count - 1;
            charInt.Value = 0;
            charInt.Maximum = (textBoxes[(int)e.NewValue].boxes[0].data.Length / 2) - 1;
            DrawText();
            UpdateCharInfo();
            enable = true;
        }

        private void boxInt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue == null || e.OldValue == null || !enable) return;
            enable = false;
            charInt.Value = 0;
            charInt.Maximum = (textBoxes[(int)textInt.Value].boxes[(int)e.NewValue].data.Length / 2) - 1;
            DrawText();
            UpdateCharInfo();
            enable = true;
        }
        private void charInt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue == null || e.OldValue == null || !enable) return;
            enable = false;
            DrawText();
            UpdateCharInfo();
            enable = true;
        }
        private void endCheck_CheckChange(object sender, RoutedEventArgs e)
        {
            if (!enable) return;
            ushort val = (ushort)(int)valInt.Value;

            valInt.Value = val ^ 0x8000;
        }

        private void lineCheck_CheckChange(object sender, RoutedEventArgs e)
        {
            if (!enable) return;
            ushort val = (ushort)(int)valInt.Value;

            valInt.Value = val ^ 0x4000;
        }

        private void newCheck_CheckChange(object sender, RoutedEventArgs e)
        {
            if (!enable) return;
            ushort val = (ushort)(int)valInt.Value;

            valInt.Value = val ^ 0x2000;
        }
        private void scrollCheck_CheckChange(object sender, RoutedEventArgs e)
        {
            if (!enable) return;
            ushort val = (ushort)(int)valInt.Value;

            valInt.Value = val ^ 0x1000;
        }
        private void valInt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!enable) return;
            BitConverter.GetBytes((ushort)(int)e.NewValue).CopyTo(textBoxes[(int)textInt.Value].boxes[(int)boxInt.Value].data, (int)(charInt.Value * 2));
            DrawText();
        }
        #endregion Events
    }
}
