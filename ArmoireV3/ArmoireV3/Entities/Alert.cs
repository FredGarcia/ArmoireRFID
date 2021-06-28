using System;
using System.Collections.Generic;
using System.Text;

namespace ArmoireV3.Entities
{
    public class Alert
    {
        // déclaration des variables internes
        private int id;
        private DateTime date_creation;
        private int alert_type_id;
        private string message;
        private int user_id;
        private int armoire_id;
        private bool traiter;

        // acceseurs/modifieurs
        public int Traiter
        {
            get { return (traiter ? 1 : 0); }
            set { traiter = (value==1 ? true : false); }
        }

        public int Armoire_ID
        {
            get { return armoire_id; }
            set { armoire_id = value; }
        }

        public int User_ID
        {
            get { return user_id; }
            set { user_id = value; }
        }

        public string Message
        {
            get { return message; }
            set { message = value; }
        }

        public int Alert_Type_Id
        {
            get { return alert_type_id; }
            set { alert_type_id = value; }
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
        public Alert()
        {
            Id = 0;
            Date_Creation = new DateTime(1971, 01, 01);
            Alert_Type_Id = 0;
            Message = " ";
            User_ID = 0;
            Armoire_ID = 0;
            Traiter = 0;
        }

        // constructeurs
        public Alert(int id, DateTime date_creation, int alert_type_id, string message, int user_id, int armoire_id, int traiter)
        {
            Id = id;
            Date_Creation = date_creation;
            Alert_Type_Id = alert_type_id;
            Message = message;
            User_ID = user_id;
            Armoire_ID = armoire_id;
            Traiter = traiter;
        }
    }
}
