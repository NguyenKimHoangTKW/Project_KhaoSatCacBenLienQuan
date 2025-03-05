using CTDT.Models;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Windows.Media;

namespace CTDT.Areas.Admin.Controllers
{
    public class MonHocAdminAPIController : ApiController
    {
        dbSurveyEntities db = new dbSurveyEntities();
        private int unixTimestamp;
        public MonHocAdminAPIController()
        {
            DateTime now = DateTime.UtcNow;
            unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }
        public class GetItemsMonHoc
        {
            public int namhoc { get; set; }
            public int hocphan { get; set; }
        }
        [HttpPost]
        [Route("api/admin/danh-sach-mon-hoc")]
        public async Task<IHttpActionResult> load_danh_sach_mon_hoc(GetItemsMonHoc items)
        {
            var get_data = await db.mon_hoc
                .Where(x => x.id_nam_hoc == items.namhoc &&
                           (items.hocphan == 0 || x.id_hoc_phan == items.hocphan))
                .OrderByDescending(x => x.id_mon_hoc)
                .Select(x => new
                {
                    value = x.id_mon_hoc,
                    ma_mon_hoc = x.ma_mon_hoc != null ? x.ma_mon_hoc : "",
                    x.ten_mon_hoc,
                    thuoc_lop = x.id_lop != null ? x.lop.ma_lop : "",
                    thuoc_hoc_phan = x.id_hoc_phan != null ? x.hoc_phan.ten_hoc_phan : "",
                    x.ngay_tao,
                    x.ngay_cap_nhat,
                    thuoc_nam = x.NamHoc.ten_namhoc,
                })
                .ToListAsync();
            if (get_data.Count > 0)
            {
                return Ok(new { data = JsonConvert.SerializeObject(get_data), success = true });
            }
            else
            {
                return Ok(new { message = "Không có dữ liệu môn học trong năm", success = false });
            }
        }
        [HttpPost]
        [Route("api/admin/them-moi-mon-hoc")]
        public async Task<IHttpActionResult> add_new(mon_hoc value)
        {
            if (db.mon_hoc.FirstOrDefault(x => x.ma_mon_hoc == value.ma_mon_hoc && x.ten_mon_hoc == value.ten_mon_hoc) != null)
            {
                return Ok(new { message = "Môn học và mã môn học này đã tồn tại, vui lòng kiểm tra lại dữ liệu", success = false });
            }

            if (string.IsNullOrEmpty(value.ma_mon_hoc))
            {
                return Ok(new { message = "Không được bỏ trống mã môn học", success = false });
            }
            if (string.IsNullOrEmpty(value.ten_mon_hoc))
            {
                return Ok(new { message = "Không được bỏ trống tên môn học", success = false });
            }
            value.ngay_tao = unixTimestamp;
            value.ngay_cap_nhat = unixTimestamp;
            db.mon_hoc.Add(value);
            await db.SaveChangesAsync();
            return Ok(new { message = "Cập nhật dữ liệu thành công", success = true });
        }
        [HttpPost]
        [Route("api/admin/info-mon-hoc")]
        public async Task<IHttpActionResult> info(mon_hoc value)
        {
            if (value.id_mon_hoc == null)
            {
                return Ok(new { message = "Không tìm thấy dữ liệu", success = false });
            }
            var get_data = await db.mon_hoc
                .Where(x => x.id_mon_hoc == value.id_mon_hoc)
                .Select(x => new
                {
                    x.ma_mon_hoc,
                    x.ten_mon_hoc,
                    lop = x.id_lop,
                    hoc_phan = x.id_hoc_phan,
                    nam_hoc = x.id_nam_hoc,
                }).FirstOrDefaultAsync();
            return Ok(new { data = JsonConvert.SerializeObject(get_data), success = true });
        }
        [HttpPost]
        [Route("api/admin/update-mon-hoc")]
        public async Task<IHttpActionResult> update(mon_hoc value)
        {
            if (value.id_mon_hoc == null)
            {
                return Ok(new { message = "Không tìm thấy dữ liệu", success = false });
            }
            var data = await db.mon_hoc
                .FirstOrDefaultAsync(x => x.id_mon_hoc == value.id_mon_hoc);
            data.ma_mon_hoc = value.ma_mon_hoc;
            data.ten_mon_hoc = value.ten_mon_hoc;
            data.id_lop = value.id_lop;
            data.id_hoc_phan = value.id_hoc_phan;
            data.id_nam_hoc = value.id_nam_hoc;
            data.ngay_cap_nhat = unixTimestamp;
            await db.SaveChangesAsync();
            return Ok(new { message = "Cập nhật dữ liệu thành công", success = true });
        }
        [HttpPost]
        [Route("api/admin/delete-mon-hoc")]
        public IHttpActionResult delete(mon_hoc value)
        {
            if (value.id_mon_hoc == null)
            {
                return Ok(new { message = "Không tìm thấy dữ liệu", success = false });
            }
            var check_nguoi_hoc_co_hoc_phan = db.nguoi_hoc_dang_co_hoc_phan
                .Where(x => x.id_mon_hoc == value.id_mon_hoc)
                .ToList();
            if (check_nguoi_hoc_co_hoc_phan.Any())
            {
                return Ok(new { message = "Môn học này đang tồn tại người học, chỉ có thể xóa cứng trong chức năng xóa dữ liệu theo năm", success = false });
            }
            var check_mon_hoc = db.mon_hoc.FirstOrDefault(x => x.id_mon_hoc == value.id_mon_hoc);
            db.mon_hoc.Remove(check_mon_hoc);
            db.SaveChanges();
            return Ok(new { message = "Cập nhật dữ liệu thành công", success = true });
        }
        [HttpPost]
        [Route("api/admin/upload-excel-mon-hoc")]
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
                            var ma_mon_hoc = worksheet.Cells[row, 2].Text;
                            var ten_mon_hoc = worksheet.Cells[row, 3].Text;
                            var ten_lop = worksheet.Cells[row, 4].Text;
                            var ten_hoc_phan = worksheet.Cells[row, 5].Text;
                            var nam_hoc = worksheet.Cells[row, 6].Text;

                            var check_lop = await db.lop.FirstOrDefaultAsync(x => x.ma_lop.ToLower().Trim() == ten_lop.ToLower().Trim());
                            if (check_lop == null)
                            {
                                return Ok(new { message = $"Lớp {ten_lop} không tồn tại, vui lòng thêm lớp này vào hệ thống và upload lại!", success = false });
                            }
                            if (string.IsNullOrEmpty(ma_mon_hoc))
                            {
                                return Ok(new { message = $"Mã môn học có 1 trường bỏ trống, vui lòng điền mã môn học để tiếp tục", success = false });
                            }
                            if (string.IsNullOrEmpty(ten_mon_hoc))
                            {
                                return Ok(new { message = $"Tên môn học có 1 trường bỏ trống, vui lòng điền tên môn học để tiếp tục", success = false });
                            }
                            var check_hoc_phan = await db.hoc_phan.FirstOrDefaultAsync(x => x.ten_hoc_phan.ToLower().Trim() == ten_hoc_phan.ToLower().Trim());
                            if (check_hoc_phan == null)
                            {
                                return Ok(new { message = $"Học phần {ten_hoc_phan} không tồn tại, vui lòng kiểm tra lại và tiếp tục", success = false });
                            }
                            var check_nam_hoc = await db.NamHoc.FirstOrDefaultAsync(x => x.ten_namhoc.ToLower().Trim() == nam_hoc.ToLower().Trim());
                            if (check_nam_hoc == null)
                            {
                                return Ok(new { message = $"Năm học {nam_hoc} không tồn tại hoặc sai định dạng, vui lòng kiểm tra lại và tiếp tục", success = false });
                            }
                            var check_mon_hoc = await db.mon_hoc
                                .FirstOrDefaultAsync(x => 
                                x.ma_mon_hoc.ToLower().Trim() == ma_mon_hoc.ToLower().Trim() &&
                                x.ten_mon_hoc.ToLower().Trim() == ten_mon_hoc.ToLower().Trim() &&
                                x.id_nam_hoc == check_nam_hoc.id_namhoc);
                            if (check_mon_hoc == null)
                            {
                                check_mon_hoc = new mon_hoc
                                {
                                    ma_mon_hoc = ma_mon_hoc.ToUpper(),
                                    ten_mon_hoc= ten_mon_hoc,
                                    id_lop = check_lop.id_lop,
                                    id_hoc_phan = check_hoc_phan.id_hoc_phan,
                                    id_nam_hoc = check_nam_hoc.id_namhoc,
                                    ngay_tao = unixTimestamp,
                                    ngay_cap_nhat = unixTimestamp,
                                };
                                db.mon_hoc.Add(check_mon_hoc);
                            }
                            else
                            {
                                check_mon_hoc.ngay_cap_nhat = unixTimestamp;
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
