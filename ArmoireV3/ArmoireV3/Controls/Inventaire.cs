using System;
using System.Collections.Generic;
using System.Linq;
using ArmoireV3.Entities;
using Impinj.OctaneSdk;
using System.Windows.Interop;
using System.Windows;
using ArmoireV3.Dialogues;

namespace ArmoireV3.Controls
{

    public class Inventaire
    {

      

       
        public static bool fini = true;
        static List<Tag> listT = new List<Tag>(); 
        static int ReaderId = Properties.Settings.Default.NumArmoire;
    

#if EDMX
        Worker mainWorker = new Worker();
#else
        private static WorkerV3 workerV3= new WorkerV3();
#endif
        #region SousFonctionUpdateListBox


#if EDMX
        // AVEC Modele EDMX
        private void ReorganiseContentArm(List<user> userg, List<content_arm> contentArm, List<epc> alltag, IEnumerable<string> deltaOut)
        {
            if (userg.Count != 0)
            {
                if (userg[0].Type == "reloader")
                {
                    foreach (content_arm carm in contentArm.Where(d => d.State != 300 && d.State != 5 && d.State != 2 && d.RFID_Reader == ReaderId))
                    {
#if EDMX
                            mainWorker.data.updateEpcReload(100, carm.Epc, ReaderId, userg[0].Id.ToString());
#else                        
                            workerV3.data.updateEpcReload(100, carm.Epc, ReaderId, userg[0].Id.ToString());
#endif
                    }
                    /*
                    foreach (content_arm carm in contentArm.Where(d => d.State == 100 && d.RFID_Reader == ReaderId))
                    {
                        mainWorker.data.updateEpcReload(100, carm.Epc, ReaderId, userg[0].Id.ToString());
                    }
                    */

                    // travail sur les epc à l'état 100 ou 300 ou 5

                    List<epc> tag100or300or5 = alltag.Where(t => (t.State == 100 || t.State == 300 || t.State == 5) && t.Last_Reader == ReaderId.ToString()).ToList(); // liste de tous les epc à l'état 100, 300 ou 5

                    foreach (content_arm carm in contentArm.Where(c => (c.State == 100 || c.State == 300 || c.State == 5) && c.RFID_Reader == ReaderId)) // test: on tient compte de l'armoire
                    {
                        // on supprime de la liste des epc à update tous les tags qui étaient dans l'armoire et qui ne sont toujours pas sortis
                        // (qui doivent donc rester à l'état 100 ou 300 ou 5)
                        if (!deltaOut.Contains(carm.Epc)) // test : il est à l'état 100 dans l'autre armoire car elle a été refermée -- le cas 4.3.1 risque de ne plus fonctionner
                        {
                            tag100or300or5.RemoveAll(t => t.Tag == carm.Epc);
                        }
                    }
                    foreach (epc e in tag100or300or5)
                    {
                        // on met tous les epc à l'état 100 ou 300 ou 5 qui ne sont pas dans l'armoire à l'état 1
#if EDMX
                            mainWorker.data.updateEpc(1, e.Tag, userg[0].Id.ToString(), ReaderId);
#else
                            workerV3.data.updateEpc(1, e.Tag, userg[0].Id.ToString(), ReaderId);
#endif

                    }


                }
            }
        }
#else
        // SANS Modele EDMX
        /// <summary>
        /// Mise à jour de la table EPC (les Articles de content_arm qui ne sont pas inconnus passent à 100)
        /// Mode rechargement seulement
        /// </summary>
        /// <param name="userg"></param>
        /// <param name="contentArm"></param>
        /// <param name="alltag"></param>
        /// <param name="deltaOut"></param>
        private static void ReorganiseContentArm(User user, List<Content_Arm> contentArm, List<Epc> alltag, IEnumerable<string> deltaOut)
        {
            workerV3 = new WorkerV3();
            Log.add("Reorganise ContentArm pour le reloader");

            foreach (Content_Arm carm in contentArm.Where(d => d.State == EtatArticle.SORTI || d.State == EtatArticle.SORTI_TailleSup && d.RFID_Reader == ReaderId))
            {
                Epc tmpepc = new Epc();
                try
                {
                    tmpepc = workerV3.data.getEpc(carm.Epc);
                }
                catch
                {
                }

                Log.add("Mise a jour content_arm Linge Sale =" + carm.Epc + "de lastuser= " + tmpepc.Last_User.ToString() + "");
            }


            // Pour chaque élément de Content_Arm de l'armoire courante dont l'état est 300 (intrus inconnu) ou 5 (intrus connus) ou 2 (perdu????)
            // A priori il ne peut pas y avoir des vt en etat perdu dans content_arm
            foreach (Content_Arm carm in contentArm.Where(d => d.State != EtatArticle.INTRUS_INCONNUS && d.State != EtatArticle.INTRUS_CONNUS &&
                d.State != EtatArticle.PERDU && d.RFID_Reader == ReaderId))
            {
                workerV3.data.updateEpcReload(100, carm.Epc, ReaderId, user.Id.ToString());
                Log.add("change content_arm Epc =" + carm.Epc + "de state= " + carm.State.ToString() + " => 100");
            }
            /*
            foreach (content_arm carm in contentArm.Where(d => d.State == 100 && d.RFID_Reader == ReaderId))
            {
                mainWorker.data.updateEpcReload(100, carm.Epc, ReaderId, userg[0].Id.ToString());
            }
            */

            // travail sur les epc à l'état 100 ou 300 ou 5
            // donc sur les epc à l'état "Présent", intrus "inconnus" ou "connus"
            List<Epc> tag100or300or5 = alltag.Where(t => (t.State == EtatArticle.RETIRABLE || t.State == EtatArticle.INTRUS_INCONNUS || t.State == EtatArticle.INTRUS_CONNUS) && t.Last_Reader == ReaderId.ToString()).ToList(); // liste de tous les epc à l'état 100, 300 ou 5

            // Pour chaque élément de Content_Arm de l'armoire courante dont l'état est 100 (présent) ou 300 (intrus inconnu) ou 5 (intrus connus) 
            foreach (Content_Arm carm in contentArm.Where(c => (c.State == EtatArticle.RETIRABLE || c.State == EtatArticle.INTRUS_INCONNUS || c.State == EtatArticle.INTRUS_CONNUS) && c.RFID_Reader == ReaderId)) // test: on tient compte de l'armoire
            {
                // on supprime de la liste des epc à updater tous les tags qui étaient dans l'armoire et qui ne sont toujours pas sortis
                // (qui doivent donc rester à l'état 100 ou 300 ou 5)
                if (!deltaOut.Contains(carm.Epc)) // test : il est à l'état 100 dans l'autre armoire car elle a été refermée -- le cas 4.3.1 risque de ne plus fonctionner
                {
                    tag100or300or5.RemoveAll(t => t.Tag == carm.Epc);
                }
            }
            foreach (Epc e in tag100or300or5)
            {
                // on met tous les epc qui étaient à l'état 100 ou 300 ou 5 qui ne sont pas dans l'armoire à l'état 1
                workerV3.data.updateEpc(1, e.Tag, user.Id.ToString(), ReaderId);
                Log.add("change Epc table Tag =" + e.Tag + "de state= " + e.State.ToString() + " => 1 (Sortie)");
            }

        }
#endif

#if EDMX

        // AVEC Modele EDMX
        private int UpdateNbChoix(List<user> userg)
        {
            int nbchoix = 0;
            //---------------------------------------------------------------------------------
            if (userg.Count != 0)
            {
                if (userg[0].Type != "reloader")
                {
                    List<ListEpcCount> teml = val.listepccount.ToList();
                    foreach (int v in teml.Select(k => k.Type))
                    {
                        List<int> tulp = teml.Where(k => k.Type == v).Select(k => k.Count).ToList();
                        for (int c = 0; c < tulp[0]; c++)
                        {
                            nbchoix++;
                        }
                    }
                }
            }
            return nbchoix;
        }
#else
        /*
        // SANS Modele EDMX
        /// <summary>
        /// Mise à jour du nombre d'articles sélectionnés
        /// Pour les utilisateurs seulement
        /// </summary>
        /// <param name="userg"></param>
        /// <returns></returns>
        private int UpdateNbChoix(/ *List<User> userg* /)
        {
            int nbchoix = 0;
            //---------------------------------------------------------------------------------
           / * if (userg.Count != 0)
            {
                if (userg[0].Type != "reloader")
                {* /
                    List<ListEpcCount> teml = val.listepccount.ToList();
                    foreach (int v in teml.Select(k => k.Type))
                    {
                        List<int> tulp = teml.Where(k => k.Type == v).Select(k => k.Count).ToList();
                        for (int c = 0; c < tulp[0]; c++)
                        {
                            nbchoix++;
                        }
                    }
           / *      }
            }* /
            return nbchoix;
        }
        */
#endif
        /// <summary>
        /// Copie des itemOut vers DeltaOut
        /// </summary>
        /// <param name="deltaOut"></param>
        /// <param name="itemOut"></param>
        static void MiseAJourListeItemsSortis(IEnumerable<string> deltaOut, List<string> itemOut)
        {
            //Liste tout les items sorties
            foreach (string az in deltaOut)
            {

                itemOut.Add(az);//Ajoute un tag a list de tout les tag sorties
            }
        }


        //---------------------------------------------------------------------------------


#if EDMX        // AVEC Modele EDMX
        private void MiseAJourListeItemsSortisParRapportALaCommande(List<user> userg, int nbchoix, List<string> itemOut, List<epc> itemRightOut, List<epc> itemOutEpc, List<epc> itemNoOut, List<epc> itemDiffOut, List<int> articleTypeSelect)
        {
            //--------------------------------------------------------------------------------
            //List<epc> tempo = userSelect.Distinct().ToList(); // BUG 20131106
            //int nbartype = tempo.Select(p => p.Article_Type_ID).Distinct().Count();
            if (userg.Count != 0)
            {
                if (userg[0].Type != "reloader")
                {
                    int r = nbchoix;
                    foreach (ListEpcCount s in val.listepccount.ToList()) // val.listepccount est alimenté avec les articles sélectionnés (nbchoisi,article_type,taille) 
                    {

                        List<epc> tempo = userSelect.Where(a => a.Article_Type_ID == s.Type && a.Taille == s.Taille).ToList(); // correctif bug 2013/10/30

                        //bool first_step = true;

                        //Si il y avait une possibilité de selection
                        if (tempo.Count != 0)
                        {
                            if (itemOut.Any())
                            {
                                //int r = nbchoix; remonté avant le foreach
                                List<string> lst = itemOut;
                                int nb=tempo.Count
                                for (int i = 0; i < nb; i++)
                                {
                                    if (lst.Contains(tempo[i].Tag))
                                    {
                                        if (r > 0)
                                        {
                                            itemRightOut.Add(tempo[i]);
                                            itemOut.Remove(tempo[i].Tag);
                                        }
                                        r--;
                                    }
                                    else
                                    {
                                        /*
                                        if (!(articleTypeSelect.Contains(tempo[i].Article_Type_ID)))
                                            articleTypeSelect.Add(tempo[i].Article_Type_ID); */
                                        // il faut recherche dans la base si itemOut contient des articletype
                                        //correspondant après le ménage qui est fait dans itemOut.remove(tempo[i].Tag))
                                    }
                                }
                                if (!(articleTypeSelect.Contains(s.Type)))
                                    articleTypeSelect.Add(s.Type);
                                /*
                                // 1e Etape : convertir les itemout restant en list d'EPC (il y a des epc taillediff, inconnu et d'autre
                                for (int j = 0; j < itemOut.Count; j++) // bug 20131106 vérifier itemOut.Count
                                {
                                    epc tmp = mainWorker.data.getEpc(itemOut[j]);
                                    if (tmp != null)
                                        itemOutEpc.Add(tmp);
                                }
                                // 2e Etape : chercher dans les itemOutEpc si les articletype choisis s'y trouve
                                //            et les mettre dans itemDiffOut puis faire le ménage dans itemOut
                                for (int k = 0; k < itemOutEpc.Count; k++) // itemOutEpc.Count doit être égale à itemOut.Count
                                {
                                    if (articleTypeSelect.Contains(itemOutEpc[k].Article_Type_ID))
                                    {
                                        itemDiffOut.Add(itemOutEpc[k]); //vérifier le nombre de itemDiffOut (2)
                                        itemOut.Remove(itemOutEpc[k].Tag);
                                    }
                                }
                                // 3e Etape : il faut décréditer le user sur ces articles
                                //            ceci est fait plus bas
                                */
                            }
                            if (itemOut.Any()) // si il reste des itemOut qui ne sont pas dans la sélection (ni taille ni article_type)
                            {
                                itemOutEpc.Clear();
                                // 1e Etape : convertir les itemout restant en list d'EPC (il y a des epc taillediff, inconnu et d'autre
                                for (int j = 0; j < itemOut.Count; j++) // bug 20131106 vérifier itemOut.Count
                                {
                                    epc tmp = mainWorker.data.getEpc(itemOut[j]);
                                    if (tmp != null)
                                        itemOutEpc.Add(tmp);
                                }
                                // 2e Etape : chercher dans les itemOutEpc si les articletype choisis s'y trouve
                                //            et les mettre dans itemDiffOut puis faire le ménage dans itemOut
                                for (int k = 0; k < itemOutEpc.Count; k++)
                                {
                                    if (articleTypeSelect.Contains(itemOutEpc[k].Article_Type_ID))
                                    {
                                        itemDiffOut.Add(itemOutEpc[k]);
                                        itemOut.Remove(itemOutEpc[k].Tag);
                                    }
                                }
                                // 3e Etape : il faut décréditer le user sur ces articles
                                //            ceci est fait plus bas
                            }
                        }
                    }

                    if (userg.Count != 0)
                    {
                        if (userg[0].Type != "reloader")
                        {
                            if (itemRightOut.Count < nbchoix)
                            {
                                List<ListEpcCount> ko = val.listepccount.ToList();

                                foreach (ListEpcCount lep in ko)
                                {
                                    foreach (epc e in userSelect.Where(t => t.Article_Type_ID == lep.Type))
                                    {
                                        if (!itemRightOut.Contains(e))
                                        {
                                            int m = itemRightOut.Where(x => x.Article_Type_ID == e.Article_Type_ID).Count();

                                            // vérifier que le e.Article_Type_ID n'est pas déjà dans itemNoOut
                                            if (m < lep.Count)
                                            {
                                                if (itemNoOut.Where(x => x.Article_Type_ID == e.Article_Type_ID).Count() < lep.Count - m)
                                                {
                                                    itemNoOut.Add(e);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
#else
        // SANS Modele EDMX
        /// <summary>
        /// Pour les utilisateurs seulement (renseigne itemRightOut, met à jour itemOut et articleTypeSelect, itemOutEpc et itemDiffOut)
        /// </summary>
        /// <param name="itemOut">Entrée : Liste des tags détéctés comme sortis par rapport au dernier SCAN</param>
        /// <param name="itemRightOut">Sortie (Cette liste est mis à jour par la méthode)Liste des tags sortis en conformité avec la commande</param>
        /// <param name="itemWarnOut">Sortie (Cette liste est mis à jour par la méthode) Liste des tags sortis hors conformité avec la commande</param>
        /// <param name="itemNoOut">Sortie (Cette liste est mis à jour par la méthode) Liste des tags non sortis mais qui font partie de la commande</param>
        /// <param name="itemDiffOut">Sortie (Cette liste est mis à jour par la méthode) Liste des tags sortis hors conformité par la taille avec la commande</param>
        /// <param name="articleTypeSelect">Sortie (Cette liste est mis à jour par la méthode) Liste des selections constituant la commande</param>
        private static void MiseAJourListeItemsSortisParRapportALaCommande(List<string> itemOut, List<Epc> alltag, List<Epc> itemRightOut, List<string> itemWarnOut, List<Epc> itemNoOut, List<Epc> itemDiffOut, List<int> articleTypeSelect)
        {
            int nbchoix = 0;

            // Mise à jour du nombre d'articles choisis
            List<ListEpcCount> teml = MainWindow.val.listepccount.ToList();
            foreach (int v in teml.Select(k => k.Type))
            {
                List<int> tulp = teml.Where(k => k.Type == v).Select(k => k.Count).ToList();
                for (int c = 0; c < tulp[0]; c++)
                {
                    nbchoix++;
                }
            }
            Log.add("NbChoix = " + nbchoix.ToString());


            int r = nbchoix;
            foreach (ListEpcCount s in MainWindow.val.listepccount.ToList()) // val.listepccount est alimenté avec les articles sélectionnés (nbchoisi,article_type,taille) 
            {
                List<Epc> ListeItemMemeArticleTaille = MainWindow.val.EpcsTailleType.Where(a => a.Article_Type_ID == s.Type && a.Taille == s.Taille).ToList();
                List<Epc> ListeItemMemeArticle = alltag.Where(a => a.Article_Type_ID == s.Type && ((a.State == EtatArticle.RETIRABLE) || (a.State == EtatArticle.INTRUS_CONNUS))).ToList();

                if (ListeItemMemeArticleTaille.Count != 0)
                {//Si il y avait une possibilité de selection
                    if (itemOut.Any())
                    {
                        List<string> lst = itemOut;
                        for (int i = 0; i < ListeItemMemeArticleTaille.Count; i++)
                        {
                            if (lst.Contains(ListeItemMemeArticleTaille[i].Tag))
                            {
                                if (r > 0)
                                {
                                    itemRightOut.Add(ListeItemMemeArticleTaille[i]);
                                    itemOut.Remove(ListeItemMemeArticleTaille[i].Tag);
                                    Log.add("itemRightOut =" + ListeItemMemeArticleTaille[i].Tag);
                                }
                                r--;
                            }

                        }
                        if (!(articleTypeSelect.Contains(s.Type)))
                            articleTypeSelect.Add(s.Type);
                    }

                }
                if (ListeItemMemeArticle.Count != 0)
                {//Si l'article pris concerne uniquement une différence de taille.
                    if (itemOut.Any())
                    {
                        List<string> lst = itemOut;
                        for (int i = 0; i < ListeItemMemeArticle.Count; i++)
                        {
                            if (lst.Contains(ListeItemMemeArticle[i].Tag))
                            {
                                if (r > 0)
                                {
                                    itemDiffOut.Add(ListeItemMemeArticle[i]);
                                    itemOut.Remove(ListeItemMemeArticle[i].Tag);
                                    Log.add("itemDiffOut =" + ListeItemMemeArticle[i].Tag);
                                }
                                r--;
                            }

                        }
                        if (!(articleTypeSelect.Contains(s.Type)))
                        {
                            articleTypeSelect.Add(s.Type);
                            Log.add("articleTypeSelectList =" + s.Type.ToString());
                        }
                    }

                }
            } // End Foreach
            // Le reste des articles sortis est pris à tort
            //itemWarnOut = itemOut;
            for (int j = 0; j < itemOut.Count; j++) // bug 20131106 vérifier itemOut.Count
            {
                Epc tmp = workerV3.data.getEpc(itemOut[j]);
                if (tmp != null)
                {
                    itemWarnOut.Add(tmp.Tag);
                    Log.add("ItemWarmOut =" + tmp.Tag);
                }
            }

            if (itemRightOut.Count < nbchoix)
            {
                List<ListEpcCount> ko = MainWindow.val.listepccount.ToList();

                foreach (ListEpcCount lep in ko)
                {
                    foreach (Epc e in MainWindow.val.EpcsTailleType.Where(t => t.Article_Type_ID == lep.Type))
                    {
                        if (!itemRightOut.Contains(e))
                        {
                            int m = itemRightOut.Where(x => x.Article_Type_ID == e.Article_Type_ID).Count();

                            // vérifier que le e.Article_Type_ID n'est pas déjà dans itemNoOut
                            if (m < lep.Count)
                            {
                                if (itemNoOut.Where(x => x.Article_Type_ID == e.Article_Type_ID).Count() < lep.Count - m)
                                {
                                    itemNoOut.Add(e);
                                    Log.add("itemNoOut =" + e.Tag);
                                }
                            }
                        }
                    }
                }
            }

        }
#endif

#if EDMX
        // AVEC Modele EDMX
        private void MiseAJourItemUnknown(List<string> itemOut, List<string> itemWarnOut, List<string> itemUnknow, List<epc> alltag, IEnumerable<string> deltaIn)
        {
            //---------------------------------------------------------------------------------
            foreach (string az in itemOut)  // on part du principe que le ménage a été fait avant et on range ce qui reste
            {
                if (!alltag.Select(x => x.Tag).Contains(az))
                {
                    if (deltaIn.Contains(az))
                    {
                        itemUnknow.Add(az);
                    }
                }
                else
                {
                    itemWarnOut.Add(az);
                }
            }
        }
#else
        /* / SANS Modele EDMX
        /// <summary>
        /// Mise à jour de la liste des intrus
        /// </summary>
        /// <param name="itemOutepc">Liste des items sortis</param>
        /// <param name="itemWarnOut">Liste des items sortis sans avoir été demandés</param>
        /// <param name="itemUnknow">Liste des items inconnus</param>
        /// <param name="alltag">Liste de tous les tag de la dotation</param>
        /// <param name="deltaIn">Liste des items entrés</param>
        private void MiseAJourItemUnknown(List<Epc> itemOutepc, List<string> itemWarnOut, List<string> itemUnknow, List<Epc> alltag, IEnumerable<string> deltaIn)
        //private void MiseAJourItemUnknown(List<string> itemOut, List<string> itemWarnOut, List<string> itemUnknow, List<Epc> alltag, IEnumerable<string> deltaIn)
        {
            Log.add("MiseAJourItemUnknown");
            foreach (Epc ep in itemOutepc)  // on part du principe que le ménage a été fait avant et on range ce qui reste
            {
                if (!alltag.Select(x => x.Tag).Contains(ep.Tag))
                {
                    if (deltaIn.Contains(ep.Tag))
                    {
                        itemUnknow.Add(ep.Tag);
                        Log.add("ItemUnknown =" + ep.Tag);
                    }
                }
                else
                {
                    itemWarnOut.Add(ep.Tag);
                    Log.add("ItemWarmOut =" + ep.Tag);
                }
            }
            //---------------------------------------------------------------------------------
           /* foreach (string az in itemOut)  // on part du principe que le ménage a été fait avant et on range ce qui reste
            {
                if (!alltag.Select(x => x.Tag).Contains(az))
                {
                    if (deltaIn.Contains(az))
                    {
                        itemUnknow.Add(az);
                    }
                }
                else
                {
                    itemWarnOut.Add(az);
                }
            }*/
        // }
#endif



#if EDMX
        //AVEC MODELE EDMX
        private void MiseAJourListeIntrus(IEnumerable<string> deltaIn, List<string> tag, List<article_taille> listArtTaille, List<string> itemIntruder, List<string> itemUnknow, List<user> userg)
        {

            if ((deltaIn.Count() != 0) && (log.textBoxID.Text != ""))
            {
                List<user> user;
                user = mainWorker.data.getUserType(log.textBoxID.Text).ToList();
                if (user[0].Type == "reloader")
                {
                    foreach (string value in deltaIn)
                    {
                        if (tag.Contains(value))
                        {
                            //article recharger
                            // le dbContext.epc n'étant pas rafraichi (où le faire??)
                            // on se trouve avec des State epc qui ne sont pas actualisés (état 4 au lieu de 10)
                            // A ETUDIER !!
                            // si le tag est dans le plan de chargement de cette armoire (fonction à implémenter)
                            epc e = mainWorker.data.getEpc(value);
                            if (listArtTaille.Where(x => x.Article_Type_ID == e.Article_Type_ID && x.Taille == e.Taille).Count() > 0)
                            {
                                mainWorker.data.updateEpcReload(100, value, ReaderId, userg[0].Id.ToString());
                            }
                            else // si il ne l'est pas, c'est un intrus connu
                            {
                                var v = value;
                                itemIntruder.Add(v);//Ajoute un tag a list contenant les intrus
                            }

                        }
                        else
                        {
                            var v = value;
                            itemUnknow.Add(v);//Ajoute un tag a list contenant les articles inconnus
                        }
                    }
                }
                else
                {
                    foreach (string value in deltaIn)
                    {
                        if (tag.Contains(value))
                        {
                            var v = value;
                            itemIntruder.Add(v);//Ajoute un tag a list contenant les intrus
                        }
                        else
                        {
                            var v = value;
                            itemUnknow.Add(v);//Ajoute un tag a list contenant les articles inconnus
                        }
                    }
                }
            }

        }
#else

        //SANS MODELE EDMX
        /// <summary>
        /// MiseAJourListeIntrus
        /// </summary>
        /// <param name="deltaIn">Entrée Ecart des Tags nouvellement dans l'armoire</param>
        /// <param name="tag">Dotation sous forme de liste de Tag seulement</param>
        /// <param name="listArtTaille">Liste des Articles Taille</param>
        /// <param name="itemIntruder">Sortie : Liste des intrus connus trouvés</param>
        /// <param name="itemUnknow">Sortie : Liste des intrus inconnus trouvés</param>
        /// <param name="userg"></param>
        private static void MiseAJourListeIntrusReloader(IEnumerable<string> deltaIn, List<string> tag, List<string> itemIntruder, List<string> itemUnknow, string userIdString)
        {
            Log.add("MiseAJourListeIntrusReloader");
            if ((deltaIn.Count() != 0) && ( Login.user!= null))
            {

                foreach (string value in deltaIn)
                {
                    if (tag.Contains(value))
                    {
                        //article rechargé
                        // le dbContext.epc n'étant pas rafraichi (où le faire??)
                        // on se trouve avec des State epc qui ne sont pas actualisés (état 4 au lieu de 10)
                        // A ETUDIER !!
                        // si le tag est dans le plan de chargement de cette armoire (fonction à implémenter)
                        //Epc ep = alltags.Where(x => x.Tag == value).Select(x => x).First();
                        Epc epc = workerV3.data.getEpc(value);
                        if (epc!=null && workerV3.data.isInPlanChargement(epc.Taille, epc.Article_Type_ID, ReaderId) == true)
                        {
                            //if (listArtTaille.Where(x => x.Article_Type_ID == ep.Article_Type_ID && x.Taille == ep.Taille).Count() > 0)
                            //{
                            workerV3.data.updateEpcReload(EtatArticle.RETIRABLE, value, ReaderId, userIdString);
                        }
                        else // si il ne l'est pas, c'est un intrus connu
                        {
                            var v = value;
                            itemIntruder.Add(v);//Ajoute un tag à la liste contenant les intrus
                        }

                    }
                    else
                    {
                        var v = value;
                        itemUnknow.Add(v);//Ajoute un tag à la liste contenant les articles inconnus
                    }
                }

            }

        }

        private static void MiseAJourListeIntrusUser(IEnumerable<string> deltaIn, List<string> tag, List<string> itemIntruder, List<string> itemUnknow)
        {
            Log.add("MiseAJourListeIntrusUser");
            if ((deltaIn.Count() != 0) && ( Login.user!= null))
            {
                // User seulement
                foreach (string value in deltaIn)
                {
                    if (tag.Contains(value))
                    {
                        var v = value;
                        itemIntruder.Add(v);//Ajoute un tag a list contenant les intrus
                    }
                    else
                    {
                        var v = value;
                        itemUnknow.Add(v);//Ajoute un tag a list contenant les articles inconnus
                    }
                }
            }

        }

#endif

#if EDMX
               // AVEC MODELE EDMX
        private bool MiseAJourItemsEntres(List<user> userg, List<epc> alltag, List<epc> itemNoOut, List<epc> itemRightOut, List<epc> itemDiffOut, List<article_type> arttyp)
        {
            bool userwithoutcredit = false;
            int arttypei = 0; // Inutilisé
            //---------------------------------------------------------------------------------
            if (userg[0].Type != "reloader")
            {
                //   item sorti
                if (itemRightOut.Any())
                {
                    List<user_article> credit = mainWorker.data.getUser_articleById(userg[0].Id).ToList();

                    List<epc> li = mainWorker.data.getEPC().ToList();

                    bool epcSortiTailleOK = false;
                    bool epcSortiTailleSup = false;

                    foreach (string s in itemRightOut.Select(x => x.Tag))
                    {
                        //Recupere la taille de l'item sortie
                        List<string> t = li.Where(p => p.Tag == s).Select(g => g.Taille).ToList();
                        List<user_article> cr = new List<user_article>();
                        // tailInf contient la référence de taille inférieure à celle sortie

                        if (t.Count > 0) // si son état est à 100 ou 5 ou 300
                        {
                            string tailInf = workerSelect.data.getTailleInfByOriginalSize(t[0]);
                            //Recupere l'article type id de l'item sorti
                            List<int> idd = li.Where(r => r.Tag == s).Select(v => v.Article_Type_ID).ToList();

                            //recupere la ligne dans user article en fonction de la taille de l'article type id de l'utilisateur en cours
                            mainWorker.data.dbContext.Refresh(System.Data.Objects.RefreshMode.StoreWins, mainWorker.data.dbContext.user_article);

                            cr = credit.Where(m => ((m.Taille == t[0]) || (m.Taille == tailInf)) && m.Article_Type_Id == idd[0]).ToList();
                            epcSortiTailleOK = false;
                            epcSortiTailleSup = false;
                            if (cr.Count != 0)
                            {
                                if (cr[0].Taille == t[0])
                                {
                                    epcSortiTailleOK = true;
                                }
                                if (cr[0].Taille == tailInf)
                                {
                                    epcSortiTailleSup = true;
                                }

                                cr[0].Credit_Restant--;
                                mainWorker.data.UpdateUserArticle(cr[0].Id, cr[0].Credit_Restant);

                                if (cr[0].Credit_Restant <= 0){
                                    userwithoutcredit = true;
                                    arttypei = cr[0].Article_Type_Id;
                                	}
                                try { // tester si le vetement est à la bonne taille
                                    if (epcSortiTailleOK)
                                        mainWorker.data.updateEpc(1, s, log.user_id, Properties.Settings.Default.NumArmoire);
                                    if (epcSortiTailleSup)
                                        mainWorker.data.updateEpc(4, s, log.user_id, Properties.Settings.Default.NumArmoire);
                                } catch (Exception e) {
                                    Log.add("Erreur update items sortis: " + e.Message);
                                    if (Properties.Settings.Default.UseDBGMSG)
                                        MessageBox.Show("Erreur update items sortis: " + e.Message);
                                }
                            }
                        }
                    }
                }

                //   item sorti de taille différente mais de articletype sélectionné
                if (itemDiffOut.Any()) {
                    List<user_article> credit = mainWorker.data.getUser_articleById(userg[0].Id).ToList();

                    List<epc> li = mainWorker.data.getEPC().ToList();

                    foreach (string s in itemDiffOut.Select(x => x.Tag))
                    {
                        //Recupere la taille de l'item sortie
                        List<string> t = li.Where(p => p.Tag == s).Select(g => g.Taille).ToList();
                        List<user_article> cr = new List<user_article>();

                        if (t.Count > 0) // si son état est à 100 ou 5 /*ou 300*/
                        {
                            //Recupere l'article type id de l'item sorti
                            List<int> idd = li.Where(r => r.Tag == s).Select(v => v.Article_Type_ID).ToList();

                            //objectif : recupere la ligne dans user article en fonction de la taille de l'article type id de l'utilisateur en cours

                            mainWorker.data.dbContext.Refresh(System.Data.Objects.RefreshMode.StoreWins, mainWorker.data.dbContext.user_article);

                            // on ne tient plus compte de la taille dans ce contexte
                            cr = credit.Where(m => m.Article_Type_Id == idd[0]).ToList();
                            if (cr.Count != 0)
                            {
                                cr[0].Credit_Restant--;
                                try
                                {
                                    mainWorker.data.UpdateUserArticle(cr[0].Id, cr[0].Credit_Restant);
                                }
                                catch (Exception e)
                                {
                                    Log.add("updateListBox : exeption à l'appel de UpdateUserArticle : " + e.Message);
                                }

                                if (cr[0].Credit_Restant <= 0)
                                {
                                    userwithoutcredit = true;
                                    arttypei = cr[0].Article_Type_Id;
                                }



                                try
                                {
                                    mainWorker.data.updateEpc(4, s, log.user_id, Properties.Settings.Default.NumArmoire);
                                }
                                catch (Exception e)
                                {
                                    Log.add("Erreur update items sortis: " + e.Message);
                                    if (Properties.Settings.Default.UseDBGMSG)
                                        MessageBox.Show("Erreur update items sortis: " + e.Message);
                                }

                                Log.add("updateListBox : fin décrémentation de crédit restant qui est à " + cr[0].Credit_Restant);

                            }
                        }
                    }
                }

                if (itemNoOut.Any())
                {
                    string message = itemNoOut.Count + ((itemNoOut.Count > 1) ? " articles commandés non retirés par rapport à la commande." : " article commandé non retiré par rapport à la commande.");

                    //Insere alert dans bdd
                    int id = mainWorker.data.insertAlert(9, userg[0].Id, Properties.Settings.Default.NumArmoire, message);
                    // syn.mailAlert(false);
                    //        syn.alert();
                    //insere dans tag alert la liste des tag en alert en fonction de l'id de l'alerte au dessus
                    foreach (string s in itemNoOut.Select(w => w.Tag))
                    {
                        List<int> uy = (alltag.Where(o => o.Tag == s).Select(d => d.Article_Type_ID)).ToList();
                        List<string> ta = alltag.Where(o => o.Tag == s).Select(d => d.Taille).ToList();
                        List<string> typ = arttyp.Where(m => m.Id == (uy[0])).Select(w => w.Code).ToList();
                        try
                        {
                            mainWorker.data.insertTagAlert(id, s, "", "", typ[0], ta[0]);
                        }
                        catch (Exception ex)
                        {
                            Log.add("Erreur insertTagAlert: " + ex.Message);
                            if (Properties.Settings.Default.UseDBGMSG)
                                MessageBox.Show("Erreur insertTagAlert: " + ex.Message);
                        }
                    }
                    synerr();
                }
            }
            return (userwithoutcredit);
        }
#else
        // AVEC MODELE EDMX
        private static bool MiseAJourItemsEntres(int userid, List<string> itemWarnOut, List<Epc> alltag, List<User_Article> credit, List<Epc> itemNoOut, List<Epc> itemRightOut, List<Epc> itemDiffOut, List<Article_Type> arttyp, List<int> arttypeidpuise)
        {
            Log.add("MiseAJourItemsEntres");
            Log.add("ItemRightOut =" + itemRightOut.Count.ToString() + " nb itemWarnOut = " + itemWarnOut.Count().ToString() + " itemNoOut=" + itemNoOut.Count.ToString() + " itemDiffOut=" + itemDiffOut.Count.ToString());
            bool userwithoutcredit = false;
            int nbchangeinaweek = 0;
            //bool simpleSizeDiff = false;
            bool respectRegleDeChange = true;
            List<int> ArticleTypeIdRegleChangeNonRespecte = new List<int>();
            List<Article_TailleDiff> ArTailDiff = new List<Article_TailleDiff>();

            int arttypei = 0; // Inutilisé
            //---------------------------------------------------------------------------------
            try
            {
                //   item sorti
                if (itemRightOut.Any())
                {
                    Log.add("Traitement des  " + itemRightOut.Count + "articles sorties conformément à la commande");
                    //List<User_Article> credit = workerV3.data.getUser_articleByUserId(userid).ToList();

                    List<Epc> li = workerV3.data.getEPC().ToList();


                    bool epcSortiTailleOK = false;
                    bool epcSortiTailleSup = false;

                    foreach (string s in itemRightOut.Select(x => x.Tag))
                    {

                        //Recupere la taille de l'item sorti
                        List<string> t = li.Where(p => p.Tag == s).Select(g => g.Taille).ToList();
                        List<User_Article> cr = new List<User_Article>();
                        // tailInf contient la référence de taille inférieure à celle sortie

                        if (t.Count > 0) // si son état est à 100 ou 5 ou 300
                        { //??? Pas 300 !!!

                            string tailInf = MainWindow.workerSelectV3.data.getTailleInfByOriginalSize(t[0]);
                            //Recupere l'article type id de l'item sorti
                            List<int> idd = li.Where(r => r.Tag == s).Select(v => v.Article_Type_ID).ToList();

                            //recupere la ligne dans user article en fonction de la taille de l'article type id de l'utilisateur en cours
#if EDMX
                            workerV3.data.dbContext.Refresh(System.Data.Objects.RefreshMode.StoreWins, workerV3.data.dbContext.user_article);
#endif

                            cr = credit.Where(m => ((m.Taille == t[0]) || (m.Taille == tailInf)) && m.Article_Type_Id == idd[0]).ToList();
                            epcSortiTailleOK = false;
                            epcSortiTailleSup = false;
                            if (cr.Count != 0)
                            {
                                if (cr[0].Taille == t[0]) epcSortiTailleOK = true;
                                if (cr[0].Taille == tailInf) epcSortiTailleSup = true;
                                cr[0].Credit_Restant--;
                                if (cr[0].Credit_Restant < 0)
                                {
                                    respectRegleDeChange = false;
                                    Properties.Settings.Default.ReglesDeChange = true;
                                    ArticleTypeIdRegleChangeNonRespecte.Add(cr[0].Article_Type_Id);
                                    Log.add("non respect de la regle de change");
                                }
                                try { workerV3.data.UpdateUserArticle(cr[0].Id, cr[0].Credit_Restant); }
                                catch (Exception e)
                                {
                                    Log.add("Erreur update crédit: " + e.Message);
                                    if (Properties.Settings.Default.UseDBGMSG)
                                        MessageBox.Show("Erreur update items sortis: " + e.Message);
                                }
                                if (cr[0].Credit_Restant <= 0)
                                {
                                    userwithoutcredit = true;
                                    arttypei = cr[0].Article_Type_Id;
                                    arttypeidpuise.Add(arttypei);
                                }
                                try
                                {  // tester si le vetement est à la bonne taille
                                    if (epcSortiTailleOK)
                                        workerV3.data.updateEpc(1, s, Login.user.Id.ToString(), Properties.Settings.Default.NumArmoire);
                                    if (epcSortiTailleSup)
                                        workerV3.data.updateEpc(4, s,Login.user.Id.ToString(), Properties.Settings.Default.NumArmoire);
                                }
                                catch (Exception e)
                                {
                                    Log.add("Erreur update items sortis: " + e.Message);
                                    if (Properties.Settings.Default.UseDBGMSG)
                                        MessageBox.Show("Erreur update items sortis: " + e.Message);
                                }
                                if (Properties.Settings.Default.ReglesDeChange == true)
                                {
                                    // La gestion de la régle de change est valable à partir du lundi
                                    nbchangeinaweek = workerV3.data.GetNumberOfArticleTypeInLogEpcSinceMonday(cr[0].Article_Type_Id, cr[0].User_Id) + 1;
                                    Log.add("nbchangeinaweek =" + nbchangeinaweek.ToString() + " Crédit = " + cr[0].Credit.ToString());
                                    // Pour la gestion des regles de changes:
                                    //test si les articles pris depuis une semaine dépassent le nombre de crédit initial attribués
                                    // SELECT * FROM `log_epc` WHERE Article_Type_Id=idd[0] AND Date_Creation >=  BETWEEN DATEDIFF(NOW() - 30 days) AND NOW();
                                    //int nbchangeinaweek = workerV3.data.GetNumberOfArticleTypeInLogEpcForLast7days(cr[0].Article_Type_Id, cr[0].User_Id);
                                    if (nbchangeinaweek > cr[0].Credit)
                                    {
                                        respectRegleDeChange = false;
                                        ArticleTypeIdRegleChangeNonRespecte.Add(cr[0].Article_Type_Id);
                                        Log.add("non respect de la regle de change");
                                    }
                                }

                            }
                        }
                    }
                }
                //   item sorti de taille différente mais de articletype sélectionné
                if (itemDiffOut.Any())
                {
                    Log.add("Traitement des " + itemDiffOut.Count + " articles sorties de taille différente mais d'un article_type du trousseau");
                    //List<User_Article> credit = workerV3.data.getUser_articleByUserId(userid).ToList();

                    List<Epc> li = workerV3.data.getEPC().ToList();

                    foreach (string s in itemDiffOut.Select(x => x.Tag))
                    {
                        //Recupere la taille de l'item sortie
                        List<string> t = li.Where(p => p.Tag == s).Select(g => g.Taille).ToList();
                        List<User_Article> cr = new List<User_Article>();

                        if (t.Count > 0) // si son état est à 100 ou 5 /*ou 300*/
                        {
                            //Recupere l'article type id de l'item sorti
                            List<int> idd = li.Where(r => r.Tag == s).Select(v => v.Article_Type_ID).ToList();

                            //objectif : recupere la ligne dans user article en fonction de la taille de l'article type id de l'utilisateur en cours

#if EDMX
                            workerV3.data.dbContext.Refresh(System.Data.Objects.RefreshMode.StoreWins, workerV3.data.dbContext.user_article);
#endif
                            // on ne tient plus compte de la taille dans ce contexte
                            cr = credit.Where(m => m.Article_Type_Id == idd[0]).ToList();
                            if (cr.Count != 0)
                            {
                                cr[0].Credit_Restant--;
                                if (cr[0].Credit_Restant < 0)
                                {
                                    respectRegleDeChange = false;
                                    Properties.Settings.Default.ReglesDeChange = true;
                                    ArticleTypeIdRegleChangeNonRespecte.Add(cr[0].Article_Type_Id);
                                    Log.add("non respect de la regle de change");
                                }
                                try { workerV3.data.UpdateUserArticle(cr[0].Id, cr[0].Credit_Restant); }
                                catch (Exception e) { Log.add("updateListBox : exeption à l'appel de UpdateUserArticle : " + e.Message); }
                                if (cr[0].Credit_Restant <= 0)
                                {
                                    userwithoutcredit = true;
                                    arttypei = cr[0].Article_Type_Id;
                                }
                                try { workerV3.data.updateEpc(4, s, Login.user.Id.ToString(), Properties.Settings.Default.NumArmoire); }
                                catch (Exception e)
                                {
                                    Log.add("Erreur update items sortis: " + e.Message);
                                    if (Properties.Settings.Default.UseDBGMSG)
                                        MessageBox.Show("Erreur update items sortis: " + e.Message);
                                }
                                Log.add("updateListBox : fin décrémentation de crédit restant qui est à " + cr[0].Credit_Restant);
                                if (Properties.Settings.Default.ReglesDeChange == true)
                                {
                                    // Pour la gestion des regles de changes:
                                    //test si les articles pris depuis une semaine dépassent le nombre de crédit initial attribués
                                    // SELECT * FROM `log_epc` WHERE Article_Type_Id=idd[0] AND Date_Creation >=  BETWEEN DATEDIFF(NOW() - 30 days) AND NOW();
                                    nbchangeinaweek = nbchangeinaweek > 0 ?  nbchangeinaweek + 1 : workerV3.data.GetNumberOfArticleTypeInLogEpcSinceMonday(cr[0].Article_Type_Id, cr[0].User_Id) + 1;
                                    Log.add("nbchangeinaweek =" + nbchangeinaweek.ToString() + " Crédit = " + cr[0].Credit.ToString());
                                    //int nbchangeinaweek = workerV3.data.GetNumberOfArticleTypeInLogEpcForLast7days(cr[0].Article_Type_Id, cr[0].User_Id);
                                    if (nbchangeinaweek > cr[0].Credit)
                                    {
                                        respectRegleDeChange = false;
                                        ArticleTypeIdRegleChangeNonRespecte.Add(cr[0].Article_Type_Id);
                                        Log.add("non respect de la regle de change");
                                    }
                                }
                            }
                        }
                    }
                }

                List<Epc> lewo = new List<Epc>();
                foreach (string iwo in itemWarnOut) lewo.Add(alltag.Where(x => (x.Tag == iwo)).First());
                if (itemNoOut.Any())
                //if (itemNoOut.Count == itemWarnOut.Count)
                {// Vérification si c'est une simple différence de taille
                    Log.add("Verification du cas simple erreur de taille");

                    //simpleSizeDiff = true;
                    List<int> idArtTypIDRet = new List<int>();
                    List<int> nbArtTypRet = new List<int>();
                    for (int i = itemNoOut.Count - 1; i >= 0; i--)
                    {
                        Epc ino = itemNoOut[i];
                        //foreach (Epc ino in itemNoOut)
                        //{
                        if (itemDiffOut.Where(x => x.Article_Type_ID == ino.Article_Type_ID).Count() > 0)
                        {
                            string Tailret = "";
                            try
                            {
                                Tailret = itemDiffOut.Where(x => x.Article_Type_ID == ino.Article_Type_ID).Select(x => x.Taille).First();
                            }
                            catch (Exception exp)
                            {
                                Log.add("Tailretiree Err: " + exp.Message);
                            }
                            //Même type trouvé
                            if (Tailret != ino.Taille)
                            {
                                Log.add("Demandé=" + ino.Taille + "Retire=" + Tailret + "Type=" + ino.Article_Type_ID.ToString());
                                Article_TailleDiff temp = new Article_TailleDiff();
                                temp.Article_Type_ID = ino.Article_Type_ID;
                                temp.Taille_Commandee = ino.Taille;
                                temp.Taille_Retiree = Tailret;
                                temp.Article_Code = arttyp.Where(x => x.Id == ino.Article_Type_ID).Select(x => x.Code).First();
                                ArTailDiff.Add(temp);
                                idArtTypIDRet.Add(ino.Article_Type_ID);
                            }

                        }
                        else
                        //}
                        //foreach (Epc ino in itemNoOut)
                        //{
                        {
                            List<int> listIdEpc = new List<int>();

                            if (lewo.Where(x => x.Article_Type_ID == ino.Article_Type_ID).Count() > 0)
                            {
                                string Tailret = "";
                                try
                                {
                                    Tailret = lewo.Where(x => x.Article_Type_ID == ino.Article_Type_ID).Select(x => x.Taille).First();
                                }
                                catch (Exception exp)
                                {
                                    Log.add("Tailretiree Err: " + exp.Message);
                                }
                                //Même type trouvé
                                if (Tailret != ino.Taille)
                                {
                                    Log.add("Demandé=" + ino.Taille + "Retire=" + Tailret + "Type=" + ino.Article_Type_ID.ToString());
                                    Article_TailleDiff temp = new Article_TailleDiff();
                                    temp.Article_Type_ID = ino.Article_Type_ID;
                                    temp.Taille_Commandee = ino.Taille;
                                    temp.Taille_Retiree = Tailret;
                                    temp.Article_Code = arttyp.Where(x => x.Id == ino.Article_Type_ID).Select(x => x.Code).First();
                                    ArTailDiff.Add(temp);
                                    try
                                    {

                                    }
                                    catch (Exception e)
                                    {
                                        if (Properties.Settings.Default.UseDBGMSG == true)
                                            MessageBox.Show("Erreur MiseAJourItemsEntres : " + e.Message);
                                    }
                                }
                            }
                            //else
                            //{
                            //    simpleSizeDiff = false;
                            //}
                        }
                    }
                    // Correctif
                    for (int i = itemDiffOut.Count - 1; i >= 0; i--)
                    {
                        if (itemNoOut.Where(x => x.Article_Type_ID == itemDiffOut[i].Article_Type_ID).FirstOrDefault() != null)
                        {
                            itemNoOut.Remove(itemNoOut.Where(x => x.Article_Type_ID == itemDiffOut[i].Article_Type_ID).FirstOrDefault());
                        }
                    }
                    if (/*(simpleSizeDiff == true) &&*/ (ArTailDiff.Count > 0))
                    {
                        string message = "Retrait de la mauvaise taille";
                        int al_id = workerV3.data.insertAlert(AlerteType.ALERTE_TAILLE, userid, ReaderId, message);
                        foreach (Article_TailleDiff temp in ArTailDiff)
                        {
                            workerV3.data.insertTagAlert(al_id, "AAAABBBBAAAABBBB", "", "", temp.Article_Code, temp.Taille_Commandee);
                            workerV3.data.insertTagAlert(al_id, "", "AAAABBBBAAAABBBB", "", temp.Article_Code, temp.Taille_Retiree);
                            itemNoOut.Remove(itemNoOut.Where(z => z.Article_Type_ID == temp.Article_Type_ID).FirstOrDefault());
                        }
                    }
                }
                //if (simpleSizeDiff == false)
                if (lewo.Any())
                {
                    if (itemWarnOut.Any())
                    {
                        Log.add("Cas des " + lewo.Count + " Articles pris à tort");
                        string message = lewo.Count + ((lewo.Count > 1) ? " articles retirés en excès par rapport à la commande." : " article retiré en excès par rapport à la commande.");

                        //Insere alert dans bdd
                        int al_id = workerV3.data.insertAlert(AlerteType.ALERTE_EXCESSIF, userid, ReaderId, message);

                        //insere les informations relatives à la commande
                        List<ListEpcCount> epcommandlist = MainWindow.val.listepccount.ToList();
                        foreach (ListEpcCount lec in epcommandlist)
                        {
                            for (int i = 0; i < lec.Count; i++)
                            {
                                string codearticle = arttyp.Where(x => x.Id == lec.Type).Select(x => x.Code).First();
                                workerV3.data.insertTagAlert(al_id, "AAAABBBBAAAABBBBAAAA", "", "", codearticle, lec.Taille);
                            }
                        }

                        //insere tout les tag pris correctement dans la table tag_alert
                        try
                        {
                            foreach (Epc e in itemRightOut)
                            {
                                workerV3.data.insertTagAlert(al_id, "", e.Tag, "", arttyp.Where(m => m.Id == e.Article_Type_ID).Select(w => w.Code).FirstOrDefault(), e.Taille);
                            }
                        }
                        catch
                        {
                            Log.add("Erreur insertTagAlert pour itemRightOut");
                        }

                        //insere dans tag alert la liste des tag en alert en fonction de l'id de l'alerte au dessus
                        foreach (string s in itemWarnOut)
                        {
                            if (lewo.Where(x => x.Tag == s).Count() > 0)
                            {
                                List<int> uy = (alltag.Where(o => o.Tag == s).Select(d => d.Article_Type_ID)).ToList();
                                List<string> ta = alltag.Where(o => o.Tag == s).Select(d => d.Taille).ToList();
                                List<string> typ = arttyp.Where(m => m.Id == uy[0]).Select(w => w.Code).ToList();
                                int artcod = arttyp.Where(m => m.Id == uy[0]).Select(w => w.Id).FirstOrDefault();
                                List<User_Article> ua = workerV3.data.getUser_articleByUserId(userid).ToList(); // on récupère les infos du trousseau

#if EDMX
                            workerV3.data.dbContext.Refresh(System.Data.Objects.RefreshMode.StoreWins, workerV3.data.dbContext.user_article);
#endif
                                try
                                {
                                    User_Article uart = ua.Where(w => w.Article_Type_Id == artcod && w.Taille == ta[0]).FirstOrDefault(); // on tient compte de la taille
                                    User_Article uartdiffout = ua.Where(w => w.Article_Type_Id == artcod && w.Taille != ta[0]).FirstOrDefault();
                                    // déjà fait dans misajourdesentrees ??
                                    int etat = 1;
                                    if (uart != null)
                                    {
                                        uart.Credit_Restant--;
                                        if (uart.Credit_Restant < 0) {
                                            respectRegleDeChange = false;
                                            Properties.Settings.Default.ReglesDeChange = true;
                                            ArticleTypeIdRegleChangeNonRespecte.Add(uart.Article_Type_Id);
                                            Log.add("non respect de la regle de change");
                                        }
                                        workerV3.data.UpdateUserArticle(uart.Id, uart.Credit_Restant);
                                        etat = 1;
                                    }
                                    if (uart == null && uartdiffout != null)
                                    {
                                        uartdiffout.Credit_Restant--;
                                        if (uartdiffout.Credit_Restant < 0)
                                        {
                                            respectRegleDeChange = false;
                                            Properties.Settings.Default.ReglesDeChange = true;
                                            ArticleTypeIdRegleChangeNonRespecte.Add(uartdiffout.Article_Type_Id);
                                            Log.add("non respect de la regle de change");
                                        }
                                        workerV3.data.UpdateUserArticle(uartdiffout.Id, uartdiffout.Credit_Restant);
                                        etat = 4;
                                    }

                                    workerV3.data.insertTagAlert(al_id, "", s, "", typ[0], ta[0]);

                                    workerV3.data.updateEpc(etat, s, Login.user.Id.ToString(), ReaderId);

                                    if (alltag.Where(x => x.Tag == s).FirstOrDefault() != null)
                                    {
                                        itemNoOut.Remove(itemNoOut.Where(x => x.Article_Type_ID == alltag.Where(y => y.Tag == s).FirstOrDefault().Article_Type_ID).FirstOrDefault());
                                    }

                                }
                                catch (Exception e)
                                {
                                    Log.add("Erreur update alertes: " + e.Message);
                                    if (Properties.Settings.Default.UseDBGMSG)
                                        MessageBox.Show("Erreur update alertes: " + e.Message);
                                }
                            }
                        }
                        MainWindow.synerr();
                    }
                }
                else
                {
                    // On ne traite le cas des items non pris que si il n'y a pas d'item pris en trop
                    if (itemNoOut.Any())
                    {
                        Log.add("Cas des " + itemNoOut.Count + "Articles non retirés");
                        string message = itemNoOut.Count + ((itemNoOut.Count > 1) ? " articles commandés non retirés par rapport à la commande." : " article commandé non retiré par rapport à la commande.");

                        //Insere alert dans bdd
                        int id = workerV3.data.insertAlert(AlerteType.ALERTE_INCOMPLET, userid, ReaderId, message);
                        // syn.mailAlert(false);
                        //        syn.alert();
                        //insere dans tag alert la liste des tag en alert en fonction de l'id de l'alerte au dessus
                        foreach (string s in itemNoOut.Select(w => w.Tag))
                        {
                            List<int> uy = (alltag.Where(o => o.Tag == s).Select(d => d.Article_Type_ID)).ToList();
                            List<string> ta = alltag.Where(o => o.Tag == s).Select(d => d.Taille).ToList();
                            List<string> typ = arttyp.Where(m => m.Id == (uy[0])).Select(w => w.Code).ToList();
                            try { workerV3.data.insertTagAlert(id, s, "", "", typ[0], ta[0]); }
                            catch (Exception ex)
                            {
                                Log.add("Erreur insertTagAlert: " + ex.Message);
                                if (Properties.Settings.Default.UseDBGMSG)
                                    MessageBox.Show("Erreur insertTagAlert: " + ex.Message);
                            }
                        }
                        MainWindow.synerr();
                    }


                }

                // --------------------------------------------------------------------------------------
                // Il est nécessaire de se baser sur une liste de crédits à jour pour traiter les règles 
                // de change : L'implementation précédente prenait en compte les crédits restants de la session 
                // précédente. Il en resultait que la règle des changes ne marchait pas lors du premier dépassement.
                // (comparaison CreditAttribué = 5 avec Crédit précédent = 5 quelque soit le nombre d'article que 
                // l'on ait pu prendre dans la session courante.
                // L'autre défaut est de traiter séparément les types d'articles suivant que le retrait était légitime
                // (article demandé et retiré) ou illegitime (article non demandé). Conséquence, la comparaison 
                // se faisait par liste et pour l'exemple précédent la comparaison ne pouvait pas se faire.
                // C'est pourquoi il faut 
                // --------------------------------------------------------------------------------------

                if (respectRegleDeChange == false && Properties.Settings.Default.ReglesDeChange == true)
                {
                    string message = "Le porteur a dépassé ses règles de change.";
                    Log.add(message);
                    //Insere alert dans bdd
                    int id = workerV3.data.insertAlert(AlerteType.ALERTE_REGLECHANGEMAX, userid, ReaderId, message);
                    // syn.mailAlert(false);
                    //        syn.alert();
                    //insere dans tag alert la liste des tag en alert en fonction de l'id de l'alerte au dessus

                    foreach (int s in ArticleTypeIdRegleChangeNonRespecte.Distinct())
                    {
                        try
                        {
                            List<string> typ = arttyp.Where(m => m.Id == s).Select(w => w.Code).ToList();
                            workerV3.data.insertTagAlert(id, "", "", "", typ[0], "");
                        }
                        catch (Exception ex)
                        {
                            Log.add("Erreur insertTagAlert: " + ex.Message);
                            if (Properties.Settings.Default.UseDBGMSG)
                                MessageBox.Show("Erreur insertTagAlert: " + ex.Message);
                        }
                    }
                    MainWindow.synerr();

                }
            }
            catch (Exception ex)
            {
                Log.add("MiseAJourAlerteMalPercu: " + ex.Message);
            }

            return (userwithoutcredit);
        }
#endif

#if EDMX
        // AVEC MODELE EDMX
        private void MiseAJourTropPercu(List<user> userg, List<string> itemWarnOut, List<epc> alltag, List<article_type> arttyp)
        {
            //---------------------------------------------------------------------------------
            if (userg.Count != 0)
                if (userg[0].Type != "reloader")
                {
                    if (itemWarnOut.Any())
                    {
                        string message = itemWarnOut.Count + ((itemWarnOut.Count > 1) ? " articles retirés en excès par rapport à la commande." : " article retiré en excès par rapport à la commande.");

                        //Insere alert dans bdd
                        int id = mainWorker.data.insertAlert(10, userg[0].Id, Properties.Settings.Default.NumArmoire, message);

                        //insere dans tag alert la liste des tag en alert en fonction de l'id de l'alerte au dessus
                        foreach (string s in itemWarnOut)
                        {
                            List<int> uy = (alltag.Where(o => o.Tag == s).Select(d => d.Article_Type_ID)).ToList();
                            List<string> ta = alltag.Where(o => o.Tag == s).Select(d => d.Taille).ToList();
                            List<string> typ = arttyp.Where(m => m.Id == uy[0]).Select(w => w.Code).ToList();

                            int artcod = arttyp.Where(m => m.Id == uy[0]).Select(w => w.Id).FirstOrDefault();
                            List<user_article> ua = mainWorker.data.getUser_articleById(userg[0].Id).ToList(); // on récupère les infos du trousseau

        
                            mainWorker.data.dbContext.Refresh(System.Data.Objects.RefreshMode.StoreWins, mainWorker.data.dbContext.user_article);
                            user_article uart = ua.Where(w => w.Article_Type_Id == artcod && w.Taille == ta[0]).FirstOrDefault(); // on tient compte de la taille
                            user_article uartdiffout = ua.Where(w => w.Article_Type_Id == artcod && w.Taille != ta[0]).FirstOrDefault();
                            try
                            {
                                int etat = 1;
                                if (uart != null)
                                {
                                    mainWorker.data.UpdateUserArticle(uart.Id, uart.Credit_Restant - 1);
                                    etat = 1;
                                }
                                if (uart == null && uartdiffout != null)
                                {
                                    mainWorker.data.UpdateUserArticle(uartdiffout.Id, uartdiffout.Credit_Restant - 1);
                                    etat = 4;
                                }
                                mainWorker.data.insertTagAlert(id, "", s, "", typ[0], ta[0]);
                                mainWorker.data.updateEpc(etat, s, log.user_id, Properties.Settings.Default.NumArmoire);
                            }
                            catch (Exception e)
                            {
                                Log.add("Erreur update alertes: " + e.Message);
                                if (Properties.Settings.Default.UseDBGMSG)
                                    MessageBox.Show("Erreur update alertes: " + e.Message);
                            }
                        }
                        synerr();
                    }
                }
        }
#else
        // SANS MODELE EDMX
        private void MiseAJourAlerteMalPercu(int userid, List<string> itemWarnOut, List<Epc> alltag, List<User_Article> ua, List<Epc> itemNoOut, List<Epc> itemDiffOut, List<Article_Type> arttyp)
        {
            try
            {
                Log.add("MiseAJourAlerteMalPercu");
                //---------------------------------------------------------------------------------
                /* if (userg.Count != 0)
                     if (userg[0].Type != "reloader")
                     {*/
                Log.add("nb itemNoOut = " + itemNoOut.Count().ToString() + " nb itemWarnOut = " + itemWarnOut.Count().ToString() + " nb itemDiffOut = " + itemDiffOut.Count().ToString());
                bool simpleSizeDiff = false;
                List<Article_TailleDiff> ArTailDiff = new List<Article_TailleDiff>();
                if (itemNoOut.Count == itemWarnOut.Count)
                {// Vérification si c'est une simple différence de taille
                    simpleSizeDiff = true;
                    List<Epc> le = new List<Epc>();
                    foreach (string iwo in itemWarnOut)
                    {
                        le.Add(alltag.Where(x => (x.Tag == iwo)).First());
                    }
                    foreach (Epc ino in itemNoOut)
                    {
                        if (le.Where(x => x.Article_Type_ID == ino.Article_Type_ID).Count() > 0)
                        {
                            string Tailret = "";
                            try
                            {
                                Tailret = le.Where(x => x.Article_Type_ID == ino.Article_Type_ID).Select(x => x.Taille).First();
                            }
                            catch (Exception exp)
                            {
                                Log.add("Tailretiree Err: " + exp.Message);
                            }
                            //Même type trouvé
                            if (Tailret != ino.Taille)
                            {
                                Log.add("Demandé=" + ino.Taille + "Retire=" + Tailret + "Type=" + ino.Article_Type_ID.ToString());
                                Article_TailleDiff temp = new Article_TailleDiff();
                                temp.Article_Type_ID = ino.Article_Type_ID;
                                temp.Taille_Commandee = ino.Taille;
                                temp.Taille_Retiree = Tailret;
                                temp.Article_Code = arttyp.Where(x => x.Id == ino.Article_Type_ID).Select(x => x.Code).First();
                                ArTailDiff.Add(temp);
                            }
                        }
                        else
                        {
                            simpleSizeDiff = false;
                        }
                    }

                    if ((simpleSizeDiff == true) && (ArTailDiff.Count > 0))
                    {
                        string message = "Retrait de la mauvaise taille";
                        int al_id = workerV3.data.insertAlert(AlerteType.ALERTE_TAILLE, userid, ReaderId, message);
                        foreach (Article_TailleDiff temp in ArTailDiff)
                        {
                            workerV3.data.insertTagAlert(al_id, "AAAABBBBAAAABBBB", "", "", temp.Article_Code, temp.Taille_Commandee);
                            workerV3.data.insertTagAlert(al_id, "", "AAAABBBBAAAABBBB", "", temp.Article_Code, temp.Taille_Retiree);
                        }
                    }
                }
                if (simpleSizeDiff == false)
                {
                    if (itemWarnOut.Any())
                    {
                        string message = itemWarnOut.Count + ((itemWarnOut.Count > 1) ? " articles retirés en excès par rapport à la commande." : " article retiré en excès par rapport à la commande.");

                        //Insere alert dans bdd
                        int al_id = workerV3.data.insertAlert(AlerteType.ALERTE_EXCESSIF, userid, ReaderId, message);

                        //insere les informations relatives à la commande
                        List<ListEpcCount> epcommandlist = MainWindow.val.listepccount.ToList();
                        foreach (ListEpcCount lec in epcommandlist)
                        {
                            for (int i = 0; i < lec.Count; i++)
                            {
                                string codearticle = arttyp.Where(x => x.Id == lec.Type).Select(x => x.Code).First();
                                workerV3.data.insertTagAlert(al_id, "AAAABBBBAAAABBBBAAAA", "", "", codearticle, lec.Taille);
                            }
                        }
                        //insere dans tag alert la liste des tag en alert en fonction de l'id de l'alerte au dessus
                        foreach (string s in itemWarnOut)
                        {
                            List<int> uy = (alltag.Where(o => o.Tag == s).Select(d => d.Article_Type_ID)).ToList();
                            List<string> ta = alltag.Where(o => o.Tag == s).Select(d => d.Taille).ToList();
                            List<string> typ = arttyp.Where(m => m.Id == uy[0]).Select(w => w.Code).ToList();
                            try
                            {
                                workerV3.data.insertTagAlert(al_id, "", s, "", typ[0], ta[0]);
                            }
                            catch (Exception e)
                            {
                                Log.add("Erreur update alertes: " + e.Message);
                                if (Properties.Settings.Default.UseDBGMSG)
                                    MessageBox.Show("Erreur update alertes: " + e.Message);
                            }
                        }
                        MainWindow.synerr();
                    }
                    else
                    {
                        // On ne traite le cas des items non pris que si il n'y a pas d'item pris en trop
                        if (itemNoOut.Any())
                        {
                            string message = itemNoOut.Count + ((itemNoOut.Count > 1) ? " articles commandés non retirés par rapport à la commande." : " article commandé non retiré par rapport à la commande.");

                            //Insere alert dans bdd
                            int id = workerV3.data.insertAlert(AlerteType.ALERTE_INCOMPLET, userid, ReaderId, message);
                            // syn.mailAlert(false);
                            //        syn.alert();
                            //insere dans tag alert la liste des tag en alert en fonction de l'id de l'alerte au dessus
                            foreach (string s in itemNoOut.Select(w => w.Tag))
                            {
                                List<int> uy = (alltag.Where(o => o.Tag == s).Select(d => d.Article_Type_ID)).ToList();
                                List<string> ta = alltag.Where(o => o.Tag == s).Select(d => d.Taille).ToList();
                                List<string> typ = arttyp.Where(m => m.Id == (uy[0])).Select(w => w.Code).ToList();
                                try
                                {
                                    workerV3.data.insertTagAlert(id, s, "", "", typ[0], ta[0]);
                                }
                                catch (Exception ex)
                                {
                                    Log.add("Erreur insertTagAlert: " + ex.Message);
                                    if (Properties.Settings.Default.UseDBGMSG)
                                        MessageBox.Show("Erreur insertTagAlert: " + ex.Message);
                                }
                            }
                            MainWindow.synerr();
                        }
                        else
                        {
                            // As t'on pris une mauvaise taille
                            //insere les informations relatives à la commande
                            if (itemDiffOut.Count() > 0)
                            {
                                /*
                                //Insere alert dans bdd
                                string message = "Retrait de la mauvaise taille";
                                int id = workerV3.data.insertAlert(AlertId.ALERTE_TAILLE, userid, ReaderId, message);

                                List<ListEpcCount> epcommandlist = val.listepccount.ToList();
                                foreach (ListEpcCount lec in epcommandlist)
                                {
                                    string strTailleRetiree = itemDiffOut.Where(x => (x.Article_Type_ID == lec.Type)).Select(x => x.Taille).First();
                                    string strTailleDemandee = lec.Taille;
                                    string codearticle = arttyp.Where(x => x.Id == lec.Type).Select(x => x.Code).First();
                                        //workerV3.data.insertTagAlert(al_id, "AAAABBBBAAAABBBB", "", "", codearticle, lec.Taille);
                            
                                }
                                */

                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Log.add("MiseAJourAlerteMalPercu: " + ex.Message);
            }
            /*  }*/
        }
#endif

#if EDMX
        // AVEC MODELE EDMX
        private void MiseAJourItemMalPlaces(List<string> itemIntruder, List<article_type> arttyp, List<user> userg)
        {
            //---------------------------------------------------------------------------------
            //item intrus connu
            if (itemIntruder.Any())
            {
                string message = itemIntruder.Count + ((itemIntruder.Count > 1) ? " intrus connus détectés" : " intrus connu détecté");

                //Insere alert dans bdd
                int idAlertType = userg[0].Type == "reloader" ? 7 : 15;
                int id = mainWorker.data.insertAlert(idAlertType, userg[0].Id, Properties.Settings.Default.NumArmoire, message);

                List<user_article> credit = mainWorker.data.getUser_articleById(userg[0].Id).ToList();
                List<epc> li = mainWorker.data.getAllEPC().ToList();
                //insere dans tag alert la liste des tag en alert en fonction de l'id de l'alerte au dessus
                foreach (string s in itemIntruder)
                {
                    List<string> t = li.Where(p => p.Tag == s).Select(g => g.Taille).ToList();
                    List<int> idd = li.Where(r => r.Tag == s).Select(v => v.Article_Type_ID).ToList();
                    if (t.Count > 0 && idd.Count > 0)
                    {
                        mainWorker.data.dbContext.Refresh(System.Data.Objects.RefreshMode.StoreWins, mainWorker.data.dbContext.user_article);
                        user_article cr = credit.FirstOrDefault(m => m.Taille == t[0] && m.Article_Type_Id == idd[0]);

                        //Recupere l'article type id de l'item sorti

                        //recupere la ligne dans user article en fonction de la taille de l'article type id de l'utilisateur en cours
                        if (cr != null)
                        {
                            cr.Credit_Restant = cr.Credit_Restant >= cr.Credit ? cr.Credit : cr.Credit_Restant + 1;
                            mainWorker.data.UpdateUserArticle(cr.Id, cr.Credit_Restant);
                        }

                        try
                        {
                            Log.add("Intrus connu détecté: " + s);
                            mainWorker.data.updateContent(5, s);//intrus
                            mainWorker.data.updateEpc(5, s, log.user_id, Properties.Settings.Default.NumArmoire);
                            List<string> typ = arttyp.Where(m => m.Id == idAlertType).Select(w => w.Code).ToList();
                            mainWorker.data.insertTagAlert(id, "", "", s, typ[0], t[0]);
                        }
                        catch (Exception e)
                        {
                            Log.add("Erreur update intrus connus: " + e.Message);
                            if (Properties.Settings.Default.UseDBGMSG)
                                MessageBox.Show("Erreur update intrus connus: " + e.Message);
                        }
                    }
                }
                synerr();
            }
        }
#else
        // SANS MODELE EDMX

        private static void MiseAJourAlerteItemMalPlaces(List<User_Article> ua4credit, List<Epc> alltag, List<string> itemIntruder, List<Article_Type> arttyp, User user)
        {
            Log.add("MiseAJourAlerteItemMalPlaces " + itemIntruder.Count.ToString());
            //---------------------------------------------------------------------------------
            //item intrus connu
            if (itemIntruder.Any())
            {
                string message = itemIntruder.Count + ((itemIntruder.Count > 1) ? " intrus connus détectés" : " intrus connu détecté");

                //Insere alert dans bdd
                int idAlertType = (user.Type == "reloader") ? AlerteType.ALERTE_MALPLACE_RELOAD : AlerteType.ALERTE_MALPLACE_USER;

                int id = workerV3.data.insertAlert(idAlertType, user.Id, Properties.Settings.Default.NumArmoire, message);

                //List<User_Article> ua4credit = workerV3.data.getUser_articleByUserId(userg[0].Id).ToList();
                //List<Epc> li = workerV3.data.getAllEPC().ToList();
                //insere dans tag alert la liste des tag en alert en fonction de l'id de l'alerte au dessus
                foreach (string s in itemIntruder)
                {
                    List<string> t = alltag.Where(p => p.Tag == s).Select(g => g.Taille).ToList();
                    List<int> idd = alltag.Where(r => r.Tag == s).Select(v => v.Article_Type_ID).ToList();
                    if (t.Count > 0 && idd.Count > 0)
                    {
#if EDMX
                        workerV3.data.dbContext.Refresh(System.Data.Objects.RefreshMode.StoreWins, workerV3.data.dbContext.user_article);
#endif
                        User_Article cr = ua4credit.FirstOrDefault(m => m.Taille == t[0] && m.Article_Type_Id == idd[0]);

                        //Recupere l'article type id de l'item sorti

                        //recupere la ligne dans user article en fonction de la taille de l'article type id de l'utilisateur en cours
                        if (cr != null)
                        {
                            if (Properties.Settings.Default.RecreditHebdo == false)
                            {
                                cr.Credit_Restant = cr.Credit_Restant >= cr.Credit ? cr.Credit : cr.Credit_Restant + 1;
                                workerV3.data.UpdateUserArticle(cr.Id, cr.Credit_Restant);
                            }
                            else// if (tmpusart.Date_Modification.Day > Properties.Settings.Default.NumJourRecreditHebdo || (tmpusart.Date_Modification.Day == Properties.Settings.Default.NumJourRecreditHebdo && tmpusart.Date_Modification.Hour > Properties.Settings.Default.HeureRecreditHebdo)))
              
                            {
                                cr.Credit_Semaine_Suivante = cr.Credit_Semaine_Suivante >= cr.Credit ? cr.Credit : cr.Credit_Semaine_Suivante + 1;
                                workerV3.data.UpdateUserArticle2(cr.Id, cr.Credit_Semaine_Suivante);
                            }
                        }

                        try
                        {
                            Log.add("Intrus connu détecté: " + s);
                            workerV3.data.updateContent(5, s);//intrus
                            workerV3.data.updateEpc(5, s, Login.user.Id.ToString(), Properties.Settings.Default.NumArmoire);
                            //List<string> typ = arttyp.Where(m => m.Id == idAlertType).Select(w => w.Code).ToList();
                            // Article_Type _Code
                            string Code_typArticle = arttyp.Where(m => m.Id == idd[0]).Select(w => w.Code).First();
                            string code_barre = alltag.Where(r => r.Tag == s).Select(v => v.Code_Barre).First();
                            workerV3.data.insertTagAlert(id, "", "", s, Code_typArticle, t[0], code_barre);
                        }
                        catch (Exception e)
                        {
                            Log.add("Erreur update intrus connus: " + e.Message);
                            if (Properties.Settings.Default.UseDBGMSG)
                                MessageBox.Show("Erreur update intrus connus: " + e.Message);
                        }
                    }
                }
                MainWindow.synerr();
            }
        }
#endif

        // AVEC MODELE EDMX
#if EDMX
        private void MiseAJourAlerteIntrusInconnus(List<user> userg, List<string> itemUnknow)
        {
            //---------------------------------------------------------------------------------
            //item intrus inconnus
            if (itemUnknow.Any())
            {
                string message = itemUnknow.Count + ((itemUnknow.Count > 1) ? " intrus inconnus détectés" : " intrus inconnu détecté");

                //Insere alert dans bdd
                int idAlertType = userg[0].Type == "reloader" ? 19 : 20;
                int id = mainWorker.data.insertAlert(idAlertType, userg[0].Id, Properties.Settings.Default.NumArmoire, message);

                //insere dans tag alert la liste des tag en alert en fonction de l'id de l'alerte au dessus
                foreach (string s in itemUnknow)
                {
                    try
                    {
                        Log.add("Intrus inconnu détecté: " + s);
                        //update du content
                        mainWorker.data.updateContent(300, s);// statut de l'epc inconnu mis à 300

                        mainWorker.data.insertTagAlert(id, "", "", s, "", "");
                    }
                    catch (Exception e)
                    {
                        Log.add("Erreur update intrus inconnus: " + e.Message);
                        if (Properties.Settings.Default.UseDBGMSG)
                            MessageBox.Show("Erreur update intrus inconnus: " + e.Message);
                    }
                }
                synerr();
            }
        }
#else
        // SANS MODELE EDMX
        private static void MiseAJourAlerteIntrusInconnus(User user, List<string> itemUnknow)
        {
            Log.add("MiseAJourAlerteIntrusInconnus " + itemUnknow.Count().ToString());
            //---------------------------------------------------------------------------------
            //item intrus inconnus
            if ((itemUnknow.Any()) && (user != null))
            {
                string message = itemUnknow.Count + ((itemUnknow.Count > 1) ? " intrus inconnus détectés" : " intrus inconnu détecté");

                //Insere alert dans bdd
                int idAlertType = (user.Type == "reloader") ? AlerteType.ALERTE_INTRUS_RELOAD : AlerteType.ALERTE_INTRUS_USER;
                int id = workerV3.data.insertAlert(idAlertType, user.Id, Properties.Settings.Default.NumArmoire, message);

                //insere dans tag alert la liste des tag en alert en fonction de l'id de l'alerte au dessus
                foreach (string s in itemUnknow)
                {
                    try
                    {
                        Log.add("Intrus inconnu détecté: " + s);
                        //update du content
                        workerV3.data.updateContent(EtatArticle.INTRUS_INCONNUS, s);// statut de l'epc inconnu mis à 300

                        workerV3.data.insertTagAlert(id, "", "", s, "", "");
                    }
                    catch (Exception e)
                    {
                        Log.add("Erreur update intrus inconnus: " + e.Message);
                        if (Properties.Settings.Default.UseDBGMSG)
                            MessageBox.Show("Erreur update intrus inconnus: " + e.Message);
                    }
                }
                MainWindow.synerr();
            }
        }
#endif


#if EDMX
        // AVEC MODELE EDMX
        private void MiseAJourAlerteCreditEpuise(List<user> userg, bool userwithoutcredit)
        {
            //---------------------------------------------------------------------------------

            if (userg[0].Type != "reloader" && userwithoutcredit == true)
            {
                try
                {
                    string message = userg[0].Prenom + " " + userg[0].Nom + " possède un crédit < ou égal a zéro";

                    //Insere alert dans bdd
                    int id = mainWorker.data.insertAlert(16, userg[0].Id, Properties.Settings.Default.NumArmoire, message);

                    synerr();
                }
                catch (Exception e)
                {
                    Log.add("Erreur insertAlert: " + e.Message);
                    if (Properties.Settings.Default.UseDBGMSG)
                        System.Windows.MessageBox.Show("Erreur insertAlert: " + e.Message);
                }

            }
        }
#else
        // SANS MODELE EDMX
        private static void MiseAJourAlerteCreditEpuise(User user, bool userwithoutcredit, List<int> articlesTypesDesEpuises, List<Article_Type> listArtTyp)
        {
            //---------------------------------------------------------------------------------
            Log.add("MiseAJourAlerteCreditEpuise " + userwithoutcredit.ToString());
            if (user != null)
            {
                if (user.Type != "reloader" && userwithoutcredit == true)
                {
                    try
                    {
                        string message = user.Prenom + " " + user.Nom + " possède un crédit < ou égal a zéro";

                        //Insere alert dans bdd
                        int id = workerV3.data.insertAlert(AlerteType.ALERTE_CREDITNUL, user.Id, Properties.Settings.Default.NumArmoire, message);
                        foreach (int ate in articlesTypesDesEpuises)
                        {
                            string code = listArtTyp.Where(x => x.Id == ate).Select(x => x.Code).FirstOrDefault();
                            workerV3.data.insertTagAlert(id, "", "", "", code, "");
                        }

                        MainWindow.synerr();
                    }
                    catch (Exception e)
                    {
                        Log.add("Erreur insertAlert: " + e.Message);
                        if (Properties.Settings.Default.UseDBGMSG)
                            System.Windows.MessageBox.Show("Erreur insertAlert: " + e.Message);
                    }

                }
            }
        }
#endif
#if EDMX
        // AVEC MODELE EDMX
        //liste de tous les tags manquant dans l'armoire courante v3
        public void malisteTousLesTagsManquants(List<epc> alltag, List<article_type> arttyp, List<user> userg)
        {
            List<epc> stocknullepc = alltag.Where(x => x.Armoire_ID == Properties.Settings.Default.NumArmoire).ToList();

            List<article_taille> listeArticle_Taille = mainWorker.data.getArticleTaille().ToList(); // tous les article_taille de l'armoire

            // pour chaque articleTypeId présent dans l'armoire
            foreach (int s in listeArticle_Taille.Select(x => x.Article_Type_ID).Distinct())
            {
                string pl = "";

                foreach (article_taille at in listeArticle_Taille.Where(x => x.Article_Type_ID == s))
                {
                    List<epc> listEpcTravail = new List<epc>();// liste d'epc

                    listEpcTravail = stocknullepc.Where(p => p.Taille == at.Taille && p.Article_Type_ID == at.Article_Type_ID && (p.State == 100 || p.State == 5)).ToList();

                    if (listEpcTravail.Count == 0)
                    {
                        if (at.Vide == false)
                        {
                            mainWorker.data.updateArticleTaille(at.Article_Type_ID, at.Taille, at.Armoire, true);
                            pl += at.Taille + ", ";
                        }
                    }
                    else
                    {
                        if (at.Vide == true)
                        {
                            mainWorker.data.updateArticleTaille(at.Article_Type_ID, at.Taille, at.Armoire, false);
                        }
                    }
                }

                if (pl != "")
                {
                    pl.Remove(pl.Length - 2);
                    string code = arttyp.FirstOrDefault(x => x.Id == s).Code;
                    string message = "Les articles, code " + code + " en taille " + pl + "ne sont plus en stock";
                    //Insere alert dans bdd
                    mainWorker.data.insertAlert(17, userg[0].Id, Properties.Settings.Default.NumArmoire, message);

                    synerr();
                }
            }
        }
#else
        //SANS MODELE EDMX
        //liste de tous les tags manquant dans l'armoire courante v3
        public static void MiseAJourAlertelisteTousLesTagsManquants(List<Epc> alltag, List<Article_Type> arttyp, User user)
        {
            Log.add("MiseAJourAlertelisteTousLesTagsManquants");
            List<Epc> stocknullepc = alltag.Where(x => x.Armoire_ID == Properties.Settings.Default.NumArmoire).ToList();

            List<Article_Taille> listeArticle_Taille = workerV3.data.getArticleTaille().ToList(); // tous les article_taille de l'armoire

            // pour chaque articleTypeId présent dans l'armoire
            foreach (int s in listeArticle_Taille.Select(x => x.Article_Type_ID).Distinct())
            {
                string pl = "";

                foreach (Article_Taille at in listeArticle_Taille.Where(x => x.Article_Type_ID == s && x.Armoire == Properties.Settings.Default.NumArmoire))
                {
                    List<Epc> listEpcTravail = new List<Epc>();// liste d'epc

                    listEpcTravail = stocknullepc.Where(p => p.Taille == at.Taille && p.Article_Type_ID == at.Article_Type_ID && ((p.State == EtatArticle.RETIRABLE) || (p.State == EtatArticle.INTRUS_CONNUS))).ToList();

                    if (listEpcTravail.Count == 0)
                    {
                        if (at.Vide == false)
                        {
                            workerV3.data.updateArticleTaille(at.Article_Type_ID, at.Taille, at.Armoire, true);
                            pl += at.Taille + ", ";
                        }
                    }
                    else
                    {
                        if (at.Vide == true)
                        {
                            workerV3.data.updateArticleTaille(at.Article_Type_ID, at.Taille, at.Armoire, false);
                        }
                    }
                }

                if (pl != "")
                {
                    pl.Remove(pl.Length - 2);
                    string code = arttyp.FirstOrDefault(x => x.Id == s).Code;
                    string message = "Les articles, code " + code + " en taille " + pl + "ne sont plus en stock";
                    //Insere alert dans bdd
                    int alertnumber = workerV3.data.insertAlert(AlerteType.ALERTE_STOCKVIDE, user.Id, Properties.Settings.Default.NumArmoire, message);
                    workerV3.data.insertTagAlert(alertnumber, "", "", "", code, pl);

                    MainWindow.synerr();
                }
            }
        }
#endif
        /// <summary>
        /// Changement d'état dus à la modification de la dotation ou du plan de chargement 
        /// </summary>
        /// <param name="tag">Listes des Epc de la dotation</param>
        /// <param name="contentArm">Listes des Epc dans l'armoire</param>
        /// <param name="user">Pour mis à jour</param>
        /// <param name="reader">Pour mis à jour</param>
        private static void MiseAJourTagInconnuVersConnus(List<string> tag, List<Content_Arm> contentArm, string user, int reader)
        {
            List<string> tagInconnus = contentArm.Where(p => p.State == EtatArticle.INTRUS_CONNUS).Select(p => p.Epc).ToList();
            if (tagInconnus!=null)
            foreach (string os in tagInconnus)
            {
                Log.add("tag " + os);
                if (tag.Contains(os) == true)
                {
                    Log.add("Le tag " + os + "est dans la base");
#if EDMX
#else
                    workerV3.data.updateEpc(100, os, user, reader);
                    workerV3.data.updateContent(100, os);
                    Content_Arm ca = contentArm.Where(p => p.Epc == os).First();
                    contentArm.Remove(ca);
                    ca.State = EtatArticle.RETIRABLE;
                    contentArm.Add(ca);
#endif
                }
            }
        }

        /// <summary>
        /// Changement d'état dus à la modification de la dotation ou du plan de chargement 
        /// </summary>
        /// <param name="tag">Listes des Epc de la dotation</param>
        /// <param name="contentArm">Listes des Epc dans l'armoire</param>
        /// <param name="user">Pour mis à jour</param>
        /// <param name="reader">Pour mis à jour</param>
        private void MiseAJourTagPresentVersIntrusConnus(List<string> itemIntruder, List<Epc> dotation, List<Content_Arm> contentArm, string user, int reader)
        {
            try
            {
                List<string> tagPresents = contentArm.Where(p => p.State == EtatArticle.RETIRABLE && p.RFID_Reader == Properties.Settings.Default.NumArmoire).Select(p => p.Epc).ToList();

                foreach (string prs in tagPresents)
                {
                    Epc ep = new Epc();
                    try
                    {
                        ep = dotation.Where(x => x.Armoire_ID == Properties.Settings.Default.NumArmoire && x.Tag == prs).First();
                    }
                    catch (Exception eer)
                    {
                        Log.add("Auparavant ce tag était lu sur une autre armoire (peut être mal place?)" + eer.Message);
                    }
                    if (ep != null)
                    {
                        if (workerV3.data.isInPlanChargement(ep.Taille, ep.Article_Type_ID, ep.Armoire_ID) == true)
                        {
                            Log.add("Le tag " + prs + " est dans le plan de chargement");

                        }
                        else
                        {
                            itemIntruder.Add(prs);//Ajoute un tag a list contenant les intrus connus
#if EDMX
#else
                            workerV3.data.updateEpc(EtatArticle.INTRUS_CONNUS, prs, user, reader);
                            workerV3.data.updateContent(EtatArticle.INTRUS_CONNUS, prs);
                            Content_Arm ca = contentArm.Where(p => p.Epc == prs).First();
                            contentArm.Remove(ca);
                            ca.State = EtatArticle.INTRUS_CONNUS;
                            contentArm.Add(ca);
#endif
                            Log.add("Le tag " + prs + " n'est pas dans le plan de chargement");
                        }
                    }
                    else
                    {
                        Log.add("Intrus inconnu " + prs);
                    }
                }
            }
            catch (Exception es)
            {
                Log.add("Error " + es.Message);
            }
        }
        #endregion //SousFonctionUpdateListBox

        private static string AnnonceListBox(List<Tag> list){
            IEnumerable<string> tagsExistants1, tagsExistants2, tagsPresents1, tagsPresents2, tagsAbsents1, tagsAbsents2, tagsSortants, tagsRentrants, tagsInconnus;
            tagsPresents2 = list.Select(p => p.Epc.ToHexString()).ToList();
            string mess = tagsPresents2.Count() + " tags dans l'armoire\n";
            tagsExistants1 = workerV3.data.getAllEPC().Select(t => t.Tag).ToList();//.Armoire.existants.SelectMany(e => e.outils).Select(o=>o.tag).ToList();mess += "\ntagsExistants1: " + tagsExistants1.Count().ToString();
            tagsPresents1 = workerV3.data.getContent().Select(t => t.Epc).ToList(); //Armoire.presents.SelectMany(e => e.outils).Select(o => o.tag).ToList(); mess += "\ntagsPresents1: " + tagsPresents1.Count().ToString();
            tagsAbsents1 = tagsExistants1.Except(tagsPresents1).ToList(); mess += "\ntagsAbsents1: " + tagsAbsents1.Count().ToString();
            tagsExistants2 = tagsExistants1.Union(tagsPresents1).Distinct().ToList(); mess += "\ntagsExistants2: " + tagsExistants2.Count().ToString();
            tagsAbsents2 = tagsExistants2.Except(tagsPresents2).ToList(); mess += "\ntagsAbsents2: " + tagsAbsents2.Count().ToString();
            tagsSortants = tagsPresents1.Intersect(tagsAbsents2).ToList(); mess += "\ntagsSortants: " + tagsSortants.Count().ToString();
            tagsRentrants = tagsPresents2.Intersect(tagsAbsents1).ToList(); mess += "\ntagsRentrants: " + tagsRentrants.Count().ToString();
            tagsInconnus = tagsPresents2.Except(tagsExistants1).ToList(); mess += "\ntagsInconnu: " + tagsInconnus.Count().ToString();
            return mess;       
        }


        //Fonction principal d'apres scan
        //Recupere le resultat du scan et effectue des comparaison pour connaitre l'état de chaque article scanné
        public static void updateListbox(List<Tag> list)  {
            fini = false;
        /*    DateTime debut = DateTime.Now;
            do { } while (ReaderControl.railmoving);
            TimeSpan ms = DateTime.Now.Subtract(debut);*/
            Log.add("Start updatelistbox");//: " + Math.Ceiling(ms.TotalMilliseconds).ToString() + " ms");

      //      if (Properties.Settings.Default.UseDBGMSG) MessageBox.Show(AnnonceListBox(list));
       
            /**** RECREDITATION DIRECTE *****/

             int n = 0;
             foreach (string tag in list.Select(p => p.Epc.ToHexString()).ToList())
            {
         
                Epc epc = workerV3.data.getEpc(tag); 
                Log.add("tag " + tag +" existant ? " + (epc!=null));
                if (epc != null && (epc.State == EtatArticle.SORTI || epc.State == EtatArticle.SORTI_TailleSup))
                {
                    Log.add("Recredite user : " + epc.Last_User + " pour  epc :" + epc.Tag + " etat : " + epc.State);
                    workerV3.data.UpdateUserArticleRestitution(epc);
                    n++;
                    workerV3.data.updateEpc(EtatArticle.RETIRABLE, epc.Tag, epc.Last_User, ReaderId);
                } else if (Properties.Settings.Default.UseDBGMSG && epc!=null) Log.add("Etat de l'article "+ epc.Tag+" arrivant: "+ epc.State);
            }
            Log.add("Recréditations : " + n + " traitement(s)"); 

            /************************************/

            try {
                if (MainWindow.isLocalBDDAvailable)
                {

#if EDMX 
                {
                    List<content_arm> contentArm = mainWorker.data.getContent().ToList();

                    List<user> userg = mainWorker.data.getUserType(log.textBoxID.Text).ToList();

                    List<article_taille> listArtTaille = mainWorker.data.getArticleTaille().ToList();
                    //TEST
                    if (userg.Count != 0)
                    {
                        mainWorker.data.scan(list, ReaderId, 100, userg[0].Id.ToString()); // je ne sais pas pourquoi mais cette ligne était commentée
                    }

                    List<article_type> arttyp = mainWorker.data.getArticle_type().ToList();//liste tout les articletypes
                    List<epc> alltag = mainWorker.data.getTagAll().ToList();//liste toutes la table epc
                    List<string> tag = mainWorker.data.getTag().ToList();//liste de tout les tags dans la base

                    //TEST
                    //mainWorker.data.scan(list, ReaderId, 100); //celle-ci ne l'était pas

                    List<string> itemOut = new List<string>();//List de tout les items sortis
                    List<string> itemIntruder = new List<string>();//List des items intrus
                    List<string> itemUnknow = new List<string>();//List des items inconnus
                    List<epc> itemNoOut = new List<epc>();//List des items commandés non sorties
                    List<string> itemWarnOut = new List<string>();//List des items  sorties non commandés
                    List<epc> itemRightOut = new List<epc>();//List des items  sorties commmandés

                    List<epc> tempListEpcSelect = new List<epc>(); //List des items selectionnés
                    List<epc> itemOutEpc = new List<epc>();

                    List<epc> itemDiffOut = new List<epc>();//List des items sorties de taille differente
                    List<int> articleTypeSelect = new List<int>();//List des articleType choisis

                    listT = list;
                    bool userwithoutcredit = false;
                    
                    IEnumerable<string> deltaOut = null;
                    IEnumerable<string> deltaIn = null;
                    string[] listag = new string[] { };

                    try
                    {
#if NEWIMPINJ
                        listag = listT.Select(p => p.Epc.ToHexString()).ToArray();
#else
                    listag = listT.Select(p => p.Epc).ToArray();
#endif
                    }
                    catch
                    {

                    }

                    string[] listit = contentArm.Where(x => x.RFID_Reader == Properties.Settings.Default.NumArmoire).Select(p => p.Epc).ToArray();
                    //liste tag précedemment dans l'armoire
                    //string[] listit = contentArm.Select(p => p.Epc).toArray();


                    //différence entre les tag avant le scan et apres        
                    deltaOut = listit.Except(listag);//liste tag sorti (ce qui se trouve dans contentarm de l'armoire et qui n'ont pas été scannés)
                    deltaIn = listag.Except(listit);//list tag entré (ce qui est scanné moins ce qui est dans contentarm where armoireid = cette armoire)
                    int nbchoix = 0;
                    int ty = nbitem;
                    //TEST
                    //List<user> userg = mainWorker.data.getUserType(log.textBoxID.Text).ToList();
                    ReorganiseContentArm(userg, contentArm, alltag, deltaOut);
                    Init();
                    nbchoix = UpdateNbChoix(userg);
                    //Liste tout les items sorties
                    MiseAJourListeItemsSortis(deltaOut, itemOut);
                    Init();
                    MiseAJourListeItemsSortisParRapportALaCommande(userg, nbchoix, itemOut, itemRightOut, itemOutEpc, itemNoOut, itemDiffOut, articleTypeSelect);

                    Init();
                    MiseAJourItemUnknown(itemOut, itemWarnOut, itemUnknow, alltag, deltaIn);
                    MiseAJourListeIntrus(deltaIn, tag, listArtTaille, itemIntruder, itemUnknow, userg);
                    Init();
                    userwithoutcredit = userwithoutcredit = MiseAJourItemsEntres(userg, alltag, itemNoOut, itemRightOut, itemDiffOut, arttyp);
                    Init();
                    MiseAJourTropPercu(userg, itemWarnOut, alltag, arttyp);
                    Init();
                    MiseAJourItemMalPlaces(itemIntruder, arttyp, userg);
                    Init();
                    MiseAJourAlerteIntrusInconnus(userg, itemUnknow);
                    Init();
                    MiseAJourAlerteCreditEpuise(userg, userwithoutcredit);
                    //liste de tous les tags manquants dans l'armoire courante (nécéssite une base d'en bas V3)
                    malisteTousLesTagsManquants(alltag, arttyp, userg);
                
                }
#else
                    {
                        List<Content_Arm> contentArm = workerV3.data.getContent().ToList();
                        // Liste des users ayant le password entré pour se connecter
                      
                        List<Article_Taille> listArtTaille = workerV3.data.getArticleTaille().ToList();

                        // Mise à jour de Content_Arm 
                        if (Login.user != null)// && Login.user.Type!="reloader") 
                        {
                            workerV3.data.scan(list, ReaderId, 100, Login.user.Id.ToString());
                        }
                       
                        List<Article_Type> arttyp = workerV3.data.getArticle_type().ToList();//liste tout les articletypes
                        List<Epc> alltag = workerV3.data.getAllEPC().ToList();//liste toutes la table epc
                        

                        List<string> itemOut = new List<string>();//Liste de tout les items sortis
                        List<string> itemIntruder = new List<string>();//Liste des items intrus
                        List<string> itemUnknow = new List<string>();//Liste des items inconnus
                        List<Epc> itemNoOut = new List<Epc>();//Liste des items commandés mais non sortis
                        List<string> itemWarnOut = new List<string>();//List des items sortis mais non commandés
                        List<Epc> itemRightOut = new List<Epc>();//List des items sorties et commmandés

                        List<Epc> tempListEpcSelect = new List<Epc>(); //List des items selectionnés
                        //List<Epc> itemOutEpc = new List<Epc>(); // Liste intermédiaire pour le calcule des inconnus

                        List<Epc> itemDiffOut = new List<Epc>();//Liste des items sortis de taille differente
                        List<int> articleTypeSelect = new List<int>();//Liste des articleType choisis
                        List<string> tags = workerV3.data.getTag().ToList();//liste de tout les tags dans la base
                        listT = list; // Liste des tags


                        IEnumerable<string> deltaOut = null;
                        IEnumerable<string> deltaIn = null;
                        string[] listag = new string[] { };

                        try
                        {
#if NEWIMPINJ
                            listag = listT.Select(p => p.Epc.ToHexString()).ToArray();
#else
                            listag = listT.Select(p => p.Epc).ToArray();
#endif
                        }
                        catch (Exception exi)
                        {
                            Log.add("Erreur lecture Tag Impinj " + exi.Message);
                        }
                        List<int> arttypeidpuise = new List<int>();
                        // Liste des tags précedemment dans l'armoire
                        string[] listit = contentArm.Where(x => x.RFID_Reader == Properties.Settings.Default.NumArmoire).Select(p => p.Epc).ToArray();

                        // Crée les listes de différences entre les tags avant le scan et apres        
                        deltaOut = listit.Except(listag);//liste tag sorti (ce qui se trouve dans contentarm de l'armoire et qui n'ont pas été scannés)
                        deltaIn = listag.Except(listit);//list tag entré (ce qui est scanné moins ce qui est dans contentarm where armoireid = cette armoire)
                        Log.add("Creation des listes DeltaIn, DeltaOut");
                        Log.add("listes DeltaIn");
                        foreach (string st in deltaIn)
                            Log.add(st);
                        Log.add("listes DeltaOut");
                        foreach (string st in deltaOut)
                            Log.add(st);

                        // Suite à des mises à jours d'EPC au travers d'ATLAS des Tags peuvent changer de status et devenir connus.
                        if (Login.user != null)
                        {
                            Log.add("MiseAJourTagInconnuVersConnus");
                            MiseAJourTagInconnuVersConnus(tags, contentArm, Login.user.Id.ToString(), Properties.Settings.Default.NumArmoire);
                        }

                        // Suite à des mises à jour du plan de chargement, certains tag peuvent devenir mal placés
                        // MiseAJourTagPresentVersIntrusConnus(itemIntruder, alltag, contentArm, useridstr, NumArmoire);
                        //List<user> userg = mainWorker.data.getUserType(log.textBoxID.Text).ToList();

                        //Liste tout les items sortis
                        if (Login.user != null)
                        {
                            Log.add("Mise a jour Items Sortis");
                            MiseAJourListeItemsSortis(deltaOut, itemOut);
                        }
                        Init();                    

                        if (Login.user.Type == "reloader")
                        {
                            ReorganiseContentArm(Login.user, contentArm, alltag, deltaOut);
                            Init();                                               
                            //workerV3.data.RestitutionScan(list, ReaderId); // Restitution d'article directement effectuée par le reloader
                            MiseAJourListeIntrusReloader(deltaIn, tags, itemIntruder, itemUnknow,Login.user.Id.ToString());

                        }
                        else
                        {
                            bool userwithoutcredit = false;
                            MiseAJourListeIntrusUser(deltaIn, tags, itemIntruder, itemUnknow);
                            MiseAJourListeItemsSortisParRapportALaCommande(itemOut, alltag, itemRightOut, itemWarnOut, itemNoOut, itemDiffOut, articleTypeSelect);
                            //MiseAJourItemUnknown(itemOutEpc, itemWarnOut, itemUnknow, alltag, deltaIn); // Mise à jour itemWarmOut et itemUnknown                    
                            Init();
                            List<User_Article> userArticles = workerV3.data.getUser_articleByUserId(Login.user.Id).ToList();
                            userwithoutcredit = MiseAJourItemsEntres(Login.user.Id, itemWarnOut, alltag, userArticles, itemNoOut, itemRightOut, itemDiffOut, arttyp, arttypeidpuise);
                            MiseAJourAlerteItemMalPlaces(userArticles, alltag, itemIntruder, arttyp, Login.user); 
                            Init();
                            // Restitution d'article directement effectuée par l'utilisateur                          
                            //workerV3.data.RestitutionScan(list, ReaderId);
                            //MiseAJourAlerteMalPercu(userid, itemWarnOut, alltag, userArticles, itemNoOut, itemDiffOut, arttyp);
                            MiseAJourAlerteCreditEpuise(Login.user, userwithoutcredit, arttypeidpuise, arttyp);
                        }

                        MiseAJourAlerteIntrusInconnus(Login.user, itemUnknow); // Eploite itemUnknown
                        Init();

                        //liste de tous les tags manquants dans l'armoire courante (nécéssite une base d'en bas V3)
                        List<Epc> alltagRemisAJour = workerV3.data.getAllEPC().ToList();//liste toutes la table epc
                        MiseAJourAlertelisteTousLesTagsManquants(alltagRemisAJour, arttyp, Login.user);
                    }
#endif
                }
                else
                {
                    throw new Exception("BDD Locale non disponible");
                }
                fini = true;
                MainWindow.session = false;
       //         if (Properties.Settings.Default.UseDBGMSG) MessageBox.Show(AnnonceListBox(list));
                Log.add("LOGIN inventaire"); MainWindow.login();
            }
            catch (Exception e)
            {
                Log.add("Erreur UpdateListBox: " + e.Message);
                if (Properties.Settings.Default.UseDBGMSG)
                {
                    MessageBox.Show("Une erreur est survenue:\n" + e.Message);
                }
            }
        }
        #region syst_windows
        static int compteur= 0;
        /// <summary>
        /// Laisse le système d'exploitation gérer les évements systèmes
        /// </summary>
        static void Init()
        {
            System.Threading.Thread.Sleep(5);
            ComponentDispatcher.ThreadIdle += new EventHandler(OnIdle);
            compteur = 0;
        }

        /// <summary>
        /// Evenement permettant de laisser du temps au CPU pour gérer des évemenents systèmes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnIdle(object sender, EventArgs e)
        {
            if (compteur == 100)
            {
                // stop processing OnIdle()
                ComponentDispatcher.ThreadIdle -= OnIdle;
            }
            else
            {
                compteur++;
            }
        }
        #endregion //syst_windows
    }   
}
