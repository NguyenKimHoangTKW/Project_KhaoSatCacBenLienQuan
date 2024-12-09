$(document).ready(function () {
    $("#hedaotao, #year").on("change", load_pks_by_nam);
});
async function load_pks_by_nam() {
    const hedaotao = $("#hedaotao").val();
    const year = $("#year").val();
    const res = await $.ajax({
        url: '/api/load_phieu_by_nam',
        type: 'POST',
        data: {
            id_namhoc: year,
            id_hedaotao: hedaotao
        }
    });
    let html = "";
    if (res.success) {
        res.data.forEach(function (item) {
            html += `<option value="${item.id_phieu}">${item.ten_phieu}</option>`;
        });
        $("#surveyid").empty().html(html);
    }
    else {
        html += `<option value=""">${res.message}</option>`;
        $("#surveyid").empty().html(html);
    }
}
