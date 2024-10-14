$(document).ready(function () {
    load_he_dao_tao();
});

function load_he_dao_tao() {
    $.ajax({
        url: '/Home/load_he_dao_tao',
        type: 'GET',
        success: function (res) {
            let body = $('#showdata');
            let html = '';
            if (res.islogin && res.ctdt) {
                html =
                    `
                    <div class="row justify-content-center mb-5 pb-3">
                        <div class="col-md-7 heading-section text-center">
                            <h2 class="mb-4" style="font-weight:bold;">DANH SÁCH CHỨC NĂNG</h2>
                        </div>
                    </div>
                    <div class="row">
                    `;

                res.data.forEach(function (chil) {
                    html +=
                        `
                        <div class="col-md-6 d-flex">
                            <div class="course align-self-stretch">
                                <a href="javascript:void(0)" onclick='window.location.href="bo-phieu-khao-sat/${chil.MaHDT}"' class="img" style="background-image: url(/Style/assets/survey.png)"></a>
                                <div class="text p-4">
                                    <p class="category"><span>KHẢO SÁT CÁC BÊN LIÊN QUAN</span></p>
                                    <h3 class="mb-3"><a href="javascript:void(0)" onclick='window.location.href="bo-phieu-khao-sat/${chil.MaHDT}"'>DÀNH CHO HỆ ${chil.TenHDT}</a></h3>
                                    <p>Khảo sát ý kiến, đánh giá, góp ý của Cán Bộ Viên Chức Giảng viên, Sinh viên, Cựu Sinh Viên, Doanh Nghiệp,...</p>
                                    <p class="d-flex justify-content-end"><a href="javascript:void(0)" onclick='window.location.href="bo-phieu-khao-sat/${chil.MaHDT}"' class="btn btn-primary">Đi đến</a></p>
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
            else if (res.islogin && res.client)
            {
                html =
                    `
                    <div class="row justify-content-center mb-5 pb-3">
                        <div class="col-md-7 heading-section text-center">
                            <h2 class="mb-4" style="font-weight:bold;">DANH SÁCH CHỨC NĂNG</h2>
                        </div>
                    </div>
                    <div class="row">
                    `;

                res.data.forEach(function (chil) {
                    html +=
                        `
                        <div class="col-md-6 d-flex">
                            <div class="course align-self-stretch">
                                <a href="javascript:void(0)" onclick='window.location.href="bo-phieu-khao-sat/${chil.MaHDT}"' class="img" style="background-image: url(/Style/assets/survey.png)"></a>
                                <div class="text p-4">
                                    <p class="category"><span>KHẢO SÁT CÁC BÊN LIÊN QUAN</span></p>
                                    <h3 class="mb-3"><a href="javascript:void(0)" onclick='window.location.href="bo-phieu-khao-sat/${chil.MaHDT}"'>DÀNH CHO HỆ ${chil.TenHDT}</a></h3>
                                    <p>Khảo sát ý kiến, đánh giá, góp ý của Cán Bộ Viên Chức Giảng viên, Sinh viên, Cựu Sinh Viên, Doanh Nghiệp,...</p>
                                    <p class="d-flex justify-content-end"><a href="javascript:void(0)" onclick='window.location.href="bo-phieu-khao-sat/${chil.MaHDT}"' class="btn btn-primary">Đi đến</a></p>
                                </div>
                            </div>
                        </div>
                        `;
                });
                html += '</div>';
                body.html(html);
            }
            else if (res.islogin && res.admin) {
                window.location.href = "/Admin/PhieuKhaoSat/Index";
            }
            else {
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

                res.data.forEach(function (chil) {
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
        },
        error: function (err) {
            console.error('Error loading data:', err);
        }
    });
}