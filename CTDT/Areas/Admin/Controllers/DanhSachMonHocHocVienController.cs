using CTDT.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace CTDT.Areas.Admin.Controllers
{
    public class DanhSachMonHocHocVienController : Controller
    {
        dbSurveyEntities db = new dbSurveyEntities();
        // GET: Admin/DanhSachMonHocHocVien
        public ActionResult Index()
        {
            return View();
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
                            var magv = worksheet.Cells[row, 1].Text;
                            var tengv = worksheet.Cells[row, 2].Text;
                            var hocphan = worksheet.Cells[row, 3].Text;
                            var mamonhoc = worksheet.Cells[row, 4].Text;
                            var tenmonhoc = worksheet.Cells[row, 5].Text;
                            var malop = worksheet.Cells[row, 6].Text;
                            var thoigianhoc = worksheet.Cells[row, 7].Text;
                            var mahv = worksheet.Cells[row, 8].Text;
                            var tenhv = worksheet.Cells[row, 9].Text;
                            var kiem_tra_truong_duy_nhat = $"{magv}-{tengv}-{hocphan}-{mamonhoc}-{tenmonhoc}-{malop}-{thoigianhoc}-{mahv}-{tenhv}";

                            // Check các trường hợp
                            var check_giang_Vien = await db.CanBoVienChuc.FirstOrDefaultAsync(x => x.MaCBVC.ToLower().Trim() == magv.ToLower().Trim() && x.TenCBVC.ToLower().Trim() == tengv.ToLower().Trim());
                            var check_hoc_phan = await db.hoc_phan.FirstOrDefaultAsync(x => x.ten_hoc_phan.ToLower().Trim() == hocphan.ToLower().Trim());
                            var check_mon_hoc = await db.mon_hoc.FirstOrDefaultAsync(x => x.ma_mon_hoc.ToLower().Trim() == mamonhoc.ToLower().Trim() && x.ten_mon_hoc.ToLower().Trim() == tenmonhoc.ToLower().Trim());
                            var check_lop = await db.lop.FirstOrDefaultAsync(x => x.ma_lop.ToLower().Trim() == malop.ToLower().Trim());
                            var check_nguoi_hoc = await db.sinhvien.FirstOrDefaultAsync(x => x.ma_sv.ToLower().Trim() == mahv.ToLower().Trim());

                            if (check_giang_Vien == null)
                            {
                                check_giang_Vien = new CanBoVienChuc
                                {
                                    MaCBVC = magv,
                                    TenCBVC = tengv,
                                    NgaySinh = null,
                                    Email = null,
                                    id_donvi = null,
                                    id_chucvu = null,
                                    id_namhoc = null,
                                    status = true
                                };
                                db.CanBoVienChuc.Add(check_giang_Vien);
                                await db.SaveChangesAsync();
                            }
                            if (check_mon_hoc == null)
                            {
                                check_mon_hoc = new mon_hoc
                                {
                                    ma_mon_hoc = mamonhoc,
                                    ten_mon_hoc = tenmonhoc,
                                    id_lop = check_lop.id_lop,
                                    id_hoc_phan = check_hoc_phan.id_hoc_phan,
                                    id_group_mh = null,
                                    ngay_cap_nhat = (int)unixTimestamp,
                                    ngay_tao = (int)unixTimestamp

                                };
                                db.mon_hoc.Add(check_mon_hoc);
                                await db.SaveChangesAsync();
                            }
                            var check_hoc_phan_nguoi_hoc = await db.nguoi_hoc_dang_co_hoc_phan
                                .FirstOrDefaultAsync(x => x.id_mon_hoc == check_mon_hoc.id_mon_hoc && x.id_sinh_vien  ==  check_nguoi_hoc.id_sv && x.id_giang_vvien == check_giang_Vien.id_CBVC);

                            if(check_hoc_phan_nguoi_hoc == null)
                            {
                                check_hoc_phan_nguoi_hoc = new nguoi_hoc_dang_co_hoc_phan
                                {
                                    id_mon_hoc = check_mon_hoc.id_mon_hoc,
                                    id_sinh_vien = check_nguoi_hoc.id_sv,
                                    id_giang_vvien = check_giang_Vien.id_CBVC,
                                    thang_by_hoc_phan = thoigianhoc,
                                    da_khao_sat = 0
                                };
                                db.nguoi_hoc_dang_co_hoc_phan.Add(check_hoc_phan_nguoi_hoc  );
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