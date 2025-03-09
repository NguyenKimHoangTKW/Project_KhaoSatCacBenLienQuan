$(".select2").select2();
let value_check = "";
let currentPage = 1;
$(document).ready(function () {
    load_data();
    $("#Modal-update").on("hidden.bs.modal", function () {
        $(this).find("input, textarea, select").val("");
        $(this).find("input[type=checkbox], input[type=radio]").prop("checked", false);
    });
    $("#ModalXoa").on("hidden.bs.modal", function () {
        $(this).find("input, textarea, select").val("");
        $(this).find("input[type=checkbox], input[type=radio]").prop("checked", false);
    });
    $("#importExcelModal").on("hidden.bs.modal", function () {
        $(this).find("input, textarea, select").val("");
        $(this).find("input[type=checkbox], input[type=radio]").prop("checked", false);
    });
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
$(document).on("click", "#btnFilter", function (event) {
    event.preventDefault();
    load_data(currentPage);
});
$(document).on("click", "#btnAdd", function (event) {
    event.preventDefault();
    $("#modal-title-update").text("Thêm mới cán bộ viên chức/giảng viên");
    const footer = $("#modal-footer-update");
    footer.empty();
    let html =
        `
        <button type="button" class="btn btn-danger btn-tone m-r-5" data-dismiss="modal">Thoát</button>
        <button type="button" class="btn btn-success btn-tone m-r-5" id="btnSaveAdd">Lưu dữ liệu</button>
        `;
    footer.html(html);
    $("#Modal-update").modal("show");
});
$(document).on("click", "#btnSaveAdd", function (event) {
    event.preventDefault();
    add_new();
});
$(document).on("submit", "#importExcelForm", async function (event) {
    event.preventDefault();
    var formData = new FormData(this);
    const res = await $.ajax({
        url: '/api/admin/upload-excel-cbvc',
        type: 'POST',
        data: formData,
        contentType: false,
        processData: false
    });
    if (res.success) {
        const Toast = Swal.mixin({
            toast: true,
            position: "top-end",
            showConfirmButton: false,
            timer: 3000,
            timerProgressBar: true,
            didOpen: (toast) => {
                toast.onmouseenter = Swal.stopTimer;
                toast.onmouseleave = Swal.resumeTimer;
            }
        });
        Toast.fire({
            icon: "success",
            title: res.message
        });
        $('#importExcelModal').modal('hide');
        load_data(currentPage);
    }
    else {
        const Toast = Swal.mixin({
            toast: true,
            position: "top-end",
            showConfirmButton: false,
            timer: 3000,
            timerProgressBar: true,
            didOpen: (toast) => {
                toast.onmouseenter = Swal.stopTimer;
                toast.onmouseleave = Swal.resumeTimer;
            }
        });
        Toast.fire({
            icon: "error",
            title: res.message
        });
    }
});
$(document).on("click", "#btnEdit", function (event) {
    event.preventDefault();
    const value = $(this).data("id");
    $("#modal-title-update").text("Cập nhật cán bộ viên chức/giảng viên");
    const footer = $("#modal-footer-update");
    footer.empty();
    let html =
        `
        <button type="button" class="btn btn-danger btn-tone m-r-5" data-dismiss="modal">Thoát</button>
        <button type="button" class="btn btn-success btn-tone m-r-5" id="btnSaveEdit">Lưu dữ liệu</button>
        `;
    footer.html(html);
    value_check = value;
    get_info(value)
    $("#Modal-update").modal("show");
});
$(document).on("click", "#btnSaveEdit", function (event) {
    event.preventDefault();
    update_cbvc(value_check);
});
$(document).on("click", "#btnDelete", function (event) {
    event.preventDefault();
    const value = $(this).data("id");
    Swal.fire({
        title: "Bạn đang thao tác xóa!",
        text: "Bạn có chắc muốn xóa dữ liệu này!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Có, xóa luôn!"
    }).then((result) => {
        if (result.isConfirmed) {
            delete_cbvc(value);
        }
    });
});
async function delete_cbvc(value) {
    const res = await $.ajax({
        url: '/api/admin/delete-cbvc',
        type: 'POST',
        data: {
            id_cbvc: value
        }
    });
    if (res.success) {
        const Toast = Swal.mixin({
            toast: true,
            position: "top-end",
            showConfirmButton: false,
            timer: 3000,
            timerProgressBar: true,
            didOpen: (toast) => {
                toast.onmouseenter = Swal.stopTimer;
                toast.onmouseleave = Swal.resumeTimer;
            }
        });
        Toast.fire({
            icon: "success",
            title: res.message
        });
        load_data(currentPage);
    }
    else {
        const Toast = Swal.mixin({
            toast: true,
            position: "top-end",
            showConfirmButton: false,
            timer: 3000,
            timerProgressBar: true,
            didOpen: (toast) => {
                toast.onmouseenter = Swal.stopTimer;
                toast.onmouseleave = Swal.resumeTimer;
            }
        });
        Toast.fire({
            icon: "error",
            title: res.message
        });
    }
}
async function update_cbvc(value) {
    const macbvc = $("#macbvc-val").val();
    const tencbvc = $("#tencbvc-val").val();
    const email = $("#email-val").val();
    const ngaysinh = $("#ngaysinh-val").val();
    const donvi = $("#donvi-val").val();
    const chucvu = $("#chucvu-val").val();
    const ctdt = $("#ctdt-val").val();
    const nam = $("#nam-val").val();
    const ghichu = $("#ghichu-val").val();
    const res = await $.ajax({
        url: '/api/admin/update-cbvc',
        type: 'POST',
        data: {
            id_CBVC: value,
            MaCBVC: macbvc,
            TenCBVC: tencbvc,
            Email: email,
            NgaySinh: ngaysinh,
            id_donvi: donvi,
            id_chucvu: chucvu,
            id_chuongtrinhdaotao: ctdt,
            id_namhoc: nam,
            description: ghichu
        }
    });
    if (res.success) {
        const Toast = Swal.mixin({
            toast: true,
            position: "top-end",
            showConfirmButton: false,
            timer: 3000,
            timerProgressBar: true,
            didOpen: (toast) => {
                toast.onmouseenter = Swal.stopTimer;
                toast.onmouseleave = Swal.resumeTimer;
            }
        });
        Toast.fire({
            icon: "success",
            title: res.message
        });
        load_data(currentPage);
    }
    else {
        const Toast = Swal.mixin({
            toast: true,
            position: "top-end",
            showConfirmButton: false,
            timer: 3000,
            timerProgressBar: true,
            didOpen: (toast) => {
                toast.onmouseenter = Swal.stopTimer;
                toast.onmouseleave = Swal.resumeTimer;
            }
        });
        Toast.fire({
            icon: "error",
            title: res.message
        });
    }
}
async function get_info(value) {
    const res = await $.ajax({
        url: '/api/admin/get-info-cbvc',
        type: 'POST',
        data: {
            id_CBVC: value
        }
    });
    const ngaysinh = res.NgaySinh ? res.NgaySinh.split("T")[0] : "";
    $("#macbvc-val").val(res.MaCBVC);
    $("#tencbvc-val").val(res.TenCBVC);
    $("#email-val").val(res.Email);
    $("#ngaysinh-val").val(ngaysinh);
    $("#donvi-val").val(res.id_donvi).trigger("change");
    $("#chucvu-val").val(res.id_chucvu).trigger("change");
    $("#ctdt-val").val(res.id_chuongtrinhdaotao).trigger("change");
    $("#nam-val").val(res.id_namhoc).trigger("change");
    $("#ghichu-val").val(res.description);
};
async function add_new() {
    const macbvc = $("#macbvc-val").val();
    const tencbvc = $("#tencbvc-val").val();
    const email = $("#email-val").val();
    const ngaysinh = $("#ngaysinh-val").val();
    const donvi = $("#donvi-val").val();
    const chucvu = $("#chucvu-val").val();
    const ctdt = $("#ctdt-val").val();
    const nam = $("#nam-val").val();
    const ghichu = $("#ghichu-val").val();
    const res = await $.ajax({
        url: '/api/admin/them-moi-cbvc',
        type: 'POST',
        data: {
            MaCBVC: macbvc,
            TenCBVC: tencbvc,
            Email: email,
            NgaySinh: ngaysinh,
            id_donvi: donvi,
            id_chucvu: chucvu,
            id_chuongtrinhdaotao: ctdt,
            id_namhoc: nam,
            description: ghichu
        }
    });
    if (res.success) {
        const Toast = Swal.mixin({
            toast: true,
            position: "top-end",
            showConfirmButton: false,
            timer: 3000,
            timerProgressBar: true,
            didOpen: (toast) => {
                toast.onmouseenter = Swal.stopTimer;
                toast.onmouseleave = Swal.resumeTimer;
            }
        });
        Toast.fire({
            icon: "success",
            title: res.message
        });
        load_data(currentPage);
    }
    else {
        const Toast = Swal.mixin({
            toast: true,
            position: "top-end",
            showConfirmButton: false,
            timer: 3000,
            timerProgressBar: true,
            didOpen: (toast) => {
                toast.onmouseenter = Swal.stopTimer;
                toast.onmouseleave = Swal.resumeTimer;
            }
        });
        Toast.fire({
            icon: "error",
            title: res.message
        });
    }
}
async function load_data(page = 1, pageSize = $("#pageSizeSelect").val()) {
    const ctdt = $("#FilterCTDT").val();
    const donvi = $("#FiterDonvi").val();
    const chucvu = $("#FilterChucvu").val();
    const nam = $("#FilterNam").val();
    const searchTerm = $("#searchInput").val();
    const res = await $.ajax({
        url: '/api/admin/danh-sach-cbvc',
        type: 'POST',
        data: {
            id_chuongtrinhdaotao: ctdt,
            id_namhoc: nam,
            id_donvi: donvi,
            id_chucvu: chucvu,
            page: page,
            pageSize: pageSize,
            searchTerm: searchTerm
        }
    });
    const body = $("#cbvcTable");
    let html = "";
    if (res.success) {
        let thead =
            `
            <tr>
                <th scope="col">STT</th>
                <th scope="col">ID CBVC</th>
                <th scope="col">Mã CBVC</th>
                <th scope="col">Tên CBVC</th>
                <th scope="col">Ngày sinh</th>
                <th scope="col">Email</th>
                <th scope="col">Thuộc đơn vị</th>
                <th scope="col">Chức vụ</th>
                <th scope="col">Thuộc chương trình đào tạo</th>
                <th scope="col">Năm hoạt động</th>
                <th scope="col">Ngày tạo</th>
                <th scope="col">Cập nhật lần cuối</th>
                <th scope="col">Mô tả</th>
                <th scope="col">Chức năng</th>
            </tr>
            `;
        body.find("thead").html(thead);
        res.data.forEach((item, index) => {
            html +=
                `
                <tr>
                    <td class="formatSo">${(page - 1) * pageSize + index + 1}</td>  
                    <td class="formatSo">${item.id_CBVC}</td>
                    <td class="formatSo">${item.MaCBVC}</td>
                    <td>${item.TenCBVC}</td>
                    <td class="formatSo">${item.NgaySinh}</td>
                    <td>${item.Email}</td>
                    <td>${item.donvi}</td>
                    <td>${item.chucvu}</td>
                    <td>${item.ctdt}</td>
                    <td class="formatSo">${item.NamHoc}</td>
                    <td class="formatSo">${unixTimestampToDate(item.ngaytao)}</td>
                    <td class="formatSo">${unixTimestampToDate(item.ngaycapnhat)}</td>
                    <td>${item.descripton}</td>
                    <td>
                        <button class="btn btn-icon btn-hover btn-sm btn-rounded pull-right" id="btnEdit" data-id="${item.id_CBVC}">
                            <i class="anticon anticon-edit"></i>
                        </button>
                        <button class="btn btn-icon btn-hover btn-sm btn-rounded pull-right" id="btnDelete" data-id="${item.id_CBVC}">
                            <i class="anticon anticon-delete"></i>
                        </button>
                    </td>
                </tr>
                `;
        });
        body.find("tbody").html(html);
        renderPagination(res.totalPages, res.currentPage);
    } else {
        html =
            `
            <tr>
                <td colspan="14" class="text-center text-danger">${res.message || 'Không có dữ liệu'}</td>
            </tr>
            `;
        body.find("tbody").html(html);
    }
};
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
    var formattedDate = dayOfWeek + ', ' + day + "-" + month + "-" + year + " " + hours + ":" + minutes + ":" + seconds;
    return formattedDate;
}