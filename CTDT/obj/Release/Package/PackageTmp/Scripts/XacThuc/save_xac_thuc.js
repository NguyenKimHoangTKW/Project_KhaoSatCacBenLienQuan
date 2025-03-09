$(document).on("click", "#btnSave", function (event) {
    event.preventDefault();
    save_xac_thuc();
});
async function save_xac_thuc() {
    const ma_vien_chuc = $("#ma-vien-chuc").val();
    const ten_vien_chuc = $("#ten-vien-chuc").val();
    const ctdt = $("#select_ctdt").val();
    const survey = $("#hiddenId").val();
    const res = await $.ajax({
        url: '/api/save_xac_thuc',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            ma_vien_chuc: ma_vien_chuc,
            ten_vien_chuc: ten_vien_chuc,
            id_ctdt: ctdt,
            surveyID: survey
        })
    });
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
            window.location.href = res.url;
        });
    }
    else if (res.is_answer) {
        Swal.fire({
            title: "Bạn đã khảo sát cho chương trình đào tạo này!",
            text: "Bạn có muốn xem lại đáp án?",
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#3085d6",
            cancelButtonColor: "#d33",
            confirmButtonText: "Xem lại"
        }).then((result) => {
            if (result.isConfirmed) {
                window.location.href = res.url;
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
            title: res.message
        });
    }
}