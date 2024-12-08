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
        // GET: CTDT/Interface
        public ActionResult bao_cao_tong_hop_ket_qua_khao_sat()
        {
            ViewBag.Year = new SelectList(db.NamHoc.OrderByDescending(x => x.id_namhoc), "id_namhoc", "ten_namhoc");
            return View();
        }
        public ActionResult giam_sat_ket_qua_khao_sat()
        {
            ViewBag.Year = new SelectList(db.NamHoc.OrderByDescending(x => x.id_namhoc), "id_namhoc", "ten_namhoc");
            return View();
        }
        public ActionResult thong_ke_khao_sat()
        {
            ViewBag.Year = new SelectList(db.NamHoc.OrderByDescending(x => x.id_namhoc), "id_namhoc", "ten_namhoc");
            return View();
        }
        public ActionResult thong_ke_tan_xuat()
        {
            var user = SessionHelper.GetUser();
            var surveyList = db.survey.Where(x => x.id_hedaotao == user.id_hdt).ToList();
            var sortedSurveyList = surveyList
                .OrderBy(s => s.surveyTitle.Split('.').First())
            .ThenBy(s => s.surveyTitle)
                .ToList();
            ViewBag.Survey = new SelectList(sortedSurveyList, "surveyID", "surveyTitle");
            ViewBag.Year = new SelectList(db.NamHoc.OrderByDescending(x => x.id_namhoc), "id_namhoc", "ten_namhoc");
            ViewBag.HocKy = new SelectList(db.hoc_ky.OrderBy(x => x.id_hk), "id_hk", "ten_hk");
            return View();
        }
        public ActionResult thong_ke_tan_xuat_theo_yeu_cau()
        {
            var user = SessionHelper.GetUser();
            ViewBag.Year = new SelectList(db.NamHoc.OrderByDescending(x => x.id_namhoc), "id_namhoc", "ten_namhoc");
            ViewBag.Lop = new SelectList(db.lop.Where(x => x.id_ctdt == user.id_ctdt).OrderBy(x => x.id_lop), "ma_lop", "ma_lop");
            return View();
        }

        public ActionResult xem_thong_ke_nguoi_khao_sat(int id)
        {
            ViewBag.id = id;
            return View();
        }
    }
}