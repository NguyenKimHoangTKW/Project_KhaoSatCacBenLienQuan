using CTDT.Helper;
using CTDT.Models;
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
        [HttpPost]
        [Route("api/load_form_phieu_khao_sat")]
        public async Task<IHttpActionResult> load_phieu_khao_sat(SaveXacThuc sxt)
        {
            var domainGmail = user.email.Split('@')[1];
            var ms_nguoi_hoc = user.email.Split('@')[0];
            var get_data = await db.survey.FirstOrDefaultAsync(x => x.surveyID == sxt.Id);
            var js_data = get_data.surveyData;
            var list_thong_tin = new List<dynamic>();
            switch (get_data.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat)
            {
                case "Phiếu học viên":
                    var nguoi_hoc = await db.sinhvien
                        .Where(x => sxt.nguoi_hoc != null ? x.id_sv == sxt.nguoi_hoc : x.ma_sv == ms_nguoi_hoc)
                        .Select(x => new
                        {
                            email = user.email,
                            ma_nguoi_hoc = x.ma_sv,
                            ten_nguoi_hoc = x.hovaten,
                            khoa = x.lop.ctdt.khoa.ten_khoa,
                            ctdt = x.lop.ctdt.ten_ctdt,
                        })
                        .FirstOrDefaultAsync();
                    list_thong_tin.Add(nguoi_hoc);
                    return Ok(new { data = js_data, info = list_thong_tin, is_nguoi_hoc = true });

                case "Phiếu giảng viên":
                    var can_bo_vien_chuc_obj = db.CanBoVienChuc.FirstOrDefault(x => x.Email == user.email);
                    var append_list_cbvc = new List<dynamic>();
                    if (can_bo_vien_chuc_obj != null)
                    {

                        var don_vi = can_bo_vien_chuc_obj.DonVi != null ? can_bo_vien_chuc_obj.DonVi.name_donvi : null;
                        var chuc_vu = can_bo_vien_chuc_obj.ChucVu != null ? can_bo_vien_chuc_obj.ChucVu.name_chucvu : null;
                        var ctdt_giang_vien = can_bo_vien_chuc_obj.ctdt != null ? can_bo_vien_chuc_obj.ctdt.ten_ctdt : null;

                        if (sxt.donvi != null || sxt.ctdt != null)
                        {
                            var don_vi_select = db.DonVi.FirstOrDefault(x => x.id_donvi == sxt.donvi);
                            var ctdt_select = db.ctdt.FirstOrDefault(x => x.id_ctdt == sxt.ctdt);
                            append_list_cbvc.Add(new
                            {
                                email = user.email,
                                ten_cbvc = can_bo_vien_chuc_obj.TenCBVC,
                                don_vi = don_vi_select != null ? don_vi_select.name_donvi : null,
                                chuc_vu = can_bo_vien_chuc_obj.ChucVu != null ? can_bo_vien_chuc_obj.ChucVu.name_chucvu : null,
                                ctdt = ctdt_select != null ? ctdt_select.ten_ctdt : null,
                            });
                        }
                        else
                        {
                            append_list_cbvc.Add(new
                            {
                                email = user.email,
                                ten_cbvc = can_bo_vien_chuc_obj.TenCBVC,
                                don_vi = don_vi,
                                chuc_vu = chuc_vu,
                                ctdt = ctdt_giang_vien,
                            });
                        }
                        return Ok(new { data = js_data, info = append_list_cbvc, is_cbvc = true });
                    }
                    else
                    {
                        return Ok(new { is_cbvc = false, message = "Người dùng không có quyền khảo sát phiếu khảo sát này" });
                    }

                case "Phiếu doanh nghiệp":
                    var ctdt_doanh_nghiep = db.ctdt.FirstOrDefault(x => x.id_ctdt == sxt.ctdt);
                    var get_thong_tin_doanh_nghiep = new
                    {
                        email = user.email,
                        ctdt = ctdt_doanh_nghiep.ten_ctdt,
                    };
                    list_thong_tin.Add(get_thong_tin_doanh_nghiep);
                    return Ok(new { data = js_data, info = list_thong_tin, is_doanh_nghiep = true });

                case "Phiếu người học có học phần":
                    var check_mon_hoc = db.nguoi_hoc_dang_co_hoc_phan.FirstOrDefault(x => x.id_nguoi_hoc_by_hoc_phan == sxt.id_nguoi_hoc_by_mon_hoc);
                    var get_thong_tin_mon_hoc = new
                    {
                        email = user.email,
                        ma_mon_hoc = check_mon_hoc.id_nguoi_hoc_by_hoc_phan,
                        ma_giang_vien = check_mon_hoc.id_giang_vvien,
                        ten_giang_vien = check_mon_hoc.CanBoVienChuc.TenCBVC,
                        ma_nguoi_hoc = check_mon_hoc.sinhvien.ma_sv,
                        ten_nguoi_hoc = check_mon_hoc.sinhvien.hovaten,
                        mon_hoc = check_mon_hoc.mon_hoc.ten_mon_hoc,
                        hoc_phan = check_mon_hoc.mon_hoc.hoc_phan.ten_hoc_phan,

                    };
                    list_thong_tin.Add(get_thong_tin_mon_hoc);
                    return Ok(new { data = js_data, info = list_thong_tin, is_hoc_phan_nguoi_hoc = true });
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
            if (survey == null)
            {
                return BadRequest("Khảo sát không tồn tại");
            }

            if (ModelState.IsValid)
            {
                answer_response aw = null;

                switch (survey.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat)
                {
                    case "Phiếu học viên":
                        var nguoi_hoc = await db.sinhvien.FirstOrDefaultAsync(x => x.ma_sv == saveForm.ma_nguoi_hoc);
                        aw = new answer_response()
                        {
                            time = unixTimestamp,
                            id_users = user.id_users,
                            id_namhoc = survey.id_namhoc,
                            id_sv = nguoi_hoc.id_sv,
                            id_ctdt = nguoi_hoc.lop.ctdt.id_ctdt,
                            surveyID = survey.surveyID,
                            json_answer = saveForm.json_answer
                        };
                        break;

                    case "Phiếu giảng viên":
                        var can_bo_vien_chuc = await db.CanBoVienChuc.FirstOrDefaultAsync(x => user.email == x.Email);
                        var don_vi_select = await db.DonVi?.FirstOrDefaultAsync(x => x.name_donvi == saveForm.don_vi);
                        var ctdt_select = await db.ctdt?.FirstOrDefaultAsync(x => x.ten_ctdt == saveForm.ctdt);
                        if (don_vi_select == null || ctdt_select == null)
                        {
                            aw = new answer_response()
                            {
                                time = unixTimestamp,
                                id_users = user.id_users,
                                id_namhoc = survey.id_namhoc,
                                id_CBVC = can_bo_vien_chuc.id_CBVC,
                                id_ctdt = ctdt_select != null ? ctdt_select.id_ctdt : (int?)null,
                                id_donvi = don_vi_select != null ? don_vi_select.id_donvi : (int?)null,
                                surveyID = survey.surveyID,
                                json_answer = saveForm.json_answer
                            };
                        }
                        else
                        {
                            aw = new answer_response()
                            {
                                time = unixTimestamp,
                                id_users = user.id_users,
                                id_namhoc = survey.id_namhoc,
                                id_CBVC = can_bo_vien_chuc.id_CBVC,
                                id_ctdt = can_bo_vien_chuc.ctdt?.id_ctdt ?? null,
                                id_donvi = can_bo_vien_chuc.DonVi?.id_donvi ?? null,
                                surveyID = survey.surveyID,
                                json_answer = saveForm.json_answer
                            };
                        }
                        break;

                    case "Phiếu doanh nghiệp":
                        var ctdt = await db.ctdt.FirstOrDefaultAsync(x => x.ten_ctdt == saveForm.ctdt);
                        aw = new answer_response()
                        {
                            time = unixTimestamp,
                            id_users = user.id_users,
                            id_namhoc = survey.id_namhoc,
                            id_ctdt = ctdt.id_ctdt,
                            surveyID = survey.surveyID,
                            json_answer = saveForm.json_answer
                        };
                        break;

                    case "Phiếu người học có học phần":
                        var get_mon_hoc = await db.nguoi_hoc_dang_co_hoc_phan.FirstOrDefaultAsync(x => x.id_nguoi_hoc_by_hoc_phan == saveForm.id_mon_hoc);
                        aw = new answer_response()
                        {
                            time = unixTimestamp,
                            id_users = user.id_users,
                            id_namhoc = survey.id_namhoc,
                            id_ctdt = get_mon_hoc.sinhvien.lop.id_ctdt,
                            id_donvi = get_mon_hoc.CanBoVienChuc?.id_donvi,
                            id_CBVC = get_mon_hoc.id_giang_vvien,
                            id_mh = get_mon_hoc.id_mon_hoc,
                            id_sv = get_mon_hoc.id_sinh_vien,
                            surveyID = survey.surveyID,
                            json_answer = saveForm.json_answer
                        };
                        get_mon_hoc.da_khao_sat = 1;
                        break;
                }
                db.answer_response.Add(aw);
                await db.SaveChangesAsync();
                return Ok(new { success = true, message = "Khảo sát thành công" });
            }

            return BadRequest("Không thể lưu khảo sát");
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
                case "Phiếu học viên":
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

                    break;

                case "Phiếu doanh nghiệp":

                    break;

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
                        khoa = answer_responses.ctdt.khoa.ten_khoa,
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
                answer.surveyID = survey.surveyID;
                answer.id_users = survey.id_users;
                answer.id_ctdt = survey.id_ctdt;
                answer.id_sv = survey.id_sv;
                answer.json_answer = aw.json_answer;
                answer.id_donvi = survey.id_donvi;
                answer.id_CBVC = survey.id_CBVC;
                answer.id_namhoc = survey.id_namhoc;
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
                            khoa = x.ctdt.khoa.ten_khoa,
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
                            khoa = x.ctdt.khoa.ten_khoa,
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
                            khoa = x.ctdt.khoa.ten_khoa,
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
