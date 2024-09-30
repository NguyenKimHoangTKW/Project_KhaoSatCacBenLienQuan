using CTDT.Helper;
using CTDT.Models;
using Microsoft.Ajax.Utilities;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Drawing.Printing;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace CTDT.Areas.Admin.Controllers
{
    [UserAuthorize(2)]
    public class CTDTController : Controller
    {
        dbSurveyEntities db = new dbSurveyEntities();
        
        // CTĐT Sau đại học
        public ActionResult ViewCTDT()
        {
            ViewBag.KhoaList = new SelectList(db.khoa.OrderBy(l => l.id_khoa), "id_khoa", "ten_khoa");
            ViewBag.HDT = new SelectList(db.hedaotao.OrderBy(l => l.id_hedaotao), "id_hedaotao", "ten_hedaotao");
            ViewBag.CTDTList = new SelectList(db.ctdt.OrderBy(l => l.id_ctdt), "id_ctdt", "ten_ctdt");
            return View();
        }
        [HttpGet]
        public ActionResult GetByID(int id)
        {
            var status = "";
            var item = db.ctdt.Where(k => k.id_ctdt == id)
                .Select(k => new
                {
                    IDCTDT = k.id_ctdt,
                    MaCTDT = k.ma_ctdt,
                    MaKhoa = k.id_khoa,
                    TenCTDT = k.ten_ctdt,
                    NgayTao = k.ngaytao,
                    NgayCapNhat = k.ngaycapnhat
                }).FirstOrDefault();
            return Json(new { data = item, status = status }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult load_ctdt(int filkhoa = 0, int filctdt = 0,int filhdt = 0)
        {
  
                var query = db.ctdt.AsQueryable();
                
                if(filctdt != 0)
                {
                    query = query.Where(x => x.id_ctdt == filctdt);
                }

                if (filkhoa != 0)
                {
                    query = query.Where(x => x.id_khoa == filkhoa);
                }
                if (filhdt != 0)
                {
                    query = query.Where(x => x.id_hdt == filhdt);
                }
                var ctdt = query
                    .Select(c => new
                    {
                        id_ctdt = c.id_ctdt,
                        ma_ctdt = c.ma_ctdt,
                        ten_ctdt = c.ten_ctdt,
                        ten_hdt = c.hedaotao.ten_hedaotao,
                        ngay_cap_nhat = c.ngaycapnhat,
                        ngay_tao = c.ngaytao,
                        ten_khoa = c.khoa.ten_khoa,
                    }).ToList();
                return Json(new { data = ctdt, status = "Load Dữ liệu thành công" }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult Add(ctdt ct)
        {
            var status = "";
            DateTime now = DateTime.UtcNow;
            if (ModelState.IsValid)
            {
                status = "Thêm mới CTĐT thành công";
                int unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                ct.ngaycapnhat = unixTimestamp;
                ct.ngaytao = unixTimestamp;
                db.ctdt.Add(ct);
                db.SaveChanges();
            }
            return Json(new { status = status }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult Edit(ctdt ct)
        {
            var status = "";
            var cTDT = db.ctdt.Find(ct.id_ctdt);
            if (cTDT != null)
            {
                if (ct.ten_ctdt == null)
                {
                    status = "Không được để trống tên CTĐT";
                }
                else
                {
                    cTDT.id_khoa = ct.id_khoa;
                    cTDT.id_ctdt = ct.id_ctdt;
                    cTDT.ma_ctdt = ct.ma_ctdt;
                    cTDT.ngaycapnhat = ct.ngaycapnhat;
                    cTDT.ngaytao = ct.ngaytao;
                    cTDT.ten_ctdt = ct.ten_ctdt;
                    db.SaveChanges();
                    status = "Cập nhật lại CTĐT thành công";
                }
            }
            else
            {
                status = "Cập nhật thông tin thất bại";
            }
            return Json(new { status = status }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult Delete(int id)
        {
            var status = "";
            var ctdtt = db.ctdt.Find(id);
            if (ctdtt != null)
            {
                db.ctdt.Remove(ctdtt);
                db.SaveChanges();
                status = "Xóa CTĐT thành công";
            }
            else
            {
                status = "Xóa CTĐT thất bại";
            }
            return Json(new { status = status }, JsonRequestBehavior.AllowGet);
        }

        public int MapHDTbyIDHDT(string tenHDT)
        {
            var hdt = db.hedaotao.Where(cv => cv.ten_hedaotao == tenHDT).FirstOrDefault();
            return hdt?.id_hedaotao ?? 0;
        }
        public int MapKhoaVienbyIDKhoaVien(string tenKhoaVien)
        {
            var khoavien = db.khoa.Where(cv => cv.ten_khoa == tenKhoaVien).FirstOrDefault();
            return khoavien?.id_khoa ?? 0;
        }

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
                            int unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

                            var TenKhoa = worksheet.Cells[row, 4].Value.ToString();
                            var maKhoa =  MapKhoaVienbyIDKhoaVien(TenKhoa);
                            var TenHDT = worksheet.Cells[row, 5].Text;
                            var mahdt = !string.IsNullOrEmpty(TenHDT) ? MapHDTbyIDHDT(TenHDT) : (int?)null;
                            var ctdt = new ctdt
                            {
                                ma_ctdt = string.IsNullOrEmpty(worksheet.Cells[row, 2].Text) ? null : worksheet.Cells[row, 2].Text,
                                ten_ctdt = worksheet.Cells[row, 3].Text,
                                id_khoa = 2,
                                id_hdt = mahdt,
                                ngaytao = unixTimestamp,
                                ngaycapnhat = unixTimestamp,
                            };
                            db.ctdt.Add(ctdt);
                        }

                        db.SaveChanges();

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