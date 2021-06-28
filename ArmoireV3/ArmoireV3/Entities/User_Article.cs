using System;
using System.Collections.Generic;
using System.Text;

namespace ArmoireV3.Entities
{
    public class User_Article
    {
        // déclaration des variables internes

        private int id;
        private DateTime date_creation;
        private DateTime date_modification;
        private string taille;
        private int credit;
        private int credit_restant;
        private int credit_semaine_suivante;
        private int user_id;
        private int article_type_id;

        // accesseurs/modifieurs

        public int Article_Type_Id
        {
            get { return article_type_id; }
            set { article_type_id = value; }
        }

        public int User_Id
        {
            get { return user_id; }
            set { user_id = value; }
        }
        
        public int Credit_Semaine_Suivante
        {
            get { return credit_semaine_suivante; }
            set { credit_semaine_suivante = value; }
        }

        public int Credit_Restant
        {
            get { return credit_restant; }
            set { credit_restant = value; }
        }

        public int Credit
        {
            get { return credit; }
            set { credit = value; }
        }

        public string Taille
        {
            get { return taille; }
            set { taille = value; }
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
        public User_Article()
        {
        }

        public User_Article(int id, DateTime date_creation, DateTime date_modification, string taille, int credit, int credit_restant, int user_id, int article_type_id)
        {
            Id = id;
            Date_Creation = date_creation;
            Date_Modification = date_modification;
            Taille = taille;
            Credit = credit;
            Credit_Restant = credit_restant;
            Credit_Semaine_Suivante = 0;
            User_Id = user_id;
            Article_Type_Id = article_type_id;
        }

        public User_Article(int id, DateTime date_creation, DateTime date_modification, string taille, int credit, int credit_restant, int credit_semaine_suivante, int user_id, int article_type_id)
        {
            Id = id;
            Date_Creation = date_creation;
            Date_Modification = date_modification;
            Taille = taille;
            Credit = credit;
            Credit_Restant = credit_restant;
            Credit_Semaine_Suivante = credit_semaine_suivante;
            User_Id = user_id;
            Article_Type_Id = article_type_id;
        }

    }
}
