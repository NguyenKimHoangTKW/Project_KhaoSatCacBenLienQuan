async function LoadCauHoiSingle(id) {
    try {
        const response = await $.ajax({
            url: '/CTDT/ThongKeKhaoSat/Loadthongketanxuatsingle',
            type: 'POST',
            data: { surveyid: id }
        });

        let container = $("#surveyContainerSingle");
        container.empty();

        if (response && response.length > 0) {
            response.forEach(function (item, questionIndex) {
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
        } else {
            container.html('');
        }
    } catch (error) {
        console.error('Error loading survey data:', error);
    }
}

async function LoadYKienKhac(id) {
    const res = await $.ajax({
        url: '/CTDT/ThongKeKhaoSat/LoadthongketanxuatYkienkhac',
        type: 'POST',
        data: { surveyid: id }
    });
    let Ykienkhac = $("#YkienkhacSurvey");
    let html = "";
    Ykienkhac.empty();
    if (res && res.length > 0) {
        html = `
                <p style="font-size: 20px; font-weight: bold; color: black;">Ý kiến khác</p>
                <div class="question-block">
                    <div class="table-responsive">
                        <table class="table table-bordered">
                            <thead style="color:black; text-align:center; font-weight:bold;font-size:15px">
                                <tr>
                                    ${res.map((item, index) => `<th scope="col" style="text-align: left;">${index + 1}. ${item.QuestionTitle}</th>`).join('')}
                                </tr>
                            </thead>
                            <tbody>
                                <tr>
                                    ${res.map(item => `
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
    } else {
        html = '<p>Không có dữ liệu.</p>';
        Ykienkhac.html(html);
    }
}
async function LoadCauHoi5Muc(id) {
    try {
        const response = await fetch('/CTDT/ThongKeKhaoSat/Loadthongketanxuat5Muc', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ surveyid: id }),
        });
        const res = await response.json();
        const tbody = $('#showdata');
        tbody.empty();
        const thead = $("#showhead");
        let html = "";

        if (res.length > 0) {
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

            res.forEach(function (item, index) {
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
        } else {
            $("#showalldata").show();
            const errorRow = '<p>Không có dữ liệu</p>';
            tbody.append(errorRow);
            $("#showhead").hide();
            $("#showfoot").hide();
            $("#TitleSurvey").hide();
        }
    } catch (error) {
        console.error("Error fetching data:", error);
        alert("An error occurred: " + error.message);
    }
}


async function LoadCauHoiNhieuLuaChon(id) {
    const res = await $.ajax({
        url: '/CTDT/ThongKeKhaoSat/Loadthongketanxuatnhieucauhoi',
        type: "POST",
        data: { surveyid: id }
    });
    let container = $("#surveyContainer");
    container.empty();

    if (res && res.length > 0) {
        res.forEach(function (item, questionIndex) {
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
    } else {
        container.html('');
    }
}