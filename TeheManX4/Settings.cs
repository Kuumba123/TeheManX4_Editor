using ICSharpCode.SharpZipLib.Checksum;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace TeheManX4
{
    class Settings
    {
        #region Constants
        public static byte[] MaxPoints = new byte[Const.MaxPoints.Length];
        public static byte[] MaxTriggers = new byte[Const.MaxTriggers.Length];
        #endregion Constants

        #region Fields
        //Reload Fields
        public static uint levelStartAddress;
        public static uint levelScreenAddress;
        public static uint levelTileAddress;
        public static uint levelEnemyAddress;
        public static int levelSize;
        public static int playerFileSize;

        //Other Fields
        public static bool ExtractedPoints;
        public static bool[] EditedPoints = new bool[Const.MaxPoints.Length];
        public static bool ExtractedTriggers;
        public static bool[] EditedTriggers = new bool[Const.MaxTriggers.Length];
        static public Process nops = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = "nops.exe",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };
        #endregion Fields

        #region Properties
        public string webPort; //Redux
        public string comPort; //NOPS
        public bool useNops;
        public bool dontUpdate;
        public bool ultimate;
        public bool defaultZero;
        public bool saveOnReload;
        #endregion Properties

        #region Methods
        public static void DefineCheckpoints()
        {
            ExtractedPoints = false;
            if (Directory.Exists(PSX.filePath + "/CHECKPOINT"))
            {
                Crc32 crc32 = new Crc32();
                crc32.Update(PSX.exe);
                long oldCrc = crc32.Value;

                uint freeDataAddress = 0x800f3314;
                uint freePointerAddress = 0x800f4124;
                int pastIndex = -1;
                foreach (var l in PSX.levels)
                {
                    int index = l.GetIndex();
                    if (index == pastIndex)
                        continue;
                    else
                        pastIndex = index;
                    EditedPoints[index] = false;
                    int checkPoint = 0;
                    BinaryReader br = new BinaryReader(new MemoryStream(File.ReadAllBytes(PSX.filePath + "/CHECKPOINT/" + l.arc.filename + ".BIN")));

                    while (true) //Copy Checkpoint Data & Fix each Pointer
                    {
                        //Dump Data
                        br.ReadBytes(36).CopyTo(PSX.exe, PSX.CpuToOffset(freeDataAddress));
                        //Fix specfic Checkpoint Pointer
                        BitConverter.GetBytes(freeDataAddress).CopyTo(PSX.exe, PSX.CpuToOffset((uint)(freePointerAddress + checkPoint * 4)));

                        checkPoint++;
                        freeDataAddress += 36;

                        if (br.BaseStream.Position == br.BaseStream.Length)
                        {
                            MaxPoints[index] = (byte)(checkPoint - 1);
                            break;
                        }
                    }
                    //Fix stage specfic pointer
                    BitConverter.GetBytes(freePointerAddress).CopyTo(PSX.exe, PSX.CpuToOffset(Const.CheckPointPointersAddress) + index * 4);
                    freePointerAddress += (uint)(checkPoint * 4);
                }
                ExtractedPoints = true;

                crc32 = new Crc32();
                crc32.Update(PSX.exe);
                if (oldCrc != crc32.Value)
                    PSX.edit = true;
            }
            else
            {
                Const.MaxPoints.CopyTo(MaxPoints, 0);
                ExtractedPoints = false;
            }
        }
        public static void DefineBoxes()
        {
            ExtractedTriggers = false;
            if (Directory.Exists(PSX.filePath + "/CAMERA"))
            {
                Crc32 crc32 = new Crc32();
                crc32.Update(PSX.exe);
                long oldCrc = crc32.Value;
                uint freeDataAddress = Const.CameraTriggerFreeDataAddress;
                int pastIndex = -1;

                List<List<uint>> triggerAddresses = new List<List<uint>>();
                for (int i = 0; i < 26; i++)
                    triggerAddresses.Add(new List<uint>());

                foreach (var l in PSX.levels)
                {
                    int index = l.GetIndex();
                    if (index == pastIndex)
                        continue;
                    else if (index > 25)
                        break;
                    else
                        pastIndex = index;


                    EditedTriggers[index] = false;
                    byte triggerCount = 0;
                    BinaryReader br = new BinaryReader(new MemoryStream(File.ReadAllBytes(PSX.filePath + "/CAMERA/" + l.arc.filename + ".BIN")));

                    while (true)
                    {
                        if (br.BaseStream.Length < 10) //No Triggers Check
                        {
                            MaxTriggers[index] = 0xFF;
                            BitConverter.GetBytes(0).CopyTo(PSX.exe, PSX.CpuToOffset(Const.CameraTriggerPointersAddress) + index * 4);
                            break;
                        }

                        //Dump Box Sides
                        br.ReadBytes(8).CopyTo(PSX.exe, PSX.CpuToOffset(freeDataAddress));
                        triggerAddresses[index].Add(freeDataAddress);

                        freeDataAddress += 8;
                        triggerCount++;
                        ushort setting;

                        while (true)
                        {
                            setting = br.ReadUInt16();
                            BitConverter.GetBytes(setting).CopyTo(PSX.exe, PSX.CpuToOffset(freeDataAddress));
                            freeDataAddress += 2;

                            if(setting == 0) //Terminator
                                break;
                        }

                        //End Check
                        if (br.BaseStream.Position == br.BaseStream.Length)
                        {
                            MaxTriggers[index] = (byte)(triggerCount - 1);
                            break;
                        }
                    }
                }
                //Dump Mips
                while (true)
                {
                    if ((freeDataAddress % 4) != 0)
                        freeDataAddress++;
                    else
                        break;
                }

                //Apply Pointers
                for (int i = 0; i < triggerAddresses.Count; i++)
                {
                    if (triggerAddresses[i].Count == 0)
                        continue;
                    //Fix Main Pointer
                    BitConverter.GetBytes(freeDataAddress).CopyTo(PSX.exe, PSX.CpuToOffset(Const.CameraTriggerPointersAddress) + i * 4);

                    //Fix Sub Pointers
                    foreach (var a in triggerAddresses[i])
                    {
                        BitConverter.GetBytes(a).CopyTo(PSX.exe, PSX.CpuToOffset(freeDataAddress));
                        freeDataAddress += 4;
                    }
                }

                //Done
                ExtractedTriggers = true;

                crc32 = new Crc32();
                crc32.Update(PSX.exe);
                if (oldCrc != crc32.Value)
                    PSX.edit = true;
            }
            else
            {
                Const.MaxTriggers.CopyTo(MaxTriggers, 0);
                ExtractedTriggers = false;
            }
        }
        public static Settings SetDefaultSettings()
        {
            Settings s = new Settings();
            s.webPort = "8080";
            s.comPort = "5";
            s.useNops = false;
            return s;
        }
        public void CheckForValidSettings()
        {
            if (webPort == null)
                webPort = "8080";
            if (comPort == null)
                comPort = "5";
        }
        public static bool IsPastVersion(string ver)
        {
            foreach (var v in Const.pastVersions)
            {
                if (v == ver)
                    return true;
            }
            return false;
        }
        #endregion Methods
    }
}

