using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TeheManX4.Forms
{
    /// <summary>
    /// Interaction logic for EnemyEditor.xaml
    /// </summary>
    public partial class EnemyEditor : UserControl
    {
        #region Properties
        WriteableBitmap bmp = new WriteableBitmap(512, 512, 96, 96, PixelFormats.Rgb24, null);
        byte[] pixels = new byte[0xC0000];
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
                for (int x = 0; x < 2; x++)
                {
                    int offsetX = (MainWindow.window.enemyE.viewerX >> 8) + x;
                    int offsetY = (MainWindow.window.enemyE.viewerY >> 8) + y;
                    if (offsetX > PSX.levels[Level.Id].width - 1 || offsetY > PSX.levels[Level.Id].height - 1)
                        Level.DrawScreen(0, x * 256, y * 256, 1536, bmp.BackBuffer);
                    else
                        Level.DrawScreen(PSX.levels[Level.Id].layout[offsetX + offsetY * PSX.levels[Level.Id].width + PSX.levels[Level.Id].size * Level.BG], x * 256, y * 256, 1536, bmp.BackBuffer);
                }
            }
            bmp.AddDirtyRect(new Int32Rect(0, 0, 512, 512));
            bmp.Unlock();
            MainWindow.window.enemyE.layoutImage.Source = bmp;
        }
        public void ReDraw()
        {
            Draw();
            DrawEnemies();
        }
        public void DrawEnemies()
        {
            DisableSelect();
            if (MainWindow.window.enemyE.canvas.Children.Count != 256)
            {
                while (true)
                {
                    if (MainWindow.window.enemyE.canvas.Children.Count == 256)
                        break;
                    EnemyLabel l = new EnemyLabel();
                    l.PreviewMouseDown += Label_PreviewMouseDown;
                    MainWindow.window.enemyE.canvas.Children.Add(l);
                }
            }
            foreach (var c in MainWindow.window.enemyE.canvas.Children)
            {
                if (c.GetType() != typeof(EnemyLabel))
                    continue;
                EnemyLabel l = (EnemyLabel)c;
                l.Visibility = Visibility.Hidden;
            }
            int offset = 0;
            //Add Each Enemy if ON SCREEN
            foreach (var e in PSX.levels[Level.Id].enemies)
            {
                if (e.x < viewerX || e.x > viewerX + 0x1FF || e.y < viewerY || e.y > viewerY + 0x1FF)
                    continue;
                if (MainWindow.window.enemyE.canvas.Children[offset].GetType() != typeof(EnemyLabel))
                    offset++;

                //New Enemy to Add to Viewer
                var l = new EnemyLabel();
                ((EnemyLabel)MainWindow.window.enemyE.canvas.Children[offset]).text.Content = Convert.ToString(e.id, 16).ToUpper();
                ((EnemyLabel)MainWindow.window.enemyE.canvas.Children[offset]).AssignTypeBorder(e.type);

                Canvas.SetLeft((EnemyLabel)MainWindow.window.enemyE.canvas.Children[offset], e.x - viewerX - Const.EnemyOffset);
                Canvas.SetTop((EnemyLabel)MainWindow.window.enemyE.canvas.Children[offset], e.y - viewerY - Const.EnemyOffset);

                ((EnemyLabel)MainWindow.window.enemyE.canvas.Children[offset]).Visibility = Visibility.Visible;
                ((EnemyLabel)MainWindow.window.enemyE.canvas.Children[offset]).Tag = e;
                offset++;
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
                    MainWindow.window.enemyE.xInt.Minimum = viewerX;
                    MainWindow.window.enemyE.xInt.Maximum = viewerX + 0x1FF;
                    MainWindow.window.enemyE.yInt.Minimum = viewerY;
                    MainWindow.window.enemyE.yInt.Maximum = viewerY + 0x1FF;
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
            if (PSX.levels[Level.Id].GetIndex() > 25)
            {
                MessageBox.Show("There are not suppoused to be enemies in this level");
                return;
            }
            if (Level.enemyExpand)
            {
                if(PSX.levels[Level.Id].enemies.Count == 255)
                {
                    MessageBox.Show("Max amount of regular enemies is 255");
                    return;
                }
            }
            //Add Enemy
            if (Level.enemyExpand)
                PSX.levels[Level.Id].edit = true;
            else
                PSX.edit = true;
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
        private void canvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (obj == null || !down)
                return;
            var pos = e.GetPosition(sender as IInputElement);
            double x = pos.X - point.X;
            double y = pos.Y - point.Y;

            //Border Checks
            if (x < 0 - Const.EnemyOffset)
                x = 0 - Const.EnemyOffset;
            if (y < 0 - Const.EnemyOffset)
                y = 0 - Const.EnemyOffset;
            if (x > 511 - Const.EnemyOffset)
                x = 511 - Const.EnemyOffset;
            if (y > 511 - Const.EnemyOffset)
                y = 511 - Const.EnemyOffset;

            Canvas.SetLeft(obj, x);
            Canvas.SetTop(obj, y);
            UpdateEnemyCordLabel((int)x, (int)y);
            var en = (Enemy)((EnemyLabel)obj).Tag;
            en.x = (short)((short)((viewerX & 0x1F00) + x) + Const.EnemyOffset);
            en.y = (short)((short)((viewerY & 0x1F00) + y) + Const.EnemyOffset);
            if (Level.enemyExpand)
                PSX.levels[Level.Id].edit = true;
            else
                PSX.edit = true;
        }

        private void canvas_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
                return;
            obj = null;
            down = false;
            canvas.ReleaseMouseCapture();
        }
        #endregion Events
    }
}
