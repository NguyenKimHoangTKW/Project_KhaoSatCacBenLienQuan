$(document).ready(function () {
    load_he_dao_tao();
});

async function load_he_dao_tao() {
    const res = await $.ajax({
        url: '/api/load_he_dao_tao',
        type: 'POST',
    })
    
    if (res.islogin && res.ctdt) {
        const data = JSON.parse(res.data);
        form_ctdt(data)
    }
    else if (res.islogin && res.client) {
        const data = JSON.parse(res.data);
        form_client(data)
    }
    else if (res.islogin && res.admin) {
        window.location.href = "/admin/danh-sach-phieu-khao-sat";
    }
    else if (res.islogin && res.khoa) {
        const data = JSON.parse(res.data);
        form_khoa(data)
    }
    else {
        const data = JSON.parse(res.data);
        form_no_login(data)
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
                                <a href="javascript:void(0)" class="img" style="background-image: url(/Style/assets/${chil.img})"></a>
                                <div class="text p-4">
                                    <p class="category"><span>KHẢO SÁT CÁC BÊN LIÊN QUAN</span></p>
                                    <h3 class="mb-3"><a href="javascript:void(0)">DÀNH CHO ${chil.TenHDT}</a></h3>
                                    <p>${chil.mo_ta}</p>
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
                                <a href="javascript:void(0)" onclick='window.location.href="bo-phieu-khao-sat/${chil.TenHDT}"' class="img" style="background-image: url(/Style/assets/${chil.img})"></a>
                                <div class="text p-4">
                                    <p class="category"><span>KHẢO SÁT CÁC BÊN LIÊN QUAN</span></p>
                                    <h3 class="mb-3"><a href="javascript:void(0)" onclick='window.location.href="/bo-phieu-khao-sat/${chil.TenHDT}"'>DÀNH CHO ${chil.TenHDT}</a></h3>
                                    <p>${chil.mo_ta}</p>
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
                                <a href="javascript:void(0)" onclick='window.location.href="bo-phieu-khao-sat/${chil.TenHDT}"' class="img" style="background-image: url(/Style/assets/${chil.img})"></a>
                                <div class="text p-4">
                                    <p class="category"><span>KHẢO SÁT CÁC BÊN LIÊN QUAN</span></p>
                                    <h3 class="mb-3"><a href="javascript:void(0)" onclick='window.location.href="/bo-phieu-khao-sat/${chil.TenHDT}"'>DÀNH CHO ${chil.TenHDT}</a></h3>
                                    <p>${chil.mo_ta}</p>
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
                                <a href="javascript:void(0)" onclick="window.location.href='/ctdt/giam-sat-ket-qua-khao-sat'" class="img" style="background-image: url(/Style/assets/ctdt.png)"></a>
                                <div class="text p-4">
                                    <p class="category"><span>KHẢO SÁT CÁC BÊN LIÊN QUAN</span></p>
                                    <h3 class="mb-3"><a href="javascript:void(0)" onclick="window.location.href='/ctdt/giam-sat-ket-qua-khao-sat'">DÀNH CHO HỆ CHƯƠNG TRÌNH ĐÀO TẠO</a></h3>
                                    <p>Thống kê kết quả khảo sát ý kiến, đánh giá, góp ý của Cán Bộ Viên Chức Giảng viên, Sinh viên, Cựu Sinh Viên, Doanh Nghiệp theo Chương trình đào tạo ...</p>
                                    <p class="d-flex justify-content-end"><a href="javascript:void(0)" onclick="window.location.href='/ctdt/giam-sat-ket-qua-khao-sat'" class="btn btn-primary">Đi đến</a></p>
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
                                <a href="javascript:void(0)" onclick='window.location.href="bo-phieu-khao-sat/${chil.TenHDT}"' class="img" style="background-image: url(/Style/assets/${chil.img})"></a>
                                <div class="text p-4">
                                    <p class="category"><span>KHẢO SÁT CÁC BÊN LIÊN QUAN</span></p>
                                    <h3 class="mb-3"><a href="javascript:void(0)" onclick='window.location.href="/bo-phieu-khao-sat/${chil.TenHDT}"'>DÀNH CHO ${chil.TenHDT}</a></h3>
                                    <p>${chil.mo_ta}</p>
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

function Alert() {
    Swal.fire({
        title: "Hiện tại xem kết quả đang được bảo trì và thống kê",
        text: "Vui lòng quay trở lại khi có thông báo",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
    })
}
