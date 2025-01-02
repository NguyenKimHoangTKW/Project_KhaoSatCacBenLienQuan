using CTDT.Helper;
using CTDT.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace CTDT.Areas.Admin.Controllers
{
    public class PhieuKhaoSatAdminAPIController : ApiController
    {
        dbSurveyEntities db = new dbSurveyEntities();
        public users user;
        public PhieuKhaoSatAdminAPIController()
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
                        ten_khoa = x.ctdt.khoa.ten_khoa ?? "",
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
        [Route("api/admin/xoa-du-lieu-phieu-khao-sat")]
        public IHttpActionResult delete_survey(survey sv)
        {
            var check_survey = db.survey.SingleOrDefault(x => x.surveyID == sv.surveyID);
            var check_answer = db.answer_response.SingleOrDefault(x => x.surveyID == sv.surveyID);
            var check_nguoi_hoc_khao_sat = db.nguoi_hoc_khao_sat.SingleOrDefault(x => x.surveyID == sv.surveyID);
            var check_nguoi_hoc_hoc_phan = db.nguoi_hoc_dang_co_hoc_phan.SingleOrDefault(x => x.surveyID == sv.surveyID);
            var check_cbvc_khao_sat = db.cbvc_khao_sat.SingleOrDefault(x => x.surveyID == sv.surveyID);
            if (check_answer != null)
            {
                db.answer_response.Remove(check_answer);
                db.SaveChanges();
            }
            if (check_nguoi_hoc_khao_sat != null)
            {
                db.nguoi_hoc_khao_sat.Remove(check_nguoi_hoc_khao_sat);
                db.SaveChanges();
            }
            if (check_nguoi_hoc_hoc_phan != null)
            {
                db.nguoi_hoc_dang_co_hoc_phan.Remove(check_nguoi_hoc_hoc_phan);
                db.SaveChanges();
            }
            if (check_cbvc_khao_sat != null)
            {
                db.cbvc_khao_sat.Remove(check_cbvc_khao_sat);
                db.SaveChanges();
            }
            db.survey.Remove(check_survey);
            db.SaveChanges();
            return Ok(new { message = "Xóa dữ liệu thành công", success = true });
        }


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
            return Ok(new
            {
                tieu_de = get_tieu_de_pks,
                dang_cau_hoi = get_dang_cau_hoi
            });
        }

        [HttpPost]
        [Route("api/admin/saveRadioOptions")]
        public IHttpActionResult save_option([FromBody] radio_cau_hoi_khac rd)
        {
            var options = rd.ten_rd_cau_hoi_khac
                            .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(o => o.Trim())
                            .Where(o => !string.IsNullOrEmpty(o));

            foreach (var option in options)
            {
                var parts = option.Split(new[] { '.' }, 2);
                if (parts.Length < 2) continue;

                int thuTu;
                if (!int.TryParse(parts[0].Trim(), out thuTu)) continue;

                var newOption = new radio_cau_hoi_khac
                {
                    thu_tu = thuTu,
                    ten_rd_cau_hoi_khac = parts[1].Trim(),
                    id_chi_tiet_cau_hoi_tieu_de = 2
                };
                db.radio_cau_hoi_khac.Add(newOption);
            }
            db.SaveChanges();
            return Ok(new { message = "Options saved successfully!" });
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
