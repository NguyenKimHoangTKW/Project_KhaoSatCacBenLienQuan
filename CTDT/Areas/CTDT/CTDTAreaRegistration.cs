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
                new { controller = "GiamSatKetQuaKhaoSat", action = "Index" }
            );
            context.MapRoute(
                "CTDT_GiamSatTyLeThamGiaKhaoSat",
                "ctdt/giam-sat-ty-le-tham-gia-khao-sat",
                new { controller = "ThongKeKhaoSat", action = "Index" }
            );
            context.MapRoute(
                "CTDT_ThongKeKetQuaKhaoSatTheoYeuCau",
                "ctdt/ket-qua-khao-sat-theo-yeu-cau",
                new { controller = "ThongKeKetQuaKhaoSatTheoYeuCau", action = "Index" }
            );
            context.MapRoute(
                "CTDT_KetQuaKhaoSat",
                "ctdt/ket-qua-khao-sat",
                new { controller = "ThongKeKhaoSat", action = "thongketanxuat" }
            );
            context.MapRoute(
                "CTDT_BaoCaoTongHop",
                "ctdt/bao-cao-tong-hop-ket-qua-khao-sat",
                new { controller = "BaoCaoTongHopKetQuaKhaoSat", action = "index" }
            );
            context.MapRoute(
                "CTDT_default",
                "CTDT/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
            
        }
    }
}