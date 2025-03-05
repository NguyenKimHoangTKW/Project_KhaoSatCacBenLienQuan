using CTDT.Helper;
using CTDT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CTDT.Areas.Admin.Controllers
{
    [UserAuthorize(2)]
    public class InterfaceAdminController : Controller
    {
        dbSurveyEntities db = new dbSurveyEntities();
        // GET: Admin/InterfaceAdmin
        public ActionResult UserInterface()
        {
            ViewBag.CTDTList = new SelectList(db.ctdt.OrderBy(l => l.id_ctdt), "id_ctdt", "ten_ctdt");
            ViewBag.DonVilist = new SelectList(db.DonVi.OrderBy(l => l.id_donvi), "id_donvi", "name_donvi");
            ViewBag.TypeUserList = new SelectList(db.typeusers.OrderBy(l => l.id_typeusers), "id_typeusers", "name_typeusers");
            ViewBag.KhoaList = new SelectList(db.khoa.OrderBy(x => x.id_khoa), "id_khoa", "ten_khoa");
            return View();
        }
        public ActionResult PhanQuyen(int id)
        {
            var PhanQuyen = db.users.Where(x => x.id_users == id).FirstOrDefault();
            return View(PhanQuyen);
        }
        
        public ActionResult Thong_Ke_Ket_Qua_Khao_Sat()
        {
            ViewBag.Year = new SelectList(db.NamHoc.OrderBy(l => l.id_namhoc), "id_namhoc", "ten_namhoc");
            ViewBag.HocKy = new SelectList(db.hoc_ky.OrderBy(l => l.id_hk), "id_hk", "ten_hk");
            ViewBag.HeDaoTao = new SelectList(db.hedaotao.OrderBy(l => l.ten_hedaotao), "id_hedaotao", "ten_hedaotao");
            ViewBag.DotKhaoSat = new SelectList(db.dot_khao_sat.OrderBy(l => l.id_dot_khao_sat), "id_dot_khao_sat", "ten_dot_khao_sat");
            return View();
        }

        public ActionResult giam_sat_ty_le_tham_gia_khao_sat()
        {
            ViewBag.Year = new SelectList(db.NamHoc.OrderBy(l => l.id_namhoc), "id_namhoc", "ten_namhoc");
            ViewBag.HeDaoTao = new SelectList(db.hedaotao.OrderBy(l => l.ten_hedaotao), "id_hedaotao", "ten_hedaotao");
            return View();
        }

        public ActionResult bao_cao_tong_hop_ket_qua_khao_sat()
        {
            ViewBag.Year = new SelectList(db.NamHoc.OrderBy(l => l.id_namhoc), "id_namhoc", "ten_namhoc");
            ViewBag.HeDaoTao = new SelectList(db.hedaotao.OrderBy(l => l.ten_hedaotao), "id_hedaotao", "ten_hedaotao");
            return View();
        }

        public ActionResult danh_sach_phieu_khao_sat()
        {
            ViewBag.HDT = new SelectList(db.hedaotao, "id_hedaotao", "ten_hedaotao");
            ViewBag.LKS = new SelectList(db.LoaiKhaoSat, "id_loaikhaosat", "name_loaikhaosat");
            ViewBag.Year = new SelectList(db.NamHoc, "id_namhoc", "ten_namhoc");
            ViewBag.DotKhaoSat = new SelectList(db.dot_khao_sat, "id_dot_khao_sat", "ten_dot_khao_sat");
            return View();
        }

        public ActionResult chi_tiet_phieu_khao_sat(int id)
        {
            ViewBag.id = id;
            ViewBag.HDT = new SelectList(db.hedaotao, "id_hedaotao", "ten_hedaotao");
            ViewBag.LKS = new SelectList(db.LoaiKhaoSat, "id_loaikhaosat", "name_loaikhaosat");
            ViewBag.Year = new SelectList(db.NamHoc, "id_namhoc", "ten_namhoc");
            ViewBag.DotKhaoSat = new SelectList(db.dot_khao_sat, "id_dot_khao_sat", "ten_dot_khao_sat");
            return View();
        }


        public ActionResult tieu_de_cau_hoi_pks()
        {
            ViewBag.Year = new SelectList(db.NamHoc.OrderBy(l => l.id_namhoc), "id_namhoc", "ten_namhoc");
            ViewBag.HeDaoTao = new SelectList(db.hedaotao.OrderBy(l => l.ten_hedaotao), "id_hedaotao", "ten_hedaotao");
            return View();
        }
        public ActionResult xem_truoc_cau_hoi_da_tao(int id)
        {
            ViewBag.id = id;
            return View();
        }


        public ActionResult danh_sach_khoa()
        {
            ViewBag.CTDTList = new SelectList(db.ctdt.OrderBy(l => l.id_ctdt), "id_ctdt", "ten_ctdt");
            return View();
        }
        public ActionResult danh_sach_ctdt()
        {
            ViewBag.KhoaList = new SelectList(db.khoa.OrderBy(l => l.id_khoa), "id_khoa", "ten_khoa");
            ViewBag.HDT = new SelectList(db.hedaotao.OrderBy(l => l.id_hedaotao), "id_hedaotao", "ten_hedaotao");
            ViewBag.CTDTList = new SelectList(db.ctdt.OrderBy(l => l.id_ctdt), "id_ctdt", "ten_ctdt");
            return View();
        }
        public ActionResult danh_sach_lop()
        {
            ViewBag.CTDTList = new SelectList(db.ctdt.OrderBy(l => l.id_ctdt), "id_ctdt", "ten_ctdt");
            return View();
        }
        public ActionResult danh_sach_nguoi_hoc()
        {
            ViewBag.LopList = new SelectList(db.lop.OrderBy(x => x.id_lop), "id_lop", "ma_lop");
            return View();
        }
        public ActionResult danh_sach_cbvc()
        {
            ViewBag.DonviList = new SelectList(db.DonVi.OrderBy(x => x.id_donvi), "id_donvi", "name_donvi");
            ViewBag.ChucvuList = new SelectList(db.ChucVu.OrderBy(x => x.id_chucvu), "id_chucvu", "name_chucvu");
            ViewBag.CtdtList = new SelectList(db.ctdt.OrderBy(x => x.id_ctdt), "id_ctdt", "ten_ctdt");
            ViewBag.Year = new SelectList(db.NamHoc.OrderBy(x => x.id_namhoc), "id_namhoc", "ten_namhoc");
            return View();
        }

        public ActionResult danh_sach_nguoi_hoc_khao_sat()
        {
            ViewBag.Year = new SelectList(db.NamHoc.OrderBy(l => l.id_namhoc), "id_namhoc", "ten_namhoc");
            ViewBag.HeDaoTao = new SelectList(db.hedaotao.OrderBy(l => l.id_hedaotao), "id_hedaotao", "ten_hedaotao");
            return View();
        }
        public ActionResult danh_sach_can_bo_vien_chuc_khao_sat()
        {
            ViewBag.Year = new SelectList(db.NamHoc.OrderBy(l => l.id_namhoc), "id_namhoc", "ten_namhoc");
            ViewBag.HeDaoTao = new SelectList(db.hedaotao.OrderBy(l => l.id_hedaotao), "id_hedaotao", "ten_hedaotao");
            return View();
        }
        public ActionResult danh_sach_nguoi_hoc_co_hoc_phan_khao_sat()
        {
            return View();
        }
        public ActionResult danh_sach_mon_hoc()
        {
            ViewBag.HocPhan = new SelectList(db.hoc_phan.OrderBy(l => l.id_hoc_phan), "id_hoc_phan", "ten_hoc_phan");
            ViewBag.Year = new SelectList(db.NamHoc.OrderByDescending(l => l.id_namhoc), "id_namhoc", "ten_namhoc");
            ViewBag.Lop = new SelectList(db.lop.OrderBy(l => l.id_lop), "id_lop", "ma_lop");
            return View();
        }
    }
}