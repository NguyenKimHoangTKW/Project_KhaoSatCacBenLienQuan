using CTDT.Helper;
using CTDT.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
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

        public JsonResult load_bao_cao(int year = 0, int hedaotao = 0)
        {
            var user = SessionHelper.GetUser();
            var query = db.survey.AsQueryable();
            var list_data = new List<dynamic>();
            var ty_le_tham_gia_khao_sat = new List<dynamic>();
            var MucDoHaiLong = new List<dynamic>();
            if (year != 0)
            {
                query = query.Where(x => x.id_namhoc == year);
            }

            if (hedaotao != 0)
            {
                query = query.Where(x => x.id_hedaotao == hedaotao);
            }

            var get_survey = query.ToList();
            var sortedSurveys = get_survey
                                .OrderBy(s => s.surveyTitle.Split('.').FirstOrDefault())
                                .ThenBy(s => s.surveyTitle)
                                .ToList();
            foreach (var item in sortedSurveys)
            {
                var get_ctdt = db.ctdt
                    .Where(x => x.id_hdt == item.id_hedaotao)
                    .ToList();
                foreach (var tylethamgia in get_ctdt)
                {
                    bool isStudent = db.answer_response.Any(aw => aw.id_sv != null && aw.surveyID == item.surveyID && aw.id_ctdt == tylethamgia.id_ctdt && aw.json_answer != null);
                    bool isCTDT = db.answer_response.Any(aw => aw.id_sv == null && aw.surveyID == item.surveyID && aw.id_ctdt == tylethamgia.id_ctdt && aw.json_answer != null);
                    bool isCBVC = db.answer_response.Any(aw => aw.id_sv == null && aw.surveyID == item.surveyID && aw.id_ctdt == tylethamgia.id_ctdt && aw.id_CBVC != null && aw.json_answer != null);
                    if (isStudent)
                    {
                        var CheckKey = db.survey.Where(x => x.surveyID == item.surveyID).FirstOrDefault();
                        var keyClassList = new JavaScriptSerializer().Deserialize<List<string>>(CheckKey.key_class);
                        var sinhvien = db.sinhvien.Where(x => keyClassList.Any(k => x.lop.ma_lop.Contains(k)) && x.lop.ctdt.id_ctdt == tylethamgia.id_ctdt).AsQueryable();
                        var TotalAll = sinhvien.Count();
                        var TotalIsKhaoSat = sinhvien.Count(sv => db.answer_response
                                            .Any(aw => aw.id_sv == sv.id_sv
                                            && aw.surveyID == item.surveyID
                                            && aw.id_ctdt == tylethamgia.id_ctdt
                                            && aw.json_answer != null));
                        double percentage = Math.Round(((double)TotalIsKhaoSat / TotalAll) * 100, 2);
                        var DataStudent = new
                        {
                            ma_phieu = item.surveyID,
                            ma_ctdt = tylethamgia.id_ctdt,
                            ty_le_da_tra_loi = percentage,
                        };
                        ty_le_tham_gia_khao_sat.Add(DataStudent);                  
                    }
                    else if (isCTDT)
                    {
                        var ctdt = db.answer_response
                       .Where(x => x.id_ctdt == tylethamgia.id_ctdt && x.id_sv == null)
                       .AsQueryable();
                        var TotalAll = ctdt.Count();
                        var DataCTDT = new
                        {
                            ma_phieu = item.surveyID,
                            ma_ctdt = tylethamgia.id_ctdt,
                            ty_le_da_tra_loi = 100,
                        };
                        ty_le_tham_gia_khao_sat.Add(DataCTDT);
                    }
                    else if (isCBVC)
                    {
                        var cbvc = db.CanBoVienChuc
                            .Where(x => x.id_chuongtrinhdaotao == tylethamgia.id_ctdt)
                            .AsQueryable();
                        var TotalAll = cbvc.Count();
                        var TotalIsKhaoSat = cbvc.Count(cb => db.answer_response
                                            .Any(aw => aw.id_CBVC == cb.id_CBVC
                                            && aw.surveyID == item.surveyID
                                            && aw.id_ctdt == tylethamgia.id_ctdt
                                            && aw.json_answer != null));
                        double percentage = Math.Round(((double)TotalIsKhaoSat / TotalAll) * 100, 2);
                        var DataCBVC = new
                        {
                            ma_phieu = item.surveyID,
                            ma_ctdt = tylethamgia.id_ctdt,
                            ty_le_da_tra_loi = percentage,
                        };
                        ty_le_tham_gia_khao_sat.Add(DataCBVC);
                    }
                    var tylehailong = db.answer_response.AsQueryable();
                    if (isStudent)
                    {
                        var CheckKey = db.survey.Where(x => x.surveyID == item.surveyID).FirstOrDefault();
                        var keyClassList = new JavaScriptSerializer().Deserialize<List<string>>(CheckKey.key_class);
                        tylehailong = tylehailong.Where(d => keyClassList.Any(k => d.sinhvien.lop.ma_lop.Contains(k)) && d.id_ctdt == tylethamgia.id_ctdt && d.surveyID == item.surveyID);
                    }
                    else
                    {
                        tylehailong = tylehailong.Where(d => d.id_ctdt == tylethamgia.id_ctdt && d.surveyID == item.surveyID);
                    }
                    var responses = tylehailong
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
                        AverageScore = ((List<dynamic>)q.Choices).Sum(c =>
                        {
                            switch (c.ChoiceText)
                            {
                                case "Hoàn toàn không đồng ý": return c.Count * 1;
                                case "Không đồng ý": return c.Count * 2;
                                case "Bình thường": return c.Count * 3;
                                case "Đồng ý": return c.Count * 4;
                                case "Hoàn toàn đồng ý": return c.Count * 5;
                                default: return 0;
                            }
                        }) / (double)q.TotalResponses,
                        MucDoHaiLong = ((List<dynamic>)q.Choices).Where(c => c.ChoiceText == "Đồng ý" || c.ChoiceText == "Hoàn toàn đồng ý")
                            .Sum(c => (double)c.Percentage)
                    }).ToList();
                    var totalMucDoHaiLong = questionDataList.Sum(x => x.MucDoHaiLong);
                    var totalPhieu = questionDataList.Count();
                    var totalAverageScore = questionDataList.Sum(x => x.AverageScore);
                    var GetDataMucDoHaiLong = new
                    {
                        ma_phieu = item.surveyID,
                        ma_ctdt = tylethamgia.id_ctdt,
                        ty_le_hai_long = totalPhieu > 0 ? Math.Round(((double)totalMucDoHaiLong / totalPhieu), 2) : 0,
                        avgscore = totalPhieu > 0 ? Math.Round(((double)totalAverageScore / totalPhieu), 2) : 0
                    };
                    MucDoHaiLong.Add(GetDataMucDoHaiLong);

                }
                list_data.Add(get_ctdt.Select(x => new
                {
                    ma_phieu = item.surveyID,
                    ma_ctdt = x.id_ctdt,
                    ten_ctdt = x.ten_ctdt,
                }));
            }

            var get_data = new
            {
                phieu_khao_sat = sortedSurveys
                .Select(x => new
                {
                    ma_phieu = x.surveyID,
                    ten_phieu = x.surveyTitle,
                }),
                chuong_trinh_dao_tao = list_data,
                ty_le_tham_gia_khao_sat = ty_le_tham_gia_khao_sat,
                muc_do_hai_long = MucDoHaiLong
            };
            return Json(new { data = get_data, success = true }, JsonRequestBehavior.AllowGet);
        }
    }
}