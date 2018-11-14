using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Domain;
using Model.Domain.Foundation.Logging;
using ReactiveDomain.Foundation;
using ReactiveDomain.Logging;
using ReactiveDomain.Messaging.Bus;

namespace Tests
{
    public class AccountRM : ReadModelBase,
        IHandle<DomainMsg.AccountCreated>,
        IHandle<DomainMsg.AccountCredited>,
        IHandle<DomainMsg.AccountDebited>

    {
        private static readonly ILogger log = LogFactoryAdapter.GetLogger(typeof(AccountRM));

        public Guid? AccountId = null;
        public decimal? AccountBalance = null;
        public long AccountEvtCount = 0;

        private long _isUpdating;

        private Timer _timer;

        public bool IsUpdating => _isUpdating > 0;

        public AccountRM(string name, Guid id, Func<IListener> getListener) 
            //: base(name, getListener) // rd 0.8.20
            : base(getListener)  // rd 0.8.17
        {
            log.Trace($"Constructing for account {id}");

            EventStream.Subscribe<DomainMsg.AccountCreated>(this);
            EventStream.Subscribe<DomainMsg.AccountCredited>(this);
            EventStream.Subscribe<DomainMsg.AccountDebited>(this);

            Start<Account>(
                id: id,
                blockUntilLive: true,
                millisecondsTimeout: 2000);
            log.Trace($"Start() unblocked: AccountId={AccountId}, AccountBalance={AccountBalance}, AccountEvtCount={AccountEvtCount}");

        }

        public void Handle(DomainMsg.AccountCreated evt)
        {
            SetTimer();

            AccountId = evt.Id;
            AccountBalance = 0;
            Interlocked.Increment(ref AccountEvtCount);
            log.Trace($"handled event #{AccountEvtCount}");

        }

        public void Handle(DomainMsg.AccountCredited evt)
        {
            SetTimer();

            Thread.Sleep(15); // todo: this call interrupts blockUntilLive with RD 0.8.20
            AccountBalance += evt.Amount;
            Interlocked.Increment(ref AccountEvtCount);
            log.Trace($"handled event #{AccountEvtCount}");
        }

        public void Handle(DomainMsg.AccountDebited evt)
        {
            SetTimer();

            AccountBalance += evt.Amount;

            Interlocked.Increment(ref AccountEvtCount);
        }

        protected void SetTimer()
        {
            Interlocked.Increment(ref _isUpdating);
            if (_timer == null)
            {
                _timer = new Timer(
                    obj => { Interlocked.Exchange(ref _isUpdating, 0); },
                    null,
                    500,
                    Timeout.Infinite);
            }
            else
            {
                _timer.Change(250, Timeout.Infinite);
            }
        }
    }
}
