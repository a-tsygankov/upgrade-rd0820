using ReactiveDomain.Logging;
using System;

namespace Model.Domain.Foundation.Logging
{
    /// <summary>
    /// Simple implementation of NLog.ILogger adepter for ReactiveDomain.Logging
    /// </summary>
    public class NLogAdapter : ILogger
    {
        private readonly NLog.ILogger _logger;

        public NLogAdapter(NLog.ILogger logger)
        {
            _logger = logger;
        }

        public void Flush(TimeSpan? maxTimeToWait = null)
        {
            if (maxTimeToWait != null)
                NLog.LogManager.Flush((TimeSpan)maxTimeToWait);
            else
                NLog.LogManager.Flush();
        }

        public void Fatal(string text)
        {
            _logger.Fatal(text);
        }

        public void Error(string text)
        {
            _logger.Error(text);
        }

        public void Info(string text)
        {
            _logger.Info(text);
        }

        public void Debug(string text)
        {
            _logger.Debug(text);
        }

        public void Trace(string text)
        {
            _logger.Trace(text);
        }

        public void Fatal(string format, params object[] args)
        {
            _logger.Fatal(format, args);
        }

        public void Error(string format, params object[] args)
        {
            _logger.Error(format, args);
        }

        public void Info(string format, params object[] args)
        {
            _logger.Info(format, args);
        }

        public void Debug(string format, params object[] args)
        {
            _logger.Debug(format, args);
        }

        public void Trace(string format, params object[] args)
        {
            _logger.Trace(format, args);
        }

        public void FatalException(Exception exc, string text)
        {
            _logger.Fatal(exc, text);
        }

        public void ErrorException(Exception exc, string text)
        {
            _logger.Error(exc, text);
        }

        public void InfoException(Exception exc, string text)
        {
            _logger.Info(exc, text);
        }

        public void DebugException(Exception exc, string text)
        {
            _logger.Debug(exc, text);
        }

        public void TraceException(Exception exc, string text)
        {
            _logger.Trace(exc, text);
        }

        public void FatalException(Exception exc, string format, params object[] args)
        {
            _logger.Fatal(exc, format, args);
        }

        public void ErrorException(Exception exc, string format, params object[] args)
        {
            _logger.Error(exc, format, args);
        }

        public void InfoException(Exception exc, string format, params object[] args)
        {
            _logger.Info(exc, format, args);
        }

        public void DebugException(Exception exc, string format, params object[] args)
        {
            _logger.Debug(exc, format, args);
        }

        public void TraceException(Exception exc, string format, params object[] args)
        {
            _logger.Trace(exc, format, args);
        }

        public LogLevel LogLevel => ConvertLogLevel();

        private LogLevel ConvertLogLevel()
        {
            if (_logger.IsTraceEnabled) return LogLevel.Trace;
            if (_logger.IsDebugEnabled) return LogLevel.Debug;
            if (_logger.IsInfoEnabled || _logger.IsWarnEnabled) return LogLevel.Info;
            if (_logger.IsErrorEnabled) return LogLevel.Error;

            return LogLevel.Fatal;
        }
    }
}
