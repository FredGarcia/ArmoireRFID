using System.Windows.Threading;

namespace AutoConnect
{/*
    interface IConnectedObject
    {
        /// <summary>
        /// get connection state : true if connection opened; false otherly
        /// </summary>
        /// <returns></returns>
        bool Is_connected();
        /// <summary>
        /// initiate and open the connection.
        /// </summary>
        void Connect();
        /// <summary>
        /// close the connection
        /// </summary>
        void CloseConnection();
    }

    /// <summary>
    /// Objet assurant le maintient de la connexion d'un objet implémentant IConnectedObject
    /// </summary>
    /// AutoConnect.IConnectedObject,
    class AutoConnector
    {
        DispatcherTimer clock = new DispatcherTimer();
        IConnectedObject myConnectedObject;

        //---------------------------------------------------------------------------------
        // Méthodes Publiques

        public AutoConnector(IConnectedObject toKeepConnected, System.TimeSpan reconnectionInterval)
        {
            myConnectedObject = toKeepConnected;

            clock.Interval = reconnectionInterval;
            clock.Tick += new System.EventHandler(clock_Tick);
        }
        public void Start()
        {
            clock.Start();
        }
        public void Stop()
        {
            clock.Stop();
        }


        //---------------------------------------------------------------------------------
        // Méthodes Privées

        void clock_Tick(object sender, System.EventArgs e)
        {
            DispatcherTimer clockSender = (DispatcherTimer)sender;
            clockSender.Tick -= clock_Tick;
            routine(clockSender);
        }

        void routine(DispatcherTimer clockSender)
        {
            if (myConnectedObject.Is_connected())
            {
            }
            else
            {
                try { myConnectedObject.CloseConnection(); }
                catch { }
                try { myConnectedObject.Connect(); }
                catch { }
            }

            clockSender.Tick += clock_Tick;
        }

    }*/
}
