$(document).ready(function () {
    LoadData();
});

function LoadData() {
    var ids = document.getElementById('surveyIds').value;

    $.ajax({
        url: '/Survey/LoadSurveyForm?ids=' + ids,
        type: 'GET',
        success: function (res) {
            var items = res.data;
            var html = "";

            if (items.length === 0) {
                html += `
                    <div class='col-lg-12 survey-item'>
                        <div class='card survey-card border-0'>
                            <div class='card-body'>
                                <h5 class='card-title text-center' style='font-family: Tahoma, sans-serif;'>Không có dữ liệu.</h5>
                            </div>
                        </div>
                    </div>`;
            } else {
                items.sort((a, b) => {
                    var MaPhieuA = a.TieuDePhieu.split('.')[0];
                    var MaPhieuB = b.TieuDePhieu.split('.')[0];
                    return MaPhieuA.localeCompare(MaPhieuB, undefined, { numeric: true, sensitivity: 'base' });
                });

                items.forEach(item => {
                    var maxChars = 150;
                    var truncatedText = item.MoTaPhieu.length > maxChars ? item.MoTaPhieu.substring(0, maxChars) + '...' : item.MoTaPhieu;
                    var MaPhieu = item.TieuDePhieu.split('.')[0];
                    var TenPhieu = item.TieuDePhieu.split('.')[1];
                    html += `
                        <div class='col-md-4 mb-4 survey-item'>
                            <div class='card feature-card h-100'>
                                <div class='card-body d-flex flex-column'>
                                    <a href='javascript:void(0)' onclick='window.location="/Survey/ListAnswerSurvey?id=${item.MaPhieu }"' class='feature-icon mb-3' style='text-decoration: none;'>
                                        <p style='font-size: 28px; font-weight: bold;color: #0056b3;'>${MaPhieu}</p>
                                    </a>
                                    
                                    <a href='javascript:void(0)' onclick='window.location="/Survey/ListAnswerSurvey?id=${item.MaPhieu }"' style='text-decoration: none;color:black;'>
                                        <h5 class='card-title'>${TenPhieu}</h5>
                                    </a>
                                    <p class='card-text flex-grow-1'>${truncatedText}</p>
                                    <button class='btn btn-primary mt-auto' onclick='window.location="/Survey/ListAnswerSurvey?id=${item.MaPhieu }"'>Xem chi tiết</button>
                                </div>
                            </div>
                        </div>`;
                });
            }
            $('#showdata').html(html);
        }
    });
}
