﻿using CTDT.Helper;
using CTDT.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web.Http;
using System.Web.Script.Serialization;
using Microsoft.Ajax.Utilities;
using CTDT.Models.Khoa;
using FirebaseAdmin.Auth;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Text.RegularExpressions;
namespace CTDT.Areas.CTDT.Controllers
{
    public class GiamSatKetQuaKhaoSatAPIController : ApiController
    {
        dbSurveyEntities db = new dbSurveyEntities();
        private users user;
        private bool is_user_hop_tac_doanh_nghiep;
        private bool is_user_ctdt;
        private bool is_user_khoa;
        public GiamSatKetQuaKhaoSatAPIController()
        {
            user = SessionHelper.GetUser();
            is_user_hop_tac_doanh_nghiep = new int?[] { 6 }.Contains(user.id_typeusers);
            is_user_ctdt = new int?[] { 3 }.Contains(user.id_typeusers);
            is_user_khoa = new int?[] { 5 }.Contains(user.id_typeusers);
        }
     
        #region Thống kê theo thông tư 01
        [HttpPost]
        [Route("api/giam_sat_ty_le_khao_sat_thong_tu_01")]
        public async Task<IHttpActionResult> load_charts_ty_le_01(FindChartsTyLeKhaoSat find)
        {
            var survey = await db.survey.Where(x => x.id_namhoc == find.id_nam_hoc 
            && x.id_hedaotao == user.id_hdt
            && (x.surveyTitle.Contains("7") || x.surveyTitle.Contains("8"))
            ).ToListAsync();
            var List_data = new List<dynamic>();
            foreach (var item_survey in survey)
            {
                var DataList = new List<dynamic>();
                bool isStudent = new[] { 1, 2, 4, 6 }.Contains(item_survey.id_loaikhaosat);
                bool isStudentBySubject = new[] { 1, 2, 4, 6 }.Contains(item_survey.id_loaikhaosat);
                bool isCTDT = new[] { 5 }.Contains(item_survey.id_loaikhaosat);
                bool isCBVC = new[] { 3, 8 }.Contains(item_survey.id_loaikhaosat);
                var json = new List<dynamic>();
                var answer_response = db.answer_response
                    .Where(x => x.surveyID == item_survey.surveyID && x.id_namhoc == find.id_nam_hoc && x.survey.id_hedaotao == user.id_hdt);
                answer_response = answer_response.Where(x => x.id_ctdt == user.id_ctdt);

                var query = await answer_response.OrderBy(x => x.surveyID).Distinct().ToListAsync();
                if (!string.IsNullOrEmpty(item_survey.surveyData))
                {
                    var keyClassList = new JavaScriptSerializer().Deserialize<List<string>>(item_survey.surveyData);

                    if (isStudent)
                    {
                        sinh_vien(DataList, find.id_ctdt, item_survey.surveyID, keyClassList);
                    }
                    else if (isStudentBySubject)
                    {
                        sinh_vien_subject(DataList, find.id_ctdt, item_survey.surveyID, keyClassList);
                    }
                }
                else
                {
                    if (isCTDT)
                    {
                        chuong_trinh_dao_tao(DataList, find.id_ctdt, item_survey.surveyID);
                    }
                    else if (isCBVC)
                    {
                        can_bo_vien_chuc(DataList, find.id_ctdt, item_survey.surveyID);
                    }

                }

                List_data.Add(new
                {
                    ten_phieu = item_survey.surveyTitle,
                    thong_ke_ty_le = DataList,
                });
            }
            if (List_data.Count > 0)
            {
                return Ok(new { data = List_data, is_data = List_data.Count > 0 });
            }
            else
            {
                return Ok(new { message = "No Data Avalible", is_data = false });
            }
        }
        #endregion
        #region Các hàm gọi đối tượng
        private void can_bo_vien_chuc(dynamic DataList, int? idctdt, int? idsurvey)
        {
            var cbvc = db.CanBoVienChuc.AsQueryable();

            if (user.id_ctdt != null)
            {
                cbvc = cbvc.Where(x => x.id_chuongtrinhdaotao == user.id_ctdt);
            }
            else if (user.id_khoa != null)
            {
                if (idctdt != 0)
                {
                    cbvc = cbvc.Where(x => x.ctdt.id_khoa == user.id_khoa && x.ctdt.id_ctdt == idctdt);
                }
                else
                {
                    cbvc = cbvc.Where(x => x.ctdt.id_khoa == user.id_khoa);
                }
            }

            var mucDoHaiLongData = new List<dynamic>();
            muc_do_hai_long(mucDoHaiLongData, idctdt, idsurvey, null, null);
            var DataCBVC = new
            {
                ty_le_da_tra_loi = cbvc.Count() > 0 ? 100 : 0,
            };

            DataList.Add(new
            {
                ty_le_tham_gia_khao_sat = DataCBVC,
                ty_le_hai_long = mucDoHaiLongData
            });
        }

        private void chuong_trinh_dao_tao(dynamic DataList, int? idctdt, int? idsurvey)
        {
            var ctdt = db.answer_response
                                .Where(x =>
                                x.surveyID == idsurvey &&
                                x.id_sv == null &&
                                x.id_mh == null &&
                                x.id_users != null &&
                                x.id_hk == null &&
                                x.id_CBVC == null);
            if (user.id_ctdt != null)
            {
                ctdt = ctdt.Where(x => x.id_ctdt == user.id_ctdt);
            }
            else if (user.id_khoa != null)
            {
                if (idctdt != 0)
                {
                    ctdt = ctdt.Where(x => x.ctdt.id_khoa == user.id_khoa && x.ctdt.id_ctdt == idctdt);
                }
                else
                {
                    ctdt = ctdt.Where(x => x.ctdt.id_khoa == user.id_khoa);
                }
            }
            var mucDoHaiLongData = new List<dynamic>();
            muc_do_hai_long(mucDoHaiLongData, idctdt, idsurvey, null, null);

            var DataCTDT = new
            {
                ty_le_da_tra_loi = ctdt.Count() > 0 ? 100 : 0,
            };
            DataList.Add(new
            {
                ty_le_tham_gia_khao_sat = DataCTDT,
                ty_le_hai_long = mucDoHaiLongData
            });
        }
        private void sinh_vien_subject(dynamic DataList, int? idctdt, int? idsurvey, List<string> keyClassList)
        {
            var hoc_ky_check = db.hoc_ky.ToList();
            foreach (var item in hoc_ky_check)
            {
                var sinhvienQuery = db.sinhvien
                   .Where(x => keyClassList.Any(k => x.lop.ma_lop.Contains(k)));
                var totalAnsweredQuery = db.answer_response
                    .Where(aw => aw.surveyID == idsurvey
                              && aw.id_hk == item.id_hk
                              && aw.id_CBVC != null
                              && aw.id_mh != null
                              && aw.json_answer != null
                              && sinhvienQuery.Any(sv => sv.id_sv == aw.id_sv));

                if (user.id_ctdt != null)
                {
                    sinhvienQuery = sinhvienQuery.Where(x => x.lop.id_ctdt == user.id_ctdt);
                    totalAnsweredQuery = totalAnsweredQuery.Where(x => x.id_ctdt == user.id_ctdt);
                }
                else if (user.id_khoa != null)
                {
                    if (idctdt != 0)
                    {
                        sinhvienQuery = sinhvienQuery.Where(x => x.lop.ctdt.id_khoa == user.id_khoa && x.lop.ctdt.id_ctdt == idctdt);
                        totalAnsweredQuery = totalAnsweredQuery.Where(x => x.ctdt.id_khoa == user.id_khoa && x.id_ctdt == idctdt);
                    }
                    else
                    {
                        sinhvienQuery = sinhvienQuery.Where(x => x.lop.ctdt.id_khoa == user.id_khoa);
                        totalAnsweredQuery = totalAnsweredQuery.Where(x => x.ctdt.id_khoa == user.id_khoa);
                    }
                }

                int totalStudents = sinhvienQuery.Count();
                int totalAnswered = totalAnsweredQuery.Count();
                double percentage = Math.Round(((double)totalAnswered / totalStudents) * 100, 2);
                var mucDoHaiLongData = new List<dynamic>();
                muc_do_hai_long(mucDoHaiLongData, idctdt, idsurvey, item.id_hk, keyClassList);
                var DataStudent = new
                {
                    ty_le_da_tra_loi = percentage
                };
                DataList.Add(new
                {
                    hoc_ky = item.ten_hk,
                    ty_le_tham_gia_khao_sat = DataStudent,
                    ty_le_hai_long = mucDoHaiLongData.Take(1).ToList()
                });
            }
        }
        private void sinh_vien(dynamic DataList, int? idctdt, int? idsurvey, List<string> keyClassList)
        {
            var sinhvienQuery = db.sinhvien
         .Where(x => keyClassList.Any(k => x.lop.ma_lop.Contains(k)));

            var totalAnsweredQuery = db.answer_response
                .Where(aw => aw.surveyID == idsurvey
                          && aw.id_hk == null
                          && aw.survey.id_hedaotao == user.id_hdt
                          && aw.id_CBVC == null
                          && aw.id_mh == null
                          && aw.json_answer != null
                          && sinhvienQuery.Any(sv => sv.id_sv == aw.id_sv));

            if (user.id_ctdt != null)
            {
                sinhvienQuery = sinhvienQuery.Where(x => x.lop.id_ctdt == user.id_ctdt);
                totalAnsweredQuery = totalAnsweredQuery.Where(x => x.id_ctdt == user.id_ctdt);
            }
            else if (user.id_khoa != null)
            {
                if (idctdt != 0)
                {
                    sinhvienQuery = sinhvienQuery.Where(x => x.lop.ctdt.id_khoa == user.id_khoa && x.lop.ctdt.id_ctdt == idctdt);
                    totalAnsweredQuery = totalAnsweredQuery.Where(x => x.ctdt.id_khoa == user.id_khoa && x.id_ctdt == idctdt);
                }
                else
                {
                    sinhvienQuery = sinhvienQuery.Where(x => x.lop.ctdt.id_khoa == user.id_khoa);
                    totalAnsweredQuery = totalAnsweredQuery.Where(x => x.ctdt.id_khoa == user.id_khoa);
                }
            }

            int totalStudents = sinhvienQuery.Count();
            int totalAnswered = totalAnsweredQuery.Count();
            double percentage = Math.Round(((double)totalAnswered / totalStudents) * 100, 2);
            var mucDoHaiLongData = new List<dynamic>();
            muc_do_hai_long(mucDoHaiLongData, idctdt, idsurvey, null, keyClassList);
            var DataStudent = new
            {
                ty_le_da_tra_loi = percentage,
            };
            DataList.Add(new
            {
                ty_le_tham_gia_khao_sat = DataStudent,
                ty_le_hai_long = mucDoHaiLongData
            });
        }
        private void muc_do_hai_long(dynamic MucDoHaiLong, int? idctdt, int? idsurvey, int? idhk, List<string> keyClassList = null)
        {
            var Mucdohailong = db.answer_response
                     .Where(d => d.surveyID == idsurvey)
                     .AsEnumerable()
                     .Where(d => keyClassList == null || keyClassList.Count == 0 || keyClassList.Any(k => d.sinhvien.lop.ma_lop.Contains(k)))
                     .AsQueryable();
            if (idhk != null)
            {
                Mucdohailong = Mucdohailong.Where(x => x.id_hk == idhk);
            }
            if (user.id_ctdt != null)
            {
                Mucdohailong = Mucdohailong.Where(x => x.id_ctdt == user.id_ctdt);
            }
            else if (user.id_khoa != null)
            {
                if (idctdt != 0)
                {
                    Mucdohailong = Mucdohailong.Where(x => x.ctdt.id_khoa == user.id_khoa && x.id_ctdt == idctdt);
                }
                else
                {
                    Mucdohailong = Mucdohailong.Where(x => x.ctdt.id_khoa == user.id_khoa);
                }
            }
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
            var GetDataMucDoHaiLong = new
            {
                avg_ty_le_hai_long = totalPhieu > 0 ? Math.Round(((double)totalMucDoHaiLong / totalPhieu), 2) : 0,
                avg_score = totalPhieu > 0 ? Math.Round(((double)totalAverageScore / totalPhieu), 2) : 0
            };
            MucDoHaiLong.Add(GetDataMucDoHaiLong);
        }
        #endregion
    }
}
