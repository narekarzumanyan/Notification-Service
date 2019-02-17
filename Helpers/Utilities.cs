using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Net.Mail;
using System.Net;
using System.Net.Security;
using System.Timers;
using SS_Notification.Models;
using System.Diagnostics;
using SS_Notification.Managers;
using SafetySystem.Data.Wrappers;
using SafetySystem.Data.EntityModel;

namespace SS_Notification.Helpers
{
   public class Utilities
    {
       
        public static void SetTimerByFixedTime()
        {
            string timeString = ConfigurationManager.AppSettings["WorkTime"];
            string[] sendTimes = timeString.Split(',');
            foreach (string item in sendTimes)
            {
                Timer createOrderTimer = new Timer();
                createOrderTimer.Elapsed += new System.Timers.ElapsedEventHandler(MainFunction);
                createOrderTimer.Interval = GetNextInterval(item.ToString());
                createOrderTimer.Enabled = true;
                createOrderTimer.AutoReset = true;
                createOrderTimer.Start();
            }
        }

        public static void SetTimerByInterval()
        {
            string timeString = ConfigurationManager.AppSettings["WorkTimeByInterval"];
            string[] sendTimes = timeString.Split(',');
            string startTime = sendTimes[0].Trim();
            int interval = Convert.ToInt32(sendTimes[1]);

            Timer createOrderTimer = new Timer();
            createOrderTimer.Elapsed += new System.Timers.ElapsedEventHandler(SetIntervals);
            createOrderTimer.Interval = GetNextInterval(startTime);
            createOrderTimer.Enabled = true;
            createOrderTimer.AutoReset = false;
            createOrderTimer.Start();

        }

        public static void SetIntervals(object sender, ElapsedEventArgs e)
        {
            Timer createOrderTimer = new Timer();
            createOrderTimer.Elapsed += new System.Timers.ElapsedEventHandler(MainFunction);
            createOrderTimer.Interval = Convert.ToInt32(ConfigurationManager.AppSettings["WorkTimeByInterval"].Split(',')[1].Trim()) * 60000;
            createOrderTimer.Enabled = true;
            createOrderTimer.AutoReset = true;
            createOrderTimer.Start();
        }

        public static void MainFunction(object sender, System.Timers.ElapsedEventArgs args)
        {
            Managers.NotificationManager.NotificationSchedulMacker();

            Managers.NotificationManager.NotificationMacker();

            Managers.NotificationManager.SubmitForReview();

            using (NotificationData data = new NotificationData())
            {
                string result = "";
                List<Notification> sendingList = data.GetMany(x => x.SentStatus == 0).ToList();
                foreach (Notification item in sendingList)
                {
                    result = SendEmail(item.SendTo, item.Subject, item.Text);
                    if (result == "success")
                    {
                        item.SentStatus = 1;
                        item.StatusMessage = "successfully sent";
                        item.SentDate = DateTime.Now;
                    }
                    else
                    {
                        item.SentStatus = 2;
                        item.StatusMessage = result;
                    }

                    data.Update(item);
                }
                data.Commit();
            }

        }         

        public static double GetNextInterval(string timeString)
        {
            DateTime t = DateTime.Parse(timeString.Trim());
            TimeSpan ts = new TimeSpan();
            ts = t - System.DateTime.Now;
            if (ts.TotalMilliseconds < 0)
            {
                ts = t.AddDays(1) - System.DateTime.Now;
            }
            return ts.TotalMilliseconds;
        }

        #region SendEmail

        public static string SendEmail(string strToList, string strSubject, string strBody)
        {
            string strMessage;
            using (var message = new MailMessage())
            {
                var smtpClient = new SmtpClient();
                try
                {
                    smtpClient.Host = ConfigurationManager.AppSettings["smtpServerHost"];
                    smtpClient.Port = Convert.ToInt32(ConfigurationManager.AppSettings["SmtpPort"]);
                    // Email From 
                    string strFrom = String.Format("{0} <{1}>", ConfigurationManager.AppSettings["HolderFrom"], ConfigurationManager.AppSettings["EmailFrom"]);
                    message.From = new MailAddress(strFrom);
                    // Email To List
                    string[] arrEmails = strToList.Split(';');
                    foreach (string strEmail in arrEmails)
                    {
                        message.To.Add(String.Format("{0} <{1}>", ConfigurationManager.AppSettings["HolderTo"], strEmail));
                    }

                    message.Subject = strSubject;
                    message.IsBodyHtml = true;
                    message.Body = strBody;

                    smtpClient.Send(message);
                    strMessage = "success";
                }
                catch (Exception ex)
                {
                    strMessage = ex.Message;
                }
            }
            return strMessage;
        }

        private static bool ValidateServerCertificate(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            // replace with proper validation
            if (sslPolicyErrors == SslPolicyErrors.None || sslPolicyErrors == SslPolicyErrors.RemoteCertificateNameMismatch)
                return true;
            return false;
        }
        #endregion

        
    }
}
