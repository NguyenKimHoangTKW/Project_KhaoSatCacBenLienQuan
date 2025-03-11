using CTDT.Helper;
using CTDT.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
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
        private users user;
        private int unixTimestamp;

        public SurveyAPIController()
        {
            user = SessionHelper.GetUser();
            DateTime now = DateTime.UtcNow;
            unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
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
                if (get_data.id_loaikhaosat == 3)
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
            else if (get_data.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu người học" || get_data.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu cựu người học")
            {
                var get_value = (int)HttpContext.Current.Session["id_nghoc_ks"];
                var nguoi_hoc_khao_sat = await db.nguoi_hoc_khao_sat
                    .Where(x => x.id_nguoi_hoc_khao_sat == get_value)
                    .Select(x => new
                    {
                        ma_nh = x.sinhvien.ma_sv,
                        ten_nh = x.sinhvien.hovaten,
                        thuoc_lop = x.sinhvien.lop.ma_lop,
                        thuoc_ctdt = x.sinhvien.lop.ctdt.ten_ctdt
                    })
                    .FirstOrDefaultAsync();
                return Ok(new { data = js_data, info = JsonConvert.SerializeObject(nguoi_hoc_khao_sat), success = true, is_nh = true });
            }
            else if (get_data.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu người học có học phần")
            {
                var get_value = (int)HttpContext.Current.Session["id_nh_co_hp_ks"];
                var nguoi_hoc_co_hoc_phan = await db.nguoi_hoc_dang_co_hoc_phan
                    .Where(x => x.id_nguoi_hoc_by_hoc_phan == get_value)
                    .Select(x => new
                    {
                        ma_nh = x.sinhvien.ma_sv,
                        ten_nh = x.sinhvien.hovaten,
                        hoc_phan = x.mon_hoc.hoc_phan.ten_hoc_phan,
                        ma_mh = x.mon_hoc.ma_mon_hoc,
                        mon_hoc = x.mon_hoc.ten_mon_hoc,
                        lop = x.mon_hoc.id_lop != null ? x.mon_hoc.lop.ma_lop : "",
                        giang_vien_giang_day = x.CanBoVienChuc.TenCBVC
                    })
                    .FirstOrDefaultAsync();
                return Ok(new { data = js_data, info = JsonConvert.SerializeObject(nguoi_hoc_co_hoc_phan), success = true, is_nh_co_hp = true });
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
                return Ok(new { message = "Biểu mẫu không tồn tại", success = false });
            }

            if (ModelState.IsValid)
            {
                if (survey.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu giảng viên")
                {
                    if (survey.id_loaikhaosat == 3)
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
                        cbvc.is_khao_sat = 1;
                    }
                }
                else if (survey.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu người học" || survey.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu cựu người học")
                {
                    var get_value = (int)HttpContext.Current.Session["id_nghoc_ks"];
                    var nguoi_hoc = await db.nguoi_hoc_khao_sat
                        .FirstOrDefaultAsync(x => x.surveyID == saveForm.idsurvey && x.id_nguoi_hoc_khao_sat == get_value);
                    aw = new answer_response()
                    {
                        time = unixTimestamp,
                        id_users = user.id_users,
                        id_namhoc = survey.id_namhoc,
                        id_nguoi_hoc_khao_sat = nguoi_hoc.id_nguoi_hoc_khao_sat,
                        surveyID = survey.surveyID,
                        json_answer = saveForm.json_answer
                    };
                    nguoi_hoc.is_khao_sat = 1;
                }
                else if (survey.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu người học có học phần")
                {
                    var get_value = (int)HttpContext.Current.Session["id_nh_co_hp_ks"];
                    var get_nhchp = await db.nguoi_hoc_dang_co_hoc_phan
                        .FirstOrDefaultAsync(x => x.surveyID == saveForm.idsurvey && x.id_nguoi_hoc_by_hoc_phan == get_value);
                    aw = new answer_response()
                    {
                        time = unixTimestamp,
                        id_users = user.id_users,
                        id_namhoc = survey.id_namhoc,
                        id_nguoi_hoc_co_hp_khao_sat = get_nhchp.id_nguoi_hoc_by_hoc_phan,
                        surveyID = survey.surveyID,
                        json_answer = saveForm.json_answer
                    };
                    get_nhchp.da_khao_sat = 1;
                }
                db.answer_response.Add(aw);
                await db.SaveChangesAsync();
                HttpContext.Current.Session.Remove("id_nghoc_ks");
                HttpContext.Current.Session.Remove("id_cbvc_ks");
                HttpContext.Current.Session.Remove("id_cbvc_ks_ctdt");
                HttpContext.Current.Session.Remove("id_nh_co_hp_ks");
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
            if (answer_responses.survey.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu người học" || answer_responses.survey.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu cựu người học")
            {
                var data_hoc_vien = new
                {
                    phieu_khao_sat = answer_responses.survey.surveyData,
                    dap_an = answer_responses.json_answer
                };
                list_info.Add(new
                {
                    ten_nh = answer_responses.nguoi_hoc_khao_sat.sinhvien.hovaten,
                    ma_nh = answer_responses.nguoi_hoc_khao_sat.sinhvien.ma_sv,
                    thuoc_lop = answer_responses.nguoi_hoc_khao_sat.sinhvien.lop.ma_lop,
                    thuoc_ctdt = answer_responses.nguoi_hoc_khao_sat.sinhvien.lop.ctdt.ten_ctdt,
                    khao_sat_lan_cuoi = answer_responses.time
                });
                return Ok(new { data = data_hoc_vien, info = JsonConvert.SerializeObject(list_info), success = true, is_nh = true });
            }
            else if (answer_responses.survey.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu giảng viên")
            {
                var data_cbvc = new
                {
                    phieu_khao_sat = answer_responses.survey.surveyData,
                    dap_an = answer_responses.json_answer
                };
                if (answer_responses.survey.id_loaikhaosat == 3)
                {
                    list_info.Add(new
                    {
                        MaCBVC = answer_responses.cbvc_khao_sat.CanBoVienChuc.MaCBVC,
                        TenCBVC = answer_responses.cbvc_khao_sat.CanBoVienChuc.TenCBVC,
                        ten_trinh_do = answer_responses.cbvc_khao_sat.CanBoVienChuc.trinh_do.ten_trinh_do,
                        name_chucvu = answer_responses.cbvc_khao_sat.CanBoVienChuc.ChucVu.name_chucvu,
                        ten_khoa = answer_responses.cbvc_khao_sat.CanBoVienChuc.khoa_vien_truong.ten_khoa,
                        nganh_dao_tao = answer_responses.cbvc_khao_sat.CanBoVienChuc.nganh_dao_tao,
                        khao_sat_cho = answer_responses.ctdt.ten_ctdt,
                        khao_sat_lan_cuoi = answer_responses.time
                    });
                    return Ok(new { data = data_cbvc, info = JsonConvert.SerializeObject(list_info), success = true, is_gv = true });
                }
                else
                {
                    list_info.Add(new
                    {
                        MaCBVC = answer_responses.cbvc_khao_sat.CanBoVienChuc.MaCBVC,
                        TenCBVC = answer_responses.cbvc_khao_sat.CanBoVienChuc.TenCBVC,
                        ten_trinh_do = answer_responses.cbvc_khao_sat.CanBoVienChuc.trinh_do.ten_trinh_do,
                        name_chucvu = answer_responses.cbvc_khao_sat.CanBoVienChuc.ChucVu.name_chucvu,
                        ten_khoa = answer_responses.cbvc_khao_sat.CanBoVienChuc.khoa_vien_truong.ten_khoa,
                        nganh_dao_tao = answer_responses.cbvc_khao_sat.CanBoVienChuc.nganh_dao_tao,
                        khao_sat_lan_cuoi = answer_responses.time
                    });
                    return Ok(new { data = data_cbvc, info = JsonConvert.SerializeObject(list_info), success = true, is_cbvc = true });
                }
            }
            else if (answer_responses.survey.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu doanh nghiệp")
            {
                var data_doanh_nghiep = new
                {
                    phieu_khao_sat = answer_responses.survey.surveyData,
                    dap_an = answer_responses.json_answer
                };
                list_info.Add(new
                {
                    user_ks = answer_responses.users.firstName + " " + answer_responses.users.lastName,
                    email_ks = answer_responses.email,
                    khao_sat_cho = answer_responses.ctdt.ten_ctdt,
                    khao_sat_lan_cuoi = answer_responses.time
                });
                return Ok(new { data = data_doanh_nghiep, info = JsonConvert.SerializeObject(list_info), success = true, is_dn = true });
            }
            else if (answer_responses.survey.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu người học có học phần")
            {
                var data_ho_vien_mon_hoc = new
                {
                    phieu_khao_sat = answer_responses.survey.surveyData,
                    dap_an = answer_responses.json_answer
                };
                list_info.Add(new
                {
                    ma_nh = answer_responses.nguoi_hoc_dang_co_hoc_phan.sinhvien.ma_sv,
                    ten_nh = answer_responses.nguoi_hoc_dang_co_hoc_phan.sinhvien.hovaten,
                    hoc_phan = answer_responses.nguoi_hoc_dang_co_hoc_phan.mon_hoc.hoc_phan.ten_hoc_phan,
                    ma_mh = answer_responses.nguoi_hoc_dang_co_hoc_phan.mon_hoc.ma_mon_hoc,
                    mon_hoc = answer_responses.nguoi_hoc_dang_co_hoc_phan.mon_hoc.ten_mon_hoc,
                    lop = answer_responses.nguoi_hoc_dang_co_hoc_phan.mon_hoc.id_lop != null ? answer_responses.nguoi_hoc_dang_co_hoc_phan.mon_hoc.lop.ma_lop : "",
                    giang_vien_giang_day = answer_responses.nguoi_hoc_dang_co_hoc_phan.CanBoVienChuc.TenCBVC,
                    khao_sat_lan_cuoi = answer_responses.time
                });
                return Ok(new { data = data_ho_vien_mon_hoc, info = JsonConvert.SerializeObject(list_info), success = true, is_hoc_phan_nguoi_hoc = true });
            }
            return Ok(new { message = "Không thể xác thực, vui lòng thử lại sau", success = false });
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
                if (item.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu người học")
                {
                    bo_phieu.AddRange(query
                        .AsEnumerable()
                        .Select(x => new
                        {
                            email = user.email,
                            ma_nh = x.id_nguoi_hoc_khao_sat != null ? x.nguoi_hoc_khao_sat.sinhvien.ma_sv : "",
                            ten_nh = x.id_nguoi_hoc_khao_sat != null ? x.nguoi_hoc_khao_sat.sinhvien.hovaten:"",
                            thuoc_lop = x.id_nguoi_hoc_khao_sat != null ? x.nguoi_hoc_khao_sat.sinhvien.lop.ma_lop:"",
                            thuoc_ctdt = x.id_nguoi_hoc_khao_sat != null ? x.nguoi_hoc_khao_sat.sinhvien.lop.ctdt.ten_ctdt:"",
                            thoi_gian_khao_sat = x.time,
                            page = unixTimestamp > item.surveyTimeEnd ? "Ngoài thời gian thực hiện khảo sát" : "Chỉnh sửa lại câu trả lời",
                            value_page = unixTimestamp > item.surveyTimeEnd ? "javascript:void(0)" : $"/phieu-khao-sat/dap-an/{x.id}/{x.surveyID}"
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
                        .AsEnumerable()
                        .Select(x => new
                        {
                            email = user.email,
                            ctdt = x.ctdt.ten_ctdt,
                            nam_hoc = x.NamHoc.ten_namhoc,
                            thoi_gian_khao_sat = x.time,
                            page = unixTimestamp > item.surveyTimeEnd ? "Ngoài thời gian thực hiện khảo sát" : $"/phieu-khao-sat/dap-an/{x.id}/{x.surveyID}"
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
                    if (item.id_loaikhaosat == 8)
                    {
                        bo_phieu.AddRange(query
                        .AsEnumerable()
                        .Select(x => new
                        {
                            email = user.email,
                            MaCBVC = x.id_cbvc_khao_sat != null ? x.cbvc_khao_sat.CanBoVienChuc.MaCBVC : "",
                            TenCBVC = x.id_cbvc_khao_sat != null ? x.cbvc_khao_sat.CanBoVienChuc.TenCBVC : "",
                            ten_trinh_do = x.id_cbvc_khao_sat != null ? x.cbvc_khao_sat.CanBoVienChuc.trinh_do?.ten_trinh_do : "",
                            name_chucvu = x.id_cbvc_khao_sat != null ? x.cbvc_khao_sat.CanBoVienChuc.ChucVu?.name_chucvu : "",
                            ten_khoa = x.id_cbvc_khao_sat != null ? x.cbvc_khao_sat.CanBoVienChuc.khoa_vien_truong?.ten_khoa : "",
                            nganh_dao_tao = x.id_cbvc_khao_sat != null ? x.cbvc_khao_sat.CanBoVienChuc.nganh_dao_tao : "",
                            thoi_gian_khao_sat = x.time,
                            page = unixTimestamp > item.surveyTimeEnd ? "Ngoài thời gian thực hiện khảo sát" : "Chỉnh sửa lại câu trả lời",
                            value_page = unixTimestamp > item.surveyTimeEnd ? "javascript:void(0)" : $"/phieu-khao-sat/dap-an/{x.id}/{x.surveyID}"
                        }).ToList());

                        surveyList.Add(new
                        {
                            ten_phieu = item.surveyTitle,
                            bo_phieu = bo_phieu,
                            is_cbvc = true
                        });
                    }
                    else if (item.id_loaikhaosat == 3)
                    {
                        bo_phieu.AddRange(query
                        .AsEnumerable()
                        .Select(x => new
                        {
                            email = user.email,
                            MaCBVC = x.id_cbvc_khao_sat != null ? x.cbvc_khao_sat.CanBoVienChuc.MaCBVC : "",
                            TenCBVC = x.id_cbvc_khao_sat != null ? x.cbvc_khao_sat.CanBoVienChuc.TenCBVC : "",
                            ten_trinh_do = x.id_cbvc_khao_sat != null ? x.cbvc_khao_sat.CanBoVienChuc.trinh_do?.ten_trinh_do : "",
                            name_chucvu = x.id_cbvc_khao_sat != null ? x.cbvc_khao_sat.CanBoVienChuc.ChucVu?.name_chucvu : "",
                            ten_khoa = x.id_cbvc_khao_sat != null ? x.cbvc_khao_sat.CanBoVienChuc.khoa_vien_truong?.ten_khoa : "",
                            nganh_dao_tao = x.id_cbvc_khao_sat != null ? x.cbvc_khao_sat.CanBoVienChuc.nganh_dao_tao : "",
                            khao_sat_cho = x.ctdt?.ten_ctdt,
                            thoi_gian_khao_sat = x.time,
                            page = unixTimestamp > item.surveyTimeEnd ? "Ngoài thời gian thực hiện khảo sát" : "Chỉnh sửa lại câu trả lời",
                            value_page = unixTimestamp > item.surveyTimeEnd ? "javascript:void(0)" : $"/phieu-khao-sat/dap-an/{x.id}/{x.surveyID}"
                        }).ToList());

                        surveyList.Add(new
                        {
                            ten_phieu = item.surveyTitle,
                            bo_phieu = bo_phieu,
                            is_gv = true
                        });
                    }
                }
                else if (item.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu người học có học phần")
                {
                    bo_phieu.AddRange(query
                        .AsEnumerable()
                        .Select(x => new
                        {
                            email = user.email,
                            ma_nh = x.id_nguoi_hoc_co_hp_khao_sat != null ? x.nguoi_hoc_dang_co_hoc_phan.sinhvien.ma_sv : "",
                            ten_nh = x.id_nguoi_hoc_co_hp_khao_sat != null ? x.nguoi_hoc_dang_co_hoc_phan.sinhvien.hovaten : "",
                            hoc_phan = x.id_nguoi_hoc_co_hp_khao_sat != null ? x.nguoi_hoc_dang_co_hoc_phan.mon_hoc.hoc_phan.ten_hoc_phan : "",
                            ma_mh = x.id_nguoi_hoc_co_hp_khao_sat != null ? x.nguoi_hoc_dang_co_hoc_phan.mon_hoc.ma_mon_hoc : "",
                            mon_hoc = x.id_nguoi_hoc_co_hp_khao_sat != null ? x.nguoi_hoc_dang_co_hoc_phan.mon_hoc.ten_mon_hoc : "",
                            lop = (x.id_nguoi_hoc_co_hp_khao_sat != null && x.nguoi_hoc_dang_co_hoc_phan?.mon_hoc?.id_lop != null) ? x.nguoi_hoc_dang_co_hoc_phan.mon_hoc.lop?.ma_lop ?? "" : "",
                            giang_vien_giang_day = x.id_nguoi_hoc_co_hp_khao_sat != null ? x.nguoi_hoc_dang_co_hoc_phan.CanBoVienChuc.TenCBVC : "",
                            thoi_gian_khao_sat = x.time,
                            page = unixTimestamp > item.surveyTimeEnd ? "Ngoài thời gian thực hiện khảo sát" : "Chỉnh sửa lại câu trả lời",
                            value_page = unixTimestamp > item.surveyTimeEnd ? "javascript:void(0)" : $"/phieu-khao-sat/dap-an/{x.id}/{x.surveyID}"
                        }).ToList());

                    surveyList.Add(new
                    {
                        ten_phieu = item.surveyTitle,
                        bo_phieu = bo_phieu,
                        is_nh_hp = true
                    });
                }
            }
            return Ok(new { data = new { survey = surveyList } });
        }

    }
}
