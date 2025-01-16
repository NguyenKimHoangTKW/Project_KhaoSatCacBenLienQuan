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
        public async Task<IHttpActionResult> danh_sach_cbvc(Data_CBVC cbvc)
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
            if (!string.IsNullOrEmpty(cbvc.searchTerm))
            {
                string keyword = cbvc.searchTerm.ToLower();
                query = query.Where(x =>
                x.MaCBVC.ToLower().Contains(keyword) ||
                x.TenCBVC.ToLower().Contains(keyword) ||
                x.Email.ToLower().Contains(keyword) ||
                x.DonVi.name_donvi.ToLower().Contains(keyword) ||
                x.ChucVu.name_chucvu.ToLower().Contains(keyword) ||
                x.ctdt.ten_ctdt.ToLower().Contains(keyword) ||
                x.NamHoc.ten_namhoc.ToLower().Contains(keyword) ||
                x.description.ToLower().Contains(keyword));
            }
            int totalRecords = await query.CountAsync();
            var pagedData = await query
               .OrderBy(x => x.id_CBVC)
               .Skip((cbvc.page - 1) * cbvc.pageSize)
               .Take(cbvc.pageSize)
               .ToListAsync();
            var get_data = pagedData
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
                return Ok(new
                {
                    data = get_data,
                    success = true,
                    totalRecords = totalRecords,
                    totalPages = (int)Math.Ceiling((double)totalRecords / cbvc.pageSize),
                    currentPage = cbvc.page
                });
            }
            else
            {
                return Ok(new { message = "Không tồn tại dữ liệu", success = false });
            }
        }

        public class Data_CBVC
        {
            public int id_chuongtrinhdaotao { get; set; }
            public int id_namhoc { get; set; }
            public int id_donvi { get; set; }
            public int id_chucvu { get; set; }
            public int page { get; set; }
            public int pageSize { get; set; }
            public string searchTerm { get; set; }
        }
        [HttpPost]
        [Route("api/admin/them-moi-cbvc")]
        public IHttpActionResult add_new(CanBoVienChuc cbvc)
        {
            if (string.IsNullOrEmpty(cbvc.TenCBVC))
            {
                return Ok(new { message = "Không được bỏ trống tên cán bộ viên chức/giảng viên", success = false });
            }
            if (db.CanBoVienChuc.FirstOrDefault(x => x.MaCBVC == cbvc.MaCBVC && x.TenCBVC == cbvc.TenCBVC) != null)
            {
                return Ok(new { message = "Cán bộ viên chức/giảng viên này đã tồn tại", success = false });
            }
            var add_new = new CanBoVienChuc
            {
                MaCBVC = cbvc.MaCBVC,
                TenCBVC = cbvc.TenCBVC,
                NgaySinh = cbvc.NgaySinh,
                Email = cbvc.Email,
                id_donvi = cbvc.id_donvi,
                id_chucvu = cbvc.id_chucvu,
                id_chuongtrinhdaotao = cbvc.id_chuongtrinhdaotao,
                id_namhoc = cbvc.id_namhoc,
                description = cbvc.description,
            };
            db.CanBoVienChuc.Add(add_new);
            db.SaveChanges();
            return Ok(new { message = "Cập nhật dữ liệu thành công", success = true });
        }
    }
}
