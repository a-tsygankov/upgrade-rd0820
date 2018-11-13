using Model.Domain.Foundation.Logging;
using ReactiveDomain.Foundation;
using ReactiveDomain.Logging;
using ReactiveDomain.Messaging;
using ReactiveDomain.Messaging.Bus;
using ReactiveDomain.Util;

namespace Domain
{
    public class AccountSvc : 
        IHandleCommand<DomainMsg.CreateAccount>,
        IHandleCommand<DomainMsg.CreditAccount>,
        IHandleCommand<DomainMsg.DebitAccount>
    {
        private static readonly ILogger log = LogFactoryAdapter.GetLogger(typeof(AccountSvc));

        private readonly IRepository _repo;
        private readonly ICommandBus _inputBus;

        public AccountSvc(ICommandBus inputBus, IRepository repo)
        {
            _repo = repo;
            _inputBus = inputBus;

            // Type-specific property commands
            _inputBus.Subscribe<DomainMsg.CreateAccount>(this);
            _inputBus.Subscribe<DomainMsg.CreditAccount>(this);
            _inputBus.Subscribe<DomainMsg.DebitAccount>(this);

            log.Debug("Service started");
        }

        public CommandResponse Handle(DomainMsg.CreateAccount cmd)
        {
            Ensure.NotEmptyGuid(cmd.Id, nameof(cmd.Id));

            var acc = Account.CreateAccount(cmd.Id, cmd);
            _repo.Save(acc);

            return cmd.Succeed();
        }

        public CommandResponse Handle(DomainMsg.CreditAccount cmd)
        {
            Ensure.NotEmptyGuid(cmd.Id, nameof(cmd.Id));

            var acc = _repo.GetById<Account>(cmd.Id);
            acc.Credit(cmd.Amount, cmd);

            _repo.Save(acc);

            return cmd.Succeed();
        }

        public CommandResponse Handle(DomainMsg.DebitAccount cmd)
        {
            Ensure.NotEmptyGuid(cmd.Id, nameof(cmd.Id));

            var acc = _repo.GetById<Account>(cmd.Id);
            acc.Debit(cmd.Amount, cmd);

            _repo.Save(acc);

            return cmd.Succeed();
        }
    }
}
