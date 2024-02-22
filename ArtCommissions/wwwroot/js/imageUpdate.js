const removeImageBtns = document.getElementsByClassName('remove-existing-image-btns');

for (i = 0; i < removeImageBtns.length; i++) {
    const currentBtn = removeImageBtns[i];

    currentBtn.addEventListener('click', () => {
        document.getElementById(`existing-sample-image-${currentBtn.dataset.entityId}`).style.display = 'none';
        document.getElementsByName(`ExistingSampleImages[${currentBtn.dataset.entityId}].ShouldRemove`)[0].value = 'True';
    });
}