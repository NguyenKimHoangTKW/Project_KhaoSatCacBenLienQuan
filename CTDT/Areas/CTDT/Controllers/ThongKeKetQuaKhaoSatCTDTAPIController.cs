using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using CTDT.Helper;
using CTDT.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CTDT.Areas.CTDT.Controllers
{
    public class ThongKeKetQuaKhaoSatCTDTAPIController : ApiController
    {
        dbSurveyEntities db = new dbSurveyEntities();
        private users user;

        public ThongKeKetQuaKhaoSatCTDTAPIController()
        {
            user = SessionHelper.GetUser();
        }
        public class get_option_ctdt
        {
            public int id_namhoc { get; set; }
            public int id_ctdt { get; set; }
            public int surveyID { get; set; }
            public int? from_date { get; set; }
            public int? to_date { get; set; }
        }
        [HttpPost]
        [Route("api/ctdt/load-survey-check-ctdt")]
        public async Task<IHttpActionResult> load_survey_by_hdt_of_ctdt(get_option_ctdt getoption)
        {
            var check_ctdt = await db.ctdt.FirstOrDefaultAsync(x => x.id_ctdt == getoption.id_ctdt);
            var get_survey = await db.survey
                .Where(x => x.id_hedaotao == check_ctdt.id_hdt && x.id_namhoc == getoption.id_namhoc)
                .Select(x => new
                {
                    value = x.surveyID,
                    name = x.id_dot_khao_sat != null ? x.surveyTitle + " - " + x.dot_khao_sat.ten_dot_khao_sat : x.surveyTitle,
                }).ToListAsync();
            if (get_survey.Count > 0)
            {
                return Ok(new { data = JsonConvert.SerializeObject(get_survey), success = true });
            }
            else
            {
                return Ok(new { message = "Chưa có dữ liệu phiếu khảo sát", success = false });
            }
        }

        [HttpPost]
        [Route("api/ctdt/thong-ke-ket-qua-khao-sat")]
        public async Task<IHttpActionResult> giam_sat_ket_qua_khao_sat(get_option_ctdt aw)
        {
            var _checkctdt = await db.ctdt.FirstOrDefaultAsync(x => x.id_ctdt == aw.id_ctdt);
            var _get_time_check = await db.survey.FirstOrDefaultAsync(x => x.surveyID == aw.surveyID);
            var check_answer = db.answer_response
                .Where(x => x.surveyID == aw.surveyID
                           && x.ctdt.id_hdt == _checkctdt.id_hdt
                           && x.id_ctdt == aw.id_ctdt).AsQueryable();
            if (aw.from_date != null && aw.to_date != null)
            {
                check_answer = check_answer.Where(x => x.time >= aw.from_date && x.time <= aw.to_date);
            }
            var get_data = await check_answer
                .Select(x => new
                {
                    JsonAnswer = x.json_answer,
                    SurveyJson = x.survey.surveyData
                })
                .ToListAsync();
            var list_count = new List<dynamic>();
            var get_time_check = new List<dynamic>();
            if (get_data.Count > 0)
            {
                if (aw.from_date != null && aw.to_date != null)
                {
                    list_count = await load_ty_le_co_dau_thoi_gian(aw);
                    get_time_check.Add(new
                    {
                        time_check_start = aw.from_date,
                        time_check_end = aw.to_date
                    });
                }
                else
                {
                    list_count = await load_ty_le_khong_dau_thoi_gian(aw);
                    get_time_check.Add(new
                    {
                        time_check_start = _get_time_check.surveyTimeStart,
                        time_check_end = _get_time_check.surveyTimeEnd
                    });
                }

                List<object> tan_xuat_5_muc = new List<object>();
                List<object> tan_xuat_1_lua_chon = new List<object>();
                List<object> tan_xuat_nhieu_lua_chon = new List<object>();
                List<object> tan_xuat_y_kien_khac = new List<object>();
                tan_xuat_5_muc = cau_hoi_5_muc(get_data);
                tan_xuat_1_lua_chon = cau_hoi_mot_lua_chon(get_data);
                tan_xuat_nhieu_lua_chon = cau_hoi_nhieu_lua_chon(get_data);
                tan_xuat_y_kien_khac = y_kien_khac(get_data);
                return Ok(new
                {
                    rate = JsonConvert.SerializeObject(list_count) ,
                    five_levels = JsonConvert.SerializeObject(tan_xuat_5_muc) ,
                    single_levels = JsonConvert.SerializeObject(tan_xuat_1_lua_chon),
                    many_leves = JsonConvert.SerializeObject(tan_xuat_nhieu_lua_chon),   
                    other_levels = JsonConvert.SerializeObject(tan_xuat_y_kien_khac),
                    time_check = JsonConvert.SerializeObject(get_time_check),
                    success = true
                });
            }
            else
            {
                return Ok(new
                {
                    message = "Chưa có dữ liệu để thống kê",
                    success = false
                });
            }
        }
        public async Task<List<dynamic>> load_ty_le_khong_dau_thoi_gian(get_option_ctdt aw)
        {
            var list_count = new List<dynamic>();
            var check_group_pks = await db.survey.FirstOrDefaultAsync(x => x.surveyID == aw.surveyID);
            var get_ctdt = await db.ctdt.FirstOrDefaultAsync(x => x.id_ctdt == aw.id_ctdt);
            if (check_group_pks.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu người học có học phần")
            {
                var count_mon_hoc = await db.nguoi_hoc_dang_co_hoc_phan
                        .Where(x => x.surveyID == aw.surveyID && x.sinhvien.lop.id_ctdt == aw.id_ctdt)
                        .ToListAsync();
                var count_da_tra_loi = count_mon_hoc.Where(x => x.da_khao_sat == 1).ToList();
                var count_chua_tra_loi = count_mon_hoc.Where(x => x.da_khao_sat == 0).ToList();

                double percentage = count_mon_hoc.Count > 0
                    ? Math.Round(((double)count_da_tra_loi.Count / count_mon_hoc.Count) * 100, 2)
                    : 0;
                var get_mh = new
                {
                    pks = check_group_pks.id_dot_khao_sat != null ? check_group_pks.surveyTitle + " - " + check_group_pks.dot_khao_sat.ten_dot_khao_sat : check_group_pks.surveyTitle,
                    tong_khao_sat = count_mon_hoc.Count,
                    tong_phieu_da_tra_loi = count_da_tra_loi.Count,
                    tong_phieu_chua_tra_loi = count_chua_tra_loi.Count,
                    ty_le_da_tra_loi = percentage,
                    ty_le_chua_tra_loi = Math.Round(100 - percentage, 2),
                    is_mon_hoc = true
                };
                list_count.Add(get_mh);
            }
            else if (check_group_pks.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu giảng viên")
            {
                if (check_group_pks.id_loaikhaosat == 3)
                {
                    var count_gv = await db.answer_response
                        .Where(x => x.surveyID == aw.surveyID && x.id_ctdt == aw.id_ctdt)
                        .ToListAsync();
                    var DataCTDT = new
                    {
                        pks = check_group_pks.surveyTitle,
                        tong_khao_sat = count_gv.Count(),
                        tong_phieu_da_tra_loi = count_gv.Count(),
                        tong_phieu_chua_tra_loi = 0,
                        ty_le_da_tra_loi = count_gv.Count() > 0 ? 100 : 0,
                        ty_le_chua_tra_loi = 0,
                        is_can_bo = true
                    };
                    list_count.Add(DataCTDT);
                }

                else if (check_group_pks.id_loaikhaosat == 8)
                {
                    var count_cbvc = await db.cbvc_khao_sat
                        .Where(x => x.surveyID == aw.surveyID && x.CanBoVienChuc.id_chuongtrinhdaotao == aw.id_ctdt).ToListAsync();
                    var count_da_tra_loi = count_cbvc.Where(x => x.is_khao_sat == 1).ToList();
                    var count_chua_tra_loi = count_cbvc.Where(x => x.is_khao_sat == 0).ToList();
                    double percentage = count_cbvc.Count > 0
                        ? Math.Round(((double)count_da_tra_loi.Count / count_cbvc.Count) * 100, 2)
                        : 0;

                    var DataCTDT = new
                    {
                        ctdt = check_group_pks.surveyTitle,
                        tong_khao_sat = count_cbvc.Count,
                        tong_phieu_da_tra_loi = count_da_tra_loi.Count,
                        tong_phieu_chua_tra_loi = count_chua_tra_loi.Count,
                        ty_le_da_tra_loi = percentage,
                        ty_le_chua_tra_loi = Math.Round(100 - percentage, 2)
                    };
                }
            }
            else if (check_group_pks.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu doanh nghiệp")
            {
                var ctdt = await db.answer_response.Where(x => x.surveyID == aw.surveyID && x.id_ctdt == aw.id_ctdt).ToListAsync();
                var datactdt = new
                {
                    pks = check_group_pks.surveyTitle,
                    tong_khao_sat = ctdt.Count,
                    tong_phieu_da_tra_loi = ctdt.Count,
                    tong_phieu_chua_tra_loi = 0,
                    ty_le_da_tra_loi = ctdt.Count > 0 ? 100 : 0,
                    ty_le_chua_tra_loi = 0
                };
                list_count.Add(datactdt);
            }
            else if (check_group_pks.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu người học" || check_group_pks.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu cựu người học")
            {
                var query = await db.nguoi_hoc_khao_sat.Where(x => x.surveyID == aw.surveyID && x.sinhvien.lop.id_ctdt == aw.id_ctdt).ToListAsync();
                var TotalAll = query.Count;
                var idphieu = db.survey.Where(x => x.surveyID == aw.surveyID).FirstOrDefault();
                var TotalDaKhaoSat = query.Where(x => x.is_khao_sat == 1).ToList();
                double percentage = TotalAll > 0 ? Math.Round(((double)TotalDaKhaoSat.Count / TotalAll) * 100, 2) : 0;
                var DataCBVC = new
                {
                    pks = check_group_pks.surveyTitle,
                    tong_khao_sat = TotalAll,
                    tong_phieu_da_tra_loi = TotalDaKhaoSat.Count,
                    tong_phieu_chua_tra_loi = (TotalAll - TotalDaKhaoSat.Count),
                    ty_le_da_tra_loi = percentage,
                    ty_le_chua_tra_loi = Math.Round(((double)100 - percentage), 2),
                };
                list_count.Add(DataCBVC);
            }
            return list_count;
        }
        public async Task<List<dynamic>> load_ty_le_co_dau_thoi_gian(get_option_ctdt aw)
        {
            var list_count = new List<dynamic>();
            var check_group_pks = await db.survey.FirstOrDefaultAsync(x => x.surveyID == aw.surveyID);
            if (check_group_pks.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu người học có học phần")
            {
                var count_mon_hoc = await db.nguoi_hoc_dang_co_hoc_phan
                        .Where(x => x.surveyID == aw.surveyID && x.sinhvien.lop.id_ctdt == aw.id_ctdt).ToListAsync();

                var count_da_tra_loi = await db.answer_response
                            .Where(x => x.time >= aw.from_date && x.time <= aw.to_date && x.surveyID == aw.surveyID && x.id_ctdt == aw.id_ctdt).ToListAsync();
                var count_chua_tra_loi = count_mon_hoc.Count - count_da_tra_loi.Count;
                double percentage = count_mon_hoc.Count > 0
                    ? Math.Round(((double)count_da_tra_loi.Count / count_mon_hoc.Count) * 100, 2)
                    : 0;
                double unpercentage = count_mon_hoc.Count > 0
                    ? Math.Round(100 - percentage, 2)
                    : 0;
                var get_mh = new
                {
                    pks = check_group_pks.id_dot_khao_sat != null ? check_group_pks.surveyTitle + " - " + check_group_pks.dot_khao_sat.ten_dot_khao_sat : check_group_pks.surveyTitle,
                    tong_khao_sat = count_mon_hoc.Count,
                    tong_phieu_da_tra_loi = count_da_tra_loi.Count,
                    tong_phieu_chua_tra_loi = count_chua_tra_loi,
                    ty_le_da_tra_loi = percentage,
                    ty_le_chua_tra_loi = unpercentage,
                    is_mon_hoc = true
                };
                list_count.Add(get_mh);
            }
            else if (check_group_pks.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu giảng viên")
            {
                if (check_group_pks.id_loaikhaosat == 3)
                {
                    var count_gv = await db.answer_response
                        .Where(x => x.surveyID == aw.surveyID && x.id_ctdt == aw.id_ctdt && x.time >= aw.from_date && x.time <= aw.to_date)
                        .ToListAsync();
                    var DataCTDT = new
                    {
                        pks = check_group_pks.surveyTitle,
                        tong_khao_sat = count_gv.Count,
                        tong_phieu_da_tra_loi = count_gv.Count,
                        tong_phieu_chua_tra_loi = 0,
                        ty_le_da_tra_loi = count_gv.Count > 0 ? 100 : 0,
                        ty_le_chua_tra_loi = 0,
                    };
                    list_count.Add(DataCTDT);
                }

                else if (check_group_pks.id_loaikhaosat == 8)
                {
                    var count_cbvc = await db.cbvc_khao_sat
                        .Where(x => x.surveyID == aw.surveyID && x.CanBoVienChuc.id_chuongtrinhdaotao == aw.id_ctdt).ToListAsync();
                    var count_da_tra_loi = await db.answer_response
                           .Where(x => x.time >= aw.from_date && x.time <= aw.to_date && x.surveyID == aw.surveyID && x.id_ctdt == aw.id_ctdt).ToListAsync();
                    var count_chua_tra_loi = count_cbvc.Count - count_da_tra_loi.Count;
                    double percentage = count_cbvc.Count > 0
                        ? Math.Round(((double)count_da_tra_loi.Count / count_cbvc.Count) * 100, 2)
                        : 0;
                    double unpercentage = count_cbvc.Count > 0
                        ? Math.Round(100 - percentage, 2)
                        : 0;
                    var DataCTDT = new
                    {
                        pks = check_group_pks.surveyTitle,
                        tong_khao_sat = count_cbvc.Count,
                        tong_phieu_da_tra_loi = count_da_tra_loi.Count,
                        tong_phieu_chua_tra_loi = count_chua_tra_loi,
                        ty_le_da_tra_loi = percentage,
                        ty_le_chua_tra_loi = unpercentage
                    };
                    list_count.Add(DataCTDT);
                }
            }
            else if (check_group_pks.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu doanh nghiệp")
            {
                var ctdt = db.answer_response.Where(x =>
                x.surveyID == aw.surveyID &&
                x.time >= aw.from_date && x.time <= aw.to_date && x.id_ctdt == aw.id_ctdt).AsQueryable();
                var totalall = await ctdt.ToListAsync();
                var datactdt = new
                {
                    pks = check_group_pks.surveyTitle,
                    tong_khao_sat = totalall.Count,
                    tong_phieu_da_tra_loi = totalall.Count,
                    tong_phieu_chua_tra_loi = 0,
                    ty_le_da_tra_loi = totalall.Count > 0 ? 100 : 0,
                    ty_le_chua_tra_loi = 0
                };
                list_count.Add(datactdt);
            }
            else if (check_group_pks.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu người học" || check_group_pks.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu cựu người học")
            {
                var query = await db.nguoi_hoc_khao_sat.Where(x => x.surveyID == aw.surveyID).ToListAsync();
                var TotalDaKhaoSat = await db.answer_response.Where(x => x.surveyID == aw.surveyID && x.time >= aw.from_date && x.time <= aw.to_date && x.sinhvien.lop.id_ctdt == aw.id_ctdt).ToListAsync();
                var TotalChuaDaKhaoSat = query.Count - TotalDaKhaoSat.Count;
                var TotalAll = query.Count;
                double percentage = TotalAll > 0 ? Math.Round(((double)TotalDaKhaoSat.Count / TotalAll) * 100, 2) : 0;
                double unpercentage = TotalAll > 0 ? Math.Round(((double)100 - percentage), 2) : 0;
                var DataCBVC = new
                {
                    pks = check_group_pks.surveyTitle,
                    tong_khao_sat = TotalAll,
                    tong_phieu_da_tra_loi = TotalDaKhaoSat.Count,
                    tong_phieu_chua_tra_loi = TotalChuaDaKhaoSat,
                    ty_le_da_tra_loi = percentage,
                    ty_le_chua_tra_loi = unpercentage,
                };
                list_count.Add(DataCBVC);
            }
            return list_count;
        }
        public List<object> cau_hoi_5_muc(dynamic get_data)
        {
            Dictionary<string, Dictionary<string, int>> frequency = new Dictionary<string, Dictionary<string, int>>();
            Dictionary<string, List<string>> choices = new Dictionary<string, List<string>>();
            List<string> specificChoices = new List<string> {
                "Hoàn toàn không đồng ý",
                "Không đồng ý",
                "Bình thường",
                "Đồng ý",
                "Hoàn toàn đồng ý"
            };
            foreach (var response in get_data)
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
        public List<object> cau_hoi_mot_lua_chon(dynamic get_data)
        {
            var questionDataDict = new Dictionary<string, dynamic>();

            List<string> specificChoices = new List<string> {
                "Hoàn toàn không đồng ý",
                "Không đồng ý",
                "Bình thường",
                "Đồng ý",
                "Hoàn toàn đồng ý"
            };
            foreach (var response in get_data)
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
            }).ToList().Cast<object>().ToList();

            return questionDataList;
        }
        public List<object> cau_hoi_nhieu_lua_chon(dynamic get_data)
        {
            var questionDataDict = new Dictionary<string, dynamic>();
            foreach (var response in get_data)
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
            }).ToList().Cast<object>().ToList();


            return questionDataList;
        }
        public List<object> y_kien_khac(dynamic get_data)
        {
            var questionDataList = new List<dynamic>();

            foreach (var response in get_data)
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


            return questionDataList;
        }

    }
}
