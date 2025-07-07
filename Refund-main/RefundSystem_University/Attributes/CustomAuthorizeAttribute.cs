using RefundSystem_University.Models;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace RefundSystem_University.Attributes
{
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext.Session["User"] is User && (string.IsNullOrEmpty(Roles) || (httpContext.Session["User"] as User).IsAdmin))
                return true;
            return false;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                base.HandleUnauthorizedRequest(filterContext);
                filterContext.Result = new RedirectToRouteResult(new
                RouteValueDictionary(new { controller = "Login", action = "Login", ReturnUrl = filterContext.HttpContext.Request.RawUrl }));
            }
            else
            {
                filterContext.Result = new RedirectToRouteResult(new
                RouteValueDictionary(new { controller = "Login", action = "Login", ReturnUrl = filterContext.HttpContext.Request.RawUrl }));
            }
        }
    }
}