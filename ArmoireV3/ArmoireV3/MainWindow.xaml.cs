using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using ArmoireV3.Controls;
using ArmoireV3.Dialogues;
using ArmoireV3.Entities;
using Impinj.OctaneSdk;

namespace ArmoireV3
{
    public partial class MainWindow : Window
    {
        public static  Dispatcher dispatcher;
        static   DispatcherTimer timerSynchroUser;
        DispatcherTimer timerSynchroLoad;
        public static bool reloaderB = false;
        public static bool rscan = false;


#if EDMX
        Worker workerSelect = new Worker();
//        WorkerV3 workerSelectV3 = new WorkerV3();

        List<listItem> temp;
        List<listItem> sampleData = new List<listItem>();
        List<Tag> listT = new List<Tag>();
        List<epc> userSelect = new List<epc>();
        //List<Epc> userSelectV3 = new List<Epc>();
        List<ListEpcCount> listepcC = new List<ListEpcCount>();
#else
 
        public static WorkerV3 workerSelectV3 = new WorkerV3();
        static List<listItem> temp;
        static List<listItem> sampleData = new List<listItem>();


#endif
        static Synchro syn;

        static int ReaderId = Properties.Settings.Default.NumArmoire;


        bool synchroUP = false;


        static bool isInitialising = true;

        public static bool isLocalBDDAvailable = true;
        static bool LundiTraite = false;
        static bool RecreditHebdoTraite = false;

        public static Login log = new Login();//login

        private static object objectLock = new object();
        private static object objectUser = new object();
        static Thread zsync;
        static Thread zsyncReload = null;
        static Thread zsyncUser = null;
        static Thread zsyncUserTimer = null;
        static Thread zsyncLoad;
        static Thread zsyncRestitution;
        static Thread zsyncAlerte;
        public static Grid mainGrid;
        //loginLoad, selectload, validload, scanload, reloaderload,
        static Selection_Article sel;//selection article
        public static Validation_Article val;//validation article
        static Reloader rel;//reload
        public static bool isRFIDreaderRestitutionConnected = false;
        public static Restitution rest;
        public static ReaderControl readercontrol = new ReaderControl();

        static int nbrel = 0;
        static int nbsel = 0;
        static int nbval = 0;
        public static bool session = false;
        public static bool demarrage;

#if EDMX
        Worker mainWorker;
#else
        private static WorkerV3 mainWorkerV3;
#endif
        //        Object mainWorker;

        static bool firstTimeConnectionLost = false; // passe à true la première fois que la connexion est perdue

        //SshClient yui = new SshClient("87.98.143.63",2222,"armoire","PassDBarm");

        //     public static List<Epc> epcs = new List<Epc>();

        // Get the version of the executing assembly (that is, this assembly).
        Assembly assem = Assembly.GetEntryAssembly();
       public static CompteRebours compterebours;
        public MainWindow()
        {
            Inventaire.fini = true; 
            session = false;
            demarrage = true;
            dispatcher = this.Dispatcher;
            mainGrid = new Grid(); this.AddChild(mainGrid);   

            GestionAntiDemarrageMultiple();
            Log.add("Initialisation de l'armoire");
            EnregistrementNumVersion();
            InitializeComponent();
            Log.add("1");
            LancementDesSynchro();
            Log.add("2");
            testDB();
#if REEL    
            readercontrol.ReaderTest();
            readercontrol.initporte();
            Log.add("3");
            readercontrol.InitialiseLedPortes();
            Log.add("4");
            PriseEnCompteRestitution();
            Log.add("5");
            readercontrol.initVerifFermeturePortes();
            readercontrol.initTimerKeepAliveImpinj();
#endif
            Log.add("6");
            
			// synchUserTimer
			initTimerSynchroUser();
            if (Properties.Settings.Default.UseSynchroTimer)
            {
           //      synchLoad()
                initTimerSynchroLoad();
            }

            Log.add("7");
            if (Properties.Settings.Default.Debug == true)
            {
                this.WindowState = System.Windows.WindowState.Normal;
                this.ResizeMode = System.Windows.ResizeMode.CanResize;
                this.WindowStyle = System.Windows.WindowStyle.ToolWindow;
            }
            else
            {
                zoomFullScreen();
            }
            Log.add("8");
            synerr();
            isInitialising = false;
            Log.add("Armoire initialisee et operationnelle");
            demarrage = false;
            login();  
        }

        private void PriseEnCompteRestitution()    {
            try  {
                if (Properties.Settings.Default.RestituionReader != null && Properties.Settings.Default.RestituionReader != "")   {
                    rest = new Restitution();
                    rest.Connect();
                    rest.finScanRestitution += new EventHandler(rest_finScanRestitution);
                    isRFIDreaderRestitutionConnected = true;
                }
                else   {   isRFIDreaderRestitutionConnected = true;   }
            }
            catch (Exception e) {
                isRFIDreaderRestitutionConnected = false;
                if (Properties.Settings.Default.UseDBGMSG)
                    MessageBox.Show("Fonctionnement sans restitution: " + e.Message);
            }
        }

        private void EnregistrementNumVersion() {
            AssemblyName assemName = assem.GetName();
            Version ver = assemName.Version;
            //Console.WriteLine("Application {0}, Version {1}", assemName.Name, ver.ToString());
            Log.add("Version : " + assemName.Name + " " + ver.ToString());

            // Mise à jour du numéro de version logiciel dans la base de donnée :
            //mainWorker.data.  Version_Armoire()
            new WorkerV3().data.updateVersionLogiciel(assemName.Name + " " + ver.ToString());
            new WorkerV3().data.updateVersionDate();
        }

        private void GestionAntiDemarrageMultiple() {
            Process currentProcess = Process.GetCurrentProcess();
            Process[] localAll = Process.GetProcesses();
            for (int i = 0; i < localAll.Count(); i++)
                if (currentProcess.ProcessName == localAll[i].ProcessName && currentProcess.TotalProcessorTime < localAll[i].TotalProcessorTime)
                    localAll[i].Kill();
        }

        void testDB() {
            try {
#if EDMX 
                    mainWorker  = new Worker();
#else
                mainWorkerV3 = new WorkerV3();
#endif
            } catch (Exception ex)  {
                Log.add("Erreur autotest: Pas de BDD (" + ex.Message + ")");
                if (Properties.Settings.Default.UseDBGMSG)  {
                    string message = "Erreur base de données: " + ex.Message;
                    if (ex.InnerException != null) message += "\n" + ex.InnerException.Message;
                    MessageBox.Show(message);
                }
                Environment.Exit(1);
            }
#if EDMX
#else
            if (mainWorkerV3.data.Is_connected() == false)
            {
                isLocalBDDAvailable = false;
                if (log != null)
                    log.isLocalBDDAvailable = isLocalBDDAvailable;
            }
#endif
        }
        public static void synerr() {
            if (Properties.Settings.Default.UseSynchro&&!session)   {
   //             synchError();
               try {
                    if (!zsyncAlerte.IsAlive) {
                        if (zsyncAlerte.ThreadState == System.Threading.ThreadState.Unstarted
                            || zsyncAlerte.ThreadState == System.Threading.ThreadState.Stopped
                            || zsyncAlerte.ThreadState == System.Threading.ThreadState.Aborted)
                        {
                            log.textBoxAnnonceSynchro.Visibility = Visibility.Visible;
                            log.textBoxAnnonceSynchro.Text = "synchronisation en cours";
                            zsyncAlerte = null;
                            zsyncAlerte = new Thread(new ThreadStart(synchError));
                            zsyncAlerte.Start();
                            log.textBoxAnnonceSynchro.Visibility = Visibility.Hidden;
                        }

                    }
                }
                catch (Exception e) {
                    Log.add("Erreur synchro init" + e.Message);
                    // mainWorker.data.AlertMessage = "Erreur synchro init " + e.InnerException + " " + e.Message;
                    if (Properties.Settings.Default.UseDBGMSG)
                        MessageBox.Show("erreur synchro init" + e.Message);
                }
           }
        }

        private void LancementDesSynchro() {
            if (Properties.Settings.Default.UseSynchro) {
                // Création d'un Tunnel SSH
                //try
                {
                    //yui.Connect();
                    //ForwardedPortLocal forward = new ForwardedPortLocal("127.0.0.1", 3306, "87.98.143.63", 3307);
                    //yui.AddForwardedPort(forward);
                    //forward.Start();
                }

                try {
                    syn = new Synchro();
                    // on vérifie si les connexions StringConnectSynchro et StringConnectAlert
                    // peuvent être établies et sont propres à recevoir des commandes                 
                    synchroUP = Synchro.testConnexion();
                    if (!synchroUP) Log.add("LancementDesSynchro non réalisable: pas de connexion" );
                }
                catch (Exception ex) {
                    Log.add("MainWindow - Erreur: " + ex.Message);
                    if (Properties.Settings.Default.UseDBGMSG) {
                        string msg = "Fonctionnement sans synchro: " + ex.Message;
                        if (ex.InnerException != null) msg += "\n" + ex.InnerException.Message;
                        MessageBox.Show(msg);
                    }
                }
                try {
                    if (synchroUP) {
                        try {
                            log.textBoxAnnonceSynchro.Visibility = Visibility.Visible;
                            log.textBoxAnnonceSynchro.Text = "synchronisation en cours";
                            zsync = new Thread(synch);
                            zsyncUserTimer = new Thread(synchUserTimer);
                            zsyncLoad = new Thread(synchLoad);
                            zsyncRestitution = new Thread(synchRestitution);
                            zsyncAlerte = new Thread(synchError);
                            log.textBoxAnnonceSynchro.Visibility = Visibility.Hidden;
                        }
                        catch (Exception e)
                        {
                            Log.add("Erreur initialisation Synchro: " + e.Message);
                            zsync.Abort();
                            zsyncUserTimer.Abort();
                            zsyncLoad.Abort();
                            zsyncRestitution.Abort();
                            zsyncAlerte.Abort();
                            synchroUP = false;
                            if (Properties.Settings.Default.UseDBGMSG)
                                MessageBox.Show("Erreur initialisation Synchro: " + e.Message);
                        }
                    }

                }
                catch (Exception e)
                {
                    Log.add("Main Window - Erreur: " + e.Message);
                    if (Properties.Settings.Default.UseDBGMSG)
                        System.Windows.MessageBox.Show("Main Window - Erreur: " + e.Message);
                }

                if (synchroUP) {
                    try {
                        if (!zsync.IsAlive) {
                            if (zsync.ThreadState == System.Threading.ThreadState.Unstarted
                                || zsync.ThreadState == System.Threading.ThreadState.Stopped
                                || zsync.ThreadState == System.Threading.ThreadState.Aborted)
                            {
                                log.textBoxAnnonceSynchro.Visibility = Visibility.Visible;
                                log.textBoxAnnonceSynchro.Text = "synchronisation en cours";
                                zsync = null;
                                zsync = new Thread(new ThreadStart(synch));
                                zsync.Start();
                                log.textBoxAnnonceSynchro.Visibility = Visibility.Hidden;

                            }
                        }
                    }
                    catch (Exception e) {
                        Log.add("erreur synchro init: " + e.Message);
                        // mainWorker.data.AlertMessage = "Erreur synchro init " + e.InnerException + " " + e.Message;
                        if (Properties.Settings.Default.UseDBGMSG)  MessageBox.Show("erreur synchro init: " + e.Message);
                    }
                }
            }
        }

        static void synchUserTimer()
        {
            if (Properties.Settings.Default.UseSynchro&&!session&&Inventaire.fini) {
         //       if ((rel == null || (rel != null && nbrel == 0)) && (sel == null || (sel != null && nbsel == 0)) && (val == null || (val != null && nbval == 0))&&!ReaderControl.scanrunning)
                {
                    try
                    {
                        TestConnexionEnCours();

                        lock (objectLock)
                        {
                            Log.add("Début SynchUserTimer");
#if EDMX 
                            mainWorker.RefreshContext();
#else
                            mainWorkerV3.RefreshContext();
#endif
                            DateTime debut = DateTime.Now;
                            syn.user();
                            syn.userart();
                            syn.epcPerdus();
                            syn.logEPC();
                            syn.Version_Armoire();
                            TimeSpan ms = DateTime.Now.Subtract(debut);
                            syn.reportCenter("Success", "SynchUserTimer (" + Math.Ceiling(ms.TotalMilliseconds).ToString() + " ms)", 6);
                            Log.add("SynchUserTimer_1");
                        }
                    }
                    catch (Exception e)
                    {
                        Log.add("Erreur synchro userTimer: " + e.Message);
                        if (Properties.Settings.Default.UseDBGMSG)
                        {
                            string s = "";
                            if (e.InnerException != null) s = "\n" + e.InnerException.Message;
                            MessageBox.Show("Erreur synchro userTimer: " + e.Message + s);
                        }
                    }
                    finally
                    {
                        Log.add("SynchUserTimer2");
                        try
                        {
                            if (Properties.Settings.Default.ReglesDeChange == true)
                                try { TraitementRegleChangeMin(); }
                                catch (Exception e) { Log.add("Erreur TraitementRegleChangeMin : " + e.Message);}

                            if (Properties.Settings.Default.RecreditHebdo == true)
                                try { TraitementRecreditHebdo();}
                                catch (Exception e) { Log.add("Erreur TraitementRecreditHebdo : " + e.Message); }
                            /*
                                                    TraitementSuiviAlerteRail();

                                                    TraitementSuiviAlerteLed();
                            */
                        }
                        catch (Exception e) { Log.add("Err synchUseTimer: zsyncUserTimer.Abort(): " + e.Message); zsyncUserTimer.Abort(); }
                        Log.add("Fin SynchUserTimer3");
                    }
                    
                }
            }

        }
#if REEL
      static  void TraitementSuiviAlerteRail()
		{
            if (ReaderControl.isRailAvailable == false)
            { // Sur une erreur de rail
                if (ReaderControl.misu.CheckRail() == true)
                { // si le rail est retrouvé
                    ReaderControl.isRailAvailable = true;
                    //log.isRailAvailable = true;
                    string message = "Rail retrouvé";
#if EDMX 
                    mainWorker.data.insertAlert(0, 1, Properties.Settings.Default.NumArmoire, message);
#else
                    mainWorkerV3.data.insertAlert(AlerteType.ALERTE_RAIL, 0, Properties.Settings.Default.NumArmoire, message);
#endif
                    if (Properties.Settings.Default.UseDBGMSG)    MessageBox.Show(message);
                    synerr();
                    if (!session) { Log.add("LOGIN suite à perte de rail"); login(); }
                }
            }
        }

#endif
        void synch() {
            if (Properties.Settings.Default.UseSynchro&&!session&&Inventaire.fini) {
                try  {
                    TestConnexionEnCours();
                    lock (objectLock) {
                        Log.add("Debut Synch");
                        DateTime debut = DateTime.Now;

                        // syn.acase(); Ne doit plus être fait au démarrage
                        // Log.add("SynchroLancement acase ok");
                        syn.alertType();
                        Log.add("Synch alertType ok");
                        syn.articleTaille();
                        Log.add("Synch articleTaille ok");
                        //syn.armoire();
                        //Log.add("SynchroLancement armoire ok");
                        syn.correspTaille();
                        Log.add("Synch correspTaillle ok");
                        syn.articleType();
                        Log.add("Synch articleType ok");
                        syn.contentarm();
                        Log.add("Synch contentarm ok");
                        syn.epc();
                        Log.add("SynchroLancement epc ok");
                        List<Alert> alert_nontraitee = syn.alert().ToList();
                        syn.tagAlert(alert_nontraitee);
                        Log.add("Synch alert ok");
                        syn.Version_Armoire();
                        Log.add("Synch version ok");

                        TimeSpan ms = DateTime.Now.Subtract(debut);
                        syn.reportCenter("Success", "SynchroLancement (" + Math.Ceiling(ms.TotalMilliseconds).ToString() + " ms)", 1);
                        Log.add("Fin SynchroLancement");
                        //isInitialising = false;
                    }
                }
                catch (Exception e)
                {
                    Log.add("Erreur synchro lancement: " + e.Message);
                    if (Properties.Settings.Default.UseDBGMSG)
                    {
                        string s = "";
                        if (e.InnerException != null) s = "\n" + e.InnerException.Message;
                        Log.add("Err synchro globale: zsync.Abort(): " + e.Message + s);
                    	zsync.Abort();
                    }
                }
                finally      {if (Properties.Settings.Default.UseDBGMSG) Log.add("Synch terminée"); }
            }
        }

        void synchLoad()
        {
            if (Properties.Settings.Default.UseSynchro&&!session&&Inventaire.fini)
            {
                try
                {
              //      if ((rel == null || (rel != null && nbrel == 0))&& (sel == null || (sel != null && nbsel == 0))&& (val == null || (val != null && nbval == 0)))
                    {
                            TestConnexionEnCours();
                        lock (objectLock) {
                            Log.add("Debut synchLoad");
                            //démarage de l'appli
                            DateTime debut = DateTime.Now;
                            //syn.acase(); Ne doit plus être fait au démarrage
                            syn.alertType();
                            syn.articleTaille();
                            //syn.armoire();
                            syn.articleType();
                            syn.correspTaille();
                            TimeSpan ms = DateTime.Now.Subtract(debut);
                            syn.reportCenter("Success", "Load (" + Math.Ceiling(ms.TotalMilliseconds).ToString() + " ms)", 4);
                            Log.add("Fin synchLoad");
                        }
                       

                    }
                }
                catch (Exception e){
                    Log.add("Erreur synchro load: " + e.Message);
                    if (Properties.Settings.Default.UseDBGMSG){
                        string s = "";
                        if (e.InnerException != null) s = "\n" + e.InnerException.Message;
						Log.add("Err synchLoad zsyncLoad.Abort(); 	zsyncLoad.Abort()" + e.Message + s);
                    }
                }
                finally    {  if (Properties.Settings.Default.UseDBGMSG)    Log.add("SynchLoad terminée");         }
            }
        }

        static void synchError() {
            List<Alert> alert_nontraitee = new List<Alert>();
            if (Properties.Settings.Default.UseSynchro&&!session&&Inventaire.fini)
            {
                try
                {
            //        if ((rel == null || (rel != null && nbrel == 0)) && (sel == null || (sel != null && nbsel == 0)) && (val == null || (val != null && nbval == 0)))
{                       
                          lock (objectLock) {
                            Log.add("Debut synchroAlert");
                            //en cas d'erreur
                            DateTime debut = DateTime.Now;
                            bool a = false;
                            bool b = false;
                            string etat = null;
                            if (Synchro.testConnexionServer())  { 
                                Log.add("Debut synchroServerAlert");
                                alert_nontraitee = syn.alert();
                                syn.tagAlert(alert_nontraitee);
                                a = true;
                                Log.add("Fin synchroServerAlert");
                            }
                            if (Synchro.testConnexionMail()) {
                                Log.add("Debut synchroMailAlert");
                                syn.mailAlert(false);
                                syn.mailtagAlert();
                                b = true;
                                Log.add("Fin synchroMailAlert");
                            }
                            Log.add("connexion au serveur : " + (a ? "reussie" : "non fonctionnelle") + "; connexion a sendmail : " + (b ? "reussie" : "non fonctionnelle"));
                            if (Synchro.testConnexionServer())
                            {
                                etat = (a && b) ? "Success" : "Failure";
                                TimeSpan ms = DateTime.Now.Subtract(debut);
                                syn.reportCenter(etat, "SynchroAlert (" + Math.Ceiling(ms.TotalMilliseconds).ToString() + " ms)", 2);
                            }

                            Log.add("Fin synchroAlert");

                        } 
                    }
                }
                catch (Exception e)
                {
                    Log.add("Erreur synchro alert: " + e.Message);
                    if (Properties.Settings.Default.UseDBGMSG) {
                        string s = "";
                        if (e.InnerException != null) s = "\n" + e.InnerException.Message;                      
  						Log.add("erreur SynchError: zsyncAlerte.Abort()"+ e.Message + s);
						zsyncAlerte.Abort();
                    }
                }
                finally {if (Properties.Settings.Default.UseDBGMSG) Log.add("SynError terminée"); }
            }
        }

        static void synchReload()  {
            if (Properties.Settings.Default.UseSynchro&&!session&&Inventaire.fini) {
                try {
                  TestConnexionEnCours();
                  lock (objectUser) {
                        Log.add("Debut SynchroReload");
                        //apres scan reload
                        DateTime debut = DateTime.Now;
                        syn.contentarm();
                        Log.add("SynchroReload contentarm ok");
                        syn.epc();
                        Log.add("SynchroReload epc ok");
                        syn.logEPC();
                        Log.add("SynchroReload logEPC ok");
                        TimeSpan ms = DateTime.Now.Subtract(debut);
                        syn.reportCenter("Success", "SynchroReload (" + Math.Ceiling(ms.TotalMilliseconds).ToString() + " ms)", 4);
                        Log.add("Fin SynchroReload");
                    }
                }
                catch (Exception e) {
                    Log.add("Erreur synchro reload: " + e.Message);
                    if (Properties.Settings.Default.UseDBGMSG)
                    {
                        string s = "";
                        if (e.InnerException != null) s = "\n" + e.InnerException.Message;
                        Log.add("Erreur synchReload: synchroReload.Abort():" + e.Message + s);
						zsyncReload.Abort();
                    }
                }
                finally  { if (Properties.Settings.Default.UseDBGMSG) Log.add("synchError (Alerte) terminée");  }
            }
        }

        private static void TestConnexionEnCours()
        {

            if (!(Synchro.testConnexionServer()))
            {
                if (!firstTimeConnectionLost)
                {
                    string message = "la connexion de synchro est perdue";
#if EDMX 
                                mainWorker.data.insertAlert(6, 0, Properties.Settings.Default.NumArmoire, message);
#else
                    mainWorkerV3.data.insertAlert(AlerteType.ALERTE_RESEAU, 0, ReaderId, message);
#endif
                    firstTimeConnectionLost = true;
                }
            }
            else
            {
                if (firstTimeConnectionLost)
                {
                    firstTimeConnectionLost = false;
                    string message = "La connexion de synchro est retrouvé";
#if EDMX 
                            mainWorker.data.insertAlert(6, 0, Properties.Settings.Default.NumArmoire, message);
#else
                    mainWorkerV3.data.insertAlert(AlerteType.ALERTE_RESEAU, 0, ReaderId, message);
#endif
                    synerr();
                }
            }
        }

        static void synchUser()
        {
            if (Properties.Settings.Default.UseSynchro&&!session&&Inventaire.fini)
            {
                try
                {
                    //if (sel == null && val == null)
                    TestConnexionEnCours();
                    lock (objectUser)
                    {
                        Log.add("Debut SynchroUser");
                        DateTime debut = DateTime.Now;
                        syn.contentarm();
                        Log.add("SynchroUser contentarm ok");
                        syn.epc();
                        Log.add("SynchroUser epc ok");
                        syn.logEPC();
                        Log.add("SynchroUser logEPC ok");
                        TimeSpan ms = DateTime.Now.Subtract(debut);
                        syn.reportCenter("Success", "SynchroUser (" + Math.Ceiling(ms.TotalMilliseconds).ToString() + " ms)", 3);
                        Log.add("Fin SynchroUser");
                    }
                }
                catch (Exception e)
                {
                    Log.add("Erreur synchro user: " + e.Message);
                    if (Properties.Settings.Default.UseDBGMSG)
                    {
                        string s = "";
                        if (e.InnerException != null) s = "\n" + e.InnerException.Message;             
 						Log.add("Erreur synchUser zsyncUser.Abort(): "+ e.Message + s );
 						zsyncUser.Abort();
                    }
                }
                finally { if (Properties.Settings.Default.UseDBGMSG) Log.add("SynchroUser terminée");  }

            }
        }
        void synchRestitution()
        {
            if (Properties.Settings.Default.UseSynchro&&!session&&Inventaire.fini)
            {
                try
                {
                    TestConnexionEnCours();
                    log.textBoxAnnonceSynchro.Visibility = Visibility.Visible;
                    log.textBoxAnnonceSynchro.Text = "synchronisation en cours";
                    lock (objectLock)
                    {
                        Log.add("Debut SynchroRestitution");
                        //apres scan restitution
                        DateTime debut = DateTime.Now;
                        syn.epc();
                        Log.add("SynchroRestitution epc ok");
                        syn.logEPC();
                        Log.add("SynchroRestitution logEPC ok");

                        TimeSpan ms = DateTime.Now.Subtract(debut);
                        syn.reportCenter("Success", "SynchroRestitution (" + Math.Ceiling(ms.TotalMilliseconds).ToString() + " ms)", 5);
                        Log.add("Fin SynchroRestitution");
                        log.textBoxAnnonceSynchro.Visibility = Visibility.Hidden;
                        log.textBoxAnnonceSynchro.Text = "synchronisation en cours";
                    }
                }
                catch (Exception e)
                {
                    Log.add("Erreur synchro restitution: " + e.Message);
                    if (Properties.Settings.Default.UseDBGMSG)
                    {
                        string s = "";
                        if (e.InnerException != null) s = "\n" + e.InnerException.Message;
                        MessageBox.Show("Erreur synchro restitution: " + e.Message + s);
						Log.add("Erreur synchRestitution zsyncRestitution.Abort(): "+ e.Message + s );
						zsyncRestitution.Abort();
                    }
                }
                finally {if (Properties.Settings.Default.UseDBGMSG) Log.add("synchRestitution terminée"); }
            }
        }

     static   void initTimerSynchroUser()
        {
            timerSynchroUser = new DispatcherTimer();
            timerSynchroUser.Tick += new EventHandler(timerSynchroUser_Tick);
            timerSynchroUser.Interval = new TimeSpan(0, 3, 0);
            timerSynchroUser.Start();
        }

        void initTimerSynchroLoad()
        {
            timerSynchroLoad = new DispatcherTimer();
            timerSynchroLoad.Tick += new EventHandler(timerSynchroLoad_Tick);
            timerSynchroLoad.Interval = new TimeSpan(1, 0, 0);
            timerSynchroLoad.Start();
        }

        void initTimerKeepAliveSynch()
        {
            timerSynchroLoad = new DispatcherTimer();
            timerSynchroLoad.Tick += new EventHandler(timerKeepAliveSynch_Tick);
            timerSynchroLoad.Interval = new TimeSpan(0, 3, 0);
            timerSynchroLoad.Start();
        }


        void timerKeepAliveSynch_Tick(object sender, object e)
        {
            try
            {
                mainWorkerV3.data.updateVersionDate();
            }
            catch (Exception ex)
            {
                Log.add("Erreur KeepAliveSynch: " + ex.Message);
                if (Properties.Settings.Default.UseDBGMSG)
                {
                    MessageBox.Show("Erreur KeepAliveSynch: " + ex.Message);
                }
            }

        }

        private static void TraitementRegleChangeMin()
        {
            WorkerV3 mainWorkerV3 = new WorkerV3();
            //Ajout du traitement de la règle de changes min :
            int nbday = (Int32)DateTime.Now.DayOfWeek;
            if ((nbday == 1) && (LundiTraite == false))
            {
                Log.add("Debut TraitementRegleChangeMin ");
                LundiTraite = true;
                // Liste des user_articles
                List<User_Article> userArticles = mainWorkerV3.data.getUser_article().ToList();
                //List<User_Article> userArticlesTraités = new List<User_Article>();

                foreach (User_Article ua in userArticles) {
                    try {
                        User thisUsr = mainWorkerV3.data.getUserById(ua.User_Id).ToList().FirstOrDefault();
                        if (thisUsr != null && thisUsr.End_of_Validity > DateTime.Now)
                        {
                            int nbchangessince7days = mainWorkerV3.data.GetNumberOfArticleTypeInLogEpcForLast7days(ua.Article_Type_Id, ua.User_Id);
                            int nbCredits = ua.Credit;
                            if (nbchangessince7days < nbCredits)
                            {
                                //userArticlesTraités.Add(ua);
                                string message = "Règles de change non remplie.";
                                Log.add(message);
                                //Insere alert dans bdd
                                int id = mainWorkerV3.data.insertAlert(AlerteType.ALERTE_REGLECHANGEMIN, ua.User_Id, ReaderId, message);

                                try
                                {
                                    List<Article_Type> lat = mainWorkerV3.data.getArticle_type_FromId(ua.Article_Type_Id);
                                    List<string> typ = lat.Where(m => m.Id == ua.Article_Type_Id).Select(w => w.Code).ToList();
                                    for (int nb = 0; nb < nbchangessince7days + 1; nb++)
                                        mainWorkerV3.data.insertTagAlert(id, "", "", "", typ[0], "");
                                }
                                catch (Exception ex)
                                {
                                    Log.add("Erreur insertTagAlert: " + ex.Message);
                                    if (Properties.Settings.Default.UseDBGMSG)
                                        MessageBox.Show("Erreur insertTagAlert: " + ex.Message);
                                }
                                synerr();

                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.add("Erreur TraitementRegleChangeMin: " + e.Message);
                        if (Properties.Settings.Default.UseDBGMSG)
                            MessageBox.Show("Erreur TraitementRegleChangeMin: " + e.Message);
                    }
                } Log.add("Fin TraitementRegleChangeMin");
            }
            else if (nbday != 1)
            {
                LundiTraite = false;
            }
        }

        /// <summary>
        /// Traitement de la recréditation hebdomadaire
        /// </summary>
        private static void TraitementRecreditHebdo()
        {
            int nbday = (Int32)DateTime.Now.DayOfWeek;

            int heure = DateTime.Now.Hour;


            // NumJourRecreditHebdo configuré de 1 à 7 pour un équivalent du lundi au dimanche
            if (Properties.Settings.Default.UseDBGMSG)
            {
                Log.add("Jour: " + (nbday == Properties.Settings.Default.NumJourRecreditHebdo));
                Log.add("Heure: " + (heure == Properties.Settings.Default.HeureRecreditHebdo));
                Log.add("RecreditHebdoTraite ? " + RecreditHebdoTraite);
            }
 
            Log.add("Recreditation: "+((nbday == Properties.Settings.Default.NumJourRecreditHebdo) &&
                (heure == Properties.Settings.Default.HeureRecreditHebdo) &&
                (RecreditHebdoTraite == false)));
            if ((nbday == Properties.Settings.Default.NumJourRecreditHebdo) &&
                (heure == Properties.Settings.Default.HeureRecreditHebdo) &&
                (RecreditHebdoTraite == false)) {

                    Log.add("Debut TraitementRecreditHebdo "); 

                RecreditHebdoTraite = true;
                // Liste des user_articles
                List<User_Article> userArticles = mainWorkerV3.data.getUser_article().ToList();
                //List<User_Article> userArticlesTraités = new List<User_Article>();
                foreach (User_Article ua in userArticles){
                    try {
                        User thisUsr = mainWorkerV3.data.getUserById(ua.User_Id).ToList().FirstOrDefault();
                        if (thisUsr != null && thisUsr.End_of_Validity > DateTime.Now)
                        {
                            if (ua.Credit_Semaine_Suivante > 0)
                            {
                                try
                                {
                                    mainWorkerV3.data.UpdateUserArticleRestitutionHebdo(ua);
                                }
                                catch (Exception ex)
                                {
                                    Log.add("Erreur UpdateUserArticleRestitutionHebdo: " + ex.Message);
                                    if (Properties.Settings.Default.UseDBGMSG)          MessageBox.Show("Erreur UpdateUserArticleRestitutionHebdo: " + ex.Message);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.add("Erreur TraitementRecreditHebdo: " + e.Message);
                        if (Properties.Settings.Default.UseDBGMSG)                      MessageBox.Show("Erreur TraitementRecreditHebdo: " + e.Message);
                    }
                }
                Log.add("Fin TraitementRecreditHebdo "); 
            }
            else if (nbday != Properties.Settings.Default.NumJourRecreditHebdo)
            {
                RecreditHebdoTraite = false;
            }
        }

     static   void timerSynchroUser_Tick(object sender, object e)
        {
            if (Properties.Settings.Default.UseSynchroTimer&&!session&&Inventaire.fini) {
                if (zsyncUserTimer==null||(zsyncUserTimer!=null&&!zsyncUserTimer.IsAlive&&(zsyncUserTimer.ThreadState == System.Threading.ThreadState.Unstarted || zsyncUserTimer.ThreadState == System.Threading.ThreadState.Stopped || zsyncUserTimer.ThreadState == System.Threading.ThreadState.Aborted)))
                    {
                        try
                        {
                            log.textBoxAnnonceSynchro.Visibility = Visibility.Visible;
                            log.textBoxAnnonceSynchro.Text = "synchronisation en cours";
                            zsyncUserTimer = new Thread(new ThreadStart(synchUserTimer));
                            zsyncUserTimer.Name = "synchUser";
                            zsyncUserTimer.Start();
                            log.textBoxAnnonceSynchro.Visibility = Visibility.Hidden;
                        }
                        catch (Exception ex)
                        {
                            Log.add("Erreur zsyncUserTimer: " + ex.Message);
                            if (Properties.Settings.Default.UseDBGMSG)
                            {
                                MessageBox.Show("Erreur zsyncUserTimer: " + ex.Message);
                            }
                        }
                    }
            }
#if REEL
            // A faire systèmatiquement toutes les 3 minutes
            if (Properties.Settings.Default.UseSynchro &&!session && Inventaire.fini)
            {
                //     synchReload();
                if(zsyncReload == null || (!zsyncReload.IsAlive && (zsyncReload.ThreadState == System.Threading.ThreadState.Unstarted || zsyncReload.ThreadState == System.Threading.ThreadState.Stopped || zsyncReload.ThreadState == System.Threading.ThreadState.Aborted)))
                    try
                    {
                        log.textBoxAnnonceSynchro.Visibility = Visibility.Visible;
                        log.textBoxAnnonceSynchro.Text = "synchronisation en cours";
                        zsyncReload = new Thread(new ThreadStart(synchReload));
                        zsyncReload.Name = "synchroReloader";
                        zsyncReload.Start();
                    }
                    catch (Exception ex)
                    {
                        Log.add("Erreur synchroReloader: " + ex.Message);
                        if (Properties.Settings.Default.UseDBGMSG)
                        {
                            MessageBox.Show("Erreur synchroReloader: " + ex.Message);
                        }
                    }

                //       synchUser(); 
                if (zsyncUser == null || (!zsyncUser.IsAlive && (zsyncUser.ThreadState == System.Threading.ThreadState.Unstarted || zsyncUser.ThreadState == System.Threading.ThreadState.Stopped || zsyncUser.ThreadState == System.Threading.ThreadState.Aborted)))
                    try
                    {
                        zsyncUser = new Thread(new ThreadStart(synchUser));
                        zsyncUser.Name = "synchUser";
                        zsyncUser.Start();
                    }
                    catch (Exception ex)
                    {
                        Log.add("Erreur zsyncUser: " + ex.Message);
                        if (Properties.Settings.Default.UseDBGMSG)
                        {
                            MessageBox.Show("Erreur zsyncUser: " + ex.Message);
                        }
                    }


            }
            if (!session && Inventaire.fini)
          try {
                    TraitementSuiviAlerteRail();

                    readercontrol.TraitementSuiviAlerteLed();

                }  catch(Exception exc) {
                    Log.add("Erreur timerSynchroUser_Tick: "+ exc.Message);
                }
            log.textBoxAnnonceSynchro.Visibility = Visibility.Hidden;
#endif
        }

        void timerSynchroLoad_Tick(object sender, object e)
        {
            if (!zsyncLoad.IsAlive&&!session&&Inventaire.fini)
            {
                if (zsyncLoad.ThreadState == System.Threading.ThreadState.Unstarted || zsyncLoad.ThreadState == System.Threading.ThreadState.Stopped || zsyncLoad.ThreadState == System.Threading.ThreadState.Aborted)
                {
                    try
                    {
                        zsyncLoad = null;
                        zsyncLoad = new Thread(new ThreadStart(synchLoad));
                        zsyncLoad.Start();
                    }
                    catch (Exception ex)
                    {
                        Log.add("Erreur zsyncLoad: " + ex.Message);
                        if (Properties.Settings.Default.UseDBGMSG)
                        {
                            MessageBox.Show("Erreur zsyncLoad: " + ex.Message);
                        }
                    }
                }
            }
        }



        //Fonction d'initialisation des contacteurs de portes



        /// <summary>
        /// Configuration lecteur rfid et lancement scan
        /// </summary>






        #region UserControl

        /// <summary>
        /// Démarrage du User control du login
        /// </summary>
        public static void login()

        { Log.add("/********* LOGIN *********/");  
#if REEL
            readercontrol.TraitementCheckAlerteLed();
#endif
            
            try{session = false;         
                mainGrid.Children.Clear();
                nbrel = 0;
                nbsel = 0;
                nbval = 0;
               
                log = new Login();
                log.isRFIDreaderConnected = ReaderControl.isRFIDreaderConnected;
                log.isRFIDreaderRestitutionConnected = isRFIDreaderRestitutionConnected;
#if REEL
                log.isRailAvailable = ReaderControl.isRailAvailable;
#endif
                log.isInitialising = isInitialising;
                log.isLocalBDDAvailable = isLocalBDDAvailable;

                log.textBoxID.Focus();
                log.Reload += new RoutedEventHandler(log_Reload);
                log.Select += new RoutedEventHandler(log_Select);
 
                 mainGrid.Children.Add(log);
                reloaderB = false;
            }
            catch (Exception ee)
            {
                Log.add("Erreur login: " + ee.Message);
                if (Properties.Settings.Default.UseDBGMSG)
                    MessageBox.Show("Erreur login: " + ee.Message);
            }
        }

        public static void Select() {
            try
            {
                session = true;
                mainGrid.Children.Clear();

                sel = new Selection_Article();
                sel.txtBienvenue.Text = log.msg;
                sel.userId.Text = Login.user.Id.ToString();

                sel.retour += new RoutedEventHandler(backtologin);
                sel.valid += new RoutedEventHandler(sel_Valid);

                mainGrid.Children.Add(sel);
                nbsel++;

                reloaderB = false;
            }
            catch (Exception e) { Log.add("Erreur select: " + e.Message); }
        }

        // Démarrage du User Control validation_Article

        public static void Valid()
        {          
            try
            {   session = true;
                mainGrid.Children.Clear();
                nbsel = 0;

                val = new Validation_Article();
                val.textBoxUserid.Text = Login.user.Id.ToString();
                val.listSelectionValidation.ItemsSource = sel.sampleData2;
                val.textBoxUserid.Text = sel.taille;

                val.BackToLogin += new RoutedEventHandler(backtologin2);
                val.Back += new RoutedEventHandler(val_Back);
                val.open += new RoutedEventHandler(val_OpenUser);
#if REEL
                val.verifstateport += new RoutedEventHandler(readercontrol.verifstateport);
#endif
#if OPTLANG
                val.txtBienvenue.Text = log.msgv + Languages.yourOrder + "\n";
#else
                val.txtBienvenue.Text = log.msgv + "Votre commande :\n";
#endif
                int i = 0;
                foreach (var item in sel.sampleData2)
                {
                    if (i != 0) val.txtBienvenue.Text += "+ ";
                    val.txtBienvenue.Text += item.Quantit + " " + item.Description + " T" + item.Taille + "";
                    i++;
                }
                mainGrid.Children.Add(val);
                nbval = 1;
            }
            catch (Exception e) { Log.add("Erreur valid: " + e.Message); }
        }


        public static void LancerCompteRebours() {

           
            
        //        if (compterebours == null)
                    try
                    {
                        Log.add("Lancement compterebours");
                        session = false;
                        nbrel = 0;
                        nbsel = 0;
                        nbval = 0;
                        mainGrid.Children.Clear();

                        compterebours = new CompteRebours();

                        compterebours.TerminerCompteRebours += new RoutedEventHandler(FinCompteRebours);

                        mainGrid.Children.Add(compterebours);;
                     
                    }
                    catch (Exception exp) { Log.add("err timercompterebours " + exp.Message); }
               
        }


        // Démarrage de la phase User control du rechargement
        public static void reload()
        {       
         try  { 
                session = true;
                mainGrid.Children.Clear();
                rel = new Reloader();               
                nbrel++;
                rel.OpenReload += new RoutedEventHandler(rel_OpenReload);
                rel.DecoClick += new RoutedEventHandler(rel_DecoReload);
                rel.FermerReload += new RoutedEventHandler(rel_FermerReload);
                //         rel.verifstateport += new RoutedEventHandler(readercontrol.verifstateport);
                mainGrid.Children.Add(rel);
                reloaderB = true;
                rscan = false;
            }
            catch (Exception e) { Log.add("erreur reload: " + e.Message); }
        }

     
        /// <summary>
        /// Evenement qui arrive lors de l'appui sur le button "DECONNEXION" dans la phase de rechargement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void rel_DecoReload(object sender, System.Windows.RoutedEventArgs e) { Log.add("LOGIN rel_DecoReload"); login(); }
        public static void rel_FermerReload(object sender, System.Windows.RoutedEventArgs e)  {
#if REEL
                readercontrol.leds.shut();           

            //    MainWindow.dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, (Action)delegate
         //       {
                    if (readercontrol.ReaderIsAvailable(Properties.Settings.Default.SpeedWayReaderConnectionString))
                    {
                        ReaderControl.scanrunning = false;
                        readercontrol.Portes();    
                        readercontrol.RFIDScanRetrait();
                    }
             //   });
#else

                MainWindow.compterebours.init(10);
                /*
                   List<Tag> list = new List<Tag>() ;
                Tag tag =null;
                TagData tagdata = new TagData();            
                tag.Epc = TagData.FromHexString("45366ABCD54EF");
                list.Add(tag);
                          
                   Inventaire.updateListbox(list);
                   */

#endif

        }

        // Evenement qui déclenche l'ouverture des portes pour rechargement
        // Provoqué par l'appui sur la touche "OUVRIR"
        private static void rel_OpenReload(object sender, System.Windows.RoutedEventArgs e)
        {
#if REEL
            readercontrol.gacheReload();
#else
            //     scan();
#endif
        }

        // Affichage du usercontrol de rechargement
        private static void log_Reload(object sender, System.Windows.RoutedEventArgs e) { reload(); }

        // Affichage du usercontrole de la selection d'article par la phase de login (mode user)
        private static void log_Select(object sender, System.Windows.RoutedEventArgs e) { Select(); }


        // Evenement qui arrive lors de l'appui sur le button "DECONNEXION" dans la phase de Selection
        private static void backtologin(object sender, System.Windows.RoutedEventArgs e) { Log.add("LOGIN backtologin"); login(); }

        // Evenement qui arrive lors de l'appui sur le bouton "DECONNEXION" dans la phase de Validation
        private static void backtologin2(object sender, System.Windows.RoutedEventArgs e) { Log.add("LOGIN backtologin2"); login(); }

        // Evenement qui arrive lors de l'appui sur le button "VALIDEZ" selection article 
        private static void sel_Valid(object sender, System.Windows.RoutedEventArgs e)
        {
            // Appel du usercontrol validation article
            Valid();
        }

        // Evenement qui arrive lors de l'appui sur le bouton "RETOUR" du usercontrol validation article
        // Appel du usercontrol selection article
        private static void val_Back(object sender, System.Windows.RoutedEventArgs e) { Select(); }

        /// Evenement qui arrive à la fin du scan, renvoi à l'écran de login

        private static void FinCompteRebours(object sender, System.Windows.RoutedEventArgs e)
        {
     /*       DateTime debut = DateTime.Now;
            do { } while (ReaderControl.scanrunning || ReaderControl.railmoving);
            TimeSpan ms = DateTime.Now.Subtract(debut);
            Log.add("Attente Rail: " + Math.Ceiling(ms.TotalMilliseconds).ToString() + " ms");*/
      //      Log.add("FinCompteRebours, scanRunning =" + ((ReaderControl.scanrunning == true) ? "true" : "false"));
            login();
        }

        /// <summary>
        /// Evenement de verification porte a la validation déclenché à l'ouverture 
        /// des portes pour un porteur après l'appel à val_scan (cf. ci_dessous) 
        /// dans la phase de validation pour lancer 
        /// le timer de contrôle du temps max d'utilisation (déclenchement d'alerte)

        // Evenement qui déclenche l'ouverture des portes pour un porteur
        // Provoqué par l'appui sur la touche "OUVRIR"

        private static void val_OpenUser(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
#if REEL
                readercontrol.gacheUser(val.Ldoor,val.Rdoor);
                // lance un timer pour controler le temps max autorisé en ouverture
          //      readercontrol.gacheReload();
#endif
                // userSelect contient la liste des epc avec l'article type de l'item selectionné et en fonction de la taille de l'utilisateur

                //    nbitem = val.nbitemout;
                // arm représente le contenu de l'armoire
#if EDMX
                List<content_arm> arm = mainWorker.data.getContent().ToList();
#else
                List<Content_Arm> arm = mainWorkerV3.data.getContent().ToList();
#endif
              
                sampleData.Clear();
                //string date = String.Format("{0:yyyy-MM-dd HH:mm:ss}", dt);
                for (int i = 0; i < arm.Count; i++)
                {
                    temp = new List<listItem>()  {
                        new listItem {
                            Id=arm[i].Id,
                            Creation_Date=arm[i].Creation_Date,
                            Epc= arm[i].Epc,
                            State =arm[i].State,
                            RFID_Reader= arm[i].RFID_Reader
                        }
                                 };
                    sampleData.Add(temp[0]);
                }
  
               
            }
            catch (Exception except) { Log.add("Erreur val_scan: " + except.Message); }
        }


        // Evenement qui arrive à la fin du Scan de restitution
        // Provoque le démarrage de la tache de synchronisation 

        private void rest_finScanRestitution(object sender, EventArgs e)
        {
            if (isRFIDreaderRestitutionConnected)
            {
                if (!zsyncRestitution.IsAlive)
                {
                    if (zsyncRestitution.ThreadState == System.Threading.ThreadState.Unstarted
                        || zsyncRestitution.ThreadState == System.Threading.ThreadState.Stopped
                        || zsyncRestitution.ThreadState == System.Threading.ThreadState.Aborted)
                    {
                        zsyncRestitution = null;
                        zsyncRestitution = new Thread(new ThreadStart(synchRestitution));
                        zsyncRestitution.Start();
                    }
                }
            }
        }

        #endregion // event

        // Passage forcé en mode plein écran
        private void zoomFullScreen()
        {
            this.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            mainGrid.LayoutTransform = new ScaleTransform((double)System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width / 1024, (double)System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height / 768);
            this.WindowState = WindowState.Maximized;
        }

        public static void testLog()
        {

     //       if ((rel == null || (rel != null && !mainGrid.Children.Contains(rel))) && (sel == null || (sel != null && !mainGrid.Children.Contains(rel))) && (val == null || (val != null && !mainGrid.Children.Contains(val))))
    if(Inventaire.fini)
            {
                // On profite de cette routine périodique pour controler l'accès à la base de donnée locale
#if EDMX            
                if ((mainWorker.data.Is_connected() == false) && (isLocalBDDAvailable == true))
                {
                    isLocalBDDAvailable = false;
                    leds.shut();
                    login();
                }
                else if ((mainWorker.data.Is_connected() == true) && (isLocalBDDAvailable == false))
                {
                    isLocalBDDAvailable = true;
                    login();
                }
#else
                if ((mainWorkerV3.data.Is_connected() == false) && (isLocalBDDAvailable == true))
                {
                    isLocalBDDAvailable = false;
#if REEL
                        readercontrol.leds.shut();
#endif
                        Log.add("TestLog1 : LOGIN"); login();
                }
                else if ((mainWorkerV3.data.Is_connected() == true) && (isLocalBDDAvailable == false))
                {
                    isLocalBDDAvailable = true;
                    Log.add("TestLog2 : LOGIN"); login();
                }
#if REEL
                if ((log.isRailAvailable) != (ReaderControl.isRailAvailable)) { Log.add("TestLog3 : LOGIN");  login(); }
#endif

#endif
            }
            if (log != null) log.isLocalBDDAvailable = isLocalBDDAvailable;
        }
    }
 
}
