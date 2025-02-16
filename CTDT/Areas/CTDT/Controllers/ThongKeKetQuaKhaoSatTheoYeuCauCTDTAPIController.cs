using CTDT.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace CTDT.Areas.CTDT.Controllers
{
    public class ThongKeKetQuaKhaoSatTheoYeuCauCTDTAPIController : ApiController
    {
        dbSurveyEntities db = new dbSurveyEntities();
        public class DoiTuongTheoYeuCau
        {
            public int surveyID { get; set; }
            public int id_ctdt { get; set; }
        }
        [HttpPost]
        [Route("api/ctdt/load-doi-tuong-thong-ke-theo-yeu-cau")]
        public async Task<IHttpActionResult> load_doi_tuong_yeu_cau(DoiTuongTheoYeuCau doituong)
        {
            var check_survey = await db.survey.FirstOrDefaultAsync(x => x.surveyID == doituong.surveyID);
            var data_list = new List<dynamic>();
            var check_object = new List<dynamic>();
            if (check_survey.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu giảng viên")
            {
                bool check_giang_vien = new[] { 3 }.Contains(check_survey.id_loaikhaosat);
                bool check_cbvc = new[] { 8 }.Contains(check_survey.id_loaikhaosat);
                if (check_cbvc)
                {
                    var check_cbvc_khao_sat = await db.cbvc_khao_sat
                        .Where(x => x.surveyID == doituong.surveyID && x.CanBoVienChuc.id_chuongtrinhdaotao == doituong.id_ctdt)
                        .Select(x => new
                        {
                            x.id_cbvc,
                            x.CanBoVienChuc.TenCBVC,
                            ctdt = x.CanBoVienChuc.ctdt.ten_ctdt,
                        })
                        .ToListAsync();
                    data_list.AddRange(check_cbvc_khao_sat);
                    check_object.Add(new
                    {
                        is_cbvc = true
                    });
                }
                else if (check_giang_vien)
                {
                    var check_giang_vien_khao_sat = await db.answer_response
                        .Where(x => x.surveyID == doituong.surveyID && x.id_ctdt == doituong.id_ctdt)
                        .Select(x => new
                        {
                            x.CanBoVienChuc.id_CBVC,
                            x.CanBoVienChuc.TenCBVC,
                            ctdt = x.ctdt.ten_ctdt
                        })
                        .ToListAsync();
                    data_list.AddRange(check_giang_vien_khao_sat);
                    check_object.Add(new
                    {
                        is_cbvc = true
                    });
                }
            }
            else if (check_survey.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "phiếu doanh nghiệp")
            {
                var ctdt = await db.answer_response
                    .Where(x => x.surveyID == doituong.surveyID && x.id_ctdt == doituong.id_ctdt)
                    .Select(x => new
                    {
                        id = x.id,
                        ten_user = x.users.firstName + " " + x.users.lastName,
                        email = x.users.email,
                        ctdt = x.ctdt.ten_ctdt
                    })
                    .ToListAsync();
                data_list.AddRange(ctdt);
                check_object.Add(new
                {
                    is_doanh_nghiep = true
                });
            }
            else if (check_survey.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu người học có học phần")
            {
                var check_hoc_phan = await db.nguoi_hoc_dang_co_hoc_phan.Where(x => x.surveyID == doituong.surveyID && x.sinhvien.lop.id_ctdt == doituong.id_ctdt)
                    .Select(x => new
                    {
                        id = x.id_nguoi_hoc_by_hoc_phan,
                        mon_hoc = x.mon_hoc.ten_mon_hoc,
                        x.sinhvien.lop.ma_lop,
                        giang_vien_giang_day = x.CanBoVienChuc.TenCBVC,
                        ma_nguoi_hoc = x.sinhvien.ma_sv,
                        ten_nguoi_hoc = x.sinhvien.hovaten
                    })
                    .ToListAsync();
                data_list.AddRange(check_hoc_phan);
                check_object.Add(new
                {
                    is_nguoi_hoc_co_hoc_phan_khao_sat = true
                });
            }
            else if (check_survey.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu người học" || check_survey.LoaiKhaoSat.group_loaikhaosat.name_gr_loaikhaosat == "Phiếu cựu người học")
            {
                var check_nguoi_hoc = await db.nguoi_hoc_khao_sat
                    .Where(x => x.surveyID == doituong.surveyID && x.sinhvien.lop.id_ctdt == doituong.id_ctdt)
                    .Select(x => new
                    {
                        x.id_nguoi_hoc_khao_sat,
                        ma_nguoi_hoc = x.sinhvien.ma_sv,
                        ten_nguoi_hoc = x.sinhvien.hovaten,
                        x.sinhvien.lop.ma_lop,
                        x.sinhvien.lop.ctdt.ten_ctdt
                    })
                    .ToListAsync();
                data_list.AddRange(check_nguoi_hoc);
                check_object.Add(new
                {
                    is_nguoi_hoc_khao_sat = true
                });
            }
            if (data_list.Count > 0)
            {
                return Ok(new { data = data_list, check_object = check_object, success = true });
            }
            else
            {
                return Ok(new { message = "Chưa có dữ liệu khảo sát trong năm học để thống kê", success = false });
            }
        }
    }
}
