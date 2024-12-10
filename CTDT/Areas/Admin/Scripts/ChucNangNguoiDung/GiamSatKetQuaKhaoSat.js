var check = false;
$(document).ready(function () {
    $("#hedaotao, #year").on("change", load_pks_by_nam);
    $("#btnThongKeMonHocGV").click(function () {
        if (check) {
            load_gv_by_mh();
            $("#loc-mon-hoc-theo-giang-vien").show();
            const bolocGv = $("#loc-giang-vien-theo-mon-hoc");
            bolocGv.hide();
            $("#lop-fil-gv, #gv-fil-gv, #mh-fil-gv").empty().append('<option value="">Chọn</option>');
        } else {
            load_gv_by_mh();
        }       
    });
    $("#btnThongKeGVMonHoc").click(function () {
        if (check) {
            $("#loc-giang-vien-theo-mon-hoc").show();
            const boLocMonHoc = $("#loc-mon-hoc-theo-giang-vien");
            boLocMonHoc.hide();
            $("#lop-fil-mh, #mh-fil-mh, #gv-fil-mh").empty().append('<option value="">Chọn</option>');
            load_mh_by_gv();
        } else {
            load_mh_by_gv();
        }
    });
    $("#btnDongBoLoc").click(function () {
        const boLocMonHoc = $("#loc-mon-hoc-theo-giang-vien");
        const bolocGv = $("#loc-giang-vien-theo-mon-hoc");
        boLocMonHoc.hide();
        bolocGv.hide();
        $("#lop-fil-mh, #mh-fil-mh, #gv-fil-mh").empty().append('<option value="">Chọn</option>');
        $("#lop-fil-gv, #gv-fil-gv, #mh-fil-gv").empty().append('<option value="">Chọn</option>');
    });
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
        check = true;
        const data = res.data;
        const lopSelect = $("#lop-fil-mh");
        lopSelect.empty().append('<option value="">Tất cả</option>');

        const lopData = [...new Map(data.map(item => [item.id_lop, item])).values()];
        lopData.forEach(lop => {
            lopSelect.append(`<option value="${lop.id_lop}">${lop.lop}</option>`);
        });

        lopSelect.on('change', function () {
            const selectedLopId = $(this).val();
            const filteredSubjects = data.filter(item => item.id_lop == selectedLopId);

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
                        gvSelect.append(`<option value="${gv.id_giang_vien}">${gv.ma_giang_vien} - ${gv.ten_giang_vien}</option>`);
                    });
                }
            });
        });
    } else {
        check = false;
        const Toast = Swal.mixin({
            toast: true,
            position: "top-end",
            showConfirmButton: false,
            timer: 2000,
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


async function load_mh_by_gv() {
    const surveyid = $("#surveyid").val();
    const ctdtid = $("#ctdt").val();
    const res = await $.ajax({
        url: '/api/loc-mon-hoc-by-giang-vien',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            surveyID: surveyid,
            id_ctdt: ctdtid
        })
    });

    if (res.success) {
        check = true;
        const data = res.data;
        const lopSelect = $("#lop-fil-gv");
        lopSelect.empty().append('<option value="">Tất cả</option>');
        const uniqueClasses = [...new Map(data.map(item => [item.id_lop, item])).values()];
        uniqueClasses.forEach(lop => {
            lopSelect.append(`<option value="${lop.id_lop}">${lop.ten_lop}</option>`);
        });

        lopSelect.off('change').on('change', function () {
            const selectedLopId = $(this).val();
            const filteredByClass = data.filter(item => item.id_lop == selectedLopId);

            const gvSelect = $("#gv-fil-gv");
            gvSelect.empty().append('<option value="">Tất cả</option>');

            const uniqueLecturers = [...new Map(filteredByClass.map(item => [item.id_giang_vien, item])).values()];
            uniqueLecturers.forEach(gv => {
                gvSelect.append(`<option value="${gv.id_giang_vien}">${gv.ma_giang_vien} - ${gv.ten_giang_vien}</option>`);
            });

            $("#mh-fil-gv").empty().append('<option value="">Tất cả</option>');
            gvSelect.off('change').on('change', function () {
                const selectedGVId = $(this).val();
                const selectedLecturer = filteredByClass.find(item => item.id_giang_vien == selectedGVId);

                const mhSelect = $("#mh-fil-gv");
                mhSelect.empty().append('<option value="">Tất cả</option>');

                if (selectedLecturer && selectedLecturer.mon_hoc) {
                    selectedLecturer.mon_hoc.forEach(mh => {
                        mhSelect.append(`<option value="${mh.id_mon_hoc}">${mh.ten_mon_hoc}</option>`);
                    });
                }
            });
        });
    } else {
        check = false;
        const Toast = Swal.mixin({
            toast: true,
            position: "top-end",
            showConfirmButton: false,
            timer: 2000,
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

$(document).on("click", "#fildata", function () {
    test();
})
async function test() {
    const surveyid = $("#surveyid").val();
    const ctdt = $("#ctdt").val();
    const lop = $("#lop-fil-mh").val();
    const mh = $("#mh-fil-mh").val();
    const gv = $("#gv-fil-mh").val();
    const res = await $.ajax({
        url: '/api/test',
        type: 'POST',
        data: JSON.stringify({
            surveyID: surveyid,
            id_ctdt: ctdt,
            id_lop: lop,
            id_mh: mh,
            id_CBVC: gv
        })
    })
    console.log(res)
}