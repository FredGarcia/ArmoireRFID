using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArmoireV3.Controls
{
   public  class selectItem
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

        private string _selectedid;
        public string SelectedID
        {
            get
            {
                return _selectedid;
            }
            set
            {
                _selectedid = value;
            }
        }

        private string _photo;
        public string Photo
        {
            get
            {
                return _photo;
            }
            set
            {
                _photo = value;
            }
        }

        private string _description;
        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
            }
        }

        private int _stock;
        public int Stock
        {
            get
            {
                return _stock;
            }
            set
            {
                _stock = value;
            }
        }

        private string _stockTotal;
        public string StockTotal
        {
            get
            {
                return _stockTotal;
            }
            set
            {
                _stockTotal = value;
            }
        }
        private int _credit;
        public int Credit
        {
            get
            {
                return _credit;
            }
            set
            {
                _credit = value;
            }
        }

    }
}
