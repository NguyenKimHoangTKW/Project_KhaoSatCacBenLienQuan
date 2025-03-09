using CTDT.Helper;
using CTDT.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace CTDT.Controllers
{
    public class SurveyAPIController : ApiController
    {
        dbSurveyEntities db = new dbSurveyEntities();
        public users user;
        public SurveyAPIController()
        {
            user = SessionHelper.GetUser();
        }
        public class SaveXacThuc
        {
            public int surveyID { get; set; }
            public int id_ctdt { get; set; }
            public string ma_vien_chuc { get; set; }
            public string ten_vien_chuc { get; set; }
        }
        [HttpPost]
        [Route("api/load_form_phieu_khao_sat")]
        public async Task<IHttpActionResult> load_phieu_khao_sat(SaveXacThuc sxt)
        {
            var domainGmail = user.email.Split('@')[1];
            var ms_nguoi_hoc = user.email.Split('@')[0];
            var get_data = await db.survey.FirstOrDefaultAsync(x => x.surveyID == sxt.surveyID);
            var js_data = get_data.surveyData;
            if (get_data.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu giảng viên")
            {
                if(get_data.id_loaikhaosat == 3)
                {
                    var get_value = (int)HttpContext.Current.Session["id_cbvc_ks"];
                    var get_ctdt = (int)HttpContext.Current.Session["id_cbvc_ks_ctdt"];
                    var _get_ctdt = await db.ctdt.FirstOrDefaultAsync(x => x.id_ctdt == get_ctdt);
                    var cbvc = await db.cbvc_khao_sat
                        .Where(x => x.surveyID == sxt.surveyID && x.id_cbvc_khao_sat == get_value)
                        .Select(x => new
                        {
                            x.CanBoVienChuc.MaCBVC,
                            x.CanBoVienChuc.TenCBVC,
                            x.CanBoVienChuc.trinh_do.ten_trinh_do,
                            x.CanBoVienChuc.ChucVu.name_chucvu,
                            x.CanBoVienChuc.khoa_vien_truong.ten_khoa,
                            x.CanBoVienChuc.nganh_dao_tao,
                            khao_sat_cho = _get_ctdt.ten_ctdt
                        })
                        .FirstOrDefaultAsync();
                    return Ok(new { data = js_data, info = JsonConvert.SerializeObject(cbvc), success = true, is_gv = true });
                }
                else
                {
                    var get_value = (int)HttpContext.Current.Session["id_cbvc_ks"];
                    var cbvc = await db.cbvc_khao_sat
                        .Where(x => x.surveyID == sxt.surveyID && x.id_cbvc_khao_sat == get_value)
                        .Select(x => new
                        {
                            x.CanBoVienChuc.MaCBVC,
                            x.CanBoVienChuc.TenCBVC,
                            x.CanBoVienChuc.trinh_do.ten_trinh_do,
                            x.CanBoVienChuc.ChucVu.name_chucvu,
                            x.CanBoVienChuc.khoa_vien_truong.ten_khoa,
                            x.CanBoVienChuc.nganh_dao_tao,
                        })
                        .FirstOrDefaultAsync();
                    return Ok(new { data = js_data, info = JsonConvert.SerializeObject(cbvc), success = true, is_cbvc = true });
                }
              
            }
            return Ok(new { message = "Vui lòng xác thực để thực hiện khảo sát" });
        }
        [HttpPost]
        [Route("api/save_form_khao_sat")]
        public async Task<IHttpActionResult> save_form(SaveForm saveForm)
        {
            var user = SessionHelper.GetUser();
            var domainGmail = user.email.Split('@')[1];
            var ms_nguoi_hoc = user.email.Split('@')[0];
            DateTime now = DateTime.UtcNow;
            int unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            var survey = await db.survey.FirstOrDefaultAsync(x => x.surveyID == saveForm.idsurvey);
            answer_response aw = null;
            if (survey == null)
            {
                return Ok(new {message = "Biểu mẫu không tồn tại",success = false});
            }

            if (ModelState.IsValid)
            {
                if(survey.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu giảng viên")
                {
                    if(survey.id_loaikhaosat == 3)
                    {
                        var get_value = (int)HttpContext.Current.Session["id_cbvc_ks"];
                        var get_ctdt = (int)HttpContext.Current.Session["id_cbvc_ks_ctdt"];
                        var _get_ctdt = await db.ctdt.FirstOrDefaultAsync(x => x.id_ctdt == get_ctdt);
                        var cbvc = await db.cbvc_khao_sat
                            .FirstOrDefaultAsync(x => x.surveyID == saveForm.idsurvey && x.id_cbvc_khao_sat == get_value);
                        aw = new answer_response()
                        {
                            time = unixTimestamp,
                            id_users = user.id_users,
                            id_namhoc = survey.id_namhoc,
                            id_cbvc_khao_sat = cbvc.id_cbvc_khao_sat,
                            id_ctdt = _get_ctdt.id_ctdt,
                            surveyID = survey.surveyID,
                            email = user.email,
                            json_answer = saveForm.json_answer
                        };
                    }
                    else
                    {
                        var get_value = (int)HttpContext.Current.Session["id_cbvc_ks"];
                        var cbvc = await db.cbvc_khao_sat
                            .FirstOrDefaultAsync(x => x.surveyID == saveForm.idsurvey && x.id_cbvc_khao_sat == get_value);
                        aw = new answer_response()
                        {
                            time = unixTimestamp,
                            id_users = user.id_users,
                            id_namhoc = survey.id_namhoc,
                            id_cbvc_khao_sat = cbvc.id_cbvc_khao_sat,
                            surveyID = survey.surveyID,
                            json_answer = saveForm.json_answer
                        };
                    }
                }
                db.answer_response.Add(aw);
                await db.SaveChangesAsync();
                return Ok(new { success = true, message = "Khảo sát thành công" });
            }
            return Ok(new { message = "Không thể lưu khảo sát", success = false });
        }

        [HttpPost]
        [Route("api/load_dap_an_pks")]
        public async Task<IHttpActionResult> load_answer_phieu_khao_sat(LoadAnswerPKS loadAnswerPKS)
        {
            var answer_responses = await db.answer_response
                .FirstOrDefaultAsync(x => x.id == loadAnswerPKS.id_answer && x.surveyID == loadAnswerPKS.id_survey);
            var list_info = new List<dynamic>();
            switch (answer_responses.survey.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat)
            {
                case "Phiếu người học":
                    var data_hoc_vien = new
                    {
                        phieu_khao_sat = answer_responses.survey.surveyData,
                        dap_an = answer_responses.json_answer
                    };
                    list_info.Add(new
                    {
                        email = answer_responses.users.email,
                        ten_nguoi_hoc = answer_responses.sinhvien.hovaten,
                        ma_nguoi_hoc = answer_responses.sinhvien.ma_sv,
                        lop = answer_responses.sinhvien.lop.ma_lop,
                        ctdt = answer_responses.ctdt.ten_ctdt,
                        khao_sat_lan_cuoi = answer_responses.time
                    });
                    return Ok(new { data = data_hoc_vien, info = list_info, is_nguoi_hoc = true });
                case "Phiếu giảng viên":
                    var data_cbvc = new
                    {
                        phieu_khao_sat = answer_responses.survey.surveyData,
                        dap_an = answer_responses.json_answer
                    };

                    return Ok(new { data = data_cbvc, info = list_info });

                case "Phiếu doanh nghiệp":
                    var data_doanh_nghiep = new
                    {
                        phieu_khao_sat = answer_responses.survey.surveyData,
                        dap_an = answer_responses.json_answer
                    };
                    return Ok(new { data = data_doanh_nghiep, info = list_info });

                case "Phiếu người học có học phần":
                    var data_ho_vien_mon_hoc = new
                    {
                        phieu_khao_sat = answer_responses.survey.surveyData,
                        dap_an = answer_responses.json_answer
                    };
                    list_info.Add(new
                    {
                        email = answer_responses.users.email,
                        ten_nguoi_hoc = answer_responses.sinhvien.hovaten,
                        ma_nguoi_hoc = answer_responses.sinhvien.ma_sv,
                        lop = answer_responses.sinhvien.lop.ma_lop,
                        ctdt = answer_responses.ctdt.ten_ctdt,
                        mon_hoc = answer_responses.mon_hoc.ten_mon_hoc,
                        hoc_phan = answer_responses.mon_hoc.hoc_phan.ten_hoc_phan,
                        ten_giang_vien = answer_responses.CanBoVienChuc.TenCBVC,
                        khao_sat_lan_cuoi = answer_responses.time
                    });
                    return Ok(new { data = data_ho_vien_mon_hoc, info = list_info, is_hoc_phan_nguoi_hoc = true });
            }
            return BadRequest();
        }
        [HttpPost]
        [Route("api/save_answer_form")]
        public async Task<IHttpActionResult> save_answer_form(answer_response aw)
        {
            var user = SessionHelper.GetUser();
            var status = "";
            var answer = await db.answer_response.FindAsync(aw.id);
            var survey = await db.answer_response.FirstOrDefaultAsync(x => x.id == answer.id);
            DateTime now = DateTime.UtcNow;
            int unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            if (answer != null)
            {
                answer.json_answer = aw.json_answer;
                answer.time = unixTimestamp;
                await db.SaveChangesAsync();
                status = "Cập nhật phiếu khảo sát thành công";
            }
            else
            {
                status = "Cập nhật phiếu khảo sát thất bại";
            }
            return Ok(new { message = status });
        }


        [HttpPost]
        [Route("api/load_bo_phieu_da_khao_sat")]
        public async Task<IHttpActionResult> load_bo_phieu_da_khao_sat(FindPKSisSurvey findPKSisSurvey)
        {
            var user = SessionHelper.GetUser();
            var get_answer_survey = await db.answer_response
                .Where(x => x.id_users == user.id_users)
                .Select(x => x.surveyID)
                .Distinct()
                .ToListAsync();
            var get_survey = await db.survey
                .Where(x => get_answer_survey.Contains(x.surveyID)
                && x.id_hedaotao == findPKSisSurvey.hedaotao
                && x.id_namhoc == findPKSisSurvey.namhoc)
                .ToListAsync();
            var surveyList = new List<dynamic>();
            foreach (var item in get_survey)
            {
                var query = db.answer_response
                    .Where(x => x.id_users == user.id_users && x.surveyID == item.surveyID)
                    .AsQueryable();
                var bo_phieu = new List<dynamic>();
                var check_loai = new List<dynamic>();
                var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                if (item.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu học viên")
                {
                    bo_phieu.AddRange(query
                        .Select(x => new
                        {
                            email = user.email,
                            nguoi_hoc = x.sinhvien.hovaten,
                            ma_nguoi_hoc = x.sinhvien.ma_sv,
                            ctdt = x.ctdt.ten_ctdt,
                            nam_hoc = x.NamHoc.ten_namhoc,
                            thoi_gian_khao_sat = x.time,
                            page = currentTime > x.time ? "/phieu-khao-sat/dap-an/" + x.id + "/" + x.surveyID : "Ngoài thời gian thực hiện khảo sát"
                        }).ToList());

                    surveyList.Add(new
                    {
                        ten_phieu = item.surveyTitle,
                        bo_phieu = bo_phieu,
                        is_student = true
                    });
                }
                else if (item.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu doanh nghiệp")
                {
                    bo_phieu.AddRange(query
                        .Select(x => new
                        {
                            email = user.email,
                            ctdt = x.ctdt.ten_ctdt,
                            nam_hoc = x.NamHoc.ten_namhoc,
                            thoi_gian_khao_sat = x.time,
                            page = currentTime > x.time ? "/phieu-khao-sat/dap-an/" + x.id + "/" + x.surveyID : "Ngoài thời gian thực hiện khảo sát"
                        }).ToList());
                    surveyList.Add(new
                    {
                        ten_phieu = item.surveyTitle,
                        bo_phieu = bo_phieu,
                        is_ctdt = true
                    });
                }
                else if (item.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu giảng viên")
                {
                    bo_phieu.AddRange(query
                        .Select(x => new
                        {
                            email = user.email,
                            ten_cbcv = x.CanBoVienChuc.TenCBVC,
                            ctdt = x.ctdt.ten_ctdt,
                            nam_hoc = x.NamHoc.ten_namhoc,
                            thoi_gian_khao_sat = x.time,
                            page = currentTime > x.time ? "/phieu-khao-sat/dap-an/" + x.id + "/" + x.surveyID : "Ngoài thời gian thực hiện khảo sát"
                        }).ToList());
                    surveyList.Add(new
                    {
                        ten_phieu = item.surveyTitle,
                        bo_phieu = bo_phieu,
                        is_cbvc = true
                    });
                }
            }
            return Ok(new { data = new { survey = surveyList } });
        }

    }
}
