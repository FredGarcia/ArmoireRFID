using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;

namespace ArmoireV3.Entities
{
    public class Epc
    {
        // déclaration des variables internes

        private int id;
        private DateTime date_creation;
        private DateTime date_modification;
        private string tag;
        private string code_Barre;
        private string taille;
        private string type_taille;
        private int cycle_Lavage_Count;
        private int state;
        private string last_User;
        private string last_Reader;
        private string last_Action;
        private DateTime last_action_date;
        private int movement;
        private int article_Type_ID;
        private int case_ID;
        private int armoire_ID;
        private bool actif;

        // accesseurs/modifieurs

        public int Actif
        {
            get { return (actif ? 1 : 0); }
            set { actif = (value == 1 ? true : false); }
        }

        public int Armoire_ID
        {
            get { return armoire_ID; }
            set { armoire_ID = value; }
        }

        public int Case_ID
        {
            get { return case_ID; }
            set { case_ID = value; }
        }

        public int Article_Type_ID
        {
            get { return article_Type_ID; }
            set { article_Type_ID = value; }
        }

        public int Movement
        {
            get { return movement; }
            set { movement = value; }
        }

        public DateTime Last_Action_Date
        {
            get { return last_action_date; }
            set
            {
                DateTime time_reference = (new DateTime(1971, 01, 01));
                if (value > time_reference)
                    last_action_date = value;
                else
                    last_action_date = time_reference;
            }
        }

        public string Last_Action
        {
            get { return last_Action; }
            set { last_Action = value; }
        }

        public string Last_Reader
        {
            get { return last_Reader; }
            set { last_Reader = value; }
        }

        public string Last_User
        {
            get { return last_User; }
            set { last_User = value; }
        }

        public int State
        {
            get { return state; }
            set { state = value; }
        }

        public int Cycle_Lavage_Count
        {
            get { return cycle_Lavage_Count; }
            set { cycle_Lavage_Count = value; }
        }

        public string Type_Taille
        {
            get { return type_taille; }
            set { type_taille = value; }
        }

        public string Taille
        {
            get { return taille; }
            set { taille = value; }
        }

        public string Code_Barre
        {
            get { return code_Barre; }
            set { code_Barre = value; }
        } 

        public string Tag
        {
            get { return tag; }
            set { tag = value; }
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

        public Epc()
        {
            Id = 0;
            Date_Creation = new DateTime(1971, 01, 01);
            Date_Modification = new DateTime(1971, 01, 01);
            Tag = " ";
            Code_Barre = " ";
            Taille = " ";
            Type_Taille = " ";
            Cycle_Lavage_Count = 0;
            State = EtatArticle.AUCUN;
            Last_User = " ";
            Last_Reader = " ";
            Last_Action = " ";
            Last_Action_Date = new DateTime(1971, 01, 01);
            Movement = 0;
            Article_Type_ID = 0;
            Case_ID = 0;
            Armoire_ID = 0;
            Actif = 0;
        }
        // constructeurs
        public Epc(int id, DateTime date_creation, DateTime date_modification, string tag, string code_barre, string taille, string type_taille, int cycle_lavage_count, int state, string last_user, string last_reader, string last_action,
            DateTime last_action_date, int movement, int article_type_id, int case_id, int armoire_id, int actif)
        {
            Id = id;
            Date_Creation = date_creation;
            Date_Modification = date_modification;
            Tag = tag;
            Code_Barre = code_barre;
            Taille = taille;
            Type_Taille = type_taille;
            Cycle_Lavage_Count = cycle_lavage_count;
            State = state;
            Last_User = last_user;
            Last_Reader = last_reader;
            Last_Action = last_action;
            Last_Action_Date = last_action_date;
            Movement = movement;
            Article_Type_ID = article_type_id;
            Case_ID = case_id;
            Armoire_ID = armoire_id;
            Actif = actif;
        }
    }
}
