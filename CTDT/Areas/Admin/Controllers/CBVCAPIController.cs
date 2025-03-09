using CTDT.Models;
using Microsoft.AspNet.SignalR.Hubs;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Web.Http;

namespace CTDT.Areas.Admin.Controllers
{
    public class CBVCAPIController : ApiController
    {
        dbSurveyEntities db = new dbSurveyEntities();
        private int unixTimestamp;
        public CBVCAPIController()
        {
            DateTime now = DateTime.UtcNow;
            unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }
        [HttpPost]
        [Route("api/admin/danh-sach-cbvc")]
        public async Task<IHttpActionResult> danh_sach_cbvc(Data_CBVC cbvc)
        {
            var query = db.CanBoVienChuc.AsQueryable();
            if (cbvc.id_chuongtrinhdaotao != 0)
            {
                query = query.Where(x => x.id_chuongtrinhdaotao == cbvc.id_chuongtrinhdaotao);
            }
            if (cbvc.id_namhoc != 0)
            {
                query = query.Where(x => x.id_namhoc == cbvc.id_namhoc);
            }
            if (cbvc.id_donvi != 0)
            {
                query = query.Where(x => x.id_donvi == cbvc.id_donvi);
            }
            if (cbvc.id_chucvu != 0)
            {
                query = query.Where(x => x.id_chucvu == cbvc.id_chucvu);
            }
            if (!string.IsNullOrEmpty(cbvc.searchTerm))
            {
                string keyword = cbvc.searchTerm.ToLower();
                query = query.Where(x =>
                x.MaCBVC.ToLower().Contains(keyword) ||
                x.TenCBVC.ToLower().Contains(keyword) ||
                x.Email.ToLower().Contains(keyword) ||
                x.DonVi.name_donvi.ToLower().Contains(keyword) ||
                x.ChucVu.name_chucvu.ToLower().Contains(keyword) ||
                x.ctdt.ten_ctdt.ToLower().Contains(keyword) ||
                x.NamHoc.ten_namhoc.ToLower().Contains(keyword) ||
                x.description.ToLower().Contains(keyword));
            }
            int totalRecords = await query.CountAsync();
            var pagedData = await query
               .OrderBy(x => x.id_CBVC)
               .Skip((cbvc.page - 1) * cbvc.pageSize)
               .Take(cbvc.pageSize)
               .ToListAsync();
            var get_data = pagedData
                .Select(x => new
                {
                    x.id_CBVC,
                    MaCBVC = x.MaCBVC != null ? x.MaCBVC : " ",
                    x.TenCBVC,
                    NgaySinh = x.NgaySinh.HasValue ? x.NgaySinh.Value.ToString("dd-MM-yyyy") : " ",
                    Email = x.Email != null ? x.Email : " ",
                    donvi = x.id_donvi != null ? x.DonVi.name_donvi : " ",
                    chucvu = x.id_chucvu != null ? x.ChucVu.name_chucvu : " ",
                    ctdt = x.id_chuongtrinhdaotao != null ? x.ctdt.ten_ctdt : " ",
                    NamHoc = x.id_namhoc != null ? x.NamHoc.ten_namhoc : " ",
                    x.ngaycapnhat,
                    x.ngaytao,
                    descripton = x.description != null ? x.description : " "
                }).ToList();
            if (get_data.Count > 0)
            {
                return Ok(new
                {
                    data = get_data,
                    success = true,
                    totalRecords = totalRecords,
                    totalPages = (int)Math.Ceiling((double)totalRecords / cbvc.pageSize),
                    currentPage = cbvc.page
                });
            }
            else
            {
                return Ok(new { message = "Không tồn tại dữ liệu", success = false });
            }
        }

        public class Data_CBVC
        {
            public int id_chuongtrinhdaotao { get; set; }
            public int id_namhoc { get; set; }
            public int id_donvi { get; set; }
            public int id_chucvu { get; set; }
            public int page { get; set; }
            public int pageSize { get; set; }
            public string searchTerm { get; set; }
        }
        [HttpPost]
        [Route("api/admin/them-moi-cbvc")]
        public IHttpActionResult add_new(CanBoVienChuc cbvc)
        {
            if (string.IsNullOrEmpty(cbvc.TenCBVC))
            {
                return Ok(new { message = "Không được bỏ trống tên cán bộ viên chức/giảng viên", success = false });
            }
            if (db.CanBoVienChuc.FirstOrDefault(x => x.MaCBVC == cbvc.MaCBVC && x.TenCBVC == cbvc.TenCBVC) != null)
            {
                return Ok(new { message = "Cán bộ viên chức/giảng viên này đã tồn tại", success = false });
            }
            var add_new = new CanBoVienChuc
            {
                MaCBVC = cbvc.MaCBVC,
                TenCBVC = cbvc.TenCBVC,
                NgaySinh = cbvc.NgaySinh,
                Email = cbvc.Email,
                id_donvi = cbvc.id_donvi,
                id_chucvu = cbvc.id_chucvu,
                id_chuongtrinhdaotao = cbvc.id_chuongtrinhdaotao,
                id_namhoc = cbvc.id_namhoc,
                ngaycapnhat = unixTimestamp,
                ngaytao = unixTimestamp,
                description = cbvc.description,
            };
            db.CanBoVienChuc.Add(add_new);
            db.SaveChanges();
            return Ok(new { message = "Cập nhật dữ liệu thành công", success = true });
        }
        [HttpPost]
        [Route("api/admin/get-info-cbvc")]
        public async Task<IHttpActionResult> get_info_cbvc(CanBoVienChuc cbvc)
        {
            var get_info = await db.CanBoVienChuc
                .Where(x => x.id_CBVC == cbvc.id_CBVC)
                .Select(x => new
                {
                    x.MaCBVC,
                    x.TenCBVC,
                    x.NgaySinh,
                    x.Email,
                    x.id_donvi,
                    x.id_chucvu,
                    x.id_chuongtrinhdaotao,
                    x.id_namhoc,
                    x.description
                }).FirstOrDefaultAsync();
            return Ok(get_info);
        }
        [HttpPost]
        [Route("api/admin/update-cbvc")]
        public async Task<IHttpActionResult> update_cbvc(CanBoVienChuc cbvc)
        {
            var update_cbvc = await db.CanBoVienChuc.SingleOrDefaultAsync(x => x.id_CBVC == cbvc.id_CBVC);
            if (string.IsNullOrEmpty(cbvc.TenCBVC))
            {
                return Ok(new { message = "Không được bỏ trống tên cán bộ viên chức/giảng viên", success = false });
            }
            update_cbvc.MaCBVC = cbvc.MaCBVC;
            update_cbvc.TenCBVC = cbvc.TenCBVC;
            update_cbvc.NgaySinh = cbvc.NgaySinh;
            update_cbvc.Email = cbvc.Email;
            update_cbvc.id_donvi = cbvc.id_donvi;
            update_cbvc.id_chucvu = cbvc.id_chucvu;
            update_cbvc.id_chuongtrinhdaotao = cbvc.id_chuongtrinhdaotao;
            update_cbvc.id_namhoc = cbvc.id_namhoc;
            update_cbvc.ngaycapnhat = unixTimestamp;
            update_cbvc.description = cbvc.description;
            await db.SaveChangesAsync();
            return Ok(new { message = "Cập nhật dữ liệu thành công", success = true });
        }
        [HttpPost]
        [Route("api/admin/delete-cbvc")]
        public IHttpActionResult delete_cbvc(CanBoVienChuc cbvc)
        {
            var check_cbvc = db.CanBoVienChuc.FirstOrDefault(x => x.id_CBVC == cbvc.id_CBVC);
            db.CanBoVienChuc.Remove(check_cbvc);
            db.SaveChanges();
            return Ok(new { message = "Xóa dữ liệu thành công", success = true });
        }

        [HttpPost]
        [Route("api/admin/upload-excel-cbvc")]
        public async Task<IHttpActionResult> UploadExcelNguoiHoc()
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
                            DateTime? ngaysinh = null;

                            var macbvc = worksheet.Cells[row, 2].Text;
                            var hoten = worksheet.Cells[row, 3].Text;
                            var Email = worksheet.Cells[row, 4].Text;
                            var ngaysinhText = worksheet.Cells[row, 5].Text;
                            var trinh_do = worksheet.Cells[row, 6].Text;
                            var chuc_vu = worksheet.Cells[row, 7].Text;
                            var don_vi = worksheet.Cells[row, 8].Text;
                            var bo_mon = worksheet.Cells[row, 9].Text;
                            var nganh_dao_tao = worksheet.Cells[row, 10].Text;
                            var namhoatdong = worksheet.Cells[row, 11].Text;
                            if (!string.IsNullOrWhiteSpace(ngaysinhText))
                            {
                                if (DateTime.TryParseExact(ngaysinhText, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                                {
                                    ngaysinh = parsedDate;
                                }
                                else
                                {
                                    return Ok(new { message = $"Ngày sinh không đúng định dạng ở dòng {row}. Vui lòng kiểm tra và thử lại.", success = false });
                                }
                            }

                            if (string.IsNullOrWhiteSpace(namhoatdong))
                            {
                                return Ok(new { message = $"Ô năm hoạt động đang có 1 ô bị bỏ trống, vui lòng kiểm tra lại", success = false });
                            }
                            var check_nam_hoat_dong = await db.NamHoc?.FirstOrDefaultAsync(x => x.ten_namhoc.ToLower().Trim() == namhoatdong.ToLower().Trim());

                            if (check_nam_hoat_dong == null)
                            {
                                return Ok(new { message = $"Năm {namhoatdong} không tồn tại hoạt sai định dạng, vui lòng kiểm tra lại", success = false });
                            }

                            if (string.IsNullOrWhiteSpace(trinh_do))
                            {
                                return Ok(new { message = $"Ô Trình độ đang có 1 ô bị bỏ trống, vui lòng kiểm tra lại", success = false });
                            }
                            var check_trinh_do = await db.trinh_do.FirstOrDefaultAsync(x => x.ten_trinh_do.ToLower().Trim() == trinh_do.ToLower().Trim());
                            if (check_trinh_do == null)
                            {
                                return Ok(new { message = $"Trình độ {trinh_do} không tồn tại hoạt sai định dạng, vui lòng kiểm tra lại", success = false });
                            }

                            var check_chuc_vu = await db.ChucVu.FirstOrDefaultAsync(x => x.name_chucvu.ToLower().Trim() == chuc_vu.ToLower().Trim());
                            if (!string.IsNullOrWhiteSpace(chuc_vu) && check_chuc_vu == null)
                            {
                                return Ok(new { message = $"Chức vụ {chuc_vu} không tồn tại hoạt sai định dạng, vui lòng kiểm tra lại", success = false });
                            }
                            if (string.IsNullOrWhiteSpace(don_vi))
                            {
                                return Ok(new { message = $"Ô Đơn vị đang có 1 ô bị bỏ trống, vui lòng kiểm tra lại", success = false });
                            }

                            var check_don_vi = await db.khoa_vien_truong
                                .FirstOrDefaultAsync(x => x.ten_khoa.ToLower().Trim() == don_vi.ToLower().Trim()
                                                        && x.id_namhoc == check_nam_hoat_dong.id_namhoc);

                            if (check_don_vi == null)
                            {
                                return Ok(new { message = $"Đơn vị {don_vi} không tồn tại hoặc sai định dạng, vui lòng kiểm tra lại", success = false });
                            }

                            var check_bo_mon = await db.bo_mon.FirstOrDefaultAsync(x => x.ten_bo_mon.ToLower().Trim() == bo_mon.ToLower().Trim() && x.id_nam_hoc == check_nam_hoat_dong.id_namhoc);
                            if (!string.IsNullOrWhiteSpace(bo_mon) && check_bo_mon == null)
                            {
                                return Ok(new { message = $"{bo_mon} không tồn tại hoạt sai định dạng, vui lòng kiểm tra lại", success = false });
                            }
                            var check_cbvc = await db.CanBoVienChuc
                                .FirstOrDefaultAsync(x =>
                                x.MaCBVC.ToLower().Trim() == macbvc.ToLower().Trim() &&
                                x.TenCBVC.ToLower().Trim() == hoten.ToLower().Trim() &&
                                x.id_namhoc == check_nam_hoat_dong.id_namhoc);
                            if (check_cbvc == null)
                            {
                                check_cbvc = new CanBoVienChuc
                                {
                                    MaCBVC = macbvc ?? "",
                                    TenCBVC = hoten,
                                    NgaySinh = ngaysinh,
                                    Email = Email ?? "",
                                    id_trinh_do = check_trinh_do.id_trinh_do,
                                    id_chucvu = string.IsNullOrWhiteSpace(chuc_vu) ? (int?)null : check_chuc_vu.id_chucvu,
                                    id_don_vi = check_don_vi.id_khoa,
                                    id_bo_mon = string.IsNullOrWhiteSpace(bo_mon) ? (int?)null : check_bo_mon.id_bo_mon,
                                    nganh_dao_tao = nganh_dao_tao,
                                    id_namhoc = check_nam_hoat_dong.id_namhoc,
                                    ngaycapnhat = unixTimestamp,
                                    ngaytao = unixTimestamp,
                                };
                                db.CanBoVienChuc.Add(check_cbvc);
                            }
                            else
                            {
                                check_cbvc.MaCBVC = macbvc ?? "";
                                check_cbvc.NgaySinh = ngaysinh;
                                check_cbvc.Email = Email ?? "";
                                check_cbvc.id_trinh_do = check_trinh_do.id_trinh_do;
                                check_cbvc.id_chucvu = string.IsNullOrWhiteSpace(chuc_vu) ? (int?)null : check_chuc_vu.id_chucvu;
                                check_cbvc.id_don_vi = check_don_vi.id_khoa;
                                check_cbvc.id_bo_mon = string.IsNullOrWhiteSpace(bo_mon) ? (int?)null : check_bo_mon.id_bo_mon;
                                check_cbvc.id_namhoc = check_nam_hoat_dong.id_namhoc;
                                check_cbvc.ngaycapnhat = unixTimestamp;
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
