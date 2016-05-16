
using Microsoft.SharePoint.Administration;
using System;
using System.Diagnostics;
using System.Reflection;

namespace d.SharePoint
{

    public static class SPLog
    {
        public static void Log(Exception ex)
        {
            Log(EventLogEntryType.Error, "Portal", ex.Message);
        }

        public static void Log(string moduleName, Exception ex)
        {
            Log(EventLogEntryType.Error, moduleName, ex.Message);
        }

        public static void Log(MethodBase method, Exception ex)
        {
            string key = method.Name + "(";
            for (int i = 0; i < method.GetParameters().Length; i++)
            {
                key += method.GetParameters().GetValue(i);
                if (i < method.GetParameters().Length - 1)
                    key += ",";
            }
            key += ")";

            Log(EventLogEntryType.Error, method.Name, string.Concat(key, Environment.NewLine, ex.Message));
        }

        public static void Log(EventLogEntryType entry, string moduleName, string logMessage)
        {
            var traceSeverity = TraceSeverity.Unexpected;
            var evtSeverity = EventSeverity.Error;

            switch (entry)
            {
                case EventLogEntryType.Information:
                case EventLogEntryType.FailureAudit:
                case EventLogEntryType.SuccessAudit:
                    traceSeverity = TraceSeverity.Verbose;
                    evtSeverity = EventSeverity.Information;
                    break;
                case EventLogEntryType.Warning:
                    traceSeverity = TraceSeverity.Medium;
                    evtSeverity = EventSeverity.Warning;
                    break;
                default:
                    traceSeverity = TraceSeverity.Unexpected;
                    evtSeverity = EventSeverity.Error;
                    break;
            }

            try
            {

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(moduleName, traceSeverity, evtSeverity), traceSeverity, logMessage, null);

            }
            catch (Exception)
            {
                // throw                
            }
        }
    }
}
