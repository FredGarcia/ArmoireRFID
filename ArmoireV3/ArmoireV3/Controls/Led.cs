using System;
using System.Linq;
using System.Threading;
using ArmoireV3.Entities;
using System.IO.Ports;
using System.IO;
namespace ArmoireV3.Controls {
    public class Led : SerialPort {
        static byte[] buffer;      
        private int READ_BUFFER_SIZE = 1024;

        public void InitCarteLed(string portName) {
            buffer = new byte[100];
            //      serialPort = new SerialPort();
            READ_BUFFER_SIZE = 1024;
            PortName = portName;
            BaudRate = 57600;
            //serialPort.BaudRate = 115200;
            Parity = Parity.None;
            DataBits = 8;
            StopBits = StopBits.One;
            Handshake = Handshake.None;
            ReadBufferSize = READ_BUFFER_SIZE;
            // Set the read/write timeouts
            ReadTimeout = 400;
            WriteTimeout = 500;
            DtrEnable = false;
            RtsEnable = false;
            ReceivedBytesThreshold = 5;
        }

        /// <summary>
        /// Tente d'ouvrir le port COM
        /// </summary>
        /// <returns>True si le port a été ouvert, False sinon</returns>
        private bool openPort() {
            if (!IsOpen) {
                try {
                    Open();
                    return true;
                }
                catch (IOException e)
                {
                    if (Properties.Settings.Default.UseDBGMSG)
                        System.Windows.MessageBox.Show("Erreur por COM: " + e.Message);
                    return false;
                }
            } else return true;
        }

        /// <summary>
        /// Tente de fermer le port COM
        /// </summary>
        /// <returns>True si le port a été fermé, False sinon</returns>
        private bool closePort() {
            if (IsOpen) {
                try {
                    this.Close();
                    this.Dispose();
                    return true;
                }
                catch (Exception e)
                {

                    if (Properties.Settings.Default.UseDBGMSG)
                        System.Windows.MessageBox.Show("Erreur port COM: " + e.Message);
                    return false;
                }
            } else return true;
        }

        /// <summary>
        /// Ouvre le port COM s'il n'est pas ouvert et écrit la commande spécifiée
        /// </summary>
        /// <param name="command">Commande à écrire dans le port COM</param>
        /// <param name="closePort">(optionnel) Spécifie si le port COM doit être fermé après exécution de la commande</param>
        /// <returns>True si l'opération s'est déroulée correctement, False sinon</returns>
        private bool writeCommand(string command, bool closePort = true) {
            if (openPort()) {
                try {
#if LEDCTRL
                    bool bres = false;

                    int nb = 0;
                    serialPort.DiscardInBuffer();
#endif
                    WriteLine(command);
#if LEDCTRL
                    System.Threading.Thread.Sleep(16);

                    nb = serialPort.Read(buffer, 0, 6); // Doit répondre ">ACK0\r\n"
                    if (nb == 0)
                    {
                        Log.add("Erreur Carte Led");
                    }

                    if (closePort == true)
                    {// demande de fermeture du port à la fin de la commande
                        bres = this.closePort();
                        if (bres == false)
                            return false;
                        else
                            if (nb == 0)
                                return false;
                            else
                                return true;
                    }
                    else
                        if (nb == 0)
                            return false;
                        else
                            return true;
#else
                    return closePort ? this.closePort() : true;
#endif
                }
                catch (Exception e)
                {
                    if (Properties.Settings.Default.UseDBGMSG)
                    {
                        System.Windows.MessageBox.Show("Erreur port COM: " + e.Message);
                    }
                    return false;
                }
            }
            else return false;
        }

        public bool TestCarteLed()
        {
            if (openPort())
            {
                int nb = 0;
                bool On = false;

                try
                {
                    this.WriteLine(">STA0\r\n");
                    On = true;
                }
                catch
                {
                    On = false;
                }
                System.Threading.Thread.Sleep(100);
                try
                {
                    nb = this.Read(buffer, 0, 6);
                    On = true;
                }
                catch (TimeoutException e)
                {
                    Log.add("TimeOut carte led: " + e.Message);
                    On = false;
                }
                catch (Exception e)
                {
                    Log.add("Impossible d'initialiser la carte led: " + e.Message);
                    On = false;
                }


                if (Properties.Settings.Default.UseDBGMSG)
                {
                    if (buffer.Count() > 0)
                    {
                        String s = "";
                        foreach (char c in buffer) s += c;
                        Log.add("Retour init carte led (nb = " + nb.ToString() + "): " + s.Substring(0, 5));
                    }
                    else
                    {
                        Log.add("Aucun retour init carte led");
                    }
                }


                if ((nb != 0) && buffer.Count() > 0)// && (buffer[0] == (byte)'<'))
                {
                    this.DiscardInBuffer();
                }
                else
                {
                    if (Properties.Settings.Default.UseDBGMSG)
                        System.Windows.MessageBox.Show("Ce n'est pas une carte gestionnaire de Led Coding");
                    this.DiscardInBuffer();
                }
                closePort();
                return On;
            }
            else return false;
        }

        public bool InitLed() {
            bool btmp = false;
            writeCommand(">SET01XO\r\n", false);
            Thread.Sleep(150);
            writeCommand(">SET05AT\r\n", false);
            Thread.Sleep(150);
            writeCommand(">SET05BT\r\n", false);
            Thread.Sleep(150);
            writeCommand(">SET01AR\r\n", false);
            Thread.Sleep(150);
            writeCommand(">SET01BR\r\n", false);
            Thread.Sleep(50);
            writeCommand(">SET01CR\r\n", false);
            Thread.Sleep(50);
            writeCommand(">SET01DR\r\n", false);
            Thread.Sleep(50);
            writeCommand(">SET01ER\r\n", false);
            Thread.Sleep(150);
            writeCommand(">SET01FR\r\n", false);
            Thread.Sleep(150);
            writeCommand(">SET01GR\r\n", false);
            Thread.Sleep(150);
            writeCommand(">SET01HR\r\n", false);
            Thread.Sleep(150);
            writeCommand(">SET01IR\r\n", false);
            Thread.Sleep(150);
            writeCommand(">SET01JR\r\n", false);
            Thread.Sleep(150);
            writeCommand(">SET01KR\r\n", false);
            Thread.Sleep(150);
            writeCommand(">SET01LR\r\n", false);
            Thread.Sleep(150);
            writeCommand(">SET02DR\r\n", false);
            Thread.Sleep(150);
            writeCommand(">SET02CR\r\n", false);
            Thread.Sleep(150);
            writeCommand(">SET02BR\r\n", false);
            Thread.Sleep(150);
            writeCommand(">SET02AR\r\n", false);
            Thread.Sleep(150);
            writeCommand(">SET02HR\r\n", false);
            Thread.Sleep(150);
            writeCommand(">SET02GR\r\n", false);
            Thread.Sleep(150);
            writeCommand(">SET02FR\r\n", false);
            Thread.Sleep(150);
            writeCommand(">SET02ER\r\n", false);
            Thread.Sleep(150);
            writeCommand(">SET02LR\r\n", false);
            Thread.Sleep(150);
            writeCommand(">SET02KR\r\n", false);
            Thread.Sleep(150);
            writeCommand(">SET02JR\r\n", false);
            Thread.Sleep(150);
            writeCommand(">SET02IR\r\n", false);
            Thread.Sleep(150);

            //2eme défilé

            Thread.Sleep(150);
            writeCommand(">SET01AV\r\n", false);
            Thread.Sleep(150);
            writeCommand(">SET01BV\r\n", false);
            Thread.Sleep(150);
            writeCommand(">SET01CV\r\n", false);
            Thread.Sleep(150);
            writeCommand(">SET01DV\r\n", false);
            Thread.Sleep(150);
            writeCommand(">SET01EV\r\n", false);
            Thread.Sleep(150);
            writeCommand(">SET01FV\r\n", false);
            Thread.Sleep(150);
            writeCommand(">SET01GV\r\n", false);
            Thread.Sleep(150);
            writeCommand(">SET01HV\r\n", false);
            Thread.Sleep(150);
            writeCommand(">SET01IV\r\n", false);
            Thread.Sleep(150);
            writeCommand(">SET01JV\r\n", false);
            Thread.Sleep(150);
            writeCommand(">SET01KV\r\n", false);
            Thread.Sleep(150);
            writeCommand(">SET01LV\r\n", false);
            Thread.Sleep(150);
            writeCommand(">SET02DV\r\n", false);
            Thread.Sleep(150);
            writeCommand(">SET02CV\r\n", false);
            Thread.Sleep(150);
            writeCommand(">SET02BV\r\n", false);
            Thread.Sleep(150);
            writeCommand(">SET02AV\r\n", false);
            Thread.Sleep(150);
            writeCommand(">SET02HV\r\n", false);
            Thread.Sleep(150);
            writeCommand(">SET02GV\r\n", false);
            Thread.Sleep(150);
            writeCommand(">SET02FV\r\n", false);
            Thread.Sleep(150);
            writeCommand(">SET02EV\r\n", false);
            Thread.Sleep(150);
            writeCommand(">SET02LV\r\n", false);
            Thread.Sleep(150);
            writeCommand(">SET02KV\r\n", false);
            Thread.Sleep(150);
            writeCommand(">SET02JV\r\n", false);
            Thread.Sleep(150);
            writeCommand(">SET02IV\r\n", false);
            Thread.Sleep(150);

            writeCommand(">SET01XO\r\n", false);
            writeCommand(">SET02XO\r\n", false);

            Thread.Sleep(50);
            writeCommand(">SET02XO\r\n", false);
            writeCommand(">SET05AR\r\n", false);
            btmp = writeCommand(">SET05BR\r\n");
            // SET01XR -toutes les rouges actives
            // SET01XV -toutes les bleues actives
            // SET01XO -toutes éteintes

            //      string commandestring = ">SET0" + indexPortLed + indexGroupeLed + indexAction + "\r\n";
            //    writeCommand(commandestring);
            //writeCommand(">SET01XO\r\n");

            //0 sur tout les ports
            //1 port 1
            //2 port 2
            //3 port 3
            //4 port 4
            //5 port 5


            //A 1ere LED du groupe jusqu'a L 12eme et derniere led du groupe
            //X toute les LED

            //R allume en rouge
            //V allume en blue
            //S clignote en rouge
            //W clignote en bleu
            //O eteint tout
            //T active les 2 LED fixe
            return btmp;
        }

        public bool userFlash(string indexPortLed, string indexGroupeLed)
        {
            string commandestring = ">SET0" + indexPortLed + indexGroupeLed + "W\r\n";
            return (writeCommand(commandestring));
        }

        public bool shut()
            {
                bool bres = false;
                writeCommand(">SET02XO\r\n");
                try { bres = writeCommand(">SET01XO\r\n"); }
                catch (Exception e) { Log.add("Erreur Led: " + e.Message); }
                return (bres);
            }


        public bool reloaderFlash(string indexPortLed, string indexGroupeLed)
        {
            bool bres = false;
            bres = writeCommand(">SET0" + indexPortLed + indexGroupeLed + "S\r\n");
            return (bres);

        }

        public bool leftDoorLed(bool leftOpen)
        {
            bool bres = false;
            string commandestring = "";
            if (leftOpen)
            {
                commandestring = ">SET03XR\r\n";//porte gauche fermé
            }

            else
            {
                commandestring = ">SET03XV\r\n";//porte gauche ouvert
            }
            bres = writeCommand(commandestring);
            return (bres);
        }

        public bool rightDoorLed(bool rightOpen)
        {
            bool bres = false;
            string commandestring = "";
            if (rightOpen)
            {
                commandestring = ">SET04XR\r\n";//porte droite fermé
            }
            else
            {
                commandestring = ">SET04XV\r\n";//porte droite ouvert
            }
            bres = writeCommand(commandestring);
            return (bres);
        }
    }
}

