// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Custom File Upload form (for Exam and Course Resources file uploads)
// Add the following code if you want the name of the file appear on select
function customFileUpload(selector) {
    $(selector).on("change", (event) => fileUpload(event.target));

    function fileUpload(htmlElement) {
        console.log(htmlElement);

        var fileName = htmlElement
            .value
            .split("\\")
            .pop();

        $(htmlElement)
            .siblings(".custom-file-label")
            .addClass("selected")
            .addClass("text-truncate")
            .html(fileName);
    }
}