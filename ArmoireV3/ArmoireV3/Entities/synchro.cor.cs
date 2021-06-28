using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data.Common;
using System.Diagnostics;
using System.Windows;

namespace ArmoireV3.Entities
{
    class synchro
    {
        Worker sync = new Worker();
        DataManager tik = new DataManager();
        static private bool isSynchronisingEpc = false;
        static private bool isSynchronisingLogEpc = false;

        /// <summary>
        /// Test l'ouverture des connexions vers les bases de données
        /// </summary>
        public bool testConnexion()
        {
            bool isServerDbOpen = false;
            bool isMailDbOpen = false;

            try
            {
                isServerDbOpen = testConnexionServer();
                isMailDbOpen = testConnexionMail();
            }
            catch (Exception ex)
            {
                Log.add("Erreur testConnexion: " + ex.Message);
                if (Properties.Settings.Default.UseDBGMSG)
                    System.Windows.MessageBox.Show("Erreur testConnexion: " + ex.Message);
            }

            string message = String.Format("La connexion du dédié est {0}, la connexion à la base sendmail est {1}", isServerDbOpen ? "ouverte" : "fermée", isMailDbOpen ? "ouverte" : "fermée");
            Log.add("testConnexion: " + message);

            return (isServerDbOpen && isMailDbOpen);
        }

        /// <summary>
        /// Test connexion pour la base du dédié
        /// </summary>
        /// <returns></returns>
        public bool testConnexionServer()
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
        public bool testConnexionMail()
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
            
                List<alert> readerAlert = sync.data.synchroAlert().ToList();

                for (int i = 0; i < readerAlert.Count(); i++)
                {      
                        List<user> user = sync.data.infoUser(readerAlert[i].User_ID).ToList();

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
                                sync.data.updateAlert(readerAlert[i].Id);
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

        public void alert()
        {
            List<alert> readerAlert = sync.data.synchroAlert().ToList();
            for (int i = 0; i < readerAlert.Count; i++)
            {
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
                    catch (Exception e)
                    {

                        bool memSettings = Properties.Settings.Default.WriteLog;

                        Properties.Settings.Default.WriteLog = true;
                        Log.add("Erreur alert : " + e.Message);
                        Properties.Settings.Default.WriteLog = memSettings;

                        if (Properties.Settings.Default.UseDBGMSG)
                            System.Windows.MessageBox.Show("Erreur alert: " + e.Message);
                    }
                }
            }
            readerAlert.Clear();            
        }
        
        //Synchro des tag en alert vers sendMail
        public void mailtagAlert()
        {
            List<tag_alert> readerAlert = sync.data.synchroTagAlert().ToList();
            for (int i = 0; i < readerAlert.Count(); i++)
            {
                using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectAlert))
                {
                    MySqlCommand commande = con.CreateCommand();
                    commande.CommandText = "REPLACE INTO tag_alert(`Id`,`Date_Creation`,`Alert_ID`,`Tag_Command`,`Tag_Retir`,`Tag_Intrus`,`Article_Type_Code`,`Taille`) VALUES ('" + readerAlert[i].Id + "','" + String.Format("{0:yyyy-MM-dd HH:mm:ss}", readerAlert[i].Date_Creation) + "','" + readerAlert[i].Alert_ID + "','" + readerAlert[i].Tag_Command + "','" + readerAlert[i].Tag_Retir + "','" + readerAlert[i].Tag_Intrus + "','" + readerAlert[i].Article_Type_Code + "','" + readerAlert[i].Taille + "')";
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
        public void tagAlert()
        {
            List<tag_alert> readerAlert = sync.data.synchroTagAlert().ToList();
            for (int i = 0; i < readerAlert.Count(); i++)
            {
                using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectSynchro))
                {
                    MySqlCommand commande = con.CreateCommand();
                    commande.CommandText = "REPLACE INTO tag_alert(`Id`,`Date_Creation`,`Alert_ID`,`Tag_Command`,`Tag_Retir`,`Tag_Intrus`,`Article_Type_Code`,`Taille`) VALUES ('" + readerAlert[i].Id + "','" + String.Format("{0:yyyy-MM-dd HH:mm:ss}", readerAlert[i].Date_Creation) + "','" + readerAlert[i].Alert_ID + "','" + readerAlert[i].Tag_Command + "','" + readerAlert[i].Tag_Retir + "','" + readerAlert[i].Tag_Intrus + "','" + readerAlert[i].Article_Type_Code + "','" + readerAlert[i].Taille + "')";
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
                    List<log_epc> readerLogEPC = sync.data.synchroLogEPC().ToList();
                    for (int i = 0; i < readerLogEPC.Count; i++)
                    {
                        using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectSynchro))
                        {
                            MySqlCommand commande = con.CreateCommand();

                            try
                            {
                                sync.data.UpdateLogEpc(readerLogEPC[i].Id, 1);

                                commande.CommandText = "INSERT INTO log_epc(`Id_Local`, `Epc_Id`,`Date_Creation`,`Tag`, `Code_Barre`, `Taille`,`Cycle_Lavage_Count`,`State`,`Last_User`,`Last_Reader`,`Last_Action`,`Last_Action_Date`,`Movement`,`Article_Type_ID`,`Case_ID`,`Armoire_ID`, `Synchronised`) VALUES ('" + readerLogEPC[i].Id + "','" + readerLogEPC[i].Epc_Id + "','" + String.Format("{0:yyyy-MM-dd HH:mm:ss}", readerLogEPC[i].Date_Creation) + "','" + readerLogEPC[i].Tag + "','" + readerLogEPC[i].Code_Barre + "','" + readerLogEPC[i].Taille + "','" + readerLogEPC[i].Cycle_Lavage_Count + "','" + readerLogEPC[i].State + "','" + readerLogEPC[i].Last_User + "','" + readerLogEPC[i].Last_Reader + "','" + readerLogEPC[i].Last_Action + "','" + String.Format("{0:yyyy-MM-dd HH:mm:ss}", readerLogEPC[i].Last_Action_Date) + "','" + readerLogEPC[i].Movement + "','" + readerLogEPC[i].Article_Type_ID + "','" + readerLogEPC[i].Case_ID + "','" + readerLogEPC[i].Armoire_ID.ToString() + "', 1)";
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

                                    sync.data.UpdateLogEpc(readerLogEPC[i].Id, 0);
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
            List<content_arm> readerContArm = sync.data.synchroContent().ToList();

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
                    commande.CommandText = "INSERT INTO content_arm(`Id`,`Creation_Date`,`EPC`,`State`,`RFID_Reader`) VALUES ('" + readerContArm[i].Id + "','" + readerContArm[i].Creation_Date.ToString("yyyy-MM-dd hh:mm:ss") + "','" + readerContArm[i].Epc + "','" + readerContArm[i].State + "','" + readerContArm[i].RFID_Reader + "')";
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
                    List<int> alType = sync.data.synchroAlertType().ToList();

                    while (list.Read())
                    {
                        if (!(alType.Contains(int.Parse(list["Id"].ToString()))))
                        {
                            sync.data.synchroInsertAlertType(int.Parse(list["Id"].ToString()), list["Type"].ToString(), list["Code"].ToString(), list["Description"].ToString(), list["Niveau"].ToString(), list["Contact"].ToString());
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
        /// Descend le plan de chargement de/des armoires(s)
        /// </summary>
        public void acase()
        {
            using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectSynchro))
            {
                MySqlCommand commande = con.CreateCommand();
                commande.CommandText = "SELECT * FROM `case` ";
                try
                {
                    con.Open();
                    DbDataReader list = commande.ExecuteReader();
                    List<int> alType = sync.data.synchroCase().ToList();

                    while (list.Read())
                    {
                        if (alType.Contains(int.Parse(list["Id"].ToString())))
                        {
                            sync.data.synchroUpdateCase(int.Parse(list["Id"].ToString()), int.Parse(list["Bind_ID"].ToString()), list["Taille"].ToString(), int.Parse(list["Max_Item"].ToString()), int.Parse(list["Article_Type_Id"].ToString()), int.Parse(list["Armoire_ID"].ToString()));
                        }
                        else
                        {

                            sync.data.synchroInsertCase(int.Parse(list["Id"].ToString()), int.Parse(list["Bind_ID"].ToString()), list["Taille"].ToString(), DateTime.Parse(list["Date_Creation"].ToString()), int.Parse(list["Max_Item"].ToString()), int.Parse(list["Article_Type_Id"].ToString()), int.Parse(list["Armoire_ID"].ToString()));
                        }
                    }
                    list.Close();
                    con.Close();
                }
                catch (Exception e)
                {
                    Log.add("Erreur acase : " + e.Message);
                }
            }
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
                    List<corresp_taille> alType = sync.data.synchroCorrespTaille().ToList();

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
                    }

                    list.Close();
                    con.Close();
                }
                catch (Exception e)
                {
                    Log.add("Erreur armoire: " + e.Message);
                }
            }
        }
        
        /// <summary>
        /// descend les article type vers l'armoire
        /// </summary>
        public void articleType()
        {
            using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectSynchro))
            {
                MySqlCommand commande = con.CreateCommand();
                commande.CommandText = "SELECT * FROM article_type ";
                try
                {
                    con.Open();
                    DbDataReader list = commande.ExecuteReader();
                    List<int> alType = sync.data.synchroArticletype().ToList();

                    while (list.Read())
                    {
                        if (alType.Contains(int.Parse(list["Id"].ToString())))
                        {
                            if (Properties.Settings.Default.BDDsynchV3)
                                sync.data.synchroUpdateArticletype(int.Parse(list["Id"].ToString()), list["Code"].ToString(), list["Description"].ToString(), list["Couleur"].ToString(), list["Sexe"].ToString(), list["Photo"].ToString(), int.Parse(list["Active"].ToString()), list["Type_Taille"].ToString(), list["Description_longue"].ToString());
                            else
                                sync.data.synchroUpdateArticletype(int.Parse(list["Id"].ToString()), list["Code"].ToString(), list["Description"].ToString(), list["Couleur"].ToString(), list["Sexe"].ToString(), list["Photo"].ToString(), int.Parse(list["Active"].ToString()));
                        }
                        else
                        {
                            if(Properties.Settings.Default.BDDsynchV3)
                                sync.data.synchroInsertArticletype(int.Parse(list["Id"].ToString()), DateTime.Parse(list["Date_Creation"].ToString()), DateTime.Parse(list["Date_Modification"].ToString()), list["Code"].ToString(), list["Description"].ToString(), list["Couleur"].ToString(), list["Sexe"].ToString(), list["Photo"].ToString(), int.Parse(list["Active"].ToString()), list["Type_Taille"].ToString(), list["Description_longue"].ToString());
                            else
                                sync.data.synchroInsertArticletype(int.Parse(list["Id"].ToString()), DateTime.Parse(list["Date_Creation"].ToString()), DateTime.Parse(list["Date_Modification"].ToString()), list["Code"].ToString(), list["Description"].ToString(), list["Couleur"].ToString(), list["Sexe"].ToString(), list["Photo"].ToString(), int.Parse(list["Active"].ToString()));
                        }
                    }
                    list.Close();
                    con.Close();
                }
                catch (Exception e)
                {
                    Log.add("Erreur articleType : " + e.Message);
                }
            }
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
                    List<int> alType = sync.data.synchroUser().ToList();

                    while (list.Read())
                    {
                        if (alType.Contains(int.Parse(list["Id"].ToString())))
                        {
                            if(Properties.Settings.Default.BDDsynchV3)
                                sync.data.synchroUpdateUser(int.Parse(list["Id"].ToString()), list["Login"].ToString(), list["Password"].ToString(), list["Type"].ToString(), list["Nom"].ToString(), list["Prenom"].ToString(), list["Sexe"].ToString(), list["Taille"].ToString(), int.Parse(list["Groupe"].ToString()), int.Parse(list["Department"].ToString()), list["Photo"].ToString(), int.Parse(list["Active"].ToString()), DateTime.Parse(list["End_of_Validity"].ToString()), list["Wearer_Code"].ToString());
                            else
                                sync.data.synchroUpdateUser(int.Parse(list["Id"].ToString()), list["Login"].ToString(), list["Password"].ToString(), list["Type"].ToString(), list["Nom"].ToString(), list["Prenom"].ToString(), list["Sexe"].ToString(), list["Taille"].ToString(), int.Parse(list["Groupe"].ToString()), int.Parse(list["Department"].ToString()), list["Photo"].ToString(), int.Parse(list["Active"].ToString()));
                        }
                        else
                        {
                            if(Properties.Settings.Default.BDDsynchV3)
                                sync.data.synchroInsertUser(int.Parse(list["Id"].ToString()), DateTime.Parse(list["Date_Creation"].ToString()), DateTime.Parse(list["Date_Modification"].ToString()), list["Login"].ToString(), list["Password"].ToString(), list["Type"].ToString(), list["Nom"].ToString(), list["Prenom"].ToString(), list["Sexe"].ToString(), list["Taille"].ToString(), int.Parse(list["Groupe"].ToString()), int.Parse(list["Department"].ToString()), list["Photo"].ToString(), DateTime.Parse(list["Last_Connection"].ToString()), int.Parse(list["Active"].ToString()), DateTime.Parse(list["End_of_Validity"].ToString()), list["Wearer_Code"].ToString());
                            else
                                sync.data.synchroInsertUser(int.Parse(list["Id"].ToString()), DateTime.Parse(list["Date_Creation"].ToString()), DateTime.Parse(list["Date_Modification"].ToString()), list["Login"].ToString(), list["Password"].ToString(), list["Type"].ToString(), list["Nom"].ToString(), list["Prenom"].ToString(), list["Sexe"].ToString(), list["Taille"].ToString(), int.Parse(list["Groupe"].ToString()), int.Parse(list["Department"].ToString()), list["Photo"].ToString(), DateTime.Parse(list["Last_Connection"].ToString()), int.Parse(list["Active"].ToString()));
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


        /// <summary>
        /// Synchronisation des crédits dans les 2 sens
        /// </summary>
        public void userart()
        {
            sync.data.dbContext.Refresh(System.Data.Objects.RefreshMode.StoreWins, sync.data.dbContext.user_article);
            List<user_article> dat = new List<user_article>();
            using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectSynchro))
            {
                MySqlCommand commande = con.CreateCommand();
                commande.CommandText = "SELECT * FROM user_article ";
                try
                {
                    con.Open();
                    DbDataReader list = commande.ExecuteReader();
                    List<int> alType = sync.data.synchroUserArt().ToList();


                    while (list.Read())
                    {
                        dat = sync.data.synchroUserArtDate(int.Parse(list["Id"].ToString())).ToList();
                        //Si l'entrée est déja dans la base
                        if (alType.Contains(int.Parse(list["Id"].ToString())))
                        {
                            //Si la modif dans sur l'armoire est plus recente que sur le serveur
                            if (dat[0].Date_Modification > (DateTime)list["Date_Modification"])
                            {
                                //Si les credits sont diffrérents entre l'armoire et le serveur
                                if (dat[0].Credit != int.Parse(list["Credit"].ToString()))
                                {
                                    int res = int.Parse(list["Credit"].ToString()) - dat[0].Credit;
                                    creditdifferentUserArt(list, dat, res);
                                }
                                else
                                {
                                    sync.data.synchroUpdateUserART(int.Parse(list["Id"].ToString()), dat[0].Date_Modification, list["Taille"].ToString(), int.Parse(list["Credit"].ToString()), (dat[0].Credit_Restant));

                                    using (var con2 = new MySqlConnection(Properties.Settings.Default.StringConnectSynchro))
                                    {
                                        MySqlCommand upda_commande = con2.CreateCommand();

                                        upda_commande.CommandText = "UPDATE user_article SET Date_Modification= " + "'" + String.Format("{0:yyyy-MM-dd HH:mm:ss}", dat[0].Date_Modification) + "'" + ", Credit_Restant=" + "'" + dat[0].Credit_Restant + "'" + "  WHERE Id= " + "'" + int.Parse(list["Id"].ToString()) + "'" + "    ";
                                        try
                                        {
                                            con2.Open();
                                            upda_commande.ExecuteNonQuery();
                                            Log.add(upda_commande.CommandText);
                                            con2.Close();
                                        }
                                        catch (Exception e)
                                        {
                                            bool memSettings = Properties.Settings.Default.WriteLog;

                                            Properties.Settings.Default.WriteLog = true;
                                            Log.add("Erreur userart: " + e.Message);
                                            Properties.Settings.Default.WriteLog = memSettings;

                                            if (Properties.Settings.Default.UseDBGMSG)
                                                System.Windows.MessageBox.Show("Erreur userart: " + e.Message);
                                        }
                                    }
                                }
                            }
                            else
                            {
               
                                //Modification prise en compte perte d'un article
                                if (int.Parse(list["Variation_Credit_Restant"].ToString()) != 0)
                                {
                                    int variat = int.Parse(list["Variation_Credit_Restant"].ToString());
                                    sync.data.synchroUpdateUserART(int.Parse(list["Id"].ToString()), DateTime.Parse(list["Date_Modification"].ToString()), list["Taille"].ToString(), int.Parse(list["Credit"].ToString()), (dat[0].Credit_Restant + variat));

                                    using (var con2 = new MySqlConnection(Properties.Settings.Default.StringConnectSynchro))
                                    {
                                        MySqlCommand upda_commande = con2.CreateCommand();

                                        upda_commande.CommandText = "UPDATE user_article SET Date_Modification= " + "'" + String.Format("{0:yyyy-MM-dd HH:mm:ss}", dat[0].Date_Modification) + "'" + ", Credit_Restant=" + "'" + dat[0].Credit_Restant + "'" + ", Variation_Credit_Restant=0 WHERE Id= " + "'" + int.Parse(list["Id"].ToString()) + "'" + "    ";
                                        
                                        try
                                        {
                                            con2.Open();
                                            upda_commande.ExecuteNonQuery();
                                            Log.add(upda_commande.CommandText);
                                            con2.Close();
                                        }
                                        catch (Exception e)
                                        {
                                            bool memSettings = Properties.Settings.Default.WriteLog;

                                            Properties.Settings.Default.WriteLog = true;
                                            Log.add("Erreur userart: " + e.Message);
                                            Properties.Settings.Default.WriteLog = memSettings;

                                            if (Properties.Settings.Default.UseDBGMSG)
                                                System.Windows.MessageBox.Show("Erreur userart: " + e.Message);
                                        }
                                    }
                                }
                                
                                else if (dat[0].Taille != list["Taille"].ToString())
                                {
                                    try
                                    {
                                        sync.data.synchroUpdateUserART(int.Parse(list["Id"].ToString()), DateTime.Parse(list["Date_Modification"].ToString()), list["Taille"].ToString(), dat[0].Credit, dat[0].Credit_Restant);
                                    }
                                    catch (Exception e)
                                    {
                                        bool memSettings = Properties.Settings.Default.WriteLog;

                                        Properties.Settings.Default.WriteLog = true;
                                        Log.add("Erreur userart: " + e.Message);
                                        Properties.Settings.Default.WriteLog = memSettings;

                                        if (Properties.Settings.Default.UseDBGMSG)
                                            System.Windows.MessageBox.Show("Erreur userart: " + e.Message);
                                    }
                                }

                                if (dat[0].Credit != int.Parse(list["Credit"].ToString()))
                                {
                                    int res = int.Parse(list["Credit"].ToString()) - dat[0].Credit;
                                    creditdifferentUserArt(list, dat, res);
                                }
                            }

                        }
                        //Insert une nouvelle ligne
                        else
                        {
                            try
                            {
                                sync.data.synchroInsertUserArt(int.Parse(list["Id"].ToString()), DateTime.Parse(list["Date_Creation"].ToString()), DateTime.Parse(list["Date_Modification"].ToString()), list["Taille"].ToString(), int.Parse(list["Credit"].ToString()), int.Parse(list["Credit_Restant"].ToString()), int.Parse(list["User_Id"].ToString()), int.Parse(list["Article_Type_Id"].ToString()));
                            }
                            catch (Exception e)
                            {
                                bool memSettings = Properties.Settings.Default.WriteLog;

                                Properties.Settings.Default.WriteLog = true;
                                Log.add("Erreur userart: " + e.Message);
                                Properties.Settings.Default.WriteLog = memSettings;

                                if (Properties.Settings.Default.UseDBGMSG)
                                    System.Windows.MessageBox.Show("Erreur userart: " + e.Message);
                            }
                        }                        
                    }
                    list.Close();
                    con.Close();
                }
                catch (Exception ex)
                {
                    Log.add("Erreur userart: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// utilisé dans userart
        /// </summary>
        void creditdifferentUserArt(DbDataReader list, List<user_article> dat, int res)
        {
            sync.data.synchroUpdateUserART(int.Parse(list["Id"].ToString()), dat[0].Date_Modification, list["Taille"].ToString(), int.Parse(list["Credit"].ToString()), (dat[0].Credit_Restant + res));

            using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectSynchro))
            {
                MySqlCommand upda_commande = con.CreateCommand();
                upda_commande.CommandText = "UPDATE user_article SET Date_Modification= " + "'" + String.Format("{0:yyyy-MM-dd HH:mm:ss}", dat[0].Date_Modification) + "'" + ", Credit_Restant=" + "'" + dat[0].Credit_Restant + "'" + "  WHERE Id= " + "'" + int.Parse(list["Id"].ToString()) + "'" + "    ";
                try
                {
                    con.Open();
                    upda_commande.ExecuteNonQuery();
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
                }
            }
        }

        /// <summary>
        /// Synchronisation de la table EPC dans les 2 sens
        /// </summary>
        public void epc()
        {
            if (!isSynchronisingEpc)
            {
                isSynchronisingEpc = true;
                try
                {
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
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectSynchro))
                    {
                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = "SELECT * FROM epc ";
                        try
                        {
                            con.Open();
                            DbDataReader list = commande.ExecuteReader();// contient toutes les lignes de la table epc sur le SERVER

                            List<int> alType = sync.data.synchroEPC().ToList();// liste de tous les ID en LOCAL
                            List<epc> dat = new List<epc>();
                            if (list != null)
                            {
                                while (list.Read())// pour chaque ligne de l table epc
                                {
                                    dat = sync.data.synchroEPCDate(int.Parse(list["Id"].ToString())).ToList(); // retourne un objet epc en fonction d'un id recuperé sur le SERVER
                                    if (alType.Contains(int.Parse(list["Id"].ToString())))// si la liste des epc en LOCAL contiens l'id de l'enregistrement SERVER qu'on est en train lire
                                    {
                                        if (int.Parse(list["State"].ToString()) == 2 && ((dat[0].State == 1) || (dat[0].State == 4)))// si l'epc est perdu sur le SERVER et sorti en LOCAL
                                        {
                                            if (Properties.Settings.Default.BDDsynchV3)
                                            {
                                                sync.data.updateEpc(2, list["Tag"].ToString(), list["Last_User"].ToString(), Properties.Settings.Default.NumArmoire, int.Parse(list["Actif"].ToString()));// on update l'etat de l'epc en LOCAL => 2 (perdu)
                                            }
                                            else
                                            {
                                                sync.data.updateEpc(2, list["Tag"].ToString(), list["Last_User"].ToString(), Properties.Settings.Default.NumArmoire);// on update l'etat de l'epc en LOCAL => 2 (perdu)
                                            }

                                            try
                                            {
                                                sync.data.dbContext.Refresh(System.Data.Objects.RefreshMode.StoreWins, sync.data.dbContext.epc);
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
                                                                                                            + "  WHERE Id= " + int.Parse(list["Id"].ToString());
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
                                                                                                            + "  WHERE Id= " + int.Parse(list["Id"].ToString());
                                            }

                                            try
                                            {
                                                con2.Open();
                                                upda_commande.ExecuteNonQuery();
                                                con2.Close();
                                            }
                                            catch (Exception e)
                                            {
                                                bool memSettings = Properties.Settings.Default.WriteLog;

                                                Properties.Settings.Default.WriteLog = true;
                                                Log.add("Erreur update epc " + int.Parse(list["Id"].ToString()) + ": " + e.Message + "(Requête: " + upda_commande.CommandText + ")");
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
                                            if (Properties.Settings.Default.BDDsynchV3)
                                            {
                                                sync.data.synchroInsertEPC(int.Parse(list["Id"].ToString()), DateTime.Parse(list["Date_Creation"].ToString()), DateTime.Parse(list["Date_Modification"].ToString()), list["Tag"].ToString(), list["Code_Barre"].ToString(), list["Taille"].ToString(), "1", int.Parse(list["Cycle_Lavage_Count"].ToString()), int.Parse(list["State"].ToString()), list["Last_User"].ToString(), list["Last_Reader"].ToString(), list["Last_Action"].ToString(), DateTime.Parse(list["Last_Action_Date"].ToString()), int.Parse(list["Movement"].ToString()), int.Parse(list["Article_Type_ID"].ToString()), Properties.Settings.Default.NumArmoire, int.Parse(list["Actif"].ToString()));//int.Parse(list["Armoire_ID"].ToString()));
                                            }
                                            else
                                            {
                                                sync.data.synchroInsertEPC(int.Parse(list["Id"].ToString()), DateTime.Parse(list["Date_Creation"].ToString()), DateTime.Parse(list["Date_Modification"].ToString()), list["Tag"].ToString(), list["Code_Barre"].ToString(), list["Taille"].ToString(), "1", int.Parse(list["Cycle_Lavage_Count"].ToString()), int.Parse(list["State"].ToString()), list["Last_User"].ToString(), list["Last_Reader"].ToString(), list["Last_Action"].ToString(), DateTime.Parse(list["Last_Action_Date"].ToString()), int.Parse(list["Movement"].ToString()), int.Parse(list["Article_Type_ID"].ToString()), Properties.Settings.Default.NumArmoire);//int.Parse(list["Armoire_ID"].ToString()));
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
                }
            }
            else
            {
                Log.add("Erreur: Les EPC sont déjà en cours de synchronisation");
            }
        }

        /// <summary>
        /// epcPerdus
        /// </summary>
        public void epcPerdus()
        {
            List<epc> listEpcLost = getLostEpc();
            foreach (epc epcLost in listEpcLost)
            {
                epc epcLocal = sync.data.getEpc(epcLost.Tag);
                if (epcLost.Date_Modification > epcLocal.Date_Modification)
                {
                    if(Properties.Settings.Default.BDDsynchV3)
                        sync.data.updateEpc(2, epcLost.Tag, epcLost.Last_User, int.Parse(epcLost.Last_Reader), epcLost.Actif);
                    else
                        sync.data.updateEpc(2, epcLost.Tag, epcLost.Last_User, int.Parse(epcLost.Last_Reader));
                }
            }
        }

        /// <summary>
        /// getLostEpc
        /// </summary>
        /// <returns></returns>
        public List<epc> getLostEpc()
        {
            List<epc> listEpc = new List<epc>();
            
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
                        epc e = new epc();
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
