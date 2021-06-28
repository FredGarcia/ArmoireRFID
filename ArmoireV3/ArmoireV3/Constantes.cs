using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArmoireV3
{
    class Constantes
    {
    }
    public static class EtatArticle
    {
        // VALEURS DES ETATS DU LINGE
        public const int AUCUN = 0;
        public const int SORTI = 1;
        public const int PERDU = 2;
        public const int DESACTIVE = 3;
        public const int SORTI_TailleSup = 4;
        public const int INTRUS_CONNUS = 5;
        public const int RESTITUE = 10;
        public const int TRI_SALE = 20;
        public const int RETIRABLE = 100;
        public const int RECEPTIONNE_BLANCHISSERIE = 15;
        public const int TRI_PROPRE = 80;
        public const int DEPART_EXPEDITION = 85;
        public const int RECEPTIONNE_ETS = 90;
        public const int INTRUS_INCONNUS = 300;
        public const int MARQUAGE = 500;
    }

    public static class AlerteType
    {
        // VALEURS DES N° d'ALERTE
        public const int ALERTE_RAIL = 0;
        public const int ALERTE_DATABASE = 1;
        public const int ALERTE_LED = 2;
        public const int ALERTE_RFID = 3;
        public const int ALERTE_BADGE = 4;
        public const int ALERTE_APPLI = 5;
        public const int ALERTE_RESEAU = 6;
        public const int ALERTE_MALPLACE_RELOAD = 7;
        public const int ALERTE_TAILLE = 8;
        public const int ALERTE_INCOMPLET = 9;
        public const int ALERTE_EXCESSIF = 10;
        public const int ALERTE_30SEC = 13;
        public const int ALERTE_1MIN = 14;
        public const int ALERTE_MALPLACE_USER = 15;
        public const int ALERTE_CREDITNUL = 16;
        public const int ALERTE_STOCKVIDE = 17;
        public const int ALERTE_INTRUS_RELOAD = 19;
        public const int ALERTE_INTRUS_USER = 20;
        public const int ALERTE_ROLLPLEIN = 21;
        public const int ALERTE_ARRETAPPLI = 22;
        public const int ALERTE_DEMARREAPPLI = 23;
        public const int ALERTE_REGLECHANGEMAX = 26;
        public const int ALERTE_REGLECHANGEMIN = 25;
    }
}
