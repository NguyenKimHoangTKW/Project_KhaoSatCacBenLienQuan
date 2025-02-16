$(document).ready(function () {

});
$(document).on("change", "#find-ctdt", function () {
    load_survey();
});

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

$(document).on("click", "#btnFilter", function (event) {
    event.preventDefault()
    load_data()
});

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
        if (isNguoiHocCoHocPhan) {
            await form_nguoi_hoc_khao_sat(thead, tbody, res.data);
        }
        else if (isCBVC) {
            await form_cbvc_khao_sat(thead, tbody, res.data);
        }
        else if (isDoanhNghiep) {
            await form_doanh_nghiep(thead, tbody, res.data);
        }
        else if (isNguoiHocKhaoSat) {
            await form_nguoi_hoc_khao_sat(thead, tbody, res.data);
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

function form_nguoi_hoc_khao_sat(thead, tbody, items) {
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
                <td><input type="checkbox" class="select-row" /></td>
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
                <td><input type="checkbox" class="select-row" /></td>
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
                <td><input type="checkbox" class="select-row" /></td>
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
                <td><input type="checkbox" class="select-row" /></td>
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
