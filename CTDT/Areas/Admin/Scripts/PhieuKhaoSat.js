var currentPage = 1;
var totalPages = 0;
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
$(document).on("click", "#EditPKS", function () {
    var id = $(this).data("id");
    GetByID(id);
});
function GetByID(id) {
    $.ajax({
        url: "/Admin/PhieuKhaoSat/GetByIDPKS",
        type: "GET",
        data: { id: id },
        success: function (res) {
            var items = res.data;
            if (items) {
                $("#EditID").val(items.IdPhieu);
                $("#EditTieuDe").val(items.TieuDe);
                $("#EditMoTa").val(items.MoTa);
                $('input[name="EditDanhChoHe"][value="' + items.HeDaoTao + '"]').prop('checked', true);
                $("#EditMaDoiTuong").val(items.DoiTuong);
                $("#EditNgayBatDau").val(formatDateTimeLocal(items.NgayBatDau));
                $("#EditNgayKetThuc").val(formatDateTimeLocal(items.NgayKetThuc));
                $("#EditMaNamHoc").val(items.NamHoc);
                $("#EditTrangThai").val(items.TrangThai);

                var keyClassArray = JSON.parse(items.key_class);
                $('input[name="eidt_radioKey"]').each(function () {
                    if (keyClassArray.includes($(this).val())) {
                        $(this).prop('checked', true);
                    } else {
                        $(this).prop('checked', false);
                    }
                });
            }
        }
    });
}

$(document).on("click", "#btnEdit", function () {
    var id = $("#EditID").val();
    EditPKS(id);
});
function EditPKS(id) {
    var tieuDe = $('#EditTieuDe').val();
    var moTa = $('#EditMoTa').val();
    var danhChoHe = $('input[name="EditDanhChoHe"]:checked').val();
    var maDoiTuong = $('#EditMaDoiTuong').val();
    var ngayBatDau = $('#EditNgayBatDau').val();
    var ngayKetThuc = $('#EditNgayKetThuc').val();
    var trangThai = $('#EditTrangThai').val();
    var maNamHoc = $("#EditMaNamHoc").val();
    var checked_key_class = [];
    $('input[name="eidt_radioKey"]:checked').each(function () {
        checked_key_class.push($(this).val());
    });
    var unixNgayBatDau = Math.floor(new Date(ngayBatDau).getTime() / 1000);
    var unixNgayKetThuc = Math.floor(new Date(ngayKetThuc).getTime() / 1000);

    var Checkdata = {
        id: id,
        surveyTitle: tieuDe,
        surveyDescription: moTa,
        id_hedaotao: danhChoHe,
        id_loaikhaosat: maDoiTuong,
        id_namhoc: maNamHoc,
        surveyTimeStart: unixNgayBatDau,
        surveyTimeEnd: unixNgayKetThuc,
        surveyStatus: trangThai,
        key_class: JSON.stringify(checked_key_class)
    };
    $.ajax({
        url: "/Admin/PhieuKhaoSat/EditPhieuKhaoSat",
        type: "POST",
        dataType: "JSON",
        contentType: "application/json; charset=utf-8",
        data: JSON.stringify(Checkdata),
        success: function (res) {
            if (res.success) {
                Swal.fire({
                    text: res.message,
                    icon: 'success',
                    showConfirmButton: false,
                    timer: 1500
                }).then(function () {
                    $("#EditSurvey").modal("hide");
                    FilData();
                });
            }
            else {
                Swal.fire({
                    icon: "error",
                    title: "Oops...",
                    text: res.message,
                    confirmButtonText: 'OK'
                });
            }
        }
    });
}
function formatDateTimeLocal(unixTimestamp) {
    var date = new Date(unixTimestamp * 1000);
    var year = date.getFullYear();
    var month = ("0" + (date.getMonth() + 1)).slice(-2);
    var day = ("0" + date.getDate()).slice(-2);
    var hours = ("0" + date.getHours()).slice(-2);
    var minutes = ("0" + date.getMinutes()).slice(-2);

    return `${year}-${month}-${day}T${hours}:${minutes}`;
}
$(document).on("click", "#DelPKS", function () {
    var id = $(this).data("id");
    Swal.fire({
        icon: 'warning',
        text: "Bạn có chắc muốn xóa phiếu khảo sát này không",
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Xóa',
        cancelButtonText: 'Hủy'
    }).then((result) => {
        if (result.isConfirmed) {
            DelPKS(id);
        }
    });
});
function DelPKS(id) {
    $.ajax({
        url: "/Admin/PhieuKhaoSat/DeletePKS",
        type: "POST",
        data: { id: id },
        success: function (res) {
            if (res.success) {
                Swal.fire({
                    text: res.message,
                    icon: 'success',
                    showConfirmButton: false,
                    timer: 1500
                }).then(function () {
                    FilData();
                });
            }
            else {
                Swal.fire({
                    icon: "error",
                    title: "Oops...",
                    text: res.message,
                    confirmButtonText: 'OK'
                });
            }
        },
        error: function () {
            Swal.fire({
                icon: "error",
                title: "Oops...",
                text: "Đã xảy ra lỗi trong quá trình xử lý",
                confirmButtonText: 'OK'
            });
        }
    });
};
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

    } else {
        $('#card-view').html(`<tr><td colspan='13' class='text-center'>${res.message}</td></tr>`);
    }
}
$(document).on("click", "#btnFilter", function () {
    load_data();
})


function AddPKS() {
    var tieuDe = $('#TieuDe').val();
    var moTa = $('#MoTa').val();
    var danhChoHe = $('input[name="DanhChoHe"]:checked').val();
    var maDoiTuong = $('#MaDoiTuong').val();
    var ngayBatDau = $('#NgayBatDau').val();
    var ngayKetThuc = $('#NgayKetThuc').val();
    var trangThai = $('#TrangThai').val();
    var maNamHoc = $("#MaNamHoc").val();
    var checked_key_class = [];
    $('input[name="radioKey"]:checked').each(function () {
        checked_key_class.push($(this).val());
    });
    var unixNgayBatDau = Math.floor(new Date(ngayBatDau).getTime() / 1000);
    var unixNgayKetThuc = Math.floor(new Date(ngayKetThuc).getTime() / 1000);

    var data = {
        surveyTitle: tieuDe,
        surveyDescription: moTa,
        id_hedaotao: danhChoHe,
        id_loaikhaosat: maDoiTuong,
        id_namhoc: maNamHoc,
        surveyTimeStart: unixNgayBatDau,
        surveyTimeEnd: unixNgayKetThuc,
        surveyStatus: trangThai,
        key_class: JSON.stringify(checked_key_class)
    };
    $.ajax({
        url: '/Admin/PhieuKhaoSat/NewSurvey',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function (response) {
            Swal.fire({
                icon: 'success',
                title: response.status,
                showConfirmButton: false,
                timer: 2000
            });
            FilData();
        },
        error: function () {
            alert('Lưu không thành công.');
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
};