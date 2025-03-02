$(".select2").select2();
let selected = [];
let option_checked = [];
let check_array_msnh = false;

$(document).ready(function () {
    $("#hedaotao, #year").on("change", load_pks_by_nam);

})
$(document).on("change", "#surveyid", function () {
    load_lop_by_hdt();
});

$(document).on("click", "#SaveData", function (event) {
    event.preventDefault();
    save();
});
$("#hedaotao").trigger("change");
$(document).on("change", "#checkAllRight", function () {
    const check_number_array = $("#optionsTextarea").val().trim();
    if (check_number_array === "") {
        check_array_msnh = false;
    }
    else {
        check_array_msnh = true;
    }
    if (check_array_msnh) {
        const isChecked = $(this).prop("checked");

        $(".select-row").prop("checked", isChecked);

        if (isChecked) {
            selected = $(".select-row").map(function () {
                return $(this).attr("data-id");
            }).get();
        } else {
            selected = [];
        }
    }
    else {
        const isChecked = $(this).prop("checked");
        $(".select-row").prop("checked", isChecked);
        selected = $(".select-row:checked").map(function () {
            return $(this).attr("data-id");
        }).get();
    }

});
$(document).on("change", ".select-row", function () {
    const check_number_array = $("#optionsTextarea").val().trim();
    if (check_number_array === "") {
        check_array_msnh = false;
    }
    else {
        check_array_msnh = true;
    }
    if (check_array_msnh) {
        let id = $(this).attr("data-id");
        let isChecked = $(this).prop("checked");

        if (isChecked) {
            if (!selected.includes(id)) selected.push(id);
        } else {
            selected = selected.filter(item => item !== id);
        }
    }
    else {
        selected = $(".select-row:checked").map(function () {
            return $(this).attr("data-id");
        }).get();
        $("#checkAllRight").prop("checked", $(".select-row").length === $(".select-row:checked").length);
    }


});

$(document).on("click", "#btnFilter", function (event) {
    event.preventDefault();
    load_dap_vien_chon_khao_sat();
});
$(document).on("change", "#lop", function () {
    load_dap_vien_chon_khao_sat();
});
$(document).on("click", "#btnInfoDapVien", function (event) {
    event.preventDefault();
    load_info_dap_vien_da_chon();   
});
async function load_pks_by_nam() {
    const hedaotao = $("#hedaotao").val();
    const year = $("#year").val();
    const res = await $.ajax({
        url: '/api/admin/load-phieu-by-nam-thuoc-nguoi-hoc',
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
async function save() {
    const survey = $("#surveyid").val();
    const res = await $.ajax({
        url: '/api/admin/save-dap-vien-khao-sat-nguoi-hoc',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            surveyID: survey,
            id_nh: selected
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
async function load_dap_vien_chon_khao_sat() {
    const lop = $("#lop").val();
    const inputText = $('#optionsTextarea').val();
    option_checked = inputText.split("\n").filter(line => line.trim() !== "");
    const survey = $("#surveyid").val();
    const res = await $.ajax({
        url: '/api/admin/danh-sach-dap-vien-chon-khao-sat',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            id_lop: lop,
            ma_nh: option_checked,
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
            <th>Mã người học</th>
            <th>Họ và tên</th>
            <th>Thuộc lớp</th>
        </tr>
    `;
        thead.html(htmlThead);
        selected = res.selected.map(item => item.value);
        let htmlTbody = "";
        res.data.forEach((item, index) => {
            const isChecked = selected.includes(item.id) ? 'checked' : '';
            htmlTbody += `
            <tr>
                <td><input type="checkbox" class="select-row" data-id="${item.id}" ${isChecked} /></td>
                <td class="formatSo">${index + 1}</td>
                <td class="formatSo">${item.ma_sv}</td>
                <td>${item.hovaten}</td>
                <td class="formatSo">${item.ma_lop}</td>
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

async function load_lop_by_hdt() {
    const hdt = $("#hedaotao").val();
    const survey = $("#surveyid").val();
    const res = await $.ajax({
        url: '/api/admin/lop-by-hdt',
        type: 'POST',    
        contentType: 'application/json',
        data: JSON.stringify({
            id_hdt: hdt,
            surveyID: survey
        })
    });
    let html = "";
    if (res.success) {
        res.data.forEach(function (item) {
            html += `<option value="${item.value}">${item.name}</option>`;
        });
        $("#lop").empty().html(html).trigger("change");
        const selectedValues = res.lop.map(item => item.value);
        $("#lop").val(selectedValues).trigger("change");
    } else {
        html += `<option value="0">${res.message}</option>`;
        $("#lop").empty().html(html).trigger("change");
    }
};

async function load_info_dap_vien_da_chon() {
    const survey = $("#surveyid").val();
    const name_survey = $("#surveyid option:selected").text();
    const res = await $.ajax({
        url: '/api/admin/info-nguoi-hoc-da-chon-khao-sat',
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
            <th>STT</th>
            <th>Mã người học</th>
            <th>Họ và tên</th>
            <th>Thuộc lớp</th>
        </tr>
    `;
        thead.html(htmlThead);
        let htmlTbody = "";
        res.data.forEach((item, index) => {
            htmlTbody += `
            <tr>
                <td class="formatSo">${index + 1}</td>
                <td class="formatSo">${item.ma_sv}</td>
                <td>${item.hovaten}</td>
                <td class="formatSo">${item.ma_lop}</td>
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
        $(".bd-example-modal-lg").modal("show");
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