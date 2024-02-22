// Buttons
const cancelOrderBtn = document.getElementById('cancel-order-btn'),
      cancelInvoiceBtn = document.getElementById('cancel-invoice-btn');

// Forms
const upsertInvoiceForm = document.getElementById('upsertInvoiceForm');

if (cancelOrderBtn) {
    cancelOrderBtn.addEventListener('click', e => {
        e.preventDefault();

        if (confirm('Are you sure you want to reject this order?')) {
            $.ajax({
                url: `/orders/${e.currentTarget.dataset.entityId}/cancel`,
                type: 'PATCH',
                success: () => location.reload(),
                error: xhr => {
                    if (xhr.status == 500)
                        alert('Something went wrong, please try again later');
                    else
                        alert(xhr.responseText)
                }
            })
        }
    })
}

if (cancelInvoiceBtn) {
    cancelInvoiceBtn.addEventListener('click', e => {
        e.preventDefault();

        if (confirm('Are you sure you want to cancel the current open invoice?')) {
            $.ajax({
                url: `/orders/${cancelInvoiceBtn.dataset.orderId}/invoices/${cancelInvoiceBtn.dataset.entityId}/cancel`,
                type: 'PATCH',
                success: () => location.reload(),
                error: xhr => {
                    console.log(xhr.status)
                    if (xhr.status == 401)
                        alert('You are not authorized for this action');
                    else if (xhr.status == 400)
                        alert(xhr.responseText);
                    else
                        alert('Something went wrong with your request. Try again later.');
                }
            })
        }
    })
}

if (upsertInvoiceForm) {
    upsertInvoiceForm.addEventListener('submit', e => {
        e.preventDefault();

        $.ajax({
            url: upsertInvoiceForm.action,
            type: 'POST',
            data: $(`#${upsertInvoiceForm.id}`).serialize(),
            success: () => location.reload(),
            error: xhr => {
                if (xhr.status === 400)
                    $(`#${upsertInvoiceForm.id}`).html(xhr.responseText);
                else if (xhr.status === 401)
                    alert('You do not have permission to perform this action');
                else
                    alert('Something went wrong with your request. Please contact us if this issue persists.');
            }
        });
    });
}
