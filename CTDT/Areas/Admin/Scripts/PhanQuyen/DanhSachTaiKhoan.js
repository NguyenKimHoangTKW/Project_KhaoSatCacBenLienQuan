load_data();
function Toast_alert(type, message) {
    const Toast = Swal.mixin({
        toast: true,
        position: "top-end",
        showConfirmButton: false,
        timer: 2000,
        timerProgressBar: true,
        didOpen: (toast) => {
            toast.onmouseenter = Swal.stopTimer;
            toast.onmouseleave = Swal.resumeTimer;
        }
    });
    Toast.fire({
        icon: type,
        title: message
    });
}
$(document).on('click', '#btnSave', function () {
    AddUser();
});
$(document).on('click', '#btnFilter', function () {
    load_data();
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
$(document).on('submit', '#importExcelForm', function () {
    e.preventDefault();
    var formData = new FormData(this);
    $.ajax({
        url: '/api/admin/import_excel_users',
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

async function load_data() {
    var ctdt_select = $('#FiterCTDT').val();
    var donvi_select = $('#FilterDonVi').val();
    var trangthai_select = $('#FilterTrangThai').val();

    const res = await $.ajax({
        url: '/api/admin/load_du_lieu_users',
        type: 'POST',
        data: {
            id_ctdt: ctdt_select,
            id_donvi: donvi_select,
            id_type_user: trangthai_select
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
        html += "<th>Đăng nhập lần cuối</th>";
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
            html += `<td>${unixTimestampToDate(items.ngay_tao)}</td>`;
            html += `<td>${unixTimestampToDate(items.ngay_cap_nhat)}</td>`;
            html += `<td>${unixTimestampToDate(items.dang_nhap_lan_cuoi)}</td>`;

            html += `<td>${items.don_vi}</td>`;
            html += `<td>${items.ctdt}</td>`;
            html += `<td>`;
            html += `<button class="btn btn-hover btn-sm btn-rounded pull-right" id="btnEdit" data-id="${items.id_user}">Cấp quyền</button>`;
            html += `<button class="btn btn-hover btn-sm btn-rounded pull-right" id="btnDel" data-id="${items.id_user}">Xóa</button>`;
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
};

$(document).on('click', '#btnEdit', function () {
    var id_users = $(this).data("id");
    window.location.href = "/Admin/InterfaceAdmin/PhanQuyen?id=" + id_users;
})

function AddUser() {
    var Email = $("#Email").val();
    $.ajax({
        url: '/api/admin/add_users',
        type: 'POST',
        data: { email: Email },
        success: function (res) {
            if (res.success) {
                Toast_alert("success", res.status);
                load_data()
            }
            else {
                Toast_alert("error", res.status);
            }
        }
    });
}
function DelNguoiDung(id) {
    $.ajax({
        type: 'POST',
        url: '/api/admin/del_users',
        data: { id_users: id },
        success: function (response) {
            Swal.fire({
                icon: 'success',
                title: response.status,
                showConfirmButton: false,
                timer: 2000
            });
            load_data()
        }
    });
}
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