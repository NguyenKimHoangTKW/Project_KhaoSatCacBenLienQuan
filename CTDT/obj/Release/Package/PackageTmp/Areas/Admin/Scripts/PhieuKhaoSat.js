var currentPage = 1;
var totalPages = 0;
$(".select2").select2();
$(document).ready(function () {
    LoadData(currentPage);
    $('#Test').click(function () {
        var selectedValues = [];
        $('input[name="radioKey"]:checked').each(function () {
            selectedValues.push($(this).val());
        });
        console.log(selectedValues);
    });
    $("#cancelmodalAdd").click(function () {
        $('#TieuDe').val('');
        $('#MoTa').val('');
        $('input[name="DanhChoHe"]').prop('checked', false);
        $('#MaDoiTuong').val('');
        $('#NgayBatDau').val('');
        $('#NgayKetThuc').val('');
    });
    $("#btnFilter").click(function () {
        var selectHdt = $("#surveyTypeTS").val();
        var selectLKS = $("#surveyType").val();
        var keyword = $('#keywordSearch').val().toLowerCase();
        var status = $('#StatusSurvey').val();
        var year = $('#YearSurvey').val();
        LoadData(1, selectHdt, selectLKS, status, year, keyword);
    });

    $(document).on("click", ".page-link", function () {
        var page = $(this).data("page");
        if (page > 0 && page <= totalPages) {
            var selectHdt = $("#surveyTypeTS").val();
            var selectLKS = $("#surveyType").val();
            var keyword = $('#keywordSearch').val().toLowerCase();
            var status = $('#StatusSurvey').val();
            var year = $('#YearSurvey').val();
            LoadData(page, selectHdt, selectLKS, status, year, keyword);
        }
    });
});
function FilData() {
    var selectHdt = $("#surveyTypeTS").val();
    var selectLKS = $("#surveyType").val();
    var keyword = $('#keywordSearch').val().toLowerCase();
    var status = $('#StatusSurvey').val();
    var year = $('#YearSurvey').val();
    LoadData(1, selectHdt, selectLKS, status, year, keyword);
}
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
        id : id,
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
function LoadData(pageNumber, hdt, loaiks, status, year, keyword) {
    $.ajax({
        url: '/PhieuKhaoSat/LoadPhieu',
        type: 'GET',
        data: {
            pageNumber: pageNumber,
            pageSize: 8,
            hdt: hdt,
            loaiks: loaiks,
            surveystatus: status,
            year: year,
            keyword: keyword
        },
        success: function (res) {
            var items = res.data;
            totalPages = res.totalPages;
            items.sort(function (a, b) {
                var maPhieuA = a.TieuDePhieu.split(".")[0];
                var maPhieuB = b.TieuDePhieu.split(".")[0];
                return maPhieuA.localeCompare(maPhieuB, undefined, { numeric: true, sensitivity: 'base' });
            });

            var html = "";
            var logoUrl = '/Areas/assets/images/logo/logo_test_2.png';
            if (items.length === 0) {
                html += "<tr><td colspan='7' class='text-center'>Không có dữ liệu</td></tr>";
            } else {
                for (let i = 0; i < items.length; i++) {
                    MaPhieu = items[i].TieuDePhieu.split(".")[0];
                    var formattedNgayTao = unixTimestampToDate(items[i].NgayTao);
                    var formattedNgayChinhSua = unixTimestampToDate(items[i].NgayChinhSua);
                    var formattedNgayBatDau = unixTimestampToDate(items[i].NgayBatDau);
                    var formattedNgayKetThuc = unixTimestampToDate(items[i].NgayKetThuc);
                    var trangThaiText = items[i].TrangThai == 0 ? 'ĐANG ĐÓNG' : 'ĐANG MỞ';
                    var styleTrangThaiText = items[i].TrangThai == 0 ? "color: red" : "color: #112bf2";
                    html += "<div class='col-lg-3 col-sm-12'>";
                    html += "    <div class='card bg-white m-b-30' style='align-self: center; text-align: center'>";
                    html += "        <div class='card-header'>";
                    html += "            <span style='color: #333;font-weight: bold;font-size: 15px'>HỆ " + items[i].TenHDT + " </span>";
                    html += "            -";
                    html += "            <span style='color: #333;font-weight: bold;font-size: 15px'>DÀNH CHO " + items[i].LoaiKhaoSat + "</span>";
                    html += "            -";
                    html += "            <span style='color: #333; font-weight: bold; font-size: 18px;" + styleTrangThaiText +"'>" + trangThaiText + "</span>";
                    html += "        </div>";
                    html += "        <img src='" + logoUrl + "' />";
                    html += "        <div class='card-body'>";
                    html += "            <div class='card-title mb-4' style='font-size: 14px; font-family: \'Inter\';font-style: normal;font-weight: 600;line-height: 150%;overflow: hidden'>";
                    html += "                <a href='#' style='color: #333'>" + items[i].TieuDePhieu + "</a>";
                    html += "                <br>";
                    html += "                <button onclick=\"window.location.href='/Admin/PhieuKhaoSat/KetQuaPKS?id=" + items[i].MaPhieu + "'\" type='button' class='btn btn-success' style='display: inline-flex; align-items: center;'>";
                    html += "                   <i class='fas fa-file-alt' style='font-size: 16px;'></i>";
                    html += "                </button>";
                    html += "                <button type='button' class='btn btn-success' id='EditPKS' style='display: inline-flex; align-items: center;' data-toggle='modal' data-target='#EditSurvey' data-id='" + items[i].MaPhieu + "'>";
                    html += "                    <i class='fas fa-edit' style='font-size: 16px;'></i>";
                    html += "                </button>";
                    html += "                <button onclick=\"window.location.href='/Admin/PhieuKhaoSat/AddSurvey?id=" + items[i].MaPhieu + "'\" type='button' class='btn btn-secondary' style='display: inline-flex; align-items: center;'>";
                    html += "                    <svg xmlns='http://www.w3.org/2000/svg' width='16' height='16' fill='currentColor' class='bi bi-pencil' viewBox='0 0 16 16'>";
                    html += "                        <path d='M12.146.146a.5.5 0 0 1 .708 0l3 3a.5.5 0 0 1 0 .708l-10 10a.5.5 0 0 1-.168.11l-5 2a.5.5 0 0 1-.65-.65l2-5a.5.5 0 0 1 .11-.168zM11.207 2.5 13.5 4.793 14.793 3.5 12.5 1.207zm1.586 3L10.5 3.207 4 9.707V10h.5a.5.5 0 0 1 .5.5v.5h.5a.5.5 0 0 1 .5.5v.5h.293zm-9.761 5.175-.106.106-1.528 3.821 3.821-1.528.106-.106A.5.5 0 0 1 5 12.5V12h-.5a.5.5 0 0 1-.5-.5V11h-.5a.5.5 0 0 1-.468-.325'></path>";
                    html += "                    </svg>";
                    html += "                </button>";
                    html += "                <button id='DelPKS' type='button' class='btn btn-danger' style='display: inline-flex; align-items: center;' data-id='" + items[i].MaPhieu + "'>";
                    html += "                    <svg xmlns='http://www.w3.org/2000/svg' width='16' height='16' fill='currentColor' class='bi bi-trash3-fill' viewBox='0 0 16 16'>";
                    html += "                        <path d='M11 1.5v1h3.5a.5.5 0 0 1 0 1h-.538l-.853 10.66A2 2 0 0 1 11.115 16h-6.23a2 2 0 0 1-1.994-1.84L2.038 3.5H1.5a.5.5 0 0 1 0-1H5v-1A1.5 1.5 0 0 1 6.5 0h3A1.5 1.5 0 0 1 11 1.5m-5 0v1h4v-1a.5.5 0 0 0-.5-.5h-3a.5.5 0 0 0-.5.5M4.5 5.029l.5 8.5a.5.5 0 1 0 .998-.06l-.5-8.5a.5.5 0 1 0-.998.06m6.53-.528a.5.5 0 0 0-.528.47l-.5 8.5a.5.5 0 0 0 .998.058l.5-8.5a.5.5 0 0 0-.47-.528M8 4.5a.5.5 0 0 0-.5.5v8.5a.5.5 0 0 0 1 0V5a.5.5 0 0 0-.5-.5'></path>";
                    html += "                    </svg>";
                    html += "                </button>";
                    html += "            </div>";
                    html += "            <ul class='list-group list-group-flush'>";
                    html += "                <li class='list-group-item'>";
                    html += "                    <svg xmlns='http://www.w3.org/2000/svg' width='16' height='16' fill='currentColor' class='bi bi-person-circle' viewBox='0 0 16 16'>";
                    html += "                        <path d='M11 6a3 3 0 1 1-6 0 3 3 0 0 1 6 0'></path>";
                    html += "                        <path fill-rule='evenodd' d='M0 8a8 8 0 1 1 16 0A8 8 0 0 1 0 8m8-7a7 7 0 0 0-5.468 11.37C3.242 11.226 4.805 10 8 10s4.757 1.225 5.468 2.37A7 7 0 0 0 8 1'></path>";
                    html += "                    </svg> Người tạo: <span style='font-weight: bold'>" + items[i].NguoiTao + "</span>";
                    html += "                </li>";
                    html += "                <li class='list-group-item'>";
                    html += "                    <svg xmlns='http://www.w3.org/2000/svg' width='16' height='16' fill='currentColor' class='bi bi-calendar-date-fill' viewBox='0 0 16 16'>";
                    html += "                        <path d='M4 .5a.5.5 0 0 0-1 0V1H2a2 2 0 0 0-2 2v1h16V3a2 2 0 0 0-2-2h-1V.5a.5.5 0 0 0-1 0V1H4zm5.402 9.746c.625 0 1.184-.484 1.184-1.18 0-.832-.527-1.23-1.16-1.23-.586 0-1.168.387-1.168 1.21 0 .817.543 1.2 1.144 1.2'></path>";
                    html += "                        <path d='M16 14V5H0v9a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2m-6.664-1.21c-1.11 0-1.656-.767-1.703-1.407h.683c.043.37.387.82 1.051.82.844 0 1.301-.848 1.305-2.164h-.027c-.153.414-.637.79-1.383.79-.852 0-1.676-.61-1.676-1.77 0-1.137.871-1.809 1.797-1.809 1.172 0 1.953.734 1.953 2.668 0 1.805-.742 2.871-2 2.871zm-2.89-5.435v5.332H5.77V8.079h-.012c-.29.156-.883.52-1.258.777V8.16a13 13 0 0 1 1.313-.805h.632z'></path>";
                    html += "                    </svg> Ngày tạo phiếu <br><span style='font-weight: bold'>" + formattedNgayTao + "</span>";
                    html += "                </li>";
                    html += "                <li class='list-group-item'>";
                    html += "                    <svg xmlns='http://www.w3.org/2000/svg' width='16' height='16' fill='currentColor' class='bi bi-calendar-date-fill' viewBox='0 0 16 16'>";
                    html += "                        <path d='M4 .5a.5.5 0 0 0-1 0V1H2a2 2 0 0 0-2 2v1h16V3a2 2 0 0 0-2-2h-1V.5a.5.5 0 0 0-1 0V1H4zm5.402 9.746c.625 0 1.184-.484 1.184-1.18 0-.832-.527-1.23-1.16-1.23-.586 0-1.168.387-1.168 1.21 0 .817.543 1.2 1.144 1.2'></path>";
                    html += "                        <path d='M16 14V5H0v9a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2m-6.664-1.21c-1.11 0-1.656-.767-1.703-1.407h.683c.043.37.387.82 1.051.82.844 0 1.301-.848 1.305-2.164h-.027c-.153.414-.637.79-1.383.79-.852 0-1.676-.61-1.676-1.77 0-1.137.871-1.809 1.797-1.809 1.172 0 1.953.734 1.953 2.668 0 1.805-.742 2.871-2 2.871zm-2.89-5.435v5.332H5.77V8.079h-.012c-.29.156-.883.52-1.258.777V8.16a13 13 0 0 1 1.313-.805h.632z'></path>";
                    html += "                    </svg> Ngày cập nhật <br><span style='font-weight: bold'>" + formattedNgayChinhSua + "</span>";
                    html += "                </li>";
                    html += "                <li class='list-group-item'>";
                    html += "                    <svg xmlns='http://www.w3.org/2000/svg' width='16' height='16' fill='currentColor' class='bi bi-calendar-date-fill' viewBox='0 0 16 16'>";
                    html += "                        <path d='M4 .5a.5.5 0 0 0-1 0V1H2a2 2 0 0 0-2 2v1h16V3a2 2 0 0 0-2-2h-1V.5a.5.5 0 0 0-1 0V1H4zm5.402 9.746c.625 0 1.184-.484 1.184-1.18 0-.832-.527-1.23-1.16-1.23-.586 0-1.168.387-1.168 1.21 0 .817.543 1.2 1.144 1.2'></path>";
                    html += "                        <path d='M16 14V5H0v9a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2m-6.664-1.21c-1.11 0-1.656-.767-1.703-1.407h.683c.043.37.387.82 1.051.82.844 0 1.301-.848 1.305-2.164h-.027c-.153.414-.637.79-1.383.79-.852 0-1.676-.61-1.676-1.77 0-1.137.871-1.809 1.797-1.809 1.172 0 1.953.734 1.953 2.668 0 1.805-.742 2.871-2 2.871zm-2.89-5.435v5.332H5.77V8.079h-.012c-.29.156-.883.52-1.258.777V8.16a13 13 0 0 1 1.313-.805h.632z'></path>";
                    html += "                    </svg> Ngày bắt đầu <br><span style='font-weight: bold'>" + formattedNgayBatDau + "</span>";
                    html += "                </li>";
                    html += "                <li class='list-group-item'>";
                    html += "                    <svg xmlns='http://www.w3.org/2000/svg' width='16' height='16' fill='currentColor' class='bi bi-calendar-date-fill' viewBox='0 0 16 16'>";
                    html += "                        <path d='M4 .5a.5.5 0 0 0-1 0V1H2a2 2 0 0 0-2 2v1h16V3a2 2 0 0 0-2-2h-1V.5a.5.5 0 0 0-1 0V1H4zm5.402 9.746c.625 0 1.184-.484 1.184-1.18 0-.832-.527-1.23-1.16-1.23-.586 0-1.168.387-1.168 1.21 0 .817.543 1.2 1.144 1.2'></path>";
                    html += "                        <path d='M16 14V5H0v9a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2m-6.664-1.21c-1.11 0-1.656-.767-1.703-1.407h.683c.043.37.387.82 1.051.82.844 0 1.301-.848 1.305-2.164h-.027c-.153.414-.637.79-1.383.79-.852 0-1.676-.61-1.676-1.77 0-1.137.871-1.809 1.797-1.809 1.172 0 1.953.734 1.953 2.668 0 1.805-.742 2.871-2 2.871zm-2.89-5.435v5.332H5.77V8.079h-.012c-.29.156-.883.52-1.258.777V8.16a13 13 0 0 1 1.313-.805h.632z'></path>";
                    html += "                    </svg> Ngày kết thúc <br><span style='font-weight: bold'>" + formattedNgayKetThuc + "</span>";
                    html += "                </li>";
                    html += "                <li class='list-group-item'>";
                    html += "                    <svg xmlns='http://www.w3.org/2000/svg' width='16' height='16' fill='currentColor' class='bi bi-calendar-date-fill' viewBox='0 0 16 16'>";
                    html += "                        <path d='M4 .5a.5.5 0 0 0-1 0V1H2a2 2 0 0 0-2 2v1h16V3a2 2 0 0 0-2-2h-1V.5a.5.5 0 0 0-1 0V1H4zm5.402 9.746c.625 0 1.184-.484 1.184-1.18 0-.832-.527-1.23-1.16-1.23-.586 0-1.168.387-1.168 1.21 0 .817.543 1.2 1.144 1.2'></path>";
                    html += "                        <path d='M16 14V5H0v9a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2m-6.664-1.21c-1.11 0-1.656-.767-1.703-1.407h.683c.043.37.387.82 1.051.82.844 0 1.301-.848 1.305-2.164h-.027c-.153.414-.637.79-1.383.79-.852 0-1.676-.61-1.676-1.77 0-1.137.871-1.809 1.797-1.809 1.172 0 1.953.734 1.953 2.668 0 1.805-.742 2.871-2 2.871zm-2.89-5.435v5.332H5.77V8.079h-.012c-.29.156-.883.52-1.258.777V8.16a13 13 0 0 1 1.313-.805h.632z'></path>";
                    html += "                    </svg> Năm hoạt động <br><span style='font-weight: bold'>" + items[i].Nam + "</span>";
                    html += "                </li>";
                    html += "            </ul>";
                    html += "        </div>";
                    html += "    </div>";
                    html += "</div>";
                }
            }
            $('#card-view').html(html);
            renderPagination(pageNumber, totalPages);
        },
        error: function () {
            var html = "<tr><td colspan='7' class='text-center'>Không thể tải dữ liệu</td></tr>";
            $('#card-view').html(html);
        }
    });
};

function renderPagination(currentPage, totalPages) {
    var html = '<nav aria-label="Page navigation example"><ul class="pagination justify-content-end">';
    html += '<li class="page-item ' + (currentPage == 1 ? 'disabled' : '') + '"><a class="page-link" href="#" data-page="' + (currentPage - 1) + '">Trước</a></li>';

    for (var i = 1; i <= totalPages; i++) {
        html += '<li class="page-item ' + (currentPage == i ? 'active' : '') + '"><a class="page-link" href="#" data-page="' + i + '">' + i + '</a></li>';
    }

    html += '<li class="page-item ' + (currentPage == totalPages ? 'disabled' : '') + '"><a class="page-link" href="#" data-page="' + (currentPage + 1) + '">Tiếp</a></li>';
    html += '</ul></nav>';

    $('#pagination').html(html);
};

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


function debounce(func, delay) {
    let timeoutId;
    return function () {
        const context = this;
        const args = arguments;
        clearTimeout(timeoutId);
        timeoutId = setTimeout(() => func.apply(context, args), delay);
    };
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
};