
$(document).ready(function () {

    $('#btnUploadFile').on('click', function () {

        var data = new FormData();

        var files = $("#fileUpload").get(0).files;

        // Add the uploaded image content to the form data collection
        if (files.length > 0) {
            data.append("UploadedImage", files[0]);
        }

        // Make Ajax request with the contentType = false, and procesDate = false
        var ajaxRequest = $.ajax({
            type: "POST",
            url: "/api/fileupload/uploadfile",
            contentType: false,
            processData: false,
            data: data,

            success: function (message) {

                alert(message);

            },
            error: function () {
                alert("Error while invoking the Web API");
            }
        });

        ajaxRequest.done(function (xhr, textStatus) {
            // Do other operation
        });
    });
});


function showXmlInTable() {
    $("#dvContent").append("<table align='center' border='3' padding = '1' width = 50% ><tr ><td width = 50%>Course Code</td><td width = 50%>Course Name</td></tr></table>");
    $.ajax({
        type: "GET",
        url: "UploadedFiles/list.xml",
        dataType: "xml",
        success: function (xml) {
            $(xml).find('course').each(function () {
                var sCode = $(this).find('number').text();
                var sName = $(this).find('title').text();
                $("#dvContent").append("<table align='center'  border='3' padding = '1' width = 50% ><tr><td width = 50% >" + sCode + "</td><td width = 50% >" + sName + "</td></tr></table>");
            });
        },

        error: function () {
            alert("An error occurred while processing XML file.");
        }

    });
}
