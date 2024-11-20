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
        public async Task<IHttpActionResult> load_phieu_khao_sat(survey Sv)
        {
            var domainGmail = user.email.Split('@')[1];
            var ms_nguoi_hoc = user.email.Split('@')[0];
            var get_data = await db.survey.Where(x => x.surveyID == Sv.surveyID).FirstOrDefaultAsync();
            var js_data = get_data.surveyData;
            var list_thong_tin = new List<dynamic>();
            if (get_data.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu học viên")
            {
                if (domainGmail.Equals("student.tdmu.edu.vn"))
                {
                    var id_nguoi_hoc = (int?)HttpContext.Current.Session["nguoi_hoc"];
                    var tach_chuoi_nam_tot_nghiep = get_data.thang_tot_nghiep != null ? get_data.thang_tot_nghiep.Split('-') : get_data.thang_nhap_hoc.Split('-');
                    string startDateString = "01/" + tach_chuoi_nam_tot_nghiep[0].PadLeft(2, '0');
                    string endDateString = "30/" + tach_chuoi_nam_tot_nghiep[1].PadLeft(2, '0');
                    DateTime startDate = DateTime.ParseExact(startDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    DateTime endDate = DateTime.ParseExact(endDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    var nguoi_hoc = (await db.sinhvien.Where(x => id_nguoi_hoc != null ? x.id_sv == id_nguoi_hoc : x.ma_sv == ms_nguoi_hoc).ToListAsync())
                            .Where(sv =>
                            {
                                var formattedNamTotNghiep = get_data.thang_tot_nghiep != null ? sv.namtotnghiep.Split('/') : sv.namnhaphoc.Split('/');
                                if (formattedNamTotNghiep.Length == 2)
                                {
                                    string formattedDate = "01/" + formattedNamTotNghiep[0].PadLeft(2, '0') + "/" + formattedNamTotNghiep[1];

                                    return DateTime.TryParseExact(
                                               formattedDate,
                                               "dd/MM/yyyy",
                                               CultureInfo.InvariantCulture,
                                               DateTimeStyles.None,
                                               out DateTime namtotnghiepDate) &&
                                           namtotnghiepDate >= startDate &&
                                           namtotnghiepDate <= endDate;
                                }
                                return false;
                            })
                            .Select(x => new
                            {
                                email = user.email,
                                ma_so_nguoi_hoc = x.ma_sv,
                                ten_nguoi_hoc = x.hovaten,
                                khoa = x.lop.ctdt.khoa.ten_khoa,
                                nganh_dao_tao = x.lop.ctdt.ten_ctdt,
                            })
                            .FirstOrDefault(x => x.ma_so_nguoi_hoc == ms_nguoi_hoc);
                    list_thong_tin.Add(nguoi_hoc);
                    return Ok(new { data = js_data, info = list_thong_tin, is_nguoi_hoc = true });
                }
                else
                {
                    return Ok(new { is_nguoi_hoc = false, message = "Người dùng không có quyền khảo sát phiếu khảo sát này" });
                }
            }
            else if (get_data.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu giảng viên")
            {
                var can_bo_vien_chuc_obj = db.CanBoVienChuc.FirstOrDefault(x => x.Email == user.email);
                var append_list_cbvc = new List<dynamic>();
                if (can_bo_vien_chuc_obj != null)
                {
                    var id_don_vi = HttpContext.Current.Session["don_vi"] as int?;
                    var id_ctdt = HttpContext.Current.Session["ctdt"] as int?;
                    var don_vi = can_bo_vien_chuc_obj.DonVi != null ? can_bo_vien_chuc_obj.DonVi.name_donvi : null;
                    var chuc_vu = can_bo_vien_chuc_obj.ChucVu != null ? can_bo_vien_chuc_obj.ChucVu.name_chucvu : null;
                    var ctdt = can_bo_vien_chuc_obj.ctdt != null ? can_bo_vien_chuc_obj.ctdt.ten_ctdt : null;
                    if (id_don_vi != null || id_ctdt != null)
                    {
                        var don_vi_select = db.DonVi.FirstOrDefault(x => x.id_donvi == id_don_vi);
                        var ctdt_select = db.ctdt.FirstOrDefault(x => x.id_ctdt == id_ctdt);
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
                            ctdt = ctdt,
                        });
                    }
                    return Ok(new { data = js_data, info = append_list_cbvc, is_cbvc = true });
                }
                else
                {
                    return Ok(new { is_cbvc = false, message = "Người dùng không có quyền khảo sát phiếu khảo sát này" });
                }

            }
            else if (get_data.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu doanh nghiệp")
            {
                var id_ctdt = (int)HttpContext.Current.Session["ctdt"];
                var ctdt = db.ctdt.Where(x => x.id_ctdt == id_ctdt).FirstOrDefault();
                var get_thong_tin_doanh_nghiep = new
                {
                    email = user.email,
                    ctdt = ctdt.ten_ctdt,
                };
                list_thong_tin.Add(get_thong_tin_doanh_nghiep);
                return Ok(new { data = js_data, info = list_thong_tin, is_doanh_nghiep = true });
            }
            else
            {
                return Ok(new { message = "Vui lòng xác thực để thực hiện khảo sát" });
            }
        }
        [HttpPost]
        [Route("api/save_form_khao_sat")]
        public async Task<IHttpActionResult> save_form(answer_response aw)
        {
            var user = SessionHelper.GetUser();
            var domainGmail = user.email.Split('@')[1];
            var ms_nguoi_hoc = user.email.Split('@')[0];
            DateTime now = DateTime.UtcNow;
            int unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            var survey = await db.survey.FirstOrDefaultAsync(x => x.surveyID == aw.surveyID);
            if (ModelState.IsValid)
            {
                if (survey.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu học viên")
                {
                    var id_nguoi_hoc = (int?)HttpContext.Current.Session["nguoi_hoc"];
                    var nguoi_hoc = await db.sinhvien.FirstOrDefaultAsync(x => id_nguoi_hoc != null ? x.id_sv == id_nguoi_hoc : x.ma_sv == ms_nguoi_hoc);
                    aw.time = unixTimestamp;
                    aw.id_users = user.id_users;
                    aw.id_namhoc = survey.id_namhoc;
                    aw.id_sv = nguoi_hoc.id_sv;
                    aw.id_ctdt = nguoi_hoc.lop.ctdt.id_ctdt;
                    db.answer_response.Add(aw);
                }
                else if (survey.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu giảng viên")
                {
                    var can_bo_vien_chuc = await db.CanBoVienChuc.FirstOrDefaultAsync(x => user.email == x.Email);
                    var id_don_vi = HttpContext.Current.Session["don_vi"] as int?;
                    var id_ctdt = HttpContext.Current.Session["ctdt"] as int?;
                    if (id_don_vi != null || id_ctdt != null)
                    {
                        var don_vi_select = await db.DonVi.FirstOrDefaultAsync(x => x.id_donvi == id_don_vi);
                        var ctdt_select = await db.ctdt.FirstOrDefaultAsync(x => x.id_ctdt == id_ctdt);

                        aw.time = unixTimestamp;
                        aw.id_users = user.id_users;
                        aw.id_namhoc = survey.id_namhoc;
                        aw.id_CBVC = can_bo_vien_chuc.id_CBVC;
                        aw.id_ctdt = ctdt_select != null ? ctdt_select.id_ctdt : (int?)null;
                        aw.id_donvi = don_vi_select != null ? don_vi_select.id_donvi : (int?)null;
                        db.answer_response.Add(aw);
                    }
                    else
                    {
                        aw.time = unixTimestamp;
                        aw.id_users = user.id_users;
                        aw.id_namhoc = survey.id_namhoc;
                        aw.id_CBVC = can_bo_vien_chuc.id_CBVC;
                        aw.id_ctdt = can_bo_vien_chuc.ctdt?.id_ctdt ?? null;
                        aw.id_donvi = can_bo_vien_chuc.DonVi?.id_donvi ?? null;
                        db.answer_response.Add(aw);
                    }
                }
                else if (survey.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu doanh nghiệp")
                {
                    var id_ctdt = (int)HttpContext.Current.Session["ctdt"];
                    var ctdt = await db.ctdt.FirstOrDefaultAsync(x => x.id_ctdt == id_ctdt);
                    aw.time = unixTimestamp;
                    aw.id_users = user.id_users;
                    aw.id_namhoc = survey.id_namhoc;
                    aw.id_ctdt = ctdt.id_ctdt;
                    db.answer_response.Add(aw);
                }
                await db.SaveChangesAsync();

            }
            return Ok(new { success = true, message = "Khảo sát thành công" });
        }

        [HttpPost]
        [Route("api/load_dap_an_pks")]
        public async Task<IHttpActionResult> load_answer_phieu_khao_sat(LoadAnswerPKS loadAnswerPKS)
        {
            var answer_responses = await db.answer_response
                .FirstOrDefaultAsync(x => x.id == loadAnswerPKS.id_answer && x.surveyID == loadAnswerPKS.id_survey);
            var get_data = new
            {
                phieu_khao_sat = answer_responses.survey.surveyData,
                dap_an = answer_responses.json_answer,
            };
            return Ok(new { data = get_data, success = true });
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
