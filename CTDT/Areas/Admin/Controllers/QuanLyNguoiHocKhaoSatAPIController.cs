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
    public class QuanLyNguoiHocKhaoSatAPIController : ApiController
    {
        dbSurveyEntities db = new dbSurveyEntities();
        [HttpPost]
        [Route("api/admin/load-phieu-by-nam-thuoc-nguoi-hoc")]
        public async Task<IHttpActionResult> load_pks_by_year(survey survey)
        {
            var validGroupIds = new List<int> { 1, 5 };
            var pks = await db.survey
                .Where(x => x.id_namhoc == survey.id_namhoc
                            && x.id_hedaotao == survey.id_hedaotao
                            && x.mo_thong_ke == 1
                            && validGroupIds.Contains(x.LoaiKhaoSat.group_loaikhaosat.id_gr_loaikhaosat))
                .Select(x => new
                {
                    id_phieu = x.surveyID,
                    ten_phieu = x.dot_khao_sat.ten_dot_khao_sat != null
                                ? x.surveyTitle + " - " + x.dot_khao_sat.ten_dot_khao_sat
                                : x.surveyTitle,
                })
                .ToListAsync();
            var ctdt = await db.ctdt
                .Where(x => x.id_hdt == survey.id_hedaotao)
                .Select(x => new
                {
                    id_ctdt = x.id_ctdt,
                    ten_ctdt = x.ten_ctdt
                })
                .ToListAsync();
            var sortedPks = pks.OrderBy(p =>
            {
                var match = System.Text.RegularExpressions.Regex.Match(p.ten_phieu, @"Phiếu (\d+)");
                return match.Success ? int.Parse(match.Groups[1].Value) : int.MaxValue;
            }).ToList();

            if (sortedPks.Count > 0)
            {
                return Ok(new { data = sortedPks, ctdt = ctdt, success = true });
            }
            else
            {
                return Ok(new { message = "Không có dữ liệu phiếu khảo sát", success = false });
            }
        }
    }
}
