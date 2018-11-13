using System;
using ReactiveDomain;
using ReactiveDomain.Messaging;

// ReSharper disable once CheckNamespace
namespace Model.Domain.Foundation
{
    public class StreamStoreMsgs
    {
        public class CatchupSubscriptionBecameLive : Message
        {
            public CatchupSubscriptionBecameLive()
            { }
        }

        public class SubscriptionDropped : Message
        {
            public readonly string ListenerName;
            public readonly SubscriptionDropReason Reason;
            public readonly Exception Exception;

            public SubscriptionDropped(string listenerName, SubscriptionDropReason reason, Exception exception)
            {
                ListenerName = listenerName;
                Reason = reason;
                Exception = exception;
            }
        }
    }
}
