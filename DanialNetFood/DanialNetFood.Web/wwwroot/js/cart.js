function addToCart(foodId) {
    $.ajax({
        url: '/Cart/AddToCart',
        type: 'POST',
        data: { foodId: foodId },
        success: function (result) {
            $('#cart-sidebar').html(result);
        },
        error: function () {
            alert('خطا در افزودن به سبد خرید');
        }
    });
}

function removeFromCart(foodId) {
    $.ajax({
        url: '/Cart/RemoveFromCart',
        type: 'POST',
        data: { foodId: foodId },
        success: function (result) {
            $('#cart-sidebar').html(result);
        },
        error: function () {
            alert('خطا در حذف از سبد خرید');
        }
    });
}
