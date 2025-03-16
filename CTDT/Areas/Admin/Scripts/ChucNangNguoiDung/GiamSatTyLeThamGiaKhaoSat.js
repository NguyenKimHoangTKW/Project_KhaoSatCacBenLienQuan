$(".select2").select2();
let check_option = null;
$(document).ready(function () {
    load_pks_by_nam();
    $("#hedaotao, #year").on("change", load_pks_by_nam);

    $("#fildata").click(function () {
        LoadChartSurvey()
    });
});

async function load_pks_by_nam() {
    const hedaotao = $("#hedaotao").val();
    const year = $("#year").val();
    const res = await $.ajax({
        url: '/api/admin/load-option-giam-sat-ty-le-tham-gia-khao-sat',
        type: 'POST',
        contentType: "application/json",
        data: JSON.stringify({
            id_namhoc: year,
            id_hedaotao: hedaotao,
            check_option: check_option
        })
    });

    let html = "";
    const body = $("#load-option");
    body.empty();
    if (res.success) {
        if (res.is_cbvc_gv) {
            $("#Result").empty();
            html += render_option_cbvc(body, res.data);
        }
        else if (res.is_nguoi_hoc) {
            html += `
                    <div class="col-12 d-flex justify-content-center">
                        <div class="d-flex gap-3">
                            <button class="btn  btn-tone m-r-5 px-4" id="btnYes" style="margin-right: 33px;">
                                Lọc 3 bước (Đơn vị => Khoa => Bộ môn)
                            </button>
                            <button class="btn  btn-tone m-r-5 px-4" id="btnNo">
                                Lọc theo chương trình đào tạo
                            </button>
                        </div>
                    </div>
                `;
                    body.html(html);

            if (res.data.length > 0) {
                updateUINH(res.data);
            }
        }
    }
    else {

    }
}
$(document).on("click", "#btnYes", function (event) {
    event.preventDefault();
    check_option = true;
    load_pks_by_nam();
});

$(document).on("click", "#btnNo", function (event) {
    event.preventDefault();
    check_option = false;
    load_pks_by_nam();
});
function updateUINH(data) {
    let resultContainer = $("#emailResult");
    resultContainer.html(check_option ? render_option_nguoi_hoc_checked(data) : render_option_nguoi_hoc_no_checked(data));
}
async function render_option_cbvc(body, data) {
    let html = "";
    html += `
            <div class="col-md-6">
                <label class="form-label" style="font-size: 16px; font-weight: bold; color: #333;">Chọn đơn vị</label>
                <select class="form-control select2" id="don_vi">
                    <option value="">Tất cả</option>
                    ${data.map(donvi => `<option value="${donvi.don_vi.value}">${donvi.don_vi.name}</option>`).join("")}
                </select>
            </div>
        `;

    html += `
            <div class="col-md-6">
                <label class="form-label" style="font-size: 16px; font-weight: bold; color: #333;">Chọn khoa</label>
                <select class="form-control select2" id="khoa">
                   <option value="">Chọn khoa</option>
                </select>
            </div>
        `;

    html += `
            <div class="col-md-6">
                <label class="form-label" style="font-size: 16px; font-weight: bold; color: #333;">Chọn bộ môn</label>
                <select class="form-control select2" id="bo_mon">
                    <option value="">Chọn bộ môn</option>
                </select>
            </div>
        `;

    body.append(html);
    $("#don_vi").change(function () {
        const selectedDonVi = $(this).val();
        const khoaDropdown = $("#khoa");
        khoaDropdown.empty().append(` <option value="">Tất cả</option>`);

        if (selectedDonVi) {
            const selectedData = data.find(d => d.don_vi.value == selectedDonVi);
            if (selectedData) {
                selectedData.khoa_data.forEach(khoa => {
                    khoaDropdown.append(`<option value="${khoa.khoa.value}">${khoa.khoa.name}</option>`);
                });
            }
        }
        khoaDropdown.trigger("change");
    });

    $("#khoa").change(function () {
        const selectedKhoa = $(this).val();
        const boMonDropdown = $("#bo_mon");
        boMonDropdown.empty().append(` <option value="">Tất cả</option>`);

        if (selectedKhoa) {
            const selectedDonVi = $("#don_vi").val();
            const selectedData = data.find(d => d.don_vi.value == selectedDonVi);

            if (selectedData) {
                const selectedKhoaData = selectedData.khoa_data.find(k => k.khoa.value == selectedKhoa);
                if (selectedKhoaData) {
                    selectedKhoaData.bo_mon.forEach(bm => {
                        boMonDropdown.append(`<option value="${bm.value}">${bm.name}</option>`);
                    });
                }
            }
        }
    });
    setTimeout(() => {
        $("#don_vi,#khoa,#bo_mon").select2();
    }, 500);
    return html;
}
async function render_option_nguoi_hoc_checked(data) {
    let html = `
        <div class="col-md-6">
            <label class="form-label"><b>Chọn đơn vị</b></label>
            <select class="form-control select2" id="don_vi">
                <option value="">Tất cả</option>
                ${data.map(donvi => `<option value="${donvi.don_vi.value}">${donvi.don_vi.name}</option>`).join("")}
            </select>
        </div>
        <div class="col-md-6">
            <label class="form-label"><b>Chọn khoa</b></label>
            <select class="form-control select2" id="khoa">
                <option value="">Chọn khoa</option>
            </select>
        </div>
        <div class="col-md-6">
            <label class="form-label"><b>Chọn bộ môn</b></label>
            <select class="form-control select2" id="bo_mon">
                <option value="">Chọn bộ môn</option>
            </select>
        </div>
    `;

    $("#Result").html(html);

    setTimeout(() => {
        $("#don_vi, #khoa, #bo_mon").select2();
    }, 300);

    $("#don_vi").change(function () {
        const selectedDonVi = $(this).val();
        const khoaDropdown = $("#khoa").empty().append(`<option value="">Tất cả</option>`);

        if (selectedDonVi) {
            const selectedData = data.find(d => d.don_vi.value == selectedDonVi);
            if (selectedData) {
                selectedData.khoa_data.forEach(khoa => {
                    khoaDropdown.append(`<option value="${khoa.khoa.value}">${khoa.khoa.name}</option>`);
                });
            }
        }
        khoaDropdown.trigger("change");
    });

    $("#khoa").change(function () {
        const selectedKhoa = $(this).val();
        const boMonDropdown = $("#bo_mon").empty().append(`<option value="">Tất cả</option>`);

        if (selectedKhoa) {
            const selectedDonVi = $("#don_vi").val();
            const selectedData = data.find(d => d.don_vi.value == selectedDonVi);
            if (selectedData) {
                const selectedKhoaData = selectedData.khoa_data.find(k => k.khoa.value == selectedKhoa);
                if (selectedKhoaData) {
                    selectedKhoaData.bo_mon.forEach(bm => {
                        boMonDropdown.append(`<option value="${bm.value}">${bm.name}</option>`);
                    });
                }
            }
        }
    });

    return html;
}

async function render_option_nguoi_hoc_no_checked(data) {
    let html = ``;
    const itemsList = Array.isArray(data) && Array.isArray(data[0]) ? data[0] : [];
    html += `
        <div class="col-md-6">
            <label class="form-label"><b>Chọn chương trình đào tạo</b></label>
            <select class="form-control select2" id="ctdt">
                <option value="">Tất cả</option>`;

    itemsList.forEach(items => {
        html += `<option value="${items.value}">${items.text}</option>`;
    });

    html += `</select>
        </div>
    `;

    $("#Result").html(html);

    setTimeout(() => {
        $("#ctdt").select2();
    }, 300);

    return html;
}


async function LoadChartSurvey() {
    const hedaotao = $("#hedaotao").val();
    const year = $("#year").val();
    const ctdt = $("#ctdt").val();
    const from_date = $('#from_date').val();
    const to_date = $('#to_date').val();
    const startTimestamp = Math.floor(new Date(from_date).getTime() / 1000);
    const endTimestamp = Math.floor(new Date(to_date).getTime() / 1000);
    const res = await $.ajax({
        url: '/api/admin/giam-sat-ty-le-tham-gia-khao-sat',
        type: 'POST',
        data: {
            id_namhoc: year,
            id_ctdt: ctdt,
            id_hdt: hedaotao,
            from_date: startTimestamp,
            to_date: endTimestamp
        },
    });

    $('#survey-list').empty();

    if (res.success) {
        const surveys = res.data;
        surveys.sort((a, b) => {
            const idA = a.ten_phieu.split(".")[0].replace(/\D/g, "");
            const idB = b.ten_phieu.split(".")[0].replace(/\D/g, "");
            return parseInt(idA) - parseInt(idB);
        });

        surveys.forEach((survey) => {
            const tenPhieuParts = survey.ten_phieu.split(".");
            const MaPhieu = tenPhieuParts[0].toUpperCase();
            const TieuDePhieu = tenPhieuParts[1]?.trim() || "Không có tiêu đề";

            const thongKeTyLe = {
                tong_khao_sat: survey.tong_khao_sat,
                tong_phieu_da_tra_loi: survey.tong_phieu_da_tra_loi,
                tong_phieu_chua_tra_loi: survey.tong_phieu_chua_tra_loi,
            };

            const card = `
                <div class="card survey-card">
                    <div class="card-body">
                        <div style="align-items: center;">
                            <p style="color:#5029ff;font-weight:bold; position: absolute; top: 0; left: 20px;">${MaPhieu}</p>
                            <a href="#" style="color:#5029ff;font-weight:bold; position: absolute; top: 14px; right: 20px;" data-toggle="modal" data-target=".bd-example-modal-lg" id="maphieu" data-tenphieu="${survey.id_phieu}">Xem chi tiết</a>
                            <hr/>
                            <p style="color:black;font-weight:bold">${TieuDePhieu}</p>
                            <hr/>
                        </div>
                        <canvas class="chart" id="donut-chart-${MaPhieu}"></canvas>
                        <p id="surveyedInfo-${MaPhieu}" style="margin: 0; color: red;"></p>
                        <hr />
                        <div style="display: flex; justify-content: space-between; align-items: center; font-weight:bold">
                            <p style="margin: 0; color: black;">Tổng phiếu: ${thongKeTyLe.tong_khao_sat || '0'}</p>
                            <p style="margin: 0; color:#5029ff;">Đã thu về: ${thongKeTyLe.tong_phieu_da_tra_loi || '0'}</p>
                            <p style="margin: 0; color:#ebb000;">Chưa thu về: ${thongKeTyLe.tong_phieu_chua_tra_loi || '0'}</p>
                        </div>
                    </div>
                </div>`;

            $('#survey-list').append(card);

            const donutCtx = document.getElementById(`donut-chart-${MaPhieu}`).getContext('2d');

            if (thongKeTyLe.tong_khao_sat > 0) {
                const datas = [thongKeTyLe.tong_phieu_chua_tra_loi, thongKeTyLe.tong_phieu_da_tra_loi];
                const colors = ['#ffc107', '#007bff'];

                const donutData = {
                    labels: ['Số phiếu chưa trả lời', 'Số phiếu đã thu'],
                    datasets: [{
                        fill: true,
                        backgroundColor: colors,
                        pointBackgroundColor: colors,
                        data: datas
                    }]
                };

                new Chart(donutCtx, {
                    type: 'doughnut',
                    data: donutData,
                    options: {
                        maintainAspectRatio: false,
                        hover: { mode: null },
                        cutoutPercentage: 45
                    }
                });
            } else {
                const donutData = {
                    labels: ['Không có dữ liệu'],
                    datasets: [{
                        fill: true,
                        backgroundColor: ['#d3d3d3'],
                        pointBackgroundColor: ['#d3d3d3'],
                        data: [1]
                    }]
                };

                new Chart(donutCtx, {
                    type: 'doughnut',
                    data: donutData,
                    options: {
                        maintainAspectRatio: false,
                        hover: { mode: null },
                        cutoutPercentage: 45
                    }
                });

                $(`#surveyedInfo-${MaPhieu}`).text('Không có dữ liệu');
            }
        });
    } else {
        const card = `
            <div class="alert alert-info" style="width: 100%; text-align: center;">
                <h5 class="alert-heading">Opps...</h5>
                <p>Không có dữ liệu phiếu khảo sát cho năm học này!</p>
            </div>`;
        $('#survey-list').append(card);
    }
}

