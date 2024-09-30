$(document).ready(function () {
    var currentPage = 1;
    var totalPages = 0;

    LoadData(currentPage);

    $("#btnSave").click(function () {
        AddKhoa();
    });
    var debouncedLoadData = debounce(function () {
        var keyword = $('#searchInput').val().toLowerCase();
        LoadData(currentPage, keyword);
    }, 1000);

    $('#searchInput').on('input', debouncedLoadData);
    $(document).on("click", "#btnEdit", function () {
        var MaKhoa = $(this).closest("tr").find("td:eq(1)").text();
        GetByID(MaKhoa);
    });

    $(document).on("click", "#btnDelete", function () {
        var MaKhoa = $(this).closest("tr").find("td:eq(1)").text();
        var TenKhoa = $(this).closest("tr").find("td:eq(2)").text();
        if (confirm("Bạn có chắc muốn xóa khoa '" + TenKhoa + "' không?")) {
            DelKhoa(MaKhoa);
        }
    });

    $(document).on("click", "#btnSaveChange", function () {
        EditKhoa();
    });

    $(document).on("click", ".page-link", function () {
        var page = $(this).data("page");
        if (page > 0 && page <= totalPages) {
            currentPage = page;
            var keyword = $('#searchInput').val().toLowerCase();
            LoadData(currentPage, keyword);
        }
    });

    function LoadData(pageNumber, keyword ="") {
        $.ajax({
            url: '/Khoa/GetDataKhoa',
            type: 'GET',
            data: {
                pageNumber: pageNumber,
                keyword: keyword,
                pageSize: 10
            },
            success: function (res) {
                var items = res.data;
                totalPages = res.totalPages;
                var html = "";

                if (items.length === 0) {
                    html += "<tr><td colspan='9' class='text-center'>Không có dữ liệu</td></tr>";
                } else {
                    for (let i = 0; i < items.length; i++) {
                        var formattedDate1 = unixTimestampToDate(items[i].NgayCapNhat);
                        var formattedDate2 = unixTimestampToDate(items[i].NgayTao);
                        var index = (pageNumber - 1) * 10 + i + 1;
                        html += "<tr>";
                        html += "<td>" + index + "</td>";
                        html += "<td>" + "#" + items[i].IDKhoa + "</td>";
                        html += "<td>" + items[i].MaKhoa + "</td>";
                        html += "<td>" + items[i].TenKhoa + "</td>";
                        html += "<td>" + formattedDate1 + "</td>";
                        html += "<td>" + formattedDate2 + "</td>";
                        html += "<td><button class='btn btn-icon btn-hover btn-sm btn-rounded pull-right' id='btnEdit' data-toggle='modal' data-target='#ModalEditKhoa'><i class='anticon anticon-edit'></i></button><button class='btn btn-icon btn-hover btn-sm btn-rounded pull-right' id='btnDelete'><i class='anticon anticon-delete'></i></button></td>";
                        html += "</tr>";
                    }
                }
                $('#showdata').html(html);
                renderPagination(pageNumber, totalPages);
            },
            error: function () {
                var html = "<tr><td colspan='7' class='text-center'>Không thể tải dữ liệu</td></tr>";
                $('#showdata').html(html);
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
    }
    function renderPagination(currentPage, totalPages) {
        var html = '<nav aria-label="Page navigation example"><ul class="pagination justify-content-end">';

        var startPage = currentPage - 2;
        var endPage = currentPage + 2;

        if (startPage < 1) {
            startPage = 1;
            endPage = 5;
        }

        if (endPage > totalPages) {
            endPage = totalPages;
            startPage = totalPages - 4;
        }

        if (startPage < 1) {
            startPage = 1;
        }

        html += '<li class="page-item ' + (currentPage == 1 ? 'disabled' : '') + '"><a class="page-link" href="#" data-page="' + (currentPage - 1) + '">Trước</a></li>';

        for (var i = startPage; i <= endPage; i++) {
            html += '<li class="page-item ' + (currentPage == i ? 'active' : '') + '"><a class="page-link" href="#" data-page="' + i + '">' + i + '</a></li>';
        }

        html += '<li class="page-item ' + (currentPage == totalPages ? 'disabled' : '') + '"><a class="page-link" href="#" data-page="' + (currentPage + 1) + '">Tiếp</a></li>';
        html += '</ul></nav>';

        $('#pagination').html(html);
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
                LoadData(currentPage);
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
                LoadData(currentPage);
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
                LoadData(currentPage);
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