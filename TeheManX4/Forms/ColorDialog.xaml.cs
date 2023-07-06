using System;
using System.Windows;
using System.Windows.Media;

namespace TeheManX4.Forms
{
    /// <summary>
    /// Interaction logic for ColorDialog.xaml
    /// </summary>
    public partial class ColorDialog : Window
    {
        #region Properties
        public bool confirm = false;
        private int col;
        private int row;
        #endregion Properties

        #region Constructors
        public ColorDialog(ushort color, int col, int row)
        {
            this.col = col;
            this.row = row;
            InitializeComponent();
            byte[] rgb = BitConverter.GetBytes(Level.To32Rgb(color));
            this.canvas.SelectedColor = Color.FromRgb(rgb[2], rgb[1], rgb[0]);

        }
        #endregion Constructors

        #region Events

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            confirm = true;
            this.Close();
        }

        private void canvas_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            ushort newC = (ushort)Level.To15Rgb(canvas.SelectedColor.Value.B, canvas.SelectedColor.Value.G, canvas.SelectedColor.Value.R);
            this.Title = "CLUT: " + Convert.ToString(row, 16).ToUpper() + "  Color: " + Convert.ToString(col, 16).ToUpper() + "    15BPP RGB #" + Convert.ToString(newC, 16).ToUpper().PadLeft(4, '0');
        }
        #endregion Events
    }
}
