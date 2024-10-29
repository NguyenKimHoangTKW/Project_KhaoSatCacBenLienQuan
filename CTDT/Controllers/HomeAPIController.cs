using CTDT.Helper;
using CTDT.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
        public async Task<IHttpActionResult> load_phieu_khao_sat([FromBody] hedaotao hdt)
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
            var emailDomain = user.email.Split('@')[1].ToLower();
            var CodeEmail = user.email.Split('@')[0];
            var get_key_code_learner = CodeEmail.ToString().Substring(0, 2);
            var phieukhaosat = await db.survey
               .Where(c => c.surveyStatus == true && c.hedaotao.ten_hedaotao == hdt.ten_hedaotao).ToListAsync();
            var list_phieu_khao_sat = new List<dynamic>();
            var check_xac_thuc = new List<dynamic>();
            foreach (var item in phieukhaosat)
            {
                var survey = await db.survey.Where(x => x.surveyID == item.surveyID).FirstOrDefaultAsync();
                bool check_answer_survey = db.answer_response.Any(x => x.surveyID == item.surveyID && x.id_users == user.id_users && x.id_namhoc == item.id_namhoc && x.json_answer != null);
                bool not_nguoi_hoc = new[] { 2, 5, 6 }.Contains(item.id_loaikhaosat);
                bool is_nguoi_hoc = new[] { 1, 4 }.Contains(item.id_loaikhaosat);
                bool is_cbvc = new[] { 3, 8 }.Contains(item.id_loaikhaosat);
                if(user.id_ctdt != null || user.id_khoa != null)
                {
                    AddSurveyToList(list_phieu_khao_sat, survey);
                }
                else if (not_nguoi_hoc)
                {
                    AddSurveyToList(list_phieu_khao_sat, survey);

                }
                else if (is_nguoi_hoc)
                {
                    var keyClassList = DeserializeKeyClass(survey.key_class);
                    if (emailDomain == "student.tdmu.edu.vn" && keyClassList.Contains(get_key_code_learner) && db.sinhvien.Any(x => x.ma_sv == CodeEmail))
                    {
                        AddSurveyToList(list_phieu_khao_sat, survey);
                    }
                }
                else if (is_cbvc)
                {
                    if (db.CanBoVienChuc.Any(x => x.Email == user.email))
                    {
                        AddSurveyToList(list_phieu_khao_sat, survey);
                    }
                }
            }
            var get_data = new
            {
                survey = list_phieu_khao_sat,
            };
            return Ok(new { data = get_data });
        }
        private void AddSurveyToList(List<dynamic> list, dynamic survey)
        {
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
        private List<string> DeserializeKeyClass(string keyClassJson)
        {
            return new JavaScriptSerializer().Deserialize<List<string>>(keyClassJson);
        }
    }
}
