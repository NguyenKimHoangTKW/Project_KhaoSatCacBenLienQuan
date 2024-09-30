$(document).ready(function () {
    LoadChartFullSurvey();
    LoadChartSurveyThongTu01();
});
$(document).on("change", "#yearGiamSat", function () {
    LoadChartFullSurvey();
    LoadChartSurveyThongTu01();
})


async function LoadChartFullSurvey() {
    var year = $("#yearGiamSat").val();

    try {
        const response = await fetch('/CTDT/GiamSatKetQuaKhaoSat/ChartsTyLeThamGiaKhaoSat', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ year: year }),
        });
        const res = await response.json();

        const barChart = document.getElementById('bar-chart').getContext('2d');
        const lineChart = document.getElementById('line-chart').getContext('2d');

        if (res.data && res.data.TitleSurvey.length > 0 && res.data.SurveyParticipationRate.length > 0 && res.data.SatisfactionLevel.length > 0) {
            let labels = [];
            let data = [];
            let MucDoHaiLong = [];

            let titleSurveyMap = {};
            let participationRateMap = {};
            let satisfactionLevelMap = {};

            res.data.TitleSurvey.forEach(function (chil) {
                if (chil.HocKy) {
                    let formatName = chil.NameSurvey.split('.')[0] + " - " + chil.HocKy;
                    titleSurveyMap[chil.IDSurvey] = formatName;
                }
                else {
                    let formatName = chil.NameSurvey.split('.')[0];
                    titleSurveyMap[chil.IDSurvey] = formatName;
                }
            });

            res.data.SurveyParticipationRate.forEach(function (chil) {
                participationRateMap[chil.IDPhieu] = chil.TyLeDaTraLoi;
            });

            res.data.SatisfactionLevel.forEach(function (survey) {
                let sum = 0;
                let totalQuestions = 0;
                survey.forEach(function (question) {
                    sum += question.TotalAgreePercentage;
                    totalQuestions++;
                });
                let AVG = totalQuestions > 0 ? sum / totalQuestions : 0;
                MucDoHaiLong.push(AVG);
            });

            for (let id in titleSurveyMap) {
                labels.push(titleSurveyMap[id]);
                data.push(participationRateMap[id] || 0);
                MucDoHaiLong.push(satisfactionLevelMap[id] || 0);
            }

            let combined = labels.map((label, index) => {
                return { label: label, data: data[index], satisfaction: MucDoHaiLong[index] };
            });

            combined.sort((a, b) => a.label.localeCompare(b.label));

            labels = combined.map(item => item.label);
            data = combined.map(item => item.data);
            MucDoHaiLong = combined.map(item => item.satisfaction);

            if (barChart.chart) barChart.chart.destroy();
            if (lineChart.chart) lineChart.chart.destroy();

            // Tỷ lệ tham gia khảo sát
            barChart.chart = new Chart(barChart, {
                type: 'bar',
                data: {
                    labels: labels,
                    datasets: [{
                        label: "Tỷ lệ tham gia khảo sát",
                        backgroundColor: "rgba(0,123,255,0.5)",
                        borderColor: "rgba(0,123,255,1)",
                        borderWidth: 1,
                        data: data
                    }]
                },
                options: {
                    scaleShowVerticalLines: false,
                    responsive: true,
                    scales: {
                        xAxes: [{
                            categoryPercentage: 0.45,
                            barPercentage: 0.70,
                            display: true,
                            scaleLabel: {
                                display: false,
                                labelString: 'Month'
                            },
                            gridLines: false,
                            ticks: {
                                display: true,
                                beginAtZero: true,
                                fontSize: 13,
                                padding: 10
                            }
                        }],
                        yAxes: [{
                            categoryPercentage: 0.35,
                            barPercentage: 0.70,
                            display: true,
                            scaleLabel: {
                                display: false,
                                labelString: 'Value'
                            },
                            gridLines: {
                                drawBorder: false,
                                offsetGridLines: false,
                                drawTicks: false,
                                borderDash: [3, 4],
                                zeroLineWidth: 1,
                                zeroLineBorderDash: [3, 4]
                            },
                            ticks: {
                                max: 100,
                                stepSize: 20,
                                display: true,
                                beginAtZero: true,
                                fontSize: 13,
                                padding: 10
                            }
                        }]
                    },
                    tooltips: {
                        enabled: true,
                        callbacks: {
                            label: function (tooltipItem, data) {
                                var dataset = data.datasets[tooltipItem.datasetIndex];
                                var currentValue = dataset.data[tooltipItem.index];
                                var label = dataset.label || '';
                                return label + ': ' + currentValue + '%';
                            }
                        }
                    }
                }
            });

            // Mức độ hài lòng
            lineChart.chart = new Chart(lineChart, {
                type: 'bar',
                data: {
                    labels: labels,
                    datasets: [{
                        label: "Mức độ hài lòng",
                        backgroundColor: "rgba(0,123,255,0.5)",
                        borderColor: "rgba(0,123,255,1)",
                        borderWidth: 1,
                        data: MucDoHaiLong
                    }]
                },
                options: {
                    scaleShowVerticalLines: false,
                    responsive: true,
                    scales: {
                        xAxes: [{
                            categoryPercentage: 0.45,
                            barPercentage: 0.70,
                            display: true,
                            scaleLabel: {
                                display: false,
                                labelString: 'Month'
                            },
                            gridLines: false,
                            ticks: {
                                display: true,
                                beginAtZero: true,
                                fontSize: 13,
                                padding: 10
                            }
                        }],
                        yAxes: [{
                            categoryPercentage: 0.35,
                            barPercentage: 0.70,
                            display: true,
                            scaleLabel: {
                                display: false,
                                labelString: 'Value'
                            },
                            gridLines: {
                                drawBorder: false,
                                offsetGridLines: false,
                                drawTicks: false,
                                borderDash: [3, 4],
                                zeroLineWidth: 1,
                                zeroLineBorderDash: [3, 4]
                            },
                            ticks: {
                                max: 100,
                                stepSize: 20,
                                display: true,
                                beginAtZero: true,
                                fontSize: 13,
                                padding: 10
                            }
                        }]
                    },
                    tooltips: {
                        enabled: true,
                        callbacks: {
                            label: function (tooltipItem, data) {
                                var dataset = data.datasets[tooltipItem.datasetIndex];
                                var currentValue = dataset.data[tooltipItem.index];
                                var label = dataset.label || '';
                                return label + ': ' + currentValue + '%';
                            }
                        }
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
                        <h5 class="alert-heading">Opps...</h5>
                        <p>Không có dữ liệu thống kê cho năm học này</p>
                    </div>
                </div>
            </div>`;
            $('#showchart').hide();
            $('#error').html(html);
            $('#error').show();
        }
    } catch (error) {
        console.error("Error fetching data:", error);
    }
}


async function LoadChartSurveyThongTu01() {
    var year = $("#yearGiamSat").val();

    try {
        const response = await fetch('/CTDT/GiamSatKetQuaKhaoSat/ChartsTyLeThamGiaKhaoSatThongTu01', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ year: year }),
        });

        const res = await response.json();

        const barChartCtx = document.getElementById('bar01-chart').getContext('2d');
        const lineChartCtx = document.getElementById('line01-chart').getContext('2d');

        if (window.barChart) {
            window.barChart.destroy();
        }
        if (window.lineChart) {
            window.lineChart.destroy();
        }

        let labels = [];
        let data = [];
        let MucDoHaiLong = [];

        if (res.data && res.data.TitleSurvey.length > 0 && res.data.SurveyParticipationRate.length > 0 && res.data.SatisfactionLevel.length > 0) {
            let titleSurveyMap = {};
            let participationRateMap = {};
            let satisfactionLevelMap = {};

            res.data.TitleSurvey.forEach(function (chil) {
                if (chil.HocKy) {
                    let formatName = chil.NameSurvey.split('.')[0] + " - " + chil.HocKy;
                    titleSurveyMap[chil.IDSurvey] = formatName;
                }
                else {
                    let formatName = chil.NameSurvey.split('.')[0];
                    titleSurveyMap[chil.IDSurvey] = formatName;
                }
            });

            res.data.SurveyParticipationRate.forEach(function (chil) {
                participationRateMap[chil.IDPhieu] = chil.TyLeDaTraLoi;
            });

            res.data.SatisfactionLevel.forEach(function (survey) {
                let sum = 0;
                let totalQuestions = 0;
                survey.forEach(function (question) {
                    sum += question.TotalAgreePercentage;
                    totalQuestions++;
                });
                let AVG = totalQuestions > 0 ? sum / totalQuestions : 0;
                MucDoHaiLong.push(AVG);
            });

            for (let id in titleSurveyMap) {
                labels.push(titleSurveyMap[id]);
                data.push(participationRateMap[id] || 0);
                MucDoHaiLong.push(satisfactionLevelMap[id] || 0);
            }

            let combined = labels.map((label, index) => {
                return { label: label, data: data[index], satisfaction: MucDoHaiLong[index] };
            });

            combined.sort((a, b) => a.label.localeCompare(b.label));

            labels = combined.map(item => item.label);
            data = combined.map(item => item.data);
            MucDoHaiLong = combined.map(item => item.satisfaction);
        }

        // Tỷ lệ tham gia khảo sát
        window.barChart = new Chart(barChartCtx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    label: "Tỷ lệ tham gia khảo sát",
                    backgroundColor: "rgba(0,123,255,0.5)",
                    borderColor: "rgba(0,123,255,1)",
                    borderWidth: 1,
                    data: data
                }]
            },
            options: {
                scaleShowVerticalLines: false,
                responsive: true,
                scales: {
                    xAxes: [{
                        categoryPercentage: 0.45,
                        barPercentage: 0.70,
                        display: true,
                        scaleLabel: {
                            display: false,
                            labelString: 'Month'
                        },
                        gridLines: false,
                        ticks: {
                            display: true,
                            beginAtZero: true,
                            fontSize: 13,
                            padding: 10
                        }
                    }],
                    yAxes: [{
                        categoryPercentage: 0.35,
                        barPercentage: 0.70,
                        display: true,
                        scaleLabel: {
                            display: false,
                            labelString: 'Value'
                        },
                        gridLines: {
                            drawBorder: false,
                            offsetGridLines: false,
                            drawTicks: false,
                            borderDash: [3, 4],
                            zeroLineWidth: 1,
                            zeroLineBorderDash: [3, 4]
                        },
                        ticks: {
                            max: 100,
                            stepSize: 20,
                            display: true,
                            beginAtZero: true,
                            fontSize: 13,
                            padding: 10
                        }
                    }]
                },
                tooltips: {
                    enabled: true,
                    callbacks: {
                        label: function (tooltipItem, data) {
                            var dataset = data.datasets[tooltipItem.datasetIndex];
                            var currentValue = dataset.data[tooltipItem.index];
                            var label = dataset.label || '';
                            return label + ': ' + currentValue + '%';
                        }
                    }
                }
            }
        });

        // Mức độ hài lòng
        window.lineChart = new Chart(lineChartCtx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    label: "Mức độ hài lòng",
                    backgroundColor: "rgba(0,123,255,0.5)",
                    borderColor: "rgba(0,123,255,1)",
                    borderWidth: 1,
                    data: MucDoHaiLong
                }]
            },
            options: {
                scaleShowVerticalLines: false,
                responsive: true,
                scales: {
                    xAxes: [{
                        categoryPercentage: 0.45,
                        barPercentage: 0.70,
                        display: true,
                        scaleLabel: {
                            display: false,
                            labelString: 'Month'
                        },
                        gridLines: false,
                        ticks: {
                            display: true,
                            beginAtZero: true,
                            fontSize: 13,
                            padding: 10
                        }
                    }],
                    yAxes: [{
                        categoryPercentage: 0.35,
                        barPercentage: 0.70,
                        display: true,
                        scaleLabel: {
                            display: false,
                            labelString: 'Value'
                        },
                        gridLines: {
                            drawBorder: false,
                            offsetGridLines: false,
                            drawTicks: false,
                            borderDash: [3, 4],
                            zeroLineWidth: 1,
                            zeroLineBorderDash: [3, 4]
                        },
                        ticks: {
                            max: 100,
                            stepSize: 20,
                            display: true,
                            beginAtZero: true,
                            fontSize: 13,
                            padding: 10
                        }
                    }]
                },
                tooltips: {
                    enabled: true,
                    callbacks: {
                        label: function (tooltipItem, data) {
                            var dataset = data.datasets[tooltipItem.datasetIndex];
                            var currentValue = dataset.data[tooltipItem.index];
                            var label = dataset.label || '';
                            return label + ': ' + currentValue + '%';
                        }
                    }
                }
            }
        });

    } catch (error) {
        console.error("Error fetching data:", error);
        let html =
            `<div class="alert alert-info">
                <div class="d-flex justify-content-start">
                    <span class="alert-icon m-r-20 font-size-30">
                        <i class="anticon anticon-close-circle"></i>
                    </span>
                    <div>
                        <h5 class="alert-heading">Error</h5>
                        <p>Không có dữ liệu thống kê cho năm học này</p>
                    </div>
                </div>
            </div>`;
        $('#error').html(html);
        $('#error').show();
    }
}
