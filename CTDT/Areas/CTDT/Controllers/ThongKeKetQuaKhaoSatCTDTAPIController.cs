using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using CTDT.Helper;
using CTDT.Models;
using Newtonsoft.Json;

namespace CTDT.Areas.CTDT.Controllers
{
    public class ThongKeKetQuaKhaoSatCTDTAPIController : ApiController
    {
        dbSurveyEntities db = new dbSurveyEntities();
        private users user;
        private bool is_user_hop_tac_doanh_nghiep;
        private bool is_user_ctdt;
        private bool is_user_khoa;
        public ThongKeKetQuaKhaoSatCTDTAPIController()
        {
            user = SessionHelper.GetUser();
        }
        [HttpPost]
        [Route("api/ctdt/load-survey-check-ctdt")]
        public async Task<IHttpActionResult> load_survey_by_hdt_of_ctdt(get_option_ctdt getoption)
        {
            var check_ctdt = await db.ctdt.FirstOrDefaultAsync(x => x.id_ctdt == getoption.id_ctdt);
            var get_survey = await db.survey
                .Where(x => x.id_hedaotao == check_ctdt.id_hdt && x.id_namhoc == getoption.id_namhoc)
                .Select(x => new
                {
                    value = x.surveyID,
                    name = x.id_dot_khao_sat != null ? x.surveyTitle + " - " + x.dot_khao_sat.ten_dot_khao_sat : x.surveyTitle,
                }).ToListAsync();
            if (get_survey.Count > 0)
            {
                return Ok(new { data = JsonConvert.SerializeObject(get_survey), success = true });
            }
            else
            {
                return Ok(new { message = "Chưa có dữ liệu phiếu khảo sát", success = false });
            }
        }
        public class get_option_ctdt
        {
            public int id_namhoc { get; set; }
            public int id_ctdt { get; set; }
        }
    }
}
