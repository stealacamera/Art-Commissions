const acceptFinalBtn = document.getElementById('accept-final-btn'),
    submitFinalImageForm = document.getElementById('submitFinalImageForm');

if (acceptFinalBtn) {
    acceptFinalBtn.addEventListener('click', () => {
        if (confirm('Are you sure you want to accept this final product? This action will close the commission and you will not be able to open it up again!')) {
            $.ajax({
                url: `/orders/${acceptFinalBtn.dataset.entityId}/finish`,
                type: 'PATCH',
                success: () => location.reload(),
                error: xhr => {
                    if (xhr.status == 500)
                        alert('Something went wrong, please try again later');
                    else
                        alert(xhr.responseText);
                }
            })
        }
    })
}

if (submitFinalImageForm) {
    submitFinalImageForm.addEventListener('submit', e => {
        e.preventDefault();
        document.body.style.cursor = 'progress';

        if (confirm('Are you sure you want to submit this? (You can resubmit later if needed. If there is an existing final product, it will be removed permanently.)')) {
            var formData = new FormData();

            Array.from(submitFinalImageForm.elements).forEach(input => {
                if (input.name)
                    formData.append(input.name, input.value)
            })

            formData.set('FinalImage', submitFinalImageForm.elements['FinalImage'].files[0]);

            $.ajax({
                url: submitFinalImageForm.action,
                type: 'POST',
                data: formData,
                contentType: false,
                processData: false,
                success: () => location.reload(),
                error: xhr => {
                    document.body.style.cursor = 'default';

                    if (xhr.status === 400)
                        $(`#${submitFinalImageForm.id}`).html(xhr.responseText);
                    else if (xhr.status === 401)
                        alert('You do not have permission to perform this action');
                    else
                        alert('Something went wrong and your request couldn\'t be processed. If this problem persists, please contact us');
                }
            });
        }
    });
}