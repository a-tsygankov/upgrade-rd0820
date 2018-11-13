using ReactiveDomain.Logging;
//using System;
//using System.Reactive.Disposables;
//using System.Reactive.Linq;

namespace Model.Domain.Foundation.Logging
{
    /// <summary>
    /// Extension methods for ReactiveDomain.Logging.Ilogger
    /// </summary>
    public static class LoggerExtensions
    {
        public static bool IsTraceEnabled(this ILogger logger)
        {
            return logger.LogLevel >= LogLevel.Trace;
        }

        public static bool IsDebugEnabled(this ILogger logger)
        {
            return logger.LogLevel >= LogLevel.Debug;
        }

        public static bool IsInfoEnabled(this ILogger logger)
        {
            return logger.LogLevel >= LogLevel.Info;
        }

        public static bool IsErrorEnabled(this ILogger logger)
        {
            return logger.LogLevel >= LogLevel.Error;
        }

        public static bool IsFatalEnabled(this ILogger logger)
        {
            return logger.LogLevel >= LogLevel.Fatal;
        }

        /// <summary>
        /// Logging IObservable  (Trace level only)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="logger"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        //public static IObservable<T> Log<T>(this IObservable<T> source, ILogger logger, string name)
        //{
        //    if (!logger.IsTraceEnabled())
        //        return source;

        //    return Observable.Create<T>(
        //        o =>
        //        {
        //            logger.Trace("{0}.Subscribe()", name);
        //            var disposal = Disposable.Create(() => logger.Trace("{0}.Dispose()", name));
        //            var subscription = source
        //                .Do(
        //                    i => logger.Trace("{0}.OnNext({1})", name, i),
        //                    ex => logger.Trace("{0}.OnError({1})", name, ex),
        //                    () => logger.Trace("{0}.OnCompleted()", name)
        //                )
        //                .Subscribe(o);
        //            return new CompositeDisposable(disposal, subscription);
        //        });
        //}
    }
}
