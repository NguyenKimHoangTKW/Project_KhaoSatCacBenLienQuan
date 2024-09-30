using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CTDT.Helper
{
    public class SessionExpireFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var user = SessionHelper.GetUser();
            var isAuthenticated = SessionHelper.IsUserLoggedIn();

            if (!isAuthenticated)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary(
                        new { controller = "Home", action = "Index" }
                    )
                );
            }
            base.OnActionExecuting(filterContext);
        }
    }
}