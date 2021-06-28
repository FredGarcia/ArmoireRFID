using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArmoireV3.Entities
{
    class Armoire
    {
        /*
        private bool editRestitution(List<Tag> list, int ReaderId)
        {
            bool bres = false;
            try
            {
                // liste des EPC lus par la restitution
                //dbContext.Refresh(System.Data.Objects.RefreshMode.StoreWins, dbContext.epc);
#if EDMX
                List<epc> epcRest = new List<epc>();
#else
                List<Epc> epcRest = new List<Epc>();
#endif
                foreach (Tag t in list)
                {
#if NEWIMPINJ
#if EDMX
                    epc e = getEpc(t.Epc.ToHexString());
#else
                    Epc e = getEpc(t.Epc.ToHexString());
#endif
#else
#if EDMX
                    epc e = getEpc(t.Epc);
#else
                    Epc e = getEpc(t.Epc);
#endif
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
                        Log.add("Recredite user : " + e.Last_User + " pour  epc :" + e.Tag + " etat : " + e.State);
                        UpdateUserArticleRestitution(e);
                        bres = true;
                    }
                    Log.add("Changement etat epc :" + e.Tag);
                    updateEpcRestitution(e, ReaderId);
                }
            }
            catch (Exception ecee)
            {
                Log.add("EntityCommandExecutionException :" + ecee.Message);
            }
            return bres;
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
                    User_Article tmpusart = null;
                    if (epc.State == EtatVt.SORTI)
                    {
                        tmpusart = GetBackUserArticleFromUser(epc.Taille, lastUser, epc.Article_Type_ID);
                        //temp = dbContext.user_article.FirstOrDefault(x => x.Taille == epc.Taille && x.User_Id == lastUser && x.Article_Type_Id == epc.Article_Type_ID);
                    }
                    else if (epc.State == EtatVt.SORTI_TailleSup)
                    {
                        // on ne tient plus compte de la taille
                        //string tailleInf = getTailleInfByOriginalSize(epc.Taille);
                        tmpusart = GetBackUserArticleFromUser(lastUser, epc.Article_Type_ID);
                        //temp = dbContext.user_article.FirstOrDefault(x => /*x.Taille == tailleInf && /* x.User_Id == lastUser && x.Article_Type_Id == epc.Article_Type_ID);
          /*          }
                    if (tmpusart.Id == 0)
                    {
                        Log.add("La ligne User_Article pour cet utilisateur a été supprimée, mis à jour du last_user!");
                    }
                    if ((tmpusart != null) && (tmpusart.Id != 0))
                    { // N.B.: Si (tmpusart.Id == 0) c'est que l'utilisateur n'a pas droit au type d'article donc pas de crédit 
                        // Il ne doit jamais y avoir tmpusart == null
                        string query = "";
                        tmpusart.Date_Modification = DateTime.Now;
                        if (Properties.Settings.Default.RecreditHebdo == true && (tmpusart.Date_Modification.Day > Properties.Settings.Default.NumJourRecreditHebdo || (tmpusart.Date_Modification.Day == Properties.Settings.Default.NumJourRecreditHebdo && tmpusart.Date_Modification.Hour > Properties.Settings.Default.HeureRecreditHebdo)))
                        {
                            tmpusart.Credit_Semaine_Suivante++;
                            query = "UPDATE `user_article` SET `Credit_Semaine_Suivante`='" + tmpusart.Credit_Semaine_Suivante.ToString() + "', `Date_Modification` = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                            + "' WHERE `Id`='" + tmpusart.Id.ToString() + "';";
                        }
                        else
                        {
                            tmpusart.Credit_Restant++;
                            query = "UPDATE `user_article` SET `Credit_Restant`='" + tmpusart.Credit_Restant.ToString() + "', `Date_Modification` = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                            + "' WHERE `Id`='" + tmpusart.Id.ToString() + "';";
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
                        Log.add("Restitution: mise à jour de l'user_article " + tmpusart.Article_Type_Id + ": Crédit restant = " + tmpusart.Credit_Restant.ToString() + " , Crédit semaine suivante = " + tmpusart.Credit_Semaine_Suivante.ToString());
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
                    tmpepc.State = EtatVt.RESTITUE;
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


        //______________________________________________________________________//

        public void updateEpcReload(int state, string tag, int reader, string user)
        {
            //Epc co = new Epc();
            DateTime dt = DateTime.Now;
            string date = String.Format("{0:yyyy-MM-dd HH:mm:ss}", dt);

            try
            {
                //co = dbContext.epc.FirstOrDefault(i => i.Tag == tag && i.State != 300 && i.State != 5 && i.State != 2);
                Epc tmpepc = new Epc();
                tmpepc = GetBackEpcfromTag(tag, " `State` != '300' AND `State` != '5' AND `State` != '2' ");

                if (tmpepc == null) // l'epc n'est ni inconnu ni perdu
                {
                    //Epc ee = dbContext.epc.FirstOrDefault(i => i.Tag == tag); // le connait-on ?
                    Epc tmpepc2 = new Epc();
                    tmpepc2 = GetBackEpcfromTag(tag);
                    if (tmpepc2 == null) // non
                    {
                        Log.add("updateEpcReload: Tag " + tag + " - State " +/* co.State +*//* " => 300");
     /*                   updateContent(300, tag);
                    }
                    else // oui
                    {
                        Log.add("updateEpcReload: Tag " + tag + " - State " +/* co.State +*//* " => " + tmpepc2.State);
  /*                      updateContent(tmpepc2.State, tag);
                    }
                }
                else
                {
                    if (tmpepc.State == EtatVt.SORTI || tmpepc.State == EtatVt.RESTITUE || tmpepc.State == EtatVt.TRI_SALE || tmpepc.State == EtatVt.AUCUN || tmpepc.State == EtatVt.SORTI_TailleSup)
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
        private Epc GetBackEpcfromTag(string tag, string moreCondition)
        {
            int i = 0;
            if (IsAvailable())
            {
                // try
                {
                    Epc tmpepc = new Epc();
                    string query = "SELECT * FROM `epc` ";
                    query += " WHERE  `Tag`= '" + tag.ToString() + "' AND " + moreCondition + " LIMIT 1;";
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
 /*           }
            else
            {
                Log.add("Erreur GetBackcasefromid: pas d'accès à la base de donnée locale");
                throw (new Exception("pas d'acces à la base locale"));
                //return new Case(0,0,"00", new DateTime(1971, 01, 01), 0, 0,0);
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
  /*          }
            else
            {
                Log.add("Erreur GetBackcasefromid: pas d'accès à la base de donnée locale");
                throw (new Exception("pas d'acces à la base locale"));
                //return new Case(0,0,"00", new DateTime(1971, 01, 01), 0, 0,0);
            }
        }
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

                                "  `State`='" + ca.State.ToString() +
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
        }*/

    }
}
