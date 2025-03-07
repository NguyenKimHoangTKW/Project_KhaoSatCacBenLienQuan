﻿using CTDT.Models;
using GoogleApi.Entities.Search.Common;
using GoogleApi.Entities.Search.Video.Common;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.UI.WebControls;

namespace CTDT.Areas.Admin.Controllers
{
    public class ThongKeKetQuaKhaoSatAPIController : ApiController
    {
        dbSurveyEntities db = new dbSurveyEntities();
        // Giám sát kết quả khảo sát - Lọc giảng viên theo môn học API
        [HttpPost]
        [Route("api/admin/loc-giang-vien-by-mon-hoc")]
        public async Task<IHttpActionResult> check_phieu_khao_sat(answer_response survey)
        {
            var check_mon_hoc = await db.nguoi_hoc_dang_co_hoc_phan
                .Where(x => x.surveyID == survey.surveyID && x.sinhvien.lop.id_ctdt == survey.id_ctdt)
                .ToListAsync();
            var data_list = check_mon_hoc
                .GroupBy(x => new
                {
                    x.mon_hoc.id_lop,
                    x.mon_hoc.lop.ma_lop,
                    x.id_mon_hoc,
                    x.mon_hoc.ten_mon_hoc
                })
                .Select(group => new
                {
                    id_lop = group.Key.id_lop,
                    lop = group.Key.ma_lop,
                    id_hoc_phan = group.Key.id_mon_hoc,
                    ten_hoc_phan = group.Key.ten_mon_hoc,
                    giang_vien = group.SelectMany(item => db.CanBoVienChuc
                        .Where(x => x.id_CBVC == item.id_giang_vvien)
                        .Select(x => new
                        {
                            id_giang_vien = x.id_CBVC,
                            ma_giang_vien = x.MaCBVC,
                            ten_giang_vien = x.TenCBVC
                        })).Distinct().ToList()
                }).ToList();

            if (data_list.Count > 0)
            {
                return Ok(new { data = data_list, success = true });
            }
            else
            {
                return Ok(new { message = "Bạn đang chọn phiếu ngoài bộ phiếu 8", success = false });
            }
        }
        // Giám sát kết quả khảo sát - Lọc môn học theo giảng viên API
        [HttpPost]
        [Route("api/admin/loc-mon-hoc-by-giang-vien")]
        public async Task<IHttpActionResult> check_mon_hoc_by_giang_vien(answer_response survey)
        {
            var check_mon_hoc = await db.nguoi_hoc_dang_co_hoc_phan
                .Where(x => x.surveyID == survey.surveyID && x.sinhvien.lop.id_ctdt == survey.id_ctdt)
                .ToListAsync();
            var data_list = check_mon_hoc
               .GroupBy(x => new
               {
                   x.mon_hoc.lop.id_lop,
                   x.mon_hoc.lop.ma_lop,
                   x.CanBoVienChuc.MaCBVC,
                   x.id_giang_vvien,
                   x.CanBoVienChuc.TenCBVC
               })
               .Select(group => new
               {
                   id_lop = group.Key.id_lop,
                   ten_lop = group.Key.ma_lop,
                   ma_giang_vien = group.Key.MaCBVC,
                   id_giang_vien = group.Key.id_giang_vvien,
                   ten_giang_vien = group.Key.TenCBVC,
                   mon_hoc = group.SelectMany(item => db.mon_hoc
                       .Where(x => x.id_mon_hoc == item.id_mon_hoc)
                       .Select(x => new
                       {
                           id_mon_hoc = x.id_mon_hoc,
                           ten_mon_hoc = x.ten_mon_hoc,
                       })).Distinct().ToList()
               }).ToList();

            if (data_list.Count > 0)
            {
                return Ok(new { data = data_list, success = true });
            }
            else
            {
                return Ok(new { message = "Bạn đang chọn phiếu ngoài bộ phiếu 8", success = false });
            }
        }
        // Các hàm để thống kê và lấy ra các câu hỏi 5 mức,...
        [HttpPost]
        [Route("api/admin/giam-sat-ket-qua-khao-sat")]
        public async Task<IHttpActionResult> giam_sat_ket_qua_khao_sat(GiamSatThongKeKetQua aw)
        {
            var check_answer = db.answer_response
                .Where(x => x.surveyID == aw.surveyID
                           && x.ctdt.id_hdt == aw.id_hdt).AsQueryable();
            if (aw.id_ctdt != null)
            {
                check_answer = check_answer.Where(x => x.id_ctdt == aw.id_ctdt);
            }
            if (aw.id_lop != null)
            {
                check_answer = check_answer.Where(x => x.sinhvien.lop.id_lop == aw.id_lop);
            }

            if (aw.id_mh != null)
            {
                check_answer = check_answer.Where(x => x.id_mh == aw.id_mh);
            }

            if (aw.id_CBVC != null)
            {
                check_answer = check_answer.Where(x => x.id_CBVC == aw.id_CBVC);
            }

            if (aw.from_date != null && aw.to_date != null)
            {
                check_answer = check_answer.Where(x => x.time >= aw.from_date && x.time <= aw.to_date);
            }
            var get_data = await check_answer
                .Select(x => new
                {
                    JsonAnswer = x.json_answer,
                    SurveyJson = x.survey.surveyData
                })
                .ToListAsync();
            var list_count = new List<dynamic>();
            if (get_data.Count > 0)
            {
                if (aw.from_date != null && aw.to_date != null)
                {
                    list_count = await load_ty_le_co_dau_thoi_gian(aw);
                }
                else
                {
                    list_count = await load_ty_le_khong_dau_thoi_gian(aw);
                }

                List<object> tan_xuat_5_muc = new List<object>();
                List<object> tan_xuat_1_lua_chon = new List<object>();
                List<object> tan_xuat_nhieu_lua_chon = new List<object>();
                List<object> tan_xuat_y_kien_khac = new List<object>();
                tan_xuat_5_muc = cau_hoi_5_muc(get_data);
                tan_xuat_1_lua_chon = cau_hoi_mot_lua_chon(get_data);
                tan_xuat_nhieu_lua_chon = cau_hoi_nhieu_lua_chon(get_data);
                tan_xuat_y_kien_khac = y_kien_khac(get_data);
                return Ok(new
                {
                    rate = list_count,
                    five_levels = tan_xuat_5_muc,
                    single_levels = tan_xuat_1_lua_chon,
                    many_leves = tan_xuat_nhieu_lua_chon,
                    other_levels = tan_xuat_y_kien_khac,
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
        public async Task<List<dynamic>> load_ty_le_co_dau_thoi_gian(GiamSatThongKeKetQua aw)
        {
            var list_count = new List<dynamic>();
            var check_group_pks = await db.survey.FirstOrDefaultAsync(x => x.surveyID == aw.surveyID);
            if (check_group_pks.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu người học có học phần")
            {
                var count_mon_hoc = await db.nguoi_hoc_dang_co_hoc_phan
                        .Where(x => x.surveyID == aw.surveyID).ToListAsync();

                var count_da_tra_loi = await db.answer_response
                            .Where(x => x.time >= aw.from_date && x.time <= aw.to_date && x.surveyID == aw.surveyID).ToListAsync();
                var get_ctdt = "Tất cả";
                if (aw.id_lop != null)
                {
                    count_mon_hoc = count_mon_hoc
                        .Where(x => x.sinhvien.id_lop == aw.id_lop).ToList();
                    count_da_tra_loi = count_da_tra_loi
                        .Where(x => x.sinhvien.id_lop == aw.id_lop).ToList();
                }
                if (aw.id_ctdt != null)
                {
                    count_mon_hoc = count_mon_hoc
                        .Where(x => x.sinhvien.lop.id_ctdt == aw.id_ctdt).ToList();
                    count_da_tra_loi = count_da_tra_loi
                        .Where(x => x.sinhvien.lop.id_ctdt == aw.id_ctdt).ToList();
                    get_ctdt = await db.ctdt.Where(x => x.id_ctdt == aw.id_ctdt).Select(x => x.ten_ctdt).FirstOrDefaultAsync();
                }
                if (aw.id_mh != null)
                {
                    count_mon_hoc = count_mon_hoc
                        .Where(x => x.id_mon_hoc == aw.id_mh).ToList();
                    count_da_tra_loi = count_da_tra_loi
                        .Where(x => x.id_mh == aw.id_mh).ToList();
                }
                if (aw.id_CBVC != null)
                {
                    count_mon_hoc = count_mon_hoc
                        .Where(x => x.id_giang_vvien == aw.id_CBVC).ToList();
                    count_da_tra_loi = count_da_tra_loi
                        .Where(x => x.id_CBVC == aw.id_CBVC).ToList();
                }


                var count_chua_tra_loi = count_mon_hoc.Where(x => x.da_khao_sat == 0).ToList();

                double percentage = count_mon_hoc.Count > 0
                    ? Math.Round(((double)count_da_tra_loi.Count / count_mon_hoc.Count) * 100, 2)
                    : 0;

                var get_mh = new
                {
                    ctdt = get_ctdt,
                    tong_khao_sat = count_mon_hoc.Count,
                    tong_phieu_da_tra_loi = count_da_tra_loi.Count,
                    tong_phieu_chua_tra_loi = count_chua_tra_loi.Count,
                    ty_le_da_tra_loi = percentage,
                    ty_le_chua_tra_loi = Math.Round(100 - percentage, 2),
                    is_mon_hoc = true
                };
                list_count.Add(get_mh);
            }
            else if (check_group_pks.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu giảng viên")
            {
                if (check_group_pks.id_loaikhaosat == 3)
                {
                    var count_gv = await db.answer_response
                        .Where(x => x.surveyID == aw.surveyID)
                        .ToListAsync();
                    var get_ctdt = "Tất cả";
                    if (aw.id_ctdt != null)
                    {
                        count_gv = count_gv
                            .Where(x => x.id_ctdt == aw.id_ctdt)
                            .ToList();
                        get_ctdt = await db.ctdt.Where(x => x.id_ctdt == aw.id_ctdt).Select(x => x.ten_ctdt).FirstOrDefaultAsync();
                    }
                    if (aw.from_date != null && aw.to_date != null)
                    {
                        count_gv = count_gv
                            .Where(x => x.time >= aw.from_date && x.time <= aw.to_date)
                            .ToList();
                    }

                    var groupedData = count_gv
                        .GroupBy(x => x.ctdt.ten_ctdt)
                        .Select(gr => new
                        {
                            ctdt = gr.Key
                        })
                        .ToList();

                    var DataCTDT = new
                    {
                        ctdt = groupedData,
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
                        .Where(x => x.surveyID == aw.surveyID).ToListAsync();
                    var count_da_tra_loi = await db.answer_response
                           .Where(x => x.time >= aw.from_date && x.time <= aw.to_date && x.surveyID == aw.surveyID).ToListAsync();
                    var get_ctdt = "Tất cả";
                    if (aw.id_ctdt != null)
                    {
                        count_cbvc = count_cbvc.Where(x => x.CanBoVienChuc.id_chuongtrinhdaotao == aw.id_ctdt).ToList();
                        count_da_tra_loi = count_da_tra_loi.Where(x => x.id_ctdt == aw.id_ctdt).ToList();
                        get_ctdt = await db.ctdt.Where(x => x.id_ctdt == aw.id_ctdt).Select(x => x.ten_ctdt).FirstOrDefaultAsync();
                    }
                    var count_chua_tra_loi = count_cbvc.Where(x => x.is_khao_sat == 0).ToList();

                    double percentage = count_cbvc.Count > 0
                        ? Math.Round(((double)count_da_tra_loi.Count / count_cbvc.Count) * 100, 2)
                        : 0;
                    var get_gv = count_cbvc
                        .GroupBy(x => new
                        {
                            x.CanBoVienChuc.ctdt.ten_ctdt,
                        })
                        .Select(gr => new
                        {
                            ctdt = gr.Key.ten_ctdt
                        }).ToList();
                    var DataCTDT = new
                    {
                        ctdt = get_gv,
                        tong_khao_sat = count_cbvc.Count,
                        tong_phieu_da_tra_loi = count_da_tra_loi.Count,
                        tong_phieu_chua_tra_loi = count_chua_tra_loi.Count,
                        ty_le_da_tra_loi = percentage,
                        ty_le_chua_tra_loi = Math.Round(100 - percentage, 2)
                    };
                    list_count.Add(DataCTDT);
                }
            }
            else if (check_group_pks.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu doanh nghiệp")
            {
                var ctdt = db.answer_response.Where(x =>
                x.surveyID == aw.surveyID &&
                x.time >= aw.from_date && x.time <= aw.to_date).AsQueryable();
                var get_ctdt = "Tất cả";
                if (aw.id_ctdt != null)
                {
                    ctdt = ctdt.Where(x => x.id_ctdt == aw.id_ctdt);
                    get_ctdt = await db.ctdt.Where(x => x.id_ctdt == aw.id_ctdt).Select(x => x.ten_ctdt).FirstOrDefaultAsync();
                }
                var totalall = await ctdt.ToListAsync();
                var datactdt = new
                {
                    ctdt = get_ctdt,
                    tong_khao_sat = totalall.Count,
                    tong_phieu_da_tra_loi = totalall.Count,
                    tong_phieu_chua_tra_loi = 0,
                    ty_le_da_tra_loi = totalall.Count > 0 ? 100 : 0,
                    ty_le_chua_tra_loi = 0
                };
                list_count.Add(datactdt);
            }
            else if (check_group_pks.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu người học" || check_group_pks.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu cựu người học")
            {
                var query = await db.nguoi_hoc_khao_sat.Where(x => x.surveyID == aw.surveyID).ToListAsync();
                var get_ctdt = "Tất cả";
                var TotalDaKhaoSat = await db.answer_response.Where(x => x.surveyID == aw.surveyID && x.time >= aw.from_date && x.time <= aw.to_date).ToListAsync();
                if (aw.id_ctdt != null)
                {
                    query = query.Where(x => x.sinhvien.lop.id_ctdt == aw.id_ctdt).ToList();
                    get_ctdt = await db.ctdt.Where(x => x.id_ctdt == aw.id_ctdt).Select(x => x.ten_ctdt).FirstOrDefaultAsync();
                }
                var TotalAll = query.Count;
                var idphieu = db.survey.Where(x => x.surveyID == aw.surveyID).FirstOrDefault();
                double percentage = TotalAll > 0 ? Math.Round(((double)TotalDaKhaoSat.Count / TotalAll) * 100, 2) : 0;
                var DataCBVC = new
                {
                    ctdt = get_ctdt,
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
        public async Task<List<dynamic>> load_ty_le_khong_dau_thoi_gian(GiamSatThongKeKetQua aw)
        {
            var list_count = new List<dynamic>();
            var check_group_pks = await db.survey.FirstOrDefaultAsync(x => x.surveyID == aw.surveyID);
            if (check_group_pks.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu người học có học phần")
            {
                var count_mon_hoc = await db.nguoi_hoc_dang_co_hoc_phan
                        .Where(x => x.surveyID == aw.surveyID)
                        .ToListAsync();
                var get_ctdt = "Tất cả";
                if (aw.id_lop != null)
                {
                    count_mon_hoc = count_mon_hoc
                        .Where(x => x.sinhvien.id_lop == aw.id_lop)
                        .ToList();
                }
                if (aw.id_ctdt != null)
                {
                    count_mon_hoc = count_mon_hoc
                        .Where(x => x.sinhvien.lop.id_ctdt == aw.id_ctdt)
                        .ToList();
                    get_ctdt = await db.ctdt.Where(x => x.id_ctdt == aw.id_ctdt).Select(x => x.ten_ctdt).FirstOrDefaultAsync();
                }
                if (aw.id_mh != null)
                {
                    count_mon_hoc = count_mon_hoc
                        .Where(x => x.id_mon_hoc == aw.id_mh)
                        .ToList();
                }
                if (aw.id_CBVC != null)
                {
                    count_mon_hoc = count_mon_hoc
                        .Where(x => x.id_giang_vvien == aw.id_CBVC)
                        .ToList();
                }

                var count_da_tra_loi = count_mon_hoc.Where(x => x.da_khao_sat == 1).ToList();
                var count_chua_tra_loi = count_mon_hoc.Where(x => x.da_khao_sat == 0).ToList();

                double percentage = count_mon_hoc.Count > 0
                    ? Math.Round(((double)count_da_tra_loi.Count / count_mon_hoc.Count) * 100, 2)
                    : 0;

                var get_mh = new
                {
                    ctdt = get_ctdt,
                    tong_khao_sat = count_mon_hoc.Count,
                    tong_phieu_da_tra_loi = count_da_tra_loi.Count,
                    tong_phieu_chua_tra_loi = count_chua_tra_loi.Count,
                    ty_le_da_tra_loi = percentage,
                    ty_le_chua_tra_loi = Math.Round(100 - percentage, 2),
                    is_mon_hoc = true
                };

                list_count.Add(get_mh);
            }
            else if (check_group_pks.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu giảng viên")
            {
                if (check_group_pks.id_loaikhaosat == 3)
                {
                    var count_gv = await db.answer_response
                        .Where(x => x.surveyID == aw.surveyID)
                        .ToListAsync();
                    var get_ctdt = "Tất cả";
                    if (aw.id_ctdt != null)
                    {
                        count_gv = count_gv
                            .Where(x => x.id_ctdt == aw.id_ctdt)
                            .ToList();
                        get_ctdt = await db.ctdt.Where(x => x.id_ctdt == aw.id_ctdt).Select(x => x.ten_ctdt).FirstOrDefaultAsync();
                    }
                    var DataCTDT = new
                    {
                        ctdt = get_ctdt,
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
                        .Where(x => x.surveyID == aw.surveyID).ToListAsync();
                    var get_ctdt = "Tất cả";
                    if (aw.id_ctdt != null)
                    {
                        count_cbvc = count_cbvc.Where(x => x.CanBoVienChuc.id_chuongtrinhdaotao == aw.id_ctdt).ToList();
                        get_ctdt = await db.ctdt.Where(x => x.id_ctdt == aw.id_ctdt).Select(x => x.ten_ctdt).FirstOrDefaultAsync();

                    }
                    var count_da_tra_loi = count_cbvc.Where(x => x.is_khao_sat == 1).ToList();
                    var count_chua_tra_loi = count_cbvc.Where(x => x.is_khao_sat == 0).ToList();

                    double percentage = count_cbvc.Count > 0
                        ? Math.Round(((double)count_da_tra_loi.Count / count_cbvc.Count) * 100, 2)
                        : 0;

                    var DataCTDT = new
                    {
                        ctdt = get_ctdt,
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
                var ctdt = await db.answer_response.Where(x => x.surveyID == aw.surveyID).ToListAsync();
                var get_ctdt = "Tất cả";
                if (aw.id_ctdt != null)
                {
                    ctdt = ctdt.Where(x => x.id_ctdt == aw.id_ctdt).ToList();
                    get_ctdt = await db.ctdt.Where(x => x.id_ctdt == aw.id_ctdt).Select(x => x.ten_ctdt).FirstOrDefaultAsync();

                }
                var datactdt = new
                {
                    ctdt = get_ctdt,
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
                var query = await db.nguoi_hoc_khao_sat.Where(x => x.surveyID == aw.surveyID).ToListAsync();
                var get_ctdt = "Tất cả";
                if (aw.id_ctdt != null)
                {
                    query = query.Where(x => x.sinhvien.lop.id_ctdt == aw.id_ctdt).ToList();
                    get_ctdt = await db.ctdt.Where(x => x.id_ctdt == aw.id_ctdt).Select(x => x.ten_ctdt).FirstOrDefaultAsync();
                }
                var TotalAll = query.Count;
                var idphieu = db.survey.Where(x => x.surveyID == aw.surveyID).FirstOrDefault();
                var TotalDaKhaoSat = query.Where(x => x.is_khao_sat == 1).ToList();
                double percentage = TotalAll > 0 ? Math.Round(((double)TotalDaKhaoSat.Count / TotalAll) * 100, 2) : 0;
                var DataCBVC = new
                {
                    ctdt = get_ctdt,
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

        // Hàm xuất dữ liệu thô
        [HttpPost]
        [Route("api/admin/export-du-lieu-tho")]
        public async Task<IHttpActionResult> export_du_lieu_tho(GiamSatThongKeKetQua aw)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var check_group_pks = await db.survey.FirstOrDefaultAsync(x => x.surveyID == aw.surveyID);
            var query = db.answer_response.Where(x => x.surveyID == aw.surveyID).AsQueryable();
            bool is_subject = false;
            bool is_student = false;
            bool is_staff = false;
            bool is_program = false;
            List<JObject> surveyData = new List<JObject>();

            if (aw.id_hdt != null)
            {
                query = query.Where(x => x.ctdt.id_hdt == aw.id_hdt);
            }

            if (aw.id_ctdt != null)
            {
                query = query.Where(x => x.id_ctdt == aw.id_ctdt);
            }

            if (aw.id_lop != null)
            {
                query = query.Where(x => x.sinhvien.lop.id_lop == aw.id_lop);
            }

            if (aw.id_mh != null)
            {
                query = query.Where(x => x.id_mh == aw.id_mh);
            }

            if (aw.id_CBVC != null)
            {
                query = query.Where(x => x.id_CBVC == aw.id_CBVC);
            }
            if (aw.from_date != null && aw.to_date != null)
            {
                query = query.Where(x => x.time >= aw.from_date && x.time <= aw.to_date);
            }
            if (check_group_pks.LoaiKhaoSat.group_loaikhaosat.id_gr_loaikhaosat == 3)
            {
                is_subject = true;
                var answers = query
                          .Select(x => new
                          {
                              DauThoiGian = x.time,
                              x.json_answer,
                              Email = x.users.email,
                              MonHoc = x.mon_hoc.ten_mon_hoc,
                              GiangVien = x.CanBoVienChuc.TenCBVC,
                              MSSV = x.sinhvien.ma_sv,
                              HoTen = x.sinhvien.hovaten,
                              NgaySinh = (DateTime?)x.sinhvien.ngaysinh,
                              Lop = x.sinhvien.lop.ma_lop,
                              CTDT = x.ctdt.ten_ctdt,
                              SDT = x.sinhvien.sodienthoai,
                          }).ToList();
                foreach (var answer in answers)
                {
                    JObject answerObject = JObject.Parse(answer.json_answer);
                    answerObject["DauThoiGian"] = answer.DauThoiGian;
                    answerObject["Email"] = answer.Email;
                    answerObject["MonHoc"] = answer.MonHoc;
                    answerObject["GiangVien"] = answer.GiangVien;
                    answerObject["MSSV"] = answer.MSSV;
                    answerObject["HoTen"] = answer.HoTen;
                    answerObject["NgaySinh"] = answer.NgaySinh?.ToString("dd-MM-yyyy");
                    answerObject["Lop"] = answer.Lop;
                    answerObject["CTDT"] = answer.CTDT;
                    answerObject["SDT"] = answer.SDT;
                    surveyData.Add(answerObject);
                }
            }
            else if (check_group_pks.LoaiKhaoSat.group_loaikhaosat.id_gr_loaikhaosat == 5)
            {
                is_student = true;
                var answers = query
                          .Select(x => new
                          {
                              DauThoiGian = x.time,
                              x.json_answer,
                              Email = x.users.email,
                              MSSV = x.sinhvien.ma_sv,
                              HoTen = x.sinhvien.hovaten,
                              NgaySinh = (DateTime?)x.sinhvien.ngaysinh,
                              Lop = x.sinhvien.lop.ma_lop,
                              CTDT = x.ctdt.ten_ctdt,
                              SDT = x.sinhvien.sodienthoai,
                          }).ToList();
                foreach (var answer in answers)
                {
                    JObject answerObject = JObject.Parse(answer.json_answer);
                    answerObject["DauThoiGian"] = answer.DauThoiGian;
                    answerObject["Email"] = answer.Email;
                    answerObject["MSSV"] = answer.MSSV;
                    answerObject["HoTen"] = answer.HoTen;
                    answerObject["NgaySinh"] = answer.NgaySinh?.ToString("dd-MM-yyyy");
                    answerObject["Lop"] = answer.Lop;
                    answerObject["CTDT"] = answer.CTDT;
                    answerObject["SDT"] = answer.SDT;
                    surveyData.Add(answerObject);
                }
            }
            else if (check_group_pks.LoaiKhaoSat.group_loaikhaosat.id_gr_loaikhaosat == 4)
            {
                is_program = true;
                var answers = query
                       .Select(x => new
                       {
                           DauThoiGian = x.time,
                           x.json_answer,
                           Email = x.users.email,
                           CTDT = x.ctdt.ten_ctdt,
                       }).ToList();

                foreach (var answer in answers)
                {
                    JObject answerObject = JObject.Parse(answer.json_answer);
                    answerObject["DauThoiGian"] = answer.DauThoiGian;
                    answerObject["Email"] = answer.Email;
                    answerObject["CTDT"] = answer.CTDT;
                    surveyData.Add(answerObject);
                }
            }
            else if (check_group_pks.LoaiKhaoSat.group_loaikhaosat.id_gr_loaikhaosat == 2)
            {
                is_staff = true;
                var answers = query
                         .Select(x => new
                         {
                             DauThoiGian = x.time,
                             x.json_answer,
                             HoTen = x.CanBoVienChuc.TenCBVC,
                             Email = x.users.email,
                             KhaoSatCTDT = x.ctdt.ten_ctdt,
                             DonVi = x.DonVi.name_donvi,
                             ChucDanh = x.CanBoVienChuc.ChucVu.name_chucvu,
                         }).ToList();

                foreach (var answer in answers)
                {
                    JObject answerObject = JObject.Parse(answer.json_answer);
                    answerObject["DauThoiGian"] = answer.DauThoiGian;
                    answerObject["Email"] = answer.Email;
                    answerObject["HoTen"] = answer.HoTen;
                    answerObject["KhaoSatCTDT"] = answer.KhaoSatCTDT;
                    answerObject["DonVi"] = answer.DonVi;
                    answerObject["ChucDanh"] = answer.ChucDanh;
                    surveyData.Add(answerObject);
                }
            }
            if (surveyData.Count > 0)
            {
                if (is_subject)
                {
                    return Ok(new { data = surveyData, success = true, is_subject = true });
                }
                else if (is_student)
                {
                    return Ok(new { data = surveyData, success = true, is_student = true });
                }
                else if (is_program)
                {
                    return Ok(new { data = surveyData, success = true, is_program = true });
                }
                else if (is_staff)
                {
                    return Ok(new { data = surveyData, success = true, is_staff = true });
                }
            }
            return Ok(new { message = "Không tìm thấy dữ liệu", success = false });
        }
    }
}
