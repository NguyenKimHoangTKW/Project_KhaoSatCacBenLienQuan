﻿let show_time_check = ``;
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
$(document).on('click', '#ExportExcel', function () {
    if ($('#bao_cao_tong_hop').text().trim() === 'Không có dữ liệu báo cáo tổng hợp cho năm học này') {
        Swal.fire({
            title: 'Không có dữ liệu',
            text: 'Không có dữ liệu có sẵn để xuất Excel',
            icon: 'warning',
            confirmButtonText: 'OK'
        });
    } else {
        let timerInterval;
        Swal.fire({
            title: "Loading ...",
            html: "Đang kiểm tra và xuất kết quả, vui lòng chờ <b></b> giây.",
            timer: 4000,
            timerProgressBar: true,
            didOpen: () => {
                Swal.showLoading();
                const timer = Swal.getPopup().querySelector("b");
                timerInterval = setInterval(() => {
                    timer.textContent = Math.ceil(Swal.getTimerLeft() / 1000);
                }, 100);
            },
            willClose: () => {
                clearInterval(timerInterval);
            }
        }).then((result) => {
            if (result.dismiss === Swal.DismissReason.timer) {
                ExportExcelBaoCaoTongHop();
            }
        });
    }
})
$(document).on('click', '#btnFilter', async function (event) {
    event.preventDefault();
    Loading()
    try {
        await LoadKetQua();
        $("#show-time-check").text(`Kết quả được thống kê từ khoảng thời gian: ${show_time_check}`);
    }
    finally {
        EndLoading()
    }
})
function getFormattedDateTime() {
    const now = new Date();
    const year = now.getFullYear();
    const month = String(now.getMonth() + 1).padStart(2, '0');
    const day = String(now.getDate()).padStart(2, '0');
    const hours = String(now.getHours()).padStart(2, '0');
    const minutes = String(now.getMinutes()).padStart(2, '0');
    const seconds = String(now.getSeconds()).padStart(2, '0');

    return `${year}-${month}-${day}_${hours}-${minutes}-${seconds}`;
}
function LayThoiGian() {
    const now = new Date();
    const year = now.getFullYear();
    const month = String(now.getMonth() + 1).padStart(2, '0');
    const day = String(now.getDate()).padStart(2, '0');
    const hours = String(now.getHours()).padStart(2, '0');
    const minutes = String(now.getMinutes()).padStart(2, '0');
    const seconds = String(now.getSeconds()).padStart(2, '0');

    return `${day}-${month}-${year} ${hours}:${minutes}:${seconds}`;
}
function ExportExcelBaoCaoTongHop() {
    let workbook = new ExcelJS.Workbook();
    let worksheet = workbook.addWorksheet('Thống kê kết quả');
    let headerStyle = {
        font: { name: 'Times New Roman', family: 4, bold: true, size: 12 },
        alignment: { horizontal: 'center', vertical: 'middle' },
        border: {
            top: { style: 'thin' },
            left: { style: 'thin' },
            bottom: { style: 'thin' },
            right: { style: 'thin' }
        },
        fill: {
            type: 'pattern',
            pattern: 'solid',
            fgColor: { argb: '83b4ff' }
        }
    };

    let cellStyle = {
        font: { name: 'Times New Roman', family: 4, size: 12 },
        border: {
            top: { style: 'thin' },
            left: { style: 'thin' },
            bottom: { style: 'thin' },
            right: { style: 'thin' }
        },
        alignment: { vertical: 'middle', wrapText: true }
    };
    let cellNumberStyle = {
        font: { name: 'Times New Roman', family: 4, size: 12 },
        border: {
            top: { style: 'thin' },
            left: { style: 'thin' },
            bottom: { style: 'thin' },
            right: { style: 'thin' }
        },
        alignment: { horizontal: 'center', vertical: 'middle' },
    };

    let CTDTExport = "Chương trình đào tạo: " + $("#find-ctdt option:selected").text().toUpperCase();
    worksheet.addRow([CTDTExport]);
    let lastRowCTDT = worksheet.lastRow.number;
    worksheet.mergeCells(`A${lastRowCTDT}:F${lastRowCTDT}`);
    let mergedCellCTDT = worksheet.getCell(`A${lastRowCTDT}`);
    mergedCellCTDT.font = { bold: true, size: 14 };
    mergedCellCTDT.alignment = { horizontal: 'center', vertical: 'middle' };
  

    let SurveyYear = "NĂM HỌC: " + $("#yearGiamSat option:selected").text().toUpperCase();
    worksheet.addRow([SurveyYear]);
    let lastRowYear = worksheet.lastRow.number;
    worksheet.mergeCells(`A${lastRowYear}:B${lastRowYear}`);
    let mergedCellYear = worksheet.getCell(`A${lastRowYear}`);
    mergedCellYear.font = { bold: true, size: 14 };
    mergedCellYear.alignment = { horizontal: 'center', vertical: 'middle' };

    let TimeCheck = "Khoảng thời gian thống kê kết quả : " + show_time_check;
    worksheet.addRow([TimeCheck]);
    let lastRowTimeCheck = worksheet.lastRow.number;
    worksheet.mergeCells(`A${lastRowTimeCheck}:B${lastRowTimeCheck}`);
    let mergedCellTimeCheck = worksheet.getCell(`A${lastRowTimeCheck}`);
    mergedCellTimeCheck.font = { bold: true, size: 14 };
    mergedCellTimeCheck.alignment = { horizontal: 'center', vertical: 'middle' };
    worksheet.addRow([]);

    let TimeExport = "Thời gian xuất kết quả : " + LayThoiGian();
    worksheet.addRow([TimeExport]);
    let lastRowTime = worksheet.lastRow.number;
    worksheet.mergeCells(`A${lastRowTime}:B${lastRowTime}`);
    let mergedCellTime = worksheet.getCell(`A${lastRowTime}`);
    mergedCellTime.font = { bold: true, size: 14 };
    mergedCellTime.alignment = { horizontal: 'center', vertical: 'middle' };
    worksheet.addRow([]);
   
    let table = document.querySelector('#bao_cao_tong_hop table');
    let thead = table.querySelector('thead');
    let tbody = table.querySelector('tbody');

    let headers = [];
    thead.querySelectorAll('th').forEach(th => {
        headers.push(th.textContent);
    });
    let headerRow = worksheet.addRow(headers);
    headerRow.eachCell((cell) => {
        cell.style = headerStyle;
    });

    tbody.querySelectorAll('tr').forEach(tr => {
        let rowData = [];
        tr.querySelectorAll('td').forEach(td => {
            rowData.push(td.textContent);
        });
        let dataRow = worksheet.addRow(rowData);
        dataRow.eachCell((cell, colNumber) => {
            if (!isNaN(cell.value) && cell.value !== '') {
                cell.style = cellNumberStyle;
            } else if (typeof cell.value === 'string' && cell.value.trim().endsWith('%')) {
                cell.style = cellNumberStyle;
            } else {
                cell.style = cellStyle;
            }
        });

    });

    worksheet.columns.forEach(column => {
        column.width = 40;
    });
    workbook.xlsx.writeBuffer().then(function (buffer) {
        const dateTime = getFormattedDateTime();
        const filename = `Báo cáo tổng hợp_${dateTime}.xlsx`;
        saveAs(new Blob([buffer], { type: "application/octet-stream" }), filename);
    });
}
async function LoadKetQua() {
    var Year = $('#yearGiamSat').val();
    var ctdt = $('#find-ctdt').val();
    const from_date = $("#from_date").val();
    const to_date = $("#to_date").val();
    const startTimestamp = convertToTimestamp(from_date);
    const endTimestamp = convertToTimestamp(to_date);
    const res = await $.ajax({
        url: '/api/ctdt/bao-cao-tong-hop-ket-qua-khao-sat',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            id_namhoc: Year,
            id_ctdt: ctdt,
            from_date: startTimestamp,
            to_date: endTimestamp
        })
    });
    let body = $('#showdata');
    let thead = $('#showthead');
    body.empty();
    thead.empty();
    let html = ``;
    if (res.success) {
        
        $("#title_notification").hide();
        const data = JSON.parse(res.data);
        data.sort((a, b) => {
            const idA = a.phieu.split(".")[0];
            const idB = b.phieu.split(".")[0];
            return idA.localeCompare(idB, undefined, { numeric: true });
        });
        let title = `
    <tr>
        <th scope="col">STT</th>
        <th scope="col">Phiếu khảo sát</th>
        <th scope="col">Tỷ lệ tham gia khảo sát</th>
        <th scope="col">Tỷ lệ chưa tham gia khảo sát</th>
        <th scope="col">Mức độ hài lòng</th>
        <th scope="col">Điểm trung bình</th>
        <th scope="col">Tỷ lệ % để phiếu đạt</th>
        <th scope="col">Kết quả</th>
    </tr>
`;
        thead.html(title);
        data.forEach(function (survey, index) {
            survey.ty_le_tham_gia_khao_sat.forEach(function (tylekhaosat) {
                html += `<tr>`;
                html += `<td class="formatSo">${index + 1}</td>`;
                html += `<td>${survey.phieu}</td>`;
                let ty_le_da_tra_loi = tylekhaosat.ty_le_da_tra_loi ? tylekhaosat.ty_le_da_tra_loi : 0;
                html += `<td class="formatSo">${ty_le_da_tra_loi}%</td>`;
                let ty_le_chua_tra_loi = tylekhaosat.ty_le_chua_tra_loi ? tylekhaosat.ty_le_chua_tra_loi : 0;
                html += `<td class="formatSo">${ty_le_chua_tra_loi}%</td>`;
                let muc_do_hai_long = tylekhaosat.muc_do_hai_long[0]; 
                let avg_ty_le_hai_long = muc_do_hai_long ? muc_do_hai_long.avg_ty_le_hai_long : 0;
                let avg_score = muc_do_hai_long ? muc_do_hai_long.avg_score : 0;
                html += `<td class="formatSo">${avg_ty_le_hai_long}%</td>`;
                html += `<td class="formatSo">${avg_score}</td>`
                html += `<td class="formatSo">${survey.ty_le_phan_tram_dat}%</td>`
                html += `<td class="formatSo" style="color:${avg_ty_le_hai_long >= survey.ty_le_phan_tram_dat && ty_le_da_tra_loi >= survey.ty_le_phan_tram_dat ? "green" : "red"};font-weight: bold;">${avg_ty_le_hai_long >= survey.ty_le_phan_tram_dat && ty_le_da_tra_loi >= survey.ty_le_phan_tram_dat ? "Đạt" : "Chưa đạt"}</td>`
                html += `</tr>`;
            });
            
        });
        const check_time_check = JSON.parse(res.time_check);
        check_time_check.forEach(timecheck => {
            show_time_check = `${unixTimestampToDate(timecheck.time_check_start)} đến ${unixTimestampToDate(timecheck.time_check_end)}`;
        });
        body.html(html);
    } else {
        html = `
                <div class="alert alert-info">
                <div class="d-flex justify-content-start">
                    <span class="alert-icon m-r-20 font-size-30">
                        <i class="anticon anticon-close-circle"></i>
                    </span>
                    <div>
                        <h5 class="alert-heading">Opps...</h5>
                        <p>${res.message}</p>
                    </div>
                </div>
            </div>
            `;
        body.html(html);
    }
}
function unixTimestampToDate(unixTimestamp) {
    var date = new Date(unixTimestamp * 1000);
    var month = ("0" + (date.getMonth() + 1)).slice(-2);
    var day = ("0" + date.getDate()).slice(-2);
    var year = date.getFullYear();
    var formattedDate = day + "-" + month + "-" + year;
    return formattedDate;
}
