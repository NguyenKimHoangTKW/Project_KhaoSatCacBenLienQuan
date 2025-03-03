$(".select2").select2();

$(document).on("click", "#btnFilter", function (event) {
    event.preventDefault();
    load_data();
});

$(document).on("click", "#btnEdit", function (event) {
    event.preventDefault();
    $("#bd-example-modal-lg").modal("show");
});

async function load_data() {
    const namhoc = $("#nam-hoc").val();
    const hocphan = $("#hoc-phan").val();
    const res = await $.ajax({
        url: '/api/admin/danh-sach-mon-hoc',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            hocphan: hocphan,
            namhoc: namhoc
        })
    });
    if ($.fn.DataTable.isDataTable('#mon-hoc-table')) {
        $('#mon-hoc-table').DataTable().clear().destroy();
    }
    const thead = $("#showthead");
    const tbody = $("#showdata");
    const title = $("#title_notification");
    thead.empty();
    tbody.empty();
    if (res.success) {
        title.hide();
        let htmlThead = `
        <tr>    
            <th>STT</th>
            <th>ID Môn học</th>
            <th>Mã môn học</th>
            <th>Tên môn học</th>
            <th>Thuộc học phần</th>
            <th>Thuộc lớp</th>
            <th>Ngày tạo</th>
            <th>Cập nhật lần cuối</th>
            <th>Năm hoạt động</th>
            <th>Chức năng</th>
        </tr>
    `;
        thead.html(htmlThead);
        let htmlTbody = "";
        res.data.forEach((item, index) => {
            htmlTbody += `
            <tr>
                <td class="formatSo">${index + 1}</td>
                <td class="formatSo">${item.value}</td>
                <td class="formatSo">${item.ma_mon_hoc}</td>
                <td>${item.ten_mon_hoc}</td>
                <td>${item.thuoc_hoc_phan}</td>
                <td class="formatSo">${item.thuoc_lop}</td>
                <td class="formatSo">${unixTimestampToDate(item.ngay_tao)}</td>
                <td class="formatSo">${unixTimestampToDate(item.ngay_cap_nhat)}</td>
                <td class="formatSo">${item.thuoc_nam}</td>
                <td>
                        <button class="btn btn-icon btn-hover btn-sm btn-rounded pull-right" id="btnEdit" data-id="${item.value}">
                            <i class="anticon anticon-edit"></i>
                        </button>
                        <button class="btn btn-icon btn-hover btn-sm btn-rounded pull-right" id="btnDelete" data-id="${item.value}">
                            <i class="anticon anticon-delete"></i>
                        </button>
                    </td>
            </tr>
        `;
        });
        tbody.html(htmlTbody);
        $('#mon-hoc-table').DataTable({
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
                    title: 'Danh sách đáp viên đã chọn khảo sát phiếu'
                },
                {
                    extend: 'print',
                    title: 'Danh sách đáp viên đã chọn khảo sát phiếu'
                }
            ]
        });
    }
    else {
        const Toast = Swal.mixin({
            toast: true,
            position: "top-end",
            showConfirmButton: false,
            timer: 3000,
            timerProgressBar: true,
            didOpen: (toast) => {
                toast.onmouseenter = Swal.stopTimer;
                toast.onmouseleave = Swal.resumeTimer;
            }
        });
        Toast.fire({
            icon: "error",
            title: res.message
        });
    }
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