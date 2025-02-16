using System.Web.Mvc;

namespace CTDT.Areas.CTDT
{
    public class CTDTAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "CTDT";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "CTDT_GiamSatKetQuaKhaoSat",
                "ctdt/giam-sat-ket-qua-khao-sat",
                new { controller = "Interface", action = "giam_sat_ket_qua_khao_sat" }
            );
            context.MapRoute(
                "CTDT_GiamSatTyLeThamGiaKhaoSat",
                "ctdt/giam-sat-ty-le-tham-gia-khao-sat",
                new { controller = "Interface", action = "thong_ke_khao_sat" }
            );
            context.MapRoute(
                "CTDT_ThongKeKetQuaKhaoSatTheoYeuCau",
                "ctdt/ket-qua-khao-sat-theo-yeu-cau",
                new { controller = "Interface", action = "thong_ke_tan_xuat_theo_yeu_cau" }
            );
            context.MapRoute(
                "CTDT_KetQuaKhaoSat",
                "ctdt/ket-qua-khao-sat",
                new { controller = "Interface", action = "thong_ke_tan_xuat" }
            );
            context.MapRoute(
                "CTDT_BaoCaoTongHop",
                "ctdt/bao-cao-tong-hop-ket-qua-khao-sat",
                new { controller = "Interface", action = "bao_cao_tong_hop_ket_qua_khao_sat" }
            );
            context.MapRoute(
                "CTDT_XemChiTietThongKe",
                "ctdt/xem-chi-tiet-thong-ke-khao-sat/{id}",
                new { controller = "Interface", action = "xem_thong_ke_nguoi_khao_sat", id = UrlParameter.Optional }
            );
            context.MapRoute(
                "CTDT_ChonDapVienThongKeTheoYeuCau",
                "ctdt/chon-dap-vien-thong-ke-theo-yeu-cau",
                new { controller = "Interface", action = "chon_dap_vien_thong_ke_theo_yeu_cau", id = UrlParameter.Optional }
            );
            context.MapRoute(
                "CTDT_default",
                "CTDT/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}