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
            var surveyList = db.survey.ToList();

            var sortedSurveyList = surveyList
                .OrderBy(s => s.surveyTitle.Split('.').First())
                .ThenBy(s => s.surveyTitle)
                .ToList();
            ViewBag.Survey = new SelectList(sortedSurveyList, "surveyTitle", "surveyTitle");
            ViewBag.Year = new SelectList(db.NamHoc.OrderByDescending(x => x.id_namhoc), "id_namhoc", "ten_namhoc");
            return View();
        }
        
    }
}