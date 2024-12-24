$(document).ready(function () {
    $("#hedaotao, #year").on("change", load_pks_by_nam);

    $("#fildata").click(function () {
        LoadChartSurvey()
    });
});

async function load_pks_by_nam() {
    const hedaotao = $("#hedaotao").val();
    const year = $("#year").val();
    const res = await $.ajax({
        url: '/api/load_phieu_by_nam',
        type: 'POST',
        data: {
            id_namhoc: year,
            id_hedaotao: hedaotao
        }
    });
    let html = "";
    let html_ctdt = `<option value="">Tất cả</option>`;

    if (res.success) {

        res.ctdt.forEach(function (ctdt) {
            html_ctdt += `<option value="${ctdt.id_ctdt}">${ctdt.ten_ctdt}</option>`;
        });
        $("#ctdt").empty().html(html_ctdt);
    } else {
        html += `<option value="">${res.message}</option>`;
        $("#surveyid").empty().html(html);
        $("#ctdt").empty().html(html);
    }
}

async function LoadChartSurvey() {
    const hedaotao = $("#hedaotao").val();
    const year = $("#year").val();
    const ctdt = $("#ctdt").val();
    const res = await $.ajax({
        url: '/api/admin/giam-sat-ty-le-tham-gia-khao-sat',
        type: 'POST',
        data: {
            id_namhoc: year,
            id_ctdt: ctdt,
            id_hdt: hedaotao
        },
    });

    $('#survey-list').empty();

    if (res.success) {
        const surveys = res.data;
        surveys.sort((a, b) => {
            const idA = a.ten_phieu.split(".")[0].replace(/\D/g, "");
            const idB = b.ten_phieu.split(".")[0].replace(/\D/g, "");
            return parseInt(idA) - parseInt(idB);
        });

        surveys.forEach((survey) => {
            const tenPhieuParts = survey.ten_phieu.split(".");
            const MaPhieu = tenPhieuParts[0].toUpperCase();
            const TieuDePhieu = tenPhieuParts[1]?.trim() || "Không có tiêu đề";

            const thongKeTyLe = {
                tong_khao_sat: survey.tong_khao_sat,
                tong_phieu_da_tra_loi: survey.tong_phieu_da_tra_loi,
                tong_phieu_chua_tra_loi: survey.tong_phieu_chua_tra_loi,
            };

            const card = `
                <div class="card survey-card">
                    <div class="card-body">
                        <div style="align-items: center;">
                            <p style="color:#5029ff;font-weight:bold; position: absolute; top: 0; left: 20px;">${MaPhieu}</p>
                            <a href="#" style="color:#5029ff;font-weight:bold; position: absolute; top: 14px; right: 20px;" data-toggle="modal" data-target=".bd-example-modal-lg" id="maphieu" data-tenphieu="${survey.id_phieu}">Xem chi tiết</a>
                            <hr/>
                            <p style="color:black;font-weight:bold">${TieuDePhieu}</p>
                            <hr/>
                        </div>
                        <canvas class="chart" id="donut-chart-${MaPhieu}"></canvas>
                        <p id="surveyedInfo-${MaPhieu}" style="margin: 0; color: red;"></p>
                        <hr />
                        <div style="display: flex; justify-content: space-between; align-items: center; font-weight:bold">
                            <p style="margin: 0; color: black;">Tổng phiếu: ${thongKeTyLe.tong_khao_sat || '0'}</p>
                            <p style="margin: 0; color:#5029ff;">Đã thu về: ${thongKeTyLe.tong_phieu_da_tra_loi || '0'}</p>
                            <p style="margin: 0; color:#ebb000;">Chưa thu về: ${thongKeTyLe.tong_phieu_chua_tra_loi || '0'}</p>
                        </div>
                    </div>
                </div>`;

            $('#survey-list').append(card);

            const donutCtx = document.getElementById(`donut-chart-${MaPhieu}`).getContext('2d');

            if (thongKeTyLe.tong_khao_sat > 0) {
                const datas = [thongKeTyLe.tong_phieu_chua_tra_loi, thongKeTyLe.tong_phieu_da_tra_loi];
                const colors = ['#ffc107', '#007bff'];

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
            } else {
                const donutData = {
                    labels: ['Không có dữ liệu'],
                    datasets: [{
                        fill: true,
                        backgroundColor: ['#d3d3d3'],
                        pointBackgroundColor: ['#d3d3d3'],
                        data: [1]
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

                $(`#surveyedInfo-${MaPhieu}`).text('Không có dữ liệu');
            }
        });
    } else {
        const card = `
            <div class="alert alert-info" style="width: 100%; text-align: center;">
                <h5 class="alert-heading">Opps...</h5>
                <p>Không có dữ liệu phiếu khảo sát cho năm học này!</p>
            </div>`;
        $('#survey-list').append(card);
    }
}