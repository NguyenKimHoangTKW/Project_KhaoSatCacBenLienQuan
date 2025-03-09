$(".select2").select2();
let value_check = "";
$(document).ready(function () {
    load_data();
});
$(document).on("click", "#btnFilter", function (event) {
    event.preventDefault()
    load_data();
});
$(document).on("click", "#btnAdd", function (event) {
    event.preventDefault();
    $(".modal-title").text("Thêm mới lớp học")
    const footer = $(".modal-footer");
    footer.empty();
    let html =
        `
        <button type="button" class="btn btn-danger btn-tone m-r-5" data-dismiss="modal">Thoát</button>
        <button type="button" class="btn btn-success btn-tone m-r-5" id="btnSaveAdd">Lưu</button>
        `;
    footer.html(html);
    $("#bd-example-modal-lg").modal("show");
});
$(document).on("click", "#btnSaveAdd", function (event) {
    event.preventDefault();
    add_new();
});
$(document).on("click", "#btnEdit", function (event) {
    event.preventDefault();
    const value = $(this).data("id");
    $(".modal-title").text("Chỉnh sửa lớp học")
    const footer = $(".modal-footer");
    footer.empty();
    let html =
        `
        <button type="button" class="btn btn-danger btn-tone m-r-5" data-dismiss="modal">Thoát</button>
        <button type="button" class="btn btn-success btn-tone m-r-5" id="btnSaveEdit">Lưu</button>
        `;
    footer.html(html);
    get_info(value);
    $("#bd-example-modal-lg").modal("show");
    value_check = value;
    
});
$(document).on("click", "#btnSaveEdit", function (event) {
    event.preventDefault();
    edit_change(value_check);
});
$(document).on("click", "#btnDelete", function (event) {
    event.preventDefault();
    const value = $(this).data("id");
    Swal.fire({
        title: "Bạn đang thao tác xóa lớp?",
        text: "Bạn có chắc muốn xóa lớp này không!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Có, xóa luôn!"
    }).then((result) => {
        if (result.isConfirmed) {
            del_change(value);
        }
    });
});
async function del_change(value) {
    const res = await $.ajax({
        url: '/api/admin/delete-lop',
        type: 'POST',
        data: {
            id_lop: value
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
        load_data();
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
async function edit_change(value) {
    const name = $("#TenLop").val();
    const ctdt = $("#MaCTDT").val();
    const res = await $.ajax({
        url: '/api/admin/update-lop',
        type: 'POST',
        data: {
            id_lop : value,
            ma_lop: name,
            id_ctdt: ctdt
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
        load_data();
    } else {
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
        url: '/api/admin/get-info-lop',
        type: 'POST',
        data: {
            id_lop: value
        }
    });
    $("#TenLop").val(res.ma_lop);
    $("#MaCTDT").val(res.id_ctdt).trigger("change");
}
async function add_new() {
    const name = $("#TenLop").val();
    const ctdt = $("#MaCTDT").val();
    const res = await $.ajax({
        url: '/api/admin/them-moi-lop',
        type: 'POST',
        data: {
            ma_lop: name,
            id_ctdt: ctdt
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
        load_data();
    } else {
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
async function load_data() {
    const ctdt = $("#FilterCTDT").val();
    const res = await $.ajax({
        url: '/api/admin/danh-sach-lop',
        type: 'POST',
        data: {
            id_ctdt: ctdt
        }
    });
    const body = $("#lopTable");
    let html = "";
    if ($.fn.DataTable.isDataTable('#lopTable')) {
        $('#lopTable').DataTable().clear().destroy();
    }
    if (res.success) {
        let thead =
            `
            <tr>
                <th scope="col">STT</th>
                <th scope="col">ID Lớp</th>
                <th scope="col">Tên lớp</th>
                <th scope="col">Thuộc CTĐT</th>
                <th scope="col">Ngày Tạo</th>
                <th scope="col">Cập nhật lần cuối</th>
                <th scope="col">Chức năng</th>
            </tr>
            `;
        body.find("thead").html(thead);
        res.data.forEach((item, index) => {
            html +=
                `
                <tr>
                    <td class="formatSo">${index + 1}</td>
                    <td class="formatSo">${item.id_lop}</td>
                    <td>${item.ma_lop}</td>
                    <td>${item.ten_ctdt}</td>
                    <td class="formatSo">${unixTimestampToDate(item.ngaytao)}</td>
                    <td class="formatSo">${unixTimestampToDate(item.ngaycapnhat)}</td>
                    <td>
                        <button class="btn btn-icon btn-hover btn-sm btn-rounded pull-right" id="btnEdit" data-id="${item.id_lop}">
                            <i class="anticon anticon-edit"></i>
                        </button>
                        <button class="btn btn-icon btn-hover btn-sm btn-rounded pull-right" id="btnDelete" data-id="${item.id_lop}">
                            <i class="anticon anticon-delete"></i>
                        </button>
                    </td>
                </tr>
                `;
        });
        body.find("tbody").html(html);
    } else {
        html =
            `
            <tr>
                <td colspan="7" class="text-center text-danger">${res.message || 'Không có dữ liệu'}</td>
            </tr>
            `;
        body.find("tbody").html(html);
    }
    $('#lopTable').DataTable({
        pageLength: 7,
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
                extend: 'excel',
                title: 'Danh sách Lớp'
            },
            {
                extend: 'print',
                title: 'Danh sách Lớp'
            }
        ]
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