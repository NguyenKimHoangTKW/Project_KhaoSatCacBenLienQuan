﻿using System.Web.Mvc;

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
                "Admin_TieuDePhieuKhaoSat",
                "admin/tieu-de-cau-hoi-phieu-khao-sat",
                new { controller = "InterfaceAdmin", action = "tieu_de_cau_hoi_pks" }
            );
            context.MapRoute(
                "Admin_XemTruocCauHoiDaTao",
                "xem-truoc-cau-hoi-da-tao/{id}",
                new { controller = "InterfaceAdmin", action = "xem_truoc_cau_hoi_da_tao", id = UrlParameter.Optional }
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
                "Admin_DanhSachKhoaVienTruong",
                "admin/danh-sach-khoa-vien-truong",
                new { controller = "InterfaceAdmin", action = "danh_sach_khoa_vien_truong" }
            );
            context.MapRoute(
                "Admin_DanhSachKhoaDuoiCap",
                "admin/danh-sach-khoa-duoi-cap",
                new { controller = "InterfaceAdmin", action = "danh_sach_khoa_duoi_cap" }
            );
            context.MapRoute(
                "Admin_DanhSachBoMon",
                "admin/danh-sach-bo-mon",
                new { controller = "InterfaceAdmin", action = "danh_sach_bo_mon" }
            );
            context.MapRoute(
                "Admin_DanhSachCTDT",
                "admin/danh-sach-ctdt",
                new { controller = "InterfaceAdmin", action = "danh_sach_ctdt" }
            );
            context.MapRoute(
                "Admin_DanhSachLop",
                "admin/danh-sach-lop",
                new { controller = "InterfaceAdmin", action = "danh_sach_lop" }
            );
            context.MapRoute(
                "Admin_DanhSachNguoiHoc",
                "admin/danh-sach-nguoi-hoc",
                new { controller = "InterfaceAdmin", action = "danh_sach_nguoi_hoc" }
            );
            context.MapRoute(
                "Admin_DanhSachCBVC",
                "admin/danh-sach-can-bo-vien-chuc",
                new { controller = "InterfaceAdmin", action = "danh_sach_cbvc" }
            );
            context.MapRoute(
                "Admin_DanhSachNguoiHocKhaoSat",
                "admin/danh-sach-nguoi-hoc-khao-sat-phieu",
                new { controller = "InterfaceAdmin", action = "danh_sach_nguoi_hoc_khao_sat" }
            );
            context.MapRoute(
                "Admin_DanhSachCanBoVienChucKhaoSat",
                "admin/danh-sach-can-bo-vien-chuc-khao-sat-phieu",
                new { controller = "InterfaceAdmin", action = "danh_sach_can_bo_vien_chuc_khao_sat" }
            );
            context.MapRoute(
                "Admin_DanhSachNguoiHocCoHocPhanKhaoSat",
                "admin/danh-sach-nguoi-hoc-co-hoc-phan-khao-sat-phieu",
                new { controller = "InterfaceAdmin", action = "danh_sach_nguoi_hoc_co_hoc_phan_khao_sat" }
            );
            context.MapRoute(
                "Admin_DanhSachMonHoc",
                "admin/danh-sach-mon-hoc",
                new { controller = "InterfaceAdmin", action = "danh_sach_mon_hoc" }
            );
            context.MapRoute(
                "Admin_default",
                "Admin/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}