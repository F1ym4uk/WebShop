

//  Reload page with uwer loguot
function logoutAndReload() {
    $.post('/Account/Logout', function () {
        location.reload();
    });
    return false;
}





// block change readonly from input where class="readonly"
document.addEventListener('DOMContentLoaded', function () {
    const emailInput = document.querySelector('.readonly');

    if (emailInput) {
        const observer = new MutationObserver(mutations => {
            mutations.forEach(mutation => {
                if (!emailInput.hasAttribute('readonly')) {
                    emailInput.setAttribute('readonly', 'readonly');
                }
            });
        });

        observer.observe(emailInput, { attributes: true, attributeFilter: ['readonly'] });

        emailInput.addEventListener('keydown', e => e.preventDefault());
        emailInput.addEventListener('paste', e => e.preventDefault());
        emailInput.addEventListener('cut', e => e.preventDefault());
    }
});