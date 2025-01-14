$(document).ready(function () {
    load_data();
});
// Sự kiện khi nhấn nút phân trang
$(document).on("click", ".page-link", function (e) {
    e.preventDefault();

    const page = $(this).data("page");
    if (page) {
        load_data(page);
    }
});

let currentPage = 1;
const pageSize = 7;

async function load_data(page = 1) {
    const lop = $("#FilterLop").val();
    const res = await $.ajax({
        url: '/api/admin/danh-sach-nguoi-hoc',
        type: 'POST',
        data: {
            id_lop: lop || 0,
            page: page,
            pageSize: pageSize
        }
    });

    const body = $("#nguoihocTable");
    let html = "";

    if (res.success) {
        let thead =
            `
            <tr>
                <th scope="col">STT</th>
                <th scope="col">ID Người học</th>
                <th scope="col">Mã số người học</th>
                <th scope="col">Tên người học</th>
                <th scope="col">Thuộc lớp</th>
                <th scope="col">Ngày sinh</th>
                <th scope="col">Số điện thoại</th>
                <th scope="col">Địa chỉ</th>
                <th scope="col">Giới tính</th>
                <th scope="col">Năm nhập học</th>
                <th scope="col">Năm tốt nghiệp</th>
                <th scope="col">Mô tả</th>
                <th scope="col">Ngày Tạo</th>
                <th scope="col">Cập nhật lần cuối</th>
                <th scope="col">Chức năng</th>
            </tr>
            `;
        body.find("thead").html(thead);
        res.data.forEach((item, index) => {
            html +=
                `
                <tr>
                     <td class="formatSo">${(page - 1) * pageSize + index + 1}</td>
                    <td class="formatSo">${item.id_sv}</td>
                    <td class="formatSo">${item.ma_sv}</td>
                    <td>${item.hovaten}</td>
                    <td class="formatSo">${item.ma_lop}</td>
                    <td class="formatSo">${item.ngaysinh}</td>
                    <td class="formatSo">${item.sodienthoai}</td>
                    <td>${item.diachi}</td>
                    <td>${item.phai}</td>
                    <td class="formatSo">${item.namnhaphoc}</td>
                    <td class="formatSo">${item.namtotnghiep}</td>
                    <td>${item.description}</td>
                    <td class="formatSo">${unixTimestampToDate(item.ngaytao)}</td>
                    <td class="formatSo">${unixTimestampToDate(item.ngaycapnhat)}</td>
                    <td>
                        <button class="btn btn-primary btn-sm" id="btnEdit" data-id="${item.id_sv}">Sửa</button>
                        <button class="btn btn-danger btn-sm" id="btnDelete" data-id="${item.id_sv}">Xóa</button>
                    </td>
                </tr>
                `;
        });
        body.find("tbody").html(html);
        renderPagination(res.totalPages, res.currentPage);
    } else {
        html = `
            <tr>
                <td colspan="14" class="text-center text-danger">${res.message || 'Không có dữ liệu'}</td>
            </tr>
        `;
        body.html(html);
        $("#paginationControls").html("");
    }
}

function renderPagination(totalPages, currentPage) {
    const paginationContainer = $("#paginationControls");
    let html = "";

    // Nút Previous
    html += `
        <li class="page-item ${currentPage === 1 ? "disabled" : ""}">
            <a class="page-link" href="#" data-page="${currentPage - 1}">Previous</a>
        </li>
    `;

    // Hiển thị trang đầu tiên
    html += `
        <li class="page-item ${currentPage === 1 ? "active" : ""}">
            <a class="page-link" href="#" data-page="1">1</a>
        </li>
    `;

    // Thêm dấu "..." nếu cần
    if (currentPage > 4) {
        html += `
            <li class="page-item disabled">
                <a class="page-link">...</a>
            </li>
        `;
    }

    const maxPagesToShow = 3; // Số trang hiển thị trước và sau trang hiện tại
    const startPage = Math.max(2, currentPage - Math.floor(maxPagesToShow / 2));
    const endPage = Math.min(totalPages - 1, currentPage + Math.floor(maxPagesToShow / 2));

    for (let i = startPage; i <= endPage; i++) {
        html += `
            <li class="page-item ${i === currentPage ? "active" : ""}">
                <a class="page-link" href="#" data-page="${i}">${i}</a>
            </li>
        `;
    }

    // Thêm dấu "..." nếu cần
    if (currentPage < totalPages - 3) {
        html += `
            <li class="page-item disabled">
                <a class="page-link">...</a>
            </li>
        `;
    }

    // Hiển thị trang cuối cùng
    if (totalPages > 1) {
        html += `
            <li class="page-item ${currentPage === totalPages ? "active" : ""}">
                <a class="page-link" href="#" data-page="${totalPages}">${totalPages}</a>
            </li>
        `;
    }

    // Nút Next
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
    var formattedDate = dayOfWeek + ', ' + day + "-" + month + "-" + year + " " + ', ' + hours + ":" + minutes + ":" + seconds;
    return formattedDate;
};