using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArmoireV3.Controls
{
   public class listItem
    {


        private int _id;
        public int Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }
        private DateTime _creation_Date;
        public DateTime Creation_Date
        {
            get
            {
                return _creation_Date;
            }
            set
            {
                _creation_Date = value;
            }
        }

        private string _epc;
        public string Epc
        {
            get
            {
                return _epc;
            }
            set
            {
                _epc = value;
            }
        }
        private int _state;
        public int State
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
            }
        }

        private int _rfid_Reader;
        public int RFID_Reader
        {
            get
            {
                return _rfid_Reader;
            }
            set
            {
                _rfid_Reader = value;
            }
        }
    }
}
