using CTDT.Helper;
using CTDT.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace CTDT.Controllers
{
    public class XacThucAPIController : ApiController
    {
        dbSurveyEntities db = new dbSurveyEntities();
        [HttpPost]
        [Route("api/xac_thuc")]
        public IHttpActionResult load_select_xac_thuc(survey Sv)
        {
            var user = SessionHelper.GetUser();
            var survey = db.survey.Where(x => x.surveyID == Sv.surveyID).FirstOrDefault();
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
                    return Ok(new { data = get_data });
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
                    return Ok(new { data = get_data });
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
                    return Ok(new { data = get_data });
                }

                return Ok(new { data = (object)null });
            }
            else
            {
                return BadRequest("Survey not found");
            }
        }

        [HttpPost]
        [Route("api/check_xac_thuc")]
        public IHttpActionResult check_xac_thuc(survey Sv)
        {
            var user = SessionHelper.GetUser();
            var survey = db.survey.FirstOrDefault(x => x.surveyID == Sv.surveyID);
            var url = "";

            if (survey == null)
            {
                return Ok(new { data = "/trang-chu", error = "Survey not found" });
            }

            var answer_survey = db.answer_response.FirstOrDefault(x => x.surveyID == survey.surveyID && x.id_users == user.id_users);

            if (answer_survey == null)
            {
                if (survey.id_loaikhaosat == 8)
                {
                    HttpContext.Current.Session.Remove("nguoi_hoc");
                    HttpContext.Current.Session.Remove("ctdt");
                    HttpContext.Current.Session.Remove("don_vi");
                    url = "/phieu-khao-sat/" + Sv.surveyID;
                    return Ok(new { data = url, non_survey = true, civil_servants = true });
                }

                else if (survey.id_loaikhaosat == 1 || survey.id_loaikhaosat == 4)
                {
                    HttpContext.Current.Session.Remove("nguoi_hoc");
                    HttpContext.Current.Session.Remove("ctdt");
                    HttpContext.Current.Session.Remove("don_vi");
                    url = "/phieu-khao-sat/" + Sv.surveyID;
                    return Json(new { data = url, non_survey = true, learner = true });
                }
                else
                {
                    url = "/xac_thuc/" + Sv.surveyID;
                    return Json(new { data = url });
                }
            }

            bool check_answer_survey = db.answer_response.Any(x => x.surveyID == answer_survey.surveyID && x.id_users == user.id_users && x.id_namhoc == survey.id_namhoc && x.json_answer != null);

            if (check_answer_survey)
            {
                if (survey.id_loaikhaosat == 8)
                {
                    url = "/phieu-khao-sat/dap-an/" + answer_survey.id + "/" + answer_survey.surveyID;
                    return Ok(new { data = url, is_answer = true });
                }
                else if (survey.id_loaikhaosat == 1 || survey.id_loaikhaosat == 4)
                {
                    url = "/phieu-khao-sat/dap-an/" + answer_survey.id + "/" + answer_survey.surveyID;
                    return Ok(new { data = url, is_answer = true });
                }
                else
                {
                    url = "/xac_thuc/" + Sv.surveyID;
                    return Ok(new { data = url });
                }

            }
            return Ok(new { data = url });
        }

        [HttpPost]
        [Route("api/get_nguoi_hoc_by_key")]
        public IHttpActionResult get_nguoi_hoc_by_key(GetNguoiHoc getNguoiHoc)
        {
            var survey = db.survey.FirstOrDefault(x => x.surveyID == getNguoiHoc.Id);
            if (survey.id_loaikhaosat == 2 || survey.id_loaikhaosat == 6)
            {
                var keyClassList = new JavaScriptSerializer().Deserialize<List<string>>(survey.key_class);
                var nguoi_hoc = db.sinhvien
                .Where(x => x.ma_sv == getNguoiHoc.ma_nguoi_hoc && keyClassList.Any(k => x.lop.ma_lop.Contains(k)))
                .Select(x => new
                {
                    ma_nguoi_hoc = x.id_sv,
                    chuong_trinh_dao_tao = x.lop.ctdt.id_ctdt,
                })
                .FirstOrDefault();
                return Ok(new { data = nguoi_hoc, success = true });
            }
            return Ok(new { success = false });
        }
        [HttpPost]
        [Route("api/save_xac_thuc")]

        public IHttpActionResult save_xac_thuc(SaveXacThuc saveXacThuc)
        {
            HttpContext.Current.Session.Remove("nguoi_hoc");
            HttpContext.Current.Session.Remove("ctdt");
            HttpContext.Current.Session.Remove("don_vi");
            HttpContext.Current.Session["nguoi_hoc"] = saveXacThuc.nguoi_hoc;
            HttpContext.Current.Session["ctdt"] = saveXacThuc.ctdt;
            HttpContext.Current.Session["don_vi"] = saveXacThuc.donvi;
            var user = SessionHelper.GetUser();
            var survey = db.survey.Where(x => x.surveyID == saveXacThuc.Id).FirstOrDefault();
            if (survey.id_loaikhaosat == 5)
            {
                var check_isAnswer = db.answer_response.Where(x =>
                    x.surveyID == survey.surveyID &&
                    x.id_ctdt == saveXacThuc.ctdt &&
                    x.id_users == user.id_users &&
                    x.id_namhoc == survey.id_namhoc &&
                    x.id_donvi == null &&
                    x.id_CBVC == null &&
                    x.id_sv == null &&
                    x.json_answer != null).FirstOrDefault();
                if (check_isAnswer != null)
                {
                    var url = "/Survey/AnswerPKS?id=" + check_isAnswer.id + "&surveyid=" + check_isAnswer.surveyID;
                    return Ok(new { is_answer = true, message = "Tài khoản này đã thực hiện khảo sát cho chương trình đào tạo này", info = url });
                }
            }
            else if (survey.id_loaikhaosat == 2 || survey.id_loaikhaosat == 6)
            {
                var check_isAnswer = db.answer_response.Where(x =>
                    x.surveyID == survey.surveyID &&
                    x.id_ctdt == saveXacThuc.ctdt &&
                    x.id_users == user.id_users &&
                    x.id_namhoc == survey.id_namhoc &&
                    x.id_donvi == null &&
                    x.id_CBVC == null &&
                    x.id_sv == saveXacThuc.nguoi_hoc &&
                    x.json_answer != null).FirstOrDefault();

                if (check_isAnswer != null)
                {
                    var url = "/Survey/AnswerPKS?id=" + check_isAnswer.id + "&surveyid=" + check_isAnswer.surveyID;
                    return Ok(new { is_answer = true, message = "Tài khoản này đã thực hiện khảo sát cho sinh viên này !", info = url });
                }
            }
            else if (survey.id_loaikhaosat == 3)
            {
                var giang_vien = db.CanBoVienChuc.Where(x => x.Email == user.email).FirstOrDefault();
                if (giang_vien != null)
                {
                    var check_isAnswer = db.answer_response.Where(x =>
                    x.surveyID == survey.surveyID &&
                    x.id_ctdt == saveXacThuc.ctdt &&
                    x.id_users == user.id_users &&
                    x.id_namhoc == survey.id_namhoc &&
                    x.id_donvi == saveXacThuc.donvi || x.id_donvi == null &&
                    x.id_CBVC == giang_vien.id_CBVC &&
                    x.id_sv == null &&
                    x.json_answer != null).FirstOrDefault();
                    if (check_isAnswer != null)
                    {
                        var url = "/Survey/AnswerPKS?id=" + check_isAnswer.id + "&surveyid=" + check_isAnswer.surveyID;
                        return Ok(new { is_answer = true, message = "Tài khoản này đã thực hiện khảo sát cho chương trình đào tạo và đơn vị này !", info = url });
                    }
                }
                else
                {
                    return Ok(new { message = "Không tìm thấy thông tin giảng viên" });
                }

            }
            return Ok(new { success = true });
        }
    }
}
