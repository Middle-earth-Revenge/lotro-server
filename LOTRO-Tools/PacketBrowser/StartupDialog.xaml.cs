using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PacketBrowser
{
    /// <summary>
    /// Interaction logic for StartupDialog.xaml
    /// </summary>
    public partial class StartupDialog : Window
    {
        public StartupDialog(ObservableCollection<string> recentSessions)
        {
            RecentSessions = recentSessions;

            InitializeComponent();
        }

        // This will contain the session selected by the user on dialog exit
        public Session SelectedSession
        {
            get;
            private set;
        }

        // The read list of recent sessions
        public ObservableCollection<string> RecentSessions
        {
            get;
            private set;
        }

        private void LoadSession_Click(object sender, RoutedEventArgs e)
        {
            // Get the source folder
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }

            LoadSessionFromFolder(dlg.SelectedPath);
        }

        private void RecentSessionList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string selectedFolder = RecentSessionList.SelectedItem as string;
            if (!string.IsNullOrEmpty(selectedFolder))
            {
                LoadSessionFromFolder(selectedFolder);
            }
        }

        private void LoadSessionFromFolder(string folder)
        {
            SelectedSession = new Session();
            SelectedSession.SourceFolder = folder;

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            string selectedFolder = RecentSessionList.SelectedItem as string;
            if (!string.IsNullOrEmpty(selectedFolder))
            {
                LoadSessionFromFolder(selectedFolder);
            }
        }
    }
}
