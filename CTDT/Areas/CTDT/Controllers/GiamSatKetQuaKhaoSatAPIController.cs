using CTDT.Helper;
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
        #region Thống kê toàn bộ phiếu
        [HttpPost]
        [Route("api/ctdt/ty_le_khao_sat")]
        public async Task<IHttpActionResult> load_charts_ty_le(FindChartsTyLeKhaoSat find)
        {
            var query = await db.answer_response
                .Where(x => x.id_namhoc == find.id_nam_hoc && x.survey.id_hedaotao == user.id_hdt)
                .GroupBy(x => x.surveyID) 
                .Select(g => g.FirstOrDefault()) 
                .ToListAsync();
            if (user.id_ctdt != null)
            {
                query = query.Where(x => x.id_ctdt == user.id_ctdt).ToList();
            }
            else if (user.id_khoa != null)
            {
                if (find.id_ctdt != 0)
                {
                    query = query.Where(x => x.ctdt.id_khoa == user.id_khoa && x.id_ctdt == find.id_ctdt).ToList();
                }
                else
                {
                    query = query.Where(x => x.ctdt.id_khoa == user.id_khoa).ToList();
                }
            }

           
            var Alldata = new List<dynamic>();
            foreach (var survey in query)
            {
                var DataList = new List<dynamic>();
                var MucDoHaiLong = new List<dynamic>();
                var idphieu = db.survey.Where(x => x.surveyID == survey.surveyID).FirstOrDefault();
                if (!string.IsNullOrEmpty(idphieu.key_class))
                {
                    var keyClassList = JsonConvert.DeserializeObject<List<string>>(idphieu.key_class);
                    bool isStudent = new[] { 1, 2, 4, 6 }.Contains(idphieu.id_loaikhaosat) && idphieu.is_hocky == false;
                    bool isStudentBySubject = new[] { 1, 2, 4, 6 }.Contains(idphieu.id_loaikhaosat) && idphieu.is_hocky == true;

                    if (isStudent)
                    {
                        sinh_vien(DataList, find.id_ctdt, survey.surveyID, keyClassList);
                        muc_do_hai_long(MucDoHaiLong, find.id_ctdt, survey.surveyID, keyClassList);
                    }
                    else if (isStudentBySubject)
                    {
                        sinh_vien_subject(DataList, find.id_ctdt, survey.hoc_ky.ten_hk, survey.id_hk, keyClassList);
                        muc_do_hai_long(MucDoHaiLong, find.id_ctdt, survey.surveyID, keyClassList);
                    }
                }
                else
                {
                    bool isCTDT = new[] { 5 }.Contains(idphieu.id_loaikhaosat);
                    bool isCBVC = new[] { 3, 8 }.Contains(idphieu.id_loaikhaosat);

                    if (isCTDT)
                    {
                        chuong_trinh_dao_tao(DataList, find.id_ctdt, survey.surveyID);
                        muc_do_hai_long(MucDoHaiLong, find.id_ctdt, survey.surveyID);
                    }
                }
                var Data = new
                {
                    ma_phieu = survey.surveyID,
                    ten_phieu = survey.survey.surveyTitle,
                    hoc_ky = survey.hoc_ky != null ? survey.hoc_ky.ten_hk : null,
                    ty_le_tham_gia_khao_sat = DataList.Select(x => new
                    {
                        ma_phieu = x.IDPhieu,
                        ty_le = x.TyLeDaTraLoi
                    }).ToList(),
                    muc_do_hai_long = MucDoHaiLong.Select(x => new
                    {
                        x.ma_phieu,
                        x.ty_le_muc_do_hai_long,
                        x.count_phieu,
                        x.avg_ty_le_hai_long,
                        x.avg_score
                    }).ToList()
                };
                Alldata.Add(Data);
            }
            if (Alldata == null || !Alldata.Any())
            {
                return Ok(new { message = "No Data Avalible", is_data = false });
            }
            return Ok(new { data = Alldata, is_data = true });
        }
        #endregion
        #region Thống kê theo thông tư 01
        [HttpPost]
        [Route("api/ctdt/ty_le_khao_sat_thong_tu_01")]
        public async Task<IHttpActionResult> load_charts_ty_le_01(FindChartsTyLeKhaoSat find)
        {
            var query = await db.answer_response
                .Where(x => x.id_namhoc == find.id_nam_hoc && x.survey.id_hedaotao == user.id_hdt && (x.survey.surveyTitle.Contains("7") || x.survey.surveyTitle.Contains("8")))
                .GroupBy(x => x.surveyID)
                .Select(g => g.FirstOrDefault())
                .ToListAsync();
            if (user.id_ctdt != null)
            {
                query = query.Where(x => x.id_ctdt == user.id_ctdt).ToList();
            }
            else if (user.id_khoa != null)
            {
                query = query.Where(x => x.ctdt.id_khoa == user.id_khoa).ToList();
                if (find.id_ctdt != 0)
                {
                    query = query.Where(x => x.id_ctdt == find.id_ctdt).ToList();
                }
            }
 
            var Alldata = new List<dynamic>();
            foreach (var survey in query)
            {
                var DataList = new List<dynamic>();
                var MucDoHaiLong = new List<dynamic>();
                var idphieu = db.survey.Where(x => x.surveyID == survey.surveyID).FirstOrDefault();
                if (!string.IsNullOrEmpty(idphieu.key_class))
                {
                    var keyClassList = new JavaScriptSerializer().Deserialize<List<string>>(idphieu.key_class);
                    bool isStudent = new[] { 1, 2, 4, 6 }.Contains(idphieu.id_loaikhaosat) && idphieu.is_hocky == false;
                    bool isStudentBySubject = new[] { 1, 2, 4, 6 }.Contains(idphieu.id_loaikhaosat) && idphieu.is_hocky == true;
                    if (isStudent)
                    {
                        sinh_vien(DataList, find.id_ctdt, survey.surveyID, keyClassList);
                        tan_xuat_cau_hoi_thong_tu_01(MucDoHaiLong, find.id_ctdt, survey.surveyID, keyClassList);
                    }
                    else if (isStudentBySubject)
                    {
                        sinh_vien_subject(DataList, find.id_ctdt, survey.hoc_ky.ten_hk, survey.id_hk, keyClassList);
                        tan_xuat_cau_hoi_thong_tu_01(MucDoHaiLong, find.id_ctdt, survey.surveyID, keyClassList);
                    }
                    var Data = new
                    {
                        ma_phieu = survey.surveyID,
                        ten_phieu = survey.survey.surveyTitle,
                        hoc_ky = survey.hoc_ky != null ? survey.hoc_ky.ten_hk : null,
                        ty_le_tham_gia_khao_sat = DataList.Select(x => new
                        {
                            ma_phieu = x.IDPhieu,
                            ty_le = x.TyLeDaTraLoi
                        }).ToList(),
                        muc_do_hai_long = MucDoHaiLong.Select(x => new
                        {
                            x.ma_phieu,
                            x.ty_le_muc_do_hai_long,
                            x.count_phieu,
                            x.avg_ty_le_hai_long,
                            x.avg_score
                        }).ToList()
                    };
                    Alldata.Add(Data);
                }
            }
            if (Alldata == null || !Alldata.Any())
            {
                return Ok(new { message = "No Data Avalible", is_data = false });
            }
            return Ok(new { data = Alldata, is_data = true });
        }
        #endregion
        #region Các hàm gọi đối tượng
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
            var idphieu = db.survey.Where(x => x.surveyID == idsurvey).FirstOrDefault();

            var DataCTDT = new
            {
                IDPhieu = idphieu.surveyID,
                TongKhaoSat = ctdt.Count(),
                TongPhieuDaTraLoi = ctdt.Count(),
                TongPhieuChuaTraLoi = 0,
                TyLeDaTraLoi = 100,
                TyLeChuaTraLoi = 0
            };
            DataList.Add(DataCTDT);
        }
        private void sinh_vien_subject(dynamic DataList, int? idctdt, string hocky, int? idsurvey, List<string> keyClassList)
        {
            var sinhvienQuery = db.sinhvien
                             .Where(x => keyClassList.Any(k => x.lop.ma_lop.Contains(k)));
            var TotalIsKhaoSat = db.answer_response
                .Where(aw => aw.surveyID == idsurvey
                          && aw.hoc_ky.ten_hk == hocky
                          && aw.id_CBVC != null
                          && aw.id_mh != null
                          && aw.json_answer != null
                          && sinhvienQuery.Any(sv => sv.id_sv == aw.id_sv));
            if (user.id_ctdt != null)
            {
                sinhvienQuery = sinhvienQuery.Where(x => x.lop.id_ctdt == user.id_ctdt);
                TotalIsKhaoSat = TotalIsKhaoSat.Where(x => x.id_ctdt == user.id_ctdt);
            }
            else if (user.id_khoa != null)
            {
                if (idctdt != 0)
                {
                    sinhvienQuery = sinhvienQuery.Where(x => x.lop.ctdt.id_khoa == user.id_khoa && x.lop.ctdt.id_ctdt == idctdt);
                    TotalIsKhaoSat = TotalIsKhaoSat.Where(x => x.ctdt.id_khoa == user.id_khoa && x.id_ctdt == idctdt);
                }
                else
                {
                    sinhvienQuery = sinhvienQuery.Where(x => x.lop.ctdt.id_khoa == user.id_khoa);
                    TotalIsKhaoSat = TotalIsKhaoSat.Where(x => x.ctdt.id_khoa == user.id_khoa);
                }
            }
            var TotalAll = sinhvienQuery.Count();
            double percentage = Math.Round(((double)TotalIsKhaoSat.Count() / TotalAll) * 100, 2);
            var idphieu = db.survey.Where(x => x.surveyID == idsurvey).FirstOrDefault();
            var DataStudent = new
            {
                IDPhieu = idphieu.surveyID,
                TongKhaoSat = TotalAll,
                TongPhieuDaTraLoi = TotalIsKhaoSat,
                TongPhieuChuaTraLoi = (TotalAll - TotalIsKhaoSat.Count()),
                TyLeDaTraLoi = percentage,
                TyLeChuaTraLoi = Math.Round(((double)100 - percentage), 2),
            };
            DataList.Add(DataStudent);
        }
        private void sinh_vien(dynamic DataList, int? idctdt, int? idsurvey, List<string> keyClassList)
        {
            var sinhvienQuery = db.sinhvien
                .Where(x => keyClassList.Any(k => x.lop.ma_lop.Contains(k)));

            var TotalIsKhaoSat = db.answer_response
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
                TotalIsKhaoSat = TotalIsKhaoSat.Where(x => x.id_ctdt == user.id_ctdt);
            }
            else if (user.id_khoa != null)
            {

                if (idctdt != 0)
                {
                    sinhvienQuery = sinhvienQuery.Where(x => x.lop.ctdt.id_khoa == user.id_khoa && x.lop.ctdt.id_ctdt == idctdt);
                    TotalIsKhaoSat = TotalIsKhaoSat.Where(x => x.ctdt.id_khoa == user.id_khoa && x.id_ctdt == idctdt);
                }
                else
                {
                    sinhvienQuery = sinhvienQuery.Where(x => x.lop.ctdt.id_khoa == user.id_khoa);
                    TotalIsKhaoSat = TotalIsKhaoSat.Where(x => x.ctdt.id_khoa == user.id_khoa);
                }
            }
            var TotalAll = sinhvienQuery.Count();
            double percentage = Math.Round(((double)TotalIsKhaoSat.Count() / TotalAll) * 100, 2);
            var idphieu = db.survey.Where(x => x.surveyID == idsurvey).FirstOrDefault();

            var DataStudent = new
            {
                IDPhieu = idphieu.surveyID,
                TongKhaoSat = TotalAll,
                TongPhieuDaTraLoi = TotalIsKhaoSat,
                TongPhieuChuaTraLoi = (TotalAll - TotalIsKhaoSat.Count()),
                TyLeDaTraLoi = percentage,
                TyLeChuaTraLoi = Math.Round(((double)100 - percentage), 2),
            };
            DataList.Add(DataStudent);
        }
        private void tan_xuat_cau_hoi_thong_tu_01(dynamic MucDoHaiLong, int? idctdt, int? idsurvey, List<string> keyClassList = null)
        {
            var Mucdohailong = db.answer_response
                     .Where(d => d.surveyID == idsurvey && (d.survey.surveyTitle.Contains("7") || d.survey.surveyTitle.Contains("8")))
                     .AsEnumerable()
                     .Where(d => keyClassList == null || keyClassList.Count == 0 || keyClassList.Any(k => d.sinhvien.lop.ma_lop.Contains(k)))
                     .AsQueryable();
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
            var lastQuestionData = questionDataDict.Values.LastOrDefault();
            if (lastQuestionData != null)
            {
                var questionDataList = new List<dynamic> { lastQuestionData }.Select(q => new
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
                    AverageScore = ((List<dynamic>)q.Choices).Cast<dynamic>().Sum(c =>
                    {
                        switch ((string)c.ChoiceText)
                        {
                            case "Hoàn toàn không đồng ý": return (int)c.Count * 1;
                            case "Không đồng ý": return (int)c.Count * 2;
                            case "Bình thường": return (int)c.Count * 3;
                            case "Đồng ý": return (int)c.Count * 4;
                            case "Hoàn toàn đồng ý": return (int)c.Count * 5;
                            default: return 0;
                        }
                    }) / (double)q.TotalResponses,
                    MucDoHaiLong = ((List<dynamic>)q.Choices).Cast<dynamic>().Where(c => (string)c.ChoiceText == "Đồng ý" || (string)c.ChoiceText == "Hoàn toàn đồng ý")
                    .Sum(c => (double)c.Percentage)
                }).ToList();

                var totalMucDoHaiLong = questionDataList.Sum(x => x.MucDoHaiLong);
                var totalPhieu = questionDataList.Count();
                var totalAverageScore = questionDataList.Sum(x => x.AverageScore);
                var maphieu = db.survey.Where(x => x.surveyID == idsurvey).FirstOrDefault();
                var GetDataMucDoHaiLong = new
                {
                    ma_phieu = maphieu.surveyID,
                    ty_le_muc_do_hai_long = totalMucDoHaiLong,
                    count_phieu = totalPhieu,
                    avg_ty_le_hai_long = totalPhieu > 0 ? Math.Round(((double)totalMucDoHaiLong / totalPhieu), 2) : 0,
                    avg_score = totalPhieu > 0 ? Math.Round(((double)totalAverageScore / totalPhieu), 2) : 0
                };
                MucDoHaiLong.Add(GetDataMucDoHaiLong);
            }
        }


        private void muc_do_hai_long(dynamic MucDoHaiLong, int? idctdt, int? idsurvey, List<string> keyClassList = null)
        {
            var Mucdohailong = db.answer_response
                     .Where(d => d.surveyID == idsurvey)
                     .AsEnumerable()
                     .Where(d => keyClassList == null || keyClassList.Count == 0 || keyClassList.Any(k => d.sinhvien.lop.ma_lop.Contains(k)))
                     .AsQueryable();
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
            var maphieu = db.survey.Where(x => x.surveyID == idsurvey).FirstOrDefault();
            var GetDataMucDoHaiLong = new
            {
                ma_phieu = maphieu.surveyID,
                ty_le_muc_do_hai_long = totalMucDoHaiLong,
                count_phieu = totalPhieu,
                avg_ty_le_hai_long = totalPhieu > 0 ? Math.Round(((double)totalMucDoHaiLong / totalPhieu), 2) : 0,
                avg_score = totalPhieu > 0 ? Math.Round(((double)totalAverageScore / totalPhieu), 2) : 0
            };
            MucDoHaiLong.Add(GetDataMucDoHaiLong);
        }
        #endregion
    }
}
