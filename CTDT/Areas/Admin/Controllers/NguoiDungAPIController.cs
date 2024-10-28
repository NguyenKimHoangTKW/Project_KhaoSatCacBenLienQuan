using CTDT.Models;
using CTDT.Models.Admin;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.UI.WebControls;
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
        public IHttpActionResult load_data(FindDataUsers findDataUsers)
        {

            var query = db.users.AsQueryable();
            if (findDataUsers.id_ctdt != 0)
            {
                query = query.Where(x => x.id_ctdt == findDataUsers.id_ctdt);
            }
            if (findDataUsers.id_donvi != 0)
            {
                query = query.Where(x => x.id_donvi == findDataUsers.id_donvi);
            }
            if (findDataUsers.id_type_user != 0)
            {
                query = query.Where(x => x.id_typeusers == findDataUsers.id_type_user);
            }

            var fil_data = query
                .Select(x => new
                {
                    id_user = x.id_users,
                    ten_user = x.firstName + " " + x.lastName,
                    email = x.email,
                    quyen_han = x.typeusers.name_typeusers,
                    ngay_cap_nhat = x.ngaycapnhat,
                    ngay_tao = x.ngaytao,
                    dang_nhap_lan_cuoi = x.dang_nhap_lan_cuoi,
                    don_vi = x.DonVi != null ? x.DonVi.name_donvi : "",
                    ctdt = x.ctdt != null ? x.ctdt.ten_ctdt : ""
                }).ToList();
            return Ok(new { data = fil_data });
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
                                id_ctdt = mactdt,
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
            var khoa = db.khoa
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
            us.id_ctdt = null;
            us.id_khoa = null;
            us.id_hdt = null;
            us.ngaycapnhat = unixTimestamp;
            db.SaveChanges();
        }

        public void save_hop_tac_doanh_nghiep(users us, PhanQuyen phanQuyen)
        {
            us.id_typeusers = phanQuyen.ma_quyen;
            us.id_ctdt = null;
            us.id_khoa = null;
            us.id_hdt = null;
            us.ngaycapnhat = unixTimestamp;
            db.SaveChanges();
        }

        public void save_admin(users us, PhanQuyen phanQuyen)
        {
            us.id_typeusers = phanQuyen.ma_quyen;
            us.id_ctdt = null;
            us.id_khoa = null;
            us.id_hdt = null;
            us.ngaycapnhat = unixTimestamp;
            db.SaveChanges();
        }

        public void save_ctdt(users us, PhanQuyen phanQuyen)
        {
            var ctdt = db.ctdt.FirstOrDefault(x => x.id_ctdt == phanQuyen.ma_ctdt);
            us.id_typeusers = phanQuyen.ma_quyen;
            us.id_ctdt = phanQuyen.ma_ctdt;
            us.id_khoa = null;
            us.id_hdt = ctdt?.id_hdt;
            us.ngaycapnhat = unixTimestamp;
            db.SaveChanges();
        }

        public void save_khoa(users us, PhanQuyen phanQuyen)
        {
            var khoa = db.khoa.FirstOrDefault(x => x.id_khoa == phanQuyen.ma_khoa);
            us.id_typeusers = phanQuyen.ma_quyen;
            us.id_ctdt = phanQuyen.ma_ctdt;
            us.id_khoa = khoa?.id_khoa;
            us.id_hdt = khoa?.ctdt.FirstOrDefault()?.id_hdt;
            us.ngaycapnhat = unixTimestamp;
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
                var userPermissions = new
                {
                    ma_quyen = user.id_typeusers,
                    ma_ctdt = user.id_ctdt,
                    ma_khoa = user.id_khoa,
                    id_hdt = user.id_hdt
                };

                return Ok(new { data = userPermissions });
            }

            return NotFound();
        }
        #endregion
    }
}
