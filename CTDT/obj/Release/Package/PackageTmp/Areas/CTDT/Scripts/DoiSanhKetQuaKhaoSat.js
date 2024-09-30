$(".select2").select2();
$(document).ready(function () {
    GetYearBySurvey();
    GetSurveyByYear();
    $('#surveyid').change(function () {
        GetYearBySurvey();
    });

    $(document).on("click", "#AddRow", function () {
        var newRow = `
            <div class="row items-push mb-3">
                <div class="col-5">
                    <label class="form-label font-weight-bold" style="font-size: 16px; color: #333;">Năm học</label>
                    <select class="form-control select2" id="year">
                        @foreach (var items in ViewBag.Year)
                        {
                            <option value="@items.Value">@items.Text</option>
                        }
                    </select>
                </div>
                <div class="col-5">
                    <label class="form-label font-weight-bold" style="font-size: 16px; color: #333;">Phiếu khảo sát</label>
                    <select class="form-control select2" id="ssss">
                        <option value="">phiếu 1</option>
                        <option value="">phiếu 2</option>
                    </select>
                </div>
                <div class="col-2 text-center">
                    <button type="button" class="btn btn-danger btn-sm btn-delete-row mt-4">Xóa</button>
                </div>
            </div>
        `;
        $("#comparison-rows").append(newRow);
    });
    $(document).on("click", ".btn-delete-row", function () {
        $(this).closest('.row').remove();
    });
});

function GetYearBySurvey() {
    var surveyid = $('#surveyid').val().toLowerCase();
    $.ajax({
        url: '/CTDT/DoiSanhKetQuaKhaoSat/LoadYearByPKS',
        type: 'GET',
        data: { namesurvey: surveyid },
        success: function (res) {
            let checkboxes = '';
            $('#yearCheckboxes').empty();
            $.each(res.data, function (index, year) {
                checkboxes = `
                    <input id="${year.TenNamHoc}" type="checkbox">
                    <label for="${year.TenNamHoc}">${year.TenNamHoc}</label>
                `;
                $('#yearCheckboxes').append(checkboxes);
            });
        },
        error: function () {
            alert('Error loading years');
        }
    });
};
function GetSurveyByYear() {
    $.ajax({
        url: '/CTDT/DoiSanhKetQuaKhaoSat/LoadSurveyByYear',
        type: 'GET',
        success: function (res) {
            let Year = $("#year");
            let SurveyByYear = $("#surveybyyear");
            let html = ``;
            Year.empty();
            SurveyByYear.empty();
            res.data.Year.forEach(function (chil, index) {
                html = `
                    <option value="${chil.IDNamHoc}">${chil.TenNamHoc}</option>
                `;
                Year.append(html);
            });
            function LoadSurveyByYear(id) {
                SurveyByYear.empty();
                let Survey = res.data.DataSurvey[id - 1];
                if (Survey) {
                    Survey.forEach(function (survey, index) {
                        html = `
                            <option value = "${survey.IDSurvey}"> ${survey.NameSurvey}</option>
                        `;
                        SurveyByYear.append(html);
                    });
                }
            }
            if (res.data.Year.length > 0) {
                LoadSurveyByYear(res.data.Year[0].IDNamHoc);
            }

            Year.on('change', function () {
                let selectedYearID = $(this).val();
                LoadSurveyByYear(selectedYearID);
            });
        }
    });
}
