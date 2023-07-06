using System.Diagnostics;
using System.Windows;

namespace TeheManX4.Forms
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        #region Constructors
        public AboutWindow()
        {
            InitializeComponent();
            Title += Const.Version;
        }
        #endregion Constructors

        #region Events
        private void Twitchlink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.twitch.tv/kuumba_");
        }
        private void Youtubelink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.youtube.com/Kuumba");
        }
        private void Twitterlink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://twitter.com/ThePogChampGuy");
        }
        #endregion Events
    }
}
