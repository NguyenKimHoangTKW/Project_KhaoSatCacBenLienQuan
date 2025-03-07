using CTDT.Helper;
using CTDT.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace CTDT.Areas.Admin.Controllers
{
    [UserAuthorize(2)]
    public class ImportDataController : Controller
    {
        dbSurveyEntities db = new dbSurveyEntities();
        // GET: Admin/ImportData
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult UploadExcel(HttpPostedFileBase excelFile, int? CheckDuLieu)
        {
            if (excelFile == null || excelFile.ContentLength <= 0)
                return Json(new { status = "Vui lòng chọn file Excel" }, JsonRequestBehavior.AllowGet);

            if (CheckDuLieu == null)
            {
                return Json(new { status = "Bạn chưa chọn định dạng dữ liệu" }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                DateTime now = DateTime.UtcNow;
                int unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage(excelFile.InputStream))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                        return Json(new { status = "Không tìm thấy worksheet trong file Excel" }, JsonRequestBehavior.AllowGet);

                    var uniqueRecords = new HashSet<string>();
                    int skippedRecords = 0;
                    int processedRecords = 0;

                    for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                    {
                        bool result = false;

                        if (CheckDuLieu == 1)
                        {
                            result = import_sinh_vien_ngoai_pham_vi_phieu_8(worksheet, row, unixTimestamp, uniqueRecords);
                        }
                        else if (CheckDuLieu == 2)
                        {
                            result = import_sinh_vien_trong_pham_vi_phieu_8(worksheet, row, unixTimestamp, uniqueRecords);
                        }
                        else if(CheckDuLieu == 3)
                        {
                            result = import_sinh_vien_ngoai_edu(worksheet, row, unixTimestamp, uniqueRecords);
                        }
                        if (!result)
                        {
                            skippedRecords++; 
                        }
                        else
                        {
                            processedRecords++; 
                        }
                    }

                    return Json(new
                    {
                        status = "Dữ liệu đã được xử lý thành công",
                        processed = processedRecords,
                        skipped = skippedRecords
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = $"Đã xảy ra lỗi: {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }


        private bool import_sinh_vien_trong_pham_vi_phieu_8(ExcelWorksheet worksheet, int row, int unixTimestamp, HashSet<string> uniqueRecords)
        {
            var mamh = worksheet.Cells[row, 1].Text.Trim();
            var hocphan = worksheet.Cells[row, 2].Text.Trim();
            var monhoc = worksheet.Cells[row, 3].Text.Trim();
            var nhommonhoc = worksheet.Cells[row, 4].Text.Trim();
            var masinhvien = worksheet.Cells[row, 5].Text.Trim();
            var tensv = worksheet.Cells[row, 6].Text.Trim() + " " + worksheet.Cells[row, 7].Text.Trim();
            var malop = worksheet.Cells[row, 8].Text.Trim();
            var hedaotao = worksheet.Cells[row, 9].Text.Trim();
            var manganh = worksheet.Cells[row, 10].Text.Trim();
            var tennganh = worksheet.Cells[row, 11].Text.Trim();
            var makhoa = worksheet.Cells[row, 12].Text.Trim();
            var tenkhoa = worksheet.Cells[row, 13].Text.Trim();
            var recordIdentifier = $"{mamh}-{monhoc}-{nhommonhoc}-{masinhvien}-{tensv}-{malop}-{hedaotao}-{tennganh}-{makhoa}-{tenkhoa}";
            if (uniqueRecords.Contains(recordIdentifier))
                return false;
            uniqueRecords.Add(recordIdentifier);
            var existingKhoa = db.khoa_vien_truong.SingleOrDefault(x => x.ten_khoa == tenkhoa);
            if (existingKhoa == null)
            {
                existingKhoa = new khoa_vien_truong
                {
                    ma_khoa = makhoa,
                    ten_khoa = tenkhoa,
                    ngaycapnhat = unixTimestamp,
                    ngaytao = unixTimestamp,
                };
                db.khoa_vien_truong.Add(existingKhoa);
                db.SaveChanges();
            }

            var existingHedaotao = db.hedaotao.SingleOrDefault(x => x.ten_hedaotao == hedaotao);
            var existingCTDT = db.ctdt.SingleOrDefault(x => x.ten_ctdt.Trim() == tennganh );
            if (existingCTDT == null)
            {
                existingCTDT = new ctdt
                {
                    ma_ctdt = manganh,
                    ten_ctdt = tennganh,
                    id_hdt = existingHedaotao?.id_hedaotao,
                    ngaycapnhat = unixTimestamp,
                    ngaytao = unixTimestamp
                };
                db.ctdt.Add(existingCTDT);
                db.SaveChanges();
            }
            var existingLop = db.lop.SingleOrDefault(x => x.ma_lop == malop);
            if (existingLop == null)
            {
                existingLop = new lop
                {
                    id_ctdt = existingCTDT.id_ctdt,
                    ma_lop = malop,
                    ngaycapnhat = unixTimestamp,
                    ngaytao = unixTimestamp,
                    status = true
                };
                db.lop.Add(existingLop);
                db.SaveChanges();
            }
            var existingSV = db.sinhvien.FirstOrDefault(x => x.ma_sv == masinhvien && x.hovaten == tensv);
            if (existingSV == null)
            {
                existingSV = new sinhvien
                {
                    id_lop = existingLop.id_lop,
                    ma_sv = masinhvien,
                    hovaten = tensv,
                    ngaycapnhat = unixTimestamp,
                    ngaytao = unixTimestamp,
                };
                db.sinhvien.Add(existingSV);
                db.SaveChanges();
            }
           
            var existingMonHoc = db.mon_hoc.SingleOrDefault(x => x.ten_mon_hoc == monhoc);
            var existingHocPhan = db.hoc_phan.SingleOrDefault(x => x.ten_hoc_phan == hocphan);
            if (existingMonHoc == null)
            {
                existingMonHoc = new mon_hoc
                {
                    ten_mon_hoc = monhoc,
                    ma_mon_hoc = mamh,
                    id_hoc_phan = existingHocPhan.id_hoc_phan,
                    ngay_cap_nhat = unixTimestamp,
                    ngay_tao = unixTimestamp,
                };
                db.mon_hoc.Add(existingMonHoc);
                db.SaveChanges();
            }

            return true;
        }
        private bool import_sinh_vien_ngoai_pham_vi_phieu_8(ExcelWorksheet worksheet, int row, int unixTimestamp, HashSet<string> uniqueRecords)
        {
            var tenKhoa = worksheet.Cells[row, 12].Text.Trim();
            var tenctdt = worksheet.Cells[row, 10].Text.Trim();
            var malop = worksheet.Cells[row, 8].Text.Trim();
            var tensv = worksheet.Cells[row, 6].Text.Trim() + " " + worksheet.Cells[row, 7].Text.Trim();
            var masv = worksheet.Cells[row, 5].Text.Trim();

            var recordIdentifier = $"{tenKhoa}-{tenctdt}-{malop}-{tensv}-{masv}";

            if (uniqueRecords.Contains(recordIdentifier))
                return false;

            uniqueRecords.Add(recordIdentifier);
            var existingKhoa = db.khoa_vien_truong.SingleOrDefault(x => x.ten_khoa == tenKhoa);
            if (existingKhoa == null)
            {
                existingKhoa = new khoa_vien_truong
                {
                    ma_khoa = worksheet.Cells[row, 11].Text.Trim(),
                    ten_khoa = tenKhoa,
                    ngaycapnhat = unixTimestamp,
                    ngaytao = unixTimestamp,
                };
                db.khoa_vien_truong.Add(existingKhoa);
                db.SaveChanges();
            }
            var existingCTDT = db.ctdt.SingleOrDefault(x => x.ten_ctdt == tenctdt);
            if (existingCTDT == null)
            {
                existingCTDT = new ctdt
                {
                    ma_ctdt = worksheet.Cells[row, 9].Text.Trim(),
                    ten_ctdt = tenctdt,
                    id_hdt = 1,
                    ngaycapnhat = unixTimestamp,
                    ngaytao = unixTimestamp
                };
                db.ctdt.Add(existingCTDT);
                db.SaveChanges();
            }
            var existingLop = db.lop.SingleOrDefault(x => x.ma_lop == malop);
            if (existingLop == null)
            {
                var lop = new lop
                {
                    id_ctdt = existingCTDT.id_ctdt,
                    ma_lop = malop,
                    ngaycapnhat = unixTimestamp,
                    ngaytao = unixTimestamp,
                    status = true
                };
                db.lop.Add(lop);
                db.SaveChanges();
            }
            var existingSV = db.sinhvien.FirstOrDefault(x => x.ma_sv == masv && x.hovaten == tensv);
            if (existingSV == null)
            {
                var sinhvien = new sinhvien
                {
                    id_lop = existingLop.id_lop,
                    ma_sv = masv,
                    hovaten = tensv,
                    ngaycapnhat = unixTimestamp,
                    ngaytao = unixTimestamp,
                };
                db.sinhvien.Add(sinhvien);
                db.SaveChanges();
            }
            return true;
        }
        private bool import_sinh_vien_ngoai_edu(ExcelWorksheet worksheet, int row, int unixTimestamp, HashSet<string> uniqueRecords)
        {
            var ten_khoa = worksheet.Cells[row, 1].Text.Trim();
            var ten_ctdt = worksheet.Cells[row, 2].Text.Trim();
            var ma_lop = worksheet.Cells[row, 3].Text.Trim();
            var ten_sv = worksheet.Cells[row, 4].Text.Trim();
            var ma_sv = worksheet.Cells[row, 5].Text.Trim();

            var recordIdentifier = $"{ten_khoa}-{ten_ctdt}-{ma_lop}-{ten_sv}-{ma_sv}";

            if (uniqueRecords.Contains(recordIdentifier))
                return false;

            uniqueRecords.Add(recordIdentifier);
            var existingKhoa = db.khoa_vien_truong.SingleOrDefault(x => x.ten_khoa == ten_khoa);
            if (existingKhoa == null)
            {
                existingKhoa = new khoa_vien_truong
                {
                    ma_khoa = null,
                    ten_khoa = ten_khoa,
                    ngaycapnhat = unixTimestamp,
                    ngaytao = unixTimestamp,
                };
                db.khoa_vien_truong.Add(existingKhoa);
                db.SaveChanges();
            }
            var existingCTDT = db.ctdt.SingleOrDefault(x => x.ten_ctdt == ten_ctdt);
            if (existingCTDT == null)
            {
                existingCTDT = new ctdt
                {
                    ma_ctdt =null,
                    ten_ctdt = ten_ctdt,
                    id_hdt = 1,
                    ngaycapnhat = unixTimestamp,
                    ngaytao = unixTimestamp
                };
                db.ctdt.Add(existingCTDT);
                db.SaveChanges();
            }
            var existingLop = db.lop.SingleOrDefault(x => x.ma_lop == ma_lop);
            if (existingLop == null)
            {
                existingLop = new lop
                {
                    id_ctdt = existingCTDT.id_ctdt,
                    ma_lop = ma_lop,
                    ngaycapnhat = unixTimestamp,
                    ngaytao = unixTimestamp,
                    status = true
                };
                db.lop.Add(existingLop);
                db.SaveChanges();
            }
            var existingSV = db.sinhvien.FirstOrDefault(x => x.ma_sv == ma_sv && x.hovaten == ten_sv);
            if (existingSV == null)
            {
                existingSV = new sinhvien
                {
                    id_lop = existingLop.id_lop,
                    ma_sv = ma_sv,
                    hovaten = ten_sv,
                    ngaycapnhat = unixTimestamp,
                    ngaytao = unixTimestamp,
                };
                db.sinhvien.Add(existingSV);
                db.SaveChanges();
            }
            return true;
        }
    }
}