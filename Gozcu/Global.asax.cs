using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Gozcu
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleResolver.Current = new ReplaceQueryStringBundlerResolver(BundleResolver.Current);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_BeginRequest()
        {
            if (!Context.Request.IsSecureConnection && !Context.Request.IsLocal)
            {
                Response.Clear();
                Response.Status = "301 Moved Permanently";
                Response.AddHeader("Location", Context.Request.Url.ToString().Insert(4, "s"));
                Response.End();
                return;
            }

            Response.AddHeader("X-UA-Compatible", "IE=Edge,chrome=1");
            Response.AddHeader("Content-Language", Thread.CurrentThread.CurrentUICulture.Name);
            Response.Headers.Remove("Server");
        }
    }
}
