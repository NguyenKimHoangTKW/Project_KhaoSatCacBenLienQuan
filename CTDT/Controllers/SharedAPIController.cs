﻿using CTDT.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace CTDT.Controllers
{
    public class SharedAPIController : ApiController
    {
        dbSurveyEntities db = new dbSurveyEntities();
        [HttpPost]
        [Route("api/load_phieu_by_nam")]
        public async Task<IHttpActionResult> load_pks_by_year(survey survey)
        {
            var pks = await db.survey
                .Where(x => x.id_namhoc == survey.id_namhoc && x.id_hedaotao == survey.id_hedaotao && x.mo_thong_ke == 1)
                .Select(x => new
                {
                    id_phieu = x.surveyID,
                    ten_phieu = x.surveyTitle,
                })
                .ToListAsync();

            var sortedPks = pks.OrderBy(p =>
            {
                var match = System.Text.RegularExpressions.Regex.Match(p.ten_phieu, @"Phiếu (\d+)");
                return match.Success ? int.Parse(match.Groups[1].Value) : int.MaxValue;
            }).ToList();

            if (sortedPks.Count > 0)
            {
                return Ok(new { data = sortedPks, success = true });
            }
            else
            {
                return Ok(new { message = "Không có dữ liệu phiếu khảo sát", success = false });
            }

        }

    }
}
