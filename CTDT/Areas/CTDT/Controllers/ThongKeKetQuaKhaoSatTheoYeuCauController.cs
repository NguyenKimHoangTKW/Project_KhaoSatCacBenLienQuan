using CTDT.Helper;
using CTDT.Models;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace CTDT.Areas.CTDT.Controllers
{
    [UserAuthorize(3)]
    public class ThongKeKetQuaKhaoSatTheoYeuCauController : Controller
    {
        dbSurveyEntities db = new dbSurveyEntities();
        // GET: CTDT/ThongKeKetQuaKhaoSatTheoYeuCau
        #region Load phiếu khảo sát và đối tượng
        public ActionResult Index()
        {
            ViewBag.Year = new SelectList(db.NamHoc.OrderByDescending(x => x.id_namhoc), "id_namhoc", "ten_namhoc");
            return View();
        }
        public JsonResult LoadPKSByYear(int id)
        {
            var user = SessionHelper.GetUser();

            var surveys = db.survey
                            .Where(x => x.id_namhoc == id && x.id_hedaotao == user.id_hdt)
                            .ToList();
            var sortedSurveys = surveys
                                .OrderBy(s => s.surveyTitle.Split('.').FirstOrDefault())
                                .ThenBy(s => s.surveyTitle)
                                .Select(x => new
                                {
                                    ma_phieu = x.surveyID,
                                    ten_phieu = x.surveyTitle
                                })
                                .ToList();
            return Json(new { data = sortedSurveys, success = true }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Tần xuất câu hỏi
        public async Task<JsonResult> load_doi_tuong_by_phieu()
        {
            var user = SessionHelper.GetUser();
            var survey = await db.survey
                .Where(x => x.id_hedaotao == user.id_hdt)
                .ToListAsync();
            var list_data = new List<dynamic>();
            foreach (var item in survey)
            {
                if (!string.IsNullOrEmpty(item.key_class))
                {
                    var keyClassList = new JavaScriptSerializer().Deserialize<List<string>>(item.key_class);
                    bool isStudent = db.answer_response
                               .Any(aw => aw.id_sv != null
                               && aw.surveyID == item.surveyID
                               && aw.id_ctdt == user.id_ctdt &&
                               keyClassList.Any(k => aw.survey.key_class.Contains(k))
                               && aw.json_answer != null);
                    if (isStudent)
                    {
                        var sinhvien = await db.sinhvien
                            .Where(x => keyClassList.Any(k => x.lop.ma_lop.Contains(k)) && x.lop.ctdt.id_ctdt == user.id_ctdt)
                            .Select(x => new
                            {
                                ma_nguoi_hoc = x.ma_sv,
                                ten_nguoi_hoc = x.hovaten,
                                thuoc_lop = x.lop.ma_lop
                            })
                            .ToListAsync();
                        list_data.Add(new
                        {
                            tham_so = item.surveyID,
                            ten_phieu = item.surveyTitle,
                            nguoi_hoc = sinhvien
                        });
                    }
                }
            }
            return Json(new { data = list_data }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadthongketanxuatYkienkhac(List<int> idanswer, int surveyid = 0)
        {
            var user = SessionHelper.GetUser();
            var query = db.answer_response.AsQueryable();
            var survey = db.survey.Where(x => x.surveyID == surveyid).FirstOrDefault();

            if (surveyid != 0)
            {
                query = query.Where(x => x.surveyID == surveyid);
            }

            if (!string.IsNullOrEmpty(survey.key_class))
            {
                var keyClassList = new JavaScriptSerializer().Deserialize<List<string>>(survey.key_class);

                bool hasAnswerResponseForStudent = db.answer_response
                    .Any(aw => aw.id_sv != null && idanswer.Contains(aw.id) && (surveyid == 0 || aw.surveyID == surveyid)
                               && aw.id_ctdt == user.id_ctdt && aw.json_answer != null);

                if (hasAnswerResponseForStudent)
                {
                    var responses = query
                        .Where(d => keyClassList.Any(k => d.sinhvien.lop.ma_lop.Contains(k))
                                    && d.id_ctdt == user.id_ctdt
                                    && d.surveyID == surveyid
                                    && idanswer.Contains(d.id))
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

                    return Json(questionDataList, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { data = (object)null }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        public JsonResult load_doi_tuong(int? idnamhoc)
        {
            var user = SessionHelper.GetUser();
            var survey = db.survey
                .Where(x => x.id_hedaotao == user.id_hdt && x.id_namhoc == idnamhoc)
                .ToList();

            var data = new List<dynamic>();
            var check_contains = new HashSet<string>();

            foreach (var items in survey)
            {
                bool isStudent = new[] { 1, 2, 4, 6 }.Contains(items.id_loaikhaosat) && (items.is_hocky == false || items.is_hocky == true);
                bool isCTDT = new[] { 5 }.Contains(items.id_loaikhaosat);
                bool isCBVC = new[] { 3, 8 }.Contains(items.id_loaikhaosat);

                if (isStudent)
                {
                    var sinh_vien = db.sinhvien
                        .Where(x => x.lop.ctdt.id_ctdt == user.id_ctdt)
                        .Select(x => new
                        {
                            ho_ten = x.hovaten,
                            ma_nguoi_hoc = x.ma_sv,
                            lop = x.lop.ma_lop,
                            ctdt = x.lop.ctdt.ten_ctdt,
                        })
                        .ToList();

                    var message = "Chọn đáp viên thống kê theo sinh viên";
                    if (!check_contains.Contains(message))
                    {
                        data.Add(new
                        {
                            message = message,
                            data = sinh_vien,
                            is_nguoi_hoc = true
                        });
                        check_contains.Add(message);
                    }
                }
                else if (isCTDT)
                {
                    var answer_response = db.answer_response
                        .Where(x => x.id_ctdt == user.id_ctdt && x.surveyID == items.surveyID)
                        .Select(x => new
                        {
                            ho_ten = x.users.firstName + " " + x.users.lastName,
                            email = x.users.email,
                            ctdt = x.ctdt.ten_ctdt,
                        })
                        .ToList();

                    var message = "Chọn đáp viên thống kê theo doanh nghiệp";
                    if (!check_contains.Contains(message))
                    {
                        data.Add(new
                        {
                            message = message,
                            data = answer_response,
                            is_doanh_nghiep = true
                        });
                        check_contains.Add(message);
                    }
                }
                else if (isCBVC)
                {
                    var cbvc = db.CanBoVienChuc
                        .Where(x => x.id_chuongtrinhdaotao == user.id_ctdt)
                        .Select(x => new
                        {
                            ma_cbvc = x.MaCBVC,
                            ho_ten = x.TenCBVC,
                            thuoc_ctdt = x.ctdt.ten_ctdt,
                            thuoc_don_vi = x.id_donvi != null ? x.DonVi.name_donvi : "Không tồn tại đơn vị"
                        })
                        .ToList();

                    var message = "Chọn đáp viên thống kê theo cán bộ viên chức trong trường";
                    if (!check_contains.Contains(message))
                    {
                        data.Add(new
                        {
                            message = message,
                            data = cbvc,
                            is_cbvc = true
                        });
                        check_contains.Add(message); 
                    }
                }
            }

            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }

    }
}