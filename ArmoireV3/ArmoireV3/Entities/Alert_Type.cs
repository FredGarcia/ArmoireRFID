using System;
using System.Collections.Generic;
using System.Text;

namespace ArmoireV3.Entities
{
    public class Alert_Type
    {
        // déclaration des variables internes

        private int id;
        private string type;
        private string code;
        private string description;
        private string niveau;
        private string contact;

        // acceseurs/modifieurs

        public string Contact
        {
            get { return contact; }
            set { contact = value; }
        }

        public string Niveau
        {
            get { return niveau; }
            set { niveau = value; }
        }

        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        public string Code
        {
            get { return code; }
            set { code = value; }
        }

        public string Type
        {
            get { return type; }
            set { type = value; }
        }

        public int Id
        {
            get { return id; }
            set { id = value; }
        }
    }
}
