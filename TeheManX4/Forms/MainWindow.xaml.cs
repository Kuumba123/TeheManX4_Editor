using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
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
        internal static DispatcherTimer animeTimer = new DispatcherTimer();
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
                Level.LoadCollisionTiles();
                PSX.lastSave = null;
                Settings.nops.EnableRaisingEvents = true;
                Settings.nops.OutputDataReceived += NOPS_OutputDataReceived;

                animeTimer.Interval = TimeSpan.FromMilliseconds(1000 / 60);
                animeTimer.Tick += AnimeEditor.ColorAnime_Tick;
                animeTimer.Start();

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
                DefineSizing();

                if (File.Exists("Layout.json"))
                {
                    Layout layout = JsonConvert.DeserializeObject<Layout>(File.ReadAllText("Layout.json"));
                    LoadLayout(layout.mainWindowLayout, window);

                    foreach (var child in layout.windowLayouts)
                    {
                        MainWindow win = new MainWindow();
                        LoadLayout(child, win);
                        win.Show();
                    }
                    ListWindow.extraLeft = layout.extraLeft;
                    ListWindow.extraTop = layout.extraTop;

                    ListWindow.screenWidth = layout.screenWidth;
                    ListWindow.screenHeight = layout.screenHeight;
                    ListWindow.screenLeft = layout.screenLeft;
                    ListWindow.screenTop = layout.screenTop;
                    ListWindow.screenState = layout.screenState;

                    ListWindow.fileWidth = layout.fileWidth;
                    ListWindow.fileHeight = layout.fileHeight;
                    ListWindow.fileLeft = layout.fileLeft;
                    ListWindow.fileTop = layout.fileTop;
                    ListWindow.fileState = layout.fileState;

                    ListWindow.clutTop = layout.clutTop;
                    ListWindow.clutLeft = layout.clutLeft;

                    ColorDialog.pickerTop = layout.pickerTop;
                    ColorDialog.pickerLeft = layout.pickerLeft;

                    Tile16Editor.manualClutLeft = layout.manualClutLeft;
                    Tile16Editor.manualClutTop = layout.manualClutTop;

                    ListWindow.tileLeft = layout.tileLeft;
                    ListWindow.tileTop = layout.tileTop;

                    TextEditor.autoTextTop = layout.autoTextTop;
                    TextEditor.autoTextLeft = layout.autoTextLeft;

                    ToolsWindow.isoToolsOpen = layout.isoToolsOpen;
                    ToolsWindow.textureToolsOpen = layout.textureToolsOpen;
                    ToolsWindow.soundToolsOpen = layout.soundToolsOpen;
                    ToolsWindow.otherToolsOpen = layout.otherToolsOpen;

                    SpriteEditor.is1X = layout.is1X;
                }
                LockWindows();

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
                    Undo.CreateUndoList();
                    if (!settings.dontResetId)
                        Level.Id = 0;
                    Level.AssignPallete();
                    PSX.levels[Level.Id].LoadTextures();
                    //Draw Everything
                    Update();
                    window.spawnE.SetupSpecialSpawn();
                    window.camE.SetupBorderInfo();
                    UnlockWindows();
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
            window.clutE.DrawClut();
            window.clutE.DrawTextures();
            window.clutE.UpdateClutTxt();
            window.spawnE.SetSpawnSettings();
            window.camE.SetupCheckValues();
            window.enemyE.ReDraw();
            window.animeE.AssignLimits();
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
            window.enemyE.camLbl.Content = "X:" + Convert.ToString(window.enemyE.viewerX >> 8, 16).PadLeft(4, '0').ToUpper() + " Y:" + Convert.ToString(window.enemyE.viewerY, 16).PadLeft(4, '0').ToUpper();
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
                    PSX.levels.Clear();
                    MessageBox.Show("The PSX.EXE (SLUS_005.61) was not found");
                    LockWindows();
                    return;
                }
                //PSX.EXE was found
                PSX.exe = File.ReadAllBytes(fd.SelectedPath + "/SLUS_005.61");
                PSX.time = File.GetLastWriteTime(fd.SelectedPath + "/SLUS_005.61");
                Level.LoadLevels(fd.SelectedPath);

                if (PSX.levels.Count == 0)
                {
                    MessageBox.Show("No ARC level files were found");
                    LockWindows();
                    return;
                }
                else if (PSX.levels.Count != Const.FilesCount)
                {
                    PSX.levels.Clear();
                    MessageBox.Show("You need to have all level files in order to use this editor");
                    LockWindows();
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
                Undo.CreateUndoList();
                if (!settings.dontResetId)
                    Level.Id = 0;
                Level.AssignPallete();
                PSX.levels[Level.Id].LoadTextures();
                //Draw Everything
                Update();
                window.spawnE.SetupSpecialSpawn();
                window.camE.SetupBorderInfo();
                UnlockWindows();
            }
        }
        private void ProcessUndo()
        {
            if (hub.SelectedItem != null)
            {
                switch (((TabItem)hub.SelectedItem).Name)
                {
                    case "layoutTab":
                        if (LayoutEditor.undos[Level.Id].Count != 0)
                        {
                            int o = LayoutEditor.undos[Level.Id].Count - 1;
                            LayoutEditor.undos[Level.Id][o].ApplyLayoutUndo();
                            LayoutEditor.undos[Level.Id].RemoveAt(o);
                        }
                        break;
                    case "screenTab":
                        if(ScreenEditor.undos[Level.Id].Count != 0)
                        {
                            int o = ScreenEditor.undos[Level.Id].Count - 1;
                            ScreenEditor.undos[Level.Id][o].ApplyScreenUndo();
                            ScreenEditor.undos[Level.Id].RemoveAt(o);
                        }
                        break;
                    case "x16Tab":
                        if (Tile16Editor.undos[Level.Id].Count != 0)
                        {
                            int o = Tile16Editor.undos[Level.Id].Count - 1;
                            Tile16Editor.undos[Level.Id][o].ApplyTilesUndo();
                            Tile16Editor.undos[Level.Id].RemoveAt(o);
                        }
                        break;
                    default:
                        break;
                }
            }
        }
        internal void ToggleCollision()
        {
            if (Level.showCollision)
                Level.showCollision = false;
            else
                Level.showCollision = true;

            window.layoutE.DrawLayout();
            window.layoutE.DrawScreen();

            window.screenE.DrawScreen();
            window.screenE.DrawTiles();
            window.screenE.DrawTile();

            window.x16E.DrawTiles();
            window.x16E.DrawTile();

            window.enemyE.Draw();
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
                var result = MessageBox.Show("Are you sure you want to delete all of Layer " + (Level.BG + 1) + "?\nThis cant be un-done", "", MessageBoxButton.YesNo);

                if(result == MessageBoxResult.Yes)
                {
                    LayoutEditor.undos[Level.Id].Clear();
                    for (int i = 0; i < PSX.levels[Level.Id].size; i++)
                        PSX.levels[Level.Id].layout[i + PSX.levels[Level.Id].size * Level.BG] = 0;
                    PSX.edit = true;
                    window.layoutE.DrawLayout(true);
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
                    window.layoutE.DrawLayout(true);
                    UpdateViewrCam();
                }
            }
            else if (key == "S")
            {
                if ((window.layoutE.viewerY >> 8) < (PSX.levels[Level.Id].height - 3))
                {
                    window.layoutE.viewerY += 0x100;
                    window.layoutE.DrawLayout(true);
                    UpdateViewrCam();
                }
            }
            else if (key == "D")
            {
                if ((window.layoutE.viewerX >> 8) < (PSX.levels[Level.Id].width - 3))
                {
                    window.layoutE.viewerX += 0x100;
                    window.layoutE.DrawLayout(true);
                    UpdateViewrCam();
                }
            }
            else if (key == "A")
            {
                if (window.layoutE.viewerX != 0)
                {
                    window.layoutE.viewerX -= 0x100;
                    window.layoutE.DrawLayout(true);
                    UpdateViewrCam();
                }
            }
            else if (key == "D1")
            {
                if (Level.BG != 0)
                {
                    Level.BG = 0;
                    window.layoutE.DrawLayout(true);
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
                    window.layoutE.DrawLayout(true);
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
                    window.layoutE.DrawLayout(true);
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
                if (ScreenEditor.undos.Count == Const.MaxUndo)
                    ScreenEditor.undos.RemoveAt(0);
                ScreenEditor.undos[Level.Id].Add(Undo.CreateGroupScreenEditUndo((byte)window.screenE.screenId, 0, 0, 16, 16));
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
                ClutEditor.clut = ClutEditor.clut - 1;
                if(ClutEditor.clut < 0 && ClutEditor.bgF == 1) ClutEditor.clut = 0x1AF;
                if (ClutEditor.bgF == 0 && ClutEditor.clut < 0)
                    ClutEditor.clut = 0x3F;

                window.clutE.DrawTextures();
                window.clutE.UpdateClutTxt();
            }
            else if (key == "Down")
            {
                ClutEditor.clut = ClutEditor.clut + 1;
                if (ClutEditor.clut > (window.clutE.clutGrid.RowDefinitions.Count - 1) || ClutEditor.clut > 0x1AF)
                    ClutEditor.clut = 0;
                if (ClutEditor.bgF == 0 && ClutEditor.clut > 0x3F)
                    ClutEditor.clut = 0;

                window.clutE.DrawTextures();
                window.clutE.UpdateClutTxt();
            }
            else if (key == "Left")
            {
                window.clutE.UpdateTpageButton(ClutEditor.page - 1);
            }
            else if (key == "Right")
            {
                window.clutE.UpdateTpageButton(ClutEditor.page + 1);
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
                if (Level.enemyExpand)
                    PSX.levels[Level.Id].edit = true;
                else
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
            else if (key == "D1")
            {
                if (Level.BG != 0)
                {
                    Level.BG = 0;
                    window.layoutE.DrawLayout(true);
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
                    window.layoutE.DrawLayout(true);
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
                    window.layoutE.DrawLayout(true);
                    window.layoutE.UpdateBtn();
                    window.enemyE.Draw();
                    if (ListWindow.screenViewOpen)
                    {
                        layoutWindow.DrawScreens();
                        layoutWindow.Title = "All Screens in Layer " + (Level.BG + 1);

                    }
                }
            }
            else if (key == "PageUp")
                window.enemyE.bar.ScrollToTop();
            else if (key == "Next")
                window.enemyE.bar.ScrollToBottom();
        }
        private void AnimeKeyCheck(string key, bool notFocus)
        {
            if (!notFocus)  //check if NumInt is focused
                return;
            if (PSX.levels[Level.Id].clutAnime == null) return;

            if(key == "Up")
            {
                AnimeEditor.clut--;
                if (AnimeEditor.clut < 0)
                    AnimeEditor.clut = (PSX.levels[Level.Id].clutAnime.Length / 32) - 1;
                window.animeE.UpdateClutTxt();
            }
            else if(key == "Down")
            {
                AnimeEditor.clut = (AnimeEditor.clut + 1) % (PSX.levels[Level.Id].clutAnime.Length / 32);
                window.animeE.UpdateClutTxt();
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

                if (Settings.ExtractedTriggers)
                {
                    for (int i = 0; i < Settings.EditedTriggers.Length; i++)
                    {
                        if (Settings.EditedTriggers[i] == true)
                        {
                            foreach (var l in PSX.levels)
                            {
                                if (l.GetIndex() == i)
                                    File.WriteAllBytes(PSX.filePath + "/CAMERA/" + l.arc.filename + ".BIN", l.GetTriggerData());
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
            if (PSX.levels[Level.Id].isMid())
                Level.SaveLevel(Level.Id - 1);
            else
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

                    int clutLength = -1;

                    //CLUT
                    if (!Level.zeroFlag && PSX.levels[Level.Id].clut_X != null)
                    {
                        await Redux.Write((uint)(Settings.levelSize + Settings.levelStartAddress), PSX.levels[Level.Id].clut_X.entries[0].data);
                        await Redux.Write(Const.UpdateClutAddress, (byte)1);
                        clutLength = PSX.levels[Level.Id].clut_X.entries[0].data.Length;
                    }
                    else if (Level.zeroFlag && PSX.levels[Level.Id].clut_Z != null)
                    {
                        await Redux.Write((uint)(Settings.levelSize + Settings.levelStartAddress), PSX.levels[Level.Id].clut_Z.entries[0].data);
                        await Redux.Write(Const.UpdateClutAddress, (byte)1);
                        clutLength = PSX.levels[Level.Id].clut_Z.entries[0].data.Length;
                    }

                    //Backup Screen Data
                    if(clutLength != -1)
                        await Redux.Write((uint)(Settings.levelSize + Settings.levelStartAddress + clutLength), PSX.levels[Level.Id].screenData);

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

                    //Check Points
                    await SpawnWindow.WriteCheckPoints();

                    //Clut Anime
                    if (PSX.levels[Level.Id].clutAnime != null)
                        await Redux.Write(Settings.levelClutAnimeAddress, PSX.levels[Level.Id].clutAnime);

                    if(index < 27)
                    {
                        uint infoAddress = BitConverter.ToUInt32(PSX.exe, PSX.CpuToOffset((uint)(Const.ClutInfoPointersAddress + index * 4)));
                        if(infoAddress != 0)
                        {
                            //Clut Anime Info
                            uint startAddress = BitConverter.ToUInt32(PSX.exe,PSX.CpuToOffset(infoAddress));
                            data = new byte[infoAddress - startAddress];
                            Array.Copy(PSX.exe,PSX.CpuToOffset(startAddress), data, 0, data.Length);
                            await Redux.Write(startAddress, data);

                            //Clut Dest Info
                            data = new byte[(Const.MaxClutAnimes[index] + 1) * 2];
                            startAddress = BitConverter.ToUInt32(PSX.exe, PSX.CpuToOffset((uint)(Const.ClutDestPointersAddress + index * 4)));
                            Array.Copy(PSX.exe, PSX.CpuToOffset(startAddress), data, 0, data.Length);
                            await Redux.Write(startAddress, data);
                        }
                    }

                    if (index < 26)
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
                        if(!Level.enemyExpand)
                            await Redux.Write(PSX.OffsetToCpu(Const.EnemyDataPointersOffset + index * 4), BitConverter.ToInt32(PSX.exe, Const.EnemyDataPointersOffset + index * 4));
                        await Redux.Write(PSX.OffsetToCpu(Const.StartEnemyDataPointersOffset + index * 4), BitConverter.ToInt32(PSX.exe, Const.StartEnemyDataPointersOffset + index * 4));
                        if (id != 9 && id != 0xA) //2nd Half
                        {
                            index++;

                            if (PSX.levels[Level.Id].arc.filename == "ST0B_0X.ARC")
                                index++;

                            if (!Level.enemyExpand)
                                await Redux.Write(PSX.OffsetToCpu(Const.EnemyDataPointersOffset + index * 4), BitConverter.ToInt32(PSX.exe, Const.EnemyDataPointersOffset + index * 4));
                            await Redux.Write(PSX.OffsetToCpu(Const.StartEnemyDataPointersOffset + index * 4), BitConverter.ToInt32(PSX.exe, Const.StartEnemyDataPointersOffset + index * 4));
                        }
                        index = PSX.levels[Level.Id].GetIndex();

                        if (Level.enemyExpand)
                            await Redux.Write(Settings.levelEnemyAddress, Level.CreateEnemyData(PSX.levels[Level.Id].enemies));

                        //Camera
                        data = new byte[0x7FC];
                        Array.Copy(PSX.exe, PSX.CpuToOffset(Const.CameraTriggerFreeDataAddress), data, 0, data.Length);
                        await Redux.Write(Const.CameraTriggerFreeDataAddress, data);
                    }

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
            IEnumerable<Dragablz.DragablzItem> tabs = this.hub.GetOrderedHeaders();

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
        private void SaveLayout() //TODO: evenchually  figure out way to save docking layouts
        {
            Layout layout = new Layout();

            if (ListWindow.extraOpen)
            {
                ListWindow.extraLeft = extraWindow.Left;
                ListWindow.extraTop = extraWindow.Top;
            }
            layout.extraLeft = ListWindow.extraLeft;
            layout.extraTop = ListWindow.extraTop;

            if (ListWindow.screenViewOpen)
            {
                ListWindow.screenLeft = layoutWindow.Left;
                ListWindow.screenTop = layoutWindow.Top;
                ListWindow.screenWidth = layoutWindow.Width;
                ListWindow.screenHeight = layoutWindow.Height;
                ListWindow.screenState = (int)layoutWindow.WindowState;
            }
            layout.screenLeft = ListWindow.screenLeft;
            layout.screenTop = ListWindow.screenTop;
            layout.screenWidth = ListWindow.screenWidth;
            layout.screenHeight = ListWindow.screenHeight;
            layout.screenState = ListWindow.screenState;

            if (ListWindow.fileViewOpen)
            {
                ListWindow.fileLeft = fileWindow.Left;
                ListWindow.fileTop = fileWindow.Top;
                ListWindow.fileWidth = fileWindow.Width;
                ListWindow.fileHeight = fileWindow.Height;
                ListWindow.fileState = (int)fileWindow.WindowState;
            }
            layout.fileLeft = ListWindow.fileLeft;
            layout.fileTop = ListWindow.fileTop;
            layout.fileWidth = ListWindow.fileWidth;
            layout.fileHeight = ListWindow.fileHeight;
            layout.fileState = ListWindow.fileState;

            layout.clutLeft = ListWindow.clutLeft;
            layout.clutTop = ListWindow.clutTop;

            layout.pickerLeft = ColorDialog.pickerLeft;
            layout.pickerTop = ColorDialog.pickerTop;

            layout.manualClutLeft = Tile16Editor.manualClutLeft;
            layout.manualClutTop = Tile16Editor.manualClutTop;

            layout.tileLeft = ListWindow.tileLeft;
            layout.tileTop = ListWindow.tileTop;

            layout.autoTextTop = TextEditor.autoTextTop;
            layout.autoTextLeft = TextEditor.autoTextLeft;

            layout.textureToolsOpen = ToolsWindow.textureToolsOpen;
            layout.soundToolsOpen = ToolsWindow.soundToolsOpen;
            layout.isoToolsOpen = ToolsWindow.isoToolsOpen;
            layout.otherToolsOpen = ToolsWindow.otherToolsOpen;

            layout.is1X = SpriteEditor.is1X;

            foreach (Window childWind in Application.Current.Windows)
            {
                if (childWind.GetType() != typeof(MainWindow)) continue;
                MainWindow window = childWind as MainWindow;
                if (window.Width < 1) continue;

                WindowLayout windowLayout = new WindowLayout();
                windowLayout.top = window.Top;
                windowLayout.left = window.Left;
                windowLayout.width = window.Width;
                windowLayout.height = window.Height;
                windowLayout.max = window.max;
                windowLayout.windowState = (int)window.WindowState;
                if(window.dock.Content.GetType() == typeof(Dragablz.Dockablz.Branch))
                {
                    windowLayout.type = typeof(BranchLayout);
                    List<Dragablz.Dockablz.Branch> branches = new List<Dragablz.Dockablz.Branch>();
                    List<Dragablz.Dockablz.Branch> innerBranches = new List<Dragablz.Dockablz.Branch>();
                    branches.Add(window.dock.Content as Dragablz.Dockablz.Branch);
                    List<string> tabs = new List<string>();
                    BranchLayout startBranch = windowLayout.child as BranchLayout;
                    BranchLayout nextBranch = windowLayout.child as BranchLayout;
                BranchLoop:
                    foreach (var b in branches)
                    {
                        if(b.FirstItem.GetType() == typeof(Dragablz.Dockablz.Branch))
                        {
                            innerBranches.Add(new Dragablz.Dockablz.Branch()
                            {
                                Orientation = ((Dragablz.Dockablz.Branch)b.FirstItem).Orientation,
                                FirstItem = ((Dragablz.Dockablz.Branch)b.FirstItem).FirstItem,
                                FirstItemLength = ((Dragablz.Dockablz.Branch)b.FirstItem).FirstItemLength,
                                SecondItem = b.SecondItem,
                                SecondItemLength = b.SecondItemLength
                            });
                        }
                        else
                        {
                            foreach (var t in ((Dragablz.TabablzControl)branches[0].FirstItem).Items)
                            {
                                if (t.GetType() != typeof(TabItem)) continue;
                                tabs.Add(((TabItem)t).Name);
                            }
                        }
                        if (b.SecondItem.GetType() == typeof(BranchLayout))
                        {
                            innerBranches.Add(new Dragablz.Dockablz.Branch()
                            {
                                Orientation = ((Dragablz.Dockablz.Branch)b.SecondItem).Orientation,
                                FirstItem = ((Dragablz.Dockablz.Branch)b.SecondItem).FirstItem,
                                FirstItemLength = ((Dragablz.Dockablz.Branch)b.SecondItem).FirstItemLength,
                                SecondItem = ((Dragablz.Dockablz.Branch)b.SecondItem).SecondItem,
                                SecondItemLength = ((Dragablz.Dockablz.Branch)b.SecondItem).SecondItemLength
                            });
                        }
                        else
                        {
                            foreach (var t in ((Dragablz.TabablzControl)branches[0].SecondItem).Items)
                            {
                                if (t.GetType() != typeof(TabItem)) continue;
                                tabs.Add(((TabItem)t).Name);
                            }
                        }
                    }
                    branches.Clear();
                    if(innerBranches.Count != 0)
                    {
                        branches = new List<Dragablz.Dockablz.Branch>(innerBranches);
                        innerBranches.Clear();
                        goto BranchLoop;
                    }
                    windowLayout.child = tabs;
                }
                else
                {
                    windowLayout.type = typeof(Dragablz.TabablzControl);
                    List<string> tabs = new List<string>();
                    foreach (var t in window.hub.GetOrderedHeaders())
                    {
                        if (t.GetType() != typeof(Dragablz.DragablzItem)) continue;
                        tabs.Add(((TabItem)t.Content).Name);
                    }
                    windowLayout.child = tabs;
                }

                if (window == MainWindow.window)
                    layout.mainWindowLayout = windowLayout;
                else
                    layout.windowLayouts.Add(windowLayout);
            }
            //Done
            string json = JsonConvert.SerializeObject(layout, Formatting.Indented);
            File.WriteAllText("Layout.json", json);
        }
        private void LoadLayout(WindowLayout winlayout, MainWindow win)
        {
            win.WindowStartupLocation = WindowStartupLocation.Manual;
            win.Left = winlayout.left;
            win.Top = winlayout.top;
            win.Width = winlayout.width;
            win.Height = winlayout.height;
            if (winlayout.max)
                win.max = true;
            else
                win.Uid = winlayout.windowState.ToString();

            if (win != window)
            {
                win.hub.Items.Clear();
                object child = winlayout.child;
                Type type = winlayout.type;
                if(type != typeof(Dragablz.TabablzControl))
                {

                }
                else
                {
                    Newtonsoft.Json.Linq.JArray jArray = child as Newtonsoft.Json.Linq.JArray;
                    foreach (var j in jArray)
                    {
                        string t = j.ToString();

                        if(t == "layoutTab")
                        {
                            window.hub.RemoveFromSource(window.layoutTab);
                            win.hub.AddToSource(window.layoutTab);
                        }
                        else if(t == "screenTab")
                        {
                            window.hub.RemoveFromSource(window.screenTab);
                            win.hub.AddToSource(window.screenTab);
                        }
                        else if (t == "x16Tab")
                        {
                            window.hub.RemoveFromSource(window.x16Tab);
                            win.hub.AddToSource(window.x16Tab);
                        }
                        else if (t == "clutTab")
                        {
                            window.hub.RemoveFromSource(window.clutTab);
                            win.hub.AddToSource(window.clutTab);
                        }
                        else if (t == "enemyTab")
                        {
                            window.hub.RemoveFromSource(window.enemyTab);
                            win.hub.AddToSource(window.enemyTab);
                        }
                        else if (t == "spawnTab")
                        {
                            window.hub.RemoveFromSource(window.spawnTab);
                            win.hub.AddToSource(window.spawnTab);
                        }
                        else if (t == "bgTab")
                        {
                            window.hub.RemoveFromSource(window.bgTab);
                            win.hub.AddToSource(window.bgTab);
                        }
                        else if (t == "camTab")
                        {
                            window.hub.RemoveFromSource(window.camTab);
                            win.hub.AddToSource(window.camTab);
                        }
                        else if (t == "animeTab")
                        {
                            window.hub.RemoveFromSource(window.animeTab);
                            win.hub.AddToSource(window.animeTab);
                        }
                    }
                }
            }
        }
        private void LockWindows()
        {
            foreach (var childWind in Application.Current.Windows)
            {
                if (childWind.GetType() != typeof(MainWindow)) continue;
                MainWindow window = childWind as MainWindow;
                if (window.Width < 1) continue;
                window.hub.Visibility = Visibility.Hidden;
            }
            if (ListWindow.screenViewOpen)
                layoutWindow.Close();
            if (ListWindow.extraOpen)
                extraWindow.Close();
            if (ListWindow.fileViewOpen)
                fileWindow.Close();
        }
        private void UnlockWindows()
        {
            foreach (var childWind in Application.Current.Windows)
            {
                if (childWind.GetType() != typeof(MainWindow)) continue;
                MainWindow window = childWind as MainWindow;
                if (window.Width < 1) continue;
                window.hub.Visibility = Visibility.Visible;
            }
            if(settings.autoScreen && !ListWindow.screenViewOpen)
            {
                layoutWindow = new ListWindow(0);
                layoutWindow.Show();
            }
            if(settings.autoExtra && !ListWindow.extraOpen)
            {
                extraWindow = new ListWindow(2);
                extraWindow.Show();
            }
            if(settings.autoFiles && !ListWindow.fileViewOpen)
            {
                fileWindow = new ListWindow(4);
                fileWindow.Show();
            }
            window.Focus();
        }
        private void CloseChildWindows()
        {
            var childWindows = Application.Current.Windows.Cast<Window>().Where(w => w != Application.Current.MainWindow).ToList();
            window.hub.ConsolidateOrphanedItems = false;
            foreach (var window in childWindows)
                window.Close();
        }
        public void DefineSizing()
        {
            int W;
            if (settings.referanceWidth < 200)
                W = (int)(40 * SystemParameters.PrimaryScreenWidth / 100);
            else
                W = 40 * settings.referanceWidth / 100;
            window.layoutE.selectImage.MaxWidth = W;
            window.screenE.tileImage.MaxWidth = W;
            window.x16E.textureImage.MaxWidth = W;
            window.enemyE.canvas.Width = window.enemyE.bmp.PixelWidth;
        }
        #endregion Methods

        #region Events
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.max) //Layout Stuff
            {
                this.WindowStyle = WindowStyle.None;
                this.WindowState = WindowState.Maximized;
            }
            else if(this.Uid != "")
                this.WindowState = (WindowState)Convert.ToInt32(this.Uid);

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
                if (PSX.levels.Count == 0) goto End;

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
                if(!settings.dontSaveLayout)
                    SaveLayout();
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
                }else if(key == "Z" && PSX.levels.Count == Const.FilesCount)
                {
                    ProcessUndo();
                }else if(key == "C" && PSX.levels.Count == Const.FilesCount && Keyboard.FocusedElement.GetType() != typeof(Xceed.Wpf.Toolkit.WatermarkTextBox) && hub.SelectedItem != null)
                {
                    if (((TabItem)hub.SelectedItem).Name == "animeTab")
                        window.animeE.CopySet();
                    else
                        ToggleCollision();
                    return;
                }
                else if(key == "V" && PSX.levels.Count == Const.FilesCount && Keyboard.FocusedElement.GetType() != typeof(Xceed.Wpf.Toolkit.WatermarkTextBox) && hub.SelectedItem != null)
                {
                    if (((TabItem)hub.SelectedItem).Name == "animeTab")
                        window.animeE.PasteSet();
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
                case "animeTab":
                    {
                        AnimeKeyCheck(key, nonNumInt);
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
        private void undoBtn_Click(object sender, RoutedEventArgs e)
        {
            if (PSX.levels.Count == Const.FilesCount)
                ProcessUndo();
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
