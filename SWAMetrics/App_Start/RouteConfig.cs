using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SWAMetrics
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "DashboardRelease",
                url: "{Dashboard}/{action}/{project}/{release}/{application}",
                defaults: new { controller = "Dashboard", action = "GetReleaseData", project = UrlParameter.Optional, release = UrlParameter.Optional, application = UrlParameter.Optional}
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Dashboard", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}