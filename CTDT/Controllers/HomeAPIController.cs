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
        public async Task<IHttpActionResult> load_he_dao_tao()
        {
            var check_user = SessionHelper.IsUserLoggedIn();
            var user = SessionHelper.GetUser();
            var hedaotao = await db.hedaotao
              .Select(c => new
              {
                  MaHDT = c.id_hedaotao,
                  TenHDT = c.ten_hedaotao,
              }).ToListAsync();
            if (check_user && user.id_typeusers == 1)
            {
                return Ok(new { data = hedaotao, islogin = true, client = true });
            }
            else if (check_user && user.id_typeusers == 2)
            {
                return Ok(new { islogin = true, admin = true });
            }
            else if (check_user && user.id_typeusers == 3)
            {
                return Ok(new { data = hedaotao, islogin = true, ctdt = true });
            }
            else
            {
                var message = "Vui lòng đăng nhập để thực hiện chức năng";
                return Ok(new { data = hedaotao, message = message, islogin = false });
            }
        }
        [HttpPost]
        [Route("api/bo_phieu_khao_sat")]
        public async Task<IHttpActionResult> load_phieu_khao_sat(hedaotao hdt)
        {
            var user = SessionHelper.GetUser();
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


                if (item.id_loaikhaosat == 5 || item.id_loaikhaosat == 2 || item.id_loaikhaosat == 6)
                {
                    var test = new
                    {
                        MaPhieu = item.surveyID,
                        TenPKS = item.surveyTitle,
                        MoTaPhieu = item.surveyDescription,
                        MaHDT = item.id_hedaotao,
                        TenHDT = item.hedaotao.ten_hedaotao,
                        TenLoaiKhaoSat = item.LoaiKhaoSat.name_loaikhaosat,
                    };
                    list_phieu_khao_sat.Add(test);

                }
                else if ((item.id_loaikhaosat == 1 || item.id_loaikhaosat == 4))
                {
                    var keyClassList = new JavaScriptSerializer().Deserialize<List<string>>(survey.key_class);
                    if (emailDomain == "student.tdmu.edu.vn" && keyClassList.Contains(get_key_code_learner) && db.sinhvien.Any(x => x.ma_sv == CodeEmail))
                    {
                        var test = new
                        {
                            MaPhieu = item.surveyID,
                            TenPKS = item.surveyTitle,
                            MoTaPhieu = item.surveyDescription,
                            MaHDT = item.id_hedaotao,
                            TenHDT = item.hedaotao.ten_hedaotao,
                            TenLoaiKhaoSat = item.LoaiKhaoSat.name_loaikhaosat
                        };
                        list_phieu_khao_sat.Add(test);
                    }
                }
                else if (item.id_loaikhaosat == 3 || item.id_loaikhaosat == 8)
                {
                    if (db.CanBoVienChuc.Any(x => x.Email == user.email))
                    {
                        var test = new
                        {
                            MaPhieu = item.surveyID,
                            TenPKS = item.surveyTitle,
                            MoTaPhieu = item.surveyDescription,
                            MaHDT = item.id_hedaotao,
                            TenHDT = item.hedaotao.ten_hedaotao,
                            TenLoaiKhaoSat = item.LoaiKhaoSat.name_loaikhaosat,
                        };
                        list_phieu_khao_sat.Add(test);
                    }
                }
            }
            var get_data = new
            {
                survey = list_phieu_khao_sat,
            };
            return Ok(new { data = get_data});
        }
    }
}
