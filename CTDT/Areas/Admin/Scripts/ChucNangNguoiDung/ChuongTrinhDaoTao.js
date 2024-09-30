$('.select2').select2();
$(document).ready(function () {
    get_data()
    $(document).on("click", "#btnSave", function () {
        AddCTDT();
    });
    $("#btnFilter").click(function () {
        get_data()
    });
    $(document).on("click", "#btnEdit", function () {
        var MaCTDT = $(this).data("id");
        GetByID(MaCTDT);
    });

    $(document).on("click", "#btnSaveChange", function () {
        EditCTDT();
    });

    $(document).on("click", "#btnDelete", function () {
        var MaCTDT = $(this).data("id");
        var TenCTDT = $(this).data("name");
        if (confirm("Bạn có chắc muốn xóa khoa '" + TenCTDT + "' không?")) {
            DelCTDT(MaCTDT);
        }
    });
    $('#importExcelForm').on('submit', function (e) {
        e.preventDefault();
        var formData = new FormData(this);
        Swal.fire({
            title: 'Đang nhập dữ liệu...',
            html: 'Vui lòng chờ.',
            allowOutsideClick: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });

        $.ajax({
            url: '/Admin/CTDT/UploadExcel',
            type: 'POST',
            data: formData,
            contentType: false,
            processData: false,
            success: function (response) {
                Swal.close();

                if (response.status.includes('Thêm người dùng thành công')) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Thành công',
                        text: response.status,
                    }).then(() => {
                        $('#importExcelModal').modal('hide');
                        LoadData(currentPage);
                    });
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Lỗi',
                        text: response.status,
                    });
                }
            },
            error: function (xhr, status, error) {
                Swal.close();
                Swal.fire({
                    icon: 'error',
                    title: 'Đã xảy ra lỗi',
                    text: 'Đã xảy ra lỗi: ' + error,
                });
            }
        });
    });
});
function get_data() {
    var filterctdt = $("#FiterCTDT").val();
    var filterkhoa = $("#FilterKhoa").val();
    var filterhdt = $("#FilterHDT").val();
    $('#ctdtTable').DataTable({

        "processing": true,
        "serverSide": false,
        "autoFill": true,
        "ajax": {
            "url": "/Admin/CTDT/load_ctdt",
            "type": "GET",
            "dataSrc": "data",
            "data": function (d) {
                d.filctdt = filterctdt;
                d.filkhoa = filterkhoa;
                d.filhdt = filterhdt;
            }
        },
        "columns": [
            {
                "data": null,
                "render": function (data, type, row, meta) {
                    return meta.row + 1;
                },
                "title": "Số Thứ Tự"
            },
            { "data": "id_ctdt" },
            { "data": "ma_ctdt" },
            { "data": "ten_khoa" },
            { "data": "ten_ctdt" },
            { "data": "ten_hdt" },
            {
                "data": "ngay_tao",
                "render": function (data, type, row) {
                    return unixTimestampToDate(data);
                }
            },
            {
                "data": "ngay_cap_nhat",
                "render": function (data, type, row) {
                    return unixTimestampToDate(data);
                }
            },
            {
                "data": null,
                "render": function (data, type, row) {
                    return "<button class='btn btn-icon btn-hover btn-sm btn-rounded pull-right' id='btnEdit' data-toggle='modal' data-target='#ModalEditCTDT' data-id='" + row.id_ctdt + "'><i class='anticon anticon-edit'></i></button> " +
                        "<button class='btn btn-icon btn-hover btn-sm btn-rounded pull-right' id='btnDelete' data-id='" + row.id_ctdt + "' data-name='" + row.ten_ctdt + "'><i class='anticon anticon-delete'></i></button>";
                },
                "orderable": false,
                "title": "Chức Năng"
            }
        ],
        "destroy": true,
        "language": {
            "emptyTable": "Không có dữ liệu tồn tại, vui lòng <b>Thêm mới</b>"
        },
        "dom": "Bfrtip",
        "buttons": ['csv', 'excel', 'pdf', 'print']
    });
}

function GetByID(id) {
    $.ajax({
        url: '/Admin/CTDT/GetByID',
        type: 'GET',
        data: { id: id },
        success: function (res) {
            $('#Edit_MaCTDT').val(res.data.MaCTDT);
            $('#Edit_TenCTDT').val(res.data.TenCTDT);
            $('#Edit_Ma_Khoa').val(res.data.MaKhoa);
            $('#change_Ngay_Tao').val(res.data.NgayTao);
            $('#change_Ngay_Cap_Nhat').val(res.data.NgayCapNhat);
        }
    });
}
function AddCTDT() {
    var ten_ctdt = $("#ten_ctdt").val().trim();
    var MaKhoa = $("#MaKhoa").val().trim();
    $.ajax({
        url: '/Admin/CTDT/Add',
        type: 'POST',
        dataType: 'JSON',
        contentType: "application/json; charset=utf-8",
        data: JSON.stringify({
            ten_ctdt: ten_ctdt,
            id_khoa: MaKhoa
        }),
        success: function (response) {
            alert(response.status);
            LoadData(currentPage);
        },
        error: function (response) {
            alert(response.status)
        },
    });
};

function EditCTDT() {
    var TenCTDT = $('#Edit_TenCTDT').val();
    var MaCTDT = $('#Edit_MaCTDT').val()
    var MaKhoa = $('#Edit_Ma_Khoa').val();
    var NgayTao = $('#change_Ngay_Tao').val();
    var NgayCapNhat = $('#change_Ngay_Cap_Nhat').val();
    var ctdt = {
        id_ctdt: MaCTDT,
        ten_ctdt: TenCTDT,
        id_khoa: MaKhoa,
        ngaycapnhat: NgayCapNhat,
        ngaytao: NgayTao,
    };

    $.ajax({
        type: 'POST',
        url: '/Admin/CTDT/Edit',
        dataType: 'JSON',
        contentType: "application/json; charset=utf-8",
        data: JSON.stringify(ctdt),
        success: function (response) {
            alert(response.status);
            LoadData(currentPage);
        }
    });
};

function DelCTDT(id) {
    $.ajax({
        type: 'POST',
        url: '/Admin/CTDT/Delete',
        data: { id: id },
        success: function (response) {
            alert(response.status);
            LoadData(currentPage);
        },
        error: function (response) {
            alert(response.status)
        },
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