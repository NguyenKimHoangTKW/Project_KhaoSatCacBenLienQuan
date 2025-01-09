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
    public class LopAPIController : ApiController
    {
        dbSurveyEntities db = new dbSurveyEntities();
        private int unixTimestamp;
        public LopAPIController()
        {
            DateTime now = DateTime.UtcNow;
            unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }
        [HttpPost]
        [Route("api/admin/danh-sach-lop")]
        public async Task<IHttpActionResult> danh_sach_lop(lop lop)
        {
            var query = db.lop.AsQueryable();
            if (lop.id_ctdt != 0)
            {
                query = query.Where(x => x.id_ctdt == lop.id_ctdt);
            }
            var get_data = await query
                .Select(x => new
                {
                    x.id_lop,
                    x.ma_lop,
                    x.ctdt.ten_ctdt,
                    x.ngaytao,
                    x.ngaycapnhat,
                    x.status
                }).ToListAsync();
            if (get_data.Any())
            {
                return Ok(new { data = get_data, success = true });
            }
            else
            {
                return Ok(new { message = "Chưa có dữ liệu tồn tại", success = false });
            }
        }
        [HttpPost]
        [Route("api/admin/them-moi-lop")]
        public IHttpActionResult them_moi_lop(lop lop)
        {
            if (string.IsNullOrEmpty(lop.ma_lop))
            {
                return Ok(new { message = "Không được bỏ trống tên lớp", success = false });
            }
            var add_new = new lop
            {
                ma_lop = lop.ma_lop,
                id_ctdt = lop.id_ctdt,
                ngaytao = unixTimestamp,
                ngaycapnhat = unixTimestamp,
                status = true
            };
            db.lop.Add(add_new);
            db.SaveChanges();
            return Ok(new { message = "Cập nhật dữ liệu thành công", success = true });
        }
        [HttpPost]
        [Route("api/admin/get-info-lop")]
        public async Task<IHttpActionResult> get_info_lop(lop lop)
        {
            var get_info = await db.lop
                .Where(x => x.id_lop == lop.id_lop)
                .Select(x => new
                {
                    x.ma_lop,
                    x.id_ctdt,
                }).FirstOrDefaultAsync();
            return Ok(get_info);
        }
        [HttpPost]
        [Route("api/admin/update-lop")]
        public IHttpActionResult update_lop(lop lop)
        {
            var check_lop = db.lop.FirstOrDefault(x => x.id_lop == lop.id_lop);
            if (string.IsNullOrEmpty(lop.ma_lop))
            {
                return Ok(new { message = "Không được bỏ trống tên lớp", success = false });
            }
            check_lop.ma_lop = lop.ma_lop;
            check_lop.id_ctdt = lop.id_ctdt;
            check_lop.ngaycapnhat = unixTimestamp;
            db.SaveChanges();
            return Ok(new { message = "Cập nhật dữ liệu thành công", success = true });
        }
        [HttpPost]
        [Route("api/admin/delete-lop-thong-thuong")]
        public IHttpActionResult delete_lop(lop lop)
        {
            var check_sinh_vien = db.sinhvien.Where(x => x.id_lop == lop.id_lop).ToList();
            if (check_sinh_vien.Any())
            {
                return Ok(new { message = "Lớp này đang tồn tại người học, không thể xóa thông thường", success = false });
            }
            var check_lop = db.lop.FirstOrDefault(x => x.id_lop == lop.id_lop);
            db.lop.Remove(check_lop);
            db.SaveChanges();
            return Ok(new { message = "Xóa dữ liệu thành công", success = true });
        }
        [HttpPost]
        [Route("api/admin/delete-lop-cung")]
        public IHttpActionResult delete_lop_cung(lop lop)
        {
            var check_sinh_vien = db.sinhvien.Where(x => x.id_lop == lop.id_lop).ToList();
            if (check_sinh_vien.Any())
            {
                db.sinhvien.RemoveRange(check_sinh_vien);
                db.SaveChanges();
            }
            var check_lop = db.lop.FirstOrDefault(x => x.id_lop == lop.id_lop);
            db.lop.Remove(check_lop);
            db.SaveChanges();
            return Ok(new { message = "Xóa dữ liệu thành công", success = true });
        }
    }
}
