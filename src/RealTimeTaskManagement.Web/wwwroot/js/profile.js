(() => {
    const readView = document.querySelector('[data-profile-read-view]');
    const editForm = document.querySelector('[data-profile-edit-form]');
    const openButton = document.querySelector('[data-profile-edit-open]');
    const cancelButton = document.querySelector('[data-profile-edit-cancel]');

    if (!readView || !editForm || !openButton || !cancelButton) {
        return;
    }

    const setEditMode = (isEditing) => {
        readView.classList.toggle('is-hidden', isEditing);
        editForm.classList.toggle('is-hidden', !isEditing);
        openButton.classList.toggle('is-hidden', isEditing);

        if (isEditing) {
            editForm.querySelector('input:not([readonly])')?.focus();
        }
    };

    openButton.addEventListener('click', () => setEditMode(true));
    cancelButton.addEventListener('click', () => {
        editForm.reset();
        setEditMode(false);
    });

    const passwordReadView = document.querySelector('[data-password-read-view]');
    const passwordForm = document.querySelector('[data-password-edit-form]');
    const passwordOpenButton = document.querySelector('[data-password-edit-open]');
    const passwordCancelButton = document.querySelector('[data-password-edit-cancel]');
    const passwordToggleButtons = document.querySelectorAll('[data-password-toggle]');

    if (!passwordReadView || !passwordForm || !passwordOpenButton || !passwordCancelButton) {
        return;
    }

    const setPasswordEditMode = (isEditing) => {
        passwordReadView.classList.toggle('is-hidden', isEditing);
        passwordForm.classList.toggle('is-hidden', !isEditing);
        passwordOpenButton.classList.toggle('is-hidden', isEditing);

        if (isEditing) {
            passwordForm.querySelector('input')?.focus();
        }
    };

    const resetPasswordVisibility = () => {
        passwordToggleButtons.forEach((button) => {
            const input = button.closest('.profile-password-input')?.querySelector('input');
            if (!input) {
                return;
            }

            input.type = 'password';
            button.dataset.visible = 'false';
            button.setAttribute('aria-pressed', 'false');
            button.setAttribute('aria-label', button.getAttribute('aria-label')?.replace('ซ่อน', 'แสดง') ?? 'แสดงรหัสผ่าน');
        });
    };

    resetPasswordVisibility();

    passwordToggleButtons.forEach((button) => {
        button.addEventListener('click', () => {
            const input = button.closest('.profile-password-input')?.querySelector('input');
            if (!input) {
                return;
            }

            const isVisible = input.type === 'password';
            input.type = isVisible ? 'text' : 'password';
            button.dataset.visible = isVisible.toString();
            button.setAttribute('aria-pressed', isVisible.toString());
            button.setAttribute(
                'aria-label',
                `${isVisible ? 'ซ่อน' : 'แสดง'}${button.getAttribute('aria-label')?.replace(/^(แสดง|ซ่อน)/, '') ?? 'รหัสผ่าน'}`);
        });
    });

    passwordOpenButton.addEventListener('click', () => setPasswordEditMode(true));
    passwordCancelButton.addEventListener('click', () => {
        passwordForm.reset();
        resetPasswordVisibility();
        setPasswordEditMode(false);
    });
})();
