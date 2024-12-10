using CTDT.Helper;
using CTDT.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
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
        public async Task<IHttpActionResult> load_select_xac_thuc(survey Sv)
        {
            var survey = await db.survey.FirstOrDefaultAsync(x => x.surveyID == Sv.surveyID);
            if (survey != null)
            {
                if (survey.id_loaikhaosat == 2 || survey.id_loaikhaosat == 6)
                {
                    var list_khoa = await db.ctdt.Where(x => x.hedaotao.id_hedaotao == survey.id_hedaotao)
                    .Select(x => new
                    {
                        ma_khoa = x.id_khoa,
                        ten_khoa = x.khoa.ten_khoa
                    })
                    .Distinct()
                    .ToListAsync();
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
                            var get_lop = await db.lop
                                .Where(x => x.id_ctdt == l.ma_ctdt)
                                .Select(x => new
                                {
                                    ma_ctdt = x.id_ctdt,
                                    ma_lop = x.id_lop,
                                    ten_lop = x.ma_lop
                                })
                                .ToListAsync();
                            list_lop.Add(get_lop);
                            foreach (var sv in get_lop)
                            {
                                var tach_chuoi_nam_tot_nghiep = survey.thang_tot_nghiep.Split('-');
                                string startDateString = "01/" + tach_chuoi_nam_tot_nghiep[0].PadLeft(7, '0');
                                string endDateString = "30/" + tach_chuoi_nam_tot_nghiep[1].PadLeft(7, '0');
                                DateTime startDate = DateTime.ParseExact(startDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                                DateTime endDate = DateTime.ParseExact(endDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);
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
                    var list_khoa = await db.ctdt.Where(x => x.hedaotao.id_hedaotao == survey.id_hedaotao)
                    .Select(x => new
                    {
                        ma_khoa = x.id_khoa,
                        ten_khoa = x.khoa.ten_khoa
                    })
                    .Distinct()
                    .ToListAsync();
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

        [HttpPost]
        [Route("api/check_xac_thuc")]
        public async Task<IHttpActionResult> check_xac_thuc(survey Sv)
        {
            // Tách chuỗi email login
            var email_String = user.email.Split('@');
            var mssv_by_email = email_String[0];
            var check_mail_student = email_String[1];
            // Check
            var survey = await db.survey.FirstOrDefaultAsync(x => x.surveyID == Sv.surveyID);
            var answer_survey = await db.answer_response.FirstOrDefaultAsync(x => x.surveyID == Sv.surveyID && x.id_users == user.id_users);

            // Nếu đã có câu trả lời khảo sát
            if (answer_survey == null)
            {
                return await Is_Non_Answer_Survey(survey, mssv_by_email, check_mail_student);
            }
            bool check_answer_survey = db.answer_response.Any(x => x.surveyID == answer_survey.surveyID && x.id_users == user.id_users && x.id_namhoc == survey.id_namhoc && x.json_answer != null);
            if (check_answer_survey)
            {
                return await Is_Answer_Survey(survey, answer_survey, mssv_by_email, check_mail_student);
            }
            return BadRequest("Không thể gửi yêu cầu");
        }
        private async Task<IHttpActionResult> Is_Answer_Survey(survey survey, answer_response answer_survey, string mssv_by_email, string check_mail_student)
        {
            var url = "";
            var check_group_loaikhaosat = await db.LoaiKhaoSat.FirstOrDefaultAsync(x => x.id_loaikhaosat == survey.id_loaikhaosat);
            // Check phiếu thuộc học viên
            if (check_group_loaikhaosat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu học viên")
            {
                bool hoc_vien_nhap_hoc = new[] { 4 }.Contains(survey.id_loaikhaosat);
                bool cuu_hoc_vien = new[] { 6 }.Contains(survey.id_loaikhaosat);
                bool hoc_vien_nhap_hoc_khong_co_thoi_gian_tot_nghiep = new[] { 9 }.Contains(survey.id_loaikhaosat);
                bool hoc_vien_cuoi_khoa_co_quyet_dinh_tot_nghiep = new[] { 12 }.Contains(survey.id_loaikhaosat);
                if (cuu_hoc_vien)
                {
                    url = "/xac_thuc/" + survey.surveyID;
                    return Ok(new { data = url, non_survey = true });
                }
                if (check_mail_student == "student.tdmu.edu.vn")
                {
                    if (hoc_vien_nhap_hoc_khong_co_thoi_gian_tot_nghiep || hoc_vien_nhap_hoc || hoc_vien_cuoi_khoa_co_quyet_dinh_tot_nghiep)
                    {
                        url = "/phieu-khao-sat/dap-an/" + answer_survey.id + "/" + answer_survey.surveyID;
                        return Ok(new { data = url, is_answer = true });
                    }
                }
                else
                {
                    return Ok(new { message = "Bạn đang đăng nhập Email cá nhân, vui lòng đăng nhập lại bằng Email người học để thực hiện khảo sát" });
                }


            }

            // Check phiếu thuộc giảng viên
            else if (check_group_loaikhaosat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu giảng viên")
            {
                bool check_mail_cbvc = await db.CanBoVienChuc.AnyAsync(x => x.Email == user.email);
                if (check_mail_cbvc)
                {
                    url = survey.id_loaikhaosat == 3 ? "/xac_thuc/" + survey.surveyID : "/phieu-khao-sat/dap-an/" + answer_survey.id + "/" + answer_survey.surveyID;
                    return Ok(new { data = url, non_survey = true });                    
                }
                else
                {
                    return Ok(new { message = survey.id_loaikhaosat == 3 ? "Bạn không thể thực hiện khảo sát phiếu này, phiếu này dành cho giảng viên" : "Bạn không thể thực hiện khảo sát phiếu này, phiếu này dành cho cán bộ viên chức" });
                }

            }
            else if (check_group_loaikhaosat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu doanh nghiệp")
            {
                bool doanh_nghiep = new[] { 5 }.Contains(survey.id_loaikhaosat);
                if (doanh_nghiep)
                {
                    url = "/xac_thuc/" + survey.surveyID;
                    return Ok(new { data = url, non_survey = true });
                }
            }
            else if (check_group_loaikhaosat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu người học có học phần")
            {
                HttpContext.Current.Session["Surveyid"] = survey.surveyID;

                if (check_mail_student == "student.tdmu.edu.vn")
                {
                    bool hoc_vien_co_hoc_phan_ly_thuyet_dang_hoc_tai_truong = new[] { 11 }.Contains(survey.id_loaikhaosat);
                    bool hoc_vien_co_hoc_phan_tot_nghiep = new[] { 13 }.Contains(survey.id_loaikhaosat);
                    // Tách chuỗi học phần survey
                    var check_hoc_vien_co_hoc_phan = await db.nguoi_hoc_dang_co_hoc_phan?.Where(x => x.sinhvien.ma_sv == mssv_by_email).ToListAsync();
                    var tach_chuoi_hoc_phan_survey = survey.hoc_phan.Split('-');
                    string startDateString = "01/" + tach_chuoi_hoc_phan_survey[0].PadLeft(7, '0');
                    string endDateString = "30/" + tach_chuoi_hoc_phan_survey[1].PadLeft(7, '0');
                    DateTime surveyStartDate = DateTime.ParseExact(startDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    DateTime surveyEndDate = DateTime.ParseExact(endDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    // Chạy vòng lặp kiểm tra
                    if (check_hoc_vien_co_hoc_phan.Any())
                    {
                        foreach (var items in check_hoc_vien_co_hoc_phan)
                        {
                            var tach_chuoi_hoc_phan_mon_hoc = items.thang_by_hoc_phan.Split('-');
                            string hocPhanStartDateString = "01/" + tach_chuoi_hoc_phan_mon_hoc[0].PadLeft(7, '0');
                            string hocPhanEndDateString = "30/" + tach_chuoi_hoc_phan_mon_hoc[1].PadLeft(7, '0');
                            DateTime hocPhanStartDate = DateTime.ParseExact(hocPhanStartDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                            DateTime hocPhanEndDate = DateTime.ParseExact(hocPhanEndDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                            if (hoc_vien_co_hoc_phan_ly_thuyet_dang_hoc_tai_truong)
                            {
                                if (check_hoc_vien_co_hoc_phan != null
                                    && items.mon_hoc.hoc_phan.ten_hoc_phan != "Báo cáo tốt nghiệp"
                                    // Kiểm tra học phần nằm trong khoảng survey
                                    && (hocPhanStartDate >= surveyStartDate
                                    && hocPhanStartDate <= surveyEndDate) || (hocPhanEndDate >= surveyStartDate
                                    && hocPhanEndDate <= surveyEndDate))
                                {
                                    url = "/xac-thuc-mon-hoc";
                                    return Ok(new { data = url, non_survey = true });
                                }
                                else
                                {
                                    return Ok(new { message = "Bạn không tồn tại học phần nào, vui lòng liên hệ với người phụ trách để biết thêm chi tiết" });
                                }
                            }
                            else if (hoc_vien_co_hoc_phan_tot_nghiep)
                            {
                                if (check_hoc_vien_co_hoc_phan != null && items.mon_hoc.hoc_phan.ten_hoc_phan == "Báo cáo tốt nghiệp" && hocPhanStartDate >= surveyStartDate && hocPhanEndDate <= surveyEndDate)
                                {
                                    HttpContext.Current.Session["Surveyid"] = survey.surveyID;
                                    url = "/xac-thuc-mon-hoc";
                                    return Ok(new { data = url, non_survey = true });
                                }
                                else
                                {
                                    return Ok(new { message = "Bạn không tồn tại học phần nào, vui lòng liên hệ với người phụ trách để biết thêm chi tiết" });
                                }
                            }
                        }
                    }
                    else
                    {
                        return Ok(new { message = "Bạn không tồn tại học phần nào, vui lòng liên hệ với người phụ trách để biết thêm chi tiết" });
                    }
                    
                }
                else
                {
                    return Ok(new { message = "Bạn đang đăng nhập Email cá nhân, vui lòng đăng nhập lại bằng Email người học để thực hiện khảo sát." });
                }
            }
            return BadRequest("Không thể gửi yêu cầu");
        }
        private async Task<IHttpActionResult> Is_Non_Answer_Survey(survey survey, string mssv_by_email, string check_mail_student)
        {
            var url = "";
            var check_group_loaikhaosat = await db.LoaiKhaoSat.FirstOrDefaultAsync(x => x.id_loaikhaosat == survey.id_loaikhaosat);

            // Check phiếu thuộc học viên
            if (check_group_loaikhaosat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu học viên")
            {
                bool cuu_hoc_vien = new[] { 6 }.Contains(survey.id_loaikhaosat);

                bool hoc_vien_nhap_hoc = new[] { 4 }.Contains(survey.id_loaikhaosat);
                bool hoc_vien_nhap_hoc_khong_co_thoi_gian_tot_nghiep = new[] { 9 }.Contains(survey.id_loaikhaosat);
                bool hoc_vien_cuoi_khoa_co_quyet_dinh_tot_nghiep = new[] { 12 }.Contains(survey.id_loaikhaosat);
                if (cuu_hoc_vien)
                {
                    url = "/xac_thuc/" + survey.surveyID;
                    return Ok(new { data = url, non_survey = true });
                }
                if (check_mail_student == "student.tdmu.edu.vn")
                {
                    if (hoc_vien_nhap_hoc_khong_co_thoi_gian_tot_nghiep)
                    {
                        var sinh_vien = await db.sinhvien.FirstOrDefaultAsync(x => x.ma_sv == mssv_by_email);
                        var isEligible = await convert_nguoi_hoc(survey.thang_nhap_hoc, sinh_vien.namnhaphoc, mssv_by_email);
                        if (isEligible != null/* && isEligible.namtotnghiep == null*/)
                        {
                            url = "/phieu-khao-sat/" + survey.surveyID;
                            return Ok(new { data = url, non_survey = true });
                        }
                        {
                            return Ok(new { message = "Bạn không thể thực hiện khảo sát phiếu này, phiếu này dành cho người học nhập học trong khoảng " + survey.thang_nhap_hoc });
                        }
                    }
                    else if (hoc_vien_nhap_hoc)
                    {
                        var sinh_vien = await db.sinhvien.FirstOrDefaultAsync(x => x.ma_sv == mssv_by_email);
                        var isEligible = await convert_nguoi_hoc(survey.thang_nhap_hoc, sinh_vien.namnhaphoc, mssv_by_email);
                        if (isEligible != null)
                        {
                            url = "/phieu-khao-sat/" + survey.surveyID;
                            return Ok(new { data = url, non_survey = true });
                        }
                        else
                        {
                            return Ok(new { message = "Bạn không thể thực hiện khảo sát phiếu này, phiếu này dành cho người học nhập học trong khoảng " + survey.thang_nhap_hoc });
                        }
                    }
                    else if (hoc_vien_cuoi_khoa_co_quyet_dinh_tot_nghiep)
                    {
                        var sinh_vien = await db.sinhvien.FirstOrDefaultAsync(x => x.ma_sv == mssv_by_email);
                        var isEligible = await convert_nguoi_hoc(survey.thang_tot_nghiep, sinh_vien.namtotnghiep, mssv_by_email);
                        if (isEligible != null)
                        {
                            url = "/phieu-khao-sat/" + survey.surveyID;
                            return Ok(new { data = url, non_survey = true });
                        }
                        else
                        {
                            return Ok(new { message = "Bạn không thể thực hiện khảo sát phiếu này, phiếu này dành cho người học nhập học có quyết định tốt nghiệp " + survey.thang_tot_nghiep });
                        }
                    }
                }

                else
                {
                    return Ok(new { message = "Bạn đang đăng nhập Email cá nhân, vui lòng đăng nhập lại bằng Email người học để thực hiện khảo sát." });
                }
            }
            // Check phiếu thuộc giảng viên
            else if (check_group_loaikhaosat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu giảng viên")
            {
                bool check_mail_cbvc = await db.CanBoVienChuc.AnyAsync(x => x.Email == user.email);
                if (check_mail_cbvc)
                {
                    url = survey.id_loaikhaosat == 3 ? "/xac_thuc/" + survey.surveyID : "/phieu-khao-sat/" + survey.surveyID;
                    return Ok(new { data = url, non_survey = true });                    
                }
                else
                {
                    return Ok(new { message = survey.id_loaikhaosat == 3 ? "Bạn không thể thực hiện khảo sát phiếu này, phiếu này dành cho giảng viên" : "Bạn không thể thực hiện khảo sát phiếu này, phiếu này dành cho cán bộ viên chức" });
                }

            }
            else if (check_group_loaikhaosat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu doanh nghiệp")
            {
                bool doanh_nghiep = new[] { 5 }.Contains(survey.id_loaikhaosat);
                if (doanh_nghiep)
                {
                    url = "/xac_thuc/" + survey.surveyID;
                    return Ok(new { data = url, non_survey = true });
                }
            }
            else if (check_group_loaikhaosat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu người học có học phần")
            {
                if (check_mail_student == "student.tdmu.edu.vn")
                {
                    bool hoc_vien_co_hoc_phan_ly_thuyet_dang_hoc_tai_truong = new[] { 11 }.Contains(survey.id_loaikhaosat);
                    bool hoc_vien_co_hoc_phan_tot_nghiep = new[] { 13 }.Contains(survey.id_loaikhaosat);
                    // Tách chuỗi học phần survey
                    var check_hoc_vien_co_hoc_phan = await db.nguoi_hoc_dang_co_hoc_phan?.Where(x => x.sinhvien.ma_sv == mssv_by_email).ToListAsync();
                    var tach_chuoi_hoc_phan_survey = survey.hoc_phan.Split('-');
                    string startDateString = "01/" + tach_chuoi_hoc_phan_survey[0].PadLeft(7, '0');
                    string endDateString = "30/" + tach_chuoi_hoc_phan_survey[1].PadLeft(7, '0');
                    DateTime surveyStartDate = DateTime.ParseExact(startDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    DateTime surveyEndDate = DateTime.ParseExact(endDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    // Chạy vòng lặp kiểm tra
                    if (check_hoc_vien_co_hoc_phan.Any())
                    {
                        foreach (var items in check_hoc_vien_co_hoc_phan)
                        {
                            if (items != null)
                            {
                                var tach_chuoi_hoc_phan_mon_hoc = items.thang_by_hoc_phan.Split('-');
                                string hocPhanStartDateString = "01/" + tach_chuoi_hoc_phan_mon_hoc[0].PadLeft(7, '0');
                                string hocPhanEndDateString = "30/" + tach_chuoi_hoc_phan_mon_hoc[1].PadLeft(7, '0');
                                DateTime hocPhanStartDate = DateTime.ParseExact(hocPhanStartDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                                DateTime hocPhanEndDate = DateTime.ParseExact(hocPhanEndDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                                if (hoc_vien_co_hoc_phan_ly_thuyet_dang_hoc_tai_truong)
                                {
                                    if (check_hoc_vien_co_hoc_phan != null
                                        && items.mon_hoc.hoc_phan.ten_hoc_phan == "Lý thuyết"
                                        // Kiểm tra học phần nằm trong khoảng survey
                                        && (hocPhanStartDate >= surveyStartDate
                                        && hocPhanStartDate <= surveyEndDate) || (hocPhanEndDate >= surveyStartDate
                                        && hocPhanEndDate <= surveyEndDate))
                                    {
                                        HttpContext.Current.Session["Surveyid"] = survey.surveyID;
                                        url = "/xac-thuc-mon-hoc";
                                        return Ok(new { data = url, non_survey = true });
                                    }
                                    else
                                    {
                                        return Ok(new { message = "Bạn không tồn tại học phần nào, vui lòng liên hệ với người phụ trách để biết thêm chi tiết" });
                                    }

                                }
                                else if (hoc_vien_co_hoc_phan_tot_nghiep)
                                {
                                    if (check_hoc_vien_co_hoc_phan != null && items.mon_hoc.hoc_phan.ten_hoc_phan == "Báo cáo tốt nghiệp" && (hocPhanStartDate >= surveyStartDate
                                        && hocPhanStartDate <= surveyEndDate) || (hocPhanEndDate >= surveyStartDate
                                        && hocPhanEndDate <= surveyEndDate))
                                    {
                                        HttpContext.Current.Session["Surveyid"] = survey.surveyID;
                                        url = "/xac-thuc-mon-hoc";
                                        return Ok(new { data = url, non_survey = true });
                                    }
                                    else
                                    {
                                        return Ok(new { message = "Bạn không tồn tại học phần nào, vui lòng liên hệ với người phụ trách để biết thêm chi tiết" });
                                    }

                                }
                            }
                           
                        }
                    }
                    else
                    {
                        return Ok(new { message = "Bạn không tồn tại học phần nào, vui lòng liên hệ với người phụ trách để biết thêm chi tiết" });
                    }

                }
                else
                {
                    return Ok(new { message = "Bạn đang đăng nhập Email cá nhân, vui lòng đăng nhập lại bằng Email người học để thực hiện khảo sát" });
                }
            }
            return BadRequest("Không thể gửi yêu cầu");
        }

        [HttpPost]
        [Route("api/get_nguoi_hoc_by_key")]
        public async Task<IHttpActionResult> get_nguoi_hoc_by_key(GetNguoiHoc getNguoiHoc)
        {
            var survey = db.survey.FirstOrDefault(x => x.surveyID == getNguoiHoc.Id);
            var tach_chuoi_nam_tot_nghiep = survey.thang_tot_nghiep.Split('-');
            string startDateString = "01/" + tach_chuoi_nam_tot_nghiep[0].PadLeft(7, '0');
            string endDateString = "30/" + tach_chuoi_nam_tot_nghiep[1].PadLeft(7, '0');
            DateTime startDate = DateTime.ParseExact(startDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime endDate = DateTime.ParseExact(endDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            if (survey.id_loaikhaosat == 2 || survey.id_loaikhaosat == 6)
            {
                var nguoi_hoc = (await db.sinhvien.ToListAsync())
                .Where(sv =>
                 {
                     if (!string.IsNullOrEmpty(sv.namtotnghiep))
                     {
                         var formattedNamTotNghiep = sv.namtotnghiep.Split('/');
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
                .Select(x => new
                {
                    ma_nguoi_hoc = x.id_sv,
                    ma_so_nguoi_hoc = x.ma_sv,
                    chuong_trinh_dao_tao = x.lop.ctdt.id_ctdt,
                })
                .FirstOrDefault(x => x.ma_so_nguoi_hoc == getNguoiHoc.ma_nguoi_hoc);
                return Ok(new { data = nguoi_hoc, success = true });
            }
            return Ok(new { success = false });
        }
        [HttpPost]
        [Route("api/save_xac_thuc")]
        public IHttpActionResult save_xac_thuc(SaveXacThuc saveXacThuc)
        {
            // Lấy thông tin khảo sát
            var survey = db.survey.FirstOrDefault(x => x.surveyID == saveXacThuc.Id);
            if (survey == null)
                return Ok(new { message = "Không tìm thấy khảo sát" });

            var check_group_loaikhaosat = db.LoaiKhaoSat.FirstOrDefault(x => x.id_loaikhaosat == survey.id_loaikhaosat);
            if (check_group_loaikhaosat == null)
                return Ok(new { message = "Không tìm thấy loại khảo sát" });

            var nameGroup = check_group_loaikhaosat.group_loaikhaosat.name_gr_loaikhaosat;

            switch (nameGroup)
            {
                case "Phiếu doanh nghiệp":
                    var checkIsAnswerDoanhNghiep = db.answer_response.FirstOrDefault(x =>
                        x.surveyID == survey.surveyID &&
                        x.id_ctdt == saveXacThuc.ctdt &&
                        x.id_users == user.id_users &&
                        x.id_namhoc == survey.id_namhoc &&
                        x.id_donvi == null &&
                        x.id_CBVC == null &&
                        x.id_sv == null &&
                        x.json_answer != null);

                    if (checkIsAnswerDoanhNghiep != null)
                    {
                        var url = "phieu-khao-sat/dap-an/" + checkIsAnswerDoanhNghiep.id + "/" + checkIsAnswerDoanhNghiep.surveyID;
                        return Ok(new { is_answer = true, message = "Tài khoản này đã thực hiện khảo sát cho chương trình đào tạo này", info = url });
                    }
                    break;

                case "Phiếu học viên":
                    var checkIsAnswerHocVien = db.answer_response.FirstOrDefault(x =>
                        x.surveyID == survey.surveyID &&
                        x.id_ctdt == saveXacThuc.ctdt &&
                        x.id_users == user.id_users &&
                        x.id_namhoc == survey.id_namhoc &&
                        x.id_donvi == null &&
                        x.id_CBVC == null &&
                        x.id_sv == saveXacThuc.nguoi_hoc &&
                        x.json_answer != null);

                    if (checkIsAnswerHocVien != null)
                    {
                        var url = "/phieu-khao-sat/dap-an/" + checkIsAnswerHocVien.id + "/" + checkIsAnswerHocVien.surveyID;
                        return Ok(new { is_answer = true, message = "Tài khoản này đã thực hiện khảo sát cho sinh viên này !", info = url });
                    }
                    break;

                case "Phiếu giảng viên":
                    var giangVien = db.CanBoVienChuc.FirstOrDefault(x => x.Email == user.email);
                    if (giangVien != null)
                    {
                        var checkIsAnswerGiangVien = db.answer_response.FirstOrDefault(x =>
                            x.surveyID == survey.surveyID &&
                            x.id_ctdt == saveXacThuc.ctdt &&
                            x.id_users == user.id_users &&
                            x.id_namhoc == survey.id_namhoc &&
                            (x.id_donvi == saveXacThuc.donvi || x.id_donvi == null) &&
                            x.id_CBVC == giangVien.id_CBVC &&
                            x.id_sv == null &&
                            x.json_answer != null);

                        if (checkIsAnswerGiangVien != null)
                        {

                            var url = "/phieu-khao-sat/dap-an/" + checkIsAnswerGiangVien.id + "/" + checkIsAnswerGiangVien.surveyID;
                            return Ok(new { is_answer = true, message = "Tài khoản này đã thực hiện khảo sát cho chương trình đào tạo và đơn vị này !", info = url });
                        }
                    }
                    else
                    {
                        return Ok(new { message = "Không tìm thấy thông tin giảng viên" });
                    }
                    break;

                case "Phiếu người học có học phần":
                    var checkMonHoc = db.nguoi_hoc_dang_co_hoc_phan.FirstOrDefault(x => x.id_nguoi_hoc_by_hoc_phan == saveXacThuc.id_nguoi_hoc_by_mon_hoc);
                    if (checkMonHoc != null)
                    {
                        var idDonVi = checkMonHoc.CanBoVienChuc?.id_donvi;
                        var CheckMonHocByHocVien = db.answer_response.FirstOrDefault(x =>
                            x.surveyID == survey.surveyID &&
                            x.id_ctdt == checkMonHoc.sinhvien.lop.id_ctdt &&
                            x.id_users == user.id_users &&
                            x.id_namhoc == survey.id_namhoc &&
                            x.id_donvi == idDonVi &&
                            x.id_CBVC == checkMonHoc.id_giang_vvien &&
                            x.id_sv == checkMonHoc.id_sinh_vien &&
                            x.id_mh == checkMonHoc.id_mon_hoc &&
                            x.json_answer != null);

                        if (CheckMonHocByHocVien != null)
                        {
                            var url = "/phieu-khao-sat/dap-an/" + CheckMonHocByHocVien.id + "/" + CheckMonHocByHocVien.surveyID;
                            return Ok(new { is_answer = true, message = "Bạn đã khảo sát môn học này, bạn có muốn chỉnh sửa lại đáp án không?", info = url });
                        }
                    }
                    return Ok(new { success = true });

                default:
                    return Ok(new { message = "Loại phiếu khảo sát không hợp lệ" });
            }

            return Ok(new { success = true });
        }



        public async Task<bool> convert_giang_vien(int? idnamhoc)
        {
            return await db.CanBoVienChuc.AnyAsync(x => x.Email == user.email && x.id_namhoc == idnamhoc);
        }
        public async Task<sinhvien> convert_nguoi_hoc(string thangstring, string namstring, string mssv)
        {
            var tach_chuoi_nam_tot_nghiep = thangstring.Split('-');
            string startDateString = "01/" + tach_chuoi_nam_tot_nghiep[0].PadLeft(7, '0');
            string endDateString = "30/" + tach_chuoi_nam_tot_nghiep[1].PadLeft(7, '0');
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
                            string formattedDate = "01/" + formattedNamTotNghiep[0].PadLeft(7, '0') + "/" + formattedNamTotNghiep[1];

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
        [HttpGet]
        [Route("api/load_danh_sach_mon_hoc")]
        public async Task<IHttpActionResult> hoc_vien_co_mon_hoc()
        {
            var email_String = user.email.Split('@');
            var mssv_by_email = email_String[0];
            var check_mail_student = email_String[1];
            var id_survey = HttpContext.Current.Session["Surveyid"] as int?;

            if (id_survey == null)
            {
                return Ok(new { message = "Không tìm thấy thông tin phiếu khảo sát.", success = false });
            }

            var survey = await db.survey.FirstOrDefaultAsync(x => x.surveyID == id_survey);

            if (survey == null)
            {
                return Ok(new { message = "Thông tin khảo sát không tồn tại.", success = false });
            }

            bool hoc_vien_co_hoc_phan_ly_thuyet_dang_hoc_tai_truong = new[] { 11 }.Contains(survey.id_loaikhaosat);
            var data_list = new List<dynamic>();

            // Tách chuỗi học phần từ survey
            var tach_chuoi_hoc_phan_survey = survey.hoc_phan.Split('-');
            if (tach_chuoi_hoc_phan_survey.Length < 2)
            {
                return Ok(new { message = "Học phần trong khảo sát không hợp lệ.", success = false });
            }

            string startDateString = "01/" + tach_chuoi_hoc_phan_survey[0].PadLeft(2, '0');
            string endDateString = "30/" + tach_chuoi_hoc_phan_survey[1].PadLeft(2, '0');
            DateTime surveyStartDate = DateTime.ParseExact(startDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime surveyEndDate = DateTime.ParseExact(endDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            if (check_mail_student != "student.tdmu.edu.vn")
            {
                return Ok(new { message = "Tài khoản của bạn không có quyền thực hiện khảo sát này.", success = false });
            }

            var check_hoc_vien_co_hoc_phan = await db.nguoi_hoc_dang_co_hoc_phan
                .Where(x => x.sinhvien.ma_sv == mssv_by_email)
                .ToListAsync();

            foreach (var item in check_hoc_vien_co_hoc_phan)
            {
                var tach_chuoi_hoc_phan_mon_hoc = item.thang_by_hoc_phan.Split('-');
                if (tach_chuoi_hoc_phan_mon_hoc.Length < 2) continue;

                string hocPhanStartDateString = "01/" + tach_chuoi_hoc_phan_mon_hoc[0].PadLeft(7, '0');
                string hocPhanEndDateString = "30/" + tach_chuoi_hoc_phan_mon_hoc[1].PadLeft(7, '0');

                DateTime hocPhanStartDate = DateTime.ParseExact(hocPhanStartDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                DateTime hocPhanEndDate = DateTime.ParseExact(hocPhanEndDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                if (hoc_vien_co_hoc_phan_ly_thuyet_dang_hoc_tai_truong && (hocPhanStartDate >= surveyStartDate
                                && hocPhanStartDate <= surveyEndDate) || (hocPhanEndDate >= surveyStartDate
                                && hocPhanEndDate <= surveyEndDate) && item.mon_hoc.hoc_phan.ten_hoc_phan != "Báo cáo tốt nghiệp")

                {
                    data_list.Add(new
                    {
                        ma_phieu = survey.surveyID,
                        id_mon_hoc = item.id_nguoi_hoc_by_hoc_phan,
                        mon_hoc = item.mon_hoc.ten_mon_hoc,
                        hoc_phan = item.mon_hoc.hoc_phan.ten_hoc_phan,
                        ten_giang_vien = item.CanBoVienChuc.TenCBVC,
                        thoi_gian_hoc = item.thang_by_hoc_phan,
                        tinh_trang_khao_sat = item.da_khao_sat == 1 ? "Đã khảo sát" : "Chưa khảo sát"
                    });
                }
                else
                {
                    return Ok(new { message = "Không tồn tại học phần lý thuyết nào trong khoảng thời gian của khảo sát.", success = false });
                }
            }


            return Ok(new { data = data_list, success = true });
        }
    }
}
