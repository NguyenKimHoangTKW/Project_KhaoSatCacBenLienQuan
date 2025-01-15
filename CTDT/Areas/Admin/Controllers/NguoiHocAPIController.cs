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

            if (!string.IsNullOrEmpty(sv.searchTerm))
            {
                string keyword = sv.searchTerm.ToLower();
                query = query.Where(x =>
                    x.hovaten.ToLower().Contains(keyword) ||
                    x.ma_sv.ToLower().Contains(keyword) ||
                    x.lop.ma_lop.ToLower().Contains(keyword) ||
                    x.diachi.ToLower().Contains(keyword) ||
                    x.phai.ToLower().Contains(keyword) ||
                    x.sodienthoai.Contains(keyword));
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
            public string searchTerm { get; set; }
        }

        [HttpPost]
        [Route("api/admin/them-moi-nguoi-hoc")]
        public IHttpActionResult add_new(sinhvien sv)
        {
            if (string.IsNullOrEmpty(sv.ma_sv))
            {
                return Ok(new { message = "Không được bỏ trống mã người học", success = false });
            }
            if (string.IsNullOrEmpty(sv.hovaten))
            {
                return Ok(new { message = "Không được bỏ trống tên người học", success = false });
            }
            if (db.sinhvien.FirstOrDefault(x => x.id_sv == sv.id_sv) != null)
            {
                return Ok(new { message = "Mã người học này đã tồn tại", success = false });
            }
            var add_new = new sinhvien
            {
                ma_sv = sv.ma_sv,
                hovaten = sv.hovaten,
                id_lop = sv.id_lop,
                ngaysinh = sv.ngaysinh,
                sodienthoai = sv.sodienthoai,
                diachi = sv.diachi,
                phai = sv.phai,
                namnhaphoc = sv.namnhaphoc,
                namtotnghiep = sv.namtotnghiep,
                description = sv.description,
                ngaytao = unixTimestamp,
                ngaycapnhat = unixTimestamp
            };
            db.sinhvien.Add(add_new);
            db.SaveChanges();
            return Ok(new { message = "Cập nhật dữ liệu thành công", success = true });
        }
        [HttpPost]
        [Route("api/admin/get-info-nguoi-hoc")]
        public async Task<IHttpActionResult> get_info(sinhvien sv)
        {
            var get_info = await db.sinhvien
                .Where(x => x.id_sv == sv.id_sv)
                .Select(x => new
                {
                    x.id_lop,
                    x.ma_sv,
                    x.hovaten,
                    x.ngaysinh,
                    x.sodienthoai,
                    x.diachi,
                    x.phai,
                    x.namnhaphoc,
                    x.namtotnghiep,
                    x.description,
                }).FirstOrDefaultAsync();
            return Ok(get_info);
        }
        [HttpPost]
        [Route("api/admin/update-nguoi-hoc")]
        public IHttpActionResult update_nguoi_hoc(sinhvien sv)
        {
            var get_data = db.sinhvien.FirstOrDefault(x => x.id_sv == sv.id_sv);
            if (string.IsNullOrEmpty(sv.ma_sv))
            {
                return Ok(new { message = "Không được bỏ trống mã người học", success = false });
            }
            if (string.IsNullOrEmpty(sv.hovaten))
            {
                return Ok(new { message = "Không được bỏ trống tên người học", success = false });
            }
            get_data.id_lop = sv.id_lop;
            get_data.ma_sv = sv.ma_sv;
            get_data.hovaten = sv.hovaten;
            get_data.ngaysinh = sv.ngaysinh;
            get_data.sodienthoai = sv.sodienthoai;
            get_data.diachi = sv.diachi;
            get_data.phai = sv.phai;
            get_data.namnhaphoc = sv.namnhaphoc;
            get_data.namtotnghiep = sv.namtotnghiep;
            get_data.description = sv.description;
            get_data.ngaycapnhat = unixTimestamp;
            db.SaveChanges();
            return Ok(new { message = "Cập nhật dữ liệu thành công", success = true });
        }
        [HttpPost]
        [Route("api/admin/delete-nguoi-hoc")]
        public IHttpActionResult delete_nguoi_hoc(sinhvien sv)
        {
            var check_danh_sach_khao_sat = db.nguoi_hoc_khao_sat.Where(x => x.id_sv == sv.id_sv).ToList();
            var check_danh_sach_nguoi_hoc = db.nguoi_hoc_dang_co_hoc_phan.Where(x => x.id_sinh_vien == sv.id_sv).ToList();
            var check_answer = db.answer_response.Where(x => x.id_sv == sv.id_sv).ToList();
            if (check_danh_sach_khao_sat.Any())
            {
                db.nguoi_hoc_khao_sat.RemoveRange(check_danh_sach_khao_sat);
                db.SaveChanges();
            }
            if (check_danh_sach_nguoi_hoc.Any())
            {
                db.nguoi_hoc_dang_co_hoc_phan.RemoveRange(check_danh_sach_nguoi_hoc);
                db.SaveChanges();
            }
            if (check_answer.Any())
            {
                db.answer_response.RemoveRange(check_answer);
                db.SaveChanges();
            }
            var check_nguoi_hoc = db.sinhvien.FirstOrDefault(x => x.id_sv == sv.id_sv);
            db.sinhvien.Remove(check_nguoi_hoc);
            db.SaveChanges();
            return Ok(new { message = "Xóa dữ liệu thành công" });
        }
    }
}
