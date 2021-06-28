using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
#if EDMX
namespace ArmoireV3.Entities
{
    public class Worker : INotifyPropertyChanged
    {
        #region Attributs
        protected object datalock = new object();
        public DataManager data { get; private set; }
        string _alerteMessage;
        bool loading;



        public bool Loading
        {
            get { return loading; }
            set { loading = value; OnPropertyChanged("Loading"); }
        }

        public string AlerteMessage
        {
            get { return _alerteMessage; }
            protected set { _alerteMessage = value; OnPropertyChanged("AlerteMessage"); }
        }
        #endregion


        public Worker()
        {
            
            if (data == null)
            {
            
                data = new DataManager();

            }
            data.PropertyChanged += new PropertyChangedEventHandler(data_PropertyChanged);
        }

        public void RefreshContext()
        {
            data.RefreshContext();
        }

        void data_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "AlertMessage":
                    break;
                case "IsConnected":
                    OnPropertyChanged("IsConnected");
                    break;
                default:
                    break;
            }
        }
        public Worker(PropertyChangedEventHandler handler)
            : this()
        {
            PropertyChanged += handler;
        }


        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion
    }
}
#endif