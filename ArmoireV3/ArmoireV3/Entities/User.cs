using System;
using System.Collections.Generic;
using System.Text;

namespace ArmoireV3.Entities
{
    public class User
    {
        // variables

        private int id;
        private DateTime date_creation;
        private DateTime date_modification;
        private string login;
        private string password;
        private string type;
        private string nom;
        private string prenom;
        private int id_Lang; // foreign key gérée par mysql
        private string sexe;
        private string taille;
        private int groupe;
        private int departement;
        private string photo;
        private DateTime last_connection;
        private int active;
        private DateTime end_of_validity;
        private string wearer_Code;

        // accesseurs/modifieurs

        public string Wearer_Code
        {
            get { return wearer_Code; }
            set { wearer_Code = value; }
        }


        public DateTime End_of_Validity
        {
            get { return end_of_validity; }
            set {
                DateTime time_reference = (new DateTime(1971, 01, 01));
                if (value > time_reference)
                    end_of_validity = value;
                else
                    end_of_validity = time_reference;
            }
        }


        public int Active
        {
            get { return active; }
            set { active = value; }
        }

        public DateTime Last_Connection
        {
            get { return last_connection; }
            set
            {
                DateTime time_reference = (new DateTime(1971, 01, 01));
                if (value > time_reference)
                    last_connection = value;
                else
                    last_connection = time_reference;
            }
        }


        public string Photo
        {
            get { return photo; }
            set { photo = value; }
        }

        public int Departement
        {
            get { return departement; }
            set { departement = value; }
        }


        public int Groupe
        {
            get { return groupe; }
            set { groupe = value; }
        }

        public string Taille
        {
            get { return taille; }
            set { taille = value; }
        }

        public string Sexe
        {
            get { return sexe; }
            set { sexe = value; }
        }


        public int Id_Lang
        {
            get { return id_Lang; }
            set { id_Lang = value; }
        }
        
        public string Prenom
        {
            get { return prenom; }
            set { prenom = value; }
        }

        public string Nom
        {
            get { return nom; }
            set { nom = value; }
        }


        public string Type
        {
            get { return type; }
            set { type = value; }
        }


        public string Password
        {
            get { return password; }
            set { password = value; }
        } 

        public string Login
        {
            get { return login; }
            set { login = value; }
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
        public User()
        {
        }

        public User(int id, DateTime date_creation, DateTime date_modification, string login, string password, string type, string nom, string prenom, int id_lang, string sexe, string taille, int groupe,
            int dpt, string photo, DateTime last_connection, int active, DateTime eov, string wearer_code)
        {
            Id = id;
            Date_Creation = date_creation;
            Date_Modification = date_modification;
            Login = login;
            Password = password;
            Type = type;
            Nom = nom;
            Prenom = prenom;
            Id_Lang = id_lang;
            Sexe = sexe;
            Taille = taille;
            Groupe = groupe;
            Departement = dpt;
            Photo = photo;
            Last_Connection = last_connection;
            Active = active;
            End_of_Validity = eov;
            Wearer_Code = wearer_code;
        }

    }
}
