using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.Configuration;
using SS_Notification.Helpers;


namespace SS_Notification
{
    public partial class Service1 : ServiceBase
    {
        protected override void OnStart(string[] args)
        {
            try
            {
                this.eventLog1 = new EventLog();
                AddLog("Server Start"+ DateTime.Now.ToString());



                if (ConfigurationManager.AppSettings["WorkTime"] != "")
                {
                    Utilities.SetTimerByFixedTime();
                }

                if (ConfigurationManager.AppSettings["WorkTimeByInterval"] != "")
                {
                    Utilities.SetTimerByInterval();
                }
              
            }
            catch (Exception ex)
            {
                AddLog(ex.Message);

            }
        }

        protected override void OnStop()
        {
            AddLog("Stop" + DateTime.Now.ToString());
        }

        public void AddLog(string log)
        {
            try
            {
                if (!EventLog.SourceExists("SS_Notification"))
                {
                    EventLog.CreateEventSource("SS_Notification", "SS_Notification");
                }
                eventLog1.Source = "SS_Notification";
                eventLog1.WriteEntry(log);
            }
            catch { }
        }
    }
}
