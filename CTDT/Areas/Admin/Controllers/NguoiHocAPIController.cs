using CTDT.Models;
using Microsoft.AspNet.SignalR.Hubs;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
namespace CTDT.Areas.Admin.Controllers
{
    public class NguoiHocAPIController : ApiController
    {
        dbSurveyEntities db = new dbSurveyEntities();
        private int unixTimestamp;
        public NguoiHocAPIController()
        {
            DateTime now = DateTime.UtcNow;
            unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }
        [HttpPost]
        [Route("api/admin/danh-sach-nguoi-hoc")]
        public async Task<IHttpActionResult> LoadDanhSachNguoiHoc(Data_SV sv)
        {
            var query = db.sinhvien.AsQueryable();

            if (sv.id_lop != 0)
            {
                query = query.Where(x => x.id_lop == sv.id_lop);
            }

            if (!string.IsNullOrEmpty(sv.searchTerm))
            {
                string keyword = sv.searchTerm.ToLower();
                query = query.Where(x =>
                    x.hovaten.ToLower().Contains(keyword) ||
                    x.ma_sv.ToLower().Contains(keyword) ||
                    x.lop.ma_lop.ToLower().Contains(keyword) ||
                    x.diachi.ToLower().Contains(keyword) ||
                    x.phai.ToLower().Contains(keyword) ||
                    x.sodienthoai.Contains(keyword));
            }

            int totalRecords = await query.CountAsync();

            var pagedData = await query
                .OrderBy(x => x.id_sv)
                .Skip((sv.page - 1) * sv.pageSize)
                .Take(sv.pageSize)
                .ToListAsync();

            var getData = pagedData.Select(x => new
            {
                x.id_sv,
                x.lop.ma_lop,
                x.ma_sv,
                x.hovaten,
                ngaysinh = x.ngaysinh?.ToString("dd-MM-yyyy") ?? " ",
                sodienthoai = x.sodienthoai ?? " ",
                diachi = x.diachi ?? " ",
                phai = x.phai ?? " ",
                namnhaphoc = x.namnhaphoc ?? " ",
                namtotnghiep = x.namtotnghiep ?? " ",
                x.ngaycapnhat,
                x.ngaytao,
                description = x.description ?? " "
            }).ToList();

            return Ok(new
            {
                data = getData,
                success = true,
                totalRecords = totalRecords,
                totalPages = (int)Math.Ceiling((double)totalRecords / sv.pageSize),
                currentPage = sv.page
            });
        }

        public class Data_SV
        {
            public int id_lop { get; set; }
            public int page { get; set; }
            public int pageSize { get; set; }
            public string searchTerm { get; set; }
        }

        [HttpPost]
        [Route("api/admin/them-moi-nguoi-hoc")]
        public IHttpActionResult add_new(sinhvien sv)
        {
            if (string.IsNullOrEmpty(sv.ma_sv))
            {
                return Ok(new { message = "Không được bỏ trống mã người học", success = false });
            }
            if (string.IsNullOrEmpty(sv.hovaten))
            {
                return Ok(new { message = "Không được bỏ trống tên người học", success = false });
            }
            if (db.sinhvien.FirstOrDefault(x => x.id_sv == sv.id_sv) != null)
            {
                return Ok(new { message = "Mã người học này đã tồn tại", success = false });
            }
            var add_new = new sinhvien
            {
                ma_sv = sv.ma_sv,
                hovaten = sv.hovaten,
                id_lop = sv.id_lop,
                ngaysinh = sv.ngaysinh,
                sodienthoai = sv.sodienthoai,
                diachi = sv.diachi,
                phai = sv.phai,
                namnhaphoc = sv.namnhaphoc,
                namtotnghiep = sv.namtotnghiep,
                description = sv.description,
                ngaytao = unixTimestamp,
                ngaycapnhat = unixTimestamp
            };
            db.sinhvien.Add(add_new);
            db.SaveChanges();
            return Ok(new { message = "Cập nhật dữ liệu thành công", success = true });
        }
        [HttpPost]
        [Route("api/admin/get-info-nguoi-hoc")]
        public async Task<IHttpActionResult> get_info(sinhvien sv)
        {
            var get_info = await db.sinhvien
                .Where(x => x.id_sv == sv.id_sv)
                .Select(x => new
                {
                    x.id_lop,
                    x.ma_sv,
                    x.hovaten,
                    x.ngaysinh,
                    x.sodienthoai,
                    x.diachi,
                    x.phai,
                    x.namnhaphoc,
                    x.namtotnghiep,
                    x.description,
                }).SingleOrDefaultAsync();
            return Ok(get_info);
        }
        [HttpPost]
        [Route("api/admin/update-nguoi-hoc")]
        public IHttpActionResult update_nguoi_hoc(sinhvien sv)
        {
            var get_data = db.sinhvien.SingleOrDefault(x => x.id_sv == sv.id_sv);
            if (string.IsNullOrEmpty(sv.ma_sv))
            {
                return Ok(new { message = "Không được bỏ trống mã người học", success = false });
            }
            if (string.IsNullOrEmpty(sv.hovaten))
            {
                return Ok(new { message = "Không được bỏ trống tên người học", success = false });
            }
            get_data.id_lop = sv.id_lop;
            get_data.ma_sv = sv.ma_sv;
            get_data.hovaten = sv.hovaten;
            get_data.ngaysinh = sv.ngaysinh;
            get_data.sodienthoai = sv.sodienthoai;
            get_data.diachi = sv.diachi;
            get_data.phai = sv.phai;
            get_data.namnhaphoc = sv.namnhaphoc;
            get_data.namtotnghiep = sv.namtotnghiep;
            get_data.description = sv.description;
            get_data.ngaycapnhat = unixTimestamp;
            db.SaveChanges();
            return Ok(new { message = "Cập nhật dữ liệu thành công", success = true });
        }
        [HttpPost]
        [Route("api/admin/delete-nguoi-hoc")]
        public IHttpActionResult delete_nguoi_hoc(sinhvien sv)
        {
            var check_danh_sach_khao_sat = db.nguoi_hoc_khao_sat.Where(x => x.id_sv == sv.id_sv).ToList();
            var check_danh_sach_nguoi_hoc = db.nguoi_hoc_dang_co_hoc_phan.Where(x => x.id_sinh_vien == sv.id_sv).ToList();
            var check_answer = db.answer_response.Where(x => x.id_sv == sv.id_sv).ToList();
            if (check_danh_sach_khao_sat.Any())
            {
                db.nguoi_hoc_khao_sat.RemoveRange(check_danh_sach_khao_sat);
                db.SaveChanges();
            }
            if (check_danh_sach_nguoi_hoc.Any())
            {
                db.nguoi_hoc_dang_co_hoc_phan.RemoveRange(check_danh_sach_nguoi_hoc);
                db.SaveChanges();
            }
            if (check_answer.Any())
            {
                db.answer_response.RemoveRange(check_answer);
                db.SaveChanges();
            }
            var check_nguoi_hoc = db.sinhvien.SingleOrDefault(x => x.id_sv == sv.id_sv);
            if (check_nguoi_hoc != null)
            {
                db.sinhvien.Remove(check_nguoi_hoc);
                db.SaveChanges();
            }
            return Ok(new { message = "Xóa dữ liệu thành công" });
        }
        [HttpPost]
        [Route("api/admin/upload-excel-nguoi-hoc")]
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

                            var masv = worksheet.Cells[row, 1].Text;
                            var hoten = worksheet.Cells[row, 2].Text;
                            var tenlop = worksheet.Cells[row, 3].Text;
                            var ngaysinhText = worksheet.Cells[row, 4].Text;
                            var sodienthoai = worksheet.Cells[row, 5].Text;
                            var diachi = worksheet.Cells[row, 6].Text;
                            var gioitinh = worksheet.Cells[row, 7].Text;
                            var namnhaphoc = worksheet.Cells[row, 8].Text;
                            var namtotnghiep = worksheet.Cells[row, 9].Text;
                            var mota = worksheet.Cells[row, 10].Text;
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
                            var checkLop = await db.lop.FirstOrDefaultAsync(x => x.ma_lop.ToLower().Trim() == tenlop.ToLower().Trim());
                            if (checkLop == null)
                            {
                                return Ok(new { message = $"Lớp {tenlop} không tồn tại, vui lòng thêm lớp này vào hệ thống và upload lại!", success = false });
                            }

                            var checkNguoiHoc = await db.sinhvien.FirstOrDefaultAsync(x => x.ma_sv == masv);
                            if (checkNguoiHoc == null)
                            {
                                checkNguoiHoc = new sinhvien
                                {
                                    id_lop = checkLop.id_lop,
                                    ma_sv = masv,
                                    hovaten = hoten,
                                    ngaysinh = ngaysinh,
                                    sodienthoai = sodienthoai ?? "",
                                    diachi = diachi ?? "",
                                    phai = gioitinh ?? "",
                                    namnhaphoc = namnhaphoc ?? "",
                                    namtotnghiep = namtotnghiep ?? "",
                                    ngaycapnhat = unixTimestamp,
                                    ngaytao = unixTimestamp,
                                    description = mota ?? ""
                                };
                                db.sinhvien.Add(checkNguoiHoc);
                            }
                            else
                            {
                                checkNguoiHoc.hovaten = hoten;
                                checkNguoiHoc.ngaysinh = ngaysinh;
                                checkNguoiHoc.sodienthoai = sodienthoai ?? "";
                                checkNguoiHoc.diachi = diachi ?? "";
                                checkNguoiHoc.phai = gioitinh ?? "";
                                checkNguoiHoc.namnhaphoc = namnhaphoc ?? "";
                                checkNguoiHoc.namtotnghiep = namtotnghiep ?? "";
                                checkNguoiHoc.ngaycapnhat = unixTimestamp;
                                checkNguoiHoc.description = mota ?? "";
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
