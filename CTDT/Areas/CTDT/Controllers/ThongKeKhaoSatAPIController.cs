using CTDT.Helper;
using CTDT.Models;
using CTDT.Models.Khoa;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace CTDT.Areas.CTDT.Controllers
{
    public class ThongKeKhaoSatAPIController : ApiController
    {
        dbSurveyEntities db = new dbSurveyEntities();
        private users user;
        private bool is_user_hop_tac_doanh_nghiep;
        private bool is_user_ctdt;
        private bool is_user_khoa;
        public ThongKeKhaoSatAPIController()
        {
            user = SessionHelper.GetUser();
            is_user_hop_tac_doanh_nghiep = new int?[] { 6 }.Contains(user.id_typeusers);
            is_user_ctdt = new int?[] { 3 }.Contains(user.id_typeusers);
            is_user_khoa = new int?[] { 5 }.Contains(user.id_typeusers);
        }
        #region Load Charts người học
        [HttpPost]
        [Route("api/giam_sat_thong_ke_nguoi_hoc")]
        public async Task<IHttpActionResult> load_charts_nguoi_hoc(FindChartsTyLeKhaoSat find)
        {
            var survey = await db.survey.Where(x => x.id_namhoc == find.id_nam_hoc && x.id_hedaotao == user.id_hdt).ToListAsync();
            var List_data = new List<dynamic>();
            foreach (var items in survey)
            {
                if (items.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu giảng viên")
                {
                    bool check_giang_vien = new[] { 3 }.Contains(items.id_loaikhaosat);
                    bool check_cbvc = new[] { 8 }.Contains(items.id_loaikhaosat);
                    if (check_cbvc)
                    {
                        var cbvc = await db.cbvc_khao_sat.Where(x => x.surveyID == items.surveyID && x.CanBoVienChuc.id_chuongtrinhdaotao == user.id_ctdt).ToListAsync();
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
                        var giang_vien = await db.answer_response.Where(x => x.surveyID == items.surveyID && x.id_ctdt == user.id_ctdt).ToListAsync();
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
                //else if (items.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu doanh nghiệp")
                //{
                //    var ctdt = db.answer_response
                //      .Where(x => x.id_ctdt == user.id_ctdt  && x.id_sv == null)
                //      .AsQueryable();
                //    var TotalAll = ctdt.Count();
                //    var DataCTDT = new
                //    {
                //        id_phieu = items.surveyID,
                //        ten_phieu = items.surveyTitle,
                //        tong_khao_sat = TotalAll,
                //        tong_phieu_da_tra_loi = TotalAll,
                //        tong_phieu_chua_tra_loi = 0,
                //        ty_le_da_tra_loi = TotalAll > 0 ? 100 : 0,
                //        ty_le_chua_tra_loi = 0
                //    };
                //    List_data.Add(DataCTDT);
                //}
                else if (items.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu người học có học phần")
                {
                    bool hoc_vien_co_hoc_phan_ly_thuyet_dang_hoc_tai_truong = new[] { 11 }.Contains(items.id_loaikhaosat);
                    bool hoc_vien_co_hoc_phan_tot_nghiep = new[] { 13 }.Contains(items.id_loaikhaosat);                   
                    if (hoc_vien_co_hoc_phan_ly_thuyet_dang_hoc_tai_truong)
                    {
                        var check_hoc_phan = await db.nguoi_hoc_dang_co_hoc_phan.Where(x => x.surveyID == items.surveyID && x.sinhvien.lop.id_ctdt == user.id_ctdt).ToListAsync();
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
            }
            return Ok(new { data = List_data , success = true});

        }
        #endregion
    }
}
