using Gozcu.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Gozcu
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //routes.MapRoute(
            //    name: "Default",
            //    url: "{controller}/{action}/{id}",
            //    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            //);

            var list = new List<UrlRouting>();
            list.Add(new UrlRouting { ControllerName = "Home", ActionName = "AcikPozisyonlar", RawUrl = "acik-pozisyonlar" });
            list.Add(new UrlRouting { ControllerName = "Home", ActionName = "AlinmamisPozisyonlar", RawUrl = "alinmamis-pozisyonlar" });
            list.Add(new UrlRouting { ControllerName = "Home", ActionName = "BugunKapananlar", RawUrl = "bugun-kapananlar" });
            list.Add(new UrlRouting { ControllerName = "Home", ActionName = "TumKapananlar", RawUrl = "tum-kapananlar" });
            list.Add(new UrlRouting { ControllerName = "Home", ActionName = "StratejiPuanlari", RawUrl = "strateji-puanlari" });
            list.Add(new UrlRouting { ControllerName = "Home", ActionName = "Ayarlar", RawUrl = "ayarlar" });
            list.Add(new UrlRouting { ControllerName = "Home", ActionName = "StratejiParametreleri", RawUrl = "strateji-parametreleri" });

            foreach (var item in list)
            {
                var _name = item.RawUrl.TrimStart('/') + "Routing";
                routes.MapRoute(
                    name: _name,
                    url: item.RawUrl.TrimStart('/'),
                    defaults: new { controller = item.ControllerName, action = item.ActionName }
                );
            }

            routes.MapRoute(
                name: "JSON",
                url: "json/{controller}/{action}"
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
