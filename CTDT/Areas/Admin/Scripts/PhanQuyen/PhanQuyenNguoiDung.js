load_phan_quyen();

async function load_phan_quyen() {
    const res = await $.ajax({
        url: '/api/admin/load_chuc_nang_phan_quyen',
        type: 'GET',
    });
    let html = "";
    let body = $('#accordionExample');

    body.empty();
    res.data.forEach(function (items, index) {
        let headingId = `heading${index}`;
        let collapseId = `collapse${index}`;
        let programId = `program${index}`;

        if (items.is_admin) {
            form_chuc_nang_admin(items, body, html, headingId, collapseId, programId);
        }
        else if (items.is_ctdt) {
            form_chuc_nang_ctdt(items, body, html, headingId, collapseId, programId);
        }
    });
}

function form_chuc_nang_admin(items, body, html, headingId, collapseId, programId) {
    html = `  <!-- Reset html to avoid concatenation issues -->
            <div class="accordion-item">
                <div class="accordion-header" id="${headingId}">
                    <input type="checkbox" class="form-check-input program_checkbox" id="${programId}">
                    <button class="accordion-button" type="button" data-bs-toggle="collapse" data-bs-target="#${collapseId}" aria-expanded="true" aria-controls="${collapseId}">
                        ${items.ten_quyen}
                    </button>
                </div>
                <div id="${collapseId}" class="accordion-collapse collapse show" aria-labelledby="${headingId}" data-bs-parent="#accordionExample">`;

    items.chuc_nang.forEach(function (chucnang) {
        html += `
                    <div class="checkbox" style="margin: 10px 0px 10px 0px;">
                        <input type="checkbox" class="form-check-input file_checkbox ${programId}" id="${chucnang.ma_chuc_nang}">
                        <label class="form-check-label" for="${chucnang.ma_chuc_nang}">${chucnang.ten_chuc_nang}</label>
                    </div>`;
    });

    html += `</div>
            </div>`;

    body.append(html);
}

function form_chuc_nang_ctdt(items, body, html, headingId, collapseId, programId) {
    html = `  <!-- Reset html to avoid concatenation issues -->
            <div class="accordion-item">
                <div class="accordion-header" id="${headingId}">
                    <input type="checkbox" class="form-check-input program_checkbox" id="${programId}">
                    <button class="accordion-button" type="button" data-bs-toggle="collapse" data-bs-target="#${collapseId}" aria-expanded="true" aria-controls="${collapseId}">
                        ${items.ten_quyen}
                    </button>
                </div>
                <div id="${collapseId}" class="accordion-collapse collapse show" aria-labelledby="${headingId}" data-bs-parent="#accordionExample">`;

    items.chuc_nang.forEach(function (chucnang) {
        html += `
                    <div class="checkbox" style="margin: 10px 0px 10px 0px;">
                        <input type="checkbox" class="form-check-input file_checkbox ${programId}" id="${chucnang.ma_chuc_nang}">
                        <label class="form-check-label" for="${chucnang.ma_chuc_nang}">${chucnang.ten_chuc_nang}</label>
                    </div>`;
    });

    html += `</div>
            </div>`;

    body.append(html);
}
