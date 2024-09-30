using CTDT.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CTDT.Areas.Admin.Controllers
{
    public class DoiTuongKhaoSatController : Controller
    {
        dbSurveyEntities db = new dbSurveyEntities();
        // GET: Admin/DoiTuongKhaoSat
        public ActionResult ViewDoiTuongKhaoSat()
        {
            return View();
        }

        public JsonResult LoadData(int pageNumber = 1, int pageSize = 10, string keyword = "")
        {
            var query = db.LoaiKhaoSat.AsQueryable();
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(l => l.name_loaikhaosat.ToLower().Contains(keyword.ToLower()));
            }
            var GetData = query
                .OrderBy(l => l.id_loaikhaosat)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    IDLKS = x.id_loaikhaosat,
                    TenLKS = x.name_loaikhaosat,
                    NgayTao = x.TimeMake,
                    NgayCapNhat = x.TimeUpdate
                }).ToList();
            var totalRecords = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            return Json(new {data = GetData, message = "Load dữ liệu thành công" , totalPages = totalPages }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetbyID(int id)
        {
            var GetID = db.LoaiKhaoSat
                .Where(x => x.id_loaikhaosat == id)
                .Select(x => new
                {
                    IDLKS = x.id_loaikhaosat,
                    TenLKS = x.name_loaikhaosat,
                }).FirstOrDefault();
            return Json(new { data = GetID, success = true, message = "Load dữ liệu thành công"}, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AddNew(LoaiKhaoSat lks)
        {
            if (ModelState.IsValid)
            {
                DateTime now = DateTime.UtcNow;
                int unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                if(db.LoaiKhaoSat.SingleOrDefault(x => x.name_loaikhaosat == lks.name_loaikhaosat) != null)
                {
                    return Json(new { success = false, message = "Đối tượng khảo sát này đã tồn tại"}, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    lks.TimeMake = unixTimestamp;
                    lks.TimeUpdate = unixTimestamp;
                    db.LoaiKhaoSat.Add(lks);
                    db.SaveChanges();
                    return Json(new { success = true, message = "Thêm dữ liệu thành công" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { success = false, message = "Có lỗi trong quá trình thêm mới dữ liệu" }, JsonRequestBehavior.AllowGet);
            }
            
        }
        public JsonResult UpdateLKS(int id, LoaiKhaoSat lks)
        {
            var query = db.LoaiKhaoSat.SingleOrDefault(x=>x.id_loaikhaosat == id);
            var CheckQuery = db.LoaiKhaoSat.SingleOrDefault(x => x.id_loaikhaosat == query.id_loaikhaosat);
            if (query != null)
            {
                DateTime now = DateTime.UtcNow;
                int unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                query.TimeMake = CheckQuery.TimeMake;
                query.TimeUpdate = unixTimestamp;
                query.name_loaikhaosat = lks.name_loaikhaosat;
                db.SaveChanges();
                return Json(new { success = true, message = "Cập nhật dữ liệu thành công" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, message = "Có lỗi trong quá trình cập nhật dữ liệu thành công" }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult DeleteLKS(int id)
        {
            var query = db.LoaiKhaoSat.SingleOrDefault(x => x.id_loaikhaosat == id);
            var checkQuerySurvey = db.survey.Any(x => x.id_loaikhaosat == query.id_loaikhaosat);
            if (query != null)
            {
                if (checkQuerySurvey)
                {
                    return Json(new { success = false, message = "Đối tượng khảo sát này đang liên kết với phiếu khảo sát, không thể xóa"}, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    db.LoaiKhaoSat.Remove(query);
                    db.SaveChanges();
                    return Json(new { success = true, message = "Xóa dữ liệu thành công" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { success = false, message = "Có lỗi trong quá trình xóa dữ liệu"}, JsonRequestBehavior.AllowGet);
            }
           
        }
    }
}