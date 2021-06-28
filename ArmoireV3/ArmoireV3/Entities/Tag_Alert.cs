using System;
using System.Collections.Generic;
using System.Text;

namespace ArmoireV3.Entities
{
    public class Tag_Alert
    {
        // déclaration des variables internes

        private int id;
        private DateTime date_creation;
        private int alert_id;
        private string tag_command;
        private string tag_retir;
        private string tag_intrus;
        private string article_type_code;
        private string taille;
        private string code_barre;

        // acceseurs/modifieurs

        public string Code_Barre
        {
            get { return code_barre; }
            set { code_barre = value; }
        }

        public string Taille
        {
            get { return taille; }
            set { taille = value; }
        }

        public string Article_Type_Code
        {
            get { return article_type_code; }
            set { article_type_code = value; }
        }

        public string Tag_Intrus
        {
            get { return tag_intrus; }
            set { tag_intrus = value; }
        }

        public string Tag_Retir
        {
            get { return tag_retir; }
            set { tag_retir = value; }
        }

        public string Tag_Command
        {
            get { return tag_command; }
            set { tag_command = value; }
        }

        public int Alert_Id
        {
            get { return alert_id; }
            set { alert_id = value; }
        }

        public int Id
        {
            get { return id; }
            set { id = value; }
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

        public Tag_Alert()
        {
            Id = 0;
            Date_Creation = new DateTime(1971, 01, 01);
            Alert_Id = 0;
            Tag_Command = " ";
            Tag_Retir = " ";
            Tag_Intrus = " ";
            Article_Type_Code = " ";
            Taille = " ";
            Code_Barre = " ";
        }
        // constructeurs
        public Tag_Alert(int id, DateTime date_creation, int alert_id, string tag_command, string tag_retir, string tag_intrus, string art_type_code, string taille)
        {
            Id = id;
            Date_Creation = date_creation;
            Alert_Id = alert_id;
            Tag_Command = tag_command;
            Tag_Retir = tag_retir;
            Tag_Intrus = tag_intrus;
            Article_Type_Code = art_type_code;
            Taille = taille;
            Code_Barre = " ";
        }
        // constructeurs
        public Tag_Alert(int id, DateTime date_creation, int alert_id, string tag_command, string tag_retir, string tag_intrus, string art_type_code, string taille, string code_barre)
        {
            Id = id;
            Date_Creation = date_creation;
            Alert_Id = alert_id;
            Tag_Command = tag_command;
            Tag_Retir = tag_retir;
            Tag_Intrus = tag_intrus;
            Article_Type_Code = art_type_code;
            Taille = taille;
            Code_Barre = code_barre;
        }

    }
}
