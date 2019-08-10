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

customFileUpload("#customFile");

// Bootstrap Tooltips (requires Popper.js)
$(() => $('[data-toggle="tooltip"]').tooltip());

// File upload form with modal
$("#form-submit").click(() => $("#upload-form").submit());

// Notifications fade out
const notificationFadeOut = (selector, timeout = 3000) => selector
    .fadeTo(timeout, 0.7)
    .fadeOut(timeout / 2);

notificationFadeOut($(".alert.alert-info"));
notificationFadeOut($(".alert.alert-success"));
notificationFadeOut($(".alert.alert-warning"));

// Toggle Order details (order items) on orders list
const toggleOrderDetails = event => {
    const { target } = event;
    const { id } = target;

    const orderSelector = "tr";
    const orderDetailsClass = "table-info font-weight-bold";

    const orderItemSelector = `.${id}`;
    $(orderItemSelector).toggle("fast");

    const order = $(target).parents(orderSelector);
    order.hasClass(orderDetailsClass)
        ? order.removeClass(orderDetailsClass)
        : order.addClass(orderDetailsClass);
};

$(".order-details-toggler").click(event => toggleOrderDetails(event));
