using Model.Domain.Foundation.Logging;
using ReactiveDomain;
using ReactiveDomain.Foundation;
using ReactiveDomain.Messaging.Bus;
using System;
using System.Net;
using System.Reflection;
using Domain;
using EventStore.ClientAPI;
using ReactiveDomain.EventStore;
using ILogger = ReactiveDomain.Logging.ILogger;

namespace Tests
{
    public class AccountFixture : IDisposable
    {
        private static readonly ILogger log = LogFactoryAdapter.GetLogger(typeof(AccountFixture));
        public IStreamStoreConnection Connection { get; set; }
        public IDispatcher MainBus;
        public IRepository Repo;
        public Func<string, IListener> GetListener => _getListener;
        public IStreamNameBuilder StreamNameBuilder => _streamNameBuilder;

        private Func<string, IListener> _getListener;
        private IStreamNameBuilder _streamNameBuilder;
        private IEventStoreConnection _connection;

        static AccountFixture()
        {
            Domain.Bootstrap.Load();
            ReactiveDomain.Foundation.BootStrap.Load();
        }

        public AccountFixture()
        {
            MainBus = new Dispatcher("Test bus");

            ConnectToLiveES();

            _streamNameBuilder = new PrefixedCamelCaseStreamNameBuilder("upgrade");

            _getListener = name => new Model.Foundation.StreamListener(
                listenerName: name,
                eventStoreConnection: Connection,
                streamNameBuilder: _streamNameBuilder,
                serializer: new JsonMessageSerializer());

            // Build event store repository
            Repo = new StreamStoreRepository(
                streamNameBuilder: _streamNameBuilder,
                eventStoreConnection: Connection,
                eventSerializer: new JsonMessageSerializer());

            var _ = new AccountSvc(MainBus, Repo);
        }

        private void ConnectToLiveES()
        {
            var userCredentials = new ReactiveDomain.UserCredentials(username: "admin", password: "changeit");

            var eventStoreLoader = new EventStoreLoader();
            eventStoreLoader.Connect(
                credentials: userCredentials,
                server: IPAddress.Parse("127.0.0.1"),
                tcpPort: 1113);

            Connection = eventStoreLoader.Connection;

            // ReSharper disable once PossibleNullReferenceException
            _connection = (IEventStoreConnection)typeof(EventStoreConnectionWrapper)
                .GetField("_conn", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(eventStoreLoader.Connection);

            log.Info("Connected to ES");
            
        }

        public bool TryGetById<TAggregate>(Guid id, out TAggregate aggregate) where TAggregate : class, IEventSource
        {
            return Repo.TryGetById<TAggregate>(id, out aggregate);
        }

        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
                return;

            Connection?.Close();
            Connection?.Dispose();

            log.Info("Disposed");
            _disposed = true;
        }
    }
}