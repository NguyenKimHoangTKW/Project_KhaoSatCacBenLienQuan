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

namespace CTDT.Areas.Admin.Controllers
{
    public class CTDTAPIController : ApiController
    {
        dbSurveyEntities db = new dbSurveyEntities();
        private int unixTimestamp;
        public CTDTAPIController()
        {
            DateTime now = DateTime.UtcNow;
            unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }
        [HttpPost]
        [Route("api/admin/danh-sach-ctdt")]
        public async Task<IHttpActionResult> danh_sach_ctdt(ctdt ctdt)
        {
            var get_data = db.ctdt.AsQueryable();

            if (ctdt.id_hdt != 0)
            {
                get_data = get_data.Where(x => x.id_hdt == ctdt.id_hdt);
            }
            if(ctdt.id_bo_mon != 0)
            {
                get_data = get_data.Where(x => x.id_bo_mon == ctdt.id_bo_mon);
            }
            var query = await get_data
                .Select(x => new
                {
                    x.id_ctdt,
                    ma_ctdt = x.ma_ctdt != null ? x.ma_ctdt :"",
                    x.ten_ctdt,
                    ten_hedaotao = x.id_hdt != null ? x.hedaotao.ten_hedaotao:"",
                    ten_bo_mon = x.id_bo_mon != null ? x.bo_mon.ten_bo_mon : "",
                    x.ngaytao,
                    x.ngaycapnhat
                }).ToListAsync();
            if (query.Any())
            {
                return Ok(new { data = JsonConvert.SerializeObject(query), success = true });
            }
            else
            {
                return Ok(new { message = "Chưa có dữ liệu tồn tại", success = false });
            }
        }
        [HttpPost]
        [Route("api/admin/them-moi-ctdt")]
        public IHttpActionResult them_moi_ctdt(ctdt ctdt)
        {
            if (string.IsNullOrEmpty(ctdt.ten_ctdt))
            {
                return Ok(new { message = "Không được bỏ trống tên ctdt", success = false });
            }
            var add_data = new ctdt
            {
                ma_ctdt = ctdt.ma_ctdt,
                ten_ctdt = ctdt.ten_ctdt,
                id_hdt = ctdt.id_hdt,
                ngaycapnhat = unixTimestamp,
                ngaytao = unixTimestamp
            };
            db.ctdt.Add(add_data);
            db.SaveChanges();
            return Ok(new { message = "Cập nhật dữ liệu thành công", success = true });
        }
        [HttpPost]
        [Route("api/admin/get-info-ctdt")]
        public async Task<IHttpActionResult> get_info_ctdt(ctdt ctdt)
        {
            var get_info = await db.ctdt
                .Where(x => x.id_ctdt == ctdt.id_ctdt)
                .Select(x => new
                {
                    x.ma_ctdt,
                    x.ten_ctdt,
                    x.id_hdt
                }).FirstOrDefaultAsync();
            return Ok(get_info);
        }
        [HttpPost]
        [Route("api/admin/cap-nhat-ctdt")]
        public IHttpActionResult update_ctdt(ctdt ctdt)
        {
            var check_ctdt = db.ctdt.FirstOrDefault(x => x.id_ctdt == ctdt.id_ctdt);
            if (string.IsNullOrEmpty(ctdt.ten_ctdt))
            {
                return Ok(new { message = "Không được bỏ trống tên ctđt", success = false });
            }
            check_ctdt.ma_ctdt = ctdt.ma_ctdt;
            check_ctdt.ten_ctdt = ctdt.ten_ctdt;
            check_ctdt.id_hdt = ctdt.id_hdt;
            check_ctdt.ngaytao = unixTimestamp;
            check_ctdt.ngaycapnhat = unixTimestamp;
            db.SaveChanges();
            return Ok(new { message = "Cập nhật dữ liệu thành công", success = true });
        }
        [HttpPost]
        [Route("api/admin/delete-ctdt")]
        public IHttpActionResult delete_ctdt(ctdt ctdt)
        {
            var check_lop = db.lop.Where(x => x.id_ctdt == ctdt.id_ctdt).ToList();
            if (check_lop.Any())
            {
                return Ok(new { message = "Chương trình đào tạo này đang tồn tại lớp học, vui lòng vào mục lớp học kiểm tra lại!", success = false });
            }
            var check_ctdt = db.ctdt.FirstOrDefault(x => x.id_ctdt == ctdt.id_ctdt);
            db.ctdt.Remove(check_ctdt);
            db.SaveChanges();
            return Ok(new { message = "Xóa dữ liệu thành công", success = true });
        }
    }
}
