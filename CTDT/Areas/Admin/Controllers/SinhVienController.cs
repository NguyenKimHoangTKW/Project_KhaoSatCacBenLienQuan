using CTDT.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Google.Cloud.Translation.V2;
using CTDT.Helper;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Globalization;
using GoogleApi.Entities.Common.Enums;
using System.Web.Helpers;
namespace CTDT.Areas.Admin.Controllers
{
    [UserAuthorize(2)]
    public class SinhVienController : Controller
    {
        dbSurveyEntities db = new dbSurveyEntities();

        // GET: Admin/SinhVien
        public ActionResult ViewSinhVien()
        {
            ViewBag.LopList = new SelectList(db.lop.Where(s => s.status == true).OrderBy(x => x.id_lop), "id_lop", "ma_lop");
            return View();
        }
        [HttpGet]
        public ActionResult LoadSinhVien(int pageNumber = 1, int pageSize = 10, int filterlop = 0, string keyword = "")
        {
            try
            {
                IQueryable<sinhvien> query = db.sinhvien;

                if (!string.IsNullOrEmpty(keyword))
                {
                    keyword = keyword.ToLower();
                    query = query.Where(l => l.ma_sv.ToLower().Contains(keyword)
                                          || l.hovaten.ToLower().Contains(keyword));
                }

                if (filterlop != 0)
                {
                    query = query.Where(l => l.id_lop == filterlop);
                }

                var totalRecords = query.Count();
                var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                var listsv = query
                    .OrderBy(sv => sv.id_sv)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .AsEnumerable()
                    .Select(sv => new
                    {
                        IDSV = sv.id_sv,
                        MSSV = sv.ma_sv,
                        HoTen = sv.hovaten,
                        NgaySinh = sv.ngaysinh?.ToString("dd-MM-yyyy") ?? "",
                        SDT = sv.sodienthoai,
                        DiaChi = sv.diachi,
                        GioiTinh = sv.phai,
                        NamTotNghiep = sv.namtotnghiep ?? "Không có dữ liệu",
                        NgayTao = sv.ngaytao,
                        Lop = sv.lop.ma_lop,
                        NgayCapNhat = sv.ngaycapnhat,
                    }).ToList();

                return Json(new { data = listsv, totalPages = totalPages, status = "Load dữ liệu thành công" }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { status = "Không tìm thấy sinh viên" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public async Task<ActionResult> UploadExcel(HttpPostedFileBase excelFile, HashSet<string> truong_duy_nhat)
        {
            if (excelFile != null && excelFile.ContentLength > 0)
            {
                try
                {
                    long unixTimestamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    using (var package = new ExcelPackage(excelFile.InputStream))
                    {
                        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                        if (worksheet == null)
                        {
                            return Json(new { status = "Không tìm thấy worksheet trong file Excel" }, JsonRequestBehavior.AllowGet);
                        }

                        for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                        {
                            var ms_nguoi_hoc = worksheet.Cells[row, 1].Text;
                            var ten_nguoi_hoc = worksheet.Cells[row, 2].Text;
                            var ngaysinh = worksheet.Cells[row, 3].Text;
                            var sdt = worksheet.Cells[row, 4].Text;
                            var dia_chi = worksheet.Cells[row, 5].Text;
                            var gioi_tinh = worksheet.Cells[row, 6].Text;
                            var ctdt = worksheet.Cells[row, 7].Text;
                            var lop = worksheet.Cells[row, 8].Text;
                            var nam_tot_nghiep = worksheet.Cells[row, 9].Text;
                            var nam_nhap_hoc = worksheet.Cells[row, 10].Text;
                            var kiem_tra_truong_duy_nhat = $"{ms_nguoi_hoc}-{ten_nguoi_hoc}-{lop}";

                            var get_lop = await db.lop.FirstOrDefaultAsync(x => x.ma_lop == lop);
                            var get_ctdt = await db.ctdt.FirstOrDefaultAsync(x => x.ten_ctdt.ToLower().Trim() == ctdt.ToLower().Trim());
                            var sinh_vien = await db.sinhvien.FirstOrDefaultAsync(x => x.ma_sv == ms_nguoi_hoc && x.hovaten.Trim() == ten_nguoi_hoc.Trim());

                            if (get_lop == null)
                            {
                                get_lop = new lop
                                {
                                    id_ctdt = get_ctdt.id_ctdt,
                                    ma_lop = lop,
                                    ngaycapnhat = (int)unixTimestamp,
                                    ngaytao = (int)unixTimestamp,
                                    status = true
                                };
                                db.lop.Add(get_lop);
                                await db.SaveChangesAsync();
                            }

                            DateTime? parsedNgaySinh = null;
                            if (!string.IsNullOrWhiteSpace(ngaysinh))
                            {
                                // Thử parse ngày sinh, format "dd/MM/yyyy"
                                if (DateTime.TryParseExact(ngaysinh, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime tempNgaySinh))
                                {
                                    parsedNgaySinh = tempNgaySinh;
                                }
                            }
                            if (sinh_vien == null)
                            {
                                sinh_vien = new sinhvien
                                {
                                    id_lop = get_lop.id_lop,
                                    ma_sv = ms_nguoi_hoc,
                                    hovaten = ten_nguoi_hoc,
                                    ngaysinh = parsedNgaySinh,
                                    sodienthoai = sdt,
                                    diachi = dia_chi,
                                    phai = gioi_tinh,
                                    namnhaphoc = string.IsNullOrWhiteSpace(nam_nhap_hoc) ? null : nam_nhap_hoc,
                                    namtotnghiep = string.IsNullOrWhiteSpace(nam_tot_nghiep) ? null : nam_tot_nghiep,
                                    ngaycapnhat = (int)unixTimestamp,
                                    ngaytao = (int)unixTimestamp,
                                    status = true
                                };
                                db.sinhvien.Add(sinh_vien);
                                await db.SaveChangesAsync();
                            }
                            else
                            {
                                sinh_vien.namnhaphoc = string.IsNullOrWhiteSpace(nam_nhap_hoc) ? null : nam_nhap_hoc;
                                sinh_vien.namtotnghiep = string.IsNullOrWhiteSpace(nam_tot_nghiep) ? null : nam_tot_nghiep;
                                sinh_vien.ngaycapnhat = (int)unixTimestamp;
                                await db.SaveChangesAsync();
                            }
                        }

                        return Json(new { status = "Import dữ liệu thành công" }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { status = $"Đã xảy ra lỗi: {ex.Message}" }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { status = "Vui lòng chọn file Excel" }, JsonRequestBehavior.AllowGet);
        }
    }
}
