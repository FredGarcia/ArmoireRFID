using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArmoireV3.Entities
{
    public class Corresp_Taille
    {
        // déclaration des variables internes
        
        private string type_taille;
        private string taille;
        private int classement_tailles;

        // acceseurs/modifieurs

        public int Classement_Tailles
        {
            get { return classement_tailles; }
            set { classement_tailles = value; }
        }
        
        public string Taille
        {
            get { return taille; }
            set { taille = value; }
        }

        public string Type_Taille
        {
            get { return type_taille; }
            set {
                if ((value != null) && (value.Length > 1))
                    type_taille = value.Substring(0, 1);
                else
                    type_taille = value;
                }                
        }

        // constructeurs

        public Corresp_Taille(string type_taille, string taille, int classement_tailles)
        {
            Type_Taille = type_taille;
            Taille = taille;
            Classement_Tailles = classement_tailles;
        }
        
    }
}
