$(document).ready(function () {
    load_bao_cao();

    $('#ExportExcel').click(function () {
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
    });
});
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

    // Styles
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
        alignment: { horizontal: 'center', vertical: 'middle' }
    };

    let TitileBaoCao = "BÁO CÁO TỔNG HỢP HỆ: " + $("#hedaotao option:selected").text().toUpperCase();
    worksheet.addRow([TitileBaoCao]);
    let lastRowTitle = worksheet.lastRow.number;
    worksheet.mergeCells(`A${lastRowTitle}:F${lastRowTitle}`);
    let mergedCellTitle = worksheet.getCell(`A${lastRowTitle}`);
    mergedCellTitle.font = { bold: true, size: 14 };
    mergedCellTitle.alignment = { horizontal: 'center', vertical: 'middle' };

    // Add Survey Year
    let SurveyYear = "NĂM HỌC: " + $("#year option:selected").text().toUpperCase();
    worksheet.addRow([SurveyYear]);
    let lastRowYear = worksheet.lastRow.number;
    worksheet.mergeCells(`A${lastRowYear}:F${lastRowYear}`);
    let mergedCellYear = worksheet.getCell(`A${lastRowYear}`);
    mergedCellYear.font = { bold: true, size: 14 };
    mergedCellYear.alignment = { horizontal: 'center', vertical: 'middle' };

    // Add Export Time
    let TimeExport = "Thời gian xuất kết quả : " + LayThoiGian();
    worksheet.addRow([TimeExport]);
    let lastRowTime = worksheet.lastRow.number;
    worksheet.mergeCells(`A${lastRowTime}:B${lastRowTime}`);
    let mergedCellTime = worksheet.getCell(`A${lastRowTime}`);
    mergedCellTime.font = { bold: true, size: 14 };
    mergedCellTime.alignment = { horizontal: 'center', vertical: 'middle' };
    worksheet.addRow([]);

    let spans = document.querySelectorAll('#accordion-default .card-header span');
    spans.forEach(span => {
        let surveyTitle = span.textContent;
        worksheet.addRow([surveyTitle]);
        let lastRowTitle = worksheet.lastRow.number;
        worksheet.mergeCells(`A${lastRowTitle}:F${lastRowTitle}`);
        let mergedCellTitle = worksheet.getCell(`A${lastRowTitle}`);
        mergedCellTitle.font = { bold: true, size: 14 };
        mergedCellTitle.alignment = { horizontal: 'center', vertical: 'middle' };

        let table = span.closest('.card').querySelector('table');
        if (table) {
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

            worksheet.addRow([]); 
        }
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


function load_bao_cao() {
    Swal.fire({
        allowOutsideClick: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });
    var namhoc = $('#year').val();
    var hedaotao = $('#hedaotao').val();
    $.ajax({
        url: '/Admin/BaoCaoTongHop/load_bao_cao',
        type: 'POST',
        data: {
            year: namhoc,
            hedaotao: hedaotao
        },
        success: function (res) {
            let body = $('#accordion-default');
            let html = '';
            body.empty();
            if (res.success && res && res.data && res.data.phieu_khao_sat.length > 0) {
                res.data.phieu_khao_sat.forEach(function (phieu, index) {
                    const ctdt = res.data.chuong_trinh_dao_tao.find(ctdtGroup => {
                        return ctdtGroup.some(ctdtItem => ctdtItem.ma_phieu === phieu.ma_phieu);
                    });

                    let collapseId = `collapse${index}`;
                    html += `
                    <div class="card">
                        <div class="card-header">
                            <h5 class="card-title">
                                <a data-toggle="collapse" href="#${collapseId}">
                                    <span>${phieu.ten_phieu}</span>
                                </a>
                            </h5>
                        </div>
                        <div id="${collapseId}" class="collapse" data-parent="#accordion-default">
                            <div class="card-body">
                                <table class="table table-bordered">
                                <thead style="color:black; text-align:center; font-weight:bold">
                                    <tr>
                                        <th scope="col">STT</th>
                                        <th scope="col">Chương trình đào tạo</th>
                                        <th scope="col">Tỷ lệ tham gia khảo sát</th>
                                        <th scope="col">Mức độ hài lòng</th>
                                        <th scope="col">Điểm trung bình</th>
                                        <th scope="col">Kết Quả</th>
                                    </tr>
                                </thead>
                                <tbody id="showdata">
                                    `;
                    ctdt.forEach(function (chuongtrinhdaotao, ctdtIndex) {
                        const tylethamgiakhaosat = res.data.ty_le_tham_gia_khao_sat.find(khao_sat =>
                            khao_sat.ma_phieu === chuongtrinhdaotao.ma_phieu &&
                            khao_sat.ma_ctdt === chuongtrinhdaotao.ma_ctdt
                        );
                        const mucdohailong = res.data.muc_do_hai_long.find(khao_sat =>
                            khao_sat.ma_phieu === chuongtrinhdaotao.ma_phieu &&
                            khao_sat.ma_ctdt === chuongtrinhdaotao.ma_ctdt
                        );
                        const tyLeDaTraLoi = tylethamgiakhaosat ? tylethamgiakhaosat.ty_le_da_tra_loi : '0';
                        const tylehailong = mucdohailong ? mucdohailong.ty_le_hai_long : '0';
                        const diemtrungbinh = mucdohailong ? mucdohailong.avgscore : '0';
                        let ketqua = '';
                        if (tyLeDaTraLoi >= 70 && tylehailong >= 70) {
                            ketqua = 'Đạt';
                        }
                        else {
                            ketqua = 'Chưa Đạt';
                        }
                        html += `
                        <tr>
                            <td scope="col" class="formatSo">${ctdtIndex + 1}</td>
                            <td scope="col">${chuongtrinhdaotao.ten_ctdt}</td>
                            <td scope="col" class="formatSo">${tyLeDaTraLoi}%</td>
                            <td scope="col" class="formatSo">${tylehailong}%</td>
                            <td scope="col" class="formatSo">${diemtrungbinh}</td>
                            <td scope="col" class="formatSo">${ketqua}</td>
                        </tr>`;
                    });

                    html += `
                                </tbody>
                            </table>
                            </div>
                        </div>
                    </div>`;
                });

                body.html(html);
            }
            else {
                html = `<div class="alert alert-info" style="text-align: center; font-weight: bold;">
                            Không có dữ liệu báo cáo tổng hợp
                        </div>
                        `;
                body.html(html);
            }
        },
        complete: function () {
            Swal.close();
        }
    });
}

$(document).on('change', '#hedaotao', function () {
    load_bao_cao();
});

$(document).on('change', '#year', function () {
    load_bao_cao();
});