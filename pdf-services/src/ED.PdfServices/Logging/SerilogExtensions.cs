using Serilog;
using Serilog.Core;

namespace ED.PdfServices
{
    public static class SerilogExtensions
    {
        public static ILogger SetupSerilog()
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.AppSettings()
                .CreateLogger();

            return Log.Logger;
        }

        public static ILogger WithSourceContext(this ILogger logger, string name)
        {
            return logger.ForContext(Constants.SourceContextPropertyName, name);
        }

        public static ILogger WithSourceContext<T>(this ILogger logger)
        {
            return WithSourceContext(logger, typeof(T).FullName);
        }
    }
}