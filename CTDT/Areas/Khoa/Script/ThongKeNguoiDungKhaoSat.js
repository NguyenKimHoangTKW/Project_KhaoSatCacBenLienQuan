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
$(document).on("change", "#Year,#fieldCTDT", async function () {
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
    var ctdt = $("#fieldCTDT").val();
    const res = await $.ajax({
        url: '/api/khoa/load_thong_ke_nguoi_hoc',
        type: 'POST',
        data: {
            id_nam_hoc: year,
            id_ctdt: ctdt
        },
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