using CTDT.Helper;
using CTDT.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CTDT.Areas.Admin.Controllers
{
    [UserAuthorize(2)]
    public class CBVCController : Controller
    {
        dbSurveyEntities db = new dbSurveyEntities();
        // GET: Admin/CBVC
        public ActionResult ViewCBVC()
        {
            ViewBag.DonviList = new SelectList(db.DonVi.OrderBy(x => x.id_donvi), "id_donvi", "name_donvi");
            ViewBag.ChucvuList = new SelectList(db.ChucVu.OrderBy(x => x.id_chucvu), "id_chucvu", "name_chucvu");
            ViewBag.CtdtList = new SelectList(db.ctdt.OrderBy(x => x.id_ctdt), "id_ctdt", "ten_ctdt");
            ViewBag.Year = new SelectList(db.NamHoc.OrderBy(x => x.id_namhoc), "id_namhoc", "ten_namhoc");
            return View();
        }
        [HttpGet]
        public ActionResult load_cbvc(int filterdonvi = 0, int filterchucvu = 0, int filterctdt = 0, int filtertrangthai = -1, int filternamhoatdong = 0)
        {
            var query = db.CanBoVienChuc.AsQueryable();
            if (filterdonvi != 0)
            {
                query = query.Where(x => x.id_donvi == filterdonvi);
            }

            if (filterchucvu != 0)
            {
                query = query.Where(x => x.id_chucvu == filterchucvu);
            }

            if (filterctdt != 0)
            {
                query = query.Where(x => x.id_chuongtrinhdaotao == filterctdt);
            }
            if (filternamhoatdong != 0)
            {
                query = query.Where(x => x.id_namhoc == filternamhoatdong);
            }
            if (filtertrangthai == 1)
            {
                query = query.Where(x => x.status == true);
            }
            else if (filtertrangthai == 0)
            {
                query = query.Where(x => x.status == false);
            }

            var GetCBVC = query
                .AsEnumerable()
                .Select(x => new
                {
                    id_cbvc = x.id_CBVC,
                    ten_cbvc = x.TenCBVC ?? "Không có dữ liệu",
                    ma_cbvc = x.MaCBVC ?? "Không có dữ liệu",
                    ngay_sinh = x.NgaySinh.HasValue ? x.NgaySinh.Value.ToString("dd-MM-yyyy") : "",
                    email = x.Email ?? "Không có dữ liệu",
                    don_vi = x.DonVi?.name_donvi ?? "Không có dữ liệu",
                    chuong_trinh_dao_tao = x.ctdt?.ten_ctdt ?? "Không có dữ liệu",
                    chuc_vu = x.ChucVu?.name_chucvu ?? "Không có dữ liệu",
                    nam_hoat_dong = x.NamHoc.ten_namhoc,
                    trang_thai = x.status,
                })
                .ToList();
            return Json(new { data = GetCBVC }, JsonRequestBehavior.AllowGet);
        }

        public int MapTenDonViToIDDonVi(string tendonvi)
        {
            var donvi = db.DonVi.Where(k => k.name_donvi == tendonvi).FirstOrDefault();
            return donvi?.id_donvi ?? 0;
        }
        public int MapTenChucVuToIDChucVu(string tenchucvu)
        {
            var chucvu = db.ChucVu.Where(cv => cv.name_chucvu == tenchucvu).FirstOrDefault();
            return chucvu?.id_chucvu ?? 0;
        }
        public int MapChuongTrinhToIDChuongTrinh(string tenchuongtrinh)
        {
            var chuongtrinh = db.ctdt.Where(cv => cv.ten_ctdt == tenchuongtrinh).FirstOrDefault();
            return chuongtrinh?.id_ctdt ?? 0;
        }
        public int MapNamhocToTenNamHoc(string tennamhoc)
        {
            var namhoc = db.NamHoc.Where(cv => cv.ten_namhoc == tennamhoc).FirstOrDefault();
            return namhoc?.id_namhoc ?? 0;
        }
        [HttpPost]
        public ActionResult UploadExcel(HttpPostedFileBase excelFile)
        {
            if (excelFile != null && excelFile.ContentLength > 0)
            {
                try
                {
                    DateTime now = DateTime.UtcNow;
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
                            var ngaySinhText = worksheet.Cells[row, 4].Text;
                            var donViText = worksheet.Cells[row, 6].Text;
                            var chucVuText = worksheet.Cells[row, 7].Text;
                            var chuongTrinhText = worksheet.Cells[row, 8].Text;
                            var namhocText = worksheet.Cells[row, 9].Text;
                            var madonvi = !string.IsNullOrEmpty(donViText) ? MapTenDonViToIDDonVi(donViText) : (int?)null;
                            var machucvu = !string.IsNullOrEmpty(chucVuText) ? MapTenChucVuToIDChucVu(chucVuText) : (int?)null;
                            var mactdt = !string.IsNullOrEmpty(chuongTrinhText) ? MapChuongTrinhToIDChuongTrinh(chuongTrinhText) : (int?)null;
                            var manamhoc = !string.IsNullOrEmpty(namhocText) ? MapNamhocToTenNamHoc(namhocText) : (int?)null;
                            DateTime? ngaySinh = null;
                            if (DateTime.TryParseExact(ngaySinhText, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedNgaySinh))
                            {
                                ngaySinh = parsedNgaySinh;
                            }

                            var cbvc = new CanBoVienChuc
                            {
                                MaCBVC = string.IsNullOrEmpty(worksheet.Cells[row, 2].Text) ? null : worksheet.Cells[row, 2].Text,
                                TenCBVC = worksheet.Cells[row, 3].Text,
                                NgaySinh = ngaySinh,
                                Email = string.IsNullOrEmpty(worksheet.Cells[row, 5].Text) ? null : worksheet.Cells[row, 5].Text,
                                status = false,
                                id_namhoc = manamhoc,
                                id_chucvu = machucvu,
                                id_donvi = madonvi,
                                id_chuongtrinhdaotao = mactdt,
                            };

                            db.CanBoVienChuc.Add(cbvc);
                        }

                        db.SaveChanges();

                        return Json(new { status = "Thêm cán bộ viên chức thành công" }, JsonRequestBehavior.AllowGet);
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