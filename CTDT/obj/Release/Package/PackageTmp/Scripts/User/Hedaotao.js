$(document).ready(function () {
    load_he_dao_tao();
});

async function load_he_dao_tao() {
    const res = await $.ajax({
        url: '/api/load_he_dao_tao',
        type: 'POST',
    })
    if (res.islogin && res.ctdt) {
        form_ctdt(res.data)
    }
    else if (res.islogin && res.client) {
        form_client(res.data)
    }
    else if (res.islogin && res.admin) {
        window.location.href = "/Admin/PhieuKhaoSat/Index";
    }
    else if (res.islogin && res.khoa) {
        form_khoa(res.data)
    }
    else {
        form_no_login(res.data)
    }
}

function form_no_login(check) {
    let body = $('#showdata');
    let html = '';
    html =
        `
                    <div class="row justify-content-center mb-5 pb-3">
                        <div class="col-md-7 heading-section text-center">
                            <h2 class="mb-4" style="font-weight:bold;">DANH SÁCH CHỨC NĂNG</h2>
                        </div>
                    </div>
                    <div class="alert alert-info" style="text-align: center;">
                        <span style="color:red">Lưu ý :</span> Vui lòng đăng nhập để thực hiện các chức năng
                    </div>
                    <div class="row">
                    `;

    check.forEach(function (chil) {
        html +=
            `
                        <div class="col-md-6 d-flex">
                            <div class="course align-self-stretch">
                                <a href="javascript:void(0)" class="img" style="background-image: url(/Style/assets/survey.png)"></a>
                                <div class="text p-4">
                                    <p class="category"><span>KHẢO SÁT CÁC BÊN LIÊN QUAN</span></p>
                                    <h3 class="mb-3"><a href="javascript:void(0)">DÀNH CHO HỆ ${chil.TenHDT}</a></h3>
                                    <p>Khảo sát ý kiến, đánh giá, góp ý của Cán Bộ Viên Chức Giảng viên, Sinh viên, Cựu Sinh Viên, Doanh Nghiệp,...</p>
                                </div>
                            </div>
                        </div>
                        `;
    });

    html += '</div>';

    body.html(html);
}

function form_client(check) {
    let body = $('#showdata');
    let html = '';
    html =
        `
                    <div class="row justify-content-center mb-5 pb-3">
                        <div class="col-md-7 heading-section text-center">
                            <h2 class="mb-4" style="font-weight:bold;">DANH SÁCH CHỨC NĂNG</h2>
                        </div>
                    </div>
                    <div class="row">
                    `;

    check.forEach(function (chil) {
        html +=
            `
                        <div class="col-md-6 d-flex">
                            <div class="course align-self-stretch">
                                <a href="javascript:void(0)" onclick='window.location.href="bo-phieu-khao-sat/${chil.TenHDT}"' class="img" style="background-image: url(/Style/assets/survey.png)"></a>
                                <div class="text p-4">
                                    <p class="category"><span>KHẢO SÁT CÁC BÊN LIÊN QUAN</span></p>
                                    <h3 class="mb-3"><a href="javascript:void(0)" onclick='window.location.href="/bo-phieu-khao-sat/${chil.TenHDT}"'>DÀNH CHO HỆ ${chil.TenHDT}</a></h3>
                                    <p>Khảo sát ý kiến, đánh giá, góp ý của Cán Bộ Viên Chức Giảng viên, Sinh viên, Cựu Sinh Viên, Doanh Nghiệp,...</p>
                                    <p class="d-flex justify-content-end"><a href="javascript:void(0)" onclick='window.location.href="/bo-phieu-khao-sat/${chil.TenHDT}"' class="btn btn-primary">Đi đến</a></p>
                                </div>
                            </div>
                        </div>
                        `;
    });
    html += '</div>';
    body.html(html);
}


function form_ctdt(check) {
    let body = $('#showdata');
    let html = '';
    html =
        `
                    <div class="row justify-content-center mb-5 pb-3">
                        <div class="col-md-7 heading-section text-center">
                            <h2 class="mb-4" style="font-weight:bold;">DANH SÁCH CHỨC NĂNG</h2>
                        </div>
                    </div>
                    <div class="row">
                    `;

    check.forEach(function (chil) {
        html +=
            `
                        <div class="col-md-6 d-flex">
                            <div class="course align-self-stretch">
                                <a href="javascript:void(0)" onclick='window.location.href="/bo-phieu-khao-sat/${chil.TenHDT}"' class="img" style="background-image: url(/Style/assets/survey.png)"></a>
                                <div class="text p-4">
                                    <p class="category"><span>KHẢO SÁT CÁC BÊN LIÊN QUAN</span></p>
                                    <h3 class="mb-3"><a href="javascript:void(0)" onclick='window.location.href="/bo-phieu-khao-sat/${chil.TenHDT}"'>DÀNH CHO HỆ ${chil.TenHDT}</a></h3>
                                    <p>Khảo sát ý kiến, đánh giá, góp ý của Cán Bộ Viên Chức Giảng viên, Sinh viên, Cựu Sinh Viên, Doanh Nghiệp,...</p>
                                    <p class="d-flex justify-content-end"><a href="javascript:void(0)" onclick='window.location.href="/bo-phieu-khao-sat/${chil.TenHDT}"' class="btn btn-primary">Đi đến</a></p>
                                </div>
                            </div>
                        </div>
                        `;
    });

    html += '</div>';
    html +=
        `
                    <div class="row">
                    <div class="col-md-12">
                            <div class="course align-self-stretch">
                                <a href="javascript:void(0)" onclick='window.location.href="/ctdt/giam-sat-ket-qua-khao-sat"' class="img" style="background-image: url(/Style/assets/ctdt.png)"></a>
                                <div class="text p-4">
                                    <p class="category"><span>KHẢO SÁT CÁC BÊN LIÊN QUAN</span></p>
                                    <h3 class="mb-3"><a href="javascript:void(0)" onclick='window.location.href="/ctdt/giam-sat-ket-qua-khao-sat"'>DÀNH CHO HỆ CHƯƠNG TRÌNH ĐÀO TẠO</a></h3>
                                    <p>Thống kê kết quả khảo sát ý kiến, đánh giá, góp ý của Cán Bộ Viên Chức Giảng viên, Sinh viên, Cựu Sinh Viên, Doanh Nghiệp theo Chương trình đào tạo ...</p>
                                    <p class="d-flex justify-content-end"><a href="javascript:void(0)" onclick='window.location.href="/ctdt/giam-sat-ket-qua-khao-sat"' class="btn btn-primary">Đi đến</a></p>
                                </div>
                            </div>
                        </div>
                    </div>
                    `;
    body.html(html);
}

function form_khoa(check) {
    let body = $('#showdata');
    let html = '';
    html =
        `
                    <div class="row justify-content-center mb-5 pb-3">
                        <div class="col-md-7 heading-section text-center">
                            <h2 class="mb-4" style="font-weight:bold;">DANH SÁCH CHỨC NĂNG</h2>
                        </div>
                    </div>
                    <div class="row">
                    `;

    check.forEach(function (chil) {
        html +=
            `
                        <div class="col-md-6 d-flex">
                            <div class="course align-self-stretch">
                                <a href="javascript:void(0)" onclick='window.location.href="bo-phieu-khao-sat/${chil.TenHDT}"' class="img" style="background-image: url(/Style/assets/survey.png)"></a>
                                <div class="text p-4">
                                    <p class="category"><span>KHẢO SÁT CÁC BÊN LIÊN QUAN</span></p>
                                    <h3 class="mb-3"><a href="javascript:void(0)" onclick='window.location.href="bo-phieu-khao-sat/${chil.TenHDT}"'>DÀNH CHO HỆ ${chil.TenHDT}</a></h3>
                                    <p>Khảo sát ý kiến, đánh giá, góp ý của Cán Bộ Viên Chức Giảng viên, Sinh viên, Cựu Sinh Viên, Doanh Nghiệp,...</p>
                                    <p class="d-flex justify-content-end"><a href="javascript:void(0)" onclick='window.location.href="bo-phieu-khao-sat/${chil.TenHDT}"' class="btn btn-primary">Đi đến</a></p>
                                </div>
                            </div>
                        </div>
                        `;
    });

    html += '</div>';
    html +=
        `
                    <div class="row">
                    <div class="col-md-12">
                            <div class="course align-self-stretch">
                                <a href="javascript:void(0)" onclick='window.location.href="/khoa/giam-sat-ket-qua-khao-sat"' class="img" style="background-image: url(/Style/assets/ctdt.png)"></a>
                                <div class="text p-4">
                                    <p class="category"><span>KHẢO SÁT CÁC BÊN LIÊN QUAN</span></p>
                                    <h3 class="mb-3"><a href="javascript:void(0)" onclick='window.location.href="/khoa/giam-sat-ket-qua-khao-sat"'>DÀNH CHO KHOA</a></h3>
                                    <p>Thống kê kết quả khảo sát ý kiến, đánh giá, góp ý của Cán Bộ Viên Chức Giảng viên, Sinh viên, Cựu Sinh Viên, Doanh Nghiệp theo Chương trình đào tạo ...</p>
                                    <p class="d-flex justify-content-end"><a href="javascript:void(0)" onclick='window.location.href="/khoa/giam-sat-ket-qua-khao-sat"' class="btn btn-primary">Đi đến</a></p>
                                </div>
                            </div>
                        </div>
                    </div>
                    `;
    body.html(html);
}
