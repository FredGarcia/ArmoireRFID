using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;

namespace ArmoireV3.Entities
{
    public class WorkerV3: INotifyPropertyChanged
    {
        #region Attributs
        protected object datalock = new object();
        public DataManagerV3 data { get; private set; } 
        string _alerteMessage;
        bool loading;



        public bool Loading
        {
            get { return loading; }
            set { loading = value; OnPropertyChanged("Loading");}
        }

        public string AlerteMessage
        {
            get { return _alerteMessage; }
            protected set { _alerteMessage = value; OnPropertyChanged("AlerteMessage");}
        }
        #endregion


        public WorkerV3()
        {
            
            if (data == null)
            {
            
                data = new DataManagerV3();

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
        public WorkerV3(PropertyChangedEventHandler handler)
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
