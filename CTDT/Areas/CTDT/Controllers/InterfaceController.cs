using CTDT.Helper;
using CTDT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CTDT.Areas.CTDT.Controllers
{
    [UserAuthorize(3)]
    public class InterfaceController : Controller
    {
        dbSurveyEntities db = new dbSurveyEntities();
        private users user;
        public InterfaceController()
        {
            user = SessionHelper.GetUser();
        }
        // GET: CTDT/Interface
        public ActionResult bao_cao_tong_hop_ket_qua_khao_sat()
        {
            var get_ctdt = db.phan_quyen_users.Where(x => x.id_users == user.id_users).OrderBy(x => x.id_phan_quyen_user).ToList();
            ViewBag.Year = new SelectList(db.NamHoc.OrderByDescending(x => x.id_namhoc), "id_namhoc", "ten_namhoc");
            ViewBag.CTDT = new SelectList(get_ctdt, "ctdt.id_ctdt", "ctdt.ten_ctdt");
            return View();
        }
        public ActionResult giam_sat_ket_qua_khao_sat()
        {
            var get_ctdt = db.phan_quyen_users.Where(x => x.id_users == user.id_users).OrderBy(x => x.id_phan_quyen_user).ToList();
            ViewBag.Year = new SelectList(db.NamHoc.OrderByDescending(x => x.id_namhoc), "id_namhoc", "ten_namhoc");
            ViewBag.CTDT = new SelectList(get_ctdt, "ctdt.id_ctdt", "ctdt.ten_ctdt");
            return View();
        }
        public ActionResult thong_ke_khao_sat()
        {
            var get_ctdt = db.phan_quyen_users.Where(x => x.id_users == user.id_users).OrderBy(x => x.id_phan_quyen_user).ToList();
            ViewBag.Year = new SelectList(db.NamHoc.OrderByDescending(x => x.id_namhoc), "id_namhoc", "ten_namhoc");
            ViewBag.CTDT = new SelectList(get_ctdt, "ctdt.id_ctdt", "ctdt.ten_ctdt");
            return View();
        }
        public ActionResult thong_ke_tan_xuat()
        {
            var get_ctdt = db.phan_quyen_users.Where(x => x.id_users == user.id_users).OrderBy(x => x.id_phan_quyen_user).ToList();
            ViewBag.Year = new SelectList(db.NamHoc.OrderByDescending(x => x.id_namhoc), "id_namhoc", "ten_namhoc");
            ViewBag.CTDT = new SelectList(get_ctdt, "ctdt.id_ctdt", "ctdt.ten_ctdt");
            return View();
        }
        public ActionResult thong_ke_tan_xuat_theo_yeu_cau()
        {
            var get_ctdt = db.phan_quyen_users.Where(x => x.id_users == user.id_users).OrderBy(x => x.id_phan_quyen_user).ToList();
            ViewBag.Year = new SelectList(db.NamHoc.OrderByDescending(x => x.id_namhoc), "id_namhoc", "ten_namhoc");
            ViewBag.CTDT = new SelectList(get_ctdt, "ctdt.id_ctdt", "ctdt.ten_ctdt");
            return View();
        }
        public ActionResult chon_dap_vien_thong_ke_theo_yeu_cau()
        {
            var get_ctdt = db.phan_quyen_users.Where(x => x.id_users == user.id_users).OrderBy(x => x.id_phan_quyen_user).ToList();
            ViewBag.Year = new SelectList(db.NamHoc.OrderByDescending(x => x.id_namhoc), "id_namhoc", "ten_namhoc");
            ViewBag.CTDT = new SelectList(get_ctdt, "ctdt.id_ctdt", "ctdt.ten_ctdt");
            return View();
        }

        public ActionResult xem_thong_ke_nguoi_khao_sat(int id)
        {
            ViewBag.id = id;
            return View();
        }
    }
}