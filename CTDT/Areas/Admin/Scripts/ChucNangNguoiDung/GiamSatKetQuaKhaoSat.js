
$(document).ready(function () {
    $("#hedaotao, #year").on("change", load_pks_by_nam);

    $("#btnThongKeMonHocGV").click(function () {
        load_gv_by_mh();
    })
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
        contentType: 'application/json',
        data: JSON.stringify({
            surveyID: surveyid,
            id_ctdt: ctdtid
        })
    });

    if (res.success) {
        const data = res.data;
        const lopSelect = $("#lop-fil-mh");
        lopSelect.empty().append('<option value="">Tất cả</option>');

        const lopData = [...new Set(data.map(item => item.lop))];
        lopData.forEach(lop => {
            lopSelect.append(`<option value="${id_lop}">${lop}</option>`);
        });

        lopSelect.on('change', function () {
            const selectedLop = $(this).val();
            const filteredSubjects = data.filter(item => item.lop === selectedLop);

            const mhSelect = $("#mh-fil-mh");
            mhSelect.empty().append('<option value="">Tất cả</option>');
            filteredSubjects.forEach(subject => {
                mhSelect.append(`<option value="${subject.id_hoc_phan}">${subject.ten_hoc_phan}</option>`);
            });

            $("#gv-fil-mh").empty().append('<option value="">Chọn giảng viên</option>');

            mhSelect.on('change', function () {
                const selectedMh = $(this).val();
                const selectedData = filteredSubjects.find(item => item.id_hoc_phan == selectedMh);
                const gvSelect = $("#gv-fil-mh");
                gvSelect.empty().append('<option value="">Tất cả</option>');

                if (selectedData && selectedData.giang_vien) {
                    selectedData.giang_vien.forEach(gv => {
                        gvSelect.append(`<option value="${gv.id_giang_vien}">${gv.ten_giang_vien}</option>`);
                    });
                }
            });
        });
    } else {
        alert(res.message || "Lỗi xảy ra khi tải dữ liệu.");
    }
}
