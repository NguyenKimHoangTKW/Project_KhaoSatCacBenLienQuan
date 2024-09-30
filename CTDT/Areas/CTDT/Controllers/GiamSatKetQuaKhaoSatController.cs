using CTDT.Helper;
using CTDT.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

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
        // ChartsTyLeThamGiaKhaoSat Full Phiếu
        public JsonResult ChartsTyLeThamGiaKhaoSat(int year = 0)
        {
            var user = SessionHelper.GetUser();
            var query = db.answer_response
                .Where(x => x.survey.id_hedaotao == user.id_hdt)
                .GroupBy(x => x.survey.surveyID)
                .Select(g => g.FirstOrDefault());
            var DataList = new List<dynamic>();
            var MucDoHaiLong = new List<dynamic>();

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
            foreach (var survey in GetAllSurvey)
            {
                bool isStudent = db.answer_response
                    .Any(x => x.id_sv != null
                    && x.surveyID == survey.IDSurvey
                    && x.id_ctdt == user.id_ctdt
                    && x.id_hk == null
                    && x.id_CBVC == null
                    && x.id_mh == null
                    && x.json_answer != null);

                bool isCTDT = db.answer_response
                    .Any(aw => aw.id_sv == null
                    && aw.surveyID == survey.IDSurvey
                    && aw.id_ctdt == user.id_ctdt
                    && aw.json_answer != null);

                bool isStudentBySubject = db.answer_response
                    .Any(x => x.id_sv != null
                    && x.surveyID == survey.IDSurvey
                    && x.id_ctdt == user.id_ctdt
                    && x.id_hk != null
                    && x.id_CBVC != null
                    && x.id_mh != null
                    && x.json_answer != null);
                var idphieu = db.survey.Where(x => x.surveyID == survey.IDSurvey).FirstOrDefault();
                if (!string.IsNullOrEmpty(idphieu.key_class))
                {
                    var keyClassList = JsonConvert.DeserializeObject<List<string>>(idphieu.key_class);

                    if (isStudent)
                    {
                        var sinhvienQuery = db.sinhvien
                            .Where(x => keyClassList.Any(k => x.lop.ma_lop.Contains(k)) && x.lop.ctdt.id_ctdt == user.id_ctdt);

                        var TotalAll = sinhvienQuery.LongCount();

                        var TotalIsKhaoSat = db.answer_response
                            .Where(aw => aw.surveyID == survey.IDSurvey
                                      && aw.id_ctdt == user.id_ctdt
                                      && aw.id_hk == null
                                      && aw.id_CBVC == null
                                      && aw.id_mh == null
                                      && aw.json_answer != null
                                      && sinhvienQuery.Any(sv => sv.id_sv == aw.id_sv))
                            .LongCount();
                        double percentage = Math.Round(((double)TotalIsKhaoSat / TotalAll) * 100, 2);

                        var DataStudent = new
                        {
                            IDPhieu = idphieu.surveyID,
                            TongKhaoSat = TotalAll,
                            TongPhieuDaTraLoi = TotalIsKhaoSat,
                            TongPhieuChuaTraLoi = TotalAll - TotalIsKhaoSat,
                            TyLeDaTraLoi = percentage,
                            TyLeChuaTraLoi = Math.Round(100 - percentage, 2),
                            isStudent = true
                        };
                        DataList.Add(DataStudent);
                    }
                    else if (isStudentBySubject)
                    {
                        var sinhvienQuery = db.sinhvien
                            .Where(x => keyClassList.Any(k => x.lop.ma_lop.Contains(k)) && x.lop.ctdt.id_ctdt == user.id_ctdt);

                        var TotalAll = sinhvienQuery.LongCount();

                        var TotalIsKhaoSat = db.answer_response
                            .Where(aw => aw.surveyID == survey.IDSurvey
                                      && aw.id_ctdt == user.id_ctdt
                                      && aw.id_hk != null
                                      && aw.id_CBVC != null
                                      && aw.id_mh != null
                                      && aw.json_answer != null
                                      && sinhvienQuery.Any(sv => sv.id_sv == aw.id_sv))
                            .LongCount();
                        double percentage = Math.Round(((double)TotalIsKhaoSat / TotalAll) * 100, 2);

                        var DataStudent = new
                        {
                            IDPhieu = idphieu.surveyID,
                            TongKhaoSat = TotalAll,
                            TongPhieuDaTraLoi = TotalIsKhaoSat,
                            TongPhieuChuaTraLoi = TotalAll - TotalIsKhaoSat,
                            TyLeDaTraLoi = percentage,
                            TyLeChuaTraLoi = Math.Round(100 - percentage, 2),
                            isStudentBySubject = true
                        };
                        DataList.Add(DataStudent);
                    }
                    var Mucdohailong = db.answer_response.Where(d => keyClassList.Any(k => d.sinhvien.lop.ma_lop.Contains(k)) && d.id_ctdt == user.id_ctdt && d.surveyID == survey.IDSurvey).AsQueryable();
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
                else
                {
                    if (isCTDT)
                    {
                        var ctdt = db.answer_response
                            .Where(x => x.id_ctdt == user.id_ctdt && x.id_sv == null)
                            .AsQueryable();
                        var TotalAll = ctdt.Count();
                        var DataCTDT = new
                        {
                            IDPhieu = idphieu.surveyID,
                            TongKhaoSat = TotalAll,
                            TongPhieuDaTraLoi = TotalAll,
                            TongPhieuChuaTraLoi = 0,
                            TyLeDaTraLoi = 100,
                            TyLeChuaTraLoi = 0,
                            isCTDT = true
                        };
                        DataList.Add(DataCTDT);
                    }
                    var Mucdohailong = db.answer_response.Where(d => d.id_ctdt == user.id_ctdt && d.surveyID == survey.IDSurvey).AsQueryable();
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
            }

            var Alldata = new
            {
                TitleSurvey = GetAllSurvey,
                SurveyParticipationRate = DataList,
                SatisfactionLevel = MucDoHaiLong
            };

            return Json(new { data = Alldata, message = "Load dữ liệu thành công" }, JsonRequestBehavior.AllowGet);
        }
        // ChartsTyLeThamGiaKhaoSat Thông tư 01
        public JsonResult ChartsTyLeThamGiaKhaoSatThongTu01(int year = 0)
        {
            var user = SessionHelper.GetUser();
            var query = db.answer_response
                .Where(x => x.survey.id_hedaotao == user.id_hdt)
                .GroupBy(x => x.survey.surveyID)
                .Select(g => g.FirstOrDefault());
            var DataList = new List<dynamic>();
            var MucDoHaiLong = new List<dynamic>();

            if (year != 0)
            {
                query = query.Where(x => x.id_namhoc == year);
            }

            var GetAllSurvey = query
             .Where(x => (x.survey.surveyTitle.Contains("7") || x.survey.surveyTitle.Contains("8")))
             .Select(x => new
             {
                 IDSurvey = x.surveyID,
                 NameSurvey = x.survey.surveyTitle,
                 HocKy = x.hoc_ky != null ? x.hoc_ky.ten_hk : null
             }).ToList();

            foreach (var survey in GetAllSurvey)
            {
                var idphieu = db.survey.Where(x => x.surveyID == survey.IDSurvey).FirstOrDefault();
                if (!string.IsNullOrEmpty(idphieu.key_class))
                {
                    var keyClassList = new JavaScriptSerializer().Deserialize<List<string>>(idphieu.key_class);
                    bool isStudent = db.answer_response
                       .Any(x => x.id_sv != null
                       && x.surveyID == survey.IDSurvey
                       && x.id_ctdt == user.id_ctdt
                       && x.id_hk == null
                       && x.id_CBVC == null
                       && x.id_mh == null
                       && x.json_answer != null);
                   bool isStudentBySubject = db.answer_response
                    .Any(x => x.id_sv != null
                    && x.surveyID == survey.IDSurvey
                    && x.id_ctdt == user.id_ctdt
                    && x.id_hk != null
                    && x.id_CBVC != null
                    && x.id_mh != null
                    && x.json_answer != null);
                    if (isStudent)
                    {
                        var sinhvienQuery = db.sinhvien
                            .Where(x => keyClassList.Any(k => x.lop.ma_lop.Contains(k)) && x.lop.ctdt.id_ctdt == user.id_ctdt);
                        var TotalAll = sinhvienQuery.LongCount();
                        var TotalIsKhaoSat = db.answer_response
                            .Where(aw => aw.surveyID == survey.IDSurvey
                                      && aw.id_ctdt == user.id_ctdt
                                      && aw.id_hk == null
                                      && aw.id_CBVC == null
                                      && aw.id_mh == null
                                      && aw.json_answer != null
                                      && sinhvienQuery.Any(sv => sv.id_sv == aw.id_sv))
                            .LongCount();

                        double percentage = Math.Round(((double)TotalIsKhaoSat / TotalAll) * 100, 2);


                        var DataStudent = new
                        {
                            IDPhieu = idphieu.surveyID,
                            TongKhaoSat = TotalAll,
                            TongPhieuDaTraLoi = TotalIsKhaoSat,
                            TongPhieuChuaTraLoi = (TotalAll - TotalIsKhaoSat),
                            TyLeDaTraLoi = percentage,
                            TyLeChuaTraLoi = Math.Round(((double)100 - percentage), 2),
                            isStudent = true
                        };
                        DataList.Add(DataStudent);
                    }
                    else if(isStudentBySubject)
                    {
                        var sinhvienQuery = db.sinhvien
                            .Where(x => keyClassList.Any(k => x.lop.ma_lop.Contains(k)) && x.lop.ctdt.id_ctdt == user.id_ctdt);
                        var TotalAll = sinhvienQuery.LongCount();
                        var TotalIsKhaoSat = db.answer_response
                            .Where(aw => aw.surveyID == survey.IDSurvey
                                      && aw.id_ctdt == user.id_ctdt
                                      && aw.id_hk != null
                                      && aw.id_CBVC != null
                                      && aw.id_mh != null
                                      && aw.json_answer != null
                                      && sinhvienQuery.Any(sv => sv.id_sv == aw.id_sv))
                            .LongCount();

                        double percentage = Math.Round(((double)TotalIsKhaoSat / TotalAll) * 100, 2);


                        var DataStudent = new
                        {
                            IDPhieu = idphieu.surveyID,
                            TongKhaoSat = TotalAll,
                            TongPhieuDaTraLoi = TotalIsKhaoSat,
                            TongPhieuChuaTraLoi = (TotalAll - TotalIsKhaoSat),
                            TyLeDaTraLoi = percentage,
                            TyLeChuaTraLoi = Math.Round(((double)100 - percentage), 2),
                            isStudentBySubject = true
                        };
                        DataList.Add(DataStudent);
                    }
                    var Mucdohailong = db.answer_response.Where(d => keyClassList.Any(k => d.sinhvien.lop.ma_lop.Contains(k)) && d.id_ctdt == user.id_ctdt && d.surveyID == survey.IDSurvey).AsQueryable();
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
            }

            var Alldata = new
            {
                TitleSurvey = GetAllSurvey,
                SurveyParticipationRate = DataList,
                SatisfactionLevel = MucDoHaiLong
            };

            return Json(new { data = Alldata, message = "Load dữ liệu thành công" }, JsonRequestBehavior.AllowGet);
        }
    }
}

