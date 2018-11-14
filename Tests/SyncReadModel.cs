using Model.Domain.Foundation.Logging;
using ReactiveDomain.Foundation;
using ReactiveDomain.Logging;
using ReactiveDomain.Messaging.Bus;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using StreamStoreMsgs = Model.Domain.Foundation.StreamStoreMsgs;

namespace Tests
{
    public class SyncReadModel :
        ReadModelBase,
        IDisposable,
        IHandle<Model.Domain.Foundation.StreamStoreMsgs.ListenerBecameLive>

    {
        private static readonly ILogger _log = LogFactoryAdapter.GetLogger(typeof(SyncReadModel));

        private readonly ConcurrentBag<string> _liveListeners = new ConcurrentBag<string>();

        public SyncReadModel(string name, Func<IListener> getListener) : base(name, getListener)
        {
            EventStream.Subscribe<Model.Domain.Foundation.StreamStoreMsgs.ListenerBecameLive>(this);
        }

        public void Handle(Model.Domain.Foundation.StreamStoreMsgs.ListenerBecameLive msg)
        {
            _liveListeners.Add(msg.ListenerName);

            _log.Trace($"Listener {msg.ListenerName} is live");
        }

        public void WaitUntil(string name)
        {
            SpinWait.SpinUntil(() => _liveListeners.Contains(name));
        }

        private void DisposeX()
        {
            EventStream?.Unsubscribe<StreamStoreMsgs.ListenerBecameLive>(this);

            base.Dispose();
            _log.Trace("disposed");
        }
    }
}
