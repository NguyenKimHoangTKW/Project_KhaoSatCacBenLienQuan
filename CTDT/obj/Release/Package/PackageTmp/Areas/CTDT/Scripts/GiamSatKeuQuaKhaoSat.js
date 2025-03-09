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
    } finally {
        EndLoading();
    }
});

$(document).on("click", "#btnFilter",async function (event) {
    event.preventDefault();
    try {
        await LoadChartFullSurvey();
    } finally {
        EndLoading();
    }
});
async function LoadChartFullSurvey() {
    var year = $("#yearGiamSat").val();
    var ctdt = $("#find-ctdt").val();
    const response = await fetch('/api/ctdt/giam_sat_ty_le_khao_sat', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({
            id_namhoc: year,
            id_ctdt: ctdt
        }),
    });
    var year_name = $("#yearGiamSat option:selected").text();
    var ctdt_name = $("#find-ctdt option:selected").text();
    $("#title-filter").text(`giám sát kết quả khảo sát : ${ctdt_name} - ${year_name}`);
    const res = await response.json();
    if (res.success) {
        const barChartCanvas = document.getElementById('bar-chart').getContext('2d');
        const lineChartCanvas = document.getElementById('line-chart').getContext('2d');
        const surveys = JSON.parse(res.data);
        surveys.sort((a, b) => {
            const idA = a.phieu.match(/\d+/) ? parseInt(a.phieu.match(/\d+/)[0]) : 0;
            const idB = b.phieu.match(/\d+/) ? parseInt(b.phieu.match(/\d+/)[0]) : 0;
            return idA - idB;
        });
        let labels = [];
        let participationData = [];
        let satisfactionData = [];
        surveys.forEach((survey) => {
            survey.ty_le_tham_gia_khao_sat.forEach((tyleKhaoSat) => {
                labels.push(survey.phieu.split('.')[0]);
                let ty_le_tham_gia = tyleKhaoSat.ty_le_da_tra_loi || 0;
                participationData.push(ty_le_tham_gia);
                let avg_ty_le_hai_long = tyleKhaoSat.muc_do_hai_long.length > 0
                    ? tyleKhaoSat.muc_do_hai_long[0].avg_ty_le_hai_long
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
    }
    else {
        let html =
            `<div class="alert alert-info">
                <div class="d-flex justify-content-start">
                    <span class="alert-icon m-r-20 font-size-30">
                        <i class="anticon anticon-close-circle"></i>
                    </span>
                    <div>
                        <h5 class="alert-heading">Oops...</h5>
                        <p>${res.message || "Không có dữ liệu khảo sát."}</p>
                    </div>
                </div>
            </div>`;
        $('#showchart').hide();
        $('#error').html(html);
        $('#error').show();
    }
}