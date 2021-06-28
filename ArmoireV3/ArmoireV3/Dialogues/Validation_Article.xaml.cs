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
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.IO;

namespace ArmoireV3.Dialogues
{
    /// <summary>
    /// Logique d'interaction pour Validation_Article.xaml
    /// </summary>
    public partial class Validation_Article : UserControl
    {
        public List<ListEpcCount> listepccount = new List<ListEpcCount>();

#if EDMX
        static Worker workerValid = new Worker();
        List<ArmoireV3.Entities.article_type> listArticleType = workerValid.data.getArticle_type().ToList();
        List<ArmoireV3.Entities.epc> listEPC = new List<epc>();
        List<ArmoireV3.Entities.@case> listCase = workerValid.data.getCase().ToList();
        //List<ArmoireV3.Entities.armoire> listArmoire = workerValid.data.getArmoire().ToList();
        List<ArmoireV3.Entities.user_article> listUserArticle = new List<user_article>();
        //  public List<ArmoireV3.Entities.epc> epcRetirer = new List<epc>();
        List<epc> epctemp2 = new List<epc>();
        public List<epc> toust = null;

#else
        static WorkerV3 workerValid = new WorkerV3();
        List<ArmoireV3.Entities.Article_Type> listArticleType = workerValid.data.getArticle_type().ToList();
        List<ArmoireV3.Entities.Epc> listEPC = new List<Epc>();
        List<ArmoireV3.Entities.Case> touteslescases = workerValid.data.getCase().ToList();
        //List<ArmoireV3.Entities.armoire> listArmoire = workerValid.data.getArmoire().ToList();
        List<ArmoireV3.Entities.User_Article> listUserArticle = new List<User_Article>();
        //  public List<ArmoireV3.Entities.epc> epcRetirer = new List<epc>();
       public  List<Epc> EpcsTailleType = new List<Epc>();
       // List<Epc> epcs = new List<Epc>();
#endif

        public  bool Ldoor = false;
        public  bool Rdoor = false;
        public Led leds = new Led();
        public int nbitemout = 0;

        string userid = "";
        DispatcherTimer verifMaxTimeByPhase;

        public static readonly RoutedEvent OpenEvent = EventManager.RegisterRoutedEvent(
 "open", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Validation_Article));

        public static readonly RoutedEvent BackToLoginEvent = EventManager.RegisterRoutedEvent(
     "BackToLogin", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Validation_Article));

        public static readonly RoutedEvent BackEvent4 = EventManager.RegisterRoutedEvent(
    "Back", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Validation_Article));


        public static readonly RoutedEvent verifstateport_event = EventManager.RegisterRoutedEvent(
 "verifstateport", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Validation_Article));


        public Validation_Article()
        {
            InitializeComponent();
            gridOpenCabinet.Visibility = Visibility.Hidden;
            userid = textBoxUserid.Text;
            leds.InitCarteLed("COM" + Properties.Settings.Default.COMled_portes.ToString());
            listEPC = workerValid.data.getEPC().Where(x => x.Actif == 1).ToList();

            initVerifMaxTimeByPhase();
        }

        void initVerifMaxTimeByPhase()
        {
            verifMaxTimeByPhase = new DispatcherTimer();
            verifMaxTimeByPhase.Tick += new EventHandler(verifMaxTimeByPhase_Tick);
            verifMaxTimeByPhase.Interval = new TimeSpan(0, 0, Properties.Settings.Default.MaxTimeByPhase); //360 = 6 minutes
            verifMaxTimeByPhase.Start();
        }

        void verifMaxTimeByPhase_Tick(object sender, object e) {
            verifMaxTimeByPhase.Stop();
            leds.shut();
            RaiseEvent(new RoutedEventArgs(BackToLoginEvent, this));
        }

        public event RoutedEventHandler verifstateport    {     add { AddHandler(verifstateport_event, value); }     remove { RemoveHandler(verifstateport_event, value); }    }

        public event RoutedEventHandler open {   add { AddHandler(OpenEvent, value); }     remove { RemoveHandler(OpenEvent, value); }    }

        public event RoutedEventHandler BackToLogin { add { AddHandler(BackToLoginEvent, value); } remove { RemoveHandler(BackToLoginEvent, value); } }


        public event RoutedEventHandler Back    {      add { AddHandler(BackEvent4, value); }        remove { RemoveHandler(BackEvent4, value); }    }

        private void buttonOuvrir_Click(object sender, RoutedEventArgs e)   {

            verifMaxTimeByPhase.Stop();
            verifMaxTimeByPhase.Tick -= new EventHandler(verifMaxTimeByPhase_Tick);

            ReaderControl.tagsreported = false;
            tohide.Visibility = Visibility.Hidden;
            buttonOuvrir.Visibility = Visibility.Hidden;
            buttonRetourRetrait.Visibility = Visibility.Hidden;
            buttonDeconnexion.Visibility = Visibility.Hidden;
            gridOpenCabinet.Visibility = Visibility.Visible;


            RaiseEvent(new RoutedEventArgs(OpenEvent, this));
            RaiseEvent(new RoutedEventArgs(verifstateport_event, this));


        }

        //Appel le usercontrle selection_Article dans le mainWindow
        private void buttonRetourRetrait_Click(object sender, RoutedEventArgs e)
        {
            verifMaxTimeByPhase.Stop();
            verifMaxTimeByPhase.Tick -= new EventHandler(verifMaxTimeByPhase_Tick);

            leds.shut();
            RaiseEvent(new RoutedEventArgs(BackEvent4, this));
        }
        //Appel le usercontrle login dans le mainWindow
        private void buttonDeconnexion_Click(object sender, RoutedEventArgs e)
        {
            verifMaxTimeByPhase.Stop();
            leds.shut();
            RaiseEvent(new RoutedEventArgs(BackToLoginEvent, this));
        }

        private void ImageFailedEventHandler(object sender, ExceptionRoutedEventArgs e)
        {
            ((Image)sender).Source = new BitmapImage(new Uri("\\Images\\DEFAULT.JPG", UriKind.Relative));
        }

        private ImageSource GetObjectImageFromDir(string ArticleTypePhoto)
        {
            string ImagePathString = Environment.CurrentDirectory + ArticleTypePhoto.Replace("/", "\\");

            if (File.Exists(ImagePathString))
            {
                // Le fichier existe
                try
                {
                    return ((ImageSource)(System.ComponentModel.TypeDescriptor.GetConverter(typeof(ImageSource)).ConvertFromInvariantString(ImagePathString)));
                }
                catch (Exception e)
                {
                    Log.add(" Le chargement du fichier image " + ImagePathString + " a échoué"+e.Message);
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

        private void mainValid_Loaded(object sender, RoutedEventArgs e)    {
            try     {
                
#if EDMX
                List<epc> tilp = new List<epc>();

                List<ArmoireV3.Entities.epc> epcuserchoice = new List<epc>();

                List<ArmoireV3.Entities.article_type> articletypeid = listArticleType;
                List<ArmoireV3.Entities.epc> epc = new List<epc>();
                List<ArmoireV3.Entities.@case> whatcase = listCase;
#else
                List<ArmoireV3.Entities.Epc> epcuserchoice = new List<Epc>();

                List<ArmoireV3.Entities.Article_Type> articletypeid = listArticleType;
                List<Epc> epcsAretirer = new List<Epc>();
                List<Case> casespartaille = touteslescases;
#endif
                ItemChoice test = (ItemChoice)listSelectionValidation.Items.GetItemAt(0);
                Ldoor = false;
                Rdoor = false;

                //Retourne le nombre d'item/ArticleType dans ma selection
                for (int i = 0; i < listSelectionValidation.Items.Count; i++)     {
                    test = (ItemChoice)listSelectionValidation.Items.GetItemAt(i);
                    articletypeid = listArticleType.Where(t => t.Id == test.Id).ToList();
                    //Retourne la quantité de chaque artcile Type
                    for (int j = 0; j < test.Quantit; j++)    {

                        //Retourne tous les epc avec l'article type de l'item selectionné
                        //   List<epc>   epctemp = listEPC.Where(t => t.Article_Type_ID == articletypeid[0].Id).ToList();

                        //Retourne  tous les epc avec l'article type de l'item selectionné et en fonction de la taille de l'utilisateur
                        EpcsTailleType = listEPC.Where(z => z.Taille == test.Taille && z.Article_Type_ID == articletypeid[0].Id).ToList();


                        var tlpepc = EpcsTailleType.Except(epcsAretirer).FirstOrDefault();

                        epcsAretirer.Add(tlpepc);
                        //Ajout des items et leur quantité que l'utilisateur a selectionné
                        //A continuer
                        List<ListEpcCount> tem = new List<ListEpcCount>();


                        if (listepccount.Any(c => c.Type == EpcsTailleType[j].Article_Type_ID))
                        {
                            ListEpcCount item = listepccount.Where(f => f.Type == EpcsTailleType[j].Article_Type_ID).FirstOrDefault();
                            if (item != null)
                            {
                                listepccount.Where(x => x.Type == item.Type).FirstOrDefault().Count++;
                            }

                        }
                        else
                        {


                            tem = new List<ListEpcCount>()
                            {
                                new ListEpcCount
                                {
                                    Count=1,
                                    Type=EpcsTailleType[j].Article_Type_ID,
                                    Taille=EpcsTailleType[j].Taille
                                }
                            };

                            listepccount.Add(tem[0]);
                        }

                    }
                    
                   
                    //j'ai la liste des EPC a retourner

                    int m = 0;
                    for (int k = 0; k < epcsAretirer.Count; k++)
                    {
                        m++;
                        //pour chaque item regarder dans article type a quel case cela correspond

                        casespartaille = touteslescases.Where(p => p.Taille == epcsAretirer[k].Taille).ToList();
#if EDMX
                        List<@case> whatcasetype = whatcase.Where(y => y.Article_Type_Id == epc[k].Article_Type_ID).ToList();

                        List<article_type> photo = workerValid.data.getArticle_type().ToList();
                        List<article_type> phot = photo.Where(u => u.Id == whatcasetype[0].Article_Type_Id).ToList();
#else
                        List<Case> casespartypetaille = casespartaille.Where(y => y.Article_Type_Id == epcsAretirer[k].Article_Type_ID).ToList();

                        List<Article_Type> photo = workerValid.data.getArticle_type().ToList();
                        List<Article_Type> phot = photo.Where(u => u.Id == casespartypetaille[0].Article_Type_Id).ToList();
#endif


                        //Affiche la case en fonction de la case_id
                        for (int c = 0; c < casespartypetaille.Count; c++)
                        {


                            switch ((((casespartypetaille[c].Bind_ID) - 1) % 24) + 1)
                            {

                                case 1:
                                    rectangleCase1.Background = Brushes.DarkOrange;
                                    t1.Text = m.ToString();
                                    i1.Source = GetObjectImageFromDir(phot[0].Photo);
                                    leds.userFlash("1", "A");
                                    Ldoor = true;
                                    break;
                                case 2:
                                    rectangleCase2.Background = Brushes.DarkOrange;
                                    t2.Text = m.ToString();
                                    i2.Source = GetObjectImageFromDir(phot[0].Photo);
                                    leds.userFlash("1", "B");
                                    Ldoor = true;
                                    break;
                                case 3:
                                    rectangleCase3.Background = Brushes.DarkOrange;
                                    t3.Text = m.ToString();
                                    i3.Source = GetObjectImageFromDir(phot[0].Photo);
                                    leds.userFlash("1", "C");
                                    Rdoor = true;
                                    break;
                                case 4:
                                    rectangleCase4.Background = Brushes.DarkOrange;
                                    t4.Text = m.ToString();
                                    i4.Source = GetObjectImageFromDir(phot[0].Photo);
                                    leds.userFlash("1", "D");
                                    Rdoor = true;
                                    break;
                                case 5:
                                    rectangleCase5.Background = Brushes.DarkOrange;
                                    t5.Text = m.ToString();
                                    i5.Source = GetObjectImageFromDir(phot[0].Photo);
                                    leds.userFlash("1", "H");
                                    Ldoor = true;
                                    break;
                                case 6:
                                    rectangleCase6.Background = Brushes.DarkOrange;
                                    t6.Text = m.ToString();
                                    i6.Source = GetObjectImageFromDir(phot[0].Photo);
                                    leds.userFlash("1", "G");
                                    Ldoor = true;
                                    break;
                                case 7:
                                    rectangleCase7.Background = Brushes.DarkOrange;
                                    t7.Text = m.ToString();
                                    i7.Source = GetObjectImageFromDir(phot[0].Photo);
                                    leds.userFlash("1", "F");
                                    Rdoor = true;
                                    break;
                                case 8:
                                    rectangleCase8.Background = Brushes.DarkOrange;
                                    t8.Text = m.ToString();
                                    i8.Source = GetObjectImageFromDir(phot[0].Photo);
                                    leds.userFlash("1", "E");
                                    Rdoor = true;
                                    break;
                                case 9:
                                    rectangleCase9.Background = Brushes.DarkOrange;
                                    t9.Text = m.ToString();
                                    i9.Source = GetObjectImageFromDir(phot[0].Photo);
                                    leds.userFlash("1", "I");
                                    Ldoor = true;
                                    break;
                                case 10:
                                    rectangleCase10.Background = Brushes.DarkOrange;
                                    t10.Text = m.ToString();
                                    i10.Source = GetObjectImageFromDir(phot[0].Photo);
                                    leds.userFlash("1", "J");
                                    Ldoor = true;
                                    break;
                                case 11:
                                    rectangleCase11.Background = Brushes.DarkOrange;
                                    t11.Text = m.ToString();
                                    i11.Source = GetObjectImageFromDir(phot[0].Photo);
                                    leds.userFlash("1", "K");
                                    Rdoor = true;
                                    break;
                                case 12:
                                    rectangleCase12.Background = Brushes.DarkOrange;
                                    t12.Text = m.ToString();
                                    i12.Source = GetObjectImageFromDir(phot[0].Photo);
                                    leds.userFlash("1", "L");
                                    Rdoor = true;
                                    break;
                                case 13:
                                    rectangleCase13.Background = Brushes.DarkOrange;
                                    t13.Text = m.ToString();
                                    i13.Source = GetObjectImageFromDir(phot[0].Photo);
                                    leds.userFlash("2", "A");
                                    Ldoor = true;
                                    break;
                                case 14:
                                    rectangleCase14.Background = Brushes.DarkOrange;
                                    t14.Text = m.ToString();
                                    i14.Source = GetObjectImageFromDir(phot[0].Photo);
                                    leds.userFlash("2", "B");
                                    Ldoor = true;
                                    break;
                                case 15:
                                    rectangleCase15.Background = Brushes.DarkOrange;
                                    t15.Text = m.ToString();
                                    i15.Source = GetObjectImageFromDir(phot[0].Photo);
                                    leds.userFlash("2", "C");
                                    Rdoor = true;
                                    break;
                                case 16:
                                    rectangleCase16.Background = Brushes.DarkOrange;
                                    t16.Text = m.ToString();
                                    i16.Source = GetObjectImageFromDir(phot[0].Photo);
                                    leds.userFlash("2", "D");
                                    Rdoor = true;
                                    break;
                                case 17:
                                    rectangleCase17.Background = Brushes.DarkOrange;
                                    t17.Text = m.ToString();
                                    i17.Source = GetObjectImageFromDir(phot[0].Photo);
                                    leds.userFlash("2", "H");
                                    Ldoor = true;
                                    break;
                                case 18:
                                    rectangleCase18.Background = Brushes.DarkOrange;
                                    t18.Text = m.ToString();
                                    i18.Source = GetObjectImageFromDir(phot[0].Photo);
                                    leds.userFlash("2", "G");
                                    Ldoor = true;
                                    break;
                                case 19:
                                    rectangleCase19.Background = Brushes.DarkOrange;
                                    t19.Text = m.ToString();
                                    i19.Source = GetObjectImageFromDir(phot[0].Photo);
                                    leds.userFlash("2", "F");
                                    Rdoor = true;
                                    break;
                                case 20:
                                    rectangleCase20.Background = Brushes.DarkOrange;
                                    t20.Text = m.ToString();
                                    i20.Source = GetObjectImageFromDir(phot[0].Photo);
                                    leds.userFlash("2", "E");
                                    Rdoor = true;
                                    break;
                                case 21:
                                    rectangleCase21.Background = Brushes.DarkOrange;
                                    t21.Text = m.ToString();
                                    i21.Source = GetObjectImageFromDir(phot[0].Photo);
                                    leds.userFlash("2", "I");
                                    Ldoor = true;
                                    break;
                                case 22:
                                    rectangleCase22.Background = Brushes.DarkOrange;
                                    t22.Text = m.ToString();
                                    i22.Source = GetObjectImageFromDir(phot[0].Photo);
                                    leds.userFlash("2", "J");
                                    Ldoor = true;
                                    break;
                                case 23:
                                    rectangleCase23.Background = Brushes.DarkOrange;
                                    t23.Text = m.ToString();
                                    i23.Source = GetObjectImageFromDir(phot[0].Photo);
                                    leds.userFlash("2", "K");
                                    Rdoor = true;
                                    break;
                                case 24:
                                    rectangleCase24.Background = Brushes.DarkOrange;
                                    t24.Text = m.ToString();
                                    i24.Source = GetObjectImageFromDir(phot[0].Photo);
                                    leds.userFlash("2", "L");
                                    Rdoor = true;
                                    break;
                                default:
                                    break;
                                //Border currentCell = (Border)FindName("rectangleCase" + k.ToString());

                            }
                        }
                    }

                    epcsAretirer.Clear();

                }
            }
            catch(Exception ex)
            {
                if (Properties.Settings.Default.UseDBGMSG)
                {
                    MessageBox.Show(ex.Message);
                }
            }

        }
    }
}
