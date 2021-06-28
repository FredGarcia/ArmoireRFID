using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using ArmoireV3.Entities;
using Impinj.OctaneSdk;
using ArmoireV3.Dialogues;
using System.Timers;


namespace ArmoireV3.Controls {
    public class ReaderControl : ImpinjReader  {
        public int nbVerifFermeturePortes = 0;
        public DispatcherTimer verifFermeturePortes = new DispatcherTimer();
       private DispatcherTimer timerKeepAliveImpinj = new DispatcherTimer();
 //       private DispatcherTimer waitforscan = new DispatcherTimer();
        private static System.Timers.Timer aTimer = new System.Timers.Timer();
        private static Object queueLock = new Object();
        public Led leds;
        private bool ldoor = false;
        private bool rdoor = false;
        public bool reported = false;
        public static bool error = false;
        public bool existScanEventHandler = false;
        public int nbTagFound = 0;
        private static DispatcherTimer gache = new DispatcherTimer();
        public static Rail misu = new Rail();
        public static bool isRailAvailable = true;
        static int ReaderId = Properties.Settings.Default.NumArmoire;
        private  delegate void TagsReportedDelegate(List<Tag> tag);
        private int nbReaderPingFail = 0;
        private int nbReaderRestitutionPingFail = 0;
        private static bool isLedAvailable = true;
        public static bool isRFIDreaderConnected = false;
        public static bool tagsreported = false;
        public static bool railmoving = false;
        public static bool scanrunning = false;
        public bool armoirefermee = true;
#if EDMX
        Worker mainWorker;
#else
        private static WorkerV3 workerV3;
#endif

        
        public ReaderControl() {
            leds = new Led();
            workerV3 = new WorkerV3();
    //        CompteRebours compterebours
        }

        //Desactive les gaches 4 secondes en mode rechargement
        public void gacheReload()   {
            scanrunning = false;
            tagsreported = false;
            gache = new DispatcherTimer();
            gache.Tick += new EventHandler(gache_Tick);
            gache.Interval = new TimeSpan(0, 0, 4);
            try {    try {
#if NEWIMPINJ
                    SetGpo((ushort)Properties.Settings.Default.InPorteDroite, false);
                    SetGpo((ushort)Properties.Settings.Default.InPorteGauche, false);
#else
            reader.SetGpo(Properties.Settings.Default.InPorteDroite, false);
            reader.SetGpo(Properties.Settings.Default.InPorteGauche, false);
#endif
                    armoirefermee = false;
                }
                catch (OctaneSdkException oe) { Log.add("GacheReloader; " + oe.Message); }
            }
            catch (Exception e) { Log.add("GacheReloader: " + e.Message); }
            gache.Start();
        }

        //Desactive les gache 4 secondes pour l'utilisation en porteur en fonction de la position des vetements à retirer
        public void gacheUser(bool Ldoor,bool Rdoor)
        {
            tagsreported = false;
         //   MessageBox.Show("ouverture porte gauche:"+Ldoor+" droite: "+ Rdoor);
            gache = new DispatcherTimer();
            gache.Tick += new EventHandler(gache_Tick);
            gache.Interval = new TimeSpan(0, 0, 4);

            try  {

                if (Ldoor)       {
#if NEWIMPINJ
                    SetGpo((ushort)Properties.Settings.Default.InPorteGauche, false);
#else
                ReaderCabinet.SetGpo(Properties.Settings.Default.InPorteGauche, false);
#endif
                }
                System.Threading.Thread.Sleep(150);
                if (Rdoor)       {
#if NEWIMPINJ
                    SetGpo((ushort)Properties.Settings.Default.InPorteDroite, false);
#else
                ReaderCabinet.SetGpo(Properties.Settings.Default.InPorteDroite, false);
#endif
                }
                armoirefermee = false;
            }
            catch (Exception E)      {        Log.add("La ressource lecteur n'est pas forcément disponible " + E.Message);         }
            gache.Start();
        }

        private void gache_Tick(object sender, object e)   {
            try  {
#if NEWIMPINJ
                SetGpo((ushort)Properties.Settings.Default.InPorteDroite, true);
                SetGpo((ushort)Properties.Settings.Default.InPorteGauche, true);
#else
            ReaderCabinet.SetGpo(Properties.Settings.Default.InPorteDroite, true);
            ReaderCabinet.SetGpo(Properties.Settings.Default.InPorteGauche, true);
#endif
            }
            catch (Exception E)
            {
                Log.add("La ressource lecteur n'est pas forcément disponible " + E.Message);
            }
            gache.Stop();
        }
        /*
              public static bool IsNotNull(object ress, string errorMessage)
                {
                    if (ress == null)
                    {
                        if (Properties.Settings.Default.UseDBGMSG)
                            MessageBox.Show(errorMessage, "Erreur Système", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }*/
        public void initporte()
        {
#if NEWIMPINJ
            try
            {

                //Status status = ReaderCabinet.QueryStatus();
                if (!existScanEventHandler)
                {
                    //ReaderCabinet.
                    TagsReported += OnTagsReported;
                    //ReaderCabinet.TagsReported += new EventHandler<TagsReported>(OnTagsReported);
                    // Assign event handlers for GPI and antenna events.
                    GpiChanged += OnGpiEvent;
                    //ReaderCabinet.Gpi2Changed += new EventHandler<GpiEvent>(ReaderScan_Gpi2Changed);
                    //ReaderCabinet.Gpi1Changed += new EventHandler<GpiEvent>(ReaderScan_Gpi1Changed);
                   ConnectionLost += OnConnectionLost;

                    // Enable keepalives.
                    //ReaderCabinet.Keepalives.Enabled = true;
                    //ReaderCabinet.Keepalives.PeriodInMs = 3000;

                    // Enable link monitor mode.
                    // If our application fails to reply to
                    // five consecutive keepalive messages,
                    // the reader will close the network connection.
                    //ReaderCabinet.Keepalives.EnableLinkMonitorMode = true;
                    //ReaderCabinet.Keepalives.LinkDownThreshold = 5;

                    // Assign an event handler that will be called
                    // when keepalive messages are received.
                    KeepaliveReceived += OnKeepaliveReceived;

                    // Assign an event handler that will be called
                    // if the reader stops sending keepalives.
                    ConnectionLost += OnConnectionLost;
                    // Assign an event handler that will be called
                    // if the reader stops sending keepalives.
                    ReaderStopped += OnReaderStopped;

                    existScanEventHandler = true;

                    // Clear all reader settings.
                    ApplyDefaultSettings();
                    SetGpo((ushort)Properties.Settings.Default.InPorteDroite, true);

                    SetGpo((ushort)Properties.Settings.Default.InPorteGauche, true);

                    Settings settings = QueryDefaultSettings();
                    // Enable all of the antenna ports.
                    settings.Antennas.EnableAll();

                    // Configure availabilty of antennas
                    settings.Antennas.GetAntenna(1).IsEnabled = true;
                    settings.Antennas.GetAntenna(2).IsEnabled = true;
                    settings.Antennas.GetAntenna(3).IsEnabled = true;
                    settings.Antennas.GetAntenna(4).IsEnabled = true;
                    // Configure antennas power
                    settings.Antennas.GetAntenna(1).TxPowerInDbm = Properties.Settings.Default.PuissanceAntenneArmoire;
                    settings.Antennas.GetAntenna(2).TxPowerInDbm = Properties.Settings.Default.PuissanceAntenneArmoire;
                    settings.Antennas.GetAntenna(3).TxPowerInDbm = Properties.Settings.Default.PuissanceAntenneArmoire;
                    settings.Antennas.GetAntenna(4).TxPowerInDbm = Properties.Settings.Default.PuissanceAntenneArmoire;

                    //Configure the tag search
                    settings.ReaderMode = ReaderMode.DenseReaderM4;
                    //settings.SearchMode = SearchMode.SingleTarget;
					settings.SearchMode = SearchMode.TagFocus;
                    settings.Session = 1;

                    //Configure inventory time
                    settings.AutoStop.Mode = AutoStopMode.Duration;
                    settings.AutoStop.DurationInMs = (uint)(Properties.Settings.Default.ReadingTimeArmoire * 1000);

                    // Start reading tags when GPI #1 goes high.
                    settings.Gpis.GetGpi(1).IsEnabled = true;
                    settings.Gpis.GetGpi(2).IsEnabled = true;
                    settings.Gpis.GetGpi(1).DebounceInMs = 200;
                    settings.Gpis.GetGpi(2).DebounceInMs = 200;
                    /*
                    settings.Gpis[1].IsEnabled = true;
                    settings.Gpis[2].IsEnabled = true;
                    settings.Gpis[1].DebounceInMs = 50;

                    settings.Gpis[2].DebounceInMs = 50;

                    settings.Gpis[3].IsEnabled = true;
                    settings.Gpis[4].IsEnabled = true;
                    settings.Gpis[3].DebounceInMs = 50;
                    settings.Gpis[4].DebounceInMs = 50;
                    */
                    // Send a report for every tag is singulated.
                    settings.Report.Mode = ReportMode.BatchAfterStop;
                    // Include the antenna number in the tag report.
                    settings.Report.IncludeAntennaPortNumber = false;
                    // Include the time of the reading in the tag report
                    settings.Report.IncludeFirstSeenTime = false;
                    // Apply the new settings.
                    ApplySettings(settings);
                    SetGpo((ushort)Properties.Settings.Default.InPorteDroite, true);
                    SetGpo((ushort)Properties.Settings.Default.InPorteGauche, true);
                }
            }
            catch (Exception e)
            {
                Log.add("Erreur d'initialisation Impinj: " + e.Message);

#else
            try
            {
                //Status status = ReaderCabinet.QueryStatus();
                if (!existScanEventHandler)
                {
                    ReaderCabinet.TagsReported += new EventHandler<TagsReportedEventArgs>(OnTagsReported);
                    ReaderCabinet.Gpi2Changed += new EventHandler<GpiChangedEventArgs>(ReaderScan_Gpi2Changed);
                    ReaderCabinet.Gpi1Changed += new EventHandler<GpiChangedEventArgs>(ReaderScan_Gpi1Changed);

                    existScanEventHandler = true;

                    // Clear all reader settings.
                    ReaderCabinet.ClearSettings();
                    ReaderCabinet.SetGpo(Properties.Settings.Default.InPorteDroite, true);

                    ReaderCabinet.SetGpo(Properties.Settings.Default.InPorteGauche, true);

                    Settings settings = ReaderCabinet.QueryFactorySettings();

                    // Configure availabilty of antennas
                    settings.Antennas[1].IsEnabled = true;
                    settings.Antennas[2].IsEnabled = true;
                    settings.Antennas[3].IsEnabled = true;
                    settings.Antennas[4].IsEnabled = true;
                    // Configure antennas power
                    settings.Antennas[1].TxPowerInDbm = Properties.Settings.Default.PuissanceAntenneArmoire;
                    settings.Antennas[2].TxPowerInDbm = Properties.Settings.Default.PuissanceAntenneArmoire;
                    settings.Antennas[3].TxPowerInDbm = Properties.Settings.Default.PuissanceAntenneArmoire;
                    settings.Antennas[4].TxPowerInDbm = Properties.Settings.Default.PuissanceAntenneArmoire;

                    //Configure the tag search
                    settings.ReaderMode = ReaderMode.DenseReaderM8;
                    settings.SearchMode = SearchMode.SingleTargetWithSuppression;

                    //Configure inventory time
                    settings.AutoStop.Mode = AutoStopMode.Duration;
                    settings.AutoStop.DurationInMs = Properties.Settings.Default.ReadingTimeArmoire * 1000;

                    settings.Gpis[1].IsEnabled = true;
                    settings.Gpis[2].IsEnabled = true;
                    settings.Gpis[1].DebounceInMs = 200;

                    settings.Gpis[2].DebounceInMs = 200;

                    settings.Gpis[3].IsEnabled = true;
                    settings.Gpis[4].IsEnabled = true;
                    settings.Gpis[3].DebounceInMs = 200;
                    settings.Gpis[4].DebounceInMs = 200;

                    // Send a report for every tag is singulated.
                    settings.Report.Mode = ReportMode.BatchAfterStop;
                    // Include the antenna number in the tag report.
                    settings.Report.IncludeAntennaPortNumber = false;
                    // Include the time of the reading in the tag report
                    settings.Report.IncludeFirstSeenTime = false;
                    // Apply the new settings.
                    ReaderCabinet.ApplySettings(settings);
                    ReaderCabinet.SetGpo(Properties.Settings.Default.InPorteDroite, true);
                    ReaderCabinet.SetGpo(Properties.Settings.Default.InPorteGauche, true);
                    Log.add("Impinj initialisé");
                }
            }
            catch (Exception e)
            {
                Log.add("Erreur d'initialisation Impinj: " + e.Message);
#endif
            }
        }
#if NEWIMPINJ
        // This event handler is called asynchronously 
        // when tag reports are available.
        // Loop through each tag in the report 
        // and print the data.
        private void OnTagsReported(ImpinjReader sender, TagReport report)
#else
        private void OnTagsReported(object sender, TagsReportedEventArgs args)
#endif
        {
#if NEWIMPINJ
            Log.add("OnTagsReported " + report.Tags.Count.ToString());
            nbTagFound = report.Tags.Count;
#else
            Log.add("OnTagsReported " + args.TagReport.Tags.Count.ToString());
            nbTagFound = args.TagReport.Tags.Count;
#endif    
            if (!tagsreported&&!MainWindow.demarrage)
            {
                scanrunning = false;
                tagsreported = true; 
       
                DateTime debut = DateTime.Now;
                do { } while (railmoving);
                TimeSpan ms = DateTime.Now.Subtract(debut);
                Log.add("Attente Rail: " + Math.Ceiling(ms.TotalMilliseconds).ToString() + " ms");
                
                try
                {
                    Log.add("OnTagsReported try");
    //          if(report!=null)          Inventaire.updateListbox(report.Tags);
                    TagsReportedDelegate del = new TagsReportedDelegate(Inventaire.updateListbox);

#if NEWIMPINJ
                    MainWindow.dispatcher.BeginInvoke(del, report.Tags);
#else
         //           this.Dispatcher.BeginInvoke(del, args.TagReport.Tags);
#endif

                }
                catch (Exception e)
                {
                    if (Properties.Settings.Default.UseDBGMSG)
                        System.Windows.MessageBox.Show("Erreur OnTagReported: " + e.Message);
                }
            }
            else Log.add("Rien de scanné");
        }

#if NEWIMPINJ
        // This event handler gets called when a GPI event occurs.
        private void OnGpiEvent(ImpinjReader sender, GpiEvent e)  {
          
            try  {
                Status status = QueryStatus();
                if (status.Gpis.GetGpi(1).State == false)  ldoor = true; //fermé
                else  ldoor = false;
                leds.leftDoorLed(ldoor);
                if (status.Gpis.GetGpi(2).State == false) rdoor = true;
                else rdoor = false;
                leds.rightDoorLed(rdoor); //fermé
                if (Properties.Settings.Default.UseDBGMSG) MessageBox.Show("ARMOIRE EN FERMETURE1: e.state=" + e.State);
                if (e.State == false&&!tagsreported) {//evenement de fermeture
                    if (Properties.Settings.Default.UseDBGMSG) MessageBox.Show("ARMOIRE EN FERMETURE2");
                    if (rdoor && ldoor  && !scanrunning)
                    {
                        leds.shut();
                        if(Properties.Settings.Default.UseDBGMSG) MessageBox.Show("ARMOIRE EN FERMETURE3");
                        MainWindow.dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, (Action)delegate
                        {
                            
                            if (ReaderIsAvailable(Properties.Settings.Default.SpeedWayReaderConnectionString))
                           {    Portes();
                                RFIDScanRetrait();
                            }
                        });
                        tagsreported = true;
                    }
                   
                } // Fin if Porte droite fermée
            }
            catch (Exception except) {
                if (Properties.Settings.Default.UseDBGMSG)    MessageBox.Show("Erreur capteur porte " + except.Message + " " + except.InnerException);
            }

        }
#else

        //Event changement état du contacteur de la porte de gauche
        private void ReaderScan_Gpi1Changed(object sender, GpiChangedEventArgs args)
        {
            try
            {
                Status status = ReaderCabinet.QueryStatus();
                if (status.Gpis[1].State == GpioState.Low)
                {
                    ldoor = true;
                    if (status.Gpis[2].State == GpioState.Low)
                    {
                        rdoor = true;
                    }

                    // Les portes sont fermées!
                    leds.leftDoorLed(true);
                    if (!rscan)
                    {
                        if (reloaderB && rdoor && ldoor)
                        {
                            if (scanRunning == false)
                            {
                                leds.shut();

                                //waitscan();
                                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, (Action)delegate
                                {
                                    scan();
                                    RFIDScanRetrait();
                                });
                                rscan = true;
                            }
                        }
                    }
                    if (validat && rdoor && ldoor)
                    {
                        leds.shut();
                        if (scanRunning == false)
                        {
                            ReaderCabinet.SetGpo(Properties.Settings.Default.InPorteDroite, true);
                            ReaderCabinet.SetGpo(Properties.Settings.Default.InPorteGauche, true);
                            System.Threading.Thread.Sleep(150);
                            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, (Action)delegate
                            {
                                scan();
                                RFIDScanRetrait();
                            });
                        }
                    }
                }
                else
                {
                    ldoor = false;
                    leds.leftDoorLed(false);
                }
            }
            catch (Exception eee)
            {
                if (Properties.Settings.Default.UseDBGMSG)
                    MessageBox.Show("capteur porte " + eee.Message + " " + eee.InnerException);
            }
        }

        //Event changement état du contacteur de la porte de droite
        private void ReaderScan_Gpi2Changed(object sender, GpiChangedEventArgs args)
        {
            try
            {
                Status status = ReaderCabinet.QueryStatus();
                if (status.Gpis[2].State == GpioState.Low)
                {
                    //porte droite fermé
                    rdoor = true;

                    if (status.Gpis[1].State == GpioState.Low)
                    {
                        ldoor = true;
                    }

                    leds.rightDoorLed(true); //fermé
                    if (!rscan)
                    {
                        if (reloaderB && rdoor && ldoor && 1 == 1)
                        {
                            if (scanRunning == false)
                            {
                                leds.shut();
                                // waitscan();
                                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, (Action)delegate
                                {
                                    scan();
                                    RFIDScanRetrait();
                                });
                                rscan = true;
                            }
                        }
                    }
                    if (validat && rdoor && ldoor)
                    {
                        leds.shut();
                        // waitscan();
                        if (scanRunning == false)
                        {
                            leds.shut();
                            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, (Action)delegate
                            {
                                scan();
                                RFIDScanRetrait();
                            });
                        }
                    }
                }
                else
                {
                    //porte droite ouverte
                    rdoor = false;
                    leds.rightDoorLed(false);//ouvert
                }
            }
            catch (Exception eee)
            {
                if (Properties.Settings.Default.UseDBGMSG)
                    MessageBox.Show("capteur porte " + eee.Message + " " + eee.InnerException);
            }
        }
#endif
#if NEWIMPINJ




        void OnConnectionLost(ImpinjReader reader)
        {
            // This event handler is called if the reader  
            // stops sending keepalive messages.
            Log.add("Impinj Connection lost :" + reader.Name + " " + reader.Address);
        }

        private void OnKeepaliveReceived(ImpinjReader reader)
        {
            // This event handler is called when a keepalive 
            // message is received from the reader.
            Log.add("Impinj Keepalive received from " + reader.Name + " " + reader.Address);
        }
        private void OnReaderStopped(ImpinjReader Sender, ReaderStoppedEvent e)
        {
            Log.add("OnReaderStopped " + nbTagFound.ToString());
            if (!tagsreported && MainWindow.demarrage == false)
            {   tagsreported = true; 
                try
                {
                    TagsReportedDelegate del = new TagsReportedDelegate(Inventaire.updateListbox);

#if NEWIMPINJ
                    MainWindow.dispatcher.BeginInvoke(del, new List<Tag>());
#else
                    this.Dispatcher.BeginInvoke(del, args.TagReport.Tags);
#endif
                    //reported = true;

                }
                catch (Exception eots)
                {
                    if (Properties.Settings.Default.UseDBGMSG)
                        System.Windows.MessageBox.Show("Erreur OnTagStopped: " + eots.Message);
                }
            }
        }
#endif

        public void Portes()
        {
            try
            {
#if NEWIMPINJ
                SetGpo((ushort)Properties.Settings.Default.InPorteDroite, true);
                SetGpo((ushort)Properties.Settings.Default.InPorteGauche, true);
#else
                ReaderCabinet.SetGpo(Properties.Settings.Default.InPorteDroite, true);
                ReaderCabinet.SetGpo(Properties.Settings.Default.InPorteGauche, true);
#endif
            }
            catch (Exception exp)
            {
                if (Properties.Settings.Default.UseDBGMSG)
                    MessageBox.Show("err portes " + exp.Message);
            }
        }
        private static void SuiteRailEvent(object source, ElapsedEventArgs e)
        {
            try
            {
                // Start reading tags.
                Log.add("LancementRailDebut2");
                Thread rail = new Thread(moveRail);
                rail.Start();
                Log.add("LancementRailFin2");
                aTimer.Dispose();
            }
            catch (Exception ex)
            {
                if (Properties.Settings.Default.UseDBGMSG)
                    Log.add("Erreur RFIDScanRetrait: " + ex.Message);
            }
        }
      
        public void RFIDScanRetrait() {
            if (MainWindow.session&&!MainWindow.demarrage&&!scanrunning)
            {
                scanrunning = true; tagsreported = false;
                verifFermeturePortes.Stop();      //      reported = false;             //   ReaderId = Properties.Settings.Default.NumArmoire;                   
                      MainWindow.LancerCompteRebours();
                      MainWindow.compterebours.init(Properties.Settings.Default.ReadingTimeArmoire - 2);
                        try  {
                            initporte();
                            if (Properties.Settings.Default.UseRail)
                            {                           
                                try
                                {  Log.add("LancementThreadRail");
                               
                                   new Thread(moveRail).Start(); 
                                   
#if NEWIMPINJ
                                    //                                    if (!rest.ReaderRestitution.QueryStatus().IsSingulating)
#endif
                                    {
                                        nbTagFound = 0;
                                        Start();
                                    }
                                }
                                catch (Exception e)
                                {
                                    if (Properties.Settings.Default.UseDBGMSG)
                                        Log.add("Erreur RFIDScanRetrait: " + e.Message);
                                }
                              
                                
                                if (Properties.Settings.Default.ReadingTimeArmoire > 17)
                                {
        /*
                                    aTimer = new System.Timers.Timer();
                                    aTimer.Elapsed += new ElapsedEventHandler(SuiteRailEvent);
                                    aTimer.Interval = 12000;
                                    aTimer.Enabled = true;
            */                 
                                }

                            }
                            else
                            {
                                try
                                {
#if NEWIMPINJ
                                    //                                    if (!rest.ReaderRestitution.QueryStatus().IsSingulating)
#endif
                                    {
                                        nbTagFound = 0;
                                        Start();
                                    }
                                }
                                catch (Exception e)
                                {
                                    if (Properties.Settings.Default.UseDBGMSG)
                                        Log.add("Erreur RFIDScanRetrait: " + e.Message);
                                }
                            }
                            if (Properties.Settings.Default.Debug)
                            {

                            }
                        }
                        catch (OctaneSdkException oe)
                        {
                            if (Properties.Settings.Default.UseDBGMSG)
                                System.Windows.MessageBox.Show("Erreur RFIDScanRetrait: " + oe.Message);
                            Inventaire.updateListbox(new List<Tag>());
                        }
                        catch (Exception e)
                        {
                            System.Windows.MessageBox.Show("Erreur RFIDScanRetrait: " + e.Message);
                            if (Properties.Settings.Default.UseDBGMSG)
                                System.Windows.MessageBox.Show("Erreur RFIDScanRetrait: " + e.Message);
                            Inventaire.updateListbox(new List<Tag>());
                        }
      //              }
                }
            }

        private void verifFermeturePortes_Tick(object sender, object e) {
            nbVerifFermeturePortes++;
            try {
                Status status = QueryStatus();
#if NEWIMPINJ
                if (status.Gpis.GetGpi(1).State == false) {
                    if (status.Gpis.GetGpi(2).State == false)
#else
            if (status.Gpis[1].State == GpioState.Low)
            {
                if (status.Gpis[2].State == GpioState.Low)
#endif
                    {
                        if (rdoor && ldoor && !scanrunning)  // (!MainWindow.rscan && MainWindow.reloaderB && rdoor && ldoor && Inventaire.scanrunning)||(
                        {
                            leds.shut();
                            MainWindow.dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, (Action)delegate
                            {
                                Portes();
                                if (ReaderIsAvailable(Properties.Settings.Default.SpeedWayReaderConnectionString))
                                {
                                    RFIDScanRetrait();
                                }
                            });
                            MainWindow.rscan = true;
                        }
                        nbVerifFermeturePortes = 0;
                        verifFermeturePortes.Stop();
                    }
                }
            }
            catch (Exception E)
            {
                Log.add("La ressource lecteur n'est pas forcément disponible " + E.Message);
            }

            if (nbVerifFermeturePortes == Properties.Settings.Default.DureePorteOuverte)     {
                string message = "Armoire ouverte depuis plus de 30 secondes"; MessageBox.Show(message);
#if EDMX
                {
                    user u = mainWorker.data.getUserType(log.textBoxID.Text).ToList().FirstOrDefault();
                    if (u == null)
                    {
                        if (Properties.Settings.Default.UseDBGMSG)
                        {
                            MessageBox.Show("Erreur identifiant null");
                        }
                    }
                    else
                    {
                        mainWorker.data.insertAlert(13, u.Id, ReaderId, message);
                    }
                }
#else
                User u = workerV3.data.getUserType(Login.user.Password).ToList().FirstOrDefault();
                if (u == null && Properties.Settings.Default.UseDBGMSG)    MessageBox.Show("Erreur identifiant null");
#endif
                if (Properties.Settings.Default.UseDBGMSG)   MessageBox.Show(message);
                MainWindow.synerr();
            }
            else if (nbVerifFermeturePortes >= Properties.Settings.Default.DureeAbandon)
            {
                nbVerifFermeturePortes = 0;
                verifFermeturePortes.Stop();
                string message = "Armoire ouverte depuis plus de 1 minute";
#if EDMX  
                {
                    user u = mainWorker.data.getUserType(log.textBoxID.Text).ToList().FirstOrDefault();
                    if (u == null)
                    {
                        if (Properties.Settings.Default.UseDBGMSG)
                        {
                            MessageBox.Show("Erreur identifiant null");
                        }
                    }
                    else
                    {
                        mainWorker.data.insertAlert(14, u.Id, ReaderId, message);
                    }
                }
#else
                User u = workerV3.data.getUserType(Login.user.Password).ToList().FirstOrDefault();
                    if (u == null)
                    {
                        if (Properties.Settings.Default.UseDBGMSG)  MessageBox.Show("Erreur identifiant null");

                    }
                    else    workerV3.data.insertAlert(AlerteType.ALERTE_1MIN, u.Id, ReaderId, message);

#endif
                if (Properties.Settings.Default.UseDBGMSG)
                {
                    MessageBox.Show(message);
                }
                MainWindow.synerr();
            }
        }
        private static void moveRail()
        {
            int nbPass = 0;
            Log.add("Deb TacheRail");
            nbPass = Properties.Settings.Default.ReadingTimeArmoire / Properties.Settings.Default.TempsPourUnCycleRail;
            lock (queueLock)
             railmoving = true;
            for ( int i=0 ; i < nbPass ; i +=2 )
                if (!Rail.MoveRail(Properties.Settings.Default.RailIsMisumi))
                {
                    isRailAvailable = false;
                    //log.isRailAvailable = this.isRailAvailable;
                    string message = "Pas de rail";
#if EDMX 
                mainWorker.data.insertAlert(0, 1, Properties.Settings.Default.NumArmoire, message);
#else
                    workerV3.data.insertAlert(AlerteType.ALERTE_RAIL, 0, Properties.Settings.Default.NumArmoire, message);
#endif
                    if (Properties.Settings.Default.UseDBGMSG)
                    {
                        MessageBox.Show("Pas de rail");
                        error = true;
                        MainWindow.synerr();
                    }
                    
                }
            Thread.Sleep(Properties.Settings.Default.TempsPourUnCycleRail);
            Log.add("Fin TacheRail");
            railmoving = false; 
            scanrunning = false; 
        }
        public void verifstateport(object sender, System.Windows.RoutedEventArgs e)
        {
            nbVerifFermeturePortes = 0;
            verifFermeturePortes.Start();
        }
        public void initVerifFermeturePortes()
        {
            verifFermeturePortes = new DispatcherTimer();
            verifFermeturePortes.Tick += new EventHandler(verifFermeturePortes_Tick);
            verifFermeturePortes.Interval = new TimeSpan(0, 0, 10);
        }
        public void InitialiseLedPortes()
        {
            try
            {
                leds.InitCarteLed("COM" + Properties.Settings.Default.COMled_portes.ToString());
                if (leds.TestCarteLed())
                {
                    leds.InitLed();
                }
                else
                {
                    isLedAvailable = false;
                    string message = "Carte LED non présente";
#if EDMX 
                        mainWorker.data.insertAlert(2, 0, Properties.Settings.Default.NumArmoire, message);
#else
                    workerV3.data.insertAlert(AlerteType.ALERTE_LED, 0, ReaderId, message);
#endif
                    if (Properties.Settings.Default.UseDBGMSG)
                        MessageBox.Show("Carte LED non présente");
                    error = true;
                    MainWindow.synerr();
                }
            }
            catch (Exception e)
            {
                Log.add("Erreur autotest: Pas de carte LED (" + e.Message + ")");
                if (Properties.Settings.Default.UseDBGMSG)
                    MessageBox.Show("Erreur carte LED: " + e.Message);
					InitialiseLedPortes();
            }

            finally
            {
                leds.leftDoorLed(true);
                leds.rightDoorLed(true);
            }
           
        }
        public void ReaderTest()
        {
            if (Properties.Settings.Default.UseRail)
            {
                try
                {
                    misu.initRail("COM" + Properties.Settings.Default.COMrail.ToString());
                }
                catch (Exception ex)
                {
                    isRailAvailable = false;
                    //log.isRailAvailable = this.isRailAvailable;
                    Log.add("Erreur autotest: Pas de rail (" + ex.Message + ")");
                    string message = "Pas de rail";
#if EDMX  
                        mainWorker.data.insertAlert(0, 1, Properties.Settings.Default.NumArmoire, message);
#else
                    workerV3.data.insertAlert(AlerteType.ALERTE_RAIL, 0, ReaderId, message);
#endif
                    if (Properties.Settings.Default.UseDBGMSG)
                        MessageBox.Show("Pas de rail");
                    error = true;
                    MainWindow.synerr();
                }
            }
         
            try
            {
                try
                {
                    Connect(Properties.Settings.Default.SpeedWayReaderConnectionString);
                }
                catch (OctaneSdkException oe) { Log.add("Octane Exception" + oe.Message); }
                isRFIDreaderConnected = true;
            }
            catch (Exception ex)
            {
                Log.add("Erreur autotest: Pas de lecteur RFID (" + ex.Message + ")");
                isRFIDreaderConnected = false;
                //string message = "Lecteur RFID non connecté";
                string message = "Connexion au lecteur RFID Armoire perdue";

#if EDMX 
                    mainWorker.data.insertAlert(3, 0, Properties.Settings.Default.NumArmoire, message);
#else
                workerV3.data.insertAlert(AlerteType.ALERTE_RFID, 0, ReaderId, message);
#endif
                error = true;
                MainWindow.synerr();
            }
            /*
            try
            {
                //toto.initialisationRFIDReader();
            }
            catch(Exception ex)
            {
                Log.add("Erreur autotest: Pas de lecteur de badge (" + ex.Message + ")");
                string message = "Lecteur Badge non présent";
#if EDMX 
                mainWorker.data.insertAlert(4, 0, Properties.Settings.Default.NumArmoire, message);
#else 
                mainWorkerV3.data.insertAlert(AlertId.ALERTE_BADGE, 0, ReaderId, message);
#endif
                if (Properties.Settings.Default.UseDBGMSG)
                    MessageBox.Show("Lecteur Badge non présent");
                error = true;
                synerr();
            }*/

            if (!Properties.Settings.Default.Debug && error)
            {
                // Environment.Exit(1);
            }
        }
        public void TraitementSuiviAlerteLed()
        {
            if (isLedAvailable == false)
            {
                try
                {
                    leds.InitCarteLed("COM" + Properties.Settings.Default.COMled_portes.ToString());
                    if (leds.TestCarteLed())
                    {
                        isLedAvailable = true;
                        string message = "Carte LED présente";
#if EDMX 
                    mainWorker.data.insertAlert(2, 0, Properties.Settings.Default.NumArmoire, message);
#else
                        workerV3.data.insertAlert(AlerteType.ALERTE_LED, 0, ReaderId, message);
#endif
                        if (Properties.Settings.Default.UseDBGMSG)
                            MessageBox.Show("Carte LED présente");
                        error = true;
                        MainWindow.synerr();
                    }
                }
                catch (Exception e) { MessageBox.Show("Erreur Carte Led" + e.Message);  }

            }

        }

        public void TraitementCheckAlerteLed()
        {
            if (isLedAvailable == true)
            {
                try
                {
                    isLedAvailable = leds.userFlash("5", "A"); //inutilisé
                    if (isLedAvailable == false)
                    {

                        string message = "Carte LED non présente";
#if EDMX 
                    mainWorker.data.insertAlert(2, 0, Properties.Settings.Default.NumArmoire, message);
#else
                        workerV3.data.insertAlert(AlerteType.ALERTE_LED, 0, ReaderId, message);
#endif
                        if (Properties.Settings.Default.UseDBGMSG)
                            MessageBox.Show("Carte LED non présente");
                        error = true;
                        MainWindow.synerr();
                    }
                }
                catch
                {
                }
            }
        }
        #region KeepAliveImpinj
        public void initTimerKeepAliveImpinj()
        {
            timerKeepAliveImpinj = new DispatcherTimer();
            timerKeepAliveImpinj.Tick += new EventHandler(timerKeepAliveImpinj_Tick);
            timerKeepAliveImpinj.Interval = new TimeSpan(0, 0, 10);
            timerKeepAliveImpinj.Start();
        }

        private void timerKeepAliveImpinj_Tick(object sender, EventArgs e)
        {
            MainWindow.testLog();
            // Partie consacrée au controle de la disponibilité du lecteur RFID
            if (ReaderIsAvailable(Properties.Settings.Default.SpeedWayReaderConnectionString))
            {
                if (!isRFIDreaderConnected)
                {
                    try
                    {
                        Log.add("trykeepAliveReconnect");
                        Connect(Properties.Settings.Default.SpeedWayReaderConnectionString);
                        Log.add("trykeepAliveReconnectEnd");
                        initporte();
                        RFIDreaderConnectionUp();
                    }
                    catch
                    {
                        RFIDreaderConnectionLost();
                    }
                }
            }
            else
            {
                RFIDreaderConnectionLost();
            }

            if (Properties.Settings.Default.RestituionReader != null && Properties.Settings.Default.RestituionReader != "")
            {
                if (ReaderIsAvailable(Properties.Settings.Default.RestituionReader))
                {
                    if (!MainWindow.isRFIDreaderRestitutionConnected)
                    {
                        try
                        {
                            Log.add("trykeepAliveRestReconnect");
                            MainWindow.rest.Connect();
                            Log.add("trykeepAliveRestReconnectEnd");
#if NEWIMPINJ
                            //                            if (!rest.ReaderRestitution.QueryStatus().IsSingulating)
#endif
                            {
                                MainWindow.rest.ReaderRestitution.Start();

                                //     ReaderCabinet.Start();
                            }
                            RFIDreaderRestitutionConnectionUp();
                        }
                        catch
                        {
                            RFIDreaderRestitutionConnectionLost();
                        }
                    }
                }
                else
                {
                    RFIDreaderRestitutionConnectionLost();
                }
            }
        }

        public bool ReaderIsAvailable(string address)
        {
            //bool breponse = false;
            Ping pingSender = new Ping();
            PingOptions options = new PingOptions();
            options.DontFragment = true;
            PingReply reply;

            byte[] buffer = Encoding.ASCII.GetBytes("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
            try
            {
                reply = pingSender.Send(address, 120, buffer, options);
                if (reply.Status != IPStatus.Success)
                {
                    Log.add("Status Ping=" + reply.Status.ToString());
                    // Second chance
                    //reply = pingSender.Send(address, 120, buffer, options);
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (PingException ep)
            {
                Log.add("Erreur PingException=" + ep.Message);
            }
            catch (Exception e)
            {
                Log.add("Erreur Ping=" + e.Message);
            }

            return false;
        }

        private void RFIDreaderConnectionLost()
        {
            Log.add("DisconnectReader");
            try { Disconnect(); }
            catch { }
            Log.add("DisconnectReaderEnd");

            nbReaderPingFail++;
            if (nbReaderPingFail == 3)
            {
                string message = "Connexion au lecteur RFID Armoire perdue";
#if EDMX
                    mainWorker.data.insertAlert(3, 0, Properties.Settings.Default.NumArmoire, message);
#else
                workerV3.data.insertAlert(AlerteType.ALERTE_RFID, 0, ReaderId, message);
#endif
                MainWindow.synerr();
            }

            existScanEventHandler = false;
            isRFIDreaderConnected = false;
            leds.shut();
            Log.add("LOGIN RFIDreaderConnectionLost "); MainWindow.login();
        }

        private void RFIDreaderRestitutionConnectionLost()
        {
            Log.add("DisconnectReaderRest");
            try { MainWindow.rest.ReaderRestitution.Disconnect(); }
            catch { }
            Log.add("DisconnectReaderRestEnd");


            nbReaderRestitutionPingFail++;
            if (nbReaderRestitutionPingFail == 3)
            {
                string message = "Connexion au lecteur RFID Restitution perdue";
#if EDMX 
                    mainWorker.data.insertAlert(3, 0, Properties.Settings.Default.NumArmoire, message);
#else
                workerV3.data.insertAlert(AlerteType.ALERTE_RFID, 0, ReaderId, message);
#endif
                MainWindow.synerr();
            }

            if (MainWindow.log.isRFIDreaderRestitutionConnected) MainWindow.log.isRFIDreaderRestitutionConnected = false;
            MainWindow.isRFIDreaderRestitutionConnected = false;
        }

        private void RFIDreaderConnectionUp()
        {
            if (nbReaderPingFail > 2)
            {
                string message = "Connexion au lecteur RFID Armoire retrouvée";
#if EDMX 
                    mainWorker.data.insertAlert(3, 0, Properties.Settings.Default.NumArmoire, message);
#else
                workerV3.data.insertAlert(AlerteType.ALERTE_RFID, 0, ReaderId, message);
#endif
                MainWindow.synerr();
            }

            if (!MainWindow.log.isRFIDreaderConnected) MainWindow.log.isRFIDreaderConnected = true;
            nbReaderPingFail = 0;
            isRFIDreaderConnected = true;
        }

        private void RFIDreaderRestitutionConnectionUp()
        {
            if (nbReaderRestitutionPingFail > 2)
            {
                string message = "Connexion au lecteur RFID Restitution retrouvée";
#if EDMX 
                    mainWorker.data.insertAlert(3, 0, Properties.Settings.Default.NumArmoire, message);
#else
                workerV3.data.insertAlert(AlerteType.ALERTE_RFID, 0, Properties.Settings.Default.NumArmoire, message);
#endif

                MainWindow.synerr();
            }

            if (!MainWindow.log.isRFIDreaderRestitutionConnected) MainWindow.log.isRFIDreaderRestitutionConnected = true;
            nbReaderRestitutionPingFail = 0;
            MainWindow.isRFIDreaderRestitutionConnected = true;
        }
        #endregion
    }
}
