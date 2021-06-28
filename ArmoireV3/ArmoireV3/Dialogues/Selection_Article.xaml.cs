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
using ArmoireV3.Dialogues;
using ArmoireV3.Entities;
using System.Collections.ObjectModel;
using ArmoireV3.Controls;
using ArmoireV3.Inter;
using System.Windows.Threading;
using System.IO;

namespace ArmoireV3.Dialogues
{
    // Classe ajoutée
    public class ArtRet
    {
        private int nbArtRet;

        public int NbArtRet
        {
            get { return nbArtRet; }
            set { nbArtRet = value; }
        }

        private bool artTailleSup;

        public bool ArtTailleSup
        {
            get { return artTailleSup; }
            set { artTailleSup = value; }
        }

        private bool artTailleNorm;
        public bool ArtTailleNorm
        {
            get { return artTailleNorm; }
            set { artTailleNorm = value; }
        }

        public ArtRet(int ndArticleRet, bool artTailNorm, bool artTailSup)
        {
            nbArtRet = ndArticleRet;
            ArtTailleNorm = artTailNorm;
            artTailleSup = artTailSup;
        }
    }
    /// <summary>
    /// Logique d'interaction pour Selection_Article.xaml
    /// </summary>
    public partial class Selection_Article : UserControl
    {
#if EDMX
        Worker workerSelect = new Worker();
        List<ArmoireV3.Entities.article_type> listArticleType = new List<article_type>();
        //List<ArmoireV3.Entities.epc> listEPC = new List<epc>(); // inutilisé
        List<ArmoireV3.Entities.user> listUser = new List<user>();
        List<user_article> cred = new List<user_article>();
#else
        WorkerV3 workerSelect = new WorkerV3();
        List<ArmoireV3.Entities.Article_Type> listArticleType = new List<Article_Type>();
        //List<ArmoireV3.Entities.epc> listEPC = new List<epc>(); // inutilisé
        List<ArmoireV3.Entities.User> listUser = new List<User>();
        List<User_Article> cred = new List<User_Article>();
#endif

        public ObservableCollection<ItemChoice> sampleData2 = new ObservableCollection<ItemChoice>();
        ObservableCollection<selectItem> sampleData = new ObservableCollection<selectItem>();
        ObservableCollection<selectItem> temp;
        ObservableCollection<ItemChoice> temp2;
        ObservableCollection<ImageSource> photoPath = new ObservableCollection<ImageSource>();
        List<selectItem> tel = new List<selectItem>();
        DispatcherTimer verifMaxTimeByPhase;

        // Variable de mémorisation des articles sélectionnés
        private ArtRet[] ArticlesRet;

        public  int quantitselect = 1;
        public  string taille = "";
        public string use = "";
        List<selectItem> itemstock = new List<selectItem>();
        List<ItemChoice> itemquantit = new List<ItemChoice>();
#if EDMX
        List<user_article> t = new List<user_article>();
#else
        List<User_Article> t = new List<User_Article>();
#endif
        public static readonly RoutedEvent RetourEvent = EventManager.RegisterRoutedEvent(
       "retour", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Selection_Article));


        public static readonly RoutedEvent ValidEvent = EventManager.RegisterRoutedEvent(
       "valid", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Selection_Article));


        public Selection_Article()
        {
            InitializeComponent();
            this.DataContext = this;
            listSelection.Visibility = Visibility.Hidden;

            listArticleType = workerSelect.data.getArticle_type().ToList();
            // inutilisé
            //listEPC = workerSelect.data.getEPC().Where(x => x.Actif == 1).ToList();
            listUser = workerSelect.data.getUser().ToList();
            cred = workerSelect.data.getUser_article().ToList();
            initVerifMaxTimeByPhase();
        }

        private void ImageFailedEventHandler(object sender, ExceptionRoutedEventArgs e)
        {
            ((Image)sender).Source = new BitmapImage(new Uri("\\Images\\DEFAULT.JPG", UriKind.Relative));
        }

        void initVerifMaxTimeByPhase()
        {
            verifMaxTimeByPhase = new DispatcherTimer();
            verifMaxTimeByPhase.Tick += new EventHandler(verifMaxTimeByPhase_Tick);
            verifMaxTimeByPhase.Interval = new TimeSpan(0, 0, Properties.Settings.Default.MaxTimeByPhase); //360 = 6 minutes
            verifMaxTimeByPhase.Start();
        }

        void verifMaxTimeByPhase_Tick(object sender, object e)
        {
            verifMaxTimeByPhase.Stop();
            verifMaxTimeByPhase.Tick -= new EventHandler(verifMaxTimeByPhase_Tick);
            RaiseEvent(new RoutedEventArgs(RetourEvent, this));
        }

        public event RoutedEventHandler retour  { add { AddHandler(RetourEvent, value); }     remove { RemoveHandler(RetourEvent, value); }   }

        public event RoutedEventHandler valid   {   add { AddHandler(ValidEvent, value); }    remove { RemoveHandler(ValidEvent, value); }    }

        private void buttonDeconnexion_Click(object sender, RoutedEventArgs e)    {
            verifMaxTimeByPhase.Stop();
            verifMaxTimeByPhase.Tick -= new EventHandler(verifMaxTimeByPhase_Tick);
            RaiseEvent(new RoutedEventArgs(RetourEvent, this));            
        }
   

        private void buttonRetrait_Click(object sender, RoutedEventArgs e)   {
            verifMaxTimeByPhase.Stop();
            verifMaxTimeByPhase.Tick -= new EventHandler(verifMaxTimeByPhase_Tick);
            if (listSelection.HasItems)
            {
                RaiseEvent(new RoutedEventArgs(ValidEvent, this));
            }         
        }

        private void buttonPlus_Click(object sender, RoutedEventArgs e)
        {
            if (listObjets.SelectedItems.Count > 0)
            {
                try
                {
                    listSelection.Visibility = Visibility.Visible;                    
#if EDMX
                    List<ArmoireV3.Entities.user> user = workerSelect.data.getUserById(int.Parse(userId.Text)).ToList();
                    
#else
                     
#endif

                    int numArmoire = Properties.Settings.Default.NumArmoire;

                    int vetPrisEtSelectJour = 0;

                    int nbArtSel = 0;

                    if (ArticlesRet != null && ArticlesRet.Count() > 0)
                    {
                        foreach (ArtRet ar in ArticlesRet)
                        {
                            if (ar != null)
                                nbArtSel += ar.NbArtRet;
                        }
                    }

                    if (Properties.Settings.Default.LimitationJour == true)
                        vetPrisEtSelectJour = workerSelect.data.GetNumberOfArticlesInLogEpcForLastDay(Login.user.Id) + nbArtSel;

                    if (vetPrisEtSelectJour < Properties.Settings.Default.MaxRetraitJour)
                    {


#if EDMX
                        // tempta correspond aux tailles d'articles de l'utilisateur user identifié par userId.text la ligne précédente
                        List<user_article> tempta = workerSelect.data.getUser_articleById(user[0].Id).ToList();
                        // articletaille contiendra les tempta correspondants à ce qui se trouve dans les cases de l'armoire identifié dans settings
                        List<user_article> articletaille = new List<user_article>();
                        List<@case> lodplan = workerSelect.data.getCase().ToList();
                        foreach (user_article ua in tempta)
                        {
                            foreach (@case cas in lodplan)
                            {
                            
                                if (ua.Article_Type_Id == cas.Article_Type_Id && ua.Credit > 0 && !articletaille.Contains(ua) && cas.Armoire_ID == numArmoire)
                                {
                                    articletaille.Add(ua);
                                }
                            }
                        }
#else
                        // tempta correspond aux tailles d'articles de l'utilisateur user identifié par userId.text la ligne précédente
                        List<User_Article> tempta = workerSelect.data.getUser_articleByUserId(Login.user.Id).ToList();
                        // articletaille contiendra les tempta correspondants à ce qui se trouve dans les cases de l'armoire identifié dans settings
                        List<User_Article> articletaille = new List<User_Article>();
                        List<Case> lodplan = workerSelect.data.getCase().ToList();

                        foreach (User_Article ua in tempta)
                        {
                            foreach (Case cas in lodplan)
                            {

                                if (ua.Article_Type_Id == cas.Article_Type_Id && ua.Credit > 0 && !articletaille.Contains(ua) && cas.Armoire_ID == numArmoire)
                                {
                                    articletaille.Add(ua);
                                }
                            }
                        }
#endif

#if EDMX
                    List<epc> stock = new List<epc>();
#else
                        List<Epc> stock = new List<Epc>();
#endif
                        listObjets.Items.Refresh();

                        try
                        {
                            string tailledemandee = "";
                            for (int i = 0; i < articletaille.Count; i++)
                                if (articletaille[i].Article_Type_Id == ((selectItem)listObjets.SelectedItem).Id)
                                {
                                    tailledemandee = articletaille[i].Taille;
                                    break;
                                }
                            // la ligne suivante va recueillir la liste d'epc ayant pour id celui de l'article sélectionné
                            // tenant compte de la taille, du numéro de l'armoire et du statut == 100
                            stock = workerSelect.data.getEpcByArticleIdAndUniqueSize(((selectItem)listObjets.SelectedItem).Id, tailledemandee, numArmoire, 100).ToList();
                        }
                        catch (Exception ex)
                        {
                            if (Properties.Settings.Default.UseDBGMSG)
                                System.Windows.MessageBox.Show("Erreur: " + ex.Message);
                        }

                        //Liste des items dans la fenetre qui contient stock dans ses champs mais ce dernier ne correspond pas 
                        itemstock = sampleData.ToList();

                        // le champs Id de (selectItem)listObjets.SelectedItem correspond à l'Id de l'article_type selectionné
                        tel = itemstock.Where(k => k.Id == ((selectItem)listObjets.SelectedItem).Id).ToList();

                        int crediit = tel[0].Credit;

                        if (ArticlesRet[((selectItem)listObjets.SelectedItem).Id - 1] == null)
                            ArticlesRet[((selectItem)listObjets.SelectedItem).Id - 1] = new ArtRet(0, false, false);


                        if ((stock.Count > ArticlesRet[((selectItem)listObjets.SelectedItem).Id - 1].NbArtRet) && (ArticlesRet[((selectItem)listObjets.SelectedItem).Id - 1].ArtTailleSup == false))
                        {
                            //int crediit = tel[0].Credit;
                            if (crediit > 0)
                            {
                                int tempp = itemstock.Where(x => x.Id == ((selectItem)listObjets.SelectedItem).Id).Select(x => x.Stock).FirstOrDefault();
                                if (tempp != 0)
                                {
                                    loadItemSelection(((selectItem)listObjets.SelectedItem).Id, stock[0].Taille);
                                    {
                                        var item = sampleData.FirstOrDefault(i => i.Id == ((selectItem)listObjets.SelectedItem).Id);
                                        if (item != null)
                                        {
                                            item.Credit--;
                                            item.Stock--;
                                            ArticlesRet[((selectItem)listObjets.SelectedItem).Id - 1].NbArtRet++;
                                            ArticlesRet[((selectItem)listObjets.SelectedItem).Id - 1].ArtTailleNorm = true;
                                        }
                                    }

                                }

                            }
                            else
                            {
#if OPTLANG
                            MessageBox.Show(Languages.notEnoughCredits);
#else
                                MessageBox.Show("Vous n'avez pas assez de crédit");
#endif
                            }
                        }
                        else
                        {
                            if (crediit > 0)
                            {
                                // recherche du nombre d'article de la taille directement supérieure
#if EDMX
                            List<epc> stockSup = new List<epc>();
#else
                                List<Epc> stockSup = new List<Epc>();
#endif
                                try
                                {
                                    string tailledemandee = "";
                                    for (int i = 0; i < articletaille.Count; i++)
                                        if (articletaille[i].Article_Type_Id == ((selectItem)listObjets.SelectedItem).Id)
                                            tailledemandee = articletaille[i].Taille;
                                    // la ligne suivante va recueillir la liste d'epc ayant pour id celui de l'article sélectionné
                                    // tenant compte de la taille, du numéro de l'armoire et du statut == 100
                                    stockSup = workerSelect.data.getEpcTailleSupByArticleIdAndOriginalSize(((selectItem)listObjets.SelectedItem).Id, tailledemandee, numArmoire, 100).ToList();

                                }
                                catch (Exception ex)
                                {
                                    if (Properties.Settings.Default.UseDBGMSG)
                                        System.Windows.MessageBox.Show("Erreur: " + ex.Message);
                                }

                                if ((stockSup.Count > ArticlesRet[((selectItem)listObjets.SelectedItem).Id - 1].NbArtRet) && (ArticlesRet[((selectItem)listObjets.SelectedItem).Id - 1].ArtTailleNorm == false))
                                {
#if OPTLANG
                                string text = Languages.propositionSizeSup;
#else
                                    string text = "L'article sélectionné n'est temporairement pas disponible dans la taille demandée.\nNous vous proposons exceptionnellement de sélectionner la taille supérieure.\nNous vous remercions de votre compréhension\n\nDésirez-vous l'article de taille supérieure ?";
#endif
                                    MessageBoxResult reponse = MessageBox.Show(text, "Stock", MessageBoxButton.YesNo);
                                    if (reponse == MessageBoxResult.Yes)
                                    {

                                        {
                                            // le champs Id de (selectItem)listObjets.SelectedItem correspond à l'Id de l'article_type selectionné
                                            int tempp = itemstock.Where(x => x.Id == ((selectItem)listObjets.SelectedItem).Id).Select(x => x.Stock).FirstOrDefault();
                                            if (tempp != 0)
                                            {
                                                loadItemSelection(((selectItem)listObjets.SelectedItem).Id, stockSup[0].Taille);
                                                {
                                                    var item = sampleData.FirstOrDefault(i => i.Id == ((selectItem)listObjets.SelectedItem).Id);
                                                    if (item != null)
                                                    {
                                                        item.Credit--;
                                                        item.Stock--;

                                                        ArticlesRet[((selectItem)listObjets.SelectedItem).Id - 1].NbArtRet++;
                                                        ArticlesRet[((selectItem)listObjets.SelectedItem).Id - 1].ArtTailleSup = true;
                                                    }
                                                }

                                            }

                                        }
                                    }
                                }
                                else
                                {

                                    if ((stockSup.Count > 0) && (ArticlesRet[((selectItem)listObjets.SelectedItem).Id - 1].NbArtRet > 0) && (ArticlesRet[((selectItem)listObjets.SelectedItem).Id - 1].ArtTailleNorm == true))
                                    {
                                        if (tel[0].Credit > 0)
#if OPTLANG
                                        MessageBox.Show(Languages.reqLargerArticleCondition);
#else
                                            MessageBox.Show("Afin de sélectionner un article de taille supérieure,\nveuillez vous connecter de nouveau après le retrait des articles selectionnés");
#endif
                                        else
#if OPTLANG
                                        MessageBox.Show(Languages.notEnoughCredits);
#else
                                            MessageBox.Show("Vous n'avez pas assez de crédit");
#endif
                                    }
                                    else
#if OPTLANG
                                    MessageBox.Show(Languages.outofstock);
#else
                                        MessageBox.Show("Stock épuisé");
#endif
                                }
                            }
                            else
#if OPTLANG
                            MessageBox.Show(Languages.notEnoughCredits);
#else
                                MessageBox.Show("Vous n'avez pas assez de crédit");
#endif
                        }
                    }
                    else
                    {
#if OPTLANG
                    MessageBox.Show(Languages.maxItemToday);
#else
                        MessageBox.Show("Vous avez dépassé votre quota journalier");
#endif
                    }
                    listObjets.Items.Refresh();
                }
                catch (Exception ex)
                {
                    if (Properties.Settings.Default.UseDBGMSG)
                    {
                        string msg = "Erreur: " + ex.Message;
                        if (ex.InnerException != null) msg += "\n" + ex.InnerException.Message;
                        MessageBox.Show(msg);
                    }
                }
            }
        }

        private void buttonMoins_Click(object sender, RoutedEventArgs e)
        {
            if (listSelection.SelectedIndex != -1)
            {
                for (int i = 0; i < listObjets.Items.Count; i++)
                {
                    if (((ItemChoice)listSelection.SelectedItem).Description == ((selectItem)listObjets.Items.GetItemAt(i)).Description)
                    {
                        listObjets.SelectedItem = listObjets.Items.GetItemAt(i);
                    }
                }
                if (((ItemChoice)listSelection.SelectedItem).Description == ((selectItem)listObjets.SelectedItem).Description)
                {
                    int artid = ((ItemChoice)listSelection.SelectedItem).Id;
                    unload();
                    var item = sampleData.FirstOrDefault(i => i.SelectedID == ((selectItem)listObjets.SelectedItem).SelectedID); //SelectedIndex.ToString());
                    if (item != null)
                    {
                        item.Credit++;
                        item.Stock++;
                        
                        ArticlesRet[artid-1].NbArtRet--;
                        //item.StockArmoire++;
                    }
                }
            }
            listObjets.Items.Refresh();
        }

        private void Grid_TouchDown(object sender, SelectionChangedEventArgs e)
        {
          
        }

        //décharge le vestiaire
        public void unload()
        {
            
            var itemDesc = sampleData2.FirstOrDefault();
            
            ItemChoice test = (ItemChoice)listSelection.SelectedItem;

            var item = sampleData2.FirstOrDefault(i => i.Description == test.Description);
      
                if (item != null)
                {
                    if (item.Quantit > 1)
                    {
                        item.Quantit--;
                    }
                    else
                    {
                        sampleData2.Remove(item);
                    }
                }

                listSelection.Items.Refresh();
            
        }

        private string LoadExternalImagePath(string ImagePathString)
        {
            ImagePathString= Environment.CurrentDirectory + ImagePathString.Replace("/","\\");

            if (File.Exists(ImagePathString))
            {
                // Le fichier existe
                try
                {
                    return ImagePathString;
                }
                catch (Exception e)
                {
                    Log.add(" Le chargement du fichier image " + ImagePathString + " a échoué" + e.Message);
                    return null;
                }
            }
            else if (File.Exists(Environment.CurrentDirectory + "\\Images\\DEFAULT.JPG"))
            {
                ImagePathString = Environment.CurrentDirectory + "\\Images\\DEFAULT.JPG";
                try
                {
                    return ImagePathString;
                }
                catch (Exception e)
                {
                    Log.add(" Le chargement du fichier image " + ImagePathString + " a échoué" + e.Message);
                    return null;
                }
            }
            else
            {
                Log.add(" Le fichier image " + ImagePathString + " n'a pas été trouvé");
                return null;
            }
        }

        //Chargement de mon vestiaire
        public void loadItem()
        {
#if EDMX
            List<ArmoireV3.Entities.user> user = workerSelect.data.getUserById(int.Parse(userId.Text)).ToList();
            // tempta correspond aux tailles d'articles de l'utilisateur user identifié par userId.text la ligne précédente
            //List<user_article> tempta = workerSelect.data.getUser_articleById(user[0].Id).ToList();
            List<user_article> tempta = workerSelect.data.getUser_articleById(user[0].Id).Where(x => x.Credit > 0).ToList();
#else
            
            // tempta correspond aux tailles d'articles de l'utilisateur user identifié par userId.text la ligne précédente
            //List<user_article> tempta = workerSelect.data.getUser_articleById(user[0].Id).ToList();
            List<User_Article> tempta = workerSelect.data.getUser_articleByUserId(Login.user.Id).Where(x => x.Credit > 0).ToList();
#endif

            //Initialisation d'un tableau d'ArtRet à la bonne dimension
            int initSizeArtRet=0;
            int j=0;
            for(int i=0;i<tempta.Count;i++)
            {
                j=tempta[i].Article_Type_Id;
                if(j>initSizeArtRet)
                    initSizeArtRet=j;
            }
            ArticlesRet = new ArtRet[initSizeArtRet];
            
            
#if EDMX
            // articletaille contiendra les tempta correspondants à ce qui se trouve dans les cases de l'armoire identifié dans settings
            List<user_article> articletaille = new List<user_article>();
            List<@case> lodplan = workerSelect.data.getCase().ToList();

            foreach (user_article ua in tempta)
            {
                foreach (@case cas in lodplan)
                {
                    
                    if (ua.Article_Type_Id == cas.Article_Type_Id && !articletaille.Contains(ua))
                    {
                        articletaille.Add(ua);
                    }
                }
            }
#else
            // articletaille contiendra les tempta correspondants à ce qui se trouve dans les cases de l'armoire identifié dans settings
            List<User_Article> articletaille = new List<User_Article>();
            List<Case> lodplan = workerSelect.data.getCase().ToList();

            foreach (User_Article ua in tempta)
            {
                foreach (Case cas in lodplan)
                {

                    if (ua.Article_Type_Id == cas.Article_Type_Id && !articletaille.Contains(ua))
                    {
                        articletaille.Add(ua);
                    }
                }
            }
#endif




#if EDMX
                List<ArmoireV3.Entities.epc> stock = new List<epc>();
#else
                List<ArmoireV3.Entities.Epc> stock = new List<Epc>();
#endif
                sampleData.Clear();
                listObjets.Items.Refresh();
                cred.Clear();
                cred = workerSelect.data.getUser_article().ToList();


                for (int i = 0; i < articletaille.Count; i++)
                {
                    // strtaille contient la taille de l'article présent dans l'armoire d'article_type articletaille[i].Article_Type_Id pour l'utilisateur logué
                    List<string> strtaille = cred.Where(z => z.User_Id == int.Parse(userId.Text) && z.Article_Type_Id == articletaille[i].Article_Type_Id).Select(w => w.Taille).ToList();
#if EDMX
                    List<epc> stockTotal = new List<epc>();
#else
                    List<Epc> stockTotal = new List<Epc>();
#endif                    
                    

                    int cre = 0;
                    //liste des credit par type d'article en fonction de l'utilisateur et de l'article type
                    t = cred.Where(r => r.User_Id == Login.user.Id && r.Article_Type_Id == articletaille[i].Article_Type_Id).ToList();
                    if (t.Count == 1)
                    {
                        cre = t[0].Credit_Restant > 0 ? t[0].Credit_Restant : 0;
                    }
                    else if (t.Count == 0)
                    {

                    }

                    else
                    {
                        /*
                         * //ERROR//
                         * À insérer dans les alertes:
                         * Erreur dans la base de données: deux user_article ont été trouvés pour le même couple user/article_type
                         */
                        //A voir comment le gerer
                        if (Properties.Settings.Default.UseDBGMSG)
                        {
#if OPTLANG
                            MessageBox.Show(Languages.errorArticleType);
#else
                            MessageBox.Show("Erreur dans les article_type");
#endif
                        }
                    }
                    

                    int g = articletaille[i].Article_Type_Id;
                    
                    
                    string pho = listArticleType.Where(x => x.Id == g).Select(x => x.Photo).FirstOrDefault();
                    string des = listArticleType.Where(x => x.Id == g).Select(x => x.Description).FirstOrDefault();
                    

                    pho =LoadExternalImagePath(pho);
                    
                    temp = new ObservableCollection<selectItem>()
                    {
                        new selectItem
                        {
                            Id=articletaille[i].Article_Type_Id,
                            SelectedID=i.ToString(),
                            Photo=pho,
                            Description = des,
                            Stock= 10,        // On ne tient compte nul part de ce stock //int.Parse(stock.Count.ToString()),
                            StockTotal="10", //stockTotal.Count.ToString(),
                            Credit= cre
                        }
                    };
                    if (temp.Count != 0)
                        sampleData.Add(temp[0]);
                    stock.Clear();
                    
                }
            this.listObjets.DataContext = sampleData;
        }

        //Chargement de ma selection
        //
        //A REVOIR SI CA FONCTIONNE BIEN
        //
        public void loadItemSelection(int iid, string tailleArticle)
        {
#if EDMX
            List<ArmoireV3.Entities.user> user = workerSelect.data.getUserById(int.Parse(userId.Text)).ToList();
#else
            List<ArmoireV3.Entities.User> user = workerSelect.data.getUserById(int.Parse(userId.Text)).ToList();

    #endif


            if (user.Count != 0)
            {
                if (sampleData2.Any(c => c.SelectedID == listObjets.SelectedIndex.ToString()))
                {
                    var item = sampleData2.FirstOrDefault(i => i.SelectedID == listObjets.SelectedIndex.ToString());

                    if (item != null)
                    {
                        item.Quantit++;
                    }
                }
                else
                {
#if EDMX
                    article_type at = listArticleType.FirstOrDefault(z => z.Id == ((selectItem)listObjets.SelectedItem).Id);
#else
                    Article_Type at = listArticleType.FirstOrDefault(z => z.Id == ((selectItem)listObjets.SelectedItem).Id);
#endif
                    if (at != null)
                    {
                        string longPathPhoto = LoadExternalImagePath(at.Photo);
                        temp2 = new ObservableCollection<ItemChoice>()
                        {
                            new ItemChoice
                            {
                                Id=at.Id,
                                SelectedID=listObjets.SelectedIndex.ToString(),
                                //Photo=at.Photo,
                                Photo=longPathPhoto,
                                Description = at.Description,
                                Taille = tailleArticle, // Taille prend la valeur de la taille de l'user ou la taille directement supérieure suivant le paramettre d'appel
                                Quantit=1                       
                            }
                        };
                        sampleData2.Add(temp2[0]); // sampleData2 contient la liste des articles sélectionnés dans la liste de gauche
                                                   // qui sont présents dans l'armoire
                                                   // et est utilisé pour remplir la liste de droite
                    }
                }
                this.listSelection.DataContext = sampleData2;
        }
            listSelection.Items.Refresh();
        }


        public void select_Art_Loaded(object sender, RoutedEventArgs e)  {      loadItem();      }

     
        private void imageButtonRetour_MouseUp(object sender, MouseButtonEventArgs e)      {   }

        private void imageButtonRetour_MouseDown(object sender, MouseButtonEventArgs e)    {   }

        private void imageButtonPrend_MouseDown(object sender, MouseButtonEventArgs e)    {
            imageButtonPrend.Height = 52;
            imageButtonPrend.Width = 54;
        }

        private void imageButtonPrend_MouseUp(object sender, MouseButtonEventArgs e) {  }

        private void imageButtonPrend_MouseLeave(object sender, MouseEventArgs e)  { }

        private void imageButtonPrend_MouseMove(object sender, MouseEventArgs e)  {
            imageButtonPrend.Height = 63;
            imageButtonPrend.Width = 64;
        }
    }
}
