var surveyid = document.getElementById('idSurvey').value;
var cbvcid = document.getElementById('idCBVC').value;
var id = document.getElementById('id').value;
$(document).ready(function () {
    checkVisibility(); // Initial check for visibility on page load
    GetByID(surveyid);
    $('#submit_phieu').click(function () {
        $('#captchaModal').modal('show');
    });

    $('#verifyCaptchaBtn').click(function () {
        var captchaResponse = grecaptcha.getResponse();
        if (captchaResponse == '') {
            alert('Vui lòng xác nhận Captcha.');
            return;
        }
        saveFormData();
        checkVisibility();
        $('#captchaModal').modal('hide');
        grecaptcha.reset();
    });

    $('.inputVal2, input[type="text"], input[type="number"], input[type="email"], textarea').blur(function () {
        if ($(this).hasClass('required-ques') && $(this).val().trim() === '') {
            $(this).closest('.item-ques').find('[id^="alert_"]').text("Câu hỏi này là bắt buộc.").show();
        } else {
            $(this).closest('.item-ques').find('[id^="alert_"]').hide();
        }
    });

    $('input[type=radio], input[type=checkbox]').change(checkVisibility);
    var backToTopButton = $('#back-to-top');

    $(window).scroll(function () {
        if ($(window).scrollTop() > 300) {
            backToTopButton.fadeIn();
        } else {
            backToTopButton.fadeOut();
        }
    });

    backToTopButton.click(function () {
        $('html, body').animate({ scrollTop: 0 }, '300');
    });

    $(window).on('scroll', function () {
        var docHeight = $(document).height();
        var winHeight = $(window).height();
        var scrollTop = $(window).scrollTop();
        var scrollPercent = (scrollTop / (docHeight - winHeight)) * 100;
        $('#progress-bar').css('width', scrollPercent + '%');
    });

    var titleElement = $('.survey-title');
    var titleText = titleElement.text();
    var codeElement = $('.code-survey-title');
    var splitTitle = titleText.split('.').map(function (part) {
        return part.trim();
    }).filter(function (part) {
        return part.length > 0;
    });
    codeElement.html(splitTitle[0]);
    titleElement.html(splitTitle[1]);

    document.querySelector('.code-survey-title').addEventListener('mouseover', function () {
        this.style.transform = 'scale(1.2)';
    });

    document.querySelector('.code-survey-title').addEventListener('mouseout', function () {
        this.style.transform = 'scale(1)';
    });
});
function GetByID(id) {
    $.ajax({
        url: '/Survey/GetByID',
        type: 'GET',
        data: { id: id },
        success: function (res) {
            if (res.status !== "Load dữ liệu thành công") {
                alert(res.status);
                return;
            }
            $('#id_donvi').val(res.data.id_donvi);
            $('#id_CBVC').val(res.data.id_CBVC);
            $('#id_users').val(res.data.id_users);
            $('#id_sv').val(res.data.id_sv);
            $('#id_ctdt').val(res.data.id_ctdt);
        }
    });
}
var timeout;
function startTimer() {
    timeout = setTimeout(function () {
        Swal.fire({
            title: "Quá thời gian thực hiện khảo sát!",
            text: "Bạn sẽ được chuyển hướng về trang chủ.",
            icon: "warning",
            confirmButtonColor: "#3085d6",
            cancelButtonColor: "#d33",
            confirmButtonText: "Đồng ý"
        }).then((result) => {
            if (result.isConfirmed) {
                fetch('/Home/ClearSession', { method: 'POST' })
                    .then(() => {
                        window.location.href = '/Home/Index';
                    })
                    .catch(() => {
                        window.location.href = '/Home/Index';
                    });
            }
        });
    }, 3000000);
}

function resetTimer() {
    clearTimeout(timeout);
    startTimer();
}

document.addEventListener('DOMContentLoaded', (event) => {
    startTimer();
});

document.addEventListener('mousemove', resetTimer);
document.addEventListener('keypress', resetTimer);
document.addEventListener('click', resetTimer);

function saveFormData() {
    var formData = {
        pages: []
    };
    var valid = true;

    $('.item-ques[name^="pages_"]').each(function () {
        var pageName = $(this).attr('name');
        var pageTitle = $(this).find('.ques-header strong').text().trim();

        var pageData = {
            name: pageName.replace(/pages_/g, ''),
            title: pageTitle,
            elements: []
        };

        $(this).nextUntil('.item-ques[name^="pages_"]').each(function () {
            var questionName = $(this).attr('name');
            if (!questionName) return;

            var questionType = $(this).find('input[type="radio"]').length > 0 ? 'radiogroup' :
                $(this).find('input[type="checkbox"]').length > 0 ? 'checkbox' :
                    $(this).find('textarea').length > 0 ? 'comment' :
                        $(this).find('select').length > 0 ? 'dropdown' : 'text';

            var questionShortName = questionName.replace(/questions_/g, '');
            var questionTitle = $(this).find('.ques-text strong').text().trim();

            var questionData = {
                type: questionType,
                name: questionShortName,
                title: questionTitle,
                response: {}
            };

            var isVisible = $(this).is(':visible');
            var isRequired = isVisible && $(this).find('.required-ques').length > 0;
            var alertElement = $(this).find('[id^="alert_"]');

            switch (questionType) {
                case 'radiogroup':
                    var selectedRadio = $(this).find('input[type="radio"]:checked');
                    var selectedValue = selectedRadio.val();
                    var otherInput = $(this).find('input[type="text"]').val();
                    var selectedText = selectedRadio.closest('label').text().trim();
                    if (isRequired && !selectedValue) {
                        valid = false;
                        alertElement.text("Câu hỏi " + questionTitle + " là bắt buộc.").show();
                    } else {
                        alertElement.hide();
                    }
                    questionData.response = {
                        "name": selectedValue,
                        "text": selectedText,
                        "other": otherInput ? otherInput : null
                    };
                    break;

                case 'checkbox':
                    var checkedValues = [];
                    var checkedTexts = [];
                    $(this).find('input[type="checkbox"]:checked').each(function () {
                        checkedValues.push($(this).val());
                        checkedTexts.push($(this).closest('label').text().trim());
                    });
                    var otherInput = $(this).find('input[type="text"]').val();
                    if (isRequired && checkedValues.length === 0) {
                        valid = false;
                        alertElement.text(questionTitle + " là bắt buộc.").show();
                    } else {
                        alertElement.hide();
                    }
                    questionData.response = {
                        "name": checkedValues,
                        "text": checkedTexts,
                        "other": otherInput ? otherInput : null
                    };
                    break;

                case 'comment':
                    var inputValue = $(this).find('textarea').val();
                    if (isRequired && !inputValue) {
                        valid = false;
                        alertElement.text(questionTitle + " là bắt buộc.").show();
                    } else {
                        alertElement.hide();
                    }
                    questionData.response = {
                        "text": inputValue
                    };
                    break;

                case 'dropdown':
                    var selectedOption = $(this).find('select option:selected').val();
                    if (isRequired && !selectedOption) {
                        valid = false;
                        alertElement.text(questionTitle + " là bắt buộc.").show();
                    } else {
                        alertElement.hide();
                    }
                    questionData.response = {
                        "text": selectedOption
                    };
                    break;

                default:
                    var inputValue = $(this).find('input[type="text"], input[type="number"], input[type="email"]').val();
                    if (isRequired && !inputValue) {
                        valid = false;
                        alertElement.text(questionTitle + " là bắt buộc.").show();
                    } else {
                        alertElement.hide();
                    }
                    questionData.response = {
                        "text": inputValue
                    };
                    break;
            }

            pageData.elements.push(questionData);
        });

        formData.pages.push(pageData);
    });

    if (valid) {
        if (!$(this).data('saved')) {
            var id_donvi = $('#id_donvi').val();
            var id_CBVC = $('#id_CBVC').val();
            var id_users = $('#id_users').val();
            var id_sv = $('#id_sv').val();
            var id_ctdt = $('#id_ctdt').val();
            $.ajax({
                url: '/Survey/EditAnswer',
                type: 'POST',
                dataType: 'JSON',
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify({
                    id: surveyid,
                    id_donvi: id_donvi,
                    id_CBVC: id_CBVC,
                    id_users: id_users,
                    id_sv: id_sv,
                    id_ctdt: id_ctdt,
                    json_answer: JSON.stringify(formData),
                    surveyID: id,
                    captchaResponse: grecaptcha.getResponse()
                }),
                success: function (response) {
                    Swal.fire({
                        icon: 'success',
                        title: response.status,
                        showConfirmButton: false,
                        timer: 2000
                    }).then(() => {
                        window.location.href = '/Home/Index';
                    });
                },
                error: function (response) {
                    alert(response.status);
                },
            });
            $(this).data('saved', true);
        }
    } else {
        Swal.fire({
            icon: 'error',
            title: 'Vui lòng hoàn thành tất cả các câu hỏi bắt buộc',
            showConfirmButton: true
        });
    }
}

function checkVisibility() {
    $('.item-ques').each(function () {
        var visibleIf = $(this).data('visibleif');
        var isRequiredField = $(this).find('.inputVal2, input[type="text"], input[type="number"], input[type="email"], textarea').hasClass('required-ques');

        if (visibleIf) {
            var isVisible = false;
            var conditions = visibleIf.split(',');
            for (var i = 0; i < conditions.length; i++) {
                var condition = conditions[i].trim();
                if ($('input[name=' + condition.split('_')[0] + ']:checked').val() === condition) {
                    isVisible = true;
                    break;
                }
            }
            if (isVisible) {
                $(this).show();
                if (isRequiredField) {
                    $(this).find('.inputVal2, input[type="text"], input[type="number"], input[type="email"], textarea').addClass('required-ques');
                }
            } else {
                $(this).hide();
                if (isRequiredField) {
                    $(this).find('.inputVal2, input[type="text"], input[type="number"], input[type="email"], textarea').removeClass('required-ques');
                }
            }
        } else {
            $(this).show();
        }
    });
}

$(document).ready(function() {
    checkVisibility();
    $('input[type=radio], input[type=checkbox]').change(checkVisibility);
});
