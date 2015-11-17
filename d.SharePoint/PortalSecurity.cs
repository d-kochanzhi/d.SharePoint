using System;
using Microsoft.SharePoint;

namespace d.SharePoint
{
    public static class PortalSecurity
    {

        /// <summary>
        /// Выполнят код с повышением прав, так же выставляет флаг AllowUnsafeUpdates в тру
        /// </summary>
        /// <param name="web"></param>
        /// <param name="action"></param>
        public static void RunWithElevatedPrivileges(SPWeb web, Action<SPSite, SPWeb> codeToRunElevated)
        {
            RunWithElevatedPrivileges(web.Site.ID, web.ID, codeToRunElevated);
        }

        public static void RunWithElevatedPrivileges(Guid siteID, Guid webID, Action<SPSite, SPWeb> codeToRunElevated)
        {
            Microsoft.SharePoint.SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (SPSite elevatedSiteInstance = new SPSite(siteID))
                {
                    using (SPWeb elevatedWebInstance = elevatedSiteInstance.OpenWeb(webID))
                    {
                        bool allow = elevatedWebInstance.AllowUnsafeUpdates;
                        elevatedWebInstance.AllowUnsafeUpdates = true;
                        codeToRunElevated(elevatedSiteInstance, elevatedWebInstance);
                        elevatedWebInstance.AllowUnsafeUpdates = allow;
                    }
                }
            });
        }

        public static void RunWithElevatedPrivileges(Microsoft.SharePoint.SPSecurity.CodeToRunElevated code)
        {
            Microsoft.SharePoint.SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                code.Invoke();
            });

        }

        /// <summary>
        /// Выполнят код с правами учетной записи, так же выставляет флаг AllowUnsafeUpdates в тру
        /// </summary>
        public static void RunWithSystemAccountPrivileges(SPWeb web, Action<SPSite, SPWeb> codeToRunElevated)
        {
            SPUserToken systemAccountToken = d.SharePoint.PortalUser.GetSystemToken(web.Site);
            if (web.CurrentUser.UserToken == systemAccountToken)
            {
                codeToRunElevated(web.Site, web);
            }
            else
            {
                using (SPSite elevatedSiteInstance = new SPSite(web.Site.ID, systemAccountToken))
                {
                    using (SPWeb elevatedWebInstance = elevatedSiteInstance.OpenWeb(web.ID))
                    {
                        bool allow = elevatedWebInstance.AllowUnsafeUpdates;
                        elevatedWebInstance.AllowUnsafeUpdates = true;
                        codeToRunElevated(elevatedSiteInstance, elevatedWebInstance);
                        elevatedWebInstance.AllowUnsafeUpdates = allow;
                    }
                }
            }
        }


        /// <summary>
        /// выполнит указанный код от имени пользователя
        /// </summary>
        /// <param name="web"></param>
        /// <param name="user"></param>
        /// <param name="codeToRun"></param>
        public static void RunWithUserPriveleges(SPWeb web, Microsoft.SharePoint.SPUser user, Action<SPSite, SPWeb> codeToRun)
        {
            if (web.CurrentUser.LoginName == user.LoginName)
            {
                codeToRun(web.Site, web);
            }
            else
            {
                SPUserToken token = d.SharePoint.PortalUser.GetUserToken(web, user);
                if (token == null)
                    throw new Exception("can't get user token");

                using (var elevatedSite = new SPSite(web.Site.ID, token))
                {
                    using (var elevatedWeb = elevatedSite.OpenWeb(web.ID))
                    {
                        bool allow = elevatedWeb.AllowUnsafeUpdates;
                        elevatedWeb.AllowUnsafeUpdates = true;
                        codeToRun(elevatedSite, elevatedWeb);
                        elevatedWeb.AllowUnsafeUpdates = allow;
                    }
                }
            }
        }
              


    }
}
