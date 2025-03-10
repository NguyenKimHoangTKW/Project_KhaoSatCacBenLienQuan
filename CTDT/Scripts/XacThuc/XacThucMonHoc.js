$(document).ready(function () {
    load_mon_hoc();
});

async function load_mon_hoc() {
    const value = $("#hiddenId").val();
    const res = await $.ajax({
        url: '/api/load_danh_sach_mon_hoc',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            surveyID: value
        })
    });
    const body = $('#show-data');
    let html = "";
    if (res.success) {
        const data = JSON.parse(res.data);
        html += `<table class="table table-bordered table-hover table-striped">
                    <thead class="text-center">
                        <tr>
                            <th scope="col">STT</th>
                            <th scope="col">Giảng viên</th>
                            <th scope="col">Môn học</th>
                            <th scope="col">Tình trạng khảo sát</th>                            
                        </tr>
                    </thead>
                    <tbody id="showdata">`;
        data.forEach(function (items, index) {
            const style_check = items.tinh_trang_khao_sat == "Chưa khảo sát" ? "color:red" : "color:green";
            html += ` 
                    <tr data-items='${items.id_mon_hoc}' style="cursor: pointer;">
                        <td class="text-center">${index + 1}</td>
                        <td class="text-center">${items.ten_giang_vien}</td>
                        <td class="text-center">${items.mon_hoc}</td>
                        <td class="text-center" style="${style_check};font-weight:bold">${items.tinh_trang_khao_sat}</td>
                    </tr>`;
        });


        html += ` </tbody>
                </table>`;
    }
    else {
        html = `
                    <div class="container" id="showdata">
                        <div class="alert alert-info" style="text-align: center;">
                           ${res.message}
                        </div>
                    </div>`;
    }
    body.html(html);
}

$('#show-data').on('click', 'tr', async function () {
    const items = $(this).data('items');
    const survey = $("#hiddenId").val();
    const res = await $.ajax({
        url: '/api/save_xac_thuc',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            surveyID: survey,
            check_hoc_phan: items
        })
    });
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
            window.location.href = res.url;
        });
    }
    else if (res.is_answer) {
        Swal.fire({
            title: "Bạn đã thực hiện khảo sát cho môn học này!",
            text: "Bạn có muốn xem lại đáp án?",
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#3085d6",
            cancelButtonColor: "#d33",
            confirmButtonText: "Xem lại"
        }).then((result) => {
            if (result.isConfirmed) {
                window.location.href = res.url;
            }
        });
    }
});