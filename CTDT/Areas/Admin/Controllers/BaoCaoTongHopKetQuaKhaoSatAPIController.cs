﻿using CTDT.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace CTDT.Areas.Admin.Controllers
{
    public class BaoCaoTongHopKetQuaKhaoSatAPIController : ApiController
    {
        dbSurveyEntities db = new dbSurveyEntities();

        [HttpPost]
        [Route("api/admin/bao-cao-tong-hop-ket-qua-khao-sat")]
        public async Task<IHttpActionResult> bao_cao_tong_hop_ket_qua_khao_sat(GiamSatThongKeKetQua giamsat)
        {
            var query = await db.survey.Where(x => x.id_namhoc == giamsat.id_namhoc).ToListAsync();
            if (giamsat.id_hdt != null)
            {
                query = query.Where(x => x.id_hedaotao == giamsat.id_hdt).ToList();
            }
            var list_data = new List<dynamic>();
            List<object> get_muc_do_hai_long = new List<object>();
            var sortedSurveys = query.ToList()
                .OrderBy(s => s.surveyTitle.Split('.').FirstOrDefault())
                .ThenBy(s => s.surveyTitle)
                .ToList();
            var check_answer = await db.answer_response
                .Where(x => x.surveyID == giamsat.surveyID
                           && x.ctdt.id_hdt == giamsat.id_hdt)
                .ToListAsync();
            if (giamsat.id_ctdt != null)
            {
                check_answer = check_answer.Where(x => x.id_ctdt == giamsat.id_ctdt).ToList();
            }
            var get_data = check_answer
                .Select(x => new
                {
                    JsonAnswer = x.json_answer,
                    SurveyJson = x.survey.surveyData
                })
                .ToList();
            get_muc_do_hai_long = muc_do_hai_long(get_data);
            foreach (var items in sortedSurveys)
            {
                if (items.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu giảng viên")
                {
                    bool check_giang_vien = new[] { 3 }.Contains(items.id_loaikhaosat);
                    bool check_cbvc = new[] { 8 }.Contains(items.id_loaikhaosat);
                    if (check_cbvc)
                    {
                        var cbvc = await db.cbvc_khao_sat.Where(x => x.surveyID == items.surveyID).ToListAsync();
                        if (giamsat.id_ctdt != null)
                        {
                            cbvc = cbvc.Where(x => x.CanBoVienChuc.id_chuongtrinhdaotao == giamsat.id_ctdt).ToList();
                        }
                        var TotalAll = cbvc.Count();
                        var TotalDaKhaoSat = cbvc.Count(x => x.is_khao_sat == 1);

                        var idphieu = db.survey.Where(x => x.surveyID == items.surveyID).FirstOrDefault();
                        double percentage = TotalAll > 0 ? Math.Round(((double)TotalDaKhaoSat / TotalAll) * 100, 2) : 0;

                        var DataCBVC = new
                        {
                            id_phieu = items.surveyID,
                            ten_phieu = items.surveyTitle,
                            tong_khao_sat = TotalAll,
                            tong_phieu_da_tra_loi = TotalDaKhaoSat,
                            tong_phieu_chua_tra_loi = (TotalAll - TotalDaKhaoSat),
                            ty_le_da_tra_loi = percentage,
                            ty_le_chua_tra_loi = Math.Round(((double)100 - percentage), 2),
                        };
                        
                        list_data.Add(new
                        {
                            ty_le_tham_gia_khao_sat = DataCBVC,
                            muc_do_hai_long = get_muc_do_hai_long
                        });
                    }
                    else if (check_giang_vien)
                    {
                        var giang_vien = await db.answer_response.Where(x => x.surveyID == items.surveyID).ToListAsync();
                        if (giamsat.id_ctdt != null)
                        {
                            giang_vien = giang_vien.Where(x => x.id_ctdt == giamsat.id_ctdt).ToList();
                        }
                        var DataGiangVien = new
                        {
                            id_phieu = items.surveyID,
                            ten_phieu = items.surveyTitle,
                            tong_khao_sat = giang_vien.Count(),
                            tong_phieu_da_tra_loi = giang_vien.Count(),
                            tong_phieu_chua_tra_loi = 0,
                            ty_le_da_tra_loi = giang_vien.Count() > 0 ? 100 : 0,
                            ty_le_chua_tra_loi = 0
                        };
                        list_data.Add(new
                        {
                            ty_le_tham_gia_khao_sat = DataGiangVien,
                            muc_do_hai_long = get_muc_do_hai_long
                        });
                    }
                }
                else if (items.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu người học có học phần")
                {
                    bool hoc_vien_co_hoc_phan_ly_thuyet_dang_hoc_tai_truong = new[] { 11 }.Contains(items.id_loaikhaosat);
                    bool hoc_vien_co_hoc_phan_tot_nghiep = new[] { 13 }.Contains(items.id_loaikhaosat);
                    if (hoc_vien_co_hoc_phan_ly_thuyet_dang_hoc_tai_truong)
                    {
                        var check_hoc_phan = await db.nguoi_hoc_dang_co_hoc_phan.Where(x => x.surveyID == items.surveyID).ToListAsync();
                        if (giamsat .id_ctdt != null)
                        {
                            check_hoc_phan = check_hoc_phan.Where(x => x.sinhvien.lop.id_ctdt == giamsat.id_ctdt).ToList();
                        }
                        var total = check_hoc_phan.Count;
                        var TotalIsKhaoSat = check_hoc_phan.Where(x => x.da_khao_sat == 1).ToList().Count;
                        double percentage = total > 0 ? Math.Round(((double)TotalIsKhaoSat / total) * 100, 2) : 0;
                        var DataStudent = new
                        {
                            id_phieu = items.surveyID,
                            ten_phieu = items.surveyTitle,
                            tong_khao_sat = total,
                            tong_phieu_da_tra_loi = TotalIsKhaoSat,
                            tong_phieu_chua_tra_loi = (total - TotalIsKhaoSat),
                            ty_le_da_tra_loi = percentage,
                            ty_le_chua_tra_loi = Math.Round(((double)100 - percentage), 2),
                        };
                        list_data.Add(new
                        {
                            ty_le_tham_gia_khao_sat = DataStudent,
                            muc_do_hai_long = get_muc_do_hai_long
                        });
                    }
                }
            }
            if (list_data.Count > 0)
            {
                return Ok(new { data = list_data, success = true });
            }
            else
            {
                return Ok(new { message = "Chưa có dữ liệu khảo sát trong năm học để thống kê", success = false });
            }
        }

        private List<object> muc_do_hai_long(dynamic getdata)
        {
            var questionDataDict = new Dictionary<string, dynamic>();
            List<string> specificChoices = new List<string> {
        "Hoàn toàn không đồng ý",
        "Không đồng ý",
        "Bình thường",
        "Đồng ý",
        "Hoàn toàn đồng ý"
    };

            foreach (var response in getdata)
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

                            if (elementChoiceTexts.SequenceEqual(specificChoices))
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

            // Wrap results in a list
            var GetDataMucDoHaiLong = new List<object>
    {
        new
        {
            avg_ty_le_hai_long = totalPhieu > 0 ? Math.Round(totalMucDoHaiLong / totalPhieu, 2) : 0,
            avg_score = totalPhieu > 0 ? Math.Round(totalAverageScore / totalPhieu, 2) : 0
        }
    };

            return GetDataMucDoHaiLong;
        }

    }
}
