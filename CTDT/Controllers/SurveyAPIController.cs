using CTDT.Helper;
using CTDT.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
        [HttpPost]
        [Route("api/load_form_phieu_khao_sat")]
        public IHttpActionResult load_phieu_khao_sat(survey Sv)
        {
            var user = SessionHelper.GetUser();
            var domainGmail = user.email.Split('@')[1];
            var ms_nguoi_hoc = user.email.Split('@')[0];
            var get_data = db.survey.Where(x => x.surveyID == Sv.surveyID).FirstOrDefault();
            var js_data = get_data.surveyData;
            var list_thong_tin = new List<dynamic>();
            if (get_data.id_loaikhaosat == 1 || get_data.id_loaikhaosat == 4)
            {
                if (domainGmail.Equals("student.tdmu.edu.vn"))
                {
                    var nguoi_hoc = db.sinhvien.Where(x => x.ma_sv == ms_nguoi_hoc).FirstOrDefault();
                    var get_thong_tin_nguoi_hoc = new
                    {
                        email = user.email,
                        ma_so_nguoi_hoc = nguoi_hoc.ma_sv,
                        ten_nguoi_hoc = nguoi_hoc.hovaten,
                        khoa = nguoi_hoc.lop.ctdt.khoa.ten_khoa,
                        nganh_dao_tao = nguoi_hoc.lop.ctdt.ten_ctdt,
                    };
                    list_thong_tin.Add(get_thong_tin_nguoi_hoc);
                    return Ok(new { data = js_data, info = list_thong_tin, is_nguoi_hoc = true });
                }
                else
                {
                    return Ok(new { is_nguoi_hoc = false, message = "Người dùng không có quyền khảo sát phiếu khảo sát này" });
                }
            }
            else if (get_data.id_loaikhaosat == 8)
            {
                var can_bo_vien_chuc = db.CanBoVienChuc.FirstOrDefault();

                if (can_bo_vien_chuc != null && user.email == can_bo_vien_chuc.Email)
                {
                    var don_vi = can_bo_vien_chuc.DonVi != null ? can_bo_vien_chuc.DonVi.name_donvi : null;
                    var chuc_vu = can_bo_vien_chuc.ChucVu != null ? can_bo_vien_chuc.ChucVu.name_chucvu : null;
                    var ctdt = can_bo_vien_chuc.ctdt != null ? can_bo_vien_chuc.ctdt.ten_ctdt : null;

                    var get_thong_tin_can_bo_vien_chuc = new
                    {
                        email = user.email,
                        ten_cbvc = can_bo_vien_chuc.TenCBVC,
                        don_vi = don_vi,
                        chuc_vu = chuc_vu,
                        ctdt = ctdt,
                    };

                    list_thong_tin.Add(get_thong_tin_can_bo_vien_chuc);
                    return Ok(new { data = js_data, info = list_thong_tin, is_cbvc = true });
                }
                else
                {
                    return Ok(new { is_cbvc = false, message = "Người dùng không có quyền khảo sát phiếu khảo sát này" });
                }
            }
            else if (get_data.id_loaikhaosat == 2 || get_data.id_loaikhaosat == 6)
            {
                var id_nguoi_hoc = (int)HttpContext.Current.Session["nguoi_hoc"];
                var nguoi_hoc = db.sinhvien.Where(x => x.id_sv == id_nguoi_hoc).FirstOrDefault();
                var get_thong_tin_nguoi_hoc = new
                {
                    email = user.email,
                    ma_so_nguoi_hoc = nguoi_hoc.ma_sv,
                    ten_nguoi_hoc = nguoi_hoc.hovaten,
                    khoa = nguoi_hoc.lop.ctdt.khoa.ten_khoa,
                    nganh_dao_tao = nguoi_hoc.lop.ctdt.ten_ctdt,
                };
                list_thong_tin.Add(get_thong_tin_nguoi_hoc);
                return Ok(new { data = js_data, info = list_thong_tin, is_cuu_nguoi_hoc = true });
            }
            else if (get_data.id_loaikhaosat == 3)
            {
                var giang_vien = db.CanBoVienChuc.FirstOrDefault();

                if (giang_vien != null && user.email == giang_vien.Email)
                {
                    var id_don_vi = HttpContext.Current.Session["don_vi"] as int?;
                    var id_ctdt = HttpContext.Current.Session["ctdt"] as int?;

                    if (id_don_vi == null || id_ctdt == null)
                    {
                        return Ok(new { is_giang_vien = false, message = "Vui lòng xác thực để thực hiện khảo sát" });
                    }

                    var don_vi = db.DonVi.FirstOrDefault(x => x.id_donvi == id_don_vi);
                    var ctdt = db.ctdt.FirstOrDefault(x => x.id_ctdt == id_ctdt);

                    var get_thong_tin_can_bo_vien_chuc = new
                    {
                        email = user.email,
                        ten_cbvc = giang_vien.TenCBVC,
                        don_vi = don_vi != null ? don_vi.name_donvi : null,
                        chuc_vu = giang_vien.ChucVu != null ? giang_vien.ChucVu.name_chucvu : null,
                        ctdt = ctdt != null ? ctdt.ten_ctdt : null,
                    };

                    list_thong_tin.Add(get_thong_tin_can_bo_vien_chuc);
                    return Ok(new { data = js_data, info = list_thong_tin, is_giang_vien = true });
                }
                else
                {
                    return Ok(new { is_giang_vien = false, message = "Người dùng không có quyền khảo sát phiếu khảo sát này" });
                }

            }
            else if (get_data.id_loaikhaosat == 5)
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
                return Ok(new { data = (object)null });
            }
        }
        [HttpPost]
        [Route("api/save_form_khao_sat")]
        public IHttpActionResult save_form(answer_response aw)
        {
            var user = SessionHelper.GetUser();
            var domainGmail = user.email.Split('@')[1];
            var ms_nguoi_hoc = user.email.Split('@')[0];
            DateTime now = DateTime.UtcNow;
            int unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            var survey = db.survey.FirstOrDefault(x => x.surveyID == aw.surveyID);
            if (ModelState.IsValid)
            {
                if (survey.id_loaikhaosat == 1 || survey.id_loaikhaosat == 4)
                {
                    if (domainGmail.Equals("student.tdmu.edu.vn"))
                    {
                        var nguoi_hoc = db.sinhvien.Where(x => x.ma_sv == ms_nguoi_hoc).FirstOrDefault();

                        aw.time = unixTimestamp;
                        aw.id_users = user.id_users;
                        aw.id_namhoc = survey.id_namhoc;
                        aw.id_sv = nguoi_hoc.id_sv;
                        aw.id_ctdt = nguoi_hoc.lop.ctdt.id_ctdt;
                        db.answer_response.Add(aw);
                        db.SaveChanges();
                    }
                }
                else if (survey.id_loaikhaosat == 8)
                {
                    var can_bo_vien_chuc = db.CanBoVienChuc.Where(x => user.email == x.Email).FirstOrDefault();
                    if (can_bo_vien_chuc != null)
                    {
                        aw.time = unixTimestamp;
                        aw.id_users = user.id_users;
                        aw.id_namhoc = survey.id_namhoc;
                        aw.id_CBVC = can_bo_vien_chuc.id_CBVC;
                        aw.id_ctdt = can_bo_vien_chuc.ctdt?.id_ctdt ?? null;
                        aw.id_donvi = can_bo_vien_chuc.DonVi?.id_donvi ?? null;
                        db.answer_response.Add(aw);
                        db.SaveChanges();
                    }
                }
                else if (survey.id_loaikhaosat == 2 || survey.id_loaikhaosat == 6)
                {
                    var id_nguoi_hoc = (int)HttpContext.Current.Session["nguoi_hoc"];
                    var nguoi_hoc = db.sinhvien.Where(x => x.id_sv == id_nguoi_hoc).FirstOrDefault();
                    aw.time = unixTimestamp;
                    aw.id_users = user.id_users;
                    aw.id_namhoc = survey.id_namhoc;
                    aw.id_sv = nguoi_hoc.id_sv;
                    aw.id_ctdt = nguoi_hoc.lop.ctdt.id_ctdt;
                    db.answer_response.Add(aw);
                    db.SaveChanges();
                }
                else if (survey.id_loaikhaosat == 3)
                {
                    var giang_vien = db.CanBoVienChuc.FirstOrDefault(x => user.email == x.Email);
                    if (giang_vien != null)
                    {
                        var id_don_vi = HttpContext.Current.Session["don_vi"] as int?;
                        var id_ctdt = HttpContext.Current.Session["ctdt"] as int?;
                        var don_vi = db.DonVi.FirstOrDefault(x => x.id_donvi == id_don_vi);
                        var ctdt = db.ctdt.FirstOrDefault(x => x.id_ctdt == id_ctdt);

                        aw.time = unixTimestamp;
                        aw.id_users = user.id_users;
                        aw.id_namhoc = survey.id_namhoc;
                        aw.id_CBVC = giang_vien.id_CBVC;
                        aw.id_ctdt = ctdt != null ? ctdt.id_ctdt : (int?)null;
                        aw.id_donvi = don_vi != null ? don_vi.id_donvi : (int?)null;

                        db.answer_response.Add(aw);
                        db.SaveChanges();
                    }

                }
                else if (survey.id_loaikhaosat == 5)
                {
                    var id_ctdt = (int)HttpContext.Current.Session["ctdt"];
                    var ctdt = db.ctdt.Where(x => x.id_ctdt == id_ctdt).FirstOrDefault();
                    aw.time = unixTimestamp;
                    aw.id_users = user.id_users;
                    aw.id_namhoc = survey.id_namhoc;
                    aw.id_ctdt = ctdt.id_ctdt;
                    db.answer_response.Add(aw);
                    db.SaveChanges();
                }
            }
            return Ok(new { success = true, message = "Thêm mới dữ liệu thành công" });
        }
        [HttpPost]
        [Route("api/load_dap_an_pks")]

        public IHttpActionResult load_answer_phieu_khao_sat(LoadAnswerPKS loadAnswerPKS)
        {
            var answer_responses = db.answer_response
                .Where(x => x.id == loadAnswerPKS.id_answer && x.surveyID == loadAnswerPKS.id_survey)
                .FirstOrDefault();
            var get_data = new
            {
                phieu_khao_sat = answer_responses.survey.surveyData,
                dap_an = answer_responses.json_answer,
            };
            return Ok(new { data = get_data, success = true });
        }
        [HttpPost]
        [Route("api/save_answer_form")]
        public IHttpActionResult save_answer_form(answer_response aw)
        {
            var user = SessionHelper.GetUser();
            var status = "";
            var answer = db.answer_response.Find(aw.id);
            var survey = db.answer_response.FirstOrDefault(x => x.id == answer.id);
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
                db.SaveChanges();
                status = "Cập nhật dữ liệu thành công";
            }
            else
            {
                status = "Cập nhật dữ liệu thất bại";
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
                bool isStudent = new[] { 1, 2, 4, 6 }.Contains(item.id_loaikhaosat);
                bool isCTDT = new[] { 5 }.Contains(item.id_loaikhaosat);
                bool isCBVC = new[] { 3, 8 }.Contains(item.id_loaikhaosat);
                var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                if (isStudent)
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
                else if (isCTDT)
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
                else if (isCBVC)
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
