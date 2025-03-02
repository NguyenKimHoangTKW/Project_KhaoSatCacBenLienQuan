using CTDT.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Management;
using static CTDT.Areas.Admin.Controllers.QuanLyNguoiHocKhaoSatAPIController;

namespace CTDT.Areas.Admin.Controllers
{
    public class QuanLyCBVCKhaoSatPhieuAPIController : ApiController
    {
        dbSurveyEntities db = new dbSurveyEntities();
        public class GetItemsCBVC
        {
            public int surveyID { get; set; }
            public int year { get; set; }
            public int[] id_cbvc { get; set; }
        }
        [HttpPost]
        [Route("api/admin/load-phieu-by-nam-thuoc-can-bo-vien-chuc")]
        public async Task<IHttpActionResult> load_pks_by_year(survey survey)
        {
            var validGroupIds = new List<int> { 2 };
            var pks = await db.survey
                .Where(x => x.id_namhoc == survey.id_namhoc
                            && x.id_hedaotao == survey.id_hedaotao
                            && x.mo_thong_ke == 1
                            && validGroupIds.Contains(x.LoaiKhaoSat.group_loaikhaosat.id_gr_loaikhaosat)
                            && x.id_loaikhaosat != 3)
                .Select(x => new
                {
                    id_phieu = x.surveyID,
                    ten_phieu = x.dot_khao_sat.ten_dot_khao_sat != null
                                ? x.surveyTitle + " - " + x.dot_khao_sat.ten_dot_khao_sat
                                : x.surveyTitle,
                })
                .ToListAsync();
            var ctdt = await db.ctdt
                .Where(x => x.id_hdt == survey.id_hedaotao)
                .Select(x => new
                {
                    id_ctdt = x.id_ctdt,
                    ten_ctdt = x.ten_ctdt
                })
                .ToListAsync();
            var sortedPks = pks.OrderBy(p =>
            {
                var match = System.Text.RegularExpressions.Regex.Match(p.ten_phieu, @"Phiếu (\d+)");
                return match.Success ? int.Parse(match.Groups[1].Value) : int.MaxValue;
            }).ToList();

            if (sortedPks.Count > 0)
            {
                return Ok(new { data = sortedPks, ctdt = ctdt, success = true });
            }
            else
            {
                return Ok(new { message = "Không có dữ liệu phiếu khảo sát", success = false });
            }
        }
        [HttpPost]
        [Route("api/admin/danh-sach-cbvc-khao-sat-phieu")]
        public async Task<IHttpActionResult> load_danh_sach_cbvc(GetItemsCBVC items)
        {
            var get_cbvc_ks = await db.CanBoVienChuc
                .Where(x => x.id_namhoc == items.year)
                .Select(x => new
                {
                    id = x.id_CBVC,
                    ma_cbvc = x.MaCBVC != null ? x.MaCBVC : "",
                    ten_cbvc = x.TenCBVC,
                    email = x.Email != null ? x.Email : "",
                    don_vi = x.id_donvi != null ? x.DonVi.name_donvi : "",
                    ctdt = x.id_chuongtrinhdaotao != null ? x.ctdt.ten_ctdt : "",
                    nam_hoat_dong = x.NamHoc.ten_namhoc,
                    mo_ta = x.description != null ? x.description : ""
                })
                .ToListAsync();
            var check_select_dap_vien = await db.cbvc_khao_sat
                .Where(x => x.surveyID == items.surveyID)
                .Select(x => new
                {
                    value = x.id_cbvc,
                })
                .ToListAsync();
            if (get_cbvc_ks.Count > 0)
            {
                return Ok(new { data = get_cbvc_ks, selected = check_select_dap_vien, success = true });
            }
            else
            {
                return Ok(new { message = "Chưa có dữ liệu", success = false });
            }
        }
        [HttpPost]
        [Route("api/admin/save-dap-vien-khao-sat-can-bo-vien-chuc")]
        public async Task<IHttpActionResult> save_dap_vien(GetItemsCBVC getitems)
        {
            if (getitems.surveyID == 0)
            {
                return Ok(new { message = "Không tìm thấy phiếu khảo sát tương ứng", success = false });
            }
            var surveyID = getitems.surveyID;
            var idList = getitems.id_cbvc.ToHashSet();
            var existingRecords = await db.cbvc_khao_sat
                .Where(x => x.surveyID == surveyID)
                .ToListAsync();
            var existingIds = existingRecords.Select(x => x.id_cbvc).ToHashSet();
            var newRecords = idList
                .Where(id => !existingIds.Contains(id))
                .Select(id => new cbvc_khao_sat
                {
                    surveyID = surveyID,
                    id_cbvc = id,
                    is_khao_sat = 0
                })
                .ToList();
            if (newRecords.Any())
            {
                db.cbvc_khao_sat.AddRange(newRecords);
            }
            var recordsToDelete = existingRecords.Where(x => !idList.Contains(x.id_cbvc)).ToList();
            if (recordsToDelete.Any())
            {
                db.cbvc_khao_sat.RemoveRange(recordsToDelete);
            }
            await db.SaveChangesAsync();
            return Ok(new { message = "Cập nhật dữ liệu thành công", success = true });
        }

        [HttpPost]
        [Route("api/admin/info-cbvc-da-chon-khao-sat")]
        public async Task<IHttpActionResult> load_info_cbvc_chon_khao_sat(GetItemsCBVC getitems)
        {
            if (getitems.surveyID == 0)
            {
                return Ok(new { message = "Không tìm thấy phiếu khảo sát tương ứng", success = false });
            }
            var get_data = await db.cbvc_khao_sat
                .Where(x => x.surveyID == getitems.surveyID)
                .Select(x => new
                {
                    id = x.CanBoVienChuc.id_CBVC,
                    ma_cbvc = x.CanBoVienChuc.MaCBVC != null ? x.CanBoVienChuc.MaCBVC : "",
                    ten_cbvc = x.CanBoVienChuc.TenCBVC,
                    email = x.CanBoVienChuc.Email != null ? x.CanBoVienChuc.Email : "",
                    don_vi = x.CanBoVienChuc.id_donvi != null ? x.CanBoVienChuc.DonVi.name_donvi : "",
                    ctdt = x.CanBoVienChuc.id_chuongtrinhdaotao != null ? x.CanBoVienChuc.ctdt.ten_ctdt : "",
                    nam_hoat_dong = x.CanBoVienChuc.NamHoc.ten_namhoc,
                    mo_ta = x.CanBoVienChuc.description != null ? x.CanBoVienChuc.description : ""
                })
                .ToListAsync();
            if (get_data.Count > 0)
            {
                return Ok(new { data = get_data, success = true });
            }
            else
            {
                return Ok(new { message = "Chưa tồn tại dữ liệu đáp viên đã chọn", success = false });
            }
        }
    }
}
