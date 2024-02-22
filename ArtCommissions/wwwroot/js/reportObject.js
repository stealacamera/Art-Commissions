const reportForm = document.getElementById('report-form');

reportForm.addEventListener('submit', e => {
    e.preventDefault();

    if (confirm('Are you sure you want to submit this report?')) {
        $.ajax({
            url: reportForm.action,
            type: 'POST',
            data: $(`#${reportForm.id}`).serialize(),
            success: () => alert('Your report was submitted. Our team will look into it.'),
            error: xhr => {
                if (xhr.status == 400)
                    $(`#${reportForm.id}`).html(xhr.responseText);
                else if (xhr.status === 401)
                    alert('You are not authorized to perform this action');
                else
                    alert('Something went wrong trying to process your request, please try again later');
            }
        });
    }
})