using CTDT.Helper;
using CTDT.Models;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CTDT.Areas.CBVC.Controllers
{
    [UserAuthorize(4)]
    public class thongkekhaosatcbvcController : Controller
    {
        dbSurveyEntities db = new dbSurveyEntities();
        // Trang Chủ
        public ActionResult Index()
        {
            ViewBag.Year = new SelectList(db.NamHoc.OrderBy(x => x.id_namhoc), "id_namhoc", "ten_namhoc");
            return View();
        }
        [HttpGet]
        public ActionResult LoadFullSurvey(int year = 0)
        {
            var query = db.survey.AsEnumerable();
            if (year != 0)
            {
                query = query.Where(x => x.id_namhoc == year);
            }
            var GetAllSurvey = query
                .Where(x => x.surveyStatus == true)
                .Select(x => new
                {
                    IDSurvey = x.surveyID,
                    NameSurvey = x.surveyTitle,
                    Year = x.NamHoc.ten_namhoc
                }).ToList();
            return Json(new { data = GetAllSurvey, message = "Load dữ liệu thành công" }, JsonRequestBehavior.AllowGet);
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
            var user = SessionHelper.GetUser();
            bool hasAnswerResponseForStaff = db.answer_response.Any(aw => aw.id_CBVC != null && (surveyid == 0 || aw.surveyID == surveyid) && aw.id_donvi == user.id_donvi && aw.json_answer != null);
            var query = db.CanBoVienChuc
                .Where(x => x.id_donvi == user.id_donvi)
                .AsQueryable();


            var filteredRecords = query.Count();
            var TotalDaKhaoSat = query.Count(sv => db.answer_response
                                .Any(aw => aw.id_CBVC == sv.id_CBVC
                                && (surveyid == 0 || aw.surveyID == surveyid)
                                && aw.id_donvi == user.id_donvi
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

            if (hasAnswerResponseForStaff)
            {
                return Json(new { data = GetSV, SumCBVC = filteredRecords, TotalCBVCDaKhaoSat = TotalDaKhaoSat }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { data = (object)null, status = "Không có dữ liệu khảo sát" }, JsonRequestBehavior.AllowGet);
            }
        }
        // Thống kê tần xuất câu hỏi
        public ActionResult thongketanxuat()
        {
            var surveyList = db.survey.ToList();

            var sortedSurveyList = surveyList
                .OrderBy(s => s.surveyTitle.Split('.').First())
                .ThenBy(s => s.surveyTitle)
                .ToList();
            ViewBag.Survey = new SelectList(sortedSurveyList, "surveyID", "surveyTitle");
            ViewBag.Year = new SelectList(db.NamHoc.OrderBy(x => x.id_namhoc), "id_namhoc", "ten_namhoc");
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
        [HttpGet]
        public JsonResult Loadthongketanxuat(int surveyid = 0)
        {
            var user = SessionHelper.GetUser();
            var query = db.answer_response.AsQueryable();

            if (surveyid != 0)
            {
                query = query.Where(x => x.surveyID == surveyid);
            }

            var responses = query
                .Where(d => d.id_donvi == user.id_donvi && d.surveyID == surveyid)
                .Select(x => new { JsonAnswer = x.json_answer, SurveyJson = x.survey.surveyData })
                .ToList();
            Dictionary<string, Dictionary<string, int>> frequency = new Dictionary<string, Dictionary<string, int>>();
            Dictionary<string, List<string>> choices = new Dictionary<string, List<string>>();

            List<string> specificChoices = new List<string> {
                "Hoàn toàn không đồng ý",
                "Không đồng ý",
                "Bình thường",
                "Đồng ý",
                "Hoàn toàn đồng ý"
            };

            foreach (var response in responses)
            {
                JObject jsonAnswerObject = JObject.Parse(response.JsonAnswer);
                JObject surveydataObject = JObject.Parse(response.SurveyJson);

                JArray answerPages = (JArray)jsonAnswerObject["pages"];
                JArray surveyPages = (JArray)surveydataObject["pages"];

                foreach (JObject surveyPage in surveyPages)
                {
                    JArray surveyElements = (JArray)surveyPage["elements"];
                    foreach (JObject surveyElement in surveyElements)
                    {
                        string type = surveyElement["type"].ToString();
                        if (type == "radiogroup")
                        {
                            JArray elementChoices = (JArray)surveyElement["choices"];
                            List<string> elementChoiceTexts = elementChoices.Select(c => c["text"].ToString()).ToList();

                            if (elementChoiceTexts.SequenceEqual(specificChoices))
                            {
                                string questionName = surveyElement["name"].ToString();
                                string questionTitle = surveyElement["title"].ToString();

                                if (!choices.ContainsKey(questionTitle))
                                {
                                    choices[questionTitle] = elementChoiceTexts;
                                }

                                foreach (JObject answerPage in answerPages)
                                {
                                    JArray answerElements = (JArray)answerPage["elements"];

                                    foreach (JObject answerElement in answerElements)
                                    {
                                        if (answerElement["name"].ToString() == questionName)
                                        {
                                            string answer = answerElement["response"]["text"]?.ToString() ?? answerElement["response"]["name"]?.ToString() ?? "";

                                            if (!frequency.ContainsKey(questionTitle))
                                            {
                                                frequency[questionTitle] = new Dictionary<string, int>();
                                            }

                                            if (!frequency[questionTitle].ContainsKey(answer))
                                            {
                                                frequency[questionTitle][answer] = 0;
                                            }

                                            frequency[questionTitle][answer]++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            var results = frequency.Select(f => new
            {
                Question = f.Key,
                TotalResponses = f.Value.Values.Sum(),
                Frequencies = f.Value,
                Percentages = f.Value.ToDictionary(
                    kvp => kvp.Key,
                    kvp => (double)kvp.Value / f.Value.Values.Sum() * 100
                ),
                AverageScore = f.Value.Sum(kvp =>
                {
                    switch (kvp.Key)
                    {
                        case "Hoàn toàn không đồng ý": return kvp.Value * 1;
                        case "Không đồng ý": return kvp.Value * 2;
                        case "Bình thường": return kvp.Value * 3;
                        case "Đồng ý": return kvp.Value * 4;
                        case "Hoàn toàn đồng ý": return kvp.Value * 5;
                        default: return 0;
                    }
                }) / (double)f.Value.Values.Sum()
            }).ToList();

            return Json(results, JsonRequestBehavior.AllowGet);
        }
        // Thống kê đối tượng khảo sát
        public ActionResult thongkekhaosat()
        {
            var surveyList = db.survey.ToList();
            var sortedSurveyList = surveyList
                .OrderBy(s => s.surveyTitle.Split('.').First())
                .ThenBy(s => s.surveyTitle)
                .ToList();
            ViewBag.PKSList = new SelectList(sortedSurveyList, "surveyID", "surveyTitle");
            return View();
        }

        [HttpGet]
        public ActionResult LoadCBVCChuaKhaoSat(int pageNumber = 1, int pageSize = 10, int survey = 0, int completed = -1)
        {
            var user = SessionHelper.GetUser();
            bool hasAnswerResponseForStaff = db.answer_response.Any(aw => aw.id_CBVC != null &&  aw.surveyID == survey && aw.id_donvi == user.id_donvi && aw.json_answer != null);
            var query = db.CanBoVienChuc
                .Where(x => x.id_donvi == user.id_donvi)
                .AsQueryable();

            if (completed == 1)
            {
                query = query.Where(sv => db.answer_response
                    .Any(aw => aw.id_CBVC == sv.id_CBVC &&  aw.surveyID == survey && aw.id_donvi == user.id_donvi && aw.json_answer != null));
            }
            else if (completed == 0)
            {
                query = query.Where(sv => !db.answer_response
                    .Any(aw => aw.id_CBVC == sv.id_CBVC &&  aw.surveyID == survey && aw.id_donvi == user.id_donvi && aw.json_answer != null));
            }

            var filteredRecords = query.Count();
            var TotalDaKhaoSat = query.Count(sv => db.answer_response
                                .Any(aw => aw.id_CBVC == sv.id_CBVC
                                && (survey == 0 || aw.surveyID == survey)
                                && aw.id_donvi == user.id_donvi
                                && aw.json_answer != null));
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
            if (hasAnswerResponseForStaff)
            {
                return Json(new { data = GetSV, totalPages = totalPages, SumCBVC = filteredRecords, TotalCBVCDaKhaoSat = TotalDaKhaoSat }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { data = (object)null, status = "Không có dữ liệu khảo sát" }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult ExportToExcel(int survey = 0, int completed = -1)
        {
            var user = SessionHelper.GetUser();
            bool hasAnswerResponseForStaff = db.answer_response.Any(aw => aw.id_CBVC != null && (survey == 0 || aw.surveyID == survey) && aw.id_donvi == user.id_donvi && aw.json_answer != null);
            string title = "Dữ liệu khảo sát";
            string surveyName = survey == 0 ? "Tất cả khảo sát" : db.survey.FirstOrDefault(s => s.surveyID == survey)?.surveyTitle ?? "Survey";

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
               .Where(x => x.id_donvi == user.id_donvi)
               .AsQueryable();

            if (completed == 1)
            {
                query = query.Where(sv => db.answer_response
                    .Any(aw => aw.id_CBVC == sv.id_CBVC && (survey == 0 || aw.surveyID == survey) && aw.id_donvi == user.id_donvi && aw.json_answer != null));
            }
            else if (completed == 0)
            {
                query = query.Where(sv => !db.answer_response
                    .Any(aw => aw.id_CBVC == sv.id_CBVC && (survey == 0 || aw.surveyID == survey) && aw.id_donvi == user.id_donvi && aw.json_answer != null));
            }

            var GetSV = query
            .OrderBy(l => l.id_CBVC)
                .AsEnumerable()
                .Select(x => new
                {
                    TenCBVC = x.TenCBVC ?? "Không có dữ liệu",
                    MaCBVC = x.MaCBVC ?? "Không có dữ liệu",
                    NgaySinh = x.NgaySinh.HasValue ? x.NgaySinh.Value.ToString("dd-MM-yyyy") : "Không có dữ liệu",
                    Email = x.Email ?? "Không có dữ liệu",
                    DonVi = x.DonVi?.name_donvi ?? "Không có dữ liệu",
                    ChuongTrinh = x.ctdt?.ten_ctdt ?? "Không có dữ liệu",
                    ChucVu = x.ChucVu?.name_chucvu ?? "Không có dữ liệu",
                    DaKhaoSat = db.answer_response.Any(aw => aw.id_CBVC == x.id_CBVC && (survey == 0 || aw.surveyID == survey)) ? "Đã khảo sát" : "Chưa khảo sát"
                })
                .ToList();

            var columnTitles = new Dictionary<string, string>
                {
                    { "TenCBVC", "Họ tên CBVC" },
                    { "MaCBVC", "Mã CBVC" },
                    { "NgaySinh", "Ngày sinh" },
                    { "Email", "Email" },
                    { "DonVi", "Đơn vị" },
                    { "ChuongTrinh", "Chương trình đào tạo" },
                    { "ChucVu", "Chức vụ" },
                    { "DaKhaoSat", "Trạng thái khảo sát" }
                };
            if (hasAnswerResponseForStaff)
            {
                return ExportDataToExcel(GetSV, title, "DoiTuongKhaoSat", surveyName, columnTitles);
            }
            else
            {
                return Json(new { data = (object)null, message = "Không có dữ liệu đối tượng khảo sát ở phiếu này" }, JsonRequestBehavior.AllowGet);
            }
        }

        private ActionResult ExportDataToExcel<T>(List<T> data, string title, string sheetName, string surveyName, Dictionary<string, string> columnTitles)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(sheetName);

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

                    for (int i = 0; i < properties.Length; i++)
                    {
                        var columnName = properties[i].Name;
                        var columnTitle = columnTitles.ContainsKey(columnName) ? columnTitles[columnName] : columnName;
                        worksheet.Cells[3, i + 1].Value = columnTitle;
                    }

                    int row = 4;
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

                    string folderPath = Server.MapPath("~/DataExport/DoiTuongKhaoSat-DonVi");
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
    }
}