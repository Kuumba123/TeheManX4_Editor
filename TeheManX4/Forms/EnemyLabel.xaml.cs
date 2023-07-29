using System.Windows.Controls;
using System.Windows.Media;

namespace TeheManX4.Forms
{
    /// <summary>
    /// Interaction logic for EnemyLabel.xaml
    /// </summary>
    public partial class EnemyLabel : UserControl
    {
        #region Constructors
        public EnemyLabel()
        {
            InitializeComponent();
        }
        #endregion Constructors

        #region Methods
        public void AssignTypeBorder(byte type)
        {
            switch (type)
            {
                case 0: //Main Objects
                    {
                        this.border.BorderBrush = Brushes.Red;
                        break;
                    }
                case 1: //Weapon Objects
                    {
                        this.border.BorderBrush = Brushes.Green;
                        break;
                    }
                case 2: //Visual Object
                    {
                        this.border.BorderBrush = Brushes.LightBlue;
                        break;
                    }
                case 3: //Effect Objects
                    {
                        this.border.BorderBrush = Brushes.HotPink;
                        break;
                    }
                case 4: //Item Objects
                    {
                        this.border.BorderBrush = Brushes.Blue;
                        break;
                    }
                case 5: //Misc Objects
                    {
                        this.border.BorderBrush = Brushes.Purple;
                        break;
                    }
                case 6: //Quad Object
                    {
                        this.border.BorderBrush = Brushes.Yellow;
                        break;
                    }
                case 7: //Foreground Object
                    {
                        this.border.BorderBrush = Brushes.Silver;
                        break;
                    }
                default:
                    this.border.BorderBrush = Brushes.Black;
                    break;
            }
        }
        #endregion Methods
    }
}
