function showPassword() {
    var PasswordInput = document.getElementById('pass');
    var check = document.getElementById('change');

    if (check.checked) {
        PasswordInput.type = "text";
    } else {
        PasswordInput.type = "password";
    }
}
