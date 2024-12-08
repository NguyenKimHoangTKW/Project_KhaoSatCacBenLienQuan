$(document).ready(function () {
    thongke();
    $("#exportExcel").click(function () {
        ExportExcelBaoCaoTongHop();
    })
});
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
function ExportExcelBaoCaoTongHop() {
    let workbook = new ExcelJS.Workbook();

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

    let TimeExport = "Thời gian xuất kết quả : " + LayThoiGian();

    workbook.addWorksheet('Thông tin xuất').addRow([TimeExport]);

    $(".datatable").each(function () {
        let tableId = $(this).attr('id');
        let table = $(this);

        const subjectHeading = table.closest(".mt-4").find("h4").text().trim();

        if (subjectHeading) {
            let worksheetName = subjectHeading.replace(/[*?:"\/\\[\]]/g, ''); 
            worksheetName = worksheetName.length > 31 ? worksheetName.substring(0, 31) : worksheetName; 

            let worksheet = workbook.addWorksheet(worksheetName);

            worksheet.addRow([subjectHeading]);
            let lastRow = worksheet.lastRow.number;
            worksheet.mergeCells(`A${lastRow}:B${lastRow}`);  
            let mergedCell = worksheet.getCell(`A${lastRow}`);
            mergedCell.font = { bold: true, size: 12 };
            mergedCell.alignment = { horizontal: 'center', vertical: 'middle' };
            worksheet.addRow([]);

            let headers = [];
            table.find('thead th').each(function () {
                headers.push($(this).text());
            });

            let headerRow = worksheet.addRow(headers);
            headerRow.eachCell((cell) => {
                cell.style = headerStyle;
            });

            table.find('tbody tr').each(function () {
                let rowData = [];
                $(this).find('td').each(function () {
                    rowData.push($(this).text().trim());
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

    workbook.eachSheet(function (worksheet) {
        worksheet.columns.forEach(column => {
            column.width = 40;
        });
    });

    workbook.xlsx.writeBuffer().then(function (buffer) {
        const dateTime = getFormattedDateTime();
        const filename = `Thống kê chi tiết khảo sát_${dateTime}.xlsx`;
        saveAs(new Blob([buffer], { type: "application/octet-stream" }), filename);
    });
}

async function thongke() {
    var id = $("#surveyid").text().trim();
    const res = await $.ajax({
        url: '/api/thong-ke-chi-tiet-khao-sat',
        type: 'POST',
        data: JSON.stringify({ surveyID: id }),
        contentType: 'application/json',
    });

    const tittle = $("#title-survey");
    const body = $(".table-responsive");
    let html = "";
    tittle.empty();
    body.empty();
    tittle.html(res.phieu);

    res.data.forEach(function (item, index) {
        if (item.success) {
            if (item.is_cbvc) {
                html += interface_cbvc(index, item.data);
            }
            else if (item.is_gv) {
                html += interface_gv(index, item.data);
            }
            else if (item.is_nhmh) {
                html += interface_hoc_phan(index, item.ten_mon_hoc, item.thong_tin);
            }
        }
        else {
            html = `
                <div class="alert alert-info">
                <div class="d-flex justify-content-start">
                    <span class="alert-icon m-r-20 font-size-30">
                        <i class="anticon anticon-close-circle"></i>
                    </span>
                    <div>
                        <h5 class="alert-heading">Opps...</h5>
                        <p>${item.message}</p>
                    </div>
                </div>
            </div>
            `;
        }
    });

    body.html(html);

    res.data.forEach(function (item, index) {
        if (item.data && item.data.length > 0) {
            const tableId = `datatable-${index}`;
            initializeDataTable(tableId);
        }
    });
}

function interface_hoc_phan(index, tenMonHoc, thongtin) {
    $("#exportExcel").show();
    let rows = "";

    thongtin.forEach((student, i) => {
        rows += `  
            <tr>
                <td>${i + 1}</td>
                <td>${student.TenCBVC}</td>
                <td>${student.hovaten}</td>
                <td>${student.ma_sv}</td>
                <td>${student.DaKhaoSat}</td>
            </tr>
        `;
    });

    const tableId = `datatable-${index}`;

    const tableHTML = `
        <div class="mt-4">
            <h4><strong>Môn học: ${tenMonHoc}</strong></h4>
            <table id="${tableId}" class="datatable table table-bordered">
                <thead style="color:black; text-align:center; font-weight:bold">
                    <tr>
                        <th>STT</th>
                        <th>Giảng viên</th>
                        <th>Họ và tên</th>
                        <th>Mã sinh viên</th>
                        <th>Tình trạng khảo sát</th>
                    </tr>
                </thead>
                <tbody>
                    ${rows}
                </tbody>
            </table>
        </div>
    `;

    return tableHTML;
}


function interface_gv(index, lecturers) {
    let rows = "";

    lecturers.forEach((lecturer, i) => {
        rows += `
            <tr>
                <td>${i + 1}</td>
                <td>${lecturer.ma_giang_vien}</td>
                <td>${lecturer.ten_giang_vien}</td>
                <td>${lecturer.ctdt_khao_sat}</td>
            </tr>
        `;
    });

    const tableId = `datatable-${index}`;

    const tableHTML = `
        <table id="${tableId}" class="datatable table table-bordered">
            <thead style="color:black; text-align:center; font-weight:bold">
                <tr>
                    <th scope="col">STT</th>
                    <th scope="col">Mã giảng viên</th>
                    <th scope="col">Tên giảng viên</th>
                    <th scope="col">CTĐT</th>
                </tr>
            </thead>
            <tbody>
                ${rows}
            </tbody>
        </table>
    `;

    return tableHTML;
}

function interface_cbvc(index, lecturers) {
    let rows = "";

    lecturers.forEach((lecturer, i) => {
        rows += `
            <tr>
                <td>${i + 1}</td>
                <td>${lecturer.ma_cbvc}</td>
                <td>${lecturer.ten_cbvc}</td>
                <td>${lecturer.thuoc_ctdt}</td>
                <td>${lecturer.DaKhaoSat}</td>
            </tr>
        `;
    });

    const tableId = `datatable-${index}`;

    const tableHTML = `
        <table id="${tableId}" class="datatable table table-bordered">
            <thead style="color:black; text-align:center; font-weight:bold">
                <tr>
                    <th scope="col">STT</th>
                    <th scope="col">Mã giảng viên</th>
                    <th scope="col">Tên giảng viên</th>
                    <th scope="col">CTĐT</th>
                    <th scope="col">Trạng thái</th>
                </tr>
            </thead>
            <tbody>
                ${rows}
            </tbody>
        </table>
    `;

    return tableHTML;
}
function initializeDataTable(tableId) {
    $(`#${tableId}`).DataTable({
        pageLength: 10,
        lengthMenu: [5, 10, 25, 50, 100],
        ordering: true,
        searching: true,
        autoWidth: false,
        responsive: true,
        language: {
            paginate: {
                next: "Next",
                previous: "Previous"
            },
            search: "Search",
            lengthMenu: "Show _MENU_ entries"
        },
        dom: "Bfrtip",
        buttons: [
            {
                extend: 'csv',
                title: 'Danh sách chi tiết thống kê - CSV'
            },
            {
                extend: 'excel',
                title: 'Danh sách chi tiết thống kê - Excel'
            },
            {
                extend: 'pdf',
                title: 'Danh sách chi tiết thống kê PDF'
            },
            {
                extend: 'print',
                title: 'Danh sách chi tiết thống kê'
            }
        ]
    });
}
