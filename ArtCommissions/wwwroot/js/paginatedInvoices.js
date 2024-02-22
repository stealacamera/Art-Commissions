const paginatedReviewsContainer = document.getElementById('invoices');
const prevBtn = document.getElementById('invoices-prev-btn'), nextBtn = document.getElementById('invoices-next-btn');

if (prevBtn)
    prevBtn.addEventListener('click', () => sendRequestForReviewsPaginationView(prevBtn.dataset.orderId, prevBtn.dataset.currPage - 1));

if (nextBtn)
    nextBtn.addEventListener('click', () => sendRequestForReviewsPaginationView(nextBtn.dataset.orderId, nextBtn.dataset.currPage + 1));

function sendRequestForReviewsPaginationView(orderId, page) {
    $.ajax({
        url: `/Invoices/GetInvoicesPaginatedListPartial?orderId=${orderId}&page=${page}`,
        action: 'GET',
        success: response => paginatedReviewsContainer.innerHTML = response,
        error: () => alert('Something went wrong with your request, try again later')
    });
}