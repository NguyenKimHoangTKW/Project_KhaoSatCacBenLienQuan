using CTDT.Models;
using Google.Apis.Util;
using OfficeOpenXml.ConditionalFormatting.Contracts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace CTDT.Areas.Admin.Controllers
{
    public class QuanLyNguoiHocKhaoSatAPIController : ApiController
    {
        dbSurveyEntities db = new dbSurveyEntities();
        public class GetAllSV
        {
            public int surveyID { get; set; }
            public int id_hdt { get; set; }
            public int[] id_lop { get; set; }
            public int[] id_nh { get; set; }
            public string[] ma_nh { get; set; }
        }
        [HttpPost]
        [Route("api/admin/load-phieu-by-nam-thuoc-nguoi-hoc")]
        public async Task<IHttpActionResult> load_pks_by_year(survey survey)
        {
            var validGroupIds = new List<int> { 1, 5 };
            var pks = await db.survey
                .Where(x => x.id_namhoc == survey.id_namhoc
                            && x.id_hedaotao == survey.id_hedaotao
                            && x.mo_thong_ke == 1
                            && validGroupIds.Contains(x.LoaiKhaoSat.group_loaikhaosat.id_gr_loaikhaosat))
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
        [Route("api/admin/danh-sach-dap-vien-chon-khao-sat")]
        public async Task<IHttpActionResult> load_danh_sach_nguoi_hoc(GetAllSV getitems)
        {
            var list_data = new List<dynamic>();

            if (getitems.ma_nh.ToList().Count > 0)
            {
                await load_dap_vien_co_dtdb(list_data, getitems);
            }
            else
            {
                await load_dap_vien_khong_dtdb(list_data, getitems);
            }

            var check_select_dap_vien = await db.nguoi_hoc_khao_sat
                .Where(x => x.surveyID == getitems.surveyID)
                .Select(x => new
                {
                    value = x.id_sv,
                })
                .ToListAsync();

            if (list_data.Count > 0)
            {
                return Ok(new { data = list_data, selected = check_select_dap_vien, success = true });
            }
            else
            {
                return Ok(new { message = "Chưa có dữ liệu người học", success = false });
            }
        }
        public async Task<dynamic> load_dap_vien_khong_dtdb(dynamic list_data, GetAllSV getitems)
        {
            foreach (var items in getitems.id_lop)
            {
                var get_all_nguoi_hoc = await db.sinhvien
                .Where(x => x.id_lop == items)
                .OrderByDescending(x => x.lop.ma_lop)
                .Select(x => new
                {
                    id = x.id_sv,
                    x.lop.ma_lop,
                    x.ma_sv,
                    x.hovaten,
                })
                .ToListAsync();
                list_data.AddRange(get_all_nguoi_hoc);
            }
            return list_data;
        }
        public async Task<dynamic> load_dap_vien_co_dtdb(dynamic list_data, GetAllSV getitems)
        {
            foreach (var items in getitems.ma_nh)
            {
                var get_all_nguoi_hoc = await db.sinhvien
                .Where(x => x.ma_sv == items)
                .Select(x => new
                {
                    id = x.id_sv,
                    x.lop.ma_lop,
                    x.ma_sv,
                    x.hovaten,
                })
                .ToListAsync();
                list_data.AddRange(get_all_nguoi_hoc);
            }
            return list_data;
        }
        [HttpPost]
        [Route("api/admin/lop-by-hdt")]
        public async Task<IHttpActionResult> load_lop_by_hdt(GetAllSV getitems)
        {
            var get_lop = await db.lop
                .Where(x => x.ctdt.id_hdt == getitems.id_hdt)
                .OrderByDescending(x => x.ma_lop)
                .Select(x => new
                {
                    value = x.id_lop,
                    name = x.ma_lop
                })
                .ToListAsync();
            var _get_lop = await db.nguoi_hoc_khao_sat
                .Where(x => x.surveyID == getitems.surveyID)
                .Select(x => new
                {
                    value = x.sinhvien.lop.id_lop,
                    name = x.sinhvien.lop.ma_lop
                })
                .Distinct()
                .ToListAsync();
            if (get_lop.Count > 0)
            {
                return Ok(new { data = get_lop, lop = _get_lop, success = true });
            }
            else
            {
                return Ok(new { message = "Chưa có dữ liệu", success = false });
            }
        }
        [HttpPost]
        [Route("api/admin/save-dap-vien-khao-sat-nguoi-hoc")]
        public async Task<IHttpActionResult> save_dap_vien(GetAllSV getitems)
        {
            if (getitems.surveyID == 0)
            {
                return Ok(new { message = "Không tìm thấy phiếu khảo sát tương ứng", success = false });
            }
            var surveyID = getitems.surveyID;
            var idList = getitems.id_nh.ToHashSet();
            var existingRecords = await db.nguoi_hoc_khao_sat
                .Where(x => x.surveyID == surveyID)
                .ToListAsync();
            var existingIds = existingRecords.Select(x => x.id_sv).ToHashSet();
            var newRecords = idList
                .Where(id => !existingIds.Contains(id))
                .Select(id => new nguoi_hoc_khao_sat
                {
                    surveyID = surveyID,
                    id_sv = id,
                    is_khao_sat = 0
                })
                .ToList();
            if (newRecords.Any())
            {
                db.nguoi_hoc_khao_sat.AddRange(newRecords);
            }
            var recordsToDelete = existingRecords.Where(x => !idList.Contains(x.id_sv)).ToList();
            if (recordsToDelete.Any())
            {
                db.nguoi_hoc_khao_sat.RemoveRange(recordsToDelete);
            }

            await db.SaveChangesAsync();
            return Ok(new { message = "Cập nhật dữ liệu thành công", success = true });
        }
        [HttpPost]
        [Route("api/admin/info-nguoi-hoc-da-chon-khao-sat")]
        public async Task<IHttpActionResult> loa_info_ddanh_sach_nguoi_hoc_chon_khao_sat(GetAllSV getitems)
        {
            if (getitems.surveyID == 0)
            {
                return Ok(new { message = "Không tìm thấy phiếu khảo sát tương ứng", success = false });
            }
            var get_items = await db.nguoi_hoc_khao_sat
                .Where(x => x.surveyID == getitems.surveyID)
                .Select(x => new
                {
                    x.sinhvien.lop.ma_lop,
                    x.sinhvien.ma_sv,
                    x.sinhvien.hovaten
                }).ToListAsync();
            if (get_items.Count > 0)
            {
                return Ok(new { data = get_items, success = true });
            }
            else
            {
                return Ok(new { message = "Chưa tồn tại dữ liệu đáp viên đã chọn", success = false });
            }
        }
    }
}
