﻿using CTDT.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.UI.WebControls;

namespace CTDT.Areas.Admin.Controllers
{
    public class ThongKeKetQuaKhaoSatAPIController : ApiController
    {
        dbSurveyEntities db = new dbSurveyEntities();
        // Giám sát kết quả khảo sát - Lọc giảng viên theo môn học API
        [HttpPost]
        [Route("api/loc-giang-vien-by-mon-hoc")]
        public async Task<IHttpActionResult> check_phieu_khao_sat(answer_response survey)
        {
            var check_mon_hoc = await db.nguoi_hoc_dang_co_hoc_phan
                .Where(x => x.surveyID == survey.surveyID && x.sinhvien.lop.id_ctdt == survey.id_ctdt)
                .ToListAsync();
            var data_list = check_mon_hoc
                .GroupBy(x => new
                {
                    x.mon_hoc.id_lop,
                    x.mon_hoc.lop.ma_lop,
                    x.id_mon_hoc,
                    x.mon_hoc.ten_mon_hoc
                })
                .Select(group => new
                {
                    id_lop = group.Key.id_lop,
                    lop = group.Key.ma_lop,
                    id_hoc_phan = group.Key.id_mon_hoc,
                    ten_hoc_phan = group.Key.ten_mon_hoc,
                    giang_vien = group.SelectMany(item => db.CanBoVienChuc
                        .Where(x => x.id_CBVC == item.id_giang_vvien)
                        .Select(x => new
                        {
                            id_giang_vien = x.id_CBVC,
                            ma_giang_vien = x.MaCBVC,
                            ten_giang_vien = x.TenCBVC
                        })).Distinct().ToList()
                }).ToList();

            if (data_list.Count > 0)
            {
                return Ok(new { data = data_list, success = true });
            }
            else
            {
                return Ok(new { message = "Bạn đang chọn phiếu ngoài bộ phiếu 8", success = false });
            }
        }
        // Giám sát kết quả khảo sát - Lọc môn học theo giảng viên API
        [HttpPost]
        [Route("api/loc-mon-hoc-by-giang-vien")]
        public async Task<IHttpActionResult> check_mon_hoc_by_giang_vien(answer_response survey)
        {
            var check_mon_hoc = await db.nguoi_hoc_dang_co_hoc_phan
                .Where(x => x.surveyID == survey.surveyID && x.sinhvien.lop.id_ctdt == survey.id_ctdt)
                .ToListAsync();
            var data_list = check_mon_hoc
               .GroupBy(x => new
               {
                   x.mon_hoc.lop.id_lop,
                   x.mon_hoc.lop.ma_lop,
                   x.CanBoVienChuc.MaCBVC,
                   x.id_giang_vvien,
                   x.CanBoVienChuc.TenCBVC
               })
               .Select(group => new
               {
                   id_lop = group.Key.id_lop,
                   ten_lop = group.Key.ma_lop,
                   ma_giang_vien = group.Key.MaCBVC,
                   id_giang_vien = group.Key.id_giang_vvien,
                   ten_giang_vien = group.Key.TenCBVC,
                   mon_hoc = group.SelectMany(item => db.mon_hoc
                       .Where(x => x.id_mon_hoc == item.id_mon_hoc)
                       .Select(x => new
                       {
                           id_mon_hoc = x.id_mon_hoc,
                           ten_mon_hoc = x.ten_mon_hoc,
                       })).Distinct().ToList()
               }).ToList();

            if (data_list.Count > 0)
            {
                return Ok(new { data = data_list, success = true });
            }
            else
            {
                return Ok(new { message = "Bạn đang chọn phiếu ngoài bộ phiếu 8", success = false });
            }
        }

        [HttpPost]
        [Route("api/test")]
        public async Task<IHttpActionResult> test(GiamSatThongKeKetQua aw)
        {
            var check_answer = await db.answer_response
                .Where(x => x.surveyID == aw.surveyID
                            && x.id_ctdt == aw.id_ctdt)
                .ToListAsync();
            if (aw.id_lop != null)
            {
                check_answer = check_answer.Where(x => x.sinhvien.lop.id_lop == aw.id_lop).ToList();
            }

            if (aw.id_mh != null)
            {
                check_answer = check_answer.Where(x => x.id_mh == aw.id_mh).ToList();
            }

            if (aw.id_CBVC != null)
            {
                check_answer = check_answer.Where(x => x.id_CBVC == aw.id_CBVC).ToList();
            }
            var get_data = check_answer
                .Select(x => new
                {
                    x.json_answer
                }).ToList();
            return Ok(new { data = get_data });
        }

    }
}