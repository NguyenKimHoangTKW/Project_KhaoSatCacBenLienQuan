$(".select2").select2();
let value_check = "";
$(document).ready(function () {
    $("#bd-example-modal-lg").on("hidden.bs.modal", function () {
        $(this).find("input, textarea, select").val("");
        $(this).find("input[type=checkbox], input[type=radio]").prop("checked", false);
    });
    $('#importExcelModal').on('hidden.bs.modal', function () {
        $(this).find('form')[0].reset(); 
    });

});
$(document).on("change", "#nam-hoc", function () {
    load_data();
});
$("#nam-hoc").trigger("change");
$(document).on("change", "#hoc-phan", function () {
    load_data();
});

$(document).on("click", "#btnFilter", function (event) {
    event.preventDefault();
    load_data();
});

$(document).on("click", "#btnEdit", function (event) {
    event.preventDefault();
    const value = $(this).data("id");
    let html = "";
    $(".modal-title").text("CẬP NHẬT MÔN HỌC");
    html +=
        `
        <button type="button" class="btn btn-default m-r-10" data-dismiss="modal">Thoát</button>
        <button type="button" id="btnUpdateObject" class="btn btn-primary">Lưu thay đổi</button>
        `;
    $(".modal-footer").html(html);
    value_check = value;
    get_info(value);
    $("#bd-example-modal-lg").modal("show");
});
$(document).on("click", "#btnAdd", function (event) {
    event.preventDefault();
    let html = "";
    $(".modal-title").text("THÊM MỚI MÔN HỌC");
    html +=
        `
        <button type="button" class="btn btn-default m-r-10" data-dismiss="modal">Thoát</button>
        <button type="button" id="btnAddNew" class="btn btn-primary">Lưu thay đổi</button>
        `;
    $(".modal-footer").html(html);
    $("#bd-example-modal-lg").modal("show");
});
$(document).on("click", "#btnAddNew", function (event) {
    event.preventDefault();
    add_new();
});
$(document).on("click", "#btnUpdateObject", function (event) {
    event.preventDefault();
    update(value_check);
})
$(document).on("click", "#btnDelete", function (event) {
    event.preventDefault();
    const value = $(this).data("id");
    Swal.fire({
        title: "Bạn đang thao tác xóa?",
        text: "Bạn chắc chắc muốn xóa dữ liệu!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Có, xóa luôn!"
    }).then((result) => {
        if (result.isConfirmed) {
            _delete(value);
        }
    });
});
$(document).on("submit", "#importExcelForm", async function (event) {
    event.preventDefault();
    var formData = new FormData(this);
    const res = await $.ajax({
        url: '/api/admin/upload-excel-mon-hoc',
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
});
async function load_data() {
    const namhoc = $("#nam-hoc").val();
    const hocphan = $("#hoc-phan").val();
    const res = await $.ajax({
        url: '/api/admin/danh-sach-mon-hoc',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            hocphan: hocphan,
            namhoc: namhoc
        })
    });
    if ($.fn.DataTable.isDataTable('#mon-hoc-table')) {
        $('#mon-hoc-table').DataTable().clear().destroy();
    }
    const thead = $("#showthead");
    const tbody = $("#showdata");
    const title = $("#title_notification");
    thead.empty();
    tbody.empty();
    if (res.success) {
        const data = JSON.parse(res.data);
        title.hide();
        let htmlThead = `
        <tr>    
            <th>STT</th>
            <th>ID Môn học</th>
            <th>Mã môn học</th>
            <th>Tên môn học</th>
            <th>Thuộc học phần</th>
            <th>Thuộc lớp</th>
            <th>Ngày tạo</th>
            <th>Cập nhật lần cuối</th>
            <th>Năm hoạt động</th>
            <th>Chức năng</th>
        </tr>
    `;
        thead.html(htmlThead);
        let htmlTbody = "";
        data.forEach((item, index) => {
            htmlTbody += `
            <tr>
                <td class="formatSo">${index + 1}</td>
                <td class="formatSo">${item.value}</td>
                <td class="formatSo">${item.ma_mon_hoc}</td>
                <td>${item.ten_mon_hoc}</td>
                <td>${item.thuoc_hoc_phan}</td>
                <td class="formatSo">${item.thuoc_lop}</td>
                <td class="formatSo">${unixTimestampToDate(item.ngay_tao)}</td>
                <td class="formatSo">${unixTimestampToDate(item.ngay_cap_nhat)}</td>
                <td class="formatSo">${item.thuoc_nam}</td>
                <td>
                        <button class="btn btn-icon btn-hover btn-sm btn-rounded pull-right" id="btnEdit" data-id="${item.value}">
                            <i class="anticon anticon-edit"></i>
                        </button>
                        <button class="btn btn-icon btn-hover btn-sm btn-rounded pull-right" id="btnDelete" data-id="${item.value}">
                            <i class="anticon anticon-delete"></i>
                        </button>
                    </td>
            </tr>
        `;
        });
        tbody.html(htmlTbody);
        $('#mon-hoc-table').DataTable({
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
                    title: 'Danh sách đáp viên đã chọn khảo sát phiếu'
                },
                {
                    extend: 'print',
                    title: 'Danh sách đáp viên đã chọn khảo sát phiếu'
                }
            ]
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
}

async function add_new() {
    const mamonhoc = $("#ma-mon-hoc-val").val();
    const tenmonhoc = $("#ten-mon-hoc-val").val();
    const hocphan = $("#hoc-phan-val").val();
    const lop = $("#lop-val").val();
    const namhoc = $("#nam-hoc-val").val();
    const res = await $.ajax({
        url: '/api/admin/them-moi-mon-hoc',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            ma_mon_hoc: mamonhoc,
            ten_mon_hoc: tenmonhoc,
            id_lop: lop,
            id_hoc_phan: hocphan,
            id_nam_hoc: namhoc
        })
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
}

async function get_info(value) {
    const res = await $.ajax({
        url: '/api/admin/info-mon-hoc',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            id_mon_hoc: value
        })
    });
    if (res.success) {
        const items = JSON.parse(res.data);
        $("#ma-mon-hoc-val").val(items.ma_mon_hoc);
        $("#ten-mon-hoc-val").val(items.ten_mon_hoc);
        $("#hoc-phan-val").val(items.hoc_phan).trigger("change");
        $("#lop-val").val(items.lop).trigger("change");
        $("#nam-hoc-val").val(items.nam_hoc).trigger("change");
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

async function update(value) {
    const mamonhoc = $("#ma-mon-hoc-val").val();
    const tenmonhoc = $("#ten-mon-hoc-val").val();
    const hocphan = $("#hoc-phan-val").val();
    const lop = $("#lop-val").val();
    const namhoc = $("#nam-hoc-val").val();
    const res = await $.ajax({
        url: '/api/admin/update-mon-hoc',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            id_mon_hoc: value,
            ma_mon_hoc: mamonhoc,
            ten_mon_hoc: tenmonhoc,
            id_lop: lop,
            id_hoc_phan: hocphan,
            id_nam_hoc: namhoc
        })
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
async function _delete(value) {
    const res = await $.ajax({
        url: '/api/admin/delete-mon-hoc',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            id_mon_hoc: value
        })
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
        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: res.message,
        });
    }
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