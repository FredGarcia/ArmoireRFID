using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ArmoireV3.Entities
{
    class Log
    {
        private static object objectLocker = new object();

        public static void add(string message)
        {
            if (Properties.Settings.Default.WriteLog)
            {
                lock (objectLocker)
                {
                    FileInfo fileinfo = new FileInfo("log.txt");
                    if (fileinfo.Exists)
                    {
                        if (fileinfo.LastWriteTime.Date < DateTime.Now.Date)
                        {
                            string backupName = "log_" + fileinfo.LastWriteTime.Date.ToString("yyyyMMdd") + ".txt";
                            File.Copy("log.txt", backupName, true);
                            File.Delete("log.txt");
                        }
                    }
                    using (StreamWriter log = fileinfo.AppendText())
                    {
                        log.WriteLine("[" + DateTime.Now.ToString() + "] " + message);
                    }
                }
            }
        }
    }
}
