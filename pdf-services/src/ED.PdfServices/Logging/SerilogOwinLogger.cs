using Microsoft.Owin.Logging;
using System;
using System.Diagnostics;

namespace ED.PdfServices
{
    public class SerilogOwinLogger : ILogger
    {
        private readonly Serilog.ILogger logger;
        public SerilogOwinLogger(string name)
        {
            this.logger = Serilog.Log.Logger.WithSourceContext<SerilogOwinLogger>();
        }

        public bool WriteCore(TraceEventType eventType, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
        {
            var level = ToLogEventLevel(eventType);

            // According to docs http://katanaproject.codeplex.com/SourceControl/latest#src/Microsoft.Owin/Logging/ILogger.cs
            // "To check IsEnabled call WriteCore with only TraceEventType and check the return value, no event will be written."
            if (state == null)
            {
                return this.logger.IsEnabled(level);
            }
            if (!this.logger.IsEnabled(level))
            {
                return false;
            }
            this.logger.Write(level, exception, formatter(state, exception));
            return true;
        }

        static Serilog.Events.LogEventLevel ToLogEventLevel(TraceEventType traceEventType)
        {
            switch (traceEventType)
            {
                case TraceEventType.Critical:
                    return Serilog.Events.LogEventLevel.Fatal;
                case TraceEventType.Error:
                    return Serilog.Events.LogEventLevel.Error;
                case TraceEventType.Warning:
                    return Serilog.Events.LogEventLevel.Warning;
                case TraceEventType.Information:
                    return Serilog.Events.LogEventLevel.Information;
                case TraceEventType.Verbose:
                    return Serilog.Events.LogEventLevel.Verbose;
                case TraceEventType.Start:
                    return Serilog.Events.LogEventLevel.Debug;
                case TraceEventType.Stop:
                    return Serilog.Events.LogEventLevel.Debug;
                case TraceEventType.Suspend:
                    return Serilog.Events.LogEventLevel.Debug;
                case TraceEventType.Resume:
                    return Serilog.Events.LogEventLevel.Debug;
                case TraceEventType.Transfer:
                    return Serilog.Events.LogEventLevel.Debug;
                default:
                    throw new ArgumentOutOfRangeException("traceEventType");
            }
        }
    }
}
