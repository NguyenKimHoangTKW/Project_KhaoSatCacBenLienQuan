let selected = [];
// Các sự kiện
$(document).ready(function () {

});
$(document).on("change", "#find-ctdt", function () {
    load_survey();
});

$(document).on("change", "#checkAllRight", function () {
    const isChecked = $(this).prop("checked");
    $(".select-row").prop("checked", isChecked);
    selected = isChecked ? $(".select-row").map(function () {
        return $(this).attr("data-id");
    }).get() : [];
});

$(document).on("change", ".select-row", function () {
    let id = $(this).attr("data-id");
    let isChecked = $(this).prop("checked");

    if (isChecked) {
        if (!selected.includes(id)) selected.push(id);
    } else {
        selected = selected.filter(item => item !== id);
    }
});

$(document).on("click", "#SaveData", function (event) {
    event.preventDefault();
    save_doi_tuong();
});
$(document).on("change", "#find-survey", function () {
    load_data();
});
$(document).on("click", "#btnFilter", function (event) {
    event.preventDefault();
    const name_survey = $("#find-survey option:selected").text();
    $("#modal-title").text(name_survey);
    load_dap_vien_thong_ke_da_chon();
})

$(document).on("click", "#btnDelete", function (event) {
    event.preventDefault();
    const value = $(this).data("id");
    Swal.fire({
        title: "Bạn đang thao tác xóa đáp viên đã chọn?",
        text: "Bạn muốn tiếp tục!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Có, tiếp tục!"
    }).then((result) => {
        if (result.isConfirmed) {
            delete_dap_vien(value)
        }
    });
});


// Các hàm
async function load_survey() {
    const year = $("#yearGiamSat").val();
    const ctdt = $("#find-ctdt").val();
    const res = await $.ajax({
        url: '/api/ctdt/load-survey-check-ctdt',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            id_namhoc: year,
            id_ctdt: ctdt
        })
    });
    const body = $("#find-survey");
    body.empty();
    let html = ``;
    if (res.success) {
        const data = JSON.parse(res.data);
        data.sort((a, b) => {
            const idA = a.name.split(".")[0];
            const idB = b.name.split(".")[0];
            return idA.localeCompare(idB, undefined, { numeric: true });
        });
        data.forEach(items => {
            html += `<option value="${items.value}">${items.name}</option>`
        });
        body.html(html).trigger("change");
    }
    else {
        html += `<option value="">${res.message}</option>`
        body.html(html).trigger("change");
    }
};

async function load_data() {
    const survey = $("#find-survey").val();
    const ctdt = $("#find-ctdt").val();
    const res = await $.ajax({
        url: "/api/ctdt/load-doi-tuong-thong-ke-theo-yeu-cau",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            surveyID: survey,
            id_ctdt: ctdt
        })
    });

    const thead = $("#showthead");
    const tbody = $("#showdata");
    const title = $("#title_notification");

    thead.empty();
    tbody.empty();

    if (res.success) {
        title.hide();
        const isNguoiHocCoHocPhan = res.check_object.some(obj => obj.is_nguoi_hoc_co_hoc_phan_khao_sat === true);
        const isCBVC = res.check_object.some(obj => obj.is_cbvc === true);
        const isDoanhNghiep = res.check_object.some(obj => obj.is_doanh_nghiep === true);
        const isNguoiHocKhaoSat = res.check_object.some(obj => obj.is_nguoi_hoc_khao_sat === true);
        const data = JSON.parse(res.data)
        if (isNguoiHocCoHocPhan) {
            await form_nguoi_hoc_co_hoc_phan_khao_sat(thead, tbody, data);
        }
        else if (isCBVC) {
            await form_cbvc_khao_sat(thead, tbody, data);
        }
        else if (isDoanhNghiep) {
            await form_doanh_nghiep(thead, tbody, data);
        }
        else if (isNguoiHocKhaoSat) {
            await form_nguoi_hoc_khao_sat(thead, tbody, data);
        }
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
async function load_dap_vien_thong_ke_da_chon() {
    const survey = $("#find-survey").val();
    const ctdt = $("#find-ctdt").val();
    const res = await $.ajax({
        url: '/api/ctdt/load-dap-vien-chon-theo-yeu-cau',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            surveyID: survey,
            id_ctdt: ctdt
        })
    });
    const thead = $("#showheadinfo");
    const tbody = $("#showdatainfo");
    thead.empty();
    tbody.empty();
    if (res.success) {
        const isNguoiHocCoHocPhan = res.check_object.some(obj => obj.is_nguoi_hoc_co_hoc_phan_khao_sat === true);
        const isCBVC = res.check_object.some(obj => obj.is_cbvc === true);
        const isDoanhNghiep = res.check_object.some(obj => obj.is_doanh_nghiep === true);
        const isNguoiHocKhaoSat = res.check_object.some(obj => obj.is_nguoi_hoc_khao_sat === true);
        const data = JSON.parse(res.data);
        if (isNguoiHocCoHocPhan) {
            await form_nguoi_hoc_co_hoc_phan_khao_sat_info(thead, tbody, data);
        }
        else if (isCBVC) {
            await form_cbvc_khao_sat_info(thead, tbody, data);
        }
        else if (isDoanhNghiep) {
            await form_doanh_nghiep_info(thead, tbody, data);
        }
        else if (isNguoiHocKhaoSat) {
            await form_nguoi_hoc_khao_sat_info(thead, tbody, data);
        }
        $("#exampleModalCenter").modal("show");
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
function form_nguoi_hoc_co_hoc_phan_khao_sat(thead, tbody, items) {
    let htmlThead = `
        
        <tr>    
            <th><input type="checkbox" id="checkAllRight" /></th>
            <th>STT</th>
            <th>Tên Môn Học</th>
            <th>Giảng viên giảng dạy</th>
            <th>Thuộc lớp</th>
            <th>Mã người học</th>
            <th>Tên người học</th>
        </tr>
        <tr>
            <th></th>
            <th></th>
            <th><input type="text" class="form-control" data-index="1" placeholder="Lọc theo môn học"></th>
            <th><input type="text" class="form-control" data-index="1" placeholder="Lọc theo giảng viên giảng dạy"></th>
            <th><input type="text" class="form-control" data-index="2" placeholder="Lọc theo lớp"></th>
            <th><input type="text" class="form-control" data-index="3" placeholder="Lọc theo mã người học"></th>
            <th><input type="text" class="form-control" data-index="4" placeholder="Lọc theo tên người học"></th>
        </tr>
    `;
    thead.html(htmlThead);

    let htmlTbody = "";
    items.forEach((item, index) => {
        htmlTbody += `
            <tr>
                <td><input type="checkbox" class="select-row" data-id="${item.id}" /></td>
                <td class="formatSo">${index + 1}</td>
                <td>${item.mon_hoc}</td>
                <td>${item.giang_vien_giang_day}</td>
                <td class="formatSo">${item.ma_lop}</td>
                <td class="formatSo">${item.ma_nguoi_hoc}</td>
                <td>${item.ten_nguoi_hoc}</td>
            </tr>
        `;
    });

    tbody.html(htmlTbody);
}
function form_cbvc_khao_sat(thead, tbody, items) {
    let htmlThead = `
        
        <tr>    
            <th><input type="checkbox" id="checkAllRight" /></th>
            <th>STT</th>
            <th>Tên CBVC/GV khảo sát</th>
            <th>Chương trình đào tạo</th>
        </tr>
        <tr>
            <th></th>
            <th></th>
            <th><input type="text" class="form-control" data-index="1" placeholder="Lọc tên CBVC/GV khảo sát"></th>
            <th><input type="text" class="form-control" data-index="2" placeholder="Chương trình đào tạo"></th>
        </tr>
    `;
    thead.html(htmlThead);

    let htmlTbody = "";
    items.forEach((item, index) => {
        htmlTbody += `
            <tr>
                <td><input type="checkbox" class="select-row" data-id="${item.id}" /></td>
                <td class="formatSo">${index + 1}</td>
                <td>${item.TenCBVC}</td>
                <td>${item.ctdt}</td>
            </tr>
        `;
    });

    tbody.html(htmlTbody);
}
function form_doanh_nghiep(thead, tbody, items) {
    let htmlThead = `
        
        <tr>    
            <th><input type="checkbox" id="checkAllRight" /></th>
            <th>STT</th>
            <th>Email</th>
            <th>Tên người khảo sát</th>
            <th>Chương trình đào tạo</th>
        </tr>
        <tr>
            <th></th>
            <th></th>
            <th><input type="text" class="form-control" data-index="1" placeholder="Lọc Email"></th>
            <th><input type="text" class="form-control" data-index="2" placeholder="Lọc tên người khảo sát"></th>
            <th><input type="text" class="form-control" data-index="2" placeholder="Lọc chương trình đào tạo"></th>
        </tr>
    `;
    thead.html(htmlThead);

    let htmlTbody = "";
    items.forEach((item, index) => {
        htmlTbody += `
            <tr>
                <td><input type="checkbox" class="select-row" data-id="${item.id}" /></td>
                <td class="formatSo">${index + 1}</td>
                <td>${item.email}</td>
                <td>${item.ten_user}</td>
                <td>${item.ctdt}</td>
            </tr>
        `;
    });

    tbody.html(htmlTbody);
}
function form_nguoi_hoc_khao_sat(thead, tbody, items) {
    let htmlThead = `
        
        <tr>    
            <th><input type="checkbox" id="checkAllRight" /></th>
            <th>STT</th>
            <th>Mã người học</th>
            <th>Tên người học</th>
            <th>Thuộc lớp</th>
            <th>Chương trình đào tạo</th>
        </tr>
        <tr>
            <th></th>
            <th></th>
            <th><input type="text" class="form-control" data-index="1" placeholder="Lọc mã người học"></th>
            <th><input type="text" class="form-control" data-index="2" placeholder="Lọc tên người học"></th>
            <th><input type="text" class="form-control" data-index="2" placeholder="Lọc lớp"></th>
            <th><input type="text" class="form-control" data-index="2" placeholder="Lọc chương trình đào tạo"></th>
        </tr>
    `;
    thead.html(htmlThead);

    let htmlTbody = "";
    items.forEach((item, index) => {
        htmlTbody += `
            <tr>
                <td><input type="checkbox" class="select-row" data-id="${item.id}" /></td>
                <td class="formatSo">${index + 1}</td>
                <td>${item.ma_nguoi_hoc}</td>
                <td>${item.ten_nguoi_hoc}</td>
                <td>${item.ma_lop}</td>
                <td>${item.ten_ctdt}</td>
            </tr>
        `;
    });

    tbody.html(htmlTbody);
}
async function save_doi_tuong() {
    const survey = $("#find-survey").val();
    const ctdt = $("#find-ctdt").val();
    const res = await $.ajax({
        url: '/api/ctdt/save-doi-tuong-khao-sat-theo-yeu-cau',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            surveyID: survey,
            id_ctdt: ctdt,
            list_value: selected
        })
    });
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
// Form đáp viên yêu cầu
function form_nguoi_hoc_co_hoc_phan_khao_sat_info(thead, tbody, items) {
    let htmlThead = `
        
        <tr>    
            <th>STT</th>
            <th>Tên Môn Học</th>
            <th>Giảng viên giảng dạy</th>
            <th>Thuộc lớp</th>
            <th>Mã người học</th>
            <th>Tên người học</th>
            <th>Chức năng</th>
        </tr>
        <tr>
            <th></th>
            <th><input type="text" class="form-control" data-index="1" placeholder="Lọc theo môn học"></th>
            <th><input type="text" class="form-control" data-index="1" placeholder="Lọc theo giảng viên giảng dạy"></th>
            <th><input type="text" class="form-control" data-index="2" placeholder="Lọc theo lớp"></th>
            <th><input type="text" class="form-control" data-index="3" placeholder="Lọc theo mã người học"></th>
            <th><input type="text" class="form-control" data-index="4" placeholder="Lọc theo tên người học"></th>
            <th></th>
        </tr>
    `;
    thead.html(htmlThead);

    let htmlTbody = "";
    items.forEach((item, index) => {
        htmlTbody += `
            <tr>
                <td class="formatSo">${index + 1}</td>
                <td>${item.mon_hoc}</td>
                <td>${item.giang_vien_giang_day}</td>
                <td class="formatSo">${item.ma_lop}</td>
                <td class="formatSo">${item.ma_nguoi_hoc}</td>
                <td>${item.ten_nguoi_hoc}</td>
                <td>
                    <button class="btn btn-icon btn-hover btn-sm btn-rounded pull-right" id="btnDelete" data-id="${item.value}">
                        <i class="anticon anticon-delete"></i>
                    </button>
                </td>
            </tr>
        `;
    });

    tbody.html(htmlTbody);
}
function form_cbvc_khao_sat_info(thead, tbody, items) {
    let htmlThead = `
        
        <tr>    
            <th>STT</th>
            <th>Tên CBVC/GV khảo sát</th>
            <th>Chương trình đào tạo</th>
            <th>Chức năng</th>
        </tr>
        <tr>
            <th></th>
            <th><input type="text" class="form-control" data-index="1" placeholder="Lọc tên CBVC/GV khảo sát"></th>
            <th><input type="text" class="form-control" data-index="2" placeholder="Chương trình đào tạo"></th>
            <th></th>
        </tr>
    `;
    thead.html(htmlThead);

    let htmlTbody = "";
    items.forEach((item, index) => {
        htmlTbody += `
            <tr>
                <td class="formatSo">${index + 1}</td>
                <td>${item.TenCBVC}</td>
                <td>${item.ctdt}</td>
                <td>
                    <button class="btn btn-icon btn-hover btn-sm btn-rounded pull-right" id="btnDelete" data-id="${item.value}">
                        <i class="anticon anticon-delete"></i>
                    </button>
                </td>
            </tr>
        `;
    });

    tbody.html(htmlTbody);
}
function form_doanh_nghiep_info(thead, tbody, items) {
    let htmlThead = `
        
        <tr>    
            <th>STT</th>
            <th>Email</th>
            <th>Tên người khảo sát</th>
            <th>Chương trình đào tạo</th>
            <th>Chức năng</th>
        </tr>
        <tr>
            <th></th>
            <th><input type="text" class="form-control" data-index="1" placeholder="Lọc Email"></th>
            <th><input type="text" class="form-control" data-index="2" placeholder="Lọc tên người khảo sát"></th>
            <th><input type="text" class="form-control" data-index="2" placeholder="Lọc chương trình đào tạo"></th>
            <th></th>
        </tr>
    `;
    thead.html(htmlThead);

    let htmlTbody = "";
    items.forEach((item, index) => {
        htmlTbody += `
            <tr>
                <td class="formatSo">${index + 1}</td>
                <td>${item.email}</td>
                <td>${item.ten_user}</td>
                <td>${item.ctdt}</td>
                <td>
                    <button class="btn btn-icon btn-hover btn-sm btn-rounded pull-right" id="btnDelete" data-id="${item.value}">
                        <i class="anticon anticon-delete"></i>
                    </button>
                </td>
            </tr>
        `;
    });

    tbody.html(htmlTbody);
}
function form_nguoi_hoc_khao_sat_info(thead, tbody, items) {
    let htmlThead = `
        
        <tr>    
            <th>STT</th>
            <th>Mã người học</th>
            <th>Tên người học</th>
            <th>Thuộc lớp</th>
            <th>Chương trình đào tạo</th>
            <th>Chức năng</th>
        </tr>
        <tr>
            <th></th>
            <th><input type="text" class="form-control" data-index="1" placeholder="Lọc mã người học"></th>
            <th><input type="text" class="form-control" data-index="2" placeholder="Lọc tên người học"></th>
            <th><input type="text" class="form-control" data-index="2" placeholder="Lọc lớp"></th>
            <th><input type="text" class="form-control" data-index="2" placeholder="Lọc chương trình đào tạo"></th>
            <th></th>
        </tr>
    `;
    thead.html(htmlThead);
    let htmlTbody = "";
    items.forEach((item, index) => {
        htmlTbody += `
            <tr>
                <td class="formatSo">${index + 1}</td>
                <td>${item.ma_nguoi_hoc}</td>
                <td>${item.ten_nguoi_hoc}</td>
                <td>${item.ma_lop}</td>
                <td>${item.ten_ctdt}</td>
                <td>
                    <button class="btn btn-icon btn-hover btn-sm btn-rounded pull-right" id="btnDelete" data-id="${item.value}">
                        <i class="anticon anticon-delete"></i>
                    </button>
                </td>
            </tr>
        `;
    });

    tbody.html(htmlTbody);
}
async function delete_dap_vien(id) {
    const res = await $.ajax({
        url: '/api/ctdt/delete-dap-vien-theo-yeu-cau',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            id_thong_ke_theo_yeu_cau: id
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
        load_dap_vien_thong_ke_da_chon();
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