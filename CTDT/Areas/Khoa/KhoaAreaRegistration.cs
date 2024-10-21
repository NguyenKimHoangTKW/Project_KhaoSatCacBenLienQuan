using System.Web.Mvc;

namespace CTDT.Areas.Khoa
{
    public class KhoaAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Khoa";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Khoa_GiamSatKetQuaKhaoSat",
                "khoa/giam-sat-ket-qua-khao-sat",
                new { controller = "InterfaceKhoa", action = "giam_sat_ket_qua_khao_sat" }
            );
            context.MapRoute(
                "Khoa_GiamSatTyLeThamGiaKhaoSat",
                "khoa/giam-sat-ty-le-tham-gia-khao-sat",
                new { controller = "InterfaceKhoa", action = "thong_ke_khao_sat" }
            );
            context.MapRoute(
                "Khoa_KetQuaKhaoSat",
                "khoa/ket-qua-khao-sat",
                new { controller = "InterfaceKhoa", action = "thong_ke_tan_xuat" }
            );
            context.MapRoute(
                "Khoa_BaoCaoTongHop",
                "khoa/bao-cao-tong-hop-ket-qua-khao-sat",
                new { controller = "InterfaceKhoa", action = "bao_cao_tong_hop_ket_qua_khao_sat" }
            );
            context.MapRoute(
                "Khoa_default",
                "Khoa/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );           
        }
    }
}