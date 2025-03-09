$(".select2").select2();
$(document).ready(function () {
    option();
    load_data();
});

$(document).on("change", "#nam-hoc, #don-vi", function () {
    option();
    $("#bd-example-modal-lg").on("hidden.bs.modal", function () {
        $(this).find("input, textarea, select").val("");
        $(this).find("input[type=checkbox], input[type=radio]").prop("checked", false);
    });
    $('#importExcelModal').on('hidden.bs.modal', function () {
        $(this).find('form')[0].reset();
    });
});
$(document).on("click", "#btnFilter", function (event) {
    event.preventDefault();
    load_data();
});
$(document).on("submit", "#importExcelForm", async function (event) {
    event.preventDefault();
    var formData = new FormData(this);
    const res = await $.ajax({
        url: '/api/admin/upload-excel-bo-mon',
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
        load_data();
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
async function option() {
    const year = $("#nam-hoc").val();
    const don_vi = $("#don-vi").val();

    const res = await $.ajax({
        url: "/api/admin/option-bo-mon",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            nam_hoc: year,
            don_vi: don_vi
        })
    });

    let html_don_vi = `<option value="">Tất cả</option>`;
    if (res.don_vi && res.don_vi.length > 0) {
        res.don_vi.forEach(donvi => {
            html_don_vi += `<option value="${donvi.value}">${donvi.text}</option>`;
        });
    }
    $("#don-vi").html(html_don_vi);
    let html_khoa = `<option value="">Tất cả</option>`;
    if (res.khoa && res.khoa.length > 0) {
        res.khoa.forEach(khoa => {
            html_khoa += `<option value="${khoa.value}">${khoa.text}</option>`;
        });
    }
    $("#khoa").html(html_khoa);
}
async function load_data() {
    const table = $("#bo-mon-table");
    const year = $("#nam-hoc").val();
    const don_vi = $("#don-vi").val();
    const khoa = $("#khoa").val();
    const res = await $.ajax({
        url: '/api/admin/danh-sach-bo-mon',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            nam_hoc: year,
            don_vi: don_vi,
            khoa: khoa
        })
    });
    if ($.fn.DataTable.isDataTable('#bo-mon-table')) {
        $('#bo-mon-table').DataTable().clear().destroy();
    }
    if (res.success) {
        let theadHtml = `
        <tr>
            <th scope="col">Số Thứ Tự</th>
            <th scope="col">ID Bộ môn</th>
            <th scope="col">Tên Bộ môn</th>
            <th scope="col">Thuộc Khoa</th>
            <th scope="col">Thuộc Đơn vị</th>
            <th scope="col">Năm hoạt động</th>
            <th scope="col">Ngày tạo</th>
            <th scope="col">Ngày cập nhật</th>
            <th scope="col">Chức năng</th>
        </tr>
    `;
        let tbodyHtml = ``;

        table.find("thead").html(theadHtml);
        const data = JSON.parse(res.data);
        data.forEach((item, index) => {
            tbodyHtml += `
                    <tr>
                        <td class="formatSo">${index + 1}</td>
                        <td class="formatSo">${item.value}</td>
                        <td>${item.ten_bo_mon}</td>
                        <td>${item.thuoc_khoa}</td>
                        <td>${item.thuoc_don_vi}</td>
                        <td class="formatSo">${item.nam_hoc}</td>
                        <td class="formatSo">${unixTimestampToDate(item.ngay_tao)}</td>
                        <td class="formatSo">${unixTimestampToDate(item.ngay_cap_nhat)}</td>
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
        table.find("tbody").html(tbodyHtml);
        $('#bo-mon-table').DataTable({
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
