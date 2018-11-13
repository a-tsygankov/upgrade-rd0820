using System;
using System.Threading;
using Model.Domain.Foundation.Logging;
using ReactiveDomain;
using ReactiveDomain.Foundation;
using ReactiveDomain.Logging;
using ReactiveDomain.Messaging;
using ReactiveDomain.Messaging.Bus;
using ReactiveDomain.Util;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable once CheckNamespace

namespace Model.Foundation
{
    /// <summary>
    /// Copied from ReactiveDomain 0.8.18 in order to add ConnectionDropped message and set default maxLiveQueueSize to 10,000
    /// </summary>
    public class StreamListener : IListener
    {
        private static readonly ILogger _log = LogFactoryAdapter.GetLogger(typeof(StreamListener));

        protected readonly string ListenerName;
        private readonly InMemoryBus _bus;
        IDisposable _subscription;
        private bool _started;
        private readonly IStreamNameBuilder _streamNameBuilder;
        private readonly IEventSerializer _serializer;
        private readonly object _startlock = new object();
        private readonly ManualResetEventSlim _liveLock = new ManualResetEventSlim();
        public ISubscriber EventStream => _bus;
        private readonly IStreamStoreConnection _eventStoreConnection;
        public long Position => _position;
        public string StreamName { get; private set; }
        private long _position;
        private long _msgCount;
        private int _resubscribeTimeout = 250;

        /// <summary>
        /// For listening to generic streams
        /// </summary>
        /// <param name="listenerName"></param>
        /// <param name="eventStoreConnection">The event store to subscribe to</param>
        /// <param name="streamNameBuilder">The source for correct stream names based on aggregates and events</param>
        /// <param name="serializer"></param>
        /// <param name="busName">The name to use for the internal bus (helpful in debugging)</param>
        public StreamListener(
            string listenerName,
            IStreamStoreConnection eventStoreConnection,
            IStreamNameBuilder streamNameBuilder,
            IEventSerializer serializer,
            string busName = null)
        {
            _bus = new InMemoryBus(busName ?? "Stream Listener");
            _eventStoreConnection =
                eventStoreConnection ?? throw new ArgumentNullException(nameof(eventStoreConnection));

            ListenerName = listenerName;
            _streamNameBuilder = streamNameBuilder;
            _serializer = serializer;
        }

        /// <summary>
        /// Event Stream Listener
        /// i.e. $et-[MessageType]
        /// </summary>
        /// <param name="tMessage"></param>
        /// <param name="checkpoint"></param>
        /// <param name="blockUntilLive"></param>
        /// <param name="millisecondsTimeout"></param>
        public void Start(
            Type tMessage,
            long? checkpoint = null,
            bool blockUntilLive = false,
            int millisecondsTimeout = 1000)
        {
            if (!tMessage.IsSubclassOf(typeof(Event)))
            {
                throw new ArgumentException("type must derive from ReactiveDomain.Messaging.Event", nameof(tMessage));
            }

            Start(
                _streamNameBuilder.GenerateForEventType(tMessage.Name),
                checkpoint,
                blockUntilLive,
                millisecondsTimeout);
        }

        /// <summary>
        /// Category Stream Listener
        /// i.e. $ce-[AggregateType]
        /// </summary>
        /// <typeparam name="TAggregate">The Aggregate type used to generate the stream name</typeparam>
        /// <param name="checkpoint"></param>
        /// <param name="blockUntilLive"></param>
        /// <param name="timeout">timeout in milliseconds default = 1000</param>
        public void Start<TAggregate>(
            long? checkpoint = null,
            bool blockUntilLive = false,
            int timeout = 1000) where TAggregate : class, IEventSource
        {
            Start(
                _streamNameBuilder.GenerateForCategory(typeof(TAggregate)),
                checkpoint,
                blockUntilLive,
                timeout);
        }

        /// <summary>
        /// Aggregate Stream listener
        /// i.e. [AggregateType]-[id]
        /// </summary>
        /// <typeparam name="TAggregate">The Aggregate type used to generate the stream name</typeparam>
        /// <param name="id"></param>
        /// <param name="checkpoint"></param>
        /// <param name="blockUntilLive"></param>
        /// <param name="timeout">timeout in milliseconds default = 1000</param>
        public void Start<TAggregate>(
            Guid id,
            long? checkpoint = null,
            bool blockUntilLive = false,
            int timeout = 1000) where TAggregate : class, IEventSource
        {
            Start(
                _streamNameBuilder.GenerateForAggregate(typeof(TAggregate), id),
                checkpoint,
                blockUntilLive,
                timeout);
        }

        /// <summary>
        /// Custom Stream name
        /// i.e. [StreamName]
        /// </summary>
        /// <param name="streamName"></param>
        /// <param name="checkpoint"></param>
        /// <param name="blockUntilLive"></param>
        /// <param name="timeout">timeout in milliseconds default = 1000</param>
        public virtual void Start(
            string streamName,
            long? checkpoint = null,
            bool blockUntilLive = false,
            int timeout = 1000)
        {
            _log.Trace($"Listener {ListenerName} started");
            _liveLock.Reset();
            lock (_startlock)
            {
                if (_started)
                    throw new InvalidOperationException("Listener already started.");
                if (!ValidateStreamName(streamName))
                    throw new ArgumentException("Stream not found.", streamName);
                StreamName = streamName;
                _subscription =
                    SubscribeToStreamFrom(
                        streamName,
                        checkpoint,
                        true,
                        eventAppeared: GotEvent,
                        liveProcessingStarted: () =>
                        {
                            _bus.Publish(new StreamStoreMsgs.CatchupSubscriptionBecameLive());
                            _liveLock.Set();
                        },
                        subscriptionDropped: (reason, exception) => SubscriptionDropped(reason, exception));
                _started = true;
            }

            if (blockUntilLive)
            {
                _log.Trace("Still blocked");
                _liveLock.Wait(timeout);
            }
        }

        private void SubscriptionDropped(SubscriptionDropReason reason, Exception exception)
        {
            _log.Trace($"Listener {ListenerName} subscription dropped, reason: {reason}, ex: {exception}");
            _bus.Publish(new Domain.Foundation.StreamStoreMsgs.SubscriptionDropped(ListenerName, reason, exception));
            _started = false;

            // resubscribe
            Thread.Sleep(_resubscribeTimeout);

            _subscription?.Dispose();

            if (reason == SubscriptionDropReason.UserInitiated)
            {
                // do not reconnect
                return;
            }

            _log.Trace($"Listener {ListenerName} subscription restarting");
            _subscription =
                SubscribeToStreamFrom(
                    StreamName,
                    Position,
                    true,
                    eventAppeared: GotEvent,
                    liveProcessingStarted: () =>
                    {
                        _bus.Publish(new StreamStoreMsgs.CatchupSubscriptionBecameLive());
                        _liveLock.Set();
                    },
                    subscriptionDropped: (r, e) => SubscriptionDropped(reason, exception));
            _started = true;
            if (_resubscribeTimeout < 60000)
                _resubscribeTimeout *= 2;
        }

        public IDisposable SubscribeToStreamFrom(
            string stream,
            long? lastCheckpoint,
            bool resolveLinkTos,
            Action<Message> eventAppeared,
            Action liveProcessingStarted = null,
            Action<SubscriptionDropReason, Exception> subscriptionDropped = null,
            UserCredentials userCredentials = null,
            int readBatchSize = 500,
            int maxLiveQueueSize = 1000)
        {
            var settings = new CatchUpSubscriptionSettings(maxLiveQueueSize, readBatchSize, false);
            StreamName = stream;
            var sub = _eventStoreConnection.SubscribeToStreamFrom(
                stream,
                lastCheckpoint,
                settings,
                resolvedEvent =>
                {
                    Interlocked.Exchange(ref _position, resolvedEvent.EventNumber);
                    eventAppeared(_serializer.Deserialize(resolvedEvent) as Message);
                },
                _ => liveProcessingStarted?.Invoke(),
                (reason, exception) => subscriptionDropped?.Invoke(reason, exception),
                userCredentials);

            return new Disposer(
                () =>
                {
                    sub.Dispose();
                    return Unit.Default;
                });
        }

        public bool ValidateStreamName(string streamName)
        {
            var isValid = _eventStoreConnection.ReadStreamForward(streamName, 0, 1) != null;
            return isValid;
        }

        protected virtual void GotEvent(Message @event)
        {
            Interlocked.Increment(ref _msgCount);
            if (@event != null) _bus.Publish(@event);
            _log.Trace($"got event #{_msgCount}");
        }

        #region Implementation of IDisposable

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            _subscription?.Dispose();
            _bus?.Dispose();
            _disposed = true;
        }

        #endregion
    }
}
