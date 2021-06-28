using System;
using System.Collections.Generic;
using System.Text;

namespace ArmoireV3.Entities
{
     public class Content_Arm
    {
        // déclaration des variables internes
        
        private int id;
        private DateTime creation_Date;
        private string epc;
        private int state;
        private int rfid_reader;

        // acceseurs/modifieurs

        public int RFID_Reader
        {
            get { return rfid_reader; }
            set { rfid_reader = value; }
        }

        public int State
        {
            get { return state; }
            set { state = value; }
        }

        public string Epc
        {
            get { return epc; }
            set { epc = value; }
        }

        public DateTime Creation_Date
        {
            get { return creation_Date; }
            set
            {
                DateTime time_reference = (new DateTime(1971, 01, 01));
                if (value > time_reference)
                    creation_Date = value;
                else
                    creation_Date = time_reference;
                
            }
        }

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        // constructeurs
        public Content_Arm()
        {
        }

        public Content_Arm(int id, DateTime creation_date, string epc, int state, int rfid_reader)
        {
            Id = id;
            Creation_Date = creation_date;
            Epc = epc;
            State = state;
            RFID_Reader = rfid_reader;
        }
        
        public Content_Arm(int id, DateTime creation_date, string epc, int state, string rfid_reader)
        {
            Id = id;
            Creation_Date = creation_date;
            Epc = epc;
            State = state;
            int result;
            bool ok = false;

            ok = int.TryParse(rfid_reader, out result);
            if (ok)
                RFID_Reader = result;
            else
                throw (new Exception("L'argument RFID_Reader doit être un entier"));
        }

    }
}
