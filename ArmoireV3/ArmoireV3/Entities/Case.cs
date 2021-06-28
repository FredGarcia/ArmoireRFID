using System;
using System.Collections.Generic;
using System.Text;

namespace ArmoireV3.Entities
{
    public class Case
    {
        // déclaration variables internes

        private int id;
        private int bind_id;
        private string taille;
        private DateTime date_creation;
        private int max_item;
        private int article_type_id;
        private int armoire_id;

        // accesseurs / modifieurs

        public int Armoire_ID
        {
            get { return armoire_id; }
            set { armoire_id = value; }
        }

        public int Article_Type_Id
        {
            get { return article_type_id; }
            set { article_type_id = value; }
        }
        
        public int Max_Item
        {
            get { return max_item; }
            set { max_item = value; }
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

        public string Taille
        {
            get { return taille; }
            set { taille = value; }
        }

        public int Bind_ID
        {
            get { return bind_id; }
            set { bind_id = value; }
        }

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        // constructeurs

        public Case(int id, int bind_id, string taille, DateTime date_creation, int max_item, int article_type_id, int armoire_id)
        {
            Id = id;
            Bind_ID = bind_id;
            Taille = taille;
            Date_Creation = date_creation;
            Max_Item = max_item;
            Article_Type_Id = article_type_id;
            Armoire_ID = armoire_id;
        }
    }
}
