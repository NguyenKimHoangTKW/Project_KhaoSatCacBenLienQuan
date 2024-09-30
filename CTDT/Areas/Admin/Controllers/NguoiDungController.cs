using CTDT.Helper;
using CTDT.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CTDT.Areas.Admin.Controllers
{
    [UserAuthorize(2)]
    public class NguoiDungController : Controller
    {
        dbSurveyEntities db = new dbSurveyEntities();

        // GET: Admin/NguoiDung
        public ActionResult ViewUser()
        {
            ViewBag.CTDTList = new SelectList(db.ctdt.OrderBy(l => l.id_ctdt), "id_ctdt", "ten_ctdt");
            ViewBag.DonVilist = new SelectList(db.DonVi.OrderBy(l => l.id_donvi), "id_donvi", "name_donvi");
            ViewBag.TypeUserList = new SelectList(db.typeusers.OrderBy(l => l.id_typeusers), "id_typeusers", "name_typeusers");
            return View();
        }
        public ActionResult PhanQuyen(int id)
        {
            var PhanQuyen = db.users.Where(x => x.id_users == id).FirstOrDefault();
            return View(PhanQuyen);
        }
        [HttpGet]
        public ActionResult GetByID(int id)
        {
            var item = db.users.Where(k => k.id_users == id)
                .Select(x => new
                {
                    id_users = x.id_users,
                    email = x.email,
                    id_typeusers = x.id_typeusers,
                    id_ctdt = x.id_ctdt,
                    id_donvi = x.id_donvi,
                    ngaycapnhat = x.ngaycapnhat,
                    ngaytao = x.ngaytao
                }).FirstOrDefault();

            if (item == null)
            {
                return Json(new { status = "Không tìm thấy người dùng" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { data = item, status = "Load dữ liệu thành công" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult load_data(int ctdt = 0, int donvi = 0 ,int trangthaiuser = 0)
        {
           
                IQueryable<users> query = db.users;
                if(ctdt != 0)
                {
                    query = query.Where(u => u.id_ctdt == ctdt);
                }
                if (donvi != 0)
                {
                    query = query.Where(u => u.id_donvi == donvi);
                }
                if (trangthaiuser != 0)
                {
                    query = query.Where(u => u.id_typeusers == trangthaiuser);
                }
                var Listuser = query
                .Select(x => new
                {
                    id_user = x.id_users,
                    ten_user = x.firstName + " " + x.lastName,
                    email = x.email,
                    quyen_han = x.typeusers.name_typeusers,
                    ngay_cap_nhat = x.ngaycapnhat,
                    ngay_tao = x.ngaytao,
                    don_vi = x.DonVi != null ? x.DonVi.name_donvi : "None",
                    ctdt = x.ctdt != null ? x.ctdt.ten_ctdt : "None"
                }).ToList();
                return Json(new { data = Listuser}, JsonRequestBehavior.AllowGet);
           
        }
        [HttpPost]
        public ActionResult Add(users us)
        {
            var status = "";
            var success = false;

            if (ModelState.IsValid)
            {
                DateTime now = DateTime.UtcNow;
                if (string.IsNullOrEmpty(us.email))
                {
                    status = "Vui lòng nhập địa chỉ Email";
                }
                else if (db.users.SingleOrDefault(t => t.email == us.email) != null)
                {
                    status = "Email đang bị trùng, vui lòng nhập lại email mới";
                }
                else
                {
                    int unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                    us.ngaytao = unixTimestamp;
                    us.ngaycapnhat = unixTimestamp;
                    db.users.Add(us);
                    db.SaveChanges();
                    status = "Thêm mới tài khoản thành công";
                    success = true;
                }
            }
            else
            {
                status = "Dữ liệu không hợp lệ";
            }

            return Json(new { status = status, success = success }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetIdHdt(int id_ctdt)
        {
            var idHdt = db.ctdt
                .Where(x => x.id_ctdt == id_ctdt)
                .Select(x => x.id_hdt)
                .FirstOrDefault();

            if (idHdt != null)
            {
                return Json(new { success = true, id_hdt = idHdt }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, id_hdt = (int?)null }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult Edit(users us)
        {
            DateTime now = DateTime.UtcNow;
            var status = "";
            var user = db.users.Find(us.id_users);
            int unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            if (user != null)
            {
                user.email = us.email;
                user.ngaytao = us.ngaytao;
                user.ngaycapnhat = unixTimestamp;
                user.id_ctdt = us.id_ctdt;
                user.id_typeusers = us.id_typeusers;
                user.id_donvi = us.id_donvi;
                user.id_hdt = us.id_hdt;
                db.SaveChanges();
                status = "Cập nhật thông tin tài khoản thành công";
            }
            else
            {
                status = "Cập nhật thông tin tài khoản thất bại";
            }
            return Json(new { status = status }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult Delete(int id)
        {
            var status = "";
            try
            {
                var user = db.users.Find(id);
                if (user != null)
                {
                    db.users.Remove(user);
                    db.SaveChanges();
                    status = "Xóa tài khoản thành công";
                }
                else
                {
                    status = "Không tìm thấy tài khoản cần xóa";
                }
            }
            catch (Exception ex)
            {
                status = "Xóa tài khoản thất bại: " + ex.Message;
            }
            return Json(new { status = status }, JsonRequestBehavior.AllowGet);
        }

        public int MapTenTypeToIDType(string tentype)
        {
            var typeuser = db.typeusers.Where(k => k.name_typeusers == tentype).FirstOrDefault();
            return typeuser ?.id_typeusers ?? 0;
        }
        public int MapTenCTDTToIDCTDT(string tenctdt)
        {
            var ctdt = db.ctdt.Where(k => k.ten_ctdt == tenctdt).FirstOrDefault();
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
                        for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                        {
                            string Chucvu = worksheet.Cells[row, 2].Value.ToString();
                            int machucvu = MapTenTypeToIDType(Chucvu);
                            string CTDT = worksheet.Cells[row, 3].Value.ToString();
                            int mactdt = MapTenCTDTToIDCTDT(CTDT);
                            var nguoidung = new users
                            {

                                email = worksheet.Cells[row, 1].Text,
                                id_typeusers = machucvu,
                                id_ctdt = mactdt,
                                ngaytao = unixTimestamp,
                                ngaycapnhat = unixTimestamp
                            };

                            db.users.Add(nguoidung);
                        }

                        db.SaveChanges();

                        return Json(new { status = "Thêm người dùng thành công" }, JsonRequestBehavior.AllowGet);
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
