$(document).ready(function () {
    LoadSurvey();
    $("#year").change(function () {
        LoadSurvey();
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
var initializing = true;
function LoadSurvey() {
    var defaultYearId = $('#year').val();
    $.ajax({
        url: "/CTDT/ThongKeKetQuaKhaoSatTheoYeuCau/LoadPKSByYear",
        type: "POST",
        data: { id: defaultYearId },
        success: function (res) {
            var surveyid = $("#surveyid");
            surveyid.empty();
            if (res.data.length === 0) {
                var html = '<option value="">Không có dữ liệu phiếu khảo sát cho năm học này</option>';
                surveyid.append(html);
                surveyid.val('').trigger('change');
            } else {
                res.data.forEach(function (Chil) {
                    var html = `<option value="${Chil.ma_phieu}">${Chil.ten_phieu}</option>`;
                    surveyid.append(html);
                });
                if (!initializing) {
                    $("#surveyid").val(res.data[0].ma_phieu).trigger('change');
                } else {
                    $("#surveyid").val(res.data[0].ma_phieu);
                }
            }
        }
    });
}


$(document).on('change', '#year_dap_vien_select', function () {
    load_dap_vien();
})
async function load_dap_vien() {
    var select_year = $('#year_dap_vien_select').val();
    var show_body = $('#body');
    const res = await $.ajax({
        url: '/api/load_doi_tuong_by_loai_khao_sat',
        type: 'POST',
        data: {
            ten_namhoc: select_year
        }
    });
    if (res.data.length >0) {
        var body = $('#show_title_collapsible');
        body.empty();
        let html = "";

        res.data.forEach(function (item, index) {
            html += generate_collapsible_card(item, index);
        });

        body.html(html);

        res.data.forEach(function (item, index) {
            var tableId = 'data_table_' + index;
            initializeDataTable(tableId);
        });
        show_body.show();
        var type = "success"
        var message = "Lọc dữ liệu thành công"
        Toast_alert(type, message)
    }
    else {
        show_body.hide();
        var type = "error"
        var message = "Không tìm thấy dữ liệu"
        Toast_alert(type, message)
    }
}

function generate_collapsible_card(item, index) {
    var collapseId = 'collapse' + index;
    var tableId = 'data_table_' + index;
    var html = "";
    html += '<div class="card-header">';
    html += '<h5 class="card-title">';
    html += '<a data-toggle="collapse" href="#' + collapseId + '" aria-expanded="false">';
    html += '<span>' + item.message + '</span>';
    html += '</a>';
    html += '</h5>';
    html += '</div>';
    html += '<div id="' + collapseId + '" class="collapse" data-parent="#accordion-default">';
    html += '<div class="card-body">';

    if (item.is_nguoi_hoc && item.data.length > 0) {
        html += generate_table_student(item.data, tableId);
    }
    else if (item.is_doanh_nghiep && item.data.length > 0) {
        html += generate_table_business(item.data, tableId);
    }
    else if (item.is_cbvc && item.data.length > 0) {
        html += generate_table_civilservants(item.data, tableId);
    }

    html += '</div>';
    html += '</div>';

    return html;
}

function generate_table_business(data, tableId) {
    var html = "";
    html += '<div class="m-t-25">';
    html += '<div class="table-responsive">';
    html += '<table class="table table-bordered" id="' + tableId + '">';
    html += '<thead>';
    html += '<tr>';
    html += '<th scope="col">STT</th>';
    html += '<th scope="col">Họ và tên</th>';
    html += '<th scope="col">Email</th>';
    html += '<th scope="col">Thuộc CTĐT</th>';
    html += '<th scope="col">Chọn</th>';
    html += '</tr>';
    html += '</thead>';
    html += '<tbody>';
    data.forEach(function (doanhnghiep, i) {
        html += '<tr>';
        html += '<td>' + (i + 1) + '</td>';
        html += '<td>' + doanhnghiep.ho_ten + '</td>';
        html += '<td>' + doanhnghiep.email + '</td>';
        html += '<td>' + doanhnghiep.ctdt + '</td>';
        html += '<td style="text-align:center;">';
        html += '<div class="form-check" style="transform: scale(1.5);">';
        html += '<input class="form-check-input" type="checkbox" id="gridCheck' + i + '" value="' + doanhnghiep.ho_ten + '" onchange="handleCheckboxChange(this)" >';
        html += '<label class="form-check-label" for="gridCheck' + i + '"></label>';
        html += '</div>';
        html += '</td>';
        html += '</tr>';
    });
    html += '</tbody>';
    html += '</table>';
    html += '</div>';
    html += '</div>';
    return html;
}

function generate_table_civilservants(data, tableId) {
    var html = "";
    html += '<div class="m-t-25">';
    html += '<div class="table-responsive">';
    html += '<table class="table table-bordered" id="' + tableId + '">';
    html += '<thead>';
    html += '<tr>';
    html += '<th scope="col">STT</th>';
    html += '<th scope="col">Họ và tên</th>';
    html += '<th scope="col">Thuộc CTĐT</th>';
    html += '<th scope="col">Thuộc Đơn vị</th>';
    html += '<th scope="col">Chọn</th>';
    html += '</tr>';
    html += '</thead>';
    html += '<tbody>';
    data.forEach(function (cbvc, i) {
        html += '<tr>';
        html += '<td>' + (i + 1) + '</td>';
        html += '<td>' + cbvc.ma_cbvc + '</td>';
        html += '<td>' + cbvc.ho_ten + '</td>';
        html += '<td>' + cbvc.thuoc_ctdt + '</td>';
        html += '<td>' + cbvc.thuoc_don_vi + '</td>';
        html += '<td style="text-align:center;">';
        html += '<div class="form-check" style="transform: scale(1.5);">';
        html += '<input class="form-check-input" type="checkbox" id="gridCheck' + i + '" value="' + cbvc.ma_cbvc + '" onchange="handleCheckboxChange(this)" >';
        html += '<label class="form-check-label" for="gridCheck' + i + '"></label>';
        html += '</div>';
        html += '</td>';
        html += '</tr>';
    });
    html += '</tbody>';
    html += '</table>';
    html += '</div>';
    html += '</div>';
    return html;
}
var selectedStudents = [];
function generate_table_student(data, tableId) {
    var html = "";
    html += '<div class="m-t-25">';
    html += '<div class="col-md-6" style="margin-bottom: 20px;">';
    html += '<label class="form-label" style="font-size: 16px; font-weight: bold; color: #333; display:block; margin-bottom: 10px;">Lọc theo Lớp</label>';
    html += '<select class="form-control lop_select" style="padding: 10px; font-size: 14px; width: 100%; height: 45px; border-radius: 5px;" data-table-id="' + tableId + '">';
    html += '<option value="">Tất cả</option>';
    var uniqueLops = [...new Set(data.map(nguoihoc => nguoihoc.lop))];
    uniqueLops.forEach(function (lop) {
        html += '<option value="' + lop + '">' + lop + '</option>';
    });
    html += '</select>';
    html += '</div>';
    html += '<div class="table-responsive">';
    html += '<table class="table table-bordered" id="' + tableId + '">';
    html += '<thead>';
    html += '<tr>';
    html += '<th scope="col">STT</th>';
    html += '<th scope="col">Họ và tên</th>';
    html += '<th scope="col">MSSV</th>';
    html += '<th scope="col">Thuộc lớp</th>';
    html += '<th scope="col">Chọn</th>';
    html += '</tr>';
    html += '</thead>';
    html += '<tbody>';
    data.forEach(function (nguoihoc, i) {
        html += '<tr>';
        html += '<td>' + (i + 1) + '</td>';
        html += '<td>' + nguoihoc.ho_ten + '</td>';
        html += '<td>' + nguoihoc.ma_nguoi_hoc + '</td>';
        html += '<td>' + nguoihoc.lop + '</td>';
        html += '<td style="text-align:center;">';
        html += '<div class="form-check" style="transform: scale(1.5);">';
        html += '<input class="form-check-input" type="checkbox" id="gridCheck' + i + '" value="' + nguoihoc.ma_nguoi_hoc + '" onchange="handleCheckboxChange(this)">';
        html += '<label class="form-check-label" for="gridCheck' + i + '"></label>';
        html += '</div>';
        html += '</td>';
        html += '</tr>';
    });
    html += '</tbody>';
    html += '</table>';
    html += '</div>';
    html += '</div>';
    return html;
}


function handleCheckboxChange(checkbox) {
    var studentId = checkbox.value;
    if (checkbox.checked) {
        selectedStudents.push(studentId);
    } else {
        var index = selectedStudents.indexOf(studentId);
        if (index > -1) {
            selectedStudents.splice(index, 1);  
        }
    }
    console.log(selectedStudents);
}
function initializeDataTable(tableId) {
    if ($.fn.DataTable.isDataTable('#' + tableId)) {
        $('#' + tableId).DataTable().clear().destroy();
    }
    var table = $('#' + tableId).DataTable({
        pageLength: 7,
        lengthMenu: [5, 10, 25, 50, 100],
        ordering: true,
        searching: true,
        language: {
            paginate: {
                next: "Next",
                previous: "Previous"
            },
            search: "Search",
            lengthMenu: "Show _MENU_ entries"
        },
        dom: "Bfrtip",
        buttons: ['csv', 'excel', 'print']
    });

    $('.lop_select[data-table-id="' + tableId + '"]').on('change', function () {
        var selectedLop = this.value;
        table.columns(3).search(selectedLop).draw();
    });
}