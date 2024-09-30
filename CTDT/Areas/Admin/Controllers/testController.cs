using CTDT.Helper;
using CTDT.Models;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CTDT.Areas.Admin.Controllers
{
    public class TestController : Controller
    {
        dbSurveyEntities db = new dbSurveyEntities();

        [UserAuthorize(2)]
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult UploadExcel(HttpPostedFileBase file, int? CheckDuLieu)
        {
            if (file != null && file.ContentLength > 0)
            {
                using (var package = new ExcelPackage(file.InputStream))
                {
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    DateTime now = DateTime.UtcNow;
                    int unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                    var worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;
                    int colCount = worksheet.Dimension.Columns;
                    var allSurveys = db.survey.ToList();
                    var truong_duy_nhat = new HashSet<string>();
                    int skippedRecords = 0;
                    int processedRecords = 0;
                    for (int row = 2; row <= rowCount; row++)
                    {
                        bool result = false;

                        if (CheckDuLieu == 1)
                        {
                            result = DapVienNot8Async(worksheet, row, colCount, allSurveys, truong_duy_nhat);
                        }
                        else if (CheckDuLieu == 2)
                        {
                            result =  DapVien8(worksheet, row, colCount, allSurveys, truong_duy_nhat);
                        }

                        if (!result)
                        {
                            skippedRecords++;
                        }
                        else
                        {
                            processedRecords++;
                        }
                    }
                    return Json(new
                    {
                        status = "Dữ liệu đã được xử lý thành công",
                        processed = processedRecords,
                        skipped = skippedRecords
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
        private bool DapVien8(ExcelWorksheet worksheet, int row, int colCount, List<survey> allSurveys, HashSet<string> truong_duy_nhat)
        {
            DateTime now = DateTime.UtcNow;
            int unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            var phieukhaosat = worksheet.Cells[row, 1].Text;
            var surveyTitle = phieukhaosat.Split('.')[0];
            var hedaotao = worksheet.Cells[row, 2].Text.ToLower();
            var hoc_ky = worksheet.Cells[row, 3].Text;
            var namhoc = worksheet.Cells[row, 4].Text.ToLower();
            var mon_hoc = worksheet.Cells[row, 5].Text;
            var ma_cbvc = worksheet.Cells[row, 6].Text;
            var ten_cbvc = worksheet.Cells[row, 7].Text;
            var ctdt = worksheet.Cells[row, 8].Text;
            var masinhvien = worksheet.Cells[row, 9].Text.Trim();
            var tensinhvien = worksheet.Cells[row, 10].Text.ToLower().Trim();
            var ma_lop = worksheet.Cells[row, 11].Text;
            var kiem_tra_truong_duy_nhat = $"{surveyTitle}-{hedaotao}-{hoc_ky}-{namhoc}-{mon_hoc}-{ma_cbvc}-{ten_cbvc}-{ctdt}-{masinhvien}-{tensinhvien}-{ma_lop}";

            if (truong_duy_nhat.Contains(kiem_tra_truong_duy_nhat))
                return false;

            truong_duy_nhat.Add(kiem_tra_truong_duy_nhat);

            var get_phieu = allSurveys.FirstOrDefault(x => x.surveyTitle.Split('.')[0] == surveyTitle
                && x.hedaotao.ten_hedaotao.ToLower() == hedaotao
                && x.NamHoc.ten_namhoc == namhoc);
            var get_mon_hoc = db.mon_hoc.FirstOrDefault(x => x.ten_mon_hoc == mon_hoc);
            var get_sinhvien = db.sinhvien.FirstOrDefault(x => x.ma_sv.Trim() == masinhvien && x.hovaten.Trim().ToLower() == tensinhvien && x.lop.ma_lop == ma_lop);
            var get_cbvc = db.CanBoVienChuc.FirstOrDefault(x => x.MaCBVC == ma_cbvc && x.TenCBVC == ten_cbvc);
            var get_hoc_ky = db.hoc_ky.FirstOrDefault(x => x.ten_hk == hoc_ky);
            var get_ctdt = db.ctdt.FirstOrDefault(x => x.ten_ctdt.ToLower().Trim() == ctdt);

            if (get_phieu == null || get_sinhvien == null || get_ctdt == null || get_mon_hoc == null || get_cbvc == null || get_hoc_ky == null)
                return false;

            var elements = new List<Dictionary<string, object>>();
            int questionNumber = 1;

            for (int col = 12; col <= colCount; col++)
            {
                string value = worksheet.Cells[row, col].Value?.ToString();
                var element = new Dictionary<string, object>
            {
                { "name", $"question{questionNumber++}" },
                { "response", new Dictionary<string, string> { { "text", value ?? string.Empty } } }
            };
                elements.Add(element);
            }

            var pages = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "elements", elements } }
        };

            string jsonPages = JsonConvert.SerializeObject(new { pages });

            var check_answer_survey = db.answer_response
                .FirstOrDefault(x => x.surveyID == get_phieu.surveyID && x.id_sv == get_sinhvien.id_sv && x.id_ctdt == get_ctdt.id_ctdt
                && (x.json_answer == null || x.json_answer == jsonPages));

            if (check_answer_survey == null)
            {
                check_answer_survey = new answer_response
                {
                    surveyID = get_phieu.surveyID,
                    id_sv = get_sinhvien.id_sv,
                    id_hk = get_hoc_ky.id_hk,
                    id_CBVC = get_cbvc.id_CBVC,
                    id_mh = get_mon_hoc.id_mon_hoc,
                    id_ctdt = get_ctdt.id_ctdt,
                    json_answer = jsonPages,
                    time = unixTimestamp,
                    id_namhoc = get_phieu.id_namhoc,
                };
                db.answer_response.Add(check_answer_survey);
            }
            else
            {
                check_answer_survey.json_answer = jsonPages;
            }

           db.SaveChanges();
            return true;
        }

        private bool DapVienNot8Async(ExcelWorksheet worksheet, int row, int colCount, List<survey> allSurveys, HashSet<string> truong_duy_nhat)
        {
            DateTime now = DateTime.UtcNow;
            int unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            var phieukhaosat = worksheet.Cells[row, 2].Text;
            var surveyTitle = phieukhaosat.Split('.')[0];
            var hedaotao = worksheet.Cells[row, 1].Text.ToLower();
            var namhoc = worksheet.Cells[row, 3].Text.ToLower();
            var masinhvien = worksheet.Cells[row, 4].Text.Trim();
            var tensinhvien = worksheet.Cells[row, 5].Text.ToLower().Trim();
            var ctdt = worksheet.Cells[row, 7].Text.ToLower().Trim();
            var kiem_tra_truong_duy_nhat = $"{surveyTitle}-{hedaotao}-{namhoc}-{masinhvien}-{tensinhvien}-{ctdt}";

            if (truong_duy_nhat.Contains(kiem_tra_truong_duy_nhat))
                return false;

            truong_duy_nhat.Add(kiem_tra_truong_duy_nhat);

            var get_phieu = allSurveys.FirstOrDefault(x => x.surveyTitle.Split('.')[0] == surveyTitle
                && x.hedaotao.ten_hedaotao.ToLower() == hedaotao
                && x.NamHoc.ten_namhoc == namhoc);

            var get_sinhvien = db.sinhvien.FirstOrDefault(x => x.ma_sv.Trim() == masinhvien && x.hovaten.Trim().ToLower() == tensinhvien);
            var get_ctdt = db.ctdt.FirstOrDefault(x => x.ten_ctdt.ToLower().Trim() == ctdt);

            if (get_phieu == null || get_sinhvien == null || get_ctdt == null)
                return false;

            var elements = new List<Dictionary<string, object>>();
            int questionNumber = 1;

            for (int col = 8; col <= colCount; col++)
            {
                string value = worksheet.Cells[row, col].Value?.ToString();
                var element = new Dictionary<string, object>
            {
                { "name", $"question{questionNumber++}" },
                { "response", new Dictionary<string, string> { { "text", value ?? string.Empty } } }
            };
                elements.Add(element);
            }

            var pages = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "elements", elements } }
        };

            string jsonPages = JsonConvert.SerializeObject(new { pages });

            var check_answer_survey = db.answer_response
                .FirstOrDefault(x => x.surveyID == get_phieu.surveyID && x.id_sv == get_sinhvien.id_sv && x.id_ctdt == get_ctdt.id_ctdt
                && (x.json_answer == null || x.json_answer == jsonPages));

            if (check_answer_survey == null)
            {
                check_answer_survey = new answer_response
                {
                    surveyID = get_phieu.surveyID,
                    id_sv = get_sinhvien.id_sv,
                    id_ctdt = get_ctdt.id_ctdt,
                    json_answer = jsonPages,
                    time = unixTimestamp,
                    id_namhoc = get_phieu.id_namhoc,
                };
                db.answer_response.Add(check_answer_survey);
            }
            else
            {
                check_answer_survey.json_answer = jsonPages;
            }

            db.SaveChanges();
            return true;
        }
    }
}
