using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArmoireV3.Controls
{
  public  class ListEpcCount
    {
        private int _count;
        public int Count
        {
            get
            {
                return _count;
            }
            set
            {
                _count = value;
            }
        }

        private int _type;
        public int Type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
            }
        }

        private string _taille;

        public string Taille
        {
            get { return _taille; }
            set { _taille = value; }
        }
       
    }
}
