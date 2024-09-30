using CTDT.Helper;
using CTDT.Models;
using GoogleApi.Entities.Translate.Common;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using OfficeOpenXml.ConditionalFormatting.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.MetadataServices;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Script.Serialization;
using System.Xml.Linq;

namespace CTDT.Controllers
{
    public class HomeController : Controller
    {
        dbSurveyEntities db = new dbSurveyEntities();
        #region load_phieu_khao_sat
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult load_chuc_nang_nguoi_dung()
        {
            return PartialView("_NavLayout");
        }
        [CheckLoginHelper]
        public ActionResult PhieuKhaoSat(int id)
        {
            ViewBag.id = id;
            var phieukhaosat = db.hedaotao
                            .Where(x => x.id_hedaotao == id)
                            .ToList();
            if (phieukhaosat != null && phieukhaosat.Any())
            {
                ViewBag.Name = phieukhaosat.First().ten_hedaotao;
            }
            else
            {
                ViewBag.Name = "";
            }
            return View(phieukhaosat);
        }
        public JsonResult load_he_dao_tao()
        {
            var check_user = SessionHelper.IsUserLoggedIn();
            var user = SessionHelper.GetUser();
            var hedaotao = db.hedaotao
              .Select(c => new
              {
                  MaHDT = c.id_hedaotao,
                  TenHDT = c.ten_hedaotao,
              }).ToList();
            if (check_user && user.id_typeusers == 1)
            {
                return Json(new { data = hedaotao, islogin = true, client = true }, JsonRequestBehavior.AllowGet);
            }
            else if (check_user && user.id_typeusers == 2)
            {
                return Json(new { islogin = true, admin = true }, JsonRequestBehavior.AllowGet);
            }
            else if (check_user && user.id_typeusers == 3)
            {
                return Json(new { data = hedaotao, islogin = true, ctdt = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var message = "Vui lòng đăng nhập để thực hiện chức năng";
                return Json(new { data = hedaotao, message = message, islogin = false }, JsonRequestBehavior.AllowGet);
            }

        }
        public JsonResult load_phieu_khao_sat(int id)
        {
            var user = SessionHelper.GetUser();
            var emailDomain = user.email.Split('@')[1].ToLower();
            var CodeEmail = user.email.Split('@')[0];
            var get_key_code_learner = CodeEmail.ToString().Substring(0, 2);
            var phieukhaosat = db.survey
               .Where(c => c.surveyStatus == true && c.id_hedaotao == id).ToList();
            var list_phieu_khao_sat = new List<dynamic>();
            var check_xac_thuc = new List<dynamic>();

            foreach (var item in phieukhaosat)
            {
                var survey = db.survey.Where(x => x.surveyID == item.surveyID).FirstOrDefault();
                bool check_answer_survey = db.answer_response.Any(x => x.surveyID == item.surveyID && x.id_users == user.id_users && x.id_namhoc == item.id_namhoc && x.json_answer != null);


                if (item.id_loaikhaosat == 5 || item.id_loaikhaosat == 2 || item.id_loaikhaosat == 6)
                {
                    var test = new
                    {
                        MaPhieu = item.surveyID,
                        TenPKS = item.surveyTitle,
                        MoTaPhieu = item.surveyDescription,
                        MaHDT = item.id_hedaotao,
                        TenHDT = item.hedaotao.ten_hedaotao,
                        TenLoaiKhaoSat = item.LoaiKhaoSat.name_loaikhaosat,
                    };
                    list_phieu_khao_sat.Add(test);

                }
                else if ((item.id_loaikhaosat == 1 || item.id_loaikhaosat == 4))
                {
                    var keyClassList = new JavaScriptSerializer().Deserialize<List<string>>(survey.key_class);
                    if (emailDomain == "student.tdmu.edu.vn" && keyClassList.Contains(get_key_code_learner) && db.sinhvien.Any(x => x.ma_sv == CodeEmail))
                    {
                        var test = new
                        {
                            MaPhieu = item.surveyID,
                            TenPKS = item.surveyTitle,
                            MoTaPhieu = item.surveyDescription,
                            MaHDT = item.id_hedaotao,
                            TenHDT = item.hedaotao.ten_hedaotao,
                            TenLoaiKhaoSat = item.LoaiKhaoSat.name_loaikhaosat
                        };
                        list_phieu_khao_sat.Add(test);
                    }
                }
                else if (item.id_loaikhaosat == 3 || item.id_loaikhaosat == 8)
                {
                    if (db.CanBoVienChuc.Any(x => x.Email == user.email))
                    {
                        var test = new
                        {
                            MaPhieu = item.surveyID,
                            TenPKS = item.surveyTitle,
                            MoTaPhieu = item.surveyDescription,
                            MaHDT = item.id_hedaotao,
                            TenHDT = item.hedaotao.ten_hedaotao,
                            TenLoaiKhaoSat = item.LoaiKhaoSat.name_loaikhaosat,
                        };
                        list_phieu_khao_sat.Add(test);
                    }
                }
            }
            var get_data = new
            {
                survey = list_phieu_khao_sat,
            };
            return Json(new { data = get_data, code = (int)HttpStatusCode.OK }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region xac_thuc

        [CheckLoginHelper]
        public ActionResult xac_thuc(int id)
        {
            ViewBag.id = id;
            return View();
        }

        public JsonResult load_select_xac_thuc(int id)
        {
            var user = SessionHelper.GetUser();
            var survey = db.survey.Where(x => x.surveyID == id).FirstOrDefault();
            if (id == null)
            {
                return Json(new { message = "Invalid survey ID" }, JsonRequestBehavior.AllowGet);
            }

            if (survey != null)
            {
                if (survey.id_loaikhaosat == 2 || survey.id_loaikhaosat == 6)
                {
                    var list_khoa = db.ctdt.Where(x => x.hedaotao.id_hedaotao == survey.id_hedaotao)
                    .Select(x => new
                    {
                        ma_khoa = x.id_khoa,
                        ten_khoa = x.khoa.ten_khoa
                    })
                    .Distinct()
                    .ToList();
                    var list_ctdt = new List<dynamic>();
                    var list_lop = new List<dynamic>();
                    var list_sinh_vien = new List<dynamic>();
                    foreach (var item in list_khoa)
                    {
                        var get_ctdt = db.ctdt.Where(x => x.id_khoa == item.ma_khoa)
                            .Select(x => new
                            {
                                ma_khoa = x.id_khoa,
                                ma_ctdt = x.id_ctdt,
                                ten_ctdt = x.ten_ctdt,
                            })
                            .ToList();
                        foreach (var l in get_ctdt)
                        {
                            var keyClassList = new JavaScriptSerializer().Deserialize<List<string>>(survey.key_class);
                            var get_lop = db.lop
                                .Where(x => x.id_ctdt == l.ma_ctdt && keyClassList.Any(k => x.ma_lop.Contains(k)))
                                .Select(x => new
                                {
                                    ma_ctdt = x.id_ctdt,
                                    ma_lop = x.id_lop,
                                    ten_lop = x.ma_lop
                                })
                                .ToList();
                            list_lop.Add(get_lop);
                            foreach (var sv in get_lop)
                            {

                                var get_sv = db.sinhvien
                                    .Where(x => x.id_lop == sv.ma_lop)
                                    .Select(x => new
                                    {
                                        ma_lop = x.id_lop,
                                        id_nguoi_hoc = x.id_sv,
                                        ma_nguoi_hoc = x.ma_sv,
                                        ten_nguoi_hoc = x.hovaten,
                                        ngay_sinh = x.ngaysinh,
                                    })
                                    .ToList();
                                list_sinh_vien.Add(get_sv);
                            }
                        }
                        list_ctdt.Add(get_ctdt);
                    }
                    var get_data = new
                    {
                        list_khoa = list_khoa,
                        list_ctdt = list_ctdt,
                        list_lop = list_lop,
                        list_nguoi_hoc = list_sinh_vien,
                        is_nguoi_hoc = true
                    };
                    return Json(new { data = get_data }, JsonRequestBehavior.AllowGet);
                }
                else if (survey.id_loaikhaosat == 5)
                {
                    var list_khoa = db.ctdt.Where(x => x.hedaotao.id_hedaotao == survey.id_hedaotao)
                    .Select(x => new
                    {
                        ma_khoa = x.id_khoa,
                        ten_khoa = x.khoa.ten_khoa
                    })
                    .Distinct()
                    .ToList();
                    var list_ctdt = new List<dynamic>();
                    foreach (var item in list_khoa)
                    {
                        var get_ctdt = db.ctdt.Where(x => x.id_khoa == item.ma_khoa)
                            .Select(x => new
                            {
                                ma_khoa = x.id_khoa,
                                ma_ctdt = x.id_ctdt,
                                ten_ctdt = x.ten_ctdt,
                            })
                            .ToList();
                        list_ctdt.Add(get_ctdt);
                    }
                    var get_data = new
                    {
                        list_khoa = list_khoa,
                        list_ctdt = list_ctdt,
                        is_doanh_nghiep = true
                    };
                    return Json(new { data = get_data }, JsonRequestBehavior.AllowGet);
                }
                else if (survey.id_loaikhaosat == 3)
                {
                    var list_khoa = db.ctdt.Where(x => x.hedaotao.id_hedaotao == survey.id_hedaotao)
                    .Select(x => new
                    {
                        ma_khoa = x.id_khoa,
                        ten_khoa = x.khoa.ten_khoa
                    })
                    .Distinct()
                    .ToList();
                    var list_don_vi = db.DonVi
                        .Select(x => new
                        {
                            ma_don_vi = x.id_donvi,
                            ten_don_vi = x.name_donvi,
                        })
                        .ToList();
                    var list_ctdt = new List<dynamic>();
                    foreach (var item in list_khoa)
                    {
                        var get_ctdt = db.ctdt.Where(x => x.id_khoa == item.ma_khoa)
                            .Select(x => new
                            {
                                ma_khoa = x.id_khoa,
                                ma_ctdt = x.id_ctdt,
                                ten_ctdt = x.ten_ctdt,
                            })
                            .ToList();
                        list_ctdt.Add(get_ctdt);
                    }
                    var get_data = new
                    {
                        list_khoa = list_khoa,
                        list_ctdt = list_ctdt,
                        list_don_vi = list_don_vi,
                        is_giang_vien = true
                    };
                    return Json(new { data = get_data }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { data = (object)null }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { message = "Survey not found" }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult check_xac_thuc(int id)
        {
            var user = SessionHelper.GetUser();
            var survey = db.survey.FirstOrDefault(x => x.surveyID == id);
            var url = "";

            if (survey == null)
            {
                return Json(new { data = "/trang-chu", error = "Survey not found" }, JsonRequestBehavior.AllowGet);
            }

            var answer_survey = db.answer_response.FirstOrDefault(x => x.surveyID == survey.surveyID && x.id_users == user.id_users);

            if (answer_survey == null)
            {
                if (survey.id_loaikhaosat == 8)
                {
                    Session.Remove("nguoi_hoc");
                    Session.Remove("ctdt");
                    Session.Remove("don_vi");
                    url = "/phieu-khao-sat/" + id;
                    return Json(new { data = url, non_survey = true, civil_servants = true }, JsonRequestBehavior.AllowGet);
                }

                else if (survey.id_loaikhaosat == 1 || survey.id_loaikhaosat == 4)
                {
                    Session.Remove("nguoi_hoc");
                    Session.Remove("ctdt");
                    Session.Remove("don_vi");
                    url = "/phieu-khao-sat/" + id;
                    return Json(new { data = url, non_survey = true, learner = true }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    url = "/xac_thuc/" + id;
                    return Json(new { data = url }, JsonRequestBehavior.AllowGet);
                }
            }

            bool check_answer_survey = db.answer_response.Any(x => x.surveyID == answer_survey.surveyID && x.id_users == user.id_users && x.id_namhoc == survey.id_namhoc && x.json_answer != null);

            if (check_answer_survey)
            {
                if (survey.id_loaikhaosat == 8)
                {
                    url = "/phieu-khao-sat/dap-an/" + answer_survey.id + "/" + answer_survey.surveyID;
                    return Json(new { data = url, is_answer = true }, JsonRequestBehavior.AllowGet);
                }
                else if (survey.id_loaikhaosat == 1 || survey.id_loaikhaosat == 4)
                {
                    url = "/phieu-khao-sat/dap-an/" + answer_survey.id + "/" + answer_survey.surveyID;
                    return Json(new { data = url, is_answer = true }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    url = "/xac_thuc/" + id;
                    return Json(new { data = url }, JsonRequestBehavior.AllowGet);
                }

            }
            return Json(new { data = url }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult get_nguoi_hoc_by_key(int id, string ma_nguoi_hoc)
        {
            var survey = db.survey.FirstOrDefault(x => x.surveyID == id);
            if (survey.id_loaikhaosat == 2 || survey.id_loaikhaosat == 6)
            {
                var keyClassList = new JavaScriptSerializer().Deserialize<List<string>>(survey.key_class);
                var nguoi_hoc = db.sinhvien
                .Where(x => x.ma_sv == ma_nguoi_hoc && keyClassList.Any(k => x.lop.ma_lop.Contains(k)))
                .Select(x => new
                {
                    ma_nguoi_hoc = x.id_sv,
                    chuong_trinh_dao_tao = x.lop.ctdt.id_ctdt,
                })
                .FirstOrDefault();
                return Json(new { data = nguoi_hoc, success = true }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult save_xac_thuc(int surveyid = 0, int nguoi_hoc = 0, int ctdt = 0, int donvi = 0)
        {
            Session.Remove("nguoi_hoc");
            Session.Remove("ctdt");
            Session.Remove("don_vi");
            Session["nguoi_hoc"] = nguoi_hoc;
            Session["ctdt"] = ctdt;
            Session["don_vi"] = donvi;
            var user = SessionHelper.GetUser();
            var survey = db.survey.Where(x => x.surveyID == surveyid).FirstOrDefault();
            if (survey.id_loaikhaosat == 5)
            {
                var check_isAnswer = db.answer_response.Where(x =>
                    x.surveyID == survey.surveyID &&
                    x.id_ctdt == ctdt &&
                    x.id_users == user.id_users &&
                    x.id_namhoc == survey.id_namhoc &&
                    x.id_donvi == null &&
                    x.id_CBVC == null &&
                    x.id_sv == null &&
                    x.json_answer != null).FirstOrDefault();
                if (check_isAnswer != null)
                {
                    var url = "/Survey/AnswerPKS?id=" + check_isAnswer.id + "&surveyid=" + check_isAnswer.surveyID;
                    return Json(new { is_answer = true, message = "Tài khoản này đã thực hiện khảo sát cho chương trình đào tạo này", info = url }, JsonRequestBehavior.AllowGet);
                }
            }
            else if (survey.id_loaikhaosat == 2 || survey.id_loaikhaosat == 6)
            {
                var check_isAnswer = db.answer_response.Where(x =>
                    x.surveyID == survey.surveyID &&
                    x.id_ctdt == ctdt &&
                    x.id_users == user.id_users &&
                    x.id_namhoc == survey.id_namhoc &&
                    x.id_donvi == null &&
                    x.id_CBVC == null &&
                    x.id_sv == nguoi_hoc &&
                    x.json_answer != null).FirstOrDefault();

                if (check_isAnswer != null)
                {
                    var url = "/Survey/AnswerPKS?id=" + check_isAnswer.id + "&surveyid=" + check_isAnswer.surveyID;
                    return Json(new { is_answer = true, message = "Tài khoản này đã thực hiện khảo sát cho sinh viên này !", info = url }, JsonRequestBehavior.AllowGet);
                }
            }
            else if (survey.id_loaikhaosat == 3)
            {
                var giang_vien = db.CanBoVienChuc.Where(x => x.Email == user.email).FirstOrDefault();
                if (giang_vien != null)
                {
                    var check_isAnswer = db.answer_response.Where(x =>
                    x.surveyID == survey.surveyID &&
                    x.id_ctdt == ctdt &&
                    x.id_users == user.id_users &&
                    x.id_namhoc == survey.id_namhoc &&
                    x.id_donvi == donvi || x.id_donvi == null &&
                    x.id_CBVC == giang_vien.id_CBVC &&
                    x.id_sv == null &&
                    x.json_answer != null).FirstOrDefault();
                    if (check_isAnswer != null)
                    {
                        var url = "/Survey/AnswerPKS?id=" + check_isAnswer.id + "&surveyid=" + check_isAnswer.surveyID;
                        return Json(new { is_answer = true, message = "Tài khoản này đã thực hiện khảo sát cho chương trình đào tạo và đơn vị này !", info = url }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { message = "Không tìm thấy thông tin giảng viên" }, JsonRequestBehavior.AllowGet);
                }

            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        #endregion
        #region bieu_mau_da_khao_sat
        [CheckLoginHelper]
        public ActionResult SurveyedForm()
        {
            ViewBag.Year = new SelectList(db.NamHoc.OrderByDescending(x => x.id_namhoc), "id_namhoc", "ten_namhoc");
            ViewBag.Hedaotao = new SelectList(db.hedaotao.OrderBy(x => x.id_hedaotao), "id_hedaotao", "ten_hedaotao");
            return View();
        }
        [HttpPost]
        public JsonResult load_bo_phieu_da_khao_sat(int year = 0, int hedaotao = 0)
        {
            var user = SessionHelper.GetUser();

            var get_answer_survey = db.answer_response
                .Where(x => x.id_users == user.id_users)
                .Select(x => x.surveyID)
                .Distinct()
                .ToList();
            var result = db.survey.AsQueryable();

            if (year != 0)
            {
                result = result.Where(x => x.id_namhoc == year);
            }
            if (hedaotao != 0)
            {
                result = result.Where(x => x.id_hedaotao == hedaotao);
            }
            var get_survey = result
                .Where(x => get_answer_survey.Contains(x.surveyID))
                .ToList();
            var surveyList = new List<dynamic>();

            foreach (var item in get_survey)
            {
                var query = db.answer_response
                    .Where(x => x.id_users == user.id_users && x.surveyID == item.surveyID)
                    .AsQueryable();

                var bo_phieu = new List<dynamic>();
                var check_loai = new List<dynamic>();
                bool isStudent = db.answer_response.Any(aw => aw.id_sv != null && aw.surveyID == item.surveyID && aw.id_users == user.id_users && aw.id_ctdt != null && aw.json_answer != null);
                bool isCTDT = db.answer_response.Any(aw => aw.id_sv == null && aw.surveyID == item.surveyID && aw.id_users == user.id_users && aw.id_ctdt != null && aw.id_CBVC == null && aw.json_answer != null);
                bool isCBVC = db.answer_response.Any(aw => aw.id_sv == null && aw.surveyID == item.surveyID && aw.id_users == user.id_users && aw.id_ctdt != null && aw.id_CBVC != null && aw.json_answer != null);
                var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                if (isStudent)
                {
                   

                    bo_phieu.AddRange(query
                        .Select(x => new
                        {
                            email = user.email,
                            nguoi_hoc = x.sinhvien.hovaten,
                            ma_nguoi_hoc = x.sinhvien.ma_sv,
                            ctdt = x.ctdt.ten_ctdt,
                            khoa = x.ctdt.khoa.ten_khoa,
                            nam_hoc = x.NamHoc.ten_namhoc,
                            thoi_gian_khao_sat = x.time,
                            page = currentTime > x.time ? "/phieu-khao-sat/dap-an/" + x.id + "/" + x.surveyID : "Ngoài thời gian thực hiện khảo sát"
                        }).ToList());

                    surveyList.Add(new
                    {
                        ten_phieu = item.surveyTitle,
                        bo_phieu = bo_phieu,
                        is_student = true
                    });
                }
                else if (isCTDT)
                {
                    bo_phieu.AddRange(query
                        .Select(x => new
                        {
                            email = user.email,
                            ctdt = x.ctdt.ten_ctdt,
                            khoa = x.ctdt.khoa.ten_khoa,
                            nam_hoc = x.NamHoc.ten_namhoc,
                            thoi_gian_khao_sat = x.time,
                            page = currentTime > x.time ? "/phieu-khao-sat/dap-an/" + x.id + "/" + x.surveyID : "Ngoài thời gian thực hiện khảo sát"
                        }).ToList());
                    surveyList.Add(new
                    {
                        ten_phieu = item.surveyTitle,
                        bo_phieu = bo_phieu,
                        is_ctdt = true
                    });
                }
                else if (isCBVC)
                {
                    bo_phieu.AddRange(query
                        .Select(x => new
                        {
                            email = user.email,
                            ten_cbcv = x.CanBoVienChuc.TenCBVC,
                            ctdt = x.ctdt.ten_ctdt,
                            khoa = x.ctdt.khoa.ten_khoa,
                            nam_hoc = x.NamHoc.ten_namhoc,
                            thoi_gian_khao_sat = x.time,
                            page = currentTime > x.time ? "/phieu-khao-sat/dap-an/" + x.id + "/" + x.surveyID : "Ngoài thời gian thực hiện khảo sát"
                        }).ToList());
                    surveyList.Add(new
                    {
                        ten_phieu = item.surveyTitle,
                        bo_phieu = bo_phieu,
                        is_cbvc = true
                    });
                }
            }

            return Json(new { data = new { survey = surveyList } }, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}