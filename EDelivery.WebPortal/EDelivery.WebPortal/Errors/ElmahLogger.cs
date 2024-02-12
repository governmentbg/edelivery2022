using System;
using System.Web;

using Grpc.Core;

namespace EDelivery.WebPortal
{
    /// <summary>
    /// Logging errors
    /// </summary>
    public sealed class ElmahLogger
    {
        private static object _lockObj = new object();
        private static ElmahLogger instance;

        static ElmahLogger()
        {
            lock (_lockObj)
            {
                instance = new ElmahLogger();
            };
        }

        public static ElmahLogger Instance
        {
            get
            {
                return instance;
            }
        }

        public void Error(string message)
        {
            this.Error(new Exception(), message);
        }

        public void Error(Exception ex, string message)
        {
            this.ErrorInternal(ex, message);
        }

        private void ErrorInternal(Exception ex, string message)
        {
            HttpContext context = HttpContext.Current;

            if (context != null)
            {
                if (ex is RpcException rpcException
                    && rpcException.StatusCode == StatusCode.Cancelled)
                {
                    return;
                }

                try
                {
                    Elmah.Error elmahEh = new Elmah.Error(ex, context);
                    elmahEh.ServerVariables.Add("APP_MESSAGE", message);
                    Elmah.ErrorLog.GetDefault(context).Log(elmahEh);
                }
                catch
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                }
            }
        }
    }
}
