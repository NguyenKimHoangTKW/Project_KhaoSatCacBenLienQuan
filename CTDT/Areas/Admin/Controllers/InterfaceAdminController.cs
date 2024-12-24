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
            return View();
        }

        public ActionResult chi_tiet_phieu_khao_sat(int id)
        {
            ViewBag.id = id;
            return View();
        }
    }
}