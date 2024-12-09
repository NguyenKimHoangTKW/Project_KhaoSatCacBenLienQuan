using CTDT.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.UI.WebControls;

namespace CTDT.Areas.Admin.Controllers
{
    public class ThongKeKetQuaKhaoSatAPIController : ApiController
    {
        dbSurveyEntities db = new dbSurveyEntities();
        [HttpPost]
        [Route("api/check-doi-tuong-phieu-khao-sat")]
        public async Task<IHttpActionResult> check_phieu_khao_sat(survey survey)
        {
            var check_doi_tuong = await db.survey.FirstOrDefaultAsync(x => x.surveyID == survey.surveyID);
            bool sau_dai_hoc = new[] { 2 }.Contains(check_doi_tuong.id_hedaotao);
            if (sau_dai_hoc)
            {

            }
            return Ok();
        }
    }
}
