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
    Loading();
    try {
        await LoadChartFullSurvey();
        await LoadChartSurveyThongTu01();
    } finally {
        EndLoading();
    }
});

$(document).on("change", "#yearGiamSat", async function () {
    Loading();
    try {
        await LoadChartFullSurvey();
        await LoadChartSurveyThongTu01();
    } finally {
        EndLoading();
    }
});
async function LoadChartFullSurvey() {
    var year = $("#yearGiamSat").val();
    const response = await fetch('/api/giam_sat_ty_le_khao_sat', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({ id_nam_hoc: year }),
    });

    const res = await response.json();

    const barChartCanvas = document.getElementById('bar-chart').getContext('2d');
    const lineChartCanvas = document.getElementById('line-chart').getContext('2d');
    const surveys = res.data;
    if (res.is_data) {
        surveys.sort((a, b) => {
            const idA = a.ten_phieu.split(".")[0];
            const idB = b.ten_phieu.split(".")[0];
            return idA.localeCompare(idB, undefined, { numeric: true });
        });
        let labels = [];
        let participationData = [];
        let satisfactionData = [];
        surveys.forEach((survey) => {
            survey.thong_ke_ty_le.forEach((tyleKhaoSat, index) => {
                let label = `${survey.ten_phieu.split('.')[0]}`;
                if (tyleKhaoSat.hoc_ky) {
                    label += ` - ${tyleKhaoSat.hoc_ky}`;
                }
                labels.push(label);

                let ty_le_tham_gia = tyleKhaoSat.ty_le_tham_gia_khao_sat
                    ? tyleKhaoSat.ty_le_tham_gia_khao_sat.ty_le_da_tra_loi
                    : 0;
                participationData.push(ty_le_tham_gia);

                let avg_ty_le_hai_long = tyleKhaoSat.ty_le_hai_long.length > 0
                    ? tyleKhaoSat.ty_le_hai_long[0].avg_ty_le_hai_long
                    : 0;
                satisfactionData.push(avg_ty_le_hai_long);
            });
        });

        if (window.barChart) window.barChart.destroy();
        if (window.lineChart) window.lineChart.destroy();
        window.barChart = new Chart(barChartCanvas, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    label: "Tỷ lệ tham gia khảo sát",
                    backgroundColor: "rgba(0,123,255,0.5)",
                    borderColor: "rgba(0,123,255,1)",
                    borderWidth: 1,
                    data: participationData
                }]
            },
            options: {
                responsive: true,
                scales: {
                    xAxes: [{
                        categoryPercentage: 0.45,
                        barPercentage: 0.70,
                        gridLines: false,
                        ticks: {
                            fontSize: 13,
                            padding: 10
                        }
                    }],
                    yAxes: [{
                        ticks: {
                            max: 100,
                            stepSize: 20,
                            beginAtZero: true,
                            fontSize: 13,
                            padding: 10
                        }
                    }]
                }
            }
        });

        window.lineChart = new Chart(lineChartCanvas, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    label: "Mức độ hài lòng",
                    backgroundColor: "rgba(0,123,255,0.5)",
                    borderColor: "rgba(0,123,255,1)",
                    borderWidth: 1,
                    data: satisfactionData
                }]
            },
            options: {
                responsive: true,
                scales: {
                    xAxes: [{
                        categoryPercentage: 0.45,
                        barPercentage: 0.70,
                        gridLines: false,
                        ticks: {
                            fontSize: 13,
                            padding: 10
                        }
                    }],
                    yAxes: [{
                        ticks: {
                            max: 100,
                            stepSize: 20,
                            beginAtZero: true,
                            fontSize: 13,
                            padding: 10
                        }
                    }]
                }
            }
        });
        $('#showchart').show();
        $('#error').hide();
    } else {
        let html =
            `<div class="alert alert-info">
                <div class="d-flex justify-content-start">
                    <span class="alert-icon m-r-20 font-size-30">
                        <i class="anticon anticon-close-circle"></i>
                    </span>
                    <div>
                        <h5 class="alert-heading">Oops...</h5>
                        <p>${res.message}</p>
                    </div>
                </div>
            </div>`;
        $('#showchart').hide();
        $('#error').html(html);
        $('#error').show();
    }
}

async function LoadChartSurveyThongTu01() {
    var year = $("#yearGiamSat").val();
    const response = await fetch('/api/giam_sat_ty_le_khao_sat_thong_tu_01', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({ id_nam_hoc: year }),
    });
    const res = await response.json();
    const barChartCtx = document.getElementById('bar01-chart').getContext('2d');
    const lineChartCtx = document.getElementById('line01-chart').getContext('2d');
    if (res.is_data) {
        let labels = [];
        let participationData = [];
        let satisfactionData = [];
        res.data.forEach((survey) => {
            survey.thong_ke_ty_le.forEach((tyleKhaoSat, index) => {
                let label = `${survey.ten_phieu.split('.')[0]}`;
                if (tyleKhaoSat.hoc_ky) {
                    label += ` - ${tyleKhaoSat.hoc_ky}`;
                }
                labels.push(label);

                let ty_le_tham_gia = tyleKhaoSat.ty_le_tham_gia_khao_sat
                    ? tyleKhaoSat.ty_le_tham_gia_khao_sat.ty_le_da_tra_loi
                    : 0;
                participationData.push(ty_le_tham_gia);

                let avg_ty_le_hai_long = tyleKhaoSat.ty_le_hai_long.length > 0
                    ? tyleKhaoSat.ty_le_hai_long[0].avg_ty_le_hai_long
                    : 0;
                satisfactionData.push(avg_ty_le_hai_long);
            });
        });

        if (window.bar01Chart) window.bar01Chart.destroy();
        if (window.line01Chart) window.line01Chart.destroy();

        window.bar01Chart = new Chart(barChartCtx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    label: "Tỷ lệ tham gia khảo sát",
                    backgroundColor: "rgba(0,123,255,0.5)",
                    borderColor: "rgba(0,123,255,1)",
                    borderWidth: 1,
                    data: participationData
                }]
            },
            options: {
                responsive: true,
                scales: {
                    xAxes: [{
                        categoryPercentage: 0.45,
                        barPercentage: 0.70,
                        gridLines: false,
                        ticks: {
                            fontSize: 13,
                            padding: 10
                        }
                    }],
                    yAxes: [{
                        ticks: {
                            max: 100,
                            stepSize: 20,
                            beginAtZero: true,
                            fontSize: 13,
                            padding: 10
                        }
                    }]
                }
            }
        });
        window.line01Chart = new Chart(lineChartCtx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    label: "Mức độ hài lòng",
                    backgroundColor: "rgba(0,123,255,0.5)",
                    borderColor: "rgba(0,123,255,1)",
                    borderWidth: 1,
                    data: satisfactionData
                }]
            },
            options: {
                responsive: true,
                scales: {
                    xAxes: [{
                        categoryPercentage: 0.45,
                        barPercentage: 0.70,
                        gridLines: false,
                        ticks: {
                            fontSize: 13,
                            padding: 10
                        }
                    }],
                    yAxes: [{
                        ticks: {
                            max: 100,
                            stepSize: 20,
                            beginAtZero: true,
                            fontSize: 13,
                            padding: 10
                        }
                    }]
                }
            }
        });
        $("#show_charts_01").show();
    }
    else {
        $("#show_charts_01").hide();
    }
}