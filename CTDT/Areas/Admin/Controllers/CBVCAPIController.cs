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
    public class CBVCAPIController : ApiController
    {
        dbSurveyEntities db = new dbSurveyEntities();
        private int unixTimestamp;
        public CBVCAPIController()
        {
            DateTime now = DateTime.UtcNow;
            unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }
        [HttpPost]
        [Route("api/admin/danh-sach-cbvc")]
        public async Task<IHttpActionResult> danh_sach_cbvc(CanBoVienChuc cbvc)
        {
            var query = db.CanBoVienChuc.AsQueryable();
            if (cbvc.id_chuongtrinhdaotao != 0)
            {
                query = query.Where(x => x.id_chuongtrinhdaotao == cbvc.id_chuongtrinhdaotao);
            }
            if (cbvc.id_namhoc != 0)
            {
                query = query.Where(x => x.id_namhoc == cbvc.id_namhoc);
            }
            if (cbvc.id_donvi != 0)
            {
                query = query.Where(x => x.id_donvi == cbvc.id_donvi);
            }
            if (cbvc.id_chucvu != 0)
            {
                query = query.Where(x => x.id_chucvu == cbvc.id_chucvu);
            }
            var rawData = await query.ToListAsync();
            var get_data = rawData
                .Select(x => new
                {
                    x.id_CBVC,
                    MaCBVC = x.MaCBVC != null ? x.MaCBVC : " ",
                    x.TenCBVC,
                    NgaySinh = x.NgaySinh.HasValue ? x.NgaySinh.Value.ToString("dd-MM-yyyy") : " ",
                    Email = x.Email != null ? x.Email : " ",
                    donvi = x.id_donvi != null ? x.DonVi.name_donvi : " ",
                    chucvu = x.id_chucvu != null ? x.ChucVu.name_chucvu : " ",
                    ctdt = x.id_chuongtrinhdaotao != null ? x.ctdt.ten_ctdt : " ",
                    NamHoc = x.id_namhoc != null ? x.NamHoc.ten_namhoc : " ",
                    descripton = x.description != null ? x.description : " "
                }).ToList();
            if (get_data.Count > 0)
            {
                return Ok(new { data = get_data, success = true });
            }
            else
            {
                return Ok(new { message = "Không tồn tại dữ liệu", success = false });
            }
        }
    }
}
