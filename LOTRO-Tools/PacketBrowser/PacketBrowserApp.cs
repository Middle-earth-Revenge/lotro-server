using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacketBrowser
{
    // Main class of the application
    class PacketBrowserApp
    {
        public PacketBrowserApp()
        {
            Sessions = new ObservableCollection<Session>();
        }

        // Open sessions
        public ObservableCollection<Session> Sessions
        {
            get;
            private set;
        }

        // Application exit callback
        public void OnExit()
        {
            // Store config
            if (Properties.Settings.Default.RecentSessions == null)
                Properties.Settings.Default.RecentSessions = new StringCollection();

            foreach (Session s in Sessions)
            {
                if (!Properties.Settings.Default.RecentSessions.Contains(s.SourceFolder))
                    Properties.Settings.Default.RecentSessions.Add(s.SourceFolder);
            }
            Properties.Settings.Default.Save();
        }
    }
}
