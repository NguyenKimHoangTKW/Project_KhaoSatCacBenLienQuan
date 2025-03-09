$(".select2").select2();
$(document).ready(function () {
    load_data();
    $("#cancelmodalAdd").click(function () {
        $('#TieuDe').val('');
        $('#MoTa').val('');
        $('input[name="DanhChoHe"]').prop('checked', false);
        $('#MaDoiTuong').val('');
        $('#NgayBatDau').val('');
        $('#NgayKetThuc').val('');
    });
});
function formatDateTimeLocal(unixTimestamp) {
    var date = new Date(unixTimestamp * 1000);
    var year = date.getFullYear();
    var month = ("0" + (date.getMonth() + 1)).slice(-2);
    var day = ("0" + date.getDate()).slice(-2);
    var hours = ("0" + date.getHours()).slice(-2);
    var minutes = ("0" + date.getMinutes()).slice(-2);

    return `${year}-${month}-${day}T${hours}:${minutes}`;
}
$(document).on("click", ".btnChiTiet", function () {
    const id = $(this).data("id");
    window.location.href = `/admin/chi-tiet-phieu-khao-sat/${id}`;
});
async function load_data() {
    const hdtid = $("#hdtid").val();
    const loaikhaosatid = $("#loaikhaosatid").val();
    const namid = $("#namid").val();
    const StatusSurvey = $("#StatusSurvey").val();
    if ($.fn.DataTable.isDataTable('#datatable')) {
        $('#datatable').DataTable().clear().destroy();
    }
    const res = await $.ajax({
        url: '/api/admin/danh-sach-phieu-khao-sat',
        type: 'POST',
        data: {
            id_hedaotao: hdtid,
            id_loaikhaosat: loaikhaosatid,
            id_namhoc: namid,
            surveyStatus: StatusSurvey
        }
    });

    if (res.success) {
        var items = res.data;
        items.sort(function (a, b) {
            var maPhieuA = a.ten_phieu.split(".")[0];
            var maPhieuB = b.ten_phieu.split(".")[0];
            return maPhieuA.localeCompare(maPhieuB, undefined, { numeric: true, sensitivity: 'base' });
        });

        var html = "";
        items.forEach(function (survey, index) {
            var MaPhieu = survey.ten_phieu.split(".")[0];
            var formattedNgayTao = unixTimestampToDate(survey.ngay_tao);
            var formattedNgayChinhSua = unixTimestampToDate(survey.ngay_cap_nhat);
            var formattedNgayBatDau = unixTimestampToDate(survey.ngay_bat_dau);
            var formattedNgayKetThuc = unixTimestampToDate(survey.ngay_ket_thuc);
            var trangThaiText = survey.trang_thai == 0 ? 'ĐANG ĐÓNG' : 'ĐANG MỞ';
            var styleTrangThaiText = survey.trang_thai == 0 ? "color: red" : "color: #112bf2";

            html += `
                <tr>
                    <td class="formatSo">${index + 1}</td>
                    <td>${survey.nguoi_tao}</td>
                    <td>${survey.ten_hdt}</td>
                    <td>${survey.ten_phieu}</td>
                    <td>${survey.mo_ta}</td>
                    <td>${survey.loai_khao_sat}</td>
                    <td class="formatSo">${formattedNgayBatDau}</td>
                    <td class="formatSo">${formattedNgayKetThuc}</td>
                    <td class="formatSo">${formattedNgayTao}</td>
                    <td class="formatSo">${formattedNgayChinhSua}</td>
                    <td class="formatSo">${survey.nam}</td>
                    <td style="${styleTrangThaiText};font-weight:bold">${trangThaiText}</td>
                    <td>
                        <button class="btn btn-info m-r-5 btnChiTiet" data-id="${survey.ma_phieu}">Chi tiết</button>
                    </td>
                </tr>
            `;
        });

        $('#card-view').html(html);
        $('#datatable').DataTable({
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
                    title: 'Danh sách người dùng - Excel'
                },
                {
                    extend: 'print',
                    title: 'Danh sách người dùng'
                }
            ]
        });

    } else {
        $('#card-view').html(`<tr><td colspan='13' class='text-center'>${res.message}</td></tr>`);
    }
}
$(document).on("click", "#btnFilter", function () {
    load_data();
})
$(document).on("click", "#btnSave", function (event) {
    event.preventDefault();
    add_new_survey();
})
async function add_new_survey() {
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
        url: '/api/admin/them-moi-phieu-khao-sat',
        type: 'POST',
        data: {
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
};