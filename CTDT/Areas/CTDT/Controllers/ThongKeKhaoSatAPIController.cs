using CTDT.Helper;
using CTDT.Models;
using CTDT.Models.Khoa;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace CTDT.Areas.CTDT.Controllers
{
    public class ThongKeKhaoSatAPIController : ApiController
    {
        dbSurveyEntities db = new dbSurveyEntities();
        private users user;
        private bool is_user_hop_tac_doanh_nghiep;
        private bool is_user_ctdt;
        private bool is_user_khoa;
        public ThongKeKhaoSatAPIController()
        {
            user = SessionHelper.GetUser();
            is_user_hop_tac_doanh_nghiep = new int?[] { 6 }.Contains(user.id_typeusers);
            is_user_ctdt = new int?[] { 3 }.Contains(user.id_typeusers);
            is_user_khoa = new int?[] { 5 }.Contains(user.id_typeusers);
        }
        #region Load Charts người học
        [HttpPost]
        [Route("api/giam_sat_thong_ke_nguoi_hoc")]
        public async Task<IHttpActionResult> load_charts_nguoi_hoc(FindChartsTyLeKhaoSat find)
        {
            var survey = await db.survey.Where(x => x.id_namhoc == find.id_nam_hoc && x.id_hedaotao == user.id_hdt).ToListAsync();
            var List_data = new List<dynamic>();
            foreach(var items in survey)
            {
                if (items.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu học viên")
                {
                    bool cuu_hoc_vien = new[] { 6 }.Contains(items.id_loaikhaosat);
                    bool hoc_vien_nhap_hoc = new[] { 4 }.Contains(items.id_loaikhaosat);
                    bool hoc_vien_nhap_hoc_khong_co_thoi_gian_tot_nghiep = new[] { 9 }.Contains(items.id_loaikhaosat);
                    bool hoc_vien_cuoi_khoa_co_quyet_dinh_tot_nghiep = new[] { 12 }.Contains(items.id_loaikhaosat);
                    if (cuu_hoc_vien)
                    {

                    }
                    else if (hoc_vien_nhap_hoc_khong_co_thoi_gian_tot_nghiep)
                    {

                       
                    }
                    else if (hoc_vien_nhap_hoc)
                    {
                        var tach_chuoi_hoc_phan_mon_hoc = items.thang_nhap_hoc.Split('-');
                        string hocPhanStartDateString = "01/" + tach_chuoi_hoc_phan_mon_hoc[0].PadLeft(7, '0');
                        string hocPhanEndDateString = "30/" + tach_chuoi_hoc_phan_mon_hoc[1].PadLeft(7, '0');
                        DateTime hocPhanStartDate = DateTime.ParseExact(hocPhanStartDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        DateTime hocPhanEndDate = DateTime.ParseExact(hocPhanEndDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        var get_sv = (await db.sinhvien.Where(x => x.lop.id_ctdt == user.id_ctdt).ToListAsync())
                               .Where(sv =>
                               {
                                   if (!string.IsNullOrEmpty(sv.namnhaphoc))
                                   {
                                       var formattedNamTotNghiep = sv.namnhaphoc.Split('/');
                                       if (formattedNamTotNghiep.Length == 2)
                                       {
                                           string formattedDate = "01/" + formattedNamTotNghiep[0].PadLeft(2, '0') + "/" + formattedNamTotNghiep[1];

                                           DateTime namtotnghiepDate;
                                           if (DateTime.TryParseExact(formattedDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out namtotnghiepDate))
                                           {
                                               return namtotnghiepDate >= hocPhanStartDate && namtotnghiepDate <= hocPhanEndDate;
                                           }
                                       }
                                   }
                                   return false;
                               })
                               .Select(x => new { 
                                   
                                   ten = x.hovaten,
                                   lop = x.lop.ma_lop
                               }).ToList();
                        var DataStudent = new
                        {
                            tong_khao_sat = get_sv.Count,
                        };

                        List_data.Add(new
                        {
                            phieu = items.surveyTitle,
                            data = DataStudent
                        });
                        return Ok(new { data = List_data, non_survey = true });
                    }
                    else if (hoc_vien_cuoi_khoa_co_quyet_dinh_tot_nghiep)
                    {
                        
                    }
                }
                // Check phiếu thuộc giảng viên
                else if (items.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu giảng viên")
                {
                    
                }
                else if (items.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu doanh nghiệp")
                {
                   
                }
                else if (items.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu người học có học phần")
                {
                    
                }
            }

            return Ok(new { message = "No Data Avalible", is_data = false });
            
        }
        public async Task<List<sinhvien>> convert_nguoi_hoc(string thangstring, string namstring)
        {
            var tach_chuoi_nam_tot_nghiep = thangstring.Split('-');
            string startDateString = "01/" + tach_chuoi_nam_tot_nghiep[0].PadLeft(7, '0') + "/" + tach_chuoi_nam_tot_nghiep[1];
            string endDateString = "30/" + tach_chuoi_nam_tot_nghiep[1].PadLeft(7, '0') + "/" + tach_chuoi_nam_tot_nghiep[0];
            DateTime startDate = DateTime.ParseExact(startDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime endDate = DateTime.ParseExact(endDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            var result = await db.sinhvien.ToListAsync();

            var filteredStudents = result
                .Where(sv =>
                {
                    if (!string.IsNullOrEmpty(namstring))
                    {
                        var formattedNamTotNghiep = namstring.Split('/');
                        if (formattedNamTotNghiep.Length == 2)
                        {
                            string formattedDate = "01/" + formattedNamTotNghiep[0].PadLeft(2, '0') + "/" + formattedNamTotNghiep[1];
                            if (DateTime.TryParseExact(
                                formattedDate,
                                "dd/MM/yyyy",
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.None,
                                out DateTime namtotnghiepDate))
                            {
                                return namtotnghiepDate >= startDate && namtotnghiepDate <= endDate;
                            }
                        }
                    }
                    return true; 
                })
                .Select(x => new sinhvien { hovaten = x.hovaten })
                .ToList();

            return filteredStudents;
        }




        #endregion
        #region Load người học
        [HttpPost]
        [Route("api/load_thong_ke_nguoi_hoc_khao_sat")]
        public async Task<IHttpActionResult> load_nguoi_hoc(FindChartsTyLeKhaoSat find)
        {
            var survey = await db.survey.Where(x => x.id_namhoc == find.id_nam_hoc && x.id_hedaotao == user.id_hdt).ToListAsync();
            var List_data = new List<dynamic>();
            foreach (var item_survey in survey)
            {
                var DataList = new List<dynamic>();
                bool isStudent = new[] { 1, 2, 4, 6 }.Contains(item_survey.id_loaikhaosat) && item_survey.is_hocky == false;
                bool isStudentBySubject = new[] { 1, 2, 4, 6 }.Contains(item_survey.id_loaikhaosat) && item_survey.is_hocky == true;
                bool isCTDT = new[] { 5 }.Contains(item_survey.id_loaikhaosat);
                bool isCBVC = new[] { 3, 8 }.Contains(item_survey.id_loaikhaosat);
                var json = new List<dynamic>();
                var answer_response = db.answer_response
                    .Where(x => x.surveyID == item_survey.surveyID && x.id_namhoc == find.id_nam_hoc && x.survey.id_hedaotao == user.id_hdt);
                answer_response = answer_response.Where(x => x.id_ctdt == user.id_ctdt);

                var query = await answer_response.OrderBy(x => x.surveyID).Distinct().ToListAsync();
                if (!string.IsNullOrEmpty(item_survey.key_class))
                {
                    var keyClassList = new JavaScriptSerializer().Deserialize<List<string>>(item_survey.key_class);

                    if (isStudent)
                    {
                        sinh_vien(DataList, find.id_ctdt, item_survey);
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
                    ma_phieu = item_survey.surveyID,
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
        private void sinh_vien_thuong(string namesurvey, int? idctdt,survey survey, dynamic list_data, List<string> keyClassList)
        {
            var tach_chuoi_nam_tot_nghiep = survey.thang_nhap_hoc.Split('-');
            string startDateString = "01/" + tach_chuoi_nam_tot_nghiep[0].PadLeft(7, '0');
            string endDateString = "30/" + tach_chuoi_nam_tot_nghiep[1].PadLeft(7, '0');
            DateTime startDate = DateTime.ParseExact(startDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime endDate = DateTime.ParseExact(endDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            var sinh_vien = (db.sinhvien.Where(x=> x.lop.id_ctdt == user.id_ctdt).ToList())
                .Where(sv =>
                {
                    if (!string.IsNullOrEmpty(sv.namnhaphoc))
                    {
                        var formattedNamTotNghiep = sv.namnhaphoc.Split('/');
                        if (formattedNamTotNghiep.Length == 2)
                        {
                            string formattedDate = "01/" + formattedNamTotNghiep[0].PadLeft(2, '0') + "/" + formattedNamTotNghiep[1];

                            return DateTime.TryParseExact(
                                       formattedDate,
                                       "dd/MM/yyyy",
                                       CultureInfo.InvariantCulture,
                                       DateTimeStyles.None,
                                       out DateTime namtotnghiepDate) &&
                                   namtotnghiepDate >= startDate &&
                                   namtotnghiepDate <= endDate;
                        }
                    }
                    return false;
                })
                .ToList();
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
        #region load tần xuất
        [HttpPost]
        [Route("api/load_tan_xuat_dap_an")]
        public async Task<IHttpActionResult> load_tan_xuat(FindChartsTyLeKhaoSat f)
        {
            var user = SessionHelper.GetUser();
            var query = db.answer_response
                .Where(x => x.surveyID == f.id_survey
                && (x.id_hk != null ? x.id_hk == f.id_hoc_ky : true))
                .AsQueryable();


            var survey = await db.survey.AsNoTracking().FirstOrDefaultAsync(x => x.surveyID == f.id_survey);
            List<object> tan_xuat_5_muc = new List<object>();
            List<object> tan_xuat_1_lua_chon = new List<object>();
            List<object> tan_xuat_nhieu_lua_chon = new List<object>();
            List<object> tan_xuat_y_kien_khac = new List<object>();
            var ty_le_khao_sat = new List<dynamic>();

            if (!string.IsNullOrEmpty(survey?.key_class))
            {
                var keyClassList = new JavaScriptSerializer().Deserialize<List<string>>(survey.key_class);
                bool isStudent = new[] { 1, 2, 4, 6 }.Contains(survey.id_loaikhaosat) && survey.is_hocky == false;
                bool isStudentBySubject = new[] { 1, 2, 4, 6 }.Contains(survey.id_loaikhaosat) && survey.is_hocky == true;
                if (isStudent)
                {
                    sinh_vien(ty_le_khao_sat, f.id_ctdt, survey);
                }
                else if (isStudentBySubject)
                {
                    sinh_vien_subject(ty_le_khao_sat, f.id_ctdt, survey.surveyID, keyClassList);

                }
                if (isStudent || isStudentBySubject)
                {
                    tan_xuat_5_muc = cau_hoi_5_muc(query, user.id_ctdt, survey.surveyID, keyClassList);
                    tan_xuat_1_lua_chon = cau_hoi_mot_lua_chon(query, user.id_ctdt, survey.surveyID, keyClassList);
                    tan_xuat_nhieu_lua_chon = cau_hoi_nhieu_lua_chon(query, user.id_ctdt, survey.surveyID, keyClassList);
                    tan_xuat_y_kien_khac = y_kien_khac(query, user.id_ctdt, survey.surveyID, keyClassList);
                }
            }
            else
            {
                bool isCTDT = new[] { 5 }.Contains(survey.id_loaikhaosat);
                bool isCBVC = new[] { 3, 8 }.Contains(survey.id_loaikhaosat);
                if (isCTDT)
                {
                    chuong_trinh_dao_tao(ty_le_khao_sat, f.id_ctdt, survey.surveyID);
                }
                else if (isCBVC)
                {
                    can_bo_vien_chuc(ty_le_khao_sat, f.id_ctdt, survey.surveyID);
                }
                if (isCTDT || isCBVC)
                {
                    tan_xuat_5_muc = cau_hoi_5_muc(query, user.id_ctdt, survey.surveyID);
                    tan_xuat_1_lua_chon = cau_hoi_mot_lua_chon(query, user.id_ctdt, survey.surveyID);
                    tan_xuat_nhieu_lua_chon = cau_hoi_nhieu_lua_chon(query, user.id_ctdt, survey.surveyID);
                    tan_xuat_y_kien_khac = y_kien_khac(query, user.id_ctdt, survey.surveyID);
                }
            }
            var data = new
            {
                rate = ty_le_khao_sat,
                five_levels = tan_xuat_5_muc,
                single_levels = tan_xuat_1_lua_chon,
                many_leves = tan_xuat_nhieu_lua_chon,
                other_levels = tan_xuat_y_kien_khac,
            };
            return Ok(data);
        }
        #endregion
        #region Thống kê tần xuất câu hỏi
        [HttpPost]
        [Route("api/load_pks_by_year")]
        public IHttpActionResult LoadPKSByYear(FindChartsTyLeKhaoSat find)
        {
            var user = SessionHelper.GetUser();

            var surveys = db.survey
                            .Where(x => x.id_namhoc == find.id_nam_hoc && x.id_hedaotao == user.id_hdt)
                            .ToList();
            var sortedSurveys = surveys
                                .OrderBy(s => s.surveyTitle.Split('.').FirstOrDefault())
                                .ThenBy(s => s.surveyTitle)
                                .Select(x => new
                                {
                                    IDSurvey = x.surveyID,
                                    NameSurvey = x.surveyTitle,
                                    is_hoc_ky = db.answer_response.Any(k => k.hoc_ky.ten_hk != null && x.surveyID == k.surveyID)
                                })
                                .ToList();
            return Ok(new { data = sortedSurveys, success = true });
        }

        #endregion
        #region Hàm load tần xuất câu hỏi
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
        private List<object> cau_hoi_mot_lua_chon(IEnumerable<answer_response> query, int? idctdt, int? idsurvey, List<string> keyClassList = null)
        {
            var responses = query
                 .Where(d => (keyClassList == null || keyClassList.Any(k => d.sinhvien.lop.ma_lop.Contains(k))) && d.id_ctdt == idctdt && d.surveyID == idsurvey)
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
            }).ToList().Cast<object>().ToList();

            return questionDataList;
        }
        private List<object> cau_hoi_nhieu_lua_chon(IEnumerable<answer_response> query, int? idctdt, int? idsurvey, List<string> keyClassList = null)
        {
            var responses = query
                 .Where(d => (keyClassList == null || keyClassList.Any(k => d.sinhvien.lop.ma_lop.Contains(k))) && d.id_ctdt == idctdt && d.surveyID == idsurvey)
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
            }).ToList().Cast<object>().ToList();


            return questionDataList;
        }
        private List<object> y_kien_khac(IEnumerable<answer_response> query, int? idctdt, int? idsurvey, List<string> keyClassList = null)
        {
            var responses = query
                  .Where(d => (keyClassList == null || keyClassList.Any(k => d.sinhvien.lop.ma_lop.Contains(k))) && d.id_ctdt == idctdt && d.surveyID == idsurvey)
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


            return questionDataList;
        }
        #endregion
        #region Hàm load đối tượng
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

            var DataCBVC = new
            {
                tong_khao_sat = cbvc.Count(),
                tong_phieu_da_tra_loi = cbvc.Count(),
                tong_phieu_chua_tra_loi = 0,
                ty_le_da_tra_loi = cbvc.Count() > 0 ? 100 : 0,
                ty_le_chua_tra_loi = 0,
            };

            DataList.Add(new
            {
                ty_le_tham_gia_khao_sat = DataCBVC,
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
            var DataCTDT = new
            {
                tong_khao_sat = ctdt.Count(),
                tong_phieu_da_tra_loi = ctdt.Count(),
                tong_phieu_chua_tra_loi = 0,
                ty_le_da_tra_loi = ctdt.Count() > 0 ? 100 : 0,
                ty_le_chua_tra_loi = 0,
            };
            DataList.Add(new
            {
                ty_le_tham_gia_khao_sat = DataCTDT,
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
                var DataStudent = new
                {
                    tong_khao_sat = totalStudents,
                    tong_phieu_da_tra_loi = totalAnswered,
                    tong_phieu_chua_tra_loi = totalStudents - totalAnswered,
                    ty_le_da_tra_loi = percentage,
                    ty_le_chua_tra_loi = Math.Round(100 - percentage, 2),
                };
                DataList.Add(new
                {
                    hoc_ky = item.ten_hk,
                    ty_le_tham_gia_khao_sat = DataStudent,
                });
            }
        }
        private void sinh_vien(dynamic DataList, int? idctdt, survey survey)
        {
            if (survey.thang_nhap_hoc != null)
            {
                // Parse date range
                var tach_chuoi_nam_tot_nghiep = survey.thang_nhap_hoc.Split('-');
                string startDateString = "01/" + tach_chuoi_nam_tot_nghiep[0].PadLeft(2, '0');
                string endDateString = "30/" + tach_chuoi_nam_tot_nghiep[1].PadLeft(2, '0');
                DateTime startDate = DateTime.ParseExact(startDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                DateTime endDate = DateTime.ParseExact(endDateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                // Load all relevant student dates into memory
                var sinhvienList = db.sinhvien
                    .Where(sv => !string.IsNullOrEmpty(sv.namnhaphoc))
                    .AsEnumerable() // Force query execution
                    .Where(sv =>
                    {
                        var parts = sv.namnhaphoc.Split('/');
                        if (parts.Length == 2)
                        {
                            string formattedDate = "01/" + parts[0].PadLeft(2, '0') + "/" + parts[1];
                            return DateTime.TryParseExact(formattedDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate) &&
                                   parsedDate >= startDate && parsedDate <= endDate;
                        }
                        return false;
                    });

                // Filter by department or program
                if (user.id_ctdt != null)
                {
                    sinhvienList = sinhvienList.Where(x => x.lop.id_ctdt == user.id_ctdt);
                }
                else if (user.id_khoa != null)
                {
                    if (idctdt != 0)
                    {
                        sinhvienList = sinhvienList.Where(x => x.lop.ctdt.id_khoa == user.id_khoa && x.lop.ctdt.id_ctdt == idctdt);
                    }
                    else
                    {
                        sinhvienList = sinhvienList.Where(x => x.lop.ctdt.id_khoa == user.id_khoa);
                    }
                }

                // Load answered survey responses
                var totalAnsweredQuery = db.answer_response
                    .Where(aw => aw.surveyID == survey.surveyID &&
                                 aw.id_hk == null &&
                                 aw.survey.id_hedaotao == user.id_hdt &&
                                 aw.id_CBVC == null &&
                                 aw.id_mh == null &&
                                 aw.json_answer != null);

                if (user.id_ctdt != null)
                {
                    totalAnsweredQuery = totalAnsweredQuery.Where(x => x.id_ctdt == user.id_ctdt);
                }
                else if (user.id_khoa != null)
                {
                    if (idctdt != 0)
                    {
                        totalAnsweredQuery = totalAnsweredQuery.Where(x => x.ctdt.id_khoa == user.id_khoa && x.id_ctdt == idctdt);
                    }
                    else
                    {
                        totalAnsweredQuery = totalAnsweredQuery.Where(x => x.ctdt.id_khoa == user.id_khoa);
                    }
                }

                // Compute results
                int totalStudents = sinhvienList.Count();
                int totalAnswered = totalAnsweredQuery.Count();
                double percentage = totalStudents > 0
                    ? Math.Round(((double)totalAnswered / totalStudents) * 100, 2)
                    : 0;

                var DataStudent = new
                {
                    tong_khao_sat = totalStudents,
                    tong_phieu_da_tra_loi = totalAnswered,
                    tong_phieu_chua_tra_loi = totalStudents - totalAnswered,
                    ty_le_da_tra_loi = percentage,
                    ty_le_chua_tra_loi = Math.Round(100 - percentage, 2),
                };

                // Add result to DataList
                DataList.Add(new
                {
                    ty_le_tham_gia_khao_sat = DataStudent,
                });
            }
        }


        #endregion  
    }
}
