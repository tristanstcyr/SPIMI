
alert('begin');
function getQueryResults() {
    var queryParam = $("input#query").attr("value");
    alert('sending:' + queryParam);
    $.ajax({
        type: "GET",
        url: "/Query",
        data: {
            query: queryParam
        },
        dataType: "json",
        success: function (result) {
            alert('success:' + result);
            $("input#query").attr("value", "");
        },
        error: function (req, status, error) {
            alert('failure:' + error);
            $("input#query").attr("value", "error");
        }
    });
}

$(document).ready(function () {
    alert('wtf');
    $("#queryBtn").click(getQueryResults);
    alert('ok');
});

