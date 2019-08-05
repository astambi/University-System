// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Custom File Upload form (upload Exam & upload resource) => Displays file name on select
const customFileUpload = inputSelector => {
    const customFileInput = ".custom-file-input";
    const customFileLabel = ".custom-file-label";

    const handleFileName = ({ target }) => {
        const fileName = target.value.split("\\").pop();

        $(target)
            .siblings(customFileLabel)
            .addClass("selected")
            .addClass("text-truncate")
            .html(fileName);
    };

    const selector = inputSelector || customFileInput;

    $(selector).change(event => handleFileName(event));
};

// Bootstrap Tooltips (requires Popper.js)
$(() => $('[data-toggle="tooltip"]').tooltip());

// File upload form with modal
$("#form-submit").click(event => {
    console.log(event.target);
    $("#upload-form").submit();
});