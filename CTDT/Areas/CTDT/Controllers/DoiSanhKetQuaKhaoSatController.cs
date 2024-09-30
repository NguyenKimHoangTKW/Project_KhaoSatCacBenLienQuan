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
    public class DoiSanhKetQuaKhaoSatController : Controller
    {
        dbSurveyEntities db = new dbSurveyEntities();
        // GET: CTDT/DoiSanhKetQuaKhaoSat
        public ActionResult Index()
        {
            var user = SessionHelper.GetUser();
            var surveyList = db.survey.Where(x => x.id_hedaotao == user.id_hdt).ToList();

            var sortedSurveyList = surveyList
                .OrderBy(s => s.surveyTitle.Split('.').First())
                .ThenBy(s => s.surveyTitle)
                .ToList();
            ViewBag.Survey = new SelectList(sortedSurveyList, "surveyTitle", "surveyTitle");
            ViewBag.Year = new SelectList(db.NamHoc.OrderByDescending(x => x.id_namhoc), "id_namhoc", "ten_namhoc");
            return View();
        }
        
        public JsonResult LoadYearByPKS(string namesurvey)
        {
            var user = SessionHelper.GetUser();
            var namhoc = db.survey
                .Where(x => x.surveyTitle == namesurvey && x.id_hedaotao == user.id_hdt)
                .Select(x => new
                {
                    TenNamHoc = x.NamHoc.ten_namhoc,
                }).ToList();
            return Json(new { data = namhoc}, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadSurveyByYear()
        {
            var user = SessionHelper.GetUser();
            var Year = db.NamHoc
                .Select(x => new
                {
                    IDNamHoc = x.id_namhoc,
                    TenNamHoc = x.ten_namhoc
                })
                .ToList();
            var datasurvey = new List<dynamic>();
            foreach(var items in Year)
            {
                var surveys = db.survey
                            .Where(x => x.id_namhoc == items.IDNamHoc && x.id_hedaotao == user.id_hdt)
                            .ToList();
                var sortedSurveys = surveys
                                    .OrderBy(s => s.surveyTitle.Split('.').FirstOrDefault())
                                    .ThenBy(s => s.surveyTitle)
                                    .Select(x => 
                                    new { IDYear = x.id_namhoc, IDSurvey = x.surveyID, NameSurvey = x.surveyTitle })
                                    .ToList();
                datasurvey.Add(sortedSurveys);
            }
            var GetData = new
            {
                Year = Year,
                DataSurvey = datasurvey
            };
            return Json(new { data = GetData }, JsonRequestBehavior.AllowGet);
        }
    }
}