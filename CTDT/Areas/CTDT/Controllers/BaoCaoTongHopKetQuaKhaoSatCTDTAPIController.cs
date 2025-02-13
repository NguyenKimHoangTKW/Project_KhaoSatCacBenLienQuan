﻿using CTDT.Helper;
using CTDT.Models;
using CTDT.Models.Khoa;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;

namespace CTDT.Areas.CTDT.Controllers
{
    public class BaoCaoTongHopKetQuaKhaoSatCTDTAPIController : ApiController
    {
        dbSurveyEntities db = new dbSurveyEntities();
        private users user;
        private bool is_user_hop_tac_doanh_nghiep;
        private bool is_user_ctdt;
        private bool is_user_khoa;
        public BaoCaoTongHopKetQuaKhaoSatCTDTAPIController()
        {
            user = SessionHelper.GetUser();
            is_user_hop_tac_doanh_nghiep = new int?[] { 6 }.Contains(user.id_typeusers);
            is_user_ctdt = new int?[] { 3 }.Contains(user.id_typeusers);
            is_user_khoa = new int?[] { 5 }.Contains(user.id_typeusers);
        }
        [HttpPost]
        [Route("api/ctdt/bao-cao-tong-hop-ket-qua-khao-sat")]
        public async Task<IHttpActionResult> bao_cao_tong_hop_ket_qua_khao_sat(GiamSatThongKeKetQua giamsat)
        {
            var ctdt = await db.ctdt.FirstOrDefaultAsync(x => x.id_ctdt == giamsat.id_ctdt);
            var query = await db.survey.Where(x => x.id_namhoc == giamsat.id_namhoc && x.mo_thong_ke == 1 && x.id_hedaotao == ctdt.id_hdt).ToListAsync();
            var list_data = new List<dynamic>();
            var sortedSurveys = query.ToList()
                .OrderBy(s => s.surveyTitle.Split('.').FirstOrDefault())
                .ThenBy(s => s.surveyTitle)
                .ToList();
            var get_time_check = new List<dynamic>();
            foreach (var items in sortedSurveys)
            {
                var list_ctdt = new List<dynamic>();
                var mucDoHaiLongData = new List<dynamic>();
                if (giamsat.from_date != null && giamsat.to_date != null)
                {
                    await load_ty_le_co_dau_thoi_gian(items, list_ctdt, mucDoHaiLongData, giamsat);
                }
                else
                {
                    await load_ty_le_khong_dau_thoi_gian(items, list_ctdt, mucDoHaiLongData, giamsat);
                }
                list_data.Add(new
                {
                    phieu = items.id_dot_khao_sat != null ? items.surveyTitle + " - " + items.dot_khao_sat.ten_dot_khao_sat : items.surveyTitle,
                    ty_le_tham_gia_khao_sat = list_ctdt,
                    ty_le_phan_tram_dat = items.ty_le_phan_tram_dat
                });
                await muc_do_hai_long(mucDoHaiLongData, items.surveyID, giamsat);
            }
            var get_time = await time_check(get_time_check, giamsat);
            if (list_data.Count > 0)
            {
                return Ok(new { data = JsonConvert.SerializeObject(list_data), time_check = JsonConvert.SerializeObject(get_time), ctdt = ctdt.ten_ctdt, success = true });
            }
            else
            {
                return Ok(new { message = "Chưa có dữ liệu khảo sát trong năm học để thống kê", success = false });
            }
        }
        private async Task<dynamic> time_check(dynamic get_time_check, GiamSatThongKeKetQua giamsat)
        {
            var check_nam = await db.NamHoc.FirstOrDefaultAsync(x => x.id_namhoc == giamsat.id_namhoc);
            if (giamsat.from_date != null && giamsat.to_date != null)
            {
                get_time_check.Add(new
                {
                    time_check_start = giamsat.from_date,
                    time_check_end = giamsat.to_date
                });
            }
            else
            {
                string startDateStr = $"01/01/{check_nam.ten_namhoc}";
                string endDateStr = $"30/12/{check_nam.ten_namhoc}";

                DateTime startDate = DateTime.ParseExact(startDateStr, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                DateTime endDate = DateTime.ParseExact(endDateStr, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                get_time_check.Add(new
                {
                    time_check_start = new DateTimeOffset(startDate).ToUnixTimeSeconds(),
                    time_check_end = new DateTimeOffset(endDate).ToUnixTimeSeconds()
                });
            }
            return get_time_check;
        }
        private async Task<dynamic> load_ty_le_khong_dau_thoi_gian(survey items, dynamic list_ctdt, dynamic mucDoHaiLongData, GiamSatThongKeKetQua giamsat)
        {
            if (items.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu giảng viên")
            {
                bool check_giang_vien = new[] { 3 }.Contains(items.id_loaikhaosat);
                bool check_cbvc = new[] { 8 }.Contains(items.id_loaikhaosat);
                if (check_cbvc)
                {
                    var cbvc = await db.cbvc_khao_sat.Where(x => x.surveyID == items.surveyID && x.CanBoVienChuc.id_chuongtrinhdaotao == giamsat.id_ctdt).ToListAsync();

                    var TotalAll = cbvc.Count();
                    var TotalDaKhaoSat = cbvc.Count(x => x.is_khao_sat == 1);
                    double percentage = TotalAll > 0 ? Math.Round(((double)TotalDaKhaoSat / TotalAll) * 100, 2) : 0;
                    double unpercentage = TotalAll > 0 ? Math.Round(((double)100 - percentage), 2) : 0;
                    list_ctdt.Add(new
                    {
                        ty_le_da_tra_loi = percentage,
                        ty_le_chua_tra_loi = unpercentage,
                        muc_do_hai_long = mucDoHaiLongData

                    });
                }
                else if (check_giang_vien)
                {
                    var giang_vien = await db.answer_response.Where(x => x.surveyID == items.surveyID && x.surveyID == items.surveyID && x.id_ctdt == giamsat.id_ctdt).ToListAsync();
                    list_ctdt.Add(new
                    {
                        ty_le_da_tra_loi = giang_vien.Count() > 0 ? 100 : 0,
                        ty_le_chua_tra_loi = 0,
                        muc_do_hai_long = mucDoHaiLongData
                    });
                }
            }
            else if (items.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu người học có học phần")
            {
                var check_hoc_phan = await db.nguoi_hoc_dang_co_hoc_phan.Where(x => x.surveyID == items.surveyID && x.sinhvien.lop.id_ctdt == giamsat.id_ctdt).ToListAsync();
                var total = check_hoc_phan.Count;
                var TotalIsKhaoSat = check_hoc_phan.Where(x => x.da_khao_sat == 1).ToList().Count;
                double percentage = total > 0 ? Math.Round(((double)TotalIsKhaoSat / total) * 100, 2) : 0;
                double unpercentage = total > 0 ? Math.Round(((double)100 - percentage), 2) : 0;
                list_ctdt.Add(new
                {
                    ty_le_da_tra_loi = percentage,
                    ty_le_chua_tra_loi = unpercentage,
                    muc_do_hai_long = mucDoHaiLongData
                });
            }
            else if (items.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu doanh nghiệp")
            {
                var _check_ctdt = await db.answer_response.Where(x => x.surveyID == items.surveyID && x.id_ctdt == giamsat.id_ctdt).ToListAsync();
                var totalall = _check_ctdt.Count;
                list_ctdt.Add(new
                {
                    ty_le_da_tra_loi = totalall > 0 ? 100 : 0,
                    ty_le_chua_tra_loi = 0,
                    muc_do_hai_long = mucDoHaiLongData
                });
            }
            else if (items.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu người học" || items.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu cựu người học")
            {
                var _query = await db.nguoi_hoc_khao_sat.Where(x => x.surveyID == items.surveyID && x.sinhvien.lop.id_ctdt == giamsat.id_ctdt).ToListAsync();
                var TotalAll = _query.Count;
                var TotalDaKhaoSat = _query.Where(x => x.is_khao_sat == 1).ToList();
                double percentage = TotalAll > 0 ? Math.Round(((double)TotalDaKhaoSat.Count / TotalAll) * 100, 2) : 0;
                double unpercentage = TotalAll > 0 ? Math.Round(((double)100 - percentage), 2) : 0;
                list_ctdt.Add(new
                {
                    ty_le_da_tra_loi = percentage,
                    ty_le_chua_tra_loi = unpercentage,
                    muc_do_hai_long = mucDoHaiLongData
                });
            }
            return list_ctdt;
        }
        private async Task<dynamic> load_ty_le_co_dau_thoi_gian(survey items, dynamic list_ctdt, dynamic mucDoHaiLongData, GiamSatThongKeKetQua giamsat)
        {
            if (items.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu giảng viên")
            {
                bool check_giang_vien = new[] { 3 }.Contains(items.id_loaikhaosat);
                bool check_cbvc = new[] { 8 }.Contains(items.id_loaikhaosat);
                if (check_cbvc)
                {
                    var count_cbvc = await db.cbvc_khao_sat
                        .Where(x => x.surveyID == items.surveyID && x.CanBoVienChuc.id_chuongtrinhdaotao == giamsat.id_ctdt).ToListAsync();
                    var count_da_tra_loi = await db.answer_response
                           .Where(x => x.time >= giamsat.from_date && x.time <= giamsat.to_date && x.surveyID == items.surveyID && x.id_ctdt == giamsat.id_ctdt).ToListAsync();
                    var count_chua_tra_loi = count_cbvc.Count - count_da_tra_loi.Count;
                    double percentage = count_cbvc.Count > 0
                        ? Math.Round(((double)count_da_tra_loi.Count / count_cbvc.Count) * 100, 2)
                        : 0;
                    double unpercentage = count_cbvc.Count > 0
                        ? Math.Round(100 - percentage, 2)
                        : 0;
                    list_ctdt.Add(new
                    {
                        ty_le_da_tra_loi = percentage,
                        ty_le_chua_tra_loi = unpercentage,
                        muc_do_hai_long = mucDoHaiLongData

                    });
                }
                else if (check_giang_vien)
                {
                    var count_gv = await db.answer_response
                                  .Where(x => x.surveyID == items.surveyID && x.id_ctdt == giamsat.id_ctdt && x.time >= giamsat.from_date && x.time <= giamsat.to_date)
                                  .ToListAsync();
                    list_ctdt.Add(new
                    {
                        ty_le_da_tra_loi = count_gv.Count() > 0 ? 100 : 0,
                        ty_le_chua_tra_loi = 0,
                        muc_do_hai_long = mucDoHaiLongData
                    });
                }
            }
            else if (items.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu người học có học phần")
            {
                var count_mon_hoc = await db.nguoi_hoc_dang_co_hoc_phan
                        .Where(x => x.surveyID == items.surveyID && x.sinhvien.lop.id_ctdt == giamsat.id_ctdt).ToListAsync();

                var count_da_tra_loi = await db.answer_response
                            .Where(x => x.time >= giamsat.from_date && x.time <= giamsat.to_date && x.surveyID == items.surveyID && x.id_ctdt == giamsat.id_ctdt).ToListAsync();
                var count_chua_tra_loi = count_mon_hoc.Count - count_da_tra_loi.Count;
                double percentage = count_mon_hoc.Count > 0
                    ? Math.Round(((double)count_da_tra_loi.Count / count_mon_hoc.Count) * 100, 2)
                    : 0;
                double unpercentage = count_mon_hoc.Count > 0
                    ? Math.Round(100 - percentage, 2)
                    : 0;
                list_ctdt.Add(new
                {
                    ty_le_da_tra_loi = percentage,
                    ty_le_chua_tra_loi = unpercentage,
                    muc_do_hai_long = mucDoHaiLongData
                });
            }
            else if (items.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu doanh nghiệp")
            {
                var ctdt = db.answer_response.Where(x =>
                x.surveyID == items.surveyID &&
                x.time >= giamsat.from_date && x.time <= giamsat.to_date && x.id_ctdt == giamsat.id_ctdt).AsQueryable();
                var totalall = await ctdt.ToListAsync();
                list_ctdt.Add(new
                {
                    ty_le_da_tra_loi = totalall.Count > 0 ? 100 : 0,
                    ty_le_chua_tra_loi = 0,
                    muc_do_hai_long = mucDoHaiLongData
                });
            }
            else if (items.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu người học" || items.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu cựu người học")
            {
                var query = await db.nguoi_hoc_khao_sat.Where(x => x.surveyID == items.surveyID && x.sinhvien.lop.id_ctdt == giamsat.id_ctdt).ToListAsync();
                var TotalDaKhaoSat = await db.answer_response.Where(x => x.surveyID == items.surveyID && x.time >= giamsat.from_date && x.time <= giamsat.to_date && x.id_ctdt == giamsat.id_ctdt).ToListAsync();
                var TotalChuaDaKhaoSat = query.Count - TotalDaKhaoSat.Count;
                var TotalAll = query.Count;
                double percentage = TotalAll > 0 ? Math.Round(((double)TotalDaKhaoSat.Count / TotalAll) * 100, 2) : 0;
                double unpercentage = TotalAll > 0 ? Math.Round(((double)100 - percentage), 2) : 0;
                list_ctdt.Add(new
                {
                    ty_le_da_tra_loi = percentage,
                    ty_le_chua_tra_loi = unpercentage,
                    muc_do_hai_long = mucDoHaiLongData
                });
            }
            return list_ctdt;
        }
        private async Task muc_do_hai_long(dynamic MucDoHaiLong, int? idsurvey, GiamSatThongKeKetQua giamsat)
        {
            var Mucdohailong = db.answer_response
                        .Where(d => d.surveyID == idsurvey &&
                        d.id_ctdt == giamsat.id_ctdt)
                        .AsEnumerable()
                        .AsQueryable();
            if (giamsat.from_date != null && giamsat.to_date != null)
            {
                Mucdohailong = Mucdohailong.Where(x => x.time >= giamsat.from_date && x.time <= giamsat.to_date);
            }
            var responses = await Mucdohailong
                .Select(x => new { IDPhieu = x.surveyID, JsonAnswer = x.json_answer, SurveyJson = x.survey.surveyData })
                .ToListAsync();
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
    }
}
