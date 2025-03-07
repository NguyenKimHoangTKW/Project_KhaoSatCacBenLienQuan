using CTDT.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.UI.WebControls;
using static CTDT.Areas.Admin.Controllers.NguoiDungAPIController;
namespace CTDT.Areas.Admin.Controllers
{
    public class NguoiDungAPIController : ApiController
    {
        dbSurveyEntities db = new dbSurveyEntities();
        DateTime now = DateTime.UtcNow;
        public int unixTimestamp;
        public NguoiDungAPIController()
        {
            unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }
        #region users
        [HttpPost]
        [Route("api/admin/load_du_lieu_users")]
        public async Task<IHttpActionResult> load_data(FindDataUsers findDataUsers)
        {

            var query = db.users.AsQueryable();
            var get_data = new List<dynamic>();
            if (findDataUsers.id_type_user != 0)
            {
                query = query.Where(x => x.id_typeusers == findDataUsers.id_type_user);
            }
            if (!string.IsNullOrEmpty(findDataUsers.searchTerm))
            {
                string keyword = findDataUsers.searchTerm.ToLower();
                query = query.Where(x =>
                    x.email.ToLower().Contains(keyword) ||
                    x.firstName.ToLower().Contains(keyword) ||
                    x.lastName.ToLower().Contains(keyword) ||
                    x.typeusers.name_typeusers.ToLower().Contains(keyword));
            }
            int totalRecords = await query.CountAsync();
            var fil_data = await query.OrderBy(x => x.id_users)
                .Skip((findDataUsers.page - 1) * findDataUsers.pageSize)
                .Take(findDataUsers.pageSize)
                .ToListAsync();
            foreach (var items in fil_data)
            {
                var get_type = new List<dynamic>();
                if (items.id_typeusers == 3)
                {
                    var get_phan_quyen = await db.phan_quyen_users
                    .Where(x => x.id_users == items.id_users)
                    .Select(x => new
                    {
                        ten_ctdt = x.ctdt.ten_ctdt != null ? x.ctdt.ten_ctdt : "",
                    }).ToListAsync();
                    get_type.AddRange(get_phan_quyen);
                }
                else if (items.id_typeusers == 5)
                {
                    var get_phan_quyen = await db.phan_quyen_users
                    .Where(x => x.id_users == items.id_users)
                    .Select(x => new
                    {
                        ten_khoa = x.khoa_vien_truong.ten_khoa != null ? x.khoa_vien_truong.ten_khoa : ""
                    }).ToListAsync();
                    get_type.AddRange(get_phan_quyen);
                }
                get_data.Add(new
                {
                    id_user = items.id_users,
                    ten_user = items.firstName + " " + items.lastName,
                    email = items.email,
                    quyen_han = items.typeusers.name_typeusers,
                    ngay_cap_nhat = items.ngaycapnhat,
                    ngay_tao = items.ngaytao,
                    thuoc_chuc_quyen = get_type,
                    dang_nhap_lan_cuoi = items.dang_nhap_lan_cuoi
                });
            }
            return Ok(new { data = get_data ,
                totalRecords = totalRecords,
                totalPages = (int)Math.Ceiling((double)totalRecords / findDataUsers.pageSize),
                currentPage = findDataUsers.page
            });
        }
        public class FindDataUsers
        {
            public int id_ctdt { get; set; }
            public int id_type_user { get; set; }
            public int page { get; set; }
            public int pageSize { get; set; }
            public string searchTerm { get; set; }
        }
        [HttpPost]
        [Route("api/admin/add_users")]
        public IHttpActionResult Add(users us)
        {
            var status = "";
            var success = false;
            if (ModelState.IsValid)
            {
                DateTime now = DateTime.UtcNow;
                if (string.IsNullOrEmpty(us.email))
                {
                    status = "Vui lòng nhập địa chỉ Email";
                }
                else if (db.users.SingleOrDefault(t => t.email == us.email) != null)
                {
                    status = "Email đang bị trùng, vui lòng nhập lại email mới";
                }
                else
                {
                    int unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                    us.ngaytao = unixTimestamp;
                    us.ngaycapnhat = unixTimestamp;
                    db.users.Add(us);
                    db.SaveChanges();
                    status = "Thêm mới tài khoản thành công";
                    success = true;
                }
            }
            else
            {
                status = "Dữ liệu không hợp lệ";
            }
            return Ok(new { status = status, success = success });
        }

        [HttpPost]
        [Route("api/admin/del_users")]
        public IHttpActionResult Delete(users us)
        {
            var status = "";
            var user = db.users.Find(us.id_users);
            if (user != null)
            {
                db.users.Remove(user);
                db.SaveChanges();
                status = "Xóa tài khoản thành công";
            }
            else
            {
                status = "Không tìm thấy tài khoản cần xóa";
            }
            return Ok(new { status = status });
        }
        public int MapTenTypeToIDType(string tentype)
        {
            var typeuser = db.typeusers.Where(k => k.name_typeusers == tentype).FirstOrDefault();
            return typeuser?.id_typeusers ?? 0;
        }
        public int MapTenCTDTToIDCTDT(string tenctdt)
        {
            var ctdt = db.ctdt.Where(k => k.ten_ctdt == tenctdt).FirstOrDefault();
            return ctdt?.id_ctdt ?? 0;
        }
        [HttpPost]
        [Route("api/admin/import_excel_users")]
        public IHttpActionResult UploadExcel(HttpPostedFileBase excelFile)
        {
            if (excelFile != null && excelFile.ContentLength > 0)
            {
                try
                {

                    DateTime now = DateTime.UtcNow;
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    using (var package = new ExcelPackage(excelFile.InputStream))
                    {
                        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                        if (worksheet == null)
                        {
                            return Ok(new { status = "Không tìm thấy worksheet trong file Excel" });
                        }
                        int unixTimestamp = (int)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                        for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                        {
                            string Chucvu = worksheet.Cells[row, 2].Value.ToString();
                            int machucvu = MapTenTypeToIDType(Chucvu);
                            string CTDT = worksheet.Cells[row, 3].Value.ToString();
                            int mactdt = MapTenCTDTToIDCTDT(CTDT);
                            var nguoidung = new users
                            {

                                email = worksheet.Cells[row, 1].Text,
                                id_typeusers = machucvu,
                                ngaytao = unixTimestamp,
                                ngaycapnhat = unixTimestamp
                            };

                            db.users.Add(nguoidung);
                        }

                        db.SaveChanges();

                        return Ok(new { status = "Thêm người dùng thành công" });
                    }
                }
                catch (Exception ex)
                {
                    return Ok(new { status = $"Đã xảy ra lỗi: {ex.Message}" });
                }
            }

            return Ok(new { status = "Vui lòng chọn file Excel" });
        }
        #endregion

        #region Phân quyền
        [HttpGet]
        [Route("api/admin/load_chuc_nang_phan_quyen")]
        public IHttpActionResult load_chuc_nang_phan_quyen()
        {
            var type_user = db.typeusers.ToList();
            var data_list = new List<dynamic>();
            foreach (var typeusers in type_user)
            {

                var is_nguoi_dung = new[] { 1 }.Contains(typeusers.id_typeusers);
                var is_Admin = new[] { 2 }.Contains(typeusers.id_typeusers);
                var is_ctdt = new[] { 3 }.Contains(typeusers.id_typeusers);
                var is_khoa = new[] { 5 }.Contains(typeusers.id_typeusers);
                var is_hop_tac_doanh_nghiep = new[] { 6 }.Contains(typeusers.id_typeusers);
                if (is_nguoi_dung)
                {
                    chuc_nang_quyen_nguoi_dung(data_list, typeusers.id_typeusers, typeusers.name_typeusers);
                }
                else if (is_Admin)
                {
                    chuc_nang_quyen_admin(data_list, typeusers.id_typeusers, typeusers.name_typeusers);
                }
                else if (is_ctdt)
                {
                    chuc_nang_quyen_ctdt(data_list, typeusers.id_typeusers, typeusers.name_typeusers);
                }
                else if (is_khoa)
                {
                    chuc_nang_quyen_khoa(data_list, typeusers.id_typeusers, typeusers.name_typeusers);
                }
                else if (is_hop_tac_doanh_nghiep)
                {
                    chuc_nang_quyen_HTDN(data_list, typeusers.id_typeusers, typeusers.name_typeusers);
                }
            }
            return Ok(new { data = data_list });
        }
        private void chuc_nang_quyen_admin(dynamic data_list, int id_typeusers, string name_typeusers)
        {
            var chuc_nang_user = db.chuc_nang_users
                        .Where(x => x.id_typeusers == id_typeusers)
                        .Select(x => new
                        {
                            id_chuc_nang = x.id_chuc_nang,
                            ma_chuc_nang = x.ma_chuc_nang,
                            ten_chuc_nang = x.ten_chuc_nang
                        })
                        .ToList();
            data_list.Add(new
            {
                id_type = id_typeusers,
                ten_quyen = name_typeusers,
                chuc_nang = chuc_nang_user,
                is_admin = true
            });
        }
        private void chuc_nang_quyen_ctdt(dynamic data_list, int id_typeusers, string name_typeusers)
        {
            var chuc_nang_user = db.chuc_nang_users
                        .Where(x => x.id_typeusers == id_typeusers)
                        .Select(x => new
                        {
                            id_chuc_nang = x.id_chuc_nang,
                            ma_chuc_nang = x.ma_chuc_nang,
                            ten_chuc_nang = x.ten_chuc_nang
                        })
                        .ToList();
            var ctdt = db.ctdt
                .Select(x => new
                {
                    ma_ctdt = x.id_ctdt,
                    ten_ctdt = x.ten_ctdt
                })
                .ToList();
            data_list.Add(new
            {
                id_type = id_typeusers,
                ten_quyen = name_typeusers,
                ctdt = ctdt,
                chuc_nang = chuc_nang_user,
                is_ctdt = true
            });
        }
        private void chuc_nang_quyen_khoa(dynamic data_list, int id_typeusers, string name_typeusers)
        {
            var khoa = db.khoa_vien_truong
                        .Select(x => new
                        {
                            ma_khoa = x.id_khoa,
                            ten_khoa = x.ten_khoa
                        })
                        .ToList();
            data_list.Add(new
            {
                id_type = id_typeusers,
                ten_quyen = name_typeusers,
                khoa = khoa,
                is_khoa = true
            });
        }
        private void chuc_nang_quyen_HTDN(dynamic data_list, int id_typeusers, string name_typeusers)
        {
            data_list.Add(new
            {
                id_type = id_typeusers,
                ten_quyen = name_typeusers,
                is_hop_tac_doanh_nghiep = true
            });
        }
        private void chuc_nang_quyen_nguoi_dung(dynamic data_list, int id_typeusers, string name_typeusers)
        {
            data_list.Add(new
            {
                id_type = id_typeusers,
                ten_quyen = name_typeusers,
                is_nguoi_dung = true
            });
        }
        #endregion

        #region Save phân quyền
        [HttpPost]
        [Route("api/admin/save_phan_quyen")]
        public IHttpActionResult save_phan_quyen(PhanQuyen phanQuyen)
        {
            var is_nguoi_dung = new int?[] { 1 }.Contains(phanQuyen.ma_quyen);
            var is_Admin = new int?[] { 2 }.Contains(phanQuyen.ma_quyen);
            var is_ctdt = new int?[] { 3 }.Contains(phanQuyen.ma_quyen);
            var is_khoa = new int?[] { 5 }.Contains(phanQuyen.ma_quyen);
            var is_hop_tac_doanh_nghiep = new int?[] { 6 }.Contains(phanQuyen.ma_quyen);
            var us = db.users.Find(phanQuyen.ma_user);

            if (us != null)
            {
                if (is_nguoi_dung)
                {
                    save_nguoi_dung(us, phanQuyen);
                }
                else if (is_hop_tac_doanh_nghiep)
                {
                    save_hop_tac_doanh_nghiep(us, phanQuyen);
                }
                else if (is_Admin)
                {
                    save_admin(us, phanQuyen);
                }
                else if (is_ctdt)
                {
                    save_ctdt(us, phanQuyen);
                }
                else if (is_khoa)
                {
                    save_khoa(us, phanQuyen);
                }
                return Ok(new { message = "Update thành công." });
            }

            return NotFound();
        }
        public void save_nguoi_dung(users us, PhanQuyen phanQuyen)
        {
            us.id_typeusers = phanQuyen.ma_quyen;
            us.ngaycapnhat = unixTimestamp;
            db.SaveChanges();
            var existingRecords = db.phan_quyen_users.Where(x => x.id_users == us.id_users).ToList();
            if (existingRecords.Any())
            {
                db.phan_quyen_users.RemoveRange(existingRecords);
                db.SaveChanges();
            }
        }

        public void save_hop_tac_doanh_nghiep(users us, PhanQuyen phanQuyen)
        {
            us.id_typeusers = phanQuyen.ma_quyen;
            us.ngaycapnhat = unixTimestamp;
            db.SaveChanges();
            var existingRecords = db.phan_quyen_users.Where(x => x.id_users == us.id_users).ToList();
            if (existingRecords.Any())
            {
                db.phan_quyen_users.RemoveRange(existingRecords);
                db.SaveChanges();
            }
        }

        public void save_admin(users us, PhanQuyen phanQuyen)
        {
            us.id_typeusers = phanQuyen.ma_quyen;
            us.ngaycapnhat = unixTimestamp;
            db.SaveChanges();
            var existingRecords = db.phan_quyen_users.Where(x => x.id_users == us.id_users).ToList();
            if (existingRecords.Any())
            {
                db.phan_quyen_users.RemoveRange(existingRecords);
                db.SaveChanges();
            }
        }

        public void save_ctdt(users us, PhanQuyen phanQuyen)
        {
            var ctdtList = db.ctdt.Where(x => phanQuyen.ma_ctdt.Contains(x.id_ctdt)).ToList();
            us.id_typeusers = phanQuyen.ma_quyen;
            us.ngaycapnhat = unixTimestamp;
            db.SaveChanges();
            var existingRecords = db.phan_quyen_users.Where(x => x.id_users == us.id_users).ToList();
            if (existingRecords.Any())
            {
                db.phan_quyen_users.RemoveRange(existingRecords);
                db.SaveChanges();
            }
            foreach (var check_ctdt in ctdtList)
            {
                var newRecord = new phan_quyen_users
                {
                    id_users = us.id_users,
                    id_ctdt = check_ctdt.id_ctdt,
                    
                };
                db.phan_quyen_users.Add(newRecord);
            }
            db.SaveChanges();
        }


        public void save_khoa(users us, PhanQuyen phanQuyen)
        {
            var khoa = db.khoa_vien_truong.FirstOrDefault(x => x.id_khoa == phanQuyen.ma_khoa);
            us.id_typeusers = phanQuyen.ma_quyen;
            us.ngaycapnhat = unixTimestamp;
            db.SaveChanges();
            var existingRecords = db.phan_quyen_users.Where(x => x.id_users == us.id_users).ToList();
            if (existingRecords.Any())
            {
                db.phan_quyen_users.RemoveRange(existingRecords);
                db.SaveChanges();
            }
            var newRecord = new phan_quyen_users
            {
                id_users = us.id_users,
                id_khoa = khoa.id_khoa
            };
            db.phan_quyen_users.Add(newRecord);
            db.SaveChanges();
        }
        #endregion

        #region Load phân quyền
        [HttpPost]
        [Route("api/admin/load_quyen_user")]
        public IHttpActionResult LoadUserPermissions(PhanQuyen phanquyen)
        {
            var user = db.users.Find(phanquyen.ma_user);
            if (user != null)
            {
                var userPermissions = new List<dynamic>();
                if (user.id_typeusers == 3)
                {
                    var phan_quyen_ctdt = db.phan_quyen_users.Where(x => x.id_users == user.id_users)
                        .Select(x => new
                        {
                            x.id_ctdt
                        }).ToList();
                    userPermissions.Add(new
                    {
                        ma_quyen = user.id_typeusers,
                        ma_ctdt = phan_quyen_ctdt
                    });
                }
                else if (user.id_typeusers == 1)
                {
                    userPermissions.Add(new
                    {
                        ma_quyen = user.id_typeusers,
                    });
                }
                else if (user.id_typeusers == 2)
                {
                    userPermissions.Add(new
                    {
                        ma_quyen = user.id_typeusers,
                    });
                }
                else if (user.id_typeusers == 5)
                {
                    var phan_quyen_khoa = db.phan_quyen_users.Where(x => x.id_users == user.id_users)
                        .Select(x => new
                        {
                            x.id_khoa
                        }).ToList();
                    userPermissions.Add(new
                    {
                        ma_quyen = user.id_typeusers,
                        ma_khoa = phan_quyen_khoa
                    });
                }

                return Ok(new { data = userPermissions });
            }
            return NotFound();
        }
        #endregion
    }
}
