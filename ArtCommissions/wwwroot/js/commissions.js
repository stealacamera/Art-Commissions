const commissionStateBtns = document.getElementsByClassName('commission-state-btn'),
      deleteBtns = document.getElementsByClassName('delete-btn');

Array.from(deleteBtns).forEach(btn => btn.addEventListener('click', e => deleteAction(e.currentTarget.dataset.entityId)));

Array.from(commissionStateBtns).forEach(btn => btn.addEventListener('click', e => {
    const newStatus = e.currentTarget.dataset.status.toLowerCase() == 'false' ? true : false;
    changeStatusAction(e.currentTarget.id, e.currentTarget.dataset.entityId, newStatus)
}));

function deleteAction(entityId) {
    if (confirm('Are you sure you want to delete this commission?'))
        deleteCommissionAsync(entityId);
}

function changeStatusAction(viewElementId, entityId, newStatus) {
    if (confirm(`Are you sure you want to ${!newStatus ? `close` : `open up`} this commission?`))
        changeCommissionStatusAsync(viewElementId, entityId, newStatus);
}

function deleteCommissionAsync(entityId) {
    $.ajax({
        url: `Commissions/Delete/${entityId}`,
        type: 'DELETE',
        success: () => document.getElementById(`commission-card-${entityId}`).remove(),
        error: xhr => {
            if (xhr.status == 401)
                alert('You are not authorized to perform this action');
            else
                alert('Something went wrong with your request, please try again later');
        }
    });
}

function changeCommissionStatusAsync(viewElementId, entityId, newStatus) {
    $.ajax({
        url: `Commissions/ChangeStatus?id=${entityId}&newStatus=${newStatus}`,
        type: 'PATCH',
        success: () => {
            const btn = document.getElementById(viewElementId);

            btn.dataset.status = newStatus;
            btn.textContent = newStatus ? 'Open it up' : 'Close it temporarily';

            document.getElementById(`status-${entityId}`).textContent = newStatus ? 'Closed' : 'Open';
        },
        error: xhr => {
            if (xhr.status == 401)
                alert("You are not authorized to perform this action");
            else
                alert('Something went wrong with your request, please try again later');
        }
    })
}