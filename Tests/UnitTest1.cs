using System;
using Domain;
using Model.Domain.Foundation.Logging;
using ReactiveDomain.Logging;
using ReactiveDomain.Messaging;
using Xunit;
using Xunit.Abstractions;

namespace Tests
{
    [Collection("Account Tests")]
    public class UnitTest1
    {
        private static readonly ILogger log = LogFactoryAdapter.GetLogger(typeof(UnitTest1));
        private readonly ITestOutputHelper _toh;
        private readonly AccountFixture _fixture;

        public UnitTest1(ITestOutputHelper toh, AccountFixture fixture)
        {
            //RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            _toh = toh;
            _fixture = fixture;
        }

        
        [ Fact]
        public void Test1()
        {
            log.Trace("Tests1 started");

            var id = Guid.NewGuid();

            CreateAccount(id);
            LoadEvents(id, 50);

            using (var rm = new AccountRM("AccountRM", id, () => _fixture.GetListener("AccountRM")))
            {
                Assert.True(rm.AccountEvtCount == 51, $"FAILED: rm.AccountEvtCount = {rm.AccountEvtCount}"); // checking that all events were handled during Start() call in rm
            }

            log.Trace("Tests1 completed");
        }

        private void CreateAccount(Guid id)
        {
            var cmd = new DomainMsg.CreateAccount(id, CorrelatedMessage.NewRoot());

            _fixture.MainBus.Send(cmd, "Failed to create a new Account");

            log.Trace($"Account {id} created");
        }

        private void LoadEvents(Guid id, int n)
        {
            for (var i = 0; i < n; i++)
            {
                var cmd = new DomainMsg.CreditAccount(id, 100, CorrelatedMessage.NewRoot());
                if (!_fixture.MainBus.TrySendAsync(cmd))
                    throw new Exception("Failed to credit account");
            }
            log.Trace($"{n} events were loaded to account {id}");
        }
    }
}