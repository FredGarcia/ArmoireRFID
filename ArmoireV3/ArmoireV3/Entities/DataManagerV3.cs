using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Impinj.OctaneSdk;
using System.Data.Linq;
using System.Windows;
using System.Reflection;
using ArmoireV3.Entities;
using MySql.Data.MySqlClient;
using System.Globalization;
using System.Threading;
using System.Text.RegularExpressions;



namespace ArmoireV3.Entities
{
    public partial class DataManagerV3 : INotifyPropertyChanged
    {
        //private synchro synchV3 = new synchro();
        
       // public ArmoireV3Entities dbContext = new ArmoireV3Entities();  // ne devrait plus être nécéssaire par la suite

        public DataManagerV3() // ne devrait plus être nécéssaire par la suite
        {
            CultureInfo culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            culture.DateTimeFormat.ShortDatePattern = "yyyy-MM-dd"; // HH:mm:ss";
            culture.DateTimeFormat.LongDatePattern = "yyyy-MM-dd HH:mm:ss";
            culture.DateTimeFormat.LongTimePattern = "HH:mm:ss";
            Thread.CurrentThread.CurrentCulture = culture; 
           // dbContext = new ArmoireV3Entities();
        }

        public DataManagerV3(string connectionString) // ne devrait plus être nécéssaire par la suite
        {
           // dbContext = new ArmoireV3Entities(connectionString);
        }

        public void RefreshContext() // ne devrait plus être nécéssaire par la suite
        {
           // dbContext = new ArmoireV3Entities();
        }

        internal bool IsAvailable()
        {
            try
            {
                using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                {
                    con.Open();
                    con.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.add("Erreur: Base locale non disponible"+ex.Message);

                if (Properties.Settings.Default.UseDBGMSG)
                    System.Windows.MessageBox.Show("Erreur: Base locale non disponible");

                return false;
            }

        }

        #region AUTOCONNECT

        public bool Is_connected() // ne devrait plus être nécéssaire par la suite
        {
            return IsAvailable();
        }
        public void Connect() // ne devrait plus être nécéssaire par la suite
        {
            /*
            try
            {
                synchV3.testConnexionLocale();
            }
            catch (Exception)
            {
                if (Properties.Settings.Default.UseDBGMSG)
                {
                    MessageBox.Show("Echec de connexion à la Base de Données.");
                }
            }
             * */
        }
        public void CloseConnection() // ne devrait plus être nécéssaire par la suite
        {
          /*  if (dbContext != null)
                dbContext.Dispose();*/
        }

        #endregion autoconnect

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion

        #region synchro

        /// <summary>
        /// retourne une liste d'un seul objet User de l'Id égale à celui demandé 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IEnumerable<User> infoUser(int id) // méthode de remplacement de IEnumerable<user> infoUser(int id) de DataManager.cs
        {
            if (IsAvailable())
            {
                List<User> listusr = new List<User>();
                string query = "SELECT `Id`, `Date_Creation`, `Date_Modification`, `Login`, `Password`, `Type`," +
                    "`Nom`, `Prenom`, `Id_Lang`, `Sexe`, `Taille`, `Groupe`, `Department`, `Photo`, `Last_Connection`,"+
                    "`Active`, `End_of_Validity`, `Wearer_Code` FROM `user` WHERE `Id`='" + id.ToString() + "' LIMIT 1;";
                try
                {
                    
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {
                            
                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;
                            
                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while(dataReader.Read())
                        {
                            int usrid = int.Parse(dataReader.GetString("Id"));
                            if(usrid!=id)
                                throw(new Exception("Id parsé ne corrrespond pas"));
                            DateTime dc = dataReader.GetDateTime("Date_Creation");
                            DateTime dm = dataReader.GetDateTime("Date_Modification");
                            string login = dataReader.GetString("Login");
                            string password = dataReader.GetString("Password");
                            string type = dataReader.GetString("Type");
                            string nom = dataReader.GetString("Nom");
                            string prenom = dataReader.GetString("Prenom");
                            int id_lang = int.Parse(dataReader.GetString("Id_Lang"));
                            string sexe = dataReader.GetString("Sexe");
                            string taille = dataReader.GetString("Taille");
                            int grp = int.Parse(dataReader.GetString("Groupe"));
                            int dpt = int.Parse(dataReader.GetString("Department"));
                            string photo = dataReader.GetString("Photo");
                            DateTime last_con = dataReader.GetDateTime("Last_Connection");
                            int active = int.Parse(dataReader.GetString("Active"));
                            DateTime end_of_val = dataReader.GetDateTime("End_of_Validity");
                            string wearer_code = dataReader.GetString("Wearer_Code");
                            User usrtmp = new User(usrid, dc, dm, login, password, type, nom, prenom, id_lang, sexe, taille, grp, dpt, photo, last_con, active, end_of_val, wearer_code);
                            listusr.Add(usrtmp);
                        }
                        con.Close();

                        if (listusr.Count() == 1)
                            return listusr;
                        else
                            return new List<User>();
                    }
                }
                catch (Exception e)
                {
                    Log.add("Erreur infoUser: " + e.Message);
                    return new List<User>();
                }
            }
            else
                return new List<User>();
        }


        /// <summary>
        /// retourne une liste des log_epc non synchronisés (champ log_epc.Synchronised = 0)
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Log_Epc> synchroLogEPC() // méthode de remplacement de IEnumerable<log_epc> synchroLogEPC() de DataManager.cs
        {
            if (IsAvailable())
            {
                List<Log_Epc> list_log_epc = new List<Log_Epc>();
                int i = 0;
                try
                {
                    string query = "SELECT `Id`, `Date_Creation`, `Epc_Id`, `Tag`, `Code_Barre`, `Taille`, `Cycle_Lavage_Count`, `State`, `Last_User`,"+
                        "`Last_Reader`, `Last_Action`, `Last_Action_Date`, `Movement`, `Article_Type_ID`, `Case_ID`, `Armoire_ID`, `Synchronised` "+
                        " FROM `log_epc` WHERE `Synchronised` = '0';";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            int logepcid = int.Parse(dataReader.GetString("Id"));
                            int synch = int.Parse(dataReader.GetString("Synchronised"));
                            if (synch != 0)
                                throw (new Exception("le Synchronised parsé pour l'Id = " + logepcid + " ne corrrespond pas"));
                            DateTime date_crea = dataReader.GetDateTime("Date_Creation");
                            int epcid = int.Parse(dataReader.GetString("Epc_Id"));
                            string tag = dataReader.GetString("Tag");
                            string codebarre = dataReader.GetString("Code_Barre");
                            string taille = dataReader.GetString("Taille");
                            int cycle_lava_count = int.Parse(dataReader.GetString("Cycle_Lavage_Count"));
                            int state = int.Parse(dataReader.GetString("State"));
                            string lastuser = dataReader.GetString("Last_User");
                            string lastreader = dataReader.GetString("Last_Reader");
                            string lastaction = dataReader.GetString("Last_Action");
                            DateTime lastactiondate = dataReader.GetDateTime("Last_Action_Date");
                            int movement = int.Parse(dataReader.GetString("Movement"));
                            int artypid = int.Parse(dataReader.GetString("Article_Type_ID"));
                            int caseid = int.Parse(dataReader.GetString("Case_ID"));
                            int armoireid = int.Parse(dataReader.GetString("Armoire_ID"));

                            Log_Epc logepc_tmp = new Log_Epc(logepcid, date_crea, epcid, tag, codebarre, taille, cycle_lava_count, state, lastuser, lastreader, lastaction, lastactiondate, movement, artypid, caseid, armoireid, synch);
                            list_log_epc.Add(logepc_tmp);
                            i++;
                        }
                        con.Close();
                    }
                    if ((i > 0) && (list_log_epc.Count() == i))
                        return list_log_epc;
                    else
                        return new List<Log_Epc>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur synchroLogEPC: " + e.Message);
                    return new List<Log_Epc>();
                }
            }
            else
                return new List<Log_Epc>();           
        }

        /// <summary>
        /// Remonté du contenue de l'armoire
        /// Retourne une List de Content_Arm de RFID_Reader égale à Properties.Settings.Default.NumArmoire
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Version_Armoire> synchroVersion() // méthode de remplacement de IEnumerable<content_arm> synchroContent() de DataManager.cs
        {
            if (IsAvailable())
            {
                List<Version_Armoire> list_Version = new List<Version_Armoire>();
                int i = 0;
                try
                {
                    string query = "SELECT `Id`, `NomPlace`, `VersLog`, `VersMat`, `DateSynchro` FROM `version`;";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            int ver_id = int.Parse(dataReader.GetString("Id"));
                            string ver_place = dataReader.GetString("NomPlace");
                            string ver_log = dataReader.GetString("VersLog");
                            string ver_mat = dataReader.GetString("VersMat");
                            DateTime date_crea = dataReader.GetDateTime("DateSynchro");

                            Version_Armoire ver_tmp = new Version_Armoire(ver_id, ver_place, ver_log, ver_mat, date_crea);
                            list_Version.Add(ver_tmp);
                            i++;
                        }
                        con.Close();
                    }
                    if ((i > 0) && (list_Version.Count() == i))
                        return list_Version;
                    else
                        return new List<Version_Armoire>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur synchroVersion: " + e.Message);
                    return new List<Version_Armoire>();
                }
            }
            else
                return new List<Version_Armoire>();
        }


        /// <summary>
        /// Remonté du contenue de l'armoire
        /// Retourne une List de Content_Arm de RFID_Reader égale à Properties.Settings.Default.NumArmoire
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Content_Arm> synchroContent() // méthode de remplacement de IEnumerable<content_arm> synchroContent() de DataManager.cs
        {
            if (IsAvailable())
            {
                List<Content_Arm> list_content_arm = new List<Content_Arm>();
                int i = 0;
                try
                {
                    string query = "SELECT `Id`, `Creation_Date`, `Epc`, `State`, `RFID_Reader`"+
                        " FROM `content_arm` WHERE `RFID_Reader` = '" + Properties.Settings.Default.NumArmoire.ToString() +"';";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            int ca_id = int.Parse(dataReader.GetString("Id"));
                            int rfid_reader = int.Parse(dataReader.GetString("RFID_Reader"));
                            if (rfid_reader != Properties.Settings.Default.NumArmoire)
                                throw (new Exception("le RFID_Reader parse pour l'Id = " + ca_id + " ne corrrespond pas"));
                            DateTime date_crea = dataReader.GetDateTime("Creation_Date");
                            string epc = dataReader.GetString("Epc");
                            int state = int.Parse(dataReader.GetString("State"));

                            Content_Arm ca_tmp = new Content_Arm(ca_id, date_crea, epc, state, rfid_reader);
                            list_content_arm.Add(ca_tmp);
                            i++;
                        }
                        con.Close();
                    }
                    if ((i > 0) && (list_content_arm.Count() == i))
                        return list_content_arm;
                    else
                        return new List<Content_Arm>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur synchroContent: " + e.Message);
                    return new List<Content_Arm>();
                }
            }
            else
                return new List<Content_Arm>();
        }

                  
        /// <summary>
        /// remonté des alertes non traitées
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Alert> synchroAlert() // méthode de remplacement de IEnumerable<alert> synchroAlert() de DataManager.cs
        {
            if (IsAvailable())
            {
                List<Alert> list_alert = new List<Alert>();
                int i = 0;
                try
                {
                    string query = "SELECT `Id`, `Date_Creation`, `Alert_Type_Id`, `Message`, `User_ID`, `Armoire_ID`, `Traiter` FROM `alert` WHERE `Traiter` = '0';";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            int alertid = int.Parse(dataReader.GetString("Id"));
                            int traiter = int.Parse(dataReader.GetString("Traiter"));
                            if (traiter != 0)
                                throw (new Exception("le champ Traiter parsé pour l'Id = " + alertid + " ne vaut pas 0"));
                            DateTime date_crea = dataReader.GetDateTime("Date_Creation");
                            int alertypid = int.Parse(dataReader.GetString("Alert_Type_Id"));
                            string message = dataReader.GetString("Message");
                            int usrid = int.Parse(dataReader.GetString("User_ID"));
                            int armid = int.Parse(dataReader.GetString("Armoire_ID"));

                            Alert alert_tmp = new Alert(alertid, date_crea, alertypid, message, usrid, armid, traiter);
                            list_alert.Add(alert_tmp);
                            i++;
                        }
                        con.Close();
                    }
                    if ((i > 0) && (list_alert.Count() == i))
                        return list_alert;
                    else
                        return new List<Alert>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur synchroAlert: " + e.Message);

                    return new List<Alert>();
                }
            }
            else
                return new List<Alert>();
        }

        
        /// <summary>
        /// remonté des tag alert
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Tag_Alert> synchroTagAlert(List<Alert> alert_nontraitee) // méthode de remplacement de IEnumerable<Tag_Alert> synchroTagAlert() de DataManager.cs
        {
            if (IsAvailable())
            {
                List<Tag_Alert> list_tag_alert = new List<Tag_Alert>();
                int i = 0;
                try
                {
                    for (int j = 0; j < alert_nontraitee.Count(); j++)
                    {
                        string query = "SELECT `Id`, `Date_Creation`, `Alert_ID`, `Tag_Command`, `Tag_Retir`, `Tag_Intrus`, `Article_Type_Code`, `Taille` FROM `tag_alert` WHERE `Alert_ID`=" + alert_nontraitee[j].Id.ToString() + ";";
                        using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                        {

                            MySqlCommand commande = con.CreateCommand();
                            commande.CommandText = query;

                            con.Open();
                            MySqlDataReader dataReader = commande.ExecuteReader();
                            while (dataReader.Read())
                            {
                                int tagalertid = int.Parse(dataReader.GetString("Id"));
                                DateTime date_crea = dataReader.GetDateTime("Date_Creation");
                                int alertid = int.Parse(dataReader.GetString("Alert_ID"));
                                string tag_com = dataReader.GetString("Tag_Command");
                                string tag_retir = dataReader.GetString("Tag_Retir");
                                string tag_intrus = dataReader.GetString("Tag_Intrus");
                                string artypcod = dataReader.GetString("Article_Type_Code");
                                string taille = dataReader.GetString("Taille");

                                Tag_Alert tag_alert_tmp = new Tag_Alert(tagalertid, date_crea, alertid, tag_com, tag_retir, tag_intrus, artypcod, taille);
                                list_tag_alert.Add(tag_alert_tmp);
                                i++;
                            }
                            con.Close();
                        }
                    }
                    if ((i > 0) && (list_tag_alert.Count() == i))
                        return list_tag_alert;
                    else
                        return new List<Tag_Alert>();

                }
                catch (Exception e)
                {
                    Log.add("Erreur synchroTagAlert: " + e.Message);
                    return new List<Tag_Alert>();
                }
            }
            else
                return new List<Tag_Alert>();
        }


        /// <summary>
        /// select, pour la verif de l'existence des numéros d'alert_type dans l'armoire
        /// </summary>
        /// <returns></returns>
        public IEnumerable<int> synchroAlertType()
        {
            if (IsAvailable())
            {
                List<int> list_alert_type = new List<int>();
                int i = 0; 
                try
                {
                    string query = "SELECT `Id` FROM `alert_type`";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            int alerttypeid = int.Parse(dataReader.GetString("Id"));
                            list_alert_type.Add(alerttypeid);
                            i++;
                        }
                        con.Close();
                    }
                    if ((i > 0) && (list_alert_type.Count() == i))
                        return list_alert_type;
                    else
                        return new List<int>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur synchroAlertType: " + e.Message);
                    return new List<int>();
                }
            }
            else
                return new List<int>();
        }

        /// <summary>
        /// insere dans l'armoire un type d'alerte (alert_type)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <param name="code"></param>
        /// <param name="desc"></param>
        /// <param name="niv"></param>
        public void synchroInsertAlertType(int id, string type, string code, string desc, string niv)
        {
            if (IsAvailable())
            {
                int result = 0;
                try
                {
                    string strcod = MySql.Data.MySqlClient.MySqlHelper.EscapeString(code);
                    string strdesc = MySql.Data.MySqlClient.MySqlHelper.EscapeString(desc);
                    string query = "INSERT INTO `alert_type` (`Id`, `Type`, `Code`, `Description`, `Niveau`"/*, `Contact`*/+ ") " +
                        " VALUES ('" + id.ToString() + "','" + type + "','" + strcod + "','" + strdesc + "','" + niv + "');";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        result = commande.ExecuteNonQuery();
                        con.Close();
                    }
                    if (result != 1)
                        throw (new Exception("requete non reussie ("+query+")"));
                }
                catch (Exception e)
                {
                    Log.add("Erreur synchroInsertAlertType: " + e.Message);

                    if (Properties.Settings.Default.UseDBGMSG)
                    {
                        MessageBox.Show("Impossible de mettre à jour les types d'alerte: " + e.Message);
                    }
                }
            }
        }

        /// <summary>
        /// insere dans l'armoire un type d'alerte (alert_type)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <param name="code"></param>
        /// <param name="desc"></param>
        /// <param name="niv"></param>
        /// <param name="contact"></param>
        public void synchroInsertAlertType(int id, string type, string code, string desc, string niv, string contact)
        {
            if (IsAvailable())
            {
                int result = 0;
                try
                {// La colonne contact a disparu
                    string strcod = MySql.Data.MySqlClient.MySqlHelper.EscapeString(code);
                    string strdesc = MySql.Data.MySqlClient.MySqlHelper.EscapeString(desc);
                    string query = "INSERT INTO `alert_type` (`Id`, `Type`, `Code`, `Description`, `Niveau`"/*, `Contact`*/+") "+
                        "VALUES ('" + id.ToString() + "','" + type + "','" + strcod + "','" + strdesc + "','" + niv +/* "','" + contact +*/ "');";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        result = commande.ExecuteNonQuery();
                        con.Close();
                    }
                    if (result != 1)
                        throw (new Exception("requete non reussie ("+query+")"));
                }
                catch (Exception e)
                {
                    Log.add("Erreur synchroInsertAlertType: " + e.Message);

                    if (Properties.Settings.Default.UseDBGMSG)
                    {
                        MessageBox.Show("Impossible de mettre à jour les types d'alerte: " + e.Message);
                    }
                }
            }
        }
        

        /// <summary>
        /// select, pour la verif de l'existence des lignes dans la table case de l'armoire
        /// </summary>
        /// <returns></returns>
        public IEnumerable<int> synchroCase()
        {
            if (IsAvailable())
            {
                List<int> list_case = new List<int>();
                string query = "SELECT `Id` FROM `case`;";
                int i = 0;
                try
                {
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            int caseid = int.Parse(dataReader.GetString("Id"));
                            list_case.Add(caseid);
                            i++;
                        }
                        con.Close();
                    }
                    if ((i > 0) && (list_case.Count() == i))
                        return list_case;
                    else
                        return new List<int>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur synchroCase: " + e.Message);
                    return new List<int>();
                }
            }
            else
                return new List<int>();
        }


        /// <summary>
        /// insertion d'une ligne dans la table 'case' en locale
        /// </summary>
        /// <param name="id"></param>
        /// <param name="bind_id"></param>
        /// <param name="taille"></param>
        /// <param name="cre"></param>
        /// <param name="maxitem"></param>
        /// <param name="articletypeid"></param>
        /// <param name="armid"></param>
        public void synchroInsertCase(int id, int bind_id, string taille, DateTime cre, int maxitem, int articletypeid, int armid)
        {
            if (IsAvailable())
            {
                int result = 0;
                DateTime time_reference = (new DateTime(1971, 01, 01));
                DateTime date_creation;
                if (cre > time_reference)
                    date_creation = cre;
                else
                    date_creation = time_reference;
                                
                try
                {
                    string query = "INSERT INTO `case` (`Id`, `Bind_ID`, `Taille`, `Date_Creation`, `Max_Item`, `Article_Type_Id`, `Armoire_ID`) "+
                        "VALUES ('" + id.ToString() + "','" + bind_id.ToString() + "','" + taille + "','" + date_creation.ToString("yyyy-MM-dd HH:mm:ss") +
                        "','" + maxitem.ToString() + "','" + articletypeid.ToString() + "','" + armid.ToString() + "');";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        result = commande.ExecuteNonQuery();
                        con.Close();
                    }
                    if (result != 1)
                        throw (new Exception("requete non reussie (" + query + ")"));
                }
                catch (Exception e)
                {
                    Log.add("Erreur synchroInsertCase: " + e.Message);
                    if (Properties.Settings.Default.UseDBGMSG)
                    {
                        MessageBox.Show("Erreur lors de la synchronisation de l'armoire: " + e.Message);
                    }
                }
            }
        }


        /// <summary>
        /// modification d'une ligne dans la table 'case' identifiée pas son id en locale
        /// </summary>
        /// <param name="id"></param>
        /// <param name="bind_id"></param>
        /// <param name="taille"></param>
        /// <param name="max"></param>
        /// <param name="arttypid"></param>
        /// <param name="arm"></param>
        public void synchroUpdateCase(int id, int bind_id, string taille, int max, int arttypid, int arm)
        {
            if (IsAvailable())
            {
                int result = 0;
                
                try
                {
                    string query = "UPDATE `case` SET `Bind_ID`='" + bind_id.ToString() + "', `Taille`='" + taille + "', `Max_Item`='" + max.ToString() +
                        "' , `Article_Type_Id`='" + arttypid.ToString() + "', `Armoire_ID`='" + arm.ToString() + "' WHERE `Id`='" + id.ToString() + "';";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        result = commande.ExecuteNonQuery();
                        con.Close();
                    }
                    if (result != 1)
                        throw (new Exception("requete non reussie (" + query + ")"));
                }
                catch (Exception e)
                {
                    Log.add("Erreur synchroUpdateCase: " + e.Message);
                    if (Properties.Settings.Default.UseDBGMSG)
                    {
                        MessageBox.Show("Erreur lors de la synchronisation de l'armoire: " + e.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Retourne une liste des corresp_taille présents dans la base locale
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Corresp_Taille> synchroCorrespTaille()
        {
            if (IsAvailable())
            {
                List<Corresp_Taille> list_ct = new List<Corresp_Taille>();
                string query = "SELECT `Type-Taille`, `Taille`, `Classement_tailles` FROM `corresp_taille`";
                int i = 0;
                try
                {
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            string typetaille = dataReader.GetString("Type-Taille");
                            string taille = dataReader.GetString("Taille");
                            int classmnttaille = int.Parse(dataReader.GetString("Classement_tailles"));
                            
                            Corresp_Taille cor_taille_tmp = new Corresp_Taille(typetaille, taille, classmnttaille);

                            list_ct.Add(cor_taille_tmp);
                            i++;
                        }
                        con.Close();
                    }
                    if ((i > 0) && (list_ct.Count() == i))
                        return list_ct;
                    else
                        return new List<Corresp_Taille>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur synchroCorrespTaille: " + e.Message);
                    return new List<Corresp_Taille>();
                }
            }
            else
                return new List<Corresp_Taille>();
        }


        public bool AreKeysPresentInCorrespTaille(string type_Taille, string taille, int classement_taille)
        {
            if (IsAvailable())
            {
                List<Corresp_Taille> list_id_article_type = new List<Corresp_Taille>();
                string query = "SELECT * FROM `corresp_taille` WHERE `Type-Taille`='" + type_Taille.ToString() + "' AND `Taille`='" + taille + "' AND `Classement_tailles`='" + classement_taille.ToString() + "';";
                int i = 0;
                try
                {
                    //`corresp_taille` (
                    //  `Type-Taille` char(1) NOT NULL,
                    //  `Taille` varchar(255) NOT NULL,
                    //  `Classement_tailles` int(11) NOT NULL
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            i++;
                        }
                        con.Close();
                    }
                    if (i > 0)
                        return true;
                    else
                        return false;
                }
                catch (Exception e)
                {
                    Log.add("Erreur : " + e.Message);
                    return false;
                }
            }
            else
                return false;
        }

        /// <summary>
        /// insertion d'une ligne dans la table corresp_taille locale
        /// </summary>
        /// <param name="type_Taille"></param>
        /// <param name="taille"></param>
        /// <param name="classement_taille"></param>
        public void synchroInsertCorrespTaille(string type_Taille, string taille, int classement_taille)
        {
            if (IsAvailable())
            {
                int result = 0;
                
                try
                {
                    string query = "INSERT INTO `corresp_taille` (`Type-Taille`, `Taille`, `Classement_tailles`) " +
                        "VALUES ('" + type_Taille + "','" + taille + "','" + classement_taille.ToString() + "');";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        result = commande.ExecuteNonQuery();
                        con.Close();
                    }
                    if (result != 1)
                        throw (new Exception("requete non reussie (" + query + ")"));
                }
                catch (Exception e)
                {
                    Log.add("Erreur synchroInsertCorrespTaille: " + e.Message);
                    if (Properties.Settings.Default.UseDBGMSG)
                    {
                        MessageBox.Show("Erreur lors de la synchronisation de l'armoire: " + e.Message);
                    }
                }
            }
        }

        public bool synchroDeleteCorrespTaille(string type_Taille, string taille, int classement_taille)
        {
            if (IsAvailable())
            {
                List<Corresp_Taille> list_id_article_type = new List<Corresp_Taille>();
                string query = "DELETE FROM `corresp_taille` WHERE `Type-Taille`='" + type_Taille.ToString() + "' AND `Taille`='" + taille + "' AND `Classement_tailles`='" + classement_taille.ToString() + "';";
                int resultat = 0;
                try
                {
                    //`corresp_taille` (
                    //  `Type-Taille` char(1) NOT NULL,
                    //  `Taille` varchar(255) NOT NULL,
                    //  `Classement_tailles` int(11) NOT NULL
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        resultat = commande.ExecuteNonQuery();
                        con.Close();
                    }
                    if (resultat == 1)
                        return true;
                    else
                        return false;
                }
                catch (Exception e)
                {
                    Log.add("Erreur synchroDeleteArticletaille : " + e.Message);
                    return false;
                }
            }
            else
                return false;
        }

        public void synchroUpdateCorrespTaille(string type_Taille, string taille, int classement_taille)
        {
            if (IsAvailable())
            {
                int result;
                //Corresp_Taille ca = dbContext.corresp_taille.FirstOrDefault(i => i.Type_Taille == type_Taille && i.Taille == taille);
               
                try
                {
                    string query = "UPDATE `corresp_taille` SET `Classement_tailles` = '" + classement_taille.ToString() +
                        "' WHERE `Type-Taille` = '" + type_Taille + "' AND `Taille` = '" + taille + "' AND `Classement_tailles` != '" + 
                        classement_taille.ToString() + "';";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        result = commande.ExecuteNonQuery();
                        con.Close();
                    }
                    if (result > 1)
                        throw (new Exception("résultat non valide pour la requête (" + query + ")"));
                }
                catch (Exception e)
                {
                    Log.add("Erreur synchroUpdateCorrespTaille: " + e.Message);
                    if (Properties.Settings.Default.UseDBGMSG)
                    {
                        MessageBox.Show("Erreur lors de la synchronisation de l'armoire: " + e.Message);
                    }
                }
            }
        }

        public bool synchroDeleteArticletaille(int arttypid, string taille, int armoire)
        {
            if (IsAvailable())
            {
                List<Article_Taille> list_id_article_type = new List<Article_Taille>();
                string query = "DELETE FROM `article_taille` WHERE `Article_Type_ID`='" + arttypid.ToString() + "' AND `Taille`='" + taille + "' AND `Armoire`='" + armoire.ToString() + "';";
                int resultat = 0;
                try
                {
                    // CREATE TABLE IF NOT EXISTS `article_taille` (
                    // `Article_Type_ID` int(8) NOT NULL COMMENT 'Id du type article correspondant au vêtement',
                    // `Taille` varchar(255) NOT NULL COMMENT 'Taille pour l''article donné',
                    // `Armoire` int(8) NOT NULL COMMENT 'Numérotation de l''armoire',
                    // `Vide` tinyint(1) NOT NULL COMMENT 'indique si le type d''article/taille est épuisé',
                    // PRIMARY KEY (`Article_Type_ID`,`Taille`,`Armoire`)
                    // ) ENGINE=MyISAM DEFAULT CHARSET=latin1;
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        resultat = commande.ExecuteNonQuery();
                        con.Close();  
                    }
                    if (resultat == 1)
                        return true;
                    else
                        return false;
                }
                catch (Exception e)
                {
                    Log.add("Erreur synchroDeleteArticletaille : " + e.Message);
                    return false;
                }
            }
            else
                return false;
        }
        public List<Article_Taille> getListArticleTaille()
        {
            if (IsAvailable())
            {
                List<Article_Taille> list_article_taille = new List<Article_Taille>();
                string query = "SELECT * FROM `article_taille` ;";
                int i = 0;
                try
                {
                    // CREATE TABLE IF NOT EXISTS `article_taille` (
                    // `Article_Type_ID` int(8) NOT NULL COMMENT 'Id du type article correspondant au vêtement',
                    // `Taille` varchar(255) NOT NULL COMMENT 'Taille pour l''article donné',
                    // `Armoire` int(8) NOT NULL COMMENT 'Numérotation de l''armoire',
                    // `Vide` tinyint(1) NOT NULL COMMENT 'indique si le type d''article/taille est épuisé',
                    // PRIMARY KEY (`Article_Type_ID`,`Taille`,`Armoire`)
                    // ) ENGINE=MyISAM DEFAULT CHARSET=latin1;
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            int arttyid = int.Parse(dataReader.GetString("Article_Type_ID"));
                            string tail = dataReader.GetString("Taille");
                            int arm = int.Parse(dataReader.GetString("Armoire"));
                            bool vid = bool.Parse(dataReader.GetString("Vide"));


                            Article_Taille tmp = new Article_Taille(arttyid, tail, arm, vid);

                            list_article_taille.Add(tmp);
                            i++;
                        }
                        con.Close();
                    }
                    if ((i > 0) && (list_article_taille.Count() == i))
                        return list_article_taille;
                    else
                        return new List<Article_Taille>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur getting Article_Taille: " + e.Message);
                    return new List<Article_Taille>();
                }
            }
            else
            {
                return new List<Article_Taille>();
            }
        }

        /// <summary>
        /// //select, pour la verif de l'existence de ces lignes dans la table article_type dans la base de l'armoire
        /// </summary>
        /// <returns></returns>
        public bool AreKeysPresentInArticleTaille(int arttypid, string taille, int armoire)
        {
            if (IsAvailable())
            {
                List<Article_Taille> list_id_article_type = new List<Article_Taille>();
                string query = "SELECT `Vide` FROM `article_taille` WHERE `Article_Type_ID`='" + arttypid.ToString() + "' AND `Taille`='" + taille + "' AND `Armoire`='" + armoire.ToString() + "';";
                int i = 0;
                try
                {
                    // CREATE TABLE IF NOT EXISTS `article_taille` (
                    // `Article_Type_ID` int(8) NOT NULL COMMENT 'Id du type article correspondant au vêtement',
                    // `Taille` varchar(255) NOT NULL COMMENT 'Taille pour l''article donné',
                    // `Armoire` int(8) NOT NULL COMMENT 'Numérotation de l''armoire',
                    // `Vide` tinyint(1) NOT NULL COMMENT 'indique si le type d''article/taille est épuisé',
                    // PRIMARY KEY (`Article_Type_ID`,`Taille`,`Armoire`)
                    // ) ENGINE=MyISAM DEFAULT CHARSET=latin1;
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            i++;
                        }
                        con.Close();
                    }
                    if (i > 0) 
                        return true;
                    else
                        return false;
                }
                catch (Exception e)
                {
                    Log.add("Erreur : " + e.Message);
                    return false;
                }
            }
            else
                return false;
        }

        /// <summary>
        /// //select, pour la verif de l'existence de ces lignes dans la table article_type dans la base de l'armoire
        /// </summary>
        /// <returns></returns>
        public IEnumerable<int> synchroListArticletypeInArticleTaille()
        {
            if (IsAvailable())
            {
                List<int> list_id_article_type = new List<int>();
                string query = "SELECT `Article_Type_ID` FROM `article_taille`";
                int i = 0;
                try
                {
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            int arttypid = int.Parse(dataReader.GetString("Article_Type_ID"));
                            list_id_article_type.Add(arttypid);
                            i++;
                        }
                        con.Close();
                    }
                    if ((i > 0) && (list_id_article_type.Count() == i))
                        return list_id_article_type;
                    else
                        return new List<int>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur synchroListArticletypeInArticleTaille: " + e.Message);
                    return new List<int>();
                }
            }
            else
                return new List<int>();
        }

        /// <summary>
        /// //select, pour la verif de l'existence de ces lignes dans la table article_type dans la base de l'armoire
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> synchroListTailleInArticleTaille(int Articletype)
        {
            if (IsAvailable())
            {
                List<string> list_taille = new List<string>();
                string query = "SELECT `Taille` FROM `article_taille` WHERE `Article_Type_ID`='" + Articletype.ToString()+"';";
                int i = 0;
                try
                {
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            string arttailtail = dataReader.GetString("Taille");
                            list_taille.Add(arttailtail);
                            i++;
                        }
                        con.Close();
                    }
                    if ((i > 0) && (list_taille.Count() == i))
                        return list_taille;
                    else
                        return new List<string>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur synchroListTailleInArticleTaille: " + e.Message);
                    return new List<string>();
                }
            }
            else
                return new List<string>();
        }

        /// <summary>
        /// //select, pour la verif de l'existence de ces lignes dans la table article_type dans la base de l'armoire
        /// </summary>
        /// <returns></returns>
        public IEnumerable<int> synchroListArmoiresInArticleTaille(int Articletype, string taille)
        {
            if (IsAvailable())
            {
                List<int> list_armoire = new List<int>();
                string query = "SELECT `Armoire` FROM `article_taille` WHERE `Article_Type_ID`='" + Articletype.ToString()+"' AND `Taille`='"+taille.ToString()+ "';";
                int i = 0;
                try
                {
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            int arttypid = int.Parse(dataReader.GetString("Armoire"));
                            list_armoire.Add(arttypid);
                            i++;
                        }
                        con.Close();
                    }
                    if ((i > 0) && (list_armoire.Count() == i))
                        return list_armoire;
                    else
                        return new List<int>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur synchroListArmoiresInArticleTaille: " + e.Message);
                    return new List<int>();
                }
            }
            else
                return new List<int>();
        }


        public void synchroInsertArticletaille(int arttypid, string taille, int armoire)
        {
            if (IsAvailable())
            {
                List<Article_Taille> list_id_article_type = new List<Article_Taille>();
                string query = "INSERT INTO `article_taille` ( `Article_Type_ID`, `Taille`, `Armoire`, `Vide`) "+
                    "VALUES ( '"+ arttypid.ToString() + "', '" + taille + "', '" + armoire.ToString() + "','1');";
                //Log.add(query);

                try
                {
                    // CREATE TABLE IF NOT EXISTS `article_taille` (
                    // `Article_Type_ID` int(8) NOT NULL COMMENT 'Id du type article correspondant au vêtement',
                    // `Taille` varchar(255) NOT NULL COMMENT 'Taille pour l''article donné',
                    // `Armoire` int(8) NOT NULL COMMENT 'Numérotation de l''armoire',
                    // `Vide` tinyint(1) NOT NULL COMMENT 'indique si le type d''article/taille est épuisé',
                    // PRIMARY KEY (`Article_Type_ID`,`Taille`,`Armoire`)
                    // ) ENGINE=MyISAM DEFAULT CHARSET=latin1;
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        commande.ExecuteNonQuery();
                        con.Close();                    
                    }

                }
                catch (Exception e)
                {
                    Log.add("Erreur inserting Article_Taille : " + e.Message);
                }
            }
            else
            {
                Log.add("Erreur Acces BDD Locale" );
            }
               
        }

        /// <summary>
        /// //select, pour la verif de l'existence de ces lignes dans la table article_type dans la base de l'armoire
        /// </summary>
        /// <returns></returns>
        public int GetNumberOfArticleTypeInLogEpcSinceMonday(int artid, int userid)
        {
            int nbr = 0;
            if (IsAvailable())
            {
                //DateTime dt = new DateTime();
                //dt= DateTime.Now;
                int nbday = (Int32)DateTime.Now.DayOfWeek; 
                /*string jour = dt.ToString("ddd");
                if (jour == "Mon")
                    nbday = 1;
                else if (jour == "Thu")
                    nbday = 2;
                else if (jour == "Wen")
                    nbday = 3;
                else if (jour == "Tur")
                    nbday = 4;
                else if (jour == "Fri")
                    nbday = 5;
                else if (jour == "Sat")
                    nbday = 6;
                else// if (jour == "Sun")
                    nbday = 7;*/

                string query = "SELECT `Id` FROM `log_epc` WHERE `Article_Type_Id`= '" + artid.ToString() +
                    "' AND `Last_User`= '" + userid.ToString() + "' AND (`State` = 1 OR `State` = 4 )" +
                    " AND `Date_Creation` >= DATE_SUB(NOW(), INTERVAL " + nbday.ToString() +" DAY);";
                try
                {
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            nbr++;
                        }
                        con.Close();
                    }
                }
                catch (Exception e)
                {
                    Log.add("Erreur GetNumberOfArticleTypeInLogEpcSinceMonday: " + e.Message);
                }
            }

            return nbr; 
        }       
        
        /// <summary>
        /// //select, pour la verif de l'existence de ces lignes dans la table article_type dans la base de l'armoire
        /// </summary>
        /// <returns></returns>
        public int GetNumberOfArticleTypeInLogEpcForLast7days(int artid, int userid)
        {
            int nbr = 0;

            if (IsAvailable())
            {
                try
                {
                    string query = "SELECT `Id` FROM `log_epc` WHERE `Article_Type_Id`= '" + artid +
                        "' AND `Last_User`= '" + userid.ToString() + "' AND (`State` = 1 OR `State` = 4)" +
                        " AND `Date_Creation` >=  DATE_SUB(NOW(), INTERVAL 7 DAY);";

                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            nbr++;
                        }
                        con.Close();
                    }

                }                    
                catch (Exception e)
                {
                    Log.add("Erreur GetNumberOfArticleTypeInLogEpcForLast7days: " + e.Message);
                }
            }

            return nbr; 
        }

        /// <summary>
        /// //select, pour la verif de l'existence de ces lignes dans la table article_type dans la base de l'armoire
        /// </summary>
        /// <returns></returns>
        public int GetNumberOfArticlesInLogEpcForLastDay(int userid)
        {
            int nbr = 0;
            int thisYear = DateTime.Now.Year;
            int thisMonth = DateTime.Now.Month;
            int thisDay = DateTime.Now.Day;
            DateTime timeToCompare = new DateTime(thisYear, thisMonth, thisDay, Properties.Settings.Default.HeureLimJour, 0, 0);
            DateTime timeToCompareBefore = timeToCompare;
            timeToCompareBefore.AddDays(-1);
            
            if (IsAvailable())
            {
                try
                {
                    
                    /*
                    if (DateTime.Now < timeToCompare)
                    {
                        timeToCompare.AddDays(-1);
                    }
                    */

                    string date2 = String.Format("{0:yyyy-MM-dd HH:mm:ss}", timeToCompare);

                    string date1 = String.Format("{0:yyyy-MM-dd HH:mm:ss}", timeToCompareBefore);

                    string query = "SELECT `Tag` FROM `log_epc` WHERE `Last_User`= '" + userid.ToString() +
                        "' AND `State` = 100" +
                        " AND (`Date_Creation` BETWEEN ('" + date2 + "' - INTERVAL 1 DAY) AND '" + date2 + "');";

                    List<string> ListTag = new List<string>();

                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            string tag = dataReader.GetString("Tag");
                            ListTag.Add(tag);
                            nbr++;
                        }
                        con.Close();
                    }

                    string querytime;

                    if (timeToCompare>DateTime.Now)
                        querytime = "BETWEEN ('" + date2 + "' - INTERVAL 1 DAY) AND '" + date2 + "'";
                    else
                        querytime = "BETWEEN '" + date2 + "' AND ('" + date2 + "' + INTERVAL 1 DAY)";
                    

                    query = "SELECT `Tag` FROM `epc` WHERE (`Last_User`= '" + userid.ToString() +
                        "' AND ((`State` != 100 AND `State` != 300 AND `State` != 5)  AND (`Date_Modification` "+querytime+") ) "+
                        "OR `Tag` IN ( SELECT `Tag` FROM `log_epc` WHERE `Last_User`= '" + userid.ToString() +
                        "' AND (`State` = 1 OR `State` = 4) AND (`Date_Creation` " + querytime + ") ))";

                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            string tag = dataReader.GetString("Tag");
                            if (!ListTag.Contains(tag))
                            {
                                ListTag.Add(tag);
                                nbr++;
                            }
                        }
                        con.Close();
                    }

                    return nbr;
                }
                catch (Exception e)
                {
                    Log.add("Erreur GetNumberOfArticleInLogEpcForLastDay: " + e.Message);
                }
            }

            return nbr;
        } 
        
        /// <summary>
        /// //select, pour la verif de l'existence de ces lignes dans la table article_type dans la base de l'armoire
        /// </summary>
        /// <returns></returns>
        public IEnumerable<int> synchroArticletype()
        {
            if (IsAvailable())
            {
                List<int> list_id_article_type = new List<int>();
                string query = "SELECT `Id` FROM `article_type`";
                int i = 0;
                try
                {
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            int arttypid = int.Parse(dataReader.GetString("Id"));
                            list_id_article_type.Add(arttypid);
                            i++;
                        }
                        con.Close();
                    }
                    if ((i > 0) && (list_id_article_type.Count() == i))
                        return list_id_article_type;
                    else
                        return new List<int>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur synchroArticletype: " + e.Message);
                    return new List<int>();
                }
            }
            else
                return new List<int>();


        }


        
        /// <summary>
        /// utilisée pour inserer dans l'armoire un article type depuis base d'en haut V2
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cre"></param>
        /// <param name="mod"></param>
        /// <param name="code"></param>
        /// <param name="desc"></param>
        /// <param name="color"></param>
        /// <param name="sexe"></param>
        /// <param name="photo"></param>
        /// <param name="act"></param>
        public void synchroInsertArticletype(int id, DateTime cre, DateTime mod, string code, string desc, string color, string sexe, string photo, int act)
        {
            if (IsAvailable())
            {
                DateTime date_creation;
                DateTime date_modification;

                DateTime time_reference = (new DateTime(1971, 01, 01));

                if (cre > time_reference)
                    date_creation = cre;
                else
                    date_creation = time_reference;

                if (mod > time_reference)
                    date_modification = mod;
                else
                    date_modification = time_reference;

                int i = 0;
                int result = 0;
                string query;
                try
                {
                    string req_verif = "SELECT `Id` FROM `article_type` WHERE `Id` = '" + id.ToString()+"';";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = req_verif;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            int arttypid = int.Parse(dataReader.GetString("Id"));
                            i++;
                        }
                        con.Close();
                    }
                    if (i == 0)
                    {
                        query = "INSERT INTO `article_type` (`Id`, `Date_Creation`, `Date_Modification`, `Code`, `Type_Taille`, `Description`,";
                        query += " `Couleur`, `Sexe`, `Photo`, `Active`, `Description_longue`) VALUES ";
                        query += "('" + id.ToString() + "','" + date_creation.ToString("yyyy-MM-dd HH:mm:ss") + "','" + 
                            date_modification.ToString("yyyy-MM-dd HH:mm:ss") + "','" + code + "','1','" + desc + "','" + color + "','" + sexe + 
                            "','" + photo + "'," + act.ToString() + ",'')";
                        using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                        {

                            MySqlCommand commande = con.CreateCommand();
                            commande.CommandText = query;

                            con.Open();
                            result = commande.ExecuteNonQuery();
                            con.Close();
                        }
                        if (result != 1)
                            throw (new Exception("requete non reussie (" + query + ")"));
                    }
                    else
                        throw (new Exception("Un article_type d'Id=" + id.ToString() + " est deja present dans la base locale"));
                }
                catch (Exception e)
                {
                    Log.add("Erreur synchroInsertArticletype: " + e.Message);
                    if (Properties.Settings.Default.UseDBGMSG)
                    {
                        MessageBox.Show("Erreur lors de la synchronisation de l'armoire: " + e.Message);
                    }
                }
            }
        }


        /// <summary>
        /// utilisée pour inserer dans l'armoire un article type depuis la base d'en haut V3
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cre"></param>
        /// <param name="mod"></param>
        /// <param name="code"></param>
        /// <param name="desc"></param>
        /// <param name="color"></param>
        /// <param name="sexe"></param>
        /// <param name="photo"></param>
        /// <param name="act"></param>
        /// <param name="typetaille"></param>
        /// <param name="desclong"></param>
        public void synchroInsertArticletype(int id, DateTime cre, DateTime mod, string code, string desc, string color, string sexe, string photo, int act, string typetaille, string desclong)
        {
            if (IsAvailable())
            {
                DateTime date_creation;
                DateTime date_modification;

                DateTime time_reference = (new DateTime(1971, 01, 01));
                
                int i = 0;
                int result = 0;
                string query;
                try
                {
                    /*
                    string req_verif = "SELECT `Id` FROM `article_type` WHERE `Id` = '" + id.ToString()+"';";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = req_verif;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            int arttypid = int.Parse(dataReader.GetString("Id"));
                            i++;
                        }
                        con.Close();
                    }
                     * */
                    if (i == 0)
                    {
                        if (cre > time_reference)
                            date_creation = cre;
                        else
                            date_creation = time_reference;

                        if (mod > time_reference)
                            date_modification = mod;
                        else
                            date_modification = time_reference;

                        query = "REPLACE INTO `article_type` (`Id`, `Date_Creation`, `Date_Modification`, `Code`, `Type_Taille`, `Description`, "+
                            "`Couleur`, `Sexe`, `Photo`, `Active`, `Description_longue`) VALUES ";
                        query += "('" + id.ToString() + "','" + date_creation.ToString("yyyy-MM-dd HH:mm:ss") + "','" + 
                            date_modification.ToString("yyyy-MM-dd HH:mm:ss") + "','" + code + "','"+ typetaille + "','" + desc + "','" + color + "','" + 
                            sexe + "','" + photo + "','" + act.ToString() + "','"+desclong+"')";
                        using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                        {

                            MySqlCommand commande = con.CreateCommand();
                            commande.CommandText = query;

                            con.Open();
                            result = commande.ExecuteNonQuery();
                            con.Close();
                        }
                        //if (result != 1)
                            //throw (new Exception("requete non reussie (" + query + ")"));
                    }
                    else
                        throw (new Exception("Un article_type d'Id=" + id.ToString() + " est deja present dans la base locale"));
                }
                catch (Exception e)
                {
                    Log.add("Erreur synchroInsertArticletype: " + e.Message);
                    if (Properties.Settings.Default.UseDBGMSG)
                    {
                        MessageBox.Show("Erreur lors de la synchronisation de l'armoire: " + e.Message);
                    }
                }
            }
        }

        //Mets a jour les modifications de l'armoire venant du dédié V2
        public void synchroUpdateArticletype(int id, string code, string desc, string color, string sexe, string photo, int act)
        {
            if (IsAvailable())
            {
                int result = 0;
                try
                {
                    string query = "UPDATE `article_type` SET `Code` = '" + code + "', `Description` = '" + desc + "', `Couleur` = '" + color +
                        "', `Sexe` = '" + sexe + "', `Photo` = '" + photo + "', `Active` = '" + act.ToString() + "' WHERE `Id` = '" + id.ToString()+"';";

                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        result = commande.ExecuteNonQuery();
                        con.Close();
                    }
                    if (result != 1)
                        throw (new Exception("requete non reussie (" + query + ")"));
                }
                catch (Exception e)
                {
                    Log.add("Erreur synchroUpdateArticletype: " + e.Message);
                    if (Properties.Settings.Default.UseDBGMSG)
                    {
                        MessageBox.Show("Erreur lors de la synchronisation de l'armoire: " + e.Message);
                    }
                }
            }
        }

        //Mets a jour les modifications de l'armoire venant du dédié V3
        public void synchroUpdateArticletype(int id, string code, string desc, string color, string sexe, string photo, int act,string typetaille, string desclong)
        {
            if (IsAvailable())
            {
                int result = 0;
                try
                {
                    string query = "UPDATE `article_type` SET `Code` = '" + code + "', `Type_Taille` = '" + typetaille + "', `Description` = '" + desc +
                        "', `Couleur` = '" + color + "', `Sexe` = '" + sexe + "', `Photo` = '" + photo + "', `Active` = '" + act.ToString() +
                        "', `Description_longue` = '" + desclong + "'  WHERE `Id` = '" + id.ToString()+"';";

                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        result = commande.ExecuteNonQuery();
                        con.Close();
                    }
                    if (result != 1)
                        throw (new Exception("requete non reussie (" + query + ")"));
                }
                catch (Exception e)
                {
                    Log.add("Erreur synchroUpdateArticletype: " + e.Message);
                    if (Properties.Settings.Default.UseDBGMSG)
                    {
                        MessageBox.Show("Erreur lors de la synchronisation de l'armoire: " + e.Message);
                    }
                }
            }
        }


        public IEnumerable<int> synchroUser()
        {
            if (IsAvailable())
            {
                List<int> list_id_user = new List<int>();
                string query = "SELECT `Id` FROM `user`";
                int i = 0;
                try
                {
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            int userid = int.Parse(dataReader.GetString("Id"));
                            list_id_user.Add(userid);
                            i++;
                        }
                        con.Close();
                    }
                    if ((i > 0) && (list_id_user.Count() == i))
                        return list_id_user;
                    else
                        return new List<int>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur synchrouser: " + e.Message);
                    return new List<int>();
                }
            }
            else
                return new List<int>();
        }


        //insere dans l'armoire les utilisateurs

        // v2 avec addon v3 sans Id_Lang
        public void synchroInsertUser(int id, DateTime cre, DateTime mod, string log, string pass, string type, string nom, string pren, string sexe, string taille, int group, int dep, string photo, DateTime connec, int act)
        {
            if (IsAvailable())
            {
                DateTime date_creation;
                DateTime date_modification;
                DateTime last_connection;
                DateTime time_reference = new DateTime(1971, 01, 01);

                DateTime eov = time_reference;

                int i = 0;
                int result = 0;
                string query;
                try
                {
                    /*
                    string req_verif = "SELECT `Id` FROM `user` WHERE `Id` = '" + id.ToString()+"';";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = req_verif;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            int arttypid = int.Parse(dataReader.GetString("Id"));
                            i++;
                        }
                        con.Close();
                    }
                     * */
                    if (i == 0)
                    {

                        if (cre > time_reference)
                            date_creation = cre;
                        else
                            date_creation = time_reference;

                        if (mod > time_reference)
                            date_modification = mod;
                        else
                            date_modification = time_reference;

                        if (connec > time_reference)
                            last_connection = connec;
                        else
                            last_connection = time_reference;

                        query = "REPLACE INTO `user` (`Id`, `Date_Creation`, `Date_Modification`, `Login`, `Password`, `Type`, `Nom`, `Prenom`, `Sexe`,"+
                            " `Taille`, `Groupe`, `Department`, `Photo`, `Last_Connection`, `Active`, `End_of_Validity`, `Wearer_Code`) ";
                        query += "VALUES ('" + id.ToString() + "', '" + date_creation.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + 
                            date_modification.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + log + "', '" + pass + "', '" + type + "', '" + nom + "', '" +
                            pren + "', '" + sexe + "', '" + taille + "', '" + group.ToString() + "', '" + dep.ToString() + "', '" + photo + "', '" + 
                            last_connection.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + act.ToString() + "', '" + 
                            eov.ToString("yyyy-MM-dd HH:mm:ss") + "', '');";
                        using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                        {

                            MySqlCommand commande = con.CreateCommand();
                            commande.CommandText = query;

                            con.Open();
                            result = commande.ExecuteNonQuery();
                            con.Close();
                        }
                        //if (result != 1)
                            //throw (new Exception("requete non reussie (" + query + ")"));
                    }
                    else
                        throw (new Exception("Un user d'Id=" + id.ToString() + " est deja présent dans la base locale"));
                }
                catch (Exception e)
                {
                    Log.add("Erreur synchroInsertUser: " + e.Message);
                    if (Properties.Settings.Default.UseDBGMSG)
                    {
                        MessageBox.Show("Erreur lors de la synchronisation de l'armoire: " + e.Message);
                    }
                }
            }
        }

        //v3 sans Id_Lang
        public void synchroInsertUser(int id, DateTime cre, DateTime mod, string log, string pass, string type, string nom, string pren, string sexe, string taille, int group, int dep, string photo, DateTime connec, int act, DateTime eov, string wearercode)
        {
            if (IsAvailable())
            {
                DateTime date_creation;
                DateTime date_modification;
                DateTime last_connection;
                DateTime time_reference = new DateTime(1971, 01, 01);

                DateTime end_of_validity;

                int i = 0;
                int result = 0;
                string query;
                try
                {
                    /*
                    string req_verif = "SELECT `Id` FROM `user` WHERE `Id` = '" + id.ToString()+"';";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = req_verif;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            int arttypid = int.Parse(dataReader.GetString("Id"));
                            i++;
                        }
                        con.Close();
                    }
                     * */
                    if (i == 0)
                    {

                        if (cre > time_reference)
                            date_creation = cre;
                        else
                            date_creation = time_reference;

                        if (mod > time_reference)
                            date_modification = mod;
                        else
                            date_modification = time_reference;

                        if (connec > time_reference)
                            last_connection = connec;
                        else
                            last_connection = time_reference;

                        if (eov > time_reference)
                            end_of_validity = eov;
                        else
                            end_of_validity = time_reference;

                        query = "REPLACE INTO `user` (`Id`, `Date_Creation`, `Date_Modification`, `Login`, `Password`, `Type`, `Nom`, `Prenom`, `Sexe`, "+
                            "`Taille`, `Groupe`, `Department`, `Photo`, `Last_Connection`, `Active`, `End_of_Validity`, `Wearer_Code`) ";
                        query += "VALUES ('" + id.ToString() + "', '" + date_creation.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + 
                            date_modification.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + log + "', '" + pass + "', '" + type + "', '" + nom + "', '" +
                            pren + "', '" + sexe + "', '" + taille + "', '" + group.ToString() + "', '" + dep.ToString() + "', '" + photo + "', '" + 
                            last_connection.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + act.ToString() + "', '" + end_of_validity.ToString("yyyy-MM-dd HH:mm:ss") +
                            "', '" + wearercode + "');";
                        using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                        {

                            MySqlCommand commande = con.CreateCommand();
                            commande.CommandText = query;

                            con.Open();
                            result = commande.ExecuteNonQuery();
                            con.Close();
                        }
                        //if (result != 1)
                            //throw (new Exception("requete non reussie (" + query + ")"));
                    }
                    else
                        throw (new Exception("Un user d'Id=" + id.ToString() + " est deja présent dans la base locale"));
                }

                catch (Exception e)
                {
                    Log.add("Erreur synchroInsertUser: " + e.Message);
                    if (Properties.Settings.Default.UseDBGMSG)
                    {
                        MessageBox.Show("Erreur lors de la synchronisation de l'armoire: " + e.Message);
                    }
                }
            }
        }

        //Mets a jour les modifications de l'armoire venant du dédié sans Id_Lang
        public void synchroUpdateUser(int id, string log, string pass, string type, string nom, string pren, string sexe, string taille, int group, int dep, string photo, int act)
        {
            if (IsAvailable())
            {
                int result = 0;

                try
                {
                    string query = "UPDATE `user` SET `Login` = '" + log + "', `Password` = '" + pass + "', `Type` = '" + type + "', `Nom` = '" + nom +
                        "', `Prenom` = '" + pren + "', `Sexe` = '" + sexe + "', `Taille` = '" + taille + "', `Groupe` = '" + group.ToString() +
                        "', `Department` = '" + dep.ToString() + "', `Photo` = '" + photo + "', `Active` = '" + act.ToString() + "' WHERE `Id` = '" + 
                        id.ToString()+"';";

                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        result = commande.ExecuteNonQuery();
                        con.Close();
                    }
                    if (result != 1)
                        throw (new Exception("requete non reussie (" + query + ")"));
                }
                catch (Exception e)
                {
                    Log.add("Erreur synchroUpdateUser: " + e.Message);
                    if (Properties.Settings.Default.UseDBGMSG)
                    {
                        MessageBox.Show("Erreur lors de la synchronisation de l'armoire: " + e.Message);
                    }
                }
            }
        }

        // v3 sans Id_Lang
        public void synchroUpdateUser(int id, string log, string pass, string type, string nom, string pren, string sexe, string taille, int group, int dep, string photo, int act, DateTime eov, string wearercode)
        {
            if (IsAvailable())
            {
                DateTime end_of_validity;
                DateTime time_reference = new DateTime(1971, 01, 01);
                int result = 0;

                try
                {
                    if (eov > time_reference)
                        end_of_validity = eov;
                    else
                        end_of_validity = time_reference;

                    string query = "UPDATE `user` SET `Login` = '" + log + "', `Password` = '" + pass + "', `Type` = '" + type + "', `Nom` = '" + nom +
                        "', `Prenom` = '" + pren + "', `Sexe` = '" + sexe + "', `Taille` = '" + taille + "', `Groupe` = '" + group.ToString() +
                        "', `Department` = '" + dep.ToString() + "', `Photo` = '" + photo + "', `Active` = '" + act.ToString() + "', `End_of_Validity` = '" +
                        end_of_validity.ToString("yyyy-MM-dd HH:mm:ss") + "', `Wearer_Code` = '" + wearercode + "' WHERE `Id` = '" + id.ToString() +"';";

                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        result = commande.ExecuteNonQuery();
                        con.Close();
                    }
                    if (result != 1)
                        throw (new Exception("requete non reussie (" + query + ")"));
                }
                catch (Exception e)
                {
                    Log.add("Erreur synchroUpdateUser: " + e.Message);
                    if (Properties.Settings.Default.UseDBGMSG)
                    {
                        MessageBox.Show("Erreur lors de la synchronisation de l'armoire: " + e.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Retourne une liste d'id présents dans la table user_article de la BDD locale
        /// </summary>
        /// <returns></returns>
        public IEnumerable<int> synchroUserArtGetIds()
        {
            if (IsAvailable())
            {
                List<int> list_id_user_article = new List<int>();
                string query = "SELECT `Id` FROM `user_article`";
                int i = 0;
                try
                {
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            int userid = int.Parse(dataReader.GetString("Id"));
                            list_id_user_article.Add(userid);
                            i++;
                        }
                        con.Close();
                    }
                    if ((i > 0) && (list_id_user_article.Count() == i))
                        return list_id_user_article;
                    else
                        return new List<int>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur synchroUserArt: " + e.Message);
                    return new List<int>();
                }
            }
            else
                return new List<int>();
        }

        public IEnumerable<User_Article> synchroUserArtGetListForUser(int userid)
        {
            
            if (IsAvailable()&&getUserById(userid).FirstOrDefault().Type!="reloader")
            {
                List<User_Article> list_user_article = new List<User_Article>();
                try
                {
                    list_user_article = getUser_articleByUserId(userid).ToList();
                    if (list_user_article != null)
                        return list_user_article;
                    else
                        throw (new Exception("pas de user_article d'Id=" + userid.ToString() + " present dans la base"));
                }
                catch (Exception e)
                {
                    Log.add("Erreur synchroUserArtDate: " + e.Message);
                    return new List<User_Article>();
                }
            }
            else
                return new List<User_Article>();
        }


        public void synchroInsertUserArt(int id, DateTime cre, DateTime mod, string taille, int cred, int credrest, int userid, int articletypeid)
        {
            if (IsAvailable())
            {
                DateTime date_creation;
                DateTime date_modification;
                DateTime time_reference = new DateTime(1971, 01, 01);

                int i = 0;
                int result = 0;
                string query;
                try
                {
                    /*
                    string req_verif = "SELECT `Id` FROM `user_article` WHERE `Id` = '" + id.ToString()+"';";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = req_verif;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            int arttypid = int.Parse(dataReader.GetString("Id"));
                            i++;
                        }
                        con.Close();
                    }*/
                    if (i == 0)
                    {

                        if (cre > time_reference)
                            date_creation = cre;
                        else
                            date_creation = time_reference;

                        if (mod > time_reference)
                            date_modification = mod;
                        else
                            date_modification = time_reference;

                        query = "REPLACE INTO `user_article` (`Id`, `Date_Creation`, `Date_Modification`, `Taille`, `Credit`, `Credit_Restant`, `Credit_Semaine_Suivante`, `User_Id`, " +
                            "`Article_Type_Id`) ";
                        query += "VALUES ('" + id.ToString() + "', '" + date_creation.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + 
                            date_modification.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + taille + "', '" + cred.ToString() + "', '" + 
                            credrest.ToString() + "', 0, '" + userid.ToString() + "', '" + articletypeid.ToString() + "');";
                        using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                        {

                            MySqlCommand commande = con.CreateCommand();
                            commande.CommandText = query;

                            con.Open();
                            result = commande.ExecuteNonQuery();
                            con.Close();
                        }
                        //if (result != 1)
                            //throw (new Exception("requete non reussie (" + query + ")"));
                    }
                    //else
                    //    throw (new Exception("Un user_article d'Id=" + id.ToString() + " est dejà present dans la base locale"));
                }
                catch (Exception e)
                {
                    Log.add("Erreur synchroInsertUserArt: " + e.Message);
                    if (Properties.Settings.Default.UseDBGMSG)
                    {
                        MessageBox.Show("Erreur lors de la synchronisation de l'armoire: " + e.Message);
                    }
                }
            }
        }

        //Mets a jour les modifications de l'armoire venant du dédié
        public void synchroUpdateUserART(int id, DateTime mod, string taille, int cred, int credres)
        {
            if (IsAvailable())
            {
                try
                {
                    int i = 0;
                    int cred_rest_before = int.MaxValue;
                    string req_controle = "SELECT `Id`, `Credit_Restant` FROM `user_article` WHERE `Id` = '" + id.ToString() + "' LIMIT 1;";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = req_controle;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            cred_rest_before = int.Parse(dataReader.GetString("Credit_Restant"));
                            i++;
                        }
                        con.Close();
                    }
                    if ((i == 1) && (cred_rest_before != int.MaxValue))
                    {

                        Log.add("Update synchro user_article local " + id.ToString() + ": Credits restants = " + cred_rest_before.ToString() + " => Credits restants = " + credres.ToString());

                        int result = 0;

                        DateTime date_modification;
                        DateTime time_reference = new DateTime(1971, 01, 01);

                        if (mod > time_reference)
                            date_modification = mod;
                        else
                            date_modification = time_reference;

                        string query = "UPDATE `user_article` SET `Date_Modification` = '" + date_modification.ToString("yyyy-MM-dd HH:mm:ss") +
                            "', `Taille` = '" + taille + "', `Credit` = '" + cred.ToString() + "', `Credit_Restant` = '" + credres.ToString() +
                            "' WHERE `Id` = '" + id.ToString() +"';";

                        using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                        {

                            MySqlCommand commande = con.CreateCommand();
                            commande.CommandText = query;

                            con.Open();
                            result = commande.ExecuteNonQuery();
                            con.Close();
                        }
                        if (result != 1)
                            throw (new Exception("requete non reussie (" + query + ")"));

                        i = 0;
                        int cred_rest_after = int.MaxValue;
                        req_controle = "SELECT `Id`, `Credit_Restant` FROM `user_article` WHERE `Id` = '" + id.ToString() + "' LIMIT 1;";
                        using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                        {

                            MySqlCommand commande = con.CreateCommand();
                            commande.CommandText = req_controle;

                            con.Open();
                            MySqlDataReader dataReader = commande.ExecuteReader();
                            while (dataReader.Read())
                            {
                                cred_rest_after = int.Parse(dataReader.GetString("Credit_Restant"));
                                i++;
                            }
                            con.Close();
                        }
                        if ((i == 1) && (cred_rest_after != int.MaxValue))
                        {
                            Log.add("Update synchro user_article local " + id.ToString() + ": Credits restants = " + cred_rest_after.ToString());
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.add("Erreur synchroUpdateUserART: " + e.Message);
                    if (Properties.Settings.Default.UseDBGMSG)
                    {
                        MessageBox.Show("Erreur lors de la synchronisation de l'armoire: " + e.Message);
                    }
                }
                
            }
        }

        public IEnumerable<Epc> synchroEPCDate(int id)
        {
            if (IsAvailable())
            {
                List<Epc> list_epc = new List<Epc>();
                int i = 0;
                try
                {
                    string query = "SELECT `Id`, `Date_Creation`, `Date_Modification`, `Tag`, `Code_Barre`, `Taille`, `Type_Taille`, `Cycle_Lavage_Count`, "+
                        "`State`, `Last_User`, `Last_Reader`, `Last_Action`, `Last_Action_Date`, `Movement`, `Article_Type_ID`, `Case_ID`, `Armoire_ID`, "+
                        "`Actif` FROM `epc` WHERE `Id` = '" + id.ToString() + "' LIMIT 1;";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            int epcid = int.Parse(dataReader.GetString("Id"));
                            DateTime date_crea = dataReader.GetDateTime("Date_Creation");
                            DateTime date_modif = dataReader.GetDateTime("Date_Modification");
                            string tag = dataReader.GetString("Tag");
                            string code_barre = dataReader.GetString("Code_Barre");
                            string taille = dataReader.GetString("Taille");
                            string type_taille = dataReader.GetString("Type_Taille");
                            int cycle_lav_count = int.Parse(dataReader.GetString("Cycle_Lavage_Count"));
                            int state = int.Parse(dataReader.GetString("State"));
                            string last_user = dataReader.GetString("Last_User");
                            string lastreader = dataReader.GetString("Last_Reader");
                            string lastaction = dataReader.GetString("Last_Action");
                            DateTime lastactiondate = dataReader.GetDateTime("Last_Action_Date");
                            int movement = int.Parse(dataReader.GetString("Movement"));
                            int artypid = int.Parse(dataReader.GetString("Article_Type_ID"));
                            int caseid = int.Parse(dataReader.GetString("Case_ID"));
                            int armoireid = int.Parse(dataReader.GetString("Armoire_ID"));
                            int actif = int.Parse(dataReader.GetString("Actif"));

                            Epc epc_tmp = new Epc(epcid,date_crea,date_modif, tag, code_barre, taille, type_taille, cycle_lav_count, state, last_user, lastreader, lastaction, lastactiondate, movement, artypid, caseid, armoireid, actif);

                            list_epc.Add(epc_tmp);
                            i++;
                        }
                        con.Close();
                    }
                    if ((i > 0) && (list_epc.Count() == i))
                        return list_epc;
                    else
                        return new List<Epc>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur synchroEPCDate: " + e.Message);
                    return new List<Epc>();
                }
            }
            else
                return new List<Epc>();


        }


        public IEnumerable<int> synchroEPC()
        {
            if (IsAvailable())
            {
                List<int> list_id_epc = new List<int>();
                string query = "SELECT `Id` FROM `epc`";
                int i = 0;
                try
                {
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            int epcid = int.Parse(dataReader.GetString("Id"));
                            list_id_epc.Add(epcid);
                            i++;
                        }
                        con.Close();
                    }
                    if ((i > 0) && (list_id_epc.Count() == i))
                        return list_id_epc;
                    else
                        return new List<int>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur synchroEPC: " + e.Message);
                    return new List<int>();
                }
            }
            else
                return new List<int>();
        }

        // v2 addon v3
        public void synchroInsertEPC(int id, DateTime cre, DateTime mod, string tag, string cb, string taille, string type_taille, int lavage, int state, string lastuser, string lastreader, string lastaction, DateTime lastactdate, int movement, int articletypeid, int caseid, int armoireid)
        {
            if (IsAvailable())
            {
                DateTime time_reference = (new DateTime(1971, 01, 01));
                DateTime last_action_date;
                DateTime date_modification;
                DateTime date_creation;

                int i = 0;
                int result = 0;
                string query;
                try
                {
                    string req_verif = "SELECT `Id` FROM `epc` WHERE `Id` = '" + id.ToString()+"';";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = req_verif;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            int epcid = int.Parse(dataReader.GetString("Id"));
                            i++;
                        }
                        con.Close();
                    }
                    if (i == 0)
                    {

                        if (cre > time_reference)
                            date_creation = cre;
                        else
                            date_creation = time_reference;

                        if (mod > time_reference)
                            date_modification = mod;
                        else
                            date_modification = time_reference;

                        if (lastactdate > time_reference)
                            last_action_date = lastactdate;
                        else
                            last_action_date = time_reference;

                        string actif = "1";

                        query = "INSERT INTO `epc`(`Id`, `Date_Creation`, `Date_Modification`, `Tag`, `Code_Barre`, `Taille`, `Type_Taille`, "+
                            " `Cycle_Lavage_Count`, `State`, `Last_User`, `Last_Reader`, `Last_Action`, `Last_Action_Date`, `Movement`, "+
                            "`Article_Type_ID`, `Case_ID`, `Armoire_ID`, `Actif`) ";
                        query += "VALUES ('" + id.ToString() + "', '" + date_creation.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + 
                            date_modification.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + tag + "', '" + cb + "', '" + taille + "', '" + type_taille + "', '" + 
                            lavage.ToString() + "', '" + state.ToString() + "', '" + lastuser + "', '" + lastreader + "', '" + lastaction + "', '" + 
                            last_action_date.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + movement.ToString() + "', '" + articletypeid.ToString() + "', '" + 
                            caseid.ToString() + "', '" + armoireid.ToString() + "', '" + actif + "');";
                        using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                        {

                            MySqlCommand commande = con.CreateCommand();
                            commande.CommandText = query;

                            con.Open();
                            result = commande.ExecuteNonQuery();
                            con.Close();
                        }
                        if (result != 1)
                            throw (new Exception("requete non réussie (" + query + ")"));
                    }
                    else
                        throw (new Exception("Un epc d'Id=" + id.ToString() + " est deje present dans la base locale"));
                }
                catch (Exception e)
                {
                    Log.add("Erreur synchroInsertEPC: " + e.Message);
                    if (Properties.Settings.Default.UseDBGMSG)
                    {
                        MessageBox.Show("Erreur lors de la synchronisation de l'armoire: " + e.Message);
                    }
                }
            }
        }

        //v3
        public void synchroInsertEPC(int id, DateTime cre, DateTime mod, string tag, string cb, string taille, string type_taille, int lavage, int state, string lastuser, string lastreader, string lastaction, DateTime lastactdate, int movement, int articletypeid, int caseid, int armoireid, int actif)
        {
            if (IsAvailable())
            {
                DateTime time_reference = (new DateTime(1971, 01, 01));
                DateTime last_action_date;
                DateTime date_modification;
                DateTime date_creation;

                int i = 0;
                int result = 0;
                string query;
                try
                {
                    string req_verif = "SELECT `Id` FROM `epc` WHERE `Id` = '" + id.ToString()+"';";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = req_verif;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            int epcid = int.Parse(dataReader.GetString("Id"));
                            i++;
                        }
                        con.Close();
                    }
                    if (i == 0)
                    {

                        if (cre > time_reference)
                            date_creation = cre;
                        else
                            date_creation = time_reference;

                        if (mod > time_reference)
                            date_modification = mod;
                        else
                            date_modification = time_reference;

                        if (lastactdate > time_reference)
                            last_action_date = lastactdate;
                        else
                            last_action_date = time_reference;


                        query = "INSERT INTO `epc`(`Id`, `Date_Creation`, `Date_Modification`, `Tag`, `Code_Barre`, `Taille`, `Type_Taille`, "+
                            "`Cycle_Lavage_Count`, `State`, `Last_User`, `Last_Reader`, `Last_Action`, `Last_Action_Date`, `Movement`, "+
                            "`Article_Type_ID`, `Case_ID`, `Armoire_ID`, `Actif`) ";
                        query += "VALUES ('" + id.ToString() + "', '" + date_creation.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + 
                            date_modification.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + tag + "', '" + cb + "', '" + taille + "', '" + type_taille + "', '" + 
                            lavage.ToString() + "', '" + state.ToString() + "', '" + lastuser + "', '" + lastreader + "', '" + lastaction + "', '" + 
                            last_action_date.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + movement.ToString() + "', '" + articletypeid.ToString() + "', '" + 
                            caseid.ToString() + "', '" + armoireid.ToString() + "', '" + actif.ToString() + "');";
                        using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                        {

                            MySqlCommand commande = con.CreateCommand();
                            commande.CommandText = query;

                            con.Open();
                            result = commande.ExecuteNonQuery();
                            con.Close();
                        }
                        if (result != 1)
                            throw (new Exception("requete non reussie (" + query + ")"));
                    }
                    else
                        throw (new Exception("Un epc d'Id=" + id.ToString() + " est deje present dans la base locale"));
                }
                catch (Exception e)
                {
                    Log.add("Erreur synchroInsertEPC: " + e.Message);
                    if (Properties.Settings.Default.UseDBGMSG)
                    {
                        MessageBox.Show("Erreur lors de la synchronisation de l'armoire: " + e.Message);
                    }
                }
            }
        }
        
        #endregion

        #region SELECTs


        public IEnumerable<User> getUser()
        {
            if (IsAvailable())
            {
                List<User> listusr = new List<User>();
                int i = 0;
                string query = "SELECT `Id`, `Date_Creation`, `Date_Modification`, `Login`, `Password`, `Type`, `Nom`, `Prenom`, `Id_Lang`, "+
                    "`Sexe`, `Taille`, `Groupe`, `Department`, `Photo`, `Last_Connection`, `Active`, `End_of_Validity`, `Wearer_Code` FROM `user` WHERE `Active` = '1';";
                try
                {

                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            int usrid = int.Parse(dataReader.GetString("Id"));
                            int active = int.Parse(dataReader.GetString("Active"));
                            if (active != 1)
                                throw (new Exception("la valeur du champs Active pour le user d'Id=" + usrid.ToString() + " parsee ne corrrespond pas"));
                            DateTime dc = dataReader.GetDateTime("Date_Creation");
                            DateTime dm = dataReader.GetDateTime("Date_Modification");
                            string login = dataReader.GetString("Login");
                            string password = dataReader.GetString("Password");
                            string type = dataReader.GetString("Type");
                            string nom = dataReader.GetString("Nom");
                            string prenom = dataReader.GetString("Prenom");
                            int id_lang = int.Parse(dataReader.GetString("Id_Lang"));
                            string sexe = dataReader.GetString("Sexe");
                            string taille = dataReader.GetString("Taille");
                            int grp = int.Parse(dataReader.GetString("Groupe"));
                            int dpt = int.Parse(dataReader.GetString("Department"));
                            string photo = dataReader.GetString("Photo");
                            DateTime last_con = dataReader.GetDateTime("Last_Connection");
                            DateTime end_of_val = dataReader.GetDateTime("End_of_Validity");
                            string wearer_code = dataReader.GetString("Wearer_Code");
                            User usrtmp = new User(usrid, dc, dm, login, password, type, nom, prenom, id_lang, sexe, taille, grp, dpt, photo, last_con, active, end_of_val, wearer_code);
                            listusr.Add(usrtmp);
                            i++;
                        }
                        con.Close();

                        if ((i>0)&&(listusr.Count() == i))
                            return listusr;
                        else
                            return new List<User>();
                    }
                }
                catch (Exception e)
                {
                    Log.add("Erreur getUser: " + e.Message);
                    return new List<User>();
                }
            }
            else
                return new List<User>();
        }

        public IEnumerable<Content_Arm> getContent()
        {
            if (IsAvailable())
            {
                List<Content_Arm> list_content_arm = new List<Content_Arm>();
                int i = 0;
                try
                {
                    string query = "SELECT `Id`, `Creation_Date`, `Epc`, `State`, `RFID_Reader` FROM `content_arm`";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            int ca_id = int.Parse(dataReader.GetString("Id"));
                            int rfid_reader = int.Parse(dataReader.GetString("RFID_Reader"));
                            if (rfid_reader == Properties.Settings.Default.NumArmoire)
                            {
                                
                                if (rfid_reader != Properties.Settings.Default.NumArmoire)
                                    throw (new Exception("le RFID_Reader parse pour l'Id = " + ca_id + " ne corrrespond pas"));
                                DateTime date_crea = dataReader.GetDateTime("Creation_Date");
                                string epc = dataReader.GetString("Epc");
                                int state = int.Parse(dataReader.GetString("State"));

                                Content_Arm ca_tmp = new Content_Arm(ca_id, date_crea, epc, state, rfid_reader);
                                list_content_arm.Add(ca_tmp);
                                i++;
                            }
                        }
                        con.Close();
                    }
                    if ((i > 0) && (list_content_arm.Count() == i))
                        return list_content_arm;
                    else
                        return new List<Content_Arm>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur getContent: " + e.Message);
                    return new List<Content_Arm>();
                }
            }
            else
                return new List<Content_Arm>();
        }

        // retourne les epc présents dans les armoires (state 100 ou 5)
        public IEnumerable<Epc> getEPC()
        {
            if (IsAvailable())
            {
                List<Epc> list_epc = new List<Epc>();
                int i = 0;
                try
                {
                    string query = "SELECT `Id`, `Date_Creation`, `Date_Modification`, `Tag`, `Code_Barre`, `Taille`, `Type_Taille`, `Cycle_Lavage_Count`,"+
                        "`State`, `Last_User`, `Last_Reader`, `Last_Action`, `Last_Action_Date`, `Movement`, `Article_Type_ID`, `Case_ID`, `Armoire_ID`,"+
                        "`Actif` FROM `epc` WHERE (`State` = '100') OR (`State` = '5');";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            int epcid = int.Parse(dataReader.GetString("Id"));
                            DateTime date_crea = dataReader.GetDateTime("Date_Creation");
                            DateTime date_modif = dataReader.GetDateTime("Date_Modification");
                            string tag = dataReader.GetString("Tag");
                            string code_barre = dataReader.GetString("Code_Barre");
                            string taille = dataReader.GetString("Taille");
                            string type_taille = dataReader.GetString("Type_Taille");
                            int cycle_lav_count = int.Parse(dataReader.GetString("Cycle_Lavage_Count"));
                            int state = int.Parse(dataReader.GetString("State"));
                            string last_user = dataReader.GetString("Last_User");
                            string lastreader = dataReader.GetString("Last_Reader");
                            string lastaction = dataReader.GetString("Last_Action");
                            DateTime lastactiondate = dataReader.GetDateTime("Last_Action_Date");
                            int movement = int.Parse(dataReader.GetString("Movement"));
                            int artypid = int.Parse(dataReader.GetString("Article_Type_ID"));
                            int caseid = int.Parse(dataReader.GetString("Case_ID"));
                            int armoireid = int.Parse(dataReader.GetString("Armoire_ID"));
                            int actif = int.Parse(dataReader.GetString("Actif"));

                            Epc epc_tmp = new Epc(epcid, date_crea, date_modif, tag, code_barre, taille, type_taille, cycle_lav_count, state, last_user, lastreader, lastaction, lastactiondate, movement, artypid, caseid, armoireid, actif);

                            list_epc.Add(epc_tmp);
                            i++;
                        }
                        con.Close();
                    }
                    if ((i > 0) && (list_epc.Count() == i))
                        return list_epc;
                    else
                        return new List<Epc>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur getEPC: " + e.Message);
                    return new List<Epc>();
                }
            }
            else
                return new List<Epc>();
        }

        /// <summary>
        /// retourne l'epc correspondant au tag donné en argument
        /// retourne null dans le cas ou il n'est pas présent dans la bdd locale
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public Epc getEpc(string tag)
        {
            string query = "";
            if (IsAvailable())
            {
                List<Epc> list_epc = new List<Epc>();
                int i = 0;
                try
                {
                    query = "SELECT `Id`, `Date_Creation`, `Date_Modification`, `Tag`, `Code_Barre`, `Taille`, `Type_Taille`, `Cycle_Lavage_Count`," +
                        " `State`, `Last_User`, `Last_Reader`, `Last_Action`, `Last_Action_Date`, `Movement`, `Article_Type_ID`, `Case_ID`, `Armoire_ID`, " +
                        "`Actif` FROM `epc` WHERE `Tag` = '" + tag + "';";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            int epcid = int.Parse(dataReader.GetString("Id"));
                            string tag_rd = dataReader.GetString("Tag");

                            DateTime date_crea = dataReader.GetDateTime("Date_Creation");
                            DateTime date_modif = dataReader.GetDateTime("Date_Modification");

                            string code_barre = dataReader.GetString("Code_Barre");
                            string taille = dataReader.GetString("Taille");
                            string type_taille = dataReader.GetString("Type_Taille");
                            int cycle_lav_count = int.Parse(dataReader.GetString("Cycle_Lavage_Count"));
                            int state = int.Parse(dataReader.GetString("State"));
                            string last_user = dataReader.GetString("Last_User");
                            string lastreader = dataReader.GetString("Last_Reader");
                            string lastaction = dataReader.GetString("Last_Action");
                            DateTime lastactiondate = dataReader.GetDateTime("Last_Action_Date");
                            int movement = int.Parse(dataReader.GetString("Movement"));
                            int artypid = int.Parse(dataReader.GetString("Article_Type_ID"));
                            int caseid = int.Parse(dataReader.GetString("Case_ID"));
                            int armoireid = int.Parse(dataReader.GetString("Armoire_ID"));
                            int actif = int.Parse(dataReader.GetString("Actif"));

                            Epc epc_tmp = new Epc(epcid, date_crea, date_modif, tag_rd, code_barre, taille, type_taille, cycle_lav_count, state, last_user, lastreader, lastaction, lastactiondate, movement, artypid, caseid, armoireid, actif);

                            list_epc.Add(epc_tmp);
                            i++;
                        }
                        con.Close();
                    }
                    if ((i > 0) && (list_epc.Count() == 1))
                        return list_epc[0];
                    else
                    {
                        if (i > 1) throw (new Exception("Plusieurs epc pour le Tag = '" + tag + "' retournés"));
                       // if (i == 0) throw (new Exception("Tag inconnu : " + tag + " retourné sql: "+query));
                        return null;
                    }
                }
                catch (Exception e)
                {
                    Log.add("Erreur getEPC: " + e.Message);
                    return null;
                }
            }
            else
                return null;
        }

        public IEnumerable<Epc> getAllEPC()
        {
            if (IsAvailable())
            {
                List<Epc> list_epc = new List<Epc>();
                int i = 0;
                try
                {
                    string query = "SELECT `Id`, `Date_Creation`, `Date_Modification`, `Tag`, `Code_Barre`, `Taille`, `Type_Taille`, `Cycle_Lavage_Count`, "+
                        "`State`, `Last_User`, `Last_Reader`, `Last_Action`, `Last_Action_Date`, `Movement`, `Article_Type_ID`, `Case_ID`, `Armoire_ID`,"+
                        " `Actif` FROM `epc`";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            int epcid = int.Parse(dataReader.GetString("Id"));
                            string tag = dataReader.GetString("Tag");

                            DateTime date_crea = dataReader.GetDateTime("Date_Creation");
                            DateTime date_modif = dataReader.GetDateTime("Date_Modification");

                            string code_barre = dataReader.GetString("Code_Barre");
                            string taille = dataReader.GetString("Taille");
                            string type_taille = dataReader.GetString("Type_Taille");
                            int cycle_lav_count = int.Parse(dataReader.GetString("Cycle_Lavage_Count"));
                            int state = int.Parse(dataReader.GetString("State"));
                            string last_user = dataReader.GetString("Last_User");
                            string lastreader = dataReader.GetString("Last_Reader");
                            string lastaction = dataReader.GetString("Last_Action");
                            DateTime lastactiondate = dataReader.GetDateTime("Last_Action_Date");
                            int movement = int.Parse(dataReader.GetString("Movement"));
                            int artypid = int.Parse(dataReader.GetString("Article_Type_ID"));
                            int caseid = int.Parse(dataReader.GetString("Case_ID"));
                            int armoireid = int.Parse(dataReader.GetString("Armoire_ID"));
                            int actif = int.Parse(dataReader.GetString("Actif"));

                            Epc epc_tmp = new Epc(epcid, date_crea, date_modif, tag, code_barre, taille, type_taille, cycle_lav_count, state, last_user, lastreader, lastaction, lastactiondate, movement, artypid, caseid, armoireid, actif);

                            list_epc.Add(epc_tmp);
                            i++;
                        }
                        con.Close();
                    }
                    if ((i > 0) && (list_epc.Count() == i))
                        return list_epc;
                    else
                    {
                        return new List<Epc>();
                    }
                }
                catch (Exception e)
                {
                    Log.add("Erreur getAllEPC: " + e.Message);
                    return new List<Epc>();
                }
            }
            else
                return new List<Epc>();
        }

        public IEnumerable<Epc> getRestEPC()
        {
            if (IsAvailable())
            {
                List<Epc> list_epc = new List<Epc>();
                int i = 0;
                try
                {
                    string query = "SELECT `Id`, `Date_Creation`, `Date_Modification`, `Tag`, `Code_Barre`, `Taille`, `Type_Taille`, `Cycle_Lavage_Count`, "+
                        "`State`, `Last_User`, `Last_Reader`, `Last_Action`, `Last_Action_Date`, `Movement`, `Article_Type_ID`, `Case_ID`, `Armoire_ID`, "+
                        "`Actif` FROM `epc` WHERE (`State` != '300') && (`State` != '100') && (`State` != '10') && (`State` != '5');";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;
                        
                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            int epcid = int.Parse(dataReader.GetString("Id"));
                            string tag = dataReader.GetString("Tag");

                            DateTime date_crea = dataReader.GetDateTime("Date_Creation");
                            DateTime date_modif = dataReader.GetDateTime("Date_Modification");

                            string code_barre = dataReader.GetString("Code_Barre");
                            string taille = dataReader.GetString("Taille");
                            string type_taille = dataReader.GetString("Type_Taille");
                            int cycle_lav_count = int.Parse(dataReader.GetString("Cycle_Lavage_Count"));
                            int state = int.Parse(dataReader.GetString("State"));
                            string last_user = dataReader.GetString("Last_User");
                            string lastreader = dataReader.GetString("Last_Reader");
                            string lastaction = dataReader.GetString("Last_Action");
                            DateTime lastactiondate = dataReader.GetDateTime("Last_Action_Date");
                            int movement = int.Parse(dataReader.GetString("Movement"));
                            int artypid = int.Parse(dataReader.GetString("Article_Type_ID"));
                            int caseid = int.Parse(dataReader.GetString("Case_ID"));
                            int armoireid = int.Parse(dataReader.GetString("Armoire_ID"));
                            int actif = int.Parse(dataReader.GetString("Actif"));

                            Epc epc_tmp = new Epc(epcid, date_crea, date_modif, tag, code_barre, taille, type_taille, cycle_lav_count, state, last_user, lastreader, lastaction, lastactiondate, movement, artypid, caseid, armoireid, actif);

                            list_epc.Add(epc_tmp);
                            i++;
                        }
                        con.Close();
                    }
                    if ((i > 0) && (list_epc.Count() == i))
                        return list_epc;
                    else
                        return new List<Epc>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur getRestEPC: " + e.Message);
                    return new List<Epc>();
                }
            }
            else
                return new List<Epc>();
        }


        public List<Article_Type> getArticle_type_FromId(int id)
        {
            if (IsAvailable())
            {
                List<Article_Type> list_art_typ = new List<Article_Type>();
                int i = 0;
                try
                {
                    string query = "SELECT `Id`, `Date_Creation`, `Date_Modification`, `Code`, `Type_Taille`, `Description`, `Couleur`, `Sexe`, `Photo`, " +
                        "`Active`, `Description_longue` FROM `article_type` WHERE `Id`='"+id.ToString()+"';";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            int arttypid = int.Parse(dataReader.GetString("Id"));

                            DateTime date_crea = dataReader.GetDateTime("Date_Creation");
                            DateTime date_modif = dataReader.GetDateTime("Date_Modification");

                            string code = dataReader.GetString("Code");
                            string type_taille = dataReader.GetString("Type_Taille");
                            string desc = dataReader.GetString("Description");
                            string couleur = dataReader.GetString("Couleur");
                            string sexe = dataReader.GetString("Sexe");
                            string photo = dataReader.GetString("Photo");
                            int active = int.Parse(dataReader.GetString("Active"));
                            string description_longue = dataReader.GetString("Description_longue");

                            Article_Type art_typ_tmp = new Article_Type(arttypid, date_crea, date_modif, code, type_taille, desc, couleur, sexe, photo, active, description_longue);

                            list_art_typ.Add(art_typ_tmp);
                            i++;
                        }
                        con.Close();
                    }
                    if ((i > 0) && (list_art_typ.Count() == i))
                        return list_art_typ;
                    else
                    {
                        return new List<Article_Type>();
                    }
                }
                catch (Exception e)
                {
                    Log.add("Erreur getArticle_type: " + e.Message);
                    return new List<Article_Type>();
                }
            }
            else
                return new List<Article_Type>();
        }


        public IEnumerable<Article_Type> getArticle_type()
        {
            if (IsAvailable())
            {
                List<Article_Type> list_art_typ = new List<Article_Type>();
                int i = 0;
                try
                {
                    string query = "SELECT `Id`, `Date_Creation`, `Date_Modification`, `Code`, `Type_Taille`, `Description`, `Couleur`, `Sexe`, `Photo`, "+
                        "`Active`, `Description_longue` FROM `article_type`";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            int arttypid = int.Parse(dataReader.GetString("Id"));

                            DateTime date_crea = dataReader.GetDateTime("Date_Creation");
                            DateTime date_modif = dataReader.GetDateTime("Date_Modification");

                            string code = dataReader.GetString("Code");
                            string type_taille = dataReader.GetString("Type_Taille");
                            string desc = dataReader.GetString("Description");
                            string couleur = dataReader.GetString("Couleur");
                            string sexe = dataReader.GetString("Sexe");
                            string photo = dataReader.GetString("Photo");
                            int active = int.Parse(dataReader.GetString("Active"));
                            string description_longue = dataReader.GetString("Description_longue");

                            Article_Type art_typ_tmp = new Article_Type(arttypid, date_crea, date_modif, code, type_taille, desc, couleur, sexe, photo, active, description_longue);

                            list_art_typ.Add(art_typ_tmp);
                            i++;
                        }
                        con.Close();
                    }
                    if ((i > 0) && (list_art_typ.Count() == i))
                        return list_art_typ;
                    else
                    {
                        return new List<Article_Type>();
                    }
                }
                catch (Exception e)
                {
                    Log.add("Erreur getArticle_type: " + e.Message);
                    return new List<Article_Type>();
                }
            }
            else
                return new List<Article_Type>();
        }

        public bool userart4userid(int id_userart_local, int userid)
        {
            string query = "vide";
            //bool result = false;
            if (IsAvailable())
            {
                try
                {
                    int i = 0;
                    query = "SELECT `Id` FROM `user_article` WHERE  `Id` = '" + id_userart_local.ToString() + "' AND `User_Id` = '" + userid.ToString() + "';";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            i++;
                        }
                        if (i > 0)
                        {
                            return true;
                        }
                        else
                        {
                            throw (new Exception("il n'y a pas de user_article pour le User_Id = " + userid.ToString()));
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.add("Erreur userart4userid : " + e.Message+" Requette: "+query);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public IEnumerable<User_Article> getUser_articleByUserId(int usid)
        {
            if (IsAvailable() && getUserById(usid).FirstOrDefault().Type != "reloader")
            {
                List<User_Article> list_user_article = new List<User_Article>();
                int i = 0;
                try
                {
                    string query = "SELECT `Id`, `Date_Creation`, `Date_Modification`, `Taille`, `Credit`, `Credit_Restant`, `Credit_Semaine_Suivante`, `User_Id`, `Article_Type_Id` " +
                        " FROM `user_article` WHERE `User_Id` = '" + usid.ToString() + "';"; //"' LIMIT 1;";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            int userartid = int.Parse(dataReader.GetString("Id"));
                            DateTime date_crea = dataReader.GetDateTime("Date_Creation");
                            DateTime date_modif = dataReader.GetDateTime("Date_Modification");
                            string taille = dataReader.GetString("Taille");
                            int credit = int.Parse(dataReader.GetString("Credit"));
                            int credit_rest = int.Parse(dataReader.GetString("Credit_Restant"));
                            int credit_sem_suiv = int.Parse(dataReader.GetString("Credit_Semaine_Suivante"));
                            int userid = int.Parse(dataReader.GetString("User_Id"));
                            int arttypid = int.Parse(dataReader.GetString("Article_Type_Id"));

                            User_Article userart_tmp = new User_Article(userartid, date_crea, date_modif, taille, credit, credit_rest, credit_sem_suiv, userid, arttypid);

                            list_user_article.Add(userart_tmp);
                            i++;
                        }
                        con.Close();
                    }
                    if ((i > 0) && (list_user_article.Count() == i))
                        return list_user_article;
                    else
                        throw (new Exception("il n'y a pas de user_article pour le User_Id = " + usid.ToString()));
                }
                catch (Exception e)
                {
                    Log.add("Erreur getUser_articleById: " + e.Message);
                    return new List<User_Article>();
                }
            }
            else
                return new List<User_Article>();
        }


        public IEnumerable<User_Article> getUser_article()
        {
            if (IsAvailable())
            {
                List<User_Article> list_user_article = new List<User_Article>();
                int i = 0;
                try
                {
                    string query = "SELECT `Id`, `Date_Creation`, `Date_Modification`, `Taille`, `Credit`, `Credit_Restant`, `Credit_Semaine_Suivante`, `User_Id`, `Article_Type_Id` " +
                        "FROM `user_article`;";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            int userartid = int.Parse(dataReader.GetString("Id"));
                            DateTime date_crea = dataReader.GetDateTime("Date_Creation");
                            DateTime date_modif = dataReader.GetDateTime("Date_Modification");
                            string taille = dataReader.GetString("Taille");
                            int credit = int.Parse(dataReader.GetString("Credit"));
                            int credit_rest = int.Parse(dataReader.GetString("Credit_Restant"));
                            int credit_sem_suiv = int.Parse(dataReader.GetString("Credit_Semaine_Suivante"));
                            int userid = int.Parse(dataReader.GetString("User_Id"));
                            int arttypid = int.Parse(dataReader.GetString("Article_Type_Id"));

                            User_Article userart_tmp = new User_Article(userartid, date_crea, date_modif, taille, credit, credit_rest, credit_sem_suiv, userid, arttypid);

                            list_user_article.Add(userart_tmp);
                            i++;
                        }
                        con.Close();
                    }
                    if ((i > 0) && (list_user_article.Count() == i))
                        return list_user_article;
                    else
                        throw (new Exception("il n'y a pas de user_article dans la base de données locale"));

                }
                catch (Exception e)
                {
                    Log.add("Erreur getUser_article: " + e.Message);
                    return new List<User_Article>();
                }
            }
            else
                return new List<User_Article>();
        }

        public IEnumerable<Case> getCase()
        {
           if (IsAvailable())
            {
                List<Case> list_case = new List<Case>();
                int i = 0;
                try
                {
                    string query = "SELECT `Id` , `Bind_ID` , `Taille` , `Date_Creation` , `Max_Item` , `Article_Type_Id` , `Armoire_ID` FROM `case` WHERE `Armoire_ID` = "+Properties.Settings.Default.NumArmoire.ToString()+" ORDER BY Id";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            int caseid = int.Parse(dataReader.GetString("Id"));
                            int bindid = int.Parse(dataReader.GetString("Bind_ID"));
                            string taille = dataReader.GetString("Taille");
                            DateTime date_crea = dataReader.GetDateTime("Date_Creation");
                            int max_item = int.Parse(dataReader.GetString("Max_Item"));
                            int art_type_id = int.Parse(dataReader.GetString("Article_Type_Id"));
                            int armid = int.Parse(dataReader.GetString("Armoire_ID"));

                            Case case_tmp = new Case(caseid, bindid, taille, date_crea, max_item, art_type_id, armid);

                            list_case.Add(case_tmp);
                            i++;
                        }
                        con.Close();
                    }
                    if ((i > 0) && (list_case.Count() == i))
                        return list_case;
                    else
                        throw (new Exception("il n'y a pas case dans la base de données locale"));
                }
                catch (Exception e)
                {
                    Log.add("Erreur getCase: " + e.Message);
                    return new List<Case>();
                }
            }
            else
               return new List<Case>();
        }
        
        public IEnumerable<User> getUserById(int clause)
        {
            if (IsAvailable())
            {
                List<User> list_user = new List<User>();
                int i = 0;
                try
                {
                    string query = "SELECT `Id`, `Date_Creation`, `Date_Modification`, `Login`, `Password`, `Type`, `Nom`, `Prenom`, `Id_Lang`, `Sexe`, `Taille`, `Groupe`, `Department`, `Photo`, `Last_Connection`, `Active`, `End_of_Validity`, `Wearer_Code` FROM `user` WHERE `Id` = "+clause.ToString();
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            int userid = int.Parse(dataReader.GetString("Id"));
                            DateTime date_crea = dataReader.GetDateTime("Date_Creation");
                            DateTime date_modif = dataReader.GetDateTime("Date_Modification");
                            string login = dataReader.GetString("Login");
                            string password = dataReader.GetString("Password");
                            string type = dataReader.GetString("Type");
                            string nom = dataReader.GetString("Nom");
                            string prenom = dataReader.GetString("Prenom");
                            int id_lang = int.Parse(dataReader.GetString("Id_Lang"));
                            string sexe = dataReader.GetString("Sexe");
                            string taille = dataReader.GetString("Taille");
                            int groupe = int.Parse(dataReader.GetString("Groupe"));
                            int dept = int.Parse(dataReader.GetString("Department"));
                            string photo = dataReader.GetString("Photo");
                            DateTime last_connexion_datetime = dataReader.GetDateTime("Last_Connection");
                            int active = int.Parse(dataReader.GetString("Active"));
                            DateTime eov = dataReader.GetDateTime("End_of_Validity");
                            string wearer_code = dataReader.GetString("Wearer_Code");

                            User user_tmp = new User(userid, date_crea, date_modif, login, password, type, nom, prenom, id_lang, sexe, taille, groupe, dept, photo, last_connexion_datetime, active, eov, wearer_code);

                            list_user.Add(user_tmp);
                            i++;
                        }
                        con.Close();
                    }
                    if ((i > 0) && (list_user.Count() == 1))
                        return list_user;
                    else
                        throw (new Exception("il n'y a pas de user d'Id = " + clause.ToString() + " dans la base de données locale"));
                }
                catch (Exception e)
                {
                    Log.add("Erreur getUserById: " + e.Message);
                    return new List<User>();
                }
            }
            else
                 return new List<User>();
        }

        public User getUserByPassword(string password)
        {
            if (IsAvailable())
            {
                List<User> list_user = new List<User>();
                int i = 0;
                try
                {
                    string query = "SELECT `Id`, `Date_Creation`, `Date_Modification`, `Login`, `Password`, `Type`, `Nom`, `Prenom`, `Id_Lang`, `Sexe`, `Taille`, `Groupe`, `Department`, `Photo`, `Last_Connection`, `Active`, `End_of_Validity`, `Wearer_Code` FROM `user` WHERE `Password` = '" + password + "';";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            int userid = int.Parse(dataReader.GetString("Id"));
                            DateTime date_crea = dataReader.GetDateTime("Date_Creation");
                            DateTime date_modif = dataReader.GetDateTime("Date_Modification");
                            string log = dataReader.GetString("Login");
                            string pass = dataReader.GetString("Password");
                            string type = dataReader.GetString("Type");
                            string nom = dataReader.GetString("Nom");
                            string prenom = dataReader.GetString("Prenom");
                            int id_lang = int.Parse(dataReader.GetString("Id_Lang"));
                            string sexe = dataReader.GetString("Sexe");
                            string taille = dataReader.GetString("Taille");
                            int groupe = int.Parse(dataReader.GetString("Groupe"));
                            int dept = int.Parse(dataReader.GetString("Department"));
                            string photo = dataReader.GetString("Photo");
                            DateTime last_connexion_datetime = dataReader.GetDateTime("Last_Connection");
                            int active = int.Parse(dataReader.GetString("Active"));
                            DateTime eov = dataReader.GetDateTime("End_of_Validity");
                            string wearer_code = dataReader.GetString("Wearer_Code");

                            User user_tmp = new User(userid, date_crea, date_modif, log, pass, type, nom, prenom, id_lang, sexe, taille, groupe, dept, photo, last_connexion_datetime, active, eov, wearer_code);

                            list_user.Add(user_tmp);
                            i++;
                        }
                        con.Close();
                    }
                    if (i > 0)
                    {
                        if (list_user.Count() == 1)
                            return list_user[0];
                        else
                            throw (new Exception("il y a " + list_user.Count() + " users de Password = '" + password + "' dans la base de données locale"));
                    }
                    else

                        throw (new Exception("il n'y a pas de user de Password = '" + password + "' dans la base de données locale"));
                }
                catch (Exception e)
                {
                    Log.add("Erreur getUserByPassword: " + e.Message);
                    return null;
                }
            }
            else
                return null;
        }

        //Recupere le type de l'utilisateur
        public IEnumerable<User> getUserType(string clause)
        {
            if (IsAvailable())
            {
                try
                {
                    User usr_tmp = getUserByPassword(clause);
                    List<User> list_usr= new List<User>();

                    if (usr_tmp != null)
                    {
                        list_usr.Add(usr_tmp);
                        return list_usr;
                    }
                    else
                        return new List<User>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur getUserType: " + e.Message);
                    return new List<User>();
                }
            }
            else
                return new List<User>();
        }

        //Recupere la liste des tags par type d'article, taille du porteur, et present dans l'armoire
        public IEnumerable<Epc> getEpcByArticleIdAndSize(int artId, string size, bool avecTaille, bool avecState)
        {
            string query = "SELECT  `Id`, `Date_Creation`, `Date_Modification`, `Tag`, `Code_Barre`, `Taille`, `Type_Taille`, `Cycle_Lavage_Count`, `State`, `Last_User`, `Last_Reader`, `Last_Action`,  `Last_Action_Date`, `Movement`, `Article_Type_ID`, `Case_ID`, `Armoire_ID`, `Actif` FROM `epc`";
            // WHERE CONDITION
            if (avecState && avecTaille)
            {
                query += " WHERE `Article_Type_ID` = " + artId.ToString() +
                    " AND " + " `State` =  100 " +
                    " AND (`Taille` = " + size.ToString() +
                    " OR `Taille` IN (SELECT  `Taille` FROM  `corresp_taille` WHERE `Classement_tailles` " +
                    " IN (SELECT  `Classement_tailles` +1 FROM  `corresp_taille` WHERE  `Taille` =  " + size.ToString() +
                    " AND  `Type-Taille` IN (SELECT  `Type_Taille` FROM  `article_type` WHERE  `Id` =" + artId.ToString() + "))))";
            }
            else
            {
                if (avecTaille && !avecState)
                {
                    query += " WHERE Article_Type_ID = " + artId.ToString() +
                    " AND (`Taille` = " + size.ToString() +
                    " OR `Taille` IN (SELECT  `Taille` FROM  `corresp_taille` WHERE `Classement_tailles` " +
                    " IN (SELECT  `Classement_tailles` +1 FROM  `corresp_taille` WHERE  `Taille` =  " + size.ToString() +
                    " AND  `Type-Taille` IN (SELECT  `Type_Taille` FROM  `article_type` WHERE  `Id` =" + artId.ToString() + "))))";
                }
                else
                {
                    if (!avecTaille && avecState)
                    {
                        query += " WHERE Article_Type_ID = " + artId.ToString() +
                            " AND " + "((State =  100) OR (State = 5 ))";
                    }
                    else // !avecTaille && !avecState
                    {
                        query += " WHERE Article_Type_ID = " + artId.ToString();
                    }
                }

            }
            //query += " LIMIT 1"; // Ne renvoyer qu'une seule occurence
            query += ";"; // termine la requete

            if (IsAvailable())
            {
                try
                {
                    return getBackEpcList(query);
                }
                catch (Exception e)
                {
                    Log.add("Erreur getEpcByArticleIdAndSize: " + e.Message);
                    return new List<Epc>();
                }
            }
            else
            {
                return new List<Epc>();
            }

        }

        // retourne une liste d'epc qui correspond par user_article_id, taille et statut
        public IEnumerable<Epc> getEpcByArticleIdAndUniqueSize(int artId, string size, int statut)
        {
            string query = "SELECT  `Id`, `Date_Creation`, `Date_Modification`, `Tag`, `Code_Barre`, `Taille`, `Type_Taille`, `Cycle_Lavage_Count`, `State`, `Last_User`, `Last_Reader`, `Last_Action`,  `Last_Action_Date`, `Movement`, `Article_Type_ID`, `Case_ID`, `Armoire_ID`, `Actif` FROM `epc`";
            // WHERE CONDITION
            query += " WHERE `Article_Type_ID` = " + artId.ToString() +
                    " AND " + " `State` =  " + statut.ToString() +
                    " AND `Taille` = " + size.ToString() +
                    " AND `Armoire_ID` = " + Properties.Settings.Default.NumArmoire.ToString();

            //query += " LIMIT 1"; // Ne renvoyer qu'une seule occurence
            query += ";"; // termine la requete

            if (IsAvailable())
            {
                try
                {
                    return getBackEpcList(query);
                }
                catch (Exception e)
                {
                    Log.add("Erreur getEpcByArticleIdAndUniqueSize: " + e.Message);
                    return new List<Epc>();
                }
            }
            else
            {
                return new List<Epc>();
            }

           
        }

        private List<Epc> getBackEpcList(string query)
            {
                List<Epc> list_epc = new List<Epc>();
                int i = 0;
                
                    

                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {
                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            int epcid = int.Parse(dataReader.GetString("Id"));
                            string tag = dataReader.GetString("Tag");

                            DateTime date_crea = dataReader.GetDateTime("Date_Creation");
                            DateTime date_modif = dataReader.GetDateTime("Date_Modification");

                            string code_barre = dataReader.GetString("Code_Barre");
                            string taille = dataReader.GetString("Taille");
                            string type_taille = dataReader.GetString("Type_Taille");
                            int cycle_lav_count = int.Parse(dataReader.GetString("Cycle_Lavage_Count"));
                            int state = int.Parse(dataReader.GetString("State"));
                            string last_user = dataReader.GetString("Last_User");
                            string lastreader = dataReader.GetString("Last_Reader");
                            string lastaction = dataReader.GetString("Last_Action");
                            DateTime lastactiondate = dataReader.GetDateTime("Last_Action_Date");
                            int movement = int.Parse(dataReader.GetString("Movement"));
                            int artypid = int.Parse(dataReader.GetString("Article_Type_ID"));
                            int caseid = int.Parse(dataReader.GetString("Case_ID"));
                            int armoireid = int.Parse(dataReader.GetString("Armoire_ID"));
                            int actif = int.Parse(dataReader.GetString("Actif"));

                            Epc epc_tmp = new Epc(epcid, date_crea, date_modif, tag, code_barre, taille, type_taille, cycle_lav_count, state, last_user, lastreader, lastaction, lastactiondate, movement, artypid, caseid, armoireid, actif);

                            list_epc.Add(epc_tmp);
                            i++;
                        }
                        con.Close();
                    }

                    if ((i > 0) && (list_epc.Count() == i))
                        return list_epc;
                    else
                    {
                        return new List<Epc>();
                    }

                
        }

        // retourne une liste d'epc qui correspond par user_article_id, taille, numArmoire et statut
        public IEnumerable<Epc> getEpcByArticleIdAndUniqueSize(int artId, string size, int numArmoire, int statut)
        {
            string query = "SELECT  `Id`, `Date_Creation`, `Date_Modification`, `Tag`, `Code_Barre`, `Taille`, `Type_Taille`, `Cycle_Lavage_Count`, `State`, `Last_User`, `Last_Reader`, `Last_Action`,  `Last_Action_Date`, `Movement`, `Article_Type_ID`, `Case_ID`, `Armoire_ID`, `Actif` FROM `epc`";
            // WHERE CONDITION IQueryable<epc> query = from f in dbContext.epc where f.Article_Type_ID == artId && f.Taille == size && f.Armoire_ID == numArmoire && f.State == statut select f;
            query += " WHERE `Article_Type_ID` = " + artId.ToString() +
                    " AND " + " `State` =  " + statut.ToString() +
                    " AND `Taille` = " + size.ToString() +
                    " AND `Armoire_ID` = " + numArmoire.ToString();
            //query += " LIMIT 1"; // Ne renvoyer qu'une seule occurence
            query += ";"; // termine la requete
          
            if (IsAvailable())
            {
                try
                {
                    return getBackEpcList(query);  
                }
                catch (Exception e)
                {
                    Log.add("Erreur getEpcByArticleIdAndUniqueSize: " + e.Message);
                    return new List<Epc>();
                }
            }
            else
            {
                return new List<Epc>();
            }
        }

        // retourne une liste d'epc qui correspond par user_article_id, et taille
        public IEnumerable<Epc> getEpcByArticleIdAndUniqueSize(int artId, string size)
        {

            string query = "SELECT  `Id`, `Date_Creation`, `Date_Modification`, `Tag`, `Code_Barre`, `Taille`, `Type_Taille`, `Cycle_Lavage_Count`, `State`, `Last_User`, `Last_Reader`, `Last_Action`,  `Last_Action_Date`, `Movement`, `Article_Type_ID`, `Case_ID`, `Armoire_ID`, `Actif` FROM `epc`";
            // WHERE CONDITION  IQueryable<epc> query = from f in dbContext.epc where ((f.Article_Type_ID == artId) && (f.Taille == size)) select f;
            query += " WHERE `Article_Type_ID` = " + artId.ToString() +
                     " AND `Taille` = " + size.ToString();

            //query += " LIMIT 1"; // Ne renvoyer qu'une seule occurence
            query += ";"; // termine la requete

     
            if (IsAvailable())
            {
                try
                {
                    return getBackEpcList(query);  
                }
                catch (Exception e)
                {
                    Log.add("Erreur getEpcByArticleIdAndUniqueSize: " + e.Message);
                    return new List<Epc>();
                }
            }
            else
            {
                return new List<Epc>();
            }


        }

        public IEnumerable<Epc> getEpcTailleSupByArticleIdAndOriginalSize(int artId, string size, int statut)
        {
            string query = "SELECT  `Id`, `Date_Creation`, `Date_Modification`, `Tag`, `Code_Barre`, `Taille`, `Type_Taille`, `Cycle_Lavage_Count`, `State`, `Last_User`, `Last_Reader`, `Last_Action`,  `Last_Action_Date`, `Movement`, `Article_Type_ID`, `Case_ID`, `Armoire_ID`, `Actif` FROM `epc`";
            // WHERE CONDITION 
            query += " WHERE `Article_Type_ID` = " + artId.ToString() +
                    " AND " + " `State` =  " + statut.ToString() +
                    " AND `Armoire_ID` = " + Properties.Settings.Default.NumArmoire.ToString();

            query += " AND `Taille` IN (SELECT  `Taille` FROM  `corresp_taille` WHERE `Classement_tailles` " +
                    " IN (SELECT  `Classement_tailles` +1 FROM  `corresp_taille` WHERE  `Taille` =  " + size.ToString() +
                    " AND  `Type-Taille` IN (SELECT  `Type_Taille` FROM  `article_type` WHERE  `Id` =" + artId.ToString() + ")))";

            //query += " LIMIT 1;"; // Ne renvoyer qu'une seule occurence
            query += ";"; // termine la requete
                
                    /*IQueryable<string> queryPlus = (from g in dbContext.corresp_taille
                                                    from h in
                                                        (from st in dbContext.corresp_taille
                                                         where st.Taille == size
                                                         select st)
                                                    where (g.Classement_tailles == h.Classement_tailles + 1)
                                                    select g.Taille);

                    IQueryable<epc> query = from f in dbContext.epc
                                            where f.Article_Type_ID == artId && f.State == statut && queryPlus.Contains(f.Taille) && f.Armoire_ID == Properties.Settings.Default.NumArmoire
                                            select f;*/
            if (IsAvailable())
            {
                try
                {
                    return getBackEpcList(query);  
                }
                catch (Exception e)
                {
                    Log.add("Erreur getEpcTailleSupByArticleIdAndOriginalSize: " + e.Message);
                    return new List<Epc>();
                }
            }
            else
            {
                return new List<Epc>();
            }
        }

        public IEnumerable<Epc> getEpcTailleSupByArticleIdAndOriginalSize(int artId, string size, int numArmoire, int statut)
        {
            string query = "SELECT  `Id`, `Date_Creation`, `Date_Modification`, `Tag`, `Code_Barre`, `Taille`, `Type_Taille`, `Cycle_Lavage_Count`, `State`, `Last_User`, `Last_Reader`, `Last_Action`,  `Last_Action_Date`, `Movement`, `Article_Type_ID`, `Case_ID`, `Armoire_ID`, `Actif` FROM `epc`";
            // WHERE CONDITION 
            query += " WHERE `Article_Type_ID` = " + artId.ToString() +
                    " AND " + " `State` =  " + statut.ToString() +
                    " AND `Armoire_ID` = " + numArmoire.ToString();

            query += " AND `Taille` IN (SELECT  `Taille` FROM  `corresp_taille` WHERE `Classement_tailles` " +
                    " IN (SELECT  `Classement_tailles` +1 FROM  `corresp_taille` WHERE  `Taille` =  " + size.ToString() +
                    " AND  `Type-Taille` IN (SELECT  `Type_Taille` FROM  `article_type` WHERE  `Id` =" + artId.ToString() + ")))";

            //query += " LIMIT 1;"; // Ne renvoyer qu'une seule occurence
            query += ";"; // termine la requete

            
            if (IsAvailable())
            {
                try
                {
                    return getBackEpcList(query);
                }
                catch (Exception e)
                {
                    Log.add("Erreur getEpcTailleSupByArticleIdAndOriginalSize: " + e.Message);
                    return new List<Epc>();
                }
            }
            else
            {
                return new List<Epc>();
            }


        }

        // TODO : Ne devrait pas servir (La Taille ne correspond à rien si elle n'est pas reliée à type Taille de Article type forcé ici arbitrairement à 1
        // récupere la Taille inférieur telle qu'elle est définie dans la table corresp_taille
        public string getTailleInfByOriginalSize(string size)
        {
            string query = "SELECT  `Taille` FROM  `corresp_taille` WHERE `Classement_tailles` " +
                    " IN (SELECT  `Classement_tailles` -1 FROM  `corresp_taille` WHERE  `Taille` =  " + size.ToString() +
                    " AND  `Type-Taille` IN (SELECT  `Type_Taille` FROM  `article_type` WHERE  `Id` = 1))";
            /*IQueryable<string> queryPlus = (from g in dbContext.corresp_taille
                                            from h in
                                                (from st in dbContext.corresp_taille
                                                 where st.Taille == size
                                                 select st)
                                            where (g.Classement_tailles == h.Classement_tailles - 1)
                                            select g.Taille); */
            //query += " LIMIT 1;"; // Ne renvoyer qu'une seule occurence
            query += ";"; // termine la requete


            if (IsAvailable())
            {
                try
                {
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {
                        string tailleretournee = null;
                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        if (dataReader.Read())
                        {
                            tailleretournee =dataReader.GetString("Taille");
                        }
                        con.Close();
                        return tailleretournee ;
                    }

                }
                catch (Exception e)
                {
                    Log.add("Erreur getTailleInfByOriginalSize: " + e.Message);
                    return null;
                }
            }
            else
                return null;
        }
        
        //Recupere les article_type pour le loader admin 
        public IEnumerable<string> getArticleTypeDesc()
        {
            int i=0;
            string query = "SELECT  `Description` FROM  `article_type` "; //from f in dbContext.article_type select f.Description;
            //query += " LIMIT 1;"; // Ne renvoyer qu'une seule occurence
            query += ";"; // termine la requete

           if (IsAvailable())
            {
                try
                {
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {
                        List<string> listeDesc = new List<string>();
                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            listeDesc.Add(dataReader.GetString("Description"));
                            i++;
                        }
                        con.Close();
                        if ( (i > 0) && (listeDesc.Count() == i))
                            return listeDesc;
                        else
                            return new List<string>();
                    }

                }
                catch (Exception e)
                {
                    Log.add("Erreur getArticleTypeDesc: " + e.Message);
                    return  new List<string>();
                }
            }
            else
                return  new List<string>();
            
        }

        //Recupere tout les tag gerer par l'armoire pour comparé apres le scan
        public IEnumerable<string> getTag()
        {
            int i = 0;
            string query = "SELECT `Tag` FROM  `epc` "; //from f in dbContext.epc select f.Tag;
            //query += " LIMIT 1;"; // Ne renvoyer qu'une seule occurence
            query += ";"; // termine la requete

            if (IsAvailable())
            {
                try
                {
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {
                        List<string> listeTags = new List<string>();
                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            listeTags.Add(dataReader.GetString("Tag"));
                            i++;
                        }
                        con.Close();
                        if ((i > 0) && (listeTags.Count() == i))
                            return listeTags;
                        else
                            return new List<string>();
                    }

                }
                catch (Exception e)
                {
                    Log.add("Erreur getTag: " + e.Message);
                    return new List<string>();
                }
            }
            else
                return new List<string>();
        }
#if EDMX
        //Recupere tout les tag toute ses infos
        public IEnumerable<Epc> getTagAll()
        {
            
            string query = "SELECT  `Id`, `Date_Creation`, `Date_Modification`, `Tag`, `Code_Barre`, `Taille`, `Type_Taille`, `Cycle_Lavage_Count`, `State`, `Last_User`, `Last_Reader`, `Last_Action`,  `Last_Action_Date`, `Movement`, `Article_Type_ID`, `Case_ID`, `Armoire_ID`, `Actif` FROM `epc`";
            //query += " LIMIT 1;"; // Ne renvoyer qu'une seule occurence
            query += ";"; // termine la requete

            if (IsAvailable())
            {
                try
                {
                    return getBackEpcList(query);
                }
                catch (Exception e)
                {
                    Log.add("Erreur getEpcByArticleIdAndUniqueSize: " + e.Message);
                    return new List<Epc>();
                }
            }
            else
            {
                return new List<Epc>();
            }

        }
#endif
        //Recupere les article_type pour le loader admin
        public IEnumerable<string> getPhoto(int id)
        {
            int i = 0;
            string query = "SELECT `Photo` FROM  `article_type` "; // from f in dbContext.article_type where f.Id == id select f.Photo;
            query += " WHERE `Id` = " + id.ToString();
            //query += " LIMIT 1;"; // Ne renvoyer qu'une seule occurence
            query += ";"; // termine la requete

            if (IsAvailable())
            {
                try
                {
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {
                        List<string> listePhotos = new List<string>();
                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            listePhotos.Add(dataReader.GetString("Photo"));
                            i++;
                        }
                        con.Close();
                        if ((i > 0) && (listePhotos.Count() == i))
                            return listePhotos;
                        else
                            return new List<string>();
                    }

                }
                catch (Exception e)
                {
                    Log.add("Erreur getPhoto: " + e.Message);
                    return new List<string>();
                }
            }
            else
                return new List<string>();


        }

        //Recupere les article type pour le loader admin
        public string getArticleTypeForUpdate(int id)
        {
            string query = "SELECT  `Description` FROM  `article_type` "; //from f in dbContext.article_type where f.Id == id select f.Description;
            query += " WHERE Id = " + id.ToString();
            query += " LIMIT 1"; // Ne renvoyer qu'une seule occurence
            query += ";"; // termine la requete

           if (IsAvailable())
            {
                try
                {
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {
                        string descreturned = "";
                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        if (dataReader.Read())
                        {
                            descreturned = dataReader.GetString("Description");
                        }
                        con.Close();
                        if (descreturned !=null)
                            return descreturned;
                        else
                            return "";
                    }

                }
                catch (Exception e)
                {
                    Log.add("Erreur getArticleTypeForUpdate: " + e.Message);
                    return "";
                }
            }
            else
                 return "";
        }


        //Recupere id d'un article type par sa description pour le loader admin
        public int getArticleTypeDescid(string desc)
        {

            string query = "SELECT  `Id` FROM  `article_type` "; //from f in dbContext.article_type where f.Description == desc select f.Id;
            query += " WHERE Description = " + desc;
            query += " LIMIT 1"; // Ne renvoyer qu'une seule occurence
            query += ";"; // termine la requete

            if (IsAvailable())
            {
                try
                {
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {
                        int Idreturned = 0;
                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        if (dataReader.Read())
                        {
                            Idreturned = int.Parse(dataReader.GetString("Id")); 
                        }
                        con.Close();
                        if (Idreturned != 0)
                            return Idreturned;
                        else
                            return 0;
                    }

                }
                catch (Exception e)
                {
                    Log.add("Erreur getArticleTypeDescid: " + e.Message);
                    return 0;
                }
            }
            else
                return 0;
        }

        private List<Case> getBackCaseList(string query)
        {
            List<Case> listecases = new List<Case>();
            int i = 0;

            using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
            {
                
                MySqlCommand commande = con.CreateCommand();
                commande.CommandText = query;

                con.Open();
                MySqlDataReader dataReader = commande.ExecuteReader();
                while (dataReader.Read())
                {
                    int caseid = int.Parse(dataReader.GetString("Id"));//  `Id` int(8) NOT NULL,
                    int casebindid = int.Parse(dataReader.GetString("Bind_ID"));//  `Bind_ID` int(32) NOT NULL,
                    string casetaille = dataReader.GetString("Taille");//  `Taille` varchar(255) NOT NULL,
                    DateTime date_crea = dataReader.GetDateTime("Date_Creation");//  `Date_Creation` datetime NOT NULL,
                    int maxitem = int.Parse(dataReader.GetString("Max_Item"));//  `Max_Item` int(8) NOT NULL,
                    int artypid = int.Parse(dataReader.GetString("Article_Type_ID"));//  `Article_Type_Id` int(8) NOT NULL,
                    int armoireid = int.Parse(dataReader.GetString("Armoire_ID")); //  `Armoire_ID` int(8) NOT NULL,

                    Case case_tmp = new Case(caseid, casebindid, casetaille, date_crea, maxitem, artypid, armoireid);

                    listecases.Add(case_tmp);
                    i++;
                }
                con.Close();
                
            }


            if ((i > 0) && (listecases.Count() == i))
                return listecases;
            else
                return new List<Case>();


        }

        //Recupere la liste de case ayant des articles de différentes caracteristiques pour obtenir la liste des tailles du plan de chargement
        public IEnumerable<Case> getTaille()
        {
            
            string query = "SELECT `Id`, `Bind_ID`, `Taille`, `Date_Creation`, `Max_Item`, `Article_Type_Id`, `Armoire_ID` FROM  `case` "; //  f in dbContext.@case.Distinct() select f;
            //query += " WHERE `Taille` IN (SELECT DISTINCT `Taille`, `Article_Type_Id`, `Armoire_ID` FROM `case`)";
            //query += " LIMIT 1;"; // Ne renvoyer qu'une seule occurence
            query += ";"; // termine la requete

            if (IsAvailable())
            {
                try
                {
                   return getBackCaseList(query);
                }
                catch (Exception e)
                {
                    Log.add("Erreur getTaille: " + e.Message);
                    return new List<Case>();
                }
            }
            else
                return new List<Case>();
        }

        public IEnumerable<Case> getCaseForReload(int articleId, string taille)
        {
            // IQueryable<Case> query = from f in dbContext.@case
            //                                  where f.Article_Type_Id == articleId
            //                                  && f.Taille == taille
            //                                  && f.Armoire_ID == Properties.Settings.Default.NumArmoire
            //                                  select f;

            string query = "SELECT `Id`, `Bind_ID`, `Taille`, `Date_Creation`, `Max_Item`, `Article_Type_Id`, `Armoire_ID` FROM  `case` "; //  f in dbContext.@case.Distinct() select f;
            query += " WHERE `Article_Type_Id` = "+articleId.ToString();
            query += " AND `Taille` = " + taille.ToString();
            query += " AND `Armoire_ID` = " + Properties.Settings.Default.NumArmoire;
            //query += " LIMIT 1;"; // Ne renvoyer qu'une seule occurence
            query += ";"; // termine la requete

            if (IsAvailable())
            {
                try
                {
                    return getBackCaseList(query);
                }
                catch (Exception e)
                {
                    Log.add("Erreur getCaseForReload: " + e.Message);
                    return new List<Case>();
                }
            }
            else
                return new List<Case>();
        }

        public bool isInPlanChargement(string Taille, int articleId, int ArmId)
        {
            List<Case> myList = new List<Case>();
            string query = "SELECT * FROM  `case` "; //  f in dbContext.@case.Distinct() select f;
            query += " WHERE `Article_Type_Id` = '" + articleId.ToString();
            query += "' AND `Armoire_ID` = '" + ArmId.ToString(); 
            query += "' AND `Taille` = '" + Taille;
            query += "';"; // termine la requete

            if (IsAvailable())
            {
                try
                {
                    myList = getBackCaseList(query);
                }
                catch (Exception e)
                {
                    Log.add("Erreur getCase: " + e.Message);
                    return false;
                }
            }
            if (myList.Count() == 0)
                return false;
            else
                return true;
        }

        public IEnumerable<Case> getCaseForReload(int articleId)
        {
            // IQueryable<Case> query = from f in dbContext.@case
            //                                  where f.Article_Type_Id == articleId
            //                                  && f.Armoire_ID == Properties.Settings.Default.NumArmoire
            //                                  select f;

            string query = "SELECT `Id`, `Bind_ID`, `Taille`, `Date_Creation`, `Max_Item`, `Article_Type_Id`, `Armoire_ID` FROM  `case` "; //  f in dbContext.@case.Distinct() select f;
            query += " WHERE `Article_Type_Id` = " + articleId.ToString();
             query += " AND `Armoire_ID` = " + Properties.Settings.Default.NumArmoire;
            //query += " LIMIT 1;"; // Ne renvoyer qu'une seule occurence
            query += ";"; // termine la requete

            if (IsAvailable())
            {
                try
                {
                    return getBackCaseList(query);
                }
                catch (Exception e)
                {
                    Log.add("Erreur getCaseForReload: " + e.Message);
                    return new List<Case>();
                }
            }
            else
                return new List<Case>();
        }
               
        /// <summary>
        /// Get de tous les article_taille de l'armoire utilisée
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Article_Taille> getArticleTaille()
        {
            if (IsAvailable())
            {
                List<Article_Taille> list_art_tail = new List<Article_Taille>();
                int i = 0;
                try
                {
                    string query = "SELECT `Article_Type_ID`, `Taille`, `Armoire`, `Vide` FROM `article_taille`";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {
                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {

                            int arttailid = int.Parse(dataReader.GetString("Article_Type_ID"));

                            string taille = dataReader.GetString("Taille");
                            int armoire = int.Parse(dataReader.GetString("Armoire"));
                            bool vide = bool.Parse(dataReader.GetString("Vide"));

                            Article_Taille art_tail_tmp = new Article_Taille(arttailid, taille, armoire, vide);

                            list_art_tail.Add(art_tail_tmp);
                            i++;
                        }
                        con.Close();
                    }
                    if ((i > 0) && (list_art_tail.Count() == i))
                        return list_art_tail;
                    else
                    {
                        return new List<Article_Taille>();
                    }

                }
                catch (Exception e)
                {
                    Log.add("Erreur getArticleTaille: " + e.Message);
                    return new List<Article_Taille>();
                }
            }
            else
            {
                return new List<Article_Taille>();
            }
        }


#if OPTLANG
        // Récupération de la langue d'un user
        public string getLangByUserID(int userID)
        {
            if (IsAvailable())
            {
                try
                {
                    int i = 0;
                    string userlang = "";
                    string query = "SELECT cultureName FROM corresp_lang, user WHERE corresp_lang.Id = user.Id_Lang AND user.Id = " + userID.ToString() + ";";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {
                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            userlang = dataReader.GetString("cultureName");

                            i++;
                        }
                        con.Close();
                    }
                    if (i == 1)
                        return userlang;
                    else
                        return "fr-FR";

                }
                catch(Exception e)
                {
                    Log.add("Erreur getLangByUserID : " + e.Message);
                    
                    if (Properties.Settings.Default.UseDBGMSG)
                    {
                        MessageBox.Show("Exception levée durant la requête sur base user et corresp_lang : " + e.Message);
                    }
                    return "fr-FR";
                }
            }
            else
                return "fr-FR";
        }

#endif
        #endregion

        #region INSERTs

        private long MyInsertquery(string query, bool demanderetourautoincrément)
        {
            Int64 newId = 0;
            int result = 0;
            using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
            {
                
                MySqlCommand commande = con.CreateCommand();
                commande.CommandText = query;

                con.Open();
                  
                if (demanderetourautoincrément == true)
                {
                    newId = Convert.ToInt64(commande.ExecuteScalar());
                    result = 1;
                }
                else
                    result = commande.ExecuteNonQuery();
 
                con.Close();
                        
            }
            if (result != 1)
                throw (new Exception("requete non reussie (" + query + ")"));
            return (long) newId;
        }

        public int insertAlert(int altype, int user, int arm, string mess)
        {
            long newId = 0;
            string date = String.Format("{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now);
            Alert alrt = new Alert();

            alrt.Date_Creation = DateTime.Now;
            alrt.Alert_Type_Id = altype;
            alrt.User_ID = user;
            alrt.Armoire_ID = arm;
            alrt.Traiter = 0;
            alrt.Message = mess;
            /* Actuellement
            `Id` int(8) NOT NULL AUTO_INCREMENT,
  `Date_Creation` datetime NOT NULL,
  `Alert_Type_Id` int(8) NOT NULL,
  `Message` varchar(2500) NOT NULL,
  `User_ID` int(32) NOT NULL,
  `Armoire_ID` int(32) NOT NULL,
  `Traiter` 
            A faire évoluer comme ci-dessous qui est la struct de la base sendmail
        `Id` int(8) NOT NULL,
  `Date_Creation` datetime NOT NULL,
  `Alert_Type_Id` int(8) NOT NULL,
  `Message` varchar(2500) NOT NULL,
  `Origin` varchar(255) NOT NULL,
  `Login` varchar(32) NOT NULL,
  `Prenom` varchar(255) NOT NULL,
  `Nom` varchar(255) NOT NULL,
  `Armoire_Name` varchar(255) NOT NULL,
  `Armoire_ID` int(32) NOT NULL,
  `Traiter` smallint(1) NOT NULL,
             * */

//            string query = "INSERT INTO `alert`(`Id`, `Date_Creation`, `Alert_Type_Id`, `Message`, `User_ID`, `Armoire_ID`, `Traiter`) ";
            string query = "INSERT INTO `alert`( `Date_Creation`, `Alert_Type_Id`, `Message`, `User_ID`, `Armoire_ID`, `Traiter`) ";
            query += "VALUES (" /*+ id.ToString()+ ",*/ + " '" + alrt.Date_Creation.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + alrt.Alert_Type_Id
                  + "', '" + alrt.Message + "', '" + alrt.User_ID + "', '" + alrt.Armoire_ID + "', '" + alrt.Traiter + "')";
            //query += ";SELECT @@IDENTITY;";
            query += ";SELECT LAST_INSERT_ID( );"; // Pour récupérer la valeur de l'autoincrément
            //query += ";SELECT CAST(LAST_INSERT_ID() AS UNSIGNED);"; // Pour récupérer la valeur de l'autoincrément caste en int
            //query += ";SELECT CONVERT(LAST_INSERT_ID(), STRING);";
            try
            {
                newId = MyInsertquery(query,true);
                Log.add("Alerte de type " + altype.ToString() + ": " + mess);
                return (int) newId;      // ATTENTION : CAST DANGEREUX          
            }
            catch (Exception e)
            {
                Log.add("Erreur insertAlert: " + e.Message);
                if (Properties.Settings.Default.UseDBGMSG)
                {
                    MessageBox.Show("Erreur insertAlert: " + e.Message);
                }
                return 0;
            }
        }

        //insertion dans la table tag_alert, liste les tag commandé, intrus et retiré
        public void insertTagAlert(int alrtID, string command, string retir, string intrus, string articletypecode, string taille)
        {
            string date = String.Format("{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now);

            Tag_Alert alrtag = new Tag_Alert();

            alrtag.Date_Creation = DateTime.Now;
            alrtag.Alert_Id = alrtID;
            alrtag.Tag_Command = command;
            alrtag.Tag_Retir = retir;
            alrtag.Tag_Intrus = intrus;
            alrtag.Article_Type_Code = articletypecode;
            alrtag.Taille = taille;
            if (command !="")
                alrtag.Code_Barre = GetBackCodeBarrefromEpc(command);
            if (retir != "")
                alrtag.Code_Barre = GetBackCodeBarrefromEpc(retir);
            if (intrus != "")
                alrtag.Code_Barre = GetBackCodeBarrefromEpc(intrus);
            /*
             * `Id` int(32) NOT NULL AUTO_INCREMENT,
             * `Date_Creation` datetime NOT NULL,
             * `Alert_ID` int(32) NOT NULL,
             * `Tag_Command` varchar(255) NOT NULL,
             * `Tag_Retir` varchar(255) NOT NULL,
             * `Tag_Intrus` varchar(255) NOT NULL,
             * `Article_Type_Code` varchar(255) NOT NULL,
             * `Taille` varchar(255) NOT NULL,
             *  `Code_Barre` varchar(255) NOT NULL,
             * */
            //string query = "INSERT INTO `tag_alert`(`Id`, `Date_Creation`, `Alert_ID`, `Tag_Command`, `Tag_Retir`, `Tag_Intrus`, `Article_Type_Code`,`Taille`) ";
            string query = "INSERT INTO `tag_alert`(`Date_Creation`, `Alert_ID`, `Tag_Command`, `Tag_Retir`, `Tag_Intrus`, `Article_Type_Code`,`Taille`, `Code_Barre`) ";
            query += "VALUES (" /*+ id.ToString() + ",*/ + " '" + alrtag.Date_Creation.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + alrtag.Alert_Id + "', '"
                + alrtag.Tag_Command + "', '" + alrtag.Tag_Retir + "', '" + alrtag.Tag_Intrus + "', '" + alrtag.Article_Type_Code + "', '" + alrtag.Taille + "', '" + alrtag.Code_Barre + "')";
            query += ";"; // Pour terminer la requete
            try
            {
                MyInsertquery(query,false); 
            }
            catch (Exception e)
            {
                Log.add("Erreur insertTagAlert: " + e.Message);
                if (Properties.Settings.Default.UseDBGMSG)
                {
                    MessageBox.Show("Erreur insertTagAlert: " + e.Message);
                }
            }
        }

        //insertion dans la table tag_alert, liste les tag commandé, intrus et retiré
        public void insertTagAlert(int alrtID, string command, string retir, string intrus, string articletypecode, string taille, string code_barre)
        {
            string date = String.Format("{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now);

            Tag_Alert alrtag = new Tag_Alert();

            alrtag.Date_Creation = DateTime.Now;
            alrtag.Alert_Id = alrtID;
            alrtag.Tag_Command = command;
            alrtag.Tag_Retir = retir;
            alrtag.Tag_Intrus = intrus;
            alrtag.Article_Type_Code = articletypecode;
            alrtag.Taille = taille;
            alrtag.Code_Barre = code_barre;

            /*
             * `Id` int(32) NOT NULL AUTO_INCREMENT,
             * `Date_Creation` datetime NOT NULL,
             * `Alert_ID` int(32) NOT NULL,
             * `Tag_Command` varchar(255) NOT NULL,
             * `Tag_Retir` varchar(255) NOT NULL,
             * `Tag_Intrus` varchar(255) NOT NULL,
             * `Article_Type_Code` varchar(255) NOT NULL,
             * `Taille` varchar(255) NOT NULL,
             *  `Code_Barre` varchar(255) NOT NULL,
             * */
            //string query = "INSERT INTO `tag_alert`(`Id`, `Date_Creation`, `Alert_ID`, `Tag_Command`, `Tag_Retir`, `Tag_Intrus`, `Article_Type_Code`,`Taille`) ";
            string query = "INSERT INTO `tag_alert`(`Date_Creation`, `Alert_ID`, `Tag_Command`, `Tag_Retir`, `Tag_Intrus`, `Article_Type_Code`,`Taille`, `Code_Barre`) ";
            query += "VALUES (" /*+ id.ToString() + ",*/ + " '" + alrtag.Date_Creation.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + alrtag.Alert_Id + "', '"
                + alrtag.Tag_Command + "', '" + alrtag.Tag_Retir + "', '" + alrtag.Tag_Intrus + "', '" + alrtag.Article_Type_Code + "', '" + alrtag.Taille + "', '" + alrtag.Code_Barre + "')";
            query += ";"; // Pour terminer la requete
            try
            {
                MyInsertquery(query, false);
            }
            catch (Exception e)
            {
                Log.add("Erreur insertTagAlert: " + e.Message);
                if (Properties.Settings.Default.UseDBGMSG)
                {
                    MessageBox.Show("Erreur insertTagAlert: " + e.Message);
                }
            }
        }
/* inutilisé
        public void insertCase(int id, string taille, int maxitem, int numArmoire, string articletype)
        {
            int result = 0;
            DateTime dt = DateTime.Now;
            string date = String.Format("{0:yyyy-MM-dd HH:mm:ss}", dt);
            int toto = getArticleTypeDescid(articletype);

            Case c = new Case();

            c.Id = id;
            c.Taille = taille;
            c.Date_Creation = DateTime.Parse(date);
            c.Max_Item = maxitem;
            c.Armoire_ID = numArmoire;
            c.Article_Type_Id = toto;

            try
            {
                result = MyInsertquery(query);
            }
            catch (Exception e)
            {
                Log.add("Erreur insertCase: " + e.Message);
                if (Properties.Settings.Default.UseDBGMSG)
                {
                    MessageBox.Show("Erreur lors de l'insertion des données: " + e.Message);
                }
            }
        }
        */
        public void insertArticleType(int id, string code, string desc, string color, string sexe, string photo, int active, string desclong)
        {


            DateTime dt = DateTime.Now;
            string date = String.Format("{0:yyyy-MM-dd HH:mm:ss}", dt);

            Article_Type a = new Article_Type();
            /*
            CREATE TABLE IF NOT EXISTS `article_type` (
             `Id` int(8) NOT NULL,
             `Date_Creation` datetime NOT NULL,
             `Date_Modification` datetime NOT NULL,
             `Code` varchar(255) NOT NULL,
             `Type_Taille` char(1) NOT NULL DEFAULT '1',
             `Description` varchar(255) NOT NULL,
             `Couleur` varchar(255) NOT NULL,
             `Sexe` varchar(255) NOT NULL,
             `Photo` varchar(255) NOT NULL,
             `Active` int(8) NOT NULL,
             `Description_longue` varchar(255) NOT NULL COMMENT 'Intitulé qui est affiché dans l''application Web',
             PRIMARY KEY (`Id`)
            ) ENGINE=MyISAM DEFAULT CHARSET=latin1;
            */

            a.Id = id;
            a.Date_Modification = DateTime.Parse(date);
            a.Date_Creation = DateTime.Parse(date);
            a.Code = code;
            a.Description = desc;
            a.Couleur = color;
            a.Sexe = sexe;
            a.Photo = photo;
            a.Active = active;
            a.Description_longue = desclong;

            try
            {

                string query = "INSERT INTO `article_type` (`Id`,  `Date_Creation``, `Date_Modification`, `Code`,`Type_Taille`, `Description`,`Couleur`,`Sexe`,`Photo`,`Active`, `Description_longue`) ";
                query += "VALUES ( '" + a.Id.ToString() + "', '" + a.Date_Creation.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + a.Date_Modification.ToString("yyyy-MM-dd HH:mm:ss") + "', '" +
                         a.Code.ToString() + "', '" + "1" + a.Description.ToString() + "', '" + a.Couleur.ToString() + "', '" + a.Sexe.ToString() + "', '" + a.Photo.ToString() + "', '" + a.Active.ToString() + "'";
                query += ";"; // Pour terminer la requête
                MyInsertquery(query, false);
                
                //dbContext.AddToarticle_type(a);
                //dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                Log.add("Erreur insertArticleType: " + e.Message);
                if (Properties.Settings.Default.UseDBGMSG)
                {
                    MessageBox.Show("Erreur lors de l'insertion des données: " + e.Message);
                }
            }

        }

        // TODO
        public void insertArticleTaille(int article_type_id, string taille, int armoire, bool vide)
        {
            /*
             * CREATE TABLE IF NOT EXISTS `article_taille` (
              `Article_Type_ID` int(8) NOT NULL COMMENT 'Id du type article correspondant au vêtement',
              `Taille` varchar(255) NOT NULL COMMENT 'Taille pour l''article donné',
              `Armoire` int(8) NOT NULL COMMENT 'Numérotation de l''armoire',
              `Vide` tinyint(1) NOT NULL COMMENT 'indique si le type d''article/taille est épuisé',
              PRIMARY KEY (`Article_Type_ID`,`Taille`,`Armoire`)
            ) ENGINE=MyISAM DEFAULT CHARSET=latin1;
             * */
            Article_Taille a = new Article_Taille();
            a.Article_Type_ID = article_type_id;
            a.Taille = taille;
            a.Armoire = armoire;
            a.Vide = vide;

            string query = "INSERT INTO `article_taille`(`Article_Type_ID`, `Taille`, `Armoire`, `Vide`) ";
            query += "VALUES ( '" + a.Article_Type_ID.ToString() + "', '" + a.Taille.ToString() + "', '" 
                  + a.Armoire.ToString() + "', '" + a.Vide.ToString() + "' )";
            query += ";"; // Pour terminer la requête
            try
            {
                MyInsertquery(query,false);
            }
            catch (Exception e)
            {
                Log.add("Erreur insertArticleTaille: " + e.Message);
                if (Properties.Settings.Default.UseDBGMSG)
                {
                    MessageBox.Show("Erreur à l'insertion d'un ArticleTaille: " + e.Message);
                }
            }
        }

        /// <summary>
        /// opération éffectuée après scan
        /// </summary>
        /// <param name="list"></param>
        /// <param name="ReaderId"></param>
        /// <param name="state"></param>
        /// <param name="UserId"></param>
        /// <returns>liste des intrus</returns>
        public List<string> scan(List<Tag> list, int ReaderId, int state, string UserId)
        {
            Log.add("Scan armoire: " + list.Count.ToString() + " articles scannés");
            // getRfidReaderState(ReaderId);
            DateTime dt = DateTime.Now;
            string date = String.Format("{0:yyyy-MM-dd HH:mm:ss}", dt);

            if (IsAvailable())
            {
                int i = 0;
                // on récupère la liste des cases de cette armoire
                List<Case> listcase = getCase().ToList();
                // on créé une liste de tag inconnu
                List<string> listintrus = new List<string>();

                // on récupère les epc inconnus sans tenir compte de ReaderId
                List<Content_Arm> listca = new List<Content_Arm>();
                try
                {
                    // Requete  from f in dbContext.content_arm where f.State == 300 || f.State == 5 select f;
                    string query = "SELECT `Id`, `Creation_Date`, `Epc`, `State`, `RFID_Reader` FROM `content_arm`";
                    query += "WHERE `State` = '300' OR `State` = '5';";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {
                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            /*
                             * CREATE TABLE IF NOT EXISTS `content_arm` (
                            `Id` int(8) NOT NULL AUTO_INCREMENT,
                            `Creation_Date` datetime NOT NULL,
                            `Epc` varchar(255) NOT NULL,
                            `State` int(3) NOT NULL,
                            `RFID_Reader` int(60) NOT NULL,
                             PRIMARY KEY (`Id`)
                             ) ENGINE=MyISAM  DEFAULT CHARSET=latin1 AUTO_INCREMENT=141 ;
                             * */
                            int contid = int.Parse(dataReader.GetString("Id"));
                            DateTime contdate = dataReader.GetDateTime("Creation_Date");
                            string contepc = dataReader.GetString("Epc");
                            int contstat = int.Parse(dataReader.GetString("State"));
                            int contread = int.Parse(dataReader.GetString("RFID_Reader"));
                            Content_Arm cont_tmp = new Content_Arm(contid, contdate, contepc, contstat, contread);
                            /*
                            bool stillinarmoire=false;
                            if (cont_tmp.RFID_Reader == Properties.Settings.Default.NumArmoire) 
                            { // verification que le tag est toujours présent
                               foreach (var tag in list)
                               {
                                   if (tag.Epc.ToHexString() == cont_tmp.Epc)
                                   {
                                       stillinarmoire = true;
                                   }
                               }
                            }
                            if (stillinarmoire==true)*/
                            {
                                listca.Add(cont_tmp);
                                i++;
                            }
                              
                        }
                        con.Close();
                    }
                }
                catch (Exception e)
                {
                    if (Properties.Settings.Default.UseDBGMSG)
                    {
                        MessageBox.Show(e.Message);
                    }
                }

                // on récupère la liste des epc à l'état 100 dans toutes les autres armoires
                List<Content_Arm> list100anothercabinet = new List<Content_Arm>();
                try
                {
                    // Requete IQueryable<Content_Arm> query = from f in dbContext.content_arm where f.State == 100 && f.RFID_Reader!=ReaderId select f;
                    string query = "SELECT `Id`, `Creation_Date`, `Epc`, `State`, `RFID_Reader` FROM `content_arm`";
                    query += "WHERE `State` = '100' AND `RFID_Reader` != '" + ReaderId.ToString() + "';";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {
                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        while (dataReader.Read())
                        {
                            /*
                             * CREATE TABLE IF NOT EXISTS `content_arm` (
                            `Id` int(8) NOT NULL AUTO_INCREMENT,
                            `Creation_Date` datetime NOT NULL,
                            `Epc` varchar(255) NOT NULL,
                            `State` int(3) NOT NULL,
                            `RFID_Reader` int(60) NOT NULL,
                             PRIMARY KEY (`Id`)
                             ) ENGINE=MyISAM  DEFAULT CHARSET=latin1 AUTO_INCREMENT=141 ;
                             * */
                            int contid = int.Parse(dataReader.GetString("Id"));
                            DateTime contdate = dataReader.GetDateTime("Creation_Date");
                            string contepc = dataReader.GetString("Epc");
                            int contstat = int.Parse(dataReader.GetString("State"));
                            int contread = int.Parse(dataReader.GetString("RFID_Reader"));
                            Content_Arm cont_tmp = new Content_Arm(contid, contdate, contepc, contstat, contread);

                            list100anothercabinet.Add(cont_tmp);
                            i++;
                        }
                        con.Close();
                    }

                }
                catch (Exception e)
                {
                    if (Properties.Settings.Default.UseDBGMSG)
                    {
                        MessageBox.Show(e.Message);
                    }
                }

                // on vide le content_arm de l'armoire courante
                try
                {
                    string query = "DELETE FROM `content_arm` WHERE `RFID_Reader`='" + Properties.Settings.Default.NumArmoire.ToString() + "';";

                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {
                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        commande.ExecuteNonQuery();
                        con.Close();
                    }
                }
                catch (Exception e)
                {
                    Log.add("Erreur scan: " + e.Message);
                    if (Properties.Settings.Default.UseDBGMSG)
                    {
                        MessageBox.Show("Erreur lors du scan: op. delete " + e.Message);
                    }
                }

                // Remplissage de la table content_arm
                try
                {
                    foreach (var tag in list)
                    {
                        Content_Arm cont = new Content_Arm();

                        cont.Creation_Date = DateTime.Parse(date);
#if NEWIMPINJ
                        cont.Epc = tag.Epc.ToHexString();
#else

                        cont.Epc = tag.Epc;
#endif

                        cont.RFID_Reader = ReaderId;

                        cont.State = state;

                        if (listca != null)
                        {
                            foreach (Content_Arm ca in listca)
                            {
                                // si le Tag trouvé après le scan était déjà dans l'armoire et qu'il était dans un état intrus
                                // 300 ou 5 alors on ne change pas cet état car on ne peut pas passer de intrus à un autre état
                                if (cont.Epc == ca.Epc)
                                {
                                    cont.State = ca.State;
                                }
                            }
                        }

                        //Test Correction Clayes 28/05/2014
                        Epc Epc_tmp = getEpc(cont.Epc);
                        if (Epc_tmp != null)
                        {
                            List<Case> listcase_tmp = listcase.Where(x => x.Article_Type_Id == Epc_tmp.Article_Type_ID && x.Taille == Epc_tmp.Taille).ToList();
                            if (listcase_tmp == null || listcase_tmp.Count == 0)
                            {
                                cont.State = 5;
                                Log.add("epc " + cont.Epc + " est intrus (state = 5)");
                                listintrus.Add(cont.Epc);
                            }
                        }

                        if (list100anothercabinet != null)
                        {
                            foreach (Content_Arm ca in list100anothercabinet)
                            {
                                // si le Tag trouvé après le scan est à 100 dans l'autre armoire,
                                // on le supprime de la table content_arm,il sera ajouté par la suite
                                // et on le met à jour dans la table epc
                                if (cont.Epc == ca.Epc)
                                {
                                    //dbContext.content_arm.ToList().RemoveAll(x => x.Epc == ca.Epc);
                                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                                    {
                                        MySqlCommand commande = con.CreateCommand();
                                        commande.CommandText = "DELETE FROM `content_arm` WHERE `Epc`='" + ca.Epc + "';";
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
                                            Log.add("Erreur contentarm DELETE epc: " + e.Message);
                                            //Properties.Settings.Default.WriteLog = memSettings;

                                            if (Properties.Settings.Default.UseDBGMSG)
                                                System.Windows.MessageBox.Show("Erreur contentarm DELETE epc: " + e.Message);
                                        }
                                    }


                                    updateEpc(state, cont.Epc, UserId, ReaderId);
                                }
                            }
                        }

                        using (var con2 = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                        {
                            MySqlCommand insa_commande = con2.CreateCommand();
                            insa_commande.CommandText = "INSERT INTO `content_arm` (`Creation_Date`,`Epc`,`State`,`RFID_Reader`) VALUES ('" + 
                                String.Format("{0:yyyy-MM-dd HH:mm:ss}", cont.Creation_Date) + "', '" +
                                    cont.Epc.ToString() + "', '" +
                                    cont.State.ToString() + "', '" +
                                    cont.RFID_Reader.ToString() + "')" +
                                    /*"  WHERE `Id`= " + "'" + cont.Id.ToString() + "'" + */";";
                           /* upda_commande.CommandText = "UPDATE `content_arm` SET " +
                                "`Creation_Date`= " + "'" + String.Format("{0:yyyy-MM-dd HH:mm:ss}", cont.Creation_Date) + "'" + 
                                    ", `Epc`=" + "'" + cont.Epc.ToString() + "'" + 
                                    ", `State`=" + "'" + cont.State.ToString() + "'" + 
                                    ", `RFID_Reader`=" + "'" + cont.RFID_Reader.ToString() + "'" + 
                                    "  WHERE `Id`= " + "'" + cont.Id.ToString() + "'" + ";";*/
                            try
                            {
                                con2.Open();
                                insa_commande.ExecuteNonQuery();
                                Log.add(insa_commande.CommandText);
                                con2.Close();
                            }
                            catch (Exception e)
                            {
                                bool memSettings = Properties.Settings.Default.WriteLog;

                                Properties.Settings.Default.WriteLog = true;
                                Log.add("Erreur insert Content_Arm cont: " + e.Message);
                                //Properties.Settings.Default.WriteLog = memSettings;

                                if (Properties.Settings.Default.UseDBGMSG)
                                    System.Windows.MessageBox.Show("Erreur Update Content_Arm cont: " + e.Message);
                            }
                        }
                        //dbContext.AddTocontent_arm(cont);
                        //dbContext.SaveChanges();

                    }
                    return listintrus;
                }
                catch (Exception e)
                {
                    Log.add("Erreur scan: " + e.Message);
                    if (Properties.Settings.Default.UseDBGMSG)
                    {
                        MessageBox.Show("Erreur lors du scan: " + e.Message);
                    }
                }
            }
            else
            {
                Log.add("Erreur scan: ");
                if (Properties.Settings.Default.UseDBGMSG)
                {
                    MessageBox.Show("Erreur lors du scan: ");
                }
            }
            return new List<string>();
        }



        #endregion

        #region UPDATEs

        public void updateContent(int state, string tag)
        {
            Content_Arm ca = new Content_Arm();
            try
            {

                if (IsAvailable())
                {
                    int i = 0;
                    //Content_Arm co = dbContext.content_arm.FirstOrDefault(i => i.Epc == tag);
                    string query = "SELECT * FROM `content_arm` ";
                    query += " WHERE  `Epc`= '" + tag.ToString() +
                        "' LIMIT 1;";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {
                        int result = 0;
                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        if (dataReader.Read())
                        {
                            // CREATE TABLE IF NOT EXISTS `content_arm` (
                            //  `Id` int(8) NOT NULL AUTO_INCREMENT,
                            //  `Creation_Date` datetime NOT NULL,
                            //  `Epc` varchar(255) NOT NULL,
                            //  `State` int(3) NOT NULL,
                            //  `RFID_Reader` int(60) NOT NULL,
                            //  PRIMARY KEY (`Id`)
                            //) ENGINE=MyISAM  DEFAULT CHARSET=latin1 AUTO_INCREMENT=141 ;

                            ca.Id = int.Parse(dataReader.GetString("Id"));
                            ca.Creation_Date = dataReader.GetDateTime("Creation_Date");
                            ca.Epc = dataReader.GetString("Epc");
                            ca.State = int.Parse(dataReader.GetString("State"));
                            ca.RFID_Reader = int.Parse(dataReader.GetString("RFID_Reader"));
                            i++;
                        }
                        con.Close();
                        if (i > 0)
                        {
                            ca.State = state;
                            // dbContext.SaveChanges();
                            query = "UPDATE `content_arm` SET " +
                                
                                "  `State`='" +  ca.State.ToString() +
                                "' WHERE `Id`='" + ca.Id.ToString() +
                                "';";
                            using (var conu = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                            {
                                MySqlCommand commandeu = conu.CreateCommand();
                                commandeu.CommandText = query;

                                conu.Open();
                                result = commandeu.ExecuteNonQuery();
                                conu.Close();
                            }

                            if (result != 1)
                                throw (new Exception("requete non reussie (" + query + ")"));
                        }
                        else
                        {
                        }
                    }
                }
                else
                {
                    throw (new Exception("Pas d'acces a la base locale "));
                }

            }
            catch (Exception e)
            {
                Log.add("Erreur updateContent: " + e.Message);
                if (Properties.Settings.Default.UseDBGMSG)
                {
                    string msg = "Erreur lors de la mise à jour des données: " + e.Message;
                    if (e.InnerException != null) msg += "\n" + e.InnerException.Message;
                    MessageBox.Show(msg);
                }
            }
        }

        /// <summary>
        /// Mise à jour de la version logicielle dans la table version 
        /// </summary>
        /// <param name="Version">N° de version logiciel tel qu'enregistré dans l'assembly</param>
        public void updateVersionLogiciel(string Version)
        {
            int result = 0;
            DateTime dt = DateTime.Now;
            string date = String.Format("{0:yyyy-MM-dd HH:mm:ss}", dt);

            try
            {

                if (IsAvailable())
                {
                    string querysel = "SELECT * FROM `version` WHERE `Id`='" + Properties.Settings.Default.NumArmoire + "';";
                    string queryupd = "UPDATE `version` SET " + 
                        "  `NomPlace`='" + Properties.Settings.Default.Emplacement.ToString() +
                        "', `VersLog` ='" + Version.Replace('.','_') +
                        "', `VersMat` ='" + Properties.Settings.Default.VersMateriel.ToString() +
                        "' WHERE `Id`='" + Properties.Settings.Default.NumArmoire +
                        "';";
                    string queryins = "INSERT INTO `version` ( `Id`,`NomPlace`,`VersLog`,`VersMat`,`DateSynchro`) VALUES ('" +
                        Properties.Settings.Default.NumArmoire.ToString() + "','" + Properties.Settings.Default.Emplacement.ToString() + "','" + Version.Replace('.', '_') + "','" + Properties.Settings.Default.VersMateriel.ToString() + "','" + date + "');";

                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {
                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = querysel;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        if (dataReader.Read())
                            result = 1;
                        con.Close();
                    }

                    if (result == 1)
                    {
                        using (var conu = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                        {
                            MySqlCommand commandeu = conu.CreateCommand();
                            commandeu.CommandText = queryupd;

                            conu.Open();
                            commandeu.ExecuteNonQuery();
                            conu.Close();
                        }
                    }
                    else
                    {
                        using (var conu = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                        {
                            MySqlCommand commandeu = conu.CreateCommand();
                            commandeu.CommandText = queryins;

                            conu.Open();
                            commandeu.ExecuteNonQuery();
                            conu.Close();
                        }
                    }


                }
            }
            catch (Exception e)
            {
                Log.add("Erreur Updating Version : " + e.Message);
            }
        }

        /// <summary>
        ///  Mise à jour de la date pour la table version 
        /// </summary>
        public void updateVersionDate()
        {
            //Epc co = new Epc();
            DateTime dt = DateTime.Now;
            string date = String.Format("{0:yyyy-MM-dd HH:mm:ss}", dt);

            try
            {

                if (IsAvailable())
                {
                    string query = "UPDATE `version` SET " +
                        //"  `DateSynchro`=" + DateTime.Parse(date).ToString() +
                          "  `DateSynchro`='" + date +
                        "' WHERE `Id`='" + Properties.Settings.Default.NumArmoire +
                        "';";
                    using (var conu = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {
                        MySqlCommand commandeu = conu.CreateCommand();
                        commandeu.CommandText = query;

                        conu.Open();
                        commandeu.ExecuteNonQuery();
                        conu.Close();
                    }


                }
            }
            catch (Exception e)
            {
                Log.add("Erreur Updating Version : " + e.Message);
            }
        }
 
        /// <summary>
        /// Mis à jour de la table Alert par l'Id
        /// </summary>
        /// <param name="id">N° d'Id dans la table alerte locale</param>
        public void updateAlert(int id)
        {
            int i=0;
             Alert al = new Alert();
            try
            {

                if (IsAvailable())
                {
                    //Alert alr = dbContext.alert.FirstOrDefault(t => t.Id == id);
                    string query = "SELECT * FROM `alert` ";
                    query += " WHERE  `Id`= '" + id.ToString() +
                        "' LIMIT 1;";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {
                        int result = 0;
                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        if (dataReader.Read())
                        {

                            //CREATE TABLE IF NOT EXISTS `alert` (
                            // `Id` int(8) NOT NULL,
                            // `Date_Creation` datetime NOT NULL,
                            // `Alert_Type_Id` int(8) NOT NULL,
                            // `Message` varchar(2500) NOT NULL,
                            // `Origin` varchar(255) NOT NULL,
                            // `Login` varchar(32) NOT NULL,
                            // `Prenom` varchar(255) NOT NULL,
                            // `Nom` varchar(255) NOT NULL,
                            // `Armoire_Name` varchar(255) NOT NULL,
                            // `Armoire_ID` int(32) NOT NULL,
                            // `Traiter` smallint(1) NOT NULL,


                            al.Id = int.Parse(dataReader.GetString("Id"));
                            al.Date_Creation = dataReader.GetDateTime("Date_Creation");
                            al.Alert_Type_Id = int.Parse(dataReader.GetString("Alert_Type_Id"));
                            al.Message = dataReader.GetString("Message");
                            al.User_ID = int.Parse(dataReader.GetString("User_ID"));
                            //al.Origin = dataReader.GetString("Origin");
                            //al.Login = dataReader.GetString("Login");
                            //al.Prenom = dataReader.GetString("Prenom");
                            //al.Nom = dataReader.GetString("Nom");
                            al.Armoire_ID = int.Parse(dataReader.GetString("Armoire_ID"));
                            al.Traiter = int.Parse(dataReader.GetString("Traiter"));
                            // at = dbContext.article_taille.FirstOrDefault(i => i.Article_Type_ID == artype && i.Taille == taille && i.Armoire == armoire);
                            i++;
                        }
                        con.Close();
                        if (i > 0)
                        {
                            al.Traiter = 1;
                            // dbContext.SaveChanges();
                            query = "UPDATE `alert` SET " +
                                //CREATE TABLE IF NOT EXISTS `article_taille` (
                                //  `Article_Type_ID` int(8) NOT NULL COMMENT 'Id du type article correspondant au vêtement',
                                //  `Taille` varchar(255) NOT NULL COMMENT 'Taille pour l''article donné',
                                //  `Armoire` int(8) NOT NULL COMMENT 'Numérotation de l''armoire',
                                //  `Vide` tinyint(1) NOT NULL COMMENT 'indique si le type d''article/taille est épuisé',

                                "  `Traiter`=" + al.Traiter.ToString() +
                                " WHERE `Id`='" + al.Id.ToString() +
                                "';";
                            using (var conu = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                            {
                                MySqlCommand commandeu = conu.CreateCommand();
                                commandeu.CommandText = query;

                                conu.Open();
                                result = commandeu.ExecuteNonQuery();
                                conu.Close();
                            }

                            if (result != 1)
                                throw (new Exception("requete non reussie (" + query + ")"));
                        }
                        else
                        {
                        }
                    }
                }
                else
                {
                    throw (new Exception("Pas d'acces a la base locale "));
                }
            }
            catch (Exception e)
            {
                Log.add("Erreur updateAlert: " + e.Message);
                if (Properties.Settings.Default.UseDBGMSG)
                {
                    string msg = "Erreur lors de la mise à jour des données: " + e.Message;
                    if (e.InnerException != null) msg += "\n" + e.InnerException.Message;
                    MessageBox.Show(msg);
                }
            }

        }

        /// <summary>
        /// Mise à jour de la table Article Taille pour une Entrée existante
        /// </summary>
        /// <param name="artype">Id du Type d'article tel que défini dans la table Article_Type</param>
        /// <param name="taille">Taille (chaine)</param>
        /// <param name="armoire">N° de l'armoire concernée</param>
        /// <param name="vide">Etat des stocks pour l'armoire concernée</param>
        public void updateArticleTaille(int artype, string taille, int armoire, bool vide)
        {
            int i = 0;
            Article_Taille at = new Article_Taille();

            try
            {
                if (IsAvailable())
                {
                    //dbContext.article_taille.FirstOrDefault(i => i.Article_Type_ID == artype && i.Taille == taille && i.Armoire == armoire);
                    string query = "SELECT * FROM `article_taille` ";
                        query += " WHERE  `Article_Type_ID`= '" + artype.ToString() + 
                            "' AND `Taille`= '" + taille.ToString() + 
                            "' AND `Armoire`= '" + armoire.ToString() + 
                            "' LIMIT 1;";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {
                        int result = 0;
                            MySqlCommand commande = con.CreateCommand();
                            commande.CommandText = query;

                            con.Open();
                            MySqlDataReader dataReader = commande.ExecuteReader();
                            if (dataReader.Read())
                            {

                                //CREATE TABLE IF NOT EXISTS `article_taille` (
                                //  `Article_Type_ID` int(8) NOT NULL COMMENT 'Id du type article correspondant au vêtement',
                                //  `Taille` varchar(255) NOT NULL COMMENT 'Taille pour l''article donné',
                                //  `Armoire` int(8) NOT NULL COMMENT 'Numérotation de l''armoire',
                                //  `Vide` tinyint(1) NOT NULL COMMENT 'indique si le type d''article/taille est épuisé',
                                //  PRIMARY KEY (`Article_Type_ID`,`Taille`,`Armoire`)
                                //) ENGINE=MyISAM DEFAULT CHARSET=latin1;


                                at.Article_Type_ID = int.Parse(dataReader.GetString("Article_Type_ID"));
                                at.Taille = dataReader.GetString("Taille");
                                at.Armoire = int.Parse(dataReader.GetString("Armoire"));
                                at.Vide = bool.Parse(dataReader.GetString("Vide"));
                                // at = dbContext.article_taille.FirstOrDefault(i => i.Article_Type_ID == artype && i.Taille == taille && i.Armoire == armoire);
                                i++;
                            }
                            con.Close();
                            if (i > 0)
                            {
                                at.Vide = vide;
                                // dbContext.SaveChanges();
                                query = "UPDATE `article_taille` SET " +
                                    //CREATE TABLE IF NOT EXISTS `article_taille` (
                                    //  `Article_Type_ID` int(8) NOT NULL COMMENT 'Id du type article correspondant au vêtement',
                                    //  `Taille` varchar(255) NOT NULL COMMENT 'Taille pour l''article donné',
                                    //  `Armoire` int(8) NOT NULL COMMENT 'Numérotation de l''armoire',
                                    //  `Vide` tinyint(1) NOT NULL COMMENT 'indique si le type d''article/taille est épuisé',

                                    "  `Vide`=" + at.Vide.ToString() +
                                    " WHERE `Article_Type_ID`='" + at.Article_Type_ID.ToString() +
                                    "' AND `Taille`= '" + taille.ToString() +
                                    "' AND `Armoire`= '" + armoire.ToString() + 
                                    "';";
                                using (var conu = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                                {
                                    MySqlCommand commandeu = conu.CreateCommand();
                                    commandeu.CommandText = query;

                                    conu.Open();
                                    result = commandeu.ExecuteNonQuery();
                                    conu.Close();
                                }

                                if (result != 1)
                                    throw (new Exception("requete non reussie (" + query + ")"));
                            }
                            else
                            {
                            }
                    }
                    


              }
              else
              {
                  Log.add("Erreur updateArticleTaille: pas d'accès à la base de donnée locale");
                    throw (new Exception("pas d'acces à la base locale"));
                    //return new Case(0,0,"00", new DateTime(1971, 01, 01), 0, 0,0);
              }
                
            }
            catch (Exception e)
            {
                Log.add("Erreur updateArticleTaille: " + e.Message);
                if (Properties.Settings.Default.UseDBGMSG)
                {
                    string msg = "Erreur lors de la mise à jour des données: " + e.Message;
                    if (e.InnerException != null) msg += "\n" + e.InnerException.Message;
                    MessageBox.Show(msg);
                }
            }
        }

        /// <summary>
        /// Mise à jour de la dotation (table epc) en fonction du 
        /// </summary>
        /// <param name="state"></param>
        /// <param name="tag"></param>
        /// <param name="reader"></param>
        /// <param name="user"></param>
        public void updateEpcReload(int state, string tag, int reader, string user)
        {
            //Epc co = new Epc();
            DateTime dt = DateTime.Now;
            string date = String.Format("{0:yyyy-MM-dd HH:mm:ss}", dt);

            try
            {
                //co = dbContext.epc.FirstOrDefault(i => i.Tag == tag && i.State != 300 && i.State != 5 && i.State != 2);
                Epc tmpepc = new Epc();
                tmpepc = GetBackEpcfromTag(tag, " `State` != '300' AND `State` != '5' AND `State` != '2' " );
                
                if (tmpepc == null) // l'epc n'est ni inconnu ni perdu
                {
                    //Epc ee = dbContext.epc.FirstOrDefault(i => i.Tag == tag); // le connait-on ?
                    Epc tmpepc2 = new Epc();
                    tmpepc2 = GetBackEpcfromTag(tag);
                    if (tmpepc2 == null) // non
                    {
                        Log.add("updateEpcReload: Tag " + tag + " - State " +/* co.State +*/ " => 300");
                        updateContent(300, tag);
                    }
                    else // oui
                    {
                        Log.add("updateEpcReload: Tag " + tag + " - State " +/* co.State +*/ " => " + tmpepc2.State);
                        updateContent(tmpepc2.State, tag);
                    }
                }
                else
                {
                    if (tmpepc.State == EtatArticle.SORTI || tmpepc.State == EtatArticle.RESTITUE || tmpepc.State == EtatArticle.TRI_SALE || tmpepc.State == EtatArticle.AUCUN || tmpepc.State == EtatArticle.SORTI_TailleSup)
                    {
                        Log.add("updateEpcReload: Tag " + tag + " - State " + tmpepc.State + " => " + state);
                        tmpepc.Armoire_ID = Properties.Settings.Default.NumArmoire;
                        tmpepc.State = state;
                        tmpepc.Last_Action_Date = DateTime.Parse(date);
                        tmpepc.Date_Modification = DateTime.Parse(date);
                        tmpepc.Last_User = user;
                        tmpepc.Last_Reader = reader.ToString();
                        tmpepc.Movement = tmpepc.Movement++;
                        tmpepc.Cycle_Lavage_Count = tmpepc.Cycle_Lavage_Count++;
                    }
                    UpdateEpcforid(tmpepc.Id, tmpepc);
                    //dbContext.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Log.add("Erreur updateEpcReload: Tag " + tag + " - " + e.Message);
                if (Properties.Settings.Default.UseDBGMSG)
                    System.Windows.MessageBox.Show("updateEpcReload: Tag inexistant");
            }
        }

        // v2
        public void updateEpc(int state, string tag, string user, int reader)
        {
            try
            {
                DateTime dt = DateTime.Now;
                string date = String.Format("{0:yyyy-MM-dd HH:mm:ss}", dt);
                //Epc co = dbContext.epc.FirstOrDefault(i => i.Tag == tag);
                Epc tmpepc = new Epc();
                tmpepc = GetBackEpcfromTag(tag);
                if (tmpepc != null)
                {
                    tmpepc.State = state;
                    tmpepc.Last_Action_Date = DateTime.Parse(date);
                    tmpepc.Date_Modification = DateTime.Parse(date);
                    tmpepc.Last_User = user;
                    tmpepc.Last_Reader = reader.ToString();
                    tmpepc.Movement = tmpepc.Movement++;
                    tmpepc.Cycle_Lavage_Count = tmpepc.Cycle_Lavage_Count++;
                    tmpepc.Armoire_ID = reader;
                    UpdateEpcforid(tmpepc.Id, tmpepc);
                    //dbContext.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Log.add("Erreur updateEpc: " + e.Message);
                if (Properties.Settings.Default.UseDBGMSG)
                    System.Windows.MessageBox.Show("updateEpc: Tag inexistant");
            }
        }

        // v3
        public void updateEpc(int state, string tag, string user, int reader, int actif)
        {
            try
            {
                 //Epc temp = dbContext.epc.FirstOrDefault(x => x.Id == e.Id);
                DateTime dt = DateTime.Now;
                string date = String.Format("{0:yyyy-MM-dd HH:mm:ss}", dt);


                //Epc co = dbContext.epc.FirstOrDefault(i => i.Tag == tag);
                Epc tmpepc = new Epc();
                tmpepc = GetBackEpcfromTag(tag);
                if (tmpepc != null)
                {

                    tmpepc.State = state;
                    tmpepc.Last_Action_Date = DateTime.Parse(date);
                    tmpepc.Date_Modification = DateTime.Parse(date);
                    tmpepc.Last_User = user;
                    tmpepc.Last_Reader = reader.ToString();
                    tmpepc.Movement = tmpepc.Movement++;
                    tmpepc.Cycle_Lavage_Count = tmpepc.Cycle_Lavage_Count++;
                    tmpepc.Armoire_ID = reader;
                    tmpepc.Actif = actif;
                    UpdateEpcforid(tmpepc.Id, tmpepc);
                    //dbContext.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Log.add("Erreur updateEpc: " + e.Message);
                if (Properties.Settings.Default.UseDBGMSG)
                    System.Windows.MessageBox.Show("updateEpc: Tag inexistant");
            }
        }

        public void updateEpcRestitution(Epc e, int reader)
        {
            try
            {
                //Epc temp = dbContext.epc.FirstOrDefault(x => x.Id == e.Id);
                DateTime dt = DateTime.Now;
                string date = String.Format("{0:yyyy-MM-dd HH:mm:ss}", dt);


                //Epc co = dbContext.epc.FirstOrDefault(i => i.Tag == tag);
                Epc tmpepc = new Epc();
                tmpepc = GetBackEpcfromTag(e.Tag);
                if (tmpepc != null)
                //if (temp != null)
                {
                    tmpepc.State = EtatArticle.RESTITUE;
                    tmpepc.Date_Modification = DateTime.Parse(date);
                    tmpepc.Last_Action_Date = DateTime.Parse(date);
                    tmpepc.Last_Reader = reader.ToString();
                    tmpepc.Cycle_Lavage_Count++;
                    tmpepc.Movement++;
                    tmpepc.Armoire_ID = reader;
                    //dbContext.SaveChanges();
                    UpdateEpcforid(tmpepc.Id, tmpepc);
                    Log.add("Restitution: mise à jour de l'epc " + tmpepc.Tag + ": State = " + tmpepc.State.ToString());
                }
            }
            catch (Exception ex)
            {
                Log.add("Erreur updateEpcRestitution: " + ex.Message);
                if (Properties.Settings.Default.UseDBGMSG)
                    System.Windows.MessageBox.Show("updateEpcRestitution: Tag inexistant");
            }
        }

        public void UpdateEpcforid(int id, Epc ep)
        {
            int result = 0;
            if (IsAvailable())
            {
                string query = "UPDATE `epc` SET " +
                    //CREATE TABLE IF NOT EXISTS `epc` (
                    //  `Id` int(8) NOT NULL,
                    //  `Date_Creation` datetime NOT NULL,
                    //  `Date_Modification` datetime NOT NULL,
                    //  `Tag` varchar(255) NOT NULL,
                    //  `Code_Barre` varchar(255) NOT NULL,
                    //  `Taille` varchar(255) NOT NULL,
                    //  `Type_Taille` char(1) NOT NULL,
                    //  `Cycle_Lavage_Count` int(8) NOT NULL,
                    //  `State` int(8) NOT NULL,
                    //  `Last_User` varchar(255) NOT NULL,
                    //  `Last_Reader` varchar(255) NOT NULL,
                    //  `Last_Action` varchar(255) NOT NULL,
                    //  `Last_Action_Date` datetime NOT NULL,
                    //  `Movement` int(8) NOT NULL,
                    //  `Article_Type_ID` int(8) NOT NULL,
                    //  `Case_ID` int(8) NOT NULL,
                    //  `Armoire_ID` int(8) NOT NULL,
                    //  `Actif` int(8) NOT NULL DEFAULT '1',
                    //  PRIMARY KEY (`Id`)
                    //) ENGINE=MyISAM DEFAULT CHARSET=latin1;

                    "  `Id`='" + ep.Id.ToString() +
                    "', `Date_Modification`='" + ep.Date_Modification.ToString("yyyy-MM-dd HH:mm:ss") +
                    "', `Date_Creation`='" + ep.Date_Creation.ToString("yyyy-MM-dd HH:mm:ss") +
                    "', `Tag`='" + ep.Tag.ToString() +
                    "', `Code_Barre`='" + ep.Code_Barre.ToString() +
                    "', `Taille` = '" + ep.Taille.ToString() +
                    "', `Type_Taille` = '" + ep.Type_Taille.ToString() +
                    "', `Cycle_Lavage_Count` = '" + ep.Cycle_Lavage_Count.ToString() +
                    "', `State` = '" + ep.State.ToString() +
                    "', `Last_User` = '" + ep.Last_User.ToString() +
                    "', `Last_Reader` = '" + ep.Last_Reader.ToString() +
                    "', `Last_Action` = '" + ep.Taille.ToString() +
                    "', `Last_Action_Date` = '" + ep.Last_Action_Date.ToString("yyyy-MM-dd HH:mm:ss") +
                    "', `Movement` = '" + ep.Movement.ToString() +

                    "', `Article_Type_Id`='" + ep.Article_Type_ID.ToString() +
                    "', `Case_ID`='" + ep.Case_ID.ToString() +
                    "', `Armoire_ID`='" + ep.Armoire_ID.ToString() +
                    "', `Actif`='" + ep.Actif.ToString() +
                     "' WHERE `Id`=" + ep.Id.ToString() + ";";
                using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                {
                    MySqlCommand commande = con.CreateCommand();
                    commande.CommandText = query;

                    con.Open();
                    result = commande.ExecuteNonQuery();
                    con.Close();
                }

                if (result != 1)
                    throw (new Exception("requete non reussie (" + query + ")"));
            }
        }

        private Epc GetBackEpcfromTag(string tag)
        {
            int i = 0;

            if (IsAvailable())
            {
                // try
                {
                    Epc tmpepc = new Epc();
                    string query = "SELECT * FROM `epc` ";
                    query += " WHERE  `Tag`= '" + tag.ToString() + "' LIMIT 1;";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        if (dataReader.Read())
                        {

                            //
                            //CREATE TABLE IF NOT EXISTS `epc` (
                            //  `Id` int(8) NOT NULL,
                            //  `Date_Creation` datetime NOT NULL,
                            //  `Date_Modification` datetime NOT NULL,
                            //  `Tag` varchar(255) NOT NULL,
                            //  `Code_Barre` varchar(255) NOT NULL,
                            //  `Taille` varchar(255) NOT NULL,
                            //  `Type_Taille` char(1) NOT NULL,
                            //  `Cycle_Lavage_Count` int(8) NOT NULL,
                            //  `State` int(8) NOT NULL,
                            //  `Last_User` varchar(255) NOT NULL,
                            //  `Last_Reader` varchar(255) NOT NULL,
                            //  `Last_Action` varchar(255) NOT NULL,
                            //  `Last_Action_Date` datetime NOT NULL,
                            //  `Movement` int(8) NOT NULL,
                            //  `Article_Type_ID` int(8) NOT NULL,
                            //  `Case_ID` int(8) NOT NULL,
                            //  `Armoire_ID` int(8) NOT NULL,
                            //  `Actif` int(8) NOT NULL DEFAULT '1',
                            //  PRIMARY KEY (`Id`)
                            //) ENGINE=MyISAM DEFAULT CHARSET=latin1;


                            tmpepc.Id = int.Parse(dataReader.GetString("Id"));
                            tmpepc.Date_Creation = dataReader.GetDateTime("Date_Creation");
                            tmpepc.Date_Modification = dataReader.GetDateTime("Date_Modification");
                            tmpepc.Tag = dataReader.GetString("Tag");
                            tmpepc.Code_Barre = dataReader.GetString("Code_Barre");
                            tmpepc.Taille = dataReader.GetString("Taille");
                            tmpepc.Type_Taille = dataReader.GetString("Type_Taille");
                            tmpepc.Cycle_Lavage_Count = int.Parse(dataReader.GetString("Cycle_Lavage_Count"));
                            tmpepc.State = int.Parse(dataReader.GetString("State"));
                            tmpepc.Last_User = dataReader.GetString("Last_User");
                            tmpepc.Last_Reader = dataReader.GetString("Last_Reader");
                            tmpepc.Last_Action = dataReader.GetString("Last_Action");
                            tmpepc.Last_Action_Date = dataReader.GetDateTime("Last_Action_Date");
                            tmpepc.Movement = int.Parse(dataReader.GetString("Movement"));
                            tmpepc.Article_Type_ID = int.Parse(dataReader.GetString("Article_Type_Id"));
                            tmpepc.Case_ID = int.Parse(dataReader.GetString("Case_ID"));
                            tmpepc.Armoire_ID = int.Parse(dataReader.GetString("Armoire_ID"));
                            tmpepc.Actif = int.Parse(dataReader.GetString("Actif"));
                            i++;
                        }
                        con.Close();
                        if (i > 0)
                            return tmpepc;
                        else
                            return null;
                    }
                    
                    
                }
                /*   catch (Exception e)
                   {
                       Log.add("Erreur GetBackcasefromid: " + e.Message);

                       return new Case(0,0,"00", new DateTime(1971, 01, 01), 0, 0,0);
                   }*/
            }
            else
            {
                Log.add("Erreur GetBackcasefromid: pas d'accès à la base de donnée locale");
                throw (new Exception("pas d'acces à la base locale"));
                //return new Case(0,0,"00", new DateTime(1971, 01, 01), 0, 0,0);
            }
        }


        private string GetBackCodeBarrefromEpc(string tag)
        {
            int i = 0;

            if (IsAvailable())
            {
                try
                {
                    Epc tmpepc = new Epc();
                    string query = "SELECT `Code_Barre` FROM `epc` ";
                    query += " WHERE  `Tag`= '" + tag.ToString() + "' LIMIT 1;";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        if (dataReader.Read())
                        {

                            //
                            //CREATE TABLE IF NOT EXISTS `epc` (
                            //  `Id` int(8) NOT NULL,
                            //  `Date_Creation` datetime NOT NULL,
                            //  `Date_Modification` datetime NOT NULL,
                            //  `Tag` varchar(255) NOT NULL,
                            //  `Code_Barre` varchar(255) NOT NULL,
                            //  `Taille` varchar(255) NOT NULL,
                            //  `Type_Taille` char(1) NOT NULL,
                            //  `Cycle_Lavage_Count` int(8) NOT NULL,
                            //  `State` int(8) NOT NULL,
                            //  `Last_User` varchar(255) NOT NULL,
                            //  `Last_Reader` varchar(255) NOT NULL,
                            //  `Last_Action` varchar(255) NOT NULL,
                            //  `Last_Action_Date` datetime NOT NULL,
                            //  `Movement` int(8) NOT NULL,
                            //  `Article_Type_ID` int(8) NOT NULL,
                            //  `Case_ID` int(8) NOT NULL,
                            //  `Armoire_ID` int(8) NOT NULL,
                            //  `Actif` int(8) NOT NULL DEFAULT '1',
                            //  PRIMARY KEY (`Id`)
                            //) ENGINE=MyISAM DEFAULT CHARSET=latin1;

                            tmpepc.Code_Barre = dataReader.GetString("Code_Barre");
                            i++;
                        }
                        con.Close();
                        if (i > 0)
                            return tmpepc.Code_Barre;
                        else
                            return "";
                    }


                }
                catch (Exception e)
                {
                    Log.add("Erreur GetBackCodeBarrefromEpc: " + e.Message);

                       return "";
                }
            }
            else
            {
                Log.add("Erreur GetBackCodeBarrefromEpc: pas d'accès à la base de donnée locale");
                throw (new Exception("pas d'acces à la base locale"));
                
            }
            //return "";
        }

        private Epc GetBackEpcfromTag(string tag, string moreCondition)
        {
            int i = 0;
            if (IsAvailable())
            {
                // try
                {
                    Epc tmpepc = new Epc();
                    string query = "SELECT * FROM `epc` ";
                    query += " WHERE  `Tag`= '" + tag.ToString() + "' AND "+ moreCondition+" LIMIT 1;";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        if (dataReader.Read())
                        {

                            //
                            //CREATE TABLE IF NOT EXISTS `epc` (
                            //  `Id` int(8) NOT NULL,
                            //  `Date_Creation` datetime NOT NULL,
                            //  `Date_Modification` datetime NOT NULL,
                            //  `Tag` varchar(255) NOT NULL,
                            //  `Code_Barre` varchar(255) NOT NULL,
                            //  `Taille` varchar(255) NOT NULL,
                            //  `Type_Taille` char(1) NOT NULL,
                            //  `Cycle_Lavage_Count` int(8) NOT NULL,
                            //  `State` int(8) NOT NULL,
                            //  `Last_User` varchar(255) NOT NULL,
                            //  `Last_Reader` varchar(255) NOT NULL,
                            //  `Last_Action` varchar(255) NOT NULL,
                            //  `Last_Action_Date` datetime NOT NULL,
                            //  `Movement` int(8) NOT NULL,
                            //  `Article_Type_ID` int(8) NOT NULL,
                            //  `Case_ID` int(8) NOT NULL,
                            //  `Armoire_ID` int(8) NOT NULL,
                            //  `Actif` int(8) NOT NULL DEFAULT '1',
                            //  PRIMARY KEY (`Id`)
                            //) ENGINE=MyISAM DEFAULT CHARSET=latin1;


                            tmpepc.Id = int.Parse(dataReader.GetString("Id"));
                            //DateTime toto;
                            //toto = dataReader.GetDateTime(dataReader.GetOrdinal("Date_Creation"));
                            //toto = Convert.ToDateTime(dataReader["Date_Creation"]).ToString("yyyy-MM-JJ HH:mm:ss");
                            //toto = dataReader.GetDateTime("Date_Creation").ToString();
                            tmpepc.Date_Creation = DateTime.Parse(dataReader.GetDateTime("Date_Creation").ToString());
                            tmpepc.Date_Modification = DateTime.Parse(dataReader.GetDateTime("Date_Modification").ToString());
                            tmpepc.Tag = dataReader.GetString("Tag");
                            tmpepc.Code_Barre = dataReader.GetString("Code_Barre");
                            tmpepc.Taille = dataReader.GetString("Taille");
                            tmpepc.Type_Taille = dataReader.GetString("Type_Taille");
                            tmpepc.Cycle_Lavage_Count = int.Parse(dataReader.GetString("Cycle_Lavage_Count"));
                            tmpepc.State = int.Parse(dataReader.GetString("State"));
                            tmpepc.Last_User = dataReader.GetString("Last_User");
                            tmpepc.Last_Reader = dataReader.GetString("Last_Reader");
                            tmpepc.Last_Action = dataReader.GetString("Last_Action");
                            tmpepc.Last_Action_Date = DateTime.Parse(dataReader.GetDateTime("Last_Action_Date").ToString());
                            tmpepc.Movement = int.Parse(dataReader.GetString("Movement"));
                            tmpepc.Article_Type_ID = int.Parse(dataReader.GetString("Article_Type_Id"));
                            tmpepc.Case_ID = int.Parse(dataReader.GetString("Case_ID"));
                            tmpepc.Armoire_ID = int.Parse(dataReader.GetString("Armoire_ID"));
                            tmpepc.Actif = int.Parse(dataReader.GetString("Actif"));
                            i++;
                        }
                        con.Close();
                        if (i> 0)
                            return tmpepc;
                        else
                            return null;
                    }


                }
                /*   catch (Exception e)
                   {
                       Log.add("Erreur GetBackcasefromid: " + e.Message);

                       return new Case(0,0,"00", new DateTime(1971, 01, 01), 0, 0,0);
                   }*/
            }
            else
            {
                Log.add("Erreur GetBackcasefromid: pas d'accès à la base de donnée locale");
                throw (new Exception("pas d'acces à la base locale"));
                //return new Case(0,0,"00", new DateTime(1971, 01, 01), 0, 0,0);
            }
        }

        public void updateEpcRestitution(int state, string tag, int reader)
        {
            try
            {

                DateTime dt = DateTime.Now;
                string date = String.Format("{0:yyyy-MM-dd HH:mm:ss}", dt);
               

                //Epc co = dbContext.epc.FirstOrDefault(i => i.Tag == tag);
                Epc tmpepc = new Epc();
                tmpepc = GetBackEpcfromTag(tag);
                if (tmpepc != null)
                {
                    int cycle = tmpepc.Cycle_Lavage_Count;

                    tmpepc.State = state;
                    tmpepc.Last_Action_Date = DateTime.Parse(date);
                    tmpepc.Date_Modification = DateTime.Parse(date);

                    tmpepc.Last_Reader = reader.ToString();

                    tmpepc.Armoire_ID = reader;

                    tmpepc.Movement = tmpepc.Movement++;
                    tmpepc.Cycle_Lavage_Count = cycle++;

                    UpdateEpcforid(tmpepc.Id, tmpepc);
                    //dbContext.SaveChanges();
                }

                

            }
            catch (Exception e)
            {
                Log.add("Erreur updateEpcRestitution: " + e.Message);
                if (Properties.Settings.Default.UseDBGMSG)
                    System.Windows.MessageBox.Show("updateEpcRestitution: Tag inexistant");
            }
        }

        public void updateCase(int id, string taille, int maxitem, int numArmoire, string articletype)
        {
            try
            {
                int arttypid = getArticleTypeDescid(articletype);

                Case casetmp = new Case(0, 0, "00", new DateTime(1971, 1, 1), 0, 0, 0);
                casetmp = GetBackcasefromid(id);
                //                Case lala = dbContext.@case.FirstOrDefault(i => i.Id == id);

                //lala.Id = id;
                casetmp.Taille = taille;

                casetmp.Max_Item = maxitem;
                casetmp.Armoire_ID = numArmoire;
                casetmp.Article_Type_Id = arttypid;

                Updatecasefromid(id, casetmp);
                //dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                Log.add("Erreur updateCase: " + e.Message);
                if (Properties.Settings.Default.UseDBGMSG)
                {
                    string msg = "Erreur lors de la mise à jour des données: " + e.Message;
                    if (e.InnerException != null) msg += "\n" + e.InnerException.Message;
                    MessageBox.Show(msg);
                }
            }
        }

        public void Updatecasefromid(int id, Case ca)
        {
            int result = 0;
            if (IsAvailable())
            {
                string query = "UPDATE `case` SET " +
                    //`Id` int(8) NOT NULL,
                    //`Bind_ID` int(32) NOT NULL,
                    //`Taille` varchar(255) NOT NULL,
                    //`Date_Creation` datetime NOT NULL,
                    //`Max_Item` int(8) NOT NULL,
                    //`Article_Type_Id` int(8) NOT NULL,
                    //`Armoire_ID` int(8) NOT NULL,
                    "  `Bind_ID`='" + ca.Bind_ID.ToString() +
                    "', `Taille` = '" + ca.Taille.ToString() +
                    "', `Date_Creation`='" + ca.Date_Creation.ToString("yyyy-MM-dd HH:mm:ss") +
                    "', `Max_Item`='" + ca.Max_Item.ToString() +
                    "', `Article_Type_Id`='" + ca.Article_Type_Id.ToString() +
                    "', `Armoire_ID`='" + ca.Armoire_ID.ToString() +
                     "' WHERE `Id`=" + ca.Id.ToString() + ";";
                using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                {
                    MySqlCommand commande = con.CreateCommand();
                    commande.CommandText = query;

                    con.Open();
                    result = commande.ExecuteNonQuery();
                    con.Close();
                }

                if (result != 1)
                    throw (new Exception("requete non reussie (" + query + ")"));
            }
        }

        private Case GetBackcasefromid(int id)
        {

            if (IsAvailable())
            {
                // try
                {
                    Case tmpcase = new Case(0,0,"00", new DateTime(1971, 01, 01), 0, 0,0);
                    string query = "SELECT * FROM `case` ";
                    query += " WHERE  `Id`= '" + id.ToString() + "' LIMIT 1;";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        if (dataReader.Read())
                        {
                                      
                            //CREATE TABLE IF NOT EXISTS `case` (
                            //`Id` int(8) NOT NULL,
                            //`Bind_ID` int(32) NOT NULL,
                            //`Taille` varchar(255) NOT NULL,
                            //`Date_Creation` datetime NOT NULL,
                            //`Max_Item` int(8) NOT NULL,
                            //`Article_Type_Id` int(8) NOT NULL,
                            //`Armoire_ID` int(8) NOT NULL,
                            //PRIMARY KEY (`Id`)
                            //) ENGINE=MyISAM DEFAULT CHARSET=latin1;

                            tmpcase.Id = int.Parse(dataReader.GetString("Id"));
                            tmpcase.Bind_ID = int.Parse(dataReader.GetString("Bind_ID"));
                            tmpcase.Taille = dataReader.GetString("Taille");
                            tmpcase.Date_Creation = dataReader.GetDateTime("Date_Creation");
                            tmpcase.Max_Item = int.Parse(dataReader.GetString("Max_Item"));
                            tmpcase.Article_Type_Id = int.Parse(dataReader.GetString("Article_Type_Id"));
                            tmpcase.Armoire_ID = int.Parse(dataReader.GetString("Armoire_ID"));
                        }
                        con.Close();
                    }
                    return tmpcase;
                }
                /*   catch (Exception e)
                   {
                       Log.add("Erreur GetBackcasefromid: " + e.Message);

                       return new Case(0,0,"00", new DateTime(1971, 01, 01), 0, 0,0);
                   }*/
            }
            else
            {
                Log.add("Erreur GetBackcasefromid: pas d'accès à la base de donnée locale");
                throw (new Exception("pas d'acces à la base locale"));
                //return new Case(0,0,"00", new DateTime(1971, 01, 01), 0, 0,0);
            }
        }

        public void UpdateUserArticleRestitutionHebdo(User_Article ua)
        {

            int result = 0;
            if (IsAvailable())
            {
                try
                {
                    string query = "UPDATE `user_article` SET " +
                        "`Date_Modification`='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +
                        "', `Credit_Restant`='" + ua.Credit_Semaine_Suivante.ToString() +
                        "' WHERE `Id`=" + ua.Id.ToString() + ";";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {
                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        result = commande.ExecuteNonQuery();
                        con.Close();
                    }

                    if (result != 1)
                        throw (new Exception("requete non reussie (" + query + ")"));

                    result = 0;

                    query = "UPDATE `user_article` SET " +
                        "`Credit_Semaine_Suivante` = '0" +
                        "' WHERE `Id`=" + ua.Id.ToString() + ";";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {
                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        result = commande.ExecuteNonQuery();
                        con.Close();
                    }

                    if (result != 1)
                        throw (new Exception("requete non reussie (" + query + ")"));
                }
                catch (Exception e)
                {
                    Log.add("Erreur UpdateUserArticleRestitutionHebdo: " + e.Message);
                    if (Properties.Settings.Default.UseDBGMSG)
                    {
                        string msg = "Erreur lors de la mise à jour des données: " + e.Message;
                        if (e.InnerException != null) msg += "\n" + e.InnerException.Message;
                        MessageBox.Show(msg);
                    }
                }
            }
        }


        private void UpdateUserArticle(User_Article ua)
        {
            int result = 0;
            if (IsAvailable())
            {
                string query = "UPDATE `user_article` SET " +
                    "  `Date_Creation`='" + ua.Date_Creation.ToString("yyyy-MM-dd HH:mm:ss") +
                    "', `Date_Modification`='" + ua.Date_Modification.ToString("yyyy-MM-dd HH:mm:ss") +
                    "', `Taille` = '" + ua.Taille.ToString() +
                    "', `Credit`='" + ua.Credit.ToString() +
                    "', `Credit_Restant`='" + ua.Credit_Restant.ToString() +
                    "', `Credit_Semaine_Suivante`='" + ua.Credit_Semaine_Suivante.ToString() +
                    "', `User_Id`='" + ua.User_Id.ToString() +
                    "', `Article_Type_Id`='" + ua.Article_Type_Id.ToString() + 
                     "' WHERE `Id`=" + ua.Id.ToString() + ";";
                using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                {
                    MySqlCommand commande = con.CreateCommand();
                    commande.CommandText = query;

                    con.Open();
                    result = commande.ExecuteNonQuery();
                    con.Close();
                }
                
                if (result != 1)
                    throw (new Exception("requete non reussie (" + query + ")"));
            }
        }

        public void UpdateUserArticle(int id, int credrest)
        {
            try
            {
                //dbContext.Refresh(System.Data.Objects.RefreshMode.StoreWins, dbContext.user_article);
                DateTime dt = DateTime.Now;
                string date = String.Format("{0:yyyy-MM-dd HH:mm:ss}", dt);

                //User_Article tmp = dbContext.user_article.FirstOrDefault(i => i.Id == id);
                User_Article tmpusar = new User_Article(0, new DateTime(1971, 01, 01), new DateTime(1971, 01, 01), "00", 0, 0, 0, 0);
                tmpusar = GetBackUserArticleFromUser(id);

                tmpusar.Date_Modification = DateTime.Parse(date);

                Log.add("Update user_article local " + tmpusar.Id + ": Credits restants = " + tmpusar.Credit_Restant);
                tmpusar.Credit_Restant = credrest;
                Log.add("Update user_article local " + tmpusar.Id + ": Credits restants = " + tmpusar.Credit_Restant);

                //dbContext.SaveChanges();
                UpdateUserArticle(tmpusar);

            }
            catch (Exception e)
            {
                Log.add("Erreur UpdateUserArticle: " + e.Message);
                if (Properties.Settings.Default.UseDBGMSG)
                {
                    string msg = "Erreur lors de la mise à jour des données: " + e.Message;
                    if (e.InnerException != null) msg += "\n" + e.InnerException.Message;
                    MessageBox.Show(msg);
                }
            }
        }

     

        public void UpdateUserArticle2(int id, int credsemsuiv)
        {
            try
            {
                //dbContext.Refresh(System.Data.Objects.RefreshMode.StoreWins, dbContext.user_article);
                DateTime dt = DateTime.Now;
                string date = String.Format("{0:yyyy-MM-dd HH:mm:ss}", dt);

                //User_Article tmp = dbContext.user_article.FirstOrDefault(i => i.Id == id);
                User_Article tmpusar = new User_Article(0, new DateTime(1971, 01, 01), new DateTime(1971, 01, 01), "00", 0, 0, 0, 0);
                tmpusar = GetBackUserArticleFromUser(id);

                tmpusar.Date_Modification = DateTime.Parse(date);

                Log.add("Update user_article local " + tmpusar.Id + ": Credits semaine suivante = " + tmpusar.Credit_Semaine_Suivante);
                tmpusar.Credit_Restant = credsemsuiv;
                Log.add("Update user_article local " + tmpusar.Id + ": Credits semaine suivante = " + tmpusar.Credit_Semaine_Suivante);

                //dbContext.SaveChanges();
                UpdateUserArticle(tmpusar);

            }
            catch (Exception e)
            {
                Log.add("Erreur UpdateUserArticle: " + e.Message);
                if (Properties.Settings.Default.UseDBGMSG)
                {
                    string msg = "Erreur lors de la mise à jour des données: " + e.Message;
                    if (e.InnerException != null) msg += "\n" + e.InnerException.Message;
                    MessageBox.Show(msg);
                }
            }
        }

        private User_Article GetBackUserArticleFromUser(string taille, int userid, int articletype)
        {
            if (IsAvailable())
            {
               // try
                {
                    User_Article tmpusart = new User_Article(0, new DateTime(1971, 01, 01), new DateTime(1971, 01, 01), "00", 0, 0, 0, 0);
                    string query = "SELECT * FROM `user_article` ";
                    query += " WHERE  `Taille` = '" + taille.ToString() + "' AND `User_Id`= '" + userid.ToString() + "' AND `Article_Type_Id` = '" + articletype.ToString() + "' LIMIT 1;";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {
                        Log.add(query);
                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        if (dataReader.Read())
                        {
                            //User_Article tmp = dbContext.user_article.FirstOrDefault(i => i.Taille == taille && i.User_Id == userid && i.Article_Type_Id == articletype);
                            //CREATE TABLE IF NOT EXISTS `user_article` (
                            //  `Id` int(8) NOT NULL,
                            //  `Date_Creation` datetime NOT NULL,
                            //  `Date_Modification` datetime NOT NULL,
                            //  `Taille` varchar(255) NOT NULL,
                            //  `Credit` int(8) NOT NULL,
                            //  `Credit_Restant` int(8) NOT NULL,
                            //  `User_Id` int(8) NOT NULL,
                            //  `Article_Type_Id` int(8) NOT NULL,
                            //  PRIMARY KEY (`Id`)
                            //) ENGINE=MyISAM DEFAULT CHARSET=latin1;
                            tmpusart.Id = int.Parse(dataReader.GetString("Id"));
                            tmpusart.Date_Creation = dataReader.GetDateTime("Date_Creation");
                            tmpusart.Date_Modification = dataReader.GetDateTime("Date_Modification");
                            tmpusart.Taille = dataReader.GetString("Taille");
                            tmpusart.Credit = int.Parse(dataReader.GetString("Credit"));
                            tmpusart.Credit_Restant = int.Parse(dataReader.GetString("Credit_Restant"));
                            tmpusart.Credit_Semaine_Suivante = int.Parse(dataReader.GetString("Credit_Semaine_Suivante"));
                            tmpusart.User_Id = int.Parse(dataReader.GetString("User_Id"));
                            tmpusart.Article_Type_Id = int.Parse(dataReader.GetString("Article_Type_Id"));
                        }
                        con.Close();
                    }
                    return tmpusart;
                }
             /*   catch (Exception e)
                {
                    Log.add("Erreur GetBackUserArticleFromUser: " + e.Message);

                    return new User_Article(0, new DateTime(1971, 01, 01), new DateTime(1971, 01, 01), "00", 0, 0, 0, 0);
                }*/
            }
            else
            {
                Log.add("Erreur GetBackUserArticleFromUser: pas d'accès à la base de donnée locale");
                throw (new Exception("pas d'acces à la base locale"));
                //return new User_Article(0, new DateTime(1971, 01, 01), new DateTime(1971, 01, 01), "00", 0, 0, 0, 0);
            }
        }

        private User_Article GetBackUserArticleFromUser(int userid, int articletype)
        {
            if (IsAvailable())
            {
                // try
                {
                    User_Article tmpusart = new User_Article(0, new DateTime(1971, 01, 01), new DateTime(1971, 01, 01), "00", 0, 0, 0, 0);
                    string query = "SELECT * FROM `user_article` ";
                    query += " WHERE  `User_Id`= '" + userid.ToString() + "' AND `Article_Type_Id` = '" + articletype.ToString() + "' LIMIT 1;";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        if (dataReader.Read())
                        {
                            //User_Article tmp = dbContext.user_article.FirstOrDefault(i => i.Taille == taille && i.User_Id == userid && i.Article_Type_Id == articletype);
                            //CREATE TABLE IF NOT EXISTS `user_article` (
                            //  `Id` int(8) NOT NULL,
                            //  `Date_Creation` datetime NOT NULL,
                            //  `Date_Modification` datetime NOT NULL,
                            //  `Taille` varchar(255) NOT NULL,
                            //  `Credit` int(8) NOT NULL,
                            //  `Credit_Restant` int(8) NOT NULL,
                            //  `User_Id` int(8) NOT NULL,
                            //  `Article_Type_Id` int(8) NOT NULL,
                            //  PRIMARY KEY (`Id`)
                            //) ENGINE=MyISAM DEFAULT CHARSET=latin1;
                            tmpusart.Id = int.Parse(dataReader.GetString("Id"));
                            tmpusart.Date_Creation = dataReader.GetDateTime("Date_Creation");
                            tmpusart.Date_Modification = dataReader.GetDateTime("Date_Modification");
                            tmpusart.Taille = dataReader.GetString("Taille");
                            tmpusart.Credit = int.Parse(dataReader.GetString("Credit"));
                            tmpusart.Credit_Restant = int.Parse(dataReader.GetString("Credit_Restant"));
                            tmpusart.Credit_Semaine_Suivante = int.Parse(dataReader.GetString("Credit_Semaine_Suivante"));
                            tmpusart.User_Id = int.Parse(dataReader.GetString("User_Id"));
                            tmpusart.Article_Type_Id = int.Parse(dataReader.GetString("Article_Type_Id"));
                        }
                        con.Close();
                    }
                    return tmpusart;
                }
                /*   catch (Exception e)
                   {
                       Log.add("Erreur GetBackUserArticleFromUser: " + e.Message);

                       return new User_Article(0, new DateTime(1971, 01, 01), new DateTime(1971, 01, 01), "00", 0, 0, 0, 0);
                   }*/
            }
            else
            {
                Log.add("Erreur GetBackUserArticleFromUser: pas d'accès à la base de donnée locale");
                throw (new Exception("pas d'acces à la base locale"));
                //return new User_Article(0, new DateTime(1971, 01, 01), new DateTime(1971, 01, 01), "00", 0, 0, 0, 0);
            }
        }

        private User_Article GetBackUserArticleFromUser(int id)
        {
            if (IsAvailable())
            {
                // try
                {
                    User_Article tmpusart = new User_Article(0, new DateTime(1971, 01, 01), new DateTime(1971, 01, 01), "00", 0, 0, 0, 0);
                    string query = "SELECT * FROM `user_article` ";
                    query += " WHERE  `Id`= '" + id.ToString() + "' LIMIT 1;";
                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        MySqlDataReader dataReader = commande.ExecuteReader();
                        if (dataReader.Read())
                        {
                            //User_Article tmp = dbContext.user_article.FirstOrDefault(i => i.Taille == taille && i.User_Id == userid && i.Article_Type_Id == articletype);
                            //CREATE TABLE IF NOT EXISTS `user_article` (
                            //  `Id` int(8) NOT NULL,
                            //  `Date_Creation` datetime NOT NULL,
                            //  `Date_Modification` datetime NOT NULL,
                            //  `Taille` varchar(255) NOT NULL,
                            //  `Credit` int(8) NOT NULL,
                            //  `Credit_Restant` int(8) NOT NULL,
                            //  `User_Id` int(8) NOT NULL,
                            //  `Article_Type_Id` int(8) NOT NULL,
                            //  PRIMARY KEY (`Id`)
                            //) ENGINE=MyISAM DEFAULT CHARSET=latin1;
                            tmpusart.Id = int.Parse(dataReader.GetString("Id"));
                            tmpusart.Date_Creation = dataReader.GetDateTime("Date_Creation");
                            tmpusart.Date_Modification = dataReader.GetDateTime("Date_Modification");
                            tmpusart.Taille = dataReader.GetString("Taille");
                            tmpusart.Credit = int.Parse(dataReader.GetString("Credit"));
                            tmpusart.Credit_Restant = int.Parse(dataReader.GetString("Credit_Restant"));
                            tmpusart.Credit_Semaine_Suivante = int.Parse(dataReader.GetString("Credit_Semaine_Suivante"));
                            tmpusart.User_Id = int.Parse(dataReader.GetString("User_Id"));
                            tmpusart.Article_Type_Id = int.Parse(dataReader.GetString("Article_Type_Id"));
                        }
                        con.Close();
                    }
                    return tmpusart;
                }
                /*   catch (Exception e)
                   {
                       Log.add("Erreur GetBackUserArticleFromUser: " + e.Message);

                       return new User_Article(0, new DateTime(1971, 01, 01), new DateTime(1971, 01, 01), "00", 0, 0, 0, 0);
                   }*/
            }
            else
            {
                Log.add("Erreur GetBackUserArticleFromUser: pas d'accès à la base de donnée locale");
                throw (new Exception("pas d'acces à la base locale"));
                //return new User_Article(0, new DateTime(1971, 01, 01), new DateTime(1971, 01, 01), "00", 0, 0, 0, 0);
            }
        }

        public static DateTime ProchaineDateRecreditation(DateTime date_emprunt, DayOfWeek recreditdayofweek, int heurecredit)
        {
            int diffjours = recreditdayofweek - date_emprunt.DayOfWeek;
            int diffheures = heurecredit - date_emprunt.Hour;
            if (diffjours < 0) diffjours = diffjours + 7; 
            if (diffheures < 0) { diffjours--; diffheures = diffheures + 24; }
            return date_emprunt + new TimeSpan(diffjours, diffheures, -date_emprunt.Minute, -date_emprunt.Second);
        }

        public void UpdateUserArticleRestitution(Epc epc)
        {
            int result = 0;
            try
            {
                if (IsAvailable())
                {
                    int lastUser = 0;
                    int.TryParse(epc.Last_User, out lastUser);
                    Log.add("Restitution: user " + lastUser);
                    Log.add("epc.State " + epc.State.ToString());
                    //  dbContext.Refresh(System.Data.Objects.RefreshMode.StoreWins, dbContext.user_article);
                    User_Article userarticle = new User_Article();
                    if (epc.State == EtatArticle.SORTI)
                    {
                        userarticle = GetBackUserArticleFromUser(epc.Taille, lastUser, epc.Article_Type_ID);
                        //temp = dbContext.user_article.FirstOrDefault(x => x.Taille == epc.Taille && x.User_Id == lastUser && x.Article_Type_Id == epc.Article_Type_ID);
                    }
                    else if (epc.State == EtatArticle.SORTI_TailleSup)
                    {
                        // on ne tient plus compte de la taille
                        //string tailleInf = getTailleInfByOriginalSize(epc.Taille);
                        userarticle = GetBackUserArticleFromUser(lastUser, epc.Article_Type_ID);
                        //temp = dbContext.user_article.FirstOrDefault(x => /*x.Taille == tailleInf &&*/ x.User_Id == lastUser && x.Article_Type_Id == epc.Article_Type_ID);
                    }
                    if (userarticle.Id == 0)
                    {
                        Log.add("La ligne User_Article pour cet utilisateur a été supprimée, mis à jour du last_user!");
                    }

                    if ((userarticle != null) && (userarticle.Id != 0))
                    { // N.B.: Si (tmpusart.Id == 0) c'est que l'utilisateur n'a pas droit au type d'article donc pas de crédit 
                        // userarticle ne doit pas être null!!!!!
                        string query = ""; int ucss = -1; int ucr = -1;
                        Log.add("Date d'emprunt: " + epc.Date_Modification);
                        DateTime daterecreditation = ProchaineDateRecreditation(epc.Date_Modification, (DayOfWeek)Properties.Settings.Default.NumJourRecreditHebdo, Properties.Settings.Default.HeureRecreditHebdo);
                        Log.add("Prochaine date de recréditation: " + daterecreditation);
                        User user = getUserById(int.Parse(epc.Last_User)).FirstOrDefault();
                        MessageBox.Show(DateTime.Now + " < " + daterecreditation + " : " + (DateTime.Now.CompareTo(daterecreditation) < 0).ToString());
                        if (Properties.Settings.Default.RecreditHebdo == true && DateTime.Now.CompareTo(daterecreditation) <= 0)
                        {
                            Log.add("Recréditation la semaine suivante pour : " + user.Prenom + "  " + user.Nom);
                            //    MessageBox.Show("Credit_Semaine_Suivante = " + userarticle.Credit_Semaine_Suivante);
                            ucss = userarticle.Credit_Semaine_Suivante + 1;
                            //   MessageBox.Show("Credit_Semaine_Suivante = " + ucss);
                            ucr = userarticle.Credit_Restant;
                            query = "UPDATE `user_article` SET `Credit_Semaine_Suivante`='" + ucss.ToString() + "', `Date_Modification` = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                            + "' WHERE `Id`='" + userarticle.Id.ToString() + "';";
                            Log.add("Restitution: mise à jour de l'user_article " + userarticle.Article_Type_Id + " Crédit semaine suivante = " + ucss.ToString());
                            Log.add("QUERY : " + query);
                        }
                        if (Properties.Settings.Default.RecreditHebdo == true && DateTime.Now.CompareTo(daterecreditation) > 0)
                        {
                            Log.add("Recréditation immédiate pour : " + user.Prenom + "  " + user.Nom);
                            ucr = userarticle.Credit_Restant + 1;
                            query = "UPDATE `user_article` SET `Credit_Restant`='" + ucr.ToString() + "', `Date_Modification` = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                            + "' WHERE `Id`='" + userarticle.Id.ToString() + "';";
                            Log.add("Restitution: mise à jour de l'user_article " + userarticle.Article_Type_Id + ": Crédit restant = " + ucr.ToString());
                            Log.add("QUERY : " + query);
                        }

                        using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                        {
                            MySqlCommand commande = con.CreateCommand();
                            commande.CommandText = query;

                            con.Open();
                            result = commande.ExecuteNonQuery();
                            con.Close();

                        }

                        if (result != 1) throw (new Exception("requete non reussie (" + query + ")"));

                    }
                }
            }
            catch (Exception e)
            {
                Log.add("Erreur UpdateUserArticleRestitution: " + e.Message);
                if (Properties.Settings.Default.UseDBGMSG)
                {
                    string msg = "Erreur lors de la mise à jour des données: " + e.Message;
                    if (e.InnerException != null) msg += "\n" + e.InnerException.Message;
                    MessageBox.Show(msg);
                }
            }
        }

        public void UpdateUserArticleRestitution(string taille, int userid, int articletype, bool restituerCredit)
        {
            int result = 0; 
            try
            {
                if (IsAvailable())
                {
                    //dbContext.Refresh(System.Data.Objects.RefreshMode.StoreWins, dbContext.user_article);
                    DateTime dt = DateTime.Now;
                    string date = String.Format("{0:yyyy-MM-dd HH:mm:ss}", dt);

                    //User_Article tmp = dbContext.user_article.FirstOrDefault(i => i.Taille == taille && i.User_Id == userid && i.Article_Type_Id == articletype);
                    //CREATE TABLE IF NOT EXISTS `user_article` (
                    //  `Id` int(8) NOT NULL,
                    //  `Date_Creation` datetime NOT NULL,
                    //  `Date_Modification` datetime NOT NULL,
                    //  `Taille` varchar(255) NOT NULL,
                    //  `Credit` int(8) NOT NULL,
                    //  `Credit_Restant` int(8) NOT NULL,
                    //  `User_Id` int(8) NOT NULL,
                    //  `Article_Type_Id` int(8) NOT NULL,
                    //  PRIMARY KEY (`Id`)
                    //) ENGINE=MyISAM DEFAULT CHARSET=latin1;

                    
                    
                    // Faire un select pour connaitre le crédit restant actuel
                    User_Article userarticle = GetBackUserArticleFromUser(taille, userid, articletype);
                    
                    userarticle.Date_Modification = DateTime.Parse(date);
                    
                    int credrest = userarticle.Credit_Restant;
                    int credsemsuiv = userarticle.Credit_Semaine_Suivante;
                    if (restituerCredit)
                    {
                        userarticle.Credit_Restant = userarticle.Credit_Restant >= userarticle.Credit ? userarticle.Credit : userarticle.Credit_Restant + 1;
                    }
                    string query = "";
                    DateTime daterecreditation = ProchaineDateRecreditation(userarticle.Date_Modification, (DayOfWeek)Properties.Settings.Default.NumJourRecreditHebdo, Properties.Settings.Default.HeureRecreditHebdo);
                    if (Properties.Settings.Default.RecreditHebdo == true && DateTime.Now >= daterecreditation)           
                    {
                        userarticle.Credit_Semaine_Suivante++;
                        query = "UPDATE `user_article` SET `Credit_Semaine_Suivante`='" + userarticle.Credit_Semaine_Suivante.ToString() + "', `Date_Modification` = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                        + "' WHERE `Id`='" + userarticle.Id.ToString() + "';";
                    }
                    else
                    {
                        userarticle.Credit_Restant++;
                        query = "UPDATE `user_article` SET `Credit_Restant`='" + userarticle.Credit_Restant.ToString() + "', `Date_Modification` = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                        + "' WHERE `Id`='" + userarticle.Id.ToString() + "';";
                    }

                    using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                    {

                        MySqlCommand commande = con.CreateCommand();
                        commande.CommandText = query;

                        con.Open();
                        result = commande.ExecuteNonQuery();
                        con.Close();
                    }
                    if (result != 1)
                        throw (new Exception("requete non reussie (" + query + ")"));
                    Log.add("Update restitution user_article " + userarticle.Id + ": Credits restants = " + credrest.ToString() + " => Credits restants = " + userarticle.Credit_Restant.ToString() + ", Crédit semaine suivante = " + credsemsuiv.ToString() + " => Crédit semaine suivante = " + userarticle.Credit_Semaine_Suivante.ToString());

                }
            }
            catch (Exception e)
            {
                Log.add("Erreur UpdateUserArticleRestitution: " + e.Message);
                if (Properties.Settings.Default.UseDBGMSG)
                {
                    string msg = "Erreur lors de la mise à jour des données: " + e.Message;
                    if (e.InnerException != null) msg += "\n" + e.InnerException.Message;
                    MessageBox.Show(msg);
                }
            }
        }



        public void UpdateLogEpc(int id, sbyte synchronised)
        {
            int result = 0; 
            try
            {
                if (IsAvailable())
                {
                        //Log_Epc tmp = dbContext.log_epc.FirstOrDefault(i => i.Id == id);
                        ////tmp.Id = id;
                        //tmp.Synchronised = synchronised;
                        //dbContext.SaveChanges();
                        Log_Epc tmploge = new Log_Epc();
                        tmploge.Id = id;
                        tmploge.Synchronised = synchronised;
                        string query = "UPDATE `log_epc` SET `Synchronised`='" + tmploge.Synchronised.ToString() + "' WHERE `Id`='" + tmploge.Id.ToString() + "';";
                        using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                        {

                            MySqlCommand commande = con.CreateCommand();
                            commande.CommandText = query;

                            con.Open();
                            result = commande.ExecuteNonQuery();
                            con.Close();
                        }
                        if (result != 1)
                            throw (new Exception("requete non reussie (" + query + ")"));
                } 
            }
            catch (Exception e)
            {
                Log.add("Erreur UpdateLogEpc: " + e.Message);
                if (Properties.Settings.Default.UseDBGMSG)
                {
                    string msg = "Erreur lors de la mise à jour des données: " + e.Message;
                    if (e.InnerException != null) msg += "\n" + e.InnerException.Message;
                    MessageBox.Show(msg);
                }
            }
        }

        public void updateConnexionUtilisateur(int user)
        {
            int result = 0;
            User co = new User();
            DateTime dt = DateTime.Now;
            string date = String.Format("{0:yyyy-MM-dd HH:mm:ss}", dt);

            try
            {

                if (IsAvailable())
                {
                    try
                    {
                        //co = dbContext.user.First(i => i.Id == user);
                        //co.Last_Connection = dt;
                        //dbContext.SaveChanges();
                        co.Id = user;
                        co.Last_Connection = dt;
                        //string query = "UPDATE `user` SET `Last_Connection`=" + co.Last_Connection.ToString() + " WHERE `Id`=" + co.Id.ToString() + ";"; Bad Date format
                        string query = "UPDATE `user` SET `Last_Connection`='" + date + "' WHERE `Id`='" + co.Id.ToString() + "';";
                        using (var con = new MySqlConnection(Properties.Settings.Default.StringConnectLocale))
                        {

                            MySqlCommand commande = con.CreateCommand();
                            commande.CommandText = query;

                            con.Open();
                            result = commande.ExecuteNonQuery();
                            con.Close();
                        }
                        if (result != 1)
                            throw (new Exception("requete non reussie (" + query + ")"));
                    }
                    catch (Exception e)
                    {
                        Log.add("Erreur mise a jour user: " + e.Message);
                        if (Properties.Settings.Default.UseDBGMSG)
                        {
                            MessageBox.Show("Erreur mise a jour user: " + e.Message);
                        }
                    }
                }
                
            }
            catch(Exception e)
            {
                Log.add("Erreur mise a jour user: " + e.Message);
                if (Properties.Settings.Default.UseDBGMSG)
                    System.Windows.MessageBox.Show("Erreur mise a jour user");
            }

        }

        #endregion

        #region Restitution

        public void RestitutionScan(List<Tag> list, int ReaderId)
        {
            string logMessage = "Scan restitution: " + list.Count + " articles scannés (";
            foreach (Tag t in list)
            {
#if NEWIMPINJ
                logMessage += t.Epc.ToHexString() + " ";
#else
                logMessage += t.Epc + " ";
#endif
            }
            Log.add(logMessage + ")");


            editRestitution(list, ReaderId);
            //   return compileScanInData();
            //dbContext.Refresh(System.Data.Objects.RefreshMode.StoreWins, dbContext.user_article);
        }

        private void editRestitution(List<Tag> list, int ReaderId)
        {
            // liste des EPC lus par la restitution
            //dbContext.Refresh(System.Data.Objects.RefreshMode.StoreWins, dbContext.epc);
            List<Epc> epcRest = new List<Epc>();
            foreach (Tag t in list)
            {
#if NEWIMPINJ
                Epc e = getEpc(t.Epc.ToHexString());
#else
                Epc e = getEpc(t.Epc);
#endif
                if (e != null && !epcRest.Contains(e))
                {
                    epcRest.Add(e);
                }
            }

            foreach (Epc e in epcRest)
            {
                Log.add("Update epc :" + e.Tag + " etat : " + e.State);

                if (e.State != 2 && e.State != 10 && e.State != 100)
                {
                    if (getUserById(int.Parse(e.Last_User)).FirstOrDefault().Type != "reloader")
                    {
                        Log.add("Recredite user : " + e.Last_User + " pour  epc :" + e.Tag + " etat : " + e.State);
                        UpdateUserArticleRestitution(e);
                    }                 
                }
            }
            foreach (Epc epc in epcRest)
            {
                Log.add("Changement etat epc :" + epc.Tag);
                updateEpcRestitution(epc, ReaderId);
            }
        }

        #endregion

        

    }
}
