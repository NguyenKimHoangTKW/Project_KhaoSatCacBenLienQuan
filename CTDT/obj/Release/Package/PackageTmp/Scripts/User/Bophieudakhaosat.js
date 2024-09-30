$(document).ready(function () {
    load_bo_phieu();
});

$(document).on('change', '#Year', function () {
    load_bo_phieu()
});
$(document).on('change', '#Hedaotao', function () {
    load_bo_phieu()
});
function load_bo_phieu() {
    Swal.fire({
        allowOutsideClick: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });
    var year = $('#Year').val();
    var hedaotao = $('#Hedaotao').val();
    $.ajax({
        url: '/Home/load_bo_phieu_da_khao_sat',
        type: 'POST',
        data: {
            year: year,
            hedaotao: hedaotao
        },
        success: function (res) {
            let body = $('#accordion-default');
            let html = '';
            body.empty();
            if (res.data.survey.length > 0 && res) {
                res.data.survey.forEach(function (phieu, index) {
                    let collapseId = `collapse${index}`;
                    html +=
                        `
                    <div class="card">
                        <div class="card-header p-2">
                            <h5 class="mb-0">
                                <a data-toggle="collapse" href="#${collapseId}">
                                    <span style="font-size: 20px;">${phieu.ten_phieu}</span>
                                </a>
                            </h5>
                        </div>
                        <div id="${collapseId}" class="collapse" data-parent="#accordion-default">
                            <div class="card-body">
                                <table class="table table-bordered">`;
                    if (phieu.is_ctdt) {
                        html += `<thead style="color:black; text-align:center; font-weight:bold">
                                        <tr>
                                            <th scope="col">STT</th>
                                            <th scope="col">Email khảo sát</th>
                                            <th scope="col">Chương trình đào tạo</th>
                                            <th scope="col">Khoa/Viện</th>
                                            <th scope="col">Thời gian khảo sát</th>
                                            <th scope="col">Năm học</th>
                                            <th scope="col">Chức năng</th>
                                        </tr>
                                    </thead>
                                    <tbody id="showdata">
                                       `
                        phieu.bo_phieu.forEach(function (item, index) {
                            html += ` 
                             <tr>
                                <td scope="col" class="formatSo">${index + 1}</td>
                                <td scope="col" class="formatSo">${item.email}</td>
                                <td scope="col" class="formatSo">${item.ctdt}</td>
                                <td scope="col" class="formatSo">${item.khoa}</td>
                                <td scope="col" class="formatSo">${unixTimestampToDate(item.thoi_gian_khao_sat)}</td>
                                <td scope="col" class="formatSo">${item.nam_hoc}</td>
                                <td scope="col" class="formatSo"><a href="${item.page}">Xem lại câu trả lời</a></td>
                             </tr>`;
                        })
                        html += ` 
                                    </tbody>`;
                        html += `</table>
                            </div>
                        </div>
                    </div>
                    `;
                    }
                    else if (phieu.is_student) {
                        html += `<thead style="color:black; text-align:center; font-weight:bold">
                                        <tr>
                                            <th scope="col">STT</th>
                                            <th scope="col">Email khảo sát</th>
                                            <th scope="col">Họ và tên người học</th>
                                            <th scope="col">Mã số người học</th>
                                            <th scope="col">Chương trình đào tạo</th>
                                            <th scope="col">Khoa/Viện</th>
                                            <th scope="col">Thời gian khảo sát</th>
                                            <th scope="col">Năm học</th>
                                            <th scope="col">Chức năng</th>
                                        </tr>
                                    </thead>
                                    <tbody id="showdata">
                                       `
                        phieu.bo_phieu.forEach(function (item, index) {
                            html += ` 
                             <tr>
                                <td scope="col" class="formatSo">${index + 1}</td>
                                <td scope="col" class="formatSo">${item.nguoi_hoc}</td>
                                <td scope="col" class="formatSo">${item.ma_nguoi_hoc}</td>
                                <td scope="col" class="formatSo">${item.email}</td>
                                <td scope="col" class="formatSo">${item.ctdt}</td>
                                <td scope="col" class="formatSo">${item.khoa}</td>
                                <td scope="col" class="formatSo">${unixTimestampToDate(item.thoi_gian_khao_sat)}</td>
                                <td scope="col" class="formatSo">${item.nam_hoc}</td>
                                <td scope="col" class="formatSo"><a href="${item.page}">Xem lại câu trả lời</a></td>
                             </tr>`;
                        })
                        html += ` 
                                    </tbody>`;
                        html += `</table>
                            </div>
                        </div>
                    </div>
                    `;
                    }
                    else if (phieu.is_cbvc) {
                        html += `<thead style="color:black; text-align:center; font-weight:bold">
                                        <tr>
                                            <th scope="col">STT</th>
                                            <th scope="col">Email khảo sát</th>
                                            <th scope="col">Họ và tên</th>
                                            <th scope="col">Chương trình đào tạo</th>
                                            <th scope="col">Khoa/Viện</th>
                                            <th scope="col">Thời gian khảo sát</th>
                                            <th scope="col">Năm học</th>
                                            <th scope="col">Chức năng</th>
                                        </tr>
                                    </thead>
                                    <tbody id="showdata">
                                       `
                        phieu.bo_phieu.forEach(function (item, index) {
                            html += ` 
                             <tr>
                                <td scope="col" class="formatSo">${index + 1}</td>
                                <td scope="col" class="formatSo">${item.email}</td>
                                <td scope="col" class="formatSo">${item.ten_cbcv}</td>
                                <td scope="col" class="formatSo">${item.ctdt}</td>
                                <td scope="col" class="formatSo">${item.khoa}</td>
                                <td scope="col" class="formatSo">${unixTimestampToDate(item.thoi_gian_khao_sat)}</td>
                                <td scope="col" class="formatSo">${item.nam_hoc}</td>
                                <td scope="col" class="formatSo"><a href="${item.page}">Xem lại câu trả lời</a></td>
                             </tr>`;
                        })
                        html += ` 
                                    </tbody>`;
                        html += `</table>
                            </div>
                        </div>
                    </div>
                    `;
                    }

                })
            }
            else {
                html = `
                    <div class="container" id="showdata">
                        <div class="alert alert-info" style="text-align: center;">
                            Chưa có biểu mẫu nào đã khảo sát
                        </div>
                    </div>`;

            }
            body.html(html);
        },
        complete: function () {
            Swal.close();
        }
    });
};
function unixTimestampToDate(unixTimestamp) {
    var date = new Date(unixTimestamp * 1000);

    var weekdays = ['Chủ Nhật', 'Thứ 2', 'Thứ 3', 'Thứ 4', 'Thứ 5', 'Thứ 6', 'Thứ 7'];

    var dayOfWeek = weekdays[date.getDay()];

    var month = ("0" + (date.getMonth() + 1)).slice(-2);
    var day = ("0" + date.getDate()).slice(-2);
    var year = date.getFullYear();
    var hours = ("0" + date.getHours()).slice(-2);
    var minutes = ("0" + date.getMinutes()).slice(-2);
    var seconds = ("0" + date.getSeconds()).slice(-2);
    var formattedDate = dayOfWeek + ', ' + day + "-" + month + "-" + year + " " + hours + ":" + minutes + ":" + seconds;
    return formattedDate;
}