$(".select2").select2();
let value_check = "";
let currentPage = 1;
$(document).ready(function () {
    load_data();
    $("#Modal-update").on("hidden.bs.modal", function () {
        $(this).find("input, textarea, select").val("");
        $(this).find("input[type=checkbox], input[type=radio]").prop("checked", false);
    });
});
$(document).on("click", ".page-link", function (e) {
    e.preventDefault();
    const page = $(this).data("page");
    if (page) {
        currentPage = page;
        load_data(currentPage);
    }
});
$(document).on("change", "#pageSizeSelect", function () {
    const pageSize = $(this).val();
    load_data(currentPage, pageSize);
});
$(document).on("click", "#btnFilter", function (event) {
    event.preventDefault();
    load_data(currentPage);
});
$(document).on("click", "#btnAdd", function (event) {
    event.preventDefault();
    $("#modal-title-update").text("Thêm mới cán bộ viên chức/giảng viên");
    const footer = $("#modal-footer-update");
    footer.empty();
    let html =
        `
        <button type="button" class="btn btn-danger btn-tone m-r-5" data-dismiss="modal">Thoát</button>
        <button type="button" class="btn btn-success btn-tone m-r-5" id="btnSaveAdd">Lưu</button>
        `;
    footer.html(html);
    $("#Modal-update").modal("show");
});
async function load_data(page = 1, pageSize = $("#pageSizeSelect").val()) {
    const ctdt = $("#FilterCTDT").val();
    const donvi = $("#FiterDonvi").val();
    const chucvu = $("#FilterChucvu").val();
    const nam = $("#FilterNam").val();
    const searchTerm = $("#searchInput").val();
    const res = await $.ajax({
        url: '/api/admin/danh-sach-cbvc',
        type: 'POST',
        data: {
            id_chuongtrinhdaotao: ctdt,
            id_namhoc: nam,
            id_donvi: donvi,
            id_chucvu: chucvu,
            page: page,
            pageSize: pageSize,
            searchTerm: searchTerm
        }
    });
    const body = $("#cbvcTable");
    let html = "";
    if (res.success) {
        let thead =
            `
            <tr>
                <th scope="col">STT</th>
                <th scope="col">ID CBVC</th>
                <th scope="col">Mã CBVC</th>
                <th scope="col">Tên CBVC</th>
                <th scope="col">Ngày sinh</th>
                <th scope="col">Email</th>
                <th scope="col">Thuộc đơn vị</th>
                <th scope="col">Chức vụ</th>
                <th scope="col">Thuộc chương trình đào tạo</th>
                <th scope="col">Năm hoạt động</th>
                <th scope="col">Mô tả</th>
                <th scope="col">Chức năng</th>
            </tr>
            `;
        body.find("thead").html(thead);
        res.data.forEach((item, index) => {
            html +=
                `
                <tr>
                    <td class="formatSo">${(page - 1) * pageSize + index + 1}</td>  
                    <td class="formatSo">${item.id_CBVC}</td>
                    <td class="formatSo">${item.MaCBVC}</td>
                    <td>${item.TenCBVC}</td>
                    <td class="formatSo">${item.NgaySinh}</td>
                    <td>${item.Email}</td>
                    <td>${item.donvi}</td>
                    <td>${item.chucvu}</td>
                    <td>${item.ctdt}</td>
                    <td class="formatSo">${item.NamHoc}</td>
                    <td>${item.descripton}</td>
                    <td>
                        <button class="btn btn-primary btn-sm" id="btnEdit" data-id="${item.id_CBVC}">Sửa</button>
                        <button class="btn btn-danger btn-sm" id="btnDelete" data-id="${item.id_CBVC}">Xóa</button>
                    </td>
                </tr>
                `;
        });
        body.find("tbody").html(html);
        renderPagination(res.totalPages, res.currentPage);
    } else {
        html =
            `
            <tr>
                <td colspan="14" class="text-center text-danger">${res.message || 'Không có dữ liệu'}</td>
            </tr>
            `;
        body.find("tbody").html(html);
    }
};
function renderPagination(totalPages, currentPage) {
    const paginationContainer = $("#paginationControls");
    let html = "";
    html += `
        <li class="page-item ${currentPage === 1 ? "disabled" : ""}">
            <a class="page-link" href="#" data-page="${currentPage - 1}">Previous</a>
        </li>
    `;

    html += `
        <li class="page-item ${currentPage === 1 ? "active" : ""}">
            <a class="page-link" href="#" data-page="1">1</a>
        </li>
    `;

    if (currentPage > 4) {
        html += `
            <li class="page-item disabled">
                <a class="page-link">...</a>
            </li>
        `;
    }
    const maxPagesToShow = 3;
    const startPage = Math.max(2, currentPage - Math.floor(maxPagesToShow / 2));
    const endPage = Math.min(totalPages - 1, currentPage + Math.floor(maxPagesToShow / 2));
    for (let i = startPage; i <= endPage; i++) {
        html += `
            <li class="page-item ${i === currentPage ? "active" : ""}">
                <a class="page-link" href="#" data-page="${i}">${i}</a>
            </li>
        `;
    }
    if (currentPage < totalPages - 3) {
        html += `
            <li class="page-item disabled">
                <a class="page-link">...</a>
            </li>
        `;
    }
    if (totalPages > 1) {
        html += `
            <li class="page-item ${currentPage === totalPages ? "active" : ""}">
                <a class="page-link" href="#" data-page="${totalPages}">${totalPages}</a>
            </li>
        `;
    }
    html += `
        <li class="page-item ${currentPage === totalPages ? "disabled" : ""}">
            <a class="page-link" href="#" data-page="${currentPage + 1}">Next</a>
        </li>
    `;
    paginationContainer.html(html);
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