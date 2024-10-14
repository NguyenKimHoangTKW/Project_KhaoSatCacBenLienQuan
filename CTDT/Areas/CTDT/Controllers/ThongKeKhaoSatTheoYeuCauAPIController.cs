using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using CTDT.Helper;
using CTDT.Models;

namespace CTDT.Areas.CTDT.Controllers
{
    public class ThongKeKhaoSatTheoYeuCauAPIController : ApiController
    {
        dbSurveyEntities db = new dbSurveyEntities();
        [HttpPost]
        [Route("api/load_doi_tuong_by_loai_khao_sat")]
        public IHttpActionResult load_doi_tuong(NamHoc namhoc)
        {
            var user = SessionHelper.GetUser();
            var survey = db.survey
                .Where(x => x.id_hedaotao == user.id_hdt && x.NamHoc.ten_namhoc == namhoc.ten_namhoc)
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
            return Ok(new { data = data });
        }
    }
}
