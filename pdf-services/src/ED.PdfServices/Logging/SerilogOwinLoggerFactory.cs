using Microsoft.Owin.Logging;

namespace ED.PdfServices
{
    public class SerilogOwinLoggerFactory : ILoggerFactory
    {
        public ILogger Create(string name)
        {
            return new SerilogOwinLogger(name);
        }
    }
}
