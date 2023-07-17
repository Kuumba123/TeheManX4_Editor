using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TeheManX4.Forms;

namespace TeheManX4
{
    class Undo
    {
        #region Properties
        internal int type;
        internal byte[] data;
        #endregion Properties

        #region Methoids
        internal static void CreateUndoList()
        {
            LayoutEditor.undos.Clear();
            ScreenEditor.undos.Clear();
            Tile16Editor.undos.Clear();
            foreach (var l in PSX.levels)
            {
                LayoutEditor.undos.Add(new List<Undo>());
                ScreenEditor.undos.Add(new List<Undo>());
                Tile16Editor.undos.Add(new List<Undo>());
            }
        }
        internal static void ClearLevelUndos()
        {
            foreach (var u in LayoutEditor.undos)
                u.Clear();
            foreach (var u in ScreenEditor.undos)
                u.Clear();
            foreach (var u in Tile16Editor.undos)
                u.Clear();
        }
        /*Layout Undo*/
        internal static Undo CreateLayoutUndo(int offset) //Offset including BG Layer
        {
            Undo u = new Undo();
            u.type = 0;

            byte[] undoData = BitConverter.GetBytes(offset);
            u.data = undoData.Concat(BitConverter.GetBytes(PSX.levels[Level.Id].layout[offset])).ToArray();

            return u;
        }
        internal void ApplyLayoutUndo()
        {
            int offset = BitConverter.ToInt32(this.data, 0);
            PSX.levels[Level.Id].layout[offset] = this.data[4];

            PSX.edit = true;

            MainWindow.window.layoutE.DrawLayout(true);
            MainWindow.window.enemyE.Draw();
        }

        /*Screen Undo*/
        internal static Undo CreateScreenEditUndo(byte screen, byte x, byte y)
        {
            byte[] undoData = new byte[5];
            undoData[0] = screen;
            undoData[1] = x;
            undoData[2] = y;
            ushort tileId = (ushort)(BitConverter.ToUInt16(PSX.levels[Level.Id].screenData, screen * 0x200 + x * 2 + y * 32) & 0x3FFF);
            BitConverter.GetBytes(tileId).CopyTo(undoData, 3);

            return new Undo() { data = undoData };
        }
        internal static Undo CreateGroupScreenEditUndo(byte screen, byte x, byte y, byte spanC, byte spanR)
        {
            Undo undo = new Undo() { type = 1 };
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            for (int r = 0; r < spanR; r++)
            {
                for (int c = 0; c < spanC; c++)
                {
                    if (x + c > 15)
                        continue;
                    if (y + r > 15)
                        continue;
                    ushort id = (ushort)(BitConverter.ToUInt16(PSX.levels[Level.Id].screenData, (x + c) * 2 + (y + r) * 32 + screen * 0x200) & 0x3FFF);
                    bw.Write(id);
                }
            }
            byte[] undoData = new byte[5];
            undoData[0] = screen;
            undoData[1] = x;
            undoData[2] = y;
            undoData[3] = spanC;
            undoData[4] = spanR;

            undo.data = undoData.Concat(ms.ToArray()).ToArray();
            return undo;
        }
        internal static Undo CreateScreenTileEditUndo(ushort tileId,byte type,byte val)
        {
            return new Undo() { type = type + 0x80, data = BitConverter.GetBytes(tileId).Concat(BitConverter.GetBytes(val)).ToArray() };
        }
        internal static Undo CreateScreenTileGroupEditUndo(byte tileColumn,byte type,byte x,byte y,byte spanC,byte spanR)
        {
            Undo undo = new Undo() { type = 0x40 + type };
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            int tileAmount = PSX.levels[Level.Id].tileInfo.Length / 4;
            tileAmount--;
            for (int r = 0; r < spanR; r++)
            {
                for (int c = 0; c < spanC; c++)
                {
                    if (x + c > 15)
                        continue;
                    if (y + r > 15)
                        continue;
                    int id = c + x + (r + y) * 16 + tileColumn * 0x100;
                    if (id > tileAmount)
                        continue;
                    bw.Write(PSX.levels[Level.Id].tileInfo[(id * 4) + type]);
                }
            }
            byte[] undoData = new byte[5];
            undoData[0] = tileColumn;
            undoData[1] = x;
            undoData[2] = y;
            undoData[3] = spanC;
            undoData[4] = spanR;

            undo.data = undoData.Concat(ms.ToArray()).ToArray();
            return undo;
        }
        internal void ApplyScreenUndo()
        {
            byte screen = this.data[0];
            byte x = this.data[1];
            byte y = this.data[2];
            bool force = false;
            if(this.type == 0)
            {
                ushort undoId = BitConverter.ToUInt16(this.data, 3);
                ushort tileId = (ushort)(BitConverter.ToUInt16(PSX.levels[Level.Id].screenData, screen * 0x200 + x * 2 + y * 32) & 0xC000);

                undoId |= tileId;

                BitConverter.GetBytes(undoId).CopyTo(PSX.levels[Level.Id].screenData, screen * 0x200 + x * 2 + y * 32);
            }
            else if(this.type == 1)
            {
                int read = 5;
                for (int r = 0; r < this.data[4]; r++)
                {
                    for (int c = 0; c < this.data[3]; c++)
                    {
                        if (x + c > 15)
                            continue;
                        if (y + r > 15)
                            continue;

                        ushort tileId = (ushort)(BitConverter.ToUInt16(PSX.levels[Level.Id].screenData, (x + c) * 2 + (y + r) * 32 + screen * 0x200) & 0xC000);
                        ushort undoId = (ushort)(BitConverter.ToUInt16(this.data, read) & 0x3FFF);

                        undoId |= tileId;

                        BitConverter.GetBytes(undoId).CopyTo(PSX.levels[Level.Id].screenData, (x + c) * 2 + (y + r) * 32 + screen * 0x200);
                        read += 2;
                    }
                }
            }
            else
            {
                int prop = this.type & 0x3;
                if ((this.type & 0x80) == 0x80)
                {
                    ushort tileId = BitConverter.ToUInt16(this.data, 0);
                    byte spec = this.data[2];

                    PSX.levels[Level.Id].tileInfo[(tileId * 4) + prop] = spec;
                }
                else //Group Edit
                {
                    int tileAmount = PSX.levels[Level.Id].tileInfo.Length / 4;
                    tileAmount--;

                    int read = 5;
                    for (int r = 0; r < this.data[4]; r++)
                    {
                        for (int c = 0; c < this.data[3]; c++)
                        {
                            if (x + c > 15)
                                continue;
                            if (y + r > 15)
                                continue;
                            int id = c + x + (r + y) * 16 + screen * 0x100;
                            if (id > tileAmount)
                                continue;

                            PSX.levels[Level.Id].tileInfo[(id * 4) + prop] = this.data[read];
                            read++;
                        }
                    }
                }
                force = true;
                MainWindow.window.screenE.DrawTiles();
                MainWindow.window.screenE.DrawTile();
                MainWindow.window.x16E.DrawTiles();
                MainWindow.window.x16E.DrawTile();
            }

            PSX.levels[Level.Id].edit = true;

            MainWindow.window.layoutE.DrawLayout();
            MainWindow.window.enemyE.Draw();
            MainWindow.window.screenE.DrawScreen();
            if (ListWindow.extraOpen)
                MainWindow.extraWindow.DrawExtra();

            if (MainWindow.window.screenE.screenId == MainWindow.window.layoutE.selectedScreen || force)
                MainWindow.window.layoutE.DrawScreen();
        }
        /*Tile x16 Undo*/
        internal static Undo CreateTileTextureGroupEditUndo(byte tileColumn, byte x, byte y, byte spanC, byte spanR)
        {
            Undo undo = new Undo() { type = 0x20 };
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            int tileAmount = PSX.levels[Level.Id].tileInfo.Length / 4;
            tileAmount--;
            for (int r = 0; r < spanR; r++)
            {
                for (int c = 0; c < spanC; c++)
                {
                    if (x + c > 15)
                        continue;
                    if (y + r > 15)
                        continue;
                    int id = c + x + (r + y) * 16 + tileColumn * 0x100;
                    if (id > tileAmount)
                        continue;
                    bw.Write(PSX.levels[Level.Id].tileInfo[(id * 4) + 2]);
                    bw.Write(PSX.levels[Level.Id].tileInfo[(id * 4) + 3]);
                }
            }
            byte[] undoData = new byte[5];
            undoData[0] = tileColumn;
            undoData[1] = x;
            undoData[2] = y;
            undoData[3] = spanC;
            undoData[4] = spanR;

            undo.data = undoData.Concat(ms.ToArray()).ToArray();
            return undo;
        }
        internal void ApplyTilesUndo()
        {
            byte screen = this.data[0];
            byte x = this.data[1];
            byte y = this.data[2];

            int prop = this.type & 0x3;
            if ((this.type & 0x80) == 0x80)
            {
                ushort tileId = BitConverter.ToUInt16(this.data, 0);
                byte spec = this.data[2];

                PSX.levels[Level.Id].tileInfo[(tileId * 4) + prop] = spec;
            }
            else if((this.type & 0x40) == 0x40) //Group Edit
            {
                int tileAmount = PSX.levels[Level.Id].tileInfo.Length / 4;
                tileAmount--;

                int read = 5;
                for (int r = 0; r < this.data[4]; r++)
                {
                    for (int c = 0; c < this.data[3]; c++)
                    {
                        if (x + c > 15)
                            continue;
                        if (y + r > 15)
                            continue;
                        int id = c + x + (r + y) * 16 + screen * 0x100;
                        if (id > tileAmount)
                            continue;

                        PSX.levels[Level.Id].tileInfo[(id * 4) + prop] = this.data[read];
                        read++;
                    }
                }
            }
            else //Cord + Tpage
            {
                int tileAmount = PSX.levels[Level.Id].tileInfo.Length / 4;
                tileAmount--;

                int read = 5;
                for (int r = 0; r < this.data[4]; r++)
                {
                    for (int c = 0; c < this.data[3]; c++)
                    {
                        if (x + c > 15)
                            continue;
                        if (y + r > 15)
                            continue;
                        int id = c + x + (r + y) * 16 + screen * 0x100;
                        if (id > tileAmount)
                            continue;

                        PSX.levels[Level.Id].tileInfo[(id * 4) + 2] = this.data[read];
                        PSX.levels[Level.Id].tileInfo[(id * 4) + 3] = this.data[read + 1];
                        read += 2;
                    }
                }
            }
            PSX.levels[Level.Id].edit = true;
            MainWindow.window.screenE.DrawTiles();
            MainWindow.window.screenE.DrawTile();
            MainWindow.window.x16E.DrawTiles();
            MainWindow.window.x16E.DrawTile();
            MainWindow.window.layoutE.DrawLayout();
            MainWindow.window.enemyE.Draw();
            MainWindow.window.screenE.DrawScreen();
            MainWindow.window.layoutE.DrawScreen();
        }
        #endregion Methoids
    }
}
