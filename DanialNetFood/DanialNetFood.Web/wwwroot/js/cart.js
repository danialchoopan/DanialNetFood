function addToCart(foodId, optionIds = []) {
    $.ajax({
        url: '/Cart/AddToCart',
        type: 'POST',
        data: { foodId: foodId, optionIds: optionIds },
        success: function (result) {
            $('#cart-sidebar').html(result);
        },
        error: function () {
            alert('خطا در افزودن به سبد خرید');
        }
    });
}

function removeFromCart(foodId, optionsHash = '') {
    $.ajax({
        url: '/Cart/RemoveFromCart',
        type: 'POST',
        data: { foodId: foodId, optionsHash: optionsHash },
        success: function (result) {
            $('#cart-sidebar').html(result);
        },
        error: function () {
            alert('خطا در حذف از سبد خرید');
        }
    });
}
