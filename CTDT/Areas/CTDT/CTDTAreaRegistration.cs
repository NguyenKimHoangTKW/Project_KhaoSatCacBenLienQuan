﻿using System.Web.Mvc;
using static GoogleApi.GoogleMaps;

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
                "CTDT_default",
                "CTDT/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}