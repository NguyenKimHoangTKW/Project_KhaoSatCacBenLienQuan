$(".select2").select2();
let selected = [];
let option_checked = [];
$(document).ready(function () {
    $("#hedaotao, #year").on("change", load_pks_by_nam);

})
$(document).on("change", "#surveyid", function () {
    load_dap_vien_khao_sat();
});
$(document).on("change", "#checkAllRight", function () {
    const isChecked = $(this).prop("checked");
    $(".select-row").prop("checked", isChecked);
    selected = $(".select-row:checked").map(function () {
        return $(this).attr("data-id");
    }).get();
});
$(document).on("change", ".select-row", function () {
    selected = $(".select-row:checked").map(function () {
        return $(this).attr("data-id");
    }).get();
    $("#checkAllRight").prop("checked", $(".select-row").length === $(".select-row:checked").length);
});
$(document).on("click", "#SaveData", function (event) {
    event.preventDefault();
    save();
});
$(document).on("click", "#btnInfoDapVien", function (event) {
    event.preventDefault();
    load_info_dap_vien_da_chon();
});
async function load_pks_by_nam() {
    const hedaotao = $("#hedaotao").val();
    const year = $("#year").val();
    const res = await $.ajax({
        url: '/api/admin/load-phieu-by-nam-thuoc-can-bo-vien-chuc',
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
async function load_dap_vien_khao_sat() {
    const year = $("#year").val();
    const survey = $("#surveyid").val();
    const res = await $.ajax({
        url: '/api/admin/danh-sach-cbvc-khao-sat-phieu',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            year: year,
            surveyID: survey
        })
    });
    if ($.fn.DataTable.isDataTable('#data-table')) {
        $('#data-table').DataTable().clear().destroy();
    }
    const thead = $("#showthead");
    const tbody = $("#showdata");
    const title = $("#title_notification");
    thead.empty();
    tbody.empty();
    if (res.success) {
        title.hide();
        let htmlThead = `
        <tr>    
            <th><input type="checkbox" id="checkAllRight" /></th>
            <th>STT</th>
            <th>Mã cán bộ viên chức</th>
            <th>Họ và tên cán bộ viên chức</th>
            <th>Email</th>
            <th>Trình độ</th>
            <th>Chức vụ</th>
            <th>Thuộc đơn vị</th>
            <th>Thuộc bộ môn</th>
            <th>nganh_dao_tao</th>
            <th>Năm hoạt động</th>
            <th>Mô tả</th>
        </tr>
    `;
        thead.html(htmlThead);
        let htmlTbody = "";
        selected = res.selected.map(item => item.value);
        res.data.forEach((item, index) => {
            const isChecked = selected.includes(item.id) ? 'checked' : '';
            htmlTbody += `
            <tr>
                <td><input type="checkbox" class="select-row" data-id="${item.id}" ${isChecked} /></td>
                <td class="formatSo">${index + 1}</td>
                <td class="formatSo">${item.ma_cbvc}</td>
                <td>${item.ten_cbvc}</td>
                <td>${item.email}</td>
                <td>${item.trinh_do}</td>
                <td>${item.chuc_vu}</td>
                <td>${item.don_vi}</td>
                <td>${item.bo_mon}</td>
                <td>${item.nganh_dao_tao}</td>
                <td class="formatSo">${item.nam_hoat_dong}</td>
                <td>${item.mo_ta}</td>
            </tr>
        `;
        });
        tbody.html(htmlTbody);
        $('#data-table').DataTable({
            paging: false,
            ordering: true,
            searching: true,
            autoWidth: false,
            responsive: true,
            scrollY: "1000px",
            scrollX: true,
            scrollCollapse: true,
            fixedHeader: true
        });
    }
};
async function save() {
    const survey = $("#surveyid").val();
    const res = await $.ajax({
        url: '/api/admin/save-dap-vien-khao-sat-can-bo-vien-chuc',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            surveyID: survey,
            id_cbvc: selected
        })
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

async function load_info_dap_vien_da_chon() {
    const survey = $("#surveyid").val();
    const name_survey = $("#surveyid option:selected").text();
    const res = await $.ajax({
        url: '/api/admin/info-cbvc-da-chon-khao-sat',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            surveyID: survey
        })
    });
    if ($.fn.DataTable.isDataTable('#data-table-info')) {
        $('#data-table-info').DataTable().clear().destroy();
    }
    const thead = $("#showthead-info");
    const tbody = $("#showdata-info");
    thead.empty();
    tbody.empty();
    if (res.success) {
        $("#show-title-survey").text(name_survey);
        let htmlThead = `
        <tr>    
            <th><input type="checkbox" id="checkAllRight" /></th>
            <th>STT</th>
            <th>Mã cán bộ viên chức</th>
            <th>Họ và tên cán bộ viên chức</th>
            <th>Email</th>
            <th>Trình độ</th>
            <th>Chức vụ</th>
            <th>Thuộc đơn vị</th>
            <th>Thuộc bộ môn</th>
            <th>nganh_dao_tao</th>
            <th>Năm hoạt động</th>
            <th>Mô tả</th>
        </tr>
    `;
        thead.html(htmlThead);
        let htmlTbody = "";
        res.data.forEach((item, index) => {
            htmlTbody += `
            <tr>
                <td><input type="checkbox" class="select-row" data-id="${item.id}"  /></td>
                <td class="formatSo">${index + 1}</td>
                <td class="formatSo">${item.ma_cbvc}</td>
                <td>${item.ten_cbvc}</td>
                <td>${item.email}</td>
                <td>${item.trinh_do}</td>
                <td>${item.chuc_vu}</td>
                <td>${item.don_vi}</td>
                <td>${item.bo_mon}</td>
                <td>${item.nganh_dao_tao}</td>
                <td class="formatSo">${item.nam_hoat_dong}</td>
                <td>${item.mo_ta}</td>
            </tr>
        `;
        });
        tbody.html(htmlTbody);
        $('#data-table-info').DataTable({
            pageLength: 7,
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
                    extend: 'excel',
                    title: 'Danh sách đáp viên đã chọn khảo sát phiếu'
                },
                {
                    extend: 'print',
                    title: 'Danh sách đáp viên đã chọn khảo sát phiếu'
                }
            ]
        });
        $(".bd-example-modal-xl").modal("show");
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