function logoutAndReload() {
    $.post('/Account/Logout', function () {
        location.reload();
    });
    return false;
}