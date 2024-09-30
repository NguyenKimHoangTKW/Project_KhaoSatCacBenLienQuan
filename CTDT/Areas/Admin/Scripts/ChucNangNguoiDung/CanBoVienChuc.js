var currentPage = 1;
var totalPages = 0;
$("#FiterDonvi,#FilterChucvu,#FilterCTDT,#FilterNam,#Fitertrangthai").select2();
$(document).ready(function () {
    get_data()
    $("#btnFilter").click(function () {
        get_data()
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
            url: '/Admin/CBVC/UploadExcel',
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
                        get_data()
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
    var filterdonvi = $("#FiterDonvi").val();
    var filterchucvu = $("#FilterChucvu").val();
    var filterctdt = $("#FilterCTDT").val();
    var filternam = $("#FilterNam").val();
    var filtertrangthai = $("#Fitertrangthai").val()
    $('#cbvcTable').DataTable({
        "processing": true,
        "serverSide": false,
        "autoFill": true,
        "ajax": {
            "url": "/Admin/CBVC/load_cbvc",
            "type": "POST",
            "dataSrc": "data",
            "data": function (d) {
                d.filterdonvi = filterdonvi;
                d.filterchucvu = filterchucvu;
                d.filterctdt = filterctdt;
                d.filtertrangthai = filtertrangthai;
                d.filternamhoatdong = filternam;
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
            { "data": "id_cbvc" },
            { "data": "ma_cbvc" },
            { "data": "ten_cbvc" },
            { "data": "ngay_sinh" },
            { "data": "email" },
            { "data": "don_vi" },
            { "data": "chuc_vu" },
            { "data": "chuong_trinh_dao_tao" },
            { "data": "trang_thai" },
            { "data": "nam_hoat_dong" },
            {
                "data": null,
                "render": function (data, type, row) {
                    return "<button class='btn btn-icon btn-hover btn-sm btn-rounded pull-right' id='btnEdit' data-toggle='modal' data-target='#ModalEditCTDT' data-id='" + row.id_cbvc + "'><i class='anticon anticon-edit'></i></button> " +
                        "<button class='btn btn-icon btn-hover btn-sm btn-rounded pull-right' id='btnDelete' data-id='" + row.id_cbvc + "' data-name='" + row.ten_cbvc + "'><i class='anticon anticon-delete'></i></button>";
                },
                "orderable": false,
                "title": "Chức Năng"
            }
        ],
        "destroy": true,
        "dom": "Bfrtip",
        "buttons": ['csv', 'excel', 'pdf', 'print']
    });
} function unixTimestampToDate(unixTimestamp) {
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