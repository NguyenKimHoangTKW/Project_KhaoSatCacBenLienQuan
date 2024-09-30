using CTDT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using CTDT.Helper;
using OfficeOpenXml;
namespace CTDT.Areas.Admin.Controllers
{
    [UserAuthorize(2)]
    public class LopController : Controller
    {
        dbSurveyEntities db = new dbSurveyEntities();
        // GET: Admin/Lop
        public ActionResult ViewLop()
        {
            var Khoa = db.lop.Select(s => s.ma_lop.Substring(0, 4)).Distinct().ToList();
            ViewBag.CTDTList = new SelectList(db.ctdt.OrderBy(l => l.id_ctdt), "id_ctdt", "ten_ctdt");
            ViewBag.Khoa = new SelectList(Khoa);
            return View();
        }
        [HttpPost]
        public ActionResult Add(lop l)
        {
            var status = "";
            DateTime now = DateTime.UtcNow;
            if (ModelState.IsValid)
            {
                status = "Thêm mới Lớp thành công";
                int unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                l.ngaycapnhat = unixTimestamp;
                l.ngaytao = unixTimestamp;
                db.lop.Add(l);
                db.SaveChanges();
            }
            return Json(new { status = status }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult LoadDataLop(int pageNumber = 1, int pageSize = 10, int ctdt = 0, int trangthai = -1,string KhoaDaoTao = "", string keyword = "")
        {
            try
            {
                IQueryable<lop> query = db.lop;

                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(l => l.ma_lop.ToLower().Contains(keyword.ToLower())
                                          || l.ctdt.ten_ctdt.ToLower().Contains(keyword.ToLower()));
                }

                if (!string.IsNullOrEmpty(KhoaDaoTao))
                {
                    query = query.Where(s => s.ma_lop.Substring(0, 4) == KhoaDaoTao);
                }

                if (ctdt != 0)
                {
                    query = query.Where(s => s.id_ctdt == ctdt);
                }

                if(trangthai == 1)
                {
                    query = query.Where(x => x.status == true);
                }
                else if(trangthai == 0)
                {
                    query = query.Where(x => x.status == false);
                }
                var totalRecords = query.Count();
                var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
                var lop = query
                    .OrderBy(l => l.id_lop)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(l => new
                    {
                        IdLop = l.id_lop,
                        MaCTDT = l.ctdt.ten_ctdt,
                        MaLop = l.ma_lop,
                        NgayCapNhat = l.ngaycapnhat,
                        NgayTao = l.ngaytao,
                        hienthi = l.status,
                    }).ToList();

                return Json(new { data = lop, totalPages = totalPages, status = "Load dữ liệu thành công" }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { status = "Load dữ liệu thất bại" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetByID(int id)
        {
            var item = db.lop.Where(k => k.id_lop == id)
                .Select(k => new
                {
                    id_lop = k.id_lop,
                    id_ctdt = k.id_ctdt,
                    ma_lop = k.ma_lop,
                    ngaytao = k.ngaytao,
                    ngaycapnhat = k.ngaycapnhat
                }).FirstOrDefault();

            var status = item != null ? "Success" : "Not Found";
            return Json(new { data = item, status = status }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Edit(lop l)
        {
            DateTime now = DateTime.UtcNow;
            int unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            var status = "";

            var lopp = db.lop.Find(l.id_lop);
            if (lopp != null)
            {
                if (string.IsNullOrEmpty(l.ma_lop))
                {
                    status = "Tên lớp không được để trống";
                }
                else if (db.lop.Any(x => x.ma_lop == l.ma_lop && x.id_lop != l.id_lop))
                {
                    status = "Tên lớp đang bị trùng";
                }
                else
                {
                    lopp.ma_lop = l.ma_lop;
                    lopp.id_ctdt = l.id_ctdt;
                    lopp.ngaycapnhat = unixTimestamp;
                    db.SaveChanges();
                    status = "Cập nhật thông tin thành công";
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
            try
            {
                var lop = db.lop.Find(id);
                if (lop != null)
                {
                    db.lop.Remove(lop);
                    db.SaveChanges();
                    status = "Xóa lớp thành công";
                }
                else
                {
                    status = "Không tìm thấy lớp cần xóa";
                }
            }
            catch (Exception ex)
            {
                status = "Xóa lớp thất bại: " + ex.Message;
            }
            return Json(new { status = status }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult IsStatus(int id)
        {
            var status = "";
            var items = db.lop.Find(id);
            if (items != null)
            {
                items.status = !items.status;
                db.Entry(items).State = EntityState.Modified;
                db.SaveChanges();
                status = "Thay đổi trạng thái lớp thành công";
            }
            return Json(new { status = status }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ChangeStatusMultiple(List<int> ids)
        {
            var status = "";
            if (ids != null && ids.Count > 0)
            {
                foreach (var id in ids)
                {
                    var item = db.lop.Find(id);
                    if (item != null)
                    {
                        item.status = !item.status;
                        db.Entry(item).State = EntityState.Modified;
                    }
                }
                db.SaveChanges();
                status = "Thay đổi trạng thái lớp thành công";
            }
            else
            {
                status = "Không có lớp nào được chọn";
            }
            return Json(new { status = status }, JsonRequestBehavior.AllowGet);
        }

        private int LayMaCTDTTuTenCTDT(string tenctdt)
        {
            var ctdt = db.ctdt.FirstOrDefault(x => x.ten_ctdt == tenctdt);
            return ctdt?.id_ctdt ?? 0;
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
                        int unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                        var check_lop = db.lop.FirstOrDefault();
                        for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                        {
                            string tenctdt = worksheet.Cells[row, 3].Value.ToString();
                            int mactdt = LayMaCTDTTuTenCTDT(tenctdt);
                            var lop = new lop
                            {
                                ma_lop = worksheet.Cells[row,2].Text,
                                ngaytao = unixTimestamp,
                                ngaycapnhat = unixTimestamp,
                                status = true
                            };
                            db.lop.Add(lop);
                        }

                        db.SaveChanges();

                        return Json(new { status = "Thêm mới dữ liệu thành công" }, JsonRequestBehavior.AllowGet);
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