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
function Toast_alert(type, message) {
    const Toast = Swal.mixin({
        toast: true,
        position: "top-end",
        showConfirmButton: false,
        timer: 2000,
        timerProgressBar: true,
        didOpen: (toast) => {
            toast.onmouseenter = Swal.stopTimer;
            toast.onmouseleave = Swal.resumeTimer;
        }
    });
    Toast.fire({
        icon: type,
        title: message
    });
}
function hideLoading() {
    Swal.close();
}
var initializing = true;
$(document).ready(function () {
    var defaultYearId = $("#year").val();
    LoadSurveys(defaultYearId);
    $("#year").change(function () {
        var selectedYearId = $(this).val();
        LoadSurveys(selectedYearId);
    });
});


$(document).on('click', '#fildata', async function () {
    var id = $("#surveyid").val();
    var hocky = $("#hocky_fil").val();
    var check_hocky = $('#hoc_ky');
    if (check_hocky.is(":visible")) {
        if (hocky.trim() === "") {
            var type = "error"
            var message = "Vui lòng chọn học kỳ"
            Toast_alert(type, message)
            return;
        }
    }

    var body = $('#tan_xuat_table');
    if (id) {
        showLoading();
        try {
            body.show();
            await loc_tan_xuat(id, hocky);
            hideLoading();
            var type = "success"
            var message = "Lọc dữ liệu thành công"
            Toast_alert(type, message)
        } catch (error) {
            body.hide();
            var type = "error"
            var message = "Không tìm thấy dữ liệu để thống kê"
            Toast_alert(type, message)
        }
    }
});

$(document).on("click", "#exportExcel", async function () {
    export_excel();
})

function export_excel() {
    if ($('#ThongKeTyLeSurvey').text().trim() === 'Không có dữ liệu') {
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
                ExportExcelKetQuaKhaoSat();
                var type = "success"
                var message = "Xuất dữ liệu thành công"
                Toast_alert(type, message)
            }
        });
    }
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
function ExportExcelKetQuaKhaoSat() {
    let workbook = new ExcelJS.Workbook();
    let worksheet = workbook.addWorksheet('Thống kê kết quả');
    let worksheetYkienKhac = workbook.addWorksheet('Ý kiến khác');
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

    let FooterStyle = {
        font: { name: 'Times New Roman', family: 4, size: 13, bold: true },
        border: {
            top: { style: 'thin' },
            left: { style: 'thin' },
            bottom: { style: 'thin' },
            right: { style: 'thin' }
        },
        alignment: { horizontal: 'center', vertical: 'middle' },
    };

    let SurveyTitle = $("#surveyid option:selected").text().toUpperCase();
    worksheet.addRow([SurveyTitle]);
    let lastRowNumber = worksheet.lastRow.number;
    worksheet.mergeCells(`A${lastRowNumber}:F${lastRowNumber}`);
    let mergedCell = worksheet.getCell(`A${lastRowNumber}`);
    mergedCell.font = { bold: true, size: 14 };
    mergedCell.alignment = { horizontal: 'center', vertical: 'middle' };


    let SurveyYear = "NĂM HỌC: " + $("#year option:selected").text().toUpperCase();
    worksheet.addRow([SurveyYear]);
    let lastRowYear = worksheet.lastRow.number;
    worksheet.mergeCells(`A${lastRowYear}:F${lastRowYear}`);
    let mergedCellYear = worksheet.getCell(`A${lastRowYear}`);
    mergedCellYear.font = { bold: true, size: 14 };
    mergedCellYear.alignment = { horizontal: 'center', vertical: 'middle' };

    let TimeExport = "Thời gian xuất dữ liệu : " + LayThoiGian();
    worksheet.addRow([TimeExport]);
    let lastRowTime = worksheet.lastRow.number;
    worksheet.mergeCells(`A${lastRowTime}:B${lastRowTime}`);
    let mergedCellTime = worksheet.getCell(`A${lastRowTime}`);
    mergedCellTime.font = { bold: true, size: 14 };
    mergedCellTime.alignment = { horizontal: 'center', vertical: 'middle' };
    worksheet.addRow([]);;


    $('#ThongKeTyLeSurvey').each(function () {
        let questionTitle = $(this).find('p').text().toUpperCase();
        let table = $(this).find('table');
        let thead = table.find('thead');
        let tbody = table.find('tbody');

        worksheet.addRow([questionTitle]);
        worksheet.mergeCells(`A${worksheet.lastRow.number}:F${worksheet.lastRow.number}`);
        worksheet.getCell(`A${worksheet.lastRow.number}`).font = { bold: true, size: 14 };
        worksheet.getCell(`A${worksheet.lastRow.number}`).alignment = { horizontal: 'center', vertical: 'middle' };

        let headers = [];
        $(thead[0]).find('th').each(function () {
            headers.push($(this).text());
        });
        let headerRow = worksheet.addRow(headers);
        headerRow.eachCell((cell) => {
            cell.style = headerStyle;
        });
        let bodys = [];
        $(tbody[0]).find('td').each(function () {
            bodys.push($(this).text());
        });
        let BodyRow = worksheet.addRow(bodys);
        BodyRow.eachCell((cell) => {
            cell.style = cellNumberStyle;
        });

        worksheet.columns.forEach(column => {
            column.width = 40;
        });

        worksheet.addRow([]);
    });
    $('#surveyContainerSingle .question-block').each(function () {
        let questionTitle = $(this).find('p').text().toUpperCase();
        let table = $(this).find('table');
        let thead = table.find('thead');
        let tbody = table.find('tbody');
        let tfoot = table.find('tfoot');

        worksheet.addRow([questionTitle]);
        worksheet.mergeCells(`A${worksheet.lastRow.number}:D${worksheet.lastRow.number}`);
        worksheet.getCell(`A${worksheet.lastRow.number}`).font = { bold: true, size: 14 };
        worksheet.getCell(`A${worksheet.lastRow.number}`).alignment = { horizontal: 'center', vertical: 'middle' };

        let headers = [];
        $(thead[0]).find('th').each(function () {
            headers.push($(this).text());
        });
        let headerRow = worksheet.addRow(headers);
        headerRow.eachCell((cell) => {
            cell.style = headerStyle;
        });
        let tbodyRows = tbody.find('tr');
        $(tbodyRows).each(function (index) {
            if (index >= 0) {
                let cells = [];
                $(this).find('td').each(function () {
                    cells.push($(this).text());
                });
                let dataRow = worksheet.addRow(cells);

                dataRow.eachCell({ includeEmpty: true }, (cell, colNumber) => {
                    if (cells[colNumber - 1].includes('%')) {
                        cell.style = cellNumberStyle;
                    } else if (!isNaN(cells[colNumber - 1]) && cells[colNumber - 1] !== '') {
                        cell.style = cellNumberStyle;
                    } else {
                        cell.style = cellStyle;
                    }
                });
            }
        });


        let footers = [];
        $(tfoot[0]).find('td').each(function () {
            footers.push($(this).text());
        });
        let footerRow = worksheet.addRow([footers[0], '', footers[1], footers[2]]);
        worksheet.mergeCells(`A${footerRow.number}:B${footerRow.number}`);
        footerRow.getCell(1).style = FooterStyle;
        footerRow.getCell(2).style = FooterStyle;
        footerRow.getCell(3).style = FooterStyle;
        footerRow.getCell(4).style = FooterStyle;

        worksheet.columns.forEach(column => {
            column.width = 40;
        });

        worksheet.addRow([]);
    });

    $('#surveyContainer .question-block').each(function () {
        let questionTitle = $(this).find('p').text().toUpperCase();
        let table = $(this).find('table');
        let thead = table.find('thead');
        let tbody = table.find('tbody');
        let tfoot = table.find('tfoot');

        worksheet.addRow([questionTitle]);
        worksheet.mergeCells(`A${worksheet.lastRow.number}:D${worksheet.lastRow.number}`);
        worksheet.getCell(`A${worksheet.lastRow.number}`).font = { bold: true, size: 14 };
        worksheet.getCell(`A${worksheet.lastRow.number}`).alignment = { horizontal: 'center', vertical: 'middle' };

        let headers = [];
        $(thead[0]).find('th').each(function () {
            headers.push($(this).text());
        });
        let headerRow = worksheet.addRow(headers);
        headerRow.eachCell((cell) => {
            cell.style = headerStyle;
        });
        let tbodyRows = tbody.find('tr');
        $(tbodyRows).each(function (index) {
            if (index >= 0) {
                let cells = [];
                $(this).find('td').each(function () {
                    cells.push($(this).text());
                });
                let dataRow = worksheet.addRow(cells);

                dataRow.eachCell({ includeEmpty: true }, (cell, colNumber) => {
                    if (cells[colNumber - 1].includes('%')) {
                        cell.style = cellNumberStyle;
                    } else if (!isNaN(cells[colNumber - 1]) && cells[colNumber - 1] !== '') {
                        cell.style = cellNumberStyle;
                    } else {
                        cell.style = cellStyle;
                    }
                });
            }
        });


        let footers = [];
        $(tfoot[0]).find('td').each(function () {
            footers.push($(this).text());
        });
        let footerRow = worksheet.addRow([footers[0], '', footers[1], footers[2]]);
        worksheet.mergeCells(`A${footerRow.number}:B${footerRow.number}`);
        footerRow.getCell(1).style = FooterStyle;
        footerRow.getCell(2).style = FooterStyle;
        footerRow.getCell(3).style = FooterStyle;
        footerRow.getCell(4).style = FooterStyle;


        worksheet.columns.forEach(column => {
            column.width = 40;
        });

        worksheet.addRow([]);
    });

    $('#Cauhoi5MucContainer').each(function () {
        let questionTitle = $(this).find('p').text().toUpperCase();
        let table = $(this).find('table');
        let thead = table.find('thead');
        let tbody = table.find('tbody');
        let tfoot = table.find('tfoot');
        let rows = table.find('tr');

        worksheet.addRow([questionTitle]);
        worksheet.mergeCells(`A${worksheet.lastRow.number}:D${worksheet.lastRow.number}`);
        worksheet.getCell(`A${worksheet.lastRow.number}`).font = { bold: true, size: 14 };
        worksheet.getCell(`A${worksheet.lastRow.number}`).alignment = { horizontal: 'center', vertical: 'middle' };

        let headers1 = [];
        $(thead[0]).find('tr:first th').each(function () {
            headers1.push($(this).text());
        });

        let adjustedHeaders = [...headers1.slice(0, 3), 'Tần số', '', '', '', '', 'Tỷ lệ phần trăm', '', '', '', '', ...headers1.slice(5)];
        let headerRow = worksheet.addRow(adjustedHeaders);
        headerRow.eachCell((cell) => {
            cell.style = headerStyle;
        });
        worksheet.mergeCells(`D${headerRow.number}:H${headerRow.number}`);
        worksheet.getCell(`D${headerRow.number}`).alignment = { horizontal: 'center' };

        worksheet.mergeCells(`I${headerRow.number}:M${headerRow.number}`);
        worksheet.getCell(`I${headerRow.number}`).alignment = { horizontal: 'center' };
        //////////////////////////////////////////////////////////////////////////////////
        let headers2 = [];
        $(thead[0]).find('tr').eq(1).find('th').each(function () {
            headers2.push($(this).text());
        });

        let adjustedHeaders2 = ['', '', '', ...headers2, ''];

        let headerRow2 = worksheet.addRow(adjustedHeaders2);
        headerRow2.eachCell((cell) => {
            cell.style = headerStyle;
        });



        let tbodyRows = tbody.find('tr');
        $(tbodyRows).each(function (index) {
            if (index >= 0) {
                let cells = [];
                $(this).find('td').each(function () {
                    cells.push($(this).text());
                });
                let dataRow = worksheet.addRow(cells);

                dataRow.eachCell({ includeEmpty: true }, (cell, colNumber) => {
                    if (cells[colNumber - 1].includes('%')) {
                        cell.style = cellNumberStyle;
                    } else if (!isNaN(cells[colNumber - 1]) && cells[colNumber - 1] !== '') {
                        cell.style = cellNumberStyle;
                    } else {
                        cell.style = cellStyle;
                    }
                });
            }
        });

        let footers = [];
        $(tfoot[0]).find('td').each(function () {
            footers.push($(this).text());
        });

        let footerRow = worksheet.addRow([footers[0], '', ...footers.slice(1)]);

        worksheet.mergeCells(`A${footerRow.number}:B${footerRow.number}`);

        footerRow.eachCell({ includeEmpty: true }, (cell) => {
            cell.style = FooterStyle;
        });



        worksheet.columns.forEach(column => {
            column.width = 40;
        });
        worksheet.addRow([]);
    });
    $('#YkienkhacSurvey').each(function () {
        let questionTitle = $(this).find('p').eq(0).text().toUpperCase();
        let table = $(this).find('table');
        let thead = table.find('thead');
        let tbody = table.find('tbody');

        worksheetYkienKhac.addRow([questionTitle]);
        worksheetYkienKhac.mergeCells(`A${worksheetYkienKhac.lastRow.number}:D${worksheetYkienKhac.lastRow.number}`);
        worksheetYkienKhac.getCell(`A${worksheetYkienKhac.lastRow.number}`).font = { bold: true, size: 14 };
        worksheetYkienKhac.getCell(`A${worksheetYkienKhac.lastRow.number}`).alignment = { horizontal: 'center', vertical: 'middle' };

        let headers = [];
        $(thead[0]).find('th').each(function () {
            headers.push($(this).text());
        });
        let headerRow = worksheetYkienKhac.addRow(headers);
        headerRow.eachCell((cell) => {
            cell.style = headerStyle;
        });

        let responsesByRow = [];
        $(tbody[0]).find('tr').each(function () {
            let row = [];
            $(this).find('td').each(function () {
                let cellResponses = [];
                $(this).find('p').each(function () {
                    let text = $(this).text().trim();
                    if (text) cellResponses.push(text);
                });
                row.push(cellResponses);
            });
            if (row.some(cell => cell.length > 0)) {
                responsesByRow.push(row);
            }
        });

        let maxResponses = Math.max(...responsesByRow.map(row => Math.max(...row.map(cell => cell.length))));

        for (let i = 0; i < maxResponses; i++) {
            let row = [];
            responsesByRow.forEach(cellResponses => {
                cellResponses.forEach(responses => {
                    row.push(responses[i] || "");
                });
            });
            if (row.some(cell => cell !== "")) {
                let BodyRow = worksheetYkienKhac.addRow(row);
                BodyRow.eachCell((cell) => {
                    cell.style = cellNumberStyle;
                });
            }
        }

        worksheetYkienKhac.columns.forEach(column => {
            column.width = 150;
        });
    });
    workbook.xlsx.writeBuffer().then(function (buffer) {
        const dateTime = getFormattedDateTime();
        const filename = `Kết quả khảo sát_${dateTime}.xlsx`;
        saveAs(new Blob([buffer], { type: "application/octet-stream" }), filename);
    });
}
function LoadSurveys(nameyear) {
    $.ajax({
        url: "/api/ctdt/load_pks_by_year",
        type: "POST",
        data: { ten_namhoc: nameyear },
        success: function (res) {
            $("#surveyid").empty();
            if (res.success) {
                var items = res.data;
                if (items.length === 0) {
                    var html = '<option value="">Không có dữ liệu phiếu khảo sát cho năm học này</option>';
                    $("#surveyid").append(html);
                    $("#surveyid").val('').trigger('change');
                } else {
                    items.forEach(function (Chil) {
                        var html = `<option value="${Chil.IDSurvey}" data-is-hoc-ky="${Chil.is_hoc_ky}">${Chil.NameSurvey}</option>`;
                        $("#surveyid").append(html);
                    });
                    $("#surveyid").off('change').on('change', function () {
                        var selectedOption = $(this).find(':selected');
                        var isHocKy = selectedOption.data('is-hoc-ky');
                        if (isHocKy) {
                            $('#hoc_ky').show();
                        } else {
                            $('#hoc_ky').hide();
                        }
                    });

                    if (!initializing) {
                        $("#surveyid").val(items[0].IDSurvey).trigger('change');
                    } else {
                        $("#surveyid").val(items[0].IDSurvey);
                    }
                }
            } else {
                var html = '<option value="">Error loading surveys</option>';
                $("#surveyid").append(html);
            }
        }
    });
}
$(window).on('load', function () {
    initializing = false;
});
// Load Câu hỏi
async function loc_tan_xuat(tenphieu, hocky) {
    const res = await $.ajax({
        url: '/api/ctdt/load_tan_xuat_dap_an',
        type: "POST",
        data: {
            id_survey: tenphieu,
            name_hoc_ky: hocky
        }
    });
    await form_ty_le(res.rate);
    await form_cau_hoi_1_lua_chon(res.single_levels);
    await form_cau_hoi_nhieu_lua_chon(res.many_leves);
    await form_cau_hoi_5_muc(res.five_levels);
    await form_y_kien_khac(res.other_levels);
}

function form_ty_le(ty_le) {
    if (ty_le) {
        let container = $("#ThongKeTyLeSurvey");
        let html = "";
        container.empty();
        html = `
        <p style="font-weight:bold;font-size:15px;text-align:center;color:black">THỐNG KÊ SỐ LƯỢNG THAM GIA KHẢO SÁT</p>
        <div class="question-block">
            <p style="font-size: 20px; font-weight: bold; color: black;"></p>
            <div class="table-responsive">
                <table class="table table-bordered">
                    <thead style="color:black; text-align:center; font-weight:bold;font-size:15px">
                        <tr>
                            <th scope="col">Tên CTĐT</th>
                            <th scope="col">Số phiếu phát ra</th>
                            <th scope="col">Số phiếu thu về</th>
                            <th scope="col">Số phiếu chưa trả lời</th>
                            <th scope="col">Tỷ lệ phiếu thu về</th>
                            <th scope="col">Tỷ lệ phiếu chưa khảo sát</th>
                        </tr>
                    </thead>
                    <tbody>
    `;

        ty_le.forEach(item => {
            html += `
            <tr>
                <td>${item.CTDT}</td>
                <td class="formatSo">${item.TongKhaoSat}</td>
                <td class="formatSo">${item.TongPhieuDaTraLoi}</td>
                <td class="formatSo">${item.TongPhieuChuaTraLoi}</td>
                <td class="formatSo">${item.TyLeDaTraLoi}%</td>
                <td class="formatSo">${item.TyLeChuaTraLoi}%</td>
            </tr>
        `;
        });

        html += `
                    </tbody>
                </table>
            </div>
        </div>`;
        container.append(html);
        $("#accordion-default").show();
    }
    else {
        container.empty();
    }
}
function form_cau_hoi_1_lua_chon(ty_le) {
    if (ty_le) {
        let container = $("#surveyContainerSingle");
        container.empty();
        ty_le.forEach(function (item, questionIndex) {
            let questionTitle = item.QuestionTitle;
            let questionHtml = `
                    <div class="question-block">
                        <p style="font-size: 20px; font-weight: bold; color: black;">${questionTitle}</p>
                        <div class="table-responsive">
                            <table class="table table-bordered">
                                <thead style="color:black; text-align:center; font-weight:bold;font-size:15px">
                                    <tr>
                                        <th scope="col">STT</th>
                                        <th scope="col">Đáp án</th>
                                        <th scope="col">Tần số</th>
                                        <th scope="col">Tỷ lệ (%)</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    ${item.Choices.map((choice, index) => `
                                        <tr>
                                            <td class="formatSo">${index + 1}</td>
                                            <td>${choice.ChoiceText}</td>
                                            <td class="formatSo">${choice.Count}</td>
                                            <td class="formatSo">${choice.Percentage.toFixed(2)}%</td>
                                        </tr>
                                    `).join('')}
                                </tbody>
                                <tfoot style="color:black;font-weight:bold;font-size:15px">
                                    <tr>
                                        <td colspan="2">Tổng</td>
                                        <td class="formatSo">${item.TotalResponses}</td>
                                        <td class="formatSo">100%</td>
                                    </tr>
                                </tfoot>
                            </table>
                        </div>
                    </div>
                    <hr />
                `;
            container.append(questionHtml);
        });
    }
    else {
        container.empty();
    }
}
function form_cau_hoi_nhieu_lua_chon(ty_le) {
    if (ty_le) {
        let container = $("#surveyContainer");
        container.empty();
        ty_le.forEach(function (item, questionIndex) {
            let questionTitle = item.QuestionTitle;
            let questionHtml = `
                        <div class="question-block">
                            <p style="font-size: 20px; font-weight: bold; color: black;" data-question-title="${questionTitle}">${questionTitle}</p>
                            <div class="table-responsive">
                                <table class="table table-bordered">
                                    <thead style="color:black; text-align:center; font-weight:bold;font-size:15px">
                                        <tr>
                                            <th scope="col">STT</th>
                                            <th scope="col">Đáp án</th>
                                            <th scope="col">Tần số</th>
                                            <th scope="col">Tỷ lệ (%)</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        ${item.Choices.map((choice, index) => `
                                            <tr>
                                                <td class="formatSo" >${index + 1}</td>
                                                <td>${choice.ChoiceText}</td>
                                                <td class="formatSo">${choice.Count}</td>
                                                <td class="formatSo">${choice.Percentage.toFixed(2)}%</td>
                                            </tr>
                                        `).join('')}
                                    </tbody>
                                    <tfoot style="color:black;font-weight:bold;font-size:15px">
                                        <tr>
                                            <td colspan="2">Tổng</td>
                                            <td class="formatSo">${item.TotalResponses}</td>
                                            <td class="formatSo">100%</td>
                                        </tr>
                                    </tfoot>
                                </table>
                            </div>
                        </div>
                        <hr />
                    `;
            container.append(questionHtml);
        });
    }
    else {
        container.empty();
    }
}
function form_cau_hoi_5_muc(ty_le) {
    if (ty_le) {
        const tbody = $('#showdata');
        tbody.empty();
        const thead = $("#showhead");
        let html = "";
        let totalResponses = 0;
        let totalStronglyDisagree = 0;
        let totalDisagree = 0;
        let totalNeutral = 0;
        let totalAgree = 0;
        let totalStronglyAgree = 0;
        let totalScore = 0;
        html = `
                <tr>
                    <th rowspan="2">STT</th>
                    <th rowspan="2">Nội dung</th>
                    <th rowspan="2">Tổng số phiếu</th>
                    <th colspan="5">Tần số</th>
                    <th colspan="5">Tỷ lệ phần trăm</th>
                    <th rowspan="2">Điểm trung bình</th>
                </tr>
                <tr>
                    <th>Hoàn toàn không đồng ý</th>
                    <th>Không đồng ý</th>
                    <th>Bình thường</th>
                    <th>Đồng ý</th>
                    <th>Hoàn toàn đồng ý</th>
                    <th>Hoàn toàn không đồng ý</th>
                    <th>Không đồng ý</th>
                    <th>Bình thường</th>
                    <th>Đồng ý</th>
                    <th>Hoàn toàn đồng ý</th>
                </tr>
            `;
        thead.html(html);

        html = `
                <tr>
                    <td colspan="2">Tổng</td>
                    <td class="formatSo" id="totalResponses"></td>
                    <td class="formatSo" id="totalStronglyDisagree"></td>
                    <td class="formatSo" id="totalDisagree"></td>
                    <td class="formatSo" id="totalNeutral"></td>
                    <td class="formatSo" id="totalAgree"></td>
                    <td class="formatSo" id="totalStronglyAgree"></td>
                    <td class="formatSo" id="percentageStronglyDisagree"></td>
                    <td class="formatSo" id="percentageDisagree"></td>
                    <td class="formatSo" id="percentageNeutral"></td>
                    <td class="formatSo" id="percentageAgree"></td>
                    <td class="formatSo" id="percentageStronglyAgree"></td>
                    <td class="formatSo" id="averageScore"></td>
                </tr>
            `;
        $("#showfoot").html(html);

        ty_le.forEach(function (item, index) {
            const row = $('<tr>');
            row.append($('<td class="formatSo">').text(index + 1));
            row.append($('<td>').text(item.Question));
            row.append($('<td class="formatSo">').text(item.TotalResponses));

            totalResponses += item.TotalResponses;

            const frequencies = item.Frequencies;
            const percentages = item.Percentages;

            const stronglyDisagree = frequencies["Hoàn toàn không đồng ý"] || 0;
            const disagree = frequencies["Không đồng ý"] || 0;
            const neutral = frequencies["Bình thường"] || 0;
            const agree = frequencies["Đồng ý"] || 0;
            const stronglyAgree = frequencies["Hoàn toàn đồng ý"] || 0;

            totalStronglyDisagree += stronglyDisagree;
            totalDisagree += disagree;
            totalNeutral += neutral;
            totalAgree += agree;
            totalStronglyAgree += stronglyAgree;

            row.append($('<td class="formatSo">').text(stronglyDisagree));
            row.append($('<td class="formatSo">').text(disagree));
            row.append($('<td class="formatSo">').text(neutral));
            row.append($('<td class="formatSo">').text(agree));
            row.append($('<td class="formatSo">').text(stronglyAgree));

            const stronglyDisagreePercentage = percentages["Hoàn toàn không đồng ý"] ? percentages["Hoàn toàn không đồng ý"].toFixed(2) + "%" : "0%";
            const disagreePercentage = percentages["Không đồng ý"] ? percentages["Không đồng ý"].toFixed(2) + "%" : "0%";
            const neutralPercentage = percentages["Bình thường"] ? percentages["Bình thường"].toFixed(2) + "%" : "0%";
            const agreePercentage = percentages["Đồng ý"] ? percentages["Đồng ý"].toFixed(2) + "%" : "0%";
            const stronglyAgreePercentage = percentages["Hoàn toàn đồng ý"] ? percentages["Hoàn toàn đồng ý"].toFixed(2) + "%" : "0%";

            row.append($('<td class="formatSo">').text(stronglyDisagreePercentage));
            row.append($('<td class="formatSo">').text(disagreePercentage));
            row.append($('<td class="formatSo">').text(neutralPercentage));
            row.append($('<td class="formatSo">').text(agreePercentage));
            row.append($('<td class="formatSo">').text(stronglyAgreePercentage));

            const averageScore = item.AverageScore;
            totalScore += averageScore * item.TotalResponses;

            row.append($('<td class="formatSo">').text(averageScore.toFixed(2)));
            tbody.append(row);
        });

        const averageScore = totalScore / totalResponses;

        $('#totalResponses').text(totalResponses);
        $('#totalStronglyDisagree').text(totalStronglyDisagree);
        $('#totalDisagree').text(totalDisagree);
        $('#totalNeutral').text(totalNeutral);
        $('#totalAgree').text(totalAgree);
        $('#totalStronglyAgree').text(totalStronglyAgree);

        $('#percentageStronglyDisagree').text(((totalStronglyDisagree / totalResponses) * 100).toFixed(2) + "%");
        $('#percentageDisagree').text(((totalDisagree / totalResponses) * 100).toFixed(2) + "%");
        $('#percentageNeutral').text(((totalNeutral / totalResponses) * 100).toFixed(2) + "%");
        $('#percentageAgree').text(((totalAgree / totalResponses) * 100).toFixed(2) + "%");
        $('#percentageStronglyAgree').text(((totalStronglyAgree / totalResponses) * 100).toFixed(2) + "%");
        $('#averageScore').text(averageScore.toFixed(2));
        $("#showhead").show();
        $("#showalldata").show();
        $("#showfoot").show();
        $("#TitleSurvey").show();
    }
    else {
        tbody.empty();
    }
}
function form_y_kien_khac(ty_le) {
    if (ty_le) {
        let Ykienkhac = $("#YkienkhacSurvey");
        let html = "";
        Ykienkhac.empty();
        html = `
                <p style="font-size: 20px; font-weight: bold; color: black;">Ý kiến khác</p>
                <div class="question-block">
                    <div class="table-responsive">
                        <table class="table table-bordered">
                            <thead style="color:black; text-align:center; font-weight:bold;font-size:15px">
                                <tr>
                                    ${ty_le.map((item, index) => `<th scope="col" style="text-align: left;">${index + 1}. ${item.QuestionTitle}</th>`).join('')}
                                </tr>
                            </thead>
                            <tbody>
                                <tr>
                                    ${ty_le.map(item => `
                                    <td>
                                        ${item.Responses.map(response => `<p style="font-size: 15px; color: black;">${response}
                                        <hr style="margin-left: -16px;margin-right: -15px;border-bottom: 1px solid black;" />
                                        </p>`).join('')}
                                        
                                    </td>
                                    `).join('')}
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
                <hr />
                `;
        Ykienkhac.append(html);
    }
    else {
        Ykienkhac.empty();
    }
}
