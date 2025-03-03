using CTDT.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Windows.Media;

namespace CTDT.Areas.Admin.Controllers
{
    public class MonHocAdminAPIController : ApiController
    {
        dbSurveyEntities db = new dbSurveyEntities();
        private int unixTimestamp;
        public MonHocAdminAPIController()
        {
            DateTime now = DateTime.UtcNow;
            unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }
        public class GetItemsMonHoc
        {
            public int namhoc { get; set; }
            public int hocphan { get; set; }
        }
        [HttpPost]
        [Route("api/admin/danh-sach-mon-hoc")]
        public async Task<IHttpActionResult> load_danh_sach_mon_hoc(GetItemsMonHoc items)
        {
            var get_data = await db.mon_hoc
                .Where(x => x.id_nam_hoc == items.namhoc &&
                           (items.hocphan == 0 || x.id_hoc_phan == items.hocphan))
                .OrderByDescending(x => x.id_mon_hoc)
                .Select(x => new
                {
                    value = x.id_mon_hoc,
                    ma_mon_hoc = x.ma_mon_hoc != null ? x.ma_mon_hoc : "",
                    x.ten_mon_hoc,
                    thuoc_lop = x.id_lop != null ? x.lop.ma_lop : "",
                    thuoc_hoc_phan = x.id_hoc_phan != null ? x.hoc_phan.ten_hoc_phan : "",
                    x.ngay_tao,
                    x.ngay_cap_nhat,
                    thuoc_nam = x.NamHoc.ten_namhoc,
                })
                .ToListAsync();
            if (get_data.Count > 0)
            {
                return Ok(new { data = get_data, success = true });
            }
            else
            {
                return Ok(new { message = "Không có dữ liệu", success = false });
            }
        }
        [HttpPost]
        [Route("api/admin/them-moi-mon-hoc")]
        public async Task<IHttpActionResult> add_new(mon_hoc value)
        {
            if (db.mon_hoc.FirstOrDefault(x => x.ma_mon_hoc == value.ma_mon_hoc && x.ten_mon_hoc == value.ten_mon_hoc) != null)
            {
                return Ok(new { message = "Môn học và mã môn học này đã tồn tại, vui lòng kiểm tra lại dữ liệu", success = false });
            }

            if (string.IsNullOrEmpty(value.ma_mon_hoc))
            {
                return Ok(new { message = "Không được bỏ trống mã môn học", success = false });
            }
            if (string.IsNullOrEmpty(value.ten_mon_hoc))
            {
                return Ok(new { message = "Không được bỏ trống tên môn học", success = false });
            }
            value.ngay_tao = unixTimestamp;
            value.ngay_cap_nhat = unixTimestamp;
            db.mon_hoc.Add(value);
            await db.SaveChangesAsync();
            return Ok(new { message = "Cập nhật dữ liệu thành công", success = true });
        }
        [HttpPost]
        [Route("api/admin/info-mon-hoc")]
        public async Task<IHttpActionResult> info(mon_hoc value)
        {
            if (value.id_mon_hoc == null)
            {
                return Ok(new { message = "Không tìm thấy dữ liệu", success = false });
            }
            var get_data = await db.mon_hoc
                .Where(x => x.id_mon_hoc == value.id_mon_hoc)
                .Select(x => new
                {
                    x.ma_mon_hoc,
                    x.ten_mon_hoc,
                    lop = x.id_lop,
                    hoc_phan = x.id_hoc_phan,
                    nam_hoc = x.id_nam_hoc,
                }).FirstOrDefaultAsync();
            return Ok(new { data = get_data, success = true });
        }
        [HttpPost]
        [Route("api/admin/update-mon-hoc")]
        public async Task<IHttpActionResult> update(mon_hoc value)
        {
            if (value.id_mon_hoc == null)
            {
                return Ok(new { message = "Không tìm thấy dữ liệu", success = false });
            }
            var data = await db.mon_hoc
                .FirstOrDefaultAsync(x => x.id_mon_hoc == value.id_mon_hoc);
            data.ma_mon_hoc = value.ma_mon_hoc;
            data.ten_mon_hoc = value.ten_mon_hoc;
            data.id_lop = value.id_lop;
            data.id_hoc_phan = value.id_hoc_phan;
            data.id_nam_hoc = value.id_nam_hoc;
            data.ngay_cap_nhat = unixTimestamp;
            await db.SaveChangesAsync();
            return Ok(new { message = "Cập nhật dữ liệu thành công", success = true });
        }
        [HttpPost]
        [Route("api/admin/delete-mon-hoc")]
        public IHttpActionResult delete(mon_hoc value)
        {
            if (value.id_mon_hoc == null)
            {
                return Ok(new { message = "Không tìm thấy dữ liệu", success = false });
            }
            var check_nguoi_hoc_co_hoc_phan = db.nguoi_hoc_dang_co_hoc_phan
                .Where(x => x.id_mon_hoc == value.id_mon_hoc)
                .ToList();
            if (check_nguoi_hoc_co_hoc_phan.Any())
            {
                return Ok(new { message = "Môn học này đang tồn tại người học, chỉ có thể xóa cứng trong chức năng xóa dữ liệu theo năm", success = false });
            }
            var check_mon_hoc = db.mon_hoc.FirstOrDefault(x => x.id_mon_hoc == value.id_mon_hoc);
            db.mon_hoc.Remove(check_mon_hoc);
            db.SaveChanges();
            return Ok(new { message = "Cập nhật dữ liệu thành công", success = true });
        }
    }
}
