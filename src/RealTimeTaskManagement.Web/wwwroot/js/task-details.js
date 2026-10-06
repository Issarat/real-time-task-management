(() => {
    const readView = document.querySelector('[data-task-read-view]');
    const editForm = document.querySelector('[data-task-edit-form]');
    const openButton = document.querySelector('[data-task-edit-open]');
    const cancelButton = document.querySelector('[data-task-edit-cancel]');
    const metadataViews = document.querySelectorAll('[data-task-meta-view]');
    const metadataEditControls = document.querySelectorAll('[data-task-edit-control]');

    if (!readView || !editForm || !openButton || !cancelButton) {
        return;
    }

    const setEditMode = (isEditing) => {
        readView.classList.toggle('is-hidden', isEditing);
        editForm.classList.toggle('is-hidden', !isEditing);
        openButton.classList.toggle('is-hidden', isEditing);
        metadataViews.forEach((element) => {
            element.classList.toggle('is-hidden', isEditing);
        });
        metadataEditControls.forEach((element) => {
            element.classList.toggle('is-hidden', !isEditing);
        });

        if (isEditing) {
            editForm.querySelector('input')?.focus();
        }
    };

    openButton.addEventListener('click', () => setEditMode(true));
    cancelButton.addEventListener('click', () => {
        editForm.reset();
        metadataEditControls.forEach((element) => element.reset());
        setEditMode(false);
    });
})();
