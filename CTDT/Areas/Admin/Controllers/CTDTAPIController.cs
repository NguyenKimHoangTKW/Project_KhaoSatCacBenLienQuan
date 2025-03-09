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
    public class CTDTAPIController : ApiController
    {
        dbSurveyEntities db = new dbSurveyEntities();
        private int unixTimestamp;
        public CTDTAPIController()
        {
            DateTime now = DateTime.UtcNow;
            unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }
        [HttpPost]
        [Route("api/admin/danh-sach-ctdt")]
        public async Task<IHttpActionResult> danh_sach_ctdt(ctdt ctdt)
        {
            var get_data = db.ctdt.AsQueryable();

            if (ctdt.id_hdt != 0)
            {
                get_data = get_data.Where(x => x.id_hdt == ctdt.id_hdt);
            }
            if (ctdt.id_bo_mon != 0)
            {
                get_data = get_data.Where(x => x.id_bo_mon == ctdt.id_bo_mon);
            }
            var query = await get_data
                .Select(x => new
                {
                    x.id_ctdt,
                    ma_ctdt = x.ma_ctdt != null ? x.ma_ctdt : "",
                    x.ten_ctdt,
                    ten_hedaotao = x.id_hdt != null ? x.hedaotao.ten_hedaotao : "",
                    ten_bo_mon = x.id_bo_mon != null ? x.bo_mon.ten_bo_mon : "",
                    ten_don_vi = x.khoa_vien_truong.ten_khoa,
                    ten_khoa = x.khoa_children.ten_khoa_children,
                    x.ngaytao,
                    x.ngaycapnhat
                }).ToListAsync();
            if (query.Any())
            {
                return Ok(new { data = JsonConvert.SerializeObject(query), success = true });
            }
            else
            {
                return Ok(new { message = "Chưa có dữ liệu tồn tại", success = false });
            }
        }
        [HttpPost]
        [Route("api/admin/them-moi-ctdt")]
        public IHttpActionResult them_moi_ctdt(ctdt ctdt)
        {
            if (string.IsNullOrEmpty(ctdt.ten_ctdt))
            {
                return Ok(new { message = "Không được bỏ trống tên ctdt", success = false });
            }
            var add_data = new ctdt
            {
                ma_ctdt = ctdt.ma_ctdt,
                ten_ctdt = ctdt.ten_ctdt,
                id_hdt = ctdt.id_hdt,
                ngaycapnhat = unixTimestamp,
                ngaytao = unixTimestamp
            };
            db.ctdt.Add(add_data);
            db.SaveChanges();
            return Ok(new { message = "Cập nhật dữ liệu thành công", success = true });
        }
        [HttpPost]
        [Route("api/admin/get-info-ctdt")]
        public async Task<IHttpActionResult> get_info_ctdt(ctdt ctdt)
        {
            var get_info = await db.ctdt
                .Where(x => x.id_ctdt == ctdt.id_ctdt)
                .Select(x => new
                {
                    x.ma_ctdt,
                    x.ten_ctdt,
                    x.id_hdt
                }).FirstOrDefaultAsync();
            return Ok(get_info);
        }
        [HttpPost]
        [Route("api/admin/cap-nhat-ctdt")]
        public IHttpActionResult update_ctdt(ctdt ctdt)
        {
            var check_ctdt = db.ctdt.FirstOrDefault(x => x.id_ctdt == ctdt.id_ctdt);
            if (string.IsNullOrEmpty(ctdt.ten_ctdt))
            {
                return Ok(new { message = "Không được bỏ trống tên ctđt", success = false });
            }
            check_ctdt.ma_ctdt = ctdt.ma_ctdt;
            check_ctdt.ten_ctdt = ctdt.ten_ctdt;
            check_ctdt.id_hdt = ctdt.id_hdt;
            check_ctdt.ngaytao = unixTimestamp;
            check_ctdt.ngaycapnhat = unixTimestamp;
            db.SaveChanges();
            return Ok(new { message = "Cập nhật dữ liệu thành công", success = true });
        }
        [HttpPost]
        [Route("api/admin/delete-ctdt")]
        public IHttpActionResult delete_ctdt(ctdt ctdt)
        {
            var check_lop = db.lop.Where(x => x.id_ctdt == ctdt.id_ctdt).ToList();
            if (check_lop.Any())
            {
                return Ok(new { message = "Chương trình đào tạo này đang tồn tại lớp học, vui lòng vào mục lớp học kiểm tra lại!", success = false });
            }
            var check_ctdt = db.ctdt.FirstOrDefault(x => x.id_ctdt == ctdt.id_ctdt);
            db.ctdt.Remove(check_ctdt);
            db.SaveChanges();
            return Ok(new { message = "Xóa dữ liệu thành công", success = true });
        }
        [HttpPost]
        [Route("api/admin/upload-excel-ctdt")]
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
                            var ma_nganh = worksheet.Cells[row, 5].Text;
                            var ten_nganh = worksheet.Cells[row, 6].Text;
                            var he_dao_tao = worksheet.Cells[row, 7].Text;
                            var ten_nam_hoc = worksheet.Cells[row, 8].Text;

                            var check_nam_hoc = await db.NamHoc.FirstOrDefaultAsync(x => x.ten_namhoc == ten_nam_hoc);
                            if (check_nam_hoc == null)
                            {
                                return Ok(new { message = $"Năm học {ten_nam_hoc} không tồn tại hoặc sai định dạng, vui lòng kiểm tra lại.", success = false });
                            }

                            var check_don_vi = await db.khoa_vien_truong.FirstOrDefaultAsync(x => x.ten_khoa.ToLower().Trim() == ten_don_vi.ToLower().Trim());
                            if (string.IsNullOrWhiteSpace(ten_don_vi))
                            {
                                return Ok(new { message = $"Có 1 ô nhập liệu Đơn vị đang bỏ trống, vui lòng kiếm tra lại", success = false });
                            }
                            else if (ten_don_vi == null)
                            {
                                return Ok(new { message = $"Đơn vị {ten_don_vi} không tồn tại hoặc sai định dạng, vui lòng kiểm tra lại.", success = false });
                            }
                            var check_khoa = await db.khoa_children.FirstOrDefaultAsync(x => x.ten_khoa_children.ToLower().Trim() == ten_khoa.ToLower().Trim());
                            if (string.IsNullOrWhiteSpace(ten_khoa))
                            {
                                return Ok(new { message = $"Có 1 ô nhập liệu khoa đang bỏ trống, vui lòng kiếm tra lại", success = false });
                            }
                            else if (check_khoa == null)
                            {
                                return Ok(new { message = $"Khoa {ten_khoa} không tồn tại hoặc sai định dạng, vui lòng kiểm tra lại.", success = false });
                            }
                            var check_bo_mon = await db.bo_mon.FirstOrDefaultAsync(x => x.ten_bo_mon.ToLower().Trim() == ten_bo_mon.ToLower().Trim());
                            if (!string.IsNullOrWhiteSpace(ten_bo_mon) && check_bo_mon == null)
                            {
                                return Ok(new { message = $"{ten_bo_mon} không tồn tại hoặc sai định dạng, vui lòng kiểm tra lại.", success = false });
                            }

                            var check_hdt = await db.hedaotao.FirstOrDefaultAsync(x => x.ten_hedaotao.ToLower().Trim() == he_dao_tao.ToLower().Trim());
                            if (check_hdt == null)
                            {
                                return Ok(new { message = $"Hệ đào tạo {he_dao_tao} không tồn tại hoặc sai định dạng, vui lòng kiểm tra lại.", success = false });
                            }

                            var check_ctdt = await db.ctdt
                                .FirstOrDefaultAsync(x =>
                                    x.ten_ctdt.ToLower().Trim() == ten_nganh.ToLower().Trim() &&
                                    x.ma_ctdt.ToLower().Trim() == ma_nganh.ToLower().Trim());

                            if (check_ctdt == null)
                            {
                                check_ctdt = new ctdt
                                {
                                    ma_ctdt = ma_nganh.ToUpper(),
                                    ten_ctdt = ten_nganh,
                                    id_khoa_vien_truong = check_don_vi.id_khoa,
                                    id_khoa_children = check_khoa.id_khoa_children,
                                    id_hdt = check_hdt.id_hedaotao,
                                    id_bo_mon = string.IsNullOrWhiteSpace(ten_bo_mon) ? (int?)null : check_bo_mon?.id_bo_mon,
                                    ngaycapnhat = unixTimestamp,
                                    ngaytao = unixTimestamp,
                                };
                                db.ctdt.Add(check_ctdt);
                            }
                            else
                            {
                                check_ctdt.id_khoa_vien_truong = check_don_vi.id_khoa;
                                check_ctdt.id_khoa_children = check_khoa.id_khoa_children;
                                check_ctdt.id_bo_mon = string.IsNullOrWhiteSpace(ten_bo_mon) ? (int?)null : check_bo_mon?.id_bo_mon;
                                check_ctdt.ngaycapnhat = unixTimestamp;
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
