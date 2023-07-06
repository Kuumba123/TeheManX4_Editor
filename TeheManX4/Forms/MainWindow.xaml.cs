using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TeheManX4.Forms;

namespace TeheManX4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields
        internal static MainWindow window;
        internal static ListWindow fileWindow;
        internal static ListWindow layoutWindow;
        internal static ListWindow extraWindow;
        internal static ListWindow loadWindow;
        internal static Settings settings = Settings.SetDefaultSettings();
        #endregion

        #region Properties
        private bool max = false;
        #endregion Properties

        #region Constructors
        public MainWindow()
        {
            InitializeComponent();
            if (window == null)
            {
                window = this;
                PSX.lastSave = null;
                Settings.nops.EnableRaisingEvents = true;
                Settings.nops.OutputDataReceived += NOPS_OutputDataReceived;
                //Window Sizing
                {
                    int Y = (int)(40 * SystemParameters.PrimaryScreenWidth / 100);
                    window.layoutE.selectImage.MaxWidth = Y;
                    window.screenE.tileImage.MaxWidth = Y;
                    window.x16E.textureImage.MaxWidth = Y;
                }

                //Open Settings
                if (File.Exists("Settings.json"))
                {
                    try
                    {
                        settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("Settings.json"));
                        settings.CheckForValidSettings();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, "ERROR");
                        Application.Current.Shutdown();
                    }
                }
                //Get Arguments
                string[] args = Environment.GetCommandLineArgs();

                if(args.Length == 2) //Open Game Files using args
                {
                    if (!Directory.Exists(args[1]))
                    {
                        MessageBox.Show("The directory: " + args[1] + " does not exist.");
                        return;
                    }
                    //Look for all Game Files
                    if (!File.Exists(args[1] + "/SLUS_005.61"))
                    {
                        MessageBox.Show("The PSX.EXE (SLUS_005.61) was not found");
                        return;
                    }
                    //PSX.EXE was found
                    PSX.exe = File.ReadAllBytes(args[1] + "/SLUS_005.61");
                    PSX.time = File.GetLastWriteTime(args[1] + "/SLUS_005.61");
                    Level.LoadLevels(args[1]);

                    if (PSX.levels.Count == 0)
                    {
                        MessageBox.Show("No ARC level files were found");
                        return;
                    }
                    else if (PSX.levels.Count != Const.FilesCount)
                    {
                        MessageBox.Show("You need to have all level files in order to use this editor");
                        PSX.levels.Clear();
                        return;
                    }

                    if (settings.defaultZero)
                    {
                        Level.zeroFlag = true;
                        clutE.playerBtn.Content = "Zero";
                    }
                    else
                    {
                        Level.zeroFlag = false;
                        clutE.playerBtn.Content = "X";
                    }

                    PSX.edit = false;
                    PSX.filePath = args[1];

                    Settings.DefineBoxes();
                    Settings.DefineCheckpoints();

                    Level.Id = 0;
                    Level.AssignPallete();
                    PSX.levels[Level.Id].LoadTextures();
                    //Draw Everything
                    Update();
                    window.spawnE.SetupSpecialSpawn();
                    window.camE.SetupBorderInfo();
                    hub.Visibility = Visibility.Visible;
                }
            }
            else
            {
                this.Title = "Tehe Sub Window";
                this.dockBar.Visibility = Visibility.Collapsed;
                this.hub.Visibility = Visibility.Visible;
            }
        }
        #endregion Constructors

        #region Methods
        public void Update()
        {
            window.layoutE.AssignLimits();
            window.screenE.AssignLimits();
            window.x16E.AssignLimits();
            window.x16E.DrawTextures();
            window.clutE.DrawTextures();
            window.clutE.DrawClut();
            window.clutE.UpdateClutTxt();
            window.spawnE.SetSpawnSettings();
            window.enemyE.ReDraw();
            window.camE.SetupCheckValues();
            UpdateViewrCam();
            UpdateEnemyViewerCam();
            window.bgE.SetSlotSettings();
            window.bgE.SetLayerSettings();
            if (ListWindow.screenViewOpen)
                layoutWindow.DrawScreens();
            if (ListWindow.extraOpen)
                extraWindow.DrawExtra();
            UpdateWindowTitle();
        }
        public void UpdateWindowTitle()
        {
            if (!Level.zeroFlag && PSX.levels[Level.Id].clut_X != null)
                window.Title = "TeheMan X4  Editor - " + PSX.levels[Level.Id].arc.filename + " w/ " + PSX.levels[Level.Id].clut_X.filename;
            else if(Level.zeroFlag && PSX.levels[Level.Id].clut_Z != null)
                window.Title = "TeheMan X4  Editor - " + PSX.levels[Level.Id].arc.filename + " w/ " + PSX.levels[Level.Id].clut_Z.filename;
            else
                window.Title = "TeheMan X4  Editor - " + PSX.levels[Level.Id].arc.filename;
        }
        public void UpdateViewrCam()
        {
            window.layoutE.camLbl.Content = "X:" + Convert.ToString(window.layoutE.viewerX >> 8, 16).PadLeft(2, '0').ToUpper() + " Y:" + Convert.ToString(window.layoutE.viewerY >> 8, 16).PadLeft(2, '0').ToUpper();
        }
        public void UpdateEnemyViewerCam()
        {
            window.enemyE.camLbl.Content = "X:" + Convert.ToString(window.enemyE.viewerX >> 8, 16).PadLeft(2, '0').ToUpper() + " Y:" + Convert.ToString(window.enemyE.viewerY >> 8, 16).PadLeft(2, '0').ToUpper();
        }
        private void OpenGame()
        {
            var fd = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            fd.Multiselect = false;
            fd.Description = "Select the Folder containing Game Files";
            fd.UseDescriptionForTitle = true;
            if ((bool)fd.ShowDialog())
            {
                //Look for all Game Files
                if (!File.Exists(fd.SelectedPath + "/SLUS_005.61"))
                {
                    MessageBox.Show("The PSX.EXE (SLUS_005.61) was not found");
                    return;
                }
                //PSX.EXE was found
                PSX.exe = File.ReadAllBytes(fd.SelectedPath + "/SLUS_005.61");
                PSX.time = File.GetLastWriteTime(fd.SelectedPath + "/SLUS_005.61");
                Level.LoadLevels(fd.SelectedPath);

                if (PSX.levels.Count == 0)
                {
                    MessageBox.Show("No ARC level files were found");
                    return;
                }
                else if (PSX.levels.Count != Const.FilesCount)
                {
                    MessageBox.Show("You need to have all level files in order to use this editor");
                    PSX.levels.Clear();
                    return;
                }

                if (settings.defaultZero)
                {
                    Level.zeroFlag = true;
                    clutE.playerBtn.Content = "Zero";
                }
                else
                {
                    Level.zeroFlag = false;
                    clutE.playerBtn.Content = "X";
                }


                PSX.filePath = fd.SelectedPath;
                PSX.edit = false;

                Settings.DefineBoxes();
                Settings.DefineCheckpoints();

                Level.Id = 0;
                Level.AssignPallete();
                PSX.levels[Level.Id].LoadTextures();
                //Draw Everything
                Update();
                window.spawnE.SetupSpecialSpawn();
                window.camE.SetupBorderInfo();
                hub.Visibility = Visibility.Visible;
            }
        }
        private void MainKeyCheck(string key)
        {
            if(key == "F1")
            {
                if (Level.Id != 0)
                {
                    Level.Id--;
                    //RE-Update
                    Level.AssignPallete();
                    PSX.levels[Level.Id].LoadTextures();
                    Update();
                }
                else
                {
                    Level.Id = PSX.levels.Count - 1;
                    //RE-Update
                    Level.AssignPallete();
                    PSX.levels[Level.Id].LoadTextures();
                    Update();
                }
            }
            else if (key == "F2")
            {
                if (Level.Id != PSX.levels.Count - 1)
                {
                    Level.Id++;
                    //RE-Update
                    Level.AssignPallete();
                    PSX.levels[Level.Id].LoadTextures();
                    Update();
                }
                else
                {
                    Level.Id = 0;
                    //RE-Update
                    Level.AssignPallete();
                    PSX.levels[Level.Id].LoadTextures();
                    Update();
                }
            }
        }
        private void LayoutKeyCheck(string key,bool notFocus)
        {
            if(key == "Delete")
            {
                var result = MessageBox.Show("Are you sure you want to delete all of Layer " + (Level.BG + 1) + "?", "", MessageBoxButton.YesNo);

                if(result == MessageBoxResult.Yes)
                {
                    for (int i = 0; i < PSX.levels[Level.Id].size; i++)
                        PSX.levels[Level.Id].layout[i + PSX.levels[Level.Id].size * Level.BG] = 0;
                    PSX.edit = true;
                    window.layoutE.DrawLayout();
                    if (ListWindow.screenViewOpen)
                        layoutWindow.DrawScreens();
                }
                return;
            }
            if (!notFocus)  //check if NumInt is focused
                return;
            if (key == "W")
            {
                if (window.layoutE.viewerY != 0)
                {
                    window.layoutE.viewerY -= 0x100;
                    window.layoutE.DrawLayout();
                    UpdateViewrCam();
                }
            }
            else if (key == "S")
            {
                if ((window.layoutE.viewerY >> 8) < (PSX.levels[Level.Id].height - 3))
                {
                    window.layoutE.viewerY += 0x100;
                    window.layoutE.DrawLayout();
                    UpdateViewrCam();
                }
            }
            else if (key == "D")
            {
                if ((window.layoutE.viewerX >> 8) < (PSX.levels[Level.Id].width - 3))
                {
                    window.layoutE.viewerX += 0x100;
                    window.layoutE.DrawLayout();
                    UpdateViewrCam();
                }
            }
            else if (key == "A")
            {
                if (window.layoutE.viewerX != 0)
                {
                    window.layoutE.viewerX -= 0x100;
                    window.layoutE.DrawLayout();
                    UpdateViewrCam();
                }
            }
            else if (key == "D1")
            {
                if (Level.BG != 0)
                {
                    Level.BG = 0;
                    window.layoutE.DrawLayout();
                    window.layoutE.UpdateBtn();
                    window.enemyE.Draw();
                    if (ListWindow.screenViewOpen)
                    {
                        layoutWindow.DrawScreens();
                        layoutWindow.Title = "All Screens in Layer " + (Level.BG + 1);
                    }

                }
            }
            else if (key == "D2")
            {
                if (Level.BG != 1)
                {
                    Level.BG = 1;
                    window.layoutE.DrawLayout();
                    window.layoutE.UpdateBtn();
                    window.enemyE.Draw();
                    if (ListWindow.screenViewOpen)
                    {
                        layoutWindow.DrawScreens();
                        layoutWindow.Title = "All Screens in Layer " + (Level.BG + 1);
                    }
                }
            }
            else if (key == "D3")
            {
                if (Level.BG != 2)
                {
                    Level.BG = 2;
                    window.layoutE.DrawLayout();
                    window.layoutE.UpdateBtn();
                    window.enemyE.Draw();
                    if (ListWindow.screenViewOpen)
                    {
                        layoutWindow.DrawScreens();
                        layoutWindow.Title = "All Screens in Layer " + (Level.BG + 1);

                    }
                }
            }
        }
        private void ScreenKeyCheck(string key)
        {
            //Clear Screen
            if (key == "Delete")
            {
                Array.Clear(PSX.levels[Level.Id].screenData, window.screenE.screenId * 0x200, 0x200);
                PSX.levels[Level.Id].edit = true;
                window.layoutE.DrawLayout();
                if (window.layoutE.selectedScreen == window.screenE.screenId)
                    window.layoutE.DrawScreen();

                window.screenE.DrawScreen();
                window.enemyE.Draw();
            }
        }
        private void ClutKeyCheck(string key)
        {
            if (key == "Up")
            {
                ClutEditor.clut = (ClutEditor.clut - 1) & 0x3F;
                window.clutE.DrawTextures();
                window.clutE.UpdateClutTxt();
            }
            else if (key == "Down")
            {
                ClutEditor.clut = (ClutEditor.clut + 1) & 0x3F;
                window.clutE.DrawTextures();
                window.clutE.UpdateClutTxt();
            }
            else if (key == "Left")
            {
                window.clutE.UpdateTpageButton((ClutEditor.page - 1) & 7);
            }
            else if (key == "Right")
            {
                window.clutE.UpdateTpageButton((ClutEditor.page + 1) & 7);
            }
            else if (key == "D1")
            {
                window.clutE.UpdateSelectedTexture(0);
            }
            else if (key == "D2")
            {
                window.clutE.UpdateSelectedTexture(1);
            }
        }
        private void EnemyKeyCheck(string key,bool notFocus)
        {
            if(key == "Delete" && window.enemyE.control.Tag != null)
            {
                PSX.levels[Level.Id].enemies.Remove((Enemy)((EnemyLabel)window.enemyE.control.Tag).Tag);
                window.enemyE.DrawEnemies();
                PSX.edit = true;
                return;
            }
            if (!notFocus)  //check if NumInt is focused
                return;
            if (key == "W")
            {
                if (window.enemyE.viewerY != 0)
                {
                    window.enemyE.viewerY -= 0x100;
                    window.enemyE.ReDraw();
                    UpdateEnemyViewerCam();
                }
            }
            else if (key == "S")
            {
                if ((window.enemyE.viewerY >> 8) < (PSX.levels[Level.Id].height - 2))
                {
                    window.enemyE.viewerY += 0x100;
                    window.enemyE.ReDraw();
                    UpdateEnemyViewerCam();
                }
            }
            else if (key == "D")
            {
                if ((window.enemyE.viewerX >> 8) < (PSX.levels[Level.Id].width - 2))
                {
                    window.enemyE.viewerX += 0x100;
                    window.enemyE.ReDraw();
                    UpdateEnemyViewerCam();
                }
            }
            else if (key == "A")
            {
                if (window.enemyE.viewerX != 0)
                {
                    window.enemyE.viewerX -= 0x100;
                    window.enemyE.ReDraw();
                    UpdateEnemyViewerCam();
                }
            }
        }
        internal bool SaveFiles(bool current = false /*option for saving current file*/)
        {
            try
            {
                if (current)
                {
                    if (PSX.levels[Level.Id].megaEdit)
                    {
                        File.WriteAllBytes(PSX.filePath + "/ARC/" + PSX.levels[Level.Id].clut_X.filename, PSX.levels[Level.Id].clut_X.GetEntriesData());
                        PSX.levels[Level.Id].megaEdit = false;
                    }
                    if (PSX.levels[Level.Id].zeroEdit)
                    {
                        File.WriteAllBytes(PSX.filePath + "/ARC/" + PSX.levels[Level.Id].clut_Z.filename, PSX.levels[Level.Id].clut_Z.GetEntriesData());
                        PSX.levels[Level.Id].zeroEdit = false;
                    }
                    if (!Level.ValidLevel(Level.Id))
                        return false;
                    if (!Level.ValidLayouts())
                        return false;
                    Level.SaveLayouts();
                    Level.SaveLevel(Level.Id);

                    if (File.Exists(PSX.levels[Level.Id].arc.path + "/ARC/" + PSX.levels[Level.Id].arc.filename))
                    {
                        if (!PSX.levels[Level.Id].edit && PSX.levels[Level.Id].time == File.GetLastWriteTime(PSX.levels[Level.Id].arc.path + "/ARC/" + PSX.levels[Level.Id].arc.filename))
                            goto Skip;
                        File.WriteAllBytes(PSX.levels[Level.Id].arc.path + "/ARC/" + PSX.levels[Level.Id].arc.filename, PSX.levels[Level.Id].arc.GetEntriesData());
                        PSX.levels[Level.Id].time = File.GetLastWriteTime(PSX.levels[Level.Id].arc.path + "/ARC/" + PSX.levels[Level.Id].arc.filename);
                        PSX.levels[Level.Id].edit = false;
                    }
                    else
                    {
                        File.WriteAllBytes(PSX.levels[Level.Id].arc.path + "/ARC/" + PSX.levels[Level.Id].arc.filename, PSX.levels[Level.Id].arc.GetEntriesData());
                        PSX.levels[Level.Id].time = File.GetLastWriteTime(PSX.levels[Level.Id].arc.path + "/STDATA/" + PSX.levels[Level.Id].arc.filename);
                        PSX.levels[Level.Id].edit = false;
                    }
                Skip:
                    goto SaveExe;
                }

                for (int i = 0; i < PSX.levels.Count; i++)
                {
                    if (PSX.levels[i].megaEdit)
                    {
                        File.WriteAllBytes(PSX.filePath + "/ARC/" + PSX.levels[i].clut_X.filename, PSX.levels[i].clut_X.GetEntriesData());
                        PSX.levels[i].megaEdit = false;
                    }
                    if (PSX.levels[i].zeroEdit)
                    {
                        File.WriteAllBytes(PSX.filePath + "/ARC/" + PSX.levels[i].clut_Z.filename, PSX.levels[i].clut_Z.GetEntriesData());
                        PSX.levels[i].zeroEdit = false;
                    }
                    if (!Level.ValidLevel(i))
                        return false;
                    if (!Level.ValidLayouts())
                        return false;
                    if (i == 0)
                        Level.SaveLayouts();
                    Level.SaveLevel(i);

                    if (File.Exists(PSX.levels[i].arc.path + "/ARC/" + PSX.levels[i].arc.filename))
                    {
                        if (!PSX.levels[i].edit && PSX.levels[i].time == File.GetLastWriteTime(PSX.levels[i].arc.path + "/ARC/" + PSX.levels[i].arc.filename))
                            continue;
                        File.WriteAllBytes(PSX.levels[i].arc.path + "/ARC/" + PSX.levels[i].arc.filename, PSX.levels[i].arc.GetEntriesData());
                        PSX.levels[i].time = File.GetLastWriteTime(PSX.levels[i].arc.path + "/ARC/" + PSX.levels[i].arc.filename);
                        PSX.levels[i].edit = false;
                    }
                    else
                    {
                        File.WriteAllBytes(PSX.levels[i].arc.path + "/ARC/" + PSX.levels[i].arc.filename, PSX.levels[i].arc.GetEntriesData());
                        PSX.levels[i].time = File.GetLastWriteTime(PSX.levels[i].arc.path + "/STDATA/" + PSX.levels[i].arc.filename);
                        PSX.levels[i].edit = false;
                    }
                }

                if (Settings.ExtractedPoints)
                {
                    for (int i = 0; i < Settings.EditedPoints.Length; i++)
                    {
                        if (Settings.EditedPoints[i] == true)
                        {
                            foreach (var l in PSX.levels)
                            {
                                if (l.GetIndex() == i)
                                    File.WriteAllBytes(PSX.filePath + "/CHECKPOINT/" + l.arc.filename + ".BIN", l.GetSpawnData());
                            }
                        }
                    }
                }

            SaveExe:
                if (File.Exists(PSX.filePath + "/SLUS_005.61"))
                {
                    if (!PSX.edit && PSX.time == File.GetLastWriteTime(PSX.filePath + "/SLUS_005.61"))
                        return true;
                    File.WriteAllBytes(PSX.filePath + "/SLUS_005.61", PSX.exe);
                    PSX.time = File.GetLastWriteTime(PSX.filePath + "/SLUS_005.61");
                    PSX.edit = false;
                }
                else
                {
                    File.WriteAllBytes(PSX.filePath + "/SLUS_005.61", PSX.exe);
                    PSX.time = File.GetLastWriteTime(PSX.filePath + "/SLUS_005.61");
                    PSX.edit = false;
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message, "SAVE ERROR");
                return false;
            }
            return true;
        }

        private async void ReLoad(bool single = false)
        {
            if (!Level.ValidLevel(Level.Id) || !Level.ValidLayouts())
                return;
            Level.SaveLevel(Level.Id);
            Level.SaveLayouts();
            if (!Level.ValidPlayer())
                return;
            try
            {
                if (settings.saveOnReload)
                    SaveFiles(true);
                if (settings.useNops) //use NOPS
                {
                    ListWindow.tab = ((TabItem)hub.SelectedItem).Name;
                    ListWindow.checkpoingGo = false;
                    loadWindow = new ListWindow(single);
                    loadWindow.ShowDialog();
                }
                else // use REDUX
                {
                    await Redux.Pause();

                    //Screen Data
                    await Redux.Write(Settings.levelScreenAddress, PSX.levels[Level.Id].screenData);
                    await Redux.Write(Settings.levelTileAddress, PSX.levels[Level.Id].tileInfo);

                    //Backup Screen Data
                    await Redux.Write((uint)(Settings.levelSize + Settings.levelStartAddress) + 0x1000, PSX.levels[Level.Id].screenData);

                    //CLUT
                    if (!Level.zeroFlag && PSX.levels[Level.Id].clut_X != null)
                    {
                        await Redux.Write((uint)(Settings.levelSize + Settings.levelStartAddress), PSX.levels[Level.Id].clut_X.entries[0].data);
                        await Redux.Write(Const.UpdateClutAddress, (byte)1);
                    }
                    else if (Level.zeroFlag && PSX.levels[Level.Id].clut_Z != null)
                    {
                        await Redux.Write((uint)(Settings.levelSize + Settings.levelStartAddress), PSX.levels[Level.Id].clut_Z.entries[0].data);
                        await Redux.Write(Const.UpdateClutAddress, (byte)1);
                    }


                    //Layout
                    await Redux.Write(Const.LayoutBufferAddress, PSX.levels[Level.Id].layout);
                    byte[] data = new byte[0x40CC];
                    Array.Copy(PSX.exe, Const.LayoutDataPointersOffset, data, 0, data.Length);
                    await Redux.Write(PSX.OffsetToCpu(Const.LayoutDataPointersOffset), data);

                    //Layout Size
                    await Redux.Write(Const.LayoutWidthAddress, PSX.levels[Level.Id].width);
                    await Redux.Write(Const.LayoutHeightAddress, PSX.levels[Level.Id].height);
                    await Redux.Write(Const.LayoutSizeAddress, PSX.levels[Level.Id].size);

                    //Update Layers flag
                    await Redux.Write(Const.UpdateLayer1Address, (byte)1);
                    await Redux.Write(Const.UpdateLayer1Address + 0x54, (byte)1);
                    await Redux.Write(Const.UpdateLayer1Address + 0x54 * 2, (byte)1);

                    //Enemy Data
                    int index = PSX.levels[Level.Id].GetIndex();
                    if(index < 26)
                    {
                        if (PSX.levels[Level.Id].isMid())
                            index--;
                        int dummyPad = 8 * 4;
                        int id = PSX.levels[Level.Id].GetId();
                        if (id == 9 || id == 0xA)
                            dummyPad = 8 * 2;
                        int enemyDataOffset = PSX.CpuToOffset(BitConverter.ToUInt32(PSX.exe, Const.EnemyDataPointersOffset + index * 4));
                        data = new byte[Const.MaxEnemies[id] * 8 + dummyPad];
                        Array.Copy(PSX.exe, enemyDataOffset, data, 0, Const.MaxEnemies[id] * 8 + dummyPad);
                        await Redux.Write(PSX.OffsetToCpu(enemyDataOffset), data);
                        //Fix Pointers
                        await Redux.Write(PSX.OffsetToCpu(Const.EnemyDataPointersOffset + index * 4), BitConverter.ToInt32(PSX.exe, Const.EnemyDataPointersOffset + index * 4));
                        await Redux.Write(PSX.OffsetToCpu(Const.StartEnemyDataPointersOffset + index * 4), BitConverter.ToInt32(PSX.exe, Const.StartEnemyDataPointersOffset + index * 4));
                        if (id != 9 && id != 0xA) //2nd Half
                        {
                            index++;

                            await Redux.Write(PSX.OffsetToCpu(Const.EnemyDataPointersOffset + index * 4), BitConverter.ToInt32(PSX.exe, Const.EnemyDataPointersOffset + index * 4));
                            await Redux.Write(PSX.OffsetToCpu(Const.StartEnemyDataPointersOffset + index * 4), BitConverter.ToInt32(PSX.exe, Const.StartEnemyDataPointersOffset + index * 4));
                        }
                        index = PSX.levels[Level.Id].GetIndex();

                        //Camera
                        data = new byte[0x7FC];
                        Array.Copy(PSX.exe, PSX.CpuToOffset(Const.CameraTriggerFreeDataAddress), data, 0, data.Length);
                        await Redux.Write(Const.CameraTriggerFreeDataAddress, data);
                    }

                    //Check Points
                    await SpawnWindow.WriteCheckPoints();


                    //Textures
                    foreach (var e in PSX.levels[Level.Id].arc.entries)
                    {
                        if (e.type != 0x010000 && e.type != 0x10102 && e.type != 0x10002)
                            continue;

                        int height = e.data.Length / 128;
                        int pages; //full pages
                        int pageSize;
                        int pageHeight;
                        if (e.type == 0x010000 || e.type == 0x10002)
                        {
                            pageSize = 0x8000;
                            pages = height / 256;
                            pageHeight = 256;
                            Array.Resize(ref data, 0x8000);
                        }
                        else
                        {
                            pageSize = 0x5800;
                            pages = height / 176;
                            pageHeight = 176;
                            Array.Resize(ref data, 0x5800);
                        }

                        int x = Const.CordTabe[e.type & 0xFF] & 0xFFFF;
                        int y = Const.CordTabe[e.type & 0xFF] >> 16;


                        for (int i = 0; i < pages; i++)
                        {
                            Array.Copy(e.data, i * pageSize, data, 0, pageSize);
                            await Redux.DrawRect(new Int32Rect(x + i * 64, y, 64, pageHeight), data);
                        }
                        if (e.data.Length % pageHeight != 0)
                        {
                            Array.Clear(data, 0, data.Length);
                            int dumpHeight = e.data.Length % 256;
                            Array.Resize(ref data, dumpHeight * 128);
                            Array.Copy(e.data, pages * pageSize, data, 0, dumpHeight * 128);
                            await Redux.DrawRect(new Int32Rect(x + pages * 64, y, 64, dumpHeight), data);
                        }
                    }

                    //Done
                    await Redux.Resume();
                }
            }
            catch (HttpRequestException e)
            {
                MessageBox.Show(e.Message, "REDUX ERROR");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "ERROR");
            }
        }
        private int GetHubIndex()
        {
            System.Collections.Generic.IEnumerable<Dragablz.DragablzItem> tabs = this.hub.GetOrderedHeaders();

            int index = 0;
            foreach (var t in tabs)
            {
                if (((TabItem)t.Content).Name == ((TabItem)this.hub.SelectedItem).Name)
                    return index;
                index++;
            }

            return index;
        }
        private int GetActualIndex(int i /*Visual Index*/)
        {
            var tabs = this.hub.GetOrderedHeaders().ToList();

            int index = 0;
            foreach (var item in this.hub.Items)
            {
                if (((TabItem)item).Name == ((TabItem)tabs[i].Content).Name)
                    return index;
                index++;
            }
            return -1;
        }
        private void CloseChildWindows()
        {
            var childWindows = Application.Current.Windows.Cast<Window>().Where(w => w != Application.Current.MainWindow).ToList();
            window.hub.ConsolidateOrphanedItems = false;
            foreach (var window in childWindows)
                window.Close();
        }
#endregion Methods

        #region Events
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Check for Update
            if (settings.dontUpdate) return;
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; Grand/3.0)");
                try
                {
                    HttpResponseMessage response = await client.GetAsync(Const.reproURL);
                    response.EnsureSuccessStatusCode();
                    string json = await response.Content.ReadAsStringAsync();
                    dynamic release = JsonConvert.DeserializeObject(json);
                    string tag = release.tag_name;
                    if (tag != Const.Version && !Settings.IsPastVersion(tag))
                    {
                        var result = MessageBox.Show($"There is a new version of this editor ({tag}) do you want to download the update?", "New Version", MessageBoxButton.YesNo);
                        if (result == MessageBoxResult.Yes)
                        {
                            //Start Downloading
                            string url = release.assets[0].browser_download_url;
                            response = await client.GetAsync(url);
                            response.EnsureSuccessStatusCode();
                            using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                            {
                                using (FileStream fileStream = new FileStream("TeheManX4 Editor " + tag + ".exe", FileMode.Create, FileAccess.Write, FileShare.None))
                                {
                                    await contentStream.CopyToAsync(fileStream);
                                }
                            }
                            System.Diagnostics.Process.Start(Directory.GetCurrentDirectory() + "/" + "TeheManX4 Editor " + tag + ".exe");
                            Application.Current.Shutdown();
                        }
                    }
                }
                catch (HttpRequestException)
                {
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
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
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Dragablz.TabablzControl.GetIsClosingAsPartOfDragOperation(this) && this == window)
                e.Cancel = true;
            else if(this == window)
            {
                foreach (var l in PSX.levels)
                {
                    if (l.edit)
                    {
                        var result = MessageBox.Show("You have edited some of your game files without saving.\nAre you sure you want to exit the editor?", "WARNING", MessageBoxButton.YesNo);
                        if (result != MessageBoxResult.Yes)
                        {
                            e.Cancel = true;
                            return;
                        }
                        else
                            goto End;
                    }
                    if (l.megaEdit || l.zeroEdit)
                    {
                        var result = MessageBox.Show("You have edited the Colors of some of the game files.\nAre you sure you want to exit the editor?", "WARNING", MessageBoxButton.YesNo);
                        if (result != MessageBoxResult.Yes)
                        {
                            e.Cancel = true;
                            return;
                        }
                        else
                            goto End;
                    }
                }
                if (PSX.edit)
                {
                    var result = MessageBox.Show("You have edited the PSX.EXE without saving.\nAre you sure you want to exit the editor?", "WARNING", MessageBoxButton.YesNo);
                    if (result != MessageBoxResult.Yes)
                    {
                        e.Cancel = true;
                        return;
                    }
                    else
                        goto End;
                }
            End:
                CloseChildWindows();
            }
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            string key = e.Key.ToString();
            if(key == "F11")
            {
                if (max)
                {
                    this.WindowStyle = WindowStyle.SingleBorderWindow;
                    this.WindowState = WindowState.Normal;
                    max = false;
                }
                else
                {
                    this.WindowStyle = WindowStyle.None;
                    this.WindowState = WindowState.Maximized;
                    max = true;
                }
                return;
            }
            if(e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                if(key == "O") //Open
                {
                    OpenGame();
                }
                else if(key == "S" && PSX.levels.Count == Const.FilesCount)
                {
                    SaveFiles();
                }else if(key == "R" && PSX.levels.Count == Const.FilesCount)
                {
                    ReLoad();
                }else if(key == "E" && PSX.levels.Count == Const.FilesCount)
                {
                    ReLoad(true);
                }
                else if (key == "Left" && PSX.levels.Count == Const.FilesCount && this.hub.Items.Count > 1)
                {
                    int hubIndex = GetHubIndex();
                    if (hubIndex == 0)
                    {
                        this.hub.SelectedIndex = GetActualIndex(this.hub.Items.Count - 1);
                        return;
                    }
                    this.hub.SelectedIndex = GetActualIndex(hubIndex - 1);
                }
                else if (key == "Right" && PSX.levels.Count == Const.FilesCount && this.hub.Items.Count > 1)
                {
                    int hubIndex = GetHubIndex();
                    if (hubIndex == this.hub.Items.Count - 1)
                    {
                        this.hub.SelectedIndex = GetActualIndex(0);
                        return;
                    }
                    this.hub.SelectedIndex = GetActualIndex(hubIndex + 1);
                }
                return;
            }
            if (PSX.levels.Count != Const.FilesCount)
                return;
            MainKeyCheck(key);
            if (hub.SelectedItem == null)
                return;
            bool nonNumInt = false;
            if (Keyboard.FocusedElement.GetType() != typeof(Xceed.Wpf.Toolkit.WatermarkTextBox)) nonNumInt = true;
            TabItem tab = (TabItem)hub.SelectedItem;
            switch (tab.Name)
            {
                case "layoutTab":
                    {
                        LayoutKeyCheck(key, nonNumInt);
                        break;
                    }
                case "screenTab":
                    {
                        ScreenKeyCheck(key);
                        break;
                    }
                case "clutTab":
                    {
                        ClutKeyCheck(key);
                        break;
                    }
                case "enemyTab":
                    {
                        EnemyKeyCheck(key, nonNumInt);
                        break;
                    }
            }
        }
        private void toolsBtn_Click(object sender, RoutedEventArgs e)
        {
            ToolsWindow tools = new ToolsWindow();
            tools.ShowDialog();
        }

        private void openBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenGame();
        }
        private void saveAsButn_Click(object sender, RoutedEventArgs e)
        {
            if (PSX.levels.Count != Const.FilesCount)
                return;

        }
        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (PSX.levels.Count == Const.FilesCount)
                SaveFiles();
        }
        private void SettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow s = new SettingsWindow();
            s.ShowDialog();
        }
        private void filesBtn_Click(object sender, RoutedEventArgs e)
        {
            if (PSX.levels.Count != Const.FilesCount || ListWindow.fileViewOpen)
                return;
            fileWindow = new ListWindow(4);
            fileWindow.Show();
        }
        private void sizeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (PSX.levels.Count != Const.FilesCount)
                return;
            SizeWindow s = new SizeWindow();
            s.ShowDialog();
        }
        private void helpBtn_Click(object sender, RoutedEventArgs e)
        {
            HelpWindow h = new HelpWindow(0);
            h.ShowDialog();
        }
        private void reloadBtn_Click(object sender, RoutedEventArgs e)
        {
            if (PSX.levels.Count == Const.FilesCount)
                ReLoad();
        }

        private void aboutBtn_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow about = new AboutWindow();
            about.ShowDialog();
        }
        public void NOPS_OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            if (loadWindow.mode > 1)
            {
                loadWindow.Dispatcher.Invoke(() =>
                {
                    TextBox t = loadWindow.grid.Children[0] as TextBox;
                    t.Text += "\n" + e.Data;
                    ScrollViewer s = loadWindow.outGrid.Children[0] as ScrollViewer;
                    s.ScrollToEnd();
                });
            }
        }
#endregion Events
    }
}
