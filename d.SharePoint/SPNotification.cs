using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;

namespace d.SharePoint
{
    public class SPNotification
    {
        public enum NotifyType
        {
            Error, Warn, Info, Success
        }

        public static void AddSharePointNotification(Page page,  string text)
        {
            //build up javascript to inject at the tail end of the page 
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("<script language='javascript'>");
            //First wait until the SP.js is loaded, otherwise the notification doesn’t work 
            //gets an null reference exception 
            stringBuilder.AppendLine("ExecuteOrDelayUntilScriptLoaded(ShowNotification, \"sp.js\");");
            stringBuilder.AppendLine("function ShowNotification()");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine(string.Format("SP.UI.Notify.addNotification(\"{0}\");", text));
            stringBuilder.AppendLine("}");
            stringBuilder.AppendLine("</script>");
            //add to the page 

            page.ClientScript.RegisterClientScriptBlock(page.GetType(), "AddSharePointNotification", stringBuilder.ToString());

        }

        public static void AddSharePointStatus(NotifyType severity, Page page, string text)
        {
            string statusBar = @"
                var statusID;
                ExecuteOrDelayUntilScriptLoaded(function(){{
                    ExecuteOrDelayUntilScriptLoaded(
                        function(){{
                            statusID = SP.UI.Status.addStatus(""{0}"", ""<span>{1}</span>"");
                            SP.UI.Status.setStatusPriColor(statusID, ""{2}"");
                        }},
                    'core.js'
                    )}},
                'sp.js'
                );";

            string color = "";
            string title = "";
            if (severity.Equals(NotifyType.Error))
            {
                color = "red";
                title = "Ошибка!";
            }
            else if (severity.Equals(NotifyType.Warn))
            {
                color = "yellow";
                title = "Внимание!";
            }
            else if (severity.Equals(NotifyType.Info))
            {
                color = "blue";
                title = "Информация!";
            }
            else
            {
                color = "green";
                title = "Ок!";
            }
            string script = string.Format(statusBar, title, text, color);
            page.ClientScript.RegisterClientScriptBlock(page.GetType(), "AddSharePointStatus", script, true);
        }


    }


}
