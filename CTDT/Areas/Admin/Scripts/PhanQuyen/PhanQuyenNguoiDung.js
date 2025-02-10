var ma_user = $('#ma_user').text();
let selectedProgramId = null;
let selectedKhoaId = null;
let selectedCtdtIds = [];
$(document).ready(function () {
    load_quyen_user().then(() => {
        load_phan_quyen(); 
    });
});
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
    let isChecked = selectedProgramId === items.id_type ? 'checked' : '';
    let html = `
        <div class="accordion-item">
            <div class="accordion-header" id="${headingId}">
                <input type="radio" name="permissionGroup" class="form-check-input program_radio" id="${programId}" ${isChecked} onchange="handleRadioSelection('${items.id_type}')">
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
            <div class="checkbox" style="margin: 10px 0;">
                <input type="checkbox" class="form-check-input file_checkbox ${programId}" id="${chucnang.ma_chuc_nang}" style="margin-right: 0.5rem;">
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
                <table class="table table-bordered ctdtTable">
                    <thead>
                        <tr>
                            <th style="width: 10%;">STT</th>
                            <th style="width: 70%;">Tên CTĐT</th>
                            <th style="width: 20%;">Chọn</th>
                        </tr>
                    </thead>
                    <tbody>
    `;
    ctdtList.forEach(function (ctdt, index) {
        let isChecked = selectedCtdtIds.includes(ctdt.ma_ctdt) ? 'checked' : '';
        html += `
            <tr>
                <td class="text-center">${index + 1}</td>
                <td>${ctdt.ten_ctdt}</td>
                <td class="text-center">
                    <input type="checkbox" name="ctdtSelection" class="form-check-input ctdt_checkbox" id="ctdt_${ctdt.ma_ctdt}" ${isChecked} onchange="handleCtdtSelection('${ctdt.ma_ctdt}')">
                </td>
            </tr>
        `;
    });
    html += `</tbody></table></div></div>`;

    setTimeout(() => {
        initializeDataTable('.ctdtTable', 'Danh sách CTĐT');
    }, 0);

    return html;
}


function render_khoa_table(khoaList) {
    let html = `
        <div class="m-t-25">
            <div class="table-responsive">
                <table class="table table-bordered khoaTable">
                    <thead>
                        <tr>
                            <th style="width: 10%;">STT</th>
                            <th style="width: 70%;">Tên Khoa</th>
                            <th style="width: 20%;">Chọn</th>
                        </tr>
                    </thead>
                    <tbody>
    `;

    khoaList.forEach(function (khoa, index) {
        let isChecked = selectedKhoaId == khoa.ma_khoa ? 'checked' : ''; 
        html += `
            <tr>
                <td class="text-center">${index + 1}</td>
                <td>${khoa.ten_khoa}</td>
                <td class="text-center">
                    <input type="radio" name="khoaSelection" class="form-check-input khoa_checkbox" id="khoa_${khoa.ma_khoa}" ${isChecked} onchange="handleKhoaSelection('${khoa.ma_khoa}')">
                </td>
            </tr>
        `;
    });

    html += `</tbody></table></div></div>`;

    setTimeout(() => {
        initializeDataTable('.khoaTable', 'Danh sách Khoa');
    }, 0);

    return html;
}
function handleRadioSelection(programId) {
    selectedProgramId = programId;
}

function handleKhoaSelection(khoaId) {
    selectedKhoaId = khoaId;
}

function handleCtdtSelection(ctdtId) {
    let index = selectedCtdtIds.indexOf(ctdtId);
    if (index === -1) {
        selectedCtdtIds.push(ctdtId);
    } else {
        selectedCtdtIds.splice(index, 1);
    }
}
async function load_quyen_user() {
    const res = await $.ajax({
        url: '/api/admin/load_quyen_user',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ ma_user: ma_user })
    });

    if (res.data && res.data.length > 0) {
        const userData = res.data[0];

        selectedProgramId = userData.ma_quyen;
        if (selectedProgramId === 3) {
            selectedCtdtIds = userData.ma_ctdt.map(item => item.id_ctdt);
        }
        if (selectedProgramId === 5 && userData.ma_khoa.length > 0) {
            selectedKhoaId = userData.ma_khoa[0].id_khoa;
        }
    }
}

function handleSave() {
    selectedCtdtIds = [];
    $('.ctdt_checkbox:checked').each(function () {
        selectedCtdtIds.push($(this).attr('id').replace('ctdt_', ''));
    });

    $.ajax({
        url: '/api/admin/save_phan_quyen',
        type: 'POST',
        data: JSON.stringify({
            ma_user: ma_user,
            ma_quyen: selectedProgramId,
            ma_ctdt: selectedCtdtIds,
            ma_khoa: selectedKhoaId
        }),
        contentType: 'application/json',
        success: function (res) {
            Toast_alert("success", res.message);
        }
    });
}

function initializeDataTable(tableClass, title) {
    $(tableClass).DataTable({
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
                title: `${title} - CSV`
            },
            {
                extend: 'excel',
                title: `${title} - Excel`
            },
            {
                extend: 'pdf',
                title: `${title} - PDF`
            },
            {
                extend: 'print',
                title: `${title}`
            }
        ]
    });
}
