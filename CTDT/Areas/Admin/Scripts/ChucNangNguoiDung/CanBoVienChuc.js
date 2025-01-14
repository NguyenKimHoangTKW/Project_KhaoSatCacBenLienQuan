
async function load_data() {
    const cbvc = $("#cbvcTable").val();
    const res = await $.ajax({
        url: '/api/admin/danh-sach-cbvc',
        type: 'POST',
        data: {
            id_lop: lop
        }
    });
    const body = $("#cbvcTable");
    let html = "";
    if ($.fn.DataTable.isDataTable('#cbvcTable')) {
        $('#cbvcTable').DataTable().clear().destroy();
    }
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
                    <td class="formatSo">${index + 1}</td>
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
    } else {
        html =
            `
            <tr>
                <td colspan="14" class="text-center text-danger">${res.message || 'Không có dữ liệu'}</td>
            </tr>
            `;
        body.find("tbody").html(html);
    }
    $('#cbvcTable').DataTable({
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
                title: 'Danh sách Lớp'
            },
            {
                extend: 'print',
                title: 'Danh sách Lớp'
            }
        ]
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
    var formattedDate = dayOfWeek + ', ' + day + "-" + month + "-" + year + " " + hours + ":" + minutes + ":" + seconds;
    return formattedDate;
}