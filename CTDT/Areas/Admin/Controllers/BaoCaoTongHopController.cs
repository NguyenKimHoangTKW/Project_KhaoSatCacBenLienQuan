using CTDT.Helper;
using CTDT.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace CTDT.Areas.Admin.Controllers
{
    [UserAuthorize(2)]
    public class BaoCaoTongHopController : Controller
    {
        dbSurveyEntities db = new dbSurveyEntities();
        // GET: Admin/BaoCaoTongHop
        public ActionResult Index()
        {
            ViewBag.NamHoc = new SelectList(db.NamHoc.OrderByDescending(x => x.id_namhoc), "id_namhoc", "ten_namhoc");
            ViewBag.HeDaoTao = new SelectList(db.hedaotao.OrderBy(x => x.id_hedaotao), "id_hedaotao", "ten_hedaotao");
            return View();
        }

        public async Task<JsonResult> load_bao_cao(int? year = 0, int? hedaotao = 0)
        {
            var user = SessionHelper.GetUser();
            var query = db.survey.Where(x => x.id_namhoc == 1 && x.id_hedaotao == 2);
            var list_data = new List<dynamic>();

            var sortedSurveys = query.ToList()
                .OrderBy(s => s.surveyTitle.Split('.').FirstOrDefault())
                .ThenBy(s => s.surveyTitle)
                .ToList();

            foreach (var item in sortedSurveys)
            {
                var get_ctdt = db.ctdt.Where(x => x.id_hdt == item.id_hedaotao).ToList();
                var ty_le_tham_gia_khao_sat = new List<dynamic>();
                foreach (var tylethamgia in get_ctdt)
                {
                    if (item.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu học viên")
                    {
                        bool cuu_hoc_vien = new[] { 6 }.Contains(item.id_loaikhaosat);
                        bool hoc_vien_nhap_hoc = new[] { 4 }.Contains(item.id_loaikhaosat);
                        bool hoc_vien_nhap_hoc_khong_co_thoi_gian_tot_nghiep = new[] { 9 }.Contains(item.id_loaikhaosat);
                        bool hoc_vien_cuoi_khoa_co_quyet_dinh_tot_nghiep = new[] { 12 }.Contains(item.id_loaikhaosat);
                        if (cuu_hoc_vien)
                        {
                            // Tách chuỗi và xử lý ngày tốt nghiệp
                            var tach_chuoi_hoc_phan_mon_hoc = item.thang_tot_nghiep.Split('-');
                            string hocPhanStartDateString = "01/" + tach_chuoi_hoc_phan_mon_hoc[0].PadLeft(7, '0');
                            string hocPhanEndDateString = "30/" + tach_chuoi_hoc_phan_mon_hoc[1].PadLeft(7, '0');
                            DateTime hocPhanStartDate = DateTime.ParseExact(hocPhanStartDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                            DateTime hocPhanEndDate = DateTime.ParseExact(hocPhanEndDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                            // Lọc danh sách sinh viên
                            var sinhvienList = await db.sinhvien
                                .Where(x => x.lop.id_ctdt == tylethamgia.id_ctdt)
                                .ToListAsync();

                            var get_sv = sinhvienList
                                .Where(sv =>
                                {
                                    if (!string.IsNullOrEmpty(sv.namtotnghiep))
                                    {
                                        var formattedNamTotNghiep = sv.namtotnghiep.Split('/');
                                        if (formattedNamTotNghiep.Length == 2)
                                        {
                                            string formattedDate = "01/" + formattedNamTotNghiep[0].PadLeft(2, '0') + "/" + formattedNamTotNghiep[1];
                                            if (DateTime.TryParseExact(formattedDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime namtotnghiepDate))
                                            {
                                                return namtotnghiepDate >= hocPhanStartDate && namtotnghiepDate <= hocPhanEndDate;
                                            }
                                        }
                                    }
                                    return false;
                                }).ToList();

                            int totalStudents = get_sv.Count;
                            if (totalStudents > 0)
                            {
                                int TotalIsKhaoSat = get_sv.Count(sv => db.answer_response
                                    .Where(aw => aw.surveyID == item.surveyID && aw.id_ctdt != null && aw.json_answer != null)
                                    .Any(aw => aw.id_sv == sv.id_sv));

                                double percentage = Math.Round(((double)TotalIsKhaoSat / totalStudents) * 100, 2);
                                var get_data = new
                                {
                                    so_phieu_phat_ra = totalStudents,
                                    so_phieu_thu_ve = TotalIsKhaoSat,
                                    so_phieu_chua_thu_ve = totalStudents - TotalIsKhaoSat,
                                    ty_le_phieu_thu_ve = percentage,
                                    ty_le_phieu_chua_thu_ve = Math.Round(100 - percentage, 2),
                                };
                                ty_le_tham_gia_khao_sat.Add(new
                                {
                                    ten_ctdt = tylethamgia.ten_ctdt,
                                    ty_le = get_data,
                                });
                            }
                        }
                        else if (hoc_vien_nhap_hoc_khong_co_thoi_gian_tot_nghiep)
                        {
                            var tach_chuoi_hoc_phan_mon_hoc = item.thang_nhap_hoc.Split('-');
                            string hocPhanStartDateString = "01/" + tach_chuoi_hoc_phan_mon_hoc[0].PadLeft(7, '0');
                            string hocPhanEndDateString = "30/" + tach_chuoi_hoc_phan_mon_hoc[1].PadLeft(7, '0');
                            DateTime hocPhanStartDate = DateTime.ParseExact(hocPhanStartDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                            DateTime hocPhanEndDate = DateTime.ParseExact(hocPhanEndDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                            var get_sv = (await db.sinhvien.Where(x => x.lop.id_ctdt == tylethamgia.id_ctdt).ToListAsync())
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
                                && aw.surveyID == item.surveyID
                                && aw.id_ctdt != null
                                && aw.json_answer != null));
                            double percentage = Math.Round(((double)TotalIsKhaoSat / get_sv.Count) * 100, 2);
                            var get_data = new
                            {
                                so_phieu_phat_ra = get_sv.Count,
                                so_phieu_thu_ve = TotalIsKhaoSat,
                                so_phieu_chua_thu_ve = get_sv.Count - TotalIsKhaoSat,
                                ty_le_phieu_thu_ve = percentage,
                                ty_le_phieu_chua_thu_ve = Math.Round(100 - percentage, 2),
                            };
                            ty_le_tham_gia_khao_sat.Add(new
                            {
                                ten_ctdt = tylethamgia.ten_ctdt,
                                ty_le = get_data,
                            });
                        }
                        else if (hoc_vien_nhap_hoc)
                        {
                            var tach_chuoi_hoc_phan_mon_hoc = item.thang_nhap_hoc.Split('-');
                            string hocPhanStartDateString = "01/" + tach_chuoi_hoc_phan_mon_hoc[0].PadLeft(7, '0');
                            string hocPhanEndDateString = "30/" + tach_chuoi_hoc_phan_mon_hoc[1].PadLeft(7, '0');
                            DateTime hocPhanStartDate = DateTime.ParseExact(hocPhanStartDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                            DateTime hocPhanEndDate = DateTime.ParseExact(hocPhanEndDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                            var get_sv = (await db.sinhvien.Where(x => x.lop.id_ctdt == tylethamgia.id_ctdt).ToListAsync())
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
                                && aw.surveyID == item.surveyID
                                && aw.id_ctdt != null
                                && aw.json_answer != null));
                            double percentage = Math.Round(((double)TotalIsKhaoSat / get_sv.Count) * 100, 2);
                            var get_data = new
                            {
                                so_phieu_phat_ra = get_sv.Count,
                                so_phieu_thu_ve = TotalIsKhaoSat,
                                so_phieu_chua_thu_ve = get_sv.Count - TotalIsKhaoSat,
                                ty_le_phieu_thu_ve = percentage,
                                ty_le_phieu_chua_thu_ve = Math.Round(100 - percentage, 2),
                            };
                            ty_le_tham_gia_khao_sat.Add(new
                            {
                                ten_ctdt = tylethamgia.ten_ctdt,
                                ty_le = get_data,
                            });
                        }
                        else if (hoc_vien_cuoi_khoa_co_quyet_dinh_tot_nghiep)
                        {
                            var tach_chuoi_hoc_phan_mon_hoc = item.thang_tot_nghiep.Split('-');
                            string hocPhanStartDateString = "01/" + tach_chuoi_hoc_phan_mon_hoc[0].PadLeft(7, '0');
                            string hocPhanEndDateString = "30/" + tach_chuoi_hoc_phan_mon_hoc[1].PadLeft(7, '0');
                            DateTime hocPhanStartDate = DateTime.ParseExact(hocPhanStartDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                            DateTime hocPhanEndDate = DateTime.ParseExact(hocPhanEndDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                            var get_sv = (await db.sinhvien.Where(x => x.lop.id_ctdt == tylethamgia.id_ctdt).ToListAsync())
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
                                && aw.surveyID == item.surveyID
                                && aw.id_ctdt != null
                                && aw.json_answer != null));
                            double percentage = Math.Round(((double)TotalIsKhaoSat / get_sv.Count) * 100, 2);
                            var get_data = new
                            {
                                so_phieu_phat_ra = get_sv.Count,
                                so_phieu_thu_ve = TotalIsKhaoSat,
                                so_phieu_chua_thu_ve = get_sv.Count - TotalIsKhaoSat,
                                ty_le_phieu_thu_ve = percentage,
                                ty_le_phieu_chua_thu_ve = Math.Round(100 - percentage, 2),
                            };
                            ty_le_tham_gia_khao_sat.Add(new
                            {
                                ten_ctdt = tylethamgia.ten_ctdt,
                                ty_le = get_data,
                            });
                        }
                    }
                    else if (item.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu giảng viên")
                    {
                        var cbvc = db.CanBoVienChuc.Where(x => x.id_chuongtrinhdaotao == tylethamgia.id_ctdt && x.MaCBVC != "Thỉnh giảng")
                        .AsQueryable();
                        var TotalAll = cbvc.Count();
                        var TotalDaKhaoSat = cbvc.Count(sv => db.answer_response
                                            .Any(aw => aw.id_CBVC == sv.id_CBVC
                                            && (item.surveyID == 0 || aw.surveyID == item.surveyID)
                                            && aw.json_answer != null));
                        var idphieu = db.survey.Where(x => x.surveyID == item.surveyID).FirstOrDefault();
                        double percentage = Math.Round(((double)TotalDaKhaoSat / TotalAll) * 100, 2);
                        var get_data = new
                        {
                            so_phieu_phat_ra = TotalAll,
                            so_phieu_thu_ve = TotalDaKhaoSat,
                            so_phieu_chua_thu_ve = TotalAll - TotalDaKhaoSat,
                            ty_le_phieu_thu_ve = percentage,
                            ty_le_phieu_chua_thu_ve = Math.Round(100 - percentage, 2),
                        };
                        ty_le_tham_gia_khao_sat.Add(new
                        {
                            ten_ctdt = tylethamgia.ten_ctdt,
                            ty_le = get_data,
                        });
                    }
                    else if (item.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu doanh nghiệp")
                    {
                        var ctdt = db.answer_response
                          .Where(x => x.id_ctdt == tylethamgia.id_ctdt && x.id_sv == null)
                          .AsQueryable();
                        var TotalAll = ctdt.Count();
                        var get_data = new
                        {
                            so_phieu_phat_ra = TotalAll,
                            so_phieu_thu_ve = TotalAll,
                            so_phieu_chua_thu_ve = 0,
                            ty_le_phieu_thu_ve = 100,
                            ty_le_phieu_chua_thu_ve = 0,
                        };
                        ty_le_tham_gia_khao_sat.Add(new
                        {
                            ten_ctdt = tylethamgia.ten_ctdt,
                            ty_le = get_data,
                        });
                    }
                    else if (item.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu người học có học phần")
                    {
                        bool hoc_vien_co_hoc_phan_ly_thuyet_dang_hoc_tai_truong = new[] { 11 }.Contains(item.id_loaikhaosat);
                        bool hoc_vien_co_hoc_phan_tot_nghiep = new[] { 13 }.Contains(item.id_loaikhaosat);
                        var check_hoc_vien_co_hoc_phan = await db.nguoi_hoc_dang_co_hoc_phan.Where(x=> x.sinhvien.lop.id_ctdt == tylethamgia.id_ctdt).ToListAsync();
                        // Tách chuỗi học phần survey
                        var tach_chuoi_hoc_phan_mon_hoc = item.hoc_phan.Split('-');
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
                            var TotalIsKhaoSat = check_hp_lt.Where(x => x.da_khao_sat == 1).ToList().Count;
                            double percentage = Math.Round(((double)TotalIsKhaoSat / total) * 100, 2);
                            var get_data = new
                            {
                                so_phieu_phat_ra = total,
                                so_phieu_thu_ve = TotalIsKhaoSat,
                                so_phieu_chua_thu_ve = total - TotalIsKhaoSat,
                                ty_le_phieu_thu_ve = percentage,
                                ty_le_phieu_chua_thu_ve = Math.Round(100 - percentage, 2),
                            };
                            ty_le_tham_gia_khao_sat.Add(new
                            {
                                ten_ctdt = tylethamgia.ten_ctdt,
                                ty_le = get_data,
                            });
                        }
                        else if (hoc_vien_co_hoc_phan_tot_nghiep)
                        {
                            var check_hp_bctn = check_hoc_vien_co_hoc_phan.Where(e => e.mon_hoc.hoc_phan.ten_hoc_phan == "Thực tập doanh nghiệp"
                           && (hocPhanStartDatee >= hocPhanStartDate
                           && hocPhanStartDatee <= hocPhanEndDate) || (hocPhanEndDatee >= hocPhanStartDate
                           && hocPhanEndDatee <= hocPhanEndDate)).ToList();
                            var total = check_hp_bctn.Count;
                            var TotalIsKhaoSat = check_hp_bctn.Where(x => x.da_khao_sat == 1).ToList().Count;
                            double percentage = Math.Round(((double)TotalIsKhaoSat / total) * 100, 2);
                            var get_data = new
                            {
                                so_phieu_phat_ra = total,
                                so_phieu_thu_ve = TotalIsKhaoSat,
                                so_phieu_chua_thu_ve = total - TotalIsKhaoSat,
                                ty_le_phieu_thu_ve = percentage,
                                ty_le_phieu_chua_thu_ve = Math.Round(100 - percentage, 2),
                            };
                            ty_le_tham_gia_khao_sat.Add(new
                            {
                                ten_ctdt = tylethamgia.ten_ctdt,
                                ty_le = get_data,
                            });
                        }
                    }

                }

                list_data.Add(new
                {
                    ten_phieu = item.surveyTitle,
                    ty_le_khao_sat = ty_le_tham_gia_khao_sat
                });
            }

            // Trả kết quả
            return Json(new { data = list_data, success = true }, JsonRequestBehavior.AllowGet);
        }


    }
}