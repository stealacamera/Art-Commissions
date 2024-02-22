const stripe = Stripe("pk_test_51NSJnMF8jxytcCnso4GvmUfcyoBV78ZMgTajhipzUzChM2kcDCEErvwVVCv2zucHi78cKxBc1DKpTYGTpEx4ZqHA00M7P6VdK8");
let elements;
let clientSecret;

initialize();
checkStatus();

document.querySelector("#payment-form").addEventListener("submit", submitPayment);

function initialize() {
    $.ajax({
        url: `/webhook/create-payment?orderId=${currentOrderId}&invoiceId=${currentInvoiceJson.Id}`,
        method: 'POST',
        success: data => {
            const appearance = { theme: 'stripe' };
            const paymentElementOptions = { layout: 'tabs' };
            elements = stripe.elements({ clientSecret: data.clientSecret, appearance });

            const paymentElement = elements.create('payment', paymentElementOptions);
            paymentElement.mount('#payment-element');
        },
        error: xhr => {
            console.log(xhr)
            alert('Something went wrong with your request. Try again later, and if the problem persists, contact us')
        }
    });


}

async function submitPayment(e) {
    e.preventDefault();
    document.body.style.cursor = 'progress';

    $.ajax({
        url: `/invoices/${currentInvoiceJson.Id}/hasChanged`,
        type: 'POST',
        data: currentInvoiceJson,
        success: async data => {
            if (data) {
                document.body.style.cursor = 'default';
                alert('This invoice has been changed by the vendor! Refresh the page to see updates');
                return;
            }
            else {
                const error = await stripe.confirmPayment({
                    elements,
                    confirmParams: { return_url: window.location.href }
                });

                if (error.type === "card_error" || error.type === "validation_error")
                    showMessage(error.message);
                else
                    showMessage("An unexpected error occurred");

                document.body.style.cursor = 'default';
            }
        },
        error: () => document.body.style.cursor = 'default'
    });
}

async function checkStatus() {
    const clientSecret = new URLSearchParams(window.location.search).get(
        "payment_intent_client_secret"
    );

    if (!clientSecret) {
        return;
    }

    const { paymentIntent } = await stripe.retrievePaymentIntent(clientSecret);

    switch (paymentIntent.status) {
        case "succeeded":
            showMessage("Payment succeeded!");
            break;
        case "processing":
            showMessage("Your payment is processing.");
            break;
        case "requires_payment_method":
            showMessage("Your payment was not successful, please try again.");
            break;
        default:
            showMessage("Something went wrong.");
            break;
    }
}

function showMessage(messageText) {
    const messageContainer = document.querySelector("#payment-message");

    messageContainer.classList.remove("hidden");
    messageContainer.textContent = messageText;

    setTimeout(function () {
        messageContainer.classList.add("hidden");
        messageContainer.textContent = "";
    }, 8000);
}