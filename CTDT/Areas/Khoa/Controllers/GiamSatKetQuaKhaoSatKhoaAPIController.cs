using CTDT.Helper;
using CTDT.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web.Http;
using System.Web.Script.Serialization;
using Microsoft.Ajax.Utilities;
using CTDT.Models.Khoa;
namespace CTDT.Areas.Khoa.Controllers
{
    public class GiamSatKetQuaKhaoSatKhoaAPIController : ApiController
    {
        dbSurveyEntities db = new dbSurveyEntities();
        private users user;

        public GiamSatKetQuaKhaoSatKhoaAPIController()
        {
            user = SessionHelper.GetUser();
        }
    }
}
