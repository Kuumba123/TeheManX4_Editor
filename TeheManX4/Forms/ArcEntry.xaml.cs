using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;

namespace TeheManX4.Forms
{
    /// <summary>
    /// Interaction logic for PacEntry.xaml
    /// </summary>
    public partial class ArcEntry : UserControl
    {
        #region Properties
        internal Entry entry = new Entry();
        #endregion Properties

        #region Constructors
        public ArcEntry()
        {
            InitializeComponent();
        }
        public ArcEntry(Entry entry)
        {
            InitializeComponent();
            this.entry = entry;
            typeInt.Value = entry.type;
        }
        #endregion Constructors

        #region Events
        private void saveBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (entry.data == null)
            {
                System.Windows.MessageBox.Show("There is no file data to save");
                return;
            }
            using (var fd = new System.Windows.Forms.SaveFileDialog())
            {
                fd.Title = "Select File Entry Save Location";
                if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        File.WriteAllBytes(fd.FileName, entry.data);
                        System.Windows.MessageBox.Show("File Entry Data Saved!");
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show(ex.Message, "ERROR");
                    }
                }
            }
        }

        private void openBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            using (var fd = new System.Windows.Forms.OpenFileDialog())
            {
                fd.Title = "Select a File to be added to the ARC/PAC file";
                if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    entry.data = File.ReadAllBytes(fd.FileName);
                    pathBox.Text = fd.FileName;
                }
            }
        }
        private void deleteBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var parent = VisualTreeHelper.GetParent(this);
            var items = parent as Panel;
            items.Children.Remove(this);
        }
        #endregion Events

        private void typeInt_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue == null || e.OldValue == null)
                return;
            entry.type = (int)e.NewValue;
        }
    }
}
