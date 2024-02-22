const reportType = document.getElementById('report-type').value;
const reportStatus = document.getElementById('report-status');
const conclusionBtnsDiv = document.getElementById('conclusion-btns');

const acceptReportBtn = document.getElementById('accept-btn'),
    rejectReportBtn = document.getElementById('reject-btn');

if (acceptReportBtn) {
    acceptReportBtn.addEventListener('click', () => {
        if (confirm('Are you sure you want to accept this report? This action is irreversible & will add a suspension strike and/or lock out a user.')) {
            $.ajax({
                url: `/Admin/${reportType}Reports/AcceptReport/${acceptReportBtn.dataset.entityId}`,
                type: 'PATCH',
                success: () => onSuccessfulReportConclusion("ACCEPTED"),
                error: () => alert('Something went wrong trying to process your request, please try again later')
            })
        }
    })
}

if (rejectReportBtn) {
    rejectReportBtn.addEventListener('click', () => {
        if (confirm('Are you sure you want to reject this report?')) {
            $.ajax({
                url: `/Admin/${reportType}Reports/RejectReport/${rejectReportBtn.dataset.entityId}`,
                type: 'PATCH',
                success: () => onSuccessfulReportConclusion("REJECTED"),
                error: () => alert('Something went wrong trying to process your request, please try again later')
            })
        }
    })
}

function onSuccessfulReportConclusion(newStatus) {
    reportStatus.textContent = newStatus;
    conclusionBtnsDiv.remove();
}