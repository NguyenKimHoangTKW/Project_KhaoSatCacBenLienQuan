﻿$(document).ready(function () {
    load_data()
    $("#btnSave").click(function () {
        AddUser();
    });

    $('#ChucVu, #Edit_ChucVu').change(function () {
        toggleCTDTDropdown($(this).attr('id'));
    });

    $("#btnFilter").click(function () {
        load_data()
    });
    $(document).on("click", "#btnEdit", function () {
        var MaUser = $(this).data('id');
        GetByID(MaUser);
    });

    $(document).on("click", "#btnSaveChange", function () {
        EditUser();
    });

    $(document).on("click", "#btnDel", function () {
        var id = $(this).data('id');
        var name = $(this).closest("tr").find("td:eq(2)").text();

        Swal.fire({
            icon: 'warning',
            title: 'Bạn có chắc muốn xóa?',
            text: "Bạn đang cố gắng xóa tài khoản '" + name + "'",
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Xóa',
            cancelButtonText: 'Hủy'
        }).then((result) => {
            if (result.isConfirmed) {
                DelNguoiDung(id);
            }
        });
    });

    $('#importExcelForm').on('submit', function (e) {
        e.preventDefault();
        var formData = new FormData(this);
        $.ajax({
            url: '/Admin/NguoiDung/UploadExcel',
            type: 'POST',
            data: formData,
            contentType: false,
            processData: false,
            success: function (response) {
                if (response.status.includes('Thành công')) {
                    alert(response.status);
                    $('#importExcelModal').modal('hide');
                    LoadData(currentPage);
                } else {
                    alert(response.status);
                }
            },
            error: function (xhr, status, error) {
                alert('Đã xảy ra lỗi: ' + error);
            }
        });
    });
});

function toggleCTDTDropdown(chucVuId) {
    var selectedRole = $('#' + chucVuId).val();
    if (selectedRole == 3) {
        $('#' + chucVuId).closest('.form-group').find('#ctdtContainer').show();
        $('#' + chucVuId).closest('.form-group').find('#donviContainer').hide();
    }
    else if (selectedRole == 4) {
        $('#' + chucVuId).closest('.form-group').find('#ctdtContainer').hide();
        $('#' + chucVuId).closest('.form-group').find('#donviContainer').show();
    }
    else {
        $('#' + chucVuId).closest('.form-group').find('#ctdtContainer').hide();
        $('#' + chucVuId).closest('.form-group').find('#donviContainer').hide();
    }
};

async function load_data() {
    var ctdt_select = $('#FiterCTDT').val();
    var donvi_select = $('#FilterDonVi').val();
    var trangthai_select = $('#FilterTrangThai').val();

    const res = await $.ajax({
        url: '/Admin/NguoiDung/load_data',
        type: 'POST',
        data: {
            ctdt: ctdt_select,
            donvi: donvi_select,
            trangthaiuser: trangthai_select
        }
    });
    if (res && res.data.length > 0) {
        var body = $('#load_data_table');
        var html = "";
        if ($.fn.DataTable.isDataTable('#load_data_table')) {
            $('#load_data_table').DataTable().clear().destroy();
        }
        body.empty();
        html += "<thead>";
        html += "<tr>";
        html += "<th>Số Thứ Tự</th>";
        html += "<th>ID người dùng</th>";
        html += "<th>Tên người dùng</th>";
        html += "<th>Email</th>";
        html += "<th>Quyền hạn</th>";
        html += "<th>Ngày tạo</th>";
        html += "<th>Ngày cập nhật</th>";
        html += "<th>Đơn vị</th>";
        html += "<th>Chương trình đào tạo</th>";
        html += "<th>Chức năng</th>";
        html += "</tr>";
        html += "</thead>";
        html += "<tbody>";
        res.data.forEach(function (items, index) {
            html += "<tr>";
            html += `<td>${index + 1}</td>`;
            html += `<td>${items.id_user}</td>`;
            html += `<td>${items.ten_user}</td>`;
            html += `<td>${items.email}</td>`;
            html += `<td>${items.quyen_han}</td>`;
            html += `<td>${unixTimestampToDate(items.ngay_cap_nhat)}</td>`;
            html += `<td>${unixTimestampToDate(items.ngay_tao)}</td>`;
            html += `<td>${items.don_vi}</td>`;
            html += `<td>${items.ctdt}</td>`;
            html += `<td>`;
            html += `<button class="btn  btn-hover btn-sm btn-rounded pull-right" data-toggle='modal' data-target='#ModalEditNguoiDung' id="btnEdit" data-id="${items.id_user}">Cấp quyền</button>`;
            html += `<button class="btn  btn-hover btn-sm btn-rounded pull-right" id="btnDel" data-id="${items.id_user}">Xóa</button>`; 
            html += `</td>`;
            html += "</tr>";
        })
        html += "</tbody>";
        body.html(html);
        $('#load_data_table').DataTable({
            pageLength: 10,
            lengthMenu: [5, 10, 25, 50, 100],
            ordering: true,
            searching: true,
            autoWidth: false,
            responsive: true,
            language: {
                paginate: {
                    next: "Next",
                    previous: "Previous"
                },
                search: "Search",
                lengthMenu: "Show _MENU_ entries"
            },
            dom: "Bfrtip",
            buttons: [
                {
                    extend: 'csv',
                    title: 'Danh sách người dùng - CSV'
                },
                {
                    extend: 'excel',
                    title: 'Danh sách người dùng - Excel'
                },
                {
                    extend: 'pdf',
                    title: 'Danh sách người dùng PDF'
                },
                {
                    extend: 'print',
                    title: 'Danh sách người dùng'
                }
            ]
        });

    }

}
function AddUser() {
    var Email = $("#Email").val();
    var ChucVu = $('#ChucVu').val();
    var CTDT = ChucVu == '3' ? $('#CTDT').val() : null;
    var DonVi = ChucVu == '4' ? $("#DonVi").val() : null;

    if (CTDT) {
        $.ajax({
            url: '/Admin/NguoiDung/GetIdHdt',
            type: 'GET',
            dataType: 'JSON',
            data: { id_ctdt: CTDT },
            success: function (response) {
                if (response.success) {
                    var idHdt = response.id_hdt;

                    $.ajax({
                        url: '/Admin/NguoiDung/Add',
                        type: 'POST',
                        dataType: 'JSON',
                        contentType: "application/json; charset=utf-8",
                        data: JSON.stringify({
                            email: Email,
                            id_typeusers: ChucVu,
                            id_ctdt: CTDT,
                            id_donvi: DonVi,
                            id_hdt: idHdt
                        }),
                        success: function (response) {
                            if (response.success) {
                                Swal.fire({
                                    icon: 'success',
                                    title: response.status,
                                    showConfirmButton: false,
                                    timer: 2000
                                });
                                LoadData(currentPage);
                                $('#bd-example-modal-lg').modal('hide');
                            } else {
                                Swal.fire({
                                    icon: 'error',
                                    title: 'Oops...',
                                    text: response.status
                                });
                            }
                        },
                        error: function (response) {
                            Swal.fire({
                                icon: 'error',
                                title: 'Oops...',
                                text: 'Đã có lỗi xảy ra. Vui lòng thử lại sau.'
                            });
                        }
                    });
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Oops...',
                        text: response.message
                    });
                }
            },
            error: function (response) {
                Swal.fire({
                    icon: 'error',
                    title: 'Oops...',
                    text: 'Đã có lỗi xảy ra. Vui lòng thử lại sau.'
                });
            }
        });
    } else {
        $.ajax({
            url: '/Admin/NguoiDung/Add',
            type: 'POST',
            dataType: 'JSON',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({
                email: Email,
                id_typeusers: ChucVu,
                id_ctdt: CTDT,
                id_donvi: DonVi
            }),
            success: function (response) {
                if (response.success) {
                    Swal.fire({
                        icon: 'success',
                        title: response.status,
                        showConfirmButton: false,
                        timer: 2000
                    });
                    LoadData(currentPage);
                    $('#bd-example-modal-lg').modal('hide');
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Oops...',
                        text: response.status
                    });
                }
            },
            error: function (response) {
                Swal.fire({
                    icon: 'error',
                    title: 'Oops...',
                    text: 'Đã có lỗi xảy ra. Vui lòng thử lại sau.'
                });
            }
        });
    }
}


function EditUser() {
    var id_users = $('#Edit_ID').val();
    var name = $('#Edit_TenNguoiDung').val();
    var email = $('#Edit_Email').val();
    var id_typeusers = $('#Edit_ChucVu').val();
    var id_ctdt = id_typeusers == 3 ? $('#Edit_CTDT').val() : null;
    var id_donvi = id_typeusers == 4 ? $('#Edit_Donvi').val() : null;
    var ngaytao = $('#change_Ngay_Tao').val();

    var users = {
        id_users: id_users,
        name: name,
        email: email,
        id_typeusers: id_typeusers,
        id_ctdt: id_ctdt,
        id_donvi: id_donvi,
        ngaytao: ngaytao
    };

    function editUserRequest(idHdt) {
        users.id_hdt = idHdt;

        $.ajax({
            type: 'POST',
            url: '/Admin/NguoiDung/Edit',
            dataType: 'JSON',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify(users),
            success: function (response) {
                Swal.fire({
                    icon: 'success',
                    title: response.status,
                    showConfirmButton: false,
                    timer: 2000
                });
                LoadData(currentPage);
            },
            error: function (response) {
                Swal.fire({
                    icon: 'error',
                    title: 'Oops...',
                    text: 'Cập nhật thông tin tài khoản thất bại'
                });
            }
        });
    }

    if (id_ctdt) {
        $.ajax({
            url: '/Admin/NguoiDung/GetIdHdt',
            type: 'GET',
            dataType: 'JSON',
            data: { id_ctdt: id_ctdt },
            success: function (res) {
                var idHdt = res.success ? res.id_hdt : null;
                editUserRequest(idHdt);
            },
            error: function () {
                Swal.fire({
                    icon: 'error',
                    title: 'Oops...',
                    text: 'Không thể lấy ID HĐT'
                });
            }
        });
    } else {
        editUserRequest(null);
    }
}


function DelNguoiDung(id) {
    $.ajax({
        type: 'POST',
        url: '/Admin/NguoiDung/Delete',
        data: JSON.stringify({ id: id }),
        contentType: 'application/json; charset=utf-8',
        success: function (response) {
            Swal.fire({
                icon: 'success',
                title: response.status,
                showConfirmButton: false,
                timer: 2000
            });
            LoadData(currentPage);
        },
        error: function (response) {
            Swal.fire({
                icon: 'error',
                title: 'Oops...',
                text: 'Xóa tài khoản thất bại: ' + response.status,
            });
        }
    });
}

function GetByID(id) {
    $.ajax({
        url: '/Admin/NguoiDung/GetByID',
        type: 'GET',
        data: { id: id },
        success: function (res) {
            if (res.status !== "Load dữ liệu thành công") {
                alert(res.status);
                return;
            }
            $('#Edit_ID').val(res.data.id_users);
            $('#Edit_TenNguoiDung').val(res.data.name);
            $('#Edit_Email').val(res.data.email);
            $('#Edit_ChucVu').val(res.data.id_typeusers);
            if (res.data.id_typeusers == 3) {
                $('#Edit_CTDT').val(res.data.id_ctdt);
                $('#Edit_CTDT').closest('.form-group').find('#ctdtContainer').show();
                $('#Edit_Donvi').closest('.form-group').find('#donviContainer').hide();

            } else if (res.data.id_typeusers == 4) {
                $('#Edit_Donvi').val(res.data.id_donvi);
                $('#Edit_Donvi').closest('.form-group').find('#donviContainer').show();
                $('#Edit_CTDT').closest('.form-group').find('#ctdtContainer').hide();
            }
            else {
                $('#Edit_CTDT').val(null);
                $('#Edit_CTDT').closest('.form-group').find('#ctdtContainer').hide();

                $('#Edit_Donvi').val(null);
                $('#Edit_Donvi').closest('.form-group').find('#donviContainer').hide();
            }

            $('#change_Ngay_Tao').val(res.data.ngaytao);
        }
    });
};

function unixTimestampToDate(unixTimestamp) {
    var date = new Date(unixTimestamp * 1000);
    var weekdays = ['Chủ Nhật', 'Thứ 2', 'Thứ 3', 'Thứ 4', 'Thứ 5', 'Thứ 6', 'Thứ 7'];
    var dayOfWeek = weekdays[date.getDay()];
    var month = ("0" + (date.getMonth() + 1)).slice(-2);
    var day = ("0" + date.getDate()).slice(-2);
    var year = date.getFullYear();
    var hours = ("0" + date.getHours()).slice(-2);
    var minutes = ("0" + date.getMinutes()).slice(-2);
    var seconds = ("0" + date.getSeconds()).slice(-2);
    var formattedDate = dayOfWeek + ', ' + day + "-" + month + "-" + year + " " + ', ' + hours + ":" + minutes + ":" + seconds;
    return formattedDate;
}