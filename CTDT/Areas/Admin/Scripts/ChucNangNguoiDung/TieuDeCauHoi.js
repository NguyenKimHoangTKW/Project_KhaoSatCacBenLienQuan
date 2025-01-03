$(document).ready(function () {
    $("#hedaotao, #year").on("change", load_pks_by_nam);
    $('#optionsTextarea').val('1. ');
    function validateRomanInput() {
        const romanRegex = /^(?=[MDCLXVI])M{0,4}(CM|CD|D?C{0,3})(XC|XL|L?X{0,3})(IX|IV|V?I{0,3})$/i;
        const input = $("#edtThuTuHienThiTitle");
        const value = input.val();

        if (!romanRegex.test(value) && value !== "") {
            input.val(value.slice(0, -1));
            Swal.fire({
                icon: "warning",
                title: "Chỉ được nhập chữ số La Mã",
                toast: true,
                position: "top-end",
                showConfirmButton: false,
                timer: 2000
            });
        }
    }
    $("#edtThuTuHienThiTitle").on("input", validateRomanInput);
    $("#exampleModal").on("hidden.bs.modal", function () {
        $(this).find("input, textarea").val("");
        $(this).find("select").prop("selectedIndex", 0);
        $(this).find(".is-invalid, .is-valid").removeClass("is-invalid is-valid");
    });
})
$(document).on('keypress', '#optionsTextarea', function (event) {
    if (event.which === 13) {
        event.preventDefault();
        const textarea = $(this);
        const text = textarea.val();
        const lines = text.split('\n');
        const lastLine = lines[lines.length - 1];
        const match = lastLine.match(/^(\d+)\./);
        const nextNumber = match ? parseInt(match[1], 10) + 1 : 1;
        textarea.val(text + `\n${nextNumber}. `);
    }
});

$(document).on('paste', '#optionsTextarea', function () {
    const textarea = $(this);
    setTimeout(function () {
        const text = textarea.val();
        const lines = text.split('\n');
        let newText = '';
        let lineNumber = 1;
        lines.forEach(function (line, index) {
            if (line.trim() !== '') {
                const currentLine = line.match(/^(\d+)\./);
                if (currentLine) {
                    newText += `${lineNumber}. ${line.substring(currentLine[0].length).trim()}\n`;
                } else {
                    newText += `${lineNumber}. ${line.trim()}\n`;
                }
                lineNumber++;
            }
        });

        textarea.val(newText.trim());
    }, 1);
});

$(document).on("click", "#saveOptions", function (event) {
    event.preventDefault();
    const inputText = $('#optionsTextarea').val();

    const options = inputText.split('\n')
        .map(option => option.trim())
        .filter(option => option !== '');

    const requestData = {
        ten_rd_cau_hoi_khac: options.join('\n')
    };
    $.ajax({
        url: '/api/admin/saveRadioOptions',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(requestData),
        success: function (response) {
            alert(response.message);
            $('#optionsTextarea').val('');
            load_tieu_de_pks()
        },
        error: function (xhr) {
            console.error(xhr.responseText);
            alert('Có lỗi xảy ra khi lưu dữ liệu.');
        }
    });
})
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
    if (res.success) {
        res.data.forEach(function (item) {
            html += `<option value="${item.id_phieu}">${item.ten_phieu}</option>`;
        });
        $("#surveyid").empty().html(html);
    } else {
        html += `<option value="">${res.message}</option>`;
        $("#surveyid").empty().html(html);
        $("#ctdt").empty().html(html);
    }
}

$(document).on("change", "#surveyid", function () {
    load_tieu_de_pks();
})

$(document).on("click", "#btnSaveChangesAddTittle", function (event) {
    event.preventDefault()
    add_tittle();
});

async function add_tittle() {
    const value = $("#surveyid").val();
    const thutu = $("#edtThuTuHienThiTitle").val();
    const tieude = $("#edttieudeTitle").val();
    const res = await $.ajax({
        url: "/api/admin/add-new-title-survey",
        type: "POST",
        data: {
            surveyID: value,
            thu_tu: thutu,
            ten_tieu_de: tieude
        }
    })
    if (res.success) {
        const Toast = Swal.mixin({
            toast: true,
            position: "top-end",
            showConfirmButton: false,
            timer: 3000,
            timerProgressBar: true,
            didOpen: (toast) => {
                toast.onmouseenter = Swal.stopTimer;
                toast.onmouseleave = Swal.resumeTimer;
            }
        });
        Toast.fire({
            icon: "success",
            title: res.message
        });
        load_tieu_de_pks()
    }
    else {
        const Toast = Swal.mixin({
            toast: true,
            position: "top-end",
            showConfirmButton: false,
            timer: 3000,
            timerProgressBar: true,
            didOpen: (toast) => {
                toast.onmouseenter = Swal.stopTimer;
                toast.onmouseleave = Swal.resumeTimer;
            }
        });
        Toast.fire({
            icon: "error",
            title: res.message
        });
    }
}
$(document).on("click", "#btnAddTitle", function () {
    const footer = $("#showfootermodaltitlesurvey");
    const titleHeaderModal = $("#exampleModalLabelTitleSurvey")
    let footer_html = "";
    footer.empty();
    footer_html =
        `
        <button type="button" class="btn btn-default" data-dismiss="modal">Đóng</button>
        <button type="button" id="btnSaveChangesAddTittle" class="btn btn-primary">Lưu dữ liệu</button>
        `;
    titleHeaderModal.text("Thêm mới tiêu đề câu hỏi")
    footer.html(footer_html);
    $("#exampleModal").modal('show');
})
$(document).on("click", "#btnDeleteTitleSurvey", function () {
    const value = $(this).data("id");
    Swal.fire({
        title: "Bạn chắc chắn muốn xóa tiêu đề?",
        text: "Nếu xóa tiêu đề chính, tất cả các tiêu đề phụ sẽ bị xóa!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Có, xóa luôn"
    }).then((result) => {
        if (result.isConfirmed) {
            delete_tieu_de_pks(value);
        }
    });
})
$(document).on("click", "#btnEditTitleSurvey", function () {
    const value = $(this).data("id");
    const footer = $("#showfootermodaltitlesurvey");
    const titleHeaderModal = $("#exampleModalLabelTitleSurvey")
    let footer_html = "";
    footer.empty();
    titleHeaderModal.empty();
    get_info_title_survey(value);
    footer_html =
        `
        <button type="button" class="btn btn-default" data-dismiss="modal">Đóng</button>
        <button type="button" id="btnSaveChangesEditTittle" class="btn btn-primary">Lưu dữ liệu</button>
        `;
    titleHeaderModal.text("Chỉnh sửa tiêu đề câu hỏi");
    footer.html(footer_html);
    $("#exampleModal").modal('show');
    $("#btnSaveChangesEditTittle").click(function (events) {
        events.preventDefault();
        update_tieu_de_pks(value);
    })
});

$(document).on("click", "#btnAddChilTitle", function (event) {
    event.preventDefault();
    load_option_children_title();
   
})

async function load_option_children_title() {
    const value_sv = $("#surveyid").val();
    const body = $("#loadoptionchiltitle");
    let html = ``;
    const res = await $.ajax({
        url: "/api/admin/option-chi-tiet-cau-hoi",
        type: "POST",
        data: {
            surveyID: value_sv
        }
    });
    if (res.success) {
        html += `
            <div class="form-group">
                <label class="form-label">Chọn tiêu đề câu hỏi chính</label>
                <select class="form-control select2" id="hedaotao">`;
        res.tieu_de.forEach(title => {
            html += `<option value="${title.value_title}">${title.name}</option>`;
        });

        html += `</select>
            </div>
            <div class="form-group">
                <label for="formGroupExampleInput2">Thứ tự hiển thị</label>
                <input type="text" class="form-control" id="formGroupExampleInput2" placeholder="Another input">
            </div>
            <div class="form-group">
                <label for="formGroupExampleInput2">Tên chi tiết câu hỏi</label>
                <input type="text" class="form-control" id="formGroupExampleInput2" placeholder="Another input">
            </div>
            <div class="form-group">
                <label class="form-label">Dạng câu hỏi</label>
                <select class="form-control select2" id="dangCauHoi">`;

        res.dang_cau_hoi.forEach(dangcauhoi => {
            html += `<option value="${dangcauhoi.value_dch}">${dangcauhoi.name}</option>`;
        });

        html += `</select>
            </div>
            <div class="form-group">
                <label for="formGroupExampleInput2">Các lựa chọn</label>
                <div class="checkbox">
                    <input id="checkbox1" type="checkbox">
                    <label for="checkbox1">Là câu hỏi bắt buộc</label>
                </div>
                <div class="checkbox">
                    <input id="checkbox2" type="checkbox">
                    <label for="checkbox2">Có ý kiến khác</label>
                </div>
            </div>
            <div id="conditionalBlock"></div>`;

        body.html(html);
        $("#chitietModal").modal("show");
        $("#dangCauHoi").on("change", function () {
            const selectedValue = $(this).val();
            const conditionalBlock = $("#conditionalBlock");

            if (selectedValue == "2") {
                conditionalBlock.html(`
                    <div class="form-group">
                        <label class="form-label">Nhập các tùy chọn (mỗi tùy chọn là 1 dòng)</label>
                        <textarea id="optionsTextarea" class="form-control" aria-label="With textarea" rows="10"></textarea>
                    </div>
                `);
            } else {
                conditionalBlock.html(""); 
            }
        });
    } else {
        const Toast = Swal.mixin({
            toast: true,
            position: "top-end",
            showConfirmButton: false,
            timer: 3000,
            timerProgressBar: true,
            didOpen: (toast) => {
                toast.onmouseenter = Swal.stopTimer;
                toast.onmouseleave = Swal.resumeTimer;
            }
        });
        Toast.fire({
            icon: "error",
            title: res.message
        });
    }
}


async function update_tieu_de_pks(value) {
    const value_s = $("#surveyid").val();
    const thutu = $("#edtThuTuHienThiTitle").val();
    const tieude = $("#edttieudeTitle").val();

    const res = await $.ajax({
        url: "/api/admin/update-title-survey",
        type: "POST",
        data: {
            id_tieu_de_phieu: value,
            surveyID: value_s,
            thu_tu: thutu,
            ten_tieu_de: tieude
        }
    });
    if (res.success) {
        const Toast = Swal.mixin({
            toast: true,
            position: "top-end",
            showConfirmButton: false,
            timer: 3000,
            timerProgressBar: true,
            didOpen: (toast) => {
                toast.onmouseenter = Swal.stopTimer;
                toast.onmouseleave = Swal.resumeTimer;
            }
        });
        Toast.fire({
            icon: "success",
            title: res.message
        });
        load_tieu_de_pks()
    }
}
async function delete_tieu_de_pks(value) {
    const res = await $.ajax({
        url: "/api/admin/delete-title-survey",
        type: "POST",
        data: {
            id_tieu_de_phieu: value
        }
    });
    if (res.success) {
        const Toast = Swal.mixin({
            toast: true,
            position: "top-end",
            showConfirmButton: false,
            timer: 3000,
            timerProgressBar: true,
            didOpen: (toast) => {
                toast.onmouseenter = Swal.stopTimer;
                toast.onmouseleave = Swal.resumeTimer;
            }
        });
        Toast.fire({
            icon: "success",
            title: res.message
        });
        load_tieu_de_pks()
    }
}
async function get_info_title_survey(value) {
    const res = await $.ajax({
        url: "/api/admin/get-info-title-survey",
        type: "POST",
        data: {
            id_tieu_de_phieu: value
        }
    });
    $("#edttieudeTitle").val(res.ten_tieu_de);
    $("#edtThuTuHienThiTitle").val(res.thu_tu);
}
async function load_tieu_de_pks() {
    const pks = $("#surveyid").val();
    const res = await $.ajax({
        url: '/api/admin/load-bo-cau-hoi-phieu-khao-sat',
        type: 'POST',
        data: {
            surveyID: pks
        },
    });

    let body = $('#tieu-de-pks');
    let html = '';

    if (res.success) {
        res.data.pages.forEach(page => {
            html += `
                <div class="parent-title border mb-4 p-3 bg-white shadow-sm rounded">
                    <div class="d-flex justify-content-between align-items-center">
                        <h4 class="text-dark font-weight-bold mb-3">
                            <i class="bi bi-list-task"></i> ${page.title}
                        </h4>
                        <div class="parent-actions">
                            <button id="btnEditTitleSurvey" data-id="${page.value_title}" class="btn btn-sm btn-outline-primary me-2">
                                <i class="bi bi-pencil"></i> Sửa
                            </button>
                            <button  id="btnDeleteTitleSurvey" data-id="${page.value_title}" class="btn btn-sm btn-outline-danger">
                                <i class="bi bi-trash"></i> Xóa
                            </button>
                        </div>
                    </div>
                    <div class="child-titles">
            `;
            page.elements.forEach(element => {
                html += `
                    <div class="child-title mb-4">
                        <div class="element-header d-flex align-items-center justify-content-between p-2 bg-light border rounded">
                            <h5 class="element-title text-primary font-weight-bold mb-0">
                                <i class="bi bi-question-circle"></i> ${element.title}
                            </h5>
                            <div class="element-actions">
                                <button id="btnEditChilTitle" class="btn btn-sm btn-outline-primary me-2">
                                    <i class="bi bi-pencil"></i> Chỉnh sửa
                                </button>
                                <button class="btn btn-sm btn-outline-danger">
                                    <i class="bi bi-trash"></i> Xóa
                                </button>
                            </div>
                        </div>
                        <ul class="child-attributes list-unstyled mt-2 ps-3">
                            <li>
                                <strong>Dạng câu hỏi:</strong> ${element.type === "radiogroup" ? "Trắc nghiệm" :
                        element.type === "checkbox" ? "Hộp kiểm" :
                            element.type === "text" ? "Đoạn trả lời ngắn" :
                                element.type === "comment" ? "Đoạn trả lời dài" : "Đoạn trả lời dài"
                    }
                            </li>
                            <li>
                                <strong>Bắt buộc:</strong> ${element.isRequired ? "Có" : "Không"}
                            </li>
                        </ul>
                `;

                if (element.choices && element.choices.length > 0) {
                    html += `
                        <div class="choices mt-3">
                            <strong>Danh sách lựa chọn:</strong>
                            <ul class="list-group mt-2">
                    `;
                    element.choices.forEach(choice => {
                        html += `
                            <li class="list-group-item p-2">${choice.text}</li>
                        `;
                    });
                    html += `
                            </ul>
                        </div>
                    `;
                }

                if (element.showOtherItem) {
                    html += `
                        <div class="other-option mt-2">
                            <strong>Ý kiến khác:</strong> Có
                        </div>
                    `;
                }

                html += `
                    </div>
                `;
            });

            html += `
                    </div>
                </div>
            `;
        });

        body.html(html);
    } else {
        html = `
            <div class="alert alert-info text-center font-weight-bold">
                ${res.message}
            </div>`;
        body.html(html);
    }
}
