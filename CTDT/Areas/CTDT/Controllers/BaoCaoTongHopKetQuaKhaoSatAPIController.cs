using CTDT.Helper;
using CTDT.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace CTDT.Areas.CTDT.Controllers
{
    public class BaoCaoTongHopKetQuaKhaoSatAPIController : ApiController
    {
        dbSurveyEntities db = new dbSurveyEntities();
        [HttpPost]
        [Route("api/bao_cao_tong_hop")]
        public IHttpActionResult x_loadbaocaotonghop(NamHoc namHoc)
        {
            var user = SessionHelper.GetUser();
            var survey = db.answer_response
                .Where(x => x.NamHoc.ten_namhoc == namHoc.ten_namhoc && x.survey.id_hedaotao == user.id_hdt)
                .Select(x => new
                {
                    ma_phieu = x.surveyID,
                    ten_phieu = x.survey.surveyTitle,
                    hoc_ky = x.hoc_ky != null ? x.hoc_ky.ten_hk : null,
                })
                .Distinct()
                .ToList()
                .Select(x => new
                {
                    x.ma_phieu,
                    x.ten_phieu,
                    x.hoc_ky,
                    so_phieu = int.TryParse(Regex.Match(x.ten_phieu, @"\d+").Value, out var number) ? number : 0
                })
                .OrderBy(x => x.so_phieu)
                .ToList();
            var DataList = new List<dynamic>();
            var MucDoHaiLong = new List<dynamic>();
            foreach (var item in survey)
            {
                var CheckKey = db.survey.Where(x => x.surveyID == item.ma_phieu).FirstOrDefault();
                if (!string.IsNullOrEmpty(CheckKey.key_class))
                {
                    var keyClassList = new JavaScriptSerializer().Deserialize<List<string>>(CheckKey.key_class);
                    bool isStudent = new[] { 1, 2, 4, 6 }.Contains(CheckKey.id_loaikhaosat) && CheckKey.is_hocky == false;
                    bool isStudentBySubject = new[] { 1, 2, 4, 6 }.Contains(CheckKey.id_loaikhaosat) && CheckKey.is_hocky == true;
                    if (isStudent)
                    {

                        sinh_vien(DataList, user.id_ctdt, item.ma_phieu, keyClassList);
                    }
                    else if (isStudentBySubject)
                    {
                        sinh_vien_subject(DataList, user.id_ctdt, item.ma_phieu, keyClassList);

                    }
                    if (isStudent || isStudentBySubject)
                    {
                        muc_do_hai_long(MucDoHaiLong, user.id_ctdt, item.ma_phieu, keyClassList);
                    }
                }
                else
                {
                    bool isCTDT = new[] { 5 }.Contains(CheckKey.id_loaikhaosat);
                    bool isCBVC = new[] { 3, 8 }.Contains(CheckKey.id_loaikhaosat);
                    if (isCTDT)
                    {
                        chuong_trinh_dao_tao(DataList, user.id_ctdt, item.ma_phieu);
                    }
                    else if (isCBVC)
                    {
                        can_bo_vien_chuc(DataList, user.id_ctdt, item.ma_phieu);
                    }
                    if (isCTDT || isCBVC)
                    {
                        muc_do_hai_long(MucDoHaiLong, user.id_ctdt, item.ma_phieu);
                    }
                }
            }
            var GetData = new
            {
                Survey = survey,
                PercentageSurvey = DataList.Select(x => new
                {
                    ma_phieu = x.IDPhieu,
                    ty_le = x.TyLeDaTraLoi,
                }),
                SatisfactionLevel = MucDoHaiLong
            };
            return Ok(new { data = GetData });
        }
        private void can_bo_vien_chuc(dynamic DataList, int? idctdt, int? idsurvey)
        {
            var cbvc = db.CanBoVienChuc
                            .Where(x => x.id_chuongtrinhdaotao == idctdt)
                            .AsQueryable();
            var TotalAll = cbvc.Count();
            var idphieu = db.survey.Where(x => x.surveyID == idsurvey).FirstOrDefault();
            var DataCBVC = new
            {
                IDPhieu = idphieu.surveyID,
                TongKhaoSat = TotalAll,
                TongPhieuDaTraLoi = TotalAll,
                TongPhieuChuaTraLoi = 0,
                TyLeDaTraLoi = 100,
                TyLeChuaTraLoi = 0
            };
            DataList.Add(DataCBVC);
        }
        private void chuong_trinh_dao_tao(dynamic DataList, int? idctdt, int? idsurvey)
        {
            var ctdt = db.answer_response
                            .Where(x => x.id_ctdt == idctdt && x.id_sv == null)
                            .AsQueryable();
            var TotalAll = ctdt.Count();
            var idphieu = db.survey.Where(x => x.surveyID == idsurvey).FirstOrDefault();

            var DataCTDT = new
            {
                IDPhieu = idphieu.surveyID,
                TongKhaoSat = TotalAll,
                TongPhieuDaTraLoi = TotalAll,
                TongPhieuChuaTraLoi = 0,
                TyLeDaTraLoi = 100,
                TyLeChuaTraLoi = 0
            };
            DataList.Add(DataCTDT);
        }
        private void sinh_vien_subject(dynamic DataList, int? idctdt, int? idsurvey, List<string> keyClassList)
        {
            var sinhvien = db.sinhvien
                             .Where(x => keyClassList.Any(k => x.lop.ma_lop.Contains(k)) && x.lop.ctdt.id_ctdt == idctdt);
            var TotalAll = sinhvien.Count();
            var TotalIsKhaoSat = sinhvien.Count(sv => db.answer_response
                                .Any(aw => aw.id_sv == sv.id_sv
                                && aw.surveyID == idsurvey
                                && aw.id_ctdt == idctdt
                                && aw.id_hk != null
                                && aw.id_CBVC != null
                                && aw.id_mh != null
                                && aw.json_answer != null));
            double percentage = Math.Round(((double)TotalIsKhaoSat / TotalAll) * 100, 2);
            var idphieu = db.survey.Where(x => x.surveyID == idsurvey).FirstOrDefault();
            var DataStudent = new
            {
                IDPhieu = idphieu.surveyID,
                TongKhaoSat = TotalAll,
                TongPhieuDaTraLoi = TotalIsKhaoSat,
                TongPhieuChuaTraLoi = (TotalAll - TotalIsKhaoSat),
                TyLeDaTraLoi = percentage,
                TyLeChuaTraLoi = Math.Round(((double)100 - percentage), 2),
            };
            DataList.Add(DataStudent);
        }
        private void sinh_vien(dynamic DataList, int? idctdt, int? idsurvey, List<string> keyClassList)
        {
            var sinhvien = db.sinhvien
                             .Where(x => keyClassList.Any(k => x.lop.ma_lop.Contains(k)) && x.lop.ctdt.id_ctdt == idctdt);
            var TotalAll = sinhvien.Count();
            var TotalIsKhaoSat = sinhvien.Count(sv => db.answer_response
                                .Any(aw => aw.id_sv == sv.id_sv
                                && aw.surveyID == idsurvey
                                && aw.id_ctdt == idctdt
                                && aw.id_hk == null
                                && aw.id_CBVC == null
                                && aw.id_mh == null
                                && aw.json_answer != null));

            double percentage = Math.Round(((double)TotalIsKhaoSat / TotalAll) * 100, 2);
            var idphieu = db.survey.Where(x => x.surveyID == idsurvey).FirstOrDefault();

            var DataStudent = new
            {
                IDPhieu = idphieu.surveyID,
                TongKhaoSat = TotalAll,
                TongPhieuDaTraLoi = TotalIsKhaoSat,
                TongPhieuChuaTraLoi = (TotalAll - TotalIsKhaoSat),
                TyLeDaTraLoi = percentage,
                TyLeChuaTraLoi = Math.Round(((double)100 - percentage), 2),
            };
            DataList.Add(DataStudent);
        }
        private void muc_do_hai_long(dynamic MucDoHaiLong, int? idctdt, int? idsurvey, List<string> keyClassList = null)
        {
            var Mucdohailong = db.answer_response
                     .Where(d => d.id_ctdt == idctdt && d.surveyID == idsurvey)
                     .AsEnumerable()
                     .Where(d => keyClassList == null || keyClassList.Count == 0 || keyClassList.Any(k => d.sinhvien.lop.ma_lop.Contains(k)))
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
            var maphieu = db.survey.Where(x => x.surveyID == idsurvey).FirstOrDefault();
            var GetDataMucDoHaiLong = new
            {
                ma_phieu = maphieu.surveyID,
                muc_do_hai_long = totalMucDoHaiLong,
                count_phieu = totalPhieu,
                ty_le_hai_long = totalPhieu > 0 ? Math.Round(((double)totalMucDoHaiLong / totalPhieu), 2) : 0,
                avgscore = totalPhieu > 0 ? Math.Round(((double)totalAverageScore / totalPhieu), 2) : 0
            };
            MucDoHaiLong.Add(GetDataMucDoHaiLong);
        }
    }
}
