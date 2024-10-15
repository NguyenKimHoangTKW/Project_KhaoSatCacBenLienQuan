using CTDT.Helper;
using CTDT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CTDT.Controllers
{
    public class AccountAPIController : ApiController
    {
        dbSurveyEntities db = new dbSurveyEntities();
        [HttpPost]
        [Route("api/session_login")]
        public IHttpActionResult Login_Google(users us)
        {
            DateTime now = DateTime.UtcNow;
            int unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            var user = db.users.Where(x => x.email == us.email).FirstOrDefault();
            var username = us.email.Split('@')[0];
            if (user == null)
            {
                user = new users
                {
                    email = us.email,
                    firstName = us.firstName,
                    username = username,
                    password = username + "@123",
                    lastName = us.lastName,
                    avatarUrl = us.avatarUrl,
                    ngaycapnhat = unixTimestamp,
                    ngaytao = unixTimestamp,
                    id_typeusers = 1
                };
                db.users.Add(user);
            }
            else
            {
                user.firstName = us.firstName;
                user.lastName = us.lastName;
                user.username = username;
                user.password = username + "@123";
                user.avatarUrl = us.avatarUrl;
                user.ngaycapnhat = unixTimestamp;

            }
            db.SaveChanges();
            SessionHelper.SetUser(user);
            return Ok(new { islogin = true });
        }
        [HttpPost]
        [Route("api/clear_session")]
        public IHttpActionResult Logout()
        {
            SessionHelper.ClearUser();
            return Ok(new { success = true });
        }
    }
}
