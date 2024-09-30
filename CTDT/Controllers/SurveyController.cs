using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Windows.Media.Media3D;
using CTDT.Helper;
using CTDT.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace CTDT.Controllers
{
    [CheckLoginHelper]
    public class SurveyController : Controller
    {
        dbSurveyEntities db = new dbSurveyEntities();
        // GET: Survey
        public ActionResult Survey(int id)
        {
            ViewBag.id = id;
            return View();
        }
        public JsonResult load_phieu_khao_sat(int id)
        {
            var user = SessionHelper.GetUser();
            var domainGmail = user.email.Split('@')[1];
            var ms_nguoi_hoc = user.email.Split('@')[0];
            var get_data = db.survey.Where(x => x.surveyID == id).FirstOrDefault();
            var js_data = get_data.surveyData;
            var list_thong_tin = new List<dynamic>();
            if (get_data.id_loaikhaosat == 1 || get_data.id_loaikhaosat == 4)
            {
                if (domainGmail.Equals("student.tdmu.edu.vn"))
                {
                    var nguoi_hoc = db.sinhvien.Where(x => x.ma_sv == ms_nguoi_hoc).FirstOrDefault();
                    var get_thong_tin_nguoi_hoc = new
                    {
                        email = user.email,
                        ma_so_nguoi_hoc = nguoi_hoc.ma_sv,
                        ten_nguoi_hoc = nguoi_hoc.hovaten,
                        khoa = nguoi_hoc.lop.ctdt.khoa.ten_khoa,
                        nganh_dao_tao = nguoi_hoc.lop.ctdt.ten_ctdt,
                    };
                    list_thong_tin.Add(get_thong_tin_nguoi_hoc);
                    return Json(new { data = js_data, info = list_thong_tin, is_nguoi_hoc = true }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { is_nguoi_hoc = false, message = "Người dùng không có quyền khảo sát phiếu khảo sát này" }, JsonRequestBehavior.AllowGet);
                }
            }
            else if (get_data.id_loaikhaosat == 8)
            {
                var can_bo_vien_chuc = db.CanBoVienChuc.FirstOrDefault();

                if (can_bo_vien_chuc != null && user.email == can_bo_vien_chuc.Email)
                {
                    var don_vi = can_bo_vien_chuc.DonVi != null ? can_bo_vien_chuc.DonVi.name_donvi : null;
                    var chuc_vu = can_bo_vien_chuc.ChucVu != null ? can_bo_vien_chuc.ChucVu.name_chucvu : null;
                    var ctdt = can_bo_vien_chuc.ctdt != null ? can_bo_vien_chuc.ctdt.ten_ctdt : null;

                    var get_thong_tin_can_bo_vien_chuc = new
                    {
                        email = user.email,
                        ten_cbvc = can_bo_vien_chuc.TenCBVC,
                        don_vi = don_vi,
                        chuc_vu = chuc_vu,
                        ctdt = ctdt,
                    };

                    list_thong_tin.Add(get_thong_tin_can_bo_vien_chuc);
                    return Json(new { data = js_data, info = list_thong_tin, is_cbvc = true }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { is_cbvc = false, message = "Người dùng không có quyền khảo sát phiếu khảo sát này" }, JsonRequestBehavior.AllowGet);
                }
            }
            else if (get_data.id_loaikhaosat == 2 || get_data.id_loaikhaosat == 6)
            {
                var id_nguoi_hoc = (int)Session["nguoi_hoc"];
                var nguoi_hoc = db.sinhvien.Where(x => x.id_sv == id_nguoi_hoc).FirstOrDefault();
                var get_thong_tin_nguoi_hoc = new
                {
                    email = user.email,
                    ma_so_nguoi_hoc = nguoi_hoc.ma_sv,
                    ten_nguoi_hoc = nguoi_hoc.hovaten,
                    khoa = nguoi_hoc.lop.ctdt.khoa.ten_khoa,
                    nganh_dao_tao = nguoi_hoc.lop.ctdt.ten_ctdt,
                };
                list_thong_tin.Add(get_thong_tin_nguoi_hoc);
                return Json(new { data = js_data, info = list_thong_tin, is_cuu_nguoi_hoc = true }, JsonRequestBehavior.AllowGet);
            }
            else if (get_data.id_loaikhaosat == 3)
            {
                var giang_vien = db.CanBoVienChuc.FirstOrDefault();

                if (giang_vien != null && user.email == giang_vien.Email)
                {
                    var id_don_vi = Session["don_vi"] as int?;
                    var id_ctdt = Session["ctdt"] as int?;

                    if (id_don_vi == null || id_ctdt == null)
                    {
                        return Json(new { is_giang_vien = false, message = "Vui lòng xác thực để thực hiện khảo sát" }, JsonRequestBehavior.AllowGet);
                    }

                    var don_vi = db.DonVi.FirstOrDefault(x => x.id_donvi == id_don_vi);
                    var ctdt = db.ctdt.FirstOrDefault(x => x.id_ctdt == id_ctdt);

                    var get_thong_tin_can_bo_vien_chuc = new
                    {
                        email = user.email,
                        ten_cbvc = giang_vien.TenCBVC,
                        don_vi = don_vi != null ? don_vi.name_donvi : null,
                        chuc_vu = giang_vien.ChucVu != null ? giang_vien.ChucVu.name_chucvu : null,
                        ctdt = ctdt != null ? ctdt.ten_ctdt : null,
                    };

                    list_thong_tin.Add(get_thong_tin_can_bo_vien_chuc);
                    return Json(new { data = js_data, info = list_thong_tin, is_giang_vien = true }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { is_giang_vien = false, message = "Người dùng không có quyền khảo sát phiếu khảo sát này" }, JsonRequestBehavior.AllowGet);
                }

            }
            else if (get_data.id_loaikhaosat == 5)
            {
                var id_ctdt = (int)Session["ctdt"];
                var ctdt = db.ctdt.Where(x => x.id_ctdt == id_ctdt).FirstOrDefault();
                var get_thong_tin_doanh_nghiep = new
                {
                    email = user.email,
                    ctdt = ctdt.ten_ctdt,
                };
                list_thong_tin.Add(get_thong_tin_doanh_nghiep);
                return Json(new { data = js_data, info = list_thong_tin, is_doanh_nghiep = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { data = (object)null }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult save_form(answer_response aw)
        {
            var user = SessionHelper.GetUser();
            var domainGmail = user.email.Split('@')[1];
            var ms_nguoi_hoc = user.email.Split('@')[0];
            DateTime now = DateTime.UtcNow;
            int unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            var survey = db.survey.FirstOrDefault(x => x.surveyID == aw.surveyID);
            if (ModelState.IsValid)
            {
                if (survey.id_loaikhaosat == 1 || survey.id_loaikhaosat == 4)
                {
                    if (domainGmail.Equals("student.tdmu.edu.vn"))
                    {
                        var nguoi_hoc = db.sinhvien.Where(x => x.ma_sv == ms_nguoi_hoc).FirstOrDefault();

                        aw.time = unixTimestamp;
                        aw.id_users = user.id_users;
                        aw.id_namhoc = survey.id_namhoc;
                        aw.id_sv = nguoi_hoc.id_sv;
                        aw.id_ctdt = nguoi_hoc.lop.ctdt.id_ctdt;
                        db.answer_response.Add(aw);
                        db.SaveChanges();
                    }
                }
                else if (survey.id_loaikhaosat == 8)
                {
                    var can_bo_vien_chuc = db.CanBoVienChuc.Where(x => user.email == x.Email).FirstOrDefault();
                    if (can_bo_vien_chuc != null)
                    {
                        aw.time = unixTimestamp;
                        aw.id_users = user.id_users;
                        aw.id_namhoc = survey.id_namhoc;
                        aw.id_CBVC = can_bo_vien_chuc.id_CBVC;
                        aw.id_ctdt = can_bo_vien_chuc.ctdt?.id_ctdt ?? null;
                        aw.id_donvi = can_bo_vien_chuc.DonVi?.id_donvi ?? null;
                        db.answer_response.Add(aw);
                        db.SaveChanges();
                    }
                }
                else if (survey.id_loaikhaosat == 2 || survey.id_loaikhaosat == 6)
                {
                    var id_nguoi_hoc = (int)Session["nguoi_hoc"];
                    var nguoi_hoc = db.sinhvien.Where(x => x.id_sv == id_nguoi_hoc).FirstOrDefault();
                    aw.time = unixTimestamp;
                    aw.id_users = user.id_users;
                    aw.id_namhoc = survey.id_namhoc;
                    aw.id_sv = nguoi_hoc.id_sv;
                    aw.id_ctdt = nguoi_hoc.lop.ctdt.id_ctdt;
                    db.answer_response.Add(aw);
                    db.SaveChanges();
                }
                else if (survey.id_loaikhaosat == 3)
                {
                    var giang_vien = db.CanBoVienChuc.FirstOrDefault(x => user.email == x.Email);
                    if (giang_vien != null)
                    {
                        var id_don_vi = Session["don_vi"] as int?;
                        var id_ctdt = Session["ctdt"] as int?;
                        var don_vi = db.DonVi.FirstOrDefault(x => x.id_donvi == id_don_vi);
                        var ctdt = db.ctdt.FirstOrDefault(x => x.id_ctdt == id_ctdt);

                        aw.time = unixTimestamp;
                        aw.id_users = user.id_users;
                        aw.id_namhoc = survey.id_namhoc;
                        aw.id_CBVC = giang_vien.id_CBVC;
                        aw.id_ctdt = ctdt != null ? ctdt.id_ctdt : (int?)null;
                        aw.id_donvi = don_vi != null ? don_vi.id_donvi : (int?)null;

                        db.answer_response.Add(aw);
                        db.SaveChanges();
                    }

                }
                else if (survey.id_loaikhaosat == 5)
                {
                    var id_ctdt = (int)Session["ctdt"];
                    var ctdt = db.ctdt.Where(x => x.id_ctdt == id_ctdt).FirstOrDefault();
                    aw.time = unixTimestamp;
                    aw.id_users = user.id_users;
                    aw.id_namhoc = survey.id_namhoc;
                    aw.id_ctdt = ctdt.id_ctdt;
                    db.answer_response.Add(aw);
                    db.SaveChanges();
                }
            }
            return Json(new { success = true, message = "Thêm mới dữ liệu thành công" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AnswerPKS(int id, int surveyid)
        {
            ViewBag.SurveyId = surveyid;
            ViewBag.Id = id;
            return View();
        }
        public JsonResult load_answer_phieu_khao_sat(int id, int surveyid)
        {
            var answer_responses = db.answer_response
                .Where(x => x.id == id && x.surveyID == surveyid)
                .FirstOrDefault();
            var get_data = new
            {
                phieu_khao_sat = answer_responses.survey.surveyData,
                dap_an = answer_responses.json_answer,
            };
            return Json(new { data = get_data, success = true}, JsonRequestBehavior.AllowGet);
        }

        public ActionResult save_answer_form(answer_response aw)
        {
            var user = SessionHelper.GetUser();
            var status = "";
            var answer = db.answer_response.Find(aw.id);
            var survey = db.answer_response.FirstOrDefault(x => x.id == answer.id);
            DateTime now = DateTime.UtcNow;
            int unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            if (answer != null)
            {
                answer.surveyID = survey.surveyID;
                answer.id_users = survey.id_users;
                answer.id_ctdt = survey.id_ctdt;
                answer.id_sv = survey.id_sv;
                answer.json_answer = aw.json_answer;
                answer.id_donvi = survey.id_donvi;
                answer.id_CBVC = survey.id_CBVC;
                answer.id_namhoc = survey.id_namhoc;
                answer.time = unixTimestamp;
                db.SaveChanges();
                status = "Cập nhật dữ liệu thành công";
            }
            else
            {
                status = "Cập nhật dữ liệu thất bại";
            }
            return Json(new { message = status }, JsonRequestBehavior.AllowGet);
        }
    }
}