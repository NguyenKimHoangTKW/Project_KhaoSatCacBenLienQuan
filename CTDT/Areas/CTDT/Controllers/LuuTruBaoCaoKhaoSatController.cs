using CTDT.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CTDT.Areas.CTDT.Controllers
{
    [UserAuthorize(3)]
    public class LuuTruBaoCaoKhaoSatController : Controller
    {
        // GET: CTDT/LuuTruBaoCaoKhaoSat
        public ActionResult Index()
        {
            return View();
        }
    }
}