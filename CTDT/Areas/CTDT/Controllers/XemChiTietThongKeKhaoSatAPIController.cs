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

namespace CTDT.Areas.CTDT.Controllers
{
    public class XemChiTietThongKeKhaoSatAPIController : ApiController
    {
        dbSurveyEntities db = new dbSurveyEntities();
        public users user;
        public XemChiTietThongKeKhaoSatAPIController()
        {
            user = SessionHelper.GetUser();
        }
        //[HttpPost]
        //[Route("api/thong-ke-chi-tiet-khao-sat")]
        //public async Task<IHttpActionResult> LoadChiTiet(survey survey)
        //{
        //    var checkSurvey = await db.survey.FirstOrDefaultAsync(x => x.surveyID == survey.surveyID);
        //    bool hocVienCoHocPhanLyThuyetDangHocTaiTruong = new[] { 11 }.Contains(checkSurvey.id_loaikhaosat);
        //    bool hocVienCoHocPhanTotNghiep = new[] { 13 }.Contains(checkSurvey.id_loaikhaosat);
        //    bool giang_vien_check = new[] { 3 }.Contains(checkSurvey.id_loaikhaosat);
        //    bool cbvc_check = new[] { 8 }.Contains(checkSurvey.id_loaikhaosat);
        //    var dataList = new List<dynamic>();
        //    if (hocVienCoHocPhanLyThuyetDangHocTaiTruong)
        //    {
        //        await LoadChiTietNguoiHocMonHocAsync(survey.surveyID, dataList);
        //    }
        //    else if (giang_vien_check)
        //    {
        //        await load_danh_sach_giang_vien(survey.surveyID, dataList);
        //    }
        //    else if (cbvc_check)
        //    {
        //        await load_danh_sac_cbvc(survey.surveyID, dataList);
        //    }
        //    return Ok(new { phieu = checkSurvey.surveyTitle, data = dataList });
        //}

        //public async Task load_danh_sach_giang_vien(int surveyId, List<dynamic> dataList)
        //{
        //    var get_cbvc = await db.answer_response.Where(x => x.surveyID == surveyId && x.id_ctdt == user.id_ctdt)
        //        .Select(x => new
        //        {
        //            ma_giang_vien = x.CanBoVienChuc.MaCBVC,
        //            ten_giang_vien = x.CanBoVienChuc.TenCBVC,
        //            ctdt_khao_sat = x.ctdt.ten_ctdt
        //        })
        //        .ToListAsync();
        //    if (get_cbvc.Count > 0)
        //    {
        //        dataList.Add(new
        //        {
        //            data = get_cbvc,
        //            success = true,
        //            is_gv = true
        //        });
        //    }
        //    else
        //    {
        //        dataList.Add(new
        //        {
        //            message = "Chưa có dữ liệu người khảo sát trong phiếu này",
        //            success = false,
        //        });
        //    }
        //}
        //public async Task load_danh_sac_cbvc(int surveyId, List<dynamic> dataList)
        //{
        //    var get_cbvc = await db.cbvc_khao_sat.Where(x => x.CanBoVienChuc.id_chuongtrinhdaotao == user.id_ctdt && x.surveyID == surveyId)
        //        .Select(x => new
        //        {
        //            ma_cbvc = x.CanBoVienChuc.MaCBVC,
        //            ten_cbvc = x.CanBoVienChuc.TenCBVC,
        //            thuoc_ctdt = x.CanBoVienChuc.ctdt.ten_ctdt,
        //            DaKhaoSat = x.is_khao_sat == 1 ? "Đã khảo sát" : "Chưa khảo sát"
        //        })
        //        .ToListAsync();
        //    if(get_cbvc.Count > 0)
        //    {
        //        dataList.Add(new
        //        {
        //            data = get_cbvc,
        //            success = true,
        //            is_cbvc = true
        //        });
        //    }
        //    else
        //    {
        //        dataList.Add(new
        //        {
        //            message = "Chưa có dữ liệu người khảo sát trong phiếu này",
        //            success = false,
        //        });
        //    }

        //}
        //public async Task LoadChiTietNguoiHocMonHocAsync(int surveyId, List<dynamic> dataList)
        //{
        //    var getData = await db.nguoi_hoc_dang_co_hoc_phan
        //        .Where(x => x.surveyID == surveyId && x.sinhvien.lop.id_ctdt == user.id_ctdt)
        //        .ToListAsync();

        //    foreach (var item in getData)
        //    {
        //        var getThongTin = await db.nguoi_hoc_dang_co_hoc_phan
        //            .Where(x => x.id_nguoi_hoc_by_hoc_phan == item.id_nguoi_hoc_by_hoc_phan)
        //            .Select(x => new
        //            {
        //                x.CanBoVienChuc.TenCBVC,
        //                x.mon_hoc.hoc_phan.ten_hoc_phan,
        //                x.sinhvien.ma_sv,
        //                x.sinhvien.hovaten,
        //                DaKhaoSat = x.da_khao_sat == 1 ? "Đã khảo sát" : "Chưa khảo sát"
        //            })
        //            .ToListAsync();

        //        var existingItem = dataList.FirstOrDefault(d => d.ten_mon_hoc == item.mon_hoc.ten_mon_hoc);
        //        if (getThongTin.Count == 0)
        //        {
        //            dataList.Add(new
        //            {
        //                message = "Chưa có dữ liệu người khảo sát trong phiếu này",
        //                success = false,
        //            });
        //        }
        //        else if (existingItem != null)
        //        {
        //            existingItem.thong_tin.AddRange(getThongTin);
        //        }
        //        else
        //        {
        //            dataList.Add(new
        //            {
        //                ten_mon_hoc = item.mon_hoc.ten_mon_hoc,
        //                thong_tin = getThongTin,
        //                success = true,
        //                is_nhmh = true
        //            });
        //        }
        //    }
        //}
    }
}
