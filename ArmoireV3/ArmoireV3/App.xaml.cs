using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Diagnostics;
using System.Threading;
using ArmoireV3.Entities;
namespace ArmoireV3
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : Application
    {
        String lastErrorMessage = "";
        bool evntpassed = false;

        public App()
        {
            /*
            Process[] liste = Process.GetProcessesByName("ArmoireV3");

            if (liste.Count() != 0)
            {
                System.Environment.Exit(-1);
            }
            */
            this.Startup += this.Application_Startup;
            InitializeComponent();
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("Une erreur non gérée a été rencontrée. Le programme va se fermer. Détails :\r\n " + e.Exception.Message,
                "Erreur Fatale", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
            ArmoireV3.Entities.Log.add("Dispatcher Unhandled Exception: " + e.Exception.Message + " (" + e.Exception.TargetSite.DeclaringType.FullName + "#" + e.Exception.TargetSite.Name + ")");
            MainWindow.Close();
            //this.Shutdown();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                DataManagerV3 data = new DataManagerV3();

                data.insertAlert(AlerteType.ALERTE_DEMARREAPPLI, 0, ArmoireV3.Properties.Settings.Default.NumArmoire, "Armoire redémarrée");
            }
            catch (Exception ase)
            {
                ArmoireV3.Entities.Log.add("Erreur d'insertion d'alerte démarrage" + ase.Message);
            }
            ArmoireV3.Entities.Log.add("============================== [Demarrage de l'application] ==============================");
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            AppDomain.CurrentDomain.FirstChanceException += new EventHandler<System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs>(CurrentDomain_FirstChanceException);
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
           
            Exception ex = e.ExceptionObject as Exception;
            MessageBox.Show("Une erreur non gérée a été rencontrée. Le programme va se fermer. Détails :\r\n " + ex.Message, "Erreur Fatale", MessageBoxButton.OK, MessageBoxImage.Error);
            ArmoireV3.Entities.Log.add("Unhandled Exception: " + ex.Message + " (" + ex.TargetSite.DeclaringType.FullName + "#" + ex.TargetSite.Name + ")");
            ArmoireV3.Entities.Log.add("StackTrace: " + ex.StackTrace);
            MainWindow.Close();
            //this.Shutdown();
            try
            {
                DataManagerV3 data = new DataManagerV3();

                data.insertAlert(5, 0, ArmoireV3.Properties.Settings.Default.NumArmoire, "Crash application Armoire");
            }
            catch (Exception cue)
            {
                ArmoireV3.Entities.Log.add("Erreur d'insertion d'alerte crash " + cue.Message);
            }
        }

        void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            string errorMessage = "Handled Exception: " + e.Exception.Message + " (" + e.Exception.TargetSite.DeclaringType.FullName + "#" + e.Exception.TargetSite.Name + ")";
            if (e.Exception.Message == "Queue Closed") {
                errorMessage = "Initialisation du lecteur de carte";
            }
            if (e.Exception.Message.Contains("index était hors limites"))
            {
                ArmoireV3.Entities.Log.add("Index");
            }
            if (e.Exception.InnerException != null)
            {
                errorMessage += " - InnerException: " + e.Exception.InnerException.Message + " (" + e.Exception.InnerException .TargetSite.DeclaringType.FullName + "#" + e.Exception.InnerException .TargetSite.Name + ")";
            }
            if (errorMessage != lastErrorMessage)
            {
                lastErrorMessage = errorMessage;
                ArmoireV3.Entities.Log.add(errorMessage);
            }
        }

        void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            /*
            ArmoireV3.Entities.Log.add("============================== [Fermeture de l'application] ==============================");
            try
            {
                DataManagerV3 data = new DataManagerV3();


                data.insertAlert(AlertId.ALERTE_ARRETAPPLI, 0, ArmoireV3.Properties.Settings.Default.NumArmoire, "Armoire arreté");
            
            }
            catch (Exception epe)
            {
                ArmoireV3.Entities.Log.add("Erreur d'insertion d'alerte " + epe.Message);
            }
            */
        }

        void App_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            if (evntpassed == false)
            {
                evntpassed = true;
                ArmoireV3.Entities.Log.add("============================== [Fermeture de l'application] ==============================");
                try
                {
                    DataManagerV3 data = new DataManagerV3();


                    data.insertAlert(AlerteType.ALERTE_ARRETAPPLI, 0, ArmoireV3.Properties.Settings.Default.NumArmoire, "Armoire arreté");

                }
                catch (Exception epe)
                {
                    ArmoireV3.Entities.Log.add("Erreur d'insertion d'alerte " + epe.Message);
                }
            }
        }
    }
}