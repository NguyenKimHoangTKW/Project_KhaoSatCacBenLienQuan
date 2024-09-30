var currentPage = 1;
var totalPages = 0;
$(document).ready(function () {
    LoadData(currentPage);
});

$(document).on("click", ".btnEdit", function () {
    var id = $(this).data("id");
    GetByID(id);
});
function AddNew() {
    var CheckData = {
        name_loaikhaosat: $("#tenDTKS").val(),
    };
    $.ajax({
        url: "/Admin/DoiTuongKhaoSat/AddNew",
        type: "POST",
        dataType: "JSON",
        contentType: "application/json; charset=utf-8",
        data: JSON.stringify(CheckData),
        success: function (res) {
            if (res.success) {
                Swal.fire({
                    text: res.message,
                    icon: 'success',
                    showConfirmButton: false,
                    timer: 1500
                }).then(function () {
                    $("#ModalAddNew").modal("hide");
                    LoadData(currentPage);
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

function GetByID(id) {
    $.ajax({
        url: "/Admin/DoiTuongKhaoSat/GetbyID",
        type: "GET",
        data: { id: id },
        success: function (res) {
            if (res.success) {
                $("#EditTenDTKS").val(res.data.TenLKS);
                $("#Edit_id").val(res.data.IDLKS);
            }
            else {
                Swal.fire({
                    icon: "error",
                    title: "Oops...",
                    text: "Load dữ liệu thất bại",
                    confirmButtonText: 'OK'
                });
            }
        }
    });
}
function LoadData(pageNumber, keyword = "") {
    $.ajax({
        url: "/Admin/DoiTuongKhaoSat/LoadData",
        type: "GET",
        data: {
            pageNumber: pageNumber,
            pageSize: 10,
            keyword: keyword
        },
        success: function (res) {
            let html = "";
            let items = res.data;
            totalPages = res.totalPages;
            if (items.length === 0) {
                html = `
                <tr>
                <td colspan="6" class="text-center">Không có dữ liệu</td>
                </tr>
                `;
                $("#showdata").html(html);
            }
            else {
                items.forEach(function (Chil, i) {
                    var index = (pageNumber - 1) * 10 + i + 1;
                    var formatNgayTao = unixTimestampToDate(Chil.NgayTao);
                    var formatCapNhat = unixTimestampToDate(Chil.NgayCapNhat);
                    html = `
                    <tr>
                        <td>${index}</td>
                        <td>${Chil.IDLKS}</td>
                        <td>${Chil.TenLKS}</td>
                        <td>${formatNgayTao}</td>
                        <td>${formatCapNhat}</td>
                        <td>
                            <button class="btn btn-icon btn-hover btn-sm btn-rounded pull-right btnEdit" data-toggle="modal" data-target="#ModalAddNew" data-id="${Chil.IDLKS}">
                                <i class="anticon anticon-edit"></i>
                            </button>
                            <button class="btn btn-icon btn-hover btn-sm btn-rounded pull-right btnDelete" data-id="${Chil.IDLKS}">
                                <i class="anticon anticon-delete"></i>
                            </button>
                        </td>
                    </tr>
                    `;
                    $("#showdata").append(html);
                    renderPagination(pageNumber, totalPages);
                });
            }
        }
    });
}

$(document).on("click", ".page-link", function () {
    var page = $(this).data("page");
    if (page > 0 && page <= totalPages) {
        currentPage = page;
        var keyword = $("#searchInput").val().toLowerCase();
        LoadData(currentPage, keyword);
    }
});


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