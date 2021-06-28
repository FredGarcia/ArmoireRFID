using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArmoireV3.Entities
{
    public class Version_Armoire
    {
        // déclaration variables internes
        private int id; //	Id int(8)
        private string nomplace; // NomPlace varchar(255)
        private string verslog; // VersLog varchar(255)
        private string versmat; // VersMat varchar(255)
        private DateTime datesynchro; //DateSynchro datetime

        // accesseurs / modifieurs

        public int Id
        {
            get { return id; }
            set { id = value; }
        }


        public string NomPlace
        {
            get { return nomplace; }
            set { nomplace = value; }
        }
        public string VersLog
        {
            get { return verslog; }
            set { verslog = value; }
        }
        public string VersMat
        {
            get { return versmat; }
            set { versmat = value; }
        }

        public DateTime DateSynchro
        {
            get { return datesynchro; }
            set
            {
                DateTime time_reference = (new DateTime(1971, 01, 01));
                if (value > time_reference)
                    datesynchro = value;
                else
                    datesynchro = time_reference;                
            }
        }

        // constructeurs
        public Version_Armoire()
        {
            Id = 0;
            NomPlace = " ";
            VersLog = " ";
            VersMat = " ";
            DateSynchro = new DateTime(1971, 01, 01);
        }

        public Version_Armoire(int id, string nomplace, string verslog, string versmat, DateTime datesynchro)
        {
            Id = id;
            NomPlace = nomplace;
            VersLog = verslog;
            VersMat = versmat;
            DateSynchro = datesynchro;
        }
    }
}
