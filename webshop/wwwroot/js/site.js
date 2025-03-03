

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




$(document).ready(function () {
    $("#PhoneNumber").on("input", function () {
        let value = $(this).val().replace(/\D/g, "");

        if (value.startsWith("375")) {
            value = value.slice(3);
        }

        if (value.length > 9) {
            value = value.slice(0, 9);
        }

        let formatted = "+375";
        if (value.length > 0) formatted += "(" + value.slice(0, 2);
        if (value.length >= 3) formatted += ")" + value.slice(2, 5);
        if (value.length >= 6) formatted += "-" + value.slice(5, 7);
        if (value.length >= 8) formatted += "-" + value.slice(7, 9);

        $(this).val(formatted);
    });
});





$(document).ready(function () {
    $(document).on('submit', '.add-to-cart-form', function (e) {
        e.preventDefault();

        var form = $(this);
        var productId = form.find('input[name="productId"]').val();

        $.ajax({
            url: '/Cart/AddToCart',
            type: 'POST',
            data: { productId: productId },
            success: function (response) {
                if (response.success) {
                    $('#cart-count').text(response.cartCount);
                } else {
                    alert('Ошибка при добавлении товара в корзину.');
                }
            }
        });
    });

    loadProducts();
});s
