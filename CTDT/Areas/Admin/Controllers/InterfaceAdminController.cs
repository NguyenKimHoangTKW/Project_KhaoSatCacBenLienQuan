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
    }
}