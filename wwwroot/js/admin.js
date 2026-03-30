(() => {
  const toggleButtons = document.querySelectorAll("[data-password-toggle]");

  toggleButtons.forEach((button) => {
    button.addEventListener("click", () => {
      const targetId = button.getAttribute("data-password-target");
      if (!targetId) {
        return;
      }

      const input = document.getElementById(targetId);
      if (!(input instanceof HTMLInputElement)) {
        return;
      }

      const isPassword = input.type === "password";
      input.type = isPassword ? "text" : "password";
      button.setAttribute("aria-pressed", String(isPassword));

      const icon = button.querySelector("i");
      if (icon) {
        icon.className = isPassword ? "fa-regular fa-eye" : "fa-regular fa-eye-slash";
      }
    });
  });

  const confirmForms = document.querySelectorAll("form[data-confirm]");
  confirmForms.forEach((form) => {
    form.addEventListener("submit", (event) => {
      const message = form.getAttribute("data-confirm");
      if (message && !window.confirm(message)) {
        event.preventDefault();
      }
    });
  });
})();
