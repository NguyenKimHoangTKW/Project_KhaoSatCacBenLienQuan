using CTDT.Helper;
using CTDT.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace CTDT.Areas.Admin.Controllers
{
    [UserAuthorize(2)]
    public class PhieuKhaoSatController : Controller
    {
        dbSurveyEntities db = new dbSurveyEntities();
        // GET: Admin/PhieuKhaoSat
        public ActionResult Index()
        {
            ViewBag.HDT = new SelectList(db.hedaotao, "id_hedaotao", "ten_hedaotao");
            ViewBag.LKS = new SelectList(db.LoaiKhaoSat, "id_loaikhaosat", "name_loaikhaosat");
            ViewBag.Year = new SelectList(db.NamHoc, "id_namhoc", "ten_namhoc");

            var maLops = db.lop.Select(s => s.ma_lop).ToList();

            var keyclass = maLops
                            .Select(ml => new string(ml.Where(char.IsDigit).Take(2).ToArray()))
                            .Where(ml => ml.Length == 2)
                            .Distinct()
                            .OrderByDescending(ml => int.Parse(ml))
                            .ToList();

            ViewBag.KeyClass = new SelectList(keyclass);

            return View();
        }


        [HttpPost]
        public ActionResult NewSurvey(survey s)
        {
            var user = SessionHelper.GetUser();
            var status = "";
            DateTime now = DateTime.UtcNow;
            if (ModelState.IsValid)
            {
                int unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                s.surveyTimeMake = unixTimestamp;
                s.surveyTimeUpdate = unixTimestamp;
                s.creator = user.id_users;
                db.survey.Add(s);
                db.SaveChanges();
                status = "Tạo mới phiếu khảo sát thành công";
            }
            return Json(new { status = status}, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult AddSurvey(int id)
        {
            ViewBag.ID = id;
            var items = db.survey.Where(x => x.surveyID == id).ToList();
           return View(items);
        }
        public ActionResult KetQuaPKS(int id)
        {
            ViewBag.id = id;
            var litsKQ = db.answer_response.Where(kq => kq.surveyID == id && kq.json_answer != null).ToList();
            ViewBag.CTDTList = new SelectList(db.ctdt.OrderBy(l => l.id_ctdt), "id_ctdt", "ten_ctdt");
            ViewBag.DonViList = new SelectList(db.DonVi.OrderBy(l => l.id_donvi), "id_donvi", "name_donvi");
            return View(litsKQ);
        }
        public ActionResult AnswerPKS(int id)
        {
            ViewBag.id = id;
            var litsKQ = db.answer_response.Where(kq => kq.id == id).ToList();
            foreach (var item in litsKQ)
            {
                ViewBag.IDPhieu = item.surveyID;
            }
            return View(litsKQ);
        }
        // Load Phiếu khảo sát
        [HttpGet]
        public ActionResult LoadPhieu(int pageNumber = 1, int pageSize = 10, int hdt = 0, int loaiks = 0, int surveystatus = -1, int year = 0, string keyword = "")
        {
            try
            {
                var query = db.survey.AsQueryable();

                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(l => l.surveyTitle.ToLower().Contains(keyword.ToLower())
                                           || l.hedaotao.ten_hedaotao.ToLower().Contains(keyword.ToLower()));
                }

                if (hdt != 0)
                {
                    query = query.Where(p => p.id_hedaotao == hdt);
                }

                if (loaiks != 0)
                {
                    query = query.Where(p => p.id_loaikhaosat == loaiks);
                }

                if (surveystatus == 1)
                {
                    query = query.Where(p => p.surveyStatus == true);
                }
                else if(surveystatus == 0)
                {
                    query = query.Where(p => p.surveyStatus == false);
                }

                if (year != 0)
                {
                    query = query.Where(p => p.id_namhoc == year);
                }

                var totalRecords = query.Count();
                var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                var ListPhieu = query
                    .OrderBy(l => l.surveyID)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new
                    {
                        MaPhieu = p.surveyID,
                        TenHDT = p.hedaotao.ten_hedaotao,
                        MaHDT = p.id_hedaotao,
                        TieuDePhieu = p.surveyTitle,
                        MoTaPhieu = p.surveyDescription,
                        NgayTao = p.surveyTimeMake,
                        NgayChinhSua = p.surveyTimeUpdate,
                        NgayBatDau = p.surveyTimeStart,
                        NgayKetThuc = p.surveyTimeEnd,
                        LoaiKhaoSat = p.LoaiKhaoSat.name_loaikhaosat,
                        MaLKS = p.id_loaikhaosat,
                        NguoiTao = p.users.firstName + " " + p.users.lastName,
                        TrangThai = p.surveyStatus,
                        Nam = p.NamHoc.ten_namhoc,
                    }).ToList();

                return Json(new { data = ListPhieu, totalPages = totalPages, status = "Load dữ liệu thành công" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = $"Load dữ liệu thất bại: {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }
        // Get ID Phieu khảo sát
        public JsonResult GetByIDPKS(int id)
        {
            var GetID = db.survey
                .Where(x => x.surveyID == id)
                .Select(x =>new
                {
                    IdPhieu = x.surveyID,
                    TieuDe = x.surveyTitle,
                    MoTa = x.surveyDescription,
                    HeDaoTao = x.id_hedaotao,
                    DoiTuong = x.id_loaikhaosat,
                    NgayBatDau = x.surveyTimeStart,
                    NgayKetThuc = x.surveyTimeEnd,
                    NamHoc = x.id_namhoc,
                    TrangThai = x.surveyStatus,
                    key_class = x.key_class
                }).FirstOrDefault();
            return Json(new { data = GetID }, JsonRequestBehavior.AllowGet);
        }
        // Chỉnh sửa Phiếu khảo sát
        public JsonResult EditPhieuKhaoSat(int id,survey sv)
        {
            var query = db.survey.FirstOrDefault(x => x.surveyID == id);
            var checkQuery = db.survey.FirstOrDefault(x => x.surveyID == query.surveyID);
            if( query != null)
            {
                DateTime now = DateTime.UtcNow;
                int unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                query.surveyTimeMake = checkQuery.surveyTimeMake;
                query.surveyTimeUpdate = unixTimestamp;
                query.surveyData = checkQuery.surveyData;
                query.creator = checkQuery.creator;
                query.surveyTitle = sv.surveyTitle;
                query.id_hedaotao = sv.id_hedaotao;
                query.surveyDescription = sv.surveyDescription;
                query.surveyStatus = sv.surveyStatus;
                query.id_loaikhaosat = sv.id_loaikhaosat;
                query.id_namhoc = sv.id_namhoc;
                query.surveyTimeStart = sv.surveyTimeStart;
                query.surveyTimeEnd = sv.surveyTimeEnd;
                query.key_class = sv.key_class;
                db.SaveChanges();
                return Json(new { success = true, message = "Cập nhật dữ liệu thành công"}, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new {success = false,message = "Có lỗi trong quá trình cập nhật" }, JsonRequestBehavior.AllowGet);
            }
            
        }

        // Xóa PKS
        public JsonResult DeletePKS(int id)
        {
            var query = db.survey.SingleOrDefault(x => x.surveyID == id);
            var checkAnswer = db.answer_response.Any(x => x.surveyID == query.surveyID);
            if (checkAnswer)
            {
                return Json(new { success = false, message = "Không thể xóa PKS này vì PKS này đang có dữ liệu khảo sát, vui lòng đóng phiếu khảo sát lại để lưu trữ, tránh bị mất dữ liệu"}, JsonRequestBehavior.AllowGet);
            }
            else
            {
                db.survey.Remove(query);
                db.SaveChanges();
                return Json(new { success = true, message ="Xóa phiếu khảo sát thành công"}, JsonRequestBehavior.AllowGet);
            }
           
        }
        [HttpGet]
        public ActionResult LoadKetQuaPKS(int id, int pageNumber = 1, int pageSize = 10)
        {
            bool hasAnswerResponseForStudent = db.answer_response.Any(aw => aw.id_sv != null && (aw.surveyID == id) && aw.json_answer != null);
            bool hasAnswerResponseForProgram = db.answer_response.Any(aw => aw.id_sv == null && (aw.surveyID == id) && aw.id_ctdt != null && aw.json_answer != null);
            bool hasAnswerResponseForStaff = db.answer_response.Any(aw => aw.id_CBVC != null && (aw.surveyID == id) && aw.id_donvi != null && aw.json_answer != null);
            if (hasAnswerResponseForStudent)
            {
                var totalRecords = db.answer_response.Count(aw => aw.surveyID == id && aw.id_sv != null);
                var GetStuden = db.answer_response
                    .Where(kq => kq.surveyID == id && kq.id_sv != null)
                    .OrderBy(kq => kq.id)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(kq => new
                    {
                        MaKQ = kq.id,
                        Email = kq.users.email,
                        MonHoc = kq.mon_hoc.ten_mon_hoc,
                        GiangVien = kq.CanBoVienChuc.TenCBVC,
                        SinhVien = kq.sinhvien.hovaten,
                        ThoiGianThucHien = kq.time,
                        CTDT = kq.ctdt.ten_ctdt,
                        MaAnswer = kq.id,
                        MSSV = kq.sinhvien.ma_sv,
                    }).ToList();

                var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
                return Json(new { status = "Load dữ liệu thành công", totalPages = totalPages, data = GetStuden }, JsonRequestBehavior.AllowGet);
            }
            else if (hasAnswerResponseForProgram)
            {
                var totalRecords = db.answer_response.Count(aw => aw.surveyID == id && aw.id_sv == null && aw.id_ctdt != null);
                var GetProgram = db.answer_response
                    .Where(kq => kq.surveyID == id && kq.id_sv == null && kq.id_ctdt != null)
                    .OrderBy(kq => kq.id)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(kq => new
                    {
                        MaKQ = kq.id,
                        Email = kq.users.email,
                        IDCTDT = kq.id_ctdt,
                        Tenkhoa = kq.ctdt.khoa.ten_khoa ?? "",
                        TenCTDT = kq.ctdt.ten_ctdt,
                        ThoiGianThucHien = kq.time,
                    }).ToList();

                var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
                return Json(new { status = "Load dữ liệu thành công", totalPages = totalPages, data = GetProgram }, JsonRequestBehavior.AllowGet);
            }
            else if (hasAnswerResponseForStaff)
            {
                var totalRecords = db.answer_response.Count(aw => aw.surveyID == id && aw.id_CBVC != null && aw.id_donvi != null);
                var GetStaff = db.answer_response
                    .Where(kq => kq.surveyID == id && kq.id_CBVC != null && kq.id_donvi != null)
                    .OrderBy(kq => kq.id)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(kq => new
                    {
                        MaKQ = kq.id,
                        DonVi = kq.DonVi.name_donvi,
                        Email = kq.users.email,
                        CBVC = kq.CanBoVienChuc.TenCBVC,
                        ThoiGianThucHien = kq.time,
                    }).ToList();

                var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
                return Json(new { status = "Load dữ liệu thành công", totalPages = totalPages, data = GetStaff }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { data = (object)null }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult AnswerSurvey(int id)
        {
            var answers = db.answer_response.Where(d => d.id == id).Select(x => x.json_answer).ToList();
            List<JObject> surveyData = new List<JObject>();

            foreach (var answer in answers)
            {
                JObject answerObject = JObject.Parse(answer);
                surveyData.Add(answerObject);
            }

            return Content(JsonConvert.SerializeObject(surveyData), "application/json");
        }
        public ActionResult ExportExcelSurvey(int id, int cttdt = 0, int donvi = 0)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            bool hasAnswerResponseForStudent = db.answer_response.Any(aw => aw.id_sv != null && aw.surveyID == id);
            bool hasAnswerResponseForProgram = db.answer_response.Any(aw => aw.id_sv == null && aw.surveyID == id && aw.id_ctdt != null);
            bool hasAnswerResponseForStaff = db.answer_response.Any(aw => aw.id_CBVC != null && aw.surveyID == id && aw.id_donvi != null);
            if (hasAnswerResponseForStudent)
            {
                var query = db.answer_response.AsQueryable();
                if (cttdt != 0)
                {
                    query = query.Where(p => p.id_ctdt == cttdt);
                }
                else
                {
                    query = query;
                }
                var answers = query.Where(d => d.surveyID == id)
                                 .Select(x => new
                                 {
                                     DauThoiGian = x.time,
                                     x.json_answer,
                                     Email = x.users.email,
                                     MonHoc = x.mon_hoc.ten_mon_hoc,
                                     GiangVien = x.CanBoVienChuc.TenCBVC,
                                     MSSV = x.sinhvien.ma_sv,
                                     HoTen = x.sinhvien.hovaten,
                                     NgaySinh = (DateTime?)x.sinhvien.ngaysinh,
                                     Lop = x.sinhvien.lop.ma_lop,
                                     CTDT = x.ctdt.ten_ctdt,
                                     Khoa = x.ctdt.khoa.ten_khoa,
                                     SDT = x.sinhvien.sodienthoai,
                                 }).ToList();
                List<JObject> surveyData = new List<JObject>();

                foreach (var answer in answers)
                {
                    JObject answerObject = JObject.Parse(answer.json_answer);
                    answerObject["DauThoiGian"] = answer.DauThoiGian;
                    answerObject["Email"] = answer.Email;
                    answerObject["MonHoc"] = answer.MonHoc;
                    answerObject["GiangVien"] = answer.GiangVien;
                    answerObject["MSSV"] = answer.MSSV;
                    answerObject["HoTen"] = answer.HoTen;
                    answerObject["NgaySinh"] = answer.NgaySinh?.ToString("dd-MM-yyyy");
                    answerObject["Lop"] = answer.Lop;
                    answerObject["CTDT"] = answer.CTDT;
                    answerObject["Khoa"] = answer.Khoa;
                    answerObject["SDT"] = answer.SDT;
                    surveyData.Add(answerObject);
                }
                return Content(JsonConvert.SerializeObject(surveyData), "application/json");
            }
            else if (hasAnswerResponseForProgram)
            {
                var query = db.answer_response.AsQueryable();
                if (cttdt != 0)
                {
                    query = query.Where(p => p.id_ctdt == cttdt);
                }
                else
                {
                    query = query;
                }
                var answers = query.Where(d => d.surveyID == id)
                                 .Select(x => new
                                 {
                                     DauThoiGian = x.time,
                                     x.json_answer,
                                     Email = x.users.email,
                                     CTDT = x.ctdt.ten_ctdt,
                                 }).ToList();
                List<JObject> surveyData = new List<JObject>();

                foreach (var answer in answers)
                {
                    JObject answerObject = JObject.Parse(answer.json_answer);
                    answerObject["DauThoiGian"] = answer.DauThoiGian;
                    answerObject["Email"] = answer.Email;
                    answerObject["CTDT"] = answer.CTDT;
                    surveyData.Add(answerObject);
                }
                return Content(JsonConvert.SerializeObject(surveyData), "application/json");
            }
            else if (hasAnswerResponseForStaff)
            {
                var query = db.answer_response.AsQueryable();
                if (donvi != 0)
                {
                    query = query.Where(p => p.id_donvi == donvi);
                }
                else
                {
                    query = query;
                }
                var answers = query.Where(d => d.surveyID == id)
                                 .Select(x => new
                                 {
                                     DauThoiGian = x.time,
                                     x.json_answer,
                                     HoTen = x.CanBoVienChuc.TenCBVC,
                                     Email = x.users.email,
                                     DonVi = x.DonVi.name_donvi,
                                     ChucDanh = x.CanBoVienChuc.ChucVu.name_chucvu,
                                 }).ToList();
                List<JObject> surveyData = new List<JObject>();

                foreach (var answer in answers)
                {
                    JObject answerObject = JObject.Parse(answer.json_answer);
                    answerObject["DauThoiGian"] = answer.DauThoiGian;
                    answerObject["Email"] = answer.Email;
                    answerObject["HoTen"] = answer.HoTen;
                    answerObject["DonVi"] = answer.DonVi;
                    answerObject["ChucDanh"] = answer.ChucDanh;
                    surveyData.Add(answerObject);
                }
                return Content(JsonConvert.SerializeObject(surveyData), "application/json");
            }
            else
            {
                return Json(new { data = (object)null, message = "Không có dữ liệu đối tượng khảo sát ở phiếu này" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult SaveExcelFile()
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                HttpPostedFileBase file = Request.Files["file"];

                if (file != null && file.ContentLength > 0)
                {
                    string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                    string fileName = $"KetQuaKhaoSat_{timestamp}.xlsx";
                    string directoryPath = Server.MapPath("~/DataExport/KetQuaPKS");
                    string filePath = Path.Combine(directoryPath, fileName);
                    Directory.CreateDirectory(directoryPath);
                    file.SaveAs(filePath);
                    return Json(new { success = true, message = "File saved successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "No file found." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error saving file: " + ex.Message });
            }
        }
    }
}