const value = $("#value").text();
$(document).ready(function () {
    const title = $("#title");
    $("#btnXemBaoCao").click(function () {
        var update = $("#load-title");
        update.hide();
        title.text("Danh sách người đã khảo sát phiếu")
        load_chi_tiet_cau_tra_loi();
    })
    $("#btnXoaPhieu").click(function (event) {
        event.preventDefault();
        Swal.fire({
            title: "Bạn đang thao tác xóa phiếu khảo sát?",
            text: "Nếu bạn xóa phiếu khảo sát, toàn bộ dữ liệu liên quan đến phiếu khảo sát này sẽ bị xóa khỏi hệ thống, bạn muốn tiếp tục xóa?",
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#3085d6",
            cancelButtonColor: "#d33",
            confirmButtonText: "Có, tôi đồng ý xóa"
        }).then((result) => {
            if (result.isConfirmed) {
                delete_phieu_khao_sat();
            }
        });
    })
    $("#btnChinhSuaPhieu").click(function (event) {
        event.preventDefault();
        title.text("Chỉnh sửa phiếu khảo sát")
        const body = $("#bodycontent");
        if ($.fn.DataTable.isDataTable('#bodycontent')) {
            $('#bodycontent').DataTable().clear().destroy();
        }
        body.hide();
        var update = $("#load-title");
        update.show();

        load_chi_tiet_update();
    });
    $("#btnSave").click(function (event) {
        event.preventDefault();
        update_survey();
    });
})

async function load_chi_tiet_update() {
    const value = $("#value").text();
    const res = await $.ajax({
        url: "/api/admin/get-info-survey",
        type: "POST",
        data: {
            surveyID: value
        }
    });
    if (res.success) {
        const item = res.data;
        $("#TieuDe").val(item.surveyTitle);
        $("#MoTa").text(item.surveyDescription);
        $(`#DanhChoHe_${item.id_hedaotao}`).prop("checked", true);
        $("#MaDoiTuong").val(item.id_loaikhaosat);
        $("#MaNamHoc").val(item.id_namhoc);
        $("#TrangThai").val(item.surveyStatus);
        $("#DotKhaoSat").val(item.id_dot_khao_sat);
        $("#EnableThongKe").val(item.mo_thong_ke);
        const startDate = new Date(item.surveyTimeStart * 1000).toISOString().slice(0, 16);
        $("#NgayBatDau").val(startDate);
        const endDate = new Date(item.surveyTimeEnd * 1000).toISOString().slice(0, 16);
        $("#NgayKetThuc").val(endDate);
    }
}

async function update_survey() {
    const tieuDe = $('#TieuDe').val();
    const moTa = $('#MoTa').val();
    const danhChoHe = $('input[name="DanhChoHe"]:checked').val();
    const maDoiTuong = $('#MaDoiTuong').val();
    const ngayBatDauInput = $('#NgayBatDau').val();
    const ngayKetThucInput = $('#NgayKetThuc').val();
    const trangThai = $('#TrangThai').val();
    const dotkhaosat = $("#DotKhaoSat").val();
    const mothongke = $("#EnableThongKe").val();
    const maNamHoc = $("#MaNamHoc").val();
    const ngayBatDau = new Date(ngayBatDauInput + 'Z'); 
    const ngayKetThuc = new Date(ngayKetThucInput + 'Z')
    const unixNgayBatDau = Math.floor(ngayBatDau.getTime() / 1000);
    const unixNgayKetThuc = Math.floor(ngayKetThuc.getTime() / 1000);
    const res = await $.ajax({
        url: '/api/admin/update-phieu-khao-sat',
        type: 'POST',
        data: {
            surveyID: value,
            surveyTitle: tieuDe,
            surveyDescription: moTa,
            id_hedaotao: danhChoHe,
            id_loaikhaosat: maDoiTuong,
            id_namhoc: maNamHoc,
            surveyTimeStart: unixNgayBatDau,
            surveyTimeEnd: unixNgayKetThuc,
            surveyStatus: trangThai,
            id_dot_khao_sat: dotkhaosat,
            mo_thong_ke: mothongke,
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
}

async function delete_phieu_khao_sat() {
    const res = await $.ajax({
        url: '/api/admin/xoa-du-lieu-phieu-khao-sat',
        type: 'POST',
        data: {
            surveyID: value
        }
    });
    if (res.success) {
        Swal.fire({
            title: res.message,
            icon: "success",
        }).then((result) => {
            if (result.isConfirmed) {
                window.location.href = "/admin/danh-sach-phieu-khao-sat";
            }
        });
    } else {
        Swal.fire({
            title: "Xóa không thành công",
            text: res.message || "Đã xảy ra lỗi.",
            icon: "error",
            draggable: true
        });
    }
}

async function load_chi_tiet_cau_tra_loi() {
    const res = await $.ajax({
        url: '/api/admin/danh-sach-cau-tra-loi-phieu',
        type: 'POST',
        data: {
            surveyID: value 
        }
    });
    const body = $("#bodycontent");
    body.empty();
    let html = ``;
    if ($.fn.DataTable.isDataTable('#bodycontent')) {
        $('#bodycontent').DataTable().clear().destroy();
    }
    if (res.success) {
        res.data.forEach(function (items) {
            if (res.is_subject) {
                html += form_load_chi_tiet_subject(items);
            }
            else if (res.is_student) {
                html += form_load_chi_tiet_student(items);
            }
            else if (res.is_program) {
                html += form_load_chi_tiet_program(items);
            }
            else if (res.is_cbvc) {
                html += form_load_chi_tiet_giang_vien(items);
            }
        })
        body.show();
        body.html(html);
        initializeDataTable();
    } else {
        html = `<tr><td colspan="10" class="text-center">Không có dữ liệu để hiển thị</td></tr>`;
        body.html(html);
    }
}
function form_load_chi_tiet_subject(data) {
    let html = `
        <thead>
            <tr>
                <th scope="col">STT</th>
                <th scope="col">Mã KQ</th>
                <th scope="col">Email khảo sát</th>
                <th scope="col">Môn học</th>
                <th scope="col">Giảng viên giảng dạy</th>
                <th scope="col">Người học</th>
                <th scope="col">MSNH</th>
                <th scope="col">Thuộc CTĐT</th>
                <th scope="col">Thời gian thực hiện khảo sát</th>
                <th scope="col">Chi tiết câu trả lời</th>
            </tr>
        </thead>
        <tbody>
    `;
    data.forEach(function (item, index) {
        html += `
            <tr>
                <td class="formatSo">${index + 1}</td>
                <td class="formatSo">${item.ma_kq}</td>
                <td>${item.email}</td>
                <td>${item.mon_hoc}</td>
                <td>${item.giang_vien}</td>
                <td>${item.sinh_vien}</td>
                <td class="formatSo">${item.msnh}</td>
                <td>${item.ctdt}</td>
                <td class="formatSo">${unixTimestampToDate(item.thoi_gian_thuc_hien)}</td>
                <td>
                    <button class="btn btn-info btnChiTiet" data-id="${item.ma_kq}">Chi tiết</button>
                </td>
            </tr>
        `;
    });

    html += `
        </tbody>
    `;
    return html;
}

function form_load_chi_tiet_student(data) {
    let html = `
        <thead>
            <tr>
                <th scope="col">STT</th>
                <th scope="col">Mã KQ</th>
                <th scope="col">Email khảo sát</th>
                <th scope="col">Người học</th>
                <th scope="col">MSNH</th>
                <th scope="col">Thuộc CTĐT</th>
                <th scope="col">Thời gian thực hiện khảo sát</th>
                <th scope="col">Chi tiết câu trả lời</th>
            </tr>
        </thead>
        <tbody>
    `;
    data.forEach(function (item, index) {
        html += `
            <tr>
                <td class="formatSo">${index + 1}</td>
                <td class="formatSo">${item.ma_kq}</td>
                <td>${item.email}</td>
                <td>${item.sinh_vien}</td>
                <td class="formatSo">${item.msnh}</td>
                <td>${item.ctdt}</td>
                <td class="formatSo">${unixTimestampToDate(item.thoi_gian_thuc_hien)}</td>
                <td>
                    <button class="btn btn-info btnChiTiet" data-id="${item.ma_kq}">Chi tiết</button>
                </td>
            </tr>
        `;
    });

    html += `
        </tbody>
    `;
    return html;
}

function form_load_chi_tiet_program(data) {
    let html = `
        <thead>
            <tr>
                <th scope="col">STT</th>
                <th scope="col">Mã KQ</th>
                <th scope="col">Email khảo sát</th>
                <th scope="col">Thuộc Khoa</th>
                <th scope="col">Thuộc CTĐT</th>
                <th scope="col">Thời gian thực hiện khảo sát</th>
                <th scope="col">Chi tiết câu trả lời</th>
            </tr>
        </thead>
        <tbody>
    `;
    data.forEach(function (item, index) {
        html += `
            <tr>
                <td class="formatSo">${index + 1}</td>
                <td class="formatSo">${item.ma_kq}</td>
                <td>${item.email}</td>
                <td>${item.ten_khoa}</td>
                <td>${item.ten_ctdt}</td>
                <td class="formatSo">${unixTimestampToDate(item.thoi_gian_thuc_hien)}</td>
                <td>
                    <button class="btn btn-info btnChiTiet" data-id="${item.ma_kq}">Chi tiết</button>
                </td>
            </tr>
        `;
    });

    html += `
        </tbody>
    `;
    return html;
}

function form_load_chi_tiet_giang_vien(data) {
    let html = `
        <thead>
            <tr>
                <th scope="col">STT</th>
                <th scope="col">Mã KQ</th>
                <th scope="col">Email khảo sát</th>
                <th scope="col">Tên người khảo sát</th>
                <th scope="col">Thuộc đơn vị</th>
                <th scope="col">Thuộc CTĐT</th>
                <th scope="col">Thời gian thực hiện khảo sát</th>
                <th scope="col">Chi tiết câu trả lời</th>
            </tr>
        </thead>
        <tbody>
    `;
    data.forEach(function (item, index) {
        html += `
            <tr>
                <td class="formatSo">${index + 1}</td>
                <td class="formatSo">${item.ma_kq}</td>
                <td>${item.email}</td>
                <td>${item.cbvc}</td>
                <td>${item.don_vi != null ? item.don_vi : ""}</td>
                <td>${item.ctdt != null ? item.ctdt : ""}</td>
                <td class="formatSo">${unixTimestampToDate(item.thoi_gian_thuc_hien)}</td>
                <td>
                    <button class="btn btn-info btnChiTiet" data-id="${item.ma_kq}">Chi tiết</button>
                </td>
            </tr>
        `;
    });

    html += `
        </tbody>
    `;
    return html;
}

$(document).on('click', '.btnChiTiet', function () {
    const value = $(this).data('id');
    load_chi_tiet_cau_hoi(value)
    $('.bd-example-modal-xl').modal('show');
});

async function load_chi_tiet_cau_hoi(id) {
    const res = await $.ajax({
        url: '/api/admin/chi-tiet-cau-tra-loi',
        type: 'POST',
        data: {
            id: id
        }
    });

    const body = $("#datatable");
    body.empty();
    if ($.fn.DataTable.isDataTable('#datatable')) {
        $('#datatable').DataTable().clear().destroy();
    }
    let html = "";

    if (res.success) {
        const data = JSON.parse(res.data);
        html += `
                <thead>
                    <tr>
                        <th scope="col">STT</th>
                        <th scope="col">Câu hỏi</th>
                        <th scope="col">Câu trả lời</th>
                    </tr>
                </thead>
                <tbody>
            `;

        let questionIndex = 1;
        data.pages.forEach((page) => {
            page.elements.forEach((element) => {
                html += `
                        <tr>
                            <td>${questionIndex}</td>
                            <td>${element.title}</td>
                            <td>${element.response ? element.response.text : "Không có câu trả lời"}</td>
                        </tr>
                    `;
                questionIndex++;
            });
        });

        html += `</tbody>`;
        body.html(html);
        $("#datatable").DataTable({
            pageLength: 100,
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
                    title: 'Danh sách chi tiết thống kê - Excel'
                },
                {
                    extend: 'print',
                    title: 'Danh sách chi tiết thống kê'
                }
            ]
        });
    } else {
        body.html('<tr><td colspan="3">Không có dữ liệu.</td></tr>');
    }
}
function initializeDataTable() {
    const table = $("#bodycontent");
    table.DataTable({
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
                title: 'Danh sách chi tiết thống kê - Excel'
            },
            {
                extend: 'print',
                title: 'Danh sách chi tiết thống kê'
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