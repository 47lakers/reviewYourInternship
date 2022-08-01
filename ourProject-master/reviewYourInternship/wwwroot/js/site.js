// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
//function loginSignup(bool) {
//    if (bool) {
//        $('#loginSignup').attr('display', 'none');
//    } else {
//        $('#loginSignup').attr('display', 'block');
//    }
//}

async function successMessage(message) {
    event.preventDefault();
    Swal.fire({
        icon: 'success',
        title: message,
        showConfirmButton: false,
        timer: 3000
    });
}

async function errorMessage(message) {
    event.preventDefault();
    Swal.fire({
        icon: 'error',
        title: message,
        showConfirmButton: false,
        timer: 3000
    });
}