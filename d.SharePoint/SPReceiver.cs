using System;
using Microsoft.SharePoint;
using System.Collections.Generic;

namespace d.SharePoint.SPReceiver
{
    public class ReceiverFactory
    {
        public static void Register<TReceiverType>(SPWeb web, string listName, SPEventReceiverType receiverType) where TReceiverType : SPItemEventReceiver
        {
            SPList list = web.Lists[listName];

            SPEventReceiverDefinition newReceiver = list.EventReceivers.Add();
            newReceiver.Class = typeof(TReceiverType).Name;
            newReceiver.Assembly = typeof(TReceiverType).Assembly.FullName;
            newReceiver.SequenceNumber = 3000;
            newReceiver.Type = receiverType;
            newReceiver.Update();

        }

        public static void Unregister<TReceiverType>(SPWeb web, string listName, SPEventReceiverType receiverType) where TReceiverType : SPItemEventReceiver
        {
            SPList list = web.Lists[listName];

            List<SPEventReceiverDefinition> EventReceiversToDelete = new List<SPEventReceiverDefinition>();
            SPEventReceiverDefinitionCollection EventReceivers = list.EventReceivers;
            for (int i = 0; i < EventReceivers.Count; i++)
            {
                if (EventReceivers[i].Class.Equals(typeof(TReceiverType).Name) && EventReceivers[i].Type == receiverType)
                {
                    EventReceiversToDelete.Add(EventReceivers[i]);
                }
            }

            int itemCount = EventReceiversToDelete.Count;

            for (int k = itemCount - 1; k >= 0; k--)
            {
                EventReceiversToDelete[k].Delete();
            }
        }

    }


}
