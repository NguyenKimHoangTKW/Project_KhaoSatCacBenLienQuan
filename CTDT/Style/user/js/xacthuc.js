$(document).ready(function () {

    const btnCheckMSSV = document.getElementById("btnCheckMSSV");
    const btnForgetMSSV = document.getElementById("forget_mssv_button");
    const btnXacThucCTDT = document.getElementById("btnXacThucCTDT");
    const base_url = $('#base_url').val();
    initSelect2(base_url);

    activaTab('home');

    $('#form_checkmssv').on('keypress', function (e) {
        return e.which !== 13;
    });

    if (btnCheckMSSV){
        btnCheckMSSV.addEventListener('click', function () {
            var MSSV = $("#mssv").val();
            if (MSSV === "" || MSSV === undefined) {
                document.querySelector("#show_error").style.visibility = "visible";
                document.querySelector("#show_error").className = "alert alert-danger d-flex align-items-center"
                document.querySelector("#error_mssv").innerHTML = "Vui lòng nhập MSSV.";
                return;
            }
            btnCheckMSSV.disabled = true;
            btnCheckMSSV.innerHTML = btnCheckMSSV.getAttribute("data-loading-text");
            $.ajax({
                url: base_url + "survey/checkDataMSSV/" + MSSV,
                type: "GET",
                async: true,
                cache: false,
                success: function (dataResult) {
                    var data = JSON.parse(dataResult);
                    if (data['code'] === 1) {
                        document.querySelector("#show_error").style.visibility = "visible";
                        document.querySelector("#show_error").className = "alert alert-success d-flex align-items-center"
                        document.querySelector("#error_mssv").innerHTML = "MSSV chính xác, vui lòng đợi giây lát....";
                        setTimeout(function () {
                            window.location.href = data['url'];
                        }, 1000);
                    } else {
                        document.querySelector("#show_error").style.visibility = "visible";
                        document.querySelector("#show_error").className = "alert alert-danger d-flex align-items-center"
                        document.querySelector("#error_mssv").innerHTML = data['message'];
                    }
                },
                complete: function () {
                    btnCheckMSSV.disabled = false;
                    btnCheckMSSV.innerHTML = "Xác thực";
                },
                error: function (xhr) {
                    alert("Error_Load_Data");
                }
            });
        });
    }

    if (btnForgetMSSV){
        btnForgetMSSV.addEventListener('click', function () {
            var fMssv = $('#select2-sv').find(':selected').val();
            if (fMssv === "Chọn tên của bạn" || fMssv === "" || fMssv === undefined) {
                document.querySelector("#show_error_forget").style.visibility = "visible";
                document.querySelector("#show_error_forget").className = "alert alert-danger d-flex align-items-center"
                document.querySelector("#error_forger_mssv").innerHTML = "Vui lòng chọn đầy đủ các thông tin để xác thực.";
                return;
            }
            btnForgetMSSV.disabled = true;
            btnForgetMSSV.innerHTML = btnForgetMSSV.getAttribute("data-loading-text");
            $.ajax({
                url: base_url + "survey/checkDataMSSV/" + fMssv,
                type: "GET",
                async: true,
                cache: false,
                success: function (dataResult) {
                    var data = JSON.parse(dataResult);
                    if (data['code'] == 1) {
                        document.querySelector("#show_error_forget").style.visibility = "visible";
                        document.querySelector("#show_error_forget").className = "alert alert-success d-flex align-items-center"
                        document.querySelector("#error_forger_mssv").innerHTML = "MSSV chính xác, vui lòng đợi giây lát....";
                        setTimeout(function () {
                            window.location.href = data['url'];
                        }, 1000);
                    } else {
                        document.querySelector("#show_error_forget").style.visibility = "visible";
                        document.querySelector("#show_error_forget").className = "alert alert-danger d-flex align-items-center"
                        document.querySelector("#error_forger_mssv").innerHTML = data['message'];
                    }
                },
                complete: function () {
                    btnForgetMSSV.disabled = false;
                    btnForgetMSSV.innerHTML = "Xác thực";
                },
                error: function (xhr) {
                    alert("Error_Load_Data");
                }
            });
        });
    }


    if (btnXacThucCTDT){
        btnXacThucCTDT.addEventListener('click', function () {
            var idCTDT = $('#select2-xacthucctdt-ctdt').find(':selected').val();
            if (idCTDT === "" || idCTDT === undefined) {
                document.querySelector("#show_error_xacthuc_ctdt").style.visibility = "visible";
                document.querySelector("#show_error_xacthuc_ctdt").className = "alert alert-danger d-flex align-items-center"
                document.querySelector("#error_xacthuc_ctdt").innerHTML = "Vui lòng chọn Chương trình đạo tạo.";
                return;
            }
            btnXacThucCTDT.disabled = true;
            btnXacThucCTDT.innerHTML = btnXacThucCTDT.getAttribute("data-loading-text");
            $.ajax({
                url: base_url + "survey/checkCTDT",
                type: "POST",
                data: {
                  "id_ctdt": idCTDT
                },
                async: true,
                cache: false,
                success: function (dataResult) {
                    var data = JSON.parse(dataResult);
                    if (data['code'] === 1) {
                        document.querySelector("#show_error_xacthuc_ctdt").style.visibility = "visible";
                        document.querySelector("#show_error_xacthuc_ctdt").className = "alert alert-success d-flex align-items-center"
                        document.querySelector("#error_xacthuc_ctdt").innerHTML = "Thành công, vui lòng đợi giây lát....";
                        setTimeout(function () {
                            window.location.href = data['url'];
                        }, 1000);
                    } else {
                        document.querySelector("#show_error_xacthuc_ctdt").style.visibility = "visible";
                        document.querySelector("#show_error_xacthuc_ctdt").className = "alert alert-danger d-flex align-items-center"
                        document.querySelector("#error_xacthuc_ctdt").innerHTML = data['message'];
                    }
                },
                complete: function () {
                    btnXacThucCTDT.disabled = false;
                    btnXacThucCTDT.innerHTML = "Xác thực";
                },
                error: function (xhr) {
                    alert("Error_Load_Data");
                }
            });
        });
    }

});

function initSelect2(base_url) {
    const select2 = ["khoa", "ctdt", "lop", "sv"];
    select2.forEach(function (value, index) {
        $('#select2-' + value).select2({
            width: '100%',
            placeholder: $(this).data('placeholder'),
        });

        $('#select2-xacthucctdt-khoa').select2({
            width: '100%',
            placeholder: $(this).data('placeholder'),
        });

        $('#select2-xacthucctdt-ctdt').select2({
            width: '100%',
            placeholder: $(this).data('placeholder'),
        });

        if (value === "khoa") {
            $('#select2-' + value).on('select2:select', function (e) {
                var data = e.params.data;
                getDataCTDT(base_url, data.id);
            });

            $('#select2-xacthucctdt-khoa').on('select2:select', function (e) {
                var data = e.params.data;
                getDataCTDT(base_url, data.id);
            });

        } else if (value === "ctdt") {
            $('#select2-' + value).on('select2:select', function (e) {
                var data = e.params.data;
                getDataLop(base_url, data.id);
            });
        } else if (value === "lop") {
            $('#select2-' + value).on('select2:select', function (e) {
                var data = e.params.data;
                getDataSV(base_url, data.id);
            });
        }


    });
}

function getDataCTDT(base_url, id_khoa) {
    $("#spinner-div").show();
    $.ajax({
        url: base_url + "survey/getDataCTDT/" + id_khoa,
        type: "GET",
        dataType: 'json',
        contentType: 'application/json;charset=utf-8',
        success: function (result) {
            if (result.code === 0) {
                return 0;
            }
            $("#select2-ctdt").empty();
            $("#select2-xacthucctdt-ctdt").empty();
            $("#select2-lop").empty();
            $("#select2-sv").empty();

            $('#select2-ctdt').val(null).trigger('change');
            $('#select2-xacthucctdt-ctdt').val(null).trigger('change');
            $('#select2-lop').append($('<option>', {value: "Chọn lớp của bạn", text: "Chọn lớp của bạn"}));
            $('#select2-sv').append($('<option>', {value: "Chọn tên của bạn", text: "Chọn tên của bạn"}));

            $('#select2-ctdt').append($('<option>', {
                value: "Chọn chương trình đào tạo của bạn",
                text: "Chọn chương trình đào tạo của bạn"
            }));

            $('#select2-xacthucctdt-ctdt').append($('<option>', {
                value: "Chọn chương trình đào tạo",
                text: "Chọn chương trình đào tạo"
            }));

            $.each(result.data, function (index, value) {
                $('#select2-ctdt').append($('<option>', {value: value.id_ctdt, text: value.ten_ctdt}));

                $('#select2-xacthucctdt-ctdt').append($('<option>', {value: value.id_ctdt, text: value.ten_ctdt}));
            });


        },
        complete: function () {
            $("#spinner-div").hide(); //Request is complete so hide spinner
        }
    });
}

function getDataLop(base_url, id_ctdt) {
    $("#spinner-div").show();
    $.ajax({
        url: base_url + "survey/getDataLop/" + id_ctdt,
        type: "GET",
        dataType: 'json',
        contentType: 'application/json;charset=utf-8',
        success: function (result) {
            if (result.code === 0) {
                return 0;
            }
            $("#select2-lop").empty();
            $("#select2-sv").empty();
            $('#select2-lop').val(null).trigger('change');
            $('#select2-sv').append($('<option>', {value: "Chọn tên của bạn", text: "Chọn tên của bạn"}));
            $('#select2-lop').append($('<option>', {value: "Chọn lớp của bạn", text: "Chọn lớp của bạn"}));
            $.each(result.data, function (index, value) {
                $('#select2-lop').append($('<option>', {value: value.id_lop, text: value.ma_lop}));
            });
        },
        complete: function () {
            $("#spinner-div").hide(); //Request is complete so hide spinner
        }
    });
}

function getDataSV(base_url, id_lop) {
    $("#spinner-div").show();
    $.ajax({
        url: base_url + "survey/getDataSV/" + id_lop,
        type: "GET",
        dataType: 'json',
        contentType: 'application/json;charset=utf-8',
        success: function (result) {
            if (result.code === 0) {
                return 0;
            }
            $("#select2-sv").empty();
            $('#select2-sv').val(null).trigger('change');
            $('#select2-sv').append($('<option>', {value: "Chọn tên của bạn", text: "Chọn tên của bạn"}));
            $.each(result.data, function (index, value) {
                const date = new Date(value.ngaysinh);  // 2009-11-10
                const month = date.getMonth() + 1;
                const day = date.getDate();
                const year = date.getFullYear();

                $('#select2-sv').append($('<option>', {
                    value: value.ma_sv,
                    text: value.hovaten + " - " + day + "/" + month + "/" + year,
                }));
            });
        },
        complete: function () {
            $("#spinner-div").hide(); //Request is complete so hide spinner
        }
    });
}

function activaTab(tab){
    $('.tab-content #'+tab).removeClass('fade');
    $('.nav-tabs .nav-item a[href="#' + tab + '"]').tab('show');
};

