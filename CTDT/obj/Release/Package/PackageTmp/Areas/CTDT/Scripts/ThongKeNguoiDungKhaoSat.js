var currentPage = 1;
var totalPages = 0;

$(document).ready(function () {
    LoadChartSurvey();
    $(document).on("change", "#Year", function () {
        LoadChartSurvey();
    });
});

function showLoading() {
    Swal.fire({
        title: 'Loading...',
        text: 'Đang tải dữ liệu, vui lòng chờ trong giây lát!',
        allowOutsideClick: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });
}

function hideLoading() {
    Swal.close();
}
function LoadChartSurvey() {
    var year = $("#Year").val();
    $.ajax({
        url: '/CTDT/ThongKeKhaoSat/LoadChartsDoiTuongChuaKhaoSat',
        type: 'GET',
        data: { year: year },
        success: function (res) {
            $('#survey-list').empty();
            if (res.data.AllSurvey && res.data.AllSurvey.length > 0) {
                res.data.AllSurvey.sort((a, b) => {
                    let idA = (typeof a.NameSurvey === 'string') ? a.NameSurvey.split(".")[0] : '';
                    let idB = (typeof b.NameSurvey === 'string') ? b.NameSurvey.split(".")[0] : '';
                    return idA.localeCompare(idB, undefined, { numeric: true });
                });

                res.data.AllSurvey.forEach(function (survey) {
                    const MaPhieu = survey.NameSurvey.split(".")[0].toUpperCase();
                    const TieuDePhieu = survey.NameSurvey.split(".")[1];
                    const surveyData = res.data.ChartSurvey.find(chil => chil.IDPhieu === survey.IDSurvey);
                    const card = `
                        <div class="card survey-card">
                            <div class="card-body">
                                <div style="align-items: center;">
                                    <p style="color:#5029ff;font-weight:bold; position: absolute; top: 0; left: 20px;">${MaPhieu}</p>
                                    <a href="" style="color:#5029ff;font-weight:bold; position: absolute; top: 14px; right: 20px;" data-toggle="modal" data-target=".bd-example-modal-lg" data-maphieu="${survey.IDSurvey}">Xem chi tiết</a>
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

                    const datas = surveyData ? [surveyData.TongPhieuChuaTraLoi, surveyData.TongPhieuDaTraLoi] : [1, 0];
                    const colors = surveyData ? ['#007bff', '#ffc107'] : ['#d3d3d3', '#d3d3d3'];

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
                                <p>Không có dữ liệu phiếu khảo sát cho năm học này !</p>
                            </div>
                        </div>
                    </div>`;
                $('#survey-list').append(card);
                $('.chart').hide();
            }
        },
        error: function (xhr, status, error) {
            console.error('Error fetching surveys: ', error);
        }
    });
}
$(document).on('click', '[data-toggle="modal"][data-maphieu]', function (e) {
    e.preventDefault();
    const maPhieu = $(this).data('maphieu');
    $('.bd-example-modal-lg').data('maphieu', maPhieu);
    fetchModalContent(maPhieu);
    $(document).on('click', '.page-link', function (e) {
        e.preventDefault();
        var page = $(this).data('page');
        if (page && page !== currentPage && page > 0 && page <= totalPages) {
            currentPage = page;
            var complete = $("#SurveyStatus").val();
            LoadData(currentPage, maPhieu, complete);
        }
    });
});



function fetchModalContent(maPhieu) {
    $.ajax({
        url: '/CTDT/ThongKeKhaoSat/LoadSurveyDetail',
        type: 'GET',
        data: { surveyid: maPhieu },
        dataType: 'json',
        success: function (response) {
            if (response.data) {
                $(".modal-title").text(response.data.NameSurvey);
                LoadData(1, response.data.IDSurvey, -1);
            } else {
                $('.modal-body').html('<p>Không có dữ liệu</p>');
            }
            $('.bd-example-modal-lg').modal('show');
        },
        error: function (xhr, status, error) {
            console.error('Error fetching survey details: ', error);
        }
    });
}

function LoadData(pageNumber, survey, completed = -1) {
    $.ajax({
        url: '/CTDT/ThongKeKhaoSat/LoadSVChuaKhaoSat',
        type: 'GET',
        data: {
            pageNumber: pageNumber,
            pageSize: 7,
            survey: survey,
            completed: completed
        },
        success: function (res) {
            var items = res.data;
            totalPages = res.totalPages;

            if (!items || items.length === 0) {
                const html = '<tr><td colspan="9" class="text-center">Không có dữ liệu</td></tr>';
                $('#showdata').html(html);
                $('#data-table-section').show();
                $('#pagination').html('');
            } else {
                let html = '';
                let header = '';
                items.forEach(function (Chil, i) {
                    var index = (pageNumber - 1) * 7 + i + 1;
                    if (Chil.MSSV) {
                        header = `
                            <tr>
                                <th scope="col">STT</th>
                                <th scope="col">MSSV</th>
                                <th scope="col">Họ và tên</th>
                                <th scope="col">Ngày sinh</th>
                                <th scope="col">Số điện thoại</th>
                                <th scope="col">Chương trình đào tạo</th>
                                <th scope="col">Lớp</th>
                                <th scope="col">Trạng thái</th>
                            </tr>
                        `;
                        html += `
                            <tr>
                                <td>${index}</td>
                                <td>${Chil.MSSV}</td>
                                <td>${Chil.Hoten}</td>
                                <td>${Chil.NgaySinh}</td>
                                <td>${Chil.SDT}</td>
                                <td>${Chil.CTDT}</td>
                                <td>${Chil.Lop}</td>
                                <td>${Chil.DaKhaoSat}</td>
                            </tr>
                        `;
                        $("#FilterSV").show();
                        $("#HideSurveystatus").show();
                        $("#ExportExcel").show();
                    }
                    else if (res.isCBVC) {
                        header = `
                            <tr>
                                <th scope="col">STT</th>
                                <th scope="col">Họ và tên</th>
                                <th scope="col">Chức vụ</th>
                                <th scope="col">Chương trình đào tạo</th>
                            </tr>
                        `;
                        html += `
                            <tr>
                                <td>${index}</td>
                                <td>${Chil.HoTen}</td>
                                <td>${Chil.ChucVu}</td>
                                <td>${Chil.CTDT}</td>
                            </tr>
                        `;
                        $("#FilterSV").hide();
                        $("#HideSurveystatus").hide();
                        $("#ExportExcel").show();
                    }
                    else {
                        header = `
                            <tr>
                                <th scope="col">STT</th>
                                <th scope="col">Họ và tên</th>
                                <th scope="col">Email</th>
                                <th scope="col">Chương trình đào tạo</th>
                            </tr>
                        `;
                        html += `
                            <tr>
                                <td>${index}</td>
                                <td>${Chil.HoTen}</td>
                                <td>${Chil.Email}</td>
                                <td>${Chil.CTDT}</td>
                            </tr>
                        `;
                        $("#FilterSV").hide();
                        $("#HideSurveystatus").hide();
                        $("#ExportExcel").show();
                    }
                });
                $('#datathead').html(header);
                $('#showdata').html(html);
                renderPagination(pageNumber, totalPages, survey);
                $('#data-table-section').show();
            }
        },
        error: function (xhr, status, error) {
            console.error('Error fetching data: ', error);
        }
    });
}
function ExportExcel() {
    var survey = $('.bd-example-modal-lg').data('maphieu');
    var completed = $("#SurveyStatus").val();
    var timerInterval;

    $.ajax({
        url: "/CTDT/ThongKeKhaoSat/ExportToExcel",
        type: "GET",
        data: {
            survey: survey,
            completed: completed
        },
        success: function (res) {
            if (res.data === null) {
                Swal.fire({
                    title: 'Thông báo',
                    text: res.message,
                    icon: 'warning',
                    confirmButtonText: 'OK'
                });
            } else {
                Swal.fire({
                    title: "Đang kiểm tra và xuất dữ liệu...",
                    html: "Vui lòng chờ trong <b></b> giây.",
                    timer: 3000,
                    timerProgressBar: true,
                    didOpen: () => {
                        Swal.showLoading();
                        const timer = Swal.getHtmlContainer().querySelector("b");
                        timerInterval = setInterval(() => {
                            timer.textContent = Math.round(Swal.getTimerLeft() / 1000);
                        }, 100);
                    },
                    willClose: () => {
                        clearInterval(timerInterval);
                    }
                }).then((result) => {
                    if (result.dismiss === Swal.DismissReason.timer) {
                        window.location.href = '/CTDT/ThongKeKhaoSat/ExportToExcel?survey=' + survey + '&completed=' + completed;
                    }
                });
            }
        },
        error: function () {
            Swal.fire({
                title: 'Lỗi',
                text: 'Không thể xuất dữ liệu. Vui lòng thử lại sau.',
                icon: 'error',
                confirmButtonText: 'OK'
            });
        }
    });
}

function Filter() {
    var Complete = $("#SurveyStatus").val()
    var survey = $('.bd-example-modal-lg').data('maphieu');
    LoadData(1, survey, Complete);
}

function renderPagination(currentPage, totalPages, maPhieu) {
    var html = '<nav aria-label="Page navigation example"><ul class="pagination justify-content-end">';

    var startPage = currentPage - 2;
    var endPage = currentPage + 2;

    if (startPage < 1) {
        startPage = 1;
        endPage = 5;
    }

    if (endPage > totalPages) {
        endPage = totalPages;
        startPage = totalPages - 4;
    }

    if (startPage < 1) {
        startPage = 1;
    }

    html += '<li class="page-item ' + (currentPage == 1 ? 'disabled' : '') + '"><a class="page-link" href="#" data-page="' + (currentPage - 1) + '" data-maphieu="' + maPhieu + '">Trước</a></li>';

    for (var i = startPage; i <= endPage; i++) {
        html += '<li class="page-item ' + (currentPage == i ? 'active' : '') + '"><a class="page-link" href="#" data-page="' + i + '" data-maphieu="' + maPhieu + '">' + i + '</a></li>';
    }

    html += '<li class="page-item ' + (currentPage == totalPages ? 'disabled' : '') + '"><a class="page-link" href="#" data-page="' + (currentPage + 1) + '" data-maphieu="' + maPhieu + '">Tiếp</a></li>';
    html += '</ul></nav>';

    $('#pagination').html(html);
}