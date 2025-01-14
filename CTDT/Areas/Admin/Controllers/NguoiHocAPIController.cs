using CTDT.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace CTDT.Areas.Admin.Controllers
{
    public class NguoiHocAPIController : ApiController
    {
        dbSurveyEntities db = new dbSurveyEntities();
        private int unixTimestamp;
        public NguoiHocAPIController()
        {
            DateTime now = DateTime.UtcNow;
            unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        [HttpPost]
        [Route("api/admin/danh-sach-nguoi-hoc")]
        public async Task<IHttpActionResult> load_danh_sach_nguoi_hoc(sinhvien sv)
        {
            var query = db.sinhvien.AsQueryable();
            if (sv.id_lop != 0)
            {
                query = query.Where(x => x.id_lop == sv.id_lop);
            }
            var rawData = await query.ToListAsync();

            var get_data =  rawData
                .Select(x => new
                {
                    x.id_sv,
                    x.lop.ma_lop,
                    x.ma_sv,
                    x.hovaten,
                    ngaysinh = x.ngaysinh.HasValue ? x.ngaysinh.Value.ToString("dd-MM-yyyy") : " ", 
                    sodienthoai = x.sodienthoai != null ? x.sodienthoai : " ",
                    diachi = x.diachi != null ? x.diachi : " ",
                    phai = x.phai != null ? x.phai : " ",
                    namnhaphoc = x.namnhaphoc != null ? x.namnhaphoc : " ",
                    namtotnghiep = x.namtotnghiep != null ? x.namtotnghiep : " ",
                    x.ngaycapnhat,
                    x.ngaytao,
                    description = x.description != null ? x.description : " " 
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
