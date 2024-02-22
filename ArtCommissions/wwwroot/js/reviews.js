const addReviewForm = document.getElementById('add-review-form');

$(function () {
    $('#Rating').barrating({
        theme: 'css-stars'
    });
});

addReviewForm.addEventListener('submit', e => {
    e.preventDefault();

    $.ajax({
        url: addReviewForm.action,
        type: 'POST',
        data: $(`#${addReviewForm.id}`).serialize(),
        success: () => location.reload(),
        error: xhr => {
            if (xhr.status === 400) {
                $(`#${addReviewForm.id}`).html(xhr.responseText);

                $(function () {
                    $('#Rating').barrating({
                        theme: 'css-stars'
                    });
                });
            }
            else if (xhr.status === 409)
                alert('You have already left a review for this commission!');
            else
                alert('Something went wrong with your request, please try again later');
        }
    });
});