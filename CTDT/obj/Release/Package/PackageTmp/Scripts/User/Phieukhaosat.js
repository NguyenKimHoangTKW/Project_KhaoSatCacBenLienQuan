$(document).ready(function () {
    LoadData();
});

function LoadData() {
    var id = $('#id').val();
    $.ajax({
        url: '/Home/load_phieu_khao_sat',
        type: 'POST',
        data: { id: id },
        success: function (res) {
            let items = res.data.survey;
            let body = $('#showdata');
            let html = "";

            if (items.length === 0) {
                html = `
                    <div class="container" id="showdata">
                        <div class="alert alert-info" style="text-align: center;">
                            Không có dữ liệu phiếu khảo sát
                        </div>
                    </div>`;
            } else {
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

            body.html(html);
        }
    });
}

$(document).on('click', '.btnCheck', function () {
    var id = $(this).data("id");
    check_xac_thuc(id);
});

function check_xac_thuc(id) {
    $.ajax({
        url: '/Home/check_xac_thuc',
        type: 'POST',
        data: { id: id },
        success: function (res) {
            let url = res.data;
            if (res.is_answer) {
                Swal.fire({
                    title: "Bạn đã khảo sát phiếu khảo sát này !",
                    text: "Bạn có muốn xem lại đáp án?",
                    icon: "warning",
                    showCancelButton: true,
                    confirmButtonColor: "#3085d6",
                    cancelButtonColor: "#d33",
                    confirmButtonText: "Xem lại"
                }).then((result) => {
                    if (result.isConfirmed) {
                        window.location.href = url;
                    }
                });
            }
            else if (res.non_survey) {
                Swal.fire({
                    title: "Loading...",
                    text: "Đang load dữ liệu phiếu khảo sát, vui lòng chờ!",
                    icon: "info",
                    allowOutsideClick: false,
                    showConfirmButton: false,
                    willOpen: () => {
                        Swal.showLoading();
                    },
                    timer: 3000,
                }).then(() => {
                    window.location.href = url;
                });
            }
            else if (res.is_clipboard) {
                window.location.href = url;
            }
            else {
                window.location.href = url;
            }
        }
    });
}
