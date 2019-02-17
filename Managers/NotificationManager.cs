using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SafetySystem.Data;
using SafetySystem.Data.EntityModel;
using SafetySystem.Data.Wrappers;
using SS_Notification.Models;
using System.Data.Objects;
using System.Configuration;

namespace SS_Notification.Managers
{
    public static class NotificationManager
    {
        public static void NotificationSchedulMacker()
        {
            List<CorrectiveActionApproval> approvalList = new List<CorrectiveActionApproval>();
            using(CorrectiveActionApprovalData data = new CorrectiveActionApprovalData())
            {
                List<int> correctiveActionID = data.GetAll().Select(x => x.CorrectiveActionID).ToList();
                List<int> correctiveActionIDDistinct = new List<int>();
                foreach (int item in correctiveActionID)
                {
                    if (!correctiveActionIDDistinct.Exists(x => x == item))
                    {
                        correctiveActionIDDistinct.Add(item);
                    }
                    
                }

                foreach (int item in correctiveActionIDDistinct)
                {
                    CorrectiveActionApproval approval = data.GetMany(x => x.IsApproved == false).First();
                    approvalList.Add(approval);
                }
                
            }

            using (NotificationScheduleData data = new NotificationScheduleData())
            {
                foreach (CorrectiveActionApproval item in approvalList)
                {
                    if (!data.Exists(x=>x.CorrectiveActionApprovalID == item.CorrectiveActionApprovalID))
                    {
                        DateTime newdatetime = DateTime.Now;
                        NotificationSchedule notificationSchedule = new NotificationSchedule()
                        {
                            CorrectiveActionApprovalID = item.CorrectiveActionApprovalID,
                            NotificationDate = null,
                            StartDate = newdatetime,
                            PeriodHours = 48,
                            PeriodCount = 2,
                            NotificationTypeID = 1,
                            NextRunDate = newdatetime
                        };
                        data.Add(notificationSchedule);
                    }
                }
                data.Commit();
            }
        }

        public static void NotificationMacker()
        {
            List<Notification> notificationList = new List<Notification>();


            using (NotificationScheduleData data = new NotificationScheduleData())
            {
                List<NotificationSchedule> notificationScheduleList = data.GetMany(x => x.StartDate.Value == x.NextRunDate.Value || x.NextRunDate.Value <=DateTime.Now).ToList();

                List<NotificationModel> notifications = data.GetMany(x => x.StartDate.Value == x.NextRunDate.Value || x.NextRunDate.Value <= DateTime.Now).Select(y =>
                    new NotificationModel() {
                    Subject = y.NotificationType.TemplateSubject,
                    Text = y.NotificationType.TemplateText.Replace("[[username]]", y.CorrectiveActionApproval.HRRM.FirstName).Replace("[[approveDate]]", y.NextRunDate.ToString()),
                    StatusMessage="",
                    SentStatus=0,
                    SentDate=DateTime.Now,
                    SendTo=y.CorrectiveActionApproval.HRRM.Email
                    }).ToList();

                foreach (NotificationModel item in notifications)
                {
                    Notification additem = new Notification()
                    {
                        SendTo = item.SendTo,
                        SentDate = item.SentDate,
                        SentStatus = item.SentStatus,
                        StatusMessage = item.StatusMessage,
                        Subject = item.Subject,
                        Text = item.Text
                    };
                    notificationList.Add(additem);
                }
                foreach (NotificationSchedule item in notificationScheduleList)
                {
                    DateTime newNexRunTime = item.NextRunDate.Value;

                    if (item.NextRunDate.Value >= item.StartDate.Value.AddHours(item.PeriodCount.Value * item.PeriodHours.Value))
                    {
                        item.NextRunDate = null;
                    }
                    else
                    {
                        item.NextRunDate = newNexRunTime.AddHours(item.PeriodHours.Value);
                    }

                    data.Update(item);
                }
                data.Commit();
            }

            using ( NotificationData data = new NotificationData())
            {
                foreach (Notification item in notificationList)
                {
                    data.Add(item);
                }
                data.Commit();
            }
        }

        public static void SubmitForReview()
        {
            List<Notification> notificationList = new List<Notification>();

            using (NotificationScheduleData data = new NotificationScheduleData())
            {
                List<NotificationSchedule> notificationScheduleList = data.GetMany(x => x.NotificationDate != null && x.NotificationDate.Value <= DateTime.Now).ToList();


                string webBaseUrl = ConfigurationManager.AppSettings["WebBaseUrl"];
                string url = String.Format("{0}IncidentReport/Index?reportID=", webBaseUrl);
                List<NotificationModel> notifications = data.GetMany(x => x.NotificationDate != null && x.NotificationDate.Value <= DateTime.Now).Select(y =>
                    new NotificationModel()
                    {
                        Subject = y.NotificationType.TemplateSubject,
                        Text = y.NotificationType.TemplateText.Replace("[[username]]", y.IncidentReport.HRRM.FirstName).Replace("[[reportNumber]]", y.IncidentReport.IncidentNumber).Replace("[[reportURL]]", url + y.IncidentReportID),
                        StatusMessage = "",
                        SentStatus = 0,
                        SentDate = DateTime.Now,
                        SendTo = y.IncidentReport.HRRM.Email
                    }).ToList();

                foreach (NotificationModel item in notifications)
                {
                    Notification additem = new Notification()
                    {
                        SendTo = item.SendTo,
                        SentDate = item.SentDate,
                        SentStatus = item.SentStatus,
                        StatusMessage = item.StatusMessage,
                        Subject = item.Subject,
                        Text = item.Text
                    };
                    notificationList.Add(additem);
                }

                foreach (NotificationSchedule item in notificationScheduleList)
                {
                    data.Delete(item);
                }
                data.Commit();
            }

            using (NotificationData data = new NotificationData())
            {
                foreach (Notification item in notificationList)
                {
                    data.Add(item);
                }
                data.Commit();
            }
        }

    }

}
