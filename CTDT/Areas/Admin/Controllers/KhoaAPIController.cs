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
    public class KhoaAPIController : ApiController
    {
        dbSurveyEntities db = new dbSurveyEntities();

        [HttpPost]
        [Route("api/admin/load-danh-sach-khoa")]
        public async Task<IHttpActionResult> load_danh_sach_khoa(khoa items)
        {
            var get_data = await db.khoa
               .Where(x => items.id_namhoc == null || x.id_namhoc == items.id_namhoc)
                .Select(x => new
                {
                    x.id_khoa,
                    x.ma_khoa,
                    x.ten_khoa,
                    x.NamHoc.ten_namhoc,
                    x.ngaycapnhat,
                    x.ngaytao
                }).ToListAsync();
            if (get_data.Any())
            {
                return Ok(new { data = JsonConvert.SerializeObject(get_data), success = true });
            }
            else
            {
                return Ok(new { message = "Chưa có dữ liệu tồn tại", success = false });
            }
        }
        [HttpPost]
        [Route("api/admin/them-moi-khoa")]
        public IHttpActionResult them_moi_khoa(khoa khoa)
        {
            DateTime now = DateTime.UtcNow;
            int unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            if (string.IsNullOrEmpty(khoa.ten_khoa))
            {
                return Ok(new { message = "Không được bỏ trống tên khoa", success = false });
            }
            var newkhoa = new khoa
            {
                ma_khoa = khoa.ma_khoa,
                ten_khoa = khoa.ten_khoa,
                ngaycapnhat = unixTimestamp,
                ngaytao = unixTimestamp
            };
            db.khoa.Add(newkhoa);
            db.SaveChanges();
            return Ok(new { message = "Cập nhật dữ liệu thành công", success = true });
        }
        [HttpPost]
        [Route("api/admin/get-info-khoa")]
        public async Task<IHttpActionResult> get_info_khoa(khoa khoa)
        {
            var get_info = await db.khoa
                .Where(x => x.id_khoa == khoa.id_khoa)
                .Select(x => new
                {
                    x.ma_khoa,
                    x.ten_khoa
                }).FirstOrDefaultAsync();
            return Ok(get_info);
        }
        [HttpPost]
        [Route("api/admin/update-khoa")]
        public IHttpActionResult update_khoa(khoa khoa)
        {
            DateTime now = DateTime.UtcNow;
            int unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            var check_khoa = db.khoa.FirstOrDefault(x => x.id_khoa == khoa.id_khoa);
            if (string.IsNullOrEmpty(khoa.ten_khoa))
            {
                return Ok(new { message = "Không được bỏ trống tên khoa", success = false });
            }
            check_khoa.ma_khoa = khoa.ma_khoa;
            check_khoa.ten_khoa = khoa.ten_khoa;
            check_khoa.ngaycapnhat = unixTimestamp;
            db.SaveChanges();
            return Ok(new { message = "Cập nhật dữ liệu thành công", success = true });
        }
        [HttpPost]
        [Route("api/admin/delete-khoa")]
        public IHttpActionResult delete_khoa(khoa khoa)
        {
            var check_ctdt = db.ctdt.Where(x => x.id_khoa == khoa.id_khoa).ToList();
            var check_khoa = db.khoa.FirstOrDefault(x => x.id_khoa == khoa.id_khoa);
            if (check_ctdt.Any())
            {
                return Ok(new { message = "Khoa này đang tồn tại dữ liệu của chương trình đào tạo, vui lòng xóa chương trình đào tạo trong khoa này trước khi xóa khóa", success = false });
            }
            db.khoa.Remove(check_khoa);
            db.SaveChanges();
            return Ok(new { message = "Cập nhật dữ liệu thành công", success = true });
        }
    }
}
