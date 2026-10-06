(() => {
    const body = document.body;
    const toggle = document.querySelector('[data-sidebar-toggle]');
    const close = document.querySelector('[data-sidebar-close]');

    const setSidebar = (isOpen) => {
        body.classList.toggle('sidebar-open', isOpen);
        toggle?.setAttribute('aria-expanded', isOpen.toString());
    };

    toggle?.addEventListener('click', () => {
        setSidebar(!body.classList.contains('sidebar-open'));
    });

    close?.addEventListener('click', () => setSidebar(false));

    window.addEventListener('resize', () => {
        if (window.innerWidth > 900) {
            setSidebar(false);
        }
    });

    const boardSearch = document.querySelector('[data-board-search]');
    boardSearch?.addEventListener('input', () => {
        const query = boardSearch.value.trim().toLocaleLowerCase('th');
        document.querySelectorAll('[data-task-card]').forEach((card) => {
            card.draggable = !query;
        });

        document.querySelectorAll('[data-board-column]').forEach((column) => {
            const cards = [...column.querySelectorAll('[data-task-card]')];
            let visibleTaskCount = 0;

            cards.forEach((card) => {
                const searchText = (card.dataset.searchText ?? '')
                    .toLocaleLowerCase('th');
                const isVisible = !query || searchText.includes(query);
                card.hidden = !isVisible;
                visibleTaskCount += isVisible ? 1 : 0;
            });

            column.querySelector('[data-column-empty]')
                ?.classList.toggle('is-hidden', visibleTaskCount > 0);
        });
    });

    const boardRoot = document.querySelector('[data-board-root]');
    const taskLists = [...document.querySelectorAll('[data-task-list]')];
    const taskCards = [...document.querySelectorAll('[data-task-card]')];
    const antiForgeryToken = document.querySelector(
        '[data-board-antiforgery] input[name="__RequestVerificationToken"]')?.value;
    const moveStatus = document.querySelector('[data-board-move-status]');
    let dragState = null;
    let suppressCardClick = false;
    let moveStatusTimer;

    const showMoveStatus = (message, isError = false) => {
        if (!moveStatus) {
            return;
        }

        window.clearTimeout(moveStatusTimer);
        moveStatus.textContent = message;
        moveStatus.classList.toggle('is-error', isError);
        moveStatus.hidden = false;
        moveStatusTimer = window.setTimeout(() => {
            moveStatus.hidden = true;
        }, isError ? 4000 : 2200);
    };

    const updateColumnState = () => {
        document.querySelectorAll('[data-board-column]').forEach((column) => {
            const taskCount = column.querySelectorAll('[data-task-card]').length;
            const count = column.querySelector('[data-column-count]');
            const empty = column.querySelector('[data-column-empty]');

            if (count) {
                count.textContent = taskCount.toString();
            }
            empty?.classList.toggle('is-hidden', taskCount > 0);
        });
    };

    const restoreOriginalPosition = (state) => {
        if (!state?.card || !state?.sourceList) {
            return;
        }

        const nextSibling = state.sourceNextSibling;
        if (nextSibling && nextSibling.parentElement === state.sourceList) {
            state.sourceList.insertBefore(state.card, nextSibling);
        } else {
            state.sourceList.appendChild(state.card);
        }
        updateColumnState();
    };

    const isValidStatusStep = (sourceList, destinationList) => {
        if (!boardRoot) {
            return false;
        }

        const columns = [...boardRoot.querySelectorAll('[data-board-column]')];
        const sourceColumn = sourceList?.closest('[data-board-column]');
        const destinationColumn = destinationList?.closest('[data-board-column]');
        const sourceIndex = columns.indexOf(sourceColumn);
        const destinationIndex = columns.indexOf(destinationColumn);

        return sourceIndex >= 0
            && destinationIndex >= 0
            && Math.abs(sourceIndex - destinationIndex) <= 1;
    };

    const getInsertBeforeCard = (list, pointerY) => {
        const cards = [...list.querySelectorAll('[data-task-card]:not(.is-dragging)')];
        return cards.reduce((closest, card) => {
            const box = card.getBoundingClientRect();
            const offset = pointerY - box.top - (box.height / 2);
            return offset < 0 && offset > closest.offset
                ? { offset, card }
                : closest;
        }, { offset: Number.NEGATIVE_INFINITY, card: null }).card;
    };

    const persistTaskPosition = async (state, destinationList) => {
        const destinationColumn = destinationList.closest('[data-board-column]');
        const boardColumnId = destinationColumn?.dataset.boardColumn;
        const projectSlug = window.location.pathname.split('/')[2];

        if (!boardColumnId || !projectSlug || !antiForgeryToken) {
            throw new Error('Missing board move information.');
        }

        const formData = new FormData();
        formData.append('__RequestVerificationToken', antiForgeryToken);
        formData.append('boardColumnId', boardColumnId);
        destinationList.querySelectorAll('[data-task-card]').forEach((card) => {
            formData.append('orderedTaskIds', card.dataset.taskId);
        });

        const response = await fetch(
            `/projects/${encodeURIComponent(projectSlug)}/tasks/${encodeURIComponent(state.card.dataset.taskId)}/move`,
            {
                method: 'POST',
                body: formData,
                credentials: 'same-origin'
            });

        if (!response.ok) {
            throw new Error('Unable to save the task position.');
        }
    };

    taskCards.forEach((card) => {
        card.addEventListener('click', (event) => {
            if (suppressCardClick) {
                event.preventDefault();
                suppressCardClick = false;
            }
        });

        card.addEventListener('dragstart', (event) => {
            if (!boardRoot) {
                event.preventDefault();
                return;
            }

            dragState = {
                card,
                sourceList: card.closest('[data-task-list]'),
                sourceNextSibling: card.nextElementSibling,
                dropHandled: false,
                invalidTarget: false
            };
            suppressCardClick = true;
            card.classList.add('is-dragging');
            event.dataTransfer.effectAllowed = 'move';
            event.dataTransfer.setData('text/plain', card.dataset.taskId ?? '');
        });

        card.addEventListener('dragend', () => {
            if (dragState && !dragState.dropHandled) {
                restoreOriginalPosition(dragState);
                if (dragState.invalidTarget) {
                    showMoveStatus('เปลี่ยนสถานะได้ครั้งละ 1 ขั้นเท่านั้น', true);
                }
            }
            card.classList.remove('is-dragging');
            taskLists.forEach((list) => {
                list.classList.remove('is-drag-over', 'is-drop-invalid');
            });
            dragState = null;
            window.setTimeout(() => {
                suppressCardClick = false;
            }, 0);
        });
    });

    taskLists.forEach((list) => {
        list.addEventListener('dragover', (event) => {
            if (!dragState) {
                return;
            }

            event.preventDefault();
            const isValidTarget = isValidStatusStep(dragState.sourceList, list);
            dragState.invalidTarget = !isValidTarget;
            event.dataTransfer.dropEffect = isValidTarget ? 'move' : 'none';
            taskLists.forEach((item) => {
                item.classList.toggle('is-drag-over', item === list && isValidTarget);
                item.classList.toggle('is-drop-invalid', item === list && !isValidTarget);
            });

            if (!isValidTarget) {
                return;
            }

            const beforeCard = getInsertBeforeCard(list, event.clientY);
            if (beforeCard) {
                list.insertBefore(dragState.card, beforeCard);
            } else {
                list.appendChild(dragState.card);
            }
        });

        list.addEventListener('drop', async (event) => {
            event.preventDefault();
            if (!dragState) {
                return;
            }

            const state = dragState;
            state.dropHandled = true;
            const isValidTarget = isValidStatusStep(state.sourceList, list);
            state.card.classList.add('is-saving-position');
            taskLists.forEach((item) => {
                item.classList.remove('is-drag-over', 'is-drop-invalid');
            });

            if (!isValidTarget) {
                restoreOriginalPosition(state);
                state.card.classList.remove('is-saving-position');
                showMoveStatus('เปลี่ยนสถานะได้ครั้งละ 1 ขั้นเท่านั้น', true);
                return;
            }
            updateColumnState();

            try {
                await persistTaskPosition(state, list);
                showMoveStatus('ย้ายงานเรียบร้อยแล้ว');
            } catch {
                restoreOriginalPosition(state);
                showMoveStatus('ไม่สามารถย้ายงานได้ ตำแหน่งถูกคืนค่าแล้ว', true);
            } finally {
                state.card.classList.remove('is-saving-position');
            }
        });
    });
})();
