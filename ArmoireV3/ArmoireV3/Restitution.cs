using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Impinj.OctaneSdk;
using ArmoireV3.Entities;
using AutoConnect;
using ArmoireV3.Dialogues;
namespace ArmoireV3
{
  public class Restitution
    {
        private string readerName = Properties.Settings.Default.RestituionReader;
        private int readerId = 9;
#if NEWIMPINJ
        public ImpinjReader ReaderRestitution = new ImpinjReader();
#else
        public SpeedwayReader ReaderRestitution = new SpeedwayReader();
#endif
        private delegate void TagsReportedDelegate(List<Tag> tag);
#if EDMX
        Worker datamanager = new Worker();
#else
        static WorkerV3 datamanager = new WorkerV3();
#endif

        public event EventHandler finScanRestitution;

        //AutoConnector myAutoConnector;

        public Restitution()
        {
      //      try { Connect(); }       catch       {        }
            //myAutoConnector = new AutoConnector(this,0.1.1);
            //myAutoConnector.Start();
        }

        public bool Is_connected()
        {
            bool b = false;
            try
            {
#if NEWIMPINJ
                Status status = ReaderRestitution.QueryStatus();
#else
                Status status = ReaderRestitution.QueryStatus(StatusRefresh.Everything);
#endif
                b = status.IsConnected;
            }
            catch 
            { 
            }
            return b;
        }
        public void Connect()
        {
#if NEWIMPINJ
            ReaderRestitution = new ImpinjReader();
#else
            ReaderRestitution = new SpeedwayReader();
#endif
            // connection
            try
            {
                ReaderRestitution.Connect(readerName);
                // application des settings
                RFIDScanRestitution();
            }
            catch (Exception ex)
            {
                if (Properties.Settings.Default.UseDBGMSG)
                   System.Windows.Forms.MessageBox.Show("Erreur : " + ex.Message);    
            }
            
           
        }
        public void CloseConnection()
        {
            ReaderRestitution.Disconnect();
        }

        private void RFIDScanRestitution()
        {
            try
            {
#if NEWIMPINJ
                Log.add("StartRestitution");
                // Assign the TagsReported event handler.
                // This specifies which method to call
                // when tags reports are available.
                ReaderRestitution.TagsReported += OnTagsReported;

                // Assign the TagOpComplete event handler.
                // This specifies which method to call
                // when tag operations are complete.
                //ReaderRestitution.TagOpComplete += OnTagOpComplete;

                // Assign an event handler for tag reports. This specifies
                // which function will be called when tag reports arrive.
                //ReaderRestitution.TagsReported += new EventHandler<TEventArgs  TagsReportedTEventArgs>(OnTagsReported);
                //ReaderRestitution.GpiChanged += new EventHandler<GpiEvent>(OnGpi1Changed); // Faire cela au cas où l'assignation de 2 gpi en trigger ne marche pas
//                ReaderRestitution.Gpi2Changed += new EventHandler<GpiEvent>(OnGpi2Changed); // Faire cela au cas où l'assignation de 2 gpi en trigger ne marche pas
                ReaderRestitution.GpiChanged += OnGpiEvent;

                // Clear all reader settings.
                ReaderRestitution.ApplyDefaultSettings();

                // Get the factory settings for the reader. We will
                // use these are a starting point and then modify
                // the settings we're interested in.
                Settings settings = ReaderRestitution.QueryDefaultSettings();

                // Configure availabilty of antennas
                settings.Antennas.GetAntenna(1).IsEnabled = true;
                settings.Antennas.GetAntenna(2).IsEnabled = false;
                settings.Antennas.GetAntenna(3).IsEnabled = false;
                settings.Antennas.GetAntenna(4).IsEnabled = false;
                // Configure antennas power
                settings.Antennas.GetAntenna(1).TxPowerInDbm = Properties.Settings.Default.PuissanceAntenneRestitution;
                //settings.Antennas.GetAntenna(2).TxPowerInDbm = Properties.Settings.Default.PuissanceAntenne;
                settings.Antennas.GetAntenna(1).RxSensitivityInDbm = -55;

                // configure Reader mode
                settings.ReaderMode = ReaderMode.AutoSetDenseReader;
                settings.SearchMode = SearchMode.SingleTarget;
                settings.Session = 1;
                //settings.TagPopulationEstimate = 5;

                //Configure Trigger start
                /*settings.Gpis[1].IsEnabled = true;
                settings.Gpis[1].DebounceInMs = 50;
                settings.AutoStart.Mode = AutoStartMode.GpiTrigger; // Désactiver si l'assignation de 2 triggers ne fonctionne pas!
                settings.AutoStart.GpiPortNumber = 1;
                settings.AutoStart.GpiLevel = true;*/

                //Configure Trigger start for 2 collectors
                /*settings.Gpis[2].IsEnabled = true;
                settings.Gpis[2].DebounceInMs = 50;
                settings.AutoStart.GpiPortNumber = 2;
                settings.AutoStart.GpiLevel = true;

                settings.AutoStart.Mode = AutoStartMode.GpiTrigger;*/
                settings.Gpis.GetGpi(1).IsEnabled = true;
                settings.Gpis.GetGpi(2).IsEnabled = true;
                settings.Gpis.GetGpi(1).DebounceInMs = 200;
                settings.Gpis.GetGpi(2).DebounceInMs = 200;
                /*
                settings.Gpis[1].IsEnabled = true;
                settings.Gpis[2].IsEnabled = true;
                settings.Gpis[1].DebounceInMs = 50;
                settings.Gpis[2].DebounceInMs = 50;
                 */
                //settings.AutoStart.GpiLevel = true;
                //settings.AutoStart.Mode = AutoStartMode.GpiTrigger;

                //Configure inventory time
                settings.AutoStop.Mode = AutoStopMode.Duration;
                settings.AutoStop.DurationInMs = (uint) (Properties.Settings.Default.ReadingTimeRestitution * 1000);

                // Send a report for every tag is singulated.
                settings.Report.Mode = ReportMode.BatchAfterStop;
                // Include the antenna number in the tag report.
                settings.Report.IncludeAntennaPortNumber = false;
                // Include the time of the reading in the tag report
                settings.Report.IncludeFirstSeenTime = true;

                // Apply the new settings.
                ReaderRestitution.ApplySettings(settings);
                // From ApplySettings, it will start reading tags on trigger.
                

#else

                // Assign an event handler for tag reports. This specifies
                // which function will be called when tag reports arrive.
                ReaderRestitution.TagsReported += new EventHandler<TagsReportedEventArgs>(OnTagsReported);
                ReaderRestitution.Gpi1Changed += new EventHandler<GpiChangedEventArgs>(OnGpi1Changed); // Faire cela au cas où l'assignation de 2 gpi en trigger ne marche pas
                ReaderRestitution.Gpi2Changed += new EventHandler<GpiChangedEventArgs>(OnGpi2Changed); // Faire cela au cas où l'assignation de 2 gpi en trigger ne marche pas


                // Clear all reader settings.
                ReaderRestitution.ClearSettings();

                // Get the factory settings for the reader. We will
                // use these are a starting point and then modify
                // the settings we're interested in.
                Settings settings = ReaderRestitution.QueryFactorySettings();

                // Configure availabilty of antennas
                settings.Antennas[1].IsEnabled = true;
                settings.Antennas[2].IsEnabled = true;
                settings.Antennas[3].IsEnabled = false;
                settings.Antennas[4].IsEnabled = false;
                // Configure antennas power
                settings.Antennas[1].TxPowerInDbm = Properties.Settings.Default.PuissanceAntenneRestitution;
                settings.Antennas[2].TxPowerInDbm = Properties.Settings.Default.PuissanceAntenneRestitution;
                //settings.Antennas[1].RxSensitivityInDbm = -53;

                // configure Reader mode
                settings.ReaderMode = ReaderMode.AutoSetDenseReader;
                settings.SearchMode = SearchMode.SingleTargetWithSuppression;
                //settings.Session = 1;
                //settings.TagPopulationEstimate = 5;

                //Configure Trigger start
                /*settings.Gpis[1].IsEnabled = true;
                settings.Gpis[1].DebounceInMs = 200;
                settings.AutoStart.Mode = AutoStartMode.GpiTrigger; // Désactiver si l'assignation de 2 triggers ne fonctionne pas!
                settings.AutoStart.GpiPortNumber = 1;
                settings.AutoStart.GpiLevel = true;*/

                //Configure Trigger start for 2 collectors
                /*settings.Gpis[2].IsEnabled = true;
                settings.Gpis[2].DebounceInMs = 200;
                settings.AutoStart.GpiPortNumber = 2;
                settings.AutoStart.GpiLevel = true;

                settings.AutoStart.Mode = AutoStartMode.GpiTrigger;*/

                settings.Gpis[1].IsEnabled = true;
                settings.Gpis[2].IsEnabled = true;
                settings.Gpis[1].DebounceInMs = 200;
                settings.Gpis[2].DebounceInMs = 200;
                //settings.AutoStart.GpiLevel = true;
                //settings.AutoStart.Mode = AutoStartMode.GpiTrigger;

                //Configure inventory time
                settings.AutoStop.Mode = AutoStopMode.Duration;
                settings.AutoStop.DurationInMs = Properties.Settings.Default.ReadingTimeRestitution * 1000;

                // Send a report for every tag is singulated.
                settings.Report.Mode = ReportMode.BatchAfterStop;
                // Include the antenna number in the tag report.
                settings.Report.IncludeAntennaPortNumber = false;
                // Include the time of the reading in the tag report
                settings.Report.IncludeFirstSeenTime = true;

                // Apply the new settings.
                ReaderRestitution.ApplySettings(settings);
                // From ApplySettings, it will start reading tags on trigger.
                
#endif
            }
            catch (OctaneSdkException e)
            {
                // Handle Octane SDK errors.
                Log.add("Octane SDK exception: "+ e.Message);
            }
            catch (Exception e)
            {
                // Handle other .NET errors.
                Log.add("Exception : "+ e.Message);
            }
            /*catch
            {
                // MessageBox.Show("Exception Bac de Restitution : " + e.Message);
                // TbOut.Text = "Connexion perdue !   Erreur : " + e.Message;
            }*/
        }
#if NEWIMPINJ
        private void OnGpiEvent(ImpinjReader sender, GpiEvent e)
        {
            Log.add("GPI Restitution Event");
            //Status status = ReaderRestitution.QueryStatus();
            //if ((status.Gpis.GetGpi(1).State = false) || (status.Gpis.GetGpi(2).State = false))
            //if (status.Gpis[1].State == GpioState.Low)
            if (e.State == false)
            { // Evenement de fermeture (quelquesoit le bac)
                try
                {
                     ReaderRestitution.Stop();
                }
                catch (Exception ex3)
                {
                    if (Properties.Settings.Default.UseDBGMSG)
                        System.Windows.Forms.MessageBox.Show(ex3.Message);
                }
                    try
                {
//                    if (!ReaderRestitution.QueryStatus().IsSingulating)
                        ReaderRestitution.Start();
                }
                catch (OctaneSdkException ex1)
                {
                    if (Properties.Settings.Default.UseDBGMSG)
                        System.Windows.Forms.MessageBox.Show(ex1.Message);
                }
                catch (Exception ex2)
                {
                    if (Properties.Settings.Default.UseDBGMSG)
                        System.Windows.Forms.MessageBox.Show(ex2.Message);
                }
            }

        }
#else
        private void OnGpi1Changed(object sender, GpiChangedEventArgs args)
        {
            Status status = ReaderRestitution.QueryStatus();
            //System.Windows.Forms.MessageBox.Show(status.Gpis[1].State.ToString());
            if (status.Gpis[1].State == GpioState.High)
            {
                try
                {
                    //if (!status.IsSingulating)
                    {
                        ReaderRestitution.Start();
                    }
                }
                catch (OctaneSdkException e)
                {
                    if (Properties.Settings.Default.UseDBGMSG)
                        System.Windows.Forms.MessageBox.Show(e.Message);
                }
                catch (Exception ex)
                {
                    if (Properties.Settings.Default.UseDBGMSG)
                        System.Windows.Forms.MessageBox.Show(ex.Message);
                }
            }

        }

        private void OnGpi2Changed(object sender, GpiChangedEventArgs args)
        {
            Status status = ReaderRestitution.QueryStatus();
            if (status.Gpis[2].State == GpioState.High)
            {
                try
                {
                    //if (!status.IsSingulating)
                    {
                        ReaderRestitution.Start();
                    }
                }
                catch (OctaneSdkException e)
                {
                    if (Properties.Settings.Default.UseDBGMSG)
                        System.Windows.Forms.MessageBox.Show(e.Message);
                }
                catch (Exception ex)
                {
                    if (Properties.Settings.Default.UseDBGMSG)
                        System.Windows.Forms.MessageBox.Show(ex.Message);
                }
            }

        }
#endif
#if NEWIMPINJ
        // This event handler is called asynchronously 
        // when tag reports are available.
        private void OnTagsReported(ImpinjReader sender, TagReport report)
        {
            // Rapport threadé :
            TagsReportedDelegate del = new TagsReportedDelegate(routineVoid);
            // this.Dispatcher.BeginInvoke(del, args.TagReport.Tags);
            del.BeginInvoke(report.Tags, null, null);            
        }
#else
        private void OnTagsReported(object sender, TagsReportedEventArgs args)
        {
            // Rapport threadé :
            TagsReportedDelegate del = new TagsReportedDelegate(routineVoid);
            // this.Dispatcher.BeginInvoke(del, args.TagReport.Tags);
            del.BeginInvoke(args.TagReport.Tags, null, null);

        }
#endif
        public void routineVoid(List<Tag> list)
        {
            // on traite les tags lus
            Log.add("Scan Restitution: " + list.Count.ToString() + " articles scannés");
            datamanager.data.RestitutionScan(list, readerId);
            // on renvoie le signal OK
#if NEWIMPINJ
            //ReaderRestitution.SetGpo(1, true);
            //System.Threading.Thread.Sleep(2000);
            //ReaderRestitution.SetGpo(1, false);
#else
            //ReaderRestitution.SetGpo(1, true);
            //System.Threading.Thread.Sleep(2000);
            //ReaderRestitution.SetGpo(1, false);
#endif
            // on relance la boucle de lecture sur trigger
            //RFIDScanRestitution();

            finScanRestitution.Invoke(null, null);
        }
       
    }
}
