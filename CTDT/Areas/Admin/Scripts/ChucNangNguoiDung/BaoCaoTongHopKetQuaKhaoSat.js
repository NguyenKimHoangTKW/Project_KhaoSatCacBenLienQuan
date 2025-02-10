$(".select2").select2();
$(document).ready(function () {
    $("#hedaotao, #year").on("change", load_pks_by_nam);
    $("#fildata").click(function () {
        load_bao_cao();
    })
    $('#exportExcel').click(function () {
        ExportExcelBaoCaoTongHop();
    })
});

async function load_pks_by_nam() {
    const hedaotao = $("#hedaotao").val();
    const year = $("#year").val();
    const res = await $.ajax({
        url: '/api/load_phieu_by_nam',
        type: 'POST',
        data: {
            id_namhoc: year,
            id_hedaotao: hedaotao
        }
    });
    let html = "";
    let html_ctdt = `<option value="">Tất cả</option>`;

    if (res.success) {

        res.ctdt.forEach(function (ctdt) {
            html_ctdt += `<option value="${ctdt.id_ctdt}">${ctdt.ten_ctdt}</option>`;
        });
        $("#ctdt").empty().html(html_ctdt).trigger("change");
    } else {
        html += `<option value="">${res.message}</option>`;
        $("#surveyid").empty().html(html).trigger("change");
        $("#ctdt").empty().html(html).trigger("change");
    }
}

function load_bao_cao() {
    Swal.fire({
        allowOutsideClick: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });

    const namhoc = $('#year').val();
    const hedaotao = $('#hedaotao').val();
    const ctdt = $('#ctdt').val();
    $.ajax({
        url: '/api/admin/bao-cao-tong-hop-ket-qua-khao-sat',
        type: 'POST',
        data: {
            id_namhoc: namhoc,
            id_hdt: hedaotao,
            id_ctdt: ctdt
        },
        success: function (res) {
            let body = $('#accordion-default');
            let html = '';
            body.empty();

            if (res.success) {
                res.data.forEach(function (item, index) {
                    if (item.get_all) {
                        let collapseId = `collapse${index}`;
                        html += `
                    <div class="card">
                        <div class="card-header">
                            <h5 class="card-title">
                                <a data-toggle="collapse" href="#${collapseId}">
                                    <span>${item.phieu}</span>
                                </a>
                            </h5>
                        </div>
                        <div id="${collapseId}" class="collapse" data-parent="#accordion-default">
                            <div class="card-body">
                                <table class="table table-bordered">
                                    <thead style="color:black; text-align:center; font-weight:bold">
                                        <tr>
                                            <th scope="col">Chương trình đào tạo</th>
                                            <th scope="col">Tổng khảo sát</th>
                                            <th scope="col">Tổng phiếu đã thu về</th>
                                            <th scope="col">Tổng phiếu chưa thu về</th>
                                            <th scope="col">Tỷ lệ phiếu đã thu</th>
                                            <th scope="col">Tỷ lệ phiếu chưa thu</th>
                                            <th scope="col">Mức độ hài lòng</th>
                                            <th scope="col">Điểm trung bình</th>
                                        </tr>
                                    </thead>
                                    <tbody>`;

                        item.ty_le_tham_gia_khao_sat.forEach(function (tylethamgia) {
                            let avg_ty_le_hai_long = tylethamgia.muc_do_hai_long[0]?.avg_ty_le_hai_long || 0.0;
                            let avg_score = tylethamgia.muc_do_hai_long[0]?.avg_score || 0.0;

                            html += `
                            <tr>
                                <td>${tylethamgia.ctdt}</td>
                                <td class="formatSo">${tylethamgia.tong_khao_sat}</td>
                                <td class="formatSo">${tylethamgia.tong_phieu_da_tra_loi}</td>
                                <td class="formatSo">${tylethamgia.tong_phieu_chua_tra_loi}</td>
                                <td class="formatSo">${tylethamgia.ty_le_da_tra_loi}%</td>
                                <td class="formatSo">${tylethamgia.ty_le_chua_tra_loi}%</td>
                                <td class="formatSo">${avg_ty_le_hai_long}%</td>
                                <td class="formatSo">${avg_score}</td>
                            </tr>`;
                        });

                        html += `</tbody>
                                </table>
                            </div>
                        </div>
                    </div>`;
                    }
                    else if (item.get_only) {
                        html +=
                            `<div class="card-body">
                                <table class="table table-bordered">
                                    <thead style="color:black; text-align:center; font-weight:bold">
                                        <tr>
                                            <th scope="col">Phiếu khảo sát</th>
                                            <th scope="col">Tổng khảo sát</th>
                                            <th scope="col">Tổng phiếu đã thu về</th>
                                            <th scope="col">Tổng phiếu chưa thu về</th>
                                            <th scope="col">Tỷ lệ phiếu đã thu</th>
                                            <th scope="col">Tỷ lệ phiếu chưa thu</th>
                                            <th scope="col">Mức độ hài lòng</th>
                                            <th scope="col">Điểm trung bình</th>
                                        </tr>
                                    </thead>
                                    <tbody>`;

                        item.ty_le_tham_gia_khao_sat.forEach(function (tylethamgia) {
                            let avg_ty_le_hai_long = tylethamgia.muc_do_hai_long[0]?.avg_ty_le_hai_long || 0.0;
                            let avg_score = tylethamgia.muc_do_hai_long[0]?.avg_score || 0.0;

                            html += `
                            <tr>
                                <td>${item.phieu}</td>
                                <td class="formatSo">${tylethamgia.tong_khao_sat}</td>
                                <td class="formatSo">${tylethamgia.tong_phieu_da_tra_loi}</td>
                                <td class="formatSo">${tylethamgia.tong_phieu_chua_tra_loi}</td>
                                <td class="formatSo">${tylethamgia.ty_le_da_tra_loi}%</td>
                                <td class="formatSo">${tylethamgia.ty_le_chua_tra_loi}%</td>
                                <td class="formatSo">${avg_ty_le_hai_long}%</td>
                                <td class="formatSo">${avg_score}</td>
                            </tr>`;
                        });

                        html += `</tbody>
                                </table>
                            </div>
                            `;
                    }
                });

                body.html(html);
            } else {
                html = `<div class="alert alert-info" style="text-align: center; font-weight: bold;">
                            Không có dữ liệu báo cáo tổng hợp
                        </div>`;
                body.html(html);
            }
        },
        complete: function () {
            Swal.close();
        }
    });
}


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