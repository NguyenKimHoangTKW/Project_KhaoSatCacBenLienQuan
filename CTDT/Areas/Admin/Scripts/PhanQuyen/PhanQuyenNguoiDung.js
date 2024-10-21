load_phan_quyen();

async function load_phan_quyen() {
    const res = await $.ajax({
        url: '/api/admin/load_chuc_nang_phan_quyen',
        type: 'GET',
    });

    let body = $('#accordionExample');
    body.empty(); 

    res.data.forEach(function (items, index) {
        let headingId = `heading${index}`;
        let collapseId = `collapse${index}`;
        let programId = `program${index}`;

        form_chuc_nang(items, body, headingId, collapseId, programId);
    });
}

function form_chuc_nang(items, body, headingId, collapseId, programId) {
    let html = `
        <div class="accordion-item">
            <div class="accordion-header" id="${headingId}">
                <input type="checkbox" class="form-check-input program_checkbox" id="${programId}">
                <button class="accordion-button" type="button" data-bs-toggle="collapse" data-bs-target="#${collapseId}" aria-expanded="false" aria-controls="${collapseId}">
                    ${items.ten_quyen}
                </button>
            </div>
            <div id="${collapseId}" class="accordion-collapse collapse" aria-labelledby="${headingId}" data-bs-parent="#accordionExample">
    `;

    if (items.is_admin || items.is_ctdt || items.is_khoa) {
        if (items.chuc_nang) {
            html += render_chuc_nang_section(items.chuc_nang, programId);
        }
       
    }

    if (items.is_ctdt) {
        html += render_ctdt_table(items.ctdt);
    }

    if (items.is_khoa) {
        html += render_khoa_table(items.khoa);
    }

    html += `</div></div>`;

    body.append(html);
}

function render_chuc_nang_section(chucNangList, programId) {
    let html = '';
    chucNangList.forEach(function (chucnang) {
        html += `
            <div class="checkbox" style="margin: 10px 0px 10px 0px;">
                <input type="checkbox" class="form-check-input file_checkbox ${programId}" id="${chucnang.ma_chuc_nang}">
                <label class="form-check-label" for="${chucnang.ma_chuc_nang}">${chucnang.ten_chuc_nang}</label>
            </div>
        `;
    });
    return html;
}
function render_ctdt_table(ctdtList) {
    let html = `
        <div class="m-t-25">
            <div class="table-responsive">
                <table class="table table-bordered">
                    <thead>
                        <tr>
                            <th>STT</th>
                            <th>Tên CTĐT</th>
                            <th>Chọn</th>
                        </tr>
                    </thead>
                    <tbody>
    `;
    ctdtList.forEach(function (ctdt, index) {
        html += `
            <tr>
                <td>${index + 1}</td>
                <td>${ctdt.ten_ctdt}</td>
                <td><input type="checkbox" class="form-check-input ctdt_checkbox" id="ctdt_${ctdt.ma_ctdt}"></td>
            </tr>
        `;
    });
    html += `</tbody></table></div></div>`;
    return html;
}
function render_khoa_table(khoaList) {
    let html = `
        <div class="m-t-25">
            <div class="table-responsive">
                <table class="table table-bordered">
                    <thead>
                        <tr>
                            <th>STT</th>
                            <th>Tên Khoa</th>
                            <th>Chọn</th>
                        </tr>
                    </thead>
                    <tbody>
    `;
    khoaList.forEach(function (khoa, index) {
        html += `
            <tr>
                <td>${index + 1}</td>
                <td>${khoa.ten_khoa}</td>
                <td><input type="checkbox" class="form-check-input ctdt_checkbox" id="khoa_${khoa.ma_khoa}"></td>
            </tr>
        `;
    });
    html += `</tbody></table></div></div>`;
    return html;
}
