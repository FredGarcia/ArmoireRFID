using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArmoireV3.Controls
{
    class viewTaken
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
