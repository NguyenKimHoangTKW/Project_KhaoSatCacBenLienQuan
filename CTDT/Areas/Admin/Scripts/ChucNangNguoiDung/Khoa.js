$(document).ready(function () {
    $("#btnSave").click(function () {
        AddKhoa();
    });
    get_data();
    var debouncedLoadData = debounce(function () {
        var keyword = $('#searchInput').val().toLowerCase();
        LoadData(currentPage, keyword);
    }, 1000);

    $('#searchInput').on('input', debouncedLoadData);
    $(document).on("click", "#btnEdit", function () {
        var id_khoa = $(this).data("id");
        GetByID(id_khoa);
    })

    $(document).on("click", "#btnDelete", function () {
        var id_khoa = $(this).data("id");
        var TenKhoa = $(this).data("name");
        if (confirm("Bạn có chắc muốn xóa khoa '" + TenKhoa + "' không?")) {
            DelKhoa(id_khoa);
        }
    });

    $(document).on("click", "#btnSaveChange", function () {
        EditKhoa();
    });
    function get_data() {
        $('#khoaTable').DataTable({
            "processing": true,
            "serverSide": false,
            "ajax": {
                "url": "/Admin/Khoa/load_khoa",
                "type": "GET",
                "dataSrc": "data"
            },
            "columns": [
                {
                    "data": null,
                    "render": function (data, type, row, meta) {
                        return meta.row + 1;
                    },
                    "title": "Số Thứ Tự"
                },
                { "data": "id_khoa" },
                { "data": "ma_khoa" },
                { "data": "ten_khoa" },
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
                        return "<button class='btn btn-icon btn-hover btn-sm btn-rounded pull-right' id='btnEdit' data-toggle='modal' data-target='#ModalEditKhoa' data-id='" + row.id_khoa + "'><i class='anticon anticon-edit'></i></button> " +
                            "<button class='btn btn-icon btn-hover btn-sm btn-rounded pull-right' id='btnDelete' data-id='" + row.id_khoa + "' data-name='" + row.ten_khoa + "'><i class='anticon anticon-delete'></i></button>";
                    },
                    "orderable": false, 
                    "title": "Chức Năng"
                }
            ],
            "destroy": true,
            "language": {
                "emptyTable":"Không có dữ liệu tồn tại, vui lòng <b>Thêm mới</b>"
            },
            "dom": "Bfrtip",
            "buttons": ['csv', 'excel', 'pdf', 'print']
        });
    }
    function AddKhoa() {
        var ma_khoa = $("#ma_khoa").val();
        var ten_khoa = $("#ten_khoa").val();
        $.ajax({
            url: '/Khoa/Add',
            type: 'POST',
            dataType: 'JSON',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({
                ma_khoa: ma_khoa,
                ten_khoa: ten_khoa
            }),
            success: function (response) {
                alert(response.status);
                get_data()
            },
            error: function (response) {
                alert(response.status);
            }
        });
    }

    function GetByID(id) {
        $.ajax({
            url: '/Khoa/GetByID',
            type: 'GET',
            data: { id: id },
            success: function (res) {
                $('#Change_ten_khoa').val(res.data.ten_khoa);
                $('#Change_Id_Khoa').val(res.data.id_khoa);
                $('#Change_ma_khoa').val(res.data.ma_khoa);
                $('#change_Ngay_Tao').val(res.data.ngaytao);
                $('#change_Ngay_Cap_Nhat').val(res.data.ngaycapnhat);
            }
        });
    }

    function EditKhoa() {
        var TenKhoa = $('#Change_ten_khoa').val();
        var IDKhoa = $('#Change_Id_Khoa').val();
        var MaKhoa = $('#Change_ma_khoa').val();
        var NgayTao = $('#change_Ngay_Tao').val();
        var NgayCapNhat = $('#change_Ngay_Cap_Nhat').val();
        var khoa = {
            ten_khoa: TenKhoa,
            id_khoa: IDKhoa,
            ma_khoa: MaKhoa,
            ngaycapnhat: NgayCapNhat,
            ngaytao: NgayTao,
        };

        $.ajax({
            type: 'POST',
            url: '/Khoa/Edit',
            dataType: 'JSON',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify(khoa),
            success: function (response) {
                alert(response.status);
                get_data()
            }
        });
    }

    function DelKhoa(id) {
        $.ajax({
            type: 'POST',
            url: '/Khoa/Delete',
            data: { id: id },
            success: function (response) {
                alert(response.status);
                get_data()
            },
            error: function (response) {
                alert(response.status);
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
        var formattedDate = dayOfWeek + ', ' + day + "-" + month + "-" + year + " " + hours + ":" + minutes + ":" + seconds;
        return formattedDate;
    }
});