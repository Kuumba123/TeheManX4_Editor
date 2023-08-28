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
            displayInt.Value = MainWindow.settings.referanceWidth;
            dontUpdateCheck.IsChecked = MainWindow.settings.dontUpdate;
            saveReloadCheck.IsChecked = MainWindow.settings.saveOnReload;
            ultimateCheck.IsChecked = MainWindow.settings.ultimate;
            zeroCheck.IsChecked = MainWindow.settings.defaultZero;
            layoutCheck.IsChecked = MainWindow.settings.dontSaveLayout;
            screenCheck.IsChecked = MainWindow.settings.autoScreen;
            extraCheck.IsChecked = MainWindow.settings.autoExtra;
            filesCheck.IsChecked = MainWindow.settings.autoFiles;
            openCheck.IsChecked = MainWindow.settings.dontResetId;
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
        private void displayInt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!enable || e.NewValue == null || e.OldValue == null) return;
            MainWindow.settings.referanceWidth = (int)e.NewValue;
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
        private void layoutCheck_Change(object sender, RoutedEventArgs e)
        {
            if (!enable)
                return;
            MainWindow.settings.dontSaveLayout = (bool)layoutCheck.IsChecked;
            edited = true;
        }
        private void screenCheck_Change(object sender, RoutedEventArgs e)
        {
            if (!enable)
                return;
            MainWindow.settings.autoScreen = (bool)screenCheck.IsChecked;
            edited = true;
        }

        private void autoCheck_Change(object sender, RoutedEventArgs e)
        {
            if (!enable)
                return;
            MainWindow.settings.autoExtra = (bool)extraCheck.IsChecked;
            edited = true;
        }
        private void filesCheck_Change(object sender, RoutedEventArgs e)
        {
            if (!enable)
                return;
            MainWindow.settings.autoFiles = (bool)filesCheck.IsChecked;
            edited = true;
        }

        private void openCheck_Change(object sender, RoutedEventArgs e)
        {
            if (!enable)
                return;
            MainWindow.settings.dontResetId = (bool)openCheck.IsChecked;
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
                MainWindow.window.DefineSizing();
            }
        }
        #endregion Events
    }
}
