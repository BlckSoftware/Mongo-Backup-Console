using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Threading;
using System.Configuration;
using System.Net.Mail;
using System.Net;

/// <summary>

/// </summary>
namespace MongoBackupConsole
{
    class Program
    {        
        static void Main(string[] args)
        {
            ConsoleExtension.Hide();
            if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "Backup"))
            {
                Directory.Delete(AppDomain.CurrentDomain.BaseDirectory + "Backup", true);
            }
            Dump.dump();            
        }
        static class Dump 
        {
           string dbName="YourDB";
            #region dump
            public static void dump()
            {
               ConsoleExtension.Hide();
               string Today = DateTime.Now.ToString("dd-MM-yyyy");// Tarih saat formatı mevcut tarih saat formatında ayarlandı.
               string time = DateTime.Now.ToString("HH:mm:ss dd-MM-yyyy ");
               
                string strCmdText;
                strCmdText = "/K mongodump --db dbName --out Backup/"+ Today;
               
                Process cmd = new Process();
                cmd.StartInfo.FileName ="cmd.exe";
                cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                cmd.StartInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                cmd.StartInfo.Arguments = strCmdText;
                cmd.Start();

                WriteToFile(time + " | ------ BACKUP İŞLEMİ  BAŞLATILDI ------ ");
                int TimeOut = Convert.ToInt32(ConfigurationManager.AppSettings["TimeOut"]);
                int key = Convert.ToInt32(ConfigurationManager.AppSettings["Key"]);
                
                for (int i = 0; i < TimeOut; i++) {  Thread.Sleep(2000); }
                try
                {
                    cmd.Kill();
                    WriteToFile(time + " | ------ MONGODUMP CONSOLE KAPATILDI -----");
                }
                catch (Exception ex)
                {
                    WriteToFile(time + " | ------ MONGODUMP CONSOLE KAPATILAMADI -----\n"+ex);
                }
                
                string path = AppDomain.CurrentDomain.BaseDirectory + "Backup" + '\\' + Today;               


                if (Directory.Exists(path))
                {
                    string log = time + " | BACKUP'I ALINDI \n\n BACKUP YERİ = " + path;
                    WriteToFile(log);
                    if (key == 1) { Yandex(log); }
                }
                else
                {
                    string log = time + " | BACKUP'I ALINAMADI BURAYI KONTROL EDİN : " + path;
                    WriteToFile(log);
                    if (key == 1) { Yandex(log); } 
                }
            }
            #endregion
            #region Mail
            public static void Yandex(string logger)
            {
                string Today = DateTime.Now.ToString("HH:mm:ss d-MM-yyyy ");
                string log = logger;
                string alıcılar = ConfigurationManager.AppSettings["To"];
                string tesis = ConfigurationManager.AppSettings["TESIS_ADI"];
                string subject = (tesis + " " + " DATABASE BACKUP hk.");
                string content = ("\n DİKKAT \n\n "+Today+" TARİHLİ  " + tesis + " SmartBaseNOSQL  DATABASE " + log + "\n \n\n  Bilgi için : 'huseyinkarayazim@gmail.com.tr' ile iletişime geçin. ");
                var _host = ConfigurationManager.AppSettings["SMTP_HOST"];
                var _port = Convert.ToInt32(ConfigurationManager.AppSettings["SMTP_PORT"]);
                var _defaultCredentials = false;
                var _enableSsl = true;
                var _emailfrom = ConfigurationManager.AppSettings["EMAIL"];//Your yandex email adress
                var _password = ConfigurationManager.AppSettings["PASS"];//Your yandex app password
                using (var smtpClient = new SmtpClient(_host, _port))
                {
                    smtpClient.EnableSsl = _enableSsl;
                    smtpClient.UseDefaultCredentials = _defaultCredentials;
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;

                    if (_defaultCredentials == false)
                    {
                        smtpClient.Credentials = new NetworkCredential(_emailfrom, _password);
                    }
                    try
                    {
                        smtpClient.Send(_emailfrom, alıcılar, subject, content);
                        WriteToFile( Today+" | MAIL GONDERILDI : \n"+log );
                    }
                    catch (Exception ex)
                    {
                        WriteToFile(Today+" | MAIL GONDERILEMEDI HATA : \n" + ex);
                    }
                }
            }
            #endregion
            #region log
            public static void WriteToFile(string Message)
            {
                string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }                
                string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\ConsoleLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
                if (!File.Exists(filepath))
                {
                    // Create a file to write to.   
                    using (StreamWriter sw = File.CreateText(filepath))
                    {
                        sw.WriteLine("---------------------------------------------------------------------------\n");
                        sw.WriteLine(Message);
                        sw.WriteLine("---------------------------------------------------------------------------\n");
                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(filepath))
                    {
                        sw.WriteLine("---------------------------------------------------------------------------\n");
                        sw.WriteLine(Message);
                        sw.WriteLine("---------------------------------------------------------------------------\n");
                    }
                }
            }
            #endregion

        }
        #region Show OR Hide Console
        static class ConsoleExtension
        {
            const int SW_HIDE = 0;
            const int SW_SHOW = 5;
            readonly static IntPtr handle = GetConsoleWindow();
            [DllImport("kernel32.dll")] static extern IntPtr GetConsoleWindow();
            [DllImport("user32.dll")] static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

            public static void Hide()
            {
                ShowWindow(handle, SW_HIDE); //hide the console
            }
            public static void Show()
            {
                ShowWindow(handle, SW_SHOW); //show the console
            }
        }
        #endregion
    }


}



