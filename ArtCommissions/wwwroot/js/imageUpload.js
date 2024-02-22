const sampleImagesInput = document.getElementById('SampleImages');
const imagePreviewContainer = document.getElementById('image-previews');

sampleImagesInput.addEventListener('change', e => {
    imagePreviewContainer.innerHTML = '';
    const photoFiles = sampleImagesInput.files;

    for (i = 0; i < photoFiles.length; i++) {
        const fileReader = new FileReader();
        fileReader.onload = addImagePreview;
        fileReader.readAsDataURL(photoFiles[i]);
    }
});

function addImagePreview(e) {
    const img = document.createElement('img');
    img.src = e.target.result;
    img.classList.add('image-upload-preview');

    imagePreviewContainer.insertAdjacentElement('beforeend', img);
}