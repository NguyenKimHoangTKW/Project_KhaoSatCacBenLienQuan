using CTDT.Helper;
using CTDT.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.UI.WebControls;
using static CTDT.Areas.CTDT.Controllers.ThongKeKetQuaKhaoSatCTDTAPIController;

namespace CTDT.Areas.CTDT.Controllers
{
    public class ThongKeKetQuaKhaoSatTheoYeuCauCTDTAPIController : ApiController
    {
        dbSurveyEntities db = new dbSurveyEntities();
        private users user;
        private int unixTimestamp;
        public ThongKeKetQuaKhaoSatTheoYeuCauCTDTAPIController()
        {
            user = SessionHelper.GetUser();
            DateTime now = DateTime.UtcNow;
            unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }
        public class DoiTuongTheoYeuCau
        {
            public int surveyID { get; set; }
            public int id_ctdt { get; set; }
        }
        public class save_thong_ke_yeu_cau
        {
            public int surveyID { get; set; }
            public int id_ctdt { get; set; }
            public List<int> list_value { get; set; }
        }
        #region Chọn đáp viên thống kê theo yêu cầu
        [HttpPost]
        [Route("api/ctdt/load-doi-tuong-thong-ke-theo-yeu-cau")]
        public async Task<IHttpActionResult> load_doi_tuong_yeu_cau(DoiTuongTheoYeuCau doituong)
        {

            var check_survey = await db.survey.FirstOrDefaultAsync(x => x.surveyID == doituong.surveyID);
            var data_list = new List<dynamic>();
            var check_object = new List<dynamic>();
            if (check_survey.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu giảng viên")
            {
                bool check_giang_vien = new[] { 3 }.Contains(check_survey.id_loaikhaosat);
                bool check_cbvc = new[] { 8 }.Contains(check_survey.id_loaikhaosat);
                if (check_cbvc)
                {
                    var check_cbvc_khao_sat = await db.cbvc_khao_sat
                        .Where(x => x.surveyID == doituong.surveyID && x.CanBoVienChuc.id_chuongtrinhdaotao == doituong.id_ctdt)
                        .Select(x => new
                        {
                            id = x.id_cbvc,
                            x.CanBoVienChuc.TenCBVC,
                            ctdt = x.CanBoVienChuc.ctdt.ten_ctdt,
                        })
                        .ToListAsync();
                    data_list.AddRange(check_cbvc_khao_sat);
                    check_object.Add(new
                    {
                        is_cbvc = true
                    });
                }
                else if (check_giang_vien)
                {
                    var check_giang_vien_khao_sat = await db.answer_response
                        .Where(x => x.surveyID == doituong.surveyID && x.id_ctdt == doituong.id_ctdt)
                        .Select(x => new
                        {
                            id = x.CanBoVienChuc.id_CBVC,
                            x.CanBoVienChuc.TenCBVC,
                            ctdt = x.ctdt.ten_ctdt
                        })
                        .ToListAsync();
                    data_list.AddRange(check_giang_vien_khao_sat);
                    check_object.Add(new
                    {
                        is_cbvc = true
                    });
                }
            }
            else if (check_survey.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu doanh nghiệp")
            {
                var ctdt = await db.answer_response
                    .Where(x => x.surveyID == doituong.surveyID && x.id_ctdt == doituong.id_ctdt)
                    .Select(x => new
                    {
                        id = x.id,
                        ten_user = x.users.firstName + " " + x.users.lastName,
                        email = x.users.email,
                        ctdt = x.ctdt.ten_ctdt
                    })
                    .ToListAsync();
                data_list.AddRange(ctdt);
                check_object.Add(new
                {
                    is_doanh_nghiep = true
                });
            }
            else if (check_survey.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu người học có học phần")
            {
                var check_hoc_phan = await db.nguoi_hoc_dang_co_hoc_phan.Where(x => x.surveyID == doituong.surveyID && x.sinhvien.lop.id_ctdt == doituong.id_ctdt)
                    .Select(x => new
                    {
                        id = x.id_nguoi_hoc_by_hoc_phan,
                        mon_hoc = x.mon_hoc.ten_mon_hoc,
                        x.sinhvien.lop.ma_lop,
                        giang_vien_giang_day = x.CanBoVienChuc.TenCBVC,
                        ma_nguoi_hoc = x.sinhvien.ma_sv,
                        ten_nguoi_hoc = x.sinhvien.hovaten
                    })
                    .ToListAsync();
                data_list.AddRange(check_hoc_phan);
                check_object.Add(new
                {
                    is_nguoi_hoc_co_hoc_phan_khao_sat = true
                });
            }
            else if (check_survey.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu người học" || check_survey.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu cựu người học")
            {
                var check_nguoi_hoc = await db.nguoi_hoc_khao_sat
                    .Where(x => x.surveyID == doituong.surveyID && x.sinhvien.lop.id_ctdt == doituong.id_ctdt)
                    .Select(x => new
                    {
                        id = x.id_nguoi_hoc_khao_sat,
                        ma_nguoi_hoc = x.sinhvien.ma_sv,
                        ten_nguoi_hoc = x.sinhvien.hovaten,
                        x.sinhvien.lop.ma_lop,
                        x.sinhvien.lop.ctdt.ten_ctdt
                    })
                    .ToListAsync();
                data_list.AddRange(check_nguoi_hoc);
                check_object.Add(new
                {
                    is_nguoi_hoc_khao_sat = true
                });
            }
            if (data_list.Count > 0)
            {
                return Ok(new { data = JsonConvert.SerializeObject(data_list), check_object = check_object, success = true });
            }
            else
            {
                return Ok(new { message = "Chưa có dữ liệu khảo sát trong năm học để thống kê", success = false });
            }
        }
        [HttpPost]
        [Route("api/ctdt/load-dap-vien-chon-theo-yeu-cau")]
        public async Task<IHttpActionResult> load_dap_vien_chon_theo_yeu_cau(DoiTuongTheoYeuCau doituong)
        {
            if (doituong.surveyID == 0)
            {
                return Ok(new { message = "Bạn chưa chọn phiếu khảo sát để xem chi tiết các đáp viên đã chọn", success = false });
            }
            var check_survey = await db.survey.FirstOrDefaultAsync(x => x.surveyID == doituong.surveyID);
            var data_list = new List<dynamic>();
            var check_object = new List<dynamic>();
            var check_dap_vien_yeu_cau = await db.thong_ke_theo_yeu_cau.Where(x => x.surveyID == doituong.surveyID && x.id_ctdt == doituong.id_ctdt).ToListAsync();
            if (check_survey.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu giảng viên")
            {
                bool check_giang_vien = new[] { 3 }.Contains(check_survey.id_loaikhaosat);
                bool check_cbvc = new[] { 8 }.Contains(check_survey.id_loaikhaosat);
                if (check_cbvc)
                {
                    foreach (var items in check_dap_vien_yeu_cau)
                    {
                        var get_cbvc_khao_sat = await db.cbvc_khao_sat
                            .Where(x => x.id_cbvc == items.id_cbvc_khao_sat)
                            .Select(x => new
                            {
                                value = items.id_thong_ke_theo_yeu_cau,
                                id = x.id_cbvc,
                                x.CanBoVienChuc.TenCBVC,
                                ctdt = x.CanBoVienChuc.ctdt.ten_ctdt,
                            })
                            .ToListAsync();
                        data_list.AddRange(get_cbvc_khao_sat);
                    }
                    check_object.Add(new
                    {
                        is_cbvc = true
                    });
                }
                else if (check_giang_vien)
                {
                    foreach (var items in check_dap_vien_yeu_cau)
                    {
                        var get_giang_vien = await db.answer_response
                            .Where(x => x.id == items.id_answer)
                            .Select(x => new
                            {
                                value = items.id_thong_ke_theo_yeu_cau,
                                id = x.CanBoVienChuc.id_CBVC,
                                x.CanBoVienChuc.TenCBVC,
                                ctdt = x.ctdt.ten_ctdt
                            })
                            .ToListAsync();
                        data_list.AddRange(get_giang_vien);
                    }
                    check_object.Add(new
                    {
                        is_cbvc = true
                    });
                }
            }
            else if (check_survey.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu doanh nghiệp")
            {
                foreach (var items in check_dap_vien_yeu_cau)
                {
                    var get_doanh_nghiep = await db.answer_response
                        .Where(x => x.id == items.id_answer)
                        .Select(x => new
                        {
                            value = items.id_thong_ke_theo_yeu_cau,
                            id = x.id,
                            ten_user = x.users.firstName + " " + x.users.lastName,
                            email = x.users.email,
                            ctdt = x.ctdt.ten_ctdt
                        })
                        .ToListAsync();
                    data_list.AddRange(get_doanh_nghiep);
                }
                check_object.Add(new
                {
                    is_doanh_nghiep = true
                });
            }
            else if (check_survey.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu người học có học phần")
            {
                foreach (var items in check_dap_vien_yeu_cau)
                {
                    var get_nguoi_hoc_co_hoc_phan_khao_sat = await db.nguoi_hoc_dang_co_hoc_phan
                        .Where(x => x.id_nguoi_hoc_by_hoc_phan == items.id_nguoi_hoc_co_hoc_phan_khao_sat)
                        .Select(x => new
                        {
                            value = items.id_thong_ke_theo_yeu_cau,
                            id = x.id_nguoi_hoc_by_hoc_phan,
                            mon_hoc = x.mon_hoc.ten_mon_hoc,
                            x.sinhvien.lop.ma_lop,
                            giang_vien_giang_day = x.CanBoVienChuc.TenCBVC,
                            ma_nguoi_hoc = x.sinhvien.ma_sv,
                            ten_nguoi_hoc = x.sinhvien.hovaten

                        })
                        .ToListAsync();
                    data_list.AddRange(get_nguoi_hoc_co_hoc_phan_khao_sat);
                }
                check_object.Add(new
                {
                    is_nguoi_hoc_co_hoc_phan_khao_sat = true
                });
            }
            else if (check_survey.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu người học" || check_survey.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu cựu người học")
            {
                foreach (var items in check_dap_vien_yeu_cau)
                {
                    var get_nguoi_hoc_khao_sat = await db.nguoi_hoc_khao_sat
                        .Where(x => x.id_nguoi_hoc_khao_sat == items.id_nguoi_hoc_khao_sat)
                        .Select(x => new
                        {
                            value = items.id_thong_ke_theo_yeu_cau,
                            id = x.id_nguoi_hoc_khao_sat,
                            ma_nguoi_hoc = x.sinhvien.ma_sv,
                            ten_nguoi_hoc = x.sinhvien.hovaten,
                            x.sinhvien.lop.ma_lop,
                            x.sinhvien.lop.ctdt.ten_ctdt

                        })
                        .ToListAsync();
                    data_list.AddRange(get_nguoi_hoc_khao_sat);
                }
                check_object.Add(new
                {
                    is_nguoi_hoc_khao_sat = true
                });
            }
            if (data_list.Count > 0)
            {
                return Ok(new { data = JsonConvert.SerializeObject(data_list), check_object = check_object, success = true });
            }
            else
            {
                return Ok(new { message = "Chưa có dữ liệu thống kê đáp viên theo yêu cầu", success = false });
            }
        }
        [HttpPost]
        [Route("api/ctdt/save-doi-tuong-khao-sat-theo-yeu-cau")]
        public async Task<IHttpActionResult> save_doi_tuong_thong_ke_yeu_cau(save_thong_ke_yeu_cau items)
        {
            var check_survey = await db.survey.FirstOrDefaultAsync(x => x.surveyID == items.surveyID);
            if (check_survey.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu giảng viên")
            {
                bool check_giang_vien = new[] { 3 }.Contains(check_survey.id_loaikhaosat);
                bool check_cbvc = new[] { 8 }.Contains(check_survey.id_loaikhaosat);
                if (check_cbvc)
                {
                    var existingRecords = await db.thong_ke_theo_yeu_cau.Where(x => x.surveyID == items.surveyID).ToListAsync();
                    if (existingRecords.Any())
                    {
                        db.thong_ke_theo_yeu_cau.RemoveRange(existingRecords);
                        await db.SaveChangesAsync();
                    }
                    foreach (var insertCBVCKS in items.list_value)
                    {
                        var newRecords = new thong_ke_theo_yeu_cau
                        {
                            surveyID = items.surveyID,
                            id_cbvc_khao_sat = insertCBVCKS,
                            id_nguoi_lap_thong_ke = user.id_users,
                            id_ctdt = items.id_ctdt,
                            ngay_tao = unixTimestamp,
                            ngay_cap_nhat = unixTimestamp,
                        };
                        db.thong_ke_theo_yeu_cau.Add(newRecords);
                    }
                    await db.SaveChangesAsync();
                }
                else if (check_giang_vien)
                {
                    var existingRecords = await db.thong_ke_theo_yeu_cau.Where(x => x.surveyID == items.surveyID).ToListAsync();
                    if (existingRecords.Any())
                    {
                        db.thong_ke_theo_yeu_cau.RemoveRange(existingRecords);
                        await db.SaveChangesAsync();
                    }
                    foreach (var insertCBVCKS in items.list_value)
                    {
                        var newRecords = new thong_ke_theo_yeu_cau
                        {
                            surveyID = items.surveyID,
                            id_answer = insertCBVCKS,
                            id_nguoi_lap_thong_ke = user.id_users,
                            id_ctdt = items.id_ctdt,
                            ngay_tao = unixTimestamp,
                            ngay_cap_nhat = unixTimestamp,
                        };
                        db.thong_ke_theo_yeu_cau.Add(newRecords);
                    }
                    await db.SaveChangesAsync();
                }
            }
            else if (check_survey.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu doanh nghiệp")
            {
                var existingRecords = await db.thong_ke_theo_yeu_cau.Where(x => x.surveyID == items.surveyID).ToListAsync();
                if (existingRecords.Any())
                {
                    db.thong_ke_theo_yeu_cau.RemoveRange(existingRecords);
                    await db.SaveChangesAsync();
                }
                foreach (var insertCBVCKS in items.list_value)
                {
                    var newRecords = new thong_ke_theo_yeu_cau
                    {
                        surveyID = items.surveyID,
                        id_cbvc_khao_sat = insertCBVCKS,
                        id_nguoi_lap_thong_ke = user.id_users,
                        id_ctdt = items.id_ctdt,
                        ngay_tao = unixTimestamp,
                        ngay_cap_nhat = unixTimestamp,
                    };
                    db.thong_ke_theo_yeu_cau.Add(newRecords);
                }
                await db.SaveChangesAsync();
            }
            else if (check_survey.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu người học có học phần")
            {
                var existingRecords = await db.thong_ke_theo_yeu_cau.Where(x => x.surveyID == items.surveyID).ToListAsync();
                if (existingRecords.Any())
                {
                    db.thong_ke_theo_yeu_cau.RemoveRange(existingRecords);
                    await db.SaveChangesAsync();
                }
                foreach (var insertCBVCKS in items.list_value)
                {
                    var newRecords = new thong_ke_theo_yeu_cau
                    {
                        surveyID = items.surveyID,
                        id_nguoi_hoc_co_hoc_phan_khao_sat = insertCBVCKS,
                        id_nguoi_lap_thong_ke = user.id_users,
                        id_ctdt = items.id_ctdt,
                        ngay_tao = unixTimestamp,
                        ngay_cap_nhat = unixTimestamp,
                    };
                    db.thong_ke_theo_yeu_cau.Add(newRecords);
                }
                await db.SaveChangesAsync();
            }
            else if (check_survey.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu người học" || check_survey.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu cựu người học")
            {
                var existingRecords = await db.thong_ke_theo_yeu_cau.Where(x => x.surveyID == items.surveyID).ToListAsync();
                if (existingRecords.Any())
                {
                    db.thong_ke_theo_yeu_cau.RemoveRange(existingRecords);
                    await db.SaveChangesAsync();
                }
                foreach (var insertCBVCKS in items.list_value)
                {
                    var newRecords = new thong_ke_theo_yeu_cau
                    {
                        surveyID = items.surveyID,
                        id_nguoi_hoc_khao_sat = insertCBVCKS,
                        id_nguoi_lap_thong_ke = user.id_users,
                        id_ctdt = items.id_ctdt,
                        ngay_tao = unixTimestamp,
                        ngay_cap_nhat = unixTimestamp,
                    };
                    db.thong_ke_theo_yeu_cau.Add(newRecords);
                }
                await db.SaveChangesAsync();
            }
            return Ok(new { message = "Cập nhật thành công." });
        }
        [HttpPost]
        [Route("api/ctdt/delete-dap-vien-theo-yeu-cau")]
        public async Task<IHttpActionResult> delete_thong_ke_theo_yeu_cau(thong_ke_theo_yeu_cau thongKe)
        {
            var check_thong_ke_theo_yeu_cau = await db.thong_ke_theo_yeu_cau.FindAsync(thongKe.id_thong_ke_theo_yeu_cau);
            if (check_thong_ke_theo_yeu_cau != null)
            {
                db.thong_ke_theo_yeu_cau.Remove(check_thong_ke_theo_yeu_cau);
                await db.SaveChangesAsync();
            }
            else
            {
                return Ok(new { message = "Không tìm thấy giá trị để cập nhật", success = false });
            }
            return Ok(new { message = "Cập nhật dữ liệu thành công", success = true });
        }

        #endregion

        #region Thống kê theo yêu cầu

        [HttpPost]
        [Route("api/ctdt/thong-ke-ket-qua-khao-sat-theo-yeu-cau")]
        public async Task<IHttpActionResult> giam_sat_ket_qua_khao_sat(get_option_ctdt aw)
        {
            var _checkctdt = await db.ctdt.FirstOrDefaultAsync(x => x.id_ctdt == aw.id_ctdt);
            var _check_yeu_cau = await db.thong_ke_theo_yeu_cau
                .Where(x => x.surveyID == aw.surveyID && x.id_ctdt == aw.id_ctdt)
                .ToListAsync();
            var check_survey = db.survey.FirstOrDefault(x => x.surveyID == aw.surveyID);
            var list_count = new List<dynamic>();
                var all_data = new List<dynamic>();

            foreach (var items in _check_yeu_cau)
            {
                List<answer_response> check_answer = null;

                if (check_survey.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu người học có học phần")
                {
                    check_answer = db.answer_response
                        .Where(x => x.surveyID == aw.surveyID
                           && x.id_sv == items.nguoi_hoc_dang_co_hoc_phan.id_sinh_vien
                           && x.id_CBVC == items.nguoi_hoc_dang_co_hoc_phan.id_giang_vvien
                           && x.id_mh == items.nguoi_hoc_dang_co_hoc_phan.id_mon_hoc)
                        .ToList();
                }

                var get_data = check_answer
                    .Select(x => new
                    {
                        JsonAnswer = x.json_answer,
                        SurveyJson = x.survey.surveyData
                    })
                    .ToList();
                all_data.AddRange(get_data);
            }
            if (all_data.Count > 0)
            {
                list_count = await load_ty_le_khong_dau_thoi_gian(aw);
                List<object> tan_xuat_5_muc = cau_hoi_5_muc(all_data);
                List<object> tan_xuat_1_lua_chon = cau_hoi_mot_lua_chon(all_data);
                List<object> tan_xuat_nhieu_lua_chon = cau_hoi_nhieu_lua_chon(all_data);
                List<object> tan_xuat_y_kien_khac = y_kien_khac(all_data);

                return Ok(new
                {
                    rate = JsonConvert.SerializeObject(list_count),
                    five_levels = JsonConvert.SerializeObject(tan_xuat_5_muc),
                    single_levels = JsonConvert.SerializeObject(tan_xuat_1_lua_chon),
                    many_levels = JsonConvert.SerializeObject(tan_xuat_nhieu_lua_chon),
                    other_levels = JsonConvert.SerializeObject(tan_xuat_y_kien_khac),
                    success = true
                });
            }
            else
            {
                return Ok(new
                {
                    message = "Chưa có dữ liệu để thống kê",
                    success = false
                });
            }
        }

        public async Task<List<dynamic>> load_ty_le_khong_dau_thoi_gian(get_option_ctdt aw)
        {
            var list_count = new List<dynamic>();
            var check_group_pks = await db.survey.FirstOrDefaultAsync(x => x.surveyID == aw.surveyID);
            var get_ctdt = await db.ctdt.FirstOrDefaultAsync(x => x.id_ctdt == aw.id_ctdt);
            if (check_group_pks.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu người học có học phần")
            {
                var count_mon_hoc = await db.thong_ke_theo_yeu_cau
                        .Where(x => x.surveyID == aw.surveyID && x.id_ctdt
                        == aw.id_ctdt)
                        .ToListAsync();
                var count_da_tra_loi = count_mon_hoc.Where(x => x.nguoi_hoc_dang_co_hoc_phan.da_khao_sat == 1).ToList();
                var count_chua_tra_loi = count_mon_hoc.Where(x => x.nguoi_hoc_dang_co_hoc_phan.da_khao_sat == 0).ToList();

                double percentage = count_mon_hoc.Count > 0
                    ? Math.Round(((double)count_da_tra_loi.Count / count_mon_hoc.Count) * 100, 2)
                    : 0;
                double unpercentage = count_mon_hoc.Count > 0
                    ? Math.Round(100 - percentage, 2)
                    : 0;
                var get_mh = new
                {
                    tong_khao_sat = count_mon_hoc.Count,
                    tong_phieu_da_tra_loi = count_da_tra_loi.Count,
                    tong_phieu_chua_tra_loi = count_chua_tra_loi.Count,
                    ty_le_da_tra_loi = percentage,
                    ty_le_chua_tra_loi = unpercentage,
                };
                list_count.Add(get_mh);
            }
            else if (check_group_pks.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu giảng viên")
            {
                if (check_group_pks.id_loaikhaosat == 3)
                {
                    var count_gv = await db.answer_response
                        .Where(x => x.surveyID == aw.surveyID && x.id_ctdt == aw.id_ctdt)
                        .ToListAsync();
                    var DataCTDT = new
                    {
                        pks = check_group_pks.surveyTitle,
                        tong_khao_sat = count_gv.Count(),
                        tong_phieu_da_tra_loi = count_gv.Count(),
                        tong_phieu_chua_tra_loi = 0,
                        ty_le_da_tra_loi = count_gv.Count() > 0 ? 100 : 0,
                        ty_le_chua_tra_loi = 0,
                        is_can_bo = true
                    };
                    list_count.Add(DataCTDT);
                }

                else if (check_group_pks.id_loaikhaosat == 8)
                {
                    var count_cbvc = await db.cbvc_khao_sat
                        .Where(x => x.surveyID == aw.surveyID && x.CanBoVienChuc.id_chuongtrinhdaotao == aw.id_ctdt).ToListAsync();
                    var count_da_tra_loi = count_cbvc.Where(x => x.is_khao_sat == 1).ToList();
                    var count_chua_tra_loi = count_cbvc.Where(x => x.is_khao_sat == 0).ToList();
                    double percentage = count_cbvc.Count > 0
                        ? Math.Round(((double)count_da_tra_loi.Count / count_cbvc.Count) * 100, 2)
                        : 0;

                    var DataCTDT = new
                    {
                        ctdt = check_group_pks.surveyTitle,
                        tong_khao_sat = count_cbvc.Count,
                        tong_phieu_da_tra_loi = count_da_tra_loi.Count,
                        tong_phieu_chua_tra_loi = count_chua_tra_loi.Count,
                        ty_le_da_tra_loi = percentage,
                        ty_le_chua_tra_loi = Math.Round(100 - percentage, 2)
                    };
                }
            }
            else if (check_group_pks.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu doanh nghiệp")
            {
                var ctdt = await db.answer_response.Where(x => x.surveyID == aw.surveyID && x.id_ctdt == aw.id_ctdt).ToListAsync();
                var datactdt = new
                {
                    pks = check_group_pks.surveyTitle,
                    tong_khao_sat = ctdt.Count,
                    tong_phieu_da_tra_loi = ctdt.Count,
                    tong_phieu_chua_tra_loi = 0,
                    ty_le_da_tra_loi = ctdt.Count > 0 ? 100 : 0,
                    ty_le_chua_tra_loi = 0
                };
                list_count.Add(datactdt);
            }
            else if (check_group_pks.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu người học" || check_group_pks.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu cựu người học")
            {
                var query = await db.nguoi_hoc_khao_sat.Where(x => x.surveyID == aw.surveyID && x.sinhvien.lop.id_ctdt == aw.id_ctdt).ToListAsync();
                var TotalAll = query.Count;
                var idphieu = db.survey.Where(x => x.surveyID == aw.surveyID).FirstOrDefault();
                var TotalDaKhaoSat = query.Where(x => x.is_khao_sat == 1).ToList();
                double percentage = TotalAll > 0 ? Math.Round(((double)TotalDaKhaoSat.Count / TotalAll) * 100, 2) : 0;
                var DataCBVC = new
                {
                    pks = check_group_pks.surveyTitle,
                    tong_khao_sat = TotalAll,
                    tong_phieu_da_tra_loi = TotalDaKhaoSat.Count,
                    tong_phieu_chua_tra_loi = (TotalAll - TotalDaKhaoSat.Count),
                    ty_le_da_tra_loi = percentage,
                    ty_le_chua_tra_loi = Math.Round(((double)100 - percentage), 2),
                };
                list_count.Add(DataCBVC);
            }
            return list_count;
        }
        public List<object> cau_hoi_5_muc(dynamic get_data)
        {
            Dictionary<string, Dictionary<string, int>> frequency = new Dictionary<string, Dictionary<string, int>>();
            Dictionary<string, List<string>> choices = new Dictionary<string, List<string>>();
            List<string> specificChoices = new List<string> {
                "Hoàn toàn không đồng ý",
                "Không đồng ý",
                "Bình thường",
                "Đồng ý",
                "Hoàn toàn đồng ý"
            };
            foreach (var response in get_data)
            {

                JObject jsonAnswerObject = JObject.Parse(response.JsonAnswer);
                JObject surveydataObject = JObject.Parse(response.SurveyJson);

                JArray answerPages = (JArray)jsonAnswerObject["pages"];
                JArray surveyPages = (JArray)surveydataObject["pages"];

                foreach (JObject surveyPage in surveyPages)
                {
                    JArray surveyElements = (JArray)surveyPage["elements"];

                    foreach (JObject surveyElement in surveyElements)
                    {
                        string type = surveyElement["type"].ToString();
                        if (type == "radiogroup")
                        {
                            JArray elementChoices = (JArray)surveyElement["choices"];
                            List<string> elementChoiceTexts = elementChoices.Select(c => c["text"].ToString()).ToList();

                            if (elementChoiceTexts.SequenceEqual(specificChoices))
                            {
                                string questionName = surveyElement["name"].ToString();
                                string questionTitle = surveyElement["title"].ToString();

                                if (!choices.ContainsKey(questionTitle))
                                {
                                    choices[questionTitle] = elementChoiceTexts;
                                }

                                foreach (JObject answerPage in answerPages)
                                {
                                    JArray answerElements = (JArray)answerPage["elements"];

                                    foreach (JObject answerElement in answerElements)
                                    {
                                        if (answerElement["name"].ToString() == questionName)
                                        {
                                            string answer = answerElement["response"]["text"]?.ToString() ?? answerElement["response"]["name"]?.ToString() ?? "";

                                            if (!frequency.ContainsKey(questionTitle))
                                            {
                                                frequency[questionTitle] = new Dictionary<string, int>();
                                            }

                                            if (!frequency[questionTitle].ContainsKey(answer))
                                            {
                                                frequency[questionTitle][answer] = 0;
                                            }

                                            frequency[questionTitle][answer]++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            var results = frequency.Select(f => (object)new
            {
                Question = f.Key,
                TotalResponses = f.Value.Values.Sum(),
                Frequencies = f.Value,
                Percentages = f.Value.ToDictionary(
                    kvp => kvp.Key,
                    kvp => (double)kvp.Value / f.Value.Values.Sum() * 100
                ),
                AverageScore = f.Value.Sum(kvp =>
                {
                    switch (kvp.Key)
                    {
                        case "Hoàn toàn không đồng ý": return kvp.Value * 1;
                        case "Không đồng ý": return kvp.Value * 2;
                        case "Bình thường": return kvp.Value * 3;
                        case "Đồng ý": return kvp.Value * 4;
                        case "Hoàn toàn đồng ý": return kvp.Value * 5;
                        default: return 0;
                    }
                }) / (double)f.Value.Values.Sum()
            }).ToList();
            return results;
        }
        public List<object> cau_hoi_mot_lua_chon(dynamic get_data)
        {
            var questionDataDict = new Dictionary<string, dynamic>();

            List<string> specificChoices = new List<string> {
                "Hoàn toàn không đồng ý",
                "Không đồng ý",
                "Bình thường",
                "Đồng ý",
                "Hoàn toàn đồng ý"
            };
            foreach (var response in get_data)
            {
                var surveyDataObject = JObject.Parse(response.SurveyJson);
                var answerDataObject = JObject.Parse(response.JsonAnswer);
                var surveyPages = (JArray)surveyDataObject["pages"];
                var answerPages = (JArray)answerDataObject["pages"];

                foreach (JObject surveyPage in surveyPages)
                {
                    var surveyElements = (JArray)surveyPage["elements"];

                    foreach (JObject surveyElement in surveyElements)
                    {
                        var type = surveyElement["type"].ToString();
                        if (type == "radiogroup")
                        {
                            var questionName = surveyElement["name"].ToString();
                            var questionTitle = surveyElement["title"].ToString();
                            var choices = (JArray)surveyElement["choices"];
                            List<string> elementChoiceTexts = choices.Select(c => c["text"].ToString()).ToList();
                            if ((!elementChoiceTexts.SequenceEqual(specificChoices)))
                            {
                                var choiceCounts = choices.ToDictionary(
                                    c => c["name"].ToString(),
                                    c =>
                                    {
                                        dynamic choice = new ExpandoObject();
                                        choice.ChoiceName = c["name"].ToString();
                                        choice.ChoiceText = c["text"].ToString();
                                        choice.Count = 0;
                                        choice.Percentage = 0.0;
                                        return choice;
                                    }
                                );

                                int totalResponses = 0;
                                foreach (JObject answerPage in answerPages)
                                {
                                    var answerElements = (JArray)answerPage["elements"];
                                    foreach (JObject answerElement in answerElements)
                                    {
                                        if (answerElement["name"].ToString() == questionName)
                                        {
                                            var responseObject = answerElement["response"];
                                            if (responseObject != null)
                                            {
                                                string responseName = responseObject["name"]?.ToString();
                                                string responseText = responseObject["text"]?.ToString();

                                                if (!string.IsNullOrEmpty(responseName) && choiceCounts.ContainsKey(responseName))
                                                {
                                                    choiceCounts[responseName].Count++;
                                                    totalResponses++;
                                                }
                                                else if (!string.IsNullOrEmpty(responseText))
                                                {
                                                    var matchingChoice = choiceCounts.Values.FirstOrDefault(c => c.ChoiceText == responseText);
                                                    if (matchingChoice != null)
                                                    {
                                                        matchingChoice.Count++;
                                                        totalResponses++;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                foreach (var choice in choiceCounts.Values)
                                {
                                    choice.Percentage = totalResponses > 0 ? (double)choice.Count / totalResponses * 100 : 0;
                                }

                                if (questionDataDict.ContainsKey(questionName))
                                {
                                    var existingQuestionData = questionDataDict[questionName];
                                    existingQuestionData.TotalResponses += totalResponses;

                                    foreach (var existingChoice in existingQuestionData.Choices)
                                    {
                                        var matchingNewChoice = choiceCounts.Values.FirstOrDefault(c => c.ChoiceName == existingChoice.ChoiceName);
                                        if (matchingNewChoice != null)
                                        {
                                            existingChoice.Count += matchingNewChoice.Count;
                                            existingChoice.Percentage = existingQuestionData.TotalResponses > 0 ? (double)existingChoice.Count / existingQuestionData.TotalResponses * 100 : 0;
                                        }
                                    }
                                }
                                else
                                {
                                    dynamic questionData = new ExpandoObject();
                                    questionData.QuestionName = questionName;
                                    questionData.QuestionTitle = questionTitle;
                                    questionData.TotalResponses = totalResponses;
                                    questionData.Choices = choiceCounts.Values.ToList();
                                    questionDataDict[questionName] = questionData;
                                }
                            }
                        }
                    }
                }
            }

            var questionDataList = questionDataDict.Values.Select(q => new
            {
                q.QuestionName,
                q.QuestionTitle,
                q.TotalResponses,
                Choices = ((List<dynamic>)q.Choices).Select(c => new
                {
                    c.ChoiceName,
                    c.ChoiceText,
                    c.Count,
                    c.Percentage
                }).ToList()
            }).ToList().Cast<object>().ToList();

            return questionDataList;
        }
        public List<object> cau_hoi_nhieu_lua_chon(dynamic get_data)
        {
            var questionDataDict = new Dictionary<string, dynamic>();
            foreach (var response in get_data)
            {
                var surveyDataObject = JObject.Parse(response.SurveyJson);
                var answerDataObject = JObject.Parse(response.JsonAnswer);
                var surveyPages = (JArray)surveyDataObject["pages"];
                var answerPages = (JArray)answerDataObject["pages"];

                foreach (JObject surveyPage in surveyPages)
                {
                    var surveyElements = (JArray)surveyPage["elements"];

                    foreach (JObject surveyElement in surveyElements)
                    {
                        var type = surveyElement["type"].ToString();
                        if (type == "checkbox")
                        {
                            var questionName = surveyElement["name"].ToString();
                            var questionTitle = surveyElement["title"].ToString();
                            var choices = (JArray)surveyElement["choices"];
                            var choiceCounts = choices.ToDictionary(
                                c => c["name"].ToString(),
                                c =>
                                {
                                    dynamic choice = new ExpandoObject();
                                    choice.ChoiceName = c["name"].ToString();
                                    choice.ChoiceText = c["text"].ToString();
                                    choice.Count = 0;
                                    choice.Percentage = 0.0;
                                    return choice;
                                }
                            );

                            int totalResponses = 0;

                            foreach (JObject answerPage in answerPages)
                            {
                                var answerElements = (JArray)answerPage["elements"];
                                foreach (JObject answerElement in answerElements)
                                {
                                    if (answerElement["name"].ToString() == questionName)
                                    {
                                        var responsesArray = (JArray)answerElement["response"]["name"];

                                        foreach (var responseName in responsesArray)
                                        {
                                            var responseString = responseName.ToString();
                                            if (choiceCounts.ContainsKey(responseString))
                                            {
                                                choiceCounts[responseString].Count++;
                                                totalResponses++;
                                            }
                                        }
                                    }
                                }
                            }

                            foreach (var choice in choiceCounts.Values)
                            {
                                choice.Percentage = totalResponses > 0 ? (double)choice.Count / totalResponses * 100 : 0;
                            }

                            if (questionDataDict.ContainsKey(questionName))
                            {
                                var existingQuestionData = questionDataDict[questionName];
                                existingQuestionData.TotalResponses += totalResponses;

                                foreach (var existingChoice in existingQuestionData.Choices)
                                {
                                    var matchingNewChoice = choiceCounts.Values.FirstOrDefault(c => c.ChoiceName == existingChoice.ChoiceName);
                                    if (matchingNewChoice != null)
                                    {
                                        existingChoice.Count += matchingNewChoice.Count;
                                        existingChoice.Percentage = existingQuestionData.TotalResponses > 0 ? (double)existingChoice.Count / existingQuestionData.TotalResponses * 100 : 0;
                                    }
                                }
                            }
                            else
                            {
                                dynamic questionData = new ExpandoObject();
                                questionData.QuestionName = questionName;
                                questionData.QuestionTitle = questionTitle;
                                questionData.TotalResponses = totalResponses;
                                questionData.Choices = choiceCounts.Values.ToList();
                                questionDataDict[questionName] = questionData;
                            }
                        }
                    }
                }
            }

            var questionDataList = questionDataDict.Values.Select(q => new
            {
                q.QuestionName,
                q.QuestionTitle,
                q.TotalResponses,
                Choices = ((List<dynamic>)q.Choices).Select(c => new
                {
                    c.ChoiceName,
                    c.ChoiceText,
                    c.Count,
                    c.Percentage
                }).ToList()
            }).ToList().Cast<object>().ToList();


            return questionDataList;
        }
        public List<object> y_kien_khac(dynamic get_data)
        {
            var questionDataList = new List<dynamic>();

            foreach (var response in get_data)
            {
                var surveyDataObject = JObject.Parse(response.SurveyJson);
                var answerDataObject = JObject.Parse(response.JsonAnswer);
                var surveyPages = (JArray)surveyDataObject["pages"];
                var answerPages = (JArray)answerDataObject["pages"];

                foreach (JObject surveyPage in surveyPages)
                {
                    var surveyElements = (JArray)surveyPage["elements"];

                    foreach (JObject surveyElement in surveyElements)
                    {
                        var type = surveyElement["type"].ToString();
                        if (type == "comment")
                        {
                            var questionName = surveyElement["name"].ToString();
                            var questionTitle = surveyElement["title"].ToString();
                            var responsesList = new List<string>();

                            foreach (JObject answerPage in answerPages)
                            {
                                var answerElements = (JArray)answerPage["elements"];

                                foreach (JObject answerElement in answerElements)
                                {
                                    if (answerElement["name"].ToString() == questionName)
                                    {
                                        var responseText = answerElement["response"]["text"].ToString();
                                        if (!string.IsNullOrEmpty(responseText))
                                        {
                                            responsesList.Add(responseText);
                                        }
                                    }
                                }
                            }

                            var existingQuestion = questionDataList.FirstOrDefault(q => q.QuestionName == questionName);

                            if (existingQuestion != null)
                            {
                                existingQuestion.Responses.AddRange(responsesList);
                            }
                            else
                            {
                                var questionData = new
                                {
                                    QuestionName = questionName,
                                    QuestionTitle = questionTitle,
                                    Responses = responsesList
                                };

                                questionDataList.Add(questionData);
                            }
                        }
                    }
                }
            }
            return questionDataList;
        }

        #endregion
    }
}
