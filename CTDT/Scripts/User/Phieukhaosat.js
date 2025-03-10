﻿$(document).ready(function () {
    LoadData();
});

async function LoadData() {
    var namehdt = $('#namehdt').val();
    const res = await $.ajax({
        url: '/api/bo_phieu_khao_sat',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ ten_hedaotao: namehdt })
    })
    let body = $('#showdata');
    let html = "";
    if (res.success) {
        let items = JSON.parse(res.data);
        items.sort((a, b) => {
            var MaPhieuA = a.TenPKS.split('.')[0];
            var MaPhieuB = b.TenPKS.split('.')[0];
            return MaPhieuA.localeCompare(MaPhieuB, undefined, { numeric: true, sensitivity: 'base' });
        });

        items.forEach(item => {
            var maxChars = 150;
            var truncatedText = item.MoTaPhieu.length > maxChars ? item.MoTaPhieu.substring(0, maxChars) + '...' : item.MoTaPhieu;
            var MaPhieu = item.TenPKS.split('.')[0];
            var TenPhieu = item.TenPKS.split('.')[1];

            html += `
                        <div class="col-md-4 d-flex">
                            <div class="blog-entry align-self-stretch">
                                <a href="javascript:void(0)" class="block-20 btnCheck" data-id="${item.MaPhieu}" style="background-image: url('/Style/assets/survey.png');"></a>
                                <div class="text p-4 d-block">
                                    <div class="meta mb-3">
                                        <div><a href="javascript:void(0)" class="btnCheck" data-id="${item.MaPhieu}">${MaPhieu}</a></div>
                                    </div>
                                    <h3 class="heading mt-3"><a href="javascript:void(0)" class="btnCheck" data-id="${item.MaPhieu}">${TenPhieu}</a></h3>
                                    <p>${truncatedText}</p>
                                    <p class="d-flex justify-content-end">
                                        <a href="javascript:void(0)" class="btn btn-primary btnCheck" data-id="${item.MaPhieu}">Khảo sát</a>
                                    </p>
                                </div>
                            </div>
                        </div>`;
        });
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
function showLoading() {
    Swal.fire({
        title: 'Loading...',
        text: 'Đang kiểm tra và tải biểu mẫu, vui lòng chờ trong giây lát!',
        allowOutsideClick: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });
}
function hideLoading() {
    Swal.close();
}
$(document).on('click', '.btnCheck', async function () {
    var id = $(this).data("id");
    showLoading()
    try {
        check_xac_thuc(id);
    }
    finally {
        hideLoading();
    }

});

async function check_xac_thuc(id) {
    const res = await $.ajax({
        url: '/api/check_xac_thuc',
        type: 'POST',
        data: { surveyID: id },
    });
    let url = res.data;
    if (res.is_answer) {
        Swal.fire({
            title: "Bạn đã khảo sát phiếu khảo sát này!",
            text: "Bạn có muốn xem lại đáp án?",
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#3085d6",
            cancelButtonColor: "#d33",
            confirmButtonText: "Xem lại"
        }).then((result) => {
            if (result.isConfirmed) {
                location.href = url;
            }
        });
    } else if (res.non_survey) {
        location.href = url;
    } else {
        Swal.fire({
            title: "Thông báo",
            text: res.message,
            icon: "warning"
        });
    }
}

