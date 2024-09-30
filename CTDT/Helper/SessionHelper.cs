using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CTDT.Helper
{
    public static class SessionHelper
    {
        private const string UserInfoSessionKey = "UserInfoSessionKey";
        private const string UserRoleSessionKey = "UserRoleSessionKey";


        public static void SetUser(CTDT.Models.users user)
        {
            HttpContext.Current.Session[UserInfoSessionKey] = user;
            HttpContext.Current.Session[UserRoleSessionKey] = user.id_typeusers;

        }

        public static CTDT.Models.users GetUser()
        {
            return HttpContext.Current.Session[UserInfoSessionKey] as CTDT.Models.users;
        }
        public static string GetUserRole()
        {
            return HttpContext.Current.Session[UserRoleSessionKey] as string;
        }
        public static void ClearUser()
        {
            HttpContext.Current.Session.Remove(UserInfoSessionKey);
            HttpContext.Current.Session.Remove(UserRoleSessionKey);
        }

        public static bool IsUserLoggedIn()
        {
            return HttpContext.Current.Session[UserInfoSessionKey] != null;
        }
    }

}