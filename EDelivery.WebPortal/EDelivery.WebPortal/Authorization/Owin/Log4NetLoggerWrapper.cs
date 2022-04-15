using System;
using System.Diagnostics;

namespace EDelivery.WebPortal.Authorization
{
    public class Log4NetLoggerWrapper : Microsoft.Owin.Logging.ILogger
    {
        private log4net.Core.ILogger log4netLogger;

        public Log4NetLoggerWrapper(log4net.Core.ILogger log4netLogger)
        {
            this.log4netLogger = log4netLogger;
        }

        public bool WriteCore(
            TraceEventType eventType,
            int eventId,
            object state,
            Exception exception,
            Func<object, Exception, string> formatter)
        {
            var level = GetLogLevel(eventType);

            // According to docs http://katanaproject.codeplex.com/SourceControl/latest#src/Microsoft.Owin/Logging/ILogger.cs
            // "To check IsEnabled call WriteCore with only TraceEventType and check the return value, no event will be written."
            if (state == null)
            {
                return this.log4netLogger.IsEnabledFor(level);
            }
            if (!this.log4netLogger.IsEnabledFor(level))
            {
                return false;
            }

            this.log4netLogger.Log(null, level, formatter(state, exception), exception);
            return true;
        }

        static log4net.Core.Level GetLogLevel(TraceEventType traceEventType)
        {
            switch (traceEventType)
            {
                case TraceEventType.Critical:
                    return log4net.Core.Level.Fatal;
                case TraceEventType.Error:
                    return log4net.Core.Level.Error;
                case TraceEventType.Warning:
                    return log4net.Core.Level.Warn;
                case TraceEventType.Information:
                    return log4net.Core.Level.Info;
                case TraceEventType.Verbose:
                    return log4net.Core.Level.Trace;
                case TraceEventType.Start:
                    return log4net.Core.Level.Debug;
                case TraceEventType.Stop:
                    return log4net.Core.Level.Debug;
                case TraceEventType.Suspend:
                    return log4net.Core.Level.Debug;
                case TraceEventType.Resume:
                    return log4net.Core.Level.Debug;
                case TraceEventType.Transfer:
                    return log4net.Core.Level.Debug;
                default:
                    throw new ArgumentOutOfRangeException(nameof(traceEventType));
            }
        }
    }
}