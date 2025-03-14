using CTDT.Helper;
using CTDT.Models;
using CTDT.Models.Khoa;
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
    public class GiamSatTyLeThamGiaKhaoSatAPIController : ApiController
    {
        dbSurveyEntities db = new dbSurveyEntities();
        public users user;
        public GiamSatTyLeThamGiaKhaoSatAPIController()
        {
            user = SessionHelper.GetUser();
        }
        [HttpPost]
        [Route("api/admin/load-option-giam-sat-ty-le-tham-gia-khao-sat")]
        public async Task<IHttpActionResult> load_pks_by_year(survey survey)
        {
            var pks = await db.survey
                .Where(x => x.id_namhoc == survey.id_namhoc && x.id_hedaotao == survey.id_hedaotao && x.mo_thong_ke == 1)
                .Select(x => new
                {
                    id_phieu = x.surveyID,
                    ten_phieu = x.dot_khao_sat.ten_dot_khao_sat != null ? x.surveyTitle + " - " + x.dot_khao_sat.ten_dot_khao_sat : x.surveyTitle,
                })
                .ToListAsync();
            var list_data = new List<dynamic>();
            if (survey.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu giảng viên")
            {
               
                var get_don_vi = await db.khoa_vien_truong
                    .Where(x => x.id_namhoc == survey.id_namhoc)
                    .Select(x => new
                    {
                        value = x.id_khoa,
                        name = x.ten_khoa
                    })
                    .ToListAsync();
                foreach (var donvi in get_don_vi)
                {
                    var get_khoa = await db.khoa_children
                        .Where(x => x.id_khoa_vien_truong == donvi.value)
                        .Select(x => new
                        {
                            value_dv = x.id_khoa_vien_truong,
                            value = x.id_khoa_children,
                            name = x.ten_khoa_children
                        })
                        .ToListAsync();
                    foreach(var khoa in get_khoa)
                    {
                        var bo_mon = await db.bo_mon
                            .Where(x => x.id_khoa_children == khoa.value)
                            .Select(x => new
                            {
                                value_k = x.id_khoa_children,
                                value = x.id_bo_mon,
                                name = x.ten_bo_mon
                            })
                            .ToListAsync();
                        list_data.Add(new
                        {
                            don_vi = get_don_vi,
                            khoa = get_khoa,
                            bo_mon = bo_mon
                        });
                    }
                }
            }
            var sortedPks = pks.OrderBy(p =>
            {
                var match = System.Text.RegularExpressions.Regex.Match(p.ten_phieu, @"Phiếu (\d+)");
                return match.Success ? int.Parse(match.Groups[1].Value) : int.MaxValue;
            }).ToList();

            if (sortedPks.Count > 0)
            {
                return Ok(new { data = sortedPks, ctdt = list_data, success = true });
            }
            else
            {
                return Ok(new { message = "Không có dữ liệu phiếu khảo sát", success = false });
            }
        }
        [HttpPost]
        [Route("api/admin/giam-sat-ty-le-tham-gia-khao-sat")]
        public async Task<IHttpActionResult> load_charts_nguoi_hoc(GiamSatThongKeKetQua find)
        {
            var List_data = new List<dynamic>();

            if (find.from_date != null && find.to_date != null)
            {
                List_data = await load_thong_ke_co_dau_thoi_gian(find);
            }
            else
            {
                List_data = await load_thong_ke_khong_dau_thoi_gian(find);
            }
            if (List_data.Count > 0)
            {
                return Ok(new { data = List_data, success = true });
            }
            else
            {
                return Ok(new { message = "Chưa có dữ liệu khảo sát trong năm học để thống kê", success = false });
            }
        }
        public async Task<List<dynamic>> load_thong_ke_khong_dau_thoi_gian(GiamSatThongKeKetQua find)
        {
            var survey = await db.survey.Where(x => x.id_namhoc == find.id_namhoc && x.id_hedaotao == find.id_hdt).ToListAsync();
            if (find.id_namhoc != null)
            {
                survey = survey.Where(x => x.id_namhoc == find.id_namhoc).ToList();
            }
            var List_data = new List<dynamic>();
            foreach (var items in survey)
            {
                if (items.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu giảng viên")
                {
                    bool check_giang_vien = new[] { 3 }.Contains(items.id_loaikhaosat);
                    bool check_cbvc = new[] { 8 }.Contains(items.id_loaikhaosat);
                    if (check_cbvc)
                    {
                        var cbvc = await db.cbvc_khao_sat.Where(x => x.surveyID == items.surveyID).ToListAsync();
                        if (find.id_ctdt != null)
                        {
                            cbvc = cbvc.Where(x => x.CanBoVienChuc.id_chuongtrinhdaotao == find.id_ctdt).ToList();
                        }
                        var TotalAll = cbvc.Count();
                        var TotalDaKhaoSat = cbvc.Count(x => x.is_khao_sat == 1);

                        var idphieu = db.survey.Where(x => x.surveyID == items.surveyID).FirstOrDefault();
                        double percentage = TotalAll > 0 ? Math.Round(((double)TotalDaKhaoSat / TotalAll) * 100, 2) : 0;

                        var DataCBVC = new
                        {
                            id_phieu = items.surveyID,
                            ten_phieu = items.surveyTitle,
                            tong_khao_sat = TotalAll,
                            tong_phieu_da_tra_loi = TotalDaKhaoSat,
                            tong_phieu_chua_tra_loi = (TotalAll - TotalDaKhaoSat),
                            ty_le_da_tra_loi = percentage,
                            ty_le_chua_tra_loi = Math.Round(((double)100 - percentage), 2),
                        };

                        List_data.Add(DataCBVC);
                    }
                    else if (check_giang_vien)
                    {
                        var giang_vien = await db.answer_response.Where(x => x.surveyID == items.surveyID).ToListAsync();
                        if (find.id_ctdt != null)
                        {
                            giang_vien = giang_vien.Where(x => x.id_ctdt == find.id_ctdt).ToList();
                        }
                        var DataGiangVien = new
                        {
                            id_phieu = items.surveyID,
                            ten_phieu = items.surveyTitle,
                            tong_khao_sat = giang_vien.Count(),
                            tong_phieu_da_tra_loi = giang_vien.Count(),
                            tong_phieu_chua_tra_loi = 0,
                            ty_le_da_tra_loi = giang_vien.Count() > 0 ? 100 : 0,
                            ty_le_chua_tra_loi = 0
                        };
                        List_data.Add(DataGiangVien);
                    }
                }
                else if (items.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "phiếu doanh nghiệp")
                {
                    var ctdt = db.answer_response.Where(x => x.surveyID == find.surveyID).AsQueryable();
                    if (find.id_ctdt != 0)
                    {
                        ctdt = ctdt.Where(x => x.id_ctdt == find.id_ctdt);
                    }
                    var totalall = await ctdt.ToListAsync();
                    var datactdt = new
                    {
                        id_phieu = items.surveyID,
                        ten_phieu = items.surveyTitle,
                        tong_khao_sat = totalall,
                        tong_phieu_da_tra_loi = totalall,
                        tong_phieu_chua_tra_loi = 0,
                        ty_le_da_tra_loi = totalall.Count > 0 ? 100 : 0,
                        ty_le_chua_tra_loi = 0
                    };
                    List_data.Add(datactdt);
                }
                else if (items.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu người học có học phần")
                {
                    bool hoc_vien_co_hoc_phan_ly_thuyet_dang_hoc_tai_truong = new[] { 11 }.Contains(items.id_loaikhaosat);
                    bool hoc_vien_co_hoc_phan_tot_nghiep = new[] { 13 }.Contains(items.id_loaikhaosat);
                    if (hoc_vien_co_hoc_phan_ly_thuyet_dang_hoc_tai_truong)
                    {
                        var check_hoc_phan = await db.nguoi_hoc_dang_co_hoc_phan.Where(x => x.surveyID == items.surveyID).ToListAsync();
                        if (find.id_ctdt != null)
                        {
                            check_hoc_phan = check_hoc_phan.Where(x => x.sinhvien.lop.id_ctdt == find.id_ctdt).ToList();
                        }
                        var total = check_hoc_phan.Count;
                        var TotalIsKhaoSat = check_hoc_phan.Where(x => x.da_khao_sat == 1).ToList().Count;
                        double percentage = total > 0 ? Math.Round(((double)TotalIsKhaoSat / total) * 100, 2) : 0;
                        var DataStudent = new
                        {
                            id_phieu = items.surveyID,
                            ten_phieu = items.surveyTitle,
                            tong_khao_sat = total,
                            tong_phieu_da_tra_loi = TotalIsKhaoSat,
                            tong_phieu_chua_tra_loi = (total - TotalIsKhaoSat),
                            ty_le_da_tra_loi = percentage,
                            ty_le_chua_tra_loi = Math.Round(((double)100 - percentage), 2),
                        };
                        List_data.Add(DataStudent);
                    }
                }
                else if (items.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu người học" || items.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu cựu người học")
                {
                    var query = await db.nguoi_hoc_khao_sat.Where(x => x.surveyID == find.surveyID).ToListAsync();
                    if (find.id_ctdt != 0)
                    {
                        query = query.Where(x => x.sinhvien.lop.id_ctdt == find.id_ctdt).ToList();
                    }
                    var TotalAll = query.Count;
                    var idphieu = db.survey.Where(x => x.surveyID == items.surveyID).FirstOrDefault();
                    var TotalDaKhaoSat = query.Where(x => x.is_khao_sat == 1).ToList();
                    double percentage = TotalAll > 0 ? Math.Round(((double)TotalDaKhaoSat.Count / TotalAll) * 100, 2) : 0;
                    var DataCBVC = new
                    {
                        id_phieu = items.surveyID,
                        ten_phieu = items.surveyTitle,
                        tong_khao_sat = TotalAll,
                        tong_phieu_da_tra_loi = TotalDaKhaoSat.Count,
                        tong_phieu_chua_tra_loi = (TotalAll - TotalDaKhaoSat.Count),
                        ty_le_da_tra_loi = percentage,
                        ty_le_chua_tra_loi = Math.Round(((double)100 - percentage), 2),
                    };
                    List_data.Add(DataCBVC);
                }
            }
            return List_data;
        }

        public async Task<List<dynamic>> load_thong_ke_co_dau_thoi_gian(GiamSatThongKeKetQua find)
        {
            var survey = await db.survey.Where(x => x.id_namhoc == find.id_namhoc && x.id_hedaotao == find.id_hdt).ToListAsync();
            if (find.id_namhoc != null)
            {
                survey = survey.Where(x => x.id_namhoc == find.id_namhoc).ToList();
            }
            var List_data = new List<dynamic>();
            foreach (var items in survey)
            {
                if (items.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu giảng viên")
                {
                    bool check_giang_vien = new[] { 3 }.Contains(items.id_loaikhaosat);
                    bool check_cbvc = new[] { 8 }.Contains(items.id_loaikhaosat);
                    if (check_cbvc)
                    {
                        var cbvc = await db.cbvc_khao_sat.Where(x => x.surveyID == items.surveyID).ToListAsync();
                        var TotalDaKhaoSat = await db.answer_response
                            .Where(x => x.time >= find.from_date && x.time <= find.to_date && x.surveyID == items.surveyID)
                            .ToListAsync();
                        if (find.id_ctdt != null)
                        {
                            cbvc = cbvc.Where(x => x.CanBoVienChuc.id_chuongtrinhdaotao == find.id_ctdt).ToList();
                            TotalDaKhaoSat = TotalDaKhaoSat
                            .Where(x => x.id_ctdt == find.id_ctdt)
                            .ToList();
                        }
                        var TotalAll = cbvc.Count();
                        var idphieu = db.survey.Where(x => x.surveyID == items.surveyID).FirstOrDefault();
                        double percentage = TotalAll > 0 ? Math.Round(((double)TotalDaKhaoSat.Count / TotalAll) * 100, 2) : 0;

                        var DataCBVC = new
                        {
                            id_phieu = items.surveyID,
                            ten_phieu = items.surveyTitle,
                            tong_khao_sat = TotalAll,
                            tong_phieu_da_tra_loi = TotalDaKhaoSat.Count,
                            tong_phieu_chua_tra_loi = (TotalAll - TotalDaKhaoSat.Count),
                            ty_le_da_tra_loi = percentage,
                            ty_le_chua_tra_loi = Math.Round(((double)100 - percentage), 2),
                        };

                        List_data.Add(DataCBVC);
                    }
                    else if (check_giang_vien)
                    {
                        var giang_vien = await db.answer_response
                            .Where(x => x.surveyID == items.surveyID && x.time >= find.from_date && x.time <= find.to_date)
                            .ToListAsync();
                        if (find.id_ctdt != null)
                        {
                            giang_vien = giang_vien.Where(x => x.id_ctdt == find.id_ctdt).ToList();
                        }
                        var DataGiangVien = new
                        {
                            id_phieu = items.surveyID,
                            ten_phieu = items.surveyTitle,
                            tong_khao_sat = giang_vien.Count(),
                            tong_phieu_da_tra_loi = giang_vien.Count(),
                            tong_phieu_chua_tra_loi = 0,
                            ty_le_da_tra_loi = giang_vien.Count() > 0 ? 100 : 0,
                            ty_le_chua_tra_loi = 0
                        };
                        List_data.Add(DataGiangVien);
                    }
                }
                else if (items.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "phiếu doanh nghiệp")
                {
                    var ctdt = db.answer_response.Where(x =>
                    x.surveyID == find.surveyID &&
                    x.time >= find.from_date && x.time <= find.to_date).AsQueryable();
                    if (find.id_ctdt != 0)
                    {
                        ctdt = ctdt.Where(x => x.id_ctdt == find.id_ctdt);
                    }
                    var totalall = await ctdt.ToListAsync();
                    var datactdt = new
                    {
                        id_phieu = items.surveyID,
                        ten_phieu = items.surveyTitle,
                        tong_khao_sat = totalall.Count,
                        tong_phieu_da_tra_loi = totalall.Count,
                        tong_phieu_chua_tra_loi = 0,
                        ty_le_da_tra_loi = totalall.Count > 0 ? 100 : 0,
                        ty_le_chua_tra_loi = 0
                    };
                    List_data.Add(datactdt);
                }
                else if (items.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu người học có học phần")
                {
                    bool hoc_vien_co_hoc_phan_ly_thuyet_dang_hoc_tai_truong = new[] { 11 }.Contains(items.id_loaikhaosat);
                    bool hoc_vien_co_hoc_phan_tot_nghiep = new[] { 13 }.Contains(items.id_loaikhaosat);
                    if (hoc_vien_co_hoc_phan_ly_thuyet_dang_hoc_tai_truong)
                    {
                        var check_hoc_phan = await db.nguoi_hoc_dang_co_hoc_phan.Where(x => x.surveyID == items.surveyID).ToListAsync();
                        var TotalIsKhaoSat = await db.answer_response
                            .Where(x => x.time >= find.from_date && x.time <= find.to_date && x.surveyID == items.surveyID)
                            .ToListAsync();
                        if (find.id_ctdt != null)
                        {
                            check_hoc_phan = check_hoc_phan.Where(x => x.sinhvien.lop.id_ctdt == find.id_ctdt).ToList();
                            TotalIsKhaoSat = TotalIsKhaoSat
                            .Where(x => x.id_ctdt == find.id_ctdt)
                            .ToList();
                        }
                        var total = check_hoc_phan.Count;

                        double percentage = total > 0 ? Math.Round(((double)TotalIsKhaoSat.Count / total) * 100, 2) : 0;
                        var DataStudent = new
                        {
                            id_phieu = items.surveyID,
                            ten_phieu = items.surveyTitle,
                            tong_khao_sat = total,
                            tong_phieu_da_tra_loi = TotalIsKhaoSat.Count,
                            tong_phieu_chua_tra_loi = (total - TotalIsKhaoSat.Count),
                            ty_le_da_tra_loi = percentage,
                            ty_le_chua_tra_loi = Math.Round(((double)100 - percentage), 2),
                        };
                        List_data.Add(DataStudent);
                    }
                }
                else if (items.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu người học" || items.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu cựu người học")
                {
                    var query = await db.nguoi_hoc_khao_sat.Where(x => x.surveyID == find.surveyID).ToListAsync();
                    var TotalDaKhaoSat = await db.answer_response.Where(x => x.surveyID == find.surveyID && x.time >= find.from_date && x.time <= find.to_date).ToListAsync();
                    if (find.id_ctdt != 0)
                    {
                        query = query.Where(x => x.sinhvien.lop.id_ctdt == find.id_ctdt).ToList();
                    }
                    var TotalAll = query.Count;
                    var idphieu = db.survey.Where(x => x.surveyID == items.surveyID).FirstOrDefault();
                    double percentage = TotalAll > 0 ? Math.Round(((double)TotalDaKhaoSat.Count / TotalAll) * 100, 2) : 0;
                    var DataCBVC = new
                    {
                        id_phieu = items.surveyID,
                        ten_phieu = items.surveyTitle,
                        tong_khao_sat = TotalAll,
                        tong_phieu_da_tra_loi = TotalDaKhaoSat.Count,
                        tong_phieu_chua_tra_loi = (TotalAll - TotalDaKhaoSat.Count),
                        ty_le_da_tra_loi = percentage,
                        ty_le_chua_tra_loi = Math.Round(((double)100 - percentage), 2),
                    };
                    List_data.Add(DataCBVC);
                }
            }
            return List_data;
        }
    }
}
