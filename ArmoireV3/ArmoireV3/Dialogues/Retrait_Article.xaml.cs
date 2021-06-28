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

namespace ArmoireV3.Dialogues
{
    /// <summary>
    /// Logique d'interaction pour Retrait_Article.xaml
    /// </summary>
    public partial class Retrait_Article : UserControl
    {
        public static readonly RoutedEvent FermerRetraitEvent = EventManager.RegisterRoutedEvent(
        "FermerRetrait", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Login));
        public event RoutedEventHandler FermerRetrait
        {

            add { AddHandler(FermerRetraitEvent, value); }

            remove { RemoveHandler(FermerRetraitEvent, value); }

        }

        public Retrait_Article()
        {
            InitializeComponent();
            if (Properties.Settings.Default.Debug == true)
            {
                buttonFermer.IsEnabled = true;
                buttonFermer.Visibility = Visibility.Visible;
            }
            else
            {
                buttonFermer.IsEnabled = false;
                buttonFermer.Visibility = Visibility.Hidden;
            }
        }

        private void Grid_TouchDown(object sender, SelectionChangedEventArgs e)
        {

        }

        public void loadItem()
        {

            

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
          
        }

        private void buttonFermer_Click(object sender, RoutedEventArgs e)
        {
      
            // Simulation de la fermeture de porte pour tests
            RaiseEvent(new RoutedEventArgs(FermerRetraitEvent, this));
            
        

        }
    }
}
