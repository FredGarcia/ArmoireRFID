using System;
using System.Collections.Generic;
using System.Text;

namespace ArmoireV3.Entities
{
    public class Article_Type
    {
        // déclaration des variables internes

        private int id;
        private DateTime date_creation;
        private DateTime date_modification;
        private string code;
        private string type_taille;
        private string description;
        private string couleur;
        private string sexe;
        private string photo;
        private int active;
        private string description_longue;

        // accesseurs/modifieurs

        public string Description_longue
        {
            get { return description_longue; }
            set { description_longue = value; }
        }

        public int Active
        {
            get { return active; }
            set { active = value; }
        }

        public string Photo
        {
            get { return photo; }
            set { photo = value; }
        }

        public string Sexe
        {
            get { return sexe; }
            set { sexe = value; }
        }

        public string Couleur
        {
            get { return couleur; }
            set { couleur = value; }
        }

        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        public string Type_Taille
        {
            get { return type_taille; }
            set
            {
                if ((value != null) && (value.Length > 1))
                    type_taille = value.Substring(0, 1);
                else
                    type_taille = value;
            }
        }

        public string Code
        {
            get { return code; }
            set { code = value; }
        }

        public DateTime Date_Modification
        {
            get { return date_modification; }
            set
            {
                DateTime time_reference = (new DateTime(1971, 01, 01));
                if (value > time_reference)
                    date_modification = value;
                else
                    date_modification = time_reference;
            }
        }


        public DateTime Date_Creation
        {
            get { return date_creation; }
            set
            {
                DateTime time_reference = (new DateTime(1971, 01, 01));
                if (value > time_reference)
                    date_creation = value;
                else
                    date_creation = time_reference;
            }
        }
        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        // constructeurs
        public Article_Type()
        {
            Id = 0;
            Date_Creation = new DateTime(1971, 01, 01);
            Date_Modification = new DateTime(1971, 01, 01);
            Code = " ";
            Type_Taille = " ";
            Description = " ";
            Couleur = " ";
            Sexe = " ";
            Photo = " ";
            Active = 0;
            Description_longue = " ";
        }
        public Article_Type(int id, DateTime date_creation, DateTime date_modification, string code, string type_taille, string description, string couleur, string sexe, string photo, int active, string description_longue)
        {
            Id = id;
            Date_Creation = date_creation;
            Date_Modification = date_modification;
            Code = code;
            Type_Taille = type_taille;
            Description = description;
            Couleur = couleur;
            Sexe = sexe;
            Photo = photo;
            Active = active;
            Description_longue = description_longue;
        }
    }
}
