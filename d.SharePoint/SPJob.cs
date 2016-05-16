using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;


namespace d.SharePoint.SPJob
{

    public class JobFactory
    {
        public static bool Register<TJobType>(string jobName, SPWeb web, SPSchedule schedule) where TJobType : Job
        {
            bool result;
            try
            {
                if (web == null || web.Site == null)
                {
                    result = false;
                    return result;
                }
                JobFactory.Unregister(jobName, web);

                TJobType tJobType = (TJobType)((object)Activator.CreateInstance(typeof(TJobType), new object[]
		            {                       
			            jobName,
			            web
		            }));
                tJobType.Schedule = (schedule ?? tJobType.GetDefaultShedule());
                tJobType.Update();

                result = true;
            }
            catch (Exception x)
            {
                SPLog.Log(System.Reflection.MethodBase.GetCurrentMethod(), x);
                result = false;
            }
           
            return result;
        }


        public static bool Register<TJobType>(string jobName, SPWeb web) where TJobType : Job
        {
            return JobFactory.Register<TJobType>(jobName, web, null);
        }

        public static void Unregister(string jobName, SPWeb web)
        {

            foreach (SPJobDefinition current in web.Site.WebApplication.JobDefinitions)
            {
                if (current.Name == jobName)
                {
                    current.Delete();
                }
            }

        }
    }


    public abstract class Job : SPJobDefinition
    {

        public Job()
            : base()
        {
        }


        public Job(string jobName, SPWeb web)
            : base(jobName, web.Site.WebApplication, null, SPJobLockType.Job)
        {
            if (string.IsNullOrEmpty(jobName) || web == null)
            {
                throw new ArgumentNullException();
            }

            base.Properties.Add("Web.Url", web.Url);
            base.Properties.Add("Site.ID", web.Site.ID);
            base.Properties.Add("Web.ID", web.ID);
        }

        protected abstract void CodeToRun();

        public override void Execute(Guid targetInstanceId)
        {
            string text = base.Properties["Web.Url"].ToString();

            if (string.IsNullOrEmpty(text))
            {

                throw new ArgumentException("Web.Url");
            }
            try
            {
                d.SharePoint.PortalSecurity.RunWithElevatedPrivileges(new Microsoft.SharePoint.SPSecurity.CodeToRunElevated(this.CodeToRun));
            }
            catch (Exception x)
            {
                SPLog.Log(System.Reflection.MethodBase.GetCurrentMethod(),x);
            }

        }

        public virtual SPSchedule GetDefaultShedule()
        {
            SPDailySchedule sPDailySchedule = new SPDailySchedule();
            sPDailySchedule.BeginHour = 1;
            sPDailySchedule.EndHour = 2;
            return sPDailySchedule;
        }
    }

}
