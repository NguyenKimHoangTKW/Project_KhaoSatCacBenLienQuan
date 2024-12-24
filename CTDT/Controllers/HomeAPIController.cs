using CTDT.Helper;
using CTDT.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;

namespace CTDT.Controllers
{
    public class HomeAPIController : ApiController
    {
        dbSurveyEntities db = new dbSurveyEntities();
        [HttpPost]
        [Route("api/load_he_dao_tao")]
        public async Task<IHttpActionResult> LoadHeDaoTao()
        {
            var isUserLoggedIn = SessionHelper.IsUserLoggedIn();
            var user = SessionHelper.GetUser();

            var hedaotao = await db.hedaotao
                .Select(c => new
                {
                    MaHDT = c.id_hedaotao,
                    TenHDT = c.ten_hedaotao
                }).ToListAsync();

            if (!isUserLoggedIn)
            {
                var message = "Vui lòng đăng nhập để thực hiện chức năng";
                return Ok(new { data = hedaotao, message, islogin = false });
            }

            var response = new { data = hedaotao, islogin = true };

            switch (user.id_typeusers)
            {
                case 1:
                    return Ok(new { response.data, response.islogin, client = true });
                case 2:
                    return Ok(new { response.islogin, admin = true });
                case 3:
                    return Ok(new { response.data, response.islogin, ctdt = true });
                case 5:
                    return Ok(new { response.data, response.islogin, khoa = true });
                case 6:
                    return Ok(new { response.data, response.islogin, hop_tac_doanh_nghiep = true });
                default:
                    var message = "Vui lòng đăng nhập để thực hiện chức năng";
                    return Ok(new { message, islogin = false });
            }
        }

        [HttpPost]
        [Route("api/bo_phieu_khao_sat")]
        public async Task<IHttpActionResult> load_phieu_khao_sat(hedaotao hdt)
        {
            if (hdt == null || string.IsNullOrEmpty(hdt.ten_hedaotao))
            {
                return BadRequest("Dữ liệu không hợp lệ");
            }
            if (!db.hedaotao.Any(x => x.ten_hedaotao == hdt.ten_hedaotao))
            {
                return BadRequest("Hệ đào tạo không tồn tại");
            }
            var user = SessionHelper.GetUser();
            if (user == null)
            {
                return BadRequest("Không thể phân loại được tài khoản");
            }
            var phieukhaosat = await db.survey
               .Where(c => c.surveyStatus == 1 && c.hedaotao.ten_hedaotao == hdt.ten_hedaotao).ToListAsync();
            var list_phieu_khao_sat = new List<dynamic>();
            foreach (var item in phieukhaosat)
            {
                AddSurveyToList(list_phieu_khao_sat, item.surveyID);
            }
            var get_data = new
            {
                survey = list_phieu_khao_sat,
            };
            return Ok(new { data = get_data });
        }
        private void AddSurveyToList(List<dynamic> list, int? surveyid)
        {
            var survey = db.survey.FirstOrDefault(x => x.surveyID == surveyid);
            var surveyDetails = new
            {
                MaPhieu = survey.surveyID,
                TenPKS = survey.surveyTitle,
                MoTaPhieu = survey.surveyDescription,
                MaHDT = survey.id_hedaotao,
                TenHDT = survey.hedaotao.ten_hedaotao,
                TenLoaiKhaoSat = survey.LoaiKhaoSat.name_loaikhaosat
            };
            list.Add(surveyDetails);
        }
    }
}
