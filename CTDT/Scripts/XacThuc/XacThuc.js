$(document).ready(function () {
    load_select_xac_thuc()
});
function load_select_xac_thuc() {
    var id = $('#hiddenId').val();
    if (id) {
        $.ajax({
            url: '/Home/load_select_xac_thuc',
            type: 'POST',
            data: { id: id },
            success: function (res) {
                let body = $('#showdata');
                body.empty();
                // Cưu người học
                if (res.data && res.data.is_nguoi_hoc) {
                    const ctdt = res.data.list_ctdt.find(chil => chil.ma_khoa == res.data.list_khoa.ma_khoa);
                    const lop = res.data.list_lop.find(chil => chil.ma_ctdt == res.data.list_ctdt.ma_ctdt);
                    const nguoi_hoc = res.data.list_nguoi_hoc.find(chil => chil.ma_lop == res.data.list_lop.ma_lop);
                    let html = `
                        <ul class="nav nav-tabs" id="myTab" role="tablist" style="padding-top: 20px;">
                            <li class="nav-item">
                                <a class="nav-link active" id="profile-tab" data-toggle="tab" href="#profile" role="tab" aria-controls="profile" aria-selected="false">Xác thực</a>
                            </li>
                            <li class="nav-item">
                                <a class="nav-link" id="home-tab" data-toggle="tab" href="#home" role="tab" aria-controls="home" aria-selected="true">Quên MSSV/MSHV</a>
                            </li>
                        </ul>
                        <div class="tab-content mt-4" id="myTabContent">
                            <div class="tab-pane fade" id="home" role="tabpanel" aria-labelledby="home-tab">
                                <div class="card">
                                    <div class="card-body">
                                        <div class="form-group">
                                            <label for="selectElement" class="form-label" style="font-weight:bold;">Chọn khoa</label>
                                            <select class="form-control select2" id="select_khoa" name="state">
                    `;

                    res.data.list_khoa.forEach(function (khoa) {
                        html += `<option value="${khoa.ma_khoa}">${khoa.ten_khoa}</option>`;
                    });

                    html += `
                                            </select>
                                        </div>
                                        <div class="form-group">
                                            <label for="selectElement" class="form-label" style="font-weight:bold;">Chọn chương trình đào tạo</label>
                                            <select class="form-control select2" id="select_ctdt" name="state">`;
                    ctdt.forEach(function (ctdt) {
                        html += `<option value = "${ctdt.ma_ctdt}" >${ctdt.ten_ctdt} </option>`
                    });
                    html += ` </select>
                                        </div>
                                        <div class="form-group">
                            <label for="selectElement" class="form-label" style="font-weight:bold;">Chọn lớp</label>
                            <select class="form-control select2" id="select_lop" name="state">`;
                    lop.forEach(function (lop) {
                        html += `<option value="${lop.ma_lop}">${lop.ten_lop}</option>`;
                    });

                    html += `</select>
                        </div>
                        <div class="form-group">
                            <label for="selectElement" class="form-label" style="font-weight:bold;">Chọn người học</label>
                            <select class="form-control select2" id="select_nguoi_hoc" name="state">`;
                    nguoi_hoc.forEach(function (nguoihoc) {
                        html += `<option value="${nguoihoc.ma_nguoi_hoc}">${nguoihoc.ten_nguoi_hoc}</option>`;
                    });
                    html += `</select>
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
                            <div class="tab-pane fade show active" id="profile" role="tabpanel" aria-labelledby="profile-tab">
                                <div class="form-group">
                                    <label for="selectElement" class="form-label" style="font-weight:bold;">Nhập MSSV/MSHV để tiếp tục</label>                                   
                                    <input type="text" class="form-control" id="check_key" placeholder="Nhập MSSV/MSHV tại đây..." />
                                    <p style="font-style:italic">*Nếu không nhớ MSSV/MSHV có thể chọn chức năng - Quên MSSV/MSHV ở bên trên</p>
                                    <hr />
                                    <div class="d-flex justify-content-center mt-4" style="gap: 20px;">
                                            <button class="btn btn-primary" id="btnSave_key">
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
                    `;
                    body.html(html);
                    // Load CTĐT By Khoa
                    $(document).on('change', "#select_khoa", function () {
                        var select_ctdt = $('#select_ctdt');
                        var id = $(this).val();
                        select_ctdt.empty();
                        let option = ``;

                        res.data.list_ctdt.forEach(function (ctdtList) {
                            ctdtList.forEach(function (chil) {
                                if (chil.ma_khoa == id) {
                                    option += `<option value="${chil.ma_ctdt}">${chil.ten_ctdt}</option>`;
                                }
                            });
                        });

                        select_ctdt.html(option);

                        select_ctdt.trigger('change');
                    });

                    // Load Lớp By CTĐT
                    $(document).on('change', "#select_ctdt", function () {
                        var select_lop = $('#select_lop');
                        var id = $(this).val();
                        select_lop.empty();
                        let option = ``;

                        res.data.list_lop.forEach(function (lopList) {
                            lopList.forEach(function (chil) {
                                if (chil.ma_ctdt == id) {
                                    option += `<option value="${chil.ma_lop}">${chil.ten_lop}</option>`;
                                }
                            });
                        });

                        select_lop.html(option);

                        select_lop.trigger('change');
                    });

                    // Load Người học by Lớp
                    $(document).on('change', '#select_lop', function () {
                        var select_nguoi_hoc = $("#select_nguoi_hoc");
                        var id = $(this).val();
                        select_nguoi_hoc.empty();
                        let option = ``;

                        res.data.list_nguoi_hoc.forEach(function (nguoiHocList) {
                            nguoiHocList.forEach(function (chil) {
                                var ngaySinh = new Date(parseInt(chil.ngay_sinh.substr(6)));
                                var formattedNgaySinh = ngaySinh.toLocaleDateString("en-US");
                                if (chil.ma_lop == id) {
                                    option += `<option value="${chil.id_nguoi_hoc}">${chil.ten_nguoi_hoc} - ${chil.ma_nguoi_hoc} - ${formattedNgaySinh}</option>`;
                                }
                            });
                        });

                        select_nguoi_hoc.html(option);
                    });

                    $('#select_khoa').trigger('change');

                }
                // Doanh nghiệp
                else if (res.data && res.data.is_doanh_nghiep) {
                    const ctdt = res.data.list_ctdt.find(chil => chil.ma_khoa == res.data.list_khoa.ma_khoa);
                    let html = `
                        <ul class="nav nav-tabs" id="myTab" role="tablist" style="padding-top: 20px;">
                            <li class="nav-item">
                                <a class="nav-link active" id="home-tab" data-toggle="tab" href="#home" role="tab" aria-controls="home" aria-selected="true">Xác thực CTĐT</a>
                            </li>
                        </ul>
                        <div class="tab-content mt-4" id="myTabContent">
                            <div class="tab-pane fade show active" id="home" role="tabpanel" aria-labelledby="home-tab">
                                <div class="card">
                                    <div class="card-body">
                                        <div class="form-group">
                                            <label for="selectElement" class="form-label" style="font-weight:bold;">Chọn khoa</label>
                                            <select class="form-control select2" id="select_khoa" name="state">
                    `;

                    res.data.list_khoa.forEach(function (khoa) {
                        html += `<option value="${khoa.ma_khoa}">${khoa.ten_khoa}</option>`;
                    });

                    html += `
                                            </select>
                                        </div>
                                        <div class="form-group">
                                            <label for="selectElement" class="form-label" style="font-weight:bold;">Chọn chương trình đào tạo</label>
                                            <select class="form-control select2" id="select_ctdt" name="state">`;
                    ctdt.forEach(function (ctdt) {
                        html += `<option value = "${ctdt.ma_ctdt}" >${ctdt.ten_ctdt} </option>`
                    });
                    html += ` </select>
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
                        </div>
                    `;
                    body.html(html);
                    // Load CTĐT By Khoa
                    $(document).on('change', "#select_khoa", function () {
                        var select_ctdt = $('#select_ctdt');
                        var id = $(this).val();
                        select_ctdt.empty();
                        let option = ``;

                        res.data.list_ctdt.forEach(function (ctdtList) {
                            ctdtList.forEach(function (chil) {
                                if (chil.ma_khoa == id) {
                                    option += `<option value="${chil.ma_ctdt}">${chil.ten_ctdt}</option>`;
                                }
                            });
                        });

                        select_ctdt.html(option);

                        select_ctdt.trigger('change');
                    });


                    $('#select_khoa').trigger('change');
                }
                // Giảng viên
                else if (res.data && res.data.is_giang_vien) {
                    const ctdt = res.data.list_ctdt.find(chil => chil.ma_khoa == res.data.list_khoa.ma_khoa);
                    let html = `
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
                                            <label for="selectElement" class="form-label" style="font-weight:bold;">Chọn khoa</label>
                                            <select class="form-control select2" id="select_khoa" name="state">
                    `;

                    res.data.list_khoa.forEach(function (khoa) {
                        html += `<option value="${khoa.ma_khoa}">${khoa.ten_khoa}</option>`;
                    });

                    html += `
                                            </select>
                                        </div>
                                        <div class="form-group">
                                            <label for="selectElement" class="form-label" style="font-weight:bold;">Chọn chương trình đào tạo</label>
                                            <select class="form-control select2" id="select_ctdt" name="state">`;
                    ctdt.forEach(function (ctdt) {
                        html += `<option value = "${ctdt.ma_ctdt}" >${ctdt.ten_ctdt} </option>`
                    });
                    html += ` </select>
                                        </div>
                                        <div class="form-group">
                                            <label for="selectElement" class="form-label" style="font-weight:bold;">Chọn đơn vị</label>
                                            <select class="form-control select2" id="select_don_vi" name="state">
                                             <option value = "" >Bỏ qua</option>
                                            `;
                    res.data.list_don_vi.forEach(function (donvi) {
                        html += `
                       
                        <option value = "${donvi.ma_don_vi}" >${donvi.ten_don_vi} </option>
                        
                        `
                    });
                    html += `
                                     </select>

                                        </div>
                                        <p style="font-style:italic">* Nếu không có Đơn vị, vui lòng chọn <b>"Bỏ qua"</b></p>
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
                        </div>
                    `;
                    body.html(html);
                    // Load CTĐT By Khoa
                    $(document).on('change', "#select_khoa", function () {
                        var select_ctdt = $('#select_ctdt');
                        var id = $(this).val();
                        select_ctdt.empty();
                        let option = ``;

                        res.data.list_ctdt.forEach(function (ctdtList) {
                            ctdtList.forEach(function (chil) {
                                if (chil.ma_khoa == id) {
                                    option += `<option value="${chil.ma_ctdt}">${chil.ten_ctdt}</option>`;
                                }
                            });
                        });

                        select_ctdt.html(option);

                        select_ctdt.trigger('change');
                    });

                    $('#select_khoa').trigger('change');
                }
            },
            error: function () {
                console.log("The requested resource does not support http method 'GET'.");
            }
        });
    } else {
        console.log("Invalid ID");
    }
}