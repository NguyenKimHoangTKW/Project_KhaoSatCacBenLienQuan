$('.select2').select2();
let value_check = "";
$(document).ready(function () {
    load_data();
    $("#btnFilter").click(function (event) {
        event.preventDefault();
        load_data();
    })
    $("#bd-example-modal-lg").on("hidden.bs.modal", function () {
        $(this).find("form").trigger("reset"); 
    });
    $(document).on('input', '#ma_ctdt', function () {
        const maxLength = 10;
        const value = $(this).val();
        if (value.length > maxLength) {
            $(this).val(value.substring(0, maxLength));
        }
    });
});
$(document).on("click", "#btnAdd", function (event) {
    event.preventDefault();
    var modalfooter = $(".modal-footer");
    $(".modal-title").text("Thêm mới chương trình đào tạo");
    let html =
        `
         <button type="button" class="btn btn-danger btn-tone m-r-5" data-dismiss="modal">Thoát</button>
        <button type="button" class="btn btn-success btn-tone m-r-5" id="btnSaveAdd">Lưu</button>
        `;
    modalfooter.html(html);
    $("#bd-example-modal-lg").modal("show");
});
$(document).on("click", "#btnSaveAdd", function (event) {
    event.preventDefault();
    add_ctdt();
});
$(document).on("click", "#btnEdit", function (event) {
    event.preventDefault();
    const value = $(this).data("id");
    var modalfooter = $(".modal-footer");
    $(".modal-title").text("Thêm mới chương trình đào tạo");
    let html =
        `
         <button type="button" class="btn btn-danger btn-tone m-r-5" data-dismiss="modal">Thoát</button>
        <button type="button" class="btn btn-success btn-tone m-r-5" id="btnSaveEdit">Lưu</button>
        `;
    modalfooter.html(html);
    get_info(value);
    $("#bd-example-modal-lg").modal("show");
    value_check = value;
});
$(document).on("click", "#btnSaveEdit", function (event) {
    event.preventDefault();
    update_ctdt(value_check);
});
$(document).on("click", "#btnDelete", function (event) {
    event.preventDefault();
    const value = $(this).data("id");
    Swal.fire({
        title: "Bạn đang thao tác xóa?",
        text: "Khi xóa sẽ mất hoàn toàn dữ liệu!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Có, xóa luôn!"
    }).then((result) => {
        if (result.isConfirmed) {
            delete_ctdt(value);
        }
    });
});
async function delete_ctdt(value) {
    const res = await $.ajax({
        url: '/api/admin/delete-ctdt',
        type: 'POST',
        data: {
            id_ctdt : value
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
        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: res.message,
        });
    }
}
async function update_ctdt(value) {
    const ma_ctdt = $("#ma_ctdt").val();
    const ten_ctdt = $("#ten_ctdt").val();
    const MaKhoa = $("#MaKhoa").val();
    const MaHDT = $("#MaHDT").val();
    const res = await $.ajax({
        url: '/api/admin/cap-nhat-ctdt',
        type: 'POST',
        data: {
            id_ctdt :value,
            ma_ctdt: ma_ctdt,
            ten_ctdt: ten_ctdt,
            id_hdt: MaHDT,
            id_khoa: MaKhoa
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
        url: '/api/admin/get-info-ctdt',
        type: 'POST',
        data: {
            id_ctdt: value
        }
    });
    $("#ma_ctdt").val(res.ma_ctdt);
    $("#ten_ctdt").val(res.ten_ctdt);
    $("#MaKhoa").val(res.id_khoa).trigger('change');
    $("#MaHDT").val(res.id_hdt).trigger('change');
}
async function add_ctdt() {
    const ma_ctdt = $("#ma_ctdt").val();
    const ten_ctdt = $("#ten_ctdt").val();
    const MaKhoa = $("#MaKhoa").val();
    const MaHDT = $("#MaHDT").val();
    const res = await $.ajax({
        url: '/api/admin/them-moi-ctdt',
        type: 'POST',
        data: {
            ma_ctdt: ma_ctdt,
            ten_ctdt: ten_ctdt,
            id_hdt: MaHDT,
            id_khoa: MaKhoa
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
    const hdt = $("#FilterHDT").val();
    const khoa = $("#FilterKhoa").val();
    const bomon = $("#FilterBoMon").val();
    const res = await $.ajax({
        url: '/api/admin/danh-sach-ctdt',
        type: 'POST',
        data: {
            id_khoa: khoa,
            id_hdt: hdt,
            id_bo_mon: bomon
        }
    });

    const body = $("#ctdtTable");
    let html = "";
    if ($.fn.DataTable.isDataTable('#ctdtTable')) {
        $('#ctdtTable').DataTable().clear().destroy();
    }
    if (res.success) {
        let thead =
            `
            <tr>
                <th scope="col">STT</th>
                <th scope="col">ID CTĐT</th>
                <th scope="col">Mã CTĐT</th>
                <th scope="col">Tên CTĐT</th>
                <th scope="col">Thuộc khoa/Viện</th>
                <th scope="col">Thuộc bộ môn</th>
                <th scope="col">Thuộc hệ đào tạo</th>
                <th scope="col">Ngày Tạo</th>
                <th scope="col">Cập nhật lần cuối</th>
                <th scope="col">Chức năng</th>
            </tr>
            `;
        body.find("thead").html(thead);
        const data = JSON.parse(res.data);
        data.forEach((item, index) => {
            html +=
                `
                <tr>
                    <td class="formatSo">${index + 1}</td>
                    <td class="formatSo">${item.id_ctdt}</td>
                    <td>${item.ma_ctdt}</td>
                    <td>${item.ten_ctdt}</td>
                    <td>${item.ten_khoa}</td>
                    <td>${item.ten_bo_mon}</td>
                    <td>${item.ten_hedaotao}</td>
                    <td class="formatSo">${unixTimestampToDate(item.ngaytao)}</td>
                    <td class="formatSo">${unixTimestampToDate(item.ngaycapnhat)}</td>
                    <td>
                        <button class="btn btn-icon btn-hover btn-sm btn-rounded pull-right" id="btnEdit" data-id="${item.id_ctdt}">
                            <i class="anticon anticon-edit"></i>
                        </button>
                        <button class="btn btn-icon btn-hover btn-sm btn-rounded pull-right" id="btnDelete" data-id="${item.id_ctdt}">
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
    $('#ctdtTable').DataTable({
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
    var formattedDate = dayOfWeek + ', ' + day + "-" + month + "-" + year + " " + ', ' + hours + ":" + minutes + ":" + seconds;
    return formattedDate;
}