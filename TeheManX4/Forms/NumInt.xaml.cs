using System.Windows.Media;

namespace TeheManX4.Forms
{
    /// <summary>
    /// Interaction logic for NumInt.xaml
    /// </summary>
    public partial class NumInt : Xceed.Wpf.Toolkit.IntegerUpDown
    {
        public NumInt()
        {
            InitializeComponent();
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            TextBox.CaretBrush = Brushes.White;
        }
    }
}
