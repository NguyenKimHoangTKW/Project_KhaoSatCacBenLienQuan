document.addEventListener("DOMContentLoaded", function() {


    const surveyTypeSelect = document.getElementById("surveyType");
    const base_url = $("#base_url").val();
    getDataSurvey(base_url, "all").done(function(data){
        data = JSON.parse(data);
        if (data.code === 0){
            showError(data.message);
        }else if (data.code === 1){
            let str = "";
            $.each(data.data, function(index, value) {
                str += setDataSurvey(base_url, value.surveyID, value.surveyTitle, value.surveyDescription);
            });
            $("#survey-item").html(str);
        }else {
            showError("Có lỗi khi lấy dữ liệu phiếu khảo sát. ");
        }
    }).fail(function( jqXHR, textStatus, errorThrown ) {
        Swal.close();
        showError("Có lỗi khi lấy dữ liệu phiếu khảo sát. " + errorThrown);
    });



    surveyTypeSelect.addEventListener("change", function() {
        const surveyType = this.value;
        Swal.fire({
            title: "Đang tải dữ liệu...",
            html: "Vui lòng đợi...",
            timerProgressBar: true,
            didOpen: () => {
                Swal.showLoading();
                getDataSurvey(base_url, surveyType).done(function(data){
                    Swal.close();
                    data = JSON.parse(data);
                    if (data.code === 0){
                        showError(data.message);
                    }else if (data.code === 1){
                        let str = "";
                        $.each(data.data, function(index, value) {
                            str += setDataSurvey(base_url, value.surveyID, value.surveyTitle, value.surveyDescription);
                        });
                        $("#survey-item").html(str);
                    }else {
                        showError("Có lỗi khi lấy dữ liệu phiếu khảo sát. ");
                    }
                }).fail(function( jqXHR, textStatus, errorThrown ) {
                    Swal.close();
                    showError("Có lỗi khi lấy dữ liệu phiếu khảo sát. " + errorThrown);
                });
            }
        });
    });
});




function getDataSurvey(base_url, surveyType = "all"){
    return $.ajax({
        url: base_url + "survey/getDataSurvey",
        type: "POST",
        data: { surveyType: surveyType },
    });
}

function setDataSurvey(base_url, surveyID, surveyTitle, surveyDescription){
    return `<div class="col-sm-6 mb-4 survey-item">
            <div class="card survey-card border-0">
                <div class="card-body">
                    <h5 class="card-title">`+surveyTitle+`</h5>
                    <p class="card-text">`+surveyDescription.substring(0,250)+`...</p>
                    <a href="`+base_url+`survey/view/`+surveyID+`" class="btn btn-primary btn-sm">Thực hiện khảo sát</a>
                </div>
            </div>
        </div>
    `;
}

function showError(errorMessage){
    Swal.fire({
        icon: "error",
        title: "Oops...",
        text: errorMessage,
    });
}







