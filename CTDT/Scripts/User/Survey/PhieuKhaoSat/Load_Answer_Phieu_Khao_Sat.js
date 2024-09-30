$(document).ready(function () {
    load_phieu_khao_sat();
});

function load_phieu_khao_sat() {
    var id = $("#id").val();
    var surveyid = $("#surveyid").val();
    $.ajax({
        url: '/Survey/load_answer_phieu_khao_sat',
        type: 'POST',
        data: {
            id: id,
            surveyid: surveyid
        },
        success: function (res) {
            var items = res.data;
            let html = '';
            let body = $('#load-title');
            body.empty();
            if (items) {
                surveyData = JSON.parse(items.phieu_khao_sat);
                Dap_an = JSON.parse(items.dap_an);
                console.log(Dap_an)
                html += `
                <h2>${surveyData.title}</h2>
                <p>${surveyData.description}</p>
                <hr />
                <form>
            `;

                surveyData.pages.forEach(function (page) {
                    html += `<h4>${page.title}</h4>`;
                    html += page.description === undefined ? `<p></p><hr />` : `<p>${page.description}</p> <hr />`;

                    page.elements.forEach(function (element) {
                        let dapan = Dap_an.pages[0].elements.find(x => x.name === element.name);
                        let value = dapan ? dapan.response.text : '';
                        let visible = true;
                        if (element.visibleIf) {
                            visible = false;
                        }
                        let css_required = element.isRequired ? 'red' : 'black';
                        switch (element.type) {
                            case 'text':
                                html += `
                            <div class="form-group" id="group-${element.name}" style="display: ${visible ? 'block' : 'none'};">
                                <label for="${element.name}">${element.title} <span style="color : ${css_required};">*</span> </label>
                                <input type="text" class="form-control input-bottom-border" id="${element.name}" name="${element.name}" value="${value}" placeholder="Vui lòng điền ${element.title} tại đây!">
                                <p style="color: red;font-style: italic;text-align: right;display :none" class="error_${element.name}"></p>
                            </div>
                        `;
                                break;
                            case 'NameSV':
                                html += `
                            <div class="form-group" id="group-${element.name}" style="display: ${visible ? 'block' : 'none'};">
                                <label for="${element.name}">${element.title} <span style="color : ${css_required};">*</span> </label>
                                <input type="text" class="form-control input-bottom-border" id="${element.name}" name="${element.name}" value="${value}">
                                <p style="color: red;font-style: italic;text-align: right;display :none" class="error_${element.name}"></p>
                            </div>
                        `;
                                break;
                            case 'MSSV':
                                html += `
                            <div class="form-group" id="group-${element.name}" style="display: ${visible ? 'block' : 'none'};">
                                <label for="${element.name}">${element.title} <span style="color : ${css_required};">*</span> </label>
                                <input type="text" class="form-control input-bottom-border" id="${element.name}" name="${element.name}" value="${value}">
                                <p style="color: red;font-style: italic;text-align: right;display :none" class="error_${element.name}"></p>
                            </div>
                        `;
                                break;
                            case 'Khoa':
                                html += `
                            <div class="form-group" id="group-${element.name}" style="display: ${visible ? 'block' : 'none'};">
                                <label for="${element.name}">${element.title} <span style="color : ${css_required};">*</span> </label>
                                <input type="text" class="form-control input-bottom-border" id="${element.name}" name="${element.name}" value="${value}">
                                <p style="color: red;font-style: italic;text-align: right;display :none" class="error_${element.name}"></p>
                            </div>
                        `;
                                break;
                            case 'NganhDaoTao':
                                html += `
                            <div class="form-group" id="group-${element.name}" style="display: ${visible ? 'block' : 'none'};">
                                <label for="${element.name}">${element.title} <span style="color : ${css_required};">*</span> </label>
                                <input type="text" class="form-control input-bottom-border" id="${element.name}" name="${element.name}" value="${value}">
                                <p style="color: red;font-style: italic;text-align: right;display :none" class="error_${element.name}"></p>
                            </div>
                        `;
                                break;
                            // Load thông tin CBVC
                            case 'NameCBVC':
                                html += `
                            <div class="form-group" id="group-${element.name}" style="display: ${visible ? 'block' : 'none'};">
                                <label for="${element.name}">${element.title} <span style="color : ${css_required};">*</span> </label>
                                <input type="text" class="form-control input-bottom-border" id="${element.name}" name="${element.name}" value="${value}">
                                <p style="color: red;font-style: italic;text-align: right;display :none" class="error_${element.name}"></p>
                            </div>
                        `;
                                break;
                            case 'DonViCBVC':
                                html += `
                            <div class="form-group" id="group-${element.name}" style="display: ${visible ? 'block' : 'none'};">
                                <label for="${element.name}">${element.title} <span style="color : ${css_required};">*</span> </label>
                                <input type="text" class="form-control input-bottom-border" id="${element.name}" name="${element.name}" value="${value}">
                                <p style="color: red;font-style: italic;text-align: right;display :none" class="error_${element.name}"></p>
                            </div>
                        `;
                                break;
                            case 'ChucDanhCBVC':
                                html += `
                            <div class="form-group" id="group-${element.name}" style="display: ${visible ? 'block' : 'none'};">
                                <label for="industry">${element.title}</label>
                                <select class="form-control input-bottom-border select2" id="industry" name="${element.name}">
                        `;
                                element.choices.forEach(function (choice) {
                                    let selected = dapan && dapan.response.text.includes(choice.text) ? 'selected' : '';
                                    html += `
                                <option value="${choice.name}" ${selected}>${choice.text}</option>
                            `;
                                });

                                html += `
                                </select>
                                <p style="color: red;font-style: italic;text-align: right;display :none" class="error_${element.name}"></p>
                            </div>
                        `;
                                break;

                            case 'radiogroup':
                                var isHorizontal = element.choices.length >= 5;
                                var layoutClass = isHorizontal ? 'horizontal-group d-flex justify-content-between' : '';
                                html += `
                            <div class="form-group" id="group-${element.name}" style="display: ${visible ? 'block' : 'none'};">
                                <label for="${element.name}">${element.title} <span style="color : ${css_required};">*</span></label>
                                <div class="${layoutClass}">
                        `;
                                element.choices.forEach(function (choice) {
                                    var choiceClass = isHorizontal ? 'form-check form-check-inline p-2' : 'form-check p-3';
                                    let checked = dapan && dapan.response.text === choice.text ? 'checked' : '';
                                    html += `
                                <div class="${choiceClass}">
                                    <input class="form-check-input" type="radio" id="${choice.name}" name="${element.name}" value="${choice.name}" ${checked}>
                                    <p class="form-check-label" for="${choice.name}">${choice.text}</p>
                                </div>
                            `;
                                });
                                html += `
                                </div>
                                <p style="color: red;font-style: italic;text-align: right;display :none" class="error_${element.name}"></p>
                            </div>
                        `;
                                break;
                            case 'checkbox':
                                html += `
                            <div class="form-group" id="group-${element.name}" style="display: ${visible ? 'block' : 'none'};">
                                <label for="checkboxes">${element.title} <span style="color : ${css_required};">*</span></label>
                        `;
                                element.choices.forEach(function (choice) {
                                    let checked = dapan && dapan.response.text.includes(choice.text) ? 'checked' : '';
                                    html += `
                                <div class="checkbox-group">
                                    <input type="checkbox" id="${choice.name}" name="${element.name}" value="${choice.name}" ${checked}>
                                    <p>${choice.text}</p>
                                </div>
                            `;
                                });
                                html += `
                                <p style="color: red;font-style: italic;text-align: right;display :none" class="error_${element.name}"></p>
                            </div>
                        `;
                                break;
                            case 'comment':
                                html += `
                                <div class="form-group" id="group-${element.name}" style="display: ${visible ? 'block' : 'none'};">
                                    <label for="suggestions">${element.title}</label>
                                    <textarea class="form-control input-bottom-border" id="${element.name}" name="${element.name}" rows="4" placeholder="Vui lòng điền ${element.title} tại đây!">${value}</textarea>
                                    <p style="color: red;font-style: italic;text-align: right;display :none" class="error_${element.name}"></p>
                                </div>
                            `;
                                break;
                            case 'select':
                                html += `
                                <div class="form-group" id="group-${element.name}" style="display: ${visible ? 'block' : 'none'};">
                                    <label for="industry">${element.title}</label>
                                    <select class="form-control input-bottom-border select2" id="industry" name="${element.name}">
                            `;
                                element.choices.forEach(function (choice) {
                                    let selected = dapan && dapan.response.text.includes(choice.text) ? 'selected' : '';
                                    html += `
                                    <option value="${choice.name}" ${selected}>${choice.text}</option>
                                `;
                                });
                                html += `
                                    </select>
                                    <p style="color: red;font-style: italic;text-align: right;display :none" class="error_${element.name}"></p>
                                </div>
                            `;
                                break;
                            default:
                                break;
                        }
                    });
                });
                html += `
            <hr />
               <p class="d-flex justify-content-end"><a href="javascript:void(0)" class="btn btn-primary" id="save">Cập nhật dữ liệu</a></p>
                <p style="text-align: right;font-style: italic;">Một lần nữa, xin chân thành cảm ơn những ý kiến đóng góp của Anh (Chị)!</p>
                </form>
            `;
                body.append(html);
                check_dieu_kien();
                $(document).on('click', '#save', function () {
                    save_answer_form();
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}