
// Các sự kiện
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
$(document).on("change", "#surveyid", function () {
    load_tieu_de_pks();
})
$(document).on("click", "#btnSaveChangesAddTittle", function (event) {
    event.preventDefault()
    add_tittle();
});
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
    const body_footer = $("#modalfooterchildrentitle");
    let html = ``;
    html += `
        <button type="button" class="btn btn-default" data-dismiss="modal">Close</button>
        <button type="button" id="AddNewChildrenTitle" class="btn btn-primary">Lưu dữ liệu</button>
    `;
    body_footer.html(html);
    load_option_children_title();
});
$(document).on("click", "#AddNewChildrenTitle", function (event) {
    event.preventDefault();
    const value_title = $("#edtSelectedTitle").val();
    const thu_tu = $("#edtThuTuChilTitle").val();
    const ten_cau_hoi = $("#edtNameChilTitle").val();
    const id_dang_cau_hoi = $("#edtSelectedDangCauHoi").val();
    const is_required = $("#ckIsRequired").prop('checked') ? 1 : 0;
    const is_orderitem = $("#ckIsOrderItem").prop('checked') ? 1 : 0;


    if (id_dang_cau_hoi == "3" || id_dang_cau_hoi == "4" || id_dang_cau_hoi == "5") {
        const inputText = $('#optionsTextarea').val();
        const options = inputText.split('\n')
            .map(option => option.trim())
            .filter(option => option !== '');

        const requestData = {
            thu_tu: thu_tu,
            id_tieu_de_phieu: value_title,
            ten_cau_hoi: ten_cau_hoi,
            id_dang_cau_hoi: id_dang_cau_hoi,
            bat_buoc: is_required,
            is_ykienkhac: is_orderitem,
            ten_rd_cau_hoi_khac: options.join('\n')
        };
        $.ajax({
            url: '/api/admin/save-children-title',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(requestData),
            success: function (response) {
                if (response.success) {
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
                        title: response.message
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
            },
            error: function (xhr) {
                console.error(xhr.responseText);
                alert('Có lỗi xảy ra khi lưu dữ liệu.');
            }
        });
    }
    else {
        const requestData = {
            thu_tu: thu_tu,
            id_tieu_de_phieu: value_title,
            ten_cau_hoi: ten_cau_hoi,
            id_dang_cau_hoi: id_dang_cau_hoi,
            bat_buoc: is_required,
            is_ykienkhac: is_orderitem,
            ten_rd_cau_hoi_khac: null
        };
        $.ajax({
            url: '/api/admin/save-children-title',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(requestData),
            success: function (response) {
                if (response.success) {
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
                        title: response.message
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
                        title: response.message
                    });
                }
            },
            error: function (xhr) {
                console.error(xhr.responseText);
                alert('Có lỗi xảy ra khi lưu dữ liệu.');
            }
        });
    }
})
$(document).on('input', '#edtThuTuChilTitle', function () {
    let value = $(this).val();
    value = value.replace(/\D/g, '');
    $(this).val(value);
});
$(document).on("click", "#EditChildrenTitle", function (event) {
    event.preventDefault();
    const value = $("#btnEditChilTitle").data("id");
    const value_title = $("#edtSelectedTitle").val();
    const thu_tu = $("#edtThuTuChilTitle").val();
    const ten_cau_hoi = $("#edtNameChilTitle").val();
    const id_dang_cau_hoi = $("#edtSelectedDangCauHoi").val();
    const is_required = $("#ckIsRequired").prop('checked') ? 1 : 0;
    const is_orderitem = $("#ckIsOrderItem").prop('checked') ? 1 : 0;
    if (id_dang_cau_hoi == "3" || id_dang_cau_hoi == "4" || id_dang_cau_hoi == "5") {
        const inputText = $('#optionsTextarea').val();
        const options = inputText.split('\n')
            .map(option => option.trim())
            .filter(option => option !== '');

        const requestData = {
            id_chi_tiet_cau_hoi_tieu_de: value,
            thu_tu: thu_tu,
            id_tieu_de_phieu: value_title,
            ten_cau_hoi: ten_cau_hoi,
            id_dang_cau_hoi: id_dang_cau_hoi,
            bat_buoc: is_required,
            is_ykienkhac: is_orderitem,
            ten_rd_cau_hoi_khac: options.join('\n')
        };
        $.ajax({
            url: '/api/admin/edit-children-title',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(requestData),
            success: function (response) {
                if (response.success) {
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
                        title: response.message
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
            },
            error: function (xhr) {
                console.error(xhr.responseText);
                alert('Có lỗi xảy ra khi lưu dữ liệu.');
            }
        });
    }
    else {
        const requestData = {
            id_chi_tiet_cau_hoi_tieu_de: value,
            thu_tu: thu_tu,
            id_tieu_de_phieu: value_title,
            ten_cau_hoi: ten_cau_hoi,
            id_dang_cau_hoi: id_dang_cau_hoi,
            bat_buoc: is_required,
            is_ykienkhac: is_orderitem,
            ten_rd_cau_hoi_khac: null
        };
        $.ajax({
            url: '/api/admin/edit-children-title',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(requestData),
            success: function (response) {
                if (response.success) {
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
                        title: response.message
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
                        title: response.message
                    });
                }
            },
            error: function (xhr) {
                console.error(xhr.responseText);
                alert('Có lỗi xảy ra khi lưu dữ liệu.');
            }
        });
    }
})
$(document).on("click", "#btnEditChilTitle", function (event) {
    event.preventDefault();
    const value = $(this).data("id");
    const body_footer = $("#modalfooterchildrentitle");
    let html = ``;
    html += `
        <button type="button" class="btn btn-default" data-dismiss="modal">Close</button>
        <button type="button" id="EditChildrenTitle" class="btn btn-primary">Lưu dữ liệu</button>
    `;
    body_footer.html(html);
    load_option_info_children_title(value);
})
$(document).on("click", "#btnDeleteChilTitle", function (event) {
    event.preventDefault();
    const value = $(this).data("id");
    Swal.fire({
        title: "Bạn muốn xóa tiêu đề con?",
        text: "Khi bạn xóa tiêu đề con, toàn bộ thông tin liên quan đến tiêu đề con này sẽ bị xóa!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Có, xóa luôn"
    }).then((result) => {
        if (result.isConfirmed) {
            delete_children_title(value);
        }
    });

})
$(document).on("click", "#btnXemTruocPhieuDaTao", function (event) {
    event.preventDefault();
    const value = $("#surveyid").val();
    if (value == "") {
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
            title: "Vui lòng chọn phiếu khảo sát để xem"
        });
    }
    else {
        window.open(`/xem-truoc-cau-hoi-da-tao/${value}`, '_blank');
    }
});

// Hàm xử lý
async function delete_children_title(value) {
    const res = await $.ajax({
        url: "/api/admin/delete-children-title",
        type: "POST",
        data: {
            id_chi_tiet_cau_hoi_tieu_de: value
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
        load_tieu_de_pks();
    }
}
async function load_option_info_children_title(value) {
    const value_sv = $("#surveyid").val();
    const body = $("#loadoptionchiltitle");
    let html = ``;
    const res = await $.ajax({
        url: "/api/admin/option-chi-tiet-cau-hoi",
        type: "POST",
        data: { surveyID: value_sv }
    });

    if (res.success) {
        const response_data = await $.ajax({
            url: '/api/admin/info-children-title',
            type: 'POST',
            data: { id_chi_tiet_cau_hoi_tieu_de: value },
        });

        if (response_data.success) {
            const item = response_data.data_chil;
            const get_rd = response_data.get_rd;

            html += `
                <div class="form-group">
                    <label class="form-label">Chọn tiêu đề câu hỏi chính</label>
                    <select class="form-control select2" id="edtSelectedTitle">`;
            res.tieu_de.forEach(title => {
                html += `<option value="${title.value_title}" ${title.value_title == item.id_tieu_de_phieu ? 'selected' : ''}>${title.name}</option>`;
            });

            html += `</select>
                </div>
                <div class="form-group">
                    <label for="formGroupExampleInput2">Thứ tự hiển thị</label>
                    <input type="text" class="form-control" autocomplete="off" id="edtThuTuChilTitle" placeholder="Nhập thứ tự hiển thị bằng số" value="${item.thu_tu}">
                </div>
                <div class="form-group">
                    <label for="formGroupExampleInput2">Tên chi tiết câu hỏi</label>
                    <input type="text" class="form-control" autocomplete="off" id="edtNameChilTitle" placeholder="Nhập tên chi tiết" value="${item.ten_cau_hoi}">
                </div>
                <div class="form-group">
                    <label class="form-label">Dạng câu hỏi</label>
                    <select class="form-control select2" id="edtSelectedDangCauHoi">`;
            res.dang_cau_hoi.forEach(dangcauhoi => {
                html += `<option value="${dangcauhoi.value_dch}" ${dangcauhoi.value_dch == item.id_dang_cau_hoi ? 'selected' : ''}>${dangcauhoi.name}</option>`;
            });

            html += `</select>
                </div>
                <div class="form-group">
                    <label for="formGroupExampleInput2">Các lựa chọn</label>
                    <div class="checkbox">
                        <input id="ckIsRequired" type="checkbox" ${item.bat_buoc ? 'checked' : ''}>
                        <label for="ckIsRequired">Là câu hỏi bắt buộc</label>
                    </div>
                    <div class="checkbox">
                        <input id="ckIsOrderItem" type="checkbox" ${item.is_ykienkhac ? 'checked' : ''}>
                        <label for="ckIsOrderItem">Có ý kiến khác</label>
                    </div>
                </div>
                <div id="conditionalBlock">`;

            if (item.id_dang_cau_hoi == 3 || item.id_dang_cau_hoi == 4 || item.id_dang_cau_hoi == 5) {
                html += `
                    <div class="form-group">
                        <label class="form-label">Nhập các tùy chọn (mỗi tùy chọn là 1 dòng)</label>
                        <textarea id="optionsTextarea" class="form-control" aria-label="With textarea" rows="10">`;
                get_rd.forEach(rd => {
                    html += `${rd.thu_tu}. ${rd.ten_rd_cau_hoi_khac}\n`;
                });
                html += `</textarea>
                    </div>`;
            }

            html += `</div>`;
            body.html(html);
            $(".select2").select2();
            $("#chitietModal").modal("show");
            $("#edtSelectedDangCauHoi").on("change", function () {
                const selectedValue = $(this).val();
                const conditionalBlock = $("#conditionalBlock");

                if (selectedValue == "3" || selectedValue == "4" || selectedValue == "5") {
                    let optionsHtml = `
                        <div class="form-group">
                            <label class="form-label">Nhập các tùy chọn (mỗi tùy chọn là 1 dòng)</label>
                            <textarea id="optionsTextarea" autocomplete="off" class="form-control" aria-label="With textarea" rows="10">`;
                    get_rd.forEach(rd => {
                        optionsHtml += `${rd.thu_tu}. ${rd.ten_rd_cau_hoi_khac}\n`;
                    });
                    optionsHtml += `</textarea>
                        </div>`;
                    conditionalBlock.html(optionsHtml);
                } else {
                    conditionalBlock.html("");
                }
                $(".select2").select2();
            });
        } else {
            Swal.fire({
                icon: "error",
                title: "Lỗi",
                text: "Không thể tải thông tin chi tiết câu hỏi!"
            });
        }
    } else {
        Swal.fire({
            icon: "error",
            title: "Lỗi",
            text: res.message
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
                <select class="form-control select2" id="edtSelectedTitle">`;
        res.tieu_de.forEach(title => {
            html += `<option value="${title.value_title}">${title.name}</option>`;
        });

        html += `</select>
            </div>
            <div class="form-group">
                <label for="formGroupExampleInput2">Thứ tự hiển thị</label>
                <input type="text" autocomplete="off" class="form-control" id="edtThuTuChilTitle" placeholder="Nhập thứ tự hiển thị bằng số">
            </div>
            <div class="form-group">
                <label for="formGroupExampleInput2">Tên chi tiết câu hỏi</label>
                <input type="text" autocomplete="off" class="form-control" id="edtNameChilTitle" placeholder="Nhập tên chi tiết">
            </div>
            <div class="form-group">
                <label class="form-label">Dạng câu hỏi</label>
                <select class="form-control select2" id="edtSelectedDangCauHoi">`;

        res.dang_cau_hoi.forEach(dangcauhoi => {
            html += `<option value="${dangcauhoi.value_dch}">${dangcauhoi.name}</option>`;
        });

        html += `</select>
            </div>
            <div class="form-group">
                <label for="formGroupExampleInput2">Các lựa chọn</label>
                <div class="checkbox">
                    <input id="ckIsRequired" type="checkbox">
                    <label for="ckIsRequired">Là câu hỏi bắt buộc</label>
                </div>
                <div class="checkbox">
                    <input id="ckIsOrderItem" type="checkbox">
                    <label for="ckIsOrderItem">Có ý kiến khác</label>
                </div>
            </div>
            <div id="conditionalBlock"></div>`;

        body.html(html);
        $("#chitietModal").modal("show");
        $("#edtSelectedDangCauHoi").on("change", function () {
            const selectedValue = $(this).val();
            const conditionalBlock = $("#conditionalBlock");

            if (selectedValue == "3" || selectedValue == "4" || selectedValue == "5") {
                conditionalBlock.html(`
                    <div class="form-group">
                        <label class="form-label">Nhập các tùy chọn (mỗi tùy chọn là 1 dòng)</label>
                        <textarea id="optionsTextarea" class="form-control" aria-label="With textarea" rows="10">1. </textarea>
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
        $("#surveyid").empty().html(html).trigger("change");
    } else {
        html += `<option value="">${res.message}</option>`;
        $("#surveyid").empty().html(html).trigger("change");
        $("#ctdt").empty().html(html).trigger("change");
    }
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
                    <div class="child-titles mt-3">
            `;
            page.elements.forEach(element => {
                html += `
                    <div class="child-title mb-4">
                        <div class="element-header d-flex align-items-center justify-content-between p-2 bg-light border rounded">
                            <h5 class="element-title text-primary font-weight-bold mb-0">
                                <i class="bi bi-question-circle"></i> ${element.title}
                            </h5>
                            <div class="element-actions">
                                <button id="btnEditChilTitle" data-id="${element.value_chil}" class="btn btn-sm btn-outline-primary me-2">
                                    <i class="bi bi-pencil"></i> Chỉnh sửa
                                </button>
                                <button id="btnDeleteChilTitle" data-id="${element.value_chil}" class="btn btn-sm btn-outline-danger">
                                    <i class="bi bi-trash"></i> Xóa
                                </button>
                            </div>
                        </div>
                        <ul class="child-attributes list-unstyled mt-2 ps-3">
                            <li>
                                <strong>Dạng câu hỏi:</strong> ${element.type === "radiogroup" ? "Trắc nghiệm" :
                        element.type === "checkbox" ? "Hộp kiểm" :
                            element.type === "select" ? "Menu thả xuống" :
                                element.type === "text" ? "Đoạn trả lời ngắn" :
                                    "Đoạn trả lời dài"
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
                            <h6 class="text-secondary mb-3">Danh sách lựa chọn:</h6>
                            <ul class="list-group list-group-flush">
                    `;
                    element.choices.forEach(choice => {
                        html += `
                            <li class="list-group-item d-flex justify-content-between align-items-center">
                                <span class="text-dark fw-bold">
                                    <i class="bi bi-chevron-right text-primary me-2"></i> ${choice.text}
                                </span>
                                <button class="btn btn-sm btn-outline-success" data-choice-id="${choice.value}">
                                    <i class="bi bi-plus-circle"></i> Thêm điều kiện
                                </button>
                            </li>
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
            <div class="alert alert-info text-center fw-bold">
                ${res.message}
            </div>`;
        body.html(html);
    }
}
$(".select2").select2();