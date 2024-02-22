const tagsTable = document.getElementById('tags-table'),
    createTagForm = document.getElementById('create-tag-form'),
    tagFormValidation = document.getElementById('tag-form-validation'),
    deleteTagBtns = document.getElementsByClassName('remove-tag-btn'),
    editTagBtns = document.getElementsByClassName('edit-tag-btn');

createTagForm.addEventListener('submit', e => {
    e.preventDefault();

    $.ajax({
        url: '/tags/create',
        type: 'POST',
        data: $(`#${createTagForm.id}`).serialize(),
        success: data => {
            const template = document.createElement('template');
            template.innerHTML = data;
            const newRow = template.content.firstElementChild;

            newRow.getElementsByClassName('remove-tag-btn')[0].addEventListener('click', e => deleteTag(e));
            newRow.getElementsByClassName('edit-tag-btn')[0].addEventListener('click', e => editTag(e));

            tagsTable.tBodies[0].insertAdjacentElement('afterbegin', newRow);
        },
        error: xhr => {
            if (xhr.status == 400)
                tagFormValidation.textContent = xhr.responseText;
            else if (xhr.status == 401)
                alert('You are not authorized to perform this action');
            else
                alert('Something went wrong. Make sure this tag doesn\'t currently exist & try again later');
        }
    });
})

Array.from(deleteTagBtns).forEach(btn => btn.addEventListener('click', e => deleteTag(e)));
Array.from(editTagBtns).forEach(btn => btn.addEventListener('click', e => editTag(e)));

function editTag(e) {
    const btn = e.target;

    const tagCell = document.getElementById(`tag-${btn.dataset.entityId}`);
    const oldValue = tagCell.textContent.trim();

    tagCell.title = 'Press enter to save your changes';
    tagCell.contentEditable = true;
    tagCell.focus();

    tagCell.addEventListener('keydown', keyEvent => {

        if (keyEvent.code === 'Enter') {
            keyEvent.preventDefault();

            var formData = new FormData();
            formData.append('Id', btn.dataset.entityId);
            formData.append('Name', tagCell.textContent.trim());

            $.ajax({
                url: `/tags/${btn.dataset.entityId}/edit`,
                type: 'PATCH',
                data: formData,
                contentType: false,
                processData: false,
                success: () => {
                    tagCell.title = '';
                    tagCell.contentEditable = false;

                    alert('Update successful');
                },
                error: xhr => {
                    tagCell.textContent = oldValue;

                    if (xhr.status == 400)
                        alert(xhr.responseText);
                    else
                        alert('Something went wrong, try again later');
                }
            });
        }
    });
}

function deleteTag(e) {
    const btn = e.target;

    if (confirm('Are you sure you want to delete this tag?')) {
        $.ajax({
            url: `/tags/${btn.dataset.entityId}/delete`,
            type: 'DELETE',
            success: () => document.getElementById(`tag-${btn.dataset.entityId}-row`).remove(),
            error: xhr => {
                if (xhr.status == 401)
                    alert('You are not authorized to perform this action');
                else
                    alert('Something went wrong, try again later');
            }
        });
    }
}