
load_mon_hoc();
async function load_mon_hoc() {
    const res = await $.ajax({
        url: '/api/load_danh_sach_mon_hoc',
        type: 'GET'
    });
    const body = $('#show-data');
    let html = "";
    if (res.success) {
        html += `<table class="table table-bordered table-hover table-striped">
                    <thead class="text-center">
                        <tr>
                            <th scope="col">STT</th>
                            <th scope="col">Giảng viên</th>
                            <th scope="col">Môn học</th>
                            <th scope="col">Thời gian học</th>
                            <th scope="col">Tình trạng khảo sát</th>                            
                        </tr>
                    </thead>
                    <tbody id="showdata">`;
        res.data.forEach(function (items, index) {
            const style_check = items.tinh_trang_khao_sat == "Chưa khảo sát" ? "color:red" : "color:green";
                html += ` 
                    <tr data-items='${JSON.stringify(items)}' style="cursor: pointer;">
                        <td class="text-center">${index + 1}</td>
                        <td class="text-center">${items.ten_giang_vien}</td>
                        <td class="text-center">${items.mon_hoc}</td>
                        <td class="text-center">${items.thoi_gian_hoc}</td>
                        <td class="text-center" style="${style_check};font-weight:bold">${items.tinh_trang_khao_sat}</td>
                    </tr>`;
        });


        html += ` </tbody>
                </table>`;
        body.html(html);
    }
}
$('#show-data').on('click', 'tr', function () {
    const items = $(this).data('items');
    var maphieu = items.ma_phieu;
    var mamonhoc = items.id_mon_hoc;
    var get_data = {
        Id: maphieu ,
        id_nguoi_hoc_by_mon_hoc: mamonhoc
    };
    localStorage.setItem("xacthucstorage", JSON.stringify(get_data));
    $.ajax({
        url: '/api/save_xac_thuc',
        type: 'POST',
        data: JSON.stringify(get_data),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (res) {
            if (res.success) {
                Swal.fire({
                    title: "Xác thực thành công",
                    text: "Đang tải dữ liệu phiếu khảo sát, vui lòng chờ!",
                    icon: "success",
                    allowOutsideClick: false,
                    showConfirmButton: false,
                    willOpen: () => {
                        Swal.showLoading();
                    },
                    timer: 3000,
                }).then(() => {
                    window.location.href = '/phieu-khao-sat/' + maphieu;
                });
            }
            else if (res.is_answer) {
                Swal.fire({
                    title: res.message,
                    text: "Bạn có muốn xem lại đáp án?",
                    icon: "warning",
                    showCancelButton: true,
                    confirmButtonColor: "#3085d6",
                    cancelButtonColor: "#d33",
                    confirmButtonText: "Xem lại"
                }).then((result) => {
                    if (result.isConfirmed) {
                        window.location.href = res.info;
                    }
                });
            }
        }
    });
});

