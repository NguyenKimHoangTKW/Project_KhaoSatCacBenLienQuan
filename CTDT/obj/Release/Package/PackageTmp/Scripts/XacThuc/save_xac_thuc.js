$(document).on('click', '#btnSave', function () {
    save_xac_thuc();
});
$(document).on('click', '#btnSave_key', function () {
    get_nguoi_hoc_by_key();
});
$(document).on("input", "#check_key",function () {
    this.value = this.value.replace(/[^0-9]/g, '');
    if (this.value.length > 13) {
        this.value = this.value.slice(0, 13);
    }
});

$(document).on("keydown", "#check_key", function (event) {
    if (event.key === "Enter") {
        event.preventDefault();
        get_nguoi_hoc_by_key();
    }
});
function get_nguoi_hoc_by_key() {
    var ma_nguoi_hoc = $("#check_key").val();
    var id_survey = $("#hiddenId").val();
    if (ma_nguoi_hoc === "") {
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
            title: "Vui lòng điền MSSV/MSHV"
        });
        return;
    }
    $.ajax({
        url: '/api/get_nguoi_hoc_by_key',
        type: 'POST',
        data: {
            Id: id_survey,
            ma_nguoi_hoc: ma_nguoi_hoc
        },
        success: function (res) {
            if (res.data != null) {
                var items = res.data;
                var get_don_vi = $("#select_don_vi").val();
                var id_survey = $("#hiddenId").val();
                var get_data = {
                    Id: id_survey,
                    nguoi_hoc: items.ma_nguoi_hoc,
                    ctdt: items.chuong_trinh_dao_tao,
                    donvi: get_don_vi
                };
                $.ajax({
                    url: '/api/save_xac_thuc',
                    type: 'POST',
                    data: JSON.stringify(get_data),
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: function (response) {
                        if (response.success) {
                            Swal.fire({
                                title: "Xác thực thành công",
                                text: "Đang tải dữ liệu phiếu khảo sát, vui lòng chờ!",
                                icon: "success",
                                allowOutsideClick: false,
                                showConfirmButton: false,
                                willOpen: () => {
                                    Swal.showLoading();
                                },
                                timer: 3000,
                            }).then(() => {
                                window.location.href = '/phieu-khao-sat/' + id_survey;
                            });
                        }
                        else if (response.is_answer) {
                            Swal.fire({
                                title: res.message,
                                text: "Bạn có muốn xem lại đáp án?",
                                icon: "warning",
                                showCancelButton: true,
                                confirmButtonColor: "#3085d6",
                                cancelButtonColor: "#d33",
                                confirmButtonText: "Xem lại"
                            }).then((result) => {
                                if (result.isConfirmed) {
                                    window.location.href = response.info;
                                }
                            });
                        }
                    }
                });
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
                    title: "Không tìm thấy SV/HV"
                });
            }
        }
    });
}

function save_xac_thuc() {
    var get_nguoi_hoc = $("#select_nguoi_hoc").val();
    var get_ctdt = $("#select_ctdt").val();
    var get_don_vi = $("#select_don_vi").val();
    var id_survey = $("#hiddenId").val();
    var get_data = {
        Id: id_survey,
        nguoi_hoc: get_nguoi_hoc,
        ctdt: get_ctdt,
        donvi: get_don_vi
    };
    localStorage.setItem("xacthucstorage", JSON.stringify(get_data));

    $.ajax({
        url: '/api/save_xac_thuc',
        type: 'POST',
        data: JSON.stringify(get_data),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (res) {
            if (res.success) {                
                Swal.fire({
                    title: "Xác thực thành công",
                    text: "Đang tải dữ liệu phiếu khảo sát, vui lòng chờ!",
                    icon: "success",
                    allowOutsideClick: false,
                    showConfirmButton: false,
                    willOpen: () => {
                        Swal.showLoading();
                    },
                    timer: 3000,
                }).then(() => {
                    window.location.href = '/phieu-khao-sat/' + id_survey;
                });
            }
            else if (res.is_answer) {
                Swal.fire({
                    title: res.message,
                    text: "Bạn có muốn xem lại đáp án?",
                    icon: "warning",
                    showCancelButton: true,
                    confirmButtonColor: "#3085d6",
                    cancelButtonColor: "#d33",
                    confirmButtonText: "Xem lại"
                }).then((result) => {
                    if (result.isConfirmed) {
                        window.location.href = res.info;
                    }
                });
            }
        }
    });
}
