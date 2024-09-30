$(document).ready(function () {
    setInterval(checkSession, 3600000);

    function checkSession() {
        $.ajax({
            url: '/Login/CheckSession',
            type: 'GET',
            success: function (res) {
                if (!res.isAuthenticated) {
                    Swal.fire({
                        title: "Phiên đăng nhập hết hạn",
                        text: "Bạn vui lòng đăng nhập lại để tiếp tục!",
                        icon: "warning",
                        confirmButtonColor: "#3085d6",
                        cancelButtonColor: "#d33",
                        confirmButtonText: "Đồng ý"
                    }).then((result) => {
                        if (result.isConfirmed) {
                            window.location.href = '/Home/Index';
                        }
                    });
                }
            },
            error: function (err) {
                console.error(err);
            }
        });
    }
});