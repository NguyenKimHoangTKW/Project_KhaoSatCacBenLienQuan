using CTDT.Helper;
using CTDT.Models;
using OfficeOpenXml.Style.XmlAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CTDT.Areas.Admin.Controllers
{
    [UserAuthorize(2)]
    public class KhoaController : Controller
    {
        dbSurveyEntities db = new dbSurveyEntities();
        // GET: Admin/Khoa
        public ActionResult ViewKhoa()
        {
            ViewBag.CTDTList = new SelectList(db.ctdt.OrderBy(l => l.id_ctdt), "id_ctdt", "ten_ctdt");
            return View();
        }

        public ActionResult load_khoa()
        {
            var get_khoa = db.khoa
                .Select(x => new
                {
                    id_khoa = x.id_khoa,
                    ma_khoa = x.ma_khoa,
                    ten_khoa = x.ten_khoa,
                    ngay_cap_nhat = x.ngaycapnhat,
                    ngay_tao = x.ngaytao

                }).ToList();
            return Json(new {data = get_khoa }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult Add(khoa k)
        {
            var status = "";
            if (ModelState.IsValid)
            {
                DateTime now = DateTime.UtcNow;
                if (string.IsNullOrEmpty(k.ten_khoa))
                {
                    status = "Tên khoa đang bị trống";
                }
                else if(db.khoa.SingleOrDefault(t=>t.ten_khoa == k.ten_khoa) != null)
                {
                    status = "Tên khoa đang bị trùng";
                }
                else
                {
                    int unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                    k.ngaycapnhat = unixTimestamp;
                    k.ngaytao = unixTimestamp;
                    status = "Thêm mới Khoa thành công";
                    db.khoa.Add(k);
                    db.SaveChanges();
                }
            }
            else
            {
                status = "Model không hợp lệ";
            }
            return Json(new { status = status }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Edit(khoa k)
        {
            var status = "";
            var khoa = db.khoa.Find(k.id_khoa);
            if(khoa != null)
            {
                if(k.ten_khoa == null)
                {
                    status = "Không được để trống tên khoa";
                }
                else if(db.khoa.SingleOrDefault(x => x.ten_khoa == k.ten_khoa) != null)
                {
                    status = "Tên khoa đang bị trùng";
                }
                else
                {
                    khoa.id_khoa = k.id_khoa;
                    khoa.ma_khoa = k.ma_khoa;
                    khoa.ten_khoa = k.ten_khoa;
                    khoa.ngaycapnhat = k.ngaycapnhat;
                    khoa.ngaytao = k.ngaytao;
                    db.SaveChanges();
                    status = "Cập nhật lại khoa thành công";
                }
            }
            else
            {
                status = "Cập nhật thông tin thất bại";
            }
            return Json(new { status = status }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetByID(int id)
        {
            var status = "";
            var item = db.khoa.Where(k => k.id_khoa == id)
                .Select(k => new
                {
                    id_khoa = k.id_khoa,
                    ten_khoa= k.ten_khoa ,
                    ma_khoa = k.ma_khoa,
                    ngaytao = k.ngaytao,
                    ngaycapnhat = k.ngaycapnhat
                }).FirstOrDefault();
            return Json(new { data = item, status = status }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var status = "";
            var khoa = db.khoa.Find(id);
            if(khoa != null)
            {
                db.khoa.Remove(khoa);
                db.SaveChanges();
                status = "Xóa Khoa thành công";
            }
            else
            {
                status = "Xóa Khoa thất bại";
            }
            return Json(new { status = status }, JsonRequestBehavior.AllowGet);
        }
    }
}