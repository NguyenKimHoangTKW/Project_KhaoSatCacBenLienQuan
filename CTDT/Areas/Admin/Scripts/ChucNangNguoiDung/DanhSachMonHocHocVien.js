$(document).ready(function () {
    $('#importExcelForm').on('submit', async function (e) {
        e.preventDefault();
        var formData = new FormData(this);

        try {
            let response = await $.ajax({
                url: '/Admin/DanhSachMonHocHocVien/UploadExcel',
                type: 'POST',
                data: formData,
                contentType: false,
                processData: false
            });

            if (response.status.includes('Thành công')) {
                alert(response.status);
                $('#importExcelModal').modal('hide');
                LoadData(currentPage);
            } else {
                alert(response.status);
            }
        } catch (error) {
            alert('Đã xảy ra lỗi: ' + error.statusText);
        }
    });
})

