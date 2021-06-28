using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;

namespace ArmoireV3
{
    public class Rail
    {
        static byte[] buffer = new byte[100];
        static SerialPort RailComPort ;
        
        public void initRail(string port)
        {
            if (Properties.Settings.Default.RailIsMisumi)
            { // Misumi
                
                RailComPort = new SerialPort(port, 38400, Parity.Odd, 8, StopBits.One);
                RailComPort.ReadTimeout = 1000;
                RailComPort.Open();
                if (!Properties.Settings.Default.RailIsCoding)
                {
                    if (RailComPort.IsOpen == true)
                        Init();
                }
                else
                {
                    if (RailComPort.IsOpen == true)
                        CodingInit();
                }
            }
            else
            { // Excitron
                RailComPort = new SerialPort(port, 57600, Parity.None, 8, StopBits.One);
                RailComPort.ReadTimeout = 1000;
                RailComPort.Open();
                if (RailComPort.IsOpen == true)
                    ExcitronInitRail();
            }
        }
        

        public bool CheckRail()
        { // A n'executer que si on est en panne de rail pour ne pas avoir de conflit d'accès aux ressources
            if (Properties.Settings.Default.RailIsMisumi)
            { // Misumi
                try
                {
                    Dispose();
                    initRail("COM" + Properties.Settings.Default.COMrail.ToString());
                }
                catch (Exception e)
                {
                    if (Properties.Settings.Default.UseDBGMSG)
                        System.Windows.MessageBox.Show("Erreur opening COM port: " + e.Message);
                    ArmoireV3.Entities.Log.add("Erreur opening COM port: " + e.Message);
                    return false;
                }
                try
                {
                    if (Properties.Settings.Default.RailIsMisumi)
                    {
                        ServoON();
                        return true;
                    }
                    else if (Properties.Settings.Default.RailIsCoding)
                    {
                        CodingServoON();
                        return true;
                    }
                    else
                    {
                        CommandWakeUp(Properties.Settings.Default.RailIsMisumi);
                        return true;
                    }
                }
                catch (Exception e)
                {
                    if (Properties.Settings.Default.UseDBGMSG)
                        System.Windows.MessageBox.Show("Erreur MoveRail: " + e.Message);
                    ArmoireV3.Entities.Log.add("Erreur MoveRail: " + e.Message);
                    return false;
                }
            }
            else
            {
                // Excitron ne renvoi pas de réponse
                return true;
            }
        }

        public static bool MoveRail(bool IsMisumi)
        {
            try
            {
                if (IsMisumi)
                {
                    if (!Properties.Settings.Default.RailIsCoding)
                        MisumiRoundTripSeq();
                    else
                        CodingRoundTripSeq();
                    return true;
                }
                else
                {
                    ExcitronRoundTripSeq();
                    return true;
                }
            }
            catch (Exception e)
            {
                if (Properties.Settings.Default.UseDBGMSG)
                    System.Windows.MessageBox.Show("Erreur MoveRail: " + e.Message);
                ArmoireV3.Entities.Log.add("Erreur MoveRail: " + e.Message);
                return false;
            }
        }

        public void Dispose()
        {
            if (RailComPort.IsOpen)
                RailComPort.Close();
            RailComPort.Dispose();
        }


        public bool TestRailDrv(bool oldControler, bool IsMisumi)
        {
            int nb = 0;
            if (IsMisumi)
            { // Misumi
                return true;
            }
            else
            { // Excitron
                try
                {// Reset : try the wake-up command
                    RailComPort.WriteLine("w");
                }
                catch
                {
                    return false;
                }
                System.Threading.Thread.Sleep(100);
                try
                {
                    nb = RailComPort.Read(buffer, 0, 6);
                }
                catch
                {
                    return false;
                }

           
                if ((nb != 0) && (buffer[0] == (byte)'w')) // Renvoi "w\n\n\r\7E"
                {
                    return true;
                }
                else if (oldControler)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }

         public void GoRight(bool isMisumi)
         {
             if (isMisumi == false)
                 ExcitronGoRight();
             else
                 MisumiGoRight();
         }

         public void GoLeft(bool isMisumi)
         {
             if (isMisumi == false)
                 ExcitronGoLeft();
             else
                 MisumiGoLeft();
         }

         public void Reset(bool isMisumi)
         {
             if (isMisumi == false)
                 ExcitronReset();
         }

         public void CommandWakeUp(bool isMisumi)
         {
             if (isMisumi == false)
                 ExcitronCommandWakeUp();
         }

         public string CommandExcitronSetSteps(string steps, bool oldversion)
         {
             try
             {
                 if (steps.Length == 7)
                 {
                     RailComPort.Write("N");
                     // textBox1.Text += serialPort.ReadLine();
                     Thread.Sleep(500);
                     RailComPort.WriteLine(steps);
                     if (oldversion == false)
                         return RailComPort.ReadLine() /*+ serialPort.ReadLine() + serialPort.ReadLine()*/;
                 }
                 else
                 {
                     return "Erreur de paramètre\n";
                 }
             }
             catch
             {
                 return "Une erreur sur commandExcitronSetStep";
             }
             return "other Error commandExcitronSetStep";
         }


         #region privéCoding

         static void CodingInit()
         {
             Thread.Sleep(100);
             CodingServoON();
             Thread.Sleep(100);
             //CodingReturnToOrigin();
             //Thread.Sleep(100);
             CodingServoOFF();
             Thread.Sleep(100);
         }
         static string CodingServoOFF()
         {
             string temp = "";
             try
             {
                 RailComPort.ReadTimeout = 100;
                 RailComPort.WriteLine("@SRVO0.1\r\n");
                 Thread.Sleep(200);
                 temp = RailComPort.ReadLine();
                 ArmoireV3.Entities.Log.add("SRVO0.1:" + temp);
             }
             catch (TimeoutException te)
             {
                 ArmoireV3.Entities.Log.add("SRVO0.1: Timeout" + te.Message);
             }
             catch (InvalidOperationException ioe)
             {
                 ArmoireV3.Entities.Log.add("SRVO0.1: InvalidOp" + ioe.Message);// Le port est fermé
             }
             catch
             {
                 ArmoireV3.Entities.Log.add("SRVO0.1: Autre Erreur");
             }
             return temp;
         }
         static string CodingServoON()
         {
             string temp = "";
             try
             {
                 RailComPort.ReadTimeout = 100;
                 RailComPort.WriteLine("@SRVO1.1\r\n");
                 Thread.Sleep(200);
                 temp = RailComPort.ReadLine();
                 ArmoireV3.Entities.Log.add("SRVO1.1:" + temp);
             }
             catch (TimeoutException te)
             {
                 ArmoireV3.Entities.Log.add("SRVO1.1: Timeout" + te.Message);
                 throw (new Exception("Erreur Rail: Timeout"));
             }
             catch (InvalidOperationException ioe)
             {
                 ArmoireV3.Entities.Log.add("SRVO1.1: InvalidOp" + ioe.Message);// Le port est fermé
                 throw (new Exception("Erreur Rail: InvalidOp"));
             }
             catch
             {
                 ArmoireV3.Entities.Log.add("SRVO1.1: Autre Erreur");
                 throw (new Exception("Erreur Rail: Autre Erreur"));
             }
             return temp;
         }

         static string CodingStopOp()
         {
             string temp = "";
             try
             {
                 RailComPort.ReadTimeout = 100;
                 RailComPort.WriteLine("@STOP.1\r\n");
                 Thread.Sleep(500);
                 temp = RailComPort.ReadLine();
                 ArmoireV3.Entities.Log.add("STOP.1:" + temp);
             }
             catch (TimeoutException te)
             {
                 ArmoireV3.Entities.Log.add("STOP.1: Timeout" + te.Message);
             }
             catch (InvalidOperationException ioe)
             {
                 ArmoireV3.Entities.Log.add("STOP.1: InvalidOp" + ioe.Message);// Le port est fermé
             }
             catch
             {
                 ArmoireV3.Entities.Log.add("STOP.1: Autre Erreur");
             }
             return temp;
         }
         static string CodingMoveToPoint(int pointNumber)
         {
             string temp = "";
             try
             {
                 RailComPort.ReadTimeout = 4000;
                 RailComPort.WriteLine("@START" + pointNumber + ".1\r\n");
                 //Thread.Sleep(4000);
                 Thread.Sleep(100);
                 temp = RailComPort.ReadLine();
                 ArmoireV3.Entities.Log.add("START:" + temp);
             }
             catch (TimeoutException te)
             {
                 ArmoireV3.Entities.Log.add("START.1: Timeout" + te.Message);
             }
             catch (InvalidOperationException ioe)
             {
                 ArmoireV3.Entities.Log.add("START.1: InvalidOp" + ioe.Message);// Le port est fermé
             }
             catch
             {
                 ArmoireV3.Entities.Log.add("START.1: Autre Erreur");
             }
             return temp;
         }

         static string CodingReturnToOrigin()
         {
             string temp = "";
             try
             {
                 RailComPort.ReadTimeout = 4000;
                 RailComPort.WriteLine("@ORG.1\r\n");
                 //Thread.Sleep(4000);
                 Thread.Sleep(100);
                 temp = RailComPort.ReadLine();
                 temp += RailComPort.ReadLine();
                 ArmoireV3.Entities.Log.add("ORG.1:" + temp);
             }
             catch (TimeoutException te)
             {
                 ArmoireV3.Entities.Log.add("ORG.1: Timeout" + te.Message);
             }
             catch (InvalidOperationException ioe)
             {
                 ArmoireV3.Entities.Log.add("ORG.1: InvalidOp" + ioe.Message);// Le port est fermé
             }
             catch
             {
                 ArmoireV3.Entities.Log.add("ORG.1: Autre Erreur");
             }
             return temp;
         }
         public string CodingReadParam()
         {
             string temp = "";
             try
             {
                 RailComPort.ReadTimeout = 100;
                 RailComPort.WriteLine("@READ\r\n");
                 Thread.Sleep(100);
                 temp = RailComPort.ReadLine();
             }
             catch
             {
                 ArmoireV3.Entities.Log.add("READ PARAM ERROR");
             }
             return temp;
         }
         public void CodingSetParam(string NbPas, string NbInt)
         {
             string temp = "";
             try
             {
                 RailComPort.ReadTimeout = 100;

                 RailComPort.WriteLine("@STEP" + NbPas + "\r\n");
                 Thread.Sleep(100);
                 temp = RailComPort.ReadLine();
                 RailComPort.WriteLine("@INTV" + NbInt + "\r\n");
                 Thread.Sleep(100);
                 temp = RailComPort.ReadLine();
             }
             catch
             {
                 ArmoireV3.Entities.Log.add("STEP_INTV ERROR");
             }
         }

         public string CodingReadSW()
         {
             string temp = "";
             string temp1 = "";
             try
             {
                 RailComPort.ReadTimeout = 100;

                 RailComPort.Write("@INPUT1\r\n");
                 Thread.Sleep(200);
                 temp = RailComPort.ReadExisting();
                 temp = temp.Replace("G", "");
                 temp = temp.Replace("?", "");
                 temp = "Input1=" + temp.Substring(0, 1);

                 RailComPort.Write("@INPUT2\r\n");
                 Thread.Sleep(200);
                 temp1 = RailComPort.ReadExisting();
                 temp1 = temp1.Replace("G", "");
                 temp1 = temp1.Replace("?", "");
                 temp1 = " Input2=" + temp1.Substring(0, 1);
                 temp = temp + temp1;
             }
             catch
             {
                 ArmoireV3.Entities.Log.add("INPUT ERROR");
             }
             return temp;
         }
         static void CodingRoundTripSeq()
         {
             CodingServoON();
             Thread.Sleep(100);
             CodingMoveToPoint(2);
             Thread.Sleep(100);
             CodingStopOp();
             Thread.Sleep(100);
             CodingMoveToPoint(1);
             Thread.Sleep(100);
             CodingStopOp();
//             CodingReturnToOrigin();
             Thread.Sleep(100);
             CodingServoOFF();

         }


         private void CodingGoRight()
         {
             CodingServoON();
             Thread.Sleep(100);
             CodingMoveToPoint(2);
             Thread.Sleep(100);
             CodingStopOp();
             Thread.Sleep(100);
             CodingServoOFF();
         }

         private void CodingGoLeft()
         {
             CodingServoON();
             Thread.Sleep(100);
             CodingMoveToPoint(1);
             Thread.Sleep(100);
             CodingStopOp();
             Thread.Sleep(100);
             CodingServoOFF();
         }

         #endregion privéCoding
        #region privéMisumi

        static void Init()
        {
            Thread.Sleep(100);
            ServoON();
            Thread.Sleep(100);
            ReturnToOrigin();
            Thread.Sleep(100);
            ServoOFF();
            Thread.Sleep(100);
        }
        static string ServoOFF()
        {
            string temp= "";
            try
            {
                RailComPort.WriteLine("@SRVO0.1\r\n");
                Thread.Sleep(200);
                temp = RailComPort.ReadLine();
                ArmoireV3.Entities.Log.add("SRVO0.1:" + temp);
            }
            catch (TimeoutException te)
            {
                ArmoireV3.Entities.Log.add("SRVO0.1: Timeout" + te.Message);
            }
            catch (InvalidOperationException ioe)
            {
                ArmoireV3.Entities.Log.add("SRVO0.1: InvalidOp" + ioe.Message);// Le port est fermé
            }
            catch
            {
                ArmoireV3.Entities.Log.add("SRVO0.1: Autre Erreur");
            }
            return temp;
        }
        static string ServoON()
        {
            string temp = "";
            try
            {
                RailComPort.WriteLine("@SRVO1.1\r\n");
                Thread.Sleep(200);
                temp = RailComPort.ReadLine();
                ArmoireV3.Entities.Log.add("SRVO1.1:" + temp);
            }
            catch (TimeoutException te)
            {
                ArmoireV3.Entities.Log.add("SRVO1.1: Timeout" + te.Message);
            }
            catch (InvalidOperationException ioe)
            {
                ArmoireV3.Entities.Log.add("SRVO1.1: InvalidOp" + ioe.Message);// Le port est fermé
            }
            catch
            {
                ArmoireV3.Entities.Log.add("SRVO1.1: Autre Erreur");
            }
            return temp;
        }

        static string StopOp()
        {
            string temp = "";
            try
            {
                RailComPort.WriteLine("@STOP.1\r\n");
                Thread.Sleep(500);
                temp = RailComPort.ReadLine();
                ArmoireV3.Entities.Log.add("STOP.1:" + temp);
            }
            catch (TimeoutException te)
            {
                ArmoireV3.Entities.Log.add("STOP.1: Timeout" + te.Message);
            }
            catch (InvalidOperationException ioe)
            {
                ArmoireV3.Entities.Log.add("STOP.1: InvalidOp" + ioe.Message);// Le port est fermé
            }
            catch
            {
                ArmoireV3.Entities.Log.add("STOP.1: Autre Erreur");
            }
            return temp;
        }
        static string MoveToPoint(int pointNumber)
        {
            string temp = "";
            try
            {
                RailComPort.ReadTimeout = 4000;
                RailComPort.WriteLine("@START" + pointNumber + ".1\r\n");
                //Thread.Sleep(4000);
                Thread.Sleep(100);
                temp = RailComPort.ReadLine();
                ArmoireV3.Entities.Log.add("START:" + temp);
            }
            catch (TimeoutException te)
            {
                ArmoireV3.Entities.Log.add("START.1: Timeout" + te.Message);
            }
            catch (InvalidOperationException ioe)
            {
                ArmoireV3.Entities.Log.add("START.1: InvalidOp" + ioe.Message);// Le port est fermé
            }
            catch
            {
                ArmoireV3.Entities.Log.add("START.1: Autre Erreur");
            }
            return temp;
        }

        static string ReturnToOrigin()
        {
            string temp = "";
            try
            {
                RailComPort.ReadTimeout = 4000;
                RailComPort.WriteLine("@ORG.1\r\n");
                //Thread.Sleep(4000);
                Thread.Sleep(100);
                temp = RailComPort.ReadLine();
                temp += RailComPort.ReadLine();
                ArmoireV3.Entities.Log.add("ORG.1:" + temp);
            }
            catch (TimeoutException te)
            {
                ArmoireV3.Entities.Log.add("ORG.1: Timeout" + te.Message);
            }
            catch (InvalidOperationException ioe)
            {
                ArmoireV3.Entities.Log.add("ORG.1: InvalidOp" + ioe.Message);// Le port est fermé
            }
            catch
            {
                ArmoireV3.Entities.Log.add("ORG.1: Autre Erreur");
            }
            return temp;
        }
 
        static void MisumiRoundTripSeq()
        {
            ServoON();
            Thread.Sleep(100);
            MoveToPoint(2);
            Thread.Sleep(4500);
            StopOp();
            MoveToPoint(1);
            Thread.Sleep(4000);
            StopOp();
            ReturnToOrigin();
            Thread.Sleep(100);
            ServoOFF();
           
        }


        private void MisumiGoRight()
        {
            ServoON();
            Thread.Sleep(100);
            MoveToPoint(2);
            Thread.Sleep(4500);
            StopOp();
            Thread.Sleep(100);
            ServoOFF();
        }

        private void MisumiGoLeft()
        {
            ServoON();
            Thread.Sleep(100);
            MoveToPoint(1);
            Thread.Sleep(4000);
            StopOp();
            Thread.Sleep(100);
            ServoOFF();
        }

        #endregion privéMisumi

        #region privéExcitron


        private void ExcitronInitRail()
        {
            string temp;
            try
            {
                RailComPort.DiscardInBuffer();
                RailComPort.WriteLine("w");
                if (Properties.Settings.Default.OldExcitron == false)
                {
                    RailComPort.ReadLine();
                    RailComPort.WriteLine("W");
                    RailComPort.ReadLine();
                    RailComPort.ReadLine();
                }
                else
                {
                    Thread.Sleep(500);
                }
                if (Properties.Settings.Default.OldExcitron == false)
                {
                    CommandSetSteps("0009500");
                }
                else
                {
                    CommandSetSteps("0018000");
                }
                CommandSetSpeed("06000");
                CommandSetTorque("080");

                RailComPort.ReadTimeout = 5000;

                RailComPort.WriteLine("G");
                if (Properties.Settings.Default.OldExcitron == false)
                {
                    temp = RailComPort.ReadLine();

                    temp = RailComPort.ReadLine();
                }
                else
                {
                    Thread.Sleep(5000);
                }
            }
            catch
            {
                ArmoireV3.Entities.Log.add("Error Getting Rail initialized");
            }

        }


        private static void ExcitronGoRight()
        {
            string temp;
            try
            {
                RailComPort.DiscardInBuffer();
                RailComPort.WriteLine("C");
                if (Properties.Settings.Default.OldExcitron == false)
                    temp = RailComPort.ReadLine();
                else
                    Thread.Sleep(100);
                Thread.Sleep(500);

                RailComPort.WriteLine("G");
                if (Properties.Settings.Default.OldExcitron == false)
                {
                    temp = RailComPort.ReadLine();
                    temp = RailComPort.ReadLine();
                    temp = RailComPort.ReadLine();
                }
                else
                {
                    Thread.Sleep(5000);
                }
            }
            catch
            {
                ArmoireV3.Entities.Log.add("Error Going Right");
            }
        }


        private static  void ExcitronGoLeft()
        {
            string temp;
            try
            {
                RailComPort.DiscardInBuffer();
                RailComPort.WriteLine("W");
                if (Properties.Settings.Default.OldExcitron == false)
                    temp = RailComPort.ReadLine();
                else
                    Thread.Sleep(100);
                Thread.Sleep(500);

                RailComPort.WriteLine("G");
                if (Properties.Settings.Default.OldExcitron == false)
                {
                    temp = RailComPort.ReadLine();
                    temp = RailComPort.ReadLine();
                    temp = RailComPort.ReadLine();

                }
                else
                {
                    Thread.Sleep(4000);
                }
            }
            catch
            {
                ArmoireV3.Entities.Log.add("Error going Left");
            }
        }
        


        private void CommandSetSteps(string steps)
        {
            try
            {
                RailComPort.DiscardInBuffer();
                if (steps.Length == 7)
                {
                    string temp;
                    RailComPort.Write("N");
                    Thread.Sleep(1000);
                    RailComPort.WriteLine(steps);
                    if (Properties.Settings.Default.OldExcitron == false)
                        temp = RailComPort.ReadLine();
                    else
                        Thread.Sleep(100);
                }
            }
            catch
            {
                ArmoireV3.Entities.Log.add("Error Setting Nb Steps");
            }
        }

        private void CommandSetTorque(string torque)
        {
            try
            {
                RailComPort.DiscardInBuffer();
                if (torque.Length == 3)
                {
                    string temp;
                    RailComPort.WriteLine("T");
                    Thread.Sleep(1000);
                    RailComPort.WriteLine(torque);
                    if (Properties.Settings.Default.OldExcitron == false)
                    {
                        temp = RailComPort.ReadLine();
                        temp = RailComPort.ReadLine();
                        temp = RailComPort.ReadLine();
                    }
                    else
                    {
                        Thread.Sleep(600);
                    }
                }
            }
            catch
            {
                ArmoireV3.Entities.Log.add("Error Setting Torque");
            }
            
        }
        
        private void CommandSetSpeed(string speed)
        {
            try
            {

                RailComPort.DiscardInBuffer();
                if (speed.Length == 5)
                {
                    string temp;
                    RailComPort.WriteLine("V");
                    Thread.Sleep(500);
                        RailComPort.WriteLine(speed);
                        if (Properties.Settings.Default.OldExcitron == false)
                        {
                            temp = RailComPort.ReadLine();
                            temp = RailComPort.ReadLine();
                        }
                        else
                        {
                            Thread.Sleep(400);
                        }
                }
            }
            catch
            {
                ArmoireV3.Entities.Log.add("Error Setting Speed");
            }
        }

        private void CommandSetAcceleration(string acc)
        {
            try
            {
                RailComPort.DiscardInBuffer();
                if (acc.Length == 4)
                {
                    string temp;
                    RailComPort.WriteLine("A");
                    Thread.Sleep(500);
                    RailComPort.WriteLine(acc);
                    if (Properties.Settings.Default.OldExcitron == false)
                    {
                        temp = RailComPort.ReadLine();
                        temp = RailComPort.ReadLine();
                    }
                    else
                    {
                        Thread.Sleep(400);
                    }
                }
            }
            catch
            {
                ArmoireV3.Entities.Log.add("Error Setting Accleration");
            }
        }

        static void ExcitronRoundTripSeq()
        {
            ExcitronGoRight();
            Thread.Sleep(600);
            ExcitronGoLeft();
        }

        private string ExcitronCommandWakeUp()
        {
            string temp="";
            try
            {
                RailComPort.DiscardInBuffer();
                RailComPort.Write("w");
                if (Properties.Settings.Default.OldExcitron == false)
                {
                    temp = RailComPort.ReadLine();
                }
                else
                {
                    Thread.Sleep(200);
                }
            }
            catch
            {
                ArmoireV3.Entities.Log.add("Error waking up");
            }
            return temp;
        }
        /*
        private void CommandWakeUp()
        {
            try
            {
                serialPort.WriteLine("G");
                textBox1.Text += "Commande de réveil exécutée\n";
                if (checkBoxVersionControleur.IsChecked == false)
                    textBox1.Text += serialPort.ReadLine() + serialPort.ReadLine();
            }
            catch
            {
                textBox1.Text += "Une erreur";
            }
        }*/

        private void ExcitronReset()
        {
            try
            {
                RailComPort.WriteLine("W");
            }
            catch
            {
                ArmoireV3.Entities.Log.add("Une erreur au reset");
            }
        }
        
        #endregion privéExcitron
    }
}
