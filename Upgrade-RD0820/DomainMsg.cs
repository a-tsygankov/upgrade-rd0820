using Newtonsoft.Json;
using ReactiveDomain.Messaging;
using System;

namespace Domain
{
    public class DomainMsg
    {
        public class CreateAccount : Command
        {
            public readonly Guid Id;

            public CreateAccount(Guid id,
                CorrelatedMessage source) : this(id, new CorrelationId(source), new SourceId(source))
            {
            }

            [JsonConstructor]
            public CreateAccount(
                Guid id,
                CorrelationId correlationId,
                SourceId sourceId)
                : base(correlationId, sourceId)
            {
                Id = id;
            }
        }

        public class AccountCreated : Event
        {
            public readonly Guid Id;

            public AccountCreated(Guid id,
                CorrelatedMessage source) : this(id, new CorrelationId(source), new SourceId(source))
            {
            }

            [JsonConstructor]
            public AccountCreated(
                Guid id,
                CorrelationId correlationId,
                SourceId sourceId)
                : base(correlationId, sourceId)
            {
                Id = id;
            }
        }

        public class CreditAccount : Command
        {
            public readonly Guid Id;
            public readonly decimal Amount;

            public CreditAccount(Guid id,
                decimal amount,
                CorrelatedMessage source) : this(id, amount, new CorrelationId(source), new SourceId(source))
            {
            }

            [JsonConstructor]
            public CreditAccount(
                Guid id,
                decimal amount,
                CorrelationId correlationId,
                SourceId sourceId)
                : base(correlationId, sourceId)
            {
                Id = id;
                Amount = amount;
            }
        }

        public class AccountCredited : Event
        {
            public readonly Guid Id;
            public readonly decimal Amount;

            public AccountCredited(Guid id,
                decimal amount,
                CorrelatedMessage source) : this(id, amount, new CorrelationId(source), new SourceId(source))
            {
            }

            [JsonConstructor]
            public AccountCredited(
                Guid id,
                decimal amount,
                CorrelationId correlationId,
                SourceId sourceId)
                : base(correlationId, sourceId)
            {
                Id = id;
                Amount = amount;
            }
        }
        public class DebitAccount : Command
        {
            public readonly Guid Id;
            public readonly decimal Amount;

            public DebitAccount(Guid id,
                decimal amount,
                CorrelatedMessage source) : this(id, amount, new CorrelationId(source), new SourceId(source))
            {
            }

            [JsonConstructor]
            public DebitAccount(
                Guid id,
                decimal amount,
                CorrelationId correlationId,
                SourceId sourceId)
                : base(correlationId, sourceId)
            {
                Id = id;
                Amount = amount;
            }
        }

        public class AccountDebited : Event
        {
            public readonly Guid Id;
            public readonly decimal Amount;

            public AccountDebited(Guid id,
                decimal amount,
                CorrelatedMessage source) : this(id, amount, new CorrelationId(source), new SourceId(source))
            {
            }

            [JsonConstructor]
            public AccountDebited(
                Guid id,
                decimal amount,
                CorrelationId correlationId,
                SourceId sourceId)
                : base(correlationId, sourceId)
            {
                Id = id;
                Amount = amount;
            }
        }
    }
}