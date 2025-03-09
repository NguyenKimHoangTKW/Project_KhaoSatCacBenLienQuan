$(document).ready(function () {
    load_select_xac_thuc()
});
let check_mail = false;

async function load_select_xac_thuc() {
    const value = $('#hiddenId').val();
    const res = await $.ajax({
        url: '/api/xac_thuc',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            surveyID: value
        })
    });

    let body = $('#showdata');
    body.empty();

    if (res.success) {
        const data = res.data[0];
        if (data.is_giang_vien) {
            let html = `
                <div class="d-flex justify-content-center mt-4" style="gap: 20px;">
                    <button class="btn btn-info" id="btnYesEmail">
                        Bấm vào đây nếu đang đăng nhập bằng Mail trường
                    </button>
                    <button class="btn btn-info" id="btnNoEmail">
                        Bấm vào đây nếu đang đăng nhập bằng Mail cá nhân (trường hợp không có mail trường)
                    </button>
                </div>
                <div id="emailResult"></div> <!-- Thêm div chứa kết quả -->
            `;
            body.html(html);

            $(document).off("click", "#btnYesEmail").on("click", "#btnYesEmail", function (event) {
                event.preventDefault();
                check_mail = true;
                updateUI(data);
            });

            $(document).off("click", "#btnNoEmail").on("click", "#btnNoEmail", function (event) {
                event.preventDefault();
                check_mail = false;
                updateUI(data);
            });
        }
    }
}
function updateUI(data) {
    let resultContainer = $("#emailResult");
    resultContainer.html(check_mail ? gv_success_email(data) : gv_failed_email(data));
    $(".select2").select2();
}

function gv_success_email(data) {
    let html = ``;
    html = `
                <ul class="nav nav-tabs" id="myTab" role="tablist" style="padding-top: 20px;">
                    <li class="nav-item">
                        <a class="nav-link active" id="home-tab" data-toggle="tab" href="#home" role="tab" aria-controls="home" aria-selected="true">Xác thực Giảng viên</a>
                    </li>
                </ul>
                <div class="tab-content mt-4" id="myTabContent">
                    <div class="tab-pane fade show active" id="home" role="tabpanel" aria-labelledby="home-tab">
                        <div class="card">
                            <div class="card-body">                                       
                                <div class="form-group">
                                    <label for="selectElement" class="form-label" style="font-weight:bold;">Chọn chương trình đào tạo muốn khảo sát</label>
                                    <select class="form-control select2" id="select_ctdt" name="state">
            `;
    data.ctdt.forEach(function (ctdt) {
        html += `<option value="${ctdt.value}">${ctdt.name}</option>`;
    });

    html += `
                                    </select>
                                </div>
                                <hr />
                                <div class="d-flex justify-content-center mt-4" style="gap: 20px;">
                                    <button class="btn btn-primary" id="btnSave">
                                        <i class="bi bi-check-lg"></i>
                                        Xác thực
                                    </button>
                                    <button class="btn btn-outline-danger" onclick="goBack()">
                                        <i class="bi bi-arrow-left-circle"></i>
                                        Quay trở lại
                                    </button>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            `;
    return html;
}

function gv_failed_email(data) {
    let html = ``;
    html = `
                <ul class="nav nav-tabs" id="myTab" role="tablist" style="padding-top: 20px;">
                    <li class="nav-item">
                        <a class="nav-link active" id="home-tab" data-toggle="tab" href="#home" role="tab" aria-controls="home" aria-selected="true">Xác thực Giảng viên</a>
                    </li>
                </ul>
                <div class="tab-content mt-4" id="myTabContent">
                    <div class="tab-pane fade show active" id="home" role="tabpanel" aria-labelledby="home-tab">
                        <div class="card">
                            <div class="card-body">
                            <div class="form-group">
                                    <label for="selectElement" class="form-label" style="font-weight:bold;">Nhập mã viên chức</label>
                                    <input type="text" class="form-control" id="ma-vien-chuc" autocomplete="off" placeholder="Nhập mã viên chức tại đây" />                                  
                            </div>
                            <div class="form-group">
                                    <label for="selectElement" class="form-label" style="font-weight:bold;">Nhập tên viên chức</label>
                                    <input type="text" class="form-control" id="ten-vien-chuc" autocomplete="off" placeholder="Nhập tên viên chức tại đây" />
                            </div>
                                <div class="form-group">
                                    <label for="selectElement" class="form-label" style="font-weight:bold;">Chọn chương trình đào tạo muốn khảo sát</label>
                                    <select class="form-control select2" id="select_ctdt" name="state">
            `;
                data.ctdt.forEach(function (ctdt) {
                    html += `<option value="${ctdt.value}">${ctdt.name}</option>`;
                });

    html += `
                                    </select>
                                </div>
                                <hr />
                                <div class="d-flex justify-content-center mt-4" style="gap: 20px;">
                                    <button class="btn btn-primary" id="btnSave">
                                        <i class="bi bi-check-lg"></i>
                                        Xác thực
                                    </button>
                                    <button class="btn btn-outline-danger" onclick="goBack()">
                                        <i class="bi bi-arrow-left-circle"></i>
                                        Quay trở lại
                                    </button>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            `;
    return html;
}