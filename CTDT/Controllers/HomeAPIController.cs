using CTDT.Helper;
using CTDT.Models;
using Newtonsoft.Json;
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
        private int unixTimestamp;
        public HomeAPIController()
        {
            DateTime now = DateTime.UtcNow;
            unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }
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
                    TenHDT = c.ten_hedaotao,
                    mo_ta = c.describe,
                    img = c.image
                }).ToListAsync();

            if (!isUserLoggedIn)
            {
                var message = "Vui lòng đăng nhập để thực hiện chức năng";
                return Ok(new { data = JsonConvert.SerializeObject(hedaotao), message, islogin = false });
            }

            var response = new { data = JsonConvert.SerializeObject(hedaotao), islogin = true };

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
            var user = SessionHelper.GetUser();

            if (hdt == null || string.IsNullOrEmpty(hdt.ten_hedaotao))
            {
                return Ok(new { message = "Dữ liệu không hợp lệ", success = false });
            }

            var check_hdt = await db.hedaotao.FirstOrDefaultAsync(x => x.ten_hedaotao == hdt.ten_hedaotao);
            if (check_hdt == null)
            {
                return Ok(new { message = "Hệ đào tạo không tồn tại", success = false });
            }

            if (user == null)
            {
                return Ok(new { message = "Không thể phân loại được tài khoản", success = false });
            }

            var phieukhaosat = await db.survey
                .Where(c => c.id_hedaotao == check_hdt.id_hedaotao)
                .ToListAsync();

            foreach (var item in phieukhaosat)
            {
                if (item.surveyTimeStart <= unixTimestamp && item.surveyTimeEnd > unixTimestamp)
                {
                    item.surveyStatus = 1;
                }
                else
                {
                    item.surveyStatus = 0;
                }
            }

            await db.SaveChangesAsync();

            var list_phieu_khao_sat = phieukhaosat
                .Where(item => item.surveyStatus == 1)
                .Select(item => new
                {
                    MaPhieu = item.surveyID,
                    TenPKS = item.surveyTitle,
                    MoTaPhieu = item.surveyDescription,
                    MaHDT = item.id_hedaotao,
                    TenHDT = item.hedaotao.ten_hedaotao,
                    TenLoaiKhaoSat = item.LoaiKhaoSat.name_loaikhaosat
                }).ToList();

            if (list_phieu_khao_sat.Count > 0)
            {
                return Ok(new { data = JsonConvert.SerializeObject(list_phieu_khao_sat), success = true });
            }
            else
            {
                return Ok(new { message = "Ngoài thời gian thực hiện khảo sát", success = false });
            }
        }

    }
}
