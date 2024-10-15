
function Loading() {
    Swal.fire({
        title: 'Loading...',
        text: 'Đang thống kê dữ liệu, vui lòng chờ trong giây lát !',
        allowOutsideClick: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });
}
function EndLoading() {
    Swal.close();
}
$(document).ready(async function () {
    Loading()
    try {
        await LoadChartSurvey();
    }
    finally {
        EndLoading()
    }
    
});
$(document).on("change", "#Year", async function () {
    Loading()
    try {
        await LoadChartSurvey();
    }
    finally {
        EndLoading()
    }
});
async function LoadChartSurvey() {
    var year = $("#Year").val();
    const res = await $.ajax({
        url: '/api/ctdt/load_thong_ke_nguoi_hoc',
        type: 'POST',
        data: { ten_namhoc: year },
    });
    $('#survey-list').empty();
    if (res.data[0].AllSurvey.length > 0) {
        res.data[0].AllSurvey.sort((a, b) => {
            let idA = (typeof a.NameSurvey === 'string') ? a.NameSurvey.split(".")[0] : '';
            let idB = (typeof b.NameSurvey === 'string') ? b.NameSurvey.split(".")[0] : '';
            return idA.localeCompare(idB, undefined, { numeric: true });
        });

        res.data[0].AllSurvey.forEach(async function (survey) {
            const MaPhieu = survey.NameSurvey.split(".")[0].toUpperCase();
            const TieuDePhieu = survey.NameSurvey.split(".")[1];
            const surveyData = res.data[0].ChartSurvey.find(chil => chil.IDPhieu === survey.IDSurvey);
            const SurveyBySubject = survey.HocKy != null ? MaPhieu + " - " + (survey.HocKy ?? MaPhieu) : MaPhieu;
            const card = `
                <div class="card survey-card">
                    <div class="card-body">
                        <div style="align-items: center;">
                            <p style="color:#5029ff;font-weight:bold; position: absolute; top: 0; left: 20px;">${SurveyBySubject}</p>
                            <a href="" style="color:#5029ff;font-weight:bold; position: absolute; top: 14px; right: 20px;" data-toggle="modal" data-target=".bd-example-modal-lg" id="maphieu" data-tenphieu="${survey.NameSurvey}">Xem chi tiết</a>
                            <hr/>
                            <p style="color:black;font-weight:bold">${TieuDePhieu}</p>
                            <hr/>
                        </div>
                        <canvas class="chart" id="donut-chart-${survey.IDSurvey}"></canvas>
                        <p id="surveyedInfo-${survey.IDSurvey}" style="margin: 0; color: red;"></p>
                        <hr />
                        <div style="display: flex; justify-content: space-between; align-items: center; font-weight:bold">
                            <p style="margin: 0; color: black;">${surveyData ? "Tổng phiếu: " + surveyData.TongKhaoSat : ''}</p>
                            <p style="margin: 0; color:#ebb000;">${surveyData ? "Đã thu về: " + surveyData.TongPhieuDaTraLoi : ''}</p>
                            <p style="margin: 0; color:#5029ff;">${surveyData ? "Chưa thu về: " + surveyData.TongPhieuChuaTraLoi : ''}</p>
                        </div>
                    </div>
                </div>`;

            $('#survey-list').append(card);
            if (!surveyData) {
                $(`#surveyedInfo-${survey.IDSurvey}`).text('Không có dữ liệu');
            } else {
                const datas = [surveyData.TongPhieuChuaTraLoi, surveyData.TongPhieuDaTraLoi];
                const colors = ['#007bff', '#ffc107'];

                const donutCtx = document.getElementById(`donut-chart-${survey.IDSurvey}`).getContext('2d');
                const donutData = {
                    labels: ['Số phiếu chưa trả lời', 'Số phiếu đã thu'],
                    datasets: [{
                        fill: true,
                        backgroundColor: colors,
                        pointBackgroundColor: colors,
                        data: datas
                    }]
                };

                new Chart(donutCtx, {
                    type: 'doughnut',
                    data: donutData,
                    options: {
                        maintainAspectRatio: false,
                        hover: { mode: null },
                        cutoutPercentage: 45
                    }
                });
            }
        });

    } else {
        const card = `
            <div class="alert alert-info" style="width: 100%;">
                <div class="d-flex justify-content-start">
                    <span class="alert-icon m-r-20 font-size-30">
                        <i class="anticon anticon-close-circle"></i>
                    </span>
                    <div>
                        <h5 class="alert-heading">Opps...</h5>
                        <p>Không có dữ liệu phiếu khảo sát cho năm học này!</p>
                    </div>
                </div>
            </div>`;
        $('#survey-list').append(card);
        $('.chart').hide();
    }
}

$(document).on("click", "#maphieu", function () {
    var maphieu = $(this).data("tenphieu");
    $('#surveyModal').modal('show');
    load_nguoi_hoc(maphieu);
})
$('#surveyModal').on('shown.bs.modal', function () {
    $('#data-table-section').show();
});
async function load_nguoi_hoc(namesurvey) {
    const res = await $.ajax({
        url: '/api/ctdt/load_thong_ke_nguoi_hoc_khao_sat',
        type: 'POST',
        data: { surveyTitle: namesurvey }
    });

    if (res && res.data.length > 0) {
        var body = $("#load_data");
        var label = $("#exampleModalLabel");
        var html = "";
        body.empty();
        if ($.fn.DataTable.isDataTable('#load_data')) {
            $('#load_data').DataTable().clear().destroy();
        }

  
        res.data.forEach(function (items, index) {
            label.html(items.ten_phieu);
            if (items.is_nguoi_hoc || items.is_nguoi_hoc_mon_hoc) {
                html += "<thead>";
                html += "<tr>";
                html += "<th>Số Thứ Tự</th>";
                html += "<th>Họ và Tên</th>";
                html += "<th>Mã Người Học</th>";
                html += "<th>Lớp</th>";
                html += "<th>Tình Trạng Khảo Sát</th>";
                html += "</tr>";
                html += "</thead>";
                html += "<tbody>";
                items.nguoi_hoc.forEach(function (nguoiHoc, subIndex) {
                    html += "<tr>";
                    html += `<td>${subIndex + 1}</td>`;
                    html += `<td>${nguoiHoc.ho_ten}</td>`;
                    html += `<td>${nguoiHoc.ma_nguoi_hoc}</td>`;
                    html += `<td>${nguoiHoc.lop}</td>`;
                    html += `<td>${nguoiHoc.tinh_trang_khao_sat}</td>`;
                    html += "</tr>";
                });
                html += "</tbody>";
            }
            else if (items.is_ctdt) {
                html += "<thead>";
                html += "<tr>";
                html += "<th>Số Thứ Tự</th>";
                html += "<th>Họ và tên</th>";
                html += "<th>Email</th>";
                html += "<th>Chương trình đào tạo</th>";
                html += "</tr>";
                html += "</thead>";
                html += "<tbody>";
                items.ctdt.forEach(function (ctdt, index) {
                    html += "<tr>";
                    html += `<td>${index + 1}</td>`;
                    html += `<td>${ctdt.ho_ten}</td>`;
                    html += `<td>${ctdt.email}</td>`;
                    html += `<td>${ctdt.ctdt}</td>`;
                    html += "</tr>";
                });
                html += "</tbody>";
            }
            else if (items.is_cbvc) {
                html += "<thead>";
                html += "<tr>";
                html += "<th>Số Thứ Tự</th>";
                html += "<th>Họ và tên</th>";
                html += "<th>Email</th>";
                html += "<th>Đơn vị</th>";
                html += "<th>Chương trình đào tạo</th>";
                html += "</tr>";
                html += "</thead>";
                html += "<tbody>";
                items.cbvc.forEach(function (cbvc, index) {
                    html += "<tr>";
                    html += `<td>${index + 1}</td>`;
                    html += `<td>${cbvc.ho_ten}</td>`;
                    html += `<td>${cbvc.email}</td>`;
                    html += `<td>${cbvc.don_vi}</td>`;
                    html += `<td>${cbvc.ctdt}</td>`;
                    html += "</tr>";
                });
                html += "</tbody>";
            }
        });

        body.html(html);
        $('#load_data').DataTable({
            pageLength: 10,
            lengthMenu: [5, 10, 25, 50, 100],
            ordering: true,
            searching: true,
            language: {
                paginate: {
                    next: "Next",
                    previous: "Previous"
                },
                search: "Search",
                lengthMenu: "Show _MENU_ entries"
            },
            dom: "Bfrtip",
            buttons: ['csv', 'excel', 'pdf', 'print']
        });
        
    } else {
        $("#load_data").html("<tr><td colspan='5'>No data available</td></tr>");
    }
}

