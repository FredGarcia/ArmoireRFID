using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArmoireV3.Entities
{
    public class Article_Taille
    {
         // déclaration des variables internes

        private int article_type_ID;
        private string taille;
        private int armoire;
        private bool vide;

        // accesseurs/modifieurs

        public int Article_Type_ID
        {
            get { return article_type_ID; }
            set { article_type_ID = value; }
        }

        public bool Vide
        {
            get { return vide; }
            set { vide = value; }
        }

        public string Taille
        {
            get { return taille; }
            set { taille = value; }
        }

        public int Armoire
        {
            get { return armoire; }
            set { armoire = value; }
        }

     
        // constructeurs
        public Article_Taille()
        {
            Article_Type_ID = 0;
            Vide = false;
            Taille = "00";
            Armoire = 0;
        }

        public Article_Taille(int article_type_ID, string taille, int armoire, bool vide)
        {
            Article_Type_ID = article_type_ID;
            Vide = vide;
            Taille = taille;
            Armoire = armoire;
        }
    }
}
