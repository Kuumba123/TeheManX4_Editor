using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TeheManX4.Forms
{
    /// <summary>
    /// Interaction logic for ToolsWindow.xaml
    /// </summary>
    public partial class ToolsWindow : Window
    {
        #region Constructors
        public ToolsWindow()
        {
            InitializeComponent();
        }
        #endregion Constructors

        #region Events
        private void texBmpBtn_Click(object sender, RoutedEventArgs e)
        {
            using (var fd = new OpenFileDialog())
            {
                fd.Filter = "ARC |*.ARC";
                fd.Title = "Open an MegaMan X4 ARC Files";
                fd.Multiselect = true;
                if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var sfd = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
                    sfd.Description = "Select Save Location";
                    sfd.UseDescriptionForTitle = true;
                    if ((bool)sfd.ShowDialog())
                    {
                        List<string> files = new List<string>(); //Files with No Textures
                        List<string> invalid = new List<string>(); //Invalid ARC Files
                        BitmapPalette pal = new BitmapPalette(Const.GreyScale);


                        //Loop Throught each Selected ARC File
                        for (int f = 0; f < fd.FileNames.Length; f++)
                        {
                            byte[] file = File.ReadAllBytes(fd.FileNames[f]);
                            ARC arc = new ARC(file);
                            int i = 0; //Texture Count

                            if(arc.entries.Count == 0)
                            {
                                invalid.Add(fd.SafeFileNames[f]);
                                continue;
                            }

                            //Check each Entry
                            foreach (var en in arc.entries)
                            {
                                if (en.type >> 16 != 1)
                                    continue;
                                try
                                {
                                    BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                                    byte[] b = new byte[en.data.Length];
                                    Array.Copy(en.data, 0, b, 0, en.data.Length);
                                    var bmp = new WriteableBitmap(256, en.data.Length / 128, 96, 96, PixelFormats.Indexed4, pal);

                                    Level.ConvertBmp(b);

                                    bmp.WritePixels(new Int32Rect(0, 0, 256, en.data.Length / 128), b, 128, 0);
                                    encoder.Frames.Add(BitmapFrame.Create(bmp));
                                    var s = File.Create(sfd.SelectedPath + "\\" + fd.SafeFileNames[f] + "_" + "TEX" + Convert.ToString(i, 16).PadLeft(2, '0') + ".bmp");
                                    encoder.Save(s);
                                    s.Close();
                                }
                                catch
                                {
                                    break;
                                }
                                i++;
                            }
                            if (i == 0)
                                files.Add(fd.SafeFileNames[f]);
                        }
                        //Extraction Completed
                        System.Windows.MessageBox.Show("Textures Extracted");

                        //Show Files with No Textures
                        if (files.Count != 0)
                        {
                            string s = null;
                            foreach (var f in files)
                            {
                                if (s != null)
                                    s += "," + f + " ";
                                else
                                    s = f + " ";
                            }
                            System.Windows.MessageBox.Show("The Following Files had no Textures in them:\n\n" + s);
                        }

                        //Show Invalid Files
                        if (invalid.Count != 0)
                        {
                            string s = null;
                            foreach (var f in invalid)
                            {
                                if (s != null)
                                    s += "," + f + " ";
                                else
                                    s = f + " ";
                            }
                            System.Windows.MessageBox.Show("The Following Files were not Valid ARC Files:\n\n" + s);
                        }
                    }
                }
            }
        }

        private void texBinBtn_Click(object sender, RoutedEventArgs e)
        {
            using (var fd = new OpenFileDialog())
            {
                fd.Filter = "ARC |*.ARC";
                fd.Title = "Open an MegaMan X4 ARC File";
                if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var file = File.ReadAllBytes(fd.FileName);
                    var arc = new ARC(file);
                    if (arc.entries.Count == 0)
                    {

                    }
                    var sfd = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
                    sfd.Description = "Select Save Location";
                    sfd.UseDescriptionForTitle = true;
                    if ((bool)sfd.ShowDialog())
                    {
                        List<string> files = new List<string>(); //Files with No Textures

                        int i = 0;
                        //Check each Entry
                        foreach (var en in arc.entries)
                        {
                            if (en.type >> 8 != 1)
                                continue;
                            File.WriteAllBytes(sfd.SelectedPath + "\\" + fd.SafeFileName + "_" + "TEX" + Convert.ToString(i, 16).PadLeft(2, '0') + ".BIN", en.data);
                            i++;
                        }
                        //Extraction Completed
                        System.Windows.MessageBox.Show("Textures Extracted");
                        if (files.Count != 0)
                        {
                            string s = "";
                            foreach (var f in files)
                            {
                                s = "," + f + " ";
                            }
                            var c = s.ToCharArray();
                            s = new string(c, 1, s.Length - 1);
                            System.Windows.MessageBox.Show("The Following Files had no Textures in them:\n\n" + s);
                        }
                    }
                }
            }
        }

        private void inertTexBtn_Click(object sender, RoutedEventArgs e)
        {
            using(var fd = new OpenFileDialog())
            {
                fd.Filter = "ARC |*.ARC";
                fd.Title = "Open an MegaMan X4 ARC File that contains Textures";
                if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    ARC pac = new ARC(File.ReadAllBytes(fd.FileName));
                    pac.path = fd.FileName;
                    if(pac.entries.Count == 0)
                    {
                        System.Windows.MessageBox.Show("This is an invalid ARC file");
                        return;
                    }
                    int amount = 0;
                    foreach (Entry en in pac.entries)
                    {
                        if (en.type >> 16 != 1)
                            continue;
                        amount++;
                    }
                    if (amount == 0)
                    {
                        System.Windows.MessageBox.Show("There are no textures in\n" + fd.SafeFileName);
                        return;
                    }
                    var lw = new ListWindow(pac, 0);
                    lw.Title = fd.SafeFileName + " Textures";
                    lw.ShowDialog(); ;
                }
            }
        }

        private void EditCompressedTexturesButton_Click(object sender, RoutedEventArgs e)
        {
            using (var fd = new OpenFileDialog())
            {
                fd.Filter = "ARC |*.ARC";
                fd.Title = "Open an MegaMan X4 ARC File that contains Compressed Textures";
                if(fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    ARC arc = new ARC(File.ReadAllBytes(fd.FileName));
                    if (arc.entries.Count == 0)
                    {
                        System.Windows.MessageBox.Show("This is an invalid ARC file");
                        return;
                    }
                    if (!arc.ContainsEntry(2))
                    {
                        System.Windows.MessageBox.Show("This arc file contains NO Compressed Textures");
                    }
                    ListWindow list = new ListWindow();
                    list.CompressedTextures(arc);
                    list.ShowDialog();
                }

            }
        }
        private void xaExtBtn_Click(object sender, RoutedEventArgs e)
        {
            using(var fd = new OpenFileDialog())
            {
                fd.Filter = "XA |*.XA";
                fd.Title = "Open an PSX XA File";
                if(fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    XA xa = new XA(File.ReadAllBytes(fd.FileName));
                    if(xa.channels.Count == 0)
                    {
                        System.Windows.MessageBox.Show("Invalid XA File");
                        return;
                    }
                    var sfd = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
                    sfd.Multiselect = false;
                    sfd.Description = "Select the folder containing extracted MegaMan X4 Game Files";
                    sfd.UseDescriptionForTitle = true;

                    if ((bool)sfd.ShowDialog())
                    {
                        for (int i = 0; i < xa.channels.Count; i++)
                        {
                            xa.AssignChannel((byte)i, 0);
                            File.WriteAllBytes(sfd.SelectedPath + "\\" + fd.SafeFileName + "_" + "XA" + Convert.ToString(i, 16).PadLeft(2, '0').ToUpper() + ".XA", xa.channels[i]);
                        }
                        System.Windows.MessageBox.Show("Extraction Completed");
                    }
                }
            }
        }
        private void gameSettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            using (var fd = new OpenFileDialog())
            {
                fd.Filter = "PSX-EXE |*61";
                fd.Title = "Select MegaMan X4 PSX-EXE";
                if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    GameSettingsWindow s = new GameSettingsWindow(fd.FileName);
                    s.Show();
                }
            }
        }
        private void sprtBtn_Click(object sender, RoutedEventArgs e)
        {
            SpriteEditor s = new SpriteEditor();
            s.ShowDialog();
        }
        private void replaceBtn_Click(object sender, RoutedEventArgs e)
        {
            using (var fd = new OpenFileDialog())
            {
                fd.Filter = "ISO |*BIN";
                fd.Title = "Open an ISO 9660 File";
                if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    PSX.OpenFileBrowser(fd.FileName);
                }
            }
        }
        private void vabExtBtn_Click(object sender, RoutedEventArgs e)
        {
            using (var fd = new OpenFileDialog())
            {
                fd.Filter = "ARC |*ARC";
                fd.Title = "Open an MegaMan X4 ARC File";
                fd.Multiselect = true;
                if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var sfd = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
                    sfd.Description = "Select VAB Files Save Location";
                    sfd.UseDescriptionForTitle = true;
                    if ((bool)sfd.ShowDialog())
                    {
                        string name = null;
                        try
                        {
                            int total = 0;
                            for (int f = 0; f < fd.FileNames.Length; f++)
                            {
                                ARC arc = new ARC(File.ReadAllBytes(fd.FileNames[f]));
                                List<byte[]> headers = new List<byte[]>();
                                List<byte[]> bodys = new List<byte[]>();
                                name = fd.FileNames[f];
                                if (arc.entries.Count == 0) continue; //Valid ARC check

                                foreach (var en in arc.entries)
                                {
                                    if ((en.type >> 16) != 2 && en.type != 4 && en.type != 6 && en.type != 7 && en.type != 8)
                                        continue;

                                    if (en.type == 4 || en.type == 6 || en.type == 7 || en.type == 8)
                                    {
                                        total++;
                                        MemoryStream ms = new MemoryStream(en.data);
                                        BinaryReader br = new BinaryReader(ms);
                                        br.BaseStream.Position = BitConverter.ToInt32(en.data, 0) & 0xFFFFFF;
                                        headers.Add(br.ReadBytes((int)(en.data.Length - br.BaseStream.Position)));
                                    }
                                    else
                                        bodys.Add(en.data);
                                }
                                for (int i = 0; i < bodys.Count; i++)
                                {
                                    byte[] vabFile = new byte[bodys[i].Length + headers[i].Length];
                                    headers[i].CopyTo(vabFile, 0);
                                    bodys[i].CopyTo(vabFile, headers[i].Length);
                                    File.WriteAllBytes(sfd.SelectedPath + "\\" + fd.SafeFileNames[f] + "_" + "VAB" + Convert.ToString(i, 16).PadLeft(2, '0') + ".vab", vabFile);
                                }
                            }
                            if (total != 0)
                                System.Windows.MessageBox.Show("VAB Export Completed");
                            else
                                System.Windows.MessageBox.Show("There were no VAB files in this ARC file.");
                        }
                        catch (Exception ex)
                        {
                            if (name != null)
                                System.Windows.MessageBox.Show(ex.Message + "\nThe error was on: " + name, "ERROR");
                            else
                                System.Windows.MessageBox.Show(ex.Message);
                        }
                    }
                }
            }
        }
        private void vabInsertBtn_Click(object sender, RoutedEventArgs e)
        {
            using (var fd = new OpenFileDialog())
            {
                fd.Filter = "ARC |*ARC";
                fd.Title = "Open an MegaMan X4 ARC File containing Sound Data";

                if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    ARC arc = new ARC(File.ReadAllBytes(fd.FileName));
                    List<byte[]> headers = new List<byte[]>();
                    List<byte[]> bodys = new List<byte[]>();
                    List<int> indexes = new List<int>();
                    for (int i = 0; i < arc.entries.Count; i++)
                    {
                        if (arc.entries[i].type == 4 || arc.entries[i].type == 6 || arc.entries[i].type == 7 || arc.entries[i].type == 8)
                        {
                            headers.Add(arc.entries[i].data);
                            bodys.Add(arc.entries[i + 1].data);
                            indexes.Add(i);
                            i += 2;
                        }
                    }
                    if (headers.Count == 0)
                    {
                        System.Windows.MessageBox.Show("There are is no Sound Data in this ARC file.");
                        return;
                    }

                    //Valid File
                    NumInt selectInt = new NumInt();
                    selectInt.Value = 0;
                    selectInt.Minimum = 0;
                    selectInt.Maximum = headers.Count - 1;
                    selectInt.FontSize = 28;
                    selectInt.ButtonSpinnerWidth = 20;
                    System.Windows.Controls.Label lbl = new System.Windows.Controls.Label()
                    {
                        Content = "VAB File",
                        Foreground = Brushes.White,
                        FontSize = 28
                    };

                    System.Windows.Controls.Button confirm = new System.Windows.Controls.Button()
                    {
                        Content = "Confirm"
                    };
                    confirm.Click += (s, arg) =>
                    {
                        using (var fd2 = new OpenFileDialog())
                        {
                            fd2.Filter = "VAB |*VAB";
                            fd2.Title = "Select an PSX VAB File";
                            if (fd2.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            {
                                byte[] vab = File.ReadAllBytes(fd2.FileName);
                                //Get VH & VB
                                ushort programC = BitConverter.ToUInt16(vab, 18);
                                int vbOffset = 512 + 0x820 + (32 * 16 * programC);

                                byte[] vabHeader = new byte[vbOffset];
                                Array.Copy(vab, vabHeader, vbOffset);

                                byte[] vabBody = new byte[vab.Length - vbOffset];
                                Array.Copy(vab, vbOffset, vabBody, 0, vabBody.Length);

                                //Save ARC File
                                arc.entries[indexes[(int)selectInt.Value] + 1].data = vabBody;

                                int address = BitConverter.ToInt32(arc.entries[indexes[(int)selectInt.Value]].data, 0) & 0xFFFFFF;
                                MemoryStream ms = new MemoryStream(arc.entries[indexes[(int)selectInt.Value]].data);
                                StreamWriter sw = new StreamWriter(ms);
                                sw.BaseStream.Position = address;
                                sw.Write(vabHeader);
                                arc.entries[indexes[(int)selectInt.Value]].data = ms.ToArray();

                                File.WriteAllBytes(fd.FileName, arc.GetEntriesData());
                                System.Windows.MessageBox.Show("ARC VAB Data Saved!");
                            }
                        }
                    };


                    //Setup Form
                    ListWindow win = new ListWindow();
                    win.Title = fd.SafeFileName + " VAB Files";
                    win.Width = 330;
                    win.Height = 180;
                    win.ResizeMode = ResizeMode.NoResize;
                    win.grid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());
                    win.grid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());
                    win.grid.ColumnDefinitions[0].Width = new GridLength(80);
                    win.outGrid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition());
                    win.outGrid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition());
                    win.outGrid.RowDefinitions[1].Height = new GridLength(35);

                    System.Windows.Controls.Grid.SetColumn(lbl, 1);
                    System.Windows.Controls.Grid.SetRow(confirm, 1);

                    win.grid.Children.Add(selectInt);
                    win.grid.Children.Add(lbl);
                    win.outGrid.Children.Add(confirm);

                    win.ShowDialog();
                }
            }
        }
        private void editSoundsBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("currently not finished.");
            return;
#if false
            using (var fd = new OpenFileDialog())
            {
                fd.Filter = "ARC |*ARC";
                fd.Title = "Open an MegaMan X4 ARC File containing Sound Data";

                if(fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    ARC arc = new ARC(File.ReadAllBytes(fd.FileName));
                    List<byte[]> headers = new List<byte[]>();
                    List<byte[]> bodys = new List<byte[]>();

                    for (int i = 0; i < arc.entries.Count; i++)
                    {
                        if (arc.entries[i].type == 4 || arc.entries[i].type == 6 || arc.entries[i].type == 7 || arc.entries[i].type == 8)
                        {
                            headers.Add(arc.entries[i].data);
                            bodys.Add(arc.entries[i + 1].data);
                            i += 2;
                        }

                        if(headers.Count == 0)
                        {
                            System.Windows.MessageBox.Show("There are is no Sound Data in this ARC file.");
                            return;
                        }

                    }
                }
            }
#endif
        }
        private void extractIsoBtn_Click(object sender, RoutedEventArgs e)
        {
            using (var fd = new OpenFileDialog())
            {
                fd.Filter = "ISO |*BIN";
                fd.Title = "Open an ISO 9660 File";
                if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (fd.SafeFileName.Contains("(Track "))
                    {
                        System.Windows.MessageBox.Show("Multi Track ISO's are not supported", "ERROR");
                        return;
                    }
                    var sfd = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
                    sfd.Description = "Select Files Save Location";
                    sfd.UseDescriptionForTitle = true;
                    if ((bool)sfd.ShowDialog())
                    {
                        PSX.Extract(fd.FileName, sfd.SelectedPath);
                    }
                }
            }
        }
        private void fixBtn_Click(object sender, RoutedEventArgs e)
        {
            using (var fd = new OpenFileDialog())
            {
                fd.Filter = "PSX-EXE |*61";
                fd.Title = "Select MegaMan X4 PSX-EXE";
                if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    using (var fd2 = new OpenFileDialog())
                    {
                        try
                        {
                            fd2.Filter = "C HEADER |*h";
                            fd2.Title = "Select C Header File Contain Files LBA";
                            if (fd2.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            {
                                string[] lines = File.ReadAllLines(fd2.FileName);
                                MemoryStream ms = new MemoryStream(File.ReadAllBytes(fd.FileName));
                                BinaryWriter bw = new BinaryWriter(ms);

                                foreach (var line in lines)
                                {
                                    if (line.Trim() == "")
                                        continue;
                                    string[] words = System.Text.RegularExpressions.Regex.Replace(line.Trim(), " {2,}", " ").Split();
                                    if (words[0] != "#define")
                                        continue;

                                    foreach (var file in Const.DiscFiles)
                                    {
                                        if (file != words[1])
                                            continue;
                                        int i = Array.FindIndex(Const.DiscFiles, x => x.Contains(file));
                                        i *= 12;
                                        i += PSX.CpuToOffset(Const.FileDataAddress, 0x80010000);
                                        bw.BaseStream.Position = i;

                                        int sector;
                                        if (words[2].Contains("0x"))
                                            sector = Convert.ToInt32(words[2], 16);
                                        else
                                            sector = Convert.ToInt32(words[2]);
                                        bw.Write(sector);
                                    }
                                }
                                //Save PSX.EXE
                                File.WriteAllBytes(fd.FileName, ms.ToArray());
                                System.Windows.MessageBox.Show("LBA Fixed !");
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Windows.MessageBox.Show(ex.Message);
                        }
                    }
                }
            }
        }
        private void createPacBtn_Click(object sender, RoutedEventArgs e)
        {
            ListWindow listWindow = new ListWindow(new ARC());
            listWindow.ShowDialog();
        }
        private void editPacBtn_Click(object sender, RoutedEventArgs e)
        {
            using (var fd = new OpenFileDialog())
            {
                fd.Filter = "|*.PAC;*ARC";
                fd.Title = "Open an MegaMan 8 PAC File or an MegaMan X4 ARC File";
                if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    ARC pac = new ARC(File.ReadAllBytes(fd.FileName));
                    if (pac.entries.Count == 0)
                    {
                        System.Windows.MessageBox.Show(pac.filename + " is an invalid PAC file", "ERROR");
                        return;
                    }
                    pac.filename = Path.GetFileName(fd.FileName);
                    ListWindow listWindow = new ListWindow(pac);
                    listWindow.ShowDialog();
                }
            }
        }
        private void editTextBtn_Click(object sender, RoutedEventArgs e)
        {
            //System.Windows.MessageBox.Show("currently not finished.");
            //return;
            using(var fd = new OpenFileDialog())
            {
                fd.Filter = "ARC |*ARC";
                fd.Title = "Open an MegaMan X4 ARC File that contains Text Data";
                if(fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    ARC arc = new ARC(File.ReadAllBytes(fd.FileName));
                    if(arc.entries.Count == 0)
                    {
                        System.Windows.MessageBox.Show(fd.SafeFileName + " is not a valid ARC File!");
                        return;
                    }
                    if (!arc.ContainsEntry(0x15))
                    {
                        System.Windows.MessageBox.Show("There is no Text Data in " + fd.SafeFileName);
                        return;
                    }
                    arc.filename = fd.SafeFileName;
                    TextEditor t = new TextEditor(arc);
                    t.ShowDialog();
                }
            }
        }
        #endregion Events
    }
}
