using CTDT.Helper;
using CTDT.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;
using System.Web.Services.Description;
using System.Xml;

namespace CTDT.Controllers
{
    public class XacThucAPIController : ApiController
    {
        dbSurveyEntities db = new dbSurveyEntities();
        public users user;
        public survey survey;

        public XacThucAPIController()
        {
            user = SessionHelper.GetUser();
        }
        [HttpPost]
        [Route("api/xac_thuc")]
        public IHttpActionResult load_select_xac_thuc(survey Sv)
        {
            var user = SessionHelper.GetUser();
            var survey = db.survey.Where(x => x.surveyID == Sv.surveyID).FirstOrDefault();
            if (survey != null)
            {
                if (survey.id_loaikhaosat == 2 || survey.id_loaikhaosat == 6)
                {
                    var list_khoa = db.ctdt.Where(x => x.hedaotao.id_hedaotao == survey.id_hedaotao)
                    .Select(x => new
                    {
                        ma_khoa = x.id_khoa,
                        ten_khoa = x.khoa.ten_khoa
                    })
                    .Distinct()
                    .ToList();
                    var list_ctdt = new List<dynamic>();
                    var list_lop = new List<dynamic>();
                    var list_sinh_vien = new List<dynamic>();
                    foreach (var item in list_khoa)
                    {
                        var get_ctdt = db.ctdt.Where(x => x.id_khoa == item.ma_khoa)
                            .Select(x => new
                            {
                                ma_khoa = x.id_khoa,
                                ma_ctdt = x.id_ctdt,
                                ten_ctdt = x.ten_ctdt,
                            })
                            .ToList();
                        foreach (var l in get_ctdt)
                        {
                            var keyClassList = new JavaScriptSerializer().Deserialize<List<string>>(survey.key_class);
                            var get_lop = db.lop
                                .Where(x => x.id_ctdt == l.ma_ctdt && keyClassList.Any(k => x.ma_lop.Contains(k)))
                                .Select(x => new
                                {
                                    ma_ctdt = x.id_ctdt,
                                    ma_lop = x.id_lop,
                                    ten_lop = x.ma_lop
                                })
                                .ToList();
                            list_lop.Add(get_lop);
                            foreach (var sv in get_lop)
                            {

                                var get_sv = db.sinhvien
                                    .Where(x => x.id_lop == sv.ma_lop)
                                    .Select(x => new
                                    {
                                        ma_lop = x.id_lop,
                                        id_nguoi_hoc = x.id_sv,
                                        ma_nguoi_hoc = x.ma_sv,
                                        ten_nguoi_hoc = x.hovaten,
                                        ngay_sinh = x.ngaysinh,
                                    })
                                    .ToList();
                                list_sinh_vien.Add(get_sv);
                            }
                        }
                        list_ctdt.Add(get_ctdt);
                    }
                    var get_data = new
                    {
                        list_khoa = list_khoa,
                        list_ctdt = list_ctdt,
                        list_lop = list_lop,
                        list_nguoi_hoc = list_sinh_vien,
                        is_nguoi_hoc = true
                    };
                    return Ok(new { data = get_data });
                }
                else if (survey.id_loaikhaosat == 5)
                {
                    var list_khoa = db.ctdt.Where(x => x.hedaotao.id_hedaotao == survey.id_hedaotao)
                    .Select(x => new
                    {
                        ma_khoa = x.id_khoa,
                        ten_khoa = x.khoa.ten_khoa
                    })
                    .Distinct()
                    .ToList();
                    var list_ctdt = new List<dynamic>();
                    foreach (var item in list_khoa)
                    {
                        var get_ctdt = db.ctdt.Where(x => x.id_khoa == item.ma_khoa)
                            .Select(x => new
                            {
                                ma_khoa = x.id_khoa,
                                ma_ctdt = x.id_ctdt,
                                ten_ctdt = x.ten_ctdt,
                            })
                            .ToList();
                        list_ctdt.Add(get_ctdt);
                    }
                    var get_data = new
                    {
                        list_khoa = list_khoa,
                        list_ctdt = list_ctdt,
                        is_doanh_nghiep = true
                    };
                    return Ok(new { data = get_data });
                }
                else if (survey.id_loaikhaosat == 3)
                {
                    var list_khoa = db.ctdt.Where(x => x.hedaotao.id_hedaotao == survey.id_hedaotao)
                    .Select(x => new
                    {
                        ma_khoa = x.id_khoa,
                        ten_khoa = x.khoa.ten_khoa
                    })
                    .Distinct()
                    .ToList();
                    var list_don_vi = db.DonVi
                        .Select(x => new
                        {
                            ma_don_vi = x.id_donvi,
                            ten_don_vi = x.name_donvi,
                        })
                        .ToList();
                    var list_ctdt = new List<dynamic>();
                    foreach (var item in list_khoa)
                    {
                        var get_ctdt = db.ctdt.Where(x => x.id_khoa == item.ma_khoa)
                            .Select(x => new
                            {
                                ma_khoa = x.id_khoa,
                                ma_ctdt = x.id_ctdt,
                                ten_ctdt = x.ten_ctdt,
                            })
                            .ToList();
                        list_ctdt.Add(get_ctdt);
                    }
                    var get_data = new
                    {
                        list_khoa = list_khoa,
                        list_ctdt = list_ctdt,
                        list_don_vi = list_don_vi,
                        is_giang_vien = true
                    };
                    return Ok(new { data = get_data });
                }

                return Ok(new { data = (object)null });
            }
            else
            {
                return BadRequest("Survey not found");
            }
        }
        public void ClearSessionData()
        {
            HttpContext.Current.Session.Remove("nguoi_hoc");
            HttpContext.Current.Session.Remove("ctdt");
            HttpContext.Current.Session.Remove("don_vi");
        }
        [HttpPost]
        [Route("api/check_xac_thuc")]
        public async Task<IHttpActionResult> check_xac_thuc(survey Sv)
        {
            var survey = await db.survey.FirstOrDefaultAsync(x => x.surveyID == Sv.surveyID);
            var url = "";
            var answer_survey = await db.answer_response.FirstOrDefaultAsync(x => x.surveyID == survey.surveyID && x.id_users == user.id_users);
            // Tách chuỗi email login
            var email_String = user.email.Split('@');
            var mssv_by_email = email_String[0];
            var check_mail_student = email_String[1];
            if (answer_survey == null)
            {
                return await Is_Non_Answer_Survey(survey, mssv_by_email, check_mail_student);
            }
            // Nếu đã có câu trả lời khảo sát
            //bool check_answer_survey = db.answer_response.Any(x => x.surveyID == answer_survey.surveyID && x.id_users == user.id_users && x.id_namhoc == survey.id_namhoc && x.json_answer != null);
            //if (check_answer_survey)
            //{
            //    if (survey.id_loaikhaosat == 8)
            //    {
            //        url = "/phieu-khao-sat/dap-an/" + answer_survey.id + "/" + answer_survey.surveyID;
            //        return Ok(new { data = url, is_answer = true });
            //    }
            //    else if (survey.id_loaikhaosat == 1 || survey.id_loaikhaosat == 4)
            //    {
            //        url = "/phieu-khao-sat/dap-an/" + answer_survey.id + "/" + answer_survey.surveyID;
            //        return Ok(new { data = url, is_answer = true });
            //    }
            //    else
            //    {
            //        url = "/xac_thuc/" + Sv.surveyID;
            //        return Ok(new { data = url, is_answer = true });
            //    }
            //}
            return BadRequest("Null");
        }
        private async Task<IHttpActionResult> Is_Non_Answer_Survey(survey survey, string mssv_by_email, string check_mail_student)
        {
            bool hoc_vien_nhap_hoc = new[] { 4 }.Contains(survey.id_loaikhaosat);
            bool cuu_hoc_vien = new[] { 6 }.Contains(survey.id_loaikhaosat);
            bool hoc_vien_nhap_hoc_khong_co_thoi_gian_tot_nghiep = new[] { 9 }.Contains(survey.id_loaikhaosat);
            bool hoc_vien_co_hoc_phan_dang_hoc_tai_truong = new[] { 11 }.Contains(survey.id_loaikhaosat);
            bool hoc_vien_cuoi_khoa_co_quyet_dinh_tot_nghiep = new[] { 12 }.Contains(survey.id_loaikhaosat);
            bool giang_vien = new[] { 3 }.Contains(survey.id_loaikhaosat);
            bool can_bo_vien_chuc = new[] { 8 }.Contains(survey.id_loaikhaosat);
            var url = "";
            if (hoc_vien_nhap_hoc_khong_co_thoi_gian_tot_nghiep)
            {
                ClearSessionData();
                if(check_mail_student == "student.tdmu.edu.vn")
                {
                    var sinh_vien = await db.sinhvien.FirstOrDefaultAsync(x => x.ma_sv == mssv_by_email);
                    var isEligible = await convert_nguoi_hoc(survey.thang_nhap_hoc, sinh_vien.namnhaphoc, mssv_by_email);

                    if (isEligible != null && isEligible.namtotnghiep == null)
                    {
                        url = "/phieu-khao-sat/" + survey.surveyID;
                        return Ok(new { data = url, non_survey = true });
                    }
                }
                else
                {
                    return Ok(new { message = "Bạn không thể thực hiện khảo sát phiếu này, phiếu này dành cho người học nhập học trong khoảng " + survey.thang_nhap_hoc });
                }
            }
            else if (hoc_vien_nhap_hoc)
            {
                ClearSessionData();
                if (check_mail_student == "student.tdmu.edu.vn")
                {
                    var sinh_vien = await db.sinhvien.FirstOrDefaultAsync(x => x.ma_sv == mssv_by_email);
                    var isEligible = await convert_nguoi_hoc(survey.thang_nhap_hoc, sinh_vien.namnhaphoc, mssv_by_email);

                    if (isEligible != null)
                    {
                        url = "/phieu-khao-sat/" + survey.surveyID;
                        return Ok(new { data = url, non_survey = true });
                    }
                }
                else
                {
                    return Ok(new { message = "Bạn không thể thực hiện khảo sát phiếu này, phiếu này dành cho người học nhập học trong khoảng " + survey.thang_nhap_hoc });
                }
            }
            else if (giang_vien || can_bo_vien_chuc)
            {
                ClearSessionData();
                bool check_giang_vien = await convert_giang_vien(survey.id_namhoc);
                if (check_giang_vien)
                {
                    url = survey.id_loaikhaosat == 3 ? "/xac_thuc/" + survey.surveyID : "/phieu-khao-sat/" + survey.surveyID;
                    return Ok(new { data = url, non_survey = true });
                }
                else
                {
                    return Ok(new { message = survey.id_loaikhaosat == 3 ? "Bạn không thể thực hiện khảo sát phiếu này, phiếu này dành cho giảng viên" : "Bạn không thể thực hiện khảo sát phiếu này, phiếu này dành cho cán bộ viên chức" });
                }
            }
            else if (hoc_vien_cuoi_khoa_co_quyet_dinh_tot_nghiep)
            {
                ClearSessionData();
                if(check_mail_student == "student.tdmu.edu.vn")
                {
                    var sinh_vien = await db.sinhvien.FirstOrDefaultAsync(x => x.ma_sv == mssv_by_email);
                    var isEligible = await convert_nguoi_hoc(survey.thang_tot_nghiep, sinh_vien.namtotnghiep, mssv_by_email);
                    if (isEligible != null)
                    {
                        url = "/phieu-khao-sat/" + survey.surveyID;
                        return Ok(new { data = url, non_survey = true });
                    }
                }
                else
                {
                    return Ok(new { message = "Bạn không thể thực hiện khảo sát phiếu này, phiếu này dành cho người học nhập học có quyết định tốt nghiệp " + survey.thang_tot_nghiep });
                }
            }
            else if (hoc_vien_co_hoc_phan_dang_hoc_tai_truong)
            {
                ClearSessionData();
                if(check_mail_student == "student.tdmu.edu.vn")
                {

                }
            }
            return Ok(new { data = "/xac_thuc/" + survey.surveyID, non_survey = true });
        }
        public async Task<bool> convert_giang_vien(int? idnamhoc)
        {
            return await db.CanBoVienChuc.AnyAsync(x => x.Email == user.email && x.id_namhoc == idnamhoc);
        }
        public async Task<sinhvien> convert_nguoi_hoc(string thangstring, string namstring, string mssv)
        {
            var tach_chuoi_nam_tot_nghiep = thangstring.Split('-');
            string startDateString = "01/" + tach_chuoi_nam_tot_nghiep[0].PadLeft(2, '0');
            string endDateString = "30/" + tach_chuoi_nam_tot_nghiep[1].PadLeft(2, '0');
            DateTime startDate = DateTime.ParseExact(startDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime endDate = DateTime.ParseExact(endDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            return (await db.sinhvien.ToListAsync())
                .Where(sv =>
                {
                    if (!string.IsNullOrEmpty(namstring))
                    {
                        var formattedNamTotNghiep = namstring.Split('/');
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
                    }
                    return false;
                })
                .FirstOrDefault(sv => sv.ma_sv == mssv);
        }
        [HttpPost]
        [Route("api/get_nguoi_hoc_by_key")]
        public IHttpActionResult get_nguoi_hoc_by_key(GetNguoiHoc getNguoiHoc)
        {
            var survey = db.survey.FirstOrDefault(x => x.surveyID == getNguoiHoc.Id);
            if (survey.id_loaikhaosat == 2 || survey.id_loaikhaosat == 6)
            {
                var keyClassList = new JavaScriptSerializer().Deserialize<List<string>>(survey.key_class);
                var nguoi_hoc = db.sinhvien
                .Where(x => x.ma_sv == getNguoiHoc.ma_nguoi_hoc && keyClassList.Any(k => x.lop.ma_lop.Contains(k)))
                .Select(x => new
                {
                    ma_nguoi_hoc = x.id_sv,
                    chuong_trinh_dao_tao = x.lop.ctdt.id_ctdt,
                })
                .FirstOrDefault();
                return Ok(new { data = nguoi_hoc, success = true });
            }
            return Ok(new { success = false });
        }
        [HttpPost]
        [Route("api/save_xac_thuc")]
        public IHttpActionResult save_xac_thuc(SaveXacThuc saveXacThuc)
        {
            HttpContext.Current.Session.Remove("nguoi_hoc");
            HttpContext.Current.Session.Remove("ctdt");
            HttpContext.Current.Session.Remove("don_vi");
            HttpContext.Current.Session["nguoi_hoc"] = saveXacThuc.nguoi_hoc;
            HttpContext.Current.Session["ctdt"] = saveXacThuc.ctdt;
            HttpContext.Current.Session["don_vi"] = saveXacThuc.donvi;
            var user = SessionHelper.GetUser();
            var survey = db.survey.Where(x => x.surveyID == saveXacThuc.Id).FirstOrDefault();
            if (survey.id_loaikhaosat == 5)
            {
                var check_isAnswer = db.answer_response.Where(x =>
                    x.surveyID == survey.surveyID &&
                    x.id_ctdt == saveXacThuc.ctdt &&
                    x.id_users == user.id_users &&
                    x.id_namhoc == survey.id_namhoc &&
                    x.id_donvi == null &&
                    x.id_CBVC == null &&
                    x.id_sv == null &&
                    x.json_answer != null).FirstOrDefault();
                if (check_isAnswer != null)
                {
                    var url = "/Survey/AnswerPKS?id=" + check_isAnswer.id + "&surveyid=" + check_isAnswer.surveyID;
                    return Ok(new { is_answer = true, message = "Tài khoản này đã thực hiện khảo sát cho chương trình đào tạo này", info = url });
                }
            }
            else if (survey.id_loaikhaosat == 2 || survey.id_loaikhaosat == 6)
            {
                var check_isAnswer = db.answer_response.Where(x =>
                    x.surveyID == survey.surveyID &&
                    x.id_ctdt == saveXacThuc.ctdt &&
                    x.id_users == user.id_users &&
                    x.id_namhoc == survey.id_namhoc &&
                    x.id_donvi == null &&
                    x.id_CBVC == null &&
                    x.id_sv == saveXacThuc.nguoi_hoc &&
                    x.json_answer != null).FirstOrDefault();

                if (check_isAnswer != null)
                {
                    var url = "/Survey/AnswerPKS?id=" + check_isAnswer.id + "&surveyid=" + check_isAnswer.surveyID;
                    return Ok(new { is_answer = true, message = "Tài khoản này đã thực hiện khảo sát cho sinh viên này !", info = url });
                }
            }
            else if (survey.id_loaikhaosat == 3)
            {
                var giang_vien = db.CanBoVienChuc.Where(x => x.Email == user.email).FirstOrDefault();
                if (giang_vien != null)
                {
                    var check_isAnswer = db.answer_response.Where(x =>
                    x.surveyID == survey.surveyID &&
                    x.id_ctdt == saveXacThuc.ctdt &&
                    x.id_users == user.id_users &&
                    x.id_namhoc == survey.id_namhoc &&
                    x.id_donvi == saveXacThuc.donvi || x.id_donvi == null &&
                    x.id_CBVC == giang_vien.id_CBVC &&
                    x.id_sv == null &&
                    x.json_answer != null).FirstOrDefault();
                    if (check_isAnswer != null)
                    {
                        var url = "/Survey/AnswerPKS?id=" + check_isAnswer.id + "&surveyid=" + check_isAnswer.surveyID;
                        return Ok(new { is_answer = true, message = "Tài khoản này đã thực hiện khảo sát cho chương trình đào tạo và đơn vị này !", info = url });
                    }
                }
                else
                {
                    return Ok(new { message = "Không tìm thấy thông tin giảng viên" });
                }

            }
            return Ok(new { success = true });
        }
    }
}
