$(document).ready(function () {
    const title = $("#title");
    title.show();
    $("#btnXemBaoCao").click(function () {
        load_chi_tiet_cau_tra_loi();
    })
})

async function load_chi_tiet_cau_tra_loi() {
    const value = $("#value").text();
    const res = await $.ajax({
        url: '/api/admin/danh-sach-cau-tra-loi-phieu',
        type: 'POST',
        data: {
            surveyID: value
        }
    });
    const body = $("#bodycontent");
    const title = $("#title");
    body.empty();
    let html = ``;

    if (res.success) {
        res.data.forEach(function (items) {
            if (res.is_subject) {
                html += form_load_chi_tiet_subject(items);
            }
            else if (res.is_student) {
                html += form_load_chi_tiet_student(items);
            }
            else if (res.is_program) {
                html += form_load_chi_tiet_program(items);
            }
        })
        
        title.hide();
        body.show();
        body.html(html);
        initializeDataTable(); 
    } else {
        html = `<tr><td colspan="10" class="text-center">Không có dữ liệu để hiển thị</td></tr>`;
        body.html(html);
    }
}

function form_load_chi_tiet_subject(data) {
    let html = `
        <thead>
            <tr>
                <th scope="col">STT</th>
                <th scope="col">Mã KQ</th>
                <th scope="col">Email khảo sát</th>
                <th scope="col">Môn học</th>
                <th scope="col">Giảng viên giảng dạy</th>
                <th scope="col">Người học</th>
                <th scope="col">MSNH</th>
                <th scope="col">Thuộc CTĐT</th>
                <th scope="col">Thời gian thực hiện khảo sát</th>
                <th scope="col">Chi tiết câu trả lời</th>
            </tr>
        </thead>
        <tbody>
    `;
    data.forEach(function (item, index) {
        html += `
            <tr>
                <td class="formatSo">${index + 1}</td>
                <td class="formatSo">${item.ma_kq}</td>
                <td>${item.email}</td>
                <td>${item.mon_hoc}</td>
                <td>${item.giang_vien}</td>
                <td>${item.sinh_vien}</td>
                <td class="formatSo">${item.msnh}</td>
                <td>${item.ctdt}</td>
                <td class="formatSo">${unixTimestampToDate(item.thoi_gian_thuc_hien)}</td>
                <td>
                    <button class="btn btn-info btnChiTiet" data-id="${item.ma_kq}">Chi tiết</button>
                </td>
            </tr>
        `;
    });

    html += `
        </tbody>
    `;
    return html;
}

function form_load_chi_tiet_student(data) {
    let html = `
        <thead>
            <tr>
                <th scope="col">STT</th>
                <th scope="col">Mã KQ</th>
                <th scope="col">Email khảo sát</th>
                <th scope="col">Người học</th>
                <th scope="col">MSNH</th>
                <th scope="col">Thuộc CTĐT</th>
                <th scope="col">Thời gian thực hiện khảo sát</th>
                <th scope="col">Chi tiết câu trả lời</th>
            </tr>
        </thead>
        <tbody>
    `;
    data.forEach(function (item, index) {
        html += `
            <tr>
                <td class="formatSo">${index + 1}</td>
                <td class="formatSo">${item.ma_kq}</td>
                <td>${item.email}</td>
                <td>${item.sinh_vien}</td>
                <td class="formatSo">${item.msnh}</td>
                <td>${item.ctdt}</td>
                <td class="formatSo">${unixTimestampToDate(item.thoi_gian_thuc_hien)}</td>
                <td>
                    <button class="btn btn-info btnChiTiet" data-id="${item.ma_kq}">Chi tiết</button>
                </td>
            </tr>
        `;
    });

    html += `
        </tbody>
    `;
    return html;
}

function form_load_chi_tiet_program(data) {
    let html = `
        <thead>
            <tr>
                <th scope="col">STT</th>
                <th scope="col">Mã KQ</th>
                <th scope="col">Email khảo sát</th>
                <th scope="col">Thuộc Khoa</th>
                <th scope="col">Thuộc CTĐT</th>
                <th scope="col">Thời gian thực hiện khảo sát</th>
                <th scope="col">Chi tiết câu trả lời</th>
            </tr>
        </thead>
        <tbody>
    `;
    data.forEach(function (item, index) {
        html += `
            <tr>
                <td class="formatSo">${index + 1}</td>
                <td class="formatSo">${item.ma_kq}</td>
                <td>${item.email}</td>
                <td>${item.ten_khoa}</td>
                <td>${item.ten_ctdt}</td>
                <td class="formatSo">${unixTimestampToDate(item.thoi_gian_thuc_hien)}</td>
                <td>
                    <button class="btn btn-info btnChiTiet" data-id="${item.ma_kq}">Chi tiết</button>
                </td>
            </tr>
        `;
    });

    html += `
        </tbody>
    `;
    return html;
}
function initializeDataTable() {
    const table = $("#bodycontent");

    if ($.fn.DataTable.isDataTable(table)) {
        table.DataTable().destroy();
    }
    table.DataTable({
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
                title: 'Danh sách chi tiết thống kê - CSV'
            },
            {
                extend: 'excel',
                title: 'Danh sách chi tiết thống kê - Excel'
            },
            {
                extend: 'pdf',
                title: 'Danh sách chi tiết thống kê PDF'
            },
            {
                extend: 'print',
                title: 'Danh sách chi tiết thống kê'
            }
        ]
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
}
