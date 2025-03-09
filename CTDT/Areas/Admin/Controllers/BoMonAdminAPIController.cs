using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;
using System.Web.Http;
using CTDT.Models;
using Newtonsoft.Json;
using OfficeOpenXml;

namespace CTDT.Areas.Admin.Controllers
{
    public class BoMonAdminAPIController : ApiController
    {
        dbSurveyEntities db = new dbSurveyEntities();
        private int unixTimestamp;
        public BoMonAdminAPIController()
        {
            DateTime now = DateTime.UtcNow;
            unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }
        public class GetBoMon
        {
            public int don_vi { get; set; }
            public int khoa { get; set; }
            public int nam_hoc { get; set; }
        }
        [HttpPost]
        [Route("api/admin/option-bo-mon")]
        public async Task<IHttpActionResult> option_bo_mon(GetBoMon options)
        {
            var get_don_vi = await db.khoa_vien_truong
                .Where(x => x.id_namhoc == options.nam_hoc)
                .Select(x => new
                {
                    value = x.id_khoa,
                    text = x.ten_khoa
                })
                .ToListAsync();

            var get_khoa = new List<dynamic>();

            if (options.don_vi != 0) 
            {
                var _get_khoa = await db.khoa_children
                    .Where(x => x.id_khoa_vien_truong == options.don_vi)
                    .Select(x => new
                    {
                        value = x.id_khoa_children,
                        text = x.ten_khoa_children
                    })
                    .ToListAsync();
                get_khoa.AddRange(_get_khoa);
            }
            return Ok(new { don_vi = get_don_vi, khoa = get_khoa });
        }
        [HttpPost]
        [Route("api/admin/danh-sach-bo-mon")]
        public async Task<IHttpActionResult> danh_sach_bo_mon(GetBoMon items)
        {
            var get_data = await db.bo_mon
                .Where(x => (items.khoa == 0 || x.id_khoa_children == items.khoa) &&
                (items.nam_hoc == 0 || x.id_nam_hoc == items.nam_hoc) &&
                (items.don_vi == 0 || x.khoa_children.id_khoa_vien_truong == items.don_vi))
                .Select(x => new
                {
                    value = x.id_bo_mon,
                    ten_bo_mon = x.ten_bo_mon,
                    thuoc_khoa = x.khoa_children.ten_khoa_children,
                    thuoc_don_vi = x.khoa_vien_truong.ten_khoa,
                    nam_hoc = x.NamHoc.ten_namhoc,
                    x.ngay_tao,
                    x.ngay_cap_nhat
                })
                .ToListAsync();
            if (get_data.Any())
            {
                return Ok(new { data = JsonConvert.SerializeObject(get_data), success = true });
            }
            else
            {
                return Ok(new { message = "Không có dữ liệu", success = false });
            }
        }
        [HttpPost]
        [Route("api/admin/upload-excel-bo-mon")]
        public async Task<IHttpActionResult> UploadExcelMonHoc()
        {
            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            foreach (var file in provider.Contents)
            {
                var fileName = file.Headers.ContentDisposition.FileName.Trim('\"');
                var fileStream = await file.ReadAsStreamAsync();

                if (fileName.EndsWith(".xlsx") || fileName.EndsWith(".xls"))
                {
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                    using (var package = new ExcelPackage(fileStream))
                    {
                        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                        if (worksheet == null)
                        {
                            return Ok(new { status = "Không tìm thấy worksheet trong file Excel", success = false });
                        }

                        for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                        {
                            var ten_don_vi = worksheet.Cells[row, 2].Text;
                            var ten_khoa = worksheet.Cells[row, 3].Text;
                            var ten_bo_mon = worksheet.Cells[row, 4].Text;
                            var ten_nam_hoc = worksheet.Cells[row, 5].Text;
                            var check_nam_hoc = await db.NamHoc.FirstOrDefaultAsync(x => x.ten_namhoc == ten_nam_hoc);
                            if (check_nam_hoc == null)
                            {
                                return Ok(new { message = $"Năm học {ten_nam_hoc} không tồn tại hoặc sai định dạng, vui lòng kiểm tra lại và tiếp tục", success = false });
                            }
                            var check_don_vi = await db.khoa_vien_truong.FirstOrDefaultAsync(x => x.ten_khoa.ToLower().Trim() == ten_don_vi.ToLower().Trim());
                            if (check_don_vi == null)
                            {
                                return Ok(new { message = $"Đơn vị {ten_don_vi} không tồn tại hoặc sai định dạng, vui lòng kiểm tra lại và tiếp tục", success = false });
                            }
                            var check_khoa = await db.khoa_children.FirstOrDefaultAsync(x => x.ten_khoa_children.ToLower().Trim() == ten_khoa.ToLower().Trim());
                            if (!string.IsNullOrWhiteSpace(ten_khoa) && check_khoa == null)
                            {
                                return Ok(new { message = $"Khoa {ten_khoa} không tồn tại hoặc sai định dạng, vui lòng kiểm tra lại và tiếp tục", success = false });
                            }
                            var check_bo_mon = await db.bo_mon
                                .FirstOrDefaultAsync(x =>
                                x.ten_bo_mon.ToLower().Trim() == ten_bo_mon.ToLower().Trim() &&
                                x.id_nam_hoc == check_nam_hoc.id_namhoc);
                            if (check_bo_mon == null)
                            {
                                check_bo_mon = new bo_mon
                                {
                                    ten_bo_mon = ten_bo_mon,
                                    id_khoa_vien_truong = check_don_vi.id_khoa,
                                    id_khoa_children = string.IsNullOrWhiteSpace(ten_khoa) ? (int?)null : check_khoa.id_khoa_children,
                                    id_nam_hoc = check_nam_hoc.id_namhoc,
                                    ngay_tao = unixTimestamp,
                                    ngay_cap_nhat = unixTimestamp,
                                };
                                db.bo_mon.Add(check_bo_mon);
                            }
                            else
                            {
                                check_bo_mon.id_khoa_vien_truong = check_don_vi.id_khoa;
                                check_bo_mon.id_khoa_children = string.IsNullOrWhiteSpace(ten_khoa) ? (int?)null : check_khoa.id_khoa_children;
                                check_bo_mon.ngay_cap_nhat = unixTimestamp;
                            }
                            await db.SaveChangesAsync();
                        }
                        return Ok(new { message = "Import dữ liệu thành công", success = true });
                    }
                }
                else
                {
                    return Ok(new { message = "Chỉ hỗ trợ upload file Excel.", success = false });
                }
            }
            return Ok(new { message = "Vui lòng chọn file Excel.", success = false });
        }
    }
}
