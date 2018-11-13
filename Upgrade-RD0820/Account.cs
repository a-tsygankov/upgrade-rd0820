using ReactiveDomain;
using ReactiveDomain.Messaging;
using ReactiveDomain.Util;
using System;
using System.Threading;
using Model.Domain.Foundation.Logging;
using ReactiveDomain.Logging;

namespace Domain
{
    public class Account : EventDrivenStateMachine
    {
        private static readonly ILogger log = LogFactoryAdapter.GetLogger(typeof(Account));

        private decimal _balance;
        private long _evtCount;

        public decimal Balance => _balance;
        public long EvtCount => _evtCount;

        private Account()
        {
            Register<DomainMsg.AccountCreated>(Apply);
            Register<DomainMsg.AccountCredited>(Apply);
            Register<DomainMsg.AccountDebited>(Apply);
        }

        public static Account CreateAccount(Guid id,
            CorrelatedMessage source)
        {
            Ensure.NotEmptyGuid(id, nameof(id));

            var acc = new Account();
            acc.Raise(new DomainMsg.AccountCreated(
                id, source));

            return acc;
        }

        public void Credit(decimal amount,
            CorrelatedMessage source)
        {
            Raise(new DomainMsg.AccountCredited(Id, amount, source));
        }

        public void Debit(decimal amount,
            CorrelatedMessage source)
        {
            Raise(new DomainMsg.AccountDebited(Id, amount, source));
        }


        private void Apply(DomainMsg.AccountDebited evt)
        {
            Interlocked.Increment(ref _evtCount);
            _balance -= evt.Amount;
        }

        private void Apply(DomainMsg.AccountCredited evt)
        {
            Interlocked.Increment(ref _evtCount);
            _balance += evt.Amount;
        }

        private void Apply(DomainMsg.AccountCreated evt)
        {
            Id = evt.Id;

            Interlocked.Exchange(ref _evtCount, 1);
            _balance = 0;
        }
    }
}