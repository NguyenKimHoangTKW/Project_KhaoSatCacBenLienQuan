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
    }
}
