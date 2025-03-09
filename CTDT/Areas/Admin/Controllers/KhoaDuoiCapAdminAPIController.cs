using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using CTDT.Models;
using Newtonsoft.Json;
using OfficeOpenXml;

namespace CTDT.Areas.Admin.Controllers
{
    public class KhoaDuoiCapAdminAPIController : ApiController
    {
        dbSurveyEntities db = new dbSurveyEntities();
        private int unixTimestamp;
        public KhoaDuoiCapAdminAPIController()
        {
            DateTime now = DateTime.UtcNow;
            unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }
        [HttpPost]
        [Route("api/admin/danh-sach-khoa-duoi-cap")]
        public async Task<IHttpActionResult> load_data(khoa_children items)
        {
            var get_data = await db.khoa_children
                .Where(x =>
                (items.id_nam_hoc == 0 || x.id_nam_hoc == items.id_nam_hoc) &&
                (items.id_khoa_vien_truong == 0 || x.id_khoa_vien_truong == items.id_khoa_vien_truong))
                 .Select(x => new
                 {
                     value = x.id_khoa_children,
                     ten_khoa = x.ten_khoa_children,
                     ten_khoa_vien_truong = x.khoa_vien_truong.ten_khoa,
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
        [Route("api/admin/upload-excel-khoa-duoi-cap")]
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
                            var ten_khoa_vien_truong = worksheet.Cells[row, 2].Text;
                            var ten_khoa = worksheet.Cells[row, 3].Text;
                            var ten_nam_hoc = worksheet.Cells[row, 4].Text;
                            var check_nam_hoc = await db.NamHoc.FirstOrDefaultAsync(x => x.ten_namhoc.ToLower().Trim() == ten_nam_hoc.ToLower().Trim());
                            if (check_nam_hoc == null)
                            {
                                return Ok(new { message = $"Năm học {ten_nam_hoc} không tồn tại hoặc sai định dạng, vui lòng kiểm tra lại và tiếp tục", success = false });
                            }
                            var check_khoa_vien_truong = await db.khoa_vien_truong
                                .FirstOrDefaultAsync(x =>
                                x.ten_khoa.ToLower().Trim() == ten_khoa_vien_truong.ToLower().Trim() &&
                                x.id_namhoc == check_nam_hoc.id_namhoc);
                            if (check_khoa_vien_truong == null)
                            {
                                return Ok(new { message = $"Khoa/Viện/Trường {ten_khoa_vien_truong} không tồn tại hoặc sai định dạng, vui lòng kiểm tra lại và tiếp tục", success = false });
                            }
                            var check_khoa = await db.khoa_children
                                .FirstOrDefaultAsync(x => x.ten_khoa_children.ToLower().Trim() == ten_khoa.ToLower().Trim());
                            if (check_khoa == null)
                            {
                                check_khoa = new khoa_children
                                {
                                    ten_khoa_children = ten_khoa,
                                    id_khoa_vien_truong = check_khoa_vien_truong.id_khoa,
                                    id_nam_hoc = check_nam_hoc.id_namhoc,
                                    ngay_tao = unixTimestamp,
                                    ngay_cap_nhat = unixTimestamp,
                                };
                                db.khoa_children.Add(check_khoa);
                            }
                            else
                            {
                                check_khoa.ngay_cap_nhat = unixTimestamp;
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
