using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows;

namespace TeheManX4.Forms
{
    /// <summary>
    /// Interaction logic for Settings Window.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        #region Properties
        private bool enable = false;
        private bool edited = false;
        #endregion  Properties

        #region Constructors
        public SettingsWindow()
        {
            InitializeComponent();

            //Redux Settings
            webBox.Text = MainWindow.settings.webPort;

            //NOPS Settings
            comBox.Text = MainWindow.settings.comPort;
            useNopsCheck.IsChecked = MainWindow.settings.useNops;

            //Options
            dontUpdateCheck.IsChecked = MainWindow.settings.dontUpdate;
            saveReloadCheck.IsChecked = MainWindow.settings.saveOnReload;
            ultimateCheck.IsChecked = MainWindow.settings.ultimate;
            zeroCheck.IsChecked = MainWindow.settings.defaultZero;
            enable = true;
        }
        #endregion Constructors

        #region Events
        private void webBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!enable)
                return;
            MainWindow.settings.webPort = webBox.Text;
            edited = true;
        }
        private void comBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!enable)
                return;
            MainWindow.settings.comPort = comBox.Text;
            edited = true;
        }
        private void useNopsCheck_Change(object sender, RoutedEventArgs e)
        {
            if (!enable)
                return;
            MainWindow.settings.useNops = (bool)useNopsCheck.IsChecked;
            edited = true;
        }
        private void dontUpdateCheck_Change(object sender, RoutedEventArgs e)
        {
            if (!enable)
                return;
            MainWindow.settings.dontUpdate = (bool)dontUpdateCheck.IsChecked;
            edited = true;
        }
        private void saveOnReloadCheck_Change(object sender, RoutedEventArgs e)
        {
            if (!enable)
                return;
            MainWindow.settings.saveOnReload = (bool)saveReloadCheck.IsChecked;
            edited = true;
        }
        private void ultimateCheck_Check_Change(object sender, RoutedEventArgs e)
        {
            if (!enable)
                return;
            MainWindow.settings.ultimate = (bool)ultimateCheck.IsChecked;
            edited = true;
        }
        private void zeroCheck_CheckChange(object sender, RoutedEventArgs e)
        {
            if (!enable)
                return;
            MainWindow.settings.defaultZero = (bool)zeroCheck.IsChecked;
            edited = true;
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (edited)
            {
                try
                {
                    string json = JsonConvert.SerializeObject(MainWindow.settings, Formatting.Indented);
                    File.WriteAllText("Settings.json", json);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    Application.Current.Shutdown();
                }
            }
        }
        #endregion Events
    }
}
