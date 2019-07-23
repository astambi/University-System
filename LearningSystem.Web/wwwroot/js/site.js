// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Custom File Upload form (for Exam and Course Resources file uploads)
// Add the following code if you want the name of the file appear on select
$(".custom-file-input").on("change", function () {
    var fileName = $(this)
        .val()
        .split("\\")
        .pop();

    $(this)
        .siblings(".custom-file-label")
        .addClass("selected")
        .addClass("text-truncate")
        .html(fileName);
});