using CTDT.Helper;
using CTDT.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CTDT.Areas.Admin.Controllers
{
    public class ThongKeKetQuaKhaoSatController : Controller
    {
        dbSurveyEntities db = new dbSurveyEntities();
        [HttpGet]
        public JsonResult Loadthongketanxuat(int surveyid = 0)
        {
            var query = db.answer_response.AsQueryable();

            if (surveyid != 0)
            {
                query = query.Where(x => x.surveyID == surveyid);
            }

            var responses = query
                .Where(d => d.surveyID == surveyid)
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
            var results = frequency.Select(f => new
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

            return Json(results, JsonRequestBehavior.AllowGet);
        }
    }
}