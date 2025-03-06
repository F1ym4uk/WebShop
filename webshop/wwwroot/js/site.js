

//  Reload page with uwer loguot
function logoutAndReload() {
    $.post('/Account/Logout', function () {
        location.reload();
    });
    return false;
}


// Validate phone number input
document.addEventListener("DOMContentLoaded", function () {
    document.getElementById("PhoneNumber").addEventListener("input", function () {
        let value = this.value.replace(/\D/g, "");

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

        this.value = formatted;
    });
});
//


// Filters 
function setupDropdown(inputId, dropdownId) {
    let input = document.getElementById(inputId);
    let dropdown = document.getElementById(dropdownId);

    input.addEventListener('focus', function () {
        dropdown.style.display = 'block';
    });

    input.addEventListener('input', function () {
        let filter = input.value.toLowerCase();
        let items = dropdown.querySelectorAll('.dropdown-item');
        items.forEach(item => {
            if (item.textContent.toLowerCase().includes(filter)) {
                item.style.display = "block";
            } else {
                item.style.display = "none";
            }
        });
    });

    dropdown.addEventListener('click', function (event) {
        if (event.target.classList.contains('dropdown-item')) {
            input.value = event.target.textContent;
            dropdown.style.display = 'none';
        }
    });

    document.addEventListener('click', function (event) {
        if (!input.contains(event.target) && !dropdown.contains(event.target)) {
            dropdown.style.display = 'none';
        }
    });
}

setupDropdown('nameInput', 'nameDropdown');
setupDropdown('categoryInput', 'categoryDropdown');

function checkProducts() {
    if (document.getElementById('product-list').children.length === 0) {
        document.getElementById('no-products').style.display = 'block';
    } else {
        document.getElementById('no-products').style.display = 'none';
    }
}

checkProducts();
//



// Filters for admin-panel
document.addEventListener("DOMContentLoaded", function () {
    const stockInput = document.getElementById("stockQuantityInput");
    const errorSpan = document.getElementById("customError");

    stockInput.addEventListener("input", function () {
        if (this.value === "" || isNaN(this.value)) {
            errorSpan.textContent = "Введите корректное число!";
        } else {
            errorSpan.textContent = "";
        }
    });

    stockInput.addEventListener("invalid", function (e) {
        e.preventDefault();
        errorSpan.textContent = "Введите корректное число!";
    });
});
//




// Close change readonly from input where class="readonly"
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
//


 

// Add to cart validate form
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
});
