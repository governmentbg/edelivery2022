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

        // TODO: ?
        public void Info(string message)
        {
            this.Error(new InfoMessage(message));
        }

        // TODO: ?
        public void Info(string message, params object[] messageParams)
        {
            if (!string.IsNullOrEmpty(message) && messageParams != null && messageParams.Length > 0)
            {
                message = string.Format(message, messageParams);
                this.Error(new InfoMessage(message));
            }
        }

        public void Error(string message)
        {
            this.Error(new Exception(message));
        }

        // TODO: ?
        public void Error(string message, params object[] messageParams)
        {
            if (!string.IsNullOrEmpty(message) && messageParams != null && messageParams.Length > 0)
            {
                message = string.Format(message, messageParams);
                this.Error(new Exception(message));
            }
        }

        // TODO: investigate
        public void Error(Exception ex)
        {
            Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
        }

        public void Error(Exception ex, string message)
        {
            this.Error(ex, message, null);
        }

        public void Error(Exception ex, string message, params object[] messageParams)
        {
            var context = HttpContext.Current;

            if (context != null)
            {
                if (ex is RpcException rpcException
                    && rpcException.StatusCode == StatusCode.Cancelled)
                {
                    return;
                }

                if (!string.IsNullOrEmpty(message)
                    && messageParams != null && messageParams.Length > 0)
                {
                    message = String.Format(message, messageParams);
                }

                try
                {
                    var elmahEh = new Elmah.Error(ex, context);
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
