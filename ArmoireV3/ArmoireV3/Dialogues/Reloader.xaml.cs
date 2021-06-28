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
using ArmoireV3.Controls;
using ArmoireV3.Inter;
using System.Collections.ObjectModel;
using System.IO;

namespace ArmoireV3.Dialogues
{
    /// <summary>
    /// Logique d'interaction pour Reloader.xaml
    /// </summary>
    public partial class Reloader : UserControl
    {

        public static readonly RoutedEvent OpenRelaodEvent = EventManager.RegisterRoutedEvent(
        "OpenReload", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Reloader));

        public static readonly RoutedEvent DecoRelaodEvent = EventManager.RegisterRoutedEvent(
        "DecoReload", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Reloader));

        public static readonly RoutedEvent DecoClickEvent = EventManager.RegisterRoutedEvent(
       "DecoClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Reloader));

        public static readonly RoutedEvent FermerReloadEvent = EventManager.RegisterRoutedEvent(
       "FermerReload", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Reloader));

   //     public static readonly RoutedEvent verifstateport_event = EventManager.RegisterRoutedEvent("verifstateport", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Reloader));
  //      public event RoutedEventHandler verifstateport { add { AddHandler(verifstateport_event, value); } remove { RemoveHandler(verifstateport_event, value); } }   

        //synchro syn = new synchro();
#if EDMX
        static Worker workerReloader = new Worker();
        List<article_type> listArticleType = workerReloader.data.getArticle_type().ToList();
        List<@case> listCase = workerReloader.data.getCase().ToList();
        List<@case> listTaille = workerReloader.data.getTaille().ToList();
#else
        static WorkerV3 workerReloader = new WorkerV3();
        List<Article_Type> listArticleType = workerReloader.data.getArticle_type().ToList();
        List<Case> listCase = workerReloader.data.getCase().ToList();
        List<Case> listTaille = workerReloader.data.getTaille().ToList();
#endif
        ObservableCollection<loadingPlan> Data = new ObservableCollection<loadingPlan>();
        ObservableCollection<loadingPlan> temp;
        public Led LedsDrv = new Led();

        public Reloader()
        {
            InitializeComponent();
            if (Properties.Settings.Default.Debug == true)
            {
                buttonFermer.IsEnabled = true;
#if OPTLANG
                
#endif
                buttonFermer.Visibility = Visibility.Visible;
            }
            else
            {
                buttonFermer.IsEnabled = false;
                buttonFermer.Visibility = Visibility.Hidden;
            }
#if EDMX
            List<article_type> newlistArticleType = new List<article_type>();
            foreach (article_type item in listArticleType)
#else
            List<Article_Type> newlistArticleType = new List<Article_Type>();
            foreach (Article_Type item in listArticleType)
#endif
            {
                if (ExistInArmoire(item))
                {
                    if (File.Exists(Environment.CurrentDirectory + item.Photo.Replace("/", "\\")))
                    {
                        item.Photo = Environment.CurrentDirectory + item.Photo.Replace("/", "\\");
                        //(new ImageSourceConverter()).ConvertFromString("pack://application:,,,/Images/Pantalon.JPG" ) as ImageSource)                        
                        newlistArticleType.Add(item);
                    }
                    else
                    {
                        item.Photo = "\\Images\\DEFAULT.JPG";
                        newlistArticleType.Add(item);
                    }
                }
            }
            listObjets.ItemsSource = newlistArticleType;

            for (int i = 0; i < newlistArticleType.Count;i++ )
            {
                Log.add(((Article_Type)listObjets.Items.GetItemAt(i)).Photo.ToString());
            }
            

            try
            {
                loadItem(0);
                //syn.init();
                LedsDrv.InitCarteLed("COM" + Properties.Settings.Default.COMled_portes.ToString());

                CleanLoadingPlan();
            }
            catch (Exception e)
            {
                if (Properties.Settings.Default.UseDBGMSG)
                    MessageBox.Show("Erreur reloader: " + e.Message);
            }

            //if (listObjets.Items.Count > 0) listObjets.SelectedItem = listObjets.SelectedIndex = 0;
            //if (listBoxTaille.Items.Count > 0) listBoxTaille.SelectedItem = listBoxTaille.SelectedIndex = 0;

#if EDMX
            List<content_arm> contentArm = workerReloader.data.getContent().Where(x => x.RFID_Reader == Properties.Settings.Default.NumArmoire).ToList();
#else
            List<Content_Arm> contentArm = workerReloader.data.getContent().Where(x => x.RFID_Reader == Properties.Settings.Default.NumArmoire).ToList();
#endif
            textBlockNbArticlesArmoire.Text = contentArm.Count().ToString();
            textBlockNbInconnusArmoire.Text = contentArm.Where(x => x.State == EtatArticle.INTRUS_INCONNUS).Count().ToString();
        }

        public event RoutedEventHandler DecoClick
        {

            add { AddHandler(DecoClickEvent, value); }

            remove { RemoveHandler(DecoClickEvent, value); }

        }


        public event RoutedEventHandler OpenReload
        {

            add { AddHandler(OpenRelaodEvent, value); }

            remove { RemoveHandler(OpenRelaodEvent, value); }

        }
        public event RoutedEventHandler DecoReload
        {

            add { AddHandler(DecoRelaodEvent, value); }

            remove { RemoveHandler(DecoRelaodEvent, value); }

        }
        public event RoutedEventHandler FermerReload  {   add { AddHandler(FermerReloadEvent, value); }     remove { RemoveHandler(FermerReloadEvent, value); }    }

        public void loadItem(int artid)
        {
            List<string> toto = new List<string>();
            if (artid == 0)
            {
                toto = listCase.Select(x => x.Taille).Distinct().ToList();
            }
            else
            {
                toto = listCase.Where(x => x.Article_Type_Id == artid).Select(x => x.Taille).Distinct().ToList();
            }
            if (toto.Count > 1)
            {
                toto.Sort();
            }

            Data.Clear();
            for (int i = 0; i < toto.Count(); i++)
            {

                //&& f.Armoire_ID==Properties.Settings.Default.NumArmoire
                //       //List<ArmoireV3.Entities.epc> tutua = listEPC.Where(t => t.Taille == tutu[j].Taille).ToList();

                temp = new ObservableCollection<loadingPlan>()
                {
                    new loadingPlan
                    {
                        Id=listCase[i].Bind_ID,
                        ArticleType=listCase[i].Article_Type_Id.ToString(),
                        Taille=toto[i],
                        NumArmoire= listCase[i].Armoire_ID,
                        MaxItem=listCase[i].Max_Item
                    }
                };
                Data.Add(temp[0]);
            }
            this.listBoxTaille.DataContext = Data;
        }
        
       
        private void ImageFailedEventHandler(object sender, ExceptionRoutedEventArgs e)
        {
            ((Image)sender).Source = new BitmapImage(new Uri("\\Images\\DEFAULT.JPG", UriKind.Relative));
        }

        private ImageSource GetBackGroundImageFromDir()
        {
            string ImagePathString = Environment.CurrentDirectory + "\\Images\\fondo.png";

            if (File.Exists(ImagePathString))
            {
                // Le fichier existe
                try
                {
                    return ((ImageSource)(System.ComponentModel.TypeDescriptor.GetConverter(typeof(ImageSource)).ConvertFromInvariantString(ImagePathString)));
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

        private ImageSource GetObjectImageFromDir(string ArticleTypePhoto)
        {
            string ImagePathString = /*Environment.CurrentDirectory +*/ ArticleTypePhoto.Replace("/","\\");

            if (File.Exists(ImagePathString))
            {
                // Le fichier existe
                try
                {
                    return ((ImageSource)(System.ComponentModel.TypeDescriptor.GetConverter(typeof(ImageSource)).ConvertFromInvariantString(ImagePathString)));
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
                    return ((ImageSource)(System.ComponentModel.TypeDescriptor.GetConverter(typeof(ImageSource)).ConvertFromInvariantString(ImagePathString)));
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

        public void CleanLoadingPlan()
        {
            // int BeginningCaseID=0;
            //if (listCase.Count > 24)
            //{
            //    if (listCase[1].Armoire_ID == Properties.Settings.Default.NumArmoire)
            //        BeginningCaseID = 0;
            //    else if (listCase[25].Armoire_ID == Properties.Settings.Default.NumArmoire)
            //        BeginningCaseID = 24;
            //}
            //else
            //{
            //    BeginningCaseID = 0;
            //}

            //for (int i = BeginningCaseID; i <= (((BeginningCaseID + 24) <= listCase.Count) ? (BeginningCaseID + 24) : listCase.Count); i++)
            //{
            for (int i = 1; i <= 24; i++)
            {
#if EDMX
                @case ctemp = listCase.Find(c => c.Bind_ID == i);
#else
                Case ctemp = listCase.Find(c => c.Bind_ID == i);
#endif
                 //i.ToString()+ "->"+ (((i-1) % (24))+1));
                switch (((i-1)%24)+1)
                {
                    case 1:
                        case1.Background = Brushes.White;
                        imgCase1.Source = GetBackGroundImageFromDir();
                        quantitCase1.Text = ctemp != null ? ctemp.Max_Item.ToString() : "0";
                        break;
                    case 2:
                        case2.Background = Brushes.White;
                        imgCase2.Source = GetBackGroundImageFromDir();;
                        quantitCase2.Text = ctemp != null ? ctemp.Max_Item.ToString() : "0";
                        break;
                    case 3:
                        case3.Background = Brushes.White;
                        imgCase3.Source = GetBackGroundImageFromDir();;
                        quantitCase3.Text = ctemp != null ? ctemp.Max_Item.ToString() : "0";
                        break;
                    case 4:
                        case4.Background = Brushes.White;
                        imgCase4.Source = GetBackGroundImageFromDir();;
                        quantitCase4.Text = ctemp != null ? ctemp.Max_Item.ToString() : "0";
                        break;
                    case 5:
                        case5.Background = Brushes.White;
                        imgCase5.Source = GetBackGroundImageFromDir();
                        quantitcase5.Text = ctemp != null ? ctemp.Max_Item.ToString() : "0";
                        break;
                    case 6:
                        Case6.Background = Brushes.White;
                        imgCase6.Source = GetBackGroundImageFromDir();
                        quantitCase6.Text = ctemp != null ? ctemp.Max_Item.ToString() : "0";
                        break;
                    case 7:
                        Case7.Background = Brushes.White;
                        imgCase7.Source = GetBackGroundImageFromDir();
                        quantitCase7.Text = ctemp != null ? ctemp.Max_Item.ToString() : "0";
                        break;
                    case 8:
                        Case8.Background = Brushes.White;
                        imgCase8.Source = GetBackGroundImageFromDir();
                        quantitCase8.Text = ctemp != null ? ctemp.Max_Item.ToString() : "0";
                        break;
                    case 9:
                        case9.Background = Brushes.White;
                        imgcase9.Source = GetBackGroundImageFromDir();
                        quantitCase9.Text = ctemp != null ? ctemp.Max_Item.ToString() : "0";
                        break;
                    case 10:
                        case10.Background = Brushes.White;
                        imgCase10.Source = GetBackGroundImageFromDir();
                        quantitCase10.Text = ctemp != null ? ctemp.Max_Item.ToString() : "0";
                        break;
                    case 11:
                        case11.Background = Brushes.White;
                        imgCase11.Source = GetBackGroundImageFromDir();
                        quantitCase11.Text = ctemp != null ? ctemp.Max_Item.ToString() : "0";
                        break;
                    case 12:
                        case12.Background = Brushes.White;
                        imgCase12.Source = GetBackGroundImageFromDir();
                        quantitCase12.Text = ctemp != null ? ctemp.Max_Item.ToString() : "0";
                        break;
                    case 13:
                        case13.Background = Brushes.White;
                        imgCase13.Source = GetBackGroundImageFromDir();
                        quantitCase13.Text = ctemp != null ? ctemp.Max_Item.ToString() : "0";
                        break;
                    case 14:
                        case14.Background = Brushes.White;
                        imgCase14.Source = GetBackGroundImageFromDir();
                        quantitCase14.Text = ctemp != null ? ctemp.Max_Item.ToString() : "0";
                        break;
                    case 15:
                        case15.Background = Brushes.White;
                        imgCase15.Source = GetBackGroundImageFromDir();
                        quantitCase15.Text = ctemp != null ? ctemp.Max_Item.ToString() : "0";
                        break;
                    case 16:
                        case16.Background = Brushes.White;
                        imgCase16.Source = GetBackGroundImageFromDir();
                        quantitCase16.Text = ctemp != null ? ctemp.Max_Item.ToString() : "0";
                        break;
                    case 17:
                        case17.Background = Brushes.White;
                        imgCase17.Source = GetBackGroundImageFromDir();
                        quantitCase17.Text = ctemp != null ? ctemp.Max_Item.ToString() : "0";
                        break;
                    case 18:
                        case18.Background = Brushes.White;
                        imgCase18.Source = GetBackGroundImageFromDir();
                        quantitCase18.Text = ctemp != null ? ctemp.Max_Item.ToString() : "0";
                        break;
                    case 19:
                        case19.Background = Brushes.White;
                        imgCase19.Source = GetBackGroundImageFromDir();
                        quantitCase19.Text = ctemp != null ? ctemp.Max_Item.ToString() : "0";
                        break;
                    case 20:
                        case20.Background = Brushes.White;
                        imgCase20.Source = GetBackGroundImageFromDir();
                        quantitCase20.Text = ctemp != null ? ctemp.Max_Item.ToString() : "0";
                        break;
                    case 21:
                        case21.Background = Brushes.White;
                        imgCase21.Source = GetBackGroundImageFromDir();
                        quantitCase21.Text = ctemp != null ? ctemp.Max_Item.ToString() : "0";
                        break;
                    case 22:
                        case22.Background = Brushes.White;
                        imgCase22.Source = GetBackGroundImageFromDir();
                        quantitCase22.Text = ctemp != null ? ctemp.Max_Item.ToString() : "0";
                        break;
                    case 23:
                        case23.Background = Brushes.White;
                        imgCase23.Source = GetBackGroundImageFromDir();
                        quantitCase23.Text = ctemp != null ? ctemp.Max_Item.ToString() : "0";
                        break;
                    case 24:
                        case24.Background = Brushes.White;
                        imgCase24.Source = GetBackGroundImageFromDir();
                        quantitCase24.Text = ctemp != null ? ctemp.Max_Item.ToString() : "0";
                        break;
                    default:
                        break;
                }
            }
        }
#if EDMX
        public bool ExistInArmoire(article_type at)
#else
        public bool ExistInArmoire(Article_Type at)
#endif
        {
            for (int i = 0; i < listCase.Count(); i++)
            {
                if (listCase[i].Article_Type_Id == at.Id)
                    return true;
            }
            return false;
        }

        public void DisplayCurrentLoadingPlan()
        {
            loadingPlan tutu = (loadingPlan)listBoxTaille.SelectedItem;
#if EDMX
            article_type art_typ = (article_type)listObjets.SelectedItem;
            List<@case> showcase = workerReloader.data.getCaseForReload(art_typ.Id, tutu.Taille).ToList();
#else
            Article_Type art_typ = (Article_Type)listObjets.SelectedItem;
            List<Case> showcase = workerReloader.data.getCaseForReload(art_typ.Id, tutu.Taille).ToList();
#endif

            calculCapacite(art_typ, tutu);

            for (int i = 0; i < showcase.Count(); i++)
            {
                //i.ToString()+ "->"+ (((i-1) % (24))+1));
                switch ((((showcase[i].Bind_ID)-1)%24)+1)
                {
                    case 1:
                        case1.Background = Brushes.GreenYellow;
                        imgCase1.Source = GetObjectImageFromDir(art_typ.Photo);
                        quantitCase1.Text = showcase[i].Max_Item.ToString();
                        quantitMaxCase1.Text = tutu.MaxItem.ToString();
                        LedsDrv.reloaderFlash("1", "A");
                        break;
                    case 2:
                        case2.Background = Brushes.GreenYellow;
                        imgCase2.Source = GetObjectImageFromDir(art_typ.Photo);
                        quantitCase2.Text = showcase[i].Max_Item.ToString();
                        quantitMaxCase2.Text = tutu.MaxItem.ToString();
                        LedsDrv.reloaderFlash("1", "B");
                        break;
                    case 3:
                        case3.Background = Brushes.GreenYellow;
                        imgCase3.Source = GetObjectImageFromDir(art_typ.Photo);
                        quantitCase3.Text = showcase[i].Max_Item.ToString();
                        quantitMaxCase3.Text = tutu.MaxItem.ToString();
                        LedsDrv.reloaderFlash("1", "C");
                        break;
                    case 4:
                        case4.Background = Brushes.GreenYellow;
                        imgCase4.Source = GetObjectImageFromDir(art_typ.Photo);
                        quantitCase4.Text = showcase[i].Max_Item.ToString();
                        quantitMaxCase4.Text = tutu.MaxItem.ToString();
                        LedsDrv.reloaderFlash("1", "D");
                        break;
                    case 5:
                        case5.Background = Brushes.GreenYellow;
                        imgCase5.Source = GetObjectImageFromDir(art_typ.Photo);
                        quantitcase5.Text = showcase[i].Max_Item.ToString();
                        quantitMaxCase5.Text = tutu.MaxItem.ToString();
                        LedsDrv.reloaderFlash("1", "H");
                        break;
                    case 6:
                        Case6.Background = Brushes.GreenYellow;
                        imgCase6.Source = GetObjectImageFromDir(art_typ.Photo);
                        quantitCase6.Text = showcase[i].Max_Item.ToString();
                        quantitMaxCase6.Text = tutu.MaxItem.ToString();
                        LedsDrv.reloaderFlash("1", "G");
                        break;
                    case 7:
                        Case7.Background = Brushes.GreenYellow;
                        imgCase7.Source = GetObjectImageFromDir(art_typ.Photo);
                        quantitCase7.Text = showcase[i].Max_Item.ToString();
                        quantitMaxCase7.Text = tutu.MaxItem.ToString();
                        LedsDrv.reloaderFlash("1", "F");
                        break;
                    case 8:
                        Case8.Background = Brushes.GreenYellow;
                        imgCase8.Source = GetObjectImageFromDir(art_typ.Photo);
                        quantitCase8.Text = showcase[i].Max_Item.ToString();
                        quantitMaxCase8.Text = tutu.MaxItem.ToString();
                        LedsDrv.reloaderFlash("1", "E");
                        break;
                    case 9:
                        case9.Background = Brushes.GreenYellow;
                        imgcase9.Source = GetObjectImageFromDir(art_typ.Photo);
                        quantitCase9.Text = showcase[i].Max_Item.ToString();
                        quantitMaxCase9.Text = tutu.MaxItem.ToString();
                        LedsDrv.reloaderFlash("1", "I");
                        break;
                    case 10:
                        case10.Background = Brushes.GreenYellow;
                        imgCase10.Source = GetObjectImageFromDir(art_typ.Photo);
                        quantitCase10.Text = showcase[i].Max_Item.ToString();
                        quantitMaxCase10.Text = tutu.MaxItem.ToString();
                        LedsDrv.reloaderFlash("1", "J");
                        break;
                    case 11:
                        case11.Background = Brushes.GreenYellow;
                        imgCase11.Source = GetObjectImageFromDir(art_typ.Photo);
                        quantitCase11.Text = showcase[i].Max_Item.ToString();
                        quantitMaxCase11.Text = tutu.MaxItem.ToString();
                        LedsDrv.reloaderFlash("1", "K");
                        break;
                    case 12:
                        case12.Background = Brushes.GreenYellow;
                        imgCase12.Source = GetObjectImageFromDir(art_typ.Photo);
                        quantitCase12.Text = showcase[i].Max_Item.ToString();
                        quantitMaxCase12.Text = tutu.MaxItem.ToString();
                        LedsDrv.reloaderFlash("1", "L");
                        break;
                    case 13:
                        case13.Background = Brushes.GreenYellow;
                        imgCase13.Source = GetObjectImageFromDir(art_typ.Photo);
                        quantitCase13.Text = showcase[i].Max_Item.ToString();
                        quantitMaxCase13.Text = tutu.MaxItem.ToString();
                        LedsDrv.reloaderFlash("2", "A");
                        break;
                    case 14:
                        case14.Background = Brushes.GreenYellow;
                        imgCase14.Source = GetObjectImageFromDir(art_typ.Photo);
                        quantitCase14.Text = showcase[i].Max_Item.ToString();
                        quantitMaxCase14.Text = tutu.MaxItem.ToString();
                        LedsDrv.reloaderFlash("2", "B");
                        break;
                    case 15:
                        case15.Background = Brushes.GreenYellow;
                        imgCase15.Source = GetObjectImageFromDir(art_typ.Photo);
                        quantitCase15.Text = showcase[i].Max_Item.ToString();
                        quantitMaxCase15.Text = tutu.MaxItem.ToString();
                        LedsDrv.reloaderFlash("2", "C");
                        break;
                    case 16:
                        case16.Background = Brushes.GreenYellow;
                        imgCase16.Source = GetObjectImageFromDir(art_typ.Photo);
                        quantitCase16.Text = showcase[i].Max_Item.ToString();
                        quantitMaxCase16.Text = tutu.MaxItem.ToString();
                        LedsDrv.reloaderFlash("2", "D");
                        break;
                    case 17:
                        case17.Background = Brushes.GreenYellow;
                        imgCase17.Source = GetObjectImageFromDir(art_typ.Photo);
                        quantitCase17.Text = showcase[i].Max_Item.ToString();
                        QuantitMaxCase17.Text = tutu.MaxItem.ToString();
                        LedsDrv.reloaderFlash("2", "H");
                        break;
                    case 18:
                        case18.Background = Brushes.GreenYellow;
                        imgCase18.Source = GetObjectImageFromDir(art_typ.Photo);
                        quantitCase18.Text = showcase[i].Max_Item.ToString();
                        quantitMaxCase18.Text = tutu.MaxItem.ToString();
                        LedsDrv.reloaderFlash("2", "G");
                        break;
                    case 19:
                        case19.Background = Brushes.GreenYellow;
                        imgCase19.Source = GetObjectImageFromDir(art_typ.Photo);
                        quantitCase19.Text = showcase[i].Max_Item.ToString();
                        quantitMaxCase19.Text = tutu.MaxItem.ToString();
                        LedsDrv.reloaderFlash("2", "F");
                        break;
                    case 20:
                        case20.Background = Brushes.GreenYellow;
                        imgCase20.Source = GetObjectImageFromDir(art_typ.Photo);
                        quantitCase20.Text = showcase[i].Max_Item.ToString();
                        quantitMaxCase20.Text = tutu.MaxItem.ToString();
                        LedsDrv.reloaderFlash("2", "E");
                        break;
                    case 21:
                        case21.Background = Brushes.GreenYellow;
                        imgCase21.Source = GetObjectImageFromDir(art_typ.Photo);
                        quantitCase21.Text = showcase[i].Max_Item.ToString();
                        quantitMaxCase21.Text = tutu.MaxItem.ToString();
                        LedsDrv.reloaderFlash("2", "I");
                        break;
                    case 22:
                        case22.Background = Brushes.GreenYellow;
                        imgCase22.Source = GetObjectImageFromDir(art_typ.Photo);
                        quantitCase22.Text = showcase[i].Max_Item.ToString();
                        quantitMaxCase22.Text = tutu.MaxItem.ToString();
                        LedsDrv.reloaderFlash("2", "J");
                        break;
                    case 23:
                        case23.Background = Brushes.GreenYellow;
                        imgCase23.Source = GetObjectImageFromDir(art_typ.Photo);
                        quantitCase23.Text = showcase[i].Max_Item.ToString();
                        quantitMaxCase23.Text = tutu.MaxItem.ToString();
                        LedsDrv.reloaderFlash("2", "K");
                        break;
                    case 24:
                        case24.Background = Brushes.GreenYellow;
                        imgCase24.Source = GetObjectImageFromDir(art_typ.Photo);
                        quantitCase24.Text = showcase[i].Max_Item.ToString();
                        quantitMaxCase24.Text = tutu.MaxItem.ToString();
                        LedsDrv.reloaderFlash("2", "L");
                        break;
                    default:
                        break;
                    //Border currentCell = (Border)FindName("rectangleCase" + k.ToString());

                }
            }
        }

        private void listObjets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listObjets.SelectedItem == null)
            {

            }
            else
            {
#if EDMX
                int test = ((article_type)listObjets.SelectedItem).Id;
#else
                int test = ((Article_Type)listObjets.SelectedItem).Id;
#endif
                loadItem(test);
                //Clear des leds
                LedsDrv.shut();
                //Déselection des cases
                CleanLoadingPlan();

            }

        }

        private void listBoxTaille_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listObjets.SelectedItem == null || listBoxTaille.SelectedItem == null)
            {

            }
            else
            {
                LedsDrv.shut();
                CleanLoadingPlan();
                DisplayCurrentLoadingPlan();
            }
        }

        private void listBoxTaille_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                int i = 0;
                foreach (object elem in listBoxTaille.ItemsSource)
                {

                    if (((System.Windows.Controls.TextBox)e.OriginalSource).Text /*e.OriginalSource.ToString()*/ == ((ArmoireV3.Controls.loadingPlan)elem).Taille)
                    {
                        listBoxTaille.SelectedIndex = i;
                        break;
                    }
                    i++;

                }
                listBoxTaille_SelectionChanged(null, null);
            }
            catch(Exception ex)
            {
                Log.add("Erreur dans la méthode ListBoxTaille_GotFocus : " + ex.Message);
            }
        }

        private void buttonOuvrir_Click(object sender, RoutedEventArgs e)
        {
            buttonOuvrir.Visibility = Visibility.Hidden;
            buttonNouveauPlan.Visibility = Visibility.Hidden;
            buttonDeconnexion.Visibility = Visibility.Hidden;
            RaiseEvent(new RoutedEventArgs(OpenRelaodEvent, this));
      //      RaiseEvent(new RoutedEventArgs(verifstateport_event, this));
        }

        private void buttonDeconnexion_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(DecoClickEvent, this));
            LedsDrv.shut();
        }

        private void case_MouseDown(object sender, RoutedEventArgs e)
        {
            switch (((Border)sender).Name)
            {
                case "case1": selectArticleTypeAndSize(1); break;
                case "case2": selectArticleTypeAndSize(2); break;
                case "case3": selectArticleTypeAndSize(3); break;
                case "case4": selectArticleTypeAndSize(4); break;
                case "case5": selectArticleTypeAndSize(5); break;
                case "Case6": selectArticleTypeAndSize(6); break;
                case "Case7": selectArticleTypeAndSize(7); break;
                case "Case8": selectArticleTypeAndSize(8); break;
                case "case9": selectArticleTypeAndSize(9); break;
                case "case10": selectArticleTypeAndSize(10); break;
                case "case11": selectArticleTypeAndSize(11); break;
                case "case12": selectArticleTypeAndSize(12); break;
                case "case13": selectArticleTypeAndSize(13); break;
                case "case14": selectArticleTypeAndSize(14); break;
                case "case15": selectArticleTypeAndSize(15); break;
                case "case16": selectArticleTypeAndSize(16); break;
                case "case17": selectArticleTypeAndSize(17); break;
                case "case18": selectArticleTypeAndSize(18); break;
                case "case19": selectArticleTypeAndSize(19); break;
                case "case20": selectArticleTypeAndSize(20); break;
                case "case21": selectArticleTypeAndSize(21); break;
                case "case22": selectArticleTypeAndSize(22); break;
                case "case23": selectArticleTypeAndSize(23); break;
                case "case24": selectArticleTypeAndSize(24); break;
            }
        }

        private void selectArticleTypeAndSize(int numCase)
        {
#if EDMX
            @case currentCase = listCase.FirstOrDefault(x => x.Bind_ID == numCase);
#else
            Case currentCase = listCase.FirstOrDefault(x => x.Bind_ID == numCase);
#endif

            if (currentCase != null)
            {
#if EDMX
                article_type artType = listArticleType.FirstOrDefault(x => x.Id == currentCase.Article_Type_Id);
#else
                Article_Type artType = listArticleType.FirstOrDefault(x => x.Id == currentCase.Article_Type_Id);
#endif
                loadItem(artType.Id);
                loadingPlan ldgPlan = Data.FirstOrDefault(x => x.Taille == currentCase.Taille);
                if (artType != null && ldgPlan != null)
                {
                    listObjets.SelectedItem = artType;
                    
                    foreach (loadingPlan lp in listBoxTaille.Items)
                    {
                        if (lp.ArticleType == ldgPlan.ArticleType && lp.Id == ldgPlan.Id && lp.MaxItem == ldgPlan.MaxItem && lp.NumArmoire == ldgPlan.NumArmoire && lp.Taille == ldgPlan.Taille)
                        {
                            listBoxTaille.SelectedItem = lp;
                            break;
                        }
                    }
                    
                    calculCapacite(artType, ldgPlan);
                }
            }
        }

#if EDMX
        private void calculCapacite(article_type artType, loadingPlan ldgPlan)
#else
        private void calculCapacite(Article_Type artType, loadingPlan ldgPlan)
#endif
        {
            try
            {
                int nbTagPresents = 0;
#if EDMX

                List<@case> cases = workerReloader.data.getCaseForReload(artType.Id, ldgPlan.Taille).ToList();
                List<epc> listTagsT = workerReloader.data.getEpcByArticleIdAndUniqueSize(artType.Id, ldgPlan.Taille).ToList(); //.Where(x => x.Armoire_ID == Properties.Settings.Default.NumArmoire && x.State != 1).ToList();
                List<epc> listTags = listTagsT.Where(x => (int.Parse(x.Last_Reader)) == Properties.Settings.Default.NumArmoire && x.State != 1).ToList();
                List<content_arm> listContentArm = workerReloader.data.getContent().Where(x => x.RFID_Reader == Properties.Settings.Default.NumArmoire).ToList();
                foreach (epc e in listTags)
#else
                List<Case> cases = workerReloader.data.getCaseForReload(artType.Id, ldgPlan.Taille).ToList();
                List<Epc> listTagsT = workerReloader.data.getEpcByArticleIdAndUniqueSize(artType.Id, ldgPlan.Taille).ToList(); //.Where(x => x.Armoire_ID == Properties.Settings.Default.NumArmoire && x.State != 1).ToList();
                List<Epc> listTags = listTagsT.Where(x => (int.Parse(x.Last_Reader)) == Properties.Settings.Default.NumArmoire && x.State != 1).ToList();
                List<Content_Arm> listContentArm = workerReloader.data.getContent().Where(x => x.RFID_Reader == Properties.Settings.Default.NumArmoire).ToList();
                foreach (Epc e in listTags)
#endif
                {
                    if (listContentArm.Where(x => x.Epc == e.Tag).Count() > 0)
                    {
                        nbTagPresents++;
                    }
                }
                int capaciteTotale = 0;
#if EDMX
               foreach (@case c in cases)
#else
                foreach (Case c in cases)
#endif
                {
                    capaciteTotale += c.Max_Item;
                }

                txtCapaciteTotale.Text = capaciteTotale.ToString();
                txtCapaciteActuelle.Text = nbTagPresents.ToString();
            }
            catch (Exception e)
            {
                Log.add("Erreur CalculCapacite: " + e.Message);
                txtCapaciteTotale.Text = "Erreur";
                txtCapaciteActuelle.Text = "Erreur";
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void buttonNouveauPlan_Click(object sender, RoutedEventArgs e)
        {
            // Bloque les boutons et met en attente.
            buttonDeconnexion.IsEnabled = false;
            buttonOuvrir.IsEnabled = false;
            buttonNouveauPlan.IsEnabled = false;
#if OPTLANG
            buttonNouveauPlan.Content = LanguageHelper.wait;
#else
            buttonNouveauPlan.Content = "Patientez!!!";
#endif

            // Lance la synchro avec le serveur.
            try
            {
                bool b = false;
                Synchro syncpc = new Synchro();
                b = syncpc.acase();
                if (b == true)
                {
                    Log.add("SynchroMiseAJourPlCh table Case ok");
                    b = syncpc.articleTaille();
                    if (b == true)
                    {
                        Log.add("SynchroMiseAJourPlCh table ArticleTaille ok");
                        syncpc.articleType();
                        if (b == true)
                            Log.add("SynchroMiseAJourPlCh table ArticleType ok");
                    }
                }
                if (b == true)
                {
#if OPTLANG
                    MessageBox.Show(Languages.evalContent);
#else
                    MessageBox.Show("Veuillez ouvrir puis fermer l'armoire pour ré-évaluer le contenu, Merci.");
#endif
                }
                else
                {
#if OPTLANG
                    MessageBox.Show(Languages.problemTryAgain);
#else
                    MessageBox.Show("Un probleme a été rencontré, veuillez re-essayer ultérieurement.");
#endif
                    Log.add("Echec lors d'une SynchroMiseAJourPlCh");
                }
            }
            catch
            {
#if OPTLANG
                    MessageBox.Show(Languages.failedSync);
#else
                    MessageBox.Show("Echec de La synchronisation");
#endif
            }
            
            // Relance la fenêtre de rechargement.

            // Rétablie les boutons 
            buttonDeconnexion.IsEnabled = true;
            buttonOuvrir.IsEnabled = true;
            buttonNouveauPlan.IsEnabled = true;
#if OPTLANG
            buttonNouveauPlan.Content = LanguageHelper.newmap;
#else
            buttonNouveauPlan.Content = "Nouveau Plan";
#endif
        }

        private void buttonFermer_Click(object sender, RoutedEventArgs e)
        {
            // Simulation de la fermeture de porte pour tests
            RaiseEvent(new RoutedEventArgs(FermerReloadEvent, this));
            
        }

       
    }
}
