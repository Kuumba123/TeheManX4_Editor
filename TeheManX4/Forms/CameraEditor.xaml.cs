using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace TeheManX4.Forms
{
    /// <summary>
    /// Interaction logic for CameraWindow.xaml
    /// </summary>
    public partial class CameraEditor : UserControl
    {
        #region Properties
        List<NumInt> borderInts = new List<NumInt>();
        #endregion Properties

        #region Fields
        private static bool raidoEnable;
        private static bool added;
        #endregion Fields

        #region Constructors
        public CameraEditor()
        {
            InitializeComponent();

            if (!added)
            {
                added = true;

                for (int i = 0; i < 4; i++)
                {
                    NumInt num = new NumInt();
                    num.Uid = (i * 2).ToString();
                    num.FontSize = 28;
                    num.Width = 95;
                    num.ButtonSpinnerWidth = 25;
                    num.Minimum = 0;
                    num.Maximum = 73;
                    num.ValueChanged += BorderSettingListInt_ValueChanged;
                    borderInts.Add(num);
                    borderPannel.Children.Add(num);
                }
            }
        }
        #endregion Constructors

        #region Methods
        public void SetupBorderInfo()
        {
            int dataOffset = PSX.CpuToOffset(Const.BorderSettingsAddress);

            borderSettingInt.Value = 0;
            AssignBorderPropertyButtons(BitConverter.ToUInt16(PSX.exe, dataOffset));
            valueInt.Value = BitConverter.ToUInt16(PSX.exe, dataOffset + 2);
        }
        public void SetupCheckValues()
        {
            int index = PSX.levels[Level.Id].GetIndex();

            if(index > 25)
            {
                DisableBorderValues();
                return;
            }

            uint address = BitConverter.ToUInt32(PSX.exe, PSX.CpuToOffset((uint)(Const.CameraTriggerPointersAddress + (index * 4))));
            if(address == 0)
            {
                DisableBorderValues();
                return;
            }
            triggerInt.Value = 0;
            int dataOffset = PSX.CpuToOffset(BitConverter.ToUInt32(PSX.exe, PSX.CpuToOffset(address)));

            triggerInt.IsEnabled = true;
            triggerInt.Maximum = Settings.MaxTriggers[index];
            DefineBorderSettingListValues(dataOffset);

        }
        private void DisableBorderValues()
        {
            triggerInt.IsEnabled = false;
            for (int i = 0; i < 4; i++)
                borderInts[i].IsEnabled = false;
            rightInt.IsEnabled = false;
            leftInt.IsEnabled = false;
            bottomInt.IsEnabled = false;
            topInt.IsEnabled = false;
        }
        public void DefineBorderSettingListValues(int dataOffset)
        {
            for (int i = 0; i < 4; i++)
            {
                if (BitConverter.ToUInt16(PSX.exe, dataOffset + 8 + i * 2) == 0)
                {
                    while (i < 4)
                    {
                        borderInts[i].IsEnabled = false;
                        i++;
                    }
                    break;
                }
                //Get Camera Setting Ids
                borderInts[i].Value = BitConverter.ToUInt16(PSX.exe, dataOffset + 8 + i * 2) - 1;
                borderInts[i].IsEnabled = true;
            }
            rightInt.Value = BitConverter.ToUInt16(PSX.exe, dataOffset);
            leftInt.Value = BitConverter.ToUInt16(PSX.exe, dataOffset + 2);
            bottomInt.Value = BitConverter.ToUInt16(PSX.exe, dataOffset + 4);
            topInt.Value = BitConverter.ToUInt16(PSX.exe, dataOffset + 6);

            rightInt.IsEnabled = true;
            leftInt.IsEnabled = true;
            bottomInt.IsEnabled = true;
            topInt.IsEnabled = true;
        }
        public void AssignBorderPropertyButtons(int setting) //For Radio Btns
        {
            raidoEnable = false;
            if(setting == 0)
            {
                leftBtn.IsChecked = true;
                rightBtn.IsChecked = false;
                topBtn.IsChecked = false;
                bottomBtn.IsChecked = false;
            }else if(setting == 1)
            {
                leftBtn.IsChecked = false;
                rightBtn.IsChecked = true;
                topBtn.IsChecked = false;
                bottomBtn.IsChecked = false;
            }else if(setting == 2)
            {
                leftBtn.IsChecked = false;
                rightBtn.IsChecked = false;
                topBtn.IsChecked = true;
                bottomBtn.IsChecked = false;
            }
            else
            {
                leftBtn.IsChecked = false;
                rightBtn.IsChecked = false;
                topBtn.IsChecked = false;
                bottomBtn.IsChecked = true;
            }
            raidoEnable = true;
        }
        #endregion Methods

        #region Events
        private void GearBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Settings.ExtractedTriggers) //Show window for configuring each trigger amount
            {
                ListWindow win = new ListWindow(7);
                win.ShowDialog();
            }
            else //Extract
            {
                MessageBox.Show("In order to edit the amount of camera triggers per stage you need to extract the data from the un-edited PSX.EXE");
                using (var fd = new System.Windows.Forms.OpenFileDialog())
                {
                    fd.Filter = "PSX-EXE |*61";
                    fd.Title = "Select MegaMan X4 PSX-EXE";

                    if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        byte[] exe = System.IO.File.ReadAllBytes(fd.FileName);

                        ICSharpCode.SharpZipLib.Checksum.Crc32 crc32 = new ICSharpCode.SharpZipLib.Checksum.Crc32();
                        crc32.Update(exe);

                        if (crc32.Value != Const.Crc)
                        {
                            MessageBox.Show("This EXE CRC is: " + Convert.ToString(crc32.Value, 16) + " it should be: " + Convert.ToString(Const.Crc, 16));
                            return;
                        }
                        else
                        {
                            //Valid EXE (Extract Triggers)
                            int pastIndex = -1;

                            System.IO.Directory.CreateDirectory(PSX.filePath + "/CAMERA");
                            foreach (var l in PSX.levels)
                            {
                                int index = l.GetIndex();
                                if (index == pastIndex)
                                    continue;
                                else if (index > 25)
                                    break;
                                else
                                    pastIndex = index;

                                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                                System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms);

                                for (int i = 0; i < Const.MaxTriggers[index] + 1; i++)
                                {
                                    uint addr = BitConverter.ToUInt32(exe, PSX.CpuToOffset((uint)(Const.CameraTriggerPointersAddress + index * 4)));

                                    if(addr == 0) //No Triggers Check
                                    {
                                        bw.Write(-1);
                                        goto End;
                                    }
                                    uint read = BitConverter.ToUInt32(exe, PSX.CpuToOffset((uint)(addr + i * 4)));
                                    bw.Write(BitConverter.ToUInt64(exe, PSX.CpuToOffset(read)));
                                    read += 8;
                                    ushort setting;

                                    while (true)
                                    {
                                        setting = BitConverter.ToUInt16(exe, PSX.CpuToOffset(read));
                                        bw.Write(setting);
                                        read += 2;

                                        if (setting == 0)
                                            break;
                                    }
                                }
                            End:
                                System.IO.File.WriteAllBytes(PSX.filePath + "/CAMERA/" + l.arc.filename + ".BIN", ms.ToArray());
                            }

                            //Done
                            MessageBox.Show("Camera Triggers extracted!\nClose the Editor and re-open to be able to edit trigger amounts");
                        }
                    }
                }
            }
        }
        private void triggerInt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue == null || e.OldValue == null || PSX.levels.Count != Const.FilesCount) return;
            int index = PSX.levels[Level.Id].GetIndex();
            uint address = BitConverter.ToUInt32(PSX.exe, PSX.CpuToOffset((uint)(Const.CameraTriggerPointersAddress + index * 4)));
            int dataOffset = PSX.CpuToOffset(BitConverter.ToUInt32(PSX.exe, PSX.CpuToOffset((uint)(address + ((int)e.NewValue * 4)))));
            DefineBorderSettingListValues(dataOffset);
        }
        private void BorderSettingListInt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e) //For Trigger Settings
        {
            if (e.NewValue == null || e.OldValue == null || PSX.levels.Count != Const.FilesCount) return;

            if (e.NewValue == null || e.OldValue == null || PSX.levels.Count != Const.FilesCount) return;
            int index = PSX.levels[Level.Id].GetIndex();
            uint address = BitConverter.ToUInt32(PSX.exe, PSX.CpuToOffset((uint)(Const.CameraTriggerPointersAddress + index * 4)));
            int dataOffset = PSX.CpuToOffset(BitConverter.ToUInt32(PSX.exe, PSX.CpuToOffset((uint)(address + ((int)triggerInt.Value * 4)))));

            dataOffset += Convert.ToInt32(((NumInt)sender).Uid) + 8;

            ushort val = BitConverter.ToUInt16(PSX.exe, dataOffset);

            if (val != (ushort)(int)e.NewValue + 1)
            {
                BitConverter.GetBytes((ushort)(int)e.NewValue + 1).CopyTo(PSX.exe, dataOffset);
                PSX.edit = true;
                Settings.EditedTriggers[index] = true;
            }
        }
        private void SideInt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e) //RLBT
        {
            if (e.NewValue == null || e.OldValue == null || PSX.levels.Count != Const.FilesCount) return;
            int index = PSX.levels[Level.Id].GetIndex();
            uint address = BitConverter.ToUInt32(PSX.exe, PSX.CpuToOffset((uint)(Const.CameraTriggerPointersAddress + index * 4)));
            int dataOffset = PSX.CpuToOffset(BitConverter.ToUInt32(PSX.exe, PSX.CpuToOffset((uint)(address + ((int)triggerInt.Value * 4)))));

            dataOffset += Convert.ToInt32(((NumInt)sender).Uid);
            ushort val = BitConverter.ToUInt16(PSX.exe, dataOffset);

            if(val != (ushort)(int)e.NewValue)
            {
                BitConverter.GetBytes((ushort)(int)e.NewValue).CopyTo(PSX.exe, dataOffset);
                PSX.edit = true;
                Settings.EditedTriggers[index] = true;

            }

        }
        private void borderSettingInt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue == null || e.OldValue == null || PSX.levels.Count != Const.FilesCount) return;
            int dataOffset = PSX.CpuToOffset((uint)(Const.BorderSettingsAddress + (int)e.NewValue * 4));
            AssignBorderPropertyButtons(BitConverter.ToUInt16(PSX.exe, dataOffset));
            valueInt.Value = BitConverter.ToUInt16(PSX.exe, dataOffset + 2);
        }
        private void valueInt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue == null || e.OldValue == null || PSX.levels.Count != Const.FilesCount) return;
            int dataOffset = PSX.CpuToOffset((uint)(Const.BorderSettingsAddress + (int)borderSettingInt.Value * 4));
            ushort val = BitConverter.ToUInt16(PSX.exe, dataOffset + 2);

            if(val != (ushort)(int)e.NewValue)
            {
                BitConverter.GetBytes((ushort)(int)e.NewValue).CopyTo(PSX.exe, dataOffset + 2);
                PSX.edit = true;
            }
        }
        private void RadioBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!raidoEnable) return;

            ushort setting = Convert.ToUInt16(((RadioButton)sender).Uid); //New Setting

            int dataOffset = PSX.CpuToOffset((uint)(Const.BorderSettingsAddress + (int)borderSettingInt.Value * 4));
            ushort val = BitConverter.ToUInt16(PSX.exe, dataOffset);

            if (val != setting)
            {
                BitConverter.GetBytes(setting).CopyTo(PSX.exe, dataOffset);
                PSX.edit = true;
            }
        }
        #endregion Events
    }
}
