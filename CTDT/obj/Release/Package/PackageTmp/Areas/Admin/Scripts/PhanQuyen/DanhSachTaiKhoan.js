$(".select2").select2();
let currentPage = 1;
$(document).ready(function () {
    load_data();
});
$(document).on("click", ".page-link", function (e) {
    e.preventDefault();
    const page = $(this).data("page");
    if (page) {
        currentPage = page;
        load_data(currentPage);
    }
});
$(document).on("change", "#pageSizeSelect", function () {
    const pageSize = $(this).val();
    load_data(currentPage, pageSize);
});
$(document).on("change", "#pageSizeSelect", function () {
    const pageSize = $(this).val();
    load_data(currentPage, pageSize);
});
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

async function load_data(page = 1, pageSize = $("#pageSizeSelect").val()) {
    var trangthai_select = $('#FilterTrangThai').val();
    const searchTerm = $("#searchInput").val();
    const res = await $.ajax({
        url: '/api/admin/load_du_lieu_users',
        type: 'POST',
        data: {
            id_type_user: trangthai_select,
            page: page,
            pageSize: pageSize,
            searchTerm: searchTerm
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
        html += "<th>Thuộc quyền CTĐT</th>";
        html += "<th>Thuộc quyền Khoa</th>";
        html += "<th>Chức năng</th>";
        html += "</tr>";
        html += "</thead>";
        html += "<tbody>";
        res.data.forEach(function (items, index) {
            html += "<tr>";
            html += `<td class="formatSo">${(page - 1) * pageSize + index + 1}</td>`;
            html += `<td class="formatSo">${items.id_user}</td>`;
            html += `<td>${items.ten_user}</td>`;
            html += `<td>${items.email}</td>`;
            html += `<td class="formatSo">${items.quyen_han}</td>`;
            html += `<td class="formatSo">${unixTimestampToDate(items.ngay_tao)}</td>`;
            html += `<td class="formatSo">${unixTimestampToDate(items.ngay_cap_nhat)}</td>`;
            html += `<td class="formatSo">${items.dang_nhap_lan_cuoi != null ? unixTimestampToDate(items.dang_nhap_lan_cuoi) : ""}</td>`;
            const ctdtList = items.thuoc_chuc_quyen.map(quyen => quyen.ten_ctdt).join(" - ");
            html += `<td style="font-weight: bold;">${ctdtList}</td>`;
            const khoalist = items.thuoc_chuc_quyen.map(quyen => quyen.ten_khoa);
            html += `<td>${khoalist}</td>`;
            html += `<td  class="formatSo">`;
            html += `<button class="btn btn-hover btn-sm btn-rounded pull-right" id="btnEdit" data-id="${items.id_user}">Cấp quyền</button>`;
            html += `<button class="btn btn-hover btn-sm btn-rounded pull-right" id="btnDel" data-id="${items.id_user}">Xóa</button>`;
            html += `</td>`;
            html += "</tr>";
        })
        html += "</tbody>";
        body.html(html);
        renderPagination(res.totalPages, res.currentPage);
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
function renderPagination(totalPages, currentPage) {
    const paginationContainer = $("#paginationControls");
    let html = "";
    html += `
        <li class="page-item ${currentPage === 1 ? "disabled" : ""}">
            <a class="page-link" href="#" data-page="${currentPage - 1}">Previous</a>
        </li>
    `;

    html += `
        <li class="page-item ${currentPage === 1 ? "active" : ""}">
            <a class="page-link" href="#" data-page="1">1</a>
        </li>
    `;

    if (currentPage > 4) {
        html += `
            <li class="page-item disabled">
                <a class="page-link">...</a>
            </li>
        `;
    }
    const maxPagesToShow = 3;
    const startPage = Math.max(2, currentPage - Math.floor(maxPagesToShow / 2));
    const endPage = Math.min(totalPages - 1, currentPage + Math.floor(maxPagesToShow / 2));
    for (let i = startPage; i <= endPage; i++) {
        html += `
            <li class="page-item ${i === currentPage ? "active" : ""}">
                <a class="page-link" href="#" data-page="${i}">${i}</a>
            </li>
        `;
    }
    if (currentPage < totalPages - 3) {
        html += `
            <li class="page-item disabled">
                <a class="page-link">...</a>
            </li>
        `;
    }
    if (totalPages > 1) {
        html += `
            <li class="page-item ${currentPage === totalPages ? "active" : ""}">
                <a class="page-link" href="#" data-page="${totalPages}">${totalPages}</a>
            </li>
        `;
    }
    html += `
        <li class="page-item ${currentPage === totalPages ? "disabled" : ""}">
            <a class="page-link" href="#" data-page="${currentPage + 1}">Next</a>
        </li>
    `;
    paginationContainer.html(html);
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