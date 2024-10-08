using CTDT.Helper;
using CTDT.Models;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;

namespace CTDT.Areas.CTDT.Controllers
{
    [UserAuthorize(3)]
    public class GiamSatKetQuaKhaoSatController : Controller
    {
        dbSurveyEntities db = new dbSurveyEntities();
        // GET: CTDT/GiamSatKetQuaKhaoSat
        public ActionResult Index()
        {
            ViewBag.Year = new SelectList(db.NamHoc.OrderByDescending(x => x.id_namhoc), "id_namhoc", "ten_namhoc");
            return View();
        }
        #region Thống kê toàn bộ phiếu
        public JsonResult load_charts_ty_le(int year = 0)
        {
            var user = SessionHelper.GetUser();
            var query = db.answer_response
                .Where(x => x.survey.id_hedaotao == user.id_hdt && x.id_namhoc == year)
                .DistinctBy(x => x.surveyID)
                .ToList();

            var DataList = new List<dynamic>();
            var MucDoHaiLong = new List<dynamic>();
            var Survey = new List<dynamic>();
            var Alldata = new List<dynamic>();
            foreach (var survey in query)
            {
                var idphieu = db.survey.Where(x => x.surveyID == survey.surveyID).FirstOrDefault();

                if (!string.IsNullOrEmpty(idphieu.key_class))
                {
                    var keyClassList = JsonConvert.DeserializeObject<List<string>>(idphieu.key_class);

                    bool isStudent = new[] { 1, 2, 4, 6 }.Contains(idphieu.id_loaikhaosat) && idphieu.is_hocky == false;
                    bool isStudentBySubject = new[] { 1, 2, 4, 6 }.Contains(idphieu.id_loaikhaosat) && idphieu.is_hocky == true;

                    if (isStudent)
                    {
                        sinh_vien(DataList, user.id_ctdt, survey.surveyID, keyClassList);
                        tan_xuat_cau_hoi_by_class(MucDoHaiLong, user.id_ctdt, survey.surveyID, keyClassList);
                    }
                    else if (isStudentBySubject)
                    {
                        sinh_vien_subject(DataList, user.id_ctdt, survey.surveyID, survey.id_hk,keyClassList);
                        tan_xuat_cau_hoi_by_class(MucDoHaiLong, user.id_ctdt, survey.surveyID, keyClassList);
                    }
                }
                else
                {
                    bool isCTDT = new[] { 5 }.Contains(idphieu.id_loaikhaosat);
                    bool isCBVC = new[] { 3, 8 }.Contains(idphieu.id_loaikhaosat);
                    if (isCTDT)
                    {
                        chuong_trinh_dao_tao(DataList, user.id_ctdt, survey.surveyID);
                        tan_xuat_cau_hoi_by_class(MucDoHaiLong, user.id_ctdt, survey.surveyID);
                    }
                }
                Survey.Add(new
                {
                    IDSurvey = survey.surveyID,
                    NameSurvey = survey.survey.surveyTitle,
                    HocKy = survey.hoc_ky != null ? survey.hoc_ky.ten_hk : null
                }); 
            }
            Alldata.Add(new
            {
                TitleSurvey = Survey,
                SurveyParticipationRate = DataList,
                SatisfactionLevel = MucDoHaiLong
            });
            return Json(new { data = Alldata, message = "Load dữ liệu thành công" }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Thống  kê theo thông tư 01
        public JsonResult load_charts_ty_le_01(int year = 0)
        {
            var user = SessionHelper.GetUser();
            var query = db.answer_response
                .Where(x => x.survey.id_hedaotao == user.id_hdt && x.id_namhoc == year && (x.survey.surveyTitle.Contains("7") || x.survey.surveyTitle.Contains("8")))
                .DistinctBy(x => x.surveyID)
                .ToList();
            var DataList = new List<dynamic>();
            var MucDoHaiLong = new List<dynamic>();
            var Survey = new List<dynamic>();
            var Alldata = new List<dynamic>();
            foreach (var survey in query)
            {
                var idphieu = db.survey.Where(x => x.surveyID == survey.surveyID).FirstOrDefault();
                if (!string.IsNullOrEmpty(idphieu.key_class))
                {
                    var keyClassList = new JavaScriptSerializer().Deserialize<List<string>>(idphieu.key_class);
                    bool isStudent = new[] { 1, 2, 4, 6 }.Contains(idphieu.id_loaikhaosat) && idphieu.is_hocky == false;
                    bool isStudentBySubject = new[] { 1, 2, 4, 6 }.Contains(idphieu.id_loaikhaosat) && idphieu.is_hocky == true;
                    if (isStudent)
                    {
                        sinh_vien(DataList, user.id_ctdt, survey.surveyID, keyClassList);
                        tan_xuat_cau_hoi_thong_tu_01(MucDoHaiLong, user.id_ctdt, survey.surveyID, keyClassList);
                    }
                    else if (isStudentBySubject)
                    {
                        sinh_vien_subject(DataList, user.id_ctdt, survey.surveyID,survey.id_hk, keyClassList);
                        tan_xuat_cau_hoi_thong_tu_01(MucDoHaiLong, user.id_ctdt, survey.surveyID, keyClassList);
                    }
                    Survey.Add(new
                    {
                        IDSurvey = survey.surveyID,
                        NameSurvey = survey.survey.surveyTitle,
                        HocKy = survey.hoc_ky != null ? survey.hoc_ky.ten_hk : null
                    });
                }
            }
            Alldata.Add(new
            {
                TitleSurvey = Survey,
                SurveyParticipationRate = DataList,
                SatisfactionLevel = MucDoHaiLong
            });
            return Json(new { data = Alldata, message = "Load dữ liệu thành công" }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Các hàm gọi đối tượng
        private void chuong_trinh_dao_tao(dynamic DataList, int? idctdt, int? idsurvey)
        {
            var ctdt = db.answer_response
                                .Where(x => x.id_ctdt == idctdt &&
                                            x.id_sv == null &&
                                            x.id_mh == null &&
                                            x.id_users != null &&
                                            x.id_hk == null &&
                                            x.id_CBVC == null)
                                .Count();
            var DataCTDT = new
            {
                IDPhieu = idsurvey,
                TongKhaoSat = ctdt,
                TongPhieuDaTraLoi = ctdt,
                TongPhieuChuaTraLoi = 0,
                TyLeDaTraLoi = 100,
                TyLeChuaTraLoi = 0,
                isCTDT = true
            };
            DataList.Add(DataCTDT);
        }
        private void sinh_vien_subject(dynamic DataList, int? idctdt, int? idsurvey, int? idhk,List<string> keyClassList)
        {
            var sinhvienQuery = db.sinhvien
                            .Where(x => keyClassList.Any(k => x.lop.ma_lop.Contains(k)) && x.lop.ctdt.id_ctdt == idctdt);

            var TotalAll = sinhvienQuery.LongCount();

            var TotalIsKhaoSat = db.answer_response
                .Where(aw => aw.surveyID == idsurvey
                          && aw.id_ctdt == idctdt
                          && aw.id_hk == idhk
                          && aw.id_CBVC != null
                          && aw.id_mh != null
                          && aw.json_answer != null
                          && sinhvienQuery.Any(sv => sv.id_sv == aw.id_sv))
                .LongCount();
            double percentage = Math.Round(((double)TotalIsKhaoSat / TotalAll) * 100, 2);

            var DataStudent = new
            {
                IDPhieu = idsurvey,
                TongKhaoSat = TotalAll,
                TongPhieuDaTraLoi = TotalIsKhaoSat,
                TongPhieuChuaTraLoi = TotalAll - TotalIsKhaoSat,
                TyLeDaTraLoi = percentage,
                TyLeChuaTraLoi = Math.Round(100 - percentage, 2),
                isStudentBySubject = true
            };
            DataList.Add(DataStudent);
        }
        private void sinh_vien(dynamic DataList, int? idctdt, int? idsurvey, List<string> keyClassList)
        {
            var sinhvienQuery = db.sinhvien
                            .Where(x => keyClassList.Any(k => x.lop.ma_lop.Contains(k)) && x.lop.ctdt.id_ctdt == idctdt);

            var TotalAll = sinhvienQuery.LongCount();

            var TotalIsKhaoSat = db.answer_response
                .Where(aw => aw.surveyID == idsurvey
                          && aw.id_ctdt == idctdt
                          && aw.id_hk == null
                          && aw.id_CBVC == null
                          && aw.id_mh == null
                          && aw.json_answer != null
                          && sinhvienQuery.Any(sv => sv.id_sv == aw.id_sv))
                .LongCount();
            double percentage = Math.Round(((double)TotalIsKhaoSat / TotalAll) * 100, 2);

            var DataStudent = new
            {
                IDPhieu = idsurvey,
                TongKhaoSat = TotalAll,
                TongPhieuDaTraLoi = TotalIsKhaoSat,
                TongPhieuChuaTraLoi = TotalAll - TotalIsKhaoSat,
                TyLeDaTraLoi = percentage,
                TyLeChuaTraLoi = Math.Round(100 - percentage, 2),
                isStudent = true
            };
            DataList.Add(DataStudent);
        }
        private void tan_xuat_cau_hoi_thong_tu_01(dynamic MucDoHaiLong, int? idctdt, int? idsurvey, List<string> keyClassList = null)
        {
            var Mucdohailong = db.answer_response
                 .Where(d => d.id_ctdt == idctdt && d.surveyID == idsurvey)
                 .AsEnumerable()
                 .Where(d => keyClassList == null || keyClassList.Any(k => d.sinhvien.lop.ma_lop.Contains(k)))
                 .AsQueryable();
            var responses = Mucdohailong
                .Select(x => new { IDPhieu = x.surveyID, JsonAnswer = x.json_answer, SurveyJson = x.survey.surveyData })
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
                            if ((elementChoiceTexts.SequenceEqual(specificChoices)))
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
            var lastQuestionData = questionDataDict.Values.LastOrDefault();

            if (lastQuestionData != null)
            {
                var questionDataList = new List<dynamic>
                    {
                        new
                        {
                            lastQuestionData.QuestionName,
                            lastQuestionData.QuestionTitle,
                            lastQuestionData.TotalResponses,
                            Choices = ((List<dynamic>)lastQuestionData.Choices).Select(c => new
                            {
                                c.ChoiceName,
                                c.ChoiceText,
                                c.Count,
                                c.Percentage
                            }).ToList(),
                            TotalAgreePercentage = ((List<dynamic>)lastQuestionData.Choices).Where(c => c.ChoiceText == "Đồng ý" || c.ChoiceText == "Hoàn toàn đồng ý")
                                .Sum(c => (double)c.Percentage)
                        }
                    };

                MucDoHaiLong.Add(questionDataList);
            }
        }
        private void tan_xuat_cau_hoi_by_class(dynamic MucDoHaiLong, int? idctdt, int? idsurvey, List<string> keyClassList = null)
        {
            var Mucdohailong = db.answer_response
                     .Where(d => d.id_ctdt == idctdt && d.surveyID == idsurvey)
                     .AsEnumerable()
                     .Where(d => keyClassList == null || keyClassList.Any(k => d.sinhvien.lop.ma_lop.Contains(k)))
                     .AsQueryable();

            var responses = Mucdohailong
                .Select(x => new { IDPhieu = x.surveyID, JsonAnswer = x.json_answer, SurveyJson = x.survey.surveyData })
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
                            if ((elementChoiceTexts.SequenceEqual(specificChoices)))
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
                }).ToList(),
                TotalAgreePercentage = ((List<dynamic>)q.Choices).Where(c => c.ChoiceText == "Đồng ý" || c.ChoiceText == "Hoàn toàn đồng ý")
                    .Sum(c => (double)c.Percentage)
            }).ToList();

            MucDoHaiLong.Add(questionDataList);
        }
        #endregion
    }
}

