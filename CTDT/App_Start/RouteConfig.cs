using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace CTDT
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapMvcAttributeRoutes();
            routes.MapRoute(
                name: "TrangChu",
                url: "trang-chu",
                defaults: new { controller = "InterfaceClient", action = "Index" },
                namespaces: new string[] { "CTDT.Controllers" }
            );
            routes.MapRoute(
               name: "BoPhieuKhaoSat",
               url: "bo-phieu-khao-sat/{namehdt}",
               defaults: new { controller = "InterfaceClient", action = "PhieuKhaoSat", namehdt = UrlParameter.Optional }
            );
            routes.MapRoute(
               name: "XacThuc",
               url: "xac_thuc/{id}",
               defaults: new { controller = "InterfaceClient", action = "xac_thuc", id = UrlParameter.Optional }
            );
            routes.MapRoute(
               name: "PhieuKhaoSat",
               url: "phieu-khao-sat/{id}",
               defaults: new { controller = "InterfaceClient", action = "Survey", id = UrlParameter.Optional }
            );
            routes.MapRoute(
               name: "XemLaiPhieuKhaoSat",
               url: "phieu-khao-sat/dap-an/{id}/{surveyid}",
               defaults: new { controller = "InterfaceClient", action = "AnswerPKS", id = UrlParameter.Optional, surveyid = UrlParameter.Optional }
            );
            routes.MapRoute(
               name: "BoPhieuDaKhaoSat",
               url: "bo-phieu-da-khao-sat",
               defaults: new { controller = "InterfaceClient", action = "SurveyedForm" }
            );
            routes.MapRoute(
               name: "XacThucMonHoc",
               url: "xac-thuc-mon-hoc",
               defaults: new { controller = "InterfaceClient", action = "Xac_thuc_mon_hoc" }
            );
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "InterfaceClient", action = "Index", id = UrlParameter.Optional }
            );
            
        }
    }
}
