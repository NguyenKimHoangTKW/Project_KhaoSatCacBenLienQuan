using CTDT.Models;
using Newtonsoft.Json;
using OfficeOpenXml;
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
    public class KhoaVienTruongAPIController : ApiController
    {
        dbSurveyEntities db = new dbSurveyEntities();
        private int unixTimestamp;
        public KhoaVienTruongAPIController()
        {
            DateTime now = DateTime.UtcNow;
            unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }
        [HttpPost]
        [Route("api/admin/load-danh-sach-khoa-vien-truong")]
        public async Task<IHttpActionResult> load_danh_sach_khoa_vien_truong(khoa_vien_truong items)
        {
            var get_data = await db.khoa_vien_truong
               .Where(x => items.id_namhoc == 0 || x.id_namhoc == items.id_namhoc)
                .Select(x => new
                {
                    x.id_khoa,
                    x.ma_khoa,
                    x.ten_khoa,
                    x.NamHoc.ten_namhoc,
                    x.ngaycapnhat,
                    x.ngaytao
                }).ToListAsync();
            if (get_data.Any())
            {
                return Ok(new { data = JsonConvert.SerializeObject(get_data), success = true });
            }
            else
            {
                return Ok(new { message = "Chưa có dữ liệu tồn tại", success = false });
            }
        }
        [HttpPost]
        [Route("api/admin/them-moi-khoa-vien-truong")]
        public IHttpActionResult them_moi_khoa_vien_truong(khoa_vien_truong khoa)
        {
            DateTime now = DateTime.UtcNow;
            int unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            if (string.IsNullOrEmpty(khoa.ten_khoa))
            {
                return Ok(new { message = "Không được bỏ trống tên khoa", success = false });
            }
            var newkhoa = new khoa_vien_truong
            {
                ma_khoa = khoa.ma_khoa,
                ten_khoa = khoa.ten_khoa,
                ngaycapnhat = unixTimestamp,
                id_namhoc = khoa.id_namhoc,
                ngaytao = unixTimestamp
            };
            db.khoa_vien_truong.Add(newkhoa);
            db.SaveChanges();
            return Ok(new { message = "Cập nhật dữ liệu thành công", success = true });
        }
        [HttpPost]
        [Route("api/admin/get-info-khoa-vien-truong")]
        public async Task<IHttpActionResult> get_info_khoa_vien_truong(khoa_vien_truong khoa)
        {
            var get_info = await db.khoa_vien_truong
                .Where(x => x.id_khoa == khoa.id_khoa)
                .Select(x => new
                {
                    x.ma_khoa,
                    x.ten_khoa,
                    x.id_namhoc
                }).FirstOrDefaultAsync();
            return Ok(JsonConvert.SerializeObject(get_info));
        }
        [HttpPost]
        [Route("api/admin/update-khoa-vien-truong")]
        public IHttpActionResult update_khoa_vien_truong(khoa_vien_truong khoa)
        {
            DateTime now = DateTime.UtcNow;
            int unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            var check_khoa = db.khoa_vien_truong.FirstOrDefault(x => x.id_khoa == khoa.id_khoa);
            if (string.IsNullOrEmpty(khoa.ten_khoa))
            {
                return Ok(new { message = "Không được bỏ trống tên khoa", success = false });
            }
            check_khoa.ma_khoa = khoa.ma_khoa;
            check_khoa.ten_khoa = khoa.ten_khoa;
            check_khoa.id_namhoc = khoa.id_namhoc;
            check_khoa.ngaycapnhat = unixTimestamp;
            db.SaveChanges();
            return Ok(new { message = "Cập nhật dữ liệu thành công", success = true });
        }
        [HttpPost]
        [Route("api/admin/delete-khoa-vien-truong")]
        public IHttpActionResult delete_khoa(khoa_vien_truong khoa)
        {
            var check_khoa = db.khoa_vien_truong.FirstOrDefault(x => x.id_khoa == khoa.id_khoa);

            db.khoa_vien_truong.Remove(check_khoa);
            db.SaveChanges();
            return Ok(new { message = "Cập nhật dữ liệu thành công", success = true });
        }
        [HttpPost]
        [Route("api/admin/upload-excel-khoa-vien-truong")]
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
                            var ma_khoa = worksheet.Cells[row, 2].Text;
                            var ten_khoa = worksheet.Cells[row, 3].Text;
                            var ten_nam_hoc = worksheet.Cells[row, 4].Text;
                            var check_nam_hoc = await db.NamHoc.FirstOrDefaultAsync(x => x.ten_namhoc.ToLower().Trim() == ten_nam_hoc.ToLower().Trim());
                            if (check_nam_hoc == null)
                            {
                                return Ok(new { message = $"Năm học {ten_nam_hoc} không tồn tại hoặc sai định dạng, vui lòng kiểm tra lại và tiếp tục", success = false });
                            }
                            var check_khoa = await db.khoa_vien_truong
                                .FirstOrDefaultAsync(x => x.ma_khoa.ToLower().Trim() == ma_khoa.ToLower().Trim() &&
                                x.ten_khoa.ToLower().Trim() == ten_khoa.ToLower().Trim());
                            if (check_khoa == null)
                            {
                                check_khoa = new khoa_vien_truong
                                {
                                    ma_khoa = string.IsNullOrWhiteSpace(ma_khoa) ? null : ma_khoa.ToUpper(),
                                    ten_khoa = ten_khoa,
                                    id_namhoc = check_nam_hoc.id_namhoc,
                                    ngaytao = unixTimestamp,
                                    ngaycapnhat = unixTimestamp,
                                };
                                db.khoa_vien_truong.Add(check_khoa);
                            }
                            else
                            {
                                check_khoa.ngaycapnhat = unixTimestamp;
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
