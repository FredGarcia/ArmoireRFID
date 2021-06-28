using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Diagnostics;
using System.Windows;
using MySql.Data.MySqlClient;

using Renci.SshNet;
//using System.Windows.Media; // Exceptioon

namespace ArmoireV3.Entities
{
    class Synchro {
#if EDMX
        Worker sync = new Worker();
#else
        WorkerV3 workerV3 = new WorkerV3();
#endif
        static private bool isSynchronisingEpc = false;
        static private bool isSynchronisingLogEpc = false;
        static private SshClient client_ssh;


        /// <summary>
        /// test la connexion MySql avec la chaine donnée en paramêtre
        /// </summary>
        /// <param name="StringConnect"></param>
        /// <returns></returns>
        public static bool testConnexion(string StringConnect)
        {
            try
            {
                using (var con = new MySqlConnection(StringConnect))
                {
                    con.Open();
                    con.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                string message = "Erreur testConnexion(" + StringConnect + ") : " + ex.Message;

                bool memSettings = Properties.Settings.Default.WriteLog;
                Properties.Settings.Default.WriteLog = true;
                Log.add(message);
                Properties.Settings.Default.WriteLog = memSettings;


                if (Properties.Settings.Default.UseDBGMSG)
                    System.Windows.MessageBox.Show(message);

                return false;
            }
        }
        
        /// <summary>
        /// Test l'ouverture des connexions vers les bases de données
        /// </summary>
        public static bool testConnexion()
        {
            bool isArmoireDbOpen = false;
            bool isServerDbOpen = false;
            bool isMailDbOpen = false;

            try
            {
                isArmoireDbOpen = testConnexionLocale();
                isServerDbOpen = testConnexionServer();
                isMailDbOpen = testConnexionMail();
            }
            catch (Exception ex)
            {
                Log.add("Erreur testConnexion: " + ex.Message);
                if (Properties.Settings.Default.UseDBGMSG)
                    System.Windows.MessageBox.Show("Erreur testConnexion: " + ex.Message);
            }

            string message = String.Format("La connexion en local est {0}, la connexion du dedie est {1}, la connexion a la base sendmail est {2}", isArmoireDbOpen ? "ouverte" : "fermee", isServerDbOpen ? "ouverte" : "fermee", isMailDbOpen ? "ouverte" : "fermee");
            Log.add("testConnexion: " + message);

            return (isServerDbOpen && isMailDbOpen);
        }

        /// <summary>
        /// Test connexion pour la base du dédié
        /// </summary>
        /// <returns></returns>
        public static bool testConnexionLocale()
        {
            try
            {
                return testConnexion(Properties.Settings.Default.StringConnectLocale);
            }
            catch (Exception ex)
            {
                bool memSettings = Properties.Settings.Default.WriteLog;

                Properties.Settings.Default.WriteLog = true;
                Log.add("Erreur testConnexionLocale: " + ex.Message);
                Properties.Settings.Default.WriteLog = memSettings;


                if (Properties.Settings.Default.UseDBGMSG)
                    System.Windows.MessageBox.Show("Erreur testConnexionServer: " + ex.Message);

                return false;
            }
        }

        public static bool TryToOpenSSHTunnel(string distantSSHServer, int distantSSHPortNum, string sshloginName, string sshpassword, uint localPort, string hostToForwardTo, uint portToForwardTo)
        {
            
             
            try
            {
                client_ssh.Disconnect();
                client_ssh.Dispose();
            }
            catch (Exception ett)
            {
                Log.add("Erreur TryToCloseSSHClient: " + ett.Message);
            }
            try
            {
                client_ssh = new SshClient(distantSSHServer, distantSSHPortNum, sshloginName, sshpassword);
                client_ssh.Connect();

                //ForwardedPortLocal tunnel = new ForwardedPortLocal("127.0.0.1", 3307, "87.98.143.63", 3307);
                ForwardedPortLocal tunnel = new ForwardedPortLocal("127.0.0.1", 3307, hostToForwardTo, portToForwardTo);
                //var tunnel = new ForwardedPortRemote(localPort, hostToForwardTo, portToForwardTo);
                client_ssh.AddForwardedPort(tunnel);

                tunnel.Exception += delegate(object sender, Renci.SshNet.Common.ExceptionEventArgs eren)
                {
                    Console.WriteLine(eren.Exception.ToString());
                };

                tunnel.Start();
                /*
                var tunnel = client_ssh.AddForwardedPort<ForwardedPortLocal>("localhost", localPort, hostToForwardTo, portToForwardTo);
                
                tunnel.Exception += new EventHandler<ExceptionEventArgs>(this.OnTunnelError);
                tunnel.Start();
                 * */


                //yui.Connect();
                //ForwardedPortLocal forward = new ForwardedPortLocal("127.0.0.1", 3306, "87.98.143.63", 3307);
                //yui.AddForwardedPort(forward);
                //forward.Start();

                return true;
            }
            catch (Exception ex)
            {
                Log.add("Error connect SSH : " + ex.Message);
                if (Properties.Settings.Default.UseDBGMSG == true)
                    MessageBox.Show("Error connect SSH : " + ex.Message);
                return false;
            }
        }
        
       
        /// <summary>
        /// This method will be called when an error has occurred.
        /// </summary>
        /// <param name="sender">Typicall it's the SshClient instance.</param>
        /// <param name="e">Information about the exception.</param>
        public void OnTunnelError(object sender, Renci.SshNet.Common.ExceptionEventArgs e)
        {
            // An error must be handled.
            System.Diagnostics.Debugger.Break();
        }

        /// <summary>
        /// Test connexion pour la base du dédié
        /// </summary>
        /// <returns></returns>
        public static bool testConnexionServer()
        {
            try
            {
                using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectSynchro))
                {
                    con.Open();
                    con.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                // Second Chance
                try
                {
                    if (Properties.Settings.Default.SSHDestinataire == "OVH")
                    {
                        TryToOpenSSHTunnel("87.98.143.63", 2222, "serge", "78lo1B", 3307, "87.98.143.63", 3307);
                        using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectSynchro))
                        {
                            con.Open();
                            con.Close();
                            return true;
                        }
                    }
                    else if (Properties.Settings.Default.SSHDestinataire == "INITIAL")
                    {
                        TryToOpenSSHTunnel("synclocker.initial-services.fr.", 22, "coding", "ferivu5383?", 3307, "synclocker.initial-services.fr", 3306);
                        using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectSynchro))
                        {
                            con.Open();
                            con.Close();
                            return true;
                        }
                    }

                }
                catch (Exception eee)
                {
                    Log.add("Erreur testConnexionServer__: " + eee.Message);
                }
                bool memSettings = Properties.Settings.Default.WriteLog;

                Properties.Settings.Default.WriteLog = true;
                Log.add("Erreur testConnexionServer: " + ex.Message);
                Properties.Settings.Default.WriteLog = memSettings;


                if (Properties.Settings.Default.UseDBGMSG)
                    System.Windows.MessageBox.Show("Erreur testConnexionServer: " + ex.Message);

                return false;
            }
        }

        /// <summary>
        /// Test connexion pour la base alert
        /// </summary>
        /// <returns></returns>
        public static bool testConnexionMail()
        {
            try
            {
                using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectAlert))
                {
                    con.Open();
                    con.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.add("Erreur testConnexionMail: " + ex.Message);
                if (Properties.Settings.Default.UseDBGMSG)
                    System.Windows.MessageBox.Show("Erreur testConnexionMail: " + ex.Message);

                return false;
            }
        }


        /// <summary>
        /// Synchro des alerts vers la base sendmail
        /// </summary>
        /// <param name="init"></param>
        public void mailAlert(bool init)
        {
#if EDMX
                List<alert> readerAlert = sync.data.synchroAlert().ToList();
#else
            List<Alert> readerAlert = workerV3.data.synchroAlert().ToList();
#endif

                for (int i = 0; i < readerAlert.Count(); i++)
                {      
#if EDMX
                        List<user> user = sync.data.infoUser(readerAlert[i].User_ID).ToList();
#else
                    List<User> user = workerV3.data.infoUser(readerAlert[i].User_ID).ToList();
#endif

                        using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectAlert))
                        {
                            
                            MySqlCommand commande = con.CreateCommand();
                            string s = "Armoire";

                            //ajouter id
                            if (!init && user.Count() > 0)
                            {
                                commande.CommandText = "REPLACE INTO alert(`Id`,`Date_Creation`,`Alert_Type_Id`,`Message`,`Origin`,`Login`,`Prenom`,`Nom`,`Armoire_Name`,`Armoire_ID`,`Traiter`) VALUES ('" + readerAlert[i].Id + "','" + String.Format("{0:yyyy-MM-dd HH:mm:ss}", readerAlert[i].Date_Creation) + "','" + readerAlert[i].Alert_Type_Id + "','" + readerAlert[i].Message + "','" + user[0].Type + "','" + user[0].Login + "','" + user[0].Prenom + "','" + user[0].Nom + "','" + s + "','" + readerAlert[i].Armoire_ID.ToString() + "','" + readerAlert[i].Traiter + "')";
                            }
                            else
                            {
                                commande.CommandText = "REPLACE INTO alert(`Id`,`Date_Creation`,`Alert_Type_Id`,`Message`,`Origin`,`Login`,`Prenom`,`Nom`,`Armoire_Name`,`Armoire_ID`,`Traiter`) VALUES ('" + readerAlert[i].Id + "','" + String.Format("{0:yyyy-MM-dd HH:mm:ss}", readerAlert[i].Date_Creation) + "','" + readerAlert[i].Alert_Type_Id + "','" + readerAlert[i].Message + "','INIT','INIT','INIT','INIT','" + s + "','" + readerAlert[i].Armoire_ID + "','" + readerAlert[i].Traiter + "')";

                            }
                            try
                            {
                                con.Open();
                                commande.ExecuteNonQuery();
                                workerV3.data.updateAlert(readerAlert[i].Id);
                                con.Close();
                            }
                            catch (Exception e)
                            {
                                Log.add("Erreur mailAlert: " + e.Message);

                                if (Properties.Settings.Default.UseDBGMSG)
                                    System.Windows.MessageBox.Show("Erreur mailAlert: " + e.Message);
                            }
                        }
                }
                readerAlert.Clear();
        }

        /// <summary>
        /// Synchro des alerts vers le dédié
        /// </summary>

        public List<Alert> alert()
        {
#if EDMX
            List<alert> readerAlert = sync.data.synchroAlert().ToList();
#else
            List<Alert> readerAlert = workerV3.data.synchroAlert().ToList();
#endif
            for (int i = 0; i < readerAlert.Count; i++)  {
                using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectSynchro))
                {
                    MySqlCommand commande = con.CreateCommand();
                    commande.CommandText = "REPLACE INTO alert(`Id`,`Date_Creation`,`Alert_Type_Id`, `Message`,`User_ID`,`Armoire_ID`,`Traiter`) VALUES ('" + readerAlert[i].Id + "','" + readerAlert[i].Date_Creation.ToString("yyyy-MM-dd HH:mm:ss") + "','" + readerAlert[i].Alert_Type_Id + "','" + readerAlert[i].Message + "','" + readerAlert[i].User_ID + "','" + Properties.Settings.Default.NumArmoire.ToString() + "','" + readerAlert[i].Traiter + "')";
                    try
                    {
                        con.Open();
                        commande.ExecuteNonQuery();
                        con.Close();
                    }
                    catch (Exception e) {
                        bool memSettings = Properties.Settings.Default.WriteLog;
                        Properties.Settings.Default.WriteLog = true;
                        Log.add("Erreur alert : " + e.Message);
                        Properties.Settings.Default.WriteLog = memSettings;
                        if (Properties.Settings.Default.UseDBGMSG)
                            System.Windows.MessageBox.Show("Erreur alert: " + e.Message);
                    }
                }
            }
            return readerAlert;            
        }
        
        //Synchro des tag en alert vers sendMail
        public void mailtagAlert()
        {
#if EDMX
            List<tag_alert> readerAlert = sync.data.synchroTagAlert().ToList();
#else
            List<Alert> alert_nontraitee = workerV3.data.synchroAlert().ToList();
            List<Tag_Alert> readerAlert = workerV3.data.synchroTagAlert(alert_nontraitee).ToList();
#endif
            
            for (int i = 0; i < readerAlert.Count(); i++)
            {
                using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectAlert))
                {
                    MySqlCommand commande = con.CreateCommand();
#if EDMX
                    commande.CommandText = "REPLACE INTO tag_alert(`Id`,`Date_Creation`,`Alert_ID`,`Tag_Command`,`Tag_Retir`,`Tag_Intrus`,`Article_Type_Code`,`Taille`) VALUES ('" + readerAlert[i].Id + "','" + String.Format("{0:yyyy-MM-dd HH:mm:ss}", readerAlert[i].Date_Creation) + "','" + 
                        readerAlert[i].Alert_ID + "','" + 
                        readerAlert[i].Tag_Command + "','" + readerAlert[i].Tag_Retir + "','" + readerAlert[i].Tag_Intrus + "','" + readerAlert[i].Article_Type_Code + "','" + readerAlert[i].Taille + "')";
#else
                    commande.CommandText = "REPLACE INTO tag_alert(`Id`,`Date_Creation`,`Alert_ID`,`Tag_Command`,`Tag_Retir`,`Tag_Intrus`,`Article_Type_Code`,`Taille`) VALUES ('" + readerAlert[i].Id + "','" + String.Format("{0:yyyy-MM-dd HH:mm:ss}", readerAlert[i].Date_Creation) + "','" + readerAlert[i].Alert_Id + "','" + readerAlert[i].Tag_Command + "','" + readerAlert[i].Tag_Retir + "','" + readerAlert[i].Tag_Intrus + "','" + readerAlert[i].Article_Type_Code + "','" + readerAlert[i].Taille + "')";
#endif
                    try
                    {
                        con.Open();
                        commande.ExecuteNonQuery();
                        con.Close();
                    }
                    catch (Exception e)
                    {
                        Log.add("Erreur mailtagAlert: " + e.Message);

                        if (Properties.Settings.Default.UseDBGMSG)
                            System.Windows.MessageBox.Show("Erreur mailtagAlert: " + e.Message);
                    }
                }
            }
            readerAlert.Clear();
        }

        /// <summary>
        /// Synchro des tag en alert vers le dédié
        /// </summary>
        public void tagAlert(List<Alert> alert_nontraitee)
        {
#if EDMX
            List<tag_alert> readerAlert = sync.data.synchroTagAlert().ToList();
#else
            List<Tag_Alert> readerAlert = workerV3.data.synchroTagAlert(alert_nontraitee).ToList();
#endif
            for (int i = 0; i < readerAlert.Count(); i++)
            {
                using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectSynchro))
                {
                    MySqlCommand commande = con.CreateCommand();
                    commande.CommandText = "REPLACE INTO tag_alert(`Id`,`Date_Creation`,`Alert_ID`,`Tag_Command`,`Tag_Retir`,`Tag_Intrus`,`Article_Type_Code`,`Taille`) "+
                        "VALUES ('" + readerAlert[i].Id + "','" + String.Format("{0:yyyy-MM-dd HH:mm:ss}", readerAlert[i].Date_Creation) + "','" + 
#if EDMX
                        readerAlert[i].Alert_ID +  "','" +
#else
                        readerAlert[i].Alert_Id +  "','" +
#endif
                       readerAlert[i].Tag_Command + "','" + readerAlert[i].Tag_Retir + "','" + readerAlert[i].Tag_Intrus + "','" + readerAlert[i].Article_Type_Code + "','" + readerAlert[i].Taille + "')";
                    try
                    {
                        con.Open();
                        commande.ExecuteNonQuery();
                        con.Close();
                    }
                    catch (Exception e)
                    {
                        bool memSettings = Properties.Settings.Default.WriteLog;

                        Properties.Settings.Default.WriteLog = true;
                        Log.add("Erreur tagAlert: " + e.Message);
                        Properties.Settings.Default.WriteLog = memSettings;

                        if (Properties.Settings.Default.UseDBGMSG)
                            System.Windows.MessageBox.Show("Erreur tagAlert: " + e.Message);
                    }
                }                    
            }
            readerAlert.Clear();
        }

        /// <summary>
        /// Synchro des logepc vers le dédié
        /// </summary>
        public void logEPC()
        { 
            if (!isSynchronisingLogEpc)
            {
                isSynchronisingLogEpc = true;
                try
                {
#if EDMX
                    List<log_epc> readerLogEPC = sync.data.synchroLogEPC().ToList();
#else
                    List<Log_Epc> readerLogEPC = workerV3.data.synchroLogEPC().ToList();
#endif
                    for (int i = 0; i < readerLogEPC.Count; i++)
                    {
                        using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectSynchro))
                        {
                            MySqlCommand commande = con.CreateCommand();

                            try
                            {
                                workerV3.data.UpdateLogEpc(readerLogEPC[i].Id, 1);

                                commande.CommandText = "INSERT INTO `log_epc` (`Id_Local`, `Epc_Id`,`Date_Creation`,`Tag`, `Code_Barre`, `Taille`,`Cycle_Lavage_Count`,"+
                                    "`State`,`Last_User`,`Last_Reader`,`Last_Action`,`Last_Action_Date`,`Movement`,`Article_Type_ID`,`Case_ID`,`Armoire_ID`, `Synchronised`)"+
                                    " VALUES ('" + readerLogEPC[i].Id + "','" + readerLogEPC[i].Epc_Id + "','" + String.Format("{0:yyyy-MM-dd HH:mm:ss}", 
                                    readerLogEPC[i].Date_Creation) + "','" + readerLogEPC[i].Tag + "','" + readerLogEPC[i].Code_Barre + "','" + readerLogEPC[i].Taille + "','" + 
                                    readerLogEPC[i].Cycle_Lavage_Count + "','" + readerLogEPC[i].State + "','" + readerLogEPC[i].Last_User + "','" + 
                                    readerLogEPC[i].Last_Reader + "','" + readerLogEPC[i].Last_Action + "','" + String.Format("{0:yyyy-MM-dd HH:mm:ss}", 
                                    readerLogEPC[i].Last_Action_Date) + "','" + readerLogEPC[i].Movement + "','" + readerLogEPC[i].Article_Type_ID + "','" + 
                                    readerLogEPC[i].Case_ID + "','" + readerLogEPC[i].Armoire_ID.ToString() + "', 1)";
                                try
                                {
                                    con.Open();
                                    commande.ExecuteNonQuery();
                                    con.Close();
                                }
                                catch (Exception e)
                                {
                                    bool memSettings = Properties.Settings.Default.WriteLog;

                                    Properties.Settings.Default.WriteLog = true;
                                    Log.add("Erreur logEPC: " + e.Message);
                                    Properties.Settings.Default.WriteLog = memSettings;

                                    workerV3.data.UpdateLogEpc(readerLogEPC[i].Id, 0);
                                    if (Properties.Settings.Default.UseDBGMSG)
                                        System.Windows.MessageBox.Show("Erreur logEPC: " + e.Message);
                                }
                            }
                            catch (Exception e)
                            {
                                bool memSettings = Properties.Settings.Default.WriteLog;

                                Properties.Settings.Default.WriteLog = true;
                                Log.add("Erreur logEPC: " + e.Message);
                                Properties.Settings.Default.WriteLog = memSettings;

                                if (Properties.Settings.Default.UseDBGMSG)
                                    System.Windows.MessageBox.Show("Erreur logEPC: " + e.Message);
                            }
                        }
                    }
                    readerLogEPC.Clear();
                }
                finally
                {
                    isSynchronisingLogEpc = false;
                }
            }
            else
            {
                Log.add("Erreur: Les LogEPC sont déjà en cours de synchronisation");

                if (Properties.Settings.Default.UseDBGMSG)
                    System.Windows.MessageBox.Show("Erreur: Les LogEPC sont déjà en cours de synchronisation");

            }
        }

        /// <summary>
        /// Monte le contenu de l'armoire
        /// </summary>
        public void contentarm()
        {
#if EDMX
            List<content_arm> readerContArm = sync.data.synchroContent().ToList();
#else
            List<Content_Arm> readerContArm = workerV3.data.synchroContent().ToList();
#endif

            using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectSynchro))
            {
                MySqlCommand commande = con.CreateCommand();
                commande.CommandText = "DELETE FROM content_arm WHERE RFID_Reader=" + Properties.Settings.Default.NumArmoire;
                try
                {
                    con.Open();
                    commande.ExecuteNonQuery();
                    con.Close();
                }
                catch (Exception e)
                {
                    bool memSettings = Properties.Settings.Default.WriteLog;

                    Properties.Settings.Default.WriteLog = true;
                    Log.add("Erreur contentarm au DELETE: " + e.Message);
                    Properties.Settings.Default.WriteLog = memSettings;

                    if (Properties.Settings.Default.UseDBGMSG)
                        System.Windows.MessageBox.Show("Erreur contentarm: " + e.Message);
                }
            }
                
            for (int i = 0; i < readerContArm.Count; i++)
            {
                using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectSynchro))
                {
                    MySqlCommand commande = con.CreateCommand();
                    commande.CommandText = "INSERT INTO content_arm(`Id`,`Creation_Date`,`EPC`,`State`,`RFID_Reader`) "+
                        "VALUES ('" + readerContArm[i].Id + "','" + readerContArm[i].Creation_Date.ToString("yyyy-MM-dd hh:mm:ss") + "','" + 
                        readerContArm[i].Epc + "','" + readerContArm[i].State + "','" + readerContArm[i].RFID_Reader + "')";
                    try
                    {
                        con.Open();
                        commande.ExecuteNonQuery();
                        con.Close();
                    }
                    catch (Exception e)
                    {
                        bool memSettings = Properties.Settings.Default.WriteLog;

                        Properties.Settings.Default.WriteLog = true;
                        Log.add("Erreur contentarm à l'INSERT: " + e.Message);
                        Properties.Settings.Default.WriteLog = memSettings;

                        if (Properties.Settings.Default.UseDBGMSG)
                            System.Windows.MessageBox.Show("Erreur contentarm: " + e.Message);
                    }
                }
            }
                
            readerContArm.Clear();
        }

        /// <summary>
        /// Descend les type d'alertes
        /// </summary>
        public void alertType()
        {
            using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectSynchro))
            {
                MySqlCommand commande = con.CreateCommand();
                commande.CommandText = "SELECT * FROM alert_type ";
                try
                {
                    con.Open();
                    DbDataReader list = commande.ExecuteReader();
                    List<int> alType = workerV3.data.synchroAlertType().ToList();

                    while (list.Read())
                    {
                        if (!(alType.Contains(int.Parse(list["Id"].ToString()))))
                        {
                            workerV3.data.synchroInsertAlertType(int.Parse(list["Id"].ToString()), list["Type"].ToString(), list["Code"].ToString(), list["Description"].ToString(), list["Niveau"].ToString(), list["Contact"].ToString());
                        }
                    }
                    list.Close();
                    con.Close();
                }
                catch (Exception mex)
                {
                    bool memSettings = Properties.Settings.Default.WriteLog;

                    Properties.Settings.Default.WriteLog = true;
                    Log.add("Erreur alertType: " + mex.Message);
                    Properties.Settings.Default.WriteLog = memSettings;

                    if (Properties.Settings.Default.UseDBGMSG)
                        System.Windows.MessageBox.Show("Erreur alertType: " + mex.Message);

                }
            }
        }


        /// <summary>
        /// Descend la table de correspondance article - Taille
        /// </summary>
        public bool articleTaille()
        {
            bool reussi = false;
            using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectSynchro))
            {
                if (Properties.Settings.Default.BDDsynchV3)
                {
                MySqlCommand commande = con.CreateCommand();
                commande.CommandText = "SELECT * FROM article_taille ";
                try
                {
                    con.Open();
                    DbDataReader list = commande.ExecuteReader();
                    
                    // CREATE TABLE IF NOT EXISTS `article_taille` (
                    // `Article_Type_ID` int(8) NOT NULL COMMENT 'Id du type article correspondant au vêtement',
                    // `Taille` varchar(255) NOT NULL COMMENT 'Taille pour l''article donné',
                    // `Armoire` int(8) NOT NULL COMMENT 'Numérotation de l''armoire',
                    // `Vide` tinyint(1) NOT NULL COMMENT 'indique si le type d''article/taille est épuisé',
                    // PRIMARY KEY (`Article_Type_ID`,`Taille`,`Armoire`)
                    // ) ENGINE=MyISAM DEFAULT CHARSET=latin1;
#if EDMX
#else

                    // Liste pour suppression des entrées en trop
                    List<Article_Taille> artTail_locale = new List<Article_Taille>();
                    List<Article_Taille> artTail_centrale = new List<Article_Taille>();
                    artTail_locale = workerV3.data.getListArticleTaille();
                   /* List<int> arType = sync.data.synchroListArticletypeInArticleTaille().ToList();
                    foreach (int at in arType)
                    {
                                
                        List<string> arTai = sync.data.synchroListTailleInArticleTaille(at).ToList();
                        foreach( string tt in arTai)
                        {
                            List<int> arArm = sync.data.synchroListArmoiresInArticleTaille(at,tt).ToList();
                            foreach (int ar in arArm)
                            {
                                artTail_locale.Add(new Article_Taille(ar, tt, ar, false));
                            }
                        }                              
                    }*/
                    int i=0;
                    while (list.Read()) // Listes des entrées sur TwisterConnect
                    {
                        int arttyp = int.Parse(list["Article_Type_ID"].ToString());
                        string tail = list["Taille"].ToString();
                        int armoire = int.Parse(list["Armoire"].ToString());

                        Article_Taille tmp = new Article_Taille(arttyp, tail, armoire, false);

                        artTail_centrale.Add(tmp);
                        i++;
                    }
                        
                    // Recherche des nouvelles entrées sur TwisterConnect:
                    foreach (Article_Taille elemnt in artTail_centrale)
                    {
                        if (workerV3.data.AreKeysPresentInArticleTaille(elemnt.Article_Type_ID, elemnt.Taille, elemnt.Armoire) == false)
                        { // l'élement est absent sur la base locale
                            // Not Yet in table.                           
                            workerV3.data.synchroInsertArticletaille(elemnt.Article_Type_ID, elemnt.Taille, elemnt.Armoire);
                        }
                        else
                        { // L'élement est présent sur les deux bases 
                            // suppression de la liste temporaire des elements commun en locale
                            artTail_locale.Remove(artTail_locale.Where(c => c.Article_Type_ID == elemnt.Article_Type_ID && c.Taille == elemnt.Taille && c.Armoire == elemnt.Armoire).First());
                        }
                    }
                    foreach (Article_Taille elemnt in artTail_locale)
                    {// l'élement est absent sur la base centrale
                            // suppression des éléments en trop dans la base locale
                        workerV3.data.synchroDeleteArticletaille(elemnt.Article_Type_ID, elemnt.Taille, elemnt.Armoire);
                    
                    }
#endif
  
                    list.Close();
                    con.Close();
                    reussi = true;
                }
                catch (Exception e)
                {
                    Log.add("Erreur articleType : " + e.Message);
                }
                }
            }
            return reussi;
        }


        /// <summary>
        /// Descend le plan de chargement de/des armoires(s)
        /// </summary>
        /// 
        public bool acase()
        {
            bool reussi = false;
            using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectSynchro))
            {
                MySqlCommand commande = con.CreateCommand();
                commande.CommandText = "SELECT * FROM `case` ";
                try
                {
                    con.Open();
                    DbDataReader list = commande.ExecuteReader();
                    List<int> alType = workerV3.data.synchroCase().ToList();

                    while (list.Read())
                    {
                        if (alType.Contains(int.Parse(list["Id"].ToString())))
                        {
                            workerV3.data.synchroUpdateCase(int.Parse(list["Id"].ToString()), int.Parse(list["Bind_ID"].ToString()), list["Taille"].ToString(), int.Parse(list["Max_Item"].ToString()), int.Parse(list["Article_Type_Id"].ToString()), int.Parse(list["Armoire_ID"].ToString()));
                        }
                        else
                        {

                            workerV3.data.synchroInsertCase(int.Parse(list["Id"].ToString()), int.Parse(list["Bind_ID"].ToString()), list["Taille"].ToString(), DateTime.Parse(list["Date_Creation"].ToString()), int.Parse(list["Max_Item"].ToString()), int.Parse(list["Article_Type_Id"].ToString()), int.Parse(list["Armoire_ID"].ToString()));
                        }
                    }
                    list.Close();
                    con.Close();
                    reussi=true;
                }
                catch (Exception e)
                {
                    Log.add("Erreur acase : " + e.Message);
                }
            }
            return reussi;
        }

        /// <summary>
        /// Descend les infos de/des correspondances de tailles vers la base de l'armoire
        /// </summary>
        public void correspTaille()
        {
            using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectSynchro))
            {
                MySqlCommand commande = con.CreateCommand();
                commande.CommandText = "SELECT * FROM corresp_taille ";
                try
                {
                    con.Open();
                    DbDataReader list = commande.ExecuteReader();
#if EDMX
                    List<corresp_taille> CorrTail_locale = sync.data.synchroCorrespTaille().ToList();
#else
                    List<Corresp_Taille> CorrTail_locale = workerV3.data.synchroCorrespTaille().ToList();
#endif
 
                    List<Corresp_Taille> CorrTail_centrale = new List<Corresp_Taille>();
                    
                    int i = 0;
                    
                    while (list.Read()) // Listes des entrées sur TwisterConnect
                    {
                        //`corresp_taille` (
                        //  `Type-Taille` char(1) NOT NULL,
                        //  `Taille` varchar(255) NOT NULL,
                        //  `Classement_tailles` int(11) NOT NULL
                            string typtail = list["Type-Taille"].ToString();
                            string tail = list["Taille"].ToString();
                            int clasTail = int.Parse(list["Classement_tailles"].ToString());

                            Corresp_Taille tmp = new Corresp_Taille(typtail, tail, clasTail);

                            CorrTail_centrale.Add(tmp);
                            i++;
                        }
                    /*
                    while (list.Read())
                    {
                        if (alType.Where(x => x.Type_Taille == list["Type-Taille"].ToString() && x.Taille == list["Taille"].ToString()).Count() > 0)
                        {
                            sync.data.synchroUpdateCorrespTaille(list["Type-Taille"].ToString(), list["Taille"].ToString(), int.Parse(list["Classement_tailles"].ToString()));
                        }
                        else
                        {
                            sync.data.synchroInsertCorrespTaille(list["Type-Taille"].ToString(), list["Taille"].ToString(), int.Parse(list["Classement_tailles"].ToString()));
                        }
                    }*/
                                    
                        // Recherche des nouvelles entrées sur TwisterConnect:
                    foreach (Corresp_Taille elemnt in CorrTail_centrale)
                        {
                            if (workerV3.data.AreKeysPresentInCorrespTaille(elemnt.Type_Taille, elemnt.Taille, elemnt.Classement_Tailles) == false)
                            { // l'élement est absent sur la base locale
                                // Not Yet in table.                           
                                workerV3.data.synchroInsertCorrespTaille(elemnt.Type_Taille, elemnt.Taille, elemnt.Classement_Tailles);
                            }
                            else 
                            { // L'élement est présent sur les deux bases 
                                // suppression de la liste temporaire des elements commun en locale
                                CorrTail_locale.Remove(CorrTail_locale.Where(c => c.Type_Taille == elemnt.Type_Taille && c.Taille == elemnt.Taille && c.Classement_Tailles == elemnt.Classement_Tailles).First());
                            }
                        }
                    foreach (Corresp_Taille elemnt in CorrTail_locale)
                        {// l'élement est absent sur la base centrale
                            // suppression des éléments en trop dans la base locale
                            workerV3.data.synchroDeleteCorrespTaille(elemnt.Type_Taille, elemnt.Taille, elemnt.Classement_Tailles);

                        }

                    list.Close();
                    con.Close();
                }
                catch (Exception e)
                {
                    Log.add("Erreur trt corresptaille: " + e.Message);
                }
            }
            

  



        }
        
        /// <summary>
        /// descend les article_type vers l'armoire
        /// </summary>
        public bool articleType()
        {
            bool reussi = false;
            using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectSynchro))
            {
                MySqlCommand commande = con.CreateCommand();
                commande.CommandText = "SELECT * FROM article_type ";
                try
                {
                    con.Open();
                    DbDataReader list = commande.ExecuteReader();
                    List<int> alType = workerV3.data.synchroArticletype().ToList();

                    while (list.Read())
                    {
                        if (alType.Contains(int.Parse(list["Id"].ToString())))
                        {
                            if (Properties.Settings.Default.BDDsynchV3)
                                workerV3.data.synchroUpdateArticletype(int.Parse(list["Id"].ToString()), list["Code"].ToString(), list["Description"].ToString(), list["Couleur"].ToString(), list["Sexe"].ToString(), list["Photo"].ToString(), int.Parse(list["Active"].ToString()), list["Type_Taille"].ToString(), list["Description_longue"].ToString());
                            else
                                workerV3.data.synchroUpdateArticletype(int.Parse(list["Id"].ToString()), list["Code"].ToString(), list["Description"].ToString(), list["Couleur"].ToString(), list["Sexe"].ToString(), list["Photo"].ToString(), int.Parse(list["Active"].ToString()));
                        }
                        else
                        {
                            if(Properties.Settings.Default.BDDsynchV3)
                                workerV3.data.synchroInsertArticletype(int.Parse(list["Id"].ToString()), DateTime.Parse(list["Date_Creation"].ToString()), DateTime.Parse(list["Date_Modification"].ToString()), list["Code"].ToString(), list["Description"].ToString(), list["Couleur"].ToString(), list["Sexe"].ToString(), list["Photo"].ToString(), int.Parse(list["Active"].ToString()), list["Type_Taille"].ToString(), list["Description_longue"].ToString());
                            else
                                workerV3.data.synchroInsertArticletype(int.Parse(list["Id"].ToString()), DateTime.Parse(list["Date_Creation"].ToString()), DateTime.Parse(list["Date_Modification"].ToString()), list["Code"].ToString(), list["Description"].ToString(), list["Couleur"].ToString(), list["Sexe"].ToString(), list["Photo"].ToString(), int.Parse(list["Active"].ToString()));
                        }
                    }
                    list.Close();
                    con.Close();
                    reussi = true;
                }
                catch (Exception e)
                {
                    Log.add("Erreur articleType : " + e.Message);
                }
            }
            return reussi;
        }

        /// <summary>
        /// Monte les versions vers Twister
        /// </summary>
        public bool Version_Armoire()
        {
#if EDMX
            List<version> Vers = sync.data.synchroVersion().ToList();
#else

            List<Version_Armoire> VersList = workerV3.data.synchroVersion().ToList(); // Liste des enregistrements sur TwisterConnect
#endif          
            bool reussi = false;
           
            for (int i = 0; i < VersList.Count; i++)
            {
                using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectSynchro))
                {
                    MySqlCommand commande = con.CreateCommand();
                    commande.CommandText = "REPLACE INTO `version`(`Id`,`NomPlace`,`VersLog`, `VersMat`,`DateSynchro`) "+
                        " VALUES ('" + VersList[i].Id + "','" +  VersList[i].NomPlace + "','" +  VersList[i].VersLog +
                        "','" +  VersList[i].VersMat + "','" +  VersList[i].DateSynchro.ToString("yyyy-MM-dd HH:mm:ss") + "');";
                    try
                    {
                        con.Open();
                        commande.ExecuteNonQuery();
                        con.Close();
                        reussi = true;
                    }
                    catch (Exception e)
                    {

                        bool memSettings = Properties.Settings.Default.WriteLog;

                        Properties.Settings.Default.WriteLog = true;
                        Log.add("Erreur vers : " + e.Message);
                        Properties.Settings.Default.WriteLog = memSettings;

                        if (Properties.Settings.Default.UseDBGMSG)
                            System.Windows.MessageBox.Show("Erreur  Sync Version : " + e.Message);
                    }
                }
            }
            VersList.Clear();   
            return reussi;
        }


        /// <summary>
        /// Descente des nouveaux utilisateurs
        /// </summary>
        public void user()
        {
            using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectSynchro))
            {
                MySqlCommand commande = con.CreateCommand();
                commande.CommandText = "SELECT * FROM user ";
                try
                {
                    con.Open();
                    DbDataReader list = commande.ExecuteReader();
                    List<int> alType = workerV3.data.synchroUser().ToList();

                    while (list.Read())
                    {
                        if (alType.Contains(int.Parse(list["Id"].ToString())))
                        {
                            if(Properties.Settings.Default.BDDsynchV3)
                                workerV3.data.synchroUpdateUser(int.Parse(list["Id"].ToString()), list["Login"].ToString(), list["Password"].ToString(), list["Type"].ToString(), list["Nom"].ToString(), list["Prenom"].ToString(), list["Sexe"].ToString(), list["Taille"].ToString(), int.Parse(list["Groupe"].ToString()), int.Parse(list["Department"].ToString()), list["Photo"].ToString(), int.Parse(list["Active"].ToString()), DateTime.Parse(list["End_of_Validity"].ToString()), list["Wearer_Code"].ToString());
                            else
                                workerV3.data.synchroUpdateUser(int.Parse(list["Id"].ToString()), list["Login"].ToString(), list["Password"].ToString(), list["Type"].ToString(), list["Nom"].ToString(), list["Prenom"].ToString(), list["Sexe"].ToString(), list["Taille"].ToString(), int.Parse(list["Groupe"].ToString()), int.Parse(list["Department"].ToString()), list["Photo"].ToString(), int.Parse(list["Active"].ToString()));
                        }
                        else
                        {
                            if(Properties.Settings.Default.BDDsynchV3)
                                workerV3.data.synchroInsertUser(int.Parse(list["Id"].ToString()), DateTime.Parse(list["Date_Creation"].ToString()), DateTime.Parse(list["Date_Modification"].ToString()), list["Login"].ToString(), list["Password"].ToString(), list["Type"].ToString(), list["Nom"].ToString(), list["Prenom"].ToString(), list["Sexe"].ToString(), list["Taille"].ToString(), int.Parse(list["Groupe"].ToString()), int.Parse(list["Department"].ToString()), list["Photo"].ToString(), DateTime.Parse(list["Last_Connection"].ToString()), int.Parse(list["Active"].ToString()), DateTime.Parse(list["End_of_Validity"].ToString()), list["Wearer_Code"].ToString());
                            else
                                workerV3.data.synchroInsertUser(int.Parse(list["Id"].ToString()), DateTime.Parse(list["Date_Creation"].ToString()), DateTime.Parse(list["Date_Modification"].ToString()), list["Login"].ToString(), list["Password"].ToString(), list["Type"].ToString(), list["Nom"].ToString(), list["Prenom"].ToString(), list["Sexe"].ToString(), list["Taille"].ToString(), int.Parse(list["Groupe"].ToString()), int.Parse(list["Department"].ToString()), list["Photo"].ToString(), DateTime.Parse(list["Last_Connection"].ToString()), int.Parse(list["Active"].ToString()));
                        }
                    }
                    list.Close();
                    con.Close();
                }
                catch (Exception e)
                {
                    Log.add("Erreur user : " + e.Message);
                }
            }
        }

        private int synchroUpdateUserART_BddCentrale(DateTime datmodlocale, int credrestlocale, int idcentrale)
        {
            int n = 0;
            using (var con2 = new MySqlConnection(Properties.Settings.Default.StringConnectSynchro))
            {
                MySqlCommand upda_commande = con2.CreateCommand();

                upda_commande.CommandText = "UPDATE `user_article` SET `Date_Modification`= '" + String.Format("{0:yyyy-MM-dd HH:mm:ss}", datmodlocale) + "', `Credit_Restant` ='" + credrestlocale.ToString() + "'  WHERE `Id`= '" + idcentrale.ToString() + "';";
                try
                {
                    con2.Open();
                   n = upda_commande.ExecuteNonQuery();
                    Log.add(upda_commande.CommandText);
                    con2.Close();
                }
                catch (Exception e)
                {
                    bool memSettings = Properties.Settings.Default.WriteLog;

                    Properties.Settings.Default.WriteLog = true;
                    Log.add("Erreur synchroUpdateUserART_BddCentrale1: " + e.Message);
                    Properties.Settings.Default.WriteLog = memSettings;

                    if (Properties.Settings.Default.UseDBGMSG)
                        System.Windows.MessageBox.Show("Erreur synchroUpdateUserART_BddCentrale2: " + e.Message);
                    return n;
                }
            }
            return n;
        }

        private int synchroUpdateUserART_BddCentraleAvecMiseA0Variation(DateTime datmodlocale, int credrestlocale, int idcentrale)
        {
            int n = 0;
            using (var con2 = new MySqlConnection(Properties.Settings.Default.StringConnectSynchro))
            {
                MySqlCommand upda_commande = con2.CreateCommand();
                upda_commande.CommandText = "UPDATE `user_article` SET `Date_Modification`= '" + String.Format("{0:yyyy-MM-dd HH:mm:ss}", datmodlocale) + "', `Credit_Restant` ='" + credrestlocale.ToString() + "' , Variation_Credit_Restant=0  WHERE `Id`= '" + idcentrale.ToString() + "';";
                try
                {
                    con2.Open();
                    n= upda_commande.ExecuteNonQuery();
                    Log.add(upda_commande.CommandText);
                    con2.Close();
                }
                catch (Exception e)
                {
                    bool memSettings = Properties.Settings.Default.WriteLog;

                    Properties.Settings.Default.WriteLog = true;
                    Log.add("Erreur synchroUpdateUserART_BddCentraleAvecMiseA0Variation: " + e.Message);
                    Properties.Settings.Default.WriteLog = memSettings;

                    if (Properties.Settings.Default.UseDBGMSG)
                        System.Windows.MessageBox.Show("Erreur synchroUpdateUserART_BddCentraleAvecMiseA0Variation: " + e.Message);                  
                }
                return n;
            }
        }

        /// <summary>
        /// Synchronisation des crédits dans les 2 sens
        /// </summary>
        public void userart()
        {
            int n = 0;
#if EDMX
            sync.data.dbContext.Refresh(System.Data.Objects.RefreshMode.StoreWins, sync.data.dbContext.user_article);
            List<user_article> dat = new List<user_article>();
#else
            List<User_Article> usartlocal = new List<User_Article>();
#endif
            using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectSynchro))
            {
                MySqlCommand commande = con.CreateCommand();
                commande.CommandText = "SELECT * FROM user_article ";
                try
                {
                    con.Open();
                    DbDataReader list = commande.ExecuteReader();
                    List<int> alType = workerV3.data.synchroUserArtGetIds().ToList();


                    while (list.Read())
                    {
                        int id_BDDCentrale = int.Parse(list["Id"].ToString());
                        int usid_BDDCentrale = int.Parse(list["User_Id"].ToString());
                        int cred_BDDCentrale = int.Parse(list["Credit"].ToString());
                        int credre_BDDCentrale = int.Parse(list["Credit_Restant"].ToString());
                        DateTime datcre_BDDCentrale = (DateTime)list["Date_Creation"];
                        DateTime datmod_BDDCentrale = (DateTime)list["Date_Modification"];
                        string tail_BDDCentrale = list["Taille"].ToString();
                        int varcredre_BDDCentrale = int.Parse(list["Variation_Credit_Restant"].ToString());
                        int usart_BDDCentrale = int.Parse(list["Article_Type_Id"].ToString());

                        //Si l'entrée est déja dans la base
                        if (workerV3.data.userart4userid(id_BDDCentrale, usid_BDDCentrale))  // Vérification que la base locale contient le userart qui sera demandé par la suite (qui à le i
                        {
                            usartlocal = workerV3.data.synchroUserArtGetListForUser(usid_BDDCentrale).ToList(); // Selection dans la base locale de la liste des Id pour l'utilisateur en cours
                            if (usartlocal == null)
                            {
                                Log.add("Erreur userart : usartlocal pour usid_BDDCentrale = " + usid_BDDCentrale.ToString());
                            }

                            
                            int usid_BDDLocale = usartlocal.Where(m => m.Id == id_BDDCentrale).FirstOrDefault().User_Id;
                            int cred_BDDLocale = usartlocal.Where(m => m.Id == id_BDDCentrale).FirstOrDefault().Credit;
                            int credre_BDDLocale = usartlocal.Where(m => m.Id == id_BDDCentrale).FirstOrDefault().Credit_Restant;
                            DateTime datcre_BDDLocale = usartlocal.Where(m => m.Id == id_BDDCentrale).FirstOrDefault().Date_Creation;
                            DateTime datmod_BDDLocale = usartlocal.Where(m => m.Id == id_BDDCentrale).FirstOrDefault().Date_Modification;
                            string tail_BDDLocale = usartlocal.Where(m => m.Id == id_BDDCentrale).FirstOrDefault().Taille;
                            if (tail_BDDLocale == null)
                            {
                                Log.add("Erreur userart : tail_BDDLocale=null pour id = " + id_BDDCentrale.ToString());
                            }

                            //Si la modif dans sur l'armoire est plus recente que sur le serveur
                            if (datmod_BDDLocale > datmod_BDDCentrale)
                            {
                                //Si les credits sont diffrérents entre l'armoire et le serveur
                                if (cred_BDDLocale != cred_BDDCentrale)
                                {
                                    int res = cred_BDDCentrale - cred_BDDLocale;
                                    n += creditdifferentUserArt(list, usartlocal.Where(m => m.Id == id_BDDCentrale).FirstOrDefault(), res);
                                }
                                else
                                {
                                    workerV3.data.synchroUpdateUserART(id_BDDCentrale, datmod_BDDLocale, tail_BDDCentrale, cred_BDDCentrale, credre_BDDLocale);
                                    n += synchroUpdateUserART_BddCentrale(datmod_BDDLocale, credre_BDDLocale, id_BDDCentrale);
                                }
                            }
                            else
                            {
               
                                //Modification prise en compte perte d'un article
                                if (varcredre_BDDCentrale != 0)
                                {
                                    //int variat = varcredre_BDDCentrale;
                                    workerV3.data.synchroUpdateUserART(id_BDDCentrale, datmod_BDDCentrale, tail_BDDCentrale, cred_BDDCentrale, (credre_BDDLocale + varcredre_BDDCentrale));
                                    n += synchroUpdateUserART_BddCentraleAvecMiseA0Variation(datmod_BDDLocale, (credre_BDDLocale + varcredre_BDDCentrale), id_BDDCentrale);                                    
                                }
                                else if (tail_BDDLocale != tail_BDDCentrale)
                                {
                                    try
                                    {
                                        workerV3.data.synchroUpdateUserART(id_BDDCentrale, datmod_BDDCentrale, tail_BDDCentrale, cred_BDDLocale, credre_BDDLocale);
                                    }
                                    catch (Exception e)
                                    {
                                        bool memSettings = Properties.Settings.Default.WriteLog;

                                        Properties.Settings.Default.WriteLog = true;
                                        Log.add("Erreur userart_1: " + e.Message);
                                        Properties.Settings.Default.WriteLog = memSettings;

                                        if (Properties.Settings.Default.UseDBGMSG)
                                            System.Windows.MessageBox.Show("Erreur userart_2: " + e.Message);
                                    }
                                }

                                if (cred_BDDLocale != cred_BDDCentrale) 
                                //    if ((cred_BDDLocale != cred_BDDCentrale) || (credre_BDDLocale != credre_BDDCentrale))
                                    {
                                    int res = cred_BDDCentrale - cred_BDDLocale;
                                    creditdifferentUserArt(list, usartlocal.Where(m => m.Id == id_BDDCentrale).FirstOrDefault(), res);
                                    //creditdifferentUserArt(list, usartlocal, res);
                                }
                            }

                        }
                        //Insert une nouvelle ligne
                        else
                        {
                            try
                            {
                                workerV3.data.synchroInsertUserArt(id_BDDCentrale, datcre_BDDCentrale, datmod_BDDCentrale, tail_BDDCentrale, cred_BDDCentrale, credre_BDDCentrale, usid_BDDCentrale, usart_BDDCentrale);
                            }
                            catch (Exception e)
                            {
                                bool memSettings = Properties.Settings.Default.WriteLog;

                                Properties.Settings.Default.WriteLog = true;
                                Log.add("Erreur userart_3: " + e.Message);
                                Properties.Settings.Default.WriteLog = memSettings;

                                if (Properties.Settings.Default.UseDBGMSG)
                                    System.Windows.MessageBox.Show("Erreur userart_4: " + e.Message);
                            }
                        }                        
                    }
                    list.Close();
                    con.Close();
                }
                catch (Exception ex)
                {
                    Log.add("Erreur userart_5: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// utilisé dans userart
        /// </summary>
        /// 
#if EDMX
        void creditdifferentUserArt(DbDataReader list, List<user_article> dat, int res)
#else
        int creditdifferentUserArt(DbDataReader list, User_Article dat, int res)
#endif
        {
            int n = 0;
            workerV3.data.synchroUpdateUserART(int.Parse(list["Id"].ToString()), dat.Date_Modification, list["Taille"].ToString(), int.Parse(list["Credit"].ToString()), (dat.Credit_Restant + res));

            using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectSynchro))
            {
                MySqlCommand upda_commande = con.CreateCommand();
                upda_commande.CommandText = "UPDATE user_article SET Date_Modification= " + "'" + String.Format("{0:yyyy-MM-dd HH:mm:ss}", dat.Date_Modification) + "'" + ", Credit_Restant=" + "'" + (dat.Credit_Restant + res) + "'" + "  WHERE Id= " + "'" + int.Parse(list["Id"].ToString()) + "'" + "    ";
                try
                {
                    con.Open();
                    n  = upda_commande.ExecuteNonQuery();
                    Log.add(upda_commande.CommandText);
                    con.Close();
                }
                catch (Exception e)
                {
                    bool memSettings = Properties.Settings.Default.WriteLog;

                    Properties.Settings.Default.WriteLog = true;
                    Log.add("Erreur creditdifferentUserArt: " + e.Message);
                    Properties.Settings.Default.WriteLog = memSettings;

                    if (Properties.Settings.Default.UseDBGMSG)
                        System.Windows.MessageBox.Show("Erreur creditdifferentUserArt: " + e.Message);
                    return n;
                }
            }
            return n;
        }

        /// <summary>
        /// Synchronisation de la table EPC dans les 2 sens
        /// </summary>
        public void epc()
        {
            Log.add("sync epc début");
            DateTime debut = DateTime.Now;
            if (!isSynchronisingEpc)
            {
                isSynchronisingEpc = true;
                int nbRow = 0;
                try
                {
#if EDMX
                    try
                    {
                        sync.data.dbContext.Refresh(System.Data.Objects.RefreshMode.StoreWins, sync.data.dbContext.epc);
                    }
                    catch (Exception e)
                    {
                        bool memSettings = Properties.Settings.Default.WriteLog;

                        Properties.Settings.Default.WriteLog = true;
                        Log.add("Erreur epc: " + e.Message);
                        Properties.Settings.Default.WriteLog = memSettings;

                        if (Properties.Settings.Default.UseDBGMSG)
                        {
                            string str = "Erreur refresh context: " + e.Message;
                            if (e.InnerException != null) str += "\n" + e.InnerException.Message;
                            MessageBox.Show(str);
                        }
                    }
#endif
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectSynchro))
                    {
                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = "SELECT * FROM epc ";
                        try
                        {
                            con.Open();
                            DbDataReader list = commande.ExecuteReader();// contient toutes les lignes de la table epc sur le SERVER

                            List<int> alType = workerV3.data.synchroEPC().ToList();// liste de tous les ID en LOCAL
#if EDMX
                            List<epc> dat = new List<epc>();
#else
                            List<Epc> dat = new List<Epc>();
#endif
                            if ((list != null) && (list.HasRows))
                            {
                                while (list.Read())// pour chaque ligne de l table epc
                                {
                                    nbRow++;//
                                    dat = workerV3.data.synchroEPCDate(int.Parse(list["Id"].ToString())).ToList(); // retourne un objet epc en fonction d'un id recuperé sur le SERVER
                                    if (alType.Contains(int.Parse(list["Id"].ToString())))// si la liste des epc en LOCAL contiens l'id de l'enregistrement SERVER qu'on est en train lire
                                    {
                                        if (int.Parse(list["State"].ToString()) == 2 && ((dat[0].State == EtatArticle.SORTI) || (dat[0].State == EtatArticle.SORTI_TailleSup)) )// si l'epc est perdu sur le SERVER et sorti en LOCAL
                                        {
                                            if (Properties.Settings.Default.BDDsynchV3)
                                            {
                                                workerV3.data.updateEpc(2, list["Tag"].ToString(), list["Last_User"].ToString(), Properties.Settings.Default.NumArmoire, int.Parse(list["Actif"].ToString()));// on update l'etat de l'epc en LOCAL => 2 (perdu)
                                            }
                                            else
                                            {
                                                workerV3.data.updateEpc(2, list["Tag"].ToString(), list["Last_User"].ToString(), Properties.Settings.Default.NumArmoire);// on update l'etat de l'epc en LOCAL => 2 (perdu)
                                            }

                                            try
                                            {
#if EDMX
                                                sync.data.dbContext.Refresh(System.Data.Objects.RefreshMode.StoreWins, sync.data.dbContext.epc);
#endif
                                            }
                                            catch (Exception e)
                                            {
                                                bool memSettings = Properties.Settings.Default.WriteLog;

                                                Properties.Settings.Default.WriteLog = true;
                                                Log.add("Erreur refresh epc: " + e.Message);
                                                Properties.Settings.Default.WriteLog = memSettings;

                                                if (Properties.Settings.Default.UseDBGMSG)
                                                {
                                                    string str = "Erreur refresh context: " + e.Message;
                                                    if (e.InnerException != null) str += "\n" + e.InnerException.Message;
                                                    MessageBox.Show(str);
                                                }
                                            }
                                        }
                                        /* on update l'epc du SERVER avec les valeurs de l'epc en LOCAL */
                                        using (var con2 = new MySqlConnection(Properties.Settings.Default.StringConnectSynchro))
                                        {
                                            MySqlCommand upda_commande = con2.CreateCommand();
                                            
                                            if(Properties.Settings.Default.BDDsynchV3)
                                            {
                                                upda_commande.CommandText = "UPDATE epc SET Date_Modification='" + String.Format("{0:yyyy-MM-dd HH:mm:ss}", dat[0].Date_Modification) + "'"
                                                                                                            + ", Taille='" + dat[0].Taille.ToString() + "'"
                                                                                                            + ", Cycle_Lavage_Count=" + dat[0].Cycle_Lavage_Count.ToString()
                                                                                                            + ", State=" + dat[0].State.ToString()
                                                                                                            + ", Last_User='" + dat[0].Last_User.ToString() + "'"
                                                                                                            + ", Last_Reader='" + dat[0].Last_Reader.ToString() + "'"
                                                                                                            + ", Last_Action_Date='" + String.Format("{0:yyyy-MM-dd HH:mm:ss}", dat[0].Date_Modification) + "'"
                                                                                                            + ", Movement=" + dat[0].Movement.ToString()
                                                                                                            + ", Article_Type_ID=" + dat[0].Article_Type_ID.ToString()
                                                                                                            + ", Last_Action='" + dat[0].Last_Action.ToString() + "'"
                                                                                                            + ", Armoire_ID=" + Properties.Settings.Default.NumArmoire.ToString()
                                                                                                            + ", Actif=" + dat[0].Actif.ToString()
                                                                                                            + "  WHERE Id= " + list["Id"].ToString();
                                            }
                                            else
                                            {
                                                upda_commande.CommandText = "UPDATE epc SET Date_Modification='" + String.Format("{0:yyyy-MM-dd HH:mm:ss}", dat[0].Date_Modification) + "'"
                                                                                                            + ", Taille='" + dat[0].Taille.ToString() + "'"
                                                                                                            + ", Cycle_Lavage_Count=" + dat[0].Cycle_Lavage_Count.ToString()
                                                                                                            + ", State=" + dat[0].State.ToString()
                                                                                                            + ", Last_User='" + dat[0].Last_User.ToString() + "'"
                                                                                                            + ", Last_Reader='" + dat[0].Last_Reader.ToString() + "'"
                                                                                                            + ", Last_Action_Date='" + String.Format("{0:yyyy-MM-dd HH:mm:ss}", dat[0].Date_Modification) + "'"
                                                                                                            + ", Movement=" + dat[0].Movement.ToString()
                                                                                                            + ", Article_Type_ID=" + dat[0].Article_Type_ID.ToString()
                                                                                                            + ", Last_Action='" + dat[0].Last_Action.ToString() + "'"
                                                                                                            + ", Armoire_ID=" + Properties.Settings.Default.NumArmoire.ToString()
                                                                                                            + "  WHERE Id= " + list["Id"].ToString();
                                            }
                                            //Log.add(upda_commande.CommandText);
                                            TimeSpan tempsecoule;
                                            DateTime starttime = DateTime.Now;
                                            try
                                            {                                          
                                                con2.Open();
                                                //upda_commande.CommandTimeout = 10;
                                                starttime = DateTime.Now;
                                                //System.Threading.Thread.Sleep(35000);
                                                upda_commande.ExecuteNonQuery();
                                                tempsecoule = DateTime.Now.Subtract(starttime);
                                                con2.Close();
                                            }
                                            catch (Exception e)
                                            {
                                                bool memSettings = Properties.Settings.Default.WriteLog;

                                                tempsecoule = DateTime.Now.Subtract(starttime);
                                                Properties.Settings.Default.WriteLog = true;
                                                Log.add("Erreur update epc " + int.Parse(list["Id"].ToString()) + " ("+tempsecoule.ToString()+": " + e.Message + "(Requête: " + upda_commande.CommandText + ")");
                                                Log.add("nbRow = " + nbRow);
                                                Properties.Settings.Default.WriteLog = memSettings;

                                                if (Properties.Settings.Default.UseDBGMSG)
                                                    System.Windows.MessageBox.Show("Erreur epc: " + e.Message);
                                            }
                                        }
                                    }
                                    else /* cas ou l'id sur le SERVER n'existe pas en LOCAL*/
                                    {
                                        /* on insert l'epc du SERVER */
                                        try
                                        {
#if EDMX
#else
                                            if (Properties.Settings.Default.BDDsynchV3)
                                            {
                                                workerV3.data.synchroInsertEPC(int.Parse(list["Id"].ToString()), DateTime.Parse(list["Date_Creation"].ToString()), DateTime.Parse(list["Date_Modification"].ToString()), list["Tag"].ToString(), list["Code_Barre"].ToString(), list["Taille"].ToString(), "1", int.Parse(list["Cycle_Lavage_Count"].ToString()), int.Parse(list["State"].ToString()), list["Last_User"].ToString(), list["Last_Reader"].ToString(), list["Last_Action"].ToString(), DateTime.Parse(list["Last_Action_Date"].ToString()), int.Parse(list["Movement"].ToString()), int.Parse(list["Article_Type_ID"].ToString()), 0, Properties.Settings.Default.NumArmoire, int.Parse(list["Actif"].ToString()));
                                            }
                                            else
#endif
                                            {
                                                workerV3.data.synchroInsertEPC(int.Parse(list["Id"].ToString()), DateTime.Parse(list["Date_Creation"].ToString()), DateTime.Parse(list["Date_Modification"].ToString()), list["Tag"].ToString(), list["Code_Barre"].ToString(), list["Taille"].ToString(), "1", int.Parse(list["Cycle_Lavage_Count"].ToString()), int.Parse(list["State"].ToString()), list["Last_User"].ToString(), list["Last_Reader"].ToString(), list["Last_Action"].ToString(), DateTime.Parse(list["Last_Action_Date"].ToString()), int.Parse(list["Movement"].ToString()), int.Parse(list["Article_Type_ID"].ToString()), 0, Properties.Settings.Default.NumArmoire);//int.Parse(list["Armoire_ID"].ToString()));
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            bool memSettings = Properties.Settings.Default.WriteLog;

                                            Properties.Settings.Default.WriteLog = true;
                                            Log.add("Erreur insert epc: " + e.Message);
                                            Properties.Settings.Default.WriteLog = memSettings;

                                            if (Properties.Settings.Default.UseDBGMSG)
                                                System.Windows.MessageBox.Show("Erreur epc: " + e.Message);
                                        }
                                    }
                                }
                                 
                                list.Close();
                            }
                            con.Close();
                        }
                        catch (Exception e)
                        {
                            Log.add("Erreur epc: " + e.Message);
                        }
                    }
                }
                finally
                {
                    isSynchronisingEpc = false;
                    Log.add(nbRow + " epc traités");
                }
            }
            else
            {
                Log.add("Erreur: Les EPC sont déjà en cours de synchronisation");
            }
            TimeSpan ms = DateTime.Now.Subtract(debut);
            reportCenter("Success", "Syn epc (" + Math.Ceiling(ms.TotalMilliseconds).ToString() + " ms)", 1);
        }

        /// <summary>
        /// epcPerdus
        /// </summary>
        public void epcPerdus()
        {
#if EDMX
            List<epc> listEpcLost = getLostEpc();
            foreach (epc epcLost in listEpcLost)
#else
            List<Epc> listEpcLost = getLostEpc();
            foreach (Epc epcLost in listEpcLost)
#endif
            {
#if EDMX
                epc epcLocal = sync.data.getEpc(epcLost.Tag);
#else
                Epc epcLocal = workerV3.data.getEpc(epcLost.Tag);
#endif
                if (epcLost.Date_Modification > epcLocal.Date_Modification)
                {
                    if(Properties.Settings.Default.BDDsynchV3)
                        workerV3.data.updateEpc(EtatArticle.PERDU, epcLost.Tag, epcLost.Last_User, int.Parse(epcLost.Last_Reader), epcLost.Actif);
                    else
                        workerV3.data.updateEpc(EtatArticle.PERDU, epcLost.Tag, epcLost.Last_User, int.Parse(epcLost.Last_Reader));
                }
            }
        }

        /// <summary>
        /// getLostEpc
        /// </summary>
        /// <returns></returns>
#if EDMX
        public List<epc> getLostEpc()
        {
            List<epc> listEpc = new List<epc>();
#else
        public List<Epc> getLostEpc()
        {
            List<Epc> listEpc = new List<Epc>();
#endif
            
            using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectSynchro))
            {
                MySqlCommand commande = con.CreateCommand();
                commande.CommandText = "SELECT * FROM epc WHERE State = 2";

                try
                {
                    con.Open();
                    DbDataReader list = commande.ExecuteReader();
                    while (list.Read())
                    {
#if EDMX
                        epc e = new epc();
#else
                        Epc e = new Epc();
#endif
                        e.Armoire_ID = int.Parse(list["Armoire_ID"].ToString());
                        e.Article_Type_ID = int.Parse(list["Article_Type_ID"].ToString());
                        e.Case_ID = int.Parse(list["Case_ID"].ToString());
                        e.Code_Barre = list["Code_Barre"].ToString();
                        e.Cycle_Lavage_Count = int.Parse(list["Cycle_Lavage_Count"].ToString());
                        e.Date_Creation = DateTime.Parse(list["Date_Creation"].ToString());
                        e.Date_Modification = DateTime.Parse(list["Date_Modification"].ToString());
                        e.Id = int.Parse(list["Id"].ToString());
                        e.Last_Action = list["Last_Action"].ToString();
                        e.Last_Action_Date = DateTime.Parse(list["Last_Action_Date"].ToString());
                        e.Last_Reader = list["Last_Reader"].ToString();
                        e.Last_User = list["Last_User"].ToString();
                        e.Movement = int.Parse(list["Movement"].ToString());
                        e.State = int.Parse(list["State"].ToString());
                        e.Tag = list["Tag"].ToString();
                        e.Taille = list["Taille"].ToString();
                        if(Properties.Settings.Default.BDDsynchV3)
                            e.Actif = int.Parse(list["Actif"].ToString());
                        else
                            e.Actif = 1;

                        listEpc.Add(e);
                    }
                    list.Close();
                    con.Close();
                }
                catch (Exception e)
                {
                    bool memSettings = Properties.Settings.Default.WriteLog;

                    Properties.Settings.Default.WriteLog = true;
                    Log.add("Erreur getLostEpc: " + e.Message);
                    Properties.Settings.Default.WriteLog = memSettings;

                    if (Properties.Settings.Default.UseDBGMSG)
                    {
                        MessageBox.Show("Erreur getLostEpc: " + e.Message);
                    }
                }
            }
            
            return listEpc;
        }

        /// <summary>
        /// reportCenter
        /// </summary>
        public void reportCenter(string state, string details, int sysid)
        {
            if (state == "Success" || state == "Failure")
            {
                string date = String.Format("{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now);
                using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectSynchro))
                {
                    MySqlCommand upda_commande = con.CreateCommand();
                    upda_commande.CommandText = "UPDATE _heartbeat SET HBDATE= " + "'" + date + "'" + ", SYNCHRODATE=" + "'" + date + "'" + ",STATE=" + "'" + state + "'" + "  ,DETAILS= " + "'" + details + "'" + " WHERE SYSID=" + sysid;
                    try
                    {
                        con.Open();
                        upda_commande.ExecuteNonQuery();
                        Log.add("Update heartbeat (success): " + details);
                        con.Close();
                    }
                    catch (Exception e)
                    {
                        bool memSettings = Properties.Settings.Default.WriteLog;

                        Properties.Settings.Default.WriteLog = true;
                        Log.add("Update heartbeat (fail): " + details + " (" + e.Message + ")");
                        Properties.Settings.Default.WriteLog = memSettings;

                        if (Properties.Settings.Default.UseDBGMSG)
                            System.Windows.MessageBox.Show("Erreur reportCenter: " + e.Message);
                    }
                }
            }
            else
            {
                Log.add("Erreur: Cannot update heartbeat");
                if (Properties.Settings.Default.UseDBGMSG)
                    System.Windows.MessageBox.Show("Erreur: Cannot update heartbeat");
            }
        }
    }
}
