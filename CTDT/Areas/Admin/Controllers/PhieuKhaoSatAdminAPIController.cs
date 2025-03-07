using CTDT.Helper;
using CTDT.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace CTDT.Areas.Admin.Controllers
{
    public class PhieuKhaoSatAdminAPIController : ApiController
    {
        dbSurveyEntities db = new dbSurveyEntities();
        private users user;
        private PhieuKhaoSatAdminAPIController()
        {
            user = SessionHelper.GetUser();
        }

        [HttpPost]
        [Route("api/admin/danh-sach-phieu-khao-sat")]
        public async Task<IHttpActionResult> danh_sach_phieu_khao_sat(survey survey)
        {
            var query = db.survey.AsQueryable();
            if (survey.id_hedaotao != 0)
            {
                query = query.Where(x => x.id_hedaotao == survey.id_hedaotao);
            }

            if (survey.id_loaikhaosat != 0)
            {
                query = query.Where(x => x.id_loaikhaosat == survey.id_loaikhaosat);
            }

            if (survey.id_namhoc != 0)
            {
                query = query.Where(x => x.id_namhoc == survey.id_namhoc);
            }

            if (survey.surveyStatus == 1 || survey.surveyStatus == 2)
            {
                query = query.Where(x => x.surveyStatus == survey.surveyStatus);
            }

            var ListPhieu = await query
                .Select(p => new
                {
                    ma_phieu = p.surveyID,
                    ten_hdt = p.hedaotao.ten_hedaotao,
                    ma_hdt = p.id_hedaotao,
                    ten_phieu = p.surveyTitle,
                    mo_ta = p.surveyDescription,
                    ngay_tao = p.surveyTimeMake,
                    ngay_cap_nhat = p.surveyTimeUpdate,
                    ngay_bat_dau = p.surveyTimeStart,
                    ngay_ket_thuc = p.surveyTimeEnd,
                    loai_khao_sat = p.LoaiKhaoSat.name_loaikhaosat,
                    ma_loai_khao_sat = p.id_loaikhaosat,
                    nguoi_tao = p.users.firstName + " " + p.users.lastName,
                    trang_thai = p.surveyStatus,
                    nam = p.NamHoc.ten_namhoc,
                }).ToListAsync();

            if (ListPhieu.Any())
            {
                return Ok(new { data = ListPhieu, success = true });
            }
            else
            {
                return Ok(new { message = "Không tồn tại dữ liệu", success = false });
            }
        }
        [HttpPost]
        [Route("api/admin/danh-sach-cau-tra-loi-phieu")]
        public async Task<IHttpActionResult> danh_sach_cac_cau_tra_loi_phieu(answer_response aw)
        {
            var get_data = await db.survey.Where(x => x.surveyID == aw.surveyID).FirstOrDefaultAsync();
            var list_data = new List<dynamic>();
            bool is_subject = false;
            bool is_student = false;
            bool is_cbvc = false;
            bool is_program = false;
            // Nếu phiếu là phiếu có môn học
            if (get_data.LoaiKhaoSat.group_loaikhaosat.id_gr_loaikhaosat == 3)
            {
                is_subject = true;
                var get_answer = await db.answer_response
                    .Where(x => x.surveyID == aw.surveyID)
                    .Select(x => new
                    {
                        ma_kq = x.id,
                        email = x.users.email,
                        mon_hoc = x.mon_hoc.ten_mon_hoc,
                        giang_vien = x.CanBoVienChuc.TenCBVC,
                        sinh_vien = x.sinhvien.hovaten,
                        thoi_gian_thuc_hien = x.time,
                        ctdt = x.ctdt.ten_ctdt,
                        msnh = x.sinhvien.ma_sv,
                    }).ToListAsync();
                list_data.Add(get_answer);
            }
            // Nếu phiếu là phiếu người học hoặc cựu người học
            else if (get_data.LoaiKhaoSat.group_loaikhaosat.id_gr_loaikhaosat == 1 || get_data.LoaiKhaoSat.group_loaikhaosat.id_gr_loaikhaosat == 5)
            {
                is_student = true;
                var get_answer = await db.answer_response
                    .Where(x => x.surveyID == aw.surveyID)
                    .Select(x => new
                    {
                        ma_kq = x.id,
                        email = x.users.email,
                        sinh_vien = x.sinhvien.hovaten,
                        thoi_gian_thuc_hien = x.time,
                        ctdt = x.ctdt.ten_ctdt,
                        msnh = x.sinhvien.ma_sv,
                    }).ToListAsync();
                list_data.Add(get_answer);
            }
            // Nếu phiếu là phiếu doanh nghiệp
            else if (get_data.LoaiKhaoSat.group_loaikhaosat.id_gr_loaikhaosat == 4)
            {
                is_program = true;
                var get_answer = await db.answer_response
                    .Where(x => x.surveyID == aw.surveyID)
                    .Select(x => new
                    {
                        ma_kq = x.id,
                        email = x.users.email,
                        ten_ctdt = x.ctdt.ten_ctdt,
                        thoi_gian_thuc_hien = x.time,
                    }).ToListAsync();
                list_data.Add(get_answer);
            }
            // Nếu phiếu là phiếu giảng viên hoặc cán bộ viên chức trong trường
            else if (get_data.LoaiKhaoSat.group_loaikhaosat.id_gr_loaikhaosat == 2)
            {
                is_cbvc = true;
                var get_answer = await db.answer_response
                    .Where(x => x.surveyID == aw.surveyID)
                    .Select(x => new
                    {
                        ma_kq = x.id,
                        don_vi = x.DonVi.name_donvi,
                        email = x.users.email,
                        cbvc = x.CanBoVienChuc.TenCBVC,
                        ctdt = x.ctdt.ten_ctdt,
                        thoi_gian_thuc_hien = x.time,
                    }).ToListAsync();
                list_data.Add(get_answer);
            }
            if (list_data.Count > 0)
            {
                if (is_subject)
                {
                    return Ok(new { data = list_data, success = true, is_subject = true });
                }
                else if (is_student)
                {
                    return Ok(new { data = list_data, success = true, is_student = true });
                }
                else if (is_program)
                {
                    return Ok(new { data = list_data, success = true, is_program = true });
                }
                else if (is_cbvc)
                {
                    return Ok(new { data = list_data, success = true, is_cbvc = true });
                }

            }
            return Ok(new { message = "Không có dữ liệu", success = false });
        }
        [HttpPost]
        [Route("api/admin/chi-tiet-cau-tra-loi")]
        public async Task<IHttpActionResult> answer_survey(answer_response aw)
        {
            var get_answer = await db.answer_response.FirstOrDefaultAsync(x => x.id == aw.id);
            if (get_answer != null)
            {
                return Ok(new { data = get_answer.json_answer, success = true });
            }
            else
            {
                return Ok(new { message = "Không tìm thấy dữ liệu", success = false });
            }

        }
        [HttpPost]
        [Route("api/admin/chi-tiet-cau-hoi-khao-sat")]
        public async Task<IHttpActionResult> chi_tiet_cau_hoi_khao_sat(survey sv)
        {
            var get_data = await db.survey.FirstOrDefaultAsync(x => x.surveyID == sv.surveyID);
            if (get_data.surveyData != null)
            {
                return Ok(new { data = get_data.surveyData, success = true });
            }
            else
            {
                return Ok(new { message = "Chưa có biểu mẫu khảo sát", success = false });
            }
        }
        [HttpPost]
        [Route("api/admin/them-moi-phieu-khao-sat")]
        public IHttpActionResult them_moi_phieu_khao_sat(survey sv)
        {
            DateTime now = DateTime.UtcNow;
            int unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            if (db.survey.FirstOrDefault(x => x.surveyTitle == sv.surveyTitle && x.id_hedaotao == sv.id_hedaotao && x.id_namhoc == sv.id_namhoc) != null)
            {
                return Ok(new { message = "Phiếu khảo sát này đã tồn tại, vui lòng kiểm tra lại", success = false });
            }
            if (string.IsNullOrEmpty(sv.surveyTitle))
            {
                return Ok(new { message = "Không được bỏ trống tên phiếu khảo sát", success = false });
            }
            var data = new survey
            {
                id_hedaotao = sv.id_hedaotao,
                surveyData = null,
                surveyTitle = sv.surveyTitle,
                surveyDescription = sv.surveyDescription,
                surveyTimeStart = sv.surveyTimeStart,
                surveyTimeEnd = sv.surveyTimeEnd,
                surveyStatus = sv.surveyStatus,
                id_loaikhaosat = sv.id_loaikhaosat,
                id_namhoc = sv.id_namhoc,
                id_dot_khao_sat = sv.id_dot_khao_sat,
                mo_thong_ke = sv.mo_thong_ke,
                creator = user.id_users,
                surveyTimeMake = unixTimestamp,
                surveyTimeUpdate = unixTimestamp
            };
            db.survey.Add(data);
            db.SaveChanges();
            return Ok(new { message = "Thêm mới dữ liệu thành công", success = true });
        }
        [HttpPost]
        [Route("api/admin/get-info-survey")]
        public async Task<IHttpActionResult> get_info_survey(survey sv)
        {
            var get_info = await db.survey
                .Where(x => x.surveyID == sv.surveyID)
                .Select(x => new
                {
                    x.id_hedaotao,
                    x.surveyTitle,
                    x.surveyDescription,
                    x.surveyTimeStart,
                    x.surveyTimeEnd,
                    x.surveyStatus,
                    x.id_loaikhaosat,
                    x.id_namhoc,
                    x.id_dot_khao_sat,
                    x.mo_thong_ke
                }).FirstOrDefaultAsync();
            return Ok(new { data = get_info, success = true });
        }
        [HttpPost]
        [Route("api/admin/update-phieu-khao-sat")]
        public IHttpActionResult update_survey(survey sv)
        {
            DateTime now = DateTime.UtcNow;
            int unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            var update_survey = db.survey.FirstOrDefault(x => x.surveyID == sv.surveyID);
            if (string.IsNullOrEmpty(sv.surveyTitle))
            {
                return Ok(new { message = "Không được bỏ trống tên phiếu", success = false });
            }
            update_survey.id_hedaotao = sv.id_hedaotao;
            update_survey.surveyTitle = sv.surveyTitle;
            update_survey.surveyDescription = sv.surveyDescription;
            update_survey.surveyTimeStart = sv.surveyTimeStart;
            update_survey.surveyTimeEnd = sv.surveyTimeEnd;
            update_survey.surveyStatus = sv.surveyStatus;
            update_survey.id_loaikhaosat = sv.id_loaikhaosat;
            update_survey.id_namhoc = sv.id_namhoc;
            update_survey.id_dot_khao_sat = sv.id_dot_khao_sat;
            update_survey.mo_thong_ke = sv.mo_thong_ke;
            update_survey.surveyTimeUpdate = unixTimestamp;
            db.SaveChanges();
            return Ok(new { message = "Cập nhật dữ liệu thành công", success = true });
        }
        [HttpPost]
        [Route("api/admin/xoa-du-lieu-phieu-khao-sat")]
        public IHttpActionResult delete_survey(survey sv)
        {
            var check_survey = db.survey.SingleOrDefault(x => x.surveyID == sv.surveyID);
            var check_answer = db.answer_response.Where(x => x.surveyID == sv.surveyID).ToList();
            var check_nguoi_hoc_khao_sat = db.nguoi_hoc_khao_sat.Where(x => x.surveyID == sv.surveyID).ToList();
            var check_nguoi_hoc_hoc_phan = db.nguoi_hoc_dang_co_hoc_phan.Where(x => x.surveyID == sv.surveyID).ToList();
            var check_cbvc_khao_sat = db.cbvc_khao_sat.Where(x => x.surveyID == sv.surveyID).ToList();
            var check_title_survey = db.tieu_de_phieu_khao_sat.Where(x => x.surveyID == sv.surveyID).ToList();
            if (check_answer.Any())
            {
                db.answer_response.RemoveRange(check_answer);
                db.SaveChanges();
            }
            if (check_nguoi_hoc_khao_sat.Any())
            {
                db.nguoi_hoc_khao_sat.RemoveRange(check_nguoi_hoc_khao_sat);
                db.SaveChanges();
            }
            if (check_nguoi_hoc_hoc_phan.Any())
            {
                db.nguoi_hoc_dang_co_hoc_phan.RemoveRange(check_nguoi_hoc_hoc_phan);
                db.SaveChanges();
            }
            if (check_cbvc_khao_sat.Any())
            {
                db.cbvc_khao_sat.RemoveRange(check_cbvc_khao_sat);
                db.SaveChanges();
            }
            if (check_title_survey.Any())
            {
                foreach(var item in check_title_survey)
                {
                    var check_chil_title = db.chi_tiet_cau_hoi_tieu_de.Where(x => x.id_tieu_de_phieu == item.id_tieu_de_phieu).ToList();
                    if (check_chil_title.Any())
                    {
                        foreach(var chil_item in check_chil_title)
                        {
                            var check_rd_cau_hoi = db.radio_cau_hoi_khac.Where(x => x.id_chi_tiet_cau_hoi_tieu_de == chil_item.id_chi_tiet_cau_hoi_tieu_de).ToList();
                            if (check_rd_cau_hoi.Any())
                            {
                                db.radio_cau_hoi_khac.RemoveRange(check_rd_cau_hoi);
                            }
                        }
                        db.chi_tiet_cau_hoi_tieu_de.RemoveRange(check_chil_title);
                    }
                }
                db.tieu_de_phieu_khao_sat.RemoveRange(check_title_survey);
            }
            db.survey.Remove(check_survey);
            db.SaveChanges();
            return Ok(new { message = "Xóa dữ liệu thành công", success = true });
        }

        // Loading bộ câu hỏi đã tạo cho phiếu khảo sát
        [HttpPost]
        [Route("api/admin/load-bo-cau-hoi-phieu-khao-sat")]
        public async Task<IHttpActionResult> tieu_de_phieu_khao_sats(survey sv)
        {
            var get_tieu_de_pks = (await db.tieu_de_phieu_khao_sat
                .Where(x => x.surveyID == sv.surveyID)
                .ToListAsync())
                .OrderBy(x => RomanToInt(x.thu_tu))
                .ToList();

            var list_data = new List<dynamic>();
            int pageCounter = 1;
            int elementCounter = 1;

            foreach (var tieude in get_tieu_de_pks)
            {
                var get_chi_tiet_cau_hoi_tieu_de = await db.chi_tiet_cau_hoi_tieu_de
                    .Where(x => x.id_tieu_de_phieu == tieude.id_tieu_de_phieu)
                    .OrderBy(x => x.thu_tu)
                    .ToListAsync();

                var chi_tiet_cau_hoi_list = new List<dynamic>();

                foreach (var chitietcauhoi in get_chi_tiet_cau_hoi_tieu_de)
                {
                    var dangCauHoi = await db.dang_cau_hoi
                        .Where(x => x.id_dang_cau_hoi == chitietcauhoi.id_dang_cau_hoi)
                        .FirstOrDefaultAsync();
                    var nhieu_lua_chon = new List<dynamic>();

                    if (dangCauHoi.id_dang_cau_hoi == 3)
                    {
                        var get_radio_cau_hoi_khac = await db.radio_cau_hoi_khac
                                .Where(x => x.id_chi_tiet_cau_hoi_tieu_de == chitietcauhoi.id_chi_tiet_cau_hoi_tieu_de)
                                .OrderBy(x => x.thu_tu)
                                .ToListAsync();

                        foreach (var getradiocauhoikhac in get_radio_cau_hoi_khac)
                        {
                            nhieu_lua_chon.Add(new
                            {
                                name = $"question{elementCounter}_{getradiocauhoikhac.thu_tu}",
                                text = getradiocauhoikhac.ten_rd_cau_hoi_khac,
                            });
                        }
                        if (chitietcauhoi.is_ykienkhac == 1)
                        {
                            if (chitietcauhoi.dieu_kien_hien_thi != null)
                            {
                                chi_tiet_cau_hoi_list.Add(new
                                {
                                    type = "radiogroup",
                                    value_chil = chitietcauhoi.id_chi_tiet_cau_hoi_tieu_de,
                                    name = $"question{elementCounter}",
                                    visible = false,
                                    visibleIf = chitietcauhoi.dieu_kien_hien_thi.Split(',').ToArray(),
                                    title = $"{chitietcauhoi.thu_tu}. {chitietcauhoi.ten_cau_hoi}",
                                    isRequired = chitietcauhoi.bat_buoc == 1 ? true : false,
                                    choices = nhieu_lua_chon,
                                    showOtherItem = true,
                                    otherText = "Ý kiến khác:"
                                });
                            }
                            else
                            {
                                chi_tiet_cau_hoi_list.Add(new
                                {
                                    type = "radiogroup",
                                    value_chil = chitietcauhoi.id_chi_tiet_cau_hoi_tieu_de,
                                    name = $"question{elementCounter}",
                                    title = $"{chitietcauhoi.thu_tu}. {chitietcauhoi.ten_cau_hoi}",
                                    isRequired = chitietcauhoi.bat_buoc == 1 ? true : false,
                                    choices = nhieu_lua_chon,
                                    showOtherItem = true,
                                    otherText = "Ý kiến khác:"
                                });
                            }

                        }
                        else
                        {
                            if (chitietcauhoi.dieu_kien_hien_thi != null)
                            {
                                chi_tiet_cau_hoi_list.Add(new
                                {
                                    type = "radiogroup",
                                    value_chil = chitietcauhoi.id_chi_tiet_cau_hoi_tieu_de,
                                    name = $"question{elementCounter}",
                                    visible = false,
                                    visibleIf = chitietcauhoi.dieu_kien_hien_thi.Split(',').ToArray(),
                                    title = $"{chitietcauhoi.thu_tu}. {chitietcauhoi.ten_cau_hoi}",
                                    isRequired = chitietcauhoi.bat_buoc == 1 ? true : false,
                                    choices = nhieu_lua_chon,
                                });
                            }
                            else
                            {
                                chi_tiet_cau_hoi_list.Add(new
                                {
                                    type = "radiogroup",
                                    value_chil = chitietcauhoi.id_chi_tiet_cau_hoi_tieu_de,
                                    name = $"question{elementCounter}",
                                    title = $"{chitietcauhoi.thu_tu}. {chitietcauhoi.ten_cau_hoi}",
                                    isRequired = chitietcauhoi.bat_buoc == 1 ? true : false,
                                    choices = nhieu_lua_chon,
                                });
                            }
                        }
                    }
                    else if (dangCauHoi.id_dang_cau_hoi == 4)
                    {
                        var get_radio_cau_hoi_khac = await db.radio_cau_hoi_khac
                                .Where(x => x.id_chi_tiet_cau_hoi_tieu_de == chitietcauhoi.id_chi_tiet_cau_hoi_tieu_de)
                                .OrderBy(x => x.thu_tu)
                                .ToListAsync();

                        foreach (var getradiocauhoikhac in get_radio_cau_hoi_khac)
                        {
                            nhieu_lua_chon.Add(new
                            {
                                name = $"question{elementCounter}_{getradiocauhoikhac.thu_tu}",
                                text = getradiocauhoikhac.ten_rd_cau_hoi_khac,
                            });
                        }
                        if (chitietcauhoi.is_ykienkhac == 1)
                        {
                            if (chitietcauhoi.dieu_kien_hien_thi != null)
                            {
                                chi_tiet_cau_hoi_list.Add(new
                                {
                                    type = "checkbox",
                                    value_chil = chitietcauhoi.id_chi_tiet_cau_hoi_tieu_de,
                                    name = $"question{elementCounter}",
                                    visible = false,
                                    visibleIf = chitietcauhoi.dieu_kien_hien_thi.Split(',').ToArray(),
                                    title = $"{chitietcauhoi.thu_tu}. {chitietcauhoi.ten_cau_hoi}",
                                    isRequired = chitietcauhoi.bat_buoc == 1 ? true : false,
                                    choices = nhieu_lua_chon,
                                    showOtherItem = true,
                                    otherText = "Ý kiến khác:"
                                });
                            }
                            else
                            {
                                chi_tiet_cau_hoi_list.Add(new
                                {
                                    type = "checkbox",
                                    value_chil = chitietcauhoi.id_chi_tiet_cau_hoi_tieu_de,
                                    name = $"question{elementCounter}",
                                    title = $"{chitietcauhoi.thu_tu}. {chitietcauhoi.ten_cau_hoi}",
                                    isRequired = chitietcauhoi.bat_buoc == 1 ? true : false,
                                    choices = nhieu_lua_chon,
                                    showOtherItem = true,
                                    otherText = "Ý kiến khác:"
                                });
                            }
                        }
                        else
                        {
                            if (chitietcauhoi.dieu_kien_hien_thi != null)
                            {
                                chi_tiet_cau_hoi_list.Add(new
                                {
                                    type = "checkbox",
                                    value_chil = chitietcauhoi.id_chi_tiet_cau_hoi_tieu_de,
                                    name = $"question{elementCounter}",
                                    visible = false,
                                    visibleIf = chitietcauhoi.dieu_kien_hien_thi.Split(',').ToArray(),
                                    title = $"{chitietcauhoi.thu_tu}. {chitietcauhoi.ten_cau_hoi}",
                                    isRequired = chitietcauhoi.bat_buoc == 1 ? true : false,
                                    choices = nhieu_lua_chon
                                });
                            }
                            else
                            {
                                chi_tiet_cau_hoi_list.Add(new
                                {
                                    type = "checkbox",
                                    value_chil = chitietcauhoi.id_chi_tiet_cau_hoi_tieu_de,
                                    name = $"question{elementCounter}",
                                    title = $"{chitietcauhoi.thu_tu}. {chitietcauhoi.ten_cau_hoi}",
                                    isRequired = chitietcauhoi.bat_buoc == 1 ? true : false,
                                    choices = nhieu_lua_chon
                                });
                            }

                        }
                    }
                    else if (dangCauHoi.id_dang_cau_hoi == 5)
                    {
                        var get_radio_cau_hoi_khac = await db.radio_cau_hoi_khac
                                .Where(x => x.id_chi_tiet_cau_hoi_tieu_de == chitietcauhoi.id_chi_tiet_cau_hoi_tieu_de)
                                .OrderBy(x => x.thu_tu)
                                .ToListAsync();

                        foreach (var getradiocauhoikhac in get_radio_cau_hoi_khac)
                        {
                            nhieu_lua_chon.Add(new
                            {
                                name = $"question{elementCounter}_{getradiocauhoikhac.thu_tu}",
                                text = getradiocauhoikhac.ten_rd_cau_hoi_khac,
                            });
                        }
                        if (chitietcauhoi.is_ykienkhac == 1)
                        {
                            if (chitietcauhoi.dieu_kien_hien_thi != null)
                            {
                                chi_tiet_cau_hoi_list.Add(new
                                {
                                    type = "select",
                                    value_chil = chitietcauhoi.id_chi_tiet_cau_hoi_tieu_de,
                                    name = $"question{elementCounter}",
                                    visible = false,
                                    visibleIf = chitietcauhoi.dieu_kien_hien_thi.Split(',').ToArray(),
                                    title = $"{chitietcauhoi.thu_tu}. {chitietcauhoi.ten_cau_hoi}",
                                    isRequired = chitietcauhoi.bat_buoc == 1 ? true : false,
                                    choices = nhieu_lua_chon,
                                    showOtherItem = true,
                                    otherText = "Ý kiến khác:"
                                });
                            }
                            else
                            {
                                chi_tiet_cau_hoi_list.Add(new
                                {
                                    type = "select",
                                    value_chil = chitietcauhoi.id_chi_tiet_cau_hoi_tieu_de,
                                    name = $"question{elementCounter}",
                                    title = $"{chitietcauhoi.thu_tu}. {chitietcauhoi.ten_cau_hoi}",
                                    isRequired = chitietcauhoi.bat_buoc == 1 ? true : false,
                                    choices = nhieu_lua_chon,
                                    showOtherItem = true,
                                    otherText = "Ý kiến khác:"
                                });
                            }
                        }
                        else
                        {
                            if (chitietcauhoi.dieu_kien_hien_thi != null)
                            {
                                chi_tiet_cau_hoi_list.Add(new
                                {
                                    type = "select",
                                    value_chil = chitietcauhoi.id_chi_tiet_cau_hoi_tieu_de,
                                    name = $"question{elementCounter}",
                                    visible = false,
                                    visibleIf = chitietcauhoi.dieu_kien_hien_thi.Split(',').ToArray(),
                                    title = $"{chitietcauhoi.thu_tu}. {chitietcauhoi.ten_cau_hoi}",
                                    isRequired = chitietcauhoi.bat_buoc == 1 ? true : false,
                                    choices = nhieu_lua_chon
                                });
                            }
                            else
                            {
                                chi_tiet_cau_hoi_list.Add(new
                                {
                                    type = "select",
                                    value_chil = chitietcauhoi.id_chi_tiet_cau_hoi_tieu_de,
                                    name = $"question{elementCounter}",
                                    title = $"{chitietcauhoi.thu_tu}. {chitietcauhoi.ten_cau_hoi}",
                                    isRequired = chitietcauhoi.bat_buoc == 1 ? true : false,
                                    choices = nhieu_lua_chon
                                });
                            }

                        }
                    }
                    else
                    {
                        if (chitietcauhoi.dieu_kien_hien_thi != null)
                        {
                            chi_tiet_cau_hoi_list.Add(new
                            {
                                value_chil = chitietcauhoi.id_chi_tiet_cau_hoi_tieu_de,
                                type = dangCauHoi.id_dang_cau_hoi == 2 ? "comment" : "text",
                                name = $"question{elementCounter}",
                                visible = false,
                                visibleIf = chitietcauhoi.dieu_kien_hien_thi.Split(',').ToArray(),
                                title = $"{chitietcauhoi.thu_tu}. {chitietcauhoi.ten_cau_hoi}",
                                isRequired = chitietcauhoi.bat_buoc == 1 ? true : false,
                            });
                        }
                        else
                        {
                            chi_tiet_cau_hoi_list.Add(new
                            {
                                value_chil = chitietcauhoi.id_chi_tiet_cau_hoi_tieu_de,
                                type = dangCauHoi.id_dang_cau_hoi == 2 ? "comment" : "text",
                                name = $"question{elementCounter}",
                                title = $"{chitietcauhoi.thu_tu}. {chitietcauhoi.ten_cau_hoi}",
                                isRequired = chitietcauhoi.bat_buoc == 1 ? true : false,
                            });
                        }
                    }
                    elementCounter++;
                }

                list_data.Add(new
                {
                    value_title = tieude.id_tieu_de_phieu,
                    name = $"page{pageCounter}",
                    title = $"PHẦN {tieude.thu_tu}. {tieude.ten_tieu_de}",
                    elements = chi_tiet_cau_hoi_list,
                });

                pageCounter++;
            }
            var result = new
            {
                title = get_tieu_de_pks.FirstOrDefault()?.survey?.surveyTitle,
                description = get_tieu_de_pks.FirstOrDefault()?.survey?.surveyDescription,
                pages = list_data
            };
            if (list_data.Count > 0)
            {
                return Ok(new { data = result, success = true });
            }
            else
            {
                return Ok(new { message = "Chưa có dữ liệu câu hỏi bộ phiếu", success = false });
            }
        }

        [HttpPost]
        [Route("api/admin/add-new-title-survey")]
        public IHttpActionResult them_moi_tieu_de_pks(tieu_de_phieu_khao_sat td)
        {
            if (string.IsNullOrEmpty(td.thu_tu))
            {
                return Ok(new { message = "Số thứ tự không được bỏ trống", success = false });
            }
            if (string.IsNullOrEmpty(td.ten_tieu_de))
            {
                return Ok(new { message = "Tên tiêu đề không được bỏ trống", success = false });
            }
            if (db.tieu_de_phieu_khao_sat.FirstOrDefault(x => x.surveyID == td.surveyID && x.thu_tu == td.thu_tu) != null)
            {
                return Ok(new { message = "Số thứ tự này đã tồn tại, vui lòng điền số khác", success = false });
            }

            if (db.tieu_de_phieu_khao_sat.FirstOrDefault(x => x.surveyID == td.surveyID && x.thu_tu == td.thu_tu) != null)
            {
                return Ok(new { message = "Tiêu đề này đã tồn tại, vui lòng nhập tiêu đề khác", success = false });
            }
            db.tieu_de_phieu_khao_sat.Add(td);
            db.SaveChanges();
            return Ok(new { message = "Thêm mới dữ liệu thành công", success = true });
        }
        [HttpPost]
        [Route("api/admin/delete-title-survey")]
        public IHttpActionResult delete_tieu_de_pks(tieu_de_phieu_khao_sat td)
        {
            var checkChiTietTitleList = db.chi_tiet_cau_hoi_tieu_de
                .Where(x => x.id_tieu_de_phieu == td.id_tieu_de_phieu)
                .ToList();
            foreach (var chiTiet in checkChiTietTitleList)
            {
                var checkRdKhac = db.radio_cau_hoi_khac
                    .Where(x => x.id_chi_tiet_cau_hoi_tieu_de == chiTiet.id_chi_tiet_cau_hoi_tieu_de).ToList();

                if (checkRdKhac.Any())
                {
                    foreach (var rd in checkRdKhac)
                    {
                        db.radio_cau_hoi_khac.Remove(rd);
                    }
                }
                db.chi_tiet_cau_hoi_tieu_de.Remove(chiTiet);
            }
            db.SaveChanges();
            var checkTitle = db.tieu_de_phieu_khao_sat
                .SingleOrDefault(x => x.id_tieu_de_phieu == td.id_tieu_de_phieu);
            db.tieu_de_phieu_khao_sat.Remove(checkTitle);
            db.SaveChanges();
            return Ok(new { message = "Xóa dữ liệu thành công", success = true });
        }
        [HttpPost]
        [Route("api/admin/get-info-title-survey")]
        public async Task<IHttpActionResult> get_info_title(tieu_de_phieu_khao_sat td)
        {
            var get_info = await db.tieu_de_phieu_khao_sat
                .Where(x => x.id_tieu_de_phieu == td.id_tieu_de_phieu)
                .Select(x => new
                {
                    x.thu_tu,
                    x.ten_tieu_de
                }).FirstOrDefaultAsync();
            return Ok(get_info);
        }
        [HttpPost]
        [Route("api/admin/update-title-survey")]
        public IHttpActionResult update_title_survey(tieu_de_phieu_khao_sat td)
        {
            var check_title = db.tieu_de_phieu_khao_sat.FirstOrDefault(x => td.id_tieu_de_phieu == x.id_tieu_de_phieu);
            check_title.surveyID = td.surveyID;
            check_title.thu_tu = td.thu_tu;
            check_title.ten_tieu_de = td.ten_tieu_de;
            db.SaveChanges();
            return Ok(new { message = "Cập nhật dữ liệu thành công", success = true });
        }
        [HttpPost]
        [Route("api/admin/option-chi-tiet-cau-hoi")]
        public async Task<IHttpActionResult> load_option(tieu_de_phieu_khao_sat td)
        {
            if (td.surveyID == null)
            {
                return Ok(new { message = "Vui lòng chọn phiếu khảo sát để tạo mới", success = false });
            }
            var get_tieu_de_pks = (await db.tieu_de_phieu_khao_sat
                .Where(x => x.surveyID == td.surveyID)
                .ToListAsync())
                .OrderBy(x => RomanToInt(x.thu_tu))
                .Select(x => new
                {
                    value_title = x.id_tieu_de_phieu,
                    name = $"{x.thu_tu}. {x.ten_tieu_de}"
                })
                .ToList();
            var get_dang_cau_hoi = await db.dang_cau_hoi
                .Select(x => new
                {
                    value_dch = x.id_dang_cau_hoi,
                    name = x.ten_dang_cau_hoi
                }).ToListAsync();
            if (get_tieu_de_pks.Count > 0)
            {
                return Ok(new
                {
                    tieu_de = get_tieu_de_pks,
                    dang_cau_hoi = get_dang_cau_hoi,
                    success = true
                });
            }
            else
            {
                return Ok(new { message = "Chưa có tiêu đề câu hỏi chính nào cho phiếu này, vui lòng tạo mới tiêu đề và quay lại để tiếp tục", success = false });
            }
        }

        [HttpPost]
        [Route("api/admin/save-children-title")]
        public IHttpActionResult save_option([FromBody] ChildrenTitleSurvey rd)
        {
            if (db.chi_tiet_cau_hoi_tieu_de.FirstOrDefault(x => x.id_tieu_de_phieu == rd.id_tieu_de_phieu && x.thu_tu == rd.thu_tu) != null)
            {
                return Ok(new { message = "Số thứ tự này đã tồn tại, vùi lòng nhập số thứ tự khác", success = false });
            }
            if (string.IsNullOrEmpty(rd.ten_cau_hoi))
            {
                return Ok(new { message = "Không được bỏ trống tên tiêu đề con", success = false });
            }
            var chil_title = new chi_tiet_cau_hoi_tieu_de
            {
                thu_tu = rd.thu_tu,
                id_tieu_de_phieu = rd.id_tieu_de_phieu,
                ten_cau_hoi = rd.ten_cau_hoi,
                id_dang_cau_hoi = rd.id_dang_cau_hoi,
                bat_buoc = rd.bat_buoc,
                is_ykienkhac = rd.is_ykienkhac,
                dieu_kien_hien_thi = null,
            };
            db.chi_tiet_cau_hoi_tieu_de.Add(chil_title);
            db.SaveChanges();
            if (!string.IsNullOrEmpty(rd.ten_rd_cau_hoi_khac))
            {
                var options = rd.ten_rd_cau_hoi_khac
                    .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(o => o.Trim())
                    .Where(o => !string.IsNullOrEmpty(o))
                    .ToList();
                var newOptions = new List<radio_cau_hoi_khac>();
                foreach (var option in options)
                {
                    var parts = option.Split(new[] { '.' }, 2);
                    if (parts.Length < 2) continue;

                    if (int.TryParse(parts[0].Trim(), out int thuTu))
                    {
                        newOptions.Add(new radio_cau_hoi_khac
                        {
                            thu_tu = thuTu,
                            ten_rd_cau_hoi_khac = parts[1].Trim(),
                            id_chi_tiet_cau_hoi_tieu_de = chil_title.id_chi_tiet_cau_hoi_tieu_de
                        });
                    }
                }
                db.radio_cau_hoi_khac.AddRange(newOptions);
                db.SaveChanges();
            }
            return Ok(new { message = "Cập nhật dữ liệu thành công", success = true });
        }
        [HttpPost]
        [Route("api/admin/info-children-title")]
        public async Task<IHttpActionResult> load_info_chil_title(ChildrenTitleSurvey chil)
        {
            var get_info_children_title = await db.chi_tiet_cau_hoi_tieu_de
                .Where(x => x.id_chi_tiet_cau_hoi_tieu_de == chil.id_chi_tiet_cau_hoi_tieu_de)
                .Select(x => new
                {
                    x.id_chi_tiet_cau_hoi_tieu_de,
                    x.thu_tu,
                    x.id_tieu_de_phieu,
                    x.ten_cau_hoi,
                    x.id_dang_cau_hoi,
                    x.bat_buoc,
                    x.is_ykienkhac
                })
                .FirstOrDefaultAsync();
            var get_info_rd = await db.radio_cau_hoi_khac
                .Where(x => x.id_chi_tiet_cau_hoi_tieu_de == get_info_children_title.id_chi_tiet_cau_hoi_tieu_de)
                .Select(x => new
                {
                    x.id_rd_cau_hoi_khac,
                    x.thu_tu,
                    x.ten_rd_cau_hoi_khac,
                })
                .ToListAsync();
            return Ok(new { data_chil = get_info_children_title, get_rd = get_info_rd, success = true });

        }
        [HttpPost]
        [Route("api/admin/edit-children-title")]
        public IHttpActionResult edit_children_title(ChildrenTitleSurvey chil)
        {
            var check_chil_title = db.chi_tiet_cau_hoi_tieu_de.FirstOrDefault(x => x.id_chi_tiet_cau_hoi_tieu_de == chil.id_chi_tiet_cau_hoi_tieu_de);
            if (chil.thu_tu == null)
            {
                return Ok(new { message = "Vui lòng không để trống thứ tự", success = false });
            }
            if (string.IsNullOrEmpty(chil.ten_cau_hoi))
            {
                return Ok(new { message = "Vui lòng không để trống tên tiêu đề", success = false });
            }
            check_chil_title.thu_tu = chil.thu_tu;
            check_chil_title.id_tieu_de_phieu = chil.id_tieu_de_phieu;
            check_chil_title.ten_cau_hoi = chil.ten_cau_hoi;
            check_chil_title.id_dang_cau_hoi = chil.id_dang_cau_hoi;
            check_chil_title.bat_buoc = chil.bat_buoc;
            check_chil_title.is_ykienkhac = chil.is_ykienkhac;
            check_chil_title.dieu_kien_hien_thi = null;
            db.SaveChanges();
            if (string.IsNullOrEmpty(chil.ten_rd_cau_hoi_khac))
            {
                var existingRadios = db.radio_cau_hoi_khac.Where(x => x.id_chi_tiet_cau_hoi_tieu_de == chil.id_chi_tiet_cau_hoi_tieu_de).ToList();
                db.radio_cau_hoi_khac.RemoveRange(existingRadios);
                db.SaveChanges();
            }
            else
            {
                var existingRadios = db.radio_cau_hoi_khac.Where(x => x.id_chi_tiet_cau_hoi_tieu_de == chil.id_chi_tiet_cau_hoi_tieu_de).ToList();
                db.radio_cau_hoi_khac.RemoveRange(existingRadios);
                db.SaveChanges();
                var options = chil.ten_rd_cau_hoi_khac
                    .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(o => o.Trim())
                    .Where(o => !string.IsNullOrEmpty(o))
                    .ToList();
                var newOptions = new List<radio_cau_hoi_khac>();
                foreach (var option in options)
                {
                    var parts = option.Split(new[] { '.' }, 2);
                    if (parts.Length < 2) continue;

                    if (int.TryParse(parts[0].Trim(), out int thuTu))
                    {
                        newOptions.Add(new radio_cau_hoi_khac
                        {
                            thu_tu = thuTu,
                            ten_rd_cau_hoi_khac = parts[1].Trim(),
                            id_chi_tiet_cau_hoi_tieu_de = check_chil_title.id_chi_tiet_cau_hoi_tieu_de
                        });
                    }
                }
                db.radio_cau_hoi_khac.AddRange(newOptions);
                db.SaveChanges();
            }
            return Ok(new { message = "Cập nhật dữ liệu thành công", success = true });
        }

        [HttpPost]
        [Route("api/admin/delete-children-title")]
        public IHttpActionResult delete_children_title(ChildrenTitleSurvey chil)
        {
            var check_rd = db.radio_cau_hoi_khac.Where(x => x.id_chi_tiet_cau_hoi_tieu_de == chil.id_chi_tiet_cau_hoi_tieu_de).ToList();
            if (check_rd.Any())
            {
                db.radio_cau_hoi_khac.RemoveRange(check_rd);
                db.SaveChanges();
            }
            var check_children_title = db.chi_tiet_cau_hoi_tieu_de.FirstOrDefault(x => x.id_chi_tiet_cau_hoi_tieu_de == chil.id_chi_tiet_cau_hoi_tieu_de);
            if (check_children_title != null)
            {
                db.chi_tiet_cau_hoi_tieu_de.Remove(check_children_title);
                db.SaveChanges();
            }
            return Ok(new { message = "Xóa dữ liệu thành công", success = true });
        }
        [HttpPost]
        [Route("api/admin/view-final-survey")]
        public async Task<IHttpActionResult> xem_truoc_va_xuat_ban(survey sv)
        {
            var get_tieu_de_pks = (await db.tieu_de_phieu_khao_sat
                .Where(x => x.surveyID == sv.surveyID)
                .ToListAsync())
                .OrderBy(x => RomanToInt(x.thu_tu))
                .ToList();

            var list_data = new List<dynamic>();
            int pageCounter = 1;
            int elementCounter = 1;

            foreach (var tieude in get_tieu_de_pks)
            {
                var get_chi_tiet_cau_hoi_tieu_de = await db.chi_tiet_cau_hoi_tieu_de
                    .Where(x => x.id_tieu_de_phieu == tieude.id_tieu_de_phieu)
                    .OrderBy(x => x.thu_tu)
                    .ToListAsync();

                var chi_tiet_cau_hoi_list = new List<dynamic>();

                foreach (var chitietcauhoi in get_chi_tiet_cau_hoi_tieu_de)
                {
                    var dangCauHoi = await db.dang_cau_hoi
                        .Where(x => x.id_dang_cau_hoi == chitietcauhoi.id_dang_cau_hoi)
                        .FirstOrDefaultAsync();
                    var nhieu_lua_chon = new List<dynamic>();

                    if (dangCauHoi.id_dang_cau_hoi == 3)
                    {
                        var get_radio_cau_hoi_khac = await db.radio_cau_hoi_khac
                                .Where(x => x.id_chi_tiet_cau_hoi_tieu_de == chitietcauhoi.id_chi_tiet_cau_hoi_tieu_de)
                                .OrderBy(x => x.thu_tu)
                                .ToListAsync();

                        foreach (var getradiocauhoikhac in get_radio_cau_hoi_khac)
                        {
                            nhieu_lua_chon.Add(new
                            {
                                name = $"question{elementCounter}_{getradiocauhoikhac.thu_tu}",
                                text = getradiocauhoikhac.ten_rd_cau_hoi_khac,
                            });
                        }
                        if (chitietcauhoi.is_ykienkhac == 1)
                        {
                            if (chitietcauhoi.dieu_kien_hien_thi != null)
                            {
                                chi_tiet_cau_hoi_list.Add(new
                                {
                                    type = "radiogroup",
                                    name = $"question{elementCounter}",
                                    visible = false,
                                    visibleIf = chitietcauhoi.dieu_kien_hien_thi.Split(',').ToArray(),
                                    title = $"{chitietcauhoi.thu_tu}. {chitietcauhoi.ten_cau_hoi}",
                                    isRequired = chitietcauhoi.bat_buoc == 1 ? true : false,
                                    choices = nhieu_lua_chon,
                                    showOtherItem = true,
                                    otherText = "Ý kiến khác:"
                                });
                            }
                            else
                            {
                                chi_tiet_cau_hoi_list.Add(new
                                {
                                    type = "radiogroup",
                                    name = $"question{elementCounter}",
                                    title = $"{chitietcauhoi.thu_tu}. {chitietcauhoi.ten_cau_hoi}",
                                    isRequired = chitietcauhoi.bat_buoc == 1 ? true : false,
                                    choices = nhieu_lua_chon,
                                    showOtherItem = true,
                                    otherText = "Ý kiến khác:"
                                });
                            }

                        }
                        else
                        {
                            if (chitietcauhoi.dieu_kien_hien_thi != null)
                            {
                                chi_tiet_cau_hoi_list.Add(new
                                {
                                    type = "radiogroup",
                                    name = $"question{elementCounter}",
                                    visible = false,
                                    visibleIf = chitietcauhoi.dieu_kien_hien_thi.Split(',').ToArray(),
                                    title = $"{chitietcauhoi.thu_tu}. {chitietcauhoi.ten_cau_hoi}",
                                    isRequired = chitietcauhoi.bat_buoc == 1 ? true : false,
                                    choices = nhieu_lua_chon,
                                });
                            }
                            else
                            {
                                chi_tiet_cau_hoi_list.Add(new
                                {
                                    type = "radiogroup",
                                    name = $"question{elementCounter}",
                                    title = $"{chitietcauhoi.thu_tu}. {chitietcauhoi.ten_cau_hoi}",
                                    isRequired = chitietcauhoi.bat_buoc == 1 ? true : false,
                                    choices = nhieu_lua_chon,
                                });
                            }
                        }
                    }
                    else if (dangCauHoi.id_dang_cau_hoi == 4)
                    {
                        var get_radio_cau_hoi_khac = await db.radio_cau_hoi_khac
                                .Where(x => x.id_chi_tiet_cau_hoi_tieu_de == chitietcauhoi.id_chi_tiet_cau_hoi_tieu_de)
                                .OrderBy(x => x.thu_tu)
                                .ToListAsync();

                        foreach (var getradiocauhoikhac in get_radio_cau_hoi_khac)
                        {
                            nhieu_lua_chon.Add(new
                            {
                                name = $"question{elementCounter}_{getradiocauhoikhac.thu_tu}",
                                text = getradiocauhoikhac.ten_rd_cau_hoi_khac,
                            });
                        }
                        if (chitietcauhoi.is_ykienkhac == 1)
                        {
                            if (chitietcauhoi.dieu_kien_hien_thi != null)
                            {
                                chi_tiet_cau_hoi_list.Add(new
                                {
                                    type = "checkbox",
                                    name = $"question{elementCounter}",
                                    visible = false,
                                    visibleIf = chitietcauhoi.dieu_kien_hien_thi.Split(',').ToArray(),
                                    title = $"{chitietcauhoi.thu_tu}. {chitietcauhoi.ten_cau_hoi}",
                                    isRequired = chitietcauhoi.bat_buoc == 1 ? true : false,
                                    choices = nhieu_lua_chon,
                                    showOtherItem = true,
                                    otherText = "Ý kiến khác:"
                                });
                            }
                            else
                            {
                                chi_tiet_cau_hoi_list.Add(new
                                {
                                    type = "checkbox",
                                    name = $"question{elementCounter}",
                                    title = $"{chitietcauhoi.thu_tu}. {chitietcauhoi.ten_cau_hoi}",
                                    isRequired = chitietcauhoi.bat_buoc == 1 ? true : false,
                                    choices = nhieu_lua_chon,
                                    showOtherItem = true,
                                    otherText = "Ý kiến khác:"
                                });
                            }
                        }
                        else
                        {
                            if (chitietcauhoi.dieu_kien_hien_thi != null)
                            {
                                chi_tiet_cau_hoi_list.Add(new
                                {
                                    type = "checkbox",
                                    name = $"question{elementCounter}",
                                    visible = false,
                                    visibleIf = chitietcauhoi.dieu_kien_hien_thi.Split(',').ToArray(),
                                    title = $"{chitietcauhoi.thu_tu}. {chitietcauhoi.ten_cau_hoi}",
                                    isRequired = chitietcauhoi.bat_buoc == 1 ? true : false,
                                    choices = nhieu_lua_chon
                                });
                            }
                            else
                            {
                                chi_tiet_cau_hoi_list.Add(new
                                {
                                    type = "checkbox",
                                    name = $"question{elementCounter}",
                                    title = $"{chitietcauhoi.thu_tu}. {chitietcauhoi.ten_cau_hoi}",
                                    isRequired = chitietcauhoi.bat_buoc == 1 ? true : false,
                                    choices = nhieu_lua_chon
                                });
                            }

                        }
                    }
                    else if (dangCauHoi.id_dang_cau_hoi == 5)
                    {
                        var get_radio_cau_hoi_khac = await db.radio_cau_hoi_khac
                                .Where(x => x.id_chi_tiet_cau_hoi_tieu_de == chitietcauhoi.id_chi_tiet_cau_hoi_tieu_de)
                                .OrderBy(x => x.thu_tu)
                                .ToListAsync();

                        foreach (var getradiocauhoikhac in get_radio_cau_hoi_khac)
                        {
                            nhieu_lua_chon.Add(new
                            {
                                name = $"question{elementCounter}_{getradiocauhoikhac.thu_tu}",
                                text = getradiocauhoikhac.ten_rd_cau_hoi_khac,
                            });
                        }
                        if (chitietcauhoi.is_ykienkhac == 1)
                        {
                            if (chitietcauhoi.dieu_kien_hien_thi != null)
                            {
                                chi_tiet_cau_hoi_list.Add(new
                                {
                                    type = "select",
                                    name = $"question{elementCounter}",
                                    visible = false,
                                    visibleIf = chitietcauhoi.dieu_kien_hien_thi.Split(',').ToArray(),
                                    title = $"{chitietcauhoi.thu_tu}. {chitietcauhoi.ten_cau_hoi}",
                                    isRequired = chitietcauhoi.bat_buoc == 1 ? true : false,
                                    choices = nhieu_lua_chon,
                                    showOtherItem = true,
                                    otherText = "Ý kiến khác:"
                                });
                            }
                            else
                            {
                                chi_tiet_cau_hoi_list.Add(new
                                {
                                    type = "select",
                                    name = $"question{elementCounter}",
                                    title = $"{chitietcauhoi.thu_tu}. {chitietcauhoi.ten_cau_hoi}",
                                    isRequired = chitietcauhoi.bat_buoc == 1 ? true : false,
                                    choices = nhieu_lua_chon,
                                    showOtherItem = true,
                                    otherText = "Ý kiến khác:"
                                });
                            }
                        }
                        else
                        {
                            if (chitietcauhoi.dieu_kien_hien_thi != null)
                            {
                                chi_tiet_cau_hoi_list.Add(new
                                {
                                    type = "select",
                                    name = $"question{elementCounter}",
                                    visible = false,
                                    visibleIf = chitietcauhoi.dieu_kien_hien_thi.Split(',').ToArray(),
                                    title = $"{chitietcauhoi.thu_tu}. {chitietcauhoi.ten_cau_hoi}",
                                    isRequired = chitietcauhoi.bat_buoc == 1 ? true : false,
                                    choices = nhieu_lua_chon
                                });
                            }
                            else
                            {
                                chi_tiet_cau_hoi_list.Add(new
                                {
                                    type = "select",
                                    name = $"question{elementCounter}",
                                    title = $"{chitietcauhoi.thu_tu}. {chitietcauhoi.ten_cau_hoi}",
                                    isRequired = chitietcauhoi.bat_buoc == 1 ? true : false,
                                    choices = nhieu_lua_chon
                                });
                            }

                        }
                    }
                    else
                    {
                        if (chitietcauhoi.dieu_kien_hien_thi != null)
                        {
                            chi_tiet_cau_hoi_list.Add(new
                            {
                                type = dangCauHoi.id_dang_cau_hoi == 2 ? "comment" : "text",
                                name = $"question{elementCounter}",
                                visible = false,
                                visibleIf = chitietcauhoi.dieu_kien_hien_thi.Split(',').ToArray(),
                                title = $"{chitietcauhoi.thu_tu}. {chitietcauhoi.ten_cau_hoi}",
                                isRequired = chitietcauhoi.bat_buoc == 1 ? true : false,
                            });
                        }
                        else
                        {
                            chi_tiet_cau_hoi_list.Add(new
                            {
                                type = dangCauHoi.id_dang_cau_hoi == 2 ? "comment" : "text",
                                name = $"question{elementCounter}",
                                title = $"{chitietcauhoi.thu_tu}. {chitietcauhoi.ten_cau_hoi}",
                                isRequired = chitietcauhoi.bat_buoc == 1 ? true : false,
                            });
                        }
                    }
                    elementCounter++;
                }

                list_data.Add(new
                {
                    name = $"page{pageCounter}",
                    title = $"PHẦN {tieude.thu_tu}. {tieude.ten_tieu_de}",
                    elements = chi_tiet_cau_hoi_list,
                });

                pageCounter++;
            }
            var result = new
            {
                title = get_tieu_de_pks.FirstOrDefault()?.survey?.surveyTitle,
                description = get_tieu_de_pks.FirstOrDefault()?.survey?.surveyDescription,
                pages = list_data
            };
            if (list_data.Count > 0)
            {
                return Ok(new { data = JsonConvert.SerializeObject(result), success = true });
            }
            else
            {
                return Ok(new { message = "Chưa có dữ liệu câu hỏi bộ phiếu", success = false });
            }
        }
        public int RomanToInt(string roman)
        {
            var romanMap = new Dictionary<char, int>
            {
                {'I', 1},
                {'V', 5},
                {'X', 10},
                {'L', 50},
                {'C', 100},
                {'D', 500},
                {'M', 1000}
            };
            int number = 0;
            for (int i = 0; i < roman.Length; i++)
            {
                if (i + 1 < roman.Length && romanMap[roman[i]] < romanMap[roman[i + 1]])
                {
                    number -= romanMap[roman[i]];
                }
                else
                {
                    number += romanMap[roman[i]];
                }
            }
            return number;
        }
    }
}
