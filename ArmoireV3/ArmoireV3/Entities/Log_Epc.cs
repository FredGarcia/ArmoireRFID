using System;
using System.Collections.Generic;
using System.Text;

namespace ArmoireV3.Entities
{
    public class Log_Epc
    {
        // déclaration variables
        
        private int id;
        private DateTime date_creation;
        private int epc_Id;
        private string tag;
        private string code_Barre;
        private string taille;
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
        private bool synchronised;

        // accesseurs/modifieurs

        public int Synchronised
        {
            get { return (synchronised ? 1 : 0); }
            set { synchronised = (value==1 ? true : false); }
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
            set { 
                DateTime time_reference = (new DateTime(1971,01,01));
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

        public int Epc_Id
        {
            get { return epc_Id; }
            set { epc_Id = value; }
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
        public Log_Epc()
        {
        }

        public Log_Epc(int id, DateTime date_creation, int epc_id, string tag, string code_barre, string taille, int cycle_lavage_count, int state, string last_user, string last_reader, string last_action,
            DateTime last_action_date, int movement, int article_type_id, int case_id, int armoire_id, int synchronised)
        {
            Id = id;
            Date_Creation = date_creation;
            Epc_Id = epc_id;
            Tag = tag;
            Code_Barre = code_barre;
            Taille = taille;
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
            Synchronised = synchronised;
        }
    }
}
