using CTDT.Helper;
using CTDT.Models;
using GoogleApi.Entities.Maps.AerialView.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.DynamicData;
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
            var select_list_data = new List<dynamic>();
            if (survey != null)
            {
                if (survey.id_loaikhaosat == 3)
                {
                    var get_ctdt = await db.ctdt
                        .Select(x => new
                        {
                            value = x.id_ctdt,
                            name = x.ten_ctdt,
                        }).ToListAsync();
                    select_list_data.Add(new { ctdt = get_ctdt, is_giang_vien = true });
                }
                else if (survey.id_loaikhaosat == 2)
                {
                    var get_ctdt = await db.ctdt
                        .Where(x => x.id_hdt == survey.id_hedaotao)
                        .Select(x => new
                        {
                            value = x.id_ctdt,
                            name = x.ten_ctdt,
                        }).ToListAsync();
                    var get_lop = await db.lop
                            .Select(x => new
                            {
                                value_ctdt = x.id_ctdt,
                                value = x.id_lop,
                                name = x.ma_lop,
                            })
                            .ToListAsync();
                    select_list_data.Add(new
                    {
                        ctdt = get_ctdt,
                        lop = get_lop,
                        is_nh = true
                    });

                }
                return Ok(new { data = select_list_data, success = true });
            }
            else
            {
                return Ok(new { message = "Không thể xác thực bộ phiếu", success = false });
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
            bool check_answer_survey = db.answer_response.Any(x => x.surveyID == answer_survey.surveyID && x.id_users == user.id_users && x.json_answer != null);
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
            if (check_group_loaikhaosat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu người học")
            {
                if (check_mail_student == "student.tdmu.edu.vn")
                {
                    url = $"/phieu-khao-sat/dap-an/{answer_survey.id}/{answer_survey.surveyID}";
                    return Ok(new { data = url, is_answer = true });
                }
                else
                {
                    return Ok(new { message = "Bạn đang đăng nhập Email cá nhân, vui lòng đăng nhập lại bằng Email người học để thực hiện khảo sát" });
                }
            }
            else if (check_group_loaikhaosat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu cựu người học")
            {
                url = $"/xac_thuc/{survey.surveyID}";
                return Ok(new { data = url, non_survey = true });
            }
            // Check phiếu thuộc giảng viên
            else if (check_group_loaikhaosat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu giảng viên")
            {
                if (survey.id_loaikhaosat == 3)
                {
                    url = $"/xac_thuc/{survey.surveyID}";
                    return Ok(new { data = url, non_survey = true });
                }
                else if (survey.id_loaikhaosat == 8)
                {
                    var check_mail_cbvc = await db.cbvc_khao_sat.FirstOrDefaultAsync(x => x.CanBoVienChuc.Email == user.email);
                    if (check_mail_cbvc != null)
                    {
                        url = $"/phieu-khao-sat/dap-an/{answer_survey.id}/{answer_survey.surveyID}";
                        return Ok(new { data = url, is_answer = true });
                    }
                    else
                    {
                        return Ok(new { message = "Bạn không thể thực hiện khảo sát phiếu này, phiếu này dành cho giảng viên", success = false });
                    }
                }
            }
            else if (check_group_loaikhaosat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu doanh nghiệp")
            {
                bool doanh_nghiep = new[] { 5 }.Contains(survey.id_loaikhaosat);
                if (doanh_nghiep)
                {
                    url = $"/xac_thuc/{survey.surveyID}";
                    return Ok(new { data = url, non_survey = true });
                }
            }
            else if (check_group_loaikhaosat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu người học có học phần")
            {
                if (check_mail_student == "student.tdmu.edu.vn")
                {
                    var check_hoc_vien_co_hoc_phan = await db.nguoi_hoc_dang_co_hoc_phan?.Where(x => x.surveyID == survey.surveyID && x.sinhvien.ma_sv == mssv_by_email).ToListAsync();
                    if (check_hoc_vien_co_hoc_phan != null)
                    {
                        url = $"/xac-thuc-mon-hoc/{survey.surveyID}";
                        return Ok(new { data = url, non_survey = true });
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
            if (check_group_loaikhaosat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu người học")
            {
                if (check_mail_student == "student.tdmu.edu.vn")
                {
                    var check_phieu = await db.nguoi_hoc_khao_sat.FirstOrDefaultAsync(x => x.surveyID == survey.surveyID && x.sinhvien.ma_sv == mssv_by_email);
                    if (check_phieu != null)
                    {
                        HttpContext.Current.Session["id_nghoc_ks"] = check_phieu.id_nguoi_hoc_khao_sat;
                        url = $"/phieu-khao-sat/{survey.surveyID}";
                        return Ok(new { data = url, non_survey = true });
                    }
                    {
                        return Ok(new { message = "Bạn không có dữ liệu khảo sát phiếu này, vui lòng liên hệ với người phụ trách để biết thêm chi tiết" });
                    }
                }
                else
                {
                    return Ok(new { message = "Bạn đang đăng nhập Email cá nhân, vui lòng đăng nhập lại bằng Email người học để thực hiện khảo sát." });
                }
            }
            else if (check_group_loaikhaosat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu cựu người học")
            {
                url = $"/xac_thuc/{survey.surveyID}";
                return Ok(new { data = url, non_survey = true });
            }
            // Check phiếu thuộc giảng viên
            else if (check_group_loaikhaosat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu giảng viên")
            {
                if (survey.id_loaikhaosat == 3)
                {
                    url = $"/xac_thuc/{survey.surveyID}";
                    return Ok(new { data = url, non_survey = true });
                }
                var check_mail_cbvc = await db.cbvc_khao_sat.FirstOrDefaultAsync(x => x.surveyID == survey.surveyID && x.CanBoVienChuc.Email == user.email);
                if (check_mail_cbvc != null)
                {
                    if (survey.id_loaikhaosat == 8)
                    {
                        HttpContext.Current.Session["id_cbvc_ks"] = check_mail_cbvc.id_cbvc_khao_sat;
                    }
                    url = $"/phieu-khao-sat/{survey.surveyID}";
                    return Ok(new { data = url, non_survey = true });
                }
                else
                {
                    return Ok(new { message = "Bạn không thể thực hiện khảo sát phiếu này, phiếu này dành cho cán bộ viên chức" });
                }

            }
            else if (check_group_loaikhaosat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu doanh nghiệp")
            {
                url = $"/xac_thuc/{survey.surveyID}";
                return Ok(new { data = url, non_survey = true });
            }
            else if (check_group_loaikhaosat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu người học có học phần")
            {
                if (check_mail_student == "student.tdmu.edu.vn")
                {
                    // Tách chuỗi học phần survey
                    var check_hoc_vien_co_hoc_phan = await db.nguoi_hoc_dang_co_hoc_phan?.Where(x => x.surveyID == survey.surveyID && x.sinhvien.ma_sv == mssv_by_email).ToListAsync();
                    if (check_hoc_vien_co_hoc_phan != null)
                    {
                        url = $"/xac-thuc-mon-hoc/{survey.surveyID}";
                        return Ok(new { data = url, non_survey = true });
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
        public class SaveXacThuc
        {
            public int surveyID { get; set; }
            public int id_ctdt { get; set; }
            public int check_hoc_phan { get; set; }
            public string ma_vien_chuc { get; set; }
            public string ten_vien_chuc { get; set; }
            public string ma_nh { get; set; }
            public string ten_nh { get; set; }
            public string ngay_sinh { get; set; }
            public int check_lop { get; set; }
            public string check_doi_tuong { get; set; }
        }
        [HttpPost]
        [Route("api/save_xac_thuc")]
        public async Task<IHttpActionResult> save_xac_thuc(SaveXacThuc sv)
        {
            var check_survey = await db.survey.FirstOrDefaultAsync(x => x.surveyID == sv.surveyID);
            bool check_giang_vien = new[] { 3 }.Contains(check_survey.id_loaikhaosat);
            if (check_giang_vien)
            {
                if (sv.check_doi_tuong == null)
                {
                    return Ok(new { message = "Vui lòng chọn phương thức xác thực", success = false });
                }
                if (sv.check_doi_tuong == "false")
                {
                    if (string.IsNullOrEmpty(sv.ma_vien_chuc))
                    {
                        return Ok(new { message = "Không được bỏ trống trường mã viên chức", success = false });
                    }
                    if (string.IsNullOrEmpty(sv.ten_vien_chuc))
                    {
                        return Ok(new { message = "Không được bỏ trống trường tên viên chức", success = false });
                    }
                    var check_gv = await db.cbvc_khao_sat
                        .FirstOrDefaultAsync(x => x.surveyID == sv.surveyID
                        && x.CanBoVienChuc.MaCBVC.ToLower().Trim() == sv.ma_vien_chuc.ToLower().Trim()
                        && x.CanBoVienChuc.TenCBVC.ToLower().Trim() == sv.ten_vien_chuc.ToLower().Trim());
                    if (check_gv == null)
                    {
                        return Ok(new { message = "Không tìm thấy thông tin giảng viên, vui lòng kiểm tra lại", success = false });
                    }
                    else
                    {
                        var check_answer = await db.answer_response.FirstOrDefaultAsync(x => x.id_users == user.id_users
                            && x.id_cbvc_khao_sat == check_gv.id_cbvc_khao_sat
                            && x.surveyID == sv.surveyID
                            && x.id_ctdt == sv.id_ctdt);
                        if (check_answer != null)
                        {
                            return Ok(new { message = "", url = $"/phieu-khao-sat/dap-an/{check_answer.id}/{check_answer.surveyID}", is_answer = true });
                        }
                        else
                        {
                            HttpContext.Current.Session["id_cbvc_ks"] = check_gv.id_cbvc_khao_sat;
                            HttpContext.Current.Session["id_cbvc_ks_ctdt"] = sv.id_ctdt;
                            return Ok(new { url = $"/phieu-khao-sat/{sv.surveyID}", success = true });
                        }
                    }
                }
                else if (sv.check_doi_tuong == "true")
                {
                    var check_gv = await db.cbvc_khao_sat
                        .FirstOrDefaultAsync(x => x.surveyID == sv.surveyID
                        && x.CanBoVienChuc.Email == user.email);

                    if (check_gv == null)
                    {
                        return Ok(new { message = "Không tìm thấy thông tin giảng viên, vui lòng kiểm tra lại", success = false });
                    }
                    else
                    {
                        var check_answer = await db.answer_response.FirstOrDefaultAsync(x => x.id_users == user.id_users
                            && x.id_cbvc_khao_sat == check_gv.id_cbvc_khao_sat
                            && x.surveyID == sv.surveyID
                            && x.id_ctdt == sv.id_ctdt);
                        if (check_answer != null)
                        {
                            return Ok(new { url = $"/phieu-khao-sat/dap-an/{check_answer.id}/{check_answer.surveyID}", is_answer = true });
                        }
                        else
                        {
                            HttpContext.Current.Session["id_cbvc_ks"] = check_gv.id_cbvc_khao_sat;
                            HttpContext.Current.Session["id_cbvc_ks_ctdt"] = sv.id_ctdt;
                            return Ok(new { url = $"/phieu-khao-sat/{sv.surveyID}", success = true });
                        }
                    }
                }
            }
            else if (check_survey.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu người học có học phần")
            {
                var check_nh_co_hoc_phan = await db.nguoi_hoc_dang_co_hoc_phan
                    .Where(x => x.surveyID == sv.surveyID
                    && x.id_nguoi_hoc_by_hoc_phan == sv.check_hoc_phan)
                    .FirstOrDefaultAsync();
                var check_answer = await db.answer_response.FirstOrDefaultAsync(x => x.id_nguoi_hoc_co_hp_khao_sat == check_nh_co_hoc_phan.id_nguoi_hoc_by_hoc_phan
                            && x.surveyID == sv.surveyID);
                if (check_answer != null)
                {
                    return Ok(new { url = $"/phieu-khao-sat/dap-an/{check_answer.id}/{check_answer.surveyID}", is_answer = true });
                }
                else
                {
                    HttpContext.Current.Session["id_nh_co_hp_ks"] = check_nh_co_hoc_phan.id_nguoi_hoc_by_hoc_phan;
                    return Ok(new { url = $"/phieu-khao-sat/{sv.surveyID}", success = true });
                }
            }
            else if (check_survey.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu cựu người học")
            {
                if (sv.check_doi_tuong == null)
                {
                    return Ok(new { message = "Vui lòng chọn phương thức xác thực", success = false });
                }
                if (sv.check_doi_tuong == "true")
                {
                    if (string.IsNullOrEmpty(sv.ma_nh))
                    {
                        return Ok(new { message = "Không được bỏ trống trường mã người học", success = false });
                    }
                    var check_nh = await db.nguoi_hoc_khao_sat
                       .Where(x => x.surveyID == sv.surveyID && x.sinhvien.ma_sv == sv.ma_nh)
                       .FirstOrDefaultAsync();
                    if (check_nh != null)
                    {
                        var check_answer = await db.answer_response.FirstOrDefaultAsync(x => x.id_users == user.id_users
                            && x.id_nguoi_hoc_khao_sat == check_nh.id_nguoi_hoc_khao_sat
                            && x.surveyID == sv.surveyID);
                        if (check_answer != null)
                        {
                            return Ok(new { message = "", url = $"/phieu-khao-sat/dap-an/{check_answer.id}/{check_answer.surveyID}", is_answer = true });
                        }
                        else
                        {
                            HttpContext.Current.Session["id_nghoc_ks"] = check_nh.id_nguoi_hoc_khao_sat;
                            return Ok(new { url = $"/phieu-khao-sat/{sv.surveyID}", success = true });
                        }
                    }
                    else
                    {
                        return Ok(new { message = "Không tìm thấy thông tin người học trong phiếu này", success = false });
                    }
                }
                else if (sv.check_doi_tuong == "false")
                {
                    if (string.IsNullOrEmpty(sv.ten_nh))
                    {
                        return Ok(new { message = "Không được bỏ trống trường họ và tên", success = false });
                    }
                    else if (string.IsNullOrEmpty(sv.ngay_sinh))
                    {
                        return Ok(new { message = "Không được bỏ trống trường ngày sinh", success = false });
                    }
                    DateTime ngaySinhDate;
                    if (!DateTime.TryParseExact(sv.ngay_sinh, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ngaySinhDate))
                    {
                        return Ok(new { message = "Ngày sinh sai định dạng, vui lòng nhập đầy đủ", success = false });
                    }
                    var check_nh = await db.nguoi_hoc_khao_sat
                        .Where(x => x.surveyID == sv.surveyID &&
                                    x.sinhvien.id_lop == sv.check_lop &&
                                    x.sinhvien.hovaten.ToLower().Trim() == sv.ten_nh.ToLower().Trim() &&
                                    x.sinhvien.ngaysinh == ngaySinhDate.Date)
                        .FirstOrDefaultAsync();
                    if (check_nh != null)
                    {
                        var check_answer = await db.answer_response.FirstOrDefaultAsync(x => x.id_users == user.id_users
                            && x.id_nguoi_hoc_khao_sat == check_nh.id_nguoi_hoc_khao_sat
                            && x.surveyID == sv.surveyID);
                        if (check_answer != null)
                        {
                            return Ok(new { message = "", url = $"/phieu-khao-sat/dap-an/{check_answer.id}/{check_answer.surveyID}", is_answer = true });
                        }
                        else
                        {
                            HttpContext.Current.Session["id_nghoc_ks"] = check_nh.id_nguoi_hoc_khao_sat;
                            return Ok(new { url = $"/phieu-khao-sat/{sv.surveyID}", success = true });
                        }
                    }
                    else
                    {
                        return Ok(new { message = "Không tìm thấy thông tin người học trong phiếu này", success = false });
                    }
                }
            }
            return Ok(new { message = "Không tìm thấy thông tin biểu mẫu", success = false });
        }
        [HttpPost]
        [Route("api/load_danh_sach_mon_hoc")]
        public async Task<IHttpActionResult> hoc_vien_co_mon_hoc(nguoi_hoc_dang_co_hoc_phan items)
        {
            var email_String = user.email.Split('@');
            var mssv_by_email = email_String[0];
            var check_mail_student = email_String[1];
            if (items.surveyID == null)
            {
                return Ok(new { message = "Bạn chưa xác thực, vui lòng quay lại bộ phiếu khảo sát và chọn lại", success = false });
            }
            var survey = await db.survey.FirstOrDefaultAsync(x => x.surveyID == items.surveyID);
            var data_list = new List<dynamic>();
            // Tách chuỗi học phần từ survey
            if (check_mail_student != "student.tdmu.edu.vn")
            {
                return Ok(new { message = "Tài khoản của bạn không có quyền thực hiện khảo sát này.", success = false });
            }
            var check_hoc_vien_co_hoc_phan = await db.nguoi_hoc_dang_co_hoc_phan
                .Where(x => x.surveyID == items.surveyID && x.sinhvien.ma_sv == mssv_by_email)
                .ToListAsync();

            foreach (var item in check_hoc_vien_co_hoc_phan)
            {
                data_list.Add(new
                {
                    ma_phieu = survey.surveyID,
                    id_mon_hoc = item.id_nguoi_hoc_by_hoc_phan,
                    mon_hoc = item.mon_hoc.ten_mon_hoc,
                    hoc_phan = item.mon_hoc.hoc_phan.ten_hoc_phan,
                    ten_giang_vien = item.CanBoVienChuc.TenCBVC,
                    tinh_trang_khao_sat = item.da_khao_sat == 1 ? "Đã khảo sát" : "Chưa khảo sát"
                });
            }
            return Ok(new { data = JsonConvert.SerializeObject(data_list), success = true });
        }
    }
}
