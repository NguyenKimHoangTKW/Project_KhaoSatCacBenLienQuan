using CTDT.Helper;
using CTDT.Models;
using CTDT.Models.Khoa;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace CTDT.Areas.Khoa.Controllers
{
    public class ThongKeKhaoSatKhoaAPIController : ApiController
    {
        dbSurveyEntities db = new dbSurveyEntities();
        private users user;

        public ThongKeKhaoSatKhoaAPIController()
        {
            user = SessionHelper.GetUser();
        }
        
    }
}