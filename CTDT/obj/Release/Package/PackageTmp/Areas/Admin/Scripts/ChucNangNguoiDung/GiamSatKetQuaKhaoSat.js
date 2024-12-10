
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
    let html_ctdt = "";
    if (res.success) {
        res.data.forEach(function (item) {
            html += `<option value="${item.id_phieu}">${item.ten_phieu}</option>`;
        });
        $("#surveyid").empty().html(html);
        res.ctdt.forEach(function (ctdt) {
            html_ctdt += `<option value="${ctdt.id_ctdt}">${ctdt.ten_ctdt}</option>`;
        })
        $("#ctdt").empty().html(html_ctdt);
    }
    else {
        html += `<option value=""">${res.message}</option>`;
        $("#surveyid").empty().html(html);
        $("#ctdt").empty().html(html);
    }
}

async function load_gv_by_mh() {
    const surveyid = $("#surveyid").val();
    const ctdtid = $("#ctdt").val();
    const res = await $.ajax({
        url: '/api/loc-giang-vien-by-mon-hoc',
        type: 'POST',
        data: {
            surveyID: surveyid,
            id_ctdt: ctdtid,
        }
    });
    if (res.success) {

    }
    else {

    }
}