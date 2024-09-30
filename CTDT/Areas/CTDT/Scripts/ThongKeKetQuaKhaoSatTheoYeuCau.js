$(document).ready(function () {
    LoadSurvey();
    load_doi_tuong_by_phieu()
    $("#year").change(function () {
        LoadSurvey();
    });
    $('.modal').on('hidden.bs.modal', function () {
        $('#maLopFilter').val('');
        $('.thamso-checkbox').prop('checked', false);
        $('#select-all').prop('checked', false);
        selectedThamSoGlobal = [];
    });
});
var initializing = true;
function LoadSurvey() {
    var defaultYearId = $('#year').val();
    $.ajax({
        url: "/CTDT/ThongKeKetQuaKhaoSatTheoYeuCau/LoadPKSByYear",
        type: "POST",
        data: { id: defaultYearId },
        success: function (res) {
            var surveyid = $("#surveyid");
            surveyid.empty();
            if (res.data.length === 0) {
                var html = '<option value="">Không có dữ liệu phiếu khảo sát cho năm học này</option>';
                surveyid.append(html);
                surveyid.val('').trigger('change');
            } else {
                res.data.forEach(function (Chil) {
                    var html = `<option value="${Chil.ma_phieu}">${Chil.ten_phieu}</option>`;
                    surveyid.append(html);
                });
                if (!initializing) {
                    $("#surveyid").val(res.data[0].ma_phieu).trigger('change');
                } else {
                    $("#surveyid").val(res.data[0].ma_phieu);
                    LoadDoiTuongBySurvey();
                }
            }
        }
    });
}

async function load_doi_tuong_by_phieu() {
    const res = await $.ajax({
        url: '/CTDT/ThongKeKetQuaKhaoSatTheoYeuCau/load_doi_tuong_by_phieu',
        type: 'POST'
    })

    console.log(res);
}

//$(document).on('click', '#save_doi_tuong', function () {
//    save_doi_tuong();
//    load_ty_le()
//});
//function save_doi_tuong() {
//    $('.thamso-checkbox:checked').each(function () {
//        let value = $(this).val();
//        if (!selectedThamSoGlobal.includes(value)) {
//            selectedThamSoGlobal.push(value);
//        }
//    });
//    $('.thamso-checkbox:not(:checked)').each(function () {
//        let value = $(this).val();
//        let index = selectedThamSoGlobal.indexOf(value);
//        if (index !== -1) {
//            selectedThamSoGlobal.splice(index, 1);
//        }
//    });
//    sessionStorage.setItem('doituong', JSON.stringify(selectedThamSoGlobal));
//}

