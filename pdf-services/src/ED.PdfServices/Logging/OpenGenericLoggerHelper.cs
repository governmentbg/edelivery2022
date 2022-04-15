using Microsoft.Extensions.Logging;
using System;

namespace ED.PdfServices
{
    public class OpenGenericLoggerHelper<T> : ILogger<T>
    {
        private readonly ILogger<T> logger;
        public OpenGenericLoggerHelper(ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger<T>();
        }

        public IDisposable BeginScope<TState>(TState state)
            => this.logger.BeginScope<TState>(state);

        public bool IsEnabled(LogLevel logLevel)
            => this.logger.IsEnabled(logLevel);

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            => this.logger.Log<TState>(logLevel, eventId, state, exception, formatter);
    }
}