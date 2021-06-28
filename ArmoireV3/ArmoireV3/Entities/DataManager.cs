using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Impinj.OctaneSdk;
using System.Data.Linq;
using System.Windows;
using System.Reflection;
using MySql.Data.MySqlClient;

#if EDMX
namespace ArmoireV3.Entities
{
    public partial class DataManager : Object, AutoConnect.IConnectedObject, INotifyPropertyChanged
    {
        public ArmoireV3Entities dbContext = new ArmoireV3Entities();

        public DataManager()
        {
            dbContext = new ArmoireV3Entities();
        }

        public DataManager(string connectionString)
        {
            dbContext = new ArmoireV3Entities(connectionString);
        }

        public void RefreshContext()
        {
            dbContext = new ArmoireV3Entities();
        }

        internal bool IsAvailable()
        {
            if (Is_connected() && dbContext.Connection.State == System.Data.ConnectionState.Closed)
            {
                return true;
            }
            else
            {
                Log.add("Erreur: Databases not available");
                return false;
            }

        }

        #region AUTOCONNECT

        public bool Is_connected()
        {
            return dbContext != null;
        }
        public void Connect()
        {
            try
            {
                if (dbContext == null)
                {
                    dbContext = new ArmoireV3Entities();
                }
            }
            catch (Exception)
            {
                if (Properties.Settings.Default.UseDBGMSG)
                {
                    MessageBox.Show("Echec de connexion à la Base de Données.");
                }
            }
        }
        public void CloseConnection()
        {
            if (dbContext != null)
                dbContext.Dispose();
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

        public IEnumerable<user> infoUser(int id)
        {
            if (IsAvailable())
            {
                try
                {
                    IQueryable<user> query = from f in dbContext.user where f.Id == id select f;
                    if (query.Count() > 0)
                        return query.ToList();
                    else
                        return new List<user>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur infoUser: " + e.Message);
                    return new List<user>();
                }
            }
            else
                return new List<user>();


        }

        //désactivé
        /*
        public IEnumerable<string> armname(int id)
        {
            if (IsAvailable())
            {
                try
                {
                    IQueryable<string> query = from f in dbContext.armoire where f.Id == id select f.Name;
                    if (query.Count() > 0)
                        return query.ToList();
                    else
                        return new List<string>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur armname: " + e.Message);
                    return new List<string>();
                }
            }
            else
                return new List<string>();

        }
        */

        //requete remonté des log epc l'armoire
        public IEnumerable<log_epc> synchroLogEPC()
        {
            if (IsAvailable())
            {
                try
                {
                    IQueryable<log_epc> query = from f in dbContext.log_epc where f.Synchronised == 0 select f;
                    if (query.Count() > 0)
                        return query.ToList();
                    else
                        return new List<log_epc>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur synchroLogEPC: " + e.Message);
                    return new List<log_epc>();
                }
            }
            else
                return new List<log_epc>();


        }


        //requete remonté du contenue de l'armoire
        public IEnumerable<content_arm> synchroContent()
        {
            if (IsAvailable())
            {
                try
                {
                    IQueryable<content_arm> query = from f in dbContext.content_arm
                                                    where f.RFID_Reader == Properties.Settings.Default.NumArmoire
                                                    orderby f.Id
                                                    select f;
                    if (query.Count() > 0)
                        return query.ToList();
                    else
                        return new List<content_arm>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur synchroContent: " + e.Message);
                    return new List<content_arm>();
                }
            }
            else
                return new List<content_arm>();


        }

        //requete remonté des alertes
        public IEnumerable<alert> synchroAlert()
        {
            if (IsAvailable())
            {
                try
                {
                    IQueryable<alert> query = from f in dbContext.alert where f.Traiter == 0 select f;
                    if (query.Count() > 0)
                        return query.ToList();
                    else
                        return new List<alert>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur synchroAlert: " + e.Message);

                    return new List<alert>();
                }
            }
            else
                return new List<alert>();


        }

        //requete remonté des tag alert
        public IEnumerable<tag_alert> synchroTagAlert()
        {
            if (IsAvailable())
            {
                try
                {
                    IQueryable<tag_alert> query = from f in dbContext.tag_alert select f;
                    if (query.Count() > 0)
                        return query.ToList();
                    else
                        return new List<tag_alert>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur synchroTagAlert: " + e.Message);
                    return new List<tag_alert>();
                }
            }
            else
                return new List<tag_alert>();


        }
        //select, pour la verif de l'existence de ces lignes dans l'armoire
        public IEnumerable<int> synchroAlertType()
        {
            if (IsAvailable())
            {
                try
                {
                    IQueryable<int> query = from f in dbContext.alert_type select f.Id;
                    if (query.Count() > 0)
                        return query.ToList();
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

        //insere dans l'armoire les alert_type sur le serveur
        public void synchroInsertAlertType(int id, string type, string code, string desc, string niv, string contact)
        {
            alert_type typeAl = new alert_type();

            typeAl.Id = id;
            typeAl.Type = type;
            typeAl.Code = code;
            typeAl.Description = desc;
            typeAl.Niveau = niv;
            typeAl.Contact = contact;
            try
            {
                dbContext.AddToalert_type(typeAl);
                dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                Log.add("Erreur synchroInsertAlertType: " + e.Message);
                
                if (Properties.Settings.Default.UseDBGMSG)
                {
                    MessageBox.Show("Impossible de mettre à jour les alertes: " + e.Message);
                }
            }

        }

        //désactivé
        /*
        //select, pour la verif de l'existence de ces lignes dans l'armoire
        public IEnumerable<int> synchroArmoire()
        {
            if (IsAvailable())
            {
                try
                {
                    IQueryable<int> query = from f in dbContext.armoire select f.Id;
                    if (query.Count() > 0)
                        return query.ToList();
                    else
                        return new List<int>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur synchroArmoire: " + e.Message);
                    return new List<int>();
                }
            }
            else
                return new List<int>();


        }
        */

        //désactivé
        /*
        //insere dans l'armoire les infos des armoires sur le serveur
        public void synchroInsertArmoire(int id, DateTime cre, DateTime mod, int nbcase, string name)
        {
            armoire typeAl = new armoire();

            typeAl.Id = id;
            typeAl.Date_Creation = cre;
            typeAl.Date_Modification = mod;
            typeAl.Nb_Case = nbcase;
            typeAl.Name = name;

            try
            {
                dbContext.AddToarmoire(typeAl);
                dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                Log.add("Erreur synchroInsertArmoire: " + e.Message);
                if (Properties.Settings.Default.UseDBGMSG)
                {
                    MessageBox.Show("Erreur lors de la synchronisation de l'armoire: " + e.Message);
                }
            }

        }
        */

        //désactivé
        /*
        //Mets a jour les modifications de l'armoire venant du dédié
        public void synchroUpdateArmoire(int id, int nbc, string name)
        {
            DateTime dt = DateTime.Now;
            string date = String.Format("{0:yyyy-MM-dd HH:mm:ss}", dt);
            armoire arm = dbContext.armoire.FirstOrDefault(i => i.Id == id);
            arm.Nb_Case = nbc;
            arm.Name = name;
            arm.Date_Modification = DateTime.Parse(date);

            try
            {
                dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                Log.add("Erreur synchroUpdateArmoire: " + e.Message);
                if (Properties.Settings.Default.UseDBGMSG)
                {
                    MessageBox.Show("Erreur lors de la synchronisation de l'armoire: " + e.Message);
                }
            }
        }
        */


        //select, pour la verif de l'existence de ces lignes dans l'armoire
        public IEnumerable<int> synchroCase()
        {
            if (IsAvailable())
            {
                try
                {
                    IQueryable<int> query = from f in dbContext.@case select f.Id;
                    if (query.Count() > 0)
                        return query.ToList();
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


        //insere dans l'armoire les infos des armoires sur le serveur
        public void synchroInsertCase(int id, int bind_id, string taille, DateTime cre, int maxitem, int articletypeid, int armid)
        {
            @case typeAl = new @case();

            typeAl.Id = id;
            typeAl.Bind_ID = bind_id;
            typeAl.Taille = taille;
            typeAl.Date_Creation = cre;
            typeAl.Max_Item = maxitem;
            typeAl.Article_Type_Id = articletypeid;
            typeAl.Armoire_ID = armid;

            try
            {
                dbContext.AddTocase(typeAl);
                dbContext.SaveChanges();
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


        //Mets a jour les modifications de l'armoire venant du dédié
        public void synchroUpdateCase(int id, int bind_id, string taille, int max, int artic, int arm)
        {

            @case ca = dbContext.@case.FirstOrDefault(i => i.Id == id);
            ca.Bind_ID = bind_id;
            ca.Taille = taille;
            ca.Max_Item = max;
            ca.Article_Type_Id = artic;
            ca.Armoire_ID = arm;

            try
            {
                dbContext.SaveChanges();
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

        public IEnumerable<corresp_taille> synchroCorrespTaille()
        {
            if (IsAvailable())
            {
                try
                {
                    IQueryable<corresp_taille> query = from f in dbContext.corresp_taille select f;
                    if (query.Count() > 0)
                        return query.ToList();
                    else
                        return new List<corresp_taille>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur synchroCorrespTaille: " + e.Message);
                    return new List<corresp_taille>();
                }
            }
            else
                return new List<corresp_taille>();
        }

        public void synchroInsertCorrespTaille(string type_Taille, string taille, int classement_taille)
        {
            corresp_taille typeAl = new corresp_taille();

            typeAl.Type_Taille = type_Taille;
            typeAl.Taille = taille;
            typeAl.Classement_tailles = classement_taille;

            try
            {
                dbContext.AddTocorresp_taille(typeAl);
                dbContext.SaveChanges();
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

        public void synchroUpdateCorrespTaille(string type_Taille, string taille, int classement_taille)
        {

            corresp_taille ca = dbContext.corresp_taille.FirstOrDefault(i => i.Type_Taille == type_Taille && i.Taille == taille);

            if (ca.Classement_tailles != classement_taille)
            {
                ca.Classement_tailles = classement_taille;

                try
                {
                    dbContext.SaveChanges();
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

        //select, pour la verif de l'existence de ces lignes dans l'armoire
        public IEnumerable<int> synchroArticletype()
        {
            if (IsAvailable())
            {
                try
                {
                    IQueryable<int> query = from f in dbContext.article_type select f.Id;
                    if (query.Count() > 0)
                        return query.ToList();
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


        //insere dans l'armoire les article type depuis base d'en haut V2
        public void synchroInsertArticletype(int id, DateTime cre, DateTime mod, string code, string desc, string color, string sexe, string photo, int act)
        {
            article_type typeAl = new article_type();

            typeAl.Id = id;
            typeAl.Date_Creation = cre;
            typeAl.Date_Modification = mod;
            typeAl.Code = code;
            typeAl.Description = desc;
            typeAl.Couleur = color;
            typeAl.Sexe = sexe;
            typeAl.Photo = photo;
            typeAl.Active = act;
            typeAl.Description_longue = "";
            typeAl.Type_Taille = "1";

            try
            {
                dbContext.AddToarticle_type(typeAl);
                dbContext.SaveChanges();
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

        //insere dans l'armoire les article type depuis base d'en haut V3
        public void synchroInsertArticletype(int id, DateTime cre, DateTime mod, string code, string desc, string color, string sexe, string photo, int act,string typetaille, string desclong)
        {
            article_type typeAl = new article_type();

            typeAl.Id = id;
            typeAl.Date_Creation = cre;
            typeAl.Date_Modification = mod;
            typeAl.Code = code;
            typeAl.Description = desc;
            typeAl.Couleur = color;
            typeAl.Sexe = sexe;
            typeAl.Photo = photo;
            typeAl.Active = act;
            typeAl.Description_longue = desclong;
            typeAl.Type_Taille = typetaille;

            try
            {
                dbContext.AddToarticle_type(typeAl);
                dbContext.SaveChanges();
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

        //Mets a jour les modifications de l'armoire venant du dédié V2
        public void synchroUpdateArticletype(int id, string code, string desc, string color, string sexe, string photo, int act)
        {

            article_type art = dbContext.article_type.FirstOrDefault(i => i.Id == id);
            art.Code = code;
            art.Description = desc;
            art.Couleur = color;
            art.Sexe = sexe;
            art.Photo = photo;
            art.Active = act;

            try
            {
                dbContext.SaveChanges();
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

        //Mets a jour les modifications de l'armoire venant du dédié V3
        public void synchroUpdateArticletype(int id, string code, string desc, string color, string sexe, string photo, int act,string typetaille, string desclong)
        {

            article_type art = dbContext.article_type.FirstOrDefault(i => i.Id == id);
            art.Code = code;
            art.Description = desc;
            art.Couleur = color;
            art.Sexe = sexe;
            art.Photo = photo;
            art.Active = act;
            art.Description_longue = desclong;
            art.Type_Taille = typetaille;

            try
            {
                dbContext.SaveChanges();
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


        public IEnumerable<int> synchroUser()
        {
            if (IsAvailable())
            {
                try
                {
                    IQueryable<int> query = from f in dbContext.user select f.Id;
                    if (query.Count() > 0)
                        return query.ToList();
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

        // v2 avec addon v3
        public void synchroInsertUser(int id, DateTime cre, DateTime mod, string log, string pass, string type, string nom, string pren, string sexe, string taille, int group, int dep, string photo, DateTime connec, int act)
        {
            user typeAl = new user();

            typeAl.Id = id;
            typeAl.Date_Creation = cre;
            typeAl.Date_Modification = mod;
            typeAl.Login = log;
            typeAl.Password = pass;
            typeAl.Type = type;
            typeAl.Nom = nom;
            typeAl.Prenom = pren;
            typeAl.Sexe = sexe;
            typeAl.Taille = taille;
            typeAl.Groupe = group;
            typeAl.Department = dep;
            typeAl.Photo = photo;
            typeAl.Date_Modification = connec;
            typeAl.Active = act;
            typeAl.End_of_Validity = new DateTime(1971, 01, 01);
            typeAl.Wearer_Code = "";

            try
            {
                dbContext.user.AddObject(typeAl);
                dbContext.SaveChanges();
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

        //v3
        public void synchroInsertUser(int id, DateTime cre, DateTime mod, string log, string pass, string type, string nom, string pren, string sexe, string taille, int group, int dep, string photo, DateTime connec, int act, DateTime eov, string wearercode)
        {
            user typeAl = new user();

            typeAl.Id = id;
            typeAl.Date_Creation = cre;
            typeAl.Date_Modification = mod;
            typeAl.Login = log;
            typeAl.Password = pass;
            typeAl.Type = type;
            typeAl.Nom = nom;
            typeAl.Prenom = pren;
            typeAl.Sexe = sexe;
            typeAl.Taille = taille;
            typeAl.Groupe = group;
            typeAl.Department = dep;
            typeAl.Photo = photo;
            typeAl.Date_Modification = connec;
            typeAl.Active = act;
            typeAl.End_of_Validity = eov;
            typeAl.Wearer_Code = wearercode;

            try
            {
                dbContext.user.AddObject(typeAl);
                dbContext.SaveChanges();
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

        //Mets a jour les modifications de l'armoire venant du dédié
        public void synchroUpdateUser(int id, string log, string pass, string type, string nom, string pren, string sexe, string taille, int group, int dep, string photo, int act)
        {

            user art = dbContext.user.FirstOrDefault(i => i.Id == id);
            art.Login = log;
            art.Password = pass;
            art.Type = type;
            art.Nom = nom;
            art.Prenom = pren;
            art.Sexe = sexe;
            art.Taille = taille;
            art.Groupe = group;
            art.Department = dep;
            art.Photo = photo;
            art.Active = act;

            try
            {
                dbContext.SaveChanges();
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

        // v3
        public void synchroUpdateUser(int id, string log, string pass, string type, string nom, string pren, string sexe, string taille, int group, int dep, string photo, int act, DateTime eov, string wearercode)
        {

            user art = dbContext.user.FirstOrDefault(i => i.Id == id);
            art.Login = log;
            art.Password = pass;
            art.Type = type;
            art.Nom = nom;
            art.Prenom = pren;
            art.Sexe = sexe;
            art.Taille = taille;
            art.Groupe = group;
            art.Department = dep;
            art.Photo = photo;
            art.Active = act;
            art.End_of_Validity = eov;
            art.Wearer_Code = wearercode;

            try
            {
                dbContext.SaveChanges();
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

        public IEnumerable<int> synchroUserArt()
        {
            if (IsAvailable())
            {
                try
                {
                    IQueryable<int> query = from f in dbContext.user_article select f.Id;
                    if (query.Count() > 0)
                        return query.ToList();
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



        public IEnumerable<user_article> synchroUserArtDate(int id)
        {
            if (IsAvailable())
            {
                try
                {
                    IQueryable<user_article> query = from f in dbContext.user_article where f.Id == id select f;
                    if (query.Count() > 0)
                        return query.ToList();
                    else
                        return new List<user_article>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur synchroUserArtDate: " + e.Message);
                    return new List<user_article>();
                }
            }
            else
                return new List<user_article>();


        }


        public void synchroInsertUserArt(int id, DateTime cre, DateTime mod, string taille, int cred, int credrest, int userid, int articletypeid)
        {
            user_article typeAl = new user_article();

            typeAl.Id = id;
            typeAl.Date_Creation = cre;
            typeAl.Date_Modification = mod;
            typeAl.Taille = taille;
            typeAl.Credit = cred;
            typeAl.Credit_Restant = credrest;
            typeAl.User_Id = userid;
            typeAl.Article_Type_Id = articletypeid;

            try
            {
                dbContext.AddTouser_article(typeAl);
                dbContext.SaveChanges();
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

        //Mets a jour les modifications de l'armoire venant du dédié
        public void synchroUpdateUserART(int id, DateTime mod, string taille, int cred, int credres)
        {

            user_article art = dbContext.user_article.FirstOrDefault(i => i.Id == id);

            Log.add("Update synchro user_article local " + art.Id + ": Credits restants = " + art.Credit_Restant.ToString() + " => Credits restants = " + credres.ToString());

            art.Date_Modification = mod;
            art.Taille = taille;
            art.Credit = cred;
            art.Credit_Restant = credres;

            Log.add("Update synchro user_article local " + art.Id + ": Credits restants = " + art.Credit_Restant);

            try
            {
                dbContext.SaveChanges();
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

        public IEnumerable<epc> synchroEPCDate(int id)
        {
            if (IsAvailable())
            {
                try
                {
                    IQueryable<epc> query = from f in dbContext.epc where f.Id == id select f;
                    if (query.Count() > 0)
                        return query.ToList();
                    else
                        return new List<epc>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur synchroEPCDate: " + e.Message);
                    return new List<epc>();
                }
            }
            else
                return new List<epc>();


        }
        public IEnumerable<int> synchroEPC()
        {
            if (IsAvailable())
            {
                try
                {
                    IQueryable<int> query = from f in dbContext.epc select f.Id;
                    if (query.Count() > 0)
                        return query.ToList();
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
        public void synchroInsertEPC(int id, DateTime cre, DateTime mod, string tag, string cb, string taille, string type_taille, int lavage, int state, string lastuser, string lastreader, string lastaction, DateTime lastactdate, int movement, int articletypeid, int armoireid)
        {

            epc typeAl = new epc();

            typeAl.Id = id;
            typeAl.Date_Creation = cre;
            typeAl.Date_Modification = mod;
            typeAl.Tag = tag;
            typeAl.Code_Barre = cb;
            typeAl.Taille = taille;
            typeAl.Type_Taille = type_taille;
            typeAl.Cycle_Lavage_Count = lavage;
            typeAl.State = state;
            typeAl.Last_User = lastuser;
            typeAl.Last_Reader = lastreader;
            typeAl.Last_Action = lastaction;
            typeAl.Last_Action_Date = lastactdate;
            typeAl.Movement = movement;
            typeAl.Article_Type_ID = articletypeid;
            typeAl.Armoire_ID = armoireid;
            typeAl.Actif = 1;

            try
            {

                dbContext.AddToepc(typeAl);
                dbContext.SaveChanges();
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

        //v3
        public void synchroInsertEPC(int id, DateTime cre, DateTime mod, string tag, string cb, string taille, string type_taille, int lavage, int state, string lastuser, string lastreader, string lastaction, DateTime lastactdate, int movement, int articletypeid, int armoireid, int actif)
        {

            epc typeAl = new epc();

            typeAl.Id = id;
            typeAl.Date_Creation = cre;
            typeAl.Date_Modification = mod;
            typeAl.Tag = tag;
            typeAl.Code_Barre = cb;
            typeAl.Taille = taille;
            typeAl.Type_Taille = type_taille;
            typeAl.Cycle_Lavage_Count = lavage;
            typeAl.State = state;
            typeAl.Last_User = lastuser;
            typeAl.Last_Reader = lastreader;
            typeAl.Last_Action = lastaction;
            typeAl.Last_Action_Date = lastactdate;
            typeAl.Movement = movement;
            typeAl.Article_Type_ID = articletypeid;
            typeAl.Armoire_ID = armoireid;
            typeAl.Actif = actif;

            try
            {

                dbContext.AddToepc(typeAl);
                dbContext.SaveChanges();
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


        // v2 (inutilisé)
        public void synchroUpdateEPC(int id, DateTime mod, string taille, int lavage, int state, string lastuser, string lastreader, string lastaction, DateTime lastactdate, int movement, int articletypeid, int armoireid)
        {
            epc art = dbContext.epc.FirstOrDefault(i => i.Id == id);
            art.Date_Modification = mod;
            art.Taille = taille;
            art.Cycle_Lavage_Count = lavage;
            art.State = state;
            art.Last_User = lastuser;
            art.Last_Reader = lastreader;
            
            int armoireIdint;
            bool convReussi = false;
            convReussi = int.TryParse(lastreader, out armoireIdint);
            if (!convReussi)
            {
                Log.add("Erreur synchroUpdateEPC: Erreur de conversion Last_Reader");
            }
            art.Armoire_ID = armoireIdint;
            
            art.Last_Action = lastaction;
            art.Last_Action_Date = lastactdate;
            art.Movement = movement;
            art.Article_Type_ID = articletypeid;

            try
            {
                dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                Log.add("Erreur synchroUpdateEPC: " + e.Message);
                if (Properties.Settings.Default.UseDBGMSG)
                {
                    MessageBox.Show("Erreur lors de la synchronisation de l'armoire: " + e.Message);
                }
            }
        }

        // v3 inutilisé
        public void synchroUpdateEPC(int id, DateTime mod, string taille, int lavage, int state, string lastuser, string lastreader, string lastaction, DateTime lastactdate, int movement, int articletypeid, int armoireid, int actif)
        {
            epc art = dbContext.epc.FirstOrDefault(i => i.Id == id);
            art.Date_Modification = mod;
            art.Taille = taille;
            art.Cycle_Lavage_Count = lavage;
            art.State = state;
            art.Last_User = lastuser;
            art.Last_Reader = lastreader;

            int armoireIdint;
            bool convReussi = false;
            convReussi = int.TryParse(lastreader, out armoireIdint);
            if (!convReussi)
            {
                Log.add("Erreur synchroUpdateEPC: Erreur de conversion Last_Reader");
            }
            art.Armoire_ID = armoireIdint;

            art.Last_Action = lastaction;
            art.Last_Action_Date = lastactdate;
            art.Movement = movement;
            art.Article_Type_ID = articletypeid;
            art.Actif = actif;

            try
            {
                dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                Log.add("Erreur synchroUpdateEPC: " + e.Message);
                if (Properties.Settings.Default.UseDBGMSG)
                {
                    MessageBox.Show("Erreur lors de la synchronisation de l'armoire: " + e.Message);
                }
            }
        }
        #endregion

        #region SELECTs


        public IEnumerable<user> getUser()
        {
            if (IsAvailable())
            {
                try
                {
                    IQueryable<user> query = from f in dbContext.user where f.Active == 1 select f;
                    if (query.Count() > 0)
                        return query.ToList();
                    else
                        return new List<user>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur getUser: " + e.Message);
                    return new List<user>();
                }
            }
            else
                return new List<user>();
        }


        public IEnumerable<content_arm> getContent()
        {
            if (IsAvailable())
            {
                try
                {
                    dbContext.Refresh(System.Data.Objects.RefreshMode.StoreWins, dbContext.content_arm);
                    IQueryable<content_arm> query = from f in dbContext.content_arm select f;
                    if (query.Count() > 0)
                        return query.ToList();
                    else
                        return new List<content_arm>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur getContent: " + e.Message);
                    return new List<content_arm>();
                }
            }
            else
                return new List<content_arm>();


        }

        // retourne les epc présents dans l'armoire
        public IEnumerable<epc> getEPC()
        {
            if (IsAvailable())
            {
                try
                {
                    IQueryable<epc> query = from f in dbContext.epc where f.State == 100 || f.State == 5 select f;
                    
                    if (query.Count() > 0)
                        return query.ToList();
                    else
                        return new List<epc>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur getEPC: " + e.Message);
                    return new List<epc>();
                }
            }
            else
                return new List<epc>();
        }

        //retourne l'epc correspondant au tag donné en argument
        public epc getEpc(string tag)
        {
            if (IsAvailable())
            {
                try
                {
                    return dbContext.epc.FirstOrDefault(x => x.Tag == tag);
                }
                catch { return null; }
            }
            else
                return null;
        }

        public IEnumerable<epc> getAllEPC()
        {
            if (IsAvailable())
            {
                try
                {
                    IQueryable<epc> query = from f in dbContext.epc select f;
                    if (query.Count() > 0)
                        return query.ToList();
                    else
                        return new List<epc>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur getAllEPC: " + e.Message);
                    return new List<epc>();
                }
            }
            else
                return new List<epc>();


        }

        public IEnumerable<epc> getRestEPC()
        {
            if (IsAvailable())
            {
                try
                {
                    IQueryable<epc> query = from f in dbContext.epc where f.State != 300 && f.State != 100 && f.State != 10 && f.State != 5 select f;
                    if (query.Count() > 0)
                        return query.ToList();
                    else
                        return new List<epc>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur getRestEPC: " + e.Message);
                    return new List<epc>();
                }
            }
            else
                return new List<epc>();
        }


        public IEnumerable<epc> getEPCUserArticle(string tag)
        {
            if (IsAvailable())
            {
                try
                {
                    IQueryable<epc> query = from f in dbContext.epc where f.Tag == tag select f;
                    if (query.Count() > 0)
                        return query.ToList();
                    else
                        return new List<epc>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur getEPCUserArticle: " + e.Message);
                    return new List<epc>();
                }
            }
            else
                return new List<epc>();

        }


        public IEnumerable<article_type> getArticle_type()
        {
            if (IsAvailable())
            {
                try
                {
                    IQueryable<article_type> query = from f in dbContext.article_type select f;
                    if (query.Count() > 0)
                        return query.ToList();
                    else
                        return new List<article_type>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur getArticle_type: " + e.Message);
                    return new List<article_type>();
                }
            }
            else
                return new List<article_type>();


        }

        public IEnumerable<user_article> getUser_articleById(int id)
        {
            if (IsAvailable())
            {
                try
                {
                    IQueryable<user_article> query = from f in dbContext.user_article where f.User_Id == id select f;
                    if (query.Count() > 0)
                        return query.ToList();
                    else
                        return new List<user_article>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur getUser_articleById: " + e.Message);
                    return new List<user_article>();
                }
            }
            else
                return new List<user_article>();
        }

        public IEnumerable<user_article> getUser_article()
        {

            IQueryable<user_article> query;

            if (IsAvailable())
            {
                try
                {
                    query = from f in dbContext.user_article select f;
                    if (query.Count() > 0)
                        return query.ToList();
                    else
                        return new List<user_article>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur getUser_article: " + e.Message);
                    return new List<user_article>();
                }
            }
            else
                return new List<user_article>();


        }
        public IEnumerable<@case> getCase()
        {
            if (IsAvailable())
            {
                try
                {
                    IQueryable<@case> query = from f in dbContext.@case where f.Armoire_ID == Properties.Settings.Default.NumArmoire orderby f.Id select f;
                    if (query.Count() > 0)
                        return query.ToList();
                    else
                        return new List<@case>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur getCase: " + e.Message);
                    return new List<@case>();
                }
            }
            else
                return new List<@case>();


        }
        
        public IEnumerable<user> getUserById(int clause)
        {
            if (IsAvailable())
            {
                try
                {
                    IQueryable<user> query = from f in dbContext.user where f.Id == clause select f;
                    if (query.Count() > 0)
                        return query.ToList();
                    else
                        return new List<user>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur getUserById: " + e.Message);
                    return new List<user>();
                }
            }
            else
                return new List<user>();
        }

        public user getUserByPassword(string password)
        {
            if (IsAvailable())
            {
                try
                {
                    IQueryable<user> query = from f in dbContext.user where f.Password == password select f;
                    if (query.Count() > 0)
                    {
                        return query.FirstOrDefault();
                    }
                    else return null;
                }
                catch { return null; }
            }
            else return null;
        }

        //Recupere le type de l'utilisateur
        public IEnumerable<user> getUserType(string clause)
        {
            if (IsAvailable())
            {
                try
                {
                    IQueryable<user> query = from f in dbContext.user where f.Password == clause select f;
                    if (query.Count() > 0)
                        return query.ToList();
                    else
                        return new List<user>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur getUserType: " + e.Message);
                    return new List<user>();
                }
            }
            else
                return new List<user>();


        }

        //Recupere la liste des tag par type d'article, taille du porteur, et present dans l'armoire
        public IEnumerable<epc> getEpcByArticleIdAndSize(int artId, string size, bool avecTaille, bool avecState)
        {
            if (IsAvailable())
            {
                try
                {
                    //Rafraichissement de epc dans le dbContext
                    dbContext.Refresh(System.Data.Objects.RefreshMode.StoreWins, dbContext.epc);

                    IQueryable<string> queryPlus = (from g in dbContext.corresp_taille
                                                    from h in
                                                        (from st in dbContext.corresp_taille
                                                         where st.Taille == size
                                                         select st)
                                                    where (g.Classement_tailles == h.Classement_tailles + 1
                                                    || g.Classement_tailles == h.Classement_tailles
                                                    || g.Classement_tailles == h.Classement_tailles - 1)
                                                    select g.Taille);
                    IQueryable<epc> query;
                    if (avecState && avecTaille)
                    {
                        query = from f in dbContext.epc
                                where f.Article_Type_ID == artId && f.State == 100 && queryPlus.Contains(f.Taille)
                                select f;
                    }
                    else
                    {
                        if (avecTaille && !avecState)
                        {
                            query = from f in dbContext.epc
                                    where f.Article_Type_ID == artId && queryPlus.Contains(f.Taille)
                                    select f;
                        }
                        else
                        {
                            if (!avecTaille && avecState)
                            {
                                query = from f in dbContext.epc
                                        where f.Article_Type_ID == artId && f.State == 100
                                        select f;
                            }
                            else
                            {
                                query = from f in dbContext.epc
                                        where f.Article_Type_ID == artId
                                        select f;
                            }
                        }

                    }

                    if (query.Count() > 0)
                        return query.ToList();
                    else
                        return new List<epc>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur getEpcByArticleIdAndSize: " + e.Message);
                    return new List<epc>();
                }
            }
            else
                return new List<epc>();


        }

        // retourne une liste d'epc qui correspond par user_article_id, taille et statut
        public IEnumerable<epc> getEpcByArticleIdAndUniqueSize(int artId, string size, int statut)
        {
            if (IsAvailable())
            {
                try
                {
                    //Rafraichissement de epc dans le dbContext
                    dbContext.Refresh(System.Data.Objects.RefreshMode.StoreWins, dbContext.epc);

                    IQueryable<epc> query = from f in dbContext.epc where f.Article_Type_ID == artId && f.Taille == size && f.State == statut && f.Armoire_ID == Properties.Settings.Default.NumArmoire select f;

                    if (query.Count() > 0)
                        return query.ToList();
                    else
                        return new List<epc>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur getEpcByArticleIdAndUniqueSize: " + e.Message);
                    return new List<epc>();
                }
            }
            else
                return new List<epc>();


        }

        // retourne une liste d'epc qui correspond par user_article_id, taille, numArmoire et statut
        public IEnumerable<epc> getEpcByArticleIdAndUniqueSize(int artId, string size, int numArmoire, int statut)
        {
            if (IsAvailable())
            {
                try
                {
                    //Rafraichissement de epc dans le dbContext
                    dbContext.Refresh(System.Data.Objects.RefreshMode.StoreWins, dbContext.epc);

                    IQueryable<epc> query = from f in dbContext.epc where f.Article_Type_ID == artId && f.Taille == size && f.Armoire_ID == numArmoire && f.State == statut select f;

                    if (query.Count() > 0)
                        return query.ToList();
                    else
                        return new List<epc>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur getEpcByArticleIdAndUniqueSize: " + e.Message);
                    return new List<epc>();
                }
            }
            else
                return new List<epc>();


        }


        // retourne une liste d'epc qui correspond par user_article_id, et taille
        public IEnumerable<epc> getEpcByArticleIdAndUniqueSize(int artId, string size)
        {
            if (IsAvailable())
            {
                try
                {
                    //Rafraichissement de epc dans le dbContext
                    dbContext.Refresh(System.Data.Objects.RefreshMode.StoreWins, dbContext.epc);

                    IQueryable<epc> query = from f in dbContext.epc where ((f.Article_Type_ID == artId) && (f.Taille == size)) select f;

                    if (query.Count() > 0)
                        return query.ToList();
                    else
                        return new List<epc>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur getEpcByArticleIdAndUniqueSize: " + e.Message);
                    return new List<epc>();
                }
            }
            else
                return new List<epc>();


        }

        public IEnumerable<epc> getEpcTailleSupByArticleIdAndOriginalSize(int artId, string size, int statut)
        {
            if (IsAvailable())
            {
                try
                {
                    //Rafraichissement de epc dans le dbContext
                    dbContext.Refresh(System.Data.Objects.RefreshMode.StoreWins, dbContext.epc);

                    IQueryable<string> queryPlus = (from g in dbContext.corresp_taille
                                                    from h in
                                                        (from st in dbContext.corresp_taille
                                                         where st.Taille == size
                                                         select st)
                                                    where (g.Classement_tailles == h.Classement_tailles + 1)
                                                    select g.Taille);

                    IQueryable<epc> query = from f in dbContext.epc
                                            where f.Article_Type_ID == artId && f.State == statut && queryPlus.Contains(f.Taille) && f.Armoire_ID == Properties.Settings.Default.NumArmoire
                                            select f;

                    if (query.Count() > 0)
                        return query.ToList();
                    else
                        return new List<epc>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur getEpcTailleSupByArticleIdAndOriginalSize: " + e.Message);
                    return new List<epc>();
                }
            }
            else
                return new List<epc>();
        }

        public IEnumerable<epc> getEpcTailleSupByArticleIdAndOriginalSize(int artId, string size, int numArmoire, int statut)
        {
            if (IsAvailable())
            {
                try
                {
                    //Rafraichissement de epc dans le dbContext
                    dbContext.Refresh(System.Data.Objects.RefreshMode.StoreWins, dbContext.epc);

                    IQueryable<string> queryPlus = (from g in dbContext.corresp_taille
                                                    from h in
                                                        (from st in dbContext.corresp_taille
                                                         where st.Taille == size
                                                         select st)
                                                    where (g.Classement_tailles == h.Classement_tailles + 1)
                                                    select g.Taille);

                    IQueryable<epc> query = from f in dbContext.epc
                                            where f.Article_Type_ID == artId && f.State == statut && queryPlus.Contains(f.Taille) && f.Armoire_ID == numArmoire
                                            select f;

                    if (query.Count() > 0)
                        return query.ToList();
                    else
                        return new List<epc>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur getEpcTailleSupByArticleIdAndOriginalSize: " + e.Message);
                    return new List<epc>();
                }
            }
            else
                return new List<epc>();
        }

        // récupere la Taille inférieur telle qu'elle est définie dans la table corresp_taille
        public string getTailleInfByOriginalSize(string size)
        {
            if (IsAvailable())
            {
                try
                {
                    IQueryable<string> queryPlus = (from g in dbContext.corresp_taille
                                                    from h in
                                                        (from st in dbContext.corresp_taille
                                                         where st.Taille == size
                                                         select st)
                                                    where (g.Classement_tailles == h.Classement_tailles - 1)
                                                    select g.Taille);

                    if (queryPlus.Count() > 0)
                        return queryPlus.FirstOrDefault().ToString();
                    else
                        return null;
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


        //Recupere les article type pour le loader admin
        public IEnumerable<string> getArticleTypeDesc()
        {
            if (IsAvailable())
            {
                try
                {
                    IQueryable<string> query = from f in dbContext.article_type select f.Description;
                    if (query.Count() > 0)
                        return query.ToList();
                    else
                        return new List<string>();
                }
                catch { return new List<string>(); }
            }
            else
                return new List<string>();


        }

        //Recupere tout les tag gerer par l'armoire pour comparé apres le scan
        public IEnumerable<string> getTag()
        {
            if (IsAvailable())
            {
                try
                {
                    IQueryable<string> query = from f in dbContext.epc select f.Tag;
                    if (query.Count() > 0)
                        return query.ToList();
                    else
                        return new List<string>();
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

        //Recupere tout les tag toute ses infos
        public IEnumerable<epc> getTagAll()
        {
            if (IsAvailable())
            {
                try
                {
                    dbContext.Refresh(System.Data.Objects.RefreshMode.StoreWins, dbContext.epc);
                    IQueryable<epc> query = from f in dbContext.epc select f;
                    if (query.Count() > 0)
                        return query.ToList();
                    else
                        return new List<epc>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur getTagAll: " + e.Message);
                    return new List<epc>();
                }
            }
            else
                return new List<epc>();
        }

        //Recupere les article type pour le loader admin
        public IEnumerable<string> getPhoto(int id)
        {
            if (IsAvailable())
            {
                try
                {
                    IQueryable<string> query = from f in dbContext.article_type where f.Id == id select f.Photo;
                    if (query.Count() > 0)
                        return query.ToList();
                    else
                        return new List<string>();
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
            if (IsAvailable())
            {
                try
                {
                    IQueryable<string> query = from f in dbContext.article_type where f.Id == id select f.Description;
                    return query.FirstOrDefault().ToString();
                }
                catch { return ""; }
            }
            else
                return "";


        }


        //Recupere id d'un article type par sa description pour le loader admin
        public int getArticleTypeDescid(string desc)
        {
            if (IsAvailable())
            {
                try
                {
                    IQueryable<int> query = from f in dbContext.article_type where f.Description == desc select f.Id;
                    return query.FirstOrDefault();
                }
                catch { return 0; }
            }
            else
                return 0;


        }

        public IEnumerable<@case> getTaille()
        {
            if (IsAvailable())
            {
                try
                {
                    IQueryable<@case> query = from f in dbContext.@case.Distinct() select f;
                    if (query.Count() > 0)
                        return query.ToList();
                    else
                        return new List<@case>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur getTaille: " + e.Message);
                    return new List<@case>();
                }
            }
            else
                return new List<@case>();
        }

        public IEnumerable<@case> getCaseForReload(int articleId, string taille)
        {
            if (IsAvailable())
            {
                try
                {
                    IQueryable<@case> query = from f in dbContext.@case
                                              where f.Article_Type_Id == articleId
                                              && f.Taille == taille
                                              && f.Armoire_ID == Properties.Settings.Default.NumArmoire
                                              select f;
                    if (query.Count() > 0)
                        return query.ToList();
                    else
                        return new List<@case>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur getCaseForReload: " + e.Message);
                    return new List<@case>();
                }
            }
            else
                return new List<@case>();
        }

        public IEnumerable<@case> getCaseForReload(int articleId)
        {
            if (IsAvailable())
            {
                try
                {
                    IQueryable<@case> query = from f in dbContext.@case
                                              where f.Article_Type_Id == articleId
                                              && f.Armoire_ID == Properties.Settings.Default.NumArmoire
                                              select f;
                    if (query.Count() > 0)
                        return query.ToList();
                    else
                        return new List<@case>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur getCaseForReload: " + e.Message);
                    return new List<@case>();
                }
            }
            else
                return new List<@case>();
        }

        /// <summary>
        /// Get de tous les article_taille de l'armoire utilisée
        /// </summary>
        /// <returns></returns>
        public IEnumerable<article_taille> getArticleTaille()
        {
            if (IsAvailable())
            {
                try
                {
                    IQueryable<article_taille> query = from f in dbContext.article_taille where f.Armoire == Properties.Settings.Default.NumArmoire select f;

                    if (query.Count() > 0)
                        return query.ToList();
                    else
                        return new List<article_taille>();
                }
                catch (Exception e)
                {
                    Log.add("Erreur getArticleTaille: " + e.Message);
                    return new List<article_taille>();
                }
            }
            else
                return new List<article_taille>();
        }


#if OPTLANG
        // Récupération de la langue d'un user
        public string getLangByUserID(int userID)
        {
            if (IsAvailable())
            {
                try
                {
                    IQueryable<string> userlang = from u in dbContext.user
                                                  from l in dbContext.corresp_lang
                                                  where u.Id_Lang == l.Id
                                                  && u.Id == userID
                                                  select l.cultureName;

                    if (userlang.Count() > 0)
                        return userlang.FirstOrDefault().ToString();
                    else
                        return "fr-FR";

                }
                catch
                {
                    if (Properties.Settings.Default.UseDBGMSG)
                    {
                        Log.add("Exception levée durant la requête sur base user et corresp_lang");
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

        public int insertAlert(int altype, int user, int arm, string mess)
        {
            string date = String.Format("{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now);

            alert alrt = new alert();

            alrt.Date_Creation = DateTime.Now;
            alrt.Alert_Type_Id = altype;
            alrt.User_ID = user;
            alrt.Armoire_ID = arm;
            alrt.Traiter = 0;
            alrt.Message = mess;

            try
            {
                dbContext.AddToalert(alrt);
                dbContext.SaveChanges();
                Log.add("Alerte de type " + altype.ToString() + ": " + mess);
                return alrt.Id;
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

            tag_alert alrtag = new tag_alert();

            alrtag.Date_Creation = DateTime.Now;
            alrtag.Alert_ID = alrtID;
            alrtag.Tag_Command = command;
            alrtag.Tag_Retir = retir;
            alrtag.Tag_Intrus = intrus;
            alrtag.Article_Type_Code = articletypecode;
            alrtag.Taille = taille;

            try
            {
                dbContext.AddTotag_alert(alrtag);
                dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                Log.add("Erreur insertTagAlert: " + e.Message);
                if (Properties.Settings.Default.UseDBGMSG)
                {
                    MessageBox.Show("Erreur lors de la création de la tagAlert: " + e.Message);
                }
            }
        }

        public void insertCase(int id, string taille, int maxitem, int numArmoire, string articletype)
        {
            DateTime dt = DateTime.Now;
            string date = String.Format("{0:yyyy-MM-dd HH:mm:ss}", dt);
            int toto = getArticleTypeDescid(articletype);

            @case c = new @case();

            c.Id = id;
            c.Taille = taille;
            c.Date_Creation = DateTime.Parse(date);
            c.Max_Item = maxitem;
            c.Armoire_ID = numArmoire;
            c.Article_Type_Id = toto;

            try
            {
                dbContext.AddTocase(c);
                dbContext.SaveChanges();
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

        public void insertArticleType(int id, string code, string desc, string color, string sexe, string photo, int active, string desclong)
        {


            DateTime dt = DateTime.Now;
            string date = String.Format("{0:yyyy-MM-dd HH:mm:ss}", dt);

            article_type a = new article_type();

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
                dbContext.AddToarticle_type(a);
                dbContext.SaveChanges();
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

        public void insertArticleTaille(int article_type_id, string taille, int armoire, bool vide)
        {
            article_taille a = new article_taille();
            a.Article_Type_ID = article_type_id;
            a.Taille = taille;
            a.Armoire = armoire;
            a.Vide = vide;

            try
            {
                dbContext.article_taille.AddObject(a);
                dbContext.SaveChanges();
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


        public void scan(List<Tag> list, int ReaderId, int state, string UserId)
        {
            Log.add("Scan armoire: " + list.Count.ToString() + " articles scannés");
            // getRfidReaderState(ReaderId);
            DateTime dt = DateTime.Now;
            string date = String.Format("{0:yyyy-MM-dd HH:mm:ss}", dt);

            

            // on récupère les epc inconnus sans tenir compte de ReaderId
            List<content_arm> listca = new List<content_arm>();
            try
            {
                if (IsAvailable())
                {
                    IQueryable<content_arm> query = from f in dbContext.content_arm where f.State == 300 || f.State == 5 select f;
                    if (query.Count() > 0)
                    {
                        listca = query.ToList();
                    }
                }
            }
            catch(Exception e)
            {
                if(Properties.Settings.Default.UseDBGMSG)
                {
                    MessageBox.Show(e.Message);
                }
            }
            
            // on récupère la liste des epc à l'état 100 dans toutes les autres armoires
            List<content_arm> list100anothercabinet = new List<content_arm>();
            try
            {
                if (IsAvailable())
                {
                    IQueryable<content_arm> query = from f in dbContext.content_arm where f.State == 100 && f.RFID_Reader!=ReaderId select f;
                    if (query.Count() > 0)
                    {
                        list100anothercabinet = query.ToList();
                    }
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
            string command = "DELETE FROM content_arm WHERE RFID_Reader=" + Properties.Settings.Default.NumArmoire;
            
            try
            {
                dbContext.ExecuteStoreCommand(command);
            }
            catch (Exception e)
            {
                Log.add("Erreur scan: " + e.Message);
                if (Properties.Settings.Default.UseDBGMSG)
                {
                    MessageBox.Show("Erreur lors du scan: " + e.Message);
                }
            }


            // Remplissage de la table content_arm
            try
            {
                foreach (var tag in list)
                {
                    content_arm cont = new content_arm();

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
                        foreach (content_arm ca in listca)
                        {
                            // si le Tag trouvé après le scan était déjà dans l'armoire et qu'il était dans un état intru
                            // 300 ou 5 alors on ne change pas cet état car on ne peut pas passé de intrus à un autre état
                            
                            if (cont.Epc == ca.Epc) 
                            {
                                cont.State = ca.State;
                            }
                        }
                    }

                    if (list100anothercabinet != null)
                    {
                        foreach (content_arm ca in list100anothercabinet)
                        {
                            // si le Tag trouvé après le scan est à 100 dans l'autre armoire,
                            // on le supprime de la table content_arm,il sera ajouté par la suite
                            // et on le met à jour dans la table epc
                            if (cont.Epc == ca.Epc)
                            {
                                dbContext.content_arm.ToList().RemoveAll(x => x.Epc == ca.Epc);
                                updateEpc(state, cont.Epc, UserId, ReaderId);
                            }
                        }
                    }


                    dbContext.AddTocontent_arm(cont);
                    dbContext.SaveChanges();

                }
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



        #endregion

        #region UPDATEs

        public void updateContent(int state, string tag)
        {

            try
            {
                content_arm co = dbContext.content_arm.FirstOrDefault(i => i.Epc == tag);
                co.State = state;
                dbContext.SaveChanges();
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
        /*
        public void updateVersionLogiciel(string Version)
        {
            try
            {
                string sourceconnectstring = "SERVER=localhost;" + "Port=3306;" + "DATABASE=coding01v3;" + "UID=armoire;" + "Pwd=$$armoire;";

                string source = "UPDATE version SET VersLog=" + Version + "WHERE Id="+Properties.Settings.Default.NumArmoire+";";
                MySqlConnection sourcemyconn1 = new MySqlConnection(sourceconnectstring);
                MySqlDataAdapter sourcemydataadapter1 = new MySqlDataAdapter();
                sourcemydataadapter1.SelectCommand = new MySqlCommand(source, sourcemyconn1);
                MySqlCommandBuilder cb1 = new MySqlCommandBuilder(sourcemydataadapter1);
                sourcemyconn1.Open();
                MySqlCommand cmd1 = new MySqlCommand(source, sourcemyconn1);
               // MySqlDataReader dataReader = cmd1.ExecuteReader();
                
                 MessageBox.Show("connected");
                //int count = 0;
                DataTable dt1 = new DataTable();
                sourcemydataadapter1.Fill(dt1);
                 
                dataGrid1.ItemsSource = dt1.DefaultView;

                   MySqlDataReader dataReader = cmd1.ExecuteReader();



                //Connect to destination database
                   string DestinationConString = "SERVER=localhost;" + "DATABASE=surveillancearmoire;" + "UID=root;";
                  
                   MySqlConnection destinationconn = new MySqlConnection(DestinationConString);
                   destinationconn.Open();
                      
                while (dataReader.Read())
                   {
                      vId = dataReader.GetString("Id");
                      vDateCreation = dataReader.GetDateTime("Date_Creation");
                       formatForMySql = vDateCreation.ToString("yyyy-MM-dd HH:mm:ss");

                      vAlertTypeId = dataReader.GetString("Alert_Type_Id");
                      vMessage = dataReader.GetString("Message");
                      vUseId = dataReader.GetString("User_ID");
                      vArmoireId = dataReader.GetString("Armoire_ID");
                      vTraiter = dataReader.GetString("Traiter");

                      string destination = "INSERT INTO alert(`Id`,`DateCreation`,`AlertTypeId`,`Message`,`UseId`,`ArmoireId`,`Traiter`) VALUES ('" + vId + "','" + formatForMySql + "','" + vAlertTypeId + "','" + vMessage + "','" + vUseId + "','" + vArmoireId + "','" + vTraiter + "' );";


                      MySqlCommand descom = new MySqlCommand(destination, destinationconn);
                      var result = descom.ExecuteNonQuery();




                    //  MessageBox.Show(vId);
                   }
            try
            {
                content_arm co = dbContext.content_arm.FirstOrDefault(i => i.Epc == tag);
                co.State = state;
                dbContext.SaveChanges();
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
            */

        public void updateAlert(int id)
        {
            try
            {
                alert alr = dbContext.alert.FirstOrDefault(t => t.Id == id);
                alr.Traiter = 1;
                dbContext.SaveChanges();
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

        public void updateArticleTaille(int artype, string taille, int armoire, bool vide)
        {
            article_taille at = new article_taille();

            try
            {
                at = dbContext.article_taille.FirstOrDefault(i => i.Article_Type_ID == artype && i.Taille == taille && i.Armoire == armoire);
                at.Vide = vide;
                dbContext.SaveChanges();
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


        public void updateEpcReload(int state, string tag, int reader, string user)
        {
            epc co = new epc();
            DateTime dt = DateTime.Now;
            string date = String.Format("{0:yyyy-MM-dd HH:mm:ss}", dt);

            try
            {
                co = dbContext.epc.FirstOrDefault(i => i.Tag == tag && i.State != 300 && i.State != 5 && i.State != 2);

                if (co == null) // l'epc n'est ni inconnu ni perdu
                {
                    epc ee = dbContext.epc.FirstOrDefault(i => i.Tag == tag); // le connait-on ?
                    if (ee == null) // non
                    {
                        Log.add("updateEpcReload: Tag " + tag + " - State " +/* co.State +*/ " => 300");
                        updateContent(300, tag);
                    }
                    else // oui
                    {
                        Log.add("updateEpcReload: Tag " + tag + " - State " +/* co.State +*/ " => " + ee.State);
                        updateContent(ee.State, tag);
                    }
                }
                else
                {
                    if (co.State == 1 || co.State == 10 || co.State == 20 || co.State == 0 || co.State == 4)
                    {
                        Log.add("updateEpcReload: Tag " + tag + " - State " + co.State + " => " + state);
                        co.Armoire_ID = Properties.Settings.Default.NumArmoire;
                        co.State = state;
                        co.Last_Action_Date = DateTime.Parse(date);
                        co.Date_Modification = DateTime.Parse(date);
                        co.Last_User = user;
                        co.Last_Reader = reader.ToString();
                        co.Movement = co.Movement++;
                        co.Cycle_Lavage_Count = co.Cycle_Lavage_Count++;
                    }
                    dbContext.SaveChanges();
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
                epc co = dbContext.epc.FirstOrDefault(i => i.Tag == tag);
                co.State = state;
                co.Last_Action_Date = DateTime.Parse(date);
                co.Date_Modification = DateTime.Parse(date);
                co.Last_User = user;
                co.Last_Reader = reader.ToString();
                co.Movement = co.Movement++;
                co.Cycle_Lavage_Count = co.Cycle_Lavage_Count++;
                co.Armoire_ID = reader;

                dbContext.SaveChanges();
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
                DateTime dt = DateTime.Now;
                string date = String.Format("{0:yyyy-MM-dd HH:mm:ss}", dt);
                epc co = dbContext.epc.FirstOrDefault(i => i.Tag == tag);
                co.State = state;
                co.Last_Action_Date = DateTime.Parse(date);
                co.Date_Modification = DateTime.Parse(date);
                co.Last_User = user;
                co.Last_Reader = reader.ToString();
                co.Movement = co.Movement++;
                co.Cycle_Lavage_Count = co.Cycle_Lavage_Count++;
                co.Armoire_ID = reader;
                co.Actif = actif;

                dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                Log.add("Erreur updateEpc: " + e.Message);
                if (Properties.Settings.Default.UseDBGMSG)
                    System.Windows.MessageBox.Show("updateEpc: Tag inexistant");
            }
        }

        public void updateEpcRestitution(epc e, int reader)
        {
            try
            {
                epc temp = dbContext.epc.FirstOrDefault(x => x.Id == e.Id);
                if (temp != null)
                {
                    temp.State = 10;
                    temp.Date_Modification = DateTime.Now;
                    temp.Last_Action_Date = DateTime.Now;
                    temp.Last_Reader = reader.ToString();
                    temp.Cycle_Lavage_Count++;
                    temp.Movement++;
                    temp.Armoire_ID = reader;
                    dbContext.SaveChanges();
                    Log.add("Restitution: mise à jour de l'epc " + temp.Tag + ": State = " + temp.State.ToString());
                }
            }
            catch (Exception ex)
            {
                Log.add("Erreur updateEpcRestitution: " + ex.Message);
                if (Properties.Settings.Default.UseDBGMSG)
                    System.Windows.MessageBox.Show("updateEpcRestitution: Tag inexistant");
            }
        }

        public void updateEpcRestitution(int state, string tag, int reader)
        {
            try
            {

                DateTime dt = DateTime.Now;
                string date = String.Format("{0:yyyy-MM-dd HH:mm:ss}", dt);


                epc co = dbContext.epc.FirstOrDefault(i => i.Tag == tag);
                if (co != null)
                {
                    int cycle = co.Cycle_Lavage_Count;

                    co.State = state;
                    co.Last_Action_Date = DateTime.Parse(date);
                    co.Date_Modification = DateTime.Parse(date);

                    co.Last_Reader = reader.ToString();

                    co.Armoire_ID = reader;

                    co.Movement = co.Movement++;
                    co.Cycle_Lavage_Count = cycle++;

                    dbContext.SaveChanges();
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
                int toto = getArticleTypeDescid(articletype);

                @case lala = dbContext.@case.FirstOrDefault(i => i.Id == id);

                lala.Id = id;
                lala.Taille = taille;

                lala.Max_Item = maxitem;
                lala.Armoire_ID = numArmoire;
                lala.Article_Type_Id = toto;

                dbContext.SaveChanges();
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

        public void UpdateUserArticle(int id, int credrest)
        {
            try
            {
                dbContext.Refresh(System.Data.Objects.RefreshMode.StoreWins, dbContext.user_article);
                DateTime dt = DateTime.Now;
                string date = String.Format("{0:yyyy-MM-dd HH:mm:ss}", dt);

                user_article tmp = dbContext.user_article.FirstOrDefault(i => i.Id == id);

                tmp.Date_Modification = DateTime.Parse(date);

                Log.add("Update user_article local " + tmp.Id + ": Credits restants = " + tmp.Credit_Restant);
                tmp.Credit_Restant = credrest;
                Log.add("Update user_article local " + tmp.Id + ": Credits restants = " + tmp.Credit_Restant);

                dbContext.SaveChanges();
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

        public void UpdateUserArticleRestitution(epc epc)
        {
            try
            {
                int lastUser = 0;
                int.TryParse(epc.Last_User, out lastUser);
                Log.add("Restitution: user " + lastUser);
                dbContext.Refresh(System.Data.Objects.RefreshMode.StoreWins, dbContext.user_article);
                user_article temp = null;
                if (epc.State == 1)
                {
                    temp = dbContext.user_article.FirstOrDefault(x => x.Taille == epc.Taille && x.User_Id == lastUser && x.Article_Type_Id == epc.Article_Type_ID);
                }
                else if (epc.State == 4)
                {
                    // on ne tient plus compte de la taille
                    //string tailleInf = getTailleInfByOriginalSize(epc.Taille);
                    temp = dbContext.user_article.FirstOrDefault(x => /*x.Taille == tailleInf &&*/ x.User_Id == lastUser && x.Article_Type_Id == epc.Article_Type_ID);
                
                }
                if (temp != null)
                {
                    temp.Date_Modification = DateTime.Now;
                    temp.Credit_Restant++;
                    dbContext.SaveChanges();
                    Log.add("Restitution: mise à jour de l'user_article " + temp.Article_Type_Id + ": Crédit restant = " + temp.Credit_Restant.ToString());
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
            dbContext.Refresh(System.Data.Objects.RefreshMode.StoreWins, dbContext.user_article);
            try
            {
                DateTime dt = DateTime.Now;
                string date = String.Format("{0:yyyy-MM-dd HH:mm:ss}", dt);

                user_article tmp = dbContext.user_article.FirstOrDefault(i => i.Taille == taille && i.User_Id == userid && i.Article_Type_Id == articletype);

                tmp.Date_Modification = DateTime.Parse(date);

                int credrest = tmp.Credit_Restant;
                if (restituerCredit)
                {
                    tmp.Credit_Restant = tmp.Credit_Restant >= tmp.Credit ? tmp.Credit : tmp.Credit_Restant + 1;
                }
                Log.add("Update restitution user_article " + tmp.Id + ": Credits restants = " + credrest.ToString() + " => Credits restants = " + tmp.Credit_Restant.ToString());
                dbContext.SaveChanges();
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
            try
            {
                log_epc tmp = dbContext.log_epc.FirstOrDefault(i => i.Id == id);

                //tmp.Id = id;
                tmp.Synchronised = synchronised;

                dbContext.SaveChanges();
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
            
            user co = new user();
            DateTime dt = DateTime.Now;
            string date = String.Format("{0:yyyy-MM-dd HH:mm:ss}", dt);

            try
            {
                co = dbContext.user.First(i => i.Id == user);
                co.Last_Connection = dt;
                dbContext.SaveChanges();
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
                logMessage += t.Epc + " ";
            }
            Log.add(logMessage + ")");


            editRestitution(list, ReaderId);
            //   return compileScanInData();
            dbContext.Refresh(System.Data.Objects.RefreshMode.StoreWins, dbContext.user_article);
        }

        private void editRestitution(List<Tag> list, int ReaderId)
        {
            // liste des EPC lus par la restitution
            dbContext.Refresh(System.Data.Objects.RefreshMode.StoreWins, dbContext.epc);
            List<epc> epcRest = new List<epc>();
            foreach (Tag t in list)
            {
#if NEWIMPINJ
                epc e = getEpc(t.Epc.ToHexString());
#else
                epc e = getEpc(t.Epc);
#endif
                if (e != null && !epcRest.Contains(e))
                {
                    epcRest.Add(e);
                }
            }

            foreach (epc e in epcRest)
            {
                Log.add("Update epc :" + e.Tag + " etat : " + e.State);

                if (e.State != 2 && e.State != 10 && e.State != 100)
                {
                    Log.add("Recredite user : " + e.Last_User + " pour  epc :" + e.Tag + " etat : " + e.State);
                    UpdateUserArticleRestitution(e);
                }
                Log.add("Changement etat epc :" + e.Tag);
                updateEpcRestitution(e, ReaderId);
            }
        }

        #endregion

        

    }
}
#endif