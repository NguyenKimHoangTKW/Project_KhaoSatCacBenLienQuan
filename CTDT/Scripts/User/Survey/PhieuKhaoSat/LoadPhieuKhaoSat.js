$('.select2').select2();

$(document).ready(function () {
    $('.select2').select2();
    load_phieu_khao_sat();

    $(window).scroll(function () {
        if ($(this).scrollTop() > 200) {
            $('#scrollToTopBtn').fadeIn();
        } else {
            $('#scrollToTopBtn').fadeOut();
        }
    });

    $('#scrollToTopBtn').click(function () {
        $('html, body').animate({ scrollTop: 0 }, 800);
        return false;
    });
});


function load_phieu_khao_sat() {
    var get_info = [];
    let load_data = localStorage.getItem("xacthucstorage");
    load_data = load_data ? JSON.parse(load_data) : {};
    let TenPhieu = '';
    let get_tempData = '';
    $.ajax({
        url: '/api/load_form_phieu_khao_sat',
        type: 'POST',
        data: {
            Id: load_data.Id,
            id_nguoi_hoc_by_mon_hoc: load_data.id_nguoi_hoc_by_mon_hoc,
            ctdt: load_data.ctdt,
            nguoi_hoc: load_data.nguoi_hoc
        },
        success: function (res) {
            let html = '';
            let body = $('#load-title');
            body.empty();
            test(res.info)

            if (res.data) {
                surveyData = JSON.parse(res.data);
                TenPhieu = surveyData.title;
                if (res.is_nguoi_hoc) {
                    let ten_nguoi_hoc = res.info[0];
                    get_info.push(ten_nguoi_hoc);
                    get_tempData = TenPhieu + "_" + get_info[0].email + "_" + get_info[0].ma_so_nguoi_hoc + "_" + get_info[0].nganh_dao_tao;
                }
                else if (res.is_cbvc) {
                    let ten_cbvc = res.info[0];
                    get_info.push(ten_cbvc);
                    get_tempData = TenPhieu + "_" + get_info[0].email + "_" + get_info[0].ten_cbvc;
                }
                else if (res.is_cuu_nguoi_hoc) {
                    let ten_nguoi_hoc = res.info[0];
                    get_info.push(ten_nguoi_hoc);
                    get_tempData = TenPhieu + "_" + get_info[0].email + "_" + get_info[0].ma_so_nguoi_hoc + "_" + get_info[0].ten_nguoi_hoc + "_" + get_info[0].nganh_dao_tao;
                }
                else if (res.is_giang_vien) {
                    let ten_giang_vien = res.info[0];
                    get_info.push(ten_giang_vien);
                    get_tempData = TenPhieu + "_" + get_info[0].email + "_" + get_info[0].ten_cbvc + "_" + get_info[0].ctdt + "_" + get_info[0].don_vi;
                }
                else if (res.is_doanh_nghiep) {
                    let doang_nghiep = res.info[0];
                    get_info.push(doang_nghiep);
                    get_tempData = TenPhieu + "_" + get_info[0].email + "_" + get_info[0].ctdt;
                }
                else {
                    html = `<div class="alert alert-info" style="text-align: center;">
                        ${res.message}
                    </div>`
                    body.html(html);
                }
                let tempData = localStorage.getItem(get_tempData);
                tempData = tempData ? JSON.parse(tempData) : {};
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
                        let luutru = tempData[element.name];
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
                                    <input type="text" class="form-control input-bottom-border" id="${element.name}" name="${element.name}" placeholder="Vui lòng điền ${element.title} tại đây!" value="${luutru || ''}">
                                    <p style="color: red;font-style: italic;text-align: right;display :none" class="error_${element.name}"></p>
                                </div>
                            `;
                                break;
                            //Load Thông tin người học
                            case 'NameSV':
                                html += `
                                <div class="form-group" id="group-${element.name}" style="display: ${visible ? 'block' : 'none'};">
                                    <label for="${element.name}">${element.title} <span style="color : ${css_required};">*</span> </label>
                                    <input type="text" class="form-control input-bottom-border" id="${element.name}" name="${element.name}" value="${get_info[0].ten_nguoi_hoc}">
                                    <p style="color: red;font-style: italic;text-align: right;display :none" class="error_${element.name}"></p>
                                </div>
                            `;
                                break;
                            case 'MSSV':
                                html += `
                                <div class="form-group" id="group-${element.name}" style="display: ${visible ? 'block' : 'none'};">
                                    <label for="${element.name}">${element.title} <span style="color : ${css_required};">*</span> </label>
                                    <input type="text" class="form-control input-bottom-border" id="${element.name}" name="${element.name}" value="${get_info[0].ma_so_nguoi_hoc}">
                                    <p style="color: red;font-style: italic;text-align: right;display :none" class="error_${element.name}"></p>
                                </div>
                            `;
                                break;
                            case 'Khoa':
                                html += `
                                <div class="form-group" id="group-${element.name}" style="display: ${visible ? 'block' : 'none'};">
                                    <label for="${element.name}">${element.title} <span style="color : ${css_required};">*</span> </label>
                                    <input type="text" class="form-control input-bottom-border" id="${element.name}" name="${element.name}" value="${get_info[0].khoa}">
                                    <p style="color: red;font-style: italic;text-align: right;display :none" class="error_${element.name}"></p>
                                </div>
                            `;
                                break;
                            case 'NganhDaoTao':
                                html += `
                                <div class="form-group" id="group-${element.name}" style="display: ${visible ? 'block' : 'none'};">
                                    <label for="${element.name}">${element.title} <span style="color : ${css_required};">*</span> </label>
                                    <input type="text" class="form-control input-bottom-border" id="${element.name}" name="${element.name}" value="${get_info[0].nganh_dao_tao}">
                                    <p style="color: red;font-style: italic;text-align: right;display :none" class="error_${element.name}"></p>
                                </div>
                            `;
                                break;
                            // Load thông tin CBVC
                            case 'NameCBVC':
                                html += `
                                <div class="form-group" id="group-${element.name}" style="display: ${visible ? 'block' : 'none'};">
                                    <label for="${element.name}">${element.title} <span style="color : ${css_required};">*</span> </label>
                                    <input type="text" class="form-control input-bottom-border" id="${element.name}" name="${element.name}" value="${get_info[0].ten_cbvc}">
                                    <p style="color: red;font-style: italic;text-align: right;display :none" class="error_${element.name}"></p>
                                </div>
                            `;
                                break;
                            case 'DonViCBVC':
                                html += `
                                <div class="form-group" id="group-${element.name}" style="display: ${visible ? 'block' : 'none'};">
                                    <label for="${element.name}">${element.title} <span style="color : ${css_required};">*</span> </label>
                                    <input type="text" class="form-control input-bottom-border" id="${element.name}" name="${element.name}" value="${get_info[0].don_vi}">
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
                                var selectedChucVu = get_info[0].chuc_vu;
                                element.choices.forEach(function (choice) {
                                    let selected = (choice.text === selectedChucVu) ? 'selected' : '';
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
                                    let checked = (luutru === choice.name) ? 'checked' : '';
                                    html += `
                                    <div class="${choiceClass}">
                                        <input class="form-check-input" type="radio" id="${choice.name}" name="${element.name}" value="${choice.name}" ${checked}>
                                        <p class="form-check-label" for="${choice.name}">${choice.text}</p>
                                    </div>
                                    `;
                                });

                                if (element.showOtherItem) {
                                    var otherChecked = (luutru === 'other') ? 'checked' : '';
                                    html += `
                                    <div class="form-check ${isHorizontal ? 'form-check-inline p-2' : 'form-check p-3'}">
                                        <input class="form-check-input" type="radio" id="${element.name}_other" name="${element.name}" value="other" ${otherChecked}>
                                        <p class="form-check-label" for="${element.name}_other">${element.otherText}</p>
                                    </div>
                                    <div class="${isHorizontal ? 'form-check-inline' : 'form-check'}" style="display: ${otherChecked ? 'block' : 'none'};">
                                        <input type="text" class="form-control" id="${element.name}_otherText" name="${element.name}_otherText" placeholder="Vui lòng chỉ rõ">
                                    </div>
                                    `;
                                }

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
                                    let checked = (Array.isArray(luutru) && luutru.includes(choice.name)) ? 'checked' : '';
                                    html += `
                                    <div class="form-check p-3">
                                        <input class="form-check-input" type="checkbox" id="${choice.name}" name="${element.name}" value="${choice.name}" ${checked}>
                                        <p class="form-check-label" for="${choice.name}">${choice.text}</p>
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
                                    <textarea class="form-control input-bottom-border" id="${element.name}" name="${element.name}" rows="4" placeholder="Vui lòng điền ${element.title} tại đây!">${luutru || ''}</textarea>
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
                                    let selected = (luutru === choice.name) ? 'selected' : '';
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
               <p class="d-flex justify-content-end"><a href="javascript:void(0)" class="btn btn-primary" id="save">Lưu dữ liệu</a></p>
                <p style="text-align: right;font-style: italic;">Một lần nữa, xin chân thành cảm ơn những ý kiến đóng góp của Anh (Chị)!</p>
                </form>
            `;

                body.html(html);

                check_dieu_kien();
                $(document).on('input change', 'input, select, textarea', function () {
                    let formData = {};
                    $('input, select, textarea').each(function () {
                        let name = $(this).attr('name');
                        if ($(this).is(':checkbox')) {
                            if (!formData[name]) {
                                formData[name] = [];
                            }
                            if ($(this).is(':checked')) {
                                formData[name].push($(this).val());
                            }
                        } else if ($(this).is(':radio')) {
                            if ($(this).is(':checked')) {
                                formData[name] = $(this).val();
                            }
                        } else {
                            formData[name] = $(this).val();
                        }
                    });
                    localStorage.setItem(get_tempData, JSON.stringify(formData));
                });

                $(document).on('click', '#save', function () {
                    save_form();
                });
            }
            else {
                surveyData = null;
            }
        },
        error: function (err) {
            console.log(err);
            alert("Có lỗi xảy ra khi tải dữ liệu.");
        }
    });
}

function test(items) {
    console.log(items[0].ten_nguoi_hoc)
}