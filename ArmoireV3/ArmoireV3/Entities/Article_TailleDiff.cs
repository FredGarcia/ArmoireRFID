using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArmoireV3.Entities
{
    public class Article_TailleDiff
    {

        private int article_type_ID;
        private string article_code;
        private string taille_retiree;
        private string taille_commandee;

        // accesseurs/modifieurs

        public int Article_Type_ID
        {
            get { return article_type_ID; }
            set { article_type_ID = value; }
        }

        public string Article_Code
        {
            get { return article_code; }
            set { article_code = value; }
        }

        public string Taille_Commandee
        {
            get { return taille_commandee; }
            set { taille_commandee = value; }
        }

        public string Taille_Retiree
        {
            get { return taille_retiree; }
            set { taille_retiree = value; }
        }
    }
}
