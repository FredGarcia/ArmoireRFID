using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArmoireV3.Controls
{
    public class ItemChoice
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

        private string _taille;
        public string Taille
        {
            get
            {
                return _taille;
            }
            set
            {
                _taille = value;
            }
        }

        private int _quantit;
        public int Quantit
        {
            get
            {
                return _quantit;
            }
            set
            {
                _quantit = value;
            }
        }
    }
}
