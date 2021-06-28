using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ArmoireV3.Entities;
using System.Windows.Threading;
using ArmoireV3.Inter;
using ArmoireV3.Controls;

namespace ArmoireV3.Dialogues
{
    /// <summary>
    /// Logique d'interaction pour Login.xaml
    /// </summary>
    public partial class Login : UserControl
    {
        public static User user=null;// correspond au User logué à l'armoire
#if EDMX
        static Worker workerLogin = new Worker();
#else
        static WorkerV3 workerLoginV3 = new WorkerV3();
#endif

        public string msg = "";
        public string msgv = "";
        DispatcherTimer timerErreur = new DispatcherTimer();

        private bool _isRFIDreaderConnected = true;
        public bool isRFIDreaderConnected
        {
            get
            {
                return _isRFIDreaderConnected;
            }
            set
            {
                _isRFIDreaderConnected = value;
                if (_isRFIDreaderConnected)
                {
                    textBoxErreurImpinj.Visibility = Visibility.Hidden;
                }
                else
                {
                    textBoxErreurImpinj.Visibility = Visibility.Visible;
                }
            }
        }

        private bool _isRailAvailable = true;
        public bool isRailAvailable
        {
            get
            {
             if (Properties.Settings.Default.UseRail == false
                    || (Properties.Settings.Default.RailIsMisumi == false && Properties.Settings.Default.RailIsCoding == false) )
                    _isRailAvailable = true;
                return _isRailAvailable;
            }
            set
            {
                _isRailAvailable = value;
                if (Properties.Settings.Default.UseRail == false
                    || (Properties.Settings.Default.RailIsMisumi == false && Properties.Settings.Default.RailIsCoding == false) )
                {
                    _isRailAvailable = true;
                }
                if (_isRailAvailable)
                {
                    textBoxErreurRailAvailable.Visibility = Visibility.Hidden;
                }
                else
                {
                    textBoxErreurRailAvailable.Visibility = Visibility.Visible;
                }
            }
        }

        private bool _isInitialising = true;
        public bool isInitialising
        {
            get
            {
                return _isInitialising;
            }
            set
            {
                _isInitialising = value;
                if (_isInitialising)
                {
                    textBoxErreurInitialisation.Visibility = Visibility.Visible; 
                }
                else
                {
                    textBoxErreurInitialisation.Visibility = Visibility.Hidden;
                }
            }
        }        

        private bool _isLocalBDDAvailable = true;
        public bool isLocalBDDAvailable
        {
            get
            {
                return _isLocalBDDAvailable;
            }
            set
            {
                _isLocalBDDAvailable = value;
                
                if (_isLocalBDDAvailable)
                {
                    textBoxErreurLocalBDDAvailable.Visibility = Visibility.Hidden;
                }
                else
                {
                    textBoxErreurLocalBDDAvailable.Visibility = Visibility.Visible;
                }
            }
        }

        
        private bool _isRFIDreaderRestitutionConnected = true;
        public bool isRFIDreaderRestitutionConnected
        {
            get
            {
                return _isRFIDreaderRestitutionConnected;
            }
            set
            {
                _isRFIDreaderRestitutionConnected = value;
                if (Properties.Settings.Default.SpeedWayReaderConnectionString == null
                    || Properties.Settings.Default.SpeedWayReaderConnectionString == "")
                {
                    _isRFIDreaderRestitutionConnected = true;
                }
                if (_isRFIDreaderRestitutionConnected)
                {
                    textBoxErreurImpinjRestitution.Visibility = Visibility.Hidden;
                }
                else
                {
                    textBoxErreurImpinjRestitution.Visibility = Visibility.Visible;
                }
            }
        }

        //  Badge test = new Badge();

        //    DispatcherTimer time = new DispatcherTimer();


        public static readonly RoutedEvent SaveButtonClickedEvent = EventManager.RegisterRoutedEvent(
            "SaveButtonClicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Login));

        public static readonly RoutedEvent FirstLoadEvent = EventManager.RegisterRoutedEvent(
            "FirstLoad", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Login));

        public static readonly RoutedEvent ReloadEvent = EventManager.RegisterRoutedEvent(
           "Reload", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Login));
        public Login()
        {
            InitializeComponent();
            if (Properties.Settings.Default.Debug == false)
                mainGrid.LayoutTransform = new ScaleTransform((double)System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width / 1024, (double)System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height / 725);
            textBoxID.Focus();

            timerErreur.Tick += new EventHandler(timerErreur_Tick);
            timerErreur.Interval = new TimeSpan(0, 0, 3);
            
            //time.Tick += new EventHandler(time_Tic);
            //time.Interval = new TimeSpan(0, 0, 0, 5, 0);
            //time.Start();

        }

        // void time_Tic(object sender, EventArgs e)
        // {
        ////     textBoxID.Text = test.getInventoryResult();
        //  //   textBoxID.Focus();
        // }

        private void buttonDeconnexion_Click(object sender, RoutedEventArgs e)
        {

        }

        private void textBoxID_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((isRFIDreaderConnected) && (isLocalBDDAvailable) && (isRailAvailable))
    if (isRFIDreaderConnected && isLocalBDDAvailable && isRailAvailable &&!MainWindow.session)// !ReaderControl.isRailRunning)
  
            {
                if (textBoxID.Text.Count() == 10)
                {
                    textBoxID.Text = textBoxID.Text.Replace("&", "1");
                    textBoxID.Text = textBoxID.Text.Replace("é", "2");
                    textBoxID.Text = textBoxID.Text.Replace("\"", "3");
                    textBoxID.Text = textBoxID.Text.Replace("'", "4");
                    textBoxID.Text = textBoxID.Text.Replace("(", "5");
                    textBoxID.Text = textBoxID.Text.Replace("-", "6");
                    textBoxID.Text = textBoxID.Text.Replace("è", "7");
                    textBoxID.Text = textBoxID.Text.Replace("_", "8");
                    textBoxID.Text = textBoxID.Text.Replace("ç", "9");
                    textBoxID.Text = textBoxID.Text.Replace("à", "0");
                    textBoxID.Select(textBoxID.Text.Length, 0);

                    login();
                }
                else if (textBoxID.Text.Count() > 10 || (timerErreur.IsEnabled && textBoxID.Text.Count() > 0))
                {
                    textBoxID.Text = "";
                    textBoxID.Focus();
                }
            }
        }

        public event RoutedEventHandler Reload
        {
            add { AddHandler(ReloadEvent, value); }

            remove { RemoveHandler(ReloadEvent, value); }

        }

        public event RoutedEventHandler Select
        {

            add { AddHandler(SaveButtonClickedEvent, value); }

            remove { RemoveHandler(SaveButtonClickedEvent, value); }

        }
        public event RoutedEventHandler FirstLoad
        {

            add { AddHandler(FirstLoadEvent, value); }

            remove { RemoveHandler(FirstLoadEvent, value); }

        }

        /// <summary>
        /// Fonction permettant de ce loger a l'application
        /// </summary>
        /// <returns></returns>
        public void login()
        {
            try
            {
#if EDMX
                {
                    workerLogin.data.dbContext.Refresh(System.Data.Objects.RefreshMode.StoreWins, workerLogin.data.dbContext.user);
                    workerLogin.data.dbContext.Refresh(System.Data.Objects.RefreshMode.StoreWins, workerLogin.data.dbContext.user_article);
                }
#else
                {
                    // workerLoginV3.data.dbContext.Refresh(System.Data.Objects.RefreshMode.StoreWins, workerLoginV3.data.dbContext.user);
                    // workerLoginV3.data.dbContext.Refresh(System.Data.Objects.RefreshMode.StoreWins, workerLoginV3.data.dbContext.user_article);
                }
#endif
            }
            catch /*(EntityException ee)*/
            {
                textBoxErreur.Text = "|-";
                return;
            }
            //List<ArmoireV3.Entities.user> listUser = workerLogin.data.getUser().ToList();
            //List<ArmoireV3.Entities.user> test = listUser.Where(p => p.Password == textBoxID.Text).ToList();
#if EDMX
            {
                user test = workerLogin.data.getUserByPassword(textBoxID.Text);

                if (test != null)
                {
                    try
                    {
                        if (test.Active == 1)
                        {
#if OPTLANG
                        // Passer l'interface utilateur dans la langue du user  
                        string userlang = workerLogin.data.getLangByUserID(test.Id);
                        Languages.Culture = new System.Globalization.CultureInfo(userlang);
                        
#endif
                            if (test.Type == "user")
                            {
                                //string datedujour = String.Format("{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now);
                                //bool btestnow = (test.End_of_Validity.CompareTo(datedujour) < 0);
                                if (test.End_of_Validity.Ticks < DateTime.Now.Ticks)
                                //if (btestnow)
                                {
                                    if (timerErreur.IsEnabled) timerErreur.Stop();
#if OPTLANG
                                textBoxErreur.Text = Languages.OutofDateUserType;
#else
                                    textBoxErreur.Text = "Date de validité dépassée";
#endif
                                    textBoxErreur.Visibility = Visibility.Visible;
                                    timerErreur.Start();
                                }
                                else
                                {
                                    // message de bienvenue
#if OPTLANG
                            msg = Languages.hello + test.Prenom + " " + test.Nom + Languages.welIDbox;
                            msgv = Languages.hello + test.Prenom + " " + test.Nom + " ";
                            msg += Languages.selectOutfit;
                            msg += Languages.discTip;
#else
                                    msg = "Bonjour  " + test.Prenom + " " + test.Nom + " et bienvenue dans votre espace ID box.\r\n";
                                    msgv = "Bonjour  " + test.Prenom + " " + test.Nom + " ";
                                    msg += "Vous pouvez dès à présent sélectionner vos tenues.\r\n";
                                    msg += "Pensez à vous déconnecter en partant !";

#endif
                                    workerLogin.data.updateConnexionUtilisateur(test.Id);
                                    user_id = test.Id.ToString();
                                    textBoxErreur.Visibility = Visibility.Hidden;
                                    RaiseEvent(new RoutedEventArgs(SaveButtonClickedEvent, this));
                                    //time.Stop();
                                }
                            }
                            else if (test.Type == "reloader")
                            {
                                textBoxErreur.Visibility = Visibility.Hidden;
                                RaiseEvent(new RoutedEventArgs(ReloadEvent, this));
                            }
                            else
                            {
                                if (timerErreur.IsEnabled) timerErreur.Stop();
#if OPTLANG
                            textBoxErreur.Text = Languages.wrongUserType;
#else
                                textBoxErreur.Text = "Type d'utilisateur incorrect";
#endif
                                textBoxErreur.Visibility = Visibility.Visible;
                                timerErreur.Start();
                            }
                        }
                        else
                        {
                            if (timerErreur.IsEnabled) timerErreur.Stop();
#if OPTLANG
                        textBoxErreur.Text = Languages.inactiveUser;
#else
                            textBoxErreur.Text = "Utilisateur inactif";
#endif
                            textBoxErreur.Visibility = Visibility.Visible;
                            timerErreur.Start();
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                        textBoxID.Clear();
                        textBoxID.Focus();
                    }
                }
                else
                {
                    if (timerErreur.IsEnabled) timerErreur.Stop();
#if OPTLANG
                textBoxErreur.Text = Languages.incorLogin;
#else
                    textBoxErreur.Text = "Login incorrect";
#endif
                    textBoxErreur.Visibility = Visibility.Visible;
                    timerErreur.Start();
                }
            }
#else // No EDMX MODEL
            {
                if (textBoxID.Text != "")
                {
                     user = workerLoginV3.data.getUserByPassword(textBoxID.Text);

                    if (user != null)
                    {
                        try
                        {
                            if (user.Active == 1)     {
#if OPTLANG
                        // Passer l'interface utilateur dans la langue du user  
                        string userlang = workerLoginV3.data.getLangByUserID(test.Id);
                        Languages.Culture = new System.Globalization.CultureInfo(userlang);
                        
#endif
                                if (user.Type == "user") {
                                    //string datedujour = String.Format("{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now);
                                    //bool btestnow = (test.End_of_Validity.CompareTo(datedujour) < 0);
                                    if (user.End_of_Validity.Ticks < DateTime.Now.Ticks)
                                    //if (btestnow)
                                    {
                                        if (timerErreur.IsEnabled) timerErreur.Stop();
#if OPTLANG
                                textBoxErreur.Text = Languages.OutofDateUserType;
#else
                                        textBoxErreur.Text = "Date de validité dépassée";
#endif
                                        textBoxErreur.Visibility = Visibility.Visible;
                                        timerErreur.Start();
                                    }
                                    else
                                    {
                                        // message de bienvenue
#if OPTLANG
                            msg = Languages.hello + test.Prenom + " " + test.Nom + Languages.welIDbox;
                            msgv = Languages.hello + test.Prenom + " " + test.Nom + " ";
                            msg += Languages.selectOutfit;
                            msg += Languages.discTip;
#else
                                        msg = "Bonjour  " + user.Prenom + " " + user.Nom + " et bienvenue dans votre espace ID box.\r\n";
                                        msgv = "Bonjour  " + user.Prenom + " " + user.Nom + " ";
                                        msg += "Vous pouvez dès à présent sélectionner vos tenues.\r\n";
                                        msg += "Pensez à vous déconnecter en partant !";

#endif
                                        workerLoginV3.data.updateConnexionUtilisateur(user.Id);                                      
                                        textBoxErreur.Visibility = Visibility.Hidden;
                                        RaiseEvent(new RoutedEventArgs(SaveButtonClickedEvent, this));
                                        //time.Stop();
                                    }
                                }
                                else if (user.Type == "reloader")
                                {
                                    textBoxErreur.Visibility = Visibility.Hidden;
                                    RaiseEvent(new RoutedEventArgs(ReloadEvent, this));
                                }
                                else
                                {
                                    if (timerErreur.IsEnabled) timerErreur.Stop();
#if OPTLANG
                            textBoxErreur.Text = Languages.wrongUserType;
#else
                                    textBoxErreur.Text = "Type d'utilisateur incorrect";
#endif
                                    textBoxErreur.Visibility = Visibility.Visible;
                                    timerErreur.Start();
                                }
                            }
                            else
                            {
                                if (timerErreur.IsEnabled) timerErreur.Stop();
#if OPTLANG
                        textBoxErreur.Text = Languages.inactiveUser;
#else
                                textBoxErreur.Text = "Utilisateur inactif";
#endif
                                textBoxErreur.Visibility = Visibility.Visible;
                                timerErreur.Start();
                            }
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message);
                            textBoxID.Clear();
                            textBoxID.Focus();
                        }
                    }
                    else
                    {
                        if (timerErreur.IsEnabled) timerErreur.Stop();
#if OPTLANG
                textBoxErreur.Text = Languages.incorLogin;
#else
                        textBoxErreur.Text = "Login incorrect";
#endif
                        textBoxErreur.Visibility = Visibility.Visible;
                        timerErreur.Start();
                    }
                }
            }
#endif     // No EDMX MODEL

        }

                    

        private void textBoxID_LostFocus(object sender, RoutedEventArgs e)
        {
            textBoxID.Focus();

        }

        private void textBoxID_FocusableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            textBoxID.Focus();
        }

        private void loginGrid_Loaded(object sender, RoutedEventArgs e)
        {
            textBoxID.Focus();
        }

        private void textBoxID_GotFocus(object sender, RoutedEventArgs e)
        {



        }

        private void timerErreur_Tick(object sender, EventArgs e)
        {
            if (timerErreur.IsEnabled) timerErreur.Stop();
            textBoxErreur.Visibility = Visibility.Hidden;
            textBoxID.Clear();
        }
    }
}
