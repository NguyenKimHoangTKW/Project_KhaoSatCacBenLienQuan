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
        public async Task<IHttpActionResult> LoadDanhSachNguoiHoc(Data_SV sv)
        {
            var query = db.sinhvien.AsQueryable();

            if (sv.id_lop != 0)
            {
                query = query.Where(x => x.id_lop == sv.id_lop);
            }

            int totalRecords = await query.CountAsync();

            var pagedData = await query
                .OrderBy(x => x.id_sv) 
                .Skip((sv.page - 1) * sv.pageSize)
                .Take(sv.pageSize) 
                .ToListAsync();

            var getData = pagedData.Select(x => new
            {
                x.id_sv,
                x.lop.ma_lop,
                x.ma_sv,
                x.hovaten,
                ngaysinh = x.ngaysinh?.ToString("dd-MM-yyyy") ?? " ",
                sodienthoai = x.sodienthoai ?? " ",
                diachi = x.diachi ?? " ",
                phai = x.phai ?? " ",
                namnhaphoc = x.namnhaphoc ?? " ",
                namtotnghiep = x.namtotnghiep ?? " ",
                x.ngaycapnhat,
                x.ngaytao,
                description = x.description ?? " "
            }).ToList();

            return Ok(new
            {
                data = getData,
                success = true,
                totalRecords = totalRecords,
                totalPages = (int)Math.Ceiling((double)totalRecords / sv.pageSize),
                currentPage = sv.page
            });
        }

        public class Data_SV
        {
            public int id_lop { get; set; }
            public int page { get; set; }
            public int pageSize { get; set; }
        }

    }
}
