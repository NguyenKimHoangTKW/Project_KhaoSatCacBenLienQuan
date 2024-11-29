using CTDT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.IO;
using CTDT.Helper;
using System.Drawing.Printing;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Globalization;
namespace CTDT.Areas.Admin.Controllers
{
    [UserAuthorize(2)]
    public class ThongKeKhaoSatController : Controller
    {
        dbSurveyEntities db = new dbSurveyEntities();
        // GET: Admin/ThongKeKhaoSat
        public ActionResult ThongKeDoiTuongKhaoSat()
        {
            ViewBag.HDT = new SelectList(db.hedaotao.OrderBy(x => x.id_hedaotao), "id_hedaotao", "ten_hedaotao");
            ViewBag.Year = new SelectList(db.NamHoc.OrderBy(x => x.id_namhoc), "id_namhoc", "ten_namhoc");
            ViewBag.CTDT = new SelectList(db.ctdt.OrderBy(x => x.id_ctdt), "id_ctdt", "ten_ctdt");
            ViewBag.DonVi = new SelectList(db.DonVi.OrderBy(x => x.id_donvi), "id_donvi", "name_donvi");
            return View();
        }
        public JsonResult LoadPKSByYear(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            var surveys = db.survey
                            .Where(x => x.id_namhoc == id && x.surveyStatus == true)
                            .ToList();
            var sortedSurveys = surveys
                                .OrderBy(s => s.surveyTitle.Split('.').FirstOrDefault())
                                .ThenBy(s => s.surveyTitle)
                                .Select(x => new { IDSurvey = x.surveyID, NameSurvey = x.surveyTitle })
                                .ToList();
            return Json(new { data = sortedSurveys, success = true }, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult>  LoadFullSurvey(int year = 0, int hdt = 0)
        {
            var user = SessionHelper.GetUser();

            var survey = await db.survey.Where(x => x.id_namhoc == year && x.id_hedaotao == hdt).ToListAsync();

            var List_data = new List<dynamic>();
            foreach (var items in survey)
            {
                if (items.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu học viên")
                {
                    bool cuu_hoc_vien = new[] { 6 }.Contains(items.id_loaikhaosat);
                    bool hoc_vien_nhap_hoc = new[] { 4 }.Contains(items.id_loaikhaosat);
                    bool hoc_vien_nhap_hoc_khong_co_thoi_gian_tot_nghiep = new[] { 9 }.Contains(items.id_loaikhaosat);
                    bool hoc_vien_cuoi_khoa_co_quyet_dinh_tot_nghiep = new[] { 12 }.Contains(items.id_loaikhaosat);
                    if (cuu_hoc_vien)
                    {
                        var tach_chuoi_hoc_phan_mon_hoc = items.thang_tot_nghiep.Split('-');
                        string hocPhanStartDateString = "01/" + tach_chuoi_hoc_phan_mon_hoc[0].PadLeft(7, '0');
                        string hocPhanEndDateString = "30/" + tach_chuoi_hoc_phan_mon_hoc[1].PadLeft(7, '0');
                        DateTime hocPhanStartDate = DateTime.ParseExact(hocPhanStartDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        DateTime hocPhanEndDate = DateTime.ParseExact(hocPhanEndDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        var get_sv = (await db.sinhvien.ToListAsync())
                               .Where(sv =>
                               {
                                   if (!string.IsNullOrEmpty(sv.namtotnghiep))
                                   {
                                       var formattedNamTotNghiep = sv.namtotnghiep.Split('/');
                                       if (formattedNamTotNghiep.Length == 2)
                                       {
                                           string formattedDate = "01/" + formattedNamTotNghiep[0].PadLeft(2, '0') + "/" + formattedNamTotNghiep[1];

                                           DateTime namtotnghiepDate;
                                           if (DateTime.TryParseExact(formattedDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out namtotnghiepDate))
                                           {
                                               return namtotnghiepDate >= hocPhanStartDate && namtotnghiepDate <= hocPhanEndDate;
                                           }
                                       }
                                   }
                                   return false;
                               })
                               .ToList();
                        var TotalIsKhaoSat = get_sv.Count(sv => db.answer_response
                            .Any(aw => aw.id_sv == sv.id_sv
                            && aw.surveyID == items.surveyID
                            && aw.id_ctdt != null
                            && aw.json_answer != null));
                        double percentage = Math.Round(((double)TotalIsKhaoSat / get_sv.Count) * 100, 2);

                        var DataStudent = new
                        {
                            IDPhieu = items.surveyID,
                            TenPhieu = items.surveyTitle,
                            TongKhaoSat = get_sv.Count,
                            TongPhieuDaTraLoi = TotalIsKhaoSat,
                            TongPhieuChuaTraLoi = (get_sv.Count - TotalIsKhaoSat),
                            TyLeDaTraLoi = percentage,
                            TyLeChuaTraLoi = Math.Round(((double)100 - percentage), 2),
                        };

                        List_data.Add(DataStudent);
                    }
                    else if (hoc_vien_nhap_hoc_khong_co_thoi_gian_tot_nghiep)
                    {
                        var tach_chuoi_hoc_phan_mon_hoc = items.thang_nhap_hoc.Split('-');
                        string hocPhanStartDateString = "01/" + tach_chuoi_hoc_phan_mon_hoc[0].PadLeft(7, '0');
                        string hocPhanEndDateString = "30/" + tach_chuoi_hoc_phan_mon_hoc[1].PadLeft(7, '0');
                        DateTime hocPhanStartDate = DateTime.ParseExact(hocPhanStartDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        DateTime hocPhanEndDate = DateTime.ParseExact(hocPhanEndDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        var get_sv = (await db.sinhvien.ToListAsync())
                               .Where(sv =>
                               {
                                   if (!string.IsNullOrEmpty(sv.namnhaphoc))
                                   {
                                       var formattedNamTotNghiep = sv.namnhaphoc.Split('/');
                                       if (formattedNamTotNghiep.Length == 2)
                                       {
                                           string formattedDate = "01/" + formattedNamTotNghiep[0].PadLeft(2, '0') + "/" + formattedNamTotNghiep[1];

                                           DateTime namtotnghiepDate;
                                           if (DateTime.TryParseExact(formattedDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out namtotnghiepDate))
                                           {
                                               return namtotnghiepDate >= hocPhanStartDate && namtotnghiepDate <= hocPhanEndDate;
                                           }
                                       }
                                   }
                                   return false;
                               })
                               .ToList();
                        var TotalIsKhaoSat = get_sv.Count(sv => db.answer_response
                            .Any(aw => aw.id_sv == sv.id_sv
                            && aw.surveyID == items.surveyID
                            && aw.id_ctdt != null
                            && aw.json_answer != null));
                        double percentage = Math.Round(((double)TotalIsKhaoSat / get_sv.Count) * 100, 2);

                        var DataStudent = new
                        {
                            IDPhieu = items.surveyID,
                            TenPhieu = items.surveyTitle,
                            TongKhaoSat = get_sv.Count,
                            TongPhieuDaTraLoi = TotalIsKhaoSat,
                            TongPhieuChuaTraLoi = (get_sv.Count - TotalIsKhaoSat),
                            TyLeDaTraLoi = percentage,
                            TyLeChuaTraLoi = Math.Round(((double)100 - percentage), 2),
                        };

                        List_data.Add(DataStudent);
                    }
                    else if (hoc_vien_nhap_hoc)
                    {
                        var tach_chuoi_hoc_phan_mon_hoc = items.thang_nhap_hoc.Split('-');
                        string hocPhanStartDateString = "01/" + tach_chuoi_hoc_phan_mon_hoc[0].PadLeft(7, '0');
                        string hocPhanEndDateString = "30/" + tach_chuoi_hoc_phan_mon_hoc[1].PadLeft(7, '0');
                        DateTime hocPhanStartDate = DateTime.ParseExact(hocPhanStartDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        DateTime hocPhanEndDate = DateTime.ParseExact(hocPhanEndDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        var get_sv = (await db.sinhvien.ToListAsync())
                               .Where(sv =>
                               {
                                   if (!string.IsNullOrEmpty(sv.namnhaphoc))
                                   {
                                       var formattedNamTotNghiep = sv.namnhaphoc.Split('/');
                                       if (formattedNamTotNghiep.Length == 2)
                                       {
                                           string formattedDate = "01/" + formattedNamTotNghiep[0].PadLeft(2, '0') + "/" + formattedNamTotNghiep[1];

                                           DateTime namtotnghiepDate;
                                           if (DateTime.TryParseExact(formattedDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out namtotnghiepDate))
                                           {
                                               return namtotnghiepDate >= hocPhanStartDate && namtotnghiepDate <= hocPhanEndDate;
                                           }
                                       }
                                   }
                                   return false;
                               })
                               .ToList();
                        var TotalIsKhaoSat = get_sv.Count(sv => db.answer_response
                            .Any(aw => aw.id_sv == sv.id_sv
                            && aw.surveyID == items.surveyID
                            && aw.id_ctdt != null
                            && aw.json_answer != null));
                        double percentage = Math.Round(((double)TotalIsKhaoSat / get_sv.Count) * 100, 2);

                        var DataStudent = new
                        {
                            IDPhieu = items.surveyID,
                            TenPhieu = items.surveyTitle,
                            TongKhaoSat = get_sv.Count,
                            TongPhieuDaTraLoi = TotalIsKhaoSat,
                            TongPhieuChuaTraLoi = (get_sv.Count - TotalIsKhaoSat),
                            TyLeDaTraLoi = percentage,
                            TyLeChuaTraLoi = Math.Round(((double)100 - percentage), 2),
                        };

                        List_data.Add(DataStudent);
                    }
                    else if (hoc_vien_cuoi_khoa_co_quyet_dinh_tot_nghiep)
                    {
                        var tach_chuoi_hoc_phan_mon_hoc = items.thang_tot_nghiep.Split('-');
                        string hocPhanStartDateString = "01/" + tach_chuoi_hoc_phan_mon_hoc[0].PadLeft(7, '0');
                        string hocPhanEndDateString = "30/" + tach_chuoi_hoc_phan_mon_hoc[1].PadLeft(7, '0');
                        DateTime hocPhanStartDate = DateTime.ParseExact(hocPhanStartDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        DateTime hocPhanEndDate = DateTime.ParseExact(hocPhanEndDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        var get_sv = (await db.sinhvien.ToListAsync())
                               .Where(sv =>
                               {
                                   if (!string.IsNullOrEmpty(sv.namtotnghiep))
                                   {
                                       var formattedNamTotNghiep = sv.namtotnghiep.Split('/');
                                       if (formattedNamTotNghiep.Length == 2)
                                       {
                                           string formattedDate = "01/" + formattedNamTotNghiep[0].PadLeft(2, '0') + "/" + formattedNamTotNghiep[1];

                                           DateTime namtotnghiepDate;
                                           if (DateTime.TryParseExact(formattedDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out namtotnghiepDate))
                                           {
                                               return namtotnghiepDate >= hocPhanStartDate && namtotnghiepDate <= hocPhanEndDate;
                                           }
                                       }
                                   }
                                   return false;
                               })
                               .ToList();
                        var TotalIsKhaoSat = get_sv.Count(sv => db.answer_response
                            .Any(aw => aw.id_sv == sv.id_sv
                            && aw.surveyID == items.surveyID
                            && aw.id_ctdt != null
                            && aw.json_answer != null));
                        double percentage = Math.Round(((double)TotalIsKhaoSat / get_sv.Count) * 100, 2);

                        var DataStudent = new
                        {
                            IDPhieu = items.surveyID,
                            TenPhieu = items.surveyTitle,
                            TongKhaoSat = get_sv.Count,
                            TongPhieuDaTraLoi = TotalIsKhaoSat,
                            TongPhieuChuaTraLoi = (get_sv.Count - TotalIsKhaoSat),
                            TyLeDaTraLoi = percentage,
                            TyLeChuaTraLoi = Math.Round(((double)100 - percentage), 2),
                        };

                        List_data.Add(DataStudent);
                    }
                }
                // Check phiếu thuộc giảng viên
                else if (items.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu giảng viên")
                {
                    var cbvc = db.CanBoVienChuc
                        .AsQueryable();
                    var TotalAll = cbvc.Count();
                    var TotalDaKhaoSat = cbvc.Count(sv => db.answer_response
                                        .Any(aw => aw.id_CBVC == sv.id_CBVC
                                        && (items.surveyID == 0 || aw.surveyID == items.surveyID)
                                        && aw.json_answer != null));
                    var idphieu = db.survey.Where(x => x.surveyID == items.surveyID).FirstOrDefault();
                    double percentage = Math.Round(((double)TotalDaKhaoSat / TotalAll) * 100, 2);
                    var DataCBVC = new
                    {
                        IDPhieu = idphieu.surveyID,
                        TenPhieu = items.surveyTitle,
                        TongKhaoSat = TotalAll,
                        TongPhieuDaTraLoi = TotalDaKhaoSat,
                        TongPhieuChuaTraLoi = (TotalAll - TotalDaKhaoSat),
                        TyLeDaTraLoi = percentage,
                        TyLeChuaTraLoi = Math.Round(((double)100 - percentage), 2),
                    };
                    List_data.Add(DataCBVC);
                }
                else if (items.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu doanh nghiệp")
                {
                    var ctdt = db.answer_response
                      .Where(x => x.id_ctdt != null && x.id_sv == null)
                      .AsQueryable();
                    var TotalAll = ctdt.Count();
                    var idphieu = db.survey.Where(x => x.surveyID == items.surveyID).FirstOrDefault();
                    var DataCTDT = new
                    {
                        IDPhieu = idphieu.surveyID,
                        TenPhieu = items.surveyTitle,
                        TongKhaoSat = TotalAll,
                        TongPhieuDaTraLoi = TotalAll,
                        TongPhieuChuaTraLoi = 0,
                        TyLeDaTraLoi = 100,
                        TyLeChuaTraLoi = 0
                    };
                    List_data.Add(DataCTDT);
                }
                else if (items.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu người học có học phần")
                {
                    bool hoc_vien_co_hoc_phan_ly_thuyet_dang_hoc_tai_truong = new[] { 11 }.Contains(items.id_loaikhaosat);
                    bool hoc_vien_co_hoc_phan_tot_nghiep = new[] { 13 }.Contains(items.id_loaikhaosat);
                    var check_hoc_vien_co_hoc_phan = await db.nguoi_hoc_dang_co_hoc_phan.ToListAsync();
                    // Tách chuỗi học phần survey
                    var tach_chuoi_hoc_phan_mon_hoc = items.hoc_phan.Split('-');
                    string hocphanstart = "01/" + tach_chuoi_hoc_phan_mon_hoc[0].PadLeft(7, '0');
                    string hocphanend = "30/" + tach_chuoi_hoc_phan_mon_hoc[1].PadLeft(7, '0');
                    DateTime hocPhanStartDate = DateTime.ParseExact(hocphanstart, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    DateTime hocPhanEndDate = DateTime.ParseExact(hocphanend, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    // Chạy vòng lặp kiểm tra
                    string hocPhanStartDateString = "01/" + tach_chuoi_hoc_phan_mon_hoc[0].PadLeft(7, '0');
                    string hocPhanEndDateString = "30/" + tach_chuoi_hoc_phan_mon_hoc[1].PadLeft(7, '0');
                    DateTime hocPhanStartDatee = DateTime.ParseExact(hocPhanStartDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    DateTime hocPhanEndDatee = DateTime.ParseExact(hocPhanEndDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    if (hoc_vien_co_hoc_phan_ly_thuyet_dang_hoc_tai_truong)
                    {
                        var check_hp_lt = check_hoc_vien_co_hoc_phan.Where(e => e.mon_hoc.hoc_phan.ten_hoc_phan != "Báo cáo tốt nghiệp"
                            && (hocPhanStartDatee >= hocPhanStartDate
                            && hocPhanStartDatee <= hocPhanEndDate) || (hocPhanEndDatee >= hocPhanStartDate
                            && hocPhanEndDatee <= hocPhanEndDate)).ToList();
                        var total = check_hp_lt.Count;
                        var TotalIsKhaoSat = check_hp_lt.Where(x=> x.da_khao_sat == 1).ToList().Count;
                        double percentage = Math.Round(((double)TotalIsKhaoSat / total) * 100, 2);
                        var DataStudent = new
                        {
                            IDPhieu = items.surveyID,
                            TenPhieu = items.surveyTitle,
                            TongKhaoSat = total,
                            TongPhieuDaTraLoi = TotalIsKhaoSat,
                            TongPhieuChuaTraLoi = (total - TotalIsKhaoSat),
                            TyLeDaTraLoi = percentage,
                            TyLeChuaTraLoi = Math.Round(((double)100 - percentage), 2),
                        };
                        List_data.Add(DataStudent);
                    }
                    else if (hoc_vien_co_hoc_phan_tot_nghiep)
                    {
                        var check_hp_bctn = check_hoc_vien_co_hoc_phan.Where(e => e.mon_hoc.hoc_phan.ten_hoc_phan == "Báo cáo tốt nghiệp"
                            && (hocPhanStartDatee >= hocPhanStartDate
                            && hocPhanStartDatee <= hocPhanEndDate) || (hocPhanEndDatee >= hocPhanStartDate
                            && hocPhanEndDatee <= hocPhanEndDate)).ToList();
                        var total = check_hp_bctn.Count;
                        var TotalIsKhaoSat = check_hp_bctn.Where(x => x.da_khao_sat == 1).ToList().Count;
                        double percentage = Math.Round(((double)TotalIsKhaoSat / total) * 100, 2);
                        var DataStudent = new
                        {
                            IDPhieu = items.surveyID,
                            TenPhieu = items.surveyTitle,
                            TongKhaoSat = total,
                            TongPhieuDaTraLoi = TotalIsKhaoSat,
                            TongPhieuChuaTraLoi = (total - TotalIsKhaoSat),
                            TyLeDaTraLoi = percentage,
                            TyLeChuaTraLoi = Math.Round(((double)100 - percentage), 2),
                        };
                        List_data.Add(DataStudent);
                    }
                }
            }

            return Json(new { data = List_data }, JsonRequestBehavior.AllowGet);

        }
        [HttpGet]
        public ActionResult LoadSurveyDetail(int surveyid)
        {
            var GetSurvey = db.survey
                .Where(x => x.surveyStatus == true && x.surveyID == surveyid)
                .Select(x => new
                {
                    IDSurvey = x.surveyID,
                    NameSurvey = x.surveyTitle
                }).FirstOrDefault();
            return Json(new { data = GetSurvey }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult LoadChartsDoiTuongChuaKhaoSat(int surveyid = 0)
        {
            bool hasAnswerResponseForStaff = db.answer_response.Any(aw => aw.id_CBVC != null && (surveyid == 0 || aw.surveyID == surveyid) && aw.id_donvi != null && aw.json_answer != null);
            bool hasAnswerResponseForCTDT = db.answer_response.Any(aw => aw.id_sv == null && (surveyid == 0 || aw.surveyID == surveyid) && aw.id_ctdt != null && aw.json_answer != null);
            bool hasAnswerResponseForSV = db.answer_response.Any(aw => aw.id_sv != null && (surveyid == 0 || aw.surveyID == surveyid) && aw.id_ctdt != null && aw.json_answer != null);
            if (hasAnswerResponseForStaff)
            {
                var query = db.CanBoVienChuc
                .AsQueryable();


                var filteredRecords = query.Count();
                var TotalDaKhaoSat = query.Count(sv => db.answer_response
                                    .Any(aw => aw.id_CBVC == sv.id_CBVC
                                    && (surveyid == 0 || aw.surveyID == surveyid)
                                    && aw.json_answer != null));
                var GetSV = query
                    .OrderBy(l => l.id_CBVC)
                    .AsEnumerable()
                    .Select(x => new
                    {
                        IDCBVC = x.id_CBVC,
                        TenCBVC = x.TenCBVC ?? "Không có dữ liệu",
                        MaCBVC = x.MaCBVC ?? "Không có dữ liệu",
                        NgaySinh = x.NgaySinh.HasValue ? x.NgaySinh.Value.ToString("dd-MM-yyyy") : "Không có dữ liệu",
                        Email = x.Email ?? "Không có dữ liệu",
                        DonVi = x.DonVi?.name_donvi ?? "Không có dữ liệu",
                        ChuongTrinh = x.ctdt?.ten_ctdt ?? "Không có dữ liệu",
                        ChucVu = x.ChucVu?.name_chucvu ?? "Không có dữ liệu",
                        MaDonVi = x.DonVi?.id_donvi ?? 0,
                        DaKhaoSat = db.answer_response.Any(aw => aw.id_CBVC == x.id_CBVC && (surveyid == 0 || aw.surveyID == surveyid)) ? "Đã khảo sát" : "Chưa khảo sát"
                    })
                    .ToList();
                return Json(new { data = GetSV, totalCount = filteredRecords, surveyedCount = TotalDaKhaoSat }, JsonRequestBehavior.AllowGet);
            }
            else if (hasAnswerResponseForCTDT)
            {
                var query = db.answer_response
                    .Where(x => x.id_ctdt != null && x.id_sv == null)
                    .AsQueryable();

                var filteredRecords = query.Count();
                var getDN = query
                    .OrderBy(l => l.id)
                    .AsEnumerable()
                    .Select(x => new
                    {
                        HoTen = x.users.firstName + " " + x.users.lastName,
                        Email = x.users.email,
                        CTDT = x.ctdt.ten_ctdt,
                    })
                    .ToList();

                return Json(new { data = getDN, totalCount = filteredRecords, surveyedCount = filteredRecords }, JsonRequestBehavior.AllowGet);
            }
            else if (hasAnswerResponseForSV)
            {
                var query = db.sinhvien
                    .Where(x => x.lop.status == true)
                    .AsQueryable();

                var filteredRecords = query.Count();
                var surveyedStudents = query.Count(sv => db.answer_response
                    .Any(aw => aw.id_sv == sv.id_sv
                    && (surveyid == 0 || aw.surveyID == surveyid)
                    && aw.json_answer != null));

                var getSV = query
                    .OrderBy(l => l.id_sv)
                    .AsEnumerable()
                    .Select(x => new
                    {
                        IDSV = x.id_sv,
                        MSSV = x.ma_sv ?? "Không có dữ liệu",
                        Hoten = x.hovaten ?? "Không có dữ liệu",
                        NgaySinh = x.ngaysinh?.ToString("dd-MM-yyyy") ?? "",
                        SDT = x.sodienthoai ?? "Không có dữ liệu",
                        CTDT = x.lop?.ctdt?.ten_ctdt ?? "Không có dữ liệu",
                        Lop = x.lop?.ma_lop ?? "Không có dữ liệu",
                    })
                    .ToList();

                return Json(new { data = getSV, surveyedCount = surveyedStudents, totalCount = filteredRecords }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { data = (object)null, status = "Không có dữ liệu khảo sát" }, JsonRequestBehavior.AllowGet);
            }
        }
        //
        [HttpGet]
        public ActionResult LoadSVChuaKhaoSat(int pageNumber = 1, int pageSize = 10, int filterctdt = 0,int filterdonvi = 0 ,int survey = 0, int completed = -1)
        {
            bool hasAnswerResponseForStudent = db.answer_response.Any(aw => aw.id_sv != null && (survey == 0 || aw.surveyID == survey) && aw.json_answer != null);
            bool hasAnswerResponseForProgram = db.answer_response.Any(aw => aw.id_sv == null && (survey == 0 || aw.surveyID == survey) && aw.id_ctdt != null && aw.json_answer != null);
            bool hasAnswerResponseForStaff = db.answer_response.Any(aw => aw.id_CBVC != null && (survey == 0 || aw.surveyID == survey) && aw.id_donvi != null && aw.json_answer != null);
            if (hasAnswerResponseForStudent)
            {
                var query = db.sinhvien.Where(x => x.lop.status == true).AsQueryable();
                if(filterctdt != 0)
                {
                    query = query.Where(x => x.lop.ctdt.id_ctdt == filterctdt);
                }
                if (completed == 1)
                {
                    query = query.Where(sv => db.answer_response.Any(aw => aw.id_sv == sv.id_sv && (survey == 0 || aw.surveyID == survey) && aw.json_answer != null));
                }
                else if(completed == 0)
                {
                    query = query.Where(sv => !db.answer_response.Any(aw => aw.id_sv == sv.id_sv && (survey == 0 || aw.surveyID == survey) && aw.json_answer != null));
                }
                var filteredRecords = query.Count();
                var GetSV = query
                    .OrderBy(l => l.id_sv)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .AsEnumerable()
                    .Select(x => new
                    {
                        IDSV = x.id_sv,
                        MSSV = x.ma_sv ?? "Không có dữ liệu",
                        Hoten = x.hovaten ?? "Không có dữ liệu",
                        NgaySinh =x.ngaysinh?.ToString("dd-MM-yyyy") ?? "",
                        SDT = x.sodienthoai ?? "Không có dữ liệu",
                        CTDT = x.lop?.ctdt?.ten_ctdt ?? "Không có dữ liệu",
                        Lop = x.lop?.ma_lop ?? "Không có dữ liệu",
                        DaKhaoSat = db.answer_response.Any(aw => aw.id_sv == x.id_sv && (survey == 0 || aw.surveyID == survey)) ? "Đã khảo sát" : "Chưa khảo sát"
                    })
                    .ToList();
                var totalPages = (int)Math.Ceiling((double)filteredRecords / pageSize);
                return Json(new { data = GetSV, hasAnswerResponseForStudent = true, totalPages = totalPages }, JsonRequestBehavior.AllowGet);
            }
            else if (hasAnswerResponseForProgram)
            {           
                var query = db.answer_response
                    .Where(x => x.id_ctdt != null && x.id_sv == null)
                    .AsQueryable();
                if (filterctdt != 0)
                {
                    query = query.Where(x => x.id_ctdt == filterctdt);
                }
                if (completed == 1 )
                {
                    query = query.Where(ct => db.answer_response.Any(aw => aw.id_ctdt == ct.id_ctdt && (survey == 0 || aw.surveyID == survey) && aw.json_answer != null));
                }
                else if(completed == 0)
                {
                    query = query.Where(ct => !db.answer_response.Any(aw => aw.id_ctdt == ct.id_ctdt && (survey == 0 || aw.surveyID == survey) && aw.json_answer != null));
                }
                var filteredRecords = query.Count();
                var GetPrograms = query
                    .OrderBy(l => l.id)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .AsEnumerable()
                    .Select(x => new
                    {
                        HoTen = x.users.firstName + " " + x.users.lastName,
                        Email = x.users.email,
                        CTDT = x.ctdt.ten_ctdt,
                    })
                    .ToList();
                var totalPages = (int)Math.Ceiling((double)filteredRecords / pageSize);
                return Json(new { data = GetPrograms, hasAnswerResponseForProgram = true, totalPages = totalPages }, JsonRequestBehavior.AllowGet);
            }
            else if (hasAnswerResponseForStaff)
            {
                var query = db.CanBoVienChuc.AsQueryable();
                if (filterdonvi != 0)
                {
                    query = query.Where(x => x.id_donvi == filterdonvi);
                }
                if (completed == 1)
                {
                    query = query.Where(cbvc => db.answer_response.Any(aw => aw.id_CBVC == cbvc.id_CBVC && (survey == 0 || aw.surveyID == survey) && aw.json_answer != null));
                }
                else if(completed == 0)
                {
                    query = query.Where(cbvc => !db.answer_response.Any(aw => aw.id_CBVC == cbvc.id_CBVC && (survey == 0 || aw.surveyID == survey) && aw.json_answer != null));
                }
                var filteredRecords = query.Count();
                var GetSV = query
                    .OrderBy(l => l.id_CBVC)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .AsEnumerable()
                    .Select(x => new
                    {
                        IDCBVC = x.id_CBVC,
                        TenCBVC = x.TenCBVC ?? "Không có dữ liệu",
                        MaCBVC = x.MaCBVC ?? "Không có dữ liệu",
                        NgaySinh = x.NgaySinh.HasValue ? x.NgaySinh.Value.ToString("dd-MM-yyyy") : "Không có dữ liệu",
                        Email = x.Email ?? "Không có dữ liệu",
                        DonVi = x.DonVi?.name_donvi ?? "Không có dữ liệu",
                        ChuongTrinh = x.ctdt?.ten_ctdt ?? "Không có dữ liệu",
                        ChucVu = x.ChucVu?.name_chucvu ?? "Không có dữ liệu",
                        MaDonVi = x.DonVi?.id_donvi ?? 0,
                        DaKhaoSat = db.answer_response.Any(aw => aw.id_CBVC == x.id_CBVC && (survey == 0 || aw.surveyID == survey)) ? "Đã khảo sát" : "Chưa khảo sát"
                    })
                    .ToList();
                var totalPages = (int)Math.Ceiling((double)filteredRecords / pageSize);
                return Json(new { data = GetSV, hasAnswerResponseForStaff = true, totalPages = totalPages }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { data = (object)null }, JsonRequestBehavior.AllowGet);
            }
        }

            public ActionResult ExportToExcel(int ctdt = 0,int filterdonvi = 0, int survey = 0, int completed = -1)
            {
                var hasAnswerResponseForStudent = db.answer_response.Any(aw => aw.id_sv != null && (survey == 0 || aw.surveyID == survey) && aw.json_answer != null);
                var hasAnswerResponseForProgram = db.answer_response.Any(aw => aw.id_sv == null && (survey == 0 || aw.surveyID == survey) && aw.id_ctdt != null && aw.json_answer != null);
                var hasAnswerResponseForStaff = db.answer_response.Any(aw => aw.id_CBVC != null && (survey == 0 || aw.surveyID == survey) && aw.id_donvi != null && aw.json_answer != null);
                string title = "Dữ liệu khảo sát";
                string surveyName = survey == 0 ? "Tất cả phiếu khảo sát" : db.survey.FirstOrDefault(s => s.surveyID == survey)?.surveyTitle ?? "Survey";
                if (hasAnswerResponseForStudent)
                {
                    string ctdtName = ctdt == 0 ? "Tất cả CTĐT" : db.ctdt.FirstOrDefault(s => s.id_ctdt == ctdt)?.ten_ctdt ?? "Survey";
                    if (completed == 1)
                    {
                        title = "Đối tượng đã hoàn thành khảo sát";
                    }
                    else if (completed == 0)
                    {
                        title = "Đối tượng chưa hoàn thành khảo sát";
                    }
                    else
                    {
                        title = "Tất cả đối tượng khảo sát";
                    }
                    var query = db.sinhvien.Where(x => x.lop.status == true).AsQueryable();

                    if (ctdt != 0) query = query.Where(ct => ct.lop.ctdt.id_ctdt == ctdt);
                    if (completed == 1)
                        query = query.Where(sv => db.answer_response.Any(aw => aw.id_sv == sv.id_sv && (survey == 0 || aw.surveyID == survey) && aw.json_answer != null));
                    else if (completed == 0)
                        query = query.Where(sv => !db.answer_response.Any(aw => aw.id_sv == sv.id_sv && (survey == 0 || aw.surveyID == survey) && aw.json_answer != null));

                    var students = query.OrderBy(l => l.id_sv).AsEnumerable().Select(x => new
                    {
                        MSSV = x.ma_sv,
                        Hoten = x.hovaten,
                        NgaySinh = x.ngaysinh?.ToString("dd-MM-yyyy") ?? "",
                        SDT = x.sodienthoai,
                        CTDT = x.lop?.ctdt?.ten_ctdt ?? "",
                        Lop = x.lop?.ma_lop ?? "",
                        DaKhaoSat = db.answer_response.Any(aw => aw.id_sv == x.id_sv && (survey == 0 || aw.surveyID == survey)) ? "Đã khảo sát" : "Chưa khảo sát"
                    }).ToList();

                    var columnTitles = new Dictionary<string, string>
                    {
                        { "MSSV", "Mã SV" },
                        { "Hoten", "Họ tên" },
                        { "NgaySinh", "Ngày sinh" },
                        { "SDT", "Số điện thoại" },
                        { "CTDT", "Chương trình đào tạo" },
                        { "Lop", "Lớp" },
                        { "DaKhaoSat", "Trạng thái khảo sát" }
                    };

                    return ExportDataToExcel(students, title, "DoiTuongKhaoSat", surveyName, ctdtName,columnTitles);
                }
                else if (hasAnswerResponseForProgram)
                {
               
                    string ctdtName = ctdt == 0 ? "Tất cả CTĐT" : db.ctdt.FirstOrDefault(s => s.id_ctdt == ctdt)?.ten_ctdt ?? "Survey";
                    title = "Tất cả đối tượng khảo sát";               
                    var query = db.answer_response
                        .Where( x => x.id_ctdt != null && x.id_sv == null)
                        .AsQueryable();

                    if (ctdt != 0) query = query.Where(ct => ct.id_ctdt == ctdt);

                    var programs = query.OrderBy(l => l.id).AsEnumerable().Select(x => new
                    {
                        HoTen = x.users.lastName + " " + x.users.firstName,
                        Email = x.users.email,
                        CTDT = x.ctdt.ten_ctdt,
                    }).ToList();

                    var columnTitles = new Dictionary<string, string>
                    {
                        { "HoTen", "Họ và tên" },
                        { "Email", "Email" },
                        { "CTDT", "Chương trình đào tạo" }
                    };

                    return ExportDataToExcel(programs, title, "DoiTuongKhaoSat", surveyName, ctdtName, columnTitles);
                }
                else if (hasAnswerResponseForStaff)
                {
                    string DonviName = filterdonvi == 0 ? "Tất cả Đơn Vị" : db.DonVi.FirstOrDefault(s => s.id_donvi == filterdonvi)?.name_donvi ?? "Survey";

                    if (completed == 1)
                    {
                        title = "Đối tượng đã hoàn thành khảo sát";
                    }
                    else if (completed == 0)
                    {
                        title = "Đối tượng chưa hoàn thành khảo sát";
                    }
                    else
                    {
                        title = "Tất cả đối tượng khảo sát";
                    }
                    var query = db.CanBoVienChuc
                        .Where(x => x.id_donvi == filterdonvi)
                        .AsQueryable();

                    if (completed == 1)
                        query = query.Where(cbvc => db.answer_response.Any(aw => aw.id_CBVC == cbvc.id_CBVC && (survey == 0 || aw.surveyID == survey) && aw.json_answer != null));
                    else if (completed == 0)
                        query = query.Where(cbvc => !db.answer_response.Any(aw => aw.id_CBVC == cbvc.id_CBVC && (survey == 0 || aw.surveyID == survey) && aw.json_answer != null));

                    var staff = query.OrderBy(l => l.id_CBVC).AsEnumerable().Select(x => new
                    {
                        MaCBVC = x.MaCBVC,
                        TenCBVC = x.TenCBVC,
                        NgaySinh = x.NgaySinh.HasValue ? x.NgaySinh.Value.ToString("dd-MM-yyyy") : "",
                        Email = x.Email,
                        DonVi = x.DonVi?.name_donvi ?? "",
                        ChuongTrinh = x.ctdt?.ten_ctdt ?? "",
                        ChucVu = x.ChucVu?.name_chucvu ?? "",
                        DaKhaoSat = db.answer_response.Any(aw => aw.id_CBVC == x.id_CBVC && (survey == 0 || aw.surveyID == survey)) ? "Đã khảo sát" : "Chưa khảo sát"
                    }).ToList();

                    var columnTitles = new Dictionary<string, string>
                    {
                        { "MaCBVC", "Mã CBVC" },
                        { "TenCBVC", "Tên CBVC" },
                        { "NgaySinh", "Ngày Sinh" },
                        { "Email", "Email" },
                        { "DonVi", "Đơn vị" },
                        { "ChuongTrinh", "Chương trình đào tạo" },
                        { "ChucVu", "Chức vụ" },
                        { "DaKhaoSat", "Trạng thái khảo sát" }
                    };

                    return ExportDataToExcel(staff, title, "DoiTuongKhaoSat", surveyName, DonviName, columnTitles);
                }
                else
                {
                    return Json(new { data = (object)null, message = "Không có dữ liệu đối tượng khảo sát ở phiếu này" }, JsonRequestBehavior.AllowGet);
                }
            }
        private ActionResult ExportDataToExcel<T>(List<T> data, string title, string sheetName, string surveyName, string ctdtName, Dictionary<string, string> columnTitles)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(sheetName);
                string TimeExport = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"); ;

                if (data.Any())
                {
                    var properties = typeof(T).GetProperties();

                    worksheet.Cells["A1:" + GetExcelColumnName(properties.Length) + "1"].Merge = true;
                    worksheet.Cells["A1"].Value = surveyName;
                    worksheet.Cells["A1"].Style.Font.Bold = true;
                    worksheet.Cells["A1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                    worksheet.Cells["A2:" + GetExcelColumnName(properties.Length) + "2"].Merge = true;
                    worksheet.Cells["A2"].Value = title;
                    worksheet.Cells["A2"].Style.Font.Bold = true;
                    worksheet.Cells["A2"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                    worksheet.Cells["A3:" + GetExcelColumnName(properties.Length) + "3"].Merge = true;
                    worksheet.Cells["A3"].Value = ctdtName;
                    worksheet.Cells["A3"].Style.Font.Bold = true;
                    worksheet.Cells["A3"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                    worksheet.Cells["A4:" + GetExcelColumnName(properties.Length) + "4"].Merge = true;
                    worksheet.Cells["A4"].Value = "Thời gian xuất kết quả : " + TimeExport;
                    worksheet.Cells["A4"].Style.Font.Bold = true;
                    worksheet.Cells["A4"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                    for (int i = 0; i < properties.Length; i++)
                    {
                        var columnName = properties[i].Name;
                        var columnTitle = columnTitles.ContainsKey(columnName) ? columnTitles[columnName] : columnName;
                        worksheet.Cells[5, i + 1].Value = columnTitle;
                        worksheet.Cells[5, i + 1].Style.Font.Bold = true;
                    }

                    int row = 6;
                    foreach (var item in data)
                    {
                        for (int col = 0; col < properties.Length; col++)
                        {
                            worksheet.Cells[row, col + 1].Value = properties[col].GetValue(item)?.ToString() ?? "";
                        }
                        row++;
                    }

                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                    string fileName = $"{sheetName}_{timestamp}.xlsx";
                    string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                    string folderPath = Server.MapPath("~/DataExport/DoiTuongKhaoSat-CTDT");
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    string filePath = Path.Combine(folderPath, fileName);
                    FileInfo file = new FileInfo(filePath);
                    package.SaveAs(file);

                    byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                    return File(fileBytes, contentType, fileName);
                }
                else
                {
                    return Json(new { data = (object)null, message = "Không có dữ liệu đối tượng khảo sát ở phiếu này" }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        private string GetExcelColumnName(int index)
        {
            const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string result = "";
            while (index > 0)
            {
                index--;
                result = letters[index % 26] + result;
                index /= 26;
            }
            return result;
        }
        // Thống kê tân xuất
        public ActionResult ThongKeKetQuaKhaoSat()
        {
            var surveyList = db.survey.ToList();

            var sortedSurveyList = surveyList
                .OrderBy(s => s.surveyTitle.Split('.').First())
                .ThenBy(s => s.surveyTitle)
                .ToList();
            ViewBag.Survey = new SelectList(sortedSurveyList, "surveyID", "surveyTitle");
            ViewBag.HDT = new SelectList(db.hedaotao.OrderBy(x => x.id_hedaotao), "id_hedaotao", "ten_hedaotao");
            ViewBag.Year = new SelectList(db.NamHoc.OrderBy(x => x.id_namhoc), "id_namhoc", "ten_namhoc");
            ViewBag.CTDT = new SelectList(db.ctdt.OrderBy(x => x.id_ctdt), "id_ctdt", "ten_ctdt");
            ViewBag.DonVi = new SelectList(db.DonVi.OrderBy(x => x.id_donvi), "id_donvi", "name_donvi");
            return View();
        }
    }
}