$(document).ready(function () {

});
function Toast_alert(type, message) {
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
        icon: type,
        title: message
    });
}
$(document).on("change", "#find-ctdt", function () {
    load_survey();
});


async function load_survey() {
    const year = $("#yearGiamSat").val();
    const ctdt = $("#find-ctdt").val();
    const res = await $.ajax({
        url: '/api/ctdt/load-survey-check-ctdt',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            id_namhoc: year,
            id_ctdt: ctdt
        })
    });
    const body = $("#find-survey");
    body.empty();
    let html = ``;
    if (res.success) {
        const data = JSON.parse(res.data);
        data.sort((a, b) => {
            const idA = a.name.split(".")[0];
            const idB = b.name.split(".")[0];
            return idA.localeCompare(idB, undefined, { numeric: true });
        });
        data.forEach(items => {
            html += `<option value="${items.value}">${items.name}</option>`
        });
        body.html(html).trigger("change");
    }
    else {
        html += `<option value="">${res.message}</option>`
        body.html(html).trigger("change");
    }
};