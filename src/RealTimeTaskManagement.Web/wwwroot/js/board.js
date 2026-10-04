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
})();
