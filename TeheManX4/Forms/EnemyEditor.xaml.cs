using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TeheManX4.Forms
{
    /// <summary>
    /// Interaction logic for EnemyEditor.xaml
    /// </summary>
    public partial class EnemyEditor : UserControl
    {
        #region Fields
        static List<Rectangle> triggerRects = new List<Rectangle>();
        static List<EnemyLabel> enemyLabels = new List<EnemyLabel>();
        #endregion Constants

        #region Properties
        internal WriteableBitmap bmp = new WriteableBitmap(768, 512, 96, 96, PixelFormats.Rgb24, null);
        public int viewerX = 0x400;
        public int viewerY = 0;
        UIElement obj;
        public FrameworkElement control = new FrameworkElement();
        bool down = false;
        Point point;
        #endregion Properties

        #region Constructors
        public EnemyEditor()
        {
            InitializeComponent();
        }
        #endregion Constructors

        #region Methods
        public void Draw()
        {
            bmp.Lock();
            for (int y = 0; y < 2; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    int offsetX = (MainWindow.window.enemyE.viewerX >> 8) + x;
                    int offsetY = (MainWindow.window.enemyE.viewerY >> 8) + y;
                    if (offsetX > PSX.levels[Level.Id].width - 1 || offsetY > PSX.levels[Level.Id].height - 1)
                        Level.DrawScreen(0, x * 256, y * 256, bmp.BackBufferStride, bmp.BackBuffer);
                    else
                        Level.DrawScreen(PSX.levels[Level.Id].layout[offsetX + offsetY * PSX.levels[Level.Id].width + PSX.levels[Level.Id].size * Level.BG], x * 256, y * 256, bmp.BackBufferStride, bmp.BackBuffer);
                }
            }
            bmp.AddDirtyRect(new Int32Rect(0, 0, bmp.PixelWidth, 512));
            bmp.Unlock();
            MainWindow.window.enemyE.layoutImage.Source = bmp;
        }
        public void ReDraw()
        {
            Draw();
            DrawEnemies();
        }
        public void DrawEnemies(bool updateTrigger = true)
        {
            DisableSelect();

            foreach (var r in enemyLabels)
                r.Visibility = Visibility.Collapsed;

            int index = PSX.levels[Level.Id].GetIndex();
            if (index > 25) goto TriggerCheck;

            while (enemyLabels.Count < PSX.levels[Level.Id].enemies.Count) //Add to Canvas
            {
                var r = new EnemyLabel();
                r.PreviewMouseDown += Label_PreviewMouseDown;
                enemyLabels.Add(r);
                MainWindow.window.enemyE.canvas.Children.Add(r);
            }

            for (int i = 0; i < PSX.levels[Level.Id].enemies.Count; i++) //Update Each Enemy
            {
                enemyLabels[i].Tag = PSX.levels[Level.Id].enemies[i];
                enemyLabels[i].text.Content = PSX.levels[Level.Id].enemies[i].id.ToString("X");
                enemyLabels[i].AssignTypeBorder(PSX.levels[Level.Id].enemies[i].type);
                Canvas.SetLeft(enemyLabels[i], PSX.levels[Level.Id].enemies[i].x - viewerX - Const.EnemyOffset);
                Canvas.SetTop(enemyLabels[i], PSX.levels[Level.Id].enemies[i].y - viewerY - Const.EnemyOffset);
                enemyLabels[i].Visibility = Visibility.Visible;
            }

        TriggerCheck:
            if (updateTrigger)
                UpdateTriggers();
        }
        public void UpdateTriggers()
        {
            foreach (var r in triggerRects)
                r.Visibility = Visibility.Collapsed;
            int index = PSX.levels[Level.Id].GetIndex();
            if (index > 25) return;

            if (BitConverter.ToUInt32(PSX.exe, PSX.CpuToOffset((uint)(Const.CameraTriggerPointersAddress + index * 4))) != 0)
            {
                while (triggerRects.Count < (MainWindow.window.camE.triggerInt.Maximum + 1)) //Add to Canvas
                {
                    Rectangle r = new Rectangle()
                    {
                        IsHitTestVisible = false,
                        StrokeThickness = 2,
                        Stroke = Brushes.Green,
                        Fill = new SolidColorBrush(Color.FromArgb(96, 0xAD, 0xD8, 0xE6))
                    };
                    triggerRects.Add(r);
                    MainWindow.window.enemyE.canvas.Children.Add(r);
                }

                for (int i = 0; i < (MainWindow.window.camE.triggerInt.Maximum + 1); i++) //Update Trigger Sizes
                {
                    uint address = BitConverter.ToUInt32(PSX.exe, PSX.CpuToOffset((uint)(Const.CameraTriggerPointersAddress + index * 4)));

                    int dataOffset = PSX.CpuToOffset(BitConverter.ToUInt32(PSX.exe, PSX.CpuToOffset((uint)(address + (i * 4)))));

                    int rightSide = BitConverter.ToUInt16(PSX.exe, dataOffset);
                    int leftSide = BitConverter.ToUInt16(PSX.exe, dataOffset + 2);
                    int bottomSide = BitConverter.ToUInt16(PSX.exe, dataOffset + 4);
                    int topSide = BitConverter.ToUInt16(PSX.exe, dataOffset + 6);

                    int width = rightSide - leftSide;
                    int height = bottomSide - topSide;

                    triggerRects[i].Width = width;
                    triggerRects[i].Height = height;

                    Canvas.SetLeft(triggerRects[i], leftSide - viewerX);
                    Canvas.SetTop(triggerRects[i], topSide - viewerY);

                    triggerRects[i].Visibility = Visibility.Visible;
                }
            }
        }
        public void DisableSelect()
        {
            MainWindow.window.enemyE.control.Tag = null;
            //Disable
            MainWindow.window.enemyE.idInt.IsEnabled = false;
            MainWindow.window.enemyE.varInt.IsEnabled = false;
            MainWindow.window.enemyE.typeInt.IsEnabled = false;
            MainWindow.window.enemyE.xInt.IsEnabled = false;
            MainWindow.window.enemyE.yInt.IsEnabled = false;
            MainWindow.window.enemyE.rangeInt.IsEnabled = false;
            MainWindow.window.enemyE.nameLbl.Content = "";
        }
        private void UpdateEnemyCordLabel(int x, int y)
        {
            MainWindow.window.enemyE.xInt.Value = x + viewerX + Const.EnemyOffset;
            MainWindow.window.enemyE.yInt.Value = y + viewerY + Const.EnemyOffset;
        }
        public static string GetObjectName(byte id, byte type)
        {
            if (type == 0)
            {
                if (Const.mainObjInfo.ContainsKey(id))
                    return Const.mainObjInfo[id].name;
            }
            else if (type == 4)
            {
                if (Const.itemObjInfo.ContainsKey(id))
                    return Const.itemObjInfo[id].name;
            }
            else if (type == 3)
            {
                if (Const.effectObjInfo.ContainsKey(id))
                    return Const.effectObjInfo[id].name;
            }
            return "";
        }
        public static string GetObjectInfo(byte id, byte type)
        {
            if (type == 0)
            {
                if (Const.mainObjInfo.ContainsKey(id))
                    return Const.mainObjInfo[id].info;
            }
            else if (type == 3)
            {
                if (Const.effectObjInfo.ContainsKey(id))
                    return Const.effectObjInfo[id].info;
            }
            else if (type == 4)
            {
                if (Const.itemObjInfo.ContainsKey(id))
                    return Const.itemObjInfo[id].info;
            }
            return "";
        }
        private void UpdateEnemyName(byte type ,byte id)
        {
            MainWindow.window.enemyE.nameLbl.Content = GetObjectName(id, type);
        }
        private bool ValidEnemyAdd()
        {
            if (PSX.levels[Level.Id].GetIndex() > 25)
            {
                MessageBox.Show("There are not suppoused to be enemies in this level");
                return false;
            }
            if (Level.enemyExpand)
            {
                if (PSX.levels[Level.Id].enemies.Count == 255)
                {
                    MessageBox.Show("Max amount of regular enemies is 255");
                    return false;
                }
            }
            //Add Enemy
            if (Level.enemyExpand)
                PSX.levels[Level.Id].edit = true;
            else
                PSX.edit = true;
            return true;
        }
        #endregion Methods

        #region Events
        private void Label_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            MainWindow.window.enemyE.control.Tag = sender;
            byte type = ((Enemy)((EnemyLabel)control.Tag).Tag).type;
            byte id = ((Enemy)((EnemyLabel)control.Tag).Tag).id;
            byte var = ((Enemy)((EnemyLabel)control.Tag).Tag).var;
            byte range = ((Enemy)((EnemyLabel)control.Tag).Tag).range;
            if (e.ChangedButton == MouseButton.Left)
            {
                if (Level.enemyExpand)
                    PSX.levels[Level.Id].edit = true;
                else
                    PSX.edit = true;
                if (!down)
                {

                    //Set Select Enemy
                    MainWindow.window.enemyE.rangeInt.Value = range;
                    MainWindow.window.enemyE.idInt.Value = id;
                    MainWindow.window.enemyE.varInt.Value = var;
                    MainWindow.window.enemyE.typeInt.Value = type;
                    //Enable
                    MainWindow.window.enemyE.idInt.IsEnabled = true;
                    MainWindow.window.enemyE.varInt.IsEnabled = true;
                    MainWindow.window.enemyE.typeInt.IsEnabled = true;
                    MainWindow.window.enemyE.xInt.IsEnabled = true;
                    MainWindow.window.enemyE.yInt.IsEnabled = true;
                    MainWindow.window.enemyE.rangeInt.IsEnabled = true;

                    UpdateEnemyName(type, id);
                }
                down = true;
                obj = sender as UIElement;
                point = e.GetPosition(MainWindow.window.enemyE.canvas);
                point.X -= Canvas.GetLeft(obj);
                point.Y -= Canvas.GetTop(obj);
                MainWindow.window.enemyE.canvas.CaptureMouse();
            }
            else
            {
                string info = GetObjectInfo(id, type);
                if (info != "")
                    MessageBox.Show(info);
            }
        }
        private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((bool)MainWindow.window.camE.triggerCheck.IsChecked)
            {
                point = e.GetPosition(MainWindow.window.enemyE.canvas);
                down = true;
                MainWindow.window.enemyE.canvas.CaptureMouse();
            }
        }
        private void canvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!down) return;

            if ((bool)MainWindow.window.camE.triggerCheck.IsChecked) //Select Trigger Size
            {
                Point mousePos = e.GetPosition(canvas);
                if (point.X < mousePos.X)
                {
                    Canvas.SetLeft(selectRect, point.X);
                    selectRect.Width = mousePos.X - point.X;
                }
                else
                {
                    Canvas.SetLeft(selectRect, mousePos.X);
                    selectRect.Width = point.X - mousePos.X;
                }

                if (point.Y < mousePos.Y)
                {
                    Canvas.SetTop(selectRect, point.Y);
                    selectRect.Height = mousePos.Y - point.Y;
                }
                else
                {
                    Canvas.SetTop(selectRect, mousePos.Y);
                    selectRect.Height = point.Y - mousePos.Y;
                }
                selectRect.Visibility = Visibility.Visible;
                return;
            }

            //Move Enemy
            if (obj == null) return;
            var pos = e.GetPosition(sender as IInputElement);
            double x = pos.X - point.X;
            double y = pos.Y - point.Y;

            //Border Checks
            if (x < 0 - Const.EnemyOffset)
                x = 0 - Const.EnemyOffset;
            if (y < 0 - Const.EnemyOffset)
                y = 0 - Const.EnemyOffset;


            Canvas.SetLeft(obj, x);
            Canvas.SetTop(obj, y);
            UpdateEnemyCordLabel((int)x, (int)y);
            var en = (Enemy)((EnemyLabel)obj).Tag;
            en.x = (short)((short)(viewerX + x) + Const.EnemyOffset);
            en.y = (short)((short)(viewerY + y) + Const.EnemyOffset);
            if (Level.enemyExpand)
                PSX.levels[Level.Id].edit = true;
            else
                PSX.edit = true;
        }
        private void canvas_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
                return;
            down = false;
            if ((bool)MainWindow.window.camE.triggerCheck.IsChecked)
            {
                selectRect.Visibility = Visibility.Collapsed;
                int index = PSX.levels[Level.Id].GetIndex();
                if (index > 25) goto End;

                uint address = BitConverter.ToUInt32(PSX.exe, PSX.CpuToOffset((uint)(Const.CameraTriggerPointersAddress + index * 4)));
                if (address == 0) goto End;

                int leftSide = ((int)(Canvas.GetLeft(selectRect) + viewerX));
                int rightSide = ((int)(Canvas.GetLeft(selectRect) + selectRect.Width + viewerX));
                int topSide = (int)(Canvas.GetTop(selectRect) + viewerY);
                int bottomSide = (int)(Canvas.GetTop(selectRect) + selectRect.Height + viewerY);

                if(leftSide < 0)
                    leftSide = 0;
                if (rightSide > 0xFFFF)
                    rightSide = 0xFFFF;
                if (topSide < 0)
                    topSide = 0;
                if (bottomSide > 0xFFFF)
                    bottomSide = 0xFFFF;

                int dataOffset = PSX.CpuToOffset(BitConverter.ToUInt32(PSX.exe, PSX.CpuToOffset((uint)(address + ((int)MainWindow.window.camE.triggerInt.Value * 4)))));

                BitConverter.GetBytes((ushort)rightSide).CopyTo(PSX.exe, dataOffset);
                MainWindow.window.camE.rightInt.Value = rightSide;
                
                BitConverter.GetBytes((ushort)leftSide).CopyTo(PSX.exe, dataOffset + 2);
                MainWindow.window.camE.leftInt.Value = leftSide;

                BitConverter.GetBytes((ushort)bottomSide).CopyTo(PSX.exe, dataOffset + 4);
                MainWindow.window.camE.bottomInt.Value = bottomSide;

                BitConverter.GetBytes((ushort)topSide).CopyTo(PSX.exe, dataOffset + 6);
                MainWindow.window.camE.topInt.Value = topSide;

                PSX.edit = true;
                UpdateTriggers();
            }
            else
                obj = null;
        End:
            canvas.ReleaseMouseCapture();
        }
        private void idInt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (control.Tag == null || e.NewValue == null || e.OldValue == null)
                return;
            ((Enemy)((EnemyLabel)control.Tag).Tag).id = (byte)(int)e.NewValue;
            ((EnemyLabel)control.Tag).text.Content = Convert.ToString(((Enemy)((EnemyLabel)control.Tag).Tag).id, 16).ToUpper();
            UpdateEnemyName(((Enemy)((EnemyLabel)control.Tag).Tag).type, ((Enemy)((EnemyLabel)control.Tag).Tag).id);
        }
        private void xInt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (control.Tag == null || e.NewValue == null || e.OldValue == null)
                return;
            ((Enemy)((EnemyLabel)control.Tag).Tag).x = (short)(int)e.NewValue;
            Canvas.SetLeft((UIElement)control.Tag, ((int)e.NewValue) - viewerX - Const.EnemyOffset);
        }
        private void yInt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (control.Tag == null || e.NewValue == null || e.OldValue == null)
                return;
            ((Enemy)((EnemyLabel)control.Tag).Tag).y = (short)(int)e.NewValue;
            Canvas.SetTop((UIElement)control.Tag, ((int)e.NewValue) - viewerY - Const.EnemyOffset);
        }
        private void varInt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (control.Tag == null || e.NewValue == null || e.OldValue == null)
                return;
            ((Enemy)((EnemyLabel)control.Tag).Tag).var = (byte)((int)e.NewValue & 0xFF);

        }
        private void typeInt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (control.Tag == null || e.NewValue == null || e.OldValue == null)
                return;
            ((Enemy)((EnemyLabel)control.Tag).Tag).type = (byte)((int)e.NewValue & 0xFF);
            ((EnemyLabel)control.Tag).AssignTypeBorder(((Enemy)((EnemyLabel)control.Tag).Tag).type);
            UpdateEnemyName(((Enemy)((EnemyLabel)control.Tag).Tag).type, ((Enemy)((EnemyLabel)control.Tag).Tag).id);
        }
        private void rangeInt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (control.Tag == null || e.NewValue == null || e.OldValue == null)
                return;
            ((Enemy)((EnemyLabel)control.Tag).Tag).range = (byte)((int)e.NewValue & 0xFF);
        }
        private void AddEnemy_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidEnemyAdd()) return;
            var en = new Enemy();
            en.x = (short)(viewerX + 0x100);
            en.y = (short)(viewerY + 0x100);
            en.id = 0xF; //Default is Met
            en.type = 0;
            PSX.levels[Level.Id].enemies.Add(en);
            DrawEnemies();
        }
        private void RemoveEnemy_Click(object sender, RoutedEventArgs e)
        {
            if (control.Tag == null)
                return;
            PSX.levels[Level.Id].enemies.Remove((Enemy)((EnemyLabel)control.Tag).Tag);
            DrawEnemies();
            if (Level.enemyExpand)
                PSX.levels[Level.Id].edit = true;
            else
                PSX.edit = true;
        }
        private void ToolsBtn_Click(object sender, RoutedEventArgs e)
        {
            if(PSX.levels[Level.Id].GetIndex() > 25)
            {
                MessageBox.Show("There are not suppoused to be enemies in this level");
                return;
            }
            ListWindow l = new ListWindow(3);
            l.ShowDialog();
        }
        private void Help_Click(object sender, RoutedEventArgs e)
        {
            HelpWindow h = new HelpWindow(3);
            h.ShowDialog();
        }
        private void EnemyImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragDrop.DoDragDrop(sender as Image,Convert.ToInt32(((Image)sender).Uid,16), DragDropEffects.Move);
        }
        private void EnemyImage_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            int value = Convert.ToInt32(((UIElement)sender).Uid,16);
            byte id = (byte)(value & 0xFF);
            byte type = (byte)((value >> 8) & 0xFF);
            string info = GetObjectInfo(id, type);
            if (info != "")
                MessageBox.Show(info);
        }
        private void canvas_Drop(object sender, DragEventArgs e)
        {
            if (!ValidEnemyAdd()) return;

            var pos = e.GetPosition(MainWindow.window.enemyE.layoutImage);
            double x = pos.X + viewerX;
            double y = pos.Y + viewerY;

            //Border Checks
            if (x < 0)
                x = 0;
            if (y < 0)
                y = 0;


            int value = (int)e.Data.GetData(typeof(int));
            byte id = (byte)(value & 0xFF);
            byte type = (byte)((value >> 8) & 0xFF);
            byte var = (byte)((value >> 16) & 0xFF);

            var en = new Enemy();
            en.x = (short)x;
            en.y = (short)y;
            en.id = id;
            en.type = type;
            en.var = var;
            PSX.levels[Level.Id].enemies.Add(en);
            DrawEnemies();
        }
        private void MainObject_Click(object sender, RoutedEventArgs e)
        {
            //if (bar.VerticalOffset != 0)
            //    bar.ScrollToTop(); //6865
        }
        private void EffectObject_Click(object sender, RoutedEventArgs e)
        {
            //if (bar.VerticalOffset != EffectObjectsScrollOffset && EffectObjectsScrollOffset != -1)
            //    bar.ScrollToVerticalOffset(EffectObjectsScrollOffset);
        }
        private void ItemObject_Click(object sender, RoutedEventArgs e)
        {
            //if (bar.VerticalOffset != ItemObjectsScrollOffset && ItemObjectsScrollOffset != -1)
            //    bar.ScrollToVerticalOffset(ItemObjectsScrollOffset);
        }
        #endregion Events
    }
}
