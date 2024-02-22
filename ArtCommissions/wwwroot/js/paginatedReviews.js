const paginatedReviewsContainer = document.getElementById('reviews');
const prevBtn = document.getElementById('reviews-prev-btn'), nextBtn = document.getElementById('reviews-next-btn');

if (prevBtn)
    prevBtn.addEventListener('click', () => sendRequestForReviewsPaginationView(prevBtn.dataset.commissionId, prevBtn.dataset.currPage - 1));

if (nextBtn)
    nextBtn.addEventListener('click', () => sendRequestForReviewsPaginationView(nextBtn.dataset.commissionId, nextBtn.dataset.currPage + 1));

function sendRequestForReviewsPaginationView(commissionId, page) {
    $.ajax({
        url: `/Reviews/GetReviewsPaginatedListPartial?commissionId=${commissionId}&page=${page}`,
        action: 'GET',
        success: response => paginatedReviewsContainer.innerHTML = response,
        error: () => alert('Something went wrong with your request, try again later')
    });
}