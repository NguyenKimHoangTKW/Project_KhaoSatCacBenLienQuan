using System.Web.Mvc;

namespace CTDT.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Admin";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Admin_ThongKeKetQuaKhaoSat",
                "admin/thong-ke-ket-qua-khao-sat",
                new { controller = "InterfaceAdmin", action = "Thong_Ke_Ket_Qua_Khao_Sat" }
            );
            context.MapRoute(
                "Admin_default",
                "Admin/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}