using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TeheManX4.Forms
{
    /// <summary>
    /// Interaction logic for ListWindow.xaml
    /// </summary>
    public partial class ListWindow : Window
    {
        #region Fields
        public static bool isAnime;
        public static bool checkpoingGo;
        public static bool screenViewOpen;
        //Screen Viewer
        public static double screenLeft = double.NaN;
        public static double screenTop = double.NaN;
        public static double screenWidth = -1;
        public static double screenHeight;
        public static int screenState;
        //Screen Flags Window
        public static bool extraOpen;
        public static double extraLeft = double.NaN;
        public static double extraTop = double.NaN;
        //Files Viewer
        public static bool fileViewOpen;
        public static double fileLeft = double.NaN;
        public static double fileTop = double.NaN;
        public static double fileWidth = -1;
        public static double fileHeight;
        public static int fileState;
        //Clut Tools
        public static double clutLeft;
        public static double clutTop;
        //TileSet Tools
        public static double tileLeft = double.NaN;
        public static double tileTop = double.NaN;

        public static string tab = "";
        public static int layoutOffset = 0;
        public static bool enemyOpen;
        #endregion

        #region Properties
        public int mode = -1;
        #endregion Properties

        #region Constructors
        public ListWindow(bool single = false) //For viewing Serial Loading
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.Title = "NOPS Output";
            this.Width = 460;
            this.Height = 400;
            this.ResizeMode = ResizeMode.NoResize;
            this.mode = 0;
            TextBox t = new TextBox();
            t.FontSize = 16;
            t.TextWrapping = TextWrapping.NoWrap;
            t.AcceptsReturn = true;
            t.IsReadOnly = true;
            t.Foreground = Brushes.White;
            t.Background = Brushes.Black;
            this.grid.Children.Add(t);

            DispatcherTimer dt = new DispatcherTimer();
            dt.Interval = TimeSpan.FromMilliseconds(1000 / 30);

            if (checkpoingGo)
            {
                dt.Tick += (s, e) =>
                {

                    if ((mode & 1) == 1) //Wait
                    {
                        if (Settings.nops.HasExited)
                            mode++;
                        return;
                    }
                    else if (mode == 20)
                    {
                        if (Settings.nops.HasExited)
                        {
                            Settings.nops.CancelOutputRead();
                            PSX.SerialCont();
                            mode = -1;
                            dt.Stop();
                            this.Close();
                        }
                    }
                    int index = PSX.levels[Level.Id].GetIndex();

                    switch (mode)
                    {
                        case 0:
                            PSX.SerialHalt();
                            Settings.nops.BeginOutputReadLine();
                            mode++;
                            break;

                        case 2: //Checkpoint Data
                            {
                                byte[] data = new byte[0x1020];
                                Array.Copy(PSX.exe, PSX.CpuToOffset(0x800f3314), data, 0, 0x1020);

                                Settings.nops.CancelOutputRead();
                                PSX.SerialWrite(0x800f3314, data);
                                this.Title = "Writting Checkpoint DATA";
                                Settings.nops.BeginOutputReadLine();
                                mode++;
                            }
                            break;

                        case 4: //Backgrounds Scroll Type
                            {
                                int o = index * 2 + PSX.CpuToOffset(Const.BackgroundTypeTableAddress);

                                Settings.nops.CancelOutputRead();
                                PSX.SerialWrite(PSX.OffsetToCpu(o), BitConverter.ToUInt16(PSX.exe, o));
                                this.Title = "Writting Scroll Type DATA";
                                Settings.nops.BeginOutputReadLine();
                                mode++;
                            }
                            break;

                        case 6: //Background & Object Priority
                            {
                                int o = index * 10 + PSX.CpuToOffset(Const.BackgroundSettingsAddress);
                                byte[] info = new byte[10];
                                Array.Copy(PSX.exe, o, info, 0, 10);

                                Settings.nops.CancelOutputRead();
                                PSX.SerialWrite(PSX.OffsetToCpu(o), info);
                                this.Title = "Writting Priority DATA";
                                Settings.nops.BeginOutputReadLine();
                                mode++;
                            }
                            break;

                        case 8: //GPU Command E1 Trans Setting
                            Settings.nops.CancelOutputRead();
                            PSX.SerialWrite((uint)(Const.TransSettingsAddress + index), PSX.exe[PSX.CpuToOffset(Const.TransSettingsAddress) + index]);
                            this.Title = "Writting GPU Command E1 Trans Info";
                            Settings.nops.BeginOutputReadLine();
                            mode++;
                            break;

                        case 10: //Peacock Spawn
                            {
                                int offset = PSX.CpuToOffset(Const.PeacockSpecialSpawnTableAddress);
                                byte[] data = new byte[12];
                                Array.Copy(PSX.exe, offset, data, 0, 12);
                                Settings.nops.CancelOutputRead();
                                this.Title = "Writting Peacock Spawn Table";
                                PSX.SerialWrite(Const.PeacockSpecialSpawnTableAddress, data);
                                Settings.nops.BeginOutputReadLine();
                                mode++;
                                break;
                            }
                        case 12: //Mushroom Spawn
                            {
                                int offset = PSX.CpuToOffset(Const.MushroomSpecialSpawnTableAddress);
                                byte[] data = new byte[16];
                                Array.Copy(PSX.exe, offset, data, 0, 16);

                                Settings.nops.CancelOutputRead();
                                PSX.SerialWrite(Const.MushroomSpecialSpawnTableAddress, data);
                                this.Title = "Writting Mushroom Spawn Table";
                                Settings.nops.BeginOutputReadLine();
                                mode++;
                                break;
                            }
                        case 14: //Refights Spawn
                            {
                                int offset = PSX.CpuToOffset(Const.RefightsSpecialSpawnTableAddress);
                                byte[] data = new byte[40];
                                Array.Copy(PSX.exe, offset, data, 0, 40);
                                this.Title = "Writting Refights Spawn Table";
                                Settings.nops.CancelOutputRead();
                                PSX.SerialWrite(Const.RefightsSpecialSpawnTableAddress, data);
                                Settings.nops.BeginOutputReadLine();
                                mode++;
                                break;
                            }
                        case 16: //Checkpoint
                            Settings.nops.CancelOutputRead();
                            PSX.SerialWrite(Const.CheckPointAddress, (byte)(int)MainWindow.window.spawnE.spawnInt.Value);
                            this.Title = "Writting Flags";
                            Settings.nops.BeginOutputReadLine();
                            mode++;
                            break;

                        case 18: //Teleport Flag
                            Settings.nops.CancelOutputRead();
                            PSX.SerialWrite(Const.ClearLevelAddress, 0xC0);
                            Settings.nops.BeginOutputReadLine();
                            mode++;
                            break;
                        default:
                            break;
                    }
                };
            }
            else
            {
                int clutLength = -1;
                if (!Level.zeroFlag && PSX.levels[Level.Id].clut_X != null)
                    clutLength = PSX.levels[Level.Id].clut_X.entries[0].data.Length;
                else if (Level.zeroFlag && PSX.levels[Level.Id].clut_Z != null)
                    clutLength = PSX.levels[Level.Id].clut_Z.entries[0].data.Length;

                dt.Tick += (s, e) =>
                {
                    if ((mode & 1) == 1) //Wait
                    {
                        if (Settings.nops.HasExited)
                            mode++;
                        return;
                    }
                    else if (mode == 50)    //Done
                    {
                        if (Settings.nops.HasExited)
                        {
                            Settings.nops.CancelOutputRead();
                            PSX.SerialCont();
                            mode = -1;
                            dt.Stop();
                            this.Close();
                        }
                    }
                    int index = PSX.levels[Level.Id].GetIndex();

                    switch (mode)
                    {
                        case 0:
                            PSX.SerialHalt();
                            Settings.nops.BeginOutputReadLine();
                            mode++;
                            break;

                        case 2: //Screen
                            if (single && tab != "screenTab")
                            {
                                mode += 4;
                                return;
                            }
                            Settings.nops.CancelOutputRead();
                            PSX.SerialWrite(Settings.levelScreenAddress, PSX.levels[Level.Id].screenData);
                            this.Title = "Writting Screen DATA";
                            Settings.nops.BeginOutputReadLine();
                            mode++;
                            break;

                        case 4: //Screen Backup
                            if(clutLength == -1)
                            {
                                mode += 2;
                                return;
                            }
                            Settings.nops.CancelOutputRead();
                            PSX.SerialWrite((uint)(Settings.levelSize + Settings.levelStartAddress) + 0x1000, PSX.levels[Level.Id].screenData);
                            this.Title = "Writting Backup Screen DATA";
                            Settings.nops.BeginOutputReadLine();
                            mode++;
                            break;

                        case 6: //Tile Info
                            if (single && tab != "x16Tab")
                            {
                                mode += 2;
                                return;
                            }
                            Settings.nops.CancelOutputRead();
                            PSX.SerialWrite(Settings.levelTileAddress, PSX.levels[Level.Id].tileInfo);
                            this.Title = "Writting Tile Info DATA";
                            Settings.nops.BeginOutputReadLine();
                            mode++;
                            break;

                        case 8: //Clut
                            if (single && tab != "clutTab")
                            {
                                mode += 4;
                                return;
                            }
                            if (!Level.zeroFlag && PSX.levels[Level.Id].clut_X != null)
                            {
                                Settings.nops.CancelOutputRead();
                                PSX.SerialWrite((uint)(Settings.levelSize + Settings.levelStartAddress), PSX.levels[Level.Id].clut_X.entries[0].data);
                                this.Title = "Writting CLUT DATA";
                                Settings.nops.BeginOutputReadLine();
                                mode++;
                            }
                            else if (Level.zeroFlag && PSX.levels[Level.Id].clut_Z != null)
                            {
                                Settings.nops.CancelOutputRead();
                                PSX.SerialWrite((uint)(Settings.levelSize + Settings.levelStartAddress), PSX.levels[Level.Id].clut_Z.entries[0].data);
                                this.Title = "Writting CLUT DATA";
                                Settings.nops.BeginOutputReadLine();
                                mode++;
                            }
                            else
                                mode += 4;
                            break;

                        case 10:
                            Settings.nops.CancelOutputRead();
                            PSX.SerialWrite(Const.UpdateClutAddress, (byte)1);
                            Settings.nops.BeginOutputReadLine();
                            mode++;
                            break;

                        case 12: //Layout
                            if (single && tab != "layoutTab")
                            {
                                mode += 4;
                                return;
                            }
                            Settings.nops.CancelOutputRead();
                            PSX.SerialWrite(Const.LayoutBufferAddress, PSX.levels[Level.Id].layout);
                            this.Title = "Writting Layout DATA";
                            Settings.nops.BeginOutputReadLine();
                            mode++;
                            break;

                        case 14:
                            {
                                byte[] data = new byte[0x3D58];
                                Array.Copy(PSX.exe, Const.LayoutDataPointersOffset, data, 0, data.Length);

                                Settings.nops.CancelOutputRead();
                                PSX.SerialWrite(PSX.OffsetToCpu(Const.LayoutDataPointersOffset), data);
                                Settings.nops.BeginOutputReadLine();
                                mode++;
                            }
                            break;

                        case 16: //Update BG Flags
                            Settings.nops.CancelOutputRead();
                            PSX.SerialWrite(Const.UpdateLayer1Address, (byte)1);
                            this.Title = "Writting Updating Layers Flags";
                            Settings.nops.BeginOutputReadLine();
                            mode++;
                            break;

                        case 18:
                            Settings.nops.CancelOutputRead();
                            PSX.SerialWrite(Const.UpdateLayer1Address + 0x54, (byte)1);
                            Settings.nops.BeginOutputReadLine();
                            mode++;
                            break;

                        case 20:
                            Settings.nops.CancelOutputRead();
                            PSX.SerialWrite(Const.UpdateLayer1Address + 0x54 * 2, (byte)1);
                            Settings.nops.BeginOutputReadLine();
                            mode++;
                            break;

                        case 22: //Enemy
                            if (index < 26)
                            {
                                if (single && tab != "enemyTab")
                                {
                                    mode += 10;
                                    return;
                                }
                                int indexC = index;
                                if (PSX.levels[Level.Id].isMid())
                                    indexC--;
                                int dummyPad = 8 * 4;
                                int id = PSX.levels[Level.Id].GetId();
                                if (id == 9 || id == 0xA)
                                    dummyPad = 8 * 2;

                                int enemyDataOffset = PSX.CpuToOffset(BitConverter.ToUInt32(PSX.exe, Const.EnemyDataPointersOffset + indexC * 4));
                                byte[] data = new byte[Const.MaxEnemies[id] * 8 + dummyPad];
                                Array.Copy(PSX.exe, enemyDataOffset, data, 0, Const.MaxEnemies[id] * 8 + dummyPad);

                                Settings.nops.CancelOutputRead();
                                PSX.SerialWrite(PSX.OffsetToCpu(enemyDataOffset), data);
                                this.Title = "Writting Enemy DATA";
                                Settings.nops.BeginOutputReadLine();
                                mode++;
                            }
                            else
                                mode += 10;
                            break;
                        case 24:
                            {
                                if (Level.enemyExpand)
                                {
                                    mode += 2;
                                    break;
                                }
                                int indexC = index;
                                if (PSX.levels[Level.Id].isMid())
                                    indexC--;

                                Settings.nops.CancelOutputRead();
                                PSX.SerialWrite(PSX.OffsetToCpu(Const.EnemyDataPointersOffset + indexC * 4), BitConverter.ToInt32(PSX.exe, Const.EnemyDataPointersOffset + indexC * 4));
                                Settings.nops.BeginOutputReadLine();
                                mode++;
                                break;
                            }

                        case 26:
                            {
                                int indexC = index;
                                if (PSX.levels[Level.Id].isMid())
                                    indexC--;
                                Settings.nops.CancelOutputRead();
                                PSX.SerialWrite(PSX.OffsetToCpu(Const.StartEnemyDataPointersOffset + indexC * 4), BitConverter.ToInt32(PSX.exe, Const.StartEnemyDataPointersOffset + indexC * 4));
                                Settings.nops.BeginOutputReadLine();
                                mode++;
                                break;
                            }

                        case 28:
                            if (PSX.levels[Level.Id].GetId() != 9 && PSX.levels[Level.Id].GetId() != 0xA)
                            {
                                if (Level.enemyExpand)
                                {
                                    mode += 2;
                                    break;
                                }
                                int indexC = index;
                                if (!PSX.levels[Level.Id].isMid())
                                    indexC++;
                                if (PSX.levels[Level.Id].arc.filename == "ST0B_0X.ARC")
                                    index++;

                                Settings.nops.CancelOutputRead();
                                PSX.SerialWrite(PSX.OffsetToCpu(Const.EnemyDataPointersOffset + indexC * 4), BitConverter.ToInt32(PSX.exe, Const.EnemyDataPointersOffset + indexC * 4));
                                Settings.nops.BeginOutputReadLine();
                                mode++;
                            }
                            else
                                mode += 4;
                            break;

                        case 30:
                            {
                                int indexC = index;
                                if (!PSX.levels[Level.Id].isMid())
                                    indexC++;
                                if (PSX.levels[Level.Id].arc.filename == "ST0B_0X.ARC")
                                    index++;

                                Settings.nops.CancelOutputRead();
                                PSX.SerialWrite(PSX.OffsetToCpu(Const.StartEnemyDataPointersOffset + indexC * 4), BitConverter.ToInt32(PSX.exe, Const.StartEnemyDataPointersOffset + indexC * 4));
                                Settings.nops.BeginOutputReadLine();
                                mode++;
                            }
                            break;

                        case 32: //Checkpoint Data
                            {
                                if (single && tab != "spawnTab" && tab != "bgTab")
                                {
                                    mode += 14;
                                    return;
                                }
                                byte[] data = new byte[0x1020];
                                Array.Copy(PSX.exe, PSX.CpuToOffset(0x800f3314), data, 0, 0x1020);

                                Settings.nops.CancelOutputRead();
                                PSX.SerialWrite(0x800f3314, data);
                                this.Title = "Writting Checkpoint DATA";
                                Settings.nops.BeginOutputReadLine();
                                mode++;
                            }
                            break;

                        case 34: //Backgrounds Scroll Type
                            {
                                int o = index * 2 + PSX.CpuToOffset(Const.BackgroundTypeTableAddress);

                                Settings.nops.CancelOutputRead();
                                PSX.SerialWrite(PSX.OffsetToCpu(o), BitConverter.ToUInt16(PSX.exe, o));
                                this.Title = "Writting Scroll Type DATA";
                                Settings.nops.BeginOutputReadLine();
                                mode++;
                            }
                            break;

                        case 36: //Background & Object Priority
                            {
                                int o = index * 10 + PSX.CpuToOffset(Const.BackgroundSettingsAddress);
                                byte[] info = new byte[10];
                                Array.Copy(PSX.exe, o, info, 0, 10);

                                Settings.nops.CancelOutputRead();
                                PSX.SerialWrite(PSX.OffsetToCpu(o), info);
                                this.Title = "Writting Priority DATA";
                                Settings.nops.BeginOutputReadLine();
                                mode++;
                            }
                            break;

                        case 38: //GPU Command E1 Trans Setting
                            Settings.nops.CancelOutputRead();
                            PSX.SerialWrite((uint)(Const.TransSettingsAddress + index), PSX.exe[PSX.CpuToOffset(Const.TransSettingsAddress) + index]);
                            this.Title = "Writting GPU Command E1 Trans Info";
                            Settings.nops.BeginOutputReadLine();
                            mode++;
                            break;

                        case 40: //Peacock Spawn
                            {
                                int offset = PSX.CpuToOffset(Const.PeacockSpecialSpawnTableAddress);
                                byte[] data = new byte[12];
                                Array.Copy(PSX.exe, offset, data, 0, 12);

                                Settings.nops.CancelOutputRead();
                                PSX.SerialWrite(Const.PeacockSpecialSpawnTableAddress, data);
                                Settings.nops.BeginOutputReadLine();
                                mode++;
                                break;
                            }
                        case 42: //Mushroom Spawn
                            {
                                int offset = PSX.CpuToOffset(Const.MushroomSpecialSpawnTableAddress);
                                byte[] data = new byte[16];
                                Array.Copy(PSX.exe, offset, data, 0, 16);

                                Settings.nops.CancelOutputRead();
                                PSX.SerialWrite(Const.MushroomSpecialSpawnTableAddress, data);
                                Settings.nops.BeginOutputReadLine();
                                mode++;
                                break;
                            }
                        case 44: //Refights Spawn
                            {
                                int offset = PSX.CpuToOffset(Const.RefightsSpecialSpawnTableAddress);
                                byte[] data = new byte[40];
                                Array.Copy(PSX.exe, offset, data, 0, 40);

                                Settings.nops.CancelOutputRead();
                                PSX.SerialWrite(Const.RefightsSpecialSpawnTableAddress, data);
                                Settings.nops.BeginOutputReadLine();
                                mode++;
                                break;
                            }
                        case 46: //Camera
                            if (PSX.levels[Level.Id].GetIndex() < 26)
                            {
                                if (single && tab != "camTab")
                                {
                                    mode += 2;
                                    return;
                                }
                                byte[] data = new byte[0x7FC];
                                Array.Copy(PSX.exe, PSX.CpuToOffset(Const.CameraTriggerFreeDataAddress), data, 0, data.Length);

                                Settings.nops.CancelOutputRead();
                                PSX.SerialWrite(Const.CameraTriggerFreeDataAddress, data);
                                Settings.nops.BeginOutputReadLine();
                                mode++;
                            }
                            else
                                mode += 2;
                            break;

                        case 48:
                            if (Level.enemyExpand)
                            {
                                if (single && tab != "enemyTab")
                                {
                                    mode += 2;
                                    return;
                                }
                                if (!Level.enemyExpand)
                                {
                                    mode += 2;
                                    return;
                                }
                                Settings.nops.CancelOutputRead();
                                PSX.SerialWrite(Settings.levelEnemyAddress, Level.CreateEnemyData(PSX.levels[Level.Id].enemies));
                                this.Title = "Writting Expanded Enemy Data";
                                Settings.nops.BeginOutputReadLine();
                                mode++;
                            }
                            else
                                mode += 2;
                            break;
                        default:
                            break;
                    }

                };
            }

            dt.Start();
        }
        public ListWindow(ARC pac) //For Editing PAC files
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            //Form Prep
            AddGrids(2, 1);
            foreach (var e in pac.entries)
                pannel.Children.Add(new ArcEntry(e));
            if (pac.filename != null)
                this.Title = "Now editing - " + pac.filename;
            else
                this.Title = "Now editing - NEW ARC FILE";
            this.Width += 100;
            this.Height += 200;
            this.outGrid.RowDefinitions.Add(new RowDefinition());
            this.outGrid.RowDefinitions.Add(new RowDefinition());
            this.outGrid.RowDefinitions.Add(new RowDefinition());
            this.outGrid.RowDefinitions[1].Height = GridLength.Auto;
            this.outGrid.RowDefinitions[2].Height = new GridLength(60);

            Separator gridSplitter = new Separator()
            {
                Background = Brushes.Gray,
                Height = 5
            };
            Grid.SetRow(gridSplitter, 1);
            Button addBtn = new Button()
            {
                Content = "Add",
                Width = 80,
                Height = 30,
                Margin = new Thickness(5, 2, 4, 0)
            };
            addBtn.Click += (s, e) =>
            {
                ArcEntry p = new ArcEntry();
                p.pathBox.Text = "...";
                pannel.Children.Add(p);
            };

            Button saveAsBtn = new Button()
            {
                Content = "Save As",
                Width = 80,
                Height = 30,
                Margin = new Thickness(5, 2, 4, 0)
            };
            saveAsBtn.Click += (s, e) =>
            {
                if (pannel.Children.Count == 1) //No Pac Entries
                {
                    MessageBox.Show("You must have atleast 1 PAC entry before you can save the file");
                    return;
                }
                ARC newPAC = new ARC(); //New ouput PAC

                foreach (var c in pannel.Children) //Validation
                {
                    if (c.GetType() != typeof(ArcEntry))
                        continue;

                    ArcEntry pacE = c as ArcEntry;
                    if (pacE.entry.data == null)
                    {
                        MessageBox.Show("Not all entries contain data");
                        return;
                    }
                    newPAC.entries.Add(pacE.entry);
                }
                //Time to Export new PAC file
                System.Windows.Forms.SaveFileDialog fd = new System.Windows.Forms.SaveFileDialog();
                fd.Filter = "ARC Files (*.ARC)|*.ARC|PAC Files (*.PAC)|*.PAC";

                if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    pac.entries.Clear();
                    foreach (var c in pannel.Children)
                    {
                        if (c.GetType() != typeof(ArcEntry))
                            continue;

                        ArcEntry arcEntry = c as ArcEntry;

                        Entry entry = new Entry();
                        entry.type = arcEntry.entry.type;
                        entry.data = arcEntry.entry.data;
                        pac.entries.Add(entry);
                    }
                    File.WriteAllBytes(fd.FileName, pac.GetEntriesData());
                    MessageBox.Show("ARC/PAC Saved!");
                }
            };
            Grid.SetRow(saveAsBtn, 2);

            CheckBox msbCheck = new CheckBox() //Sega Saturn support
            {
                Content = "Is MSB",
                FontSize = 18,
                VerticalAlignment = VerticalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                IsChecked = pac.isMSB
            };
            RoutedEventHandler r = new RoutedEventHandler((s,e) => { pac.isMSB = (bool)msbCheck.IsChecked; return; });
            msbCheck.Checked += r;
            msbCheck.Unchecked += r;
            StackPanel stackP = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            Grid.SetRow(stackP, 2);

            //Add Contents to Pannel
            this.outGrid.Children.Add(stackP);
            this.outGrid.Children.Add(gridSplitter);
            stackP.Children.Add(addBtn);
            stackP.Children.Add(msbCheck);
            stackP.Children.Add(saveAsBtn);
        }
        public ListWindow(ARC arc,int type) //General Textures
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.Width = 804;
            this.Height = 812;
            int count = 0;
            List<Color> grayscalePalette = new List<Color>();

            for (int i = 0; i < 256; i++)
                grayscalePalette.Add(Color.FromRgb((byte)i, (byte)i, (byte)i));
            BitmapPalette grayscale8bpp = new BitmapPalette(grayscalePalette);
            //Create Context Menu
            ContextMenu contextMenu = new ContextMenu();

            //Specfic Items
            MenuItem item1 = new MenuItem() { Header = "Swap Bit-Depth" };
            item1.Click += (send, arg) =>
            {
                Image clickedImage = contextMenu.PlacementTarget as Image;
                int slot = Convert.ToInt32(clickedImage.Uid);
                ARC filePAC = (ARC)clickedImage.Tag;

                int width;
                PixelFormat destFormat;
                BitmapPalette pal;
                byte[] existingData = new byte[filePAC.entries[slot].data.Length];
                filePAC.entries[slot].data.CopyTo(existingData, 0);

                if (clickedImage.Source.Width == 256)
                {
                    width = 128;
                    destFormat = PixelFormats.Indexed8;
                    pal = grayscale8bpp;
                }
                else
                {
                    width = 256;
                    destFormat = PixelFormats.Indexed4;
                    pal = Const.GreyScalePallete;
                }

                int height = filePAC.entries[slot].data.Length / 128;

                if (width == 256)
                    Level.ConvertBmp(existingData);

                clickedImage.Source = BitmapSource.Create(width,
                height,
                96,
                96,
                destFormat,
                pal,
                existingData,
                128);
            };
            MenuItem item2 = new MenuItem() { Header = "Show Info" };
            item2.Click += (send, arg) =>
            {
                Image clickedImage = contextMenu.PlacementTarget as Image;
                string x;
                string y;
                string endY;
                int id = Convert.ToInt32(clickedImage.Name.Replace("_", ""));
                int endId = (id >> 8) & 0xFF;
                int cordId = id & 0xFF;
                int height = (int)clickedImage.Source.Height;
                if (cordId > Const.CordTabe.Length - 1)
                {
                    x = "?";
                    y = "?";
                }
                else
                {
                    x = (Const.CordTabe[cordId] & 0xFFFF).ToString();
                    y = (Const.CordTabe[cordId] >> 16).ToString();
                }

                if (endId == 0)
                    endY = "\n(Keeps going til it hits the end of the vertical page)";
                else if (endId == 1)
                    endY = "\n(Goes back up after Drawing 176 vertical pixels of the Texture)";
                else if (endId == 2)
                    endY = "\n(Goes back up to Y:176 after Drawing 80 vertical pixels)";
                else
                    endY = "";

                //Display Info
                MessageBox.Show("X: " + x + " Y: " + y + $"\nWidth: {clickedImage.Source.Width} Height: " + height + endY, "Texture Info");
            };
            MenuItem item3 = new MenuItem() { Header = "Extract" };
            item3.Click += (send, arg) =>
            {
                using(var sfd = new System.Windows.Forms.SaveFileDialog())
                {
                    sfd.Title = "Select Texture Save Location";
                    if (type == 0)
                        sfd.Filter = "BMP File|*.bmp";
                    else
                        sfd.Filter = "BIN File|*.bin";

                    if(sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        Image im = contextMenu.PlacementTarget as Image;
                        int id = Convert.ToInt32(im.Uid);
                        ARC filePAC = (ARC)im.Tag;

                        if (type == 0)
                        {
                            ImageSource imageSource = im.Source;

                            BitmapEncoder bmpEncoder = new BmpBitmapEncoder();
                            bmpEncoder.Frames.Add(BitmapFrame.Create((BitmapSource)imageSource));

                            var s = File.Create(sfd.FileName);
                            bmpEncoder.Save(s);
                            s.Close();
                        }
                        else
                            File.WriteAllBytes(sfd.FileName, filePAC.entries[id].data);

                        MessageBox.Show("Texture Data Extracted!");
                    }
                }
            };

            contextMenu.Items.Add(item1);
            contextMenu.Items.Add(item2);
            contextMenu.Items.Add(item3);

            for (int i = 0; i < arc.entries.Count; i++) //Go Through each Entry
            {
                if (arc.entries[i].type >> 16 != 1)
                    continue;
                //Fancy Border
                var b = new Border();
                if ((count & 1) == 0)
                    b.BorderBrush = Brushes.Red;
                else
                    b.BorderBrush = Brushes.BlueViolet;
                b.BorderThickness = new Thickness(2);
                count++;

                //Setup Texture
                Image image = new Image();
                //Define LEFT & RIGHT Click Events
                MouseButtonEventHandler p1 = (s, e) =>
                {
                    using (var fd = new System.Windows.Forms.OpenFileDialog())
                    {
                        byte[] data;
                        var im = (Image)s;
                        if (type == 0)
                        {
                            if(im.Source.Width == 256)
                            {
                                fd.Filter = "4bpp BMP |*.BMP";
                                fd.Title = "Open an 4bpp Bitmap";
                            }
                            else
                            {
                                fd.Filter = "8bpp BMP |*.BMP";
                                fd.Title = "Open an 8bpp Bitmap";
                            }
                        }
                        else
                        {
                            fd.Filter = "BIN |*BIN";
                            fd.Title = "Open an BIN File";
                        }
                        if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            if (type == 0) //BMP
                            {
                                Uri uri = new Uri(fd.FileName);
                                WriteableBitmap tex = new WriteableBitmap(new BitmapImage(uri));
                                Uri.EscapeUriString(fd.FileName);

                                if (tex.PixelWidth != im.Source.Width)
                                {
                                    MessageBox.Show($"Texture must\nHave a width of {im.Source.Width} pixels.", "ERROR");
                                    return;
                                }

                                if(im.Source.Width == 256)
                                {
                                    if (tex.Format != PixelFormats.Indexed4)
                                    {
                                        MessageBox.Show("Texture must be in 4bpp.", "ERROR");
                                        return;
                                    }
                                }
                                else
                                {
                                    if (tex.Format != PixelFormats.Indexed8)
                                    {
                                        MessageBox.Show("Texture must be in 8bpp.", "ERROR");
                                        return;
                                    }
                                }

                                data = new byte[256 * tex.PixelHeight / 2];
                                tex.CopyPixels(data, 128, 0);

                                if(im.Source.Width == 256)
                                    Level.ConvertBmp(data);

                                //Get PAC Info
                                int id = Convert.ToInt32(im.Uid);
                                ARC filePAC = (ARC)im.Tag;
                                Array.Resize(ref arc.entries[id].data, data.Length);
                                Array.Copy(data, arc.entries[id].data, data.Length);
                                File.WriteAllBytes(arc.path, arc.GetEntriesData());
                                im.Source = tex;
                            }
                            else //BIN
                            {
                                data = File.ReadAllBytes(fd.FileName);

                                //Get PAC Info
                                var id = Convert.ToInt32(im.Uid);
                                var filePAC = (ARC)im.Tag;
                                Array.Resize(ref arc.entries[id].data, data.Length);
                                Array.Copy(data, arc.entries[id].data, data.Length);
                                File.WriteAllBytes(arc.path, arc.GetEntriesData());

                                Level.ConvertBmp(data);
                                WriteableBitmap tex = new WriteableBitmap(256, data.Length / 128, 96, 96, PixelFormats.Indexed4, new BitmapPalette(Const.GreyScale));
                                tex.WritePixels(new Int32Rect(0, 0, 256, data.Length / 128), data, 128, 0);
                                im.Source = tex;
                            }
                        }
                    }
                };
                MouseButtonEventHandler p2 = (s, e) =>
                {
                    contextMenu.PlacementTarget = s as Image;
                    contextMenu.IsOpen = true;
                };
                image.MouseLeftButtonDown += p1;
                image.MouseRightButtonDown += p2;
                image.Tag = arc;
                image.Uid = i.ToString();   //For Getting PAC data
                image.Name = "_" + arc.entries[i].type.ToString(); //For XY

                //Draw Texture
                byte[] p = new byte[arc.entries[i].data.Length];
                Array.Copy(arc.entries[i].data, p, p.Length);
                Level.ConvertBmp(p);

                var bmp = new WriteableBitmap(256, p.Length / 128, 96, 96, PixelFormats.Indexed4, Const.GreyScalePallete);
                bmp.WritePixels(new Int32Rect(0, 0, 256, p.Length / 128), p, 128, 0);
                //Add to Window
                image.Source = bmp;
                b.Child = image;
                pannel.Children.Add(b);
            }
        }
        public ListWindow() //Dummy
        {
            InitializeComponent();
        }
        public ListWindow(int action) //Various
        {
            Action[] acts = new Action[] { ScreenGrid, LayoutEdit, ExtraGrid, EnemyTools, FileViewer , ClutTools , CheckpointEdit , CameraEdit };
            InitializeComponent();
            mode = -action;
            acts[action]();
        }
        #endregion Constructors

        #region Methods
        private void ScreenGrid()
        {
            screenViewOpen = true;
            this.Width = 1060;
            this.Height = 934;
            this.Title = "All Screens in Layer " + (Level.BG + 1);
            this.scroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
            if(screenWidth != -1)
            {
                this.Left = screenLeft;
                this.Top = screenTop;
                this.Width = screenWidth;
                this.Height = screenHeight;
                if(this.WindowState != (WindowState)screenState)
                {
                    this.Loaded += (s, e) =>
                    {
                        this.WindowState = (WindowState)screenState;
                    };
                }
            }
            this.AddGrids(PSX.levels[Level.Id].width, PSX.levels[Level.Id].height);

            //Add Buttons to Grid
            for (int y = 0; y < PSX.levels[Level.Id].height; y++)
            {
                for (int x = 0; x < PSX.levels[Level.Id].width; x++)
                {
                    Button b = new Button();
                    b.Margin = new Thickness(1);
                    b.Padding = new Thickness(1);
                    b.Width = 30;
                    b.Click += (s, e) =>
                    {
                        MainWindow.window.layoutE.viewerX = Grid.GetColumn(s as UIElement) << 8;
                        MainWindow.window.layoutE.viewerY = Grid.GetRow(s as UIElement) << 8;
                        MainWindow.window.UpdateViewrCam();
                        MainWindow.window.layoutE.DrawLayout();
                    };
                    b.MouseRightButtonDown += (s, e) =>
                    {
                        MainWindow.window.enemyE.viewerX = Grid.GetColumn(s as UIElement) << 8;
                        MainWindow.window.enemyE.viewerY = Grid.GetRow(s as UIElement) << 8;
                        MainWindow.window.enemyE.ReDraw();
                    };
                    b.Content = Convert.ToString(PSX.levels[Level.Id].layout[x + (y * PSX.levels[Level.Id].width) + Level.BG * PSX.levels[Level.Id].size], 16).ToUpper();
                    Grid.SetColumn(b, x);
                    Grid.SetRow(b, y);
                    grid.Children.Add(b);
                }
            }
        }
        public void EditScreen(int x,int y)
        {
            foreach (var c in grid.Children)
            {
                if (c.GetType() != typeof(Button))
                    continue;
                Button button = c as Button;
                if(Grid.GetColumn(button) == x && Grid.GetRow(button) == y)
                {
                    int i = PSX.levels[Level.Id].size * Level.BG;
                    i += x + y * PSX.levels[Level.Id].width;
                    button.Content = Convert.ToString(PSX.levels[Level.Id].layout[i], 16).ToUpper();
                    return;
                }
            }
        }
        private void LayoutEdit()
        {
            this.Title = "Set New Screen ID";
            this.Width = 330;
            this.Height = 90;
            this.scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            this.ResizeMode = ResizeMode.NoResize;
            System.Drawing.Point point = System.Windows.Forms.Control.MousePosition;
            this.Left = point.X - 180;
            this.Top = point.Y - 50;
            //Setup INT UP/DOWN
            var integer = new NumInt();
            integer.Maximum = PSX.levels[Level.Id].screenData.Length / 0x200;
            integer.Minimum = 0;
            integer.ButtonSpinnerWidth = 45;
            integer.ParsingNumberStyle = System.Globalization.NumberStyles.HexNumber;
            integer.FormatString = "X";
            integer.FontSize = 40;
            integer.TextAlignment = TextAlignment.Center;
            integer.AllowTextInput = true;
            //Set current screen Value
            integer.Value = PSX.levels[Level.Id].layout[layoutOffset];

            //Setup Confirm Button
            Button okBtn = new Button();
            okBtn.Content = "Modify";
            okBtn.Click += (s, e) =>
            {
                try
                {
                    //Edit & Save Undo
                    if (LayoutEditor.undos.Count == Const.MaxUndo)
                        LayoutEditor.undos.RemoveAt(0);
                    LayoutEditor.undos[Level.Id].Add(Undo.CreateLayoutUndo(layoutOffset));

                    PSX.levels[Level.Id].layout[layoutOffset] = (byte)integer.Value;
                    PSX.levels[Level.Id].edit = true;
                    MainWindow.window.layoutE.DrawLayout(true);
                    MainWindow.window.enemyE.Draw();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "ERROR");
                    return;
                }
                this.Close();
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            Grid.SetColumn(okBtn, 0);
            Grid.SetColumn(integer, 1);
            grid.Children.Add(integer);
            grid.Children.Add(okBtn);
        }
        private void ExtraGrid() //Change Extra flags in Screen
        {
            extraOpen = true;
            this.AddGrids(16, 16);
            this.scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            this.Width = 592;
            this.Height = 646;
            this.Title = "Screen: " + Convert.ToString(MainWindow.window.screenE.screenId, 16).ToUpper() + " Tile Flags";
            this.ResizeMode = ResizeMode.CanMinimize;
            if (!double.IsNaN(extraLeft))
            {
                this.Left = extraLeft;
                this.Top = extraTop;
            }

            //Add Top Bar Controls
            DockPanel dock = new DockPanel();
            Rectangle rect = new Rectangle()
            {
                Width = 10,
                Height = 10,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(5, 0, 5, 0),
                Fill = Brushes.Blue
            };
            Button bt1 = new Button()
            {
                Content = "Fill",
                Width = 55,
                Style = Application.Current.FindResource("TileButton") as Style
            }; //Fill
            bt1.Click += (s, e) =>
            {
                byte arg;
                if (rect.Fill == Brushes.Blue) //Trans or Priority
                    arg = 0x40;
                else
                    arg = 0x80;

                for (int i = 0; i < 0x100; i++)
                {
                    byte flag = PSX.levels[Level.Id].screenData[MainWindow.window.screenE.screenId * 0x200 + 1 + i * 2];
                    flag |= arg;
                    PSX.levels[Level.Id].screenData[MainWindow.window.screenE.screenId * 0x200 + 1 + i * 2] = flag;
                }
                PSX.levels[Level.Id].edit = true;
                DrawExtra();
            };
            Button bt2 = new Button()
            {
                Content = "Clear",
                Width = 55,
                Style = Application.Current.FindResource("TileButton") as Style
            }; //Clear
            bt2.Click += (s, e) =>
            {
                byte arg;
                if (rect.Fill == Brushes.Blue) //Trans or Priority
                    arg = 0x40;
                else
                    arg = 0x80;

                for (int i = 0; i < 0x100; i++)
                {
                    byte flag = PSX.levels[Level.Id].screenData[MainWindow.window.screenE.screenId * 0x200 + 1 + i * 2];
                    flag &= (byte)(arg ^ 0xFF);
                    PSX.levels[Level.Id].screenData[MainWindow.window.screenE.screenId * 0x200 + 1 + i * 2] = flag;
                }
                PSX.levels[Level.Id].edit = true;
                DrawExtra();
            };
            Button bt3 = new Button()
            {
                Content = "Toggle",
                Width = 65,
                Style = Application.Current.FindResource("TileButton") as Style
            }; //Toggle
            bt3.Click += (s, e) =>
            {
                if (rect.Fill == Brushes.Blue)
                    rect.Fill = Brushes.Red;
                else
                    rect.Fill = Brushes.Blue;
            };
            Button bt4 = new Button()
            {
                Content = "Show Collision",
                Width = 115,
                Style = Application.Current.FindResource("TileButton") as Style
            }; //Show Collision
            bt4.Click += (s, e) =>
            {
                MainWindow.window.ToggleCollision();
            };
            Button bt5 = new Button()
            {
                Content = "Tilesets",
                Width = 70,
                Style = Application.Current.FindResource("TileButton") as Style
            };
            bt5.Click += (s, e) =>
            {
                if(MainWindow.window.screenE.screenCursor.Visibility == Visibility.Hidden)
                {
                    MessageBox.Show("You must select a group of tiles in the current screen " +
                        "to be able to use these tile-set tools.");
                    return;
                }

                ListWindow tileWin = new ListWindow();
                tileWin.ResizeMode = ResizeMode.NoResize;
                tileWin.Title = "Export Tileset";
                tileWin.scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                tileWin.Height = 375;
                if (!double.IsNaN(tileLeft))
                {
                    tileWin.Left = tileLeft;
                    tileWin.Top = tileTop;
                }
                tileWin.Closing += (ss, ee) =>
                {
                    tileLeft = tileWin.Left;
                    tileTop = tileWin.Top;
                };
                CheckBox includeStructCheck = new CheckBox()
                {
                    Content = "Include Struct",
                    FontSize = 16,
                    VerticalContentAlignment = VerticalAlignment.Center
                };

                CheckBox includeTableCheck = new CheckBox()
                {
                    Content = "Include Tile Table",
                    FontSize = 16,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    IsChecked = true
                };
                CheckBox infoCheck = new CheckBox()
                {
                    Content = "Include Info",
                    FontSize = 16,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    IsChecked = true
                };
                Label nameLbl = new Label()
                {
                    Content = "Base Name",
                    FontSize = 18,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                TextBox nameBox = new TextBox()
                {
                    Text = PSX.levels[Level.Id].arc.filename.Replace(".ARC", ""),
                    FontSize = 18,
                    HorizontalContentAlignment = HorizontalAlignment.Center
                };
                Label labelX = new Label()
                {
                    Content = "Base X",
                    FontSize = 18,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                NumInt intX = new NumInt()
                {
                    ShowButtonSpinner = false,
                    FontSize = 18,
                    TextAlignment = TextAlignment.Center
                };
                Label labelY = new Label()
                {
                    Content = "Base Y",
                    FontSize = 18,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                NumInt intY = new NumInt()
                {
                    ShowButtonSpinner = false,
                    FontSize = 18,
                    TextAlignment = TextAlignment.Center
                };
                Label layerLbl = new Label()
                {
                    Content = "Layer",
                    FontSize = 18,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                NumInt layerInt = new NumInt()
                {
                    ShowButtonSpinner = false,
                    FontSize = 18,
                    TextAlignment = TextAlignment.Center
                };

                string str;

                Button export = new Button()
                {
                    Content = "Export as C"
                };
                export.Click += (sen, arg) =>
                {
                    if (intX.Value == null || intY.Value == null || layerInt.Value == null)
                    {
                        MessageBox.Show("Invalid Parameters", "ERROR");
                        return;
                    }
                    using (var sfd = new System.Windows.Forms.SaveFileDialog())
                    {
                        sfd.FileName = $"{nameBox.Text}.c";
                        sfd.Title = "Select Tileset Save Location";
                        sfd.Filter = "C Source File |*.c";
                        if(sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            int scX = Grid.GetColumn(MainWindow.window.screenE.screenCursor);
                            int scY = Grid.GetRow(MainWindow.window.screenE.screenCursor);
                            int cols = Grid.GetColumnSpan(MainWindow.window.screenE.screenCursor);
                            int rows = Grid.GetRowSpan(MainWindow.window.screenE.screenCursor);
                            str = Level.CreateTilesetString((int)intX.Value, (int)intY.Value, scX, scY, cols, rows, (byte)(int)layerInt.Value, (bool)includeStructCheck.IsChecked, (bool)includeTableCheck.IsChecked, nameBox.Text, (bool)infoCheck.IsChecked);

                            File.WriteAllText(sfd.FileName, str);
                            MessageBox.Show("Tileset Exported!");
                        }
                    }
                };

                Button clipboardC = new Button()
                {
                    Content = "Copy to Clipboard as C"
                };
                clipboardC.Click += (sen, arg) =>
                {
                    if(intX.Value == null || intY.Value == null || layerInt.Value == null)
                    {
                        MessageBox.Show("Invalid Parameters", "ERROR");
                        return;
                    }
                    int scX = Grid.GetColumn(MainWindow.window.screenE.screenCursor);
                    int scY = Grid.GetRow(MainWindow.window.screenE.screenCursor);
                    int cols = Grid.GetColumnSpan(MainWindow.window.screenE.screenCursor);
                    int rows = Grid.GetRowSpan(MainWindow.window.screenE.screenCursor);
                    str = Level.CreateTilesetString((int)intX.Value, (int)intY.Value, scX, scY, cols, rows, (byte)(int)layerInt.Value, (bool)includeStructCheck.IsChecked, (bool)includeTableCheck.IsChecked, nameBox.Text, (bool)infoCheck.IsChecked);
                    Clipboard.SetDataObject(str);
                    //File.WriteAllText("Test.txt", str);
                };

                tileWin.pannel.Children.Add(export);
                tileWin.pannel.Children.Add(clipboardC);
                tileWin.pannel.Children.Add(nameLbl);
                tileWin.pannel.Children.Add(nameBox);
                tileWin.pannel.Children.Add(labelX);
                tileWin.pannel.Children.Add(intX);
                tileWin.pannel.Children.Add(labelY);
                tileWin.pannel.Children.Add(intY);
                tileWin.pannel.Children.Add(layerLbl);
                tileWin.pannel.Children.Add(layerInt);

                tileWin.outGrid.RowDefinitions.Add(new RowDefinition());
                tileWin.outGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto});

                StackPanel stackP = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                stackP.Children.Add(includeStructCheck);
                stackP.Children.Add(includeTableCheck);
                stackP.Children.Add(infoCheck);
                Grid.SetRow(stackP, 1);

                tileWin.outGrid.Children.Add(stackP);

                if (!double.IsNaN(tileLeft))
                {
                    tileWin.Left = tileLeft;
                    tileWin.Top = tileTop;
                }

                tileWin.ShowDialog();
            };

            dock.Children.Add(bt1);
            dock.Children.Add(bt2);
            dock.Children.Add(bt3);
            dock.Children.Add(bt4);
            dock.Children.Add(bt5);

            Button texBtn = new Button()
            {
                Content = "8bpp Texture",
                Width = 105,
                Style = Application.Current.FindResource("TileButton") as Style
            };
            texBtn.Click += (s, e) =>
            {
                if (Level.textureSupport)
                {
                    MessageBox.Show("You already have the 8bpp Texture patch applied to the game.\nIf you want to use 8bpp textures just set the " +
                        "tile's T-page property to 8-B");
                    return;
                }
                else
                {
                    var result = MessageBox.Show("Would you like to apply the 8bpp Texture patch? " +
                        "Normally MegaMan X4 only supports 4bpp Textures for the Background Layers 1-3. " +
                        "This patch will allow you to use 8bpp Textures similarly to how MegaMan X5/X6 Support 8bpp Textures.\n" +
                        "This  will also expand the max amount of colors to 8192 (0x200 sets), " +
                        "however I would only set it to 0x140 sets at most. " +
                        "Would you like to apply the patch?", "8bpp Texture Patch" , MessageBoxButton.YesNo);
                    if (result != MessageBoxResult.Yes) return;

                    System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

                    Stream resourceStream = assembly.GetManifestResourceStream("TeheManX4.Resources.Patches.Texture_8bpp.drawbackgroundc.bin");
                    MemoryStream ms = new MemoryStream();

                    resourceStream.CopyTo(ms);
                    ms.ToArray().CopyTo(PSX.exe, PSX.CpuToOffset(0x80026648));

                    resourceStream = assembly.GetManifestResourceStream("TeheManX4.Resources.Patches.Texture_8bpp.setuppagesc.bin");
                    ms = new MemoryStream();

                    resourceStream.CopyTo(ms);
                    ms.ToArray().CopyTo(PSX.exe, PSX.CpuToOffset(0x80025da0));

                    System.Text.Encoding.ASCII.GetBytes("TEXTURE_8BPP").CopyTo(PSX.exe, PSX.CpuToOffset(0x800260e4));
                    byte[] clutRECT = new byte[] { 0, 0, 0xE0, 1, 0, 1, 0x20, 0 };
                    clutRECT.CopyTo(PSX.exe, PSX.CpuToOffset(0x800f1658));

                    //Update GUI
                    MainWindow.window.screenE.pageInt.Maximum = 0xB;
                    MainWindow.window.x16E.pageInt.Maximum = 0xB;
                    ClutEditor.maxPage = 0xB;
                    for (int i = 0; i < 4; i++)
                    {
                        ((Button)MainWindow.window.x16E.tPagePannel.Children[9 + i]).Visibility = Visibility.Visible;
                        ((Button)MainWindow.window.clutE.pagePannel.Children[9 + i]).Visibility = Visibility.Visible;
                    }
                    PSX.edit = true;
                    Level.textureSupport = true;

                    //Done
                    MessageBox.Show("8bpp Texture patch has been applied! Now hit the save and now when you want to use 8bpp Textures " +
                        $"set the tile's T-page property to 8-B. " +
                        "Also keep in mind that if the T-page property is set to 8-B the Clut property should be a multiple of " +
                        "10hex and should be no greater than 30hex (cause of the Clut layout in VRAM). " +
                        "Lastly if you want to change amount of colors in a level " +
                        " set the the clut set count in the size window.\n" +
                        $" You can also view the patch source at {Const.texture8bppURL}");
                }
            };
            dock.Children.Add(texBtn);

            dock.Children.Add(rect);
            this.pannel.Children.Insert(0, dock);

            //Add Grid
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    Rectangle r = new Rectangle();
                    Border b = new Border();
                    b.MouseDown += (s, e) => //Click event for changing flags
                    {
                        int col = Grid.GetColumn(s as UIElement);
                        int row = Grid.GetRow(s as UIElement);
                        byte flag = PSX.levels[Level.Id].screenData[MainWindow.window.screenE.screenId * 0x200 + 1 + (col * 2) + row * 32];
                        byte arg;
                        if (rect.Fill == Brushes.Blue) //Trans or Priority
                            arg = 0x40;
                        else
                            arg = 0x80;
                        Rectangle re = ((Border)s).Child as Rectangle;
                        if (e.ChangedButton == MouseButton.Right)
                        {
                            if ((flag & arg) == 0)
                                return;
                            flag &= (byte)(arg ^ 0xFF);
                            PSX.levels[Level.Id].screenData[MainWindow.window.screenE.screenId * 0x200 + 1 + (col * 2) + row * 32] = flag;
                            PSX.levels[Level.Id].edit = true;
                            if (arg == 0x40)
                                re.Fill = Brushes.Black;
                            else
                                ((Border)s).BorderBrush = Brushes.White;
                        }
                        else
                        {
                            if ((flag & arg) != 0)
                                return;
                            flag |= arg;
                            PSX.levels[Level.Id].screenData[MainWindow.window.screenE.screenId * 0x200 + 1 + (col * 2) + row * 32] = flag;
                            PSX.levels[Level.Id].edit = true;
                            if (arg == 0x40)
                                re.Fill = Brushes.Blue;
                            else
                                ((Border)s).BorderBrush = Brushes.Red;
                        }
                    };
                    b.MouseMove += (s, e) => //Ease of Use
                    {
                        if (e.RightButton == MouseButtonState.Pressed || e.LeftButton == MouseButtonState.Pressed)
                        {
                            Point p = e.GetPosition(grid);
                            HitTestResult result = VisualTreeHelper.HitTest(grid, p);
                            if (result != null)
                            {
                                int col = Grid.GetColumn(s as UIElement);
                                int row = Grid.GetRow(s as UIElement);
                                byte flag = PSX.levels[Level.Id].screenData[MainWindow.window.screenE.screenId * 0x200 + 1 + (col * 2) + row * 32];
                                byte arg;

                                if (rect.Fill == Brushes.Blue) //Trans or Priority
                                    arg = 0x40;
                                else
                                    arg = 0x80;
                                Border bd = s as Border;
                                Rectangle re2 = bd.Child as Rectangle;

                                if (e.RightButton == MouseButtonState.Pressed)
                                {
                                    if ((flag & arg) == 0)
                                        return;
                                    flag &= (byte)(arg ^ 0xFF);
                                    PSX.levels[Level.Id].screenData[MainWindow.window.screenE.screenId * 0x200 + 1 + (col * 2) + row * 32] = flag;
                                    PSX.levels[Level.Id].edit = true;
                                    if (arg == 0x40)
                                        re2.Fill = Brushes.Black;
                                    else
                                        ((Border)s).BorderBrush = Brushes.White;
                                }
                                else
                                {
                                    if ((flag & arg) != 0)
                                        return;
                                    flag |= arg;
                                    PSX.levels[Level.Id].screenData[MainWindow.window.screenE.screenId * 0x200 + 1 + (col * 2) + row * 32] = flag;
                                    PSX.levels[Level.Id].edit = true;
                                    if (arg == 0x40)
                                        re2.Fill = Brushes.Blue;
                                    else
                                        ((Border)s).BorderBrush = Brushes.Red;
                                }
                            }
                        }
                    };
                    b.BorderThickness = new Thickness(2);
                    r.Width = 32;
                    r.Height = 32;
                    ushort tile = BitConverter.ToUInt16(PSX.levels[Level.Id].screenData, MainWindow.window.screenE.screenId * 0x200 + (x * 2) + y * 32);

                    //Set flags
                    if ((tile & 0x4000) == 0x4000)
                        r.Fill = Brushes.Blue;
                    else
                        r.Fill = Brushes.Black;
                    if ((tile & 0x8000) == 0x8000)
                        b.BorderBrush = Brushes.Red;
                    else
                        b.BorderBrush = Brushes.White;

                    //Prep for adding to Form
                    b.Child = r;
                    Grid.SetColumn(b, x);
                    Grid.SetRow(b, y);
                    this.grid.Children.Add(b);
                }
            }
        }
        private void EnemyTools() //Start Enemies , etc
        {
            this.Title = PSX.levels[Level.Id].arc.filename + " Start Enemies: " + PSX.levels[Level.Id].startEnemies.Count.ToString();
            this.Width = 785;
            this.MaxWidth = 785;
            this.MinWidth = 785;
            this.Height = 585;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.scroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            //Rows
            this.outGrid.RowDefinitions.Add(new RowDefinition());
            this.outGrid.RowDefinitions.Add(new RowDefinition());
            this.outGrid.RowDefinitions[1].Height = new GridLength(80);
            //...

            Label nameLbl = new Label()
            {
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 20
            };

            ListView listView = new ListView();
            listView.Tag = nameLbl;
            listView.Background = Brushes.Black;
            foreach (var e in PSX.levels[Level.Id].startEnemies)
            {
                EnemyEntry entry = new EnemyEntry(e.id, e.var, e.type, e.range, e.x, e.y);
                entry.Tag = listView;
                listView.Items.Add(entry);
            }
            listView.SelectionChanged += (s, e) =>
            {
                if (listView.Items.Count != 0 && listView.SelectedItem != null)
                {
                    EnemyEntry entry = listView.SelectedItem as EnemyEntry;
                    nameLbl.Content = EnemyEditor.GetObjectName((byte)(int)entry.idInt.Value, (byte)(int)entry.typeInt.Value);
                }
                else
                    nameLbl.Content = "";
            };
            pannel.Children.Add(listView);

            //Remove
            Button rmvBtn = new Button()
            {
                Width = 90,
                Height = 40,
                FontSize = 20,
                Margin = new Thickness(5, 2, 5, 0),
                Content = "Remove"
            };
            rmvBtn.Click += (s, e) =>
            {
                if (listView.SelectedIndex == -1)
                    return;
                if (listView.SelectedItems.Count > 1)
                {
                    while (true)
                    {
                        if (listView.SelectedItems.Count == 0)
                            break;
                        listView.Items.RemoveAt(0);
                    }
                }
                else
                    listView.Items.Remove(listView.SelectedItem);
                this.Title = PSX.levels[Level.Id].arc.filename + " Start Enemies: " + listView.Items.Count.ToString();
                PSX.edit = true;
            };

            //Add
            Button addBtn = new Button()
            {
                Width = 50,
                Height = 40,
                FontSize = 20,
                Margin = new Thickness(5, 2, 5, 0),
                Content = "Add"
            };
            addBtn.Click += (s, e) =>
            {
                EnemyEntry entry = new EnemyEntry(3, 0, 3, 0, 0, 0);
                entry.Tag = listView;
                listView.Items.Add(entry);
                this.Title = PSX.levels[Level.Id].arc.filename + " Start Enemies: " + listView.Items.Count.ToString();
                PSX.edit = true;
            };

            //Expand
            Button expan = new Button()
            {
                Width = 130,
                Height = 40,
                FontSize = 20,
                Margin = new Thickness(5, 2, 5, 0),
                Content = "Expand Patch"
            };
            expan.Click += (s, e) =>
            {
                if (Level.enemyExpand)
                {
                    MessageBox.Show("You already have the expansion");
                    return;
                }
                else
                {
                    MessageBoxResult result = MessageBox.Show("Would you like to enable the enemy expansion patch? " +
                        "Normally the game keeps its enemy data in the PSX.EXE and theres a limit per 1st-half + 2nd-half. " +
                        "This patch will allow you to put up to 255 enemies per single level by using the unused ARC entry type 0x14.", "Expansion", MessageBoxButton.YesNo);
                    if (result != MessageBoxResult.Yes) return;

                    bool removeUnused = false;

                    result = MessageBox.Show("Would you also like to remove that unused ARC entry  type 0xB as well as fix " +
                        "the Arc entry order for the screen/tile-info data? This will help speed up loads and save CPU-RAM.", "Remove Unused", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                        removeUnused = true;

                    foreach (var l in PSX.levels)
                    {
                        int index = l.GetIndex();
                        if (index < 26 || l.arc.ContainsEntry(0xB))
                        {
                            ARC arc = l.arc;

                            if (removeUnused) //also fix entry 0 & 1 order
                            {
                                if (l.arc.ContainsEntry(0xB))
                                    arc.entries.RemoveAt(arc.GetIndexOfType(0xB));

                                byte[] screenData = arc.LoadEntry(0);
                                byte[] tileInfo = arc.LoadEntry(1);

                                arc.entries.RemoveAt(arc.GetIndexOfType(0));
                                arc.entries.RemoveAt(arc.GetIndexOfType(1));

                                arc.entries.Add(new Entry() { type = 0, data = screenData });
                                arc.entries.Add(new Entry() { type = 1, data = tileInfo });
                            }

                            if(index < 26)
                            {
                                while (l.enemies.Count > 255)
                                {
                                    int o = l.enemies.Count;
                                    l.enemies.RemoveAt(o);
                                }

                                arc.entries.Add(new Entry() { type = 0x14, data = Level.CreateEnemyData(l.enemies) });
                            }

                            //Edits Finished
                            l.edit = true;
                            l.arc = arc;
                        }
                    }
                    byte[] clearBIN = new byte[]
                    {
                    0x80, 0x1F, 0x02, 0x3C, 0x17, 0x80, 0x04, 0x3C, 0x44, 0x00, 0x43, 0x8C,
                    0xCC, 0x21, 0x82, 0x90, 0xCD, 0x21, 0x84, 0x90, 0x40, 0x10, 0x02, 0x00,
                    0x21, 0x10, 0x44, 0x00, 0x80, 0x10, 0x02, 0x00, 0x0F, 0x80, 0x04, 0x3C,
                    0x21, 0x10, 0x82, 0x00, 0xC8, 0x43, 0x43, 0xAC, 0xFF, 0x00, 0x04, 0x24,
                    0x03, 0x00, 0x62, 0x90, 0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x44, 0x14,
                    0x00, 0x00, 0x00, 0x00, 0x08, 0x00, 0xE0, 0x03, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x62, 0x90, 0x08, 0x00, 0x63, 0x24, 0x70, 0x00, 0x42, 0x30,
                    0xF6, 0xFF, 0x00, 0x10, 0xF8, 0xFF, 0x62, 0xA0
                    };
                    clearBIN.CopyTo(PSX.exe, PSX.CpuToOffset(0x80028db4));

                    //Done
                    PSX.edit = true;
                    Level.enemyExpand = true;
                    MainWindow.window.enemyE.DrawEnemies();
                    MessageBox.Show("Expansion Enabled! Make sure to hit save and rebuild the disc before using the Reload function.");
                }
            };

            //Delete All
            Button delete = new Button()
            {
                Width = 100,
                Height = 40,
                FontSize = 20,
                Margin = new Thickness(5, 2, 5, 0),
                Content = "Delete All"
            };
            delete.Click += (s, e) =>
            {
                var result = MessageBox.Show("Are you sure you want to delete all enemies in the level?\n(this also includes regular enemies)", "WARNING", MessageBoxButton.YesNo);
                if(result == MessageBoxResult.Yes)
                {
                    PSX.levels[Level.Id].enemies.Clear();
                    PSX.levels[Level.Id].startEnemies.Clear();
                    listView.Items.Clear();
                    this.Title = PSX.levels[Level.Id].arc.filename + " Start Enemies: 0";
                    MainWindow.window.enemyE.DrawEnemies();
                    PSX.levels[Level.Id].edit = true;
                }
            };

            //Export
            Button export = new Button()
            {
                Width = 75,
                Height = 40,
                FontSize = 20,
                Margin = new Thickness(5, 2, 5, 0),
                Content = "Export"
            };
            export.Click += (s, e) =>
            {
                using(var sfd = new System.Windows.Forms.SaveFileDialog())
                {
                    sfd.Filter = "JSON File|*.json";
                    sfd.Title = "Select the JSON Export Location for the regular & start Enemy Data";
                    if(sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        EnemyCollection collection = new EnemyCollection();
                        collection.enemies = PSX.levels[Level.Id].enemies;
                        collection.startEnemies = PSX.levels[Level.Id].startEnemies;

                        string json = JsonConvert.SerializeObject(collection, Formatting.Indented);
                        File.WriteAllText(sfd.FileName, json);
                        MessageBox.Show("Regular and Start Enemy Data Exported!");
                        Close();
                    }
                }
            };

            //Import
            Button import = new Button()
            {
                Width = 75,
                Height = 40,
                FontSize = 20,
                Margin = new Thickness(5, 2, 5, 0),
                Content = "Import"
            };
            import.Click += (s, e) =>
            {
                using (var fd = new System.Windows.Forms.OpenFileDialog())
                {
                    fd.Filter = "JSON |*json";
                    fd.Title = "Select the JSON file contaning the regular & start Enemy Data";
                    if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        EnemyCollection collection = JsonConvert.DeserializeObject<EnemyCollection>(File.ReadAllText(fd.FileName));
                        PSX.levels[Level.Id].startEnemies = collection.startEnemies;
                        PSX.levels[Level.Id].enemies = collection.enemies;

                        MessageBox.Show("Regular and Start Enemy Data Imported!");
                        MainWindow.window.enemyE.DrawEnemies();
                        
                        PSX.levels[Level.Id].edit = true;

                        if (Level.enemyExpand) PSX.levels[Level.Id].edit = true;
                        Close();
                    }
                }
            };

            StackPanel stackP = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            Grid.SetRow(stackP, 1);

            stackP.Children.Add(nameLbl);
            stackP.Children.Add(expan);
            stackP.Children.Add(export);
            stackP.Children.Add(import);
            stackP.Children.Add(delete);
            stackP.Children.Add(rmvBtn);
            stackP.Children.Add(addBtn);

            this.outGrid.Children.Add(stackP);
        }
        private void FileViewer() //For Viewing Game Files
        {
            InitializeComponent();
            this.Title = "Level Files";
            this.ResizeMode = ResizeMode.CanResize;
            this.Width = 290;
            this.Height = 500;
            this.MaxWidth = 290;
            this.MinWidth = 290;
            fileViewOpen = true;
            if (fileWidth != -1)
            {
                this.Left = fileLeft;
                this.Top = fileTop;
                this.Width = fileWidth;
                this.Height = fileHeight;
                if (this.WindowState != (WindowState)fileState)
                {
                    this.Loaded += (s, e) =>
                    {
                        this.WindowState = (WindowState)fileState;
                    };
                }
            }
            int i = 0;
            foreach (var l in PSX.levels)
            {
                Button b = new Button();
                b.Content = l.arc.filename.Replace("_","__");
                b.Click += (s, e) =>
                {
                    Level.Id = Convert.ToInt32(b.Uid);
                    Level.AssignPallete();
                    PSX.levels[Level.Id].LoadTextures();
                    MainWindow.window.Update();
                };
                b.Uid = i.ToString();
                i++;
                pannel.Children.Add(b);
            }
        }
        private void ClutTools()
        {
            this.Title = "CLUT TOOLS";

            this.ResizeMode = ResizeMode.NoResize;
            this.scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            if (!double.IsNaN(clutLeft))
            {
                this.Left = clutLeft;
                this.Top = clutTop;
            }
            int id;
            if (isAnime)
                id = AnimeEditor.clut;
            else
                id = ClutEditor.clut;

            Label exportLbl = new Label()
            {
                Content = "Export Set Count",
                FontSize = 18,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            NumInt exportInt = new NumInt()
            {
                Value = 1,
                FontSize = 18,
                Minimum = 1,
                Maximum = 128,
                ShowButtonSpinner = false,
                TextAlignment = TextAlignment.Center
            };
            int exportCount = 1;

            //Create Tool Buttons
            Button cpyBackground = new Button()
            {
                Content = "Copy Background Clut to "
            };
            if (!Level.zeroFlag)
                cpyBackground.Content += "Zero Clut";
            else
                cpyBackground.Content += "X Clut";
            cpyBackground.Click += (s, e) =>
            {
                if (!Level.zeroFlag)    //MegaMan X
                {
                    if (PSX.levels[Level.Id].clut_X == null)
                    {
                        MessageBox.Show("Could not find X's Clut");
                        return;
                    }
                    Array.Copy(PSX.levels[Level.Id].clut_X.entries[0].data, 2048, PSX.levels[Level.Id].clut_Z.entries[0].data, 2048, 2048);
                    PSX.levels[Level.Id].zeroEdit = true;
                    Level.AssignPallete();
                    MessageBox.Show("Background Clut Copied");


                    //Close Window
                    Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive).Close();
                }
                else //Zero
                {
                    if (PSX.levels[Level.Id].clut_Z == null)
                    {
                        MessageBox.Show("Could not find Zero's Clut");
                        return;
                    }
                    Array.Copy(PSX.levels[Level.Id].clut_Z.entries[0].data, 2048, PSX.levels[Level.Id].clut_X.entries[0].data, 2048, 2048);
                    PSX.levels[Level.Id].megaEdit = true;
                    Level.AssignPallete();
                    MessageBox.Show("Background Clut Copied");


                    //Close Window
                    Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive).Close();
                }
            };

            Button cpyObject = new Button()
            {
                Content = "Copy Object Clut to "
            };
            if (!Level.zeroFlag)
                cpyObject.Content += "Zero Clut";
            else
                cpyObject.Content += "X Clut";
            cpyObject.Click += (s, e) =>
            {
                if (!Level.zeroFlag)    //MegaMan X
                {
                    if (PSX.levels[Level.Id].clut_X == null)
                    {
                        MessageBox.Show("Could not find X's Clut");
                        return;
                    }
                    Array.Copy(PSX.levels[Level.Id].clut_X.entries[0].data, 768, PSX.levels[Level.Id].clut_Z.entries[0].data, 768, 1280);
                    PSX.levels[Level.Id].zeroEdit = true;
                    Level.AssignPallete();
                    MessageBox.Show("Object Clut Copied");


                    //Close Window
                    Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive).Close();
                }
                else //Zero
                {
                    if (PSX.levels[Level.Id].clut_Z == null)
                    {
                        MessageBox.Show("Could not find Zero's Clut");
                        return;
                    }
                    Array.Copy(PSX.levels[Level.Id].clut_Z.entries[0].data, 768, PSX.levels[Level.Id].clut_X.entries[0].data, 768, 1280);
                    PSX.levels[Level.Id].megaEdit = true;
                    MessageBox.Show("Object Clut Copied");


                    //Close Window
                    Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive).Close();
                }
            };

            Button importSet = new Button()
            {
                Content = $"Replace at Clut {Convert.ToString(id,16).ToUpper()} from Txt"
            };
            importSet.Click += (s, e) =>
            {
                using (var fd = new System.Windows.Forms.OpenFileDialog())
                {
                    try
                    {
                        fd.Filter = "Text File |*.TXT";
                        fd.Title = "Select the Text File containning your CLUT";
                        if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            string[] lines = File.ReadAllLines(fd.FileName);
                            List<Color> colors = new List<Color>();

                            foreach (var l in lines)
                            {
                                if (l.Trim() == "" || l.Trim() == "\n") continue;

                                uint val = Convert.ToUInt32(l.Replace("#", "").Trim(), 16);
                                Color color;
                                color = Color.FromRgb((byte)(val >> 16), (byte)((val >> 8) & 0xFF), (byte)(val & 0xFF));
                                colors.Add(color);
                            }
                            if(colors.Count < 16)
                            {
                                while (colors.Count < 16) colors.Add(Color.FromRgb(0, 0, 0));
                            }
                            int i = 0;
                            foreach (var c in colors)
                            {
                                int color = Level.To15Rgb(c.B, c.G, c.R);

                                if (isAnime)
                                {
                                    BitConverter.GetBytes((ushort)color).CopyTo(PSX.levels[Level.Id].clutAnime, AnimeEditor.clut * 32 + i * 2);
                                    PSX.levels[Level.Id].edit = true;
                                }
                                else
                                {
                                    if (!Level.zeroFlag)
                                    {
                                        BitConverter.GetBytes((ushort)color).CopyTo(PSX.levels[Level.Id].clut_X.entries[0].data, (i + (ClutEditor.clut + ClutEditor.bgF * 64) * 16) * 2);
                                        PSX.levels[Level.Id].megaEdit = true;
                                    }
                                    else
                                    {
                                        BitConverter.GetBytes((ushort)color).CopyTo(PSX.levels[Level.Id].clut_Z.entries[0].data, (i + (ClutEditor.clut + ClutEditor.bgF * 64) * 16) * 2);
                                        PSX.levels[Level.Id].zeroEdit = true;
                                    }
                                }
                                i++;
                            }
                            MessageBox.Show("Clut Imported!");

                            //Close Window
                            Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive).Close();

                            if (isAnime)
                            {
                                MainWindow.window.animeE.DrawClut();
                                return;
                            }

                            Level.AssignPallete();
                            //Updating the rest of GUI
                            MainWindow.window.clutE.DrawTextures();
                            MainWindow.window.clutE.DrawClut();
                            if (ClutEditor.bgF == 0)
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
                    }catch(Exception ex)
                    {
                        MessageBox.Show(ex.Message + ".\nClut might have still been modified");
                    }
                }
            };

            Button importPAL = new Button()
            {
                Content = $"Replace at Clut {Convert.ToString(id, 16).ToUpper()} from PAL"
            };
            importPAL.Click += (s, e) =>
            {
                using (var fd = new System.Windows.Forms.OpenFileDialog())
                {
                    try
                    {
                        fd.Filter = "YYCHR PAL File |*.PAL";
                        fd.Title = "Select the PAL File containning your CLUT";
                        if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            List<Color> colors = new List<Color>();
                            byte[] data = File.ReadAllBytes(fd.FileName);
                            int i = 0;
                            while (true)
                            {
                                Color color = Color.FromRgb(data[i], data[i + 1], data[i + 2]);
                                colors.Add(color);
                                i += 3;
                                if (i >= data.Length)
                                    break;
                            }
                            if (colors.Count < 16)
                            {
                                while (colors.Count < 16) colors.Add(Color.FromRgb(0, 0, 0));
                            }

                            if (isAnime)
                                i = AnimeEditor.clut * 32;
                            else
                                i = (ClutEditor.clut + (ClutEditor.bgF * 64)) * 16 * 2;

                            foreach (var c in colors)
                            {
                                int color = Level.To15Rgb(c.B, c.G, c.R);
                                if (isAnime)
                                {
                                    BitConverter.GetBytes((ushort)color).CopyTo(PSX.levels[Level.Id].clutAnime, i);
                                    PSX.levels[Level.Id].edit = true;
                                }
                                else
                                {
                                    if (!Level.zeroFlag)
                                    {
                                        BitConverter.GetBytes((ushort)color).CopyTo(PSX.levels[Level.Id].clut_X.entries[0].data, i);
                                        PSX.levels[Level.Id].megaEdit = true;
                                    }
                                    else
                                    {
                                        BitConverter.GetBytes((ushort)color).CopyTo(PSX.levels[Level.Id].clut_Z.entries[0].data, i);
                                        PSX.levels[Level.Id].zeroEdit = true;
                                    }
                                }
                                i += 2;
                            }
                            MessageBox.Show("Clut Imported!");

                            //Close Window
                            Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive).Close();

                            if (isAnime)
                            {
                                MainWindow.window.animeE.DrawClut();
                                return;
                            }

                            Level.AssignPallete();
                            //Updating the rest of GUI
                            MainWindow.window.clutE.DrawTextures();
                            MainWindow.window.clutE.DrawClut();
                            if (ClutEditor.bgF == 0)
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
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message + ".\nClut might have still been modified");
                    }
                }
            };

            Button exportSet = new Button()
            {
                Content = $"Export Clut {Convert.ToString(id, 16).ToUpper()} as Txt"
            };
            exportSet.Click += (s, e) =>
            {
                using(var sfd = new System.Windows.Forms.SaveFileDialog())
                {
                    sfd.FileName = $"clut {Convert.ToString(id, 16).ToUpper()}.txt";
                    sfd.Title = $"Select Clut {Convert.ToString(id, 16).ToUpper()} save location";

                    if(sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        string lines = null;
                        if (exportInt.Value != null)
                            exportCount = (int)exportInt.Value;

                        for (int i = 0; i < exportCount * 16; i++)
                        {
                            int color;

                            if (isAnime)
                                color = Level.To32Rgb(BitConverter.ToUInt16(PSX.levels[Level.Id].clutAnime, AnimeEditor.clut * 32 + i * 2));
                            else
                            {
                                if (!Level.zeroFlag)
                                    color = Level.To32Rgb(BitConverter.ToUInt16(PSX.levels[Level.Id].clut_X.entries[0].data, ((ClutEditor.clut + (ClutEditor.bgF * 64)) * 16 + i) * 2));
                                else
                                    color = Level.To32Rgb(BitConverter.ToUInt16(PSX.levels[Level.Id].clut_Z.entries[0].data, ((ClutEditor.clut + (ClutEditor.bgF * 64)) * 16 + i) * 2));
                            }

                            string r = Convert.ToString(color >> 16,16).ToUpper().PadLeft(2, '0');
                            string g = Convert.ToString((color >> 8) & 0xFF, 16).ToUpper().PadLeft(2, '0');
                            string b = Convert.ToString(color & 0xFF, 16).ToUpper().PadLeft(2, '0');

                            if(lines == null)
                                lines = "#" + r + g + b;
                            else
                                lines += "\n#" + r + g + b;
                        }
                        File.WriteAllText(sfd.FileName, lines);
                        MessageBox.Show("Clut Set Exported!");
                        //Close Window
                        Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive).Close();
                    }
                }
            };

            if (!isAnime)
            {
                pannel.Children.Add(cpyBackground);
                pannel.Children.Add(cpyObject);
                Height = 255;
            }
            else
                Height = 192;

            pannel.Children.Add(importSet);
            pannel.Children.Add(importPAL);
            pannel.Children.Add(exportSet);
            pannel.Children.Add(exportLbl);
            pannel.Children.Add(exportInt);
        }
        private void CheckpointEdit()
        {
            this.Title = "Checkpoints";

            this.Height = 500;
            this.Width = 300;
            this.MaxWidth = 300;
            this.MinWidth = 300;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            int pastIndex = -1;


            foreach (var l in PSX.levels)
            {
                int index = l.GetIndex();
                if (index == pastIndex)
                    continue;
                else
                    pastIndex = index;
                Grid g = new Grid();
                g.ColumnDefinitions.Add(new ColumnDefinition());
                g.ColumnDefinitions.Add(new ColumnDefinition());
                g.ColumnDefinitions[1].Width = GridLength.Auto;
                Label lbl = new Label() { Foreground = Brushes.White, Content = l.arc.filename , FontSize = 20 };
                NumInt num = new NumInt() { Value = Settings.MaxPoints[index] + 1, FontSize = 20 , Width = 70 , HorizontalAlignment = HorizontalAlignment.Right , Minimum = 1 , Maximum = 0xFF};

                //For Tracking during Resize
                num.Uid = l.GetIndex().ToString();

                Grid.SetColumn(num, 1);
                g.Children.Add(num);    //Keep at Index
                g.Children.Add(lbl);
                this.pannel.Children.Add(g);
            }
            Button confirm = new Button() { Content = "Confirm" };
            Grid.SetRow(confirm, 1);

            confirm.Click += (s, e) =>
            {
                int totalMax = 0;
                int total = 0;
                foreach (var b in Settings.MaxPoints) //Calculate Max Amount of Checkpoints
                {
                    if (b == 0xFF)
                        continue;

                    totalMax += b + 1;
                }

                bool change = false;
                foreach (var c in this.pannel.Children) //Validation
                {
                    if (c.GetType() != typeof(Grid))
                        continue;
                    Grid loopG = c as Grid;
                    if (loopG.Children.Count != 2)
                        continue;

                    total += (int)((NumInt)loopG.Children[0]).Value;
                }

                if(total > totalMax)
                    MessageBox.Show($"Max total checkpoints is {totalMax} , you have entered  {total} witch is more than the max!");
                else
                {
                    foreach (var c in this.pannel.Children) //Editing Change Checkpoints
                    {
                        if (c.GetType() != typeof(Grid))
                            continue;
                        Grid loopG = c as Grid;
                        if (loopG.Children.Count != 2)
                            continue;

                        int val = (int)((NumInt)loopG.Children[0]).Value;
                        int index = Convert.ToInt32(((NumInt)loopG.Children[0]).Uid);

                        if (Settings.MaxPoints[index] == val - 1)
                            continue;

                        //Resize Checkpoint File
                        try
                        {
                            byte[] data = new byte[36];
                            MemoryStream ms = new MemoryStream();
                            BinaryWriter bw = new BinaryWriter(ms);
                            for (int i = 0; i < Settings.MaxPoints[index] + 1; i++)
                            {
                                uint addr = BitConverter.ToUInt32(PSX.exe, PSX.CpuToOffset((uint)(Const.CheckPointPointersAddress + index * 4)));
                                uint read = BitConverter.ToUInt32(PSX.exe, PSX.CpuToOffset((uint)(addr + i * 4)));

                                Array.Copy(PSX.exe, PSX.CpuToOffset(read), data, 0, 36);
                                bw.Write(data);
                            }
                            byte[] file = ms.ToArray();
                            Array.Resize(ref file, val * 36);
                            File.WriteAllBytes(PSX.filePath + "/CHECKPOINT/" + PSX.levels[index].arc.filename + ".BIN", file);
                            change = true;
                            PSX.edit = true;
                        }
                        catch(Exception ex)
                        {
                            MessageBox.Show($"ERROR on {PSX.levels[index].arc.filename} - " + ex.Message);
                            return;
                        }
                    }
                    //End of LOOP
                    if (!change)
                    {
                        this.Close();
                        return;
                    }
                    Settings.DefineCheckpoints();
                    MessageBox.Show("Checkpoint Sizes Edited!");
                    MainWindow.window.spawnE.SetSpawnSettings();
                    this.Close();
                }
            };

            this.outGrid.RowDefinitions.Add(new RowDefinition());
            this.outGrid.RowDefinitions.Add(new RowDefinition());
            this.outGrid.RowDefinitions[1].Height = GridLength.Auto;
            outGrid.Children.Add(confirm);
        }
        private void CameraEdit()
        {
            this.Width = 300;
            this.MaxWidth = 300;
            this.Height = 420;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.Title = "Camera";

            int pastIndex = -1;
            foreach (var l in PSX.levels)
            {
                int index = l.GetIndex();
                if (index == pastIndex)
                    continue;
                else if (index > 25)
                    break;

                pastIndex = index;

                Expander exp = new Expander();
                exp.Uid = index.ToString();
                StackPanel headerPannel = new StackPanel() { Orientation = Orientation.Horizontal , Uid = l.arc.filename};

                Button addBtn = new Button() { Content = "Add" , Tag = exp};
                addBtn.Click += (s, e) =>
                {
                    CameraTriggerEntry t = new CameraTriggerEntry();
                    StackPanel stack = ((Expander)addBtn.Tag).Content as StackPanel;
                    stack.Children.Add(t);
                };


                headerPannel.Children.Add(new Label() { Content = l.arc.filename.Replace("_", "__") });
                headerPannel.Children.Add(addBtn);


                //Add Camera Trigger Entries
                StackPanel triggersPannel = new StackPanel();
                for (int i = 0; i < Settings.MaxTriggers[index] + 1; i++)
                {
                    uint addr = BitConverter.ToUInt32(PSX.exe, PSX.CpuToOffset((uint)(Const.CameraTriggerPointersAddress + index * 4)));
                    if (addr == 0)
                        break;

                    CameraTriggerEntry t = new CameraTriggerEntry(PSX.CpuToOffset(BitConverter.ToUInt32(PSX.exe, PSX.CpuToOffset((uint)(addr + i * 4)))));
                    triggersPannel.Children.Add(t);
                }
                exp.Content = triggersPannel;
                exp.Header = headerPannel;
                this.pannel.Children.Add(exp);
            }


            Button confirm = new Button() { Content = "Confirm" }; //Check then Save to Files
            confirm.Click += (s, e) =>
            {
                int totalSize = 0;

                //Calculate Space
                foreach (var child in this.pannel.Children)
                {
                    if (child.GetType() != typeof(Expander))
                        continue;
                    Expander exportExp = child as Expander;

                    int index = Convert.ToInt32(exportExp.Uid);

                    foreach (var c in ((StackPanel)exportExp.Content).Children)
                    {
                        CameraTriggerEntry t = c as CameraTriggerEntry;
                        totalSize += 2 + t.settings.Count * 2 + 8 + 4;
                    }
                }
                while (true)
                {
                    if (totalSize % 4 == 0)
                        break;
                    totalSize++;
                }

                if(totalSize > Const.MaxCameraTriggerDataSize) //Check
                {
                    MessageBox.Show("The Camera Trigger settings cant be any bigger than " + Const.MaxCameraTriggerDataSize + " the size of your data is " + totalSize);
                    return;
                }

                //Valid (Export to CAMERA Folder)
                foreach (var child in this.pannel.Children)
                {
                    if (child.GetType() != typeof(Expander))
                        continue;
                    Expander exportExp = child as Expander;

                    int index = Convert.ToInt32(exportExp.Uid);
                    string fileName = ((StackPanel)exportExp.Header).Uid;

                    MemoryStream ms = new MemoryStream();
                    BinaryWriter bw = new BinaryWriter(ms);

                    foreach (var c in ((StackPanel)exportExp.Content).Children)
                    {
                        CameraTriggerEntry t = c as CameraTriggerEntry;

                        bw.Write(t.rightSide);
                        bw.Write(t.leftSide);
                        bw.Write(t.bottomSide);
                        bw.Write(t.topSide);

                        foreach (var set in t.settings)
                            bw.Write(set);

                        bw.Write((ushort)0); //Terminator
                    }

                    if(ms.Length < 8)
                        bw.Write(-1);

                    File.WriteAllBytes(PSX.filePath + "/CAMERA/" + fileName + ".BIN", ms.ToArray());
                }
                //Done
                Settings.DefineBoxes();
                MessageBox.Show("Camera Setting Sizes Edited!");
                MainWindow.window.camE.SetupCheckValues();
                MainWindow.window.enemyE.UpdateTriggers();
                Close();
            };

            Grid.SetRow(confirm, 1);
            this.outGrid.RowDefinitions.Add(new RowDefinition());
            this.outGrid.RowDefinitions.Add(new RowDefinition());
            this.outGrid.RowDefinitions[1].Height = GridLength.Auto;
            outGrid.Children.Add(confirm);
        }
        public void DrawScreens()
        {
            while (grid.Children.Count != PSX.levels[Level.Id].size)
            {
                if (grid.Children.Count > PSX.levels[Level.Id].size)
                    grid.Children.RemoveAt(0);
                else if (grid.Children.Count < PSX.levels[Level.Id].size)
                {
                    Button b = new Button();
                    b.Margin = new Thickness(1);
                    b.Padding = new Thickness(1);
                    b.Width = 30;
                    b.Click += (s, e) =>
                    {
                        MainWindow.window.layoutE.viewerX = Grid.GetColumn(s as UIElement) << 8;
                        MainWindow.window.layoutE.viewerY = Grid.GetRow(s as UIElement) << 8;
                        MainWindow.window.UpdateViewrCam();
                        MainWindow.window.layoutE.DrawLayout();
                    };
                    b.MouseRightButtonDown += (s, e) =>
                    {
                        MainWindow.window.enemyE.viewerX = Grid.GetColumn(s as UIElement) << 8;
                        MainWindow.window.enemyE.viewerY = Grid.GetRow(s as UIElement) << 8;
                        MainWindow.window.enemyE.ReDraw();
                    };
                    grid.Children.Add(b);
                }
            }
            if (grid.ColumnDefinitions.Count != PSX.levels[Level.Id].width || grid.RowDefinitions.Count != PSX.levels[Level.Id].height)
                AddGrids(PSX.levels[Level.Id].width, PSX.levels[Level.Id].height);

            for (int i = 0; i < grid.Children.Count; i++)
            {
                var b = grid.Children[i] as Button;
                int c = i % PSX.levels[Level.Id].width;
                int r = i / PSX.levels[Level.Id].width;
                Grid.SetColumn(b, c);
                Grid.SetRow(b, r);
                b.Content = Convert.ToString(PSX.levels[Level.Id].layout[c + (r * PSX.levels[Level.Id].width) + Level.BG * PSX.levels[Level.Id].size], 16).ToUpper();
            }
        }
        public void DrawExtra()
        {
            this.Title = "Screen: " + Convert.ToString(MainWindow.window.screenE.screenId, 16).ToUpper() + " Tile Flags";
            foreach (var u in this.grid.Children)
            {
                var b = u as Border;
                var rect = b.Child as Rectangle;

                int c = Grid.GetColumn(u as UIElement);
                int r = Grid.GetRow(u as UIElement);

                ushort tile = BitConverter.ToUInt16(PSX.levels[Level.Id].screenData, MainWindow.window.screenE.screenId * 0x200 + (c * 2) + r * 32);

                //Set flags Color
                if ((tile & 0x4000) == 0x4000)
                {
                    if (rect.Fill != Brushes.Blue)
                        rect.Fill = Brushes.Blue;
                }
                else
                {
                    if (rect.Fill != Brushes.Black)
                        rect.Fill = Brushes.Black;
                }
                if ((tile & 0x8000) == 0x8000)
                {
                    if (b.BorderBrush != Brushes.Red)
                        b.BorderBrush = Brushes.Red;
                }
                else
                {
                    if (b.BorderBrush != Brushes.White)
                        b.BorderBrush = Brushes.White;
                }
            }
        }
        private void AddGrids(int c, int r)
        {
            while (true)
            {
                if (grid.ColumnDefinitions.Count == c)
                    break;
                else if (grid.ColumnDefinitions.Count > c)
                    grid.ColumnDefinitions.RemoveAt(0);
                else
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition());
                    grid.ColumnDefinitions[grid.ColumnDefinitions.Count - 1].Width = GridLength.Auto;
                }
            }
            while (true)
            {
                if (grid.RowDefinitions.Count == r)
                    break;
                else if (grid.RowDefinitions.Count > r)
                    grid.RowDefinitions.RemoveAt(0);
                else
                {
                    grid.RowDefinitions.Add(new RowDefinition());
                    grid.RowDefinitions[grid.RowDefinitions.Count - 1].Height = GridLength.Auto;
                }
            }
        }
        #endregion Methods

        #region Events
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            switch (this.mode)
            {
                case 0: //Layout Viewer
                    screenViewOpen = false;
                    screenLeft = this.Left;
                    screenTop = this.Top;
                    screenWidth = this.Width;
                    screenHeight = this.Height;
                    screenState = (int)this.WindowState;
                    break;
                case -1:
                    break;
                case -2:
                    extraOpen = false;
                    extraLeft = this.Left;
                    extraTop = this.Top;
                    break;

                case -3: //Enemy Tools
                    ListView listView = pannel.Children[1] as ListView;
                    PSX.levels[Level.Id].startEnemies.Clear();
                    foreach (var i in listView.Items) //Apply Enemies to Form
                    {
                        if (i.GetType() != typeof(EnemyEntry))
                            continue;
                        EnemyEntry entry = i as EnemyEntry;
                        Enemy en = new Enemy();
                        en.id = (byte)entry.idInt.Value;
                        en.var = (byte)entry.varInt.Value;
                        en.type = (byte)entry.typeInt.Value;
                        en.range = (byte)entry.rangeInt.Value;
                        en.x = (short)entry.xInt.Value;
                        en.y = (short)entry.yInt.Value;
                        PSX.levels[Level.Id].startEnemies.Add(en);
                    }

                    //MainWindow.window.enemyE.DisableSelect();
                    //MainWindow.window.enemyE.DrawEnemies();
                    break;
                case -4:
                    fileViewOpen = false;
                    fileLeft = this.Left;
                    fileTop = this.Top;
                    fileWidth = this.Width;
                    fileHeight = this.Height;
                    fileState = (int)this.WindowState;
                    break;
                case -5:
                    clutLeft = this.Left;
                    clutTop = this.Top;
                    break;
                default:
                    break;
            }
        }
        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Xceed.Wpf.Toolkit.WatermarkTextBox num = Keyboard.FocusedElement as Xceed.Wpf.Toolkit.WatermarkTextBox;
            if (num != null)
            {
                TraversalRequest tRequest = new TraversalRequest(FocusNavigationDirection.Next);
                num.MoveFocus(tRequest);

                while (true)
                {
                    if (Keyboard.FocusedElement.GetType() != typeof(Xceed.Wpf.Toolkit.WatermarkTextBox))
                        break;
                    ((Xceed.Wpf.Toolkit.WatermarkTextBox)Keyboard.FocusedElement).MoveFocus(tRequest);
                }
            }
        }
        #endregion Events
    }
}
