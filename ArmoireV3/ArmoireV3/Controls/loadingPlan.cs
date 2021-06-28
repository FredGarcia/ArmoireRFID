using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArmoireV3.Controls
{
    class loadingPlan
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
        private int _maxItem;
        public int MaxItem
        {
            get
            {
                return _maxItem;
            }
            set
            {
                _maxItem = value;
            }
        }
        private string _articleType;
        public string ArticleType
        {
            get
            {
                return _articleType;
            }
            set
            {
                _articleType = value;
            }
        }
        private int _numArmoire;
        public int NumArmoire
        {
            get
            {
                return _numArmoire;
            }
            set
            {
                _numArmoire = value;
            }
        }
    }
}
