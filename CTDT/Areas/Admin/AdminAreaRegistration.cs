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
                "Admin_GiamSatTyLeThamGiaKhaoSat",
                "admin/giam-sat-ty-le-tham-gia-khao-sat",
                new { controller = "InterfaceAdmin", action = "giam_sat_ty_le_tham_gia_khao_sat" }
            );
            context.MapRoute(
                "Admin_BaoCaoTongHopKetQuaKhaoSat",
                "admin/bao-cao-tong-hop-ket-qua-khao-sat",
                new { controller = "InterfaceAdmin", action = "bao_cao_tong_hop_ket_qua_khao_sat" }
            );
            context.MapRoute(
                "Admin_DanhSachPhieuKhaoSat",
                "admin/danh-sach-phieu-khao-sat",
                new { controller = "InterfaceAdmin", action = "danh_sach_phieu_khao_sat" }
            );
            context.MapRoute(
                "Admin_ChiTietPhieuKhaoSat",
                "admin/chi-tiet-phieu-khao-sat/{id}",
                new { controller = "InterfaceAdmin", action = "chi_tiet_phieu_khao_sat", id = UrlParameter.Optional }
            );
            context.MapRoute(
                "Admin_default",
                "Admin/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}