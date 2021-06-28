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
using System.Windows.Threading;
using System.Windows.Media.Animation;
using ArmoireV3.Entities;

namespace ArmoireV3.Dialogues
{
    /// <summary>
    /// Logique d'interaction pour Scan.xaml
    /// </summary>
    public partial class CompteRebours : UserControl
    {
        int attente=0;
        public static DispatcherTimer timer=new DispatcherTimer();
        public static readonly RoutedEvent FinCompteRebours = EventManager.RegisterRoutedEvent("FinCompteRebours", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MainWindow));
  //      public static readonly RoutedEvent ScanTerminerEvent = EventManager.RegisterRoutedEvent( "FinScan", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Selection_Article));
        public int Attente {
            get { return attente; }
            set { attente = value; tempsRestant.Text = Attente.ToString(); } }

        public CompteRebours()
        {
            InitializeComponent();
     //       int division_s = (Properties.Settings.Default.ReadingTimeArmoire-2) / 9;
     //       int division_ms = ((Properties.Settings.Default.ReadingTimeArmoire -2)* 1000 / 9) - ((Properties.Settings.Default.ReadingTimeArmoire-2) / 9) * 1000;
            //Log.add("s=" + division_s + " ms=" + division_ms);
       //     timer.Interval = new TimeSpan(0, 0, 0, division_s, division_ms);
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += new EventHandler(clock_Tick);
            //Attente = 30;
        }

        public event RoutedEventHandler TerminerCompteRebours {
            add { AddHandler(FinCompteRebours, value); }
            remove { RemoveHandler(FinCompteRebours, value); }
        }

        void clock_Tick(object sender, EventArgs e) {

            if (Attente <= 0)      {
                ((DispatcherTimer)sender).Stop();
                RaiseEvent(new RoutedEventArgs(FinCompteRebours, this));
            }
            else
            {
                Attente--;
            }
        }
        public void init(int attente)
        {
            Log.add("Init Compte à rebours");
            timer.Stop();
            Attente = attente;
            timer.Start();
        }
        public class GifImage : Image
        {

            public int FrameIndex
            {

                get { return (int)GetValue(FrameIndexProperty); }

                set { SetValue(FrameIndexProperty, value); }

            }



            public static readonly DependencyProperty FrameIndexProperty =

                DependencyProperty.Register("FrameIndex", typeof(int), typeof(GifImage), new UIPropertyMetadata(0, new PropertyChangedCallback(ChangingFrameIndex)));



            static void ChangingFrameIndex(DependencyObject obj, DependencyPropertyChangedEventArgs ev)
            {

                GifImage ob = obj as GifImage;

                ob.Source = ob.gf.Frames[(int)ev.NewValue];

                ob.InvalidateVisual();

            }

            GifBitmapDecoder gf;

            Int32Animation anim;

            public GifImage(Uri uri)
            {

                gf = new GifBitmapDecoder(uri, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

                anim = new Int32Animation(0, gf.Frames.Count - 1, new Duration(new TimeSpan(0, 0, 0, gf.Frames.Count / 10, (int)((gf.Frames.Count / 10.0 - gf.Frames.Count / 10) * 1000))));

                anim.RepeatBehavior = RepeatBehavior.Forever;

                Source = gf.Frames[0];

            }

            bool animationIsWorking = false;

            protected override void OnRender(DrawingContext dc)
            {

                base.OnRender(dc);

                if (!animationIsWorking)
                {

                    BeginAnimation(FrameIndexProperty, anim);

                    animationIsWorking = true;

                }

            }
        }

    }


    // RaiseEvent(new RoutedEventArgs(SaveButtonClickedEvent2, this));
}
