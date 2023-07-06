using System;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TeheManX4.Forms
{
    /// <summary>
    /// Interaction logic for SpawnWindow.xaml
    /// </summary>
    public partial class SpawnWindow : UserControl
    {
        #region Constructors
        public SpawnWindow()
        {
            InitializeComponent();
        }
        #endregion Constructors

        #region Methods
        public void SetSpawnSettings()
        {
            int index = PSX.levels[Level.Id].GetIndex();

            if (Settings.MaxPoints[index] == 0xFF)
            {
                MainWindow.window.spawnE.spawnInt.IsEnabled = false;
                MainWindow.window.spawnE.IsEnabled = false;
                MainWindow.window.spawnE.megaIntX.IsEnabled = false;
                MainWindow.window.spawnE.megaIntY.IsEnabled = false;
                MainWindow.window.spawnE.camIntX.IsEnabled = false;
                MainWindow.window.spawnE.camIntY.IsEnabled = false;
                MainWindow.window.spawnE.cam2IntX.IsEnabled = false;
                MainWindow.window.spawnE.cam2IntY.IsEnabled = false;
                MainWindow.window.spawnE.cam3IntX.IsEnabled = false;
                MainWindow.window.spawnE.cam3IntY.IsEnabled = false;
                MainWindow.window.spawnE.camLeftInt.IsEnabled = false;
                MainWindow.window.spawnE.camRightInt.IsEnabled = false;
                MainWindow.window.spawnE.camTopInt.IsEnabled = false;
                MainWindow.window.spawnE.camBottomInt.IsEnabled = false;
                MainWindow.window.spawnE.flipInt.IsEnabled = false;
                MainWindow.window.spawnE.baseX2Int.IsEnabled = false;
                MainWindow.window.spawnE.baseY2Int.IsEnabled = false;
                MainWindow.window.spawnE.baseX3Int.IsEnabled = false;
                MainWindow.window.spawnE.baseY3Int.IsEnabled = false;
                MainWindow.window.spawnE.colTimer.IsEnabled = false;
            }
            else
            {
                MainWindow.window.spawnE.spawnInt.Value = 0;
                MainWindow.window.spawnE.spawnInt.Maximum = Settings.MaxPoints[index];
                uint address = BitConverter.ToUInt32(PSX.exe, PSX.CpuToOffset((uint)(Const.CheckPointPointersAddress + (index * 4))));
                uint dataAddress = BitConverter.ToUInt32(PSX.exe, (int)(PSX.CpuToOffset(address) + (MainWindow.window.spawnE.spawnInt.Value * 4)));
                SetIntValues(PSX.CpuToOffset(dataAddress));

                if (!MainWindow.window.spawnE.megaIntX.IsEnabled)
                {
                    MainWindow.window.spawnE.IsEnabled = true;
                    MainWindow.window.spawnE.megaIntX.IsEnabled = true;
                    MainWindow.window.spawnE.megaIntY.IsEnabled = true;
                    MainWindow.window.spawnE.camIntX.IsEnabled = true;
                    MainWindow.window.spawnE.camIntY.IsEnabled = true;
                    MainWindow.window.spawnE.cam2IntX.IsEnabled = true;
                    MainWindow.window.spawnE.cam2IntY.IsEnabled = true;
                    MainWindow.window.spawnE.cam3IntX.IsEnabled = true;
                    MainWindow.window.spawnE.cam3IntY.IsEnabled = true;
                    MainWindow.window.spawnE.camLeftInt.IsEnabled = true;
                    MainWindow.window.spawnE.camRightInt.IsEnabled = true;
                    MainWindow.window.spawnE.camTopInt.IsEnabled = true;
                    MainWindow.window.spawnE.camBottomInt.IsEnabled = true;
                    MainWindow.window.spawnE.flipInt.IsEnabled = true;
                    MainWindow.window.spawnE.baseX2Int.IsEnabled = true;
                    MainWindow.window.spawnE.baseY2Int.IsEnabled = true;
                    MainWindow.window.spawnE.baseX3Int.IsEnabled = true;
                    MainWindow.window.spawnE.baseY3Int.IsEnabled = true;
                    MainWindow.window.spawnE.colTimer.IsEnabled = true;
                }
            }
        }
        public void SetupSpecialSpawn()
        {

            uint addr;
            if (tableInt.Value == 0)
            {
                addr = Const.PeacockSpecialSpawnTableAddress;
                idInt.Maximum = 5;
            }
            else if (tableInt.Value == 1)
            {
                addr = Const.MushroomSpecialSpawnTableAddress;
                idInt.Maximum = 7;
            }
            else
            {
                addr = Const.RefightsSpecialSpawnTableAddress;
                idInt.Maximum = 19;
            }
            if (idInt.Value != 0)
                idInt.Value = 0;

            ushort drop = BitConverter.ToUInt16(PSX.exe, PSX.CpuToOffset((uint)(addr + idInt.Value * 2)));
            dropInt.Value = drop;
        }
        private void SetIntValues(int offset)
        {
            MainWindow.window.spawnE.megaIntX.Value = BitConverter.ToUInt16(PSX.exe, offset);
            MainWindow.window.spawnE.megaIntY.Value = BitConverter.ToUInt16(PSX.exe, offset + 2);
            MainWindow.window.spawnE.camIntX.Value = BitConverter.ToUInt16(PSX.exe, offset + 4);
            MainWindow.window.spawnE.camIntY.Value = BitConverter.ToUInt16(PSX.exe, offset + 6);
            MainWindow.window.spawnE.cam2IntX.Value = BitConverter.ToUInt16(PSX.exe, offset + 8);
            MainWindow.window.spawnE.cam2IntY.Value = BitConverter.ToUInt16(PSX.exe, offset + 10);
            MainWindow.window.spawnE.cam3IntX.Value = BitConverter.ToUInt16(PSX.exe, offset + 12);
            MainWindow.window.spawnE.cam3IntY.Value = BitConverter.ToUInt16(PSX.exe, offset + 14);
            MainWindow.window.spawnE.camLeftInt.Value = BitConverter.ToUInt16(PSX.exe, offset + 16);
            MainWindow.window.spawnE.camRightInt.Value = BitConverter.ToUInt16(PSX.exe, offset + 18);
            MainWindow.window.spawnE.camTopInt.Value = BitConverter.ToUInt16(PSX.exe, offset + 20);
            MainWindow.window.spawnE.camBottomInt.Value = BitConverter.ToUInt16(PSX.exe, offset + 22);
            MainWindow.window.spawnE.flipInt.Value = BitConverter.ToUInt16(PSX.exe, offset + 24);
            MainWindow.window.spawnE.baseX2Int.Value = BitConverter.ToUInt16(PSX.exe, offset + 26);
            MainWindow.window.spawnE.baseY2Int.Value = BitConverter.ToUInt16(PSX.exe, offset + 28);
            MainWindow.window.spawnE.baseX3Int.Value = BitConverter.ToUInt16(PSX.exe, offset + 30);
            MainWindow.window.spawnE.baseY3Int.Value = BitConverter.ToUInt16(PSX.exe, offset + 32);
            MainWindow.window.spawnE.colTimer.Value = BitConverter.ToUInt16(PSX.exe, offset + 34);
        }
        public static async Task WriteCheckPoints()
        {
            int index = PSX.levels[Level.Id].GetIndex();

            //Checkpoint Data
            byte[] data = new byte[0x1020];
            Array.Copy(PSX.exe, PSX.CpuToOffset(0x800f3314), data, 0, 0x1020);
            await Redux.Write(0x800f3314, data);

            //Backgrounds Scroll Type
            int o = index * 2 + PSX.CpuToOffset(Const.BackgroundTypeTableAddress);
            await Redux.Write(PSX.OffsetToCpu(o), BitConverter.ToUInt16(PSX.exe, o));

            //Background & Object Priority
            o = index * 10 + PSX.CpuToOffset(Const.BackgroundSettingsAddress);
            byte[] info = new byte[10];
            Array.Copy(PSX.exe, o, info, 0, 10);
            await Redux.Write(PSX.OffsetToCpu(o), info);

            //GPU Command E1 Trans Setting
            await Redux.Write((uint)(Const.TransSettingsAddress + index), PSX.exe[PSX.CpuToOffset(Const.TransSettingsAddress) + index]);

            //Peacock Spawn
            int offset = PSX.CpuToOffset(Const.PeacockSpecialSpawnTableAddress);
            Array.Resize(ref data, 12);
            Array.Copy(PSX.exe, offset, data, 0, 12);
            await Redux.Write(Const.PeacockSpecialSpawnTableAddress, data);

            //Mushroom Spawn
            offset = PSX.CpuToOffset(Const.MushroomSpecialSpawnTableAddress);
            Array.Resize(ref data, 16);
            Array.Copy(PSX.exe, offset, data, 0, 16);
            await Redux.Write(Const.MushroomSpecialSpawnTableAddress, data);

            //Refights Spawn
            offset = PSX.CpuToOffset(Const.RefightsSpecialSpawnTableAddress);
            Array.Resize(ref data, 40);
            Array.Copy(PSX.exe, offset, data, 0, 40);
            await Redux.Write(Const.RefightsSpecialSpawnTableAddress, data);
        }
        #endregion Methods

        #region Events
        private void spawnInt_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            if (PSX.levels.Count != Const.FilesCount || megaIntX == null)
                return;
            int stageId = PSX.levels[Level.Id].GetIndex();
            if (stageId == -1)
                return;
            uint address = BitConverter.ToUInt32(PSX.exe, PSX.CpuToOffset((uint)(Const.CheckPointPointersAddress + (stageId * 4))));
            uint dataAddress = BitConverter.ToUInt32(PSX.exe, PSX.CpuToOffset(address) + ((int)e.NewValue * 4));

            SetIntValues(PSX.CpuToOffset(dataAddress));

        }
        private void setting_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            if (PSX.levels.Count != Const.FilesCount || e.NewValue == null || e.OldValue == null)
                return;
            int stageId = PSX.levels[Level.Id].GetIndex();
            if (stageId == -1)
                return;
            uint address = BitConverter.ToUInt32(PSX.exe, PSX.CpuToOffset((uint)(Const.CheckPointPointersAddress + (stageId * 4))));
            uint dataAddress = BitConverter.ToUInt32(PSX.exe, (int)(PSX.CpuToOffset(address) + (MainWindow.window.spawnE.spawnInt.Value * 4)));

            NumInt updown = (NumInt)sender;

            int t = 0;

            int i = Convert.ToInt32(updown.Uid.Split(' ')[0]);
            if (updown.Uid.Split(' ').Length != 1)
                t = Convert.ToInt32(updown.Uid.Split(' ')[1]);
            switch (t) //Size of data to modify
            {
                case 1:
                    byte b = PSX.exe[PSX.CpuToOffset((uint)(dataAddress + i))];
                    if (b == (byte)(int)e.NewValue)
                        return;
                    PSX.exe[PSX.CpuToOffset((uint)(dataAddress + i))] = (byte)(int)e.NewValue;
                    PSX.edit = true;
                    break;
                default:
                    ushort val = BitConverter.ToUInt16(PSX.exe, PSX.CpuToOffset((uint)(dataAddress + i)));
                    if (val == (ushort)(int)e.NewValue)
                        return;
                    PSX.exe[PSX.CpuToOffset((uint)(dataAddress + i))] = (byte)(((int)e.NewValue) & 0xFF);
                    PSX.exe[PSX.CpuToOffset((uint)(dataAddress + i + 1))] = (byte)(((int)e.NewValue >> 8) & 0xFF);
                    PSX.edit = true;
                    Settings.EditedPoints[stageId] = true;
                    break;
            }
        }
        private async void Goto_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (MainWindow.settings.useNops)
            {
                try
                {
                    ListWindow.checkpoingGo = true;
                    MainWindow.loadWindow = new ListWindow(false);
                    MainWindow.loadWindow.ShowDialog();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message, "ERROR");
                }
            }
            else
            {
                try
                {
                    await Redux.Pause();
                    await WriteCheckPoints();
                    await Redux.Write(Const.CheckPointAddress, (byte)spawnInt.Value);
                    await Redux.Write(Const.ClearLevelAddress, 0xC0);
                    await Redux.Resume();
                }
                catch (System.Net.Http.HttpRequestException ex)
                {
                    System.Windows.MessageBox.Show(ex.Message, "REDUX ERROR");
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message, "ERROR");
                }
            }
        }
        private void GearBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (Settings.ExtractedPoints) //Show window for configuring each point amount
            {
                ListWindow win = new ListWindow(6);
                win.ShowDialog();
            }
            else //Extract
            {
                System.Windows.MessageBox.Show("In order to edit the amount of checkpoints per stage you need to extract the data from the un-edited PSX.EXE");
                using(var fd = new System.Windows.Forms.OpenFileDialog())
                {
                    fd.Filter = "PSX-EXE |*61";
                    fd.Title = "Select MegaMan X4 PSX-EXE";

                    if(fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        byte[] exe = System.IO.File.ReadAllBytes(fd.FileName);

                        ICSharpCode.SharpZipLib.Checksum.Crc32 crc32 = new ICSharpCode.SharpZipLib.Checksum.Crc32();
                        crc32.Update(exe);

                        if(crc32.Value != Const.Crc)
                        {
                            System.Windows.MessageBox.Show("Modified EXE , CRC is: " + Convert.ToString(crc32.Value, 16) + " it should be: " + Convert.ToString(Const.Crc, 16));
                            return;
                        }
                        else
                        {
                            //Valid EXE (Extract Checkpoints)
                            byte[] data = new byte[36];
                            int pastIndex = -1;

                            System.IO.Directory.CreateDirectory(PSX.filePath + "/CHECKPOINT");
                            foreach (var l in PSX.levels)
                            {
                                int index = l.GetIndex();
                                if (index == pastIndex)
                                    continue;
                                else
                                    pastIndex = index;

                                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                                System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms);

                                for (int i = 0; i < Const.MaxPoints[index] + 1; i++)
                                {
                                    uint addr = BitConverter.ToUInt32(exe, PSX.CpuToOffset((uint)(Const.CheckPointPointersAddress + index * 4)));
                                    uint read = BitConverter.ToUInt32(exe, PSX.CpuToOffset((uint)(addr + i * 4)));

                                    Array.Copy(exe, PSX.CpuToOffset(read), data, 0, 36);
                                    bw.Write(data);
                                }
                                System.IO.File.WriteAllBytes(PSX.filePath + "/CHECKPOINT/" + l.arc.filename + ".BIN", ms.ToArray());
                            }

                            //Done
                            System.Windows.MessageBox.Show("Checkpoints extracted!\nClose the Editor and re-open to be able to edit checkpoint amounts");
                        }
                    }
                }
            }
        }
        private void tableInt_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue == null || e.OldValue == null || PSX.levels.Count != Const.FilesCount) return;
            SetupSpecialSpawn();
        }

        private void idInt_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue == null || e.OldValue == null || PSX.levels.Count != Const.FilesCount) return;
            uint addr;
            if (tableInt.Value == 0)
                addr = Const.PeacockSpecialSpawnTableAddress;
            else if (tableInt.Value == 1)
                addr = Const.MushroomSpecialSpawnTableAddress;
            else
                addr = Const.RefightsSpecialSpawnTableAddress;
            ushort drop = BitConverter.ToUInt16(PSX.exe, PSX.CpuToOffset((uint)(addr + idInt.Value * 2)));
            dropInt.Value = drop;
        }

        private void dropInt_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue == null || e.OldValue == null || PSX.levels.Count != Const.FilesCount) return;
            uint addr;
            if (tableInt.Value == 0)
                addr = Const.PeacockSpecialSpawnTableAddress;
            else if (tableInt.Value == 1)
                addr = Const.MushroomSpecialSpawnTableAddress;
            else
                addr = Const.RefightsSpecialSpawnTableAddress;

            ushort drop = BitConverter.ToUInt16(PSX.exe, PSX.CpuToOffset((uint)(addr + idInt.Value * 2)));
            if (drop == (ushort)(int)e.NewValue) return;

            BitConverter.GetBytes((ushort)(int)e.NewValue).CopyTo(PSX.exe, PSX.CpuToOffset((uint)(addr + idInt.Value * 2)));
            PSX.edit = true;
        }
#endregion Events
    }
}
