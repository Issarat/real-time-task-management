(() => {
    document.querySelectorAll('[data-project-toast]').forEach((toast) => {
        let dismissTimer;

        const dismiss = () => {
            if (toast.classList.contains('is-dismissing')) {
                return;
            }

            toast.classList.add('is-dismissing');
            window.setTimeout(() => toast.remove(), 200);
        };

        const dismissDelay = Number.parseInt(toast.dataset.autoDismissMs, 10);
        dismissTimer = window.setTimeout(
            dismiss,
            Number.isFinite(dismissDelay) ? dismissDelay : 5000);

        toast.querySelector('[data-toast-dismiss]')?.addEventListener('click', () => {
            window.clearTimeout(dismissTimer);
            dismiss();
        });
    });

    const dialogs = new Map();
    const openers = new Map();

    document.querySelectorAll('dialog.project-dialog').forEach((dialog) => {
        if (!(dialog instanceof HTMLDialogElement) || !dialog.id) {
            return;
        }

        dialogs.set(dialog.id, dialog);

        const closeDialog = () => {
            if (dialog.open) {
                dialog.close();
            }
            openers.get(dialog.id)?.focus();
            openers.delete(dialog.id);
        };

        dialog.querySelectorAll('[data-dialog-close]').forEach((button) => {
            button.addEventListener('click', closeDialog);
        });

        dialog.addEventListener('click', (event) => {
            if (event.target === dialog) {
                closeDialog();
            }
        });

        dialog.addEventListener('cancel', (event) => {
            event.preventDefault();
            closeDialog();
        });
    });

    const openDialog = (dialogId, trigger) => {
        const dialog = dialogs.get(dialogId);
        if (!dialog) {
            return;
        }

        if (trigger) {
            openers.set(dialogId, trigger);
        }
        if (!dialog.open) {
            dialog.showModal();
        }

        const autofocusTarget = dialog.querySelector('[data-dialog-autofocus]');
        window.requestAnimationFrame(() => autofocusTarget?.focus());
    };

    document.querySelectorAll('[data-dialog-open]').forEach((trigger) => {
        trigger.addEventListener('click', () => {
            openDialog(trigger.dataset.dialogOpen, trigger);
        });
    });

    const createDialog = dialogs.get('create-project-dialog');
    const descriptionInput = createDialog?.querySelector('[name="CreateProject.Description"]');
    const descriptionCount = createDialog?.querySelector('[data-description-count]');
    const updateDescriptionCount = () => {
        if (descriptionCount && descriptionInput) {
            descriptionCount.textContent = descriptionInput.value.length.toString();
        }
    };

    descriptionInput?.addEventListener('input', updateDescriptionCount);
    updateDescriptionCount();

    document.querySelectorAll('[data-invite-code]').forEach((input) => {
        input.addEventListener('input', () => {
            const caretPosition = input.selectionStart;
            input.value = input.value.toUpperCase().replace(/\s+/g, '');
            if (caretPosition !== null) {
                input.setSelectionRange(caretPosition, caretPosition);
            }
        });
    });

    const inviteCopyButton = document.querySelector('[data-invite-copy]');
    const generatedInviteInput = document.querySelector('[data-generated-invite-code]');
    const inviteCopyLabel = document.querySelector('[data-invite-copy-label]');
    const inviteCopyStatus = document.querySelector('[data-invite-copy-status]');

    inviteCopyButton?.addEventListener('click', async () => {
        if (!(generatedInviteInput instanceof HTMLInputElement)) {
            return;
        }

        let wasCopied = false;
        try {
            if (navigator.clipboard) {
                await navigator.clipboard.writeText(generatedInviteInput.value);
                wasCopied = true;
            }
        } catch {
            wasCopied = false;
        }

        if (!wasCopied) {
            generatedInviteInput.select();
            wasCopied = document.execCommand('copy');
            generatedInviteInput.setSelectionRange(0, 0);
        }

        if (wasCopied) {
            if (inviteCopyLabel) {
                inviteCopyLabel.textContent = 'คัดลอกแล้ว';
            }
            if (inviteCopyStatus) {
                inviteCopyStatus.textContent = 'คัดลอก Invite Code แล้ว';
            }
        }
    });

    dialogs.forEach((dialog, dialogId) => {
        if (dialog.dataset.openOnLoad === 'true') {
            openDialog(dialogId, null);
        }
    });
})();
