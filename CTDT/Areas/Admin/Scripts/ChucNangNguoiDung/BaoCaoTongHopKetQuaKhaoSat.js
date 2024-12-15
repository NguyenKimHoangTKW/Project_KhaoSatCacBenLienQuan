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
    let html_ctdt = `<option value="">Tất cả</option>`;

    if (res.success) {

        res.ctdt.forEach(function (ctdt) {
            html_ctdt += `<option value="${ctdt.id_ctdt}">${ctdt.ten_ctdt}</option>`;
        });
        $("#ctdt").empty().html(html_ctdt);
    } else {
        html += `<option value="">${res.message}</option>`;
        $("#surveyid").empty().html(html);
        $("#ctdt").empty().html(html);
    }
}