$(document).ready(function () {
    load_bo_phieu();
});
function showLoading() {
    Swal.fire({
        title: 'Loading...',
        text: 'Đang kiểm tra và tải dữ liệu, vui lòng chờ trong giây lát!',
        allowOutsideClick: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });
}
function hideLoading() {
    Swal.close();
}
$(document).on('change', '#Year', function () {
    load_bo_phieu()
});
$(document).on('change', '#Hedaotao', function () {
    load_bo_phieu()
});
async function load_bo_phieu() {
    var year = $('#Year').val();
    var hedaotao = $('#Hedaotao').val();
    const res = await $.ajax({
        url: '/api/load_bo_phieu_da_khao_sat',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            namhoc: year,
            hedaotao: hedaotao
        })
    })
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
                                            <th scope="col">Mã người học</th>
                                            <th scope="col">Họ và tên người học</th>                                           
                                            <th scope="col">Thuộc lớp</th>
                                            <th scope="col">Thuộc chương trình đào tạo</th>
                                            <th scope="col">Thời gian khảo sát</th>
                                            <th scope="col">Chức năng</th>
                                        </tr>
                                    </thead>
                                    <tbody id="showdata">
                                       `
                phieu.bo_phieu.forEach(function (item, index) {
                    html += ` 
                             <tr>
                                <td scope="col" class="formatSo">${index + 1}</td>
                                <td scope="col">${item.email}</td>
                                <td scope="col" class="formatSo">${item.ma_nh}</td>
                                <td scope="col">${item.ten_nh}</td>
                                <td scope="col">${item.thuoc_lop}</td>
                                <td scope="col">${item.thuoc_ctdt}</td>
                                <td scope="col" class="formatSo">${unixTimestampToDate(item.thoi_gian_khao_sat)}</td>
                                <td scope="col" class="formatSo"><a href="${item.value_page}">${item.page}</a></td>
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
                                            <th scope="col">Mã cán bộ viên chức</th>
                                            <th scope="col">Tên cán bộ viên chức</th>
                                            <th scope="col">Chức vụ</th>
                                            <th scope="col">Trình độ</th>
                                            <th scope="col">Thuộc đơn vị</th>
                                            <th scope="col">Ngành đào tạo</th>
                                            <th scope="col">Khảo sát lần cuối</th>
                                            <th scope="col">Chức năng</th>
                                        </tr>
                                    </thead>
                                    <tbody id="showdata">
                                       `
                phieu.bo_phieu.forEach(function (item, index) {
                    html += ` 
                             <tr>
                                <td scope="col" class="formatSo">${index + 1}</td>
                                <td scope="col">${item.email}</td>
                                <td scope="col">${item.MaCBVC}</td>
                                <td scope="col">${item.TenCBVC}</td>
                                <td scope="col">${item.name_chucvu}</td>
                                <td scope="col">${item.ten_trinh_do}</td>
                                <td scope="col">${item.ten_khoa}</td>
                                <td scope="col">${item.nganh_dao_tao}</td>
                                <td scope="col" class="formatSo">${unixTimestampToDate(item.thoi_gian_khao_sat)}</td>
                                <td scope="col" class="formatSo"><a href="${item.value_page}">${item.page}</a></td>
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
            else if (phieu.is_gv) {
                html += `<thead style="color:black; text-align:center; font-weight:bold">
                                        <tr>
                                            <th scope="col">STT</th>
                                            <th scope="col">Email khảo sát</th>
                                            <th scope="col">Mã cán bộ viên chức</th>
                                            <th scope="col">Tên cán bộ viên chức</th>
                                            <th scope="col">Chức vụ</th>
                                            <th scope="col">Trình độ</th>
                                            <th scope="col">Thuộc đơn vị</th>
                                            <th scope="col">Ngành đào tạo</th>
                                            <th scope="col">Khảo sát cho chương trình đào tạo</th>
                                            <th scope="col">Khảo sát lần cuối</th>
                                            <th scope="col">Chức năng</th>
                                        </tr>
                                    </thead>
                                    <tbody id="showdata">
                                       `
                phieu.bo_phieu.forEach(function (item, index) {
                    html += ` 
                             <tr>
                                <td scope="col" class="formatSo">${index + 1}</td>
                                <td scope="col">${item.email}</td>
                                <td scope="col">${item.MaCBVC}</td>
                                <td scope="col">${item.TenCBVC}</td>
                                <td scope="col">${item.name_chucvu}</td>
                                <td scope="col">${item.ten_trinh_do}</td>
                                <td scope="col">${item.ten_khoa}</td>
                                <td scope="col">${item.nganh_dao_tao}</td>
                                <td scope="col">${item.khao_sat_cho}</td>
                                <td scope="col" class="formatSo">${unixTimestampToDate(item.thoi_gian_khao_sat)}</td>
                                <td scope="col"><a href="${item.value_page}">${item.page}</a></td>
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
            else if (phieu.is_nh_hp) {
                html += `<thead style="color:black; text-align:center; font-weight:bold">
                                        <tr>
                                            <th scope="col">STT</th>
                                            <th scope="col">Email khảo sát</th>
                                            <th scope="col">Mã người học</th>
                                            <th scope="col">Họ và tên người học</th>                                           
                                            <th scope="col">Học phần</th>                                           
                                            <th scope="col">Mã môn học</th>                                           
                                            <th scope="col">Môn học</th>                                           
                                            <th scope="col">Lớp</th>                                           
                                            <th scope="col">Giảng viên giảng dạy</th>                                           
                                            <th scope="col">Chức năng</th>
                                        </tr>
                                    </thead>
                                    <tbody id="showdata">
                                       `
                phieu.bo_phieu.forEach(function (item, index) {
                    html += ` 
                             <tr>
                                <td scope="col" class="formatSo">${index + 1}</td>
                                <td scope="col">${item.email}</td>
                                <td scope="col" class="formatSo">${item.ma_nh}</td>
                                <td scope="col">${item.ten_nh}</td>
                                <td scope="col">${item.hoc_phan}</td>
                                <td scope="col" class="formatSo">${item.ma_mh}</td>
                                <td scope="col">${item.mon_hoc}</td>
                                <td scope="col" class="formatSo">${item.lop}</td>
                                <td scope="col">${item.giang_vien_giang_day}</td>
                                <td scope="col" class="formatSo">${unixTimestampToDate(item.thoi_gian_khao_sat)}</td>
                                <td scope="col"><a href="${item.value_page}">${item.page}</a></td>
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
