using ReactiveDomain.Logging;
using System;

namespace Model.Domain.Foundation.Logging
{
    /// <summary>
    /// Temporary replacement for ReactiveDomain.Logging.LogManager since it wraps all the loggers created into LazyLogger
    /// which is locked on ERROR level.
    ///
    /// Todo: switch to LogManager once the problem is fixed
    /// </summary>
    public static class LogFactoryAdapter
    {
        public static readonly Func<string, ILogger> LogFactory = name => new NLogAdapter(NLog.LogManager.GetLogger(name));

        public static ILogger GetLogger(string name) => LogFactory(name);
        public static ILogger GetLogger(Type type) => LogFactory(type.FullName);

        static LogFactoryAdapter()
        {
            LogManager.SetLogFactory(LogFactory);
        }

        public static void Init(ILogger globalLogger)
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var exc = e.ExceptionObject as Exception;
                if (exc != null)
                {
                    globalLogger.FatalException(exc, "Global Unhandled Exception occurred.");
                }
                else
                    globalLogger.Fatal("Global Unhandled Exception object: {0}.", e.ExceptionObject);

                globalLogger.Flush(TimeSpan.FromMilliseconds(500));
            };
        }
    }
}
