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

                            var macbvc = worksheet.Cells[row, 1].Text;
                            var hoten = worksheet.Cells[row, 2].Text;
                            var ngaysinhText = worksheet.Cells[row, 3].Text;
                            var Email = worksheet.Cells[row, 4].Text;
                            var donvi = worksheet.Cells[row, 5].Text;
                            var chucvu = worksheet.Cells[row, 6].Text;
                            var chuongtrinhdaotao = worksheet.Cells[row, 7].Text;
                            var namhoatdong = worksheet.Cells[row, 8].Text;
                            var mota = worksheet.Cells[row, 9].Text;
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
                            var check_don_vi = await db.DonVi?.FirstOrDefaultAsync(x => x.name_donvi.ToLower().Trim() == donvi.ToLower().Trim());
                            var check_chuc_vu = await db.ChucVu?.FirstOrDefaultAsync(x => x.name_chucvu.ToLower().Trim() == chucvu.ToLower().Trim());
                            var check_ctdt = await db.ctdt?.FirstOrDefaultAsync(x => x.ten_ctdt.ToLower().Trim() == chucvu.ToLower().Trim());
                            var check_nam_hoat_dong = await db.NamHoc?.FirstOrDefaultAsync(x => x.ten_namhoc.ToLower().Trim() == namhoatdong.ToLower().Trim());
                            if (check_don_vi != null)
                            {
                                return Ok(new { message = $"Đơn vị {donvi} không tồn tại, vui lòng thêm đơn vị này vào hệ thống và upload lại!", success = false });
                            }
                            if (check_chuc_vu != null)
                            {
                                return Ok(new { message = $"Chức vụ {chucvu} không tồn tại, vui lòng thêm chức vụ này vào hệ thống và upload lại!", success = false });
                            }
                            if (check_ctdt != null)
                            {
                                return Ok(new { message = $"CTĐT {chuongtrinhdaotao} không tồn tại, vui lòng thêm CTĐT này vào hệ thống và upload lại!", success = false });
                            }
                            var check_cbvc = await db.CanBoVienChuc.FirstOrDefaultAsync(x => x.MaCBVC.ToLower().Trim() == macbvc.ToLower().Trim() && x.TenCBVC.ToLower().Trim() == hoten.ToLower().Trim());
                            if (check_cbvc == null)
                            {
                                check_cbvc = new CanBoVienChuc
                                {
                                    MaCBVC = macbvc ?? "",
                                    TenCBVC = hoten,
                                    NgaySinh = ngaysinh,
                                    Email = Email ?? "",
                                    id_donvi = donvi != "" ? check_don_vi.id_donvi : (int?)null,
                                    id_chucvu = chucvu != "" ? check_chuc_vu.id_chucvu : (int?)null,
                                    id_chuongtrinhdaotao = chuongtrinhdaotao != "" ? check_ctdt.id_ctdt : (int?)null,
                                    id_namhoc = check_nam_hoat_dong.id_namhoc,
                                    ngaycapnhat = unixTimestamp,
                                    ngaytao = unixTimestamp,
                                    description = mota ?? ""
                                };
                                db.CanBoVienChuc.Add(check_cbvc);
                            }
                            else
                            {
                                check_cbvc.MaCBVC = macbvc ?? "";
                                check_cbvc.TenCBVC = hoten;
                                check_cbvc.NgaySinh = ngaysinh;
                                check_cbvc.Email = Email ?? "";
                                check_cbvc.id_donvi = donvi != "" ? check_don_vi.id_donvi : (int?)null;
                                check_cbvc.id_chucvu = chucvu != "" ? check_chuc_vu.id_chucvu : (int?)null;
                                check_cbvc.id_chuongtrinhdaotao = chuongtrinhdaotao != "" ? check_ctdt.id_ctdt : (int?)null;
                                check_cbvc.id_namhoc = check_nam_hoat_dong.id_namhoc;
                                check_cbvc.ngaycapnhat = unixTimestamp;
                                check_cbvc.description = mota ?? "";
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
