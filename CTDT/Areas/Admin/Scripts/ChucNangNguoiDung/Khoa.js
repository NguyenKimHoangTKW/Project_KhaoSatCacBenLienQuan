
$(document).ready(function () {
    load_data();
});

$(document).on("click", "#btnAdd", function (event) {
    event.preventDefault();
    $(".modal-title").text("Thêm mới Khoa/Viện");
    const body = $("#body-content");
    body.empty();
    let html = ``;
    html +=
        `
        <form id="form-validation">
                <div class="form-group row">
                    <label class="col-sm-2 col-form-label control-label">Mã Khoa/Viện *</label>
                    <div class="col-sm-10">
                        <input type="text" class="form-control" id="ma_khoa" placeholder="Nhập mã Khoa/Viện ... *">
                    </div>
                </div>
                <div class="form-group row">
                    <label class="col-sm-2 col-form-label control-label">Tên Khoa/Viện *</label>
                    <div class="col-sm-10">
                        <input type="text" class="form-control" name="ten_khoa" id="ten_khoa" placeholder="Nhập tên Khoa/Viện ... *">
                    </div>
                </div>
            </form>
            <div class="modal-footer">
                <button type="button" class="btn btn-danger btn-tone m-r-5" data-dismiss="modal">Thoát</button>
                <button type="button" class="btn btn-success btn-tone m-r-5" id="btnSaveAdd">Lưu</button>
            </div>
        </div>
        `;
    body.html(html);
    $("#bd-example-modal-lg").show("modal");
});
$(document).on("click", "#btnSaveAdd", function (event) {
    event.preventDefault();
    add_khoa();
});
$(document).on("click", "#btnEdit", function (event) {
    event.preventDefault();
    const value = $(this).data("id");
    get_info(value);
    $(document).on("click", "#btnSaveEdit", function () {
        edit_khoa(value);
        load_data();
    });
});
$(document).on("click", "#btnDelete", function (event) {
    event.preventDefault();
    const value = $(this).data("id");
    Swal.fire({
        title: "Bạn đang thao tác xóa?",
        text: "Bạn muốn xóa Khoa?",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Có, xóa luôn!"
    }).then((result) => {
        if (result.isConfirmed) {
            delete_khoa(value);
        }
    });
});
async function delete_khoa(value) {
    const res = await $.ajax({
        url: '/api/admin/delete-khoa',
        type: 'POST',
        data: {
            id_khoa: value
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
async function edit_khoa(value) {
    const ma_khoa = $("#ma_khoa").val();
    const ten_khoa = $("#ten_khoa").val();
    const res = await $.ajax({
        url: '/api/admin/update-khoa',
        type: 'POST',
        data: {
            id_khoa: value,
            ma_khoa: ma_khoa,
            ten_khoa: ten_khoa
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
};
async function get_info(value) {
    $(".modal-title").text("Cập nhật Khoa/Viện");
    const body = $("#body-content");
    body.empty();
    let html = ``;
    const res = await $.ajax({
        url: '/api/admin/get-info-khoa',
        type: 'POST',
        data: { id_khoa: value }
    });
    html += `
            <form id="form-validation">
                <div class="form-group row">
                    <label class="col-sm-2 col-form-label control-label">Mã Khoa/Viện *</label>
                    <div class="col-sm-10">
                        <input type="text" class="form-control" id="ma_khoa" placeholder="Nhập mã Khoa/Viện ... *" value="${res.ma_khoa}">
                    </div>
                </div>
                <div class="form-group row">
                    <label class="col-sm-2 col-form-label control-label">Tên Khoa/Viện *</label>
                    <div class="col-sm-10">
                        <input type="text" class="form-control" name="ten_khoa" id="ten_khoa" placeholder="Nhập tên Khoa/Viện ... *" value="${res.ten_khoa}">
                    </div>
                </div>
            </form>
            <div class="modal-footer">
                <button type="button" class="btn btn-danger btn-tone m-r-5" data-dismiss="modal">Thoát</button>
                <button type="button" class="btn btn-success btn-tone m-r-5" id="btnSaveEdit">Lưu</button>
            </div>
        `;
    body.html(html);
    $("#bd-example-modal-lg").modal('show');
}

async function add_khoa() {
    const ma_khoa = $("#ma_khoa").val();
    const ten_khoa = $("#ten_khoa").val();
    const res = await $.ajax({
        url: '/api/admin/them-moi-khoa',
        type: 'POST',
        data: {
            ma_khoa: ma_khoa,
            ten_khoa: ten_khoa
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
async function load_data() {
    const table = $("#khoaTable");
    let theadHtml = `
        <tr>
            <th scope="col">Số Thứ Tự</th>
            <th scope="col">ID Khoa/Viện</th>
            <th scope="col">Mã Khoa/Viện</th>
            <th scope="col">Tên Khoa/Viện</th>
            <th scope="col">Ngày tạo</th>
            <th scope="col">Ngày cập nhật</th>
            <th scope="col">Chức năng</th>
        </tr>
    `;
    let tbodyHtml = ``;

    table.find("thead").html(theadHtml);
    try {
        const res = await $.ajax({
            url: '/api/admin/load-danh-sach-khoa',
            type: 'GET'
        });

        if ($.fn.DataTable.isDataTable('#khoaTable')) {
            $('#khoaTable').DataTable().clear().destroy();
        }
        if (res.success) {
            res.data.forEach((item, index) => {
                tbodyHtml += `
                    <tr>
                        <td class="formatSo">${index + 1}</td>
                        <td class="formatSo">${item.id_khoa}</td>
                        <td>${item.ma_khoa}</td>
                        <td>${item.ten_khoa}</td>
                        <td class="formatSo">${unixTimestampToDate(item.ngaytao)}</td>
                        <td class="formatSo">${unixTimestampToDate(item.ngaycapnhat)}</td>
                        <td>
                            <button class="btn btn-primary btn-sm" id="btnEdit" data-id="${item.id_khoa}">Sửa</button>
                            <button class="btn btn-danger btn-sm" id="btnDelete" data-id="${item.id_khoa}">Xóa</button>
                        </td>
                    </tr>
                `;
            });
        } else {
            tbodyHtml = `
                <tr>
                    <td colspan="7" class="text-center text-danger">${res.message || 'Không có dữ liệu'}</td>
                </tr>
            `;
        }
    } catch (error) {
        tbodyHtml = `
            <tr>
                <td colspan="7" class="text-center text-danger">Lỗi tải dữ liệu từ server</td>
            </tr>
        `;
    }

    table.find("tbody").html(tbodyHtml);
    $('#khoaTable').DataTable({
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
                title: 'Danh sách khoa/viện'
            },
            {
                extend: 'print',
                title: 'Danh sách khoa/viện'
            }
        ]
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
    var formattedDate = dayOfWeek + ', ' + day + "-" + month + "-" + year + " " + hours + ":" + minutes + ":" + seconds;
    return formattedDate;
}
