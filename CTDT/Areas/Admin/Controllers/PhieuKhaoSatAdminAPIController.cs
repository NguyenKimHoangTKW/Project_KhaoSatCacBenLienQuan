using CTDT.Helper;
using CTDT.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace CTDT.Areas.Admin.Controllers
{
    public class PhieuKhaoSatAdminAPIController : ApiController
    {
        dbSurveyEntities db = new dbSurveyEntities();
        public users user;
        public PhieuKhaoSatAdminAPIController()
        {
            user = SessionHelper.GetUser();
        }

        [HttpPost]
        [Route("api/admin/danh-sach-phieu-khao-sat")]
        public async Task<IHttpActionResult> danh_sach_phieu_khao_sat(survey survey)
        {
            var query = db.survey.AsQueryable();
            if (survey.id_hedaotao != 0)
            {
                query = query.Where(x => x.id_hedaotao == survey.id_hedaotao);
            }

            if (survey.id_loaikhaosat != 0)
            {
                query = query.Where(x => x.id_loaikhaosat == survey.id_loaikhaosat);
            }

            if (survey.id_namhoc != 0)
            {
                query = query.Where(x => x.id_namhoc == survey.id_namhoc);
            }

            if (survey.surveyStatus == 1 || survey.surveyStatus == 2)
            {
                query = query.Where(x => x.surveyStatus == survey.surveyStatus);
            }

            var ListPhieu = await query
                .Select(p => new
                {
                    ma_phieu = p.surveyID,
                    ten_hdt = p.hedaotao.ten_hedaotao,
                    ma_hdt = p.id_hedaotao,
                    ten_phieu = p.surveyTitle,
                    mo_ta = p.surveyDescription,
                    ngay_tao = p.surveyTimeMake,
                    ngay_cap_nhat = p.surveyTimeUpdate,
                    ngay_bat_dau = p.surveyTimeStart,
                    ngay_ket_thuc = p.surveyTimeEnd,
                    loai_khao_sat = p.LoaiKhaoSat.name_loaikhaosat,
                    ma_loai_khao_sat = p.id_loaikhaosat,
                    nguoi_tao = p.users.firstName + " " + p.users.lastName,
                    trang_thai = p.surveyStatus,
                    nam = p.NamHoc.ten_namhoc,
                }).ToListAsync();

            if (ListPhieu.Any())
            {
                return Ok(new { data = ListPhieu, success = true });
            }
            else
            {
                return Ok(new { message = "Không tồn tại dữ liệu", success = false });
            }
        }

        [HttpPost]
        [Route("api/admin/danh-sach-cau-tra-loi-phieu")]
        public async Task<IHttpActionResult> danh_sach_cac_cau_tra_loi_phieu(answer_response aw)
        {
            var get_data = await db.survey.Where(x => x.surveyID == aw.surveyID).FirstOrDefaultAsync();
            var list_data = new List<dynamic>();
            bool is_subject = false;
            bool is_student = false;
            bool is_cbvc = false;
            bool is_program = false;
            // Nếu phiếu là phiếu có môn học
            if (get_data.LoaiKhaoSat.group_loaikhaosat.id_gr_loaikhaosat == 3)
            {
                is_subject = true;
                var get_answer = await db.answer_response
                    .Where(x => x.surveyID == aw.surveyID)
                    .Select(x => new
                    {
                        ma_kq = x.id,
                        email = x.users.email,
                        mon_hoc = x.mon_hoc.ten_mon_hoc,
                        giang_vien = x.CanBoVienChuc.TenCBVC,
                        sinh_vien = x.sinhvien.hovaten,
                        thoi_gian_thuc_hien = x.time,
                        ctdt = x.ctdt.ten_ctdt,
                        msnh = x.sinhvien.ma_sv,
                    }).ToListAsync();
                list_data.Add(get_answer);
            }
            // Nếu phiếu là phiếu người học hoặc cựu người học
            else if (get_data.LoaiKhaoSat.group_loaikhaosat.id_gr_loaikhaosat == 1 || get_data.LoaiKhaoSat.group_loaikhaosat.id_gr_loaikhaosat == 5)
            {
                is_student = true;
                var get_answer = await db.answer_response
                    .Where(x => x.surveyID == aw.surveyID)
                    .Select(x => new
                    {
                        ma_kq = x.id,
                        email = x.users.email,
                        sinh_vien = x.sinhvien.hovaten,
                        thoi_gian_thuc_hien = x.time,
                        ctdt = x.ctdt.ten_ctdt,
                        msnh = x.sinhvien.ma_sv,
                    }).ToListAsync();
                list_data.Add(get_answer);
            }
            // Nếu phiếu là phiếu doanh nghiệp
            else if (get_data.LoaiKhaoSat.group_loaikhaosat.id_gr_loaikhaosat == 4)
            {
                is_program = true;
                var get_answer = await db.answer_response
                    .Where(x => x.surveyID == aw.surveyID)
                    .Select(x => new
                    {
                        ma_kq = x.id,
                        email = x.users.email,
                        ten_khoa = x.ctdt.khoa.ten_khoa ?? "",
                        ten_ctdt = x.ctdt.ten_ctdt,
                        thoi_gian_thuc_hien = x.time,
                    }).ToListAsync();
                list_data.Add(get_answer);
            }
            // Nếu phiếu là phiếu giảng viên hoặc cán bộ viên chức trong trường
            else if (get_data.LoaiKhaoSat.group_loaikhaosat.id_gr_loaikhaosat == 2)
            {
                is_cbvc = true;
                var get_answer = await db.answer_response
                    .Where(x => x.surveyID == aw.surveyID)
                    .Select(x => new
                    {
                        ma_kq = x.id,
                        don_vi = x.DonVi.name_donvi,
                        email = x.users.email,
                        cbvc = x.CanBoVienChuc.TenCBVC,
                        thoi_gian_thuc_hien = x.time,
                    }).ToListAsync();
                list_data.Add(get_answer);
            }
            if (list_data.Count > 0)
            {
                if (is_subject)
                {
                    return Ok(new { data = list_data, success = true, is_subject = true });
                }
                else if (is_student)
                {
                    return Ok(new { data = list_data, success = true, is_student = true });
                }
                else if (is_program)
                {
                    return Ok(new { data = list_data, success = true, is_program = true });
                }
                else if (is_cbvc)
                {
                    return Ok(new { data = list_data, success = true, is_cbvc = true });
                }

            }
            return Ok(new { message = "Không có dữ liệu", success = false });
        }
    }
}
