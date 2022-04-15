using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EDelivery.WebPortal.Errors
{
    public class LogErrorsAttribute : FilterAttribute, IExceptionFilter
    {
        #region IExceptionFilter Members

        void IExceptionFilter.OnException(ExceptionContext filterContext)
        {
            if (filterContext != null && filterContext.Exception != null)
            {
                ElmahLogger.Instance.Error(filterContext.Exception, "Error in " + filterContext.RequestContext?.HttpContext?.Request.RawUrl);
            }

        }

        #endregion
    }
}