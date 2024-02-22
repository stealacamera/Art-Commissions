deleteBtns = document.getElementsByClassName('delete-btn');

Array.from(deleteBtns).forEach(btn => btn.addEventListener('click', e => {
    if (confirm('Are you sure you want to delete this order?')) {
        const entityId = e.currentTarget.dataset.entityId;

        $.ajax({
            url: `/User/Orders/Delete/${entityId}`,
            type: 'DELETE',
            success: () => document.getElementById(`order-card-${entityId}`).remove(),
            error: () => alert('Something went wrong with your request, please try again later')
        });
    }
}))