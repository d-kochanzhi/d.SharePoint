# d.SharePoint
```
 SPWeb web = properties.Feature.Parent as SPWeb;

try
{
//Register and Unregister SharePoint timer job
d.SharePoint.SPJob.JobFactory.Register<MyTimerJob>("myTimerJob", web, new SPHourlySchedule() { BeginMinute = 10, EndMinute = 30 });
d.SharePoint.SPJob.JobFactory.Unregister("myTimerJob", web);

//Register and Unregister SharePoint receiver
d.SharePoint.SPReceiver.ReceiverFactory.Register<MyEventReceiver>(web, "MyList", SPEventReceiverType.ItemAdding);
d.SharePoint.SPReceiver.ReceiverFactory.Unregister<MyEventReceiver>(web, "MyList", SPEventReceiverType.ItemAdding);

//Run code with different privileges
d.SharePoint.PortalSecurity.RunWithElevatedPrivileges(() => { web.EnsureUser(""); });
d.SharePoint.PortalSecurity.RunWithElevatedPrivileges(web, (elevatedSite, elevatedWeb) => { elevatedWeb.Update(); });
d.SharePoint.PortalSecurity.RunWithUserPriveleges(web, web.Users["someUser"], (elevatedSite, elevatedWeb) => { elevatedWeb.Update(); });
}
catch (Exception ex)
{
// log uls
d.SharePoint.SPLog.Log(ex);
d.SharePoint.SPLog.Log("FeatureActivated", ex);
}
```
