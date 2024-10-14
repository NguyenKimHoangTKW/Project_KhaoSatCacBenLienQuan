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
        await LoadKetQua();
    }
    finally {
        EndLoading()
    }
});

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
$(document).on('change', '#Year', async function () {
    Loading()
    try {
        await LoadKetQua();
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

    let SurveyYear = "NĂM HỌC: " + $("#Year option:selected").text().toUpperCase();
    worksheet.addRow([SurveyYear]);
    let lastRowYear = worksheet.lastRow.number;
    worksheet.mergeCells(`A${lastRowYear}:F${lastRowYear}`);
    let mergedCellYear = worksheet.getCell(`A${lastRowYear}`);
    mergedCellYear.font = { bold: true, size: 14 };
    mergedCellYear.alignment = { horizontal: 'center', vertical: 'middle' };

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
    var Year = $('#Year').val();
    try {
        const res = await $.ajax({
            url: '/CTDT/BaoCaoTongHopKetQuaKhaoSat/x_loadbaocaotonghop',
            type: 'POST',
            data: { id: Year }
        });

        let body = $('#showdata');
        let thead = $('#showthead');
        body.empty();
        thead.empty();
        let html = ``;

        if (res && res.data && res.data.Survey.length > 0) {
            let title = `
                <tr>
                    <th scope="col">STT</th>
                    <th scope="col">Phiếu khảo sát</th>
                    <th scope="col">Học kỳ</th>
                    <th scope="col">Tỷ lệ tham gia khảo sát</th>
                    <th scope="col">Mức độ hài lòng</th>
                    <th scope="col">Điểm trung bình</th>
                </tr>
            `;
            thead.html(title);

            res.data.Survey.forEach(function (survey, index) {
                html += `<tr>`;
                html += `<td class="formatSo">${index + 1}</td>`;
                html += `<td>${survey.ten_phieu}</td>`;
                html += `<td>${survey.hoc_ky || ''}</td>`;

                const Percent = res.data.PercentageSurvey.find(chil => chil.ma_phieu == survey.ma_phieu);
                html += `<td class="formatSo">${Percent ? Percent.ty_le : 0}%</td>`;

                const Satisfaction = res.data.SatisfactionLevel.find(chil => chil.ma_phieu == survey.ma_phieu);
                if (Satisfaction) {
                    html += `<td class="formatSo">${Satisfaction.ty_le_hai_long}%</td>`;
                    html += `<td class="formatSo">${Satisfaction.avgscore}</td>`;
                } else {
                    html += `<td class="formatSo">0%</td>`;
                    html += `<td class="formatSo">0</td>`;
                }

                html += `</tr>`;
            });

            body.html(html);
        } else {
            html = `
                <div class="alert alert-info" style="text-align: center;">
                  <strong>Không có dữ liệu báo cáo tổng hợp cho năm học này</strong>
                </div>
            `;
            body.html(html);
        }
    } catch (error) {
        console.error('An error occurred:', error);
        body.html(`
            <div class="alert alert-danger" style="text-align: center;">
              <strong>Đã xảy ra lỗi khi tải dữ liệu.</strong>
            </div>
        `);
    }
}
