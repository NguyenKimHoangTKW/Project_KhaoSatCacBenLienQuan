$(document).ready(function () {
    LoadSurvey();
    $("#year").change(function () {
        LoadSurvey();
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
                }
            }
        }
    });
}


$(document).on('change', '#year_dap_vien_select', function () {
    load_dap_vien();
})
async function load_dap_vien() {

    const res = await $.ajax({
        url: '/CTDT/ThongKeKetQuaKhaoSatTheoYeuCau/load_doi_tuong',
        type: 'POST',
        data: {
            idnamhoc: 1
        }   
    })
    console.log(res);
}
