using CTDT.Helper;
using CTDT.Models;
using CTDT.Models.Khoa;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace CTDT.Areas.Khoa.Controllers
{
    public class BaoCaoTongHopKetQuaKhaoSatKhoaAPIController : ApiController
    {
        dbSurveyEntities db = new dbSurveyEntities();
        private users user;

        public BaoCaoTongHopKetQuaKhaoSatKhoaAPIController()
        {
            user = SessionHelper.GetUser();
        }
        
    }
}
