using System.Windows;
using System.Windows.Controls;

namespace TeheManX4.Forms
{
    /// <summary>
    /// Interaction logic for EnemyEntry.xaml
    /// </summary>
    public partial class EnemyEntry : UserControl
    {
        #region Fields
        bool enable;
        #endregion

        #region Constructors
        public EnemyEntry(byte id,byte var,byte type,byte range,short x,short y)
        {
            enable = false;
            InitializeComponent();

            idInt.Value = id;
            varInt.Value = var;
            typeInt.Value = type;
            rangeInt.Value = range;
            xInt.Value = x;
            yInt.Value = y;
            enable = true;
        }
        #endregion Constructors

        #region Events
        private void objectTypeId_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!enable) return;
            PSX.edit = true;

            ListView listView = Tag as ListView;

            if(listView.SelectedItem == this)
            {
                Label name = listView.Tag as Label;
                name.Content = EnemyEditor.GetObjectName((byte)(int)idInt.Value, (byte)(int)typeInt.Value);
            }

        }
        private void setting_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!enable) return;
            PSX.edit = true;
        }
        #endregion Events
    }
}
