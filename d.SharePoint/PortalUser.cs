using System;
using System.Collections.Generic;
using Microsoft.SharePoint;
using System.Web.Hosting;
using System.DirectoryServices.AccountManagement;
using Microsoft.SharePoint.Utilities;


namespace d.SharePoint
{
    public static class PortalUser
    {
            
 
        /// <summary>
        /// Получить группу пользователей по ИД или NULL
        /// </summary>
        /// <param name="web"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public static SPGroup GetGroupById(SPWeb web, int groupId)
        {
            try
            {
                return web.Groups.GetByID(groupId);
            }
            catch { return null; }
        }

        /// <summary>
        /// попытка получения пользователя по логину
        /// </summary>
        /// <param name="loginName"></param>
        /// <returns></returns>
        public static SPUser GetUserByLoginName(SPWeb web, string loginName)
        {
            SPUser ret = null;
            try
            {
                PortalSecurity.RunWithElevatedPrivileges(web, (elSite, elWeb) =>
                {
                    ret = elWeb.AllUsers[loginName];
                });
            }
            catch
            {
                // скорее всего не удалось найти юзвера по логину или логин был без домена, попробуем иначе
                try
                {
                    PortalSecurity.RunWithElevatedPrivileges(web, (elSite, elWeb) =>
                    {
                        SPPrincipalInfo pinfo = SPUtility.ResolvePrincipal(elWeb,
                            loginName,
                            SPPrincipalType.User,
                            SPPrincipalSource.All,
                            elWeb.AllUsers, false);

                        if (pinfo != null && !string.IsNullOrEmpty(pinfo.LoginName))
                            ret = elWeb.AllUsers[pinfo.LoginName];
                    });
                }
                catch { }
            }
            return ret;
        }

        /// <summary>
        /// попытка получения пользователя по логину, если юзвер не найден и в логине есть домен, то будет попытка добавить его в шарик
        /// </summary>
        /// <param name="loginName"></param>
        /// <returns></returns>
        public static SPUser GetEnsuredUserByLoginName(SPWeb web, string loginName)
        {
            SPUser ret = GetUserByLoginName(web, loginName);
            try
            {
                if (ret == null)
                {
                    PortalSecurity.RunWithElevatedPrivileges(web, (elSite, elWeb) =>
                    {
                        elWeb.AllowUnsafeUpdates = true;
                        elWeb.Update();
                        ret = elWeb.EnsureUser(loginName);
                    });
                }
            }
            catch (Exception ex)
            {
                SPLog.Log(ex);
            }
            return ret;
        }


        /// <summary>
        /// Получить токен системной учетки для указанного сайта
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public static SPUserToken GetSystemToken(SPSite site)
        {
            bool cade = Microsoft.SharePoint.SPSecurity.CatchAccessDeniedException;
            Microsoft.SharePoint.SPSecurity.CatchAccessDeniedException = false;
            SPUserToken token = null;
            try
            {
                token = site.SystemAccount.UserToken;
            }
            catch (UnauthorizedAccessException)
            {
                Microsoft.SharePoint.SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    using (SPSite elevatedSite = new SPSite(site.ID))
                    {
                        token = elevatedSite.SystemAccount.UserToken;
                    }
                });
            }
            finally
            {
                Microsoft.SharePoint.SPSecurity.CatchAccessDeniedException = cade;
            }
            return token;
        }


        /// <summary>
        /// Достает список пользователей из доменной группы
        /// </summary>
        /// <param name="web"></param>
        /// <param name="domainGroup"></param>
        /// <returns></returns>
        public static List<Microsoft.SharePoint.SPUser> GetMembersFromDomainGroup(SPWeb web, Microsoft.SharePoint.SPUser domainGroup)
        {
            if (domainGroup.IsDomainGroup)
            {
                using (HostingEnvironment.Impersonate())
                {
                    try
                    {
                        string groupName = domainGroup.Name;

                        int ind;
                        string domain = string.Empty;
                        if ((ind = groupName.IndexOf("\\")) != -1)
                            domain = groupName.Substring(0, ind);

                        PrincipalSearchResult<Principal> members = null;
                        var principalContext = new PrincipalContext(ContextType.Domain);
                        var group = GroupPrincipal.FindByIdentity(principalContext, groupName);
                        members = group.GetMembers();

                        List<Microsoft.SharePoint.SPUser> ret = new List<Microsoft.SharePoint.SPUser>();

                        foreach (Principal member in members)
                        {
                            UserPrincipal oUserPrincipal = UserPrincipal.FindByIdentity(principalContext, string.Format("{0}\\{1}", domain, member.SamAccountName));
                            if (oUserPrincipal != null && (!oUserPrincipal.IsAccountLockedOut() & oUserPrincipal.AccountExpirationDate == null))
                            {
                                Microsoft.SharePoint.SPUser user;
                                if ((user = GetEnsuredUserByLoginName(web, string.Format("{0}\\{1}", domain, member.SamAccountName))) != null)
                                    ret.Add(user);
                            }


                        }

                        return ret;
                    }
                    catch (Exception x)
                    {
                        SPLog.Log(x);
                    }
                    return new List<Microsoft.SharePoint.SPUser>();
                }
            }
            else
                return new List<Microsoft.SharePoint.SPUser>() { domainGroup };
        }


        /// <summary>
        /// попытается получить токен пользователя
        /// </summary>
        /// <param name="web"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static SPUserToken GetUserToken(SPWeb web, Microsoft.SharePoint.SPUser user)
        {
            if (web == null || user == null)
                return null;

            SPUserToken ret = null;
            d.SharePoint.PortalSecurity.RunWithSystemAccountPrivileges(web, (site, elevatedWeb) =>
            {
                bool cade = Microsoft.SharePoint.SPSecurity.CatchAccessDeniedException;
                Microsoft.SharePoint.SPSecurity.CatchAccessDeniedException = false;
                try
                {
                    Microsoft.SharePoint.SPUser userInfo = elevatedWeb.AllUsers[user.LoginName];
                    if (userInfo != null)
                        ret = userInfo.UserToken;
                }
                catch (UnauthorizedAccessException)
                {

                }
                finally
                {
                    Microsoft.SharePoint.SPSecurity.CatchAccessDeniedException = cade;
                }
            });
            return ret;
        }

    }
}
