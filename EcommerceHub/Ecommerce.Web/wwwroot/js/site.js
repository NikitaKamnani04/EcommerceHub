// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// const { post } = require("jquery");

// Write your JavaScript code.


// $(document).ready() - ye jQuery ka tareeka hai "jab poora page load ho jaye, tab ye code chalao"
// Isse pehle chalate toh HTML elements abhi bane hi nahi hote, click events attach nahi hote

$(document).ready(function () {

    // "Add to Cart" button pe click event laga rahe hain
    // Lekin dhyan do - hum "document" pe event laga rahe hain, button pe direct nahi
    // Kyun? Kyunki Product Details page ka button abhi tak humne banaya nahi tha is tarike se
    // Ye "event delegation" kehlata hai - future mein bane buttons pe bhi kaam karega

    $(document).on('click', '.add-to-cart-btn', function () {


        // "this" - jis button pe click hua, usi ka reference
        // data-product-id attribute se ProductID nikaal rahe hain
        var productId = $(this).data('product-id');

        // Quantity input se value nikaal rahe hain (agar hai toh), warna default 1
        var quantity = $('#quantity').val() || 1;

        // $.ajax() - jQuery ka tareeka hai server ko HTTP request bhejne ka, BINA page reload kiye
        $.ajax({
            url: '/Cart/AddToCart',  // Kaunse controller/action ko call karna hai
            type: 'POST',   // POST request (data bhej rahe hain)
            contentType: 'application/json', // Batao ki hum JSON bhej rahe hain
            data: JSON.stringify({
                productId: productId,
                quantity: quantity
            }),
            success: function (response) {
                // Agar request successful hui (server ne 200 OK bheja)
                // Cart count badge ko update karo (navbar mein)

                $('#cartCount').text(response.cartCount);

                // Chhota "Added!" feedback dikhao (temporary)
                showToast('Added to cart successfully!');
            },
            error: function (xhr) {
                // Agar error aayi (jaise 401 - login nahi hai, ya 400 - stock nahi hai)
                if (xhr.status === 401) {
                    showToast('Please login to add items to cart.');
                }
                else {
                    showToast('Something went wrong. Please try again.');
                }
            }

        });

    });

    // ==================== REMOVE FROM CART ====================

    $(document).on('click', '.remove-cart-item', function () {

        var cartItemId = $(this).data('cart-item-id');

        $.ajax({
            url: '/Cart/Remove',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(cartItemId),

            success: function () {
                showToast('Item removed from cart.');

                // Cart page ko reload karo
                location.reload();
            },

            error: function (xhr) {
                if (xhr.status === 401) {
                    showToast('Please login first.');
                }
                else {
                    showToast('Unable to remove item. Please try again.');
                }
            }
        });

    });



    // Wishlist button pe click event
    $(document).on('click', '.wishlist-btn', function () {
        var button = $(this);
        var productId = button.data('product-id');

        // Check karo - ye chhota icon button hai ya bada text-wala button
        var isIconOnly = button.hasClass('wishlist-icon-btn');

        $.ajax({
            url: '/Wishlist/Toggle',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ productId: productId }),
            success: function (response) {
                if (response.isInWishlist) {
                    if (isIconOnly) {
                        // Icon-only button - sirf symbol change karo (♡ se ♥)
                        button.text('♥').addClass('active');
                    } else {
                        // Bada text-wala button (Details page)
                        button.html('♥ In Wishlist');
                        button.removeClass('btn-outline-danger').addClass('btn-danger');
                    }
                    showToast('Added to wishlist!');
                } else {
                    if (isIconOnly) {
                        button.text('♡').removeClass('active');
                    } else {
                        button.html('♡ Wishlist');
                        button.removeClass('btn-danger').addClass('btn-outline-danger');
                    }
                    showToast('Removed from wishlist.');
                }
            },
            error: function (xhr) {
                if (xhr.status === 401) {
                    showToast('Please login to use wishlist.');
                } else {
                    showToast('Something went wrong. Please try again.');
                }
            }
        });

        // ==================== LOAD WISHLIST ====================

       
    });

    $.ajax({
        url: '/Wishlist/GetWishlistProductIds',
        type: 'GET',

        success: function (productIds) {

            $('.wishlist-btn').each(function () {

                var button = $(this);
                var productId = Number(button.data('product-id'));

                if (productIds.map(Number).includes(productId)) {

                    button
                        .text('♥')
                        .addClass('active');

                }
            });
        }
    });

    // Chhota reusable function - "Toast" notification dikhane ke liye
    // (jaise "Added to cart!" wala popup jo 2-3 second mein gayab ho jata hai)

    function showToast(message) {
        // Agar pehle se koi toast hai, use hata do (duplicate na ho)
        $('.toast-notification').remove();

        // Naya toast element banao aur body mein add karo
        var toast = $('<div class="toast-notification">' + message + '</div>');
        $('body').append(toast);

        // 0.1 second baad "show" class add karo (CSS transition ke liye)
        setTimeout(function () { toast.addClass('show'); }, 100);

        // 3 second baad toast ko hata do
        setTimeout(function () {
            toast.removeClass('show');
            setTimeout(function () { toast.remove(); }, 300);   // fade-out animation complete hone do
        }, 3000);

    }
})
