using CTDT.Helper;
using CTDT.Models;
using Google.Apis.Util;
using GoogleApi.Entities.Search.Common.Enums;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing.Printing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;
using System.Windows.Markup;

namespace CTDT.Areas.CTDT.Controllers
{
    [UserAuthorize(3)]
    public class ThongKeKhaoSatController : Controller
    {
        dbSurveyEntities db = new dbSurveyEntities();
        public ActionResult Index()
        {
            ViewBag.Year = new SelectList(db.NamHoc.OrderByDescending(x => x.id_namhoc), "id_namhoc", "ten_namhoc");
            return View();
        }
        #region Load Charts người học
        public ActionResult load_charts_nguoi_hoc(int year = 0)
        {
            var user = SessionHelper.GetUser();

            var query = db.answer_response
                .Where(x => x.survey.id_hedaotao == user.id_hdt)
                .GroupBy(x => x.survey.surveyID)
                .Select(g => g.FirstOrDefault());

            if (year != 0)
            {
                query = query.Where(x => x.id_namhoc == year);
            }

            var GetAllSurvey = query
                .Select(x => new
                {
                    IDSurvey = x.surveyID,
                    NameSurvey = x.survey.surveyTitle,
                    HocKy = x.hoc_ky != null ? x.hoc_ky.ten_hk : null
                }).ToList();

            var ChartSurvey = new List<dynamic>();

            foreach (var survey in GetAllSurvey)
            {
                var idphieu = db.survey.Where(x => x.surveyID == survey.IDSurvey).FirstOrDefault();

                if (!string.IsNullOrEmpty(idphieu.key_class))
                {
                    var keyClassList = new JavaScriptSerializer().Deserialize<List<string>>(idphieu.key_class);
                    bool isStudent = new[] { 1, 2, 4, 6 }.Contains(idphieu.id_loaikhaosat) && idphieu.is_hocky == false;
                    bool isStudentBySubject = new[] { 1, 2, 4, 6 }.Contains(idphieu.id_loaikhaosat) && idphieu.is_hocky == true;
                    if (isStudent)
                    {
                        sinh_vien_thuong(ChartSurvey, user.id_ctdt, idphieu.surveyID, keyClassList);
                    }
                    else if (isStudentBySubject)
                    {
                        sinh_vien_subject(ChartSurvey, user.id_ctdt, idphieu.surveyID, keyClassList);
                    }
                }
                else
                {
                    bool isCTDT = new[] { 5 }.Contains(idphieu.id_loaikhaosat);
                    bool isCBVC = new[] { 3, 8 }.Contains(idphieu.id_loaikhaosat);
                    if (isCTDT)
                    {
                        chuong_trinh_dao_tao(ChartSurvey, user.id_ctdt, idphieu.surveyID);
                    }
                    else if (isCBVC)
                    {
                        can_bo_vien_chuc(ChartSurvey, user.id_ctdt, idphieu.surveyID);
                    }
                }
            }
            var Alldata = new
            {
                AllSurvey = GetAllSurvey,
                ChartSurvey = ChartSurvey,
            };
            return Json(new { data = Alldata }, JsonRequestBehavior.AllowGet);
        }
        private void can_bo_vien_chuc(dynamic ChartSurvey, int? idctdt, int? surveyid)
        {
            var cbvc = db.CanBoVienChuc
                                .Where(x => x.id_chuongtrinhdaotao == idctdt)
                                .ToList();
            var TotalAll = cbvc.Count();
            var DataCBVC = new
            {
                IDPhieu = surveyid,
                TongKhaoSat = TotalAll,
                TongPhieuDaTraLoi = TotalAll,
                TongPhieuChuaTraLoi = 0,
                TyLeDaTraLoi = 100,
                TyLeChuaTraLoi = 0
            };
            ChartSurvey.Add(DataCBVC);
        }
        private void chuong_trinh_dao_tao(dynamic ChartSurvey, int? idctdt, int? surveyid)
        {
            var ctdt = db.answer_response
                                .Where(x => x.id_ctdt == idctdt &&
                                            x.id_sv == null &&
                                            x.id_mh == null &&
                                            x.id_users != null &&
                                            x.id_users != null &&
                                            x.id_hk == null &&
                                            x.id_CBVC == null)
                                .Count();
            var DataCTDT = new
            {
                IDPhieu = surveyid,
                TongKhaoSat = ctdt,
                TongPhieuDaTraLoi = ctdt,
                TongPhieuChuaTraLoi = 0,
                TyLeDaTraLoi = 100,
                TyLeChuaTraLoi = 0
            };
            ChartSurvey.Add(DataCTDT);
        }
        private void sinh_vien_subject(dynamic ChartSurvey, int? idctdt, int? surveyid, List<string> keyClassList)
        {
            var sinhvienQuery = db.sinhvien
                        .Where(x => keyClassList.Any(k => x.lop.ma_lop.Contains(k)) && x.lop.ctdt.id_ctdt == idctdt);

            var TotalAll = sinhvienQuery.LongCount();
            var TotalIsKhaoSat = sinhvienQuery.LongCount(sv => db.answer_response
                .Any(aw => aw.id_sv == sv.id_sv &&
                           aw.surveyID == surveyid &&
                           aw.id_hk != null &&
                           aw.id_mh != null &&
                           aw.id_ctdt == idctdt &&
                           aw.id_CBVC != null &&
                           aw.json_answer != null));

            double? percentage = TotalAll > 0
                ? Math.Round(((double)TotalIsKhaoSat / TotalAll) * 100, 2)
                : (double?)null;
            var DataStudentBySubject = new
            {
                IDPhieu = surveyid,
                TongKhaoSat = TotalAll,
                TongPhieuDaTraLoi = TotalIsKhaoSat,
                TongPhieuChuaTraLoi = (TotalAll - TotalIsKhaoSat),
                TyLeDaTraLoi = percentage ?? 0,
                TyLeChuaTraLoi = Math.Round(100 - (percentage ?? 0), 2),
                isStudentBySubject = true
            };
            ChartSurvey.Add(DataStudentBySubject);
        }
        private void sinh_vien_thuong(dynamic ChartSurvey, int? idctdt, int? surveyid, List<string> keyClassList)
        {
            var sinhvienQuery = db.sinhvien
                        .Where(x => keyClassList.Any(k => x.lop.ma_lop.Contains(k)) && x.lop.ctdt.id_ctdt == idctdt);
            var TotalAll = sinhvienQuery.LongCount();
            var TotalIsKhaoSat = sinhvienQuery.LongCount(sv => db.answer_response
                .Any(aw => aw.id_sv == sv.id_sv &&
                           aw.surveyID == surveyid &&
                           aw.id_hk == null &&
                           aw.id_mh == null &&
                           aw.id_ctdt == idctdt &&
                           aw.id_CBVC == null &&
                           aw.json_answer != null));
            double? percentage = TotalAll > 0
                ? Math.Round(((double)TotalIsKhaoSat / TotalAll) * 100, 2)
                : (double?)null;
            var DataStudent = new
            {
                IDPhieu = surveyid,
                TongKhaoSat = TotalAll,
                TongPhieuDaTraLoi = TotalIsKhaoSat,
                TongPhieuChuaTraLoi = (TotalAll - TotalIsKhaoSat),
                TyLeDaTraLoi = percentage ?? 0,
                TyLeChuaTraLoi = Math.Round(100 - (percentage ?? 0), 2),
                isStudent = true
            };
            ChartSurvey.Add(DataStudent);
        }
        #endregion
        #region Load người học
        public JsonResult load_nguoi_hoc(int surveyid)
        {
            var user = SessionHelper.GetUser();
            var survey = db.survey
                .Where(x => x.surveyID == surveyid)
                .ToList();
            var list_data = new List<dynamic>();

            foreach (var surveys in survey)
            {
                if (!string.IsNullOrEmpty(surveys.key_class))
                {
                    var keyClassList = new JavaScriptSerializer().Deserialize<List<string>>(surveys.key_class);
                    bool isStudent = new[] { 1, 2, 4, 6 }.Contains(surveys.id_loaikhaosat) && surveys.is_hocky == false;
                    bool isStudentBySubject = new[] { 1, 2, 4, 6 }.Contains(surveys.id_loaikhaosat) && surveys.is_hocky == true;
                    if (isStudent)
                    {
                        sinh_vien_thuong(surveys.surveyTitle, user.id_ctdt, surveys.surveyID, list_data, keyClassList);
                    }
                    else if (isStudentBySubject)
                    {
                        sinh_vien_subject(surveys.surveyTitle, user.id_ctdt, surveys.surveyID, list_data, keyClassList);
                    }
                }
                else
                {
                    bool isCTDT = new[] { 5 }.Contains(surveys.id_loaikhaosat);
                    bool isCBVC = new[] { 3, 8 }.Contains(surveys.id_loaikhaosat);
                    if (isCTDT)
                    {
                        chuong_trinh_dao_tao(surveys.surveyTitle, user.id_ctdt, list_data);
                    }
                    else if (isCBVC)
                    {
                        can_bo_vien_chuc(surveys.surveyTitle, user.id_ctdt, list_data);
                    }
                }
            }
            return Json(new { data = list_data }, JsonRequestBehavior.AllowGet);
        }
        private void can_bo_vien_chuc(string namesurvey, int? idctdt, dynamic list_data)
        {
            var cbvc = db.CanBoVienChuc
                            .Where(x => x.id_chuongtrinhdaotao == idctdt)
                            .Select(x => new
                            {
                                ten_cbvc = x.TenCBVC,
                                email = x.Email,
                                don_vi = x.DonVi != null ? x.DonVi.name_donvi : null,
                                ctdt = x.ctdt.ten_ctdt
                            }).ToList();
            list_data.Add(new
            {
                ten_phieu = namesurvey,
                cbvc = cbvc,
                is_cbvc = true
            });
        }
        private void chuong_trinh_dao_tao(string namesurvey, int? idctdt, dynamic list_data)
        {
            var ctdt = db.answer_response
                            .Where(x => x.id_ctdt == idctdt)
                            .Select(x => new
                            {
                                ho_ten = x.users.firstName + " " + x.users.lastName,
                                email = x.users.email,
                                ctdt = x.ctdt.ten_ctdt
                            }).ToList();
            list_data.Add(new
            {
                ten_phieu = namesurvey,
                ctdt = ctdt,
                is_ctdt = true
            });
        }
        private void sinh_vien_thuong(string namesurvey, int? idctdt, int surveyid, dynamic list_data, List<string> keyClassList)
        {
            var sinh_vien = db.sinhvien
                             .Where(x => keyClassList.Any(k => x.lop.ma_lop.Contains(k)) && x.lop.ctdt.id_ctdt == idctdt)
                             .Select(x => new
                             {
                                 ho_ten = x.hovaten,
                                 ma_nguoi_hoc = x.ma_sv,
                                 lop = x.lop.ma_lop,
                                 tinh_trang_khao_sat = db.answer_response.Any(aw => aw.id_sv == x.id_sv && aw.surveyID == surveyid) ? "Đã khảo sát" : "Chưa khảo sát"
                             }).ToList();
            list_data.Add(new
            {
                ten_phieu = namesurvey,
                nguoi_hoc = sinh_vien,
                is_nguoi_hoc = true
            });
        }
        private void sinh_vien_subject(string namesurvey, int? idctdt, int surveyid, dynamic list_data, List<string> keyClassList)
        {
            var sinh_vien = db.sinhvien
                             .Where(x => keyClassList.Any(k => x.lop.ma_lop.Contains(k)) && x.lop.ctdt.id_ctdt == idctdt)
                             .Select(x => new
                             {
                                 ho_ten = x.hovaten,
                                 ma_nguoi_hoc = x.ma_sv,
                                 lop = x.lop.ma_lop,
                                 tinh_trang_khao_sat = db.answer_response.Any(aw => aw.id_sv == x.id_sv && aw.surveyID == surveyid) ? "Đã khảo sát" : "Chưa khảo sát"
                             }).ToList();
            list_data.Add(new
            {
                ten_phieu = namesurvey,
                nguoi_hoc = sinh_vien,
                is_nguoi_hoc_mon_hoc = true
            });
        }
        #endregion
        #region Thống kê tần xuất câu hỏi
        public ActionResult thongketanxuat()
        {
            var user = SessionHelper.GetUser();
            var surveyList = db.survey.Where(x => x.id_hedaotao == user.id_hdt).ToList();
            var sortedSurveyList = surveyList
                .OrderBy(s => s.surveyTitle.Split('.').First())
                .ThenBy(s => s.surveyTitle)
                .ToList();
            ViewBag.Survey = new SelectList(sortedSurveyList, "surveyID", "surveyTitle");
            ViewBag.Year = new SelectList(db.NamHoc.OrderByDescending(x => x.id_namhoc), "id_namhoc", "ten_namhoc");
            return View();
        }
        public JsonResult LoadPKSByYear(int id)
        {
            var user = SessionHelper.GetUser();

            var surveys = db.survey
                            .Where(x => x.id_namhoc == id && x.id_hedaotao == user.id_hdt)
                            .ToList();
            var sortedSurveys = surveys
                                .OrderBy(s => s.surveyTitle.Split('.').FirstOrDefault())
                                .ThenBy(s => s.surveyTitle)
                                .Select(x => new { IDSurvey = x.surveyID, NameSurvey = x.surveyTitle })
                                .ToList();
            return Json(new { data = sortedSurveys, success = true }, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> load_tan_xuat(int surveyid = 0)
        {
            var user = SessionHelper.GetUser();
            var query = db.answer_response.AsQueryable();
            var survey = await db.survey.AsNoTracking().FirstOrDefaultAsync(x => x.surveyID == surveyid);
            List<object> results = new List<object>();

            if (surveyid != 0)
            {
                query = query.Where(x => x.surveyID == surveyid);
            }

            if (!string.IsNullOrEmpty(survey?.key_class))
            {
                var keyClassList = new JavaScriptSerializer().Deserialize<List<string>>(survey.key_class);
                bool isStudent = new[] { 1, 2, 4, 6 }.Contains(survey.id_loaikhaosat) && survey.is_hocky == false;
                bool isStudentBySubject = new[] { 1, 2, 4, 6 }.Contains(survey.id_loaikhaosat) && survey.is_hocky == true;
                if (isStudent || isStudentBySubject)
                {
                    results = cau_hoi_5_muc(query, user.id_ctdt, survey.surveyID, keyClassList);
                }
            }
            else
            {
                bool isCTDT = new[] { 5 }.Contains(survey.id_loaikhaosat);
                bool isCBVC = new[] { 3, 8 }.Contains(survey.id_loaikhaosat);
                if (isCTDT)
                {
                    results = cau_hoi_5_muc(query, user.id_ctdt, survey.surveyID);
                }
                else if (isCBVC)
                {
                    results = cau_hoi_5_muc(query, user.id_ctdt, survey.surveyID);
                }
            }

            return Json(new { Results = results }, JsonRequestBehavior.AllowGet);
        }


        private List<object> cau_hoi_5_muc(IEnumerable<answer_response> query, int? idctdt, int? idsurvey, List<string> keyClassList = null)
        {
            var responses = query
                .Where(d => (keyClassList == null || keyClassList.Any(k => d.sinhvien.lop.ma_lop.Contains(k))) && d.id_ctdt == idctdt && d.surveyID == idsurvey)
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

            var results = frequency.Select(f => (object)new
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

            return results;
        }


        public JsonResult Loadthongketanxuat5Muc(int surveyid = 0)
        {
            var user = SessionHelper.GetUser();
            var query = db.answer_response.AsQueryable();
            var survey = db.survey.Where(x => x.surveyID == surveyid).FirstOrDefault();
            if (surveyid != 0)
            {
                query = query.Where(x => x.surveyID == surveyid);
            }
            if (!string.IsNullOrEmpty(survey.key_class))
            {
                var keyClassList = new JavaScriptSerializer().Deserialize<List<string>>(survey.key_class);
                bool hasAnswerResponseForStudent = db.answer_response
                    .Any(aw => aw.id_sv != null
                    && (surveyid == 0 || aw.surveyID == surveyid)
                    && aw.id_ctdt == user.id_ctdt &&
                    keyClassList.Any(k => aw.survey.key_class.Contains(k))
                    && aw.json_answer != null);
                if (hasAnswerResponseForStudent)
                {
                    var responses = query
                    .Where(d => keyClassList.Any(k => d.sinhvien.lop.ma_lop.Contains(k)) && d.id_ctdt == user.id_ctdt && d.surveyID == surveyid)
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
            }
            else
            {
                bool hasAnswerResponseForCTDT = db.answer_response
                    .Any(aw => aw.id_sv == null
                    && (surveyid == 0 || aw.surveyID == surveyid)
                    && aw.id_ctdt == user.id_ctdt
                    && aw.id_CBVC == null
                    && aw.json_answer != null);
                bool hasAnswerResponseForCBVC = db.answer_response
                    .Any(aw => aw.id_sv == null
                    && (surveyid == 0 || aw.surveyID == surveyid)
                    && aw.id_ctdt == user.id_ctdt
                    && aw.id_CBVC != null
                    && aw.json_answer != null);
                if (hasAnswerResponseForCTDT || hasAnswerResponseForCBVC)
                {
                    var responses = query
                    .Where(d => d.id_ctdt == user.id_ctdt && d.surveyID == surveyid)
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
            }
            return Json(new { data = (object)null }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult Loadthongketanxuatsingle(int surveyid = 0)
        {
            var user = SessionHelper.GetUser();
            var query = db.answer_response.AsQueryable();
            var survey = db.survey.Where(x => x.surveyID == surveyid).FirstOrDefault();
            if (surveyid != 0)
            {
                query = query.Where(x => x.surveyID == surveyid);
            }
            if (!string.IsNullOrEmpty(survey.key_class))
            {
                var keyClassList = new JavaScriptSerializer().Deserialize<List<string>>(survey.key_class);
                bool hasAnswerResponseForStudent = db.answer_response
                    .Any(aw => aw.id_sv != null
                    && (surveyid == 0 || aw.surveyID == surveyid)
                    && aw.id_ctdt == user.id_ctdt &&
                    keyClassList.Any(k => aw.survey.key_class.Contains(k))
                    && aw.json_answer != null);
                if (hasAnswerResponseForStudent)
                {
                    var responses = query
                    .Where(d => keyClassList.Any(k => d.sinhvien.lop.ma_lop.Contains(k)) && d.id_ctdt == user.id_ctdt && d.surveyID == surveyid)
                    .Select(x => new { JsonAnswer = x.json_answer, SurveyJson = x.survey.surveyData })
                    .ToList();

                    var questionDataDict = new Dictionary<string, dynamic>();

                    List<string> specificChoices = new List<string> {
                "Hoàn toàn không đồng ý",
                "Không đồng ý",
                "Bình thường",
                "Đồng ý",
                "Hoàn toàn đồng ý"
            };
                    foreach (var response in responses)
                    {
                        var surveyDataObject = JObject.Parse(response.SurveyJson);
                        var answerDataObject = JObject.Parse(response.JsonAnswer);
                        var surveyPages = (JArray)surveyDataObject["pages"];
                        var answerPages = (JArray)answerDataObject["pages"];

                        foreach (JObject surveyPage in surveyPages)
                        {
                            var surveyElements = (JArray)surveyPage["elements"];

                            foreach (JObject surveyElement in surveyElements)
                            {
                                var type = surveyElement["type"].ToString();
                                if (type == "radiogroup")
                                {
                                    var questionName = surveyElement["name"].ToString();
                                    var questionTitle = surveyElement["title"].ToString();
                                    var choices = (JArray)surveyElement["choices"];
                                    List<string> elementChoiceTexts = choices.Select(c => c["text"].ToString()).ToList();
                                    if ((!elementChoiceTexts.SequenceEqual(specificChoices)))
                                    {
                                        var choiceCounts = choices.ToDictionary(
                                            c => c["name"].ToString(),
                                            c =>
                                            {
                                                dynamic choice = new ExpandoObject();
                                                choice.ChoiceName = c["name"].ToString();
                                                choice.ChoiceText = c["text"].ToString();
                                                choice.Count = 0;
                                                choice.Percentage = 0.0;
                                                return choice;
                                            }
                                        );

                                        int totalResponses = 0;
                                        foreach (JObject answerPage in answerPages)
                                        {
                                            var answerElements = (JArray)answerPage["elements"];
                                            foreach (JObject answerElement in answerElements)
                                            {
                                                if (answerElement["name"].ToString() == questionName)
                                                {
                                                    var responseObject = answerElement["response"];
                                                    if (responseObject != null)
                                                    {
                                                        string responseName = responseObject["name"]?.ToString();
                                                        string responseText = responseObject["text"]?.ToString();

                                                        if (!string.IsNullOrEmpty(responseName) && choiceCounts.ContainsKey(responseName))
                                                        {
                                                            choiceCounts[responseName].Count++;
                                                            totalResponses++;
                                                        }
                                                        else if (!string.IsNullOrEmpty(responseText))
                                                        {
                                                            var matchingChoice = choiceCounts.Values.FirstOrDefault(c => c.ChoiceText == responseText);
                                                            if (matchingChoice != null)
                                                            {
                                                                matchingChoice.Count++;
                                                                totalResponses++;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        foreach (var choice in choiceCounts.Values)
                                        {
                                            choice.Percentage = totalResponses > 0 ? (double)choice.Count / totalResponses * 100 : 0;
                                        }

                                        if (questionDataDict.ContainsKey(questionName))
                                        {
                                            var existingQuestionData = questionDataDict[questionName];
                                            existingQuestionData.TotalResponses += totalResponses;

                                            foreach (var existingChoice in existingQuestionData.Choices)
                                            {
                                                var matchingNewChoice = choiceCounts.Values.FirstOrDefault(c => c.ChoiceName == existingChoice.ChoiceName);
                                                if (matchingNewChoice != null)
                                                {
                                                    existingChoice.Count += matchingNewChoice.Count;
                                                    existingChoice.Percentage = existingQuestionData.TotalResponses > 0 ? (double)existingChoice.Count / existingQuestionData.TotalResponses * 100 : 0;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            dynamic questionData = new ExpandoObject();
                                            questionData.QuestionName = questionName;
                                            questionData.QuestionTitle = questionTitle;
                                            questionData.TotalResponses = totalResponses;
                                            questionData.Choices = choiceCounts.Values.ToList();
                                            questionDataDict[questionName] = questionData;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    var questionDataList = questionDataDict.Values.Select(q => new
                    {
                        q.QuestionName,
                        q.QuestionTitle,
                        q.TotalResponses,
                        Choices = ((List<dynamic>)q.Choices).Select(c => new
                        {
                            c.ChoiceName,
                            c.ChoiceText,
                            c.Count,
                            c.Percentage
                        }).ToList()
                    }).ToList();

                    return Json(questionDataList, JsonRequestBehavior.AllowGet);
                }


            }
            else
            {
                bool hasAnswerResponseForCTDT = db.answer_response
                    .Any(aw => aw.id_sv == null
                    && (surveyid == 0 || aw.surveyID == surveyid)
                    && aw.id_ctdt == user.id_ctdt
                    && aw.id_CBVC == null
                    && aw.json_answer != null);
                bool hasAnswerResponseForCBVC = db.answer_response
                    .Any(aw => aw.id_sv == null
                    && (surveyid == 0 || aw.surveyID == surveyid)
                    && aw.id_ctdt == user.id_ctdt
                    && aw.id_CBVC != null
                    && aw.json_answer != null);
                if (hasAnswerResponseForCTDT || hasAnswerResponseForCBVC)
                {
                    var responses = query
                    .Where(d => d.id_ctdt == user.id_ctdt && d.surveyID == surveyid)
                    .Select(x => new { JsonAnswer = x.json_answer, SurveyJson = x.survey.surveyData })
                    .ToList();

                    var questionDataDict = new Dictionary<string, dynamic>();

                    List<string> specificChoices = new List<string> {
                "Hoàn toàn không đồng ý",
                "Không đồng ý",
                "Bình thường",
                "Đồng ý",
                "Hoàn toàn đồng ý"
            };
                    foreach (var response in responses)
                    {
                        var surveyDataObject = JObject.Parse(response.SurveyJson);
                        var answerDataObject = JObject.Parse(response.JsonAnswer);
                        var surveyPages = (JArray)surveyDataObject["pages"];
                        var answerPages = (JArray)answerDataObject["pages"];

                        foreach (JObject surveyPage in surveyPages)
                        {
                            var surveyElements = (JArray)surveyPage["elements"];

                            foreach (JObject surveyElement in surveyElements)
                            {
                                var type = surveyElement["type"].ToString();
                                if (type == "radiogroup")
                                {
                                    var questionName = surveyElement["name"].ToString();
                                    var questionTitle = surveyElement["title"].ToString();
                                    var choices = (JArray)surveyElement["choices"];
                                    List<string> elementChoiceTexts = choices.Select(c => c["text"].ToString()).ToList();
                                    if ((!elementChoiceTexts.SequenceEqual(specificChoices)))
                                    {
                                        var choiceCounts = choices.ToDictionary(
                                            c => c["name"].ToString(),
                                            c =>
                                            {
                                                dynamic choice = new ExpandoObject();
                                                choice.ChoiceName = c["name"].ToString();
                                                choice.ChoiceText = c["text"].ToString();
                                                choice.Count = 0;
                                                choice.Percentage = 0.0;
                                                return choice;
                                            }
                                        );

                                        int totalResponses = 0;
                                        foreach (JObject answerPage in answerPages)
                                        {
                                            var answerElements = (JArray)answerPage["elements"];
                                            foreach (JObject answerElement in answerElements)
                                            {
                                                if (answerElement["name"].ToString() == questionName)
                                                {
                                                    var responseObject = answerElement["response"];
                                                    if (responseObject != null)
                                                    {
                                                        string responseName = responseObject["name"]?.ToString();
                                                        string responseText = responseObject["text"]?.ToString();

                                                        if (!string.IsNullOrEmpty(responseName) && choiceCounts.ContainsKey(responseName))
                                                        {
                                                            choiceCounts[responseName].Count++;
                                                            totalResponses++;
                                                        }
                                                        else if (!string.IsNullOrEmpty(responseText))
                                                        {
                                                            var matchingChoice = choiceCounts.Values.FirstOrDefault(c => c.ChoiceText == responseText);
                                                            if (matchingChoice != null)
                                                            {
                                                                matchingChoice.Count++;
                                                                totalResponses++;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        foreach (var choice in choiceCounts.Values)
                                        {
                                            choice.Percentage = totalResponses > 0 ? (double)choice.Count / totalResponses * 100 : 0;
                                        }

                                        if (questionDataDict.ContainsKey(questionName))
                                        {
                                            var existingQuestionData = questionDataDict[questionName];
                                            existingQuestionData.TotalResponses += totalResponses;

                                            foreach (var existingChoice in existingQuestionData.Choices)
                                            {
                                                var matchingNewChoice = choiceCounts.Values.FirstOrDefault(c => c.ChoiceName == existingChoice.ChoiceName);
                                                if (matchingNewChoice != null)
                                                {
                                                    existingChoice.Count += matchingNewChoice.Count;
                                                    existingChoice.Percentage = existingQuestionData.TotalResponses > 0 ? (double)existingChoice.Count / existingQuestionData.TotalResponses * 100 : 0;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            dynamic questionData = new ExpandoObject();
                                            questionData.QuestionName = questionName;
                                            questionData.QuestionTitle = questionTitle;
                                            questionData.TotalResponses = totalResponses;
                                            questionData.Choices = choiceCounts.Values.ToList();
                                            questionDataDict[questionName] = questionData;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    var questionDataList = questionDataDict.Values.Select(q => new
                    {
                        q.QuestionName,
                        q.QuestionTitle,
                        q.TotalResponses,
                        Choices = ((List<dynamic>)q.Choices).Select(c => new
                        {
                            c.ChoiceName,
                            c.ChoiceText,
                            c.Count,
                            c.Percentage
                        }).ToList()
                    }).ToList();

                    return Json(questionDataList, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { data = (object)null }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult Loadthongketanxuatnhieucauhoi(int surveyid = 0)
        {
            var user = SessionHelper.GetUser();
            var query = db.answer_response.AsQueryable();
            var survey = db.survey.Where(x => x.surveyID == surveyid).FirstOrDefault();
            if (surveyid != 0)
            {
                query = query.Where(x => x.surveyID == surveyid);
            }
            if (!string.IsNullOrEmpty(survey.key_class))
            {
                var keyClassList = new JavaScriptSerializer().Deserialize<List<string>>(survey.key_class);

                bool hasAnswerResponseForStudent = db.answer_response
                    .Any(aw => aw.id_sv != null
                    && (surveyid == 0 || aw.surveyID == surveyid)
                    && aw.id_ctdt == user.id_ctdt &&
                    keyClassList.Any(k => aw.survey.key_class.Contains(k))
                    && aw.json_answer != null);
                if (hasAnswerResponseForStudent)
                {
                    var responses = query
                    .Where(d => keyClassList.Any(k => d.sinhvien.lop.ma_lop.Contains(k)) && d.id_ctdt == user.id_ctdt && d.surveyID == surveyid)
                    .Select(x => new { JsonAnswer = x.json_answer, SurveyJson = x.survey.surveyData })
                    .ToList();

                    var questionDataDict = new Dictionary<string, dynamic>();

                    foreach (var response in responses)
                    {
                        var surveyDataObject = JObject.Parse(response.SurveyJson);
                        var answerDataObject = JObject.Parse(response.JsonAnswer);
                        var surveyPages = (JArray)surveyDataObject["pages"];
                        var answerPages = (JArray)answerDataObject["pages"];

                        foreach (JObject surveyPage in surveyPages)
                        {
                            var surveyElements = (JArray)surveyPage["elements"];

                            foreach (JObject surveyElement in surveyElements)
                            {
                                var type = surveyElement["type"].ToString();
                                if (type == "checkbox")
                                {
                                    var questionName = surveyElement["name"].ToString();
                                    var questionTitle = surveyElement["title"].ToString();
                                    var choices = (JArray)surveyElement["choices"];
                                    var choiceCounts = choices.ToDictionary(
                                        c => c["name"].ToString(),
                                        c =>
                                        {
                                            dynamic choice = new ExpandoObject();
                                            choice.ChoiceName = c["name"].ToString();
                                            choice.ChoiceText = c["text"].ToString();
                                            choice.Count = 0;
                                            choice.Percentage = 0.0;
                                            return choice;
                                        }
                                    );

                                    int totalResponses = 0;

                                    foreach (JObject answerPage in answerPages)
                                    {
                                        var answerElements = (JArray)answerPage["elements"];
                                        foreach (JObject answerElement in answerElements)
                                        {
                                            if (answerElement["name"].ToString() == questionName)
                                            {
                                                var responsesArray = (JArray)answerElement["response"]["name"];

                                                foreach (var responseName in responsesArray)
                                                {
                                                    var responseString = responseName.ToString();
                                                    if (choiceCounts.ContainsKey(responseString))
                                                    {
                                                        choiceCounts[responseString].Count++;
                                                        totalResponses++;
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    foreach (var choice in choiceCounts.Values)
                                    {
                                        choice.Percentage = totalResponses > 0 ? (double)choice.Count / totalResponses * 100 : 0;
                                    }

                                    if (questionDataDict.ContainsKey(questionName))
                                    {
                                        var existingQuestionData = questionDataDict[questionName];
                                        existingQuestionData.TotalResponses += totalResponses;

                                        foreach (var existingChoice in existingQuestionData.Choices)
                                        {
                                            var matchingNewChoice = choiceCounts.Values.FirstOrDefault(c => c.ChoiceName == existingChoice.ChoiceName);
                                            if (matchingNewChoice != null)
                                            {
                                                existingChoice.Count += matchingNewChoice.Count;
                                                existingChoice.Percentage = existingQuestionData.TotalResponses > 0 ? (double)existingChoice.Count / existingQuestionData.TotalResponses * 100 : 0;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        dynamic questionData = new ExpandoObject();
                                        questionData.QuestionName = questionName;
                                        questionData.QuestionTitle = questionTitle;
                                        questionData.TotalResponses = totalResponses;
                                        questionData.Choices = choiceCounts.Values.ToList();
                                        questionDataDict[questionName] = questionData;
                                    }
                                }
                            }
                        }
                    }

                    var questionDataList = questionDataDict.Values.Select(q => new
                    {
                        q.QuestionName,
                        q.QuestionTitle,
                        q.TotalResponses,
                        Choices = ((List<dynamic>)q.Choices).Select(c => new
                        {
                            c.ChoiceName,
                            c.ChoiceText,
                            c.Count,
                            c.Percentage
                        }).ToList()
                    }).ToList();

                    return Json(questionDataList, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                bool hasAnswerResponseForCTDT = db.answer_response.
                    Any(aw => aw.id_sv == null
                    && (surveyid == 0 || aw.surveyID == surveyid)
                    && aw.id_ctdt == user.id_ctdt
                    && aw.id_CBVC == null
                    && aw.json_answer != null);

                bool hasAnswerResponseForCBVC = db.answer_response.
                    Any(aw => aw.id_sv == null
                    && (surveyid == 0 || aw.surveyID == surveyid)
                    && aw.id_ctdt == user.id_ctdt
                    && aw.id_CBVC != null
                    && aw.json_answer != null);

                if (hasAnswerResponseForCTDT || hasAnswerResponseForCBVC)
                {
                    var responses = query
                    .Where(d => d.id_ctdt == user.id_ctdt && d.surveyID == surveyid)
                    .Select(x => new { JsonAnswer = x.json_answer, SurveyJson = x.survey.surveyData })
                    .ToList();

                    var questionDataDict = new Dictionary<string, dynamic>();

                    foreach (var response in responses)
                    {
                        var surveyDataObject = JObject.Parse(response.SurveyJson);
                        var answerDataObject = JObject.Parse(response.JsonAnswer);
                        var surveyPages = (JArray)surveyDataObject["pages"];
                        var answerPages = (JArray)answerDataObject["pages"];

                        foreach (JObject surveyPage in surveyPages)
                        {
                            var surveyElements = (JArray)surveyPage["elements"];

                            foreach (JObject surveyElement in surveyElements)
                            {
                                var type = surveyElement["type"].ToString();
                                if (type == "checkbox")
                                {
                                    var questionName = surveyElement["name"].ToString();
                                    var questionTitle = surveyElement["title"].ToString();
                                    var choices = (JArray)surveyElement["choices"];
                                    var choiceCounts = choices.ToDictionary(
                                        c => c["name"].ToString(),
                                        c =>
                                        {
                                            dynamic choice = new ExpandoObject();
                                            choice.ChoiceName = c["name"].ToString();
                                            choice.ChoiceText = c["text"].ToString();
                                            choice.Count = 0;
                                            choice.Percentage = 0.0;
                                            return choice;
                                        }
                                    );

                                    int totalResponses = 0;

                                    foreach (JObject answerPage in answerPages)
                                    {
                                        var answerElements = (JArray)answerPage["elements"];
                                        foreach (JObject answerElement in answerElements)
                                        {
                                            if (answerElement["name"].ToString() == questionName)
                                            {
                                                var responsesArray = (JArray)answerElement["response"]["name"];

                                                foreach (var responseName in responsesArray)
                                                {
                                                    var responseString = responseName.ToString();
                                                    if (choiceCounts.ContainsKey(responseString))
                                                    {
                                                        choiceCounts[responseString].Count++;
                                                        totalResponses++;
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    foreach (var choice in choiceCounts.Values)
                                    {
                                        choice.Percentage = totalResponses > 0 ? (double)choice.Count / totalResponses * 100 : 0;
                                    }

                                    if (questionDataDict.ContainsKey(questionName))
                                    {
                                        var existingQuestionData = questionDataDict[questionName];
                                        existingQuestionData.TotalResponses += totalResponses;

                                        foreach (var existingChoice in existingQuestionData.Choices)
                                        {
                                            var matchingNewChoice = choiceCounts.Values.FirstOrDefault(c => c.ChoiceName == existingChoice.ChoiceName);
                                            if (matchingNewChoice != null)
                                            {
                                                existingChoice.Count += matchingNewChoice.Count;
                                                existingChoice.Percentage = existingQuestionData.TotalResponses > 0 ? (double)existingChoice.Count / existingQuestionData.TotalResponses * 100 : 0;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        dynamic questionData = new ExpandoObject();
                                        questionData.QuestionName = questionName;
                                        questionData.QuestionTitle = questionTitle;
                                        questionData.TotalResponses = totalResponses;
                                        questionData.Choices = choiceCounts.Values.ToList();
                                        questionDataDict[questionName] = questionData;
                                    }
                                }
                            }
                        }
                    }

                    var questionDataList = questionDataDict.Values.Select(q => new
                    {
                        q.QuestionName,
                        q.QuestionTitle,
                        q.TotalResponses,
                        Choices = ((List<dynamic>)q.Choices).Select(c => new
                        {
                            c.ChoiceName,
                            c.ChoiceText,
                            c.Count,
                            c.Percentage
                        }).ToList()
                    }).ToList();

                    return Json(questionDataList, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { data = (object)null }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadthongketanxuatYkienkhac(int surveyid = 0)
        {
            var user = SessionHelper.GetUser();
            var query = db.answer_response.AsQueryable();
            var survey = db.survey.Where(x => x.surveyID == surveyid).FirstOrDefault();
            if (surveyid != 0)
            {
                query = query.Where(x => x.surveyID == surveyid);
            }
            if (!string.IsNullOrEmpty(survey.key_class))
            {
                var keyClassList = new JavaScriptSerializer().Deserialize<List<string>>(survey.key_class);
                bool hasAnswerResponseForStudent = db.answer_response.Any(aw => aw.id_sv != null && (surveyid == 0 || aw.surveyID == surveyid) && aw.id_ctdt == user.id_ctdt && aw.json_answer != null);
                if (hasAnswerResponseForStudent)
                {
                    var responses = query
                    .Where(d => keyClassList.Any(k => d.sinhvien.lop.ma_lop.Contains(k)) && d.id_ctdt == user.id_ctdt && d.surveyID == surveyid)
                    .Select(x => new { JsonAnswer = x.json_answer, SurveyJson = x.survey.surveyData })
                    .ToList();

                    var questionDataList = new List<dynamic>();

                    foreach (var response in responses)
                    {
                        var surveyDataObject = JObject.Parse(response.SurveyJson);
                        var answerDataObject = JObject.Parse(response.JsonAnswer);
                        var surveyPages = (JArray)surveyDataObject["pages"];
                        var answerPages = (JArray)answerDataObject["pages"];

                        foreach (JObject surveyPage in surveyPages)
                        {
                            var surveyElements = (JArray)surveyPage["elements"];

                            foreach (JObject surveyElement in surveyElements)
                            {
                                var type = surveyElement["type"].ToString();
                                if (type == "comment")
                                {
                                    var questionName = surveyElement["name"].ToString();
                                    var questionTitle = surveyElement["title"].ToString();
                                    var responsesList = new List<string>();

                                    foreach (JObject answerPage in answerPages)
                                    {
                                        var answerElements = (JArray)answerPage["elements"];

                                        foreach (JObject answerElement in answerElements)
                                        {
                                            if (answerElement["name"].ToString() == questionName)
                                            {
                                                var responseText = answerElement["response"]["text"].ToString();
                                                if (!string.IsNullOrEmpty(responseText))
                                                {
                                                    responsesList.Add(responseText);
                                                }
                                            }
                                        }
                                    }

                                    var existingQuestion = questionDataList.FirstOrDefault(q => q.QuestionName == questionName);

                                    if (existingQuestion != null)
                                    {
                                        existingQuestion.Responses.AddRange(responsesList);
                                    }
                                    else
                                    {
                                        var questionData = new
                                        {
                                            QuestionName = questionName,
                                            QuestionTitle = questionTitle,
                                            Responses = responsesList
                                        };

                                        questionDataList.Add(questionData);
                                    }
                                }
                            }
                        }
                    }

                    return Json(questionDataList, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                bool hasAnswerResponseForCTDT = db.answer_response
                    .Any(aw => aw.id_sv == null
                    && (surveyid == 0 || aw.surveyID == surveyid)
                    && aw.id_ctdt == user.id_ctdt
                    && aw.id_CBVC == null
                    && aw.json_answer != null);
                bool hasAnswerResponseForCBVC = db.answer_response
                    .Any(aw => aw.id_sv == null
                    && (surveyid == 0 || aw.surveyID == surveyid)
                    && aw.id_ctdt == user.id_ctdt
                    && aw.id_CBVC != null
                    && aw.json_answer != null);
                if (hasAnswerResponseForCTDT || hasAnswerResponseForCBVC)
                {
                    var responses = query
                    .Where(d => d.id_ctdt == user.id_ctdt && d.surveyID == surveyid)
                    .Select(x => new { JsonAnswer = x.json_answer, SurveyJson = x.survey.surveyData })
                    .ToList();

                    var questionDataList = new List<dynamic>();

                    foreach (var response in responses)
                    {
                        var surveyDataObject = JObject.Parse(response.SurveyJson);
                        var answerDataObject = JObject.Parse(response.JsonAnswer);
                        var surveyPages = (JArray)surveyDataObject["pages"];
                        var answerPages = (JArray)answerDataObject["pages"];

                        foreach (JObject surveyPage in surveyPages)
                        {
                            var surveyElements = (JArray)surveyPage["elements"];

                            foreach (JObject surveyElement in surveyElements)
                            {
                                var type = surveyElement["type"].ToString();
                                if (type == "comment")
                                {
                                    var questionName = surveyElement["name"].ToString();
                                    var questionTitle = surveyElement["title"].ToString();
                                    var responsesList = new List<string>();

                                    foreach (JObject answerPage in answerPages)
                                    {
                                        var answerElements = (JArray)answerPage["elements"];

                                        foreach (JObject answerElement in answerElements)
                                        {
                                            if (answerElement["name"].ToString() == questionName)
                                            {
                                                var responseText = answerElement["response"]["text"].ToString();
                                                if (!string.IsNullOrEmpty(responseText))
                                                {
                                                    responsesList.Add(responseText);
                                                }
                                            }
                                        }
                                    }

                                    var existingQuestion = questionDataList.FirstOrDefault(q => q.QuestionName == questionName);

                                    if (existingQuestion != null)
                                    {
                                        existingQuestion.Responses.AddRange(responsesList);
                                    }
                                    else
                                    {
                                        var questionData = new
                                        {
                                            QuestionName = questionName,
                                            QuestionTitle = questionTitle,
                                            Responses = responsesList
                                        };

                                        questionDataList.Add(questionData);
                                    }
                                }
                            }
                        }
                    }

                    return Json(questionDataList, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { data = (object)null }, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> LoadThongKeTyLeKhaoSat(int? surveyid)
        {
            var user = SessionHelper.GetUser();
            var DataList = new List<dynamic>();
            var NameCTDT = user.ctdt.ten_ctdt;

            if (!surveyid.HasValue)
            {
                return Json(new { message = "Không thể tìm thấy dữ liệu" }, JsonRequestBehavior.AllowGet);
            }

            var survey = await db.survey.FirstOrDefaultAsync(x => x.surveyID == surveyid);

            if (survey == null)
            {
                return Json(new { message = "Không thể tìm thấy dữ liệu" }, JsonRequestBehavior.AllowGet);
            }

            if (!string.IsNullOrEmpty(survey.key_class))
            {
                var keyClassList = new JavaScriptSerializer().Deserialize<List<string>>(survey.key_class);

                bool isStudent = await db.answer_response
                    .AnyAsync(aw => aw.id_sv != null
                        && aw.surveyID == surveyid
                        && aw.id_ctdt == user.id_ctdt
                        && keyClassList.Any(k => aw.survey.key_class.Contains(k))
                        && aw.json_answer != null);

                if (isStudent)
                {
                    var TotalAll = await db.sinhvien
                        .Where(x => keyClassList.Any(k => x.lop.ma_lop.Contains(k))
                                    && x.lop.ctdt.id_ctdt == user.id_ctdt)
                        .LongCountAsync();

                    var TotalIsKhaoSat = await db.sinhvien
                        .Where(x => keyClassList.Any(k => x.lop.ma_lop.Contains(k))
                                    && x.lop.ctdt.id_ctdt == user.id_ctdt
                                    && db.answer_response.Any(aw => aw.id_sv == x.id_sv
                                                                     && aw.surveyID == surveyid
                                                                     && aw.json_answer != null))
                        .LongCountAsync();

                    double percentage = TotalAll > 0 ? Math.Round((double)TotalIsKhaoSat / TotalAll * 100, 2) : 0;

                    var DataStudent = new
                    {
                        TenCTDT = NameCTDT,
                        TongKhaoSat = TotalAll,
                        TongPhieuDaTraLoi = TotalIsKhaoSat,
                        TongPhieuChuaTraLoi = TotalAll - TotalIsKhaoSat,
                        TyLeDaTraLoi = percentage,
                        TyLeChuaTraLoi = Math.Round(100 - percentage, 2),
                    };

                    DataList.Add(DataStudent);
                    return Json(new { data = DataList, isStudent = true }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                bool isCTDT = await db.answer_response
                    .AnyAsync(aw => aw.id_sv == null
                        && aw.surveyID == surveyid
                        && aw.id_ctdt == user.id_ctdt
                        && aw.id_CBVC == null
                        && aw.json_answer != null);

                bool isCBVC = await db.answer_response
                    .AnyAsync(aw => aw.id_sv == null
                        && aw.surveyID == surveyid
                        && aw.id_ctdt == user.id_ctdt
                        && aw.id_CBVC != null
                        && aw.json_answer != null);

                if (isCTDT)
                {
                    var TotalAll = await db.answer_response
                        .Where(x => x.id_ctdt == user.id_ctdt && x.id_sv == null)
                        .LongCountAsync();

                    var DataCTDT = new
                    {
                        TenCTDT = NameCTDT,
                        TongKhaoSat = TotalAll,
                        TongPhieuDaTraLoi = TotalAll,
                        TongPhieuChuaTraLoi = 0,
                        TyLeDaTraLoi = 100,
                        TyLeChuaTraLoi = 0
                    };

                    DataList.Add(DataCTDT);
                    return Json(new { data = DataList, isCTDT = true }, JsonRequestBehavior.AllowGet);
                }
                else if (isCBVC)
                {
                    var TotalAll = await db.CanBoVienChuc
                        .Where(x => x.id_chuongtrinhdaotao == user.id_ctdt)
                        .LongCountAsync();

                    var DataCBVC = new
                    {
                        TenCTDT = NameCTDT,
                        TongKhaoSat = TotalAll,
                        TongPhieuDaTraLoi = TotalAll,
                        TongPhieuChuaTraLoi = 0,
                        TyLeDaTraLoi = 100,
                        TyLeChuaTraLoi = 0
                    };

                    DataList.Add(DataCBVC);
                    return Json(new { data = DataList, isCBVC = true }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { message = "Không thể tìm thấy dữ liệu" }, JsonRequestBehavior.AllowGet);
        }

        #endregion
        #region Format Excel
        private ActionResult ExportDataToExcel<T>(List<T> data, string title, string sheetName, string surveyName, Dictionary<string, string> columnTitles)
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
                    worksheet.Cells["A3"].Value = "Thời gian xuất kết quả : " + TimeExport;
                    worksheet.Cells["A3"].Style.Font.Bold = true;
                    worksheet.Cells["A3"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;


                    for (int i = 0; i < properties.Length; i++)
                    {
                        var columnName = properties[i].Name;
                        var columnTitle = columnTitles.ContainsKey(columnName) ? columnTitles[columnName] : columnName;
                        worksheet.Cells[4, i + 1].Value = columnTitle;
                    }

                    int row = 5;
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
        #endregion
    }
}