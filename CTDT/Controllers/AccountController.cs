using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using CTDT.Helper;
using CTDT.Models;
using Microsoft.Owin.Security;

namespace CTDT.Controllers
{
    public class AccountController : Controller
    {
        dbSurveyEntities db = new dbSurveyEntities();
        public JsonResult Login_Google(string email, string firstname, string lastname, string urlimage)
        {
            DateTime now = DateTime.UtcNow;
            int unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            var user = db.users.Where(x => x.email == email).FirstOrDefault();
            var username = email.Split('@')[0];
            if (user == null)
            {
                user = new users
                {
                    email = email,
                    firstName = firstname,
                    username = username,
                    password = username + "@123",
                    lastName = lastname,
                    avatarUrl = urlimage,
                    ngaycapnhat = unixTimestamp,
                    ngaytao = unixTimestamp,
                    id_typeusers = 1
                };
                db.users.Add(user);
            }
            else
            {
                user.firstName = firstname;
                user.lastName = lastname;
                user.username = username;
                user.password = username + "@123";
                user.avatarUrl = urlimage;
                user.ngaycapnhat = unixTimestamp;

            }
            db.SaveChanges();
            SessionHelper.SetUser(user);
            return Json(new {islogin = true},JsonRequestBehavior.AllowGet);
        }
        
        public JsonResult Logout()
        {
            SessionHelper.ClearUser();
            return Json(new {success = true},JsonRequestBehavior.AllowGet);
        }

    }
}
