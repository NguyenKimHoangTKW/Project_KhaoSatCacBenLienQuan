
var dataSurvey = [];
$(document).ready(function () {
    const btn_submit_phieu = document.getElementById("submit_phieu");
    const base_url = document.getElementById("base_url").value;
    const surveyID = document.getElementById("surveyID").value;

    $("#spinner-div").show();
    $.ajax({
        url: base_url + "survey/loadQuestion/" + surveyID,
        type: "GET",
        async: false,
        cache: false,
    }).done(function(dataResult){
        setTimeout(()=>{
            dataSurvey = JSON.parse(dataResult);
            $("#spinner-div").hide();
            document.getElementsByName("form_survey")[0].style.display = null;
        },1000);
    }).fail(function( jqXHR, textStatus, errorThrown ) {
        $("#spinner-div").hide();
        showError("Có lỗi khi lấy dữ liệu: " + errorThrown);
    })




    btn_submit_phieu.addEventListener('click', function () {
        var dataPostSurvey = {};
        /*btn_submit_phieu.disabled = true;
        btn_submit_phieu.innerHTML = btn_submit_phieu.getAttribute("data-loading-text");*/
        var errorParagraphs = document.querySelectorAll('p[name="tag_p_error"]');
        errorParagraphs.forEach(function (paragraph) {
            paragraph.style.display = 'none';
        });
        var data = $("#form_survey").serialize();
        $.ajax({
            url: base_url + "survey/checkSurvey/" + surveyID,
            type: "POST",
            data: data,
            async: true,
            cache: false,
            success: function (dataResult) {
                btn_submit_phieu.disabled = false;
                btn_submit_phieu.innerHTML = "GỬI";
                var data = JSON.parse(dataResult);
                if (data['code'] === 1) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Thành công !!!',
                        text: data['message'],
                    }).then(function () {
                        window.location.href = data['url'];
                    });
                } else if (data['code'] === 0) {
                    if (data['questionType'] === "checkbox") {
                        document.getElementsByName(data['questionID'] + "[]")[0].focus();
                    } else {
                        document.getElementsByName(data['questionID'])[0].focus();
                    }
                    let errorQuestion = document.getElementById("alert_" + data['questionID']);
                    errorQuestion.style.display = "";
                    document.getElementById("alert_" + data['questionID']).innerText = data['message'];
                }
            },
            error: function (xhr) {
                alert("Error_Load_Data");
            }
        });
    });
});




function checkItem(item){
    $.each(dataSurvey.data.pages,function(key_pages, pages){
        if (pages.visible !== undefined){
            $.each(pages.visibleIf,function(key_visibleIf, visibleIfs){
                const arrVisibleIfs = visibleIfs.split("_");
                if (item.name === arrVisibleIfs[0]){
                    if (visibleIfs === item.value){
                        document.getElementsByName("pages_"+pages.name)[0].style.display = null;
                        return false;
                    }else {
                        document.getElementsByName("pages_"+pages.name)[0].style.display = "none";
                    }
                }
            });
        }

        $.each(pages.elements,function(key_questions, questions){
            if (questions.visible !== undefined){
                $.each(questions.visibleIf,function(key_visibleIf, visibleIfs){
                    const arrVisibleIfs = visibleIfs.split("_");
                    if (item.name === arrVisibleIfs[0]){
                        if (visibleIfs === item.value){
                            document.getElementsByName("questions_"+questions.name)[0].style.display = null;
                            return false;
                        }else {
                            document.getElementsByName("questions_"+questions.name)[0].style.display = "none";
                        }
                    }
                });
            }
        });
    });


}



function _alert(mess) {
    alert(mess);
}
