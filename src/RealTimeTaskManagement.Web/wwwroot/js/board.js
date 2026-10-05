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
})();
