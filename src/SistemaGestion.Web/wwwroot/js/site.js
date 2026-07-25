(() => {
    const storageKey = "sg-operaciones-theme";
    const root = document.documentElement;
    const systemTheme = window.matchMedia("(prefers-color-scheme: dark)");

    function setTheme(theme, persist = false) {
        root.dataset.theme = theme;
        if (persist) localStorage.setItem(storageKey, theme);

        document.querySelectorAll("[data-theme-toggle]").forEach(button => {
            const darkIsActive = theme === "dark";
            const action = darkIsActive ? "Cambiar a modo claro" : "Cambiar a modo oscuro";
            button.title = action;
            button.setAttribute("aria-label", action);
            button.setAttribute("aria-pressed", darkIsActive.toString());
            const icon = button.querySelector("[data-theme-icon]");
            if (icon) icon.textContent = darkIsActive ? "light_mode" : "dark_mode";
        });
    }

    document.querySelectorAll("[data-theme-toggle]").forEach(button => {
        button.addEventListener("click", () =>
            setTheme(root.dataset.theme === "dark" ? "light" : "dark", true));
    });

    document.querySelectorAll("[data-sidebar-toggle]").forEach(button => {
        button.addEventListener("click", () => document.body.classList.toggle("sidebar-open"));
    });

    systemTheme.addEventListener("change", event => {
        if (!localStorage.getItem(storageKey)) setTheme(event.matches ? "dark" : "light");
    });

    setTheme(root.dataset.theme || (systemTheme.matches ? "dark" : "light"));
})();
