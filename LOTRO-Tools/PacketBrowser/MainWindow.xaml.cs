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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PacketBrowser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            if (m_FirstStart)
            {
                m_FirstStart = false;
                ShowStartupDialog();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Notify the view model
            PacketBrowserApp app = (PacketBrowserApp)DataContext;
            app.OnExit();
        }

        private void ShowStartupDialog()
        {
            PacketBrowserApp app = (PacketBrowserApp)DataContext;

            // Load recent sessions
            ObservableCollection<string> recentSessions = new ObservableCollection<string>();
            if (Properties.Settings.Default.RecentSessions != null)
            {
                foreach (string sessionFolder in Properties.Settings.Default.RecentSessions)
                {
                    recentSessions.Add(sessionFolder);
                }
            }

            // Show the startup dialog
            StartupDialog dlg = new StartupDialog(recentSessions);
            dlg.Owner = this;
            dlg.ShowDialog();

            if (dlg.SelectedSession != null)
            {
                // Check if the session is not yet there
                if (app.Sessions.Where(s => s.SourceFolder == dlg.SelectedSession.SourceFolder).FirstOrDefault() == null)
                {
                    // Initialize a new session
                    app.Sessions.Add(dlg.SelectedSession);
                }
            }
        }

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ShowStartupDialog();
        }

        private bool m_FirstStart = true;
    }
}
